using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Payroll.Database;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Payroll.Services
{
    public class UserResolverService
    {
        public readonly IHttpContextAccessor _context;
        private readonly UserManager<AppUser> userManager;

        public UserResolverService(IHttpContextAccessor context)
        {
            _context = context;
        }

        public IHttpContextAccessor GetContext => _context;


        public string GetUser()
        {
            return _context.HttpContext?.User?.Identity?.Name;
        }

        public ClaimsPrincipal GetClaimsPrincipal()
        {
            return _context.HttpContext?.User;
        }

        public string GeAuditTrailtActionDescription()
        {
            var service = _context.HttpContext.RequestServices.GetService(typeof(IActionContextAccessor)) as IActionContextAccessor;
            if (service != null)
            {
                var auditAction = (service.ActionContext.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)
                        .MethodInfo.GetCustomAttributes(typeof(Filters.AuditTrailActionAttribute), true).FirstOrDefault();
                if (auditAction != null)
                    return (auditAction as Filters.AuditTrailActionAttribute).GetDescription();
            }

            return "";
        }

        public string Get(string claimType)
        {
            return _context.HttpContext?.User?.FindFirstValue(claimType);
        }

        /// <summary>
        /// Check if User Type is Employee
        /// </summary>
        /// <returns></returns>
        public bool IsEmployee()
        {
            return _context.HttpContext?.User?.FindFirstValue(CustomClaimTypes.UserType) == UserType.Employee.ToString();
        }

        public List<string> GetEmployeeRoles() =>_context.HttpContext?.User?.Claims.Where(a => a.Type == ClaimTypes.Role)
                .Select(a => a.Value).ToList() ?? default;
                
        //        Select(a=> new {
        //        type = CustomClaimTypes.EmployeeRoleId,
        //        value = CustomClaimTypes.EmployeeRole
        //    })

        //        .ToDictionary(a => a.Type == CustomClaimTypes.EmployeeRoleId, a => a.Type == CustomClaimTypes.EmployeeRole);
        //}
        public List<string> GetEmployeeRoleIds() => _context.HttpContext?.User?.Claims.Where(a => a.Type == CustomClaimTypes.EmployeeRoleId)
                .Select(a => a.Value).ToList() ?? default;
        

            public int GetCompanyId()
        {
            int id = 0;
            try
            {
                int.TryParse(_context.HttpContext?.User?.FindFirstValue(CustomClaimTypes.accessgrant_companyId), out id);
            }
            catch (Exception)
            {
            }
            return id;
        }
        public int GetEmployeeId()
        {
            int id = 0;
            try
            {
                int.TryParse(_context.HttpContext?.User?.FindFirstValue(CustomClaimTypes.EmployeeId), out id);
            }
            catch (Exception)
            {
            }
            return id;
        }
        public string GetUserId()
        {
            return _context.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? SystemUserInfo.id;
        }
        public string GetUserName()
        {
            return _context.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ?? SystemUserInfo.name;
        }

        //public async Task<string> GetRolesAsync(AppUser user)
        //{
        //    var roles = await userManager.GetRolesAsync(user);
        //    return string.Join(",", roles);
        //}



        public bool IsInScope(string scope)
        {
            var claim = _context.HttpContext?.User.FindFirst(CustomClaimTypes.EmployeeRoleScope);
            if (claim == null) { return false; }
            var roleDeparments = claim.Value;
            if (string.IsNullOrWhiteSpace(roleDeparments)) { return false; }
            var isInRole = roleDeparments
                .Split(',')
                .Any(d => d == scope || d == "all");

            return isInRole;
        }

        public bool IsAdmin()
        {
            return _context.HttpContext.User.IsInRole(Roles.PayAll.admin);
        }

        public bool IsMyProfile(int empId)
        {
            return IsEmployee() && GetEmployeeId() == empId; //  && isin IsInScope(Scope.my_profile);
        }

        public bool IsMyDirectReport(int empId)
        {
            var ctx = (PayrollDbContext)_context.HttpContext.RequestServices.GetService(typeof(PayrollDbContext));
            return IsEmployee() && _context.HttpContext.User.IsInRole(Roles.Company.supervisor) && ctx.Employees.Any(a => a.Id == empId && a.ReportingEmployeeId == GetEmployeeId());
        }


        public bool CanViewBasedOnScope(int empId)
        {
            var ctx = (PayrollDbContext)_context.HttpContext.RequestServices.GetService(typeof(PayrollDbContext));
            return IsEmployee() && IsInScope(Scope.my_direct_reports) && ctx.Employees.Any(a => a.Id == GetEmployeeId() && a.EmployeeActionDirectReports.Any(t => t.Id == empId));
        }

    }
}
