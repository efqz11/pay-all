using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Payroll.Database;
using Payroll.Models;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Services
{
    public class ScheduledSystemTaskService
    {
        private readonly Database.AccountDbContext context;
        private readonly UserResolverService userResolverService;
        private readonly UserManager<AppUser> userManager;
        private readonly EventLogService eventLogService;
        private readonly PayrollDbContext payrollDbContext;

        public ScheduledSystemTaskService(Payroll.Database.AccountDbContext payrolDbContext, UserResolverService userResolverService,
            UserManager<AppUser> userManager, EventLogService eventLogService, PayrollDbContext payrollDbContext)
        {
            this.context = payrolDbContext;
            this.userResolverService = userResolverService;
            this.userManager = userManager;
            this.eventLogService = eventLogService;
            this.payrollDbContext = payrollDbContext;
        }
        

        public async Task<bool> DoScheduledSystemTask(int taskId)
        {

            var taskObj = await context.ScheduledSystemTasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (taskObj == null)
                await eventLogService.LogEventAsync(
                    result: EventLogResults.FAILURE,
                    dataType: EvengLogDataTypes.SCHEDULED_SYSTEM_TASK,
                    logType: EventLogTypes.EXECUTE_SCHEDULED_TASK,
                    key: taskId.ToString(),
                    summary: "Task was not found"
                    );

            taskObj.LastProcessedOn = DateTime.UtcNow;
            await context.SaveChangesAsync();


            (bool status, List<string> summary) result;
            bool isProcessingSuccessful = true;
            var actions = new List<string>() { $"Scheduled Task {taskObj.Name} Started" };
            switch (taskObj.Id)
            {
                case ScheduledSystemTasks.PAID_TIME_OFF_INITIAL_ACCRUALS:
                    result = await ProcessInitialPtoAccrualAsync();

                    isProcessingSuccessful = result.status;
                    if (result.summary.Any())
                        actions.AddRange(result.summary);
                    break;
                default:
                    break;
            }

            //if(!isProcessingSuccessful)
            //    await eventLogService.LogEventAsync(
            //        result: EventLogResults.FAILURE,
            //        dataType: EvengLogDataTypes.SCHEDULED_SYSTEM_TASK,
            //        logType: EventLogTypes.EXECUTE_SCHEDULED_TASK,
            //        key: taskId.ToString(),
            //        summary: "Task was not found"
            //        );


            await eventLogService.LogEventAsync(
                    result: EventLogResults.SUCCESS,
                    dataType: EvengLogDataTypes.SCHEDULED_SYSTEM_TASK,
                    logType: EventLogTypes.EXECUTE_SCHEDULED_TASK,
                    key: taskId.ToString(),
                    summary: "Task Completed Successfully",
                    actions: actions.ToArray()
                    );


            return true;

        }

        private async Task<(bool status, List<string> summary)> ProcessInitialPtoAccrualAsync()
        {
            int year = DateTime.UtcNow.Year;
            var companies = await context.CompanyAccounts.Where(a => a.Status == CompanyStatus.Approved).Select(x => new { x.Id, x.Name, x.WhenToApplyPaidTimeOffPolicyAfterJoining }).ToArrayAsync();
            var approvedCmpIds = companies.Select(a => a.Id).ToArray();
            var actions = new List<string>();


            // new dayoffRecords
            List<DayOffEmployee> newRecords = new List<DayOffEmployee>();

            // select all employees who are enrolled just today or within last 5 days
            var empls = await payrollDbContext.Employees.Where(a => a.DateOfJoined.HasValue && a.DateOfJoined >= DateTime.UtcNow.Date.AddDays(-5) && a.DateOfJoined.Value.Date <= DateTime.UtcNow.Date && approvedCmpIds.Contains(a.CompanyId) && !a.DayOffEmployees.Any(d=> d.Year == year)).ToListAsync();

            var distinctCompanies = empls.Select(a => a.CompanyId).Distinct().ToArray();
            var dayOffsOfCompanies = await payrollDbContext.DayOffs.Where(a => distinctCompanies.Contains(a.CompanyId)).ToArrayAsync();

            actions.Add($"{empls.Count()} Employee(s) found who have joined date between last 5 days who doesn't have days for year {year} and from an Approved Company");

            // run for each company
            foreach (var cmpId in distinctCompanies)
            {
                actions.Add($"Company {companies.First(a=> a.Id == cmpId).Name} ({cmpId}) requires to apply paid time off's to new Employee(s) {companies.First(a => a.Id == cmpId).WhenToApplyPaidTimeOffPolicyAfterJoining}");

                List <Employee> _empls = null;
                switch (companies.First(a=> a.Id == cmpId).WhenToApplyPaidTimeOffPolicyAfterJoining)
                {
                    case WhenToApplyPaidTimeOffPolicyAfterJoining.After_3_months:
                        _empls = empls.Where(a => a.DateOfJoined.Value.Date == DateTime.UtcNow.AddMonths(-3).Date && a.CompanyId == cmpId).ToList();
                        break;
                    case WhenToApplyPaidTimeOffPolicyAfterJoining.After_6_months:
                        _empls = empls.Where(a => a.DateOfJoined.Value.Date == DateTime.UtcNow.AddMonths(-6).Date && a.CompanyId == cmpId).ToList();
                        break;
                    case WhenToApplyPaidTimeOffPolicyAfterJoining.After_1_year:
                        _empls = empls.Where(a => a.DateOfJoined.Value.Date == DateTime.UtcNow.AddYears(-1).Date && a.CompanyId == cmpId).ToList();
                        break;
                    case WhenToApplyPaidTimeOffPolicyAfterJoining.Immediately:
                        _empls = empls.Where(a=> a.CompanyId == cmpId).ToList();
                        break;
                    default:
                        _empls = new List<Employee>();
                        break;
                }



                if(_empls.Count <= 0)
                {
                    actions.Add($"No matching employees found. moving to next company...");
                    continue;
                }

                actions.Add($"Creating Day offs for {_empls.Count()} employee(s) in Company");

                if (dayOffsOfCompanies.Any(a => a.CompanyId == cmpId))
                {
                    foreach (var dayoff in dayOffsOfCompanies.Where(a => a.CompanyId == cmpId))
                    {
                        if (true) // dayoff.IsApplicationAfterFirstYear)
                            continue;

                        newRecords.AddRange(
                            _empls.Where(a =>  
                            (dayoff.IsForSpecificGender == false || a.Gender == dayoff.Gender))
                            .Select(a => new DayOffEmployee(year, a.Id, dayoff)).ToList()
                            );

                        actions.Add($"{dayoff.Name} ({dayoff.TotalHoursPerYear} days) created for {_empls.Count(a =>  (dayoff.IsForSpecificGender == false || dayoff.Gender == a.Gender))} employee(s) · [{string.Join(", ", _empls.Where(a => (dayoff.IsForSpecificGender == false || dayoff.Gender == a.Gender)).Select(a=> a.Id).ToArray())}]");
                    }
                }

                actions.Add($"Company employees PTO's updated and moving to next...");
            }


            await payrollDbContext.DayOffEmployees.AddRangeAsync(newRecords);
            var rows = await payrollDbContext.SaveChangesAsync();
            actions.Add($"{rows} records added to database");

            return (true, actions);
        }
    }
}
