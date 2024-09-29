using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Payroll.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace Payroll
{
    public class UrlAttribute : ValidationAttribute
    {
        public UrlAttribute()
        {
        }

        public override bool IsValid(object value)
        {
            var text = value as string;
            Uri uri;

            if (string.IsNullOrWhiteSpace(text))
                return true;

            return (!string.IsNullOrWhiteSpace(text) && Uri.TryCreate(text, UriKind.Absolute, out uri));
        }
    }

    public class SelectableFieldAttribute : Attribute
    {
    }

    public class PayAdjustmentFieldAttribute : Attribute
    {
    }

    public class NotificaitonAvatarAttribute : Attribute
    {
    }

    public class AuditableEntityAttribute : Attribute
    {
    }

    public class AuditTrailActionAttribute : Attribute
    {
        private string description;

        public AuditTrailActionAttribute(string description)
        {
            this.description = description;
        }

        internal string GetDescription() => description;
    }

    public class ActionPermissionsAttribute : Attribute
    {
        // READ, UPDATE, DELETE
        private string[] permission;


        public ActionPermissionsAttribute(params string[] permission)
        {
            this.permission = permission;
        }

        public string[] GetPermission => permission;
    }


    /// <summary>
    /// Authorize only to view users with all_employee roles and id as your own
    /// </summary>
    public class MyProfileAuthorize : AuthorizeAttribute, IAuthorizationFilter
    {
        public string RouteParam = "id";
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if(context.RouteData.Values.ContainsKey(RouteParam))
            if (context.HttpContext.User.IsInRole(Payroll.Models.Roles.Company.all_employees) &&  context.HttpContext.User.GetEmployeeId() == int.Parse(context.RouteData.Values[RouteParam].ToString()))
                return;

            context.Result = new UnauthorizedResult();
            return;
        }
    }

    /// <summary>
    /// Authorize only to view users with all_employee roles and id as your own OR 
    /// supervisor with id as his/her's direct reports
    /// </summary>
    public class MyProfileOrDirectReportAuthorize : AuthorizeAttribute, IAuthorizationFilter
    {
        public string RouteParam = "id";
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.HttpContext.User.IsAdmin())
                return;

            if (context.RouteData.Values.ContainsKey(RouteParam))
            {
                var routeEmpId = int.Parse(context.RouteData.Values[RouteParam].ToString());
                if (context.HttpContext.User.IsInRole(Payroll.Models.Roles.Company.all_employees) && context.HttpContext.User.GetEmployeeId() == routeEmpId)
                    return;


                var ctx = (PayrollDbContext)context.HttpContext.RequestServices.GetService(typeof(PayrollDbContext));
                if (context.HttpContext.User.IsInRole(Payroll.Models.Roles.Company.supervisor) && ctx.Employees.Any(a => a.Id == routeEmpId && a.ReportingEmployeeId == context.HttpContext.User.GetEmployeeId()))
                    return;
            }

            context.Result = new UnauthorizedResult();
            return;
        }
    }

    public class EmployeeRoleAuthorize : AuthorizeAttribute, IAuthorizationFilter
    {
        public string[] Roles { get; set; }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.HttpContext.User.IsAdmin())
                return;

                if(context.HttpContext.User.IsInAnyOfRoles(Roles))
                return;

                   
            context.Result = new UnauthorizedResult();
            return;    
        }
    }

}
