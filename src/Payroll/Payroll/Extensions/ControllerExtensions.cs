using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Payroll
{
    // public static class xxx
    // {
    //     public static IAsyncEnumerable<TEntity> AsAsyncEnumerable<TEntity>(this Microsoft.EntityFrameworkCore.DbSet<TEntity> obj) where TEntity : class
    //     {
    //         return Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsAsyncEnumerable(obj);
    //     }
    //     public static IQueryable<TEntity> Where<TEntity>(this Microsoft.EntityFrameworkCore.DbSet<TEntity> obj, System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where TEntity : class
    //     {
    //         return System.Linq.Queryable.Where(obj, predicate).AsEnumerable<TEntity>();
    //     }
    //     public static Task<bool> AynAsync<TEntity>(this Microsoft.EntityFrameworkCore.DbSet<TEntity> obj) where TEntity : class
    //     {
    //         return Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(obj);
    //     }

    // }
}


namespace Payroll
{
    public static class ControllerExtensions
    {

        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

                if (viewResult.Success == false)
                {
                    return $"A view with the name {viewName} could not be found";
                }

                ViewContext viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions()
            );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }

        public static int GetEmployeeId(this IPrincipal principal)
        {
            if (principal == null || principal.Identity == null) { return 0; }

            var identity = principal.Identity;

            var claim = ((ClaimsIdentity)identity).FindFirst(CustomClaimTypes.EmployeeId);
            if (claim == null) { return 0; }

            return int.Parse(claim.Value);
        }

        public static bool IsNotInRole(this IPrincipal principal, string role)
        {
            if (principal == null || principal.Identity == null) { return false; }

            var identity = principal.Identity;

            var claim = ((ClaimsIdentity)identity).FindFirst(ClaimTypes.Role);
            if (claim == null) { return false; }

            var roleDeparments = claim.Value;
            if (string.IsNullOrWhiteSpace(roleDeparments)) { return false; }

            var isInRole = roleDeparments
                .Split(',')
                .Count(d => d == Roles.Company.administrator) <= 0;

            return isInRole;
        }


        public static List<SearchResult> GetMenuForUser(this IPrincipal principal)
        {
            var allMenus = FeatureMenus.FeatureSearchList;

            if (principal == null || principal.Identity == null) { return default; }

            var identity = principal.Identity;
            
            var newMenu  = new List<SearchResult>();            
            newMenu.Add(allMenus.First(x=> x.Name == FeatureMenus.MenuItem.Staffs));

            if(principal.IsInAnyOfRoles(Roles.PayAll.admin, Roles.Company.hr_manager, Roles.Company.management))
                newMenu.Add(allMenus.First(x=> x.Name == FeatureMenus.MenuItem.Jobs));

            // if(principal.IsInAnyOfRoles(Roles.PayAll.admin, Roles.Company.hr_manager, Roles.Company.supervisor))
            //     newMenu.Add(allMenus.First(x=> x.Name == FeatureMenus.MenuItem.Schedule));
            
            if(principal.IsInAnyOfRoles(Roles.Company.all_employees)){
                newMenu.Add(allMenus.First(x=> x.Name == FeatureMenus.MenuItem.Calendar));
                newMenu.Add(allMenus.First(x=> x.Name == FeatureMenus.MenuItem.Schedule));
            }

            if(principal.IsHrManagerOrAdmin()){
                newMenu.Add(allMenus.First(x=> x.Name == FeatureMenus.MenuItem.PayComponents));
                newMenu.Add(allMenus.First(x=> x.Name == FeatureMenus.MenuItem.Payroll));
                newMenu.Add(allMenus.First(x=> x.Name == FeatureMenus.MenuItem.TimeOff));
                // newMenu.Add(allMenus.First(x=> x.Name == FeatureMenus.MenuItem.Calendar));
                newMenu.Add(allMenus.First(x=> x.Name == FeatureMenus.MenuItem.AbsenceCalendar));
                newMenu.Add(allMenus.First(x=> x.Name == FeatureMenus.MenuItem.TimeTrackingApprovals));
                newMenu.Add(allMenus.First(x=> x.Name == FeatureMenus.MenuItem.Announcements));
            }


            if (principal.IsAdmin()) {
                newMenu.Add(allMenus.First(x => x.Name == FeatureMenus.MenuItem.Company));
                newMenu.Add(allMenus.First(x => x.Name == FeatureMenus.MenuItem.User));
            }

            return newMenu;
        }

        public static bool IsInAnyOfRoles(this IPrincipal principal, params string[] role)
        {
            if (principal == null || principal.Identity == null) { return false; }

            var identity = principal.Identity;

            var claims = ((ClaimsIdentity)identity).FindAll(ClaimTypes.Role);
            if (claims == null) { return false; }


            var isInRole = claims.Select(a=> a.Value)
                .Intersect(role).Count() > 0; // .Any(a => role.Contains(a));

            return isInRole;
        }

        public static bool IsOnlyOfAllEmployeeRole(this IPrincipal principal)
        {
            if (principal == null || principal.Identity == null) { return false; }

            var identity = principal.Identity;

            var claims = ((ClaimsIdentity)identity).FindAll(ClaimTypes.Role);
            if (claims == null) { return false; }


            return claims.Count() == 1 && claims.First().Value == Roles.Company.all_employees;
        }

        public static bool IsHrManagerOrAdmin(this IPrincipal principal)
        {
            if (principal == null || principal.Identity == null) { return false; }

            var identity = principal.Identity;
            return (principal.IsInRole(Roles.Company.hr_manager) || principal.IsInRole(Roles.Company.administrator) || principal.IsInRole(Roles.PayAll.admin));
        }

        public static bool IsAdmin(this IPrincipal principal)
        {
            if (principal == null || principal.Identity == null) { return false; }

            var identity = principal.Identity;
            return (principal.IsInRole(Roles.PayAll.admin));
        }

        //public static bool IsInScope(this IPrincipal principal, string scope)
        //{
        //    if (principal == null || principal.Identity == null) { return false; }

        //    var identity = principal.Identity;

        //    var claim = ((ClaimsIdentity)identity).FindFirst(CustomClaimTypes.EmployeeRoleScope);
        //    if (claim == null) { return false; }

        //    var roleDeparments = claim.Value;
        //    if (string.IsNullOrWhiteSpace(roleDeparments)) { return false; }

        //    var isInRole = roleDeparments
        //        .Split(',')
        //        .Any(d => d == scope || d == "all");

        //    return isInRole;
        //}

    }

}

