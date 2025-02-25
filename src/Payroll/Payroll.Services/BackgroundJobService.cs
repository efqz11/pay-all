using Microsoft.AspNetCore.Identity;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payroll.ViewModels;
using Payroll.Database;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Hangfire;
using System.Text;

namespace Payroll.Services
{

    public class BackgroundJobService
    {
        private readonly PayAdjustmentService payAdjustmentService;
        private readonly PayrollDbContext context;
        private readonly AccountDbContext accountDbContext;
        private readonly ScheduleService scheduleService;
        private readonly Hangfire.IBackgroundJobClient backgroundJobClient;
        private readonly UserResolverService userResolverService;
        private readonly NotificationService notificationService;
        private readonly AccessGrantService accessGrantService;
        private Models.BackgroundJob _backgroundJob;
        public string _logger { get; set; }

        public BackgroundJobService(PayrollDbContext context, AccountDbContext accountDbContext, ScheduleService scheduleService, Hangfire.IBackgroundJobClient backgroundJobClient, UserResolverService userResolverService, NotificationService notificationService)
        {
            this.context = context;
            this.backgroundJobClient = backgroundJobClient;
            this.userResolverService = userResolverService;
            this.notificationService = notificationService;
            this.accountDbContext = accountDbContext;
            this.scheduleService = scheduleService;
        }

        //public async Task<IdentityResult> ContractToggleEveryDay()
        //{
        //    var now = DateTime.UtcNow.Date;
        //    var emplIdsContractToSetActive = new List<int>();
        //    var emplIdsContractInactive = new List<int>();
        //    var allContractToStartOnToday = await context.Contracts
        //            .Where(a => a.IsActive == false && a.StartDate.Date == now).ToArrayAsync();

        //    try
        //    {
        //        if (allContractToStartOnToday.Length > 0)
        //            for (int i = 0; i < allContractToStartOnToday.Length; i++)
        //            {
        //                allContractToStartOnToday[i].IsActive = true;
        //                emplIdsContractToSetActive.Add(allContractToStartOnToday[i].EmployeeId);
        //            }


        //        var allContractActiveAndEndingSoon = await context.Contracts
        //                .Where(a => a.IsActive == true && a.EndDate.Date < now).ToArrayAsync();
        //        for (int i = 0; i < allContractActiveAndEndingSoon.Length; i++)
        //        {
        //            allContractActiveAndEndingSoon[i].IsActive = false;
        //            emplIdsContractInactive.Add(allContractActiveAndEndingSoon[i].EmployeeId);
        //        }

        //        var allEmpls = emplIdsContractToSetActive.Union(emplIdsContractInactive);

        //        var employees = await context.Employees.Where(a => allEmpls.Contains(a.Id)).ToArrayAsync();

        //        // set employee contracts ON and OFF
        //        for (int i = 0; i < emplIdsContractToSetActive.Count(); i++)
        //        {
        //            if (employees.Any(e => e.Id == emplIdsContractToSetActive[i]))
        //            {
        //                employees.First(e => e.Id == emplIdsContractToSetActive[i]).IsContractActive = true;
        //                employees.First(e => e.Id == emplIdsContractToSetActive[i]).IsContractEnding = false;

        //                if (allContractToStartOnToday.First(a => a.EmployeeId == emplIdsContractInactive[i]).ContractType == ContractType.PROBATION)
        //                    employees.First(e => e.Id == emplIdsContractToSetActive[i]).IsOnProbation = true;

        //            }
        //        }

