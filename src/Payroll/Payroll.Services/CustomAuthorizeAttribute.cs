using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Payroll.Services
{
    //using AuthorizeAttribute for the predefined properties and functions from Authorize Attribute.



    public class A1AuthorizePermission : AuthorizeAttribute, IAuthorizationFilter
    {
        public string Permissions { get; set; } //Permission string to get from controller

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // user is not logged in > return back -to LOGIN
            if (context.HttpContext.User.Identity.IsAuthenticated == false) return;

            bool hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
                                     .Any(em => em.GetType() == typeof(AllowAnonymousAttribute)); //< -- Here it is
            if (hasAllowAnonymous) return;

            var isTmpSwitched = context.HttpContext.User.HasClaim(p => p.Type == CustomClaimTypes.EmployeeTempSwitch && p.Value == "1");
            if (context.HttpContext.User.IsInRole("admin") && !isTmpSwitched)
                return;

            var _c = (context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor);
            var thisRoute = $"{_c.MethodInfo.DeclaringType.Name}.{_c.MethodInfo.Name}.{String.Join(",", _c.MethodInfo.GetCustomAttributes(false).Select(a => a.GetType().Name.Replace("Attribute", "Attribute")))}";


            if (context.HttpContext.User.HasClaim(c => c.Type == CustomClaimTypes.EmployeeRoleRoute && c.Value == thisRoute))
                return; //User Authorized. Without

            //Validate if any permissions are passed when using attribute at controller or action level

            //if (string.IsNullOrEmpty(Permissions))
            //{
            //    //Validation cannot take place without any permissions so returning unauthorized
            //    context.Result = new UnauthorizedResult();
            //    return;
            //}



            //Identity.Name will have windows logged in the user id, in case of Windows Authentication
            //Indentity.Name will have user name passed from token, in case of JWT Authentication, and having claim type "ClaimTypes.Name"

            //var userName = context.HttpContext.User.Identity.Name;

            //string action = ""; var controller = ""; var attr = "";
            //var service = context.HttpContext.RequestServices.GetService(typeof(IActionContextAccessor)) as IActionContextAccessor;
            //if (service != null)
            //{
            //    var _c = (service.ActionContext.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor);
            //    controller = _c.ControllerName;
            //    action = _c.ActionName;
            //    attr = service.ActionContext.ActionDescriptor.DisplayName;
            //    //attr = service.ActionContext.ActionDescriptor.ActionConstraints.
            //    //var auditAction = (service.ActionContext.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)
            //    //        .MethodInfo.GetCustomAttributes(typeof(Filters.AuditTrailActionAttribute), true).FirstOrDefault();
            //    //if (auditAction != null)
            //    //    return (auditAction as Filters.AuditTrailActionAttribute).GetDescription();
            //}

            //var thisRoute = $"{context.ActionDescriptor.}.{action}.{attr}";
            //if (context.HttpContext.User.HasClaim(c=> c.Type== CustomClaimTypes.EmployeeRoleRoute && c.Value == thisRoute ))
            //    return; //User Authorized. Without

            //return "";

            //var controller = context.HttpContext.co
            //var assignedPermissionsForUser = User MockData.UserPermissions.Where(x => x.Key == userName).Select(x => x.Value).ToList();

            //var requiredPermissions = Permissions.Split(",");
            //foreach (var x in requiredPermissions)
            //{
            //    if (assignedPermissionsForUser.Contains(x))
            //        return; //User Authorized. Without setting any result value and just returning is sufficient for authorizing user
            //}


            context.Result = new UnauthorizedResult();
            return;
        }

    }
}
