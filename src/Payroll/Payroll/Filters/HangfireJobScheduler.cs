using Hangfire;
using Hangfire.Dashboard;
using Payroll.Models;

namespace Payroll.Filters
{
    public class HangfireJobScheduler
    {
        public static void ScheduleRecurringJobs()
        {
            //RecurringJob.RemoveIfExists(nameof(Payroll.Services.BackgroundJobService) + ".ContractToggleEveryDay");
            //RecurringJob.AddOrUpdate<Services.BackgroundJobService>(
            //    methodCall: job => job.ContractToggleEveryDay(),
            //    cronExpression: Cron.Daily);


            //RecurringJob.RemoveIfExists(nameof(Payroll.Services.BackgroundJobService) + ".MapContractToEmployees");
            //RecurringJob.AddOrUpdate<Services.BackgroundJobService>(
            //    methodCall: job => job.MapContractToEmployees(),
            //    cronExpression: Cron.Weekly);
        }
    }
}