        //        // INACTIVE OR OFF''
        //        for (int i = 0; i < emplIdsContractInactive.Count(); i++)
        //        {
        //            if (employees?.Any(e => e.Id == emplIdsContractToSetActive[i]) ?? false)
        //            {
        //                employees.First(e => e.Id == emplIdsContractToSetActive[i]).IsContractActive = false;
        //                employees.First(e => e.Id == emplIdsContractToSetActive[i]).IsContractEnding = true;
        //                employees.First(e => e.Id == emplIdsContractToSetActive[i]).IsOnProbation = false;
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        return IdentityResult.Failed(new IdentityError
        //        {
        //            Code = ex.GetType().ToString(),
        //            Description = ex.Message
        //        });
        //    }
        //    //// have intersecting employee contract to set 'active' and 'inactive'
        //    //var intersectingEmplIds = emplIdsContractToSetActive.Intersect(emplIdsContractInactive);
        //    //if(intersectingEmplIds.Any())
        //    //{
        //    //    var employees = await context.Employees.Where(a => allEmpls.Contains(a.Id)).ToArrayAsync();
                
        //    //}


        //    await context.SaveChangesAsync();
        //    return IdentityResult.Success;
        //}

        //public async Task<IdentityResult> MapContractToEmployees()
        //{
        //    _logger = "";
        //    Append("Job Started.... ");
        //    var now = DateTime.UtcNow.Date;
        //    var emplIdsContractToSetActive = new List<int>();
        //    var emplIdsContractInactive = new List<int>();
        //    var allContractToStartOnToday = await context.Contracts
        //            .Where(a => a.IsActive == false && a.StartDate.Date <= now && a.EndDate.Date <= now).ToArrayAsync();

        //    Append("all contracts started, but not ended : " + allContractToStartOnToday.Count());
        //    if (allContractToStartOnToday.Length > 0)
        //        for (int i = 0; i < allContractToStartOnToday.Length; i++)
        //        {
        //            allContractToStartOnToday[i].IsActive = true;
        //            emplIdsContractToSetActive.Add(allContractToStartOnToday[i].EmployeeId);
        //            Append("Contract " + allContractToStartOnToday[i].Name + " marked ACTIVE");
        //        }


        //    var allContractActiveAndEndingSoon = await context.Contracts
        //            .Where(a => a.IsActive == true && now <= a.EndDate.Date && (a.EndDate.Date-now).TotalDays <= 30).ToArrayAsync();
        //    Append("all contracts active and has only 30 days left until expiry : " + allContractActiveAndEndingSoon.Count());

        //    for (int i = 0; i < allContractActiveAndEndingSoon.Length; i++)
        //    {
        //        allContractActiveAndEndingSoon[i].IsActive = false;
        //        emplIdsContractInactive.Add(allContractActiveAndEndingSoon[i].EmployeeId);
        //        Append("Contract " + allContractActiveAndEndingSoon[i].Name + " marked INACTIVE");
        //    }

        //    var allEmpls = emplIdsContractToSetActive.Union(emplIdsContractInactive);
        //    var employees = await context.Employees.Where(a => allEmpls.Contains(a.Id)).ToArrayAsync();

        //    if (allEmpls.Count() > 0)
        //    {
        //        Append("!Has many conflicting employees to update contract");
        //        allEmpls.Select(x => new { id = x, name = employees.First(a => a.Id == x).Name, empId = employees.First(a => a.Id == x).EmpID }).ToList().ForEach(a =>
        //        {
        //            Append($"{a.empId} · {a.name}  (#{a.id})");
        //        });

        //    }


        //    // set employee contracts ON and OFF
        //    Append("Activating Employee mapping to contracts : " + emplIdsContractToSetActive.Count());
        //    for (int i = 0; i < emplIdsContractToSetActive.Count(); i++)
        //    {
        //        if (employees.Any(e => e.Id == emplIdsContractToSetActive[i]))
        //        {
        //            employees.First(e => e.Id == emplIdsContractToSetActive[i]).IsContractActive = true;
        //            employees.First(e => e.Id == emplIdsContractToSetActive[i]).IsContractEnding = false;

        //            if (allContractToStartOnToday.First(a => a.EmployeeId == emplIdsContractToSetActive[i]).ContractType == ContractType.PROBATION)
        //                employees.First(e => e.Id == emplIdsContractToSetActive[i]).IsOnProbation = true;

        //            Append($"{employees.First(e => e.Id == emplIdsContractToSetActive[i]).EmpID} · {employees.First(e => e.Id == emplIdsContractToSetActive[i]).Name}  (#{emplIdsContractToSetActive[i]}) (activated)");
        //        }
        //    }

        //    // INACTIVE OR OFF''
        //    Append("Removing Employee mapping from ended contracts contracts : " + emplIdsContractInactive.Count());
        //    for (int i = 0; i < emplIdsContractInactive.Count(); i++)
        //    {
        //        if (employees.Any(e => e.Id == emplIdsContractInactive[i]))
        //        {
        //            var _end = allContractActiveAndEndingSoon.First(c => c.EmployeeId == emplIdsContractInactive[i]).EndDate;
        //            if ((now - _end).TotalDays < 1)
        //            {
        //                employees.First(e => e.Id == emplIdsContractInactive[i]).IsContractActive = false;
        //                employees.First(e => e.Id == emplIdsContractInactive[i]).IsContractEnding = true;
        //                employees.First(e => e.Id == emplIdsContractInactive[i]).IsContractEnded = true;
        //                employees.First(e => e.Id == emplIdsContractInactive[i]).IsOnProbation = false;
        //                Append($"{employees.First(e => e.Id == emplIdsContractInactive[i]).EmpID} · {employees.First(e => e.Id == emplIdsContractInactive[i]).Name}  (#{emplIdsContractInactive[i]}) (!ended)");
        //            }
        //            else
        //            {
        //                employees.First(e => e.Id == emplIdsContractInactive[i]).IsContractEnding = true;
        //                Append($"{employees.First(e => e.Id == emplIdsContractInactive[i]).EmpID} · {employees.First(e => e.Id == emplIdsContractInactive[i]).Name}  (#{emplIdsContractInactive[i]}) (ending)");
        //            }

        //        }
        //    }

        //    Append("Job Ended.... ");
        //    await accountDbContext.TaskRunReports.AddRangeAsync(new TaskRunReport
        //    {
        //        JobName = "MapContractToEmployees",
        //        IsRecurringJob = true,
        //        Report = _logger.ToString(),
        //        TaskReportType = TaskReportType.MapContractToEmployeeWeekly
        //    });

        //    //// have intersecting employee contract to set 'active' and 'inactive'
        //    //var intersectingEmplIds = emplIdsContractToSetActive.Intersect(emplIdsContractInactive);
        //    //if(intersectingEmplIds.Any())
        //    //{
        //    //    var employees = await context.Employees.Where(a => allEmpls.Contains(a.Id)).ToArrayAsync();

        //    //}

        //    await context.SaveChangesAsync();
        //    await accountDbContext.SaveChangesAsync();

        //    return IdentityResult.Success;
        //}

        private void Append(string v)
        {
            if (!string.IsNullOrWhiteSpace(_logger))
                _logger += "\n";

            _logger += v;
        }

        public async Task<IdentityResult> CreateNewWorkItem(Guid identifier, TaskType taskType)
        {
            var backJob = await context.BackgroundJobs.FirstOrDefaultAsync(x => x.Identifier == identifier && x.TaskStatus == Models.TaskStatus.Scheduled);
            if (backJob == null)
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"Scheduled Background job {identifier} was not found. It might have been removed or otherwise completed"
                });


