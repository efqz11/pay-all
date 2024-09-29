using Hangfire.Dashboard;
using Payroll.Models;

namespace Payroll.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.IsInRole(Roles.PayAll.admin);
        }
    }
}