            var sch = await context.Schedules.FindAsync(backJob.ScheduleId);
            if (sch == null)
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"Schedule for background job {identifier} - {backJob.TaskType} was not found"
                });


            var days = sch.DaysData;

            var allWorks = await context.Works
                .Where(x => x.CompanyId == sch.CompanyId)
                .ToListAsync();

            if (allWorks.Count <= 0) throw new Exception("Work was not found");

            DateTime? nextRunDate = sch.NextRunDate;

            var newWorkItems = scheduleService.GenerateWorkItemsForLoopRepeating
                    (1, allWorks, sch, out nextRunDate, overriddenStartDate: sch.NextRunDate);


            if (newWorkItems.Any())
            {
                backJob.TaskStatus = Models.TaskStatus.Ended;
                backJob.NextRunDate = nextRunDate;
                backJob.EndedDate = DateTime.Now;

                context.WorkItems.AddRange(newWorkItems);
            }


            if (sch.IsRepeatEndDateNever && nextRunDate.HasValue)
            {
                // Repeat end date is never 
                // => then schedule background job to run
                var nextBackJob = await scheduleService.CreateNewBackgroundJobAsync(sch, nextRunDate.Value);

                var hangfireJobId = backgroundJobClient
                        .Schedule(() =>
                                    CreateNewWorkItem(nextBackJob.Identifier, nextBackJob.TaskType),
                                    nextBackJob.RunDate);

                nextBackJob.HangfireJobId = hangfireJobId;
                sch.backgroundJobs.Add(nextBackJob);
                sch.HasBackgroundJob = true;
                //sch.BackgroundJob = nextBackJob;
                sch.HangfireJobId = hangfireJobId;
                sch.NextRunDate = nextRunDate;

            }


            await context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<Models.BackgroundJob> CreateNewBackgroundJobAsync(Announcement ann, TaskType taskType, DateTime nextRunDate)
        {
            if (await context.BackgroundJobs.AnyAsync(x => x.IsActive && x.TaskStatus == Models.TaskStatus.Scheduled && x.AnnouncementId == ann.Id))
                if (ann.Start > DateTime.Now.Date) throw new ApplicationException("Cannot schedule for future date");

            return new Models.BackgroundJob
            {
                Name = taskType.GetDisplayName(),
                Details = ann.Title.LimitTo(100),
                Identifier = Guid.NewGuid(),
                AnnouncementId = ann.Id,
                RunDate = nextRunDate,
                TaskType = taskType,
                TaskStatus = Models.TaskStatus.Scheduled,
                CompanyAccountId = userResolverService.GetCompanyId()
            };
        }

        public async Task ScheduleAnnounceMmentPublishDate(Announcement ann)
        {
            if (ann.Start.HasValue == false)
                throw new Exception("Scheduled announcement doesnt have start date");
            var newJob = await CreateNewBackgroundJobAsync(ann, TaskType.PublishAnnouncement, ann.Start.Value);

            newJob.HangfireJobId =  backgroundJobClient.Schedule(() => PublishAnnouncement(newJob.Id, ann.Title), ann.Start.Value);

            ann.backgroundJobs.Add(newJob);
        }

        public async Task PublishAnnouncement(int id, string title)
        {
            await GetBackgrundJobAsync(id);
            var ann = await context.Announcements
                        .FirstOrDefaultAsync(x => x.Id == id && x.Status == AnnouncementStatus.Published);
            if (ann != null)
            {
                ann.Status = AnnouncementStatus.Published;
                ann.PublishedDate = ann.Start.Value;

                if (ann.End.HasValue == false)
                    throw new Exception("Scheduled announcement doesnt have start date");

                /// send notifications
                await notificationService.SendToEmployeesAsync(
                    type: NotificationTypeConstants.PublishAnnouncement,
                    obj: ann,
                    companyAccountId: ann.CompanyId,
                    empIds: ann.EmployeeSelectorVm.EmployeeIds);
                ann.TotalInteractionsCount = ann.EmployeeSelectorVm?.EmployeeIds?.Count() ?? 0;

                var newJob = await CreateNewBackgroundJobAsync(ann, TaskType.ExpireAnnouncement, ann.End.Value);

                ann.backgroundJobs.Add(newJob);
                UpdateBackgroundJobAsync();
                newJob.HangfireJobId =  backgroundJobClient.Schedule(() => ExpireAnnouncement(newJob.Id, ann.Title), ann.End.Value);

                await context.SaveChangesAsync();
            }
        }

        private async Task ExpireAnnouncement(int id, string title)
        {
            var ann = await context.Announcements
                        .FirstOrDefaultAsync(x => x.Id == id && x.Status == AnnouncementStatus.Published);
            if (ann != null)
            {
                ann.Status = AnnouncementStatus.Expired;
                ann.ExpiredDate = ann.End.Value;
                await context.SaveChangesAsync();
            }
        }

        private void UpdateBackgroundJobAsync()
        {
            if (_backgroundJob == null)
                throw new ApplicationException("background job was not found!");

            _backgroundJob.TaskStatus = Models.TaskStatus.Ended;
            _backgroundJob.EndedDate = DateTime.UtcNow;
        }

        private async Task GetBackgrundJobAsync(int id)
        {
            _backgroundJob = await context.BackgroundJobs.FindAsync(id);
            if (_backgroundJob == null)
                throw new ApplicationException("background job was not found!");

            if (_backgroundJob.TaskStatus != Models.TaskStatus.Scheduled)
                throw new ApplicationException("background job state was not scheduled");

        }
    }
}
