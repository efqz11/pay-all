using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Helpers;
using ChartJSCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;

namespace Payroll.Controllers
{
    public class HomeController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly SearchService searchService;
        private readonly AccountDbContext accountDb;
        private readonly LogDbContext logDbContext;
        private readonly UserResolverService userResolverService;
        private readonly ScheduleService scheduleService;
        private readonly EmployeeService employeeService;
        private readonly AccessGrantService accessGrantService;
        private readonly FileUploadService fileUploadService;
        private readonly CompanyService companyService;
        private readonly BackgroundJobService backgroundJobService;
        private readonly ILogger<HomeController> logger;
        private readonly NotificationService notificationService;

        public HomeController(PayrollDbContext context, SearchService searchService, AccountDbContext accountDb, LogDbContext logDbContext, UserResolverService userResolverService, ScheduleService scheduleService, EmployeeService  employeeService, AccessGrantService accessGrantService, FileUploadService fileUploadService, CompanyService companyService, BackgroundJobService backgroundJobService, ILogger<HomeController> logger, NotificationService notificationService)
        {
            this.context = context;
            this.searchService = searchService;
            this.accountDb = accountDb;
            this.logDbContext = logDbContext;
            this.userResolverService = userResolverService;
            this.scheduleService = scheduleService;
            this.employeeService = employeeService;
            this.accessGrantService = accessGrantService;
            this.fileUploadService = fileUploadService;
            this.companyService = companyService;
            this.backgroundJobService = backgroundJobService;
            this.logger = logger;
            this.notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(Roles.PayAll.admin))
                return RedirectToAction(nameof(Admin));

            if (User.IsHrManagerOrAdmin())
                return RedirectToAction(nameof(Admin));

            return RedirectToAction(nameof(Manager));
        }

        
        public async Task<IActionResult> Admin()
        {
            // if (!User.IsInRole(Roles.PayAll.admin))
            //     return RedirectToAction(nameof(Manager));

            var item = new HomeAdminVm
            {
                DepartmentCount = await context.Departments.CountAsync(x => x.CompanyId == userResolverService.GetCompanyId()),
                CompanyAccountCount = await accountDb.CompanyAccounts.CountAsync(),
                WorkCount = await context.Works.CountAsync(x => x.CompanyId == userResolverService.GetCompanyId()),
                PayAdjustmentCount = await context.PayAdjustments.CountAsync(x => x.CompanyId == userResolverService.GetCompanyId()),
                //CompanyAccountCount = await accountDb.CompanyAccounts.CountAsync(),
                EmployeeCount = await context.Departments.Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .SelectMany(x => x.Employees).CountAsync(),

                DepartmentByEmployeePie = GetComapreHorizontalBarGraph(await context.Departments.Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .SelectMany(x => x.Employees).GroupBy(x => x.Department.Name)
                .OrderByDescending(x => x.Count())
                .Select(x => new ComapareData { Key = x.Key, CurrentValue = decimal.Parse(x.Count().ToString()) })
                .ToListAsync(), "Employee by Department"),

                JobTypeByEmployeePie = GetComapreHorizontalBarGraph(await context.Departments.Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .SelectMany(x => x.Employees).GroupBy(x => x.JobType)
                .OrderByDescending(x => x.Count())
                .Select(x => new ComapareData { Key = x.Key.GetDisplayName(), CurrentValue = decimal.Parse(x.Count().ToString()) })
                .ToListAsync(), "Employee Status"),

                AttendanceTodayAndThisWeek = await context.Attendances.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Date >= DateTime.UtcNow.Date && x.Date < DateTime.UtcNow.AddDays(7))
                .Include(x=> x.Employee)
                    .ThenInclude(x=> x.Department)
                .ToListAsync(),
                
                PendingTmeSheetCount = await context.Attendances.CountAsync(x=> !x.IsPublished && x.HasClockRecords),
                // PendingShiftsToPublishCount = await context.Attendances.CountAsync(x=> !x.IsPublished),

                UpcomingBirthDays = await context.Departments.Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .SelectMany(x => x.Employees).Where(x => x.DateOfBirth.HasValue && x.DateOfBirth.Value >= DateTime.UtcNow && x.DateOfBirth.Value <= DateTime.UtcNow.AddMonths(4))
                .ToListAsync(),

                NewCompaniesRegistering = await accountDb.CompanyAccounts.Where(x => x.Status == CompanyStatus.Pending)
                .ToListAsync(),

                //LastPayrol = await context.PayrollPeriods.Where(x => x.StartDate.Date <= DateTime.UtcNow.Date && x.EndDate.Date >= DateTime.UtcNow.Date).FirstOrDefaultAsync(),

                Announcements = await context.Announcements.Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .Include(x => x.CreatedEmployee)
                .Include(x => x.FileDatas)
                .OrderByDescending(x => x.Id).Take(2).ToListAsync(),
                
                // RequestNotificationsDictionry = await context.Requests.Where(x=> x.CompanyId == userResolverService.GetCompanyId() && x.Status == WorkItemStatus.Submitted).GroupBy(z=> z.RequestType)
                // .AsQueryable()
                // .ToDictionaryAsync(a=> a.Key, a=> a.Count())
            };

            
            var q = context.Requests.Where(x=> x.CompanyId == userResolverService.GetCompanyId() && x.Status == WorkItemStatus.Submitted).GroupBy(z=> z.RequestType)
                .Select(x => new { x.Key, Count = x.Count()});
                item.RequestNotificationsDictionry  = await q.ToDictionaryAsync(a=> a.Key, a=> a.Count);
            


            //return new HomeAdminVm
            //{
            //    CompanyAccountCount = await accountDb.CompanyAccounts.CountAsync(),
            //    CompanyCount = await context.Companies.CountAsync(),
            //    NewCompaniesRegistering = await accountDb.CompanyAccounts.Where(x=> x.Status == CompanyStatus.PendingApproval).ToListAsync(),

            //}
            //var xx = context.PayrollPeriods
            //    .Include(x => x.PayrollPeriodEmployees)
            //    .First();

            //context.Entry(xx).Collection(e => e.PayrollPeriodEmployees)
            //    .Query().OfType<PayrollPeriodEmployee>()
            //    .Include(x=> x.PayrollPeriodAdditions)
            //    .Include(x => x.PayrollPeriodDeductions)
            //    .Include(e => e.Employee)
            //    .Load();

            return View(item);
        }


        public async Task<IActionResult> Manager(DateTime? start = null, DateTime? end = null, int type = 0, bool showSeen = false, int page = 1, int limit = 10)
        {
            var _start = start.HasValue ? start.Value : scheduleService.thisWeekStart;
            var _end = end.HasValue ? end.Value : scheduleService.thisWeekEnd;
            var empId = userResolverService.GetEmployeeId();
            var emp = await employeeService.GetEmployeeById(empId);
            var vm = new HomeEmployeeVm
            {
                Employee = emp,
                Notifications = await notificationService.GetEmployeeNotifications(
                    empId,
                    _start,
                    _end,
                    showSeen,
                    type,
                    page,
                    limit),
                //EmployeeUserId = id,
                FilterForm = new FilterForm
                {
                    //UserId = id,
                    EmployeeId = empId,
                    Start = start,
                    End = end,
                    Type = type,
                    ShowSeen = showSeen,
                    Page = page,
                    Limit = limit,
                },
                MyTeam = await employeeService.GetAllEmployeesInMyDepartment(emp),
                MyDepartment = await employeeService.GetAllEmployeesInMyDepartment(emp),
                WhoIsOut = await companyService.GetWhosIsOutList()
                //AttedanceSchedule = await scheduleService.GetWhosIsOutList(empId: userResolverService.GetEmployeeId()),
            };

            ViewBag.ShowDetails = true;
            ViewBag.Start = _start;
            ViewBag.End = _end;
            ViewBag.Employee = emp;
            ViewBag.DurationText = scheduleService.GetDurationText(_start, _end, includeDays: false);
            ViewBag.NotificationTypes = new SelectList(await notificationService.GetNotificationTypes(), "Id", "Type", type);
            return View(vm);
        }

        public async Task<IActionResult> GetScheduleChartWithAbsentEmployee()
        {
            var start = DateTime.Now.AddDays(-30).Date;
            var end = DateTime.Now.AddDays(30).Date;
            var totalDays = (end - start).TotalDays;
            var chartdata = await context.Attendances
                .Where(x => x.Date >= start && x.Date <= end)
                .GroupBy(x => new { x.Date })
                .Select(x => new
                {
                    Date = x.Key,
                    DateString = x.Key.Date.ToString("yyyy-MM-dd hh:mm"),
                    TotalScheduledHours = x.Sum(a => EF.Functions.DateDiffHour(a.WorkStartTime, a.WorkEndTime)),
                    ActualWorkedHours = x.Sum(a => a.TotalWorkedHours),
                    ActualWorkedHoursPerSchedule = x.Sum(a => a.TotalHoursWorkedPerSchedule),
                    TotalScheduledEmployees = x.Count(),
                    TotalAbsentEmployeeCount = x.Count(a => a.CurrentStatus == AttendanceStatus.Absent),
                    LateEmployeeCount = x.Count(a => a.CurrentStatus == AttendanceStatus.Late),
                    TotalLateHours = x.Sum(a => (int)(a.TotalLateMins / 60)),
                }).ToListAsync();


            var taskCompletionData = await context.WorkItems
                .Where(x => x.Date >= start && x.Date <= end && x.Work != null && x.IsEmployeeTask == false)
                .GroupBy(x => x.Work.Type)
                .Select(x => new
                {
                    Type = x.Key,
                    TotalWorkAssigned = x.Count(),
                    TotalFailed = x.Count(a => a.IsCompleted && a.Status == WorkItemStatus.FailedWithDeduction),
                    TotalCompleted = x.Count(a => a.IsCompleted && a.Status == WorkItemStatus.Completed),
                    TotalApproved = x.Sum(a => a.TotalApproved),
                    RemainingSubmissions = x.Sum(a => a.RemainingSubmissions),
                }).ToListAsync();

            var clockInTaskPerformace = new Dictionary<string, int>();
            clockInTaskPerformace.Add("Completed", taskCompletionData.Where(x => x.Type == WorkType.ExpectClockInRecordsBefore).Sum(a => a.TotalCompleted));
            clockInTaskPerformace.Add("Failed", taskCompletionData.Where(x => x.Type == WorkType.ExpectClockInRecordsBefore).Sum(a => a.TotalFailed));
            clockInTaskPerformace.Add("Waiting", taskCompletionData.Where(x => x.Type == WorkType.ExpectClockInRecordsBefore)
                .Sum(a => a.TotalWorkAssigned - (a.TotalApproved + a.TotalFailed)));


            var submissionTaskPerformace = new Dictionary<string, int>();
            submissionTaskPerformace.Add("Completed", taskCompletionData.Where(x => x.Type == WorkType.RequireSubmissions).Sum(a => a.TotalCompleted));
            submissionTaskPerformace.Add("Failed", taskCompletionData.Where(x => x.Type == WorkType.RequireSubmissions).Sum(a => a.TotalFailed));
            submissionTaskPerformace.Add("Waiting", taskCompletionData.Where(x => x.Type == WorkType.RequireSubmissions)
                .Sum(a => a.TotalWorkAssigned - (a.TotalApproved + a.TotalFailed)));

            // var scheduleChartWithAbsentEmployee = chartdata;

            return Json(new
            {
                chartdata,
                clockInTaskPerformace = clockInTaskPerformace.Select(x => new { x.Key, x.Value }).ToArray(),
                submissionTaskPerformace = submissionTaskPerformace.Select(x => new { x.Key, x.Value }).ToArray()
            });
        }


        public async Task<IActionResult> GetAttendanceStatusGraph(TimeFrame tf)
        {
            var start = DateTime.Now;
            scheduleService.SetBaseDate(start);
            var end = DateTime.Now.AddDays(1).Date;
            if(tf == TimeFrame.Weekly){
                start = scheduleService.thisWeekStart;
                end = scheduleService.thisWeekEnd;
            }
            else if (tf == TimeFrame.Monthly){
                start = scheduleService.thisMonthStart;
                end = scheduleService.thisMonthEnd;
            }

            var totalDays = (end - start).TotalDays;

            var chartdata = await context.Attendances
                .Where(x => x.Date >= start && x.Date <= end)
                .GroupBy(x => x.CurrentStatus)
                .Select(x => new
                {
                    Status = x.Key.GetDisplayName(),
                    Total = x.Count(),
                }).ToListAsync();
            return Json(chartdata);
        }


        public async Task<IActionResult> GetAttendanceByStatus(DateTime? date = null)
        {
            var start = date?.Date ?? DateTime.Now.Date;
            ViewBag.Date = start;

            var attnd = await context.Attendances.Where(x => x.Date.Date == date
            && (x.CurrentStatus == AttendanceStatus.Late || x.CurrentStatus == AttendanceStatus.Absent ||  x.CurrentStatus == AttendanceStatus.Early))
                .OrderByDescending(a => a.CheckInTime)
                .Include(x=> x.Employee)
                .ThenInclude(x=> x.Department)
                .ToListAsync();

            return PartialView("_ViewAbsentLateEmployees", attnd);
        }

        //public async Task<IActionResult> UpdateWeeklySchedule(DateTime? date = null, DateTime? end = null)
        //{
        //    ViewBag.IsDisplayedInDahsboard = true;
        //    if (date.HasValue)
        //        scheduleService.SetBaseDate(date);
        //    //if (end.HasValue)
        //    //    scheduleService.SetEndDate(end);

        //    ViewBag.WeekStart = scheduleService.thisWeekStart;
        //    ViewBag.WeekEnd = end?? scheduleService.thisWeekEnd;
        //    ViewBag.CurrentRangeDisplay = scheduleService.thisWeekStart.ToString("MMM dd") + " - " +
        //        (scheduleService.thisWeekEnd.ToString(scheduleService.thisWeekEnd.Month == scheduleService.thisWeekStart.Month ? "dd, yyyy" : "MMM dd, yyyy"));

        //    var item = new HomeEmployeeVm
        //    {
        //        AttedanceSchedule = await scheduleService.GetCurrentSecdule(empId: userResolverService.GetEmployeeId()),
        //        WorkSchedule = await scheduleService.GetCurrentSecduledTasks(empId: userResolverService.GetEmployeeId()),
        //        WeekScheduleTasks = await scheduleService.GetSecduledTasksForThisWeek(empId: userResolverService.GetEmployeeId()),
        //        WorkTimes = await context.CompanyWorkTimes
        //        .Where(x => x.CompanyId == userResolverService.GetCompanyId())
        //        .OrderBy(x => x.StartTime)
        //        .ToListAsync()
        //    };

        //    return PartialView("_Schedule", item);
        //}

        public async Task<IActionResult> Employee()
        {
            var _start =  scheduleService.thisWeekStart;
            var _end = scheduleService.thisWeekEnd;
            ViewBag.Complete = "on";
            ViewBag.IsDisplayedInDahsboard = true;
            //SetViewBag();
            

            var item = new HomeEmployeeVm
            {
                //Announcements = await companyService.GetAnnouncementsForEmployee(),
                AttedanceSchedule = await scheduleService.GetCurrentSecdule(empId: userResolverService.GetEmployeeId()),
                WorkSchedule = await scheduleService.GetCurrentSecduledTasks(empId: userResolverService.GetEmployeeId()),
                Employee = await employeeService.GetCurrentEmployeeWithDepartment(),
                DayOffs = await employeeService.GetCurrentEmployeeDayOffs(),
                WeekScheduleTasks = await scheduleService.GetSecduledTasksForThisWeek(empId: userResolverService.GetEmployeeId()),
                WorkTimes = await companyService.GetWorkTimes(),
            };
            
            return View(item);
        }

        public async Task<IActionResult> GetEmployeeCard()
        {
            var _start = scheduleService.thisWeekStart;
            var _end = scheduleService.thisWeekEnd;
            ViewBag.Complete = "on";
            ViewBag.IsDisplayedInDahsboard = true;
            SetViewBag();
            int empId = userResolverService.GetEmployeeId();

            var keyValues = await employeeService.GetEmployeeCardAsync(empId, _start, _end);
            ViewBag.Grade = keyValues.Grade;
            ViewBag.Employee = await context.Employees
                .Include(x=> x.KpiValues)
                .FirstOrDefaultAsync(x=> x.Id == empId);

            return PartialView("_EmployeeCard", ViewBag.Employee);
        }

        public async Task<IActionResult> Sidebar()
        {
            ViewBag.IsDisplayedInDahsboard = true;
            SetViewBag();

            var item = new HomeEmployeeVm
            {
                Employee = await employeeService.GetCurrentEmployeeWithDepartment(),
                DayOffs = await employeeService.GetCurrentEmployeeDayOffs(),
                WeekScheduleTasks = await scheduleService.GetSecduledTasksForThisWeek(empId: userResolverService.GetEmployeeId()),
                WorkTimes = await companyService.GetWorkTimes(),
                
            };

            return PartialView("_SideBar", item);
        }
        
        private void SetViewBag()
        {
            ViewBag.WeekStart = scheduleService.thisWeekStart;
            ViewBag.WeekEnd = scheduleService.thisWeekEnd;
            ViewBag.CurrentRangeDisplay = scheduleService.thisWeekStart.ToString("MMM dd") + " - " +
                (scheduleService.thisWeekEnd.ToString(scheduleService.thisWeekEnd.Month == scheduleService.thisWeekStart.Month ? "dd, yyyy" : "MMM dd, yyyy"));
        }

        //public async Task<IActionResult> EmployeeHome(DateTime? start = null, DateTime? end = null)
        //{
        //    var _start = start.HasValue ? start.Value : scheduleService.thisWeekStart;
        //    var _end = end.HasValue ? end.Value : scheduleService.thisWeekEnd;

        //    var taskData = await context.WorkItems.Where(x => x.EmployeeId == userResolverService.GetEmployeeId()
        //            && x.IsEmployeeTask && x.Date >= _end && x.Date <= _end)
        //            .GroupBy(x => new { x.Work, x.WorkId, x.Status })
        //            .Select(x => new TaskData
        //            {
        //                WorkId = x.Key.WorkId,
        //                WorkName = x.Key.Work.Name,
        //                WorkType = x.Key.Work.Type,
        //                Status = x.Key.Status,
        //                TotalCompleted = x.Count(z => z.Status == WorkItemStatus.Completed),
        //                CompletionCount = x.Count(z => z.Status == WorkItemStatus.Completed) > 0 ? x.Sum(z => z.TotalApproved)/ x.Sum(z => z.Work.TotalRequiredCount) : 0f,
        //                TotalFailed = x.Count(z => z.Status == WorkItemStatus.FailedWithDeduction),
        //                FailedCount = x.Count(z => z.Status == WorkItemStatus.FailedWithDeduction) > 0 ? x.Sum(z => z.TotalApproved) / x.Sum(z => z.Work.TotalRequiredCount) : 0f,
        //                TotalRequired = x.Sum(z => z.Work.TotalRequiredCount)
        //            }).ToListAsync();
        //    //var taskCompletionOverview = taskData
        //    //    .Where(z => z.Status == WorkItemStatus.Completed)
        //    //    .GroupBy(x => x.WorkType)
        //    //    .Select(x => new TaskCompletionData
        //    //    {
        //    //        Name = x.Key,
        //    //        TotalRequired = x.Sum(z => z.TotalRequired),
        //    //        TotalCompleted = x.Sum(z=> z.TotalCompleted)
        //    //    }).FirstOrDefault();
        //    //var FailedTasks = taskData
        //    //    .Where(z => z.Status == WorkItemStatus.FailedWithDeduction)
        //    //    .GroupBy(x => x.WorkType)
        //    //    .Select(x => new TaskFailedData
        //    //    {
        //    //        Name = x.Key,
        //    //        TotalRequired = x.Sum(z => z.TotalRequired),
        //    //        TotalCompleted = x.Sum(z => z.TotalCompleted)
        //    //    }).FirstOrDefault();
        //    var vm = new HomeEmployeeVm
        //    {
        //        Announcements = await companyService.GetAnnouncementsForEmployee(limit:2),
        //        AttedanceSchedule = await scheduleService.GetCurrentSecdule(empId: userResolverService.GetEmployeeId()),
        //        WorkSchedule = await scheduleService.GetCurrentSecduledTasks(empId: userResolverService.GetEmployeeId()),
        //        Employee = await employeeService.GetCurrentEmployeeWithDepartment(),
        //        DayOffs = await employeeService.GetCurrentEmployeeDayOffs(),
        //        WeekScheduleTasks = await scheduleService.GetSecduledTasksForThisWeek(empId: userResolverService.GetEmployeeId()),
        //        WorkTimes = await accountDb.CompanyAccountWorkTimes
        //        .Where(x => x.CompanyAccountId == userResolverService.GetCompanyId())
        //        .OrderBy(x => x.StartTime)
        //        .ToListAsync(),

        //        TaskCompletionRates = taskData
        //    };

        //    ViewBag.Start = _start;
        //    ViewBag.End = _end;
        //    ViewBag.DurationText = scheduleService.GetDurationText(_start, _end, includeDays: false);
        //    return PartialView("_EmployeeHome", vm);
        //}

        public async Task<IActionResult> Sb_WelcomeBack(int id)
        {
            var empId = id;
            var vm = new HomeEmployeeVm
            {
                //Announcements = await companyService.GetAnnouncementsForEmployee(),
                //AttedanceSchedule = await scheduleService.GetCurrentSecdule(empId: userResolverService.GetEmployeeId()),
                //WorkSchedule = await scheduleService.GetCurrentSecduledTasks(empId: userResolverService.GetEmployeeId()),
                Employee = await employeeService.GetCurrentEmployeeWithDepartment(id),
                DayOffs = await employeeService.GetCurrentEmployeeDayOffs(id),
                WeekScheduleTasks = await scheduleService.GetSecduledTasksForThisWeek(empId: empId, complete: "off"),
                WorkTimes = await companyService.GetWorkTimes(),
                PublicHolidaysUpcoming = await companyService.GetUpComingPublicHolidays(limit: 2),
                AwaitingRequestCount = await context.Requests.CountAsync(a => a.EmployeeId == empId &&
                a.Status == WorkItemStatus.Submitted),
                //Announcements = await companyService.GetAnnouncementsForEmployee(limit: 2),
            };

            var timeSheetStats = await context.Attendances.Where(a => a.Date >= scheduleService.thisWeekStart && a.Date <= scheduleService.thisWeekEnd && a.IsPublished && a.EmployeeId == empId)
                .AsQueryable()
                .GroupBy(a => a.Date.Date)
                .Select(a=> new { a.Key, Count = a.Sum(x=> x.TotalWorkedHours) })
                .ToDictionaryAsync(x=> x.Key, x=> x.Count);

            var dates = Enumerable.Range(0, 7).Select((i, a) => 
            new Tuple<DateTime, double>(
                scheduleService.thisWeekStart.AddDays(i),
                timeSheetStats.FirstOrDefault(x => x.Key == scheduleService.thisWeekStart.AddDays(i)).Value));
            vm.ThisWeekTimeSheetStats = dates;

            // time off balances
            var dayOffBalances = await employeeService.GetCurrentEmployeeDayOffBalances(empId);
            var weeklyTaskStatus = await scheduleService.GetSecduledTasksForThisWeekByStatus(empId);

            ViewBag.dayOffBalances = dayOffBalances;
            ViewBag.weeklyTaskStatus = weeklyTaskStatus;
            return PartialView("_EmployeeWelcome", vm);
        }



        public async Task<IActionResult> WorkItemSubmissions(int? deptId = null, int? empId = null, DateTime? start = null, DateTime? end = null, int? wId = null, WorkItemStatus? status = null)
        {

            ViewBag.EmployeeId = new SelectList(await accessGrantService.GetEmployeesOfCurremtCompany(true), "Id", "Name");
            ViewBag.WorkId = new SelectList(await context.Works.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.IsAdvancedCreate).ToArrayAsync(), "Id", "Name");

            var vm = await context.WorkItemSubmissions.Where(x => x.WorkItem != null &&
            x.WorkItem.Work.CompanyId == userResolverService.GetCompanyId() &&
            (x.Status == WorkItemStatus.Submitted) &&
            (deptId == null || x.WorkItem.Employee.DepartmentId == deptId) &&
            (empId == null || x.WorkItem.EmployeeId == empId) &&
            (wId == null || x.WorkItem.WorkId == wId) &&
            ((!start.HasValue && !end.HasValue) || start >= x.WorkItem.WorkStartTime && end <= x.WorkItem.WorkEndTime))
            .Select(x=> x.WorkItem).Distinct()
            .Include(x => x.Employee)
                .ThenInclude(x => x.Department)
            .Include(x=> x.Work)
            .Include(x => x.WorkItemSubmissions)
            .ToListAsync();

            return PartialView("_AdminWorkApproval", vm);
        }

        public async Task<IActionResult> WorkItemSubmissionsApproval(int wiId, int id, int approve = 0)
        {
            var vm = await context.WorkItemSubmissions.Where(x =>
            x.Status == WorkItemStatus.Submitted &&
            x.Id == id && x.WorkItemId == wiId)
            .Include(x => x.WorkItem)
                .ThenInclude(x => x.Employee)
                    .ThenInclude(x => x.Department)
            .Include(x=> x.WorkItem)
                .ThenInclude(x=> x.Work)
            .FirstOrDefaultAsync();

            if (vm == null)
                return ThrowJsonError("Work item was not found");

            if (approve == 1)
                vm.IsApproved = true;

            return PartialView("_AdminWorkApprovalAction", vm);
        }

        [HttpPost]
        public async Task<IActionResult> WorkItemSubmissionsApproval(WorkItemSubmission model)
        {
            var vm = await context.WorkItemSubmissions.Where(x =>
            (x.Status == WorkItemStatus.Submitted) &&
            x.Id == model.Id && x.WorkItemId == model.WorkItemId)
            //.Include(x => x.WorkItem)
            //    .ThenInclude(x => x.Employee)
            //        .ThenInclude(x => x.Department)
            //.Include(x => x.WorkItem)
            //    .ThenInclude(x => x.Work)
            .FirstOrDefaultAsync();

            var workItem = await context.WorkItems.Where(x =>
            x.Id == model.WorkItemId)
            .Include(x => x.Work)
            .Include(x => x.WorkItemSubmissions)
            .FirstOrDefaultAsync();


            if (vm == null)
                return ThrowJsonError("Work item was not found");

            vm.ActionTakenDate = DateTime.UtcNow;
            vm.ActionTakenUserId = userResolverService.GetUserId();
            vm.ActionTakenUserName = userResolverService.GetUserName();
            vm.ActionTakenReasonSummary = model.ActionTakenReasonSummary;


            if (model.IsApproved == false)
            {
                if (string.IsNullOrWhiteSpace(model.ActionTakenReasonSummary))
                    return ThrowJsonError("Please enter reason for the employee");

                vm.Status = WorkItemStatus.Rejected;
                workItem.TotalSubmitted -= 1;
                workItem.RemainingSubmissions += 1;
                workItem.PercentSubmitted = workItem.GetSubmittedPercent(workItem.TotalSubmitted);

                await context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                workItem.TotalApproved += 1;
                workItem.PercentApproved = workItem.GetSubmittedPercent(workItem.TotalApproved);
                vm.Status = WorkItemStatus.Approved;

                scheduleService.AfterApproveWorkItem(workItem);
            }


            await context.SaveChangesAsync();
            return Ok();
        }


        public async Task<IActionResult> RequestApprovals(int? deptId = null, int? empId = null, DateTime? start = null, DateTime? end = null, int? wId = null, RequestType? type = null)
        {
            //if(type != RequestType.)
            ViewBag.EmployeeId = new SelectList(await accessGrantService.GetEmployeesOfCurremtCompany(true), "Id", "Name");
            //ViewBag.WorkId = new SelectList(await context.Works.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.IsEmployeeTask == false && x.IsAdvancedCreate).ToArrayAsync(), "Id", "Name");

            var vm = await context.Requests.Where(x =>
            x.CompanyId == userResolverService.GetCompanyId() &&
            (x.Status == WorkItemStatus.Submitted) &&
            (type == null || x.RequestType == type) &&
            (deptId == null || x.Employee.DepartmentId == deptId) &&
            (empId == null || x.EmployeeId == empId) &&
            ((start==null && end == null) || start >= x.Start && end <= x.End))
            .Include(x => x.Employee)
                .ThenInclude(x => x.Department)
            .Include(x => x.DayOff)
            .Include(x => x.Attendance)
            .Include(x => x.WorkItem)
            .OrderBy(x => x.Employee.Department.DisplayOrder)
            .ThenBy(x => x.Employee.EmpID)
            .ToListAsync();

            ViewBag.Header = type?.ToString() + " Approvals";
            return PartialView("_RequestApproval", vm);
        }

        public async Task<IActionResult> AddOrUpdateUserTask(int? id = null, DateTime? onDate = null, int wtId = 0)
        {
            //var firstDayOfWeek = await accountDb.CompanyAccounts.Where(x => x.Id == userResolverService.GetCompanyId()).Select(x => x.WeekStartDay).FirstOrDefaultAsync();

            ScheduleCreateVm vm = new ScheduleCreateVm();
            //vm.Days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>()
            //    .OrderBy(x => (x - firstDayOfWeek + 7) % 7)
            //    .Select(x => new DayVm { DayOfWeek = x, CompanyId = userResolverService.GetCompanyId() }).ToList();

            //var workTime = await accountDb.CompanyAccountWorkTimes.FirstOrDefaultAsync(x => x.Id == wtId && x.CompanyAccountId == userResolverService.GetCompanyId());
            ViewBag.WeekStart = vm.ShiftDurationStart = onDate ?? scheduleService.thisWeekStart;
            ViewBag.WeekEnd = vm.ShiftDurationEnd = onDate ?? scheduleService.thisWeekEnd;


            WorkItem workItem = null;
            if (onDate.HasValue)
                workItem = new WorkItem(onDate.Value);
            else
                workItem = new WorkItem();

            if (id.HasValue)
                workItem = await context.WorkItems.FirstOrDefaultAsync(x => x.Id == id && x.IsEmployeeTask);

            if (workItem == null)
                return ThrowJsonError("Task was not found");
            else
            {
                vm.Name = workItem.TaskName;
                vm.Details = workItem.TaskDescription;
            }

            vm.TimeStart = workItem?.WorkStartTime.TimeOfDay ?? onDate?.TimeOfDay ?? scheduleService.thisWeekStart.TimeOfDay;
            vm.TimeEnd = workItem?.WorkEndTime.TimeOfDay ?? onDate?.AddHours(2).TimeOfDay ?? scheduleService.thisWeekStart.AddHours(2).TimeOfDay;
            vm.ShiftDurationStart = workItem?.Date ?? onDate ?? scheduleService.thisWeekStart;
            vm.Id = id ?? 0;
            return PartialView("_AddOrUpdateUserTask", vm);
        }

        //[Ignore]
        [HttpPost]
        public async Task<IActionResult> CreateUserTask(ScheduleCreateVm model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return ThrowJsonError($"Give a name for your task");

            if (ModelState.IsValid)
            {

                model.ScheduleFor = ScheduleFor.Work;
                WorkItem workItem = null;

                // create user task

                model.ShiftDurationStart = DateTime.UtcNow;

                workItem = new WorkItem(onDate: model.ShiftDurationStart.Date)
                {
                    TaskName = model.Name,
                    TaskDescription = model.Details,
                    IsEmployeeTask = true,
                    EmployeeId = userResolverService.GetEmployeeId(),
                    Status = WorkItemStatus.Draft,
                };

                workItem.WorkStartTime = model.ShiftDurationStart;

                // wt update
                if (model.DueDate != null)
                {
                    workItem.WorkEndTime = model.DueDate;
                    workItem.DueDate = model.DueDate;
                }
                else {
                    workItem.WorkEndTime = workItem.WorkStartTime;
                }
                

                context.WorkItems.Add(workItem);

                await context.SaveChangesAsync();


                return PartialView("_SideBarTasks", workItem);
            }

            return ThrowJsonError();

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateUserTask(ScheduleCreateVm model)
        {
            if (model.ShiftDurationEnd.HasValue
                    && model.ShiftDurationStart > model.ShiftDurationEnd)
                return ThrowJsonError($"End date cannot be greater than Start date");
            //if (model.TimeEnd > model.TimeStart)
            //    return ThrowJsonError($"End Time cannot be greater than Start time");

            if (string.IsNullOrWhiteSpace(model.Name))
                return ThrowJsonError($"Give a name for your task");



            if (ModelState.IsValid)
            {

                model.ScheduleFor = ScheduleFor.Work;
                WorkItem workItem = null;

                if (model.Id <= 0)
                {

                    // create user task
                    workItem = new WorkItem(onDate: model.ShiftDurationStart.Date)
                    {
                        TaskName = model.Name,
                        TaskDescription = model.Details,
                        IsEmployeeTask = true,
                        EmployeeId = userResolverService.GetEmployeeId(),
                    };
                }
                else
                {
                    workItem = await context.WorkItems.FirstOrDefaultAsync(x=> x.Id == model.Id && x.IsEmployeeTask);

                }

                if (workItem == null)
                    return ThrowJsonError("Task was not found");
                else
                {
                    workItem.TaskName = model.Name;
                    workItem.TaskDescription = model.Details;
                }

                workItem.Date = workItem.WorkEndTime = workItem.WorkStartTime = model.ShiftDurationStart.Date;
                if (model.IsAllDay)
                {
                    workItem.DueDate = workItem.Date.Date.AddDays(1).AddMinutes(-1);
                }
                else
                {
                    workItem.WorkStartTime = workItem.WorkStartTime.Add(model.TimeStart);


                    if (model.TimeEnd < model.TimeStart)
                        workItem.WorkEndTime = workItem.WorkEndTime.AddDays(1);

                    workItem.WorkEndTime = workItem.WorkEndTime.Add(model.TimeEnd);
                    workItem.DueDate = workItem.WorkEndTime;
                }

                if(model.Id > 0)
                    context.WorkItems.Update(workItem);
                else
                    context.WorkItems.Add(workItem);

                await context.SaveChangesAsync();


                //scheduleService.SetBaseDate(model.ShiftDurationStart);

                //model.WorkId = work.Id;
                //model.ShiftDurationEnd = scheduleService.thisWeekEnd;
                //model.EmployeeIds = new[] { userResolverService.GetEmployeeId() };
                //model.Days = new List<DayVm> { { new DayVm { DayOfWeek = model.ShiftDurationStart.DayOfWeek, WorkIds = new[] { workItem.Id } } } };
                //var newschedule = await scheduleService.CreateNewScheduleAsync(model);

                //try
                //{
                //    await scheduleService.RunScheduleForWorkTime(newschedule);
                //}
                //catch (ApplicationException ex)
                //{
                //    return ThrowJsonError(ex.Message);
                //}
                //catch (Exception ex)
                //{
                //    return ThrowJsonError("Fatal error: " + ex.Message);
                //}

                return await Sidebar();
            }

            return ThrowJsonError();
        }


        
        [HttpPost]
        public async Task<IActionResult> TaskCompleted(int id)
        {

            if (ModelState.IsValid)
            {
                var task = await context.WorkItems.FirstOrDefaultAsync(x => x.Id == id && x.IsEmployeeTask);

                if (task == null)
                    return ThrowJsonError($"Task was not found");


                if (task.Status != WorkItemStatus.Draft)
                    return ThrowJsonError($"Task was not in draft mode");

                task.Status = WorkItemStatus.Completed;

                await context.SaveChangesAsync();

                return await Sidebar();
            }

            return ThrowJsonError();
        }



        public Chart GetComapreHorizontalBarGraph(List<ComapareData> values, string title = "")
        {
            //var compareData = comparsonVm.CompareDatas
            Chart chart = new Chart();

            chart.Type = Enums.ChartType.Doughnut;

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = values.Select(x => x.Key).ToList();

            chart.Options.Responsive = true;
            chart.Options.MaintainAspectRatio = true;
            chart.Options.Legend = new Legend { Position = "bottom", FullWidth = true, Display = false, Labels = new LegendLabel { FontSize = 8 } };
            chart.Options.Title = new Title { Text = title, Display = true, FontSize = 13 };
            chart.Options.Layout = new Layout { Padding = new Padding { PaddingObject = new PaddingObject { Left = 75, Bottom = 5, Right = 75, Top = 5 } } };
            var outlbaelDict = new Dictionary<string, object>();

            outlbaelDict.Add("plugins", new Plugins { outlabels = new outlabels { color = "black", stretch = 25, Text = "%l", borderRadius = 5, borderWidth = 1, display = true, padding = 3 } });
            chart.Options.PluginDynamic = outlbaelDict;
            //chart.Options.ZoomOutPercentage = 55,

            PieDataset dataset = new PieDataset
            {
                Data = values.Select(x => double.Parse(x.CurrentValue.ToString())).ToList(),
                Label = "Employee by Department",
                Type = Enums.ChartType.Pie,
                BackgroundColor = new List<ChartColor>
                {
                    ChartColor.FromRgba(255, 99, 132, 0.2),
                    ChartColor.FromRgba(54, 162, 235, 0.2),
                    ChartColor.FromRgba(255, 206, 86, 0.2),
                    ChartColor.FromRgba(75, 192, 192, 0.2),
                    ChartColor.FromRgba(153, 102, 255, 0.2),
                    ChartColor.FromRgba(255, 159, 64, 0.2),
                    ChartColor.FromRgba(255, 99, 132, 0.2),
                },
                BorderColor = new List<ChartColor>
                {
                    ChartColor.FromRgb(255, 99, 132),
                    ChartColor.FromRgb(54, 162, 235),
                    ChartColor.FromRgb(255, 206, 86),
                    ChartColor.FromRgb(75, 192, 192),
                    ChartColor.FromRgb(153, 102, 255),
                    ChartColor.FromRgb(255, 159, 64),
                    ChartColor.FromRgb(255, 99, 132),
                },
                BorderWidth = 1
            };


            data.Datasets = new List<Dataset>();
            data.Datasets.Add(dataset);

            chart.Data = data;

            return chart;
        }

        public class Plugins
        {
            public outlabels outlabels { get; set; }
        }

        public class outlabels
        {
            public int zoomOutPercentage { get; set; }

            public int stretch { get; set; }
            public int padding { get; set; }
            public int borderRadius { get; set; }
            public int borderWidth { get; set; }
            public bool display { get; set; }

            public string Text { get; set; }
            public string color { get; set; }
        }


        public IActionResult Search (string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok();

            return PartialView("_SearchResult", searchService.GetSearhResult(query, Url));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/Diagnostics")]
        public IActionResult Diagnostics()
        {
            TempData["CONSTIRNG_ACCOUNT"] = accountDb.Database.GetDbConnection().ConnectionString;
            TempData["CONSTIRNG_MASTER"] = context.Database.GetDbConnection().ConnectionString;
            TempData["CONSTIRNG_LOG"] = logDbContext.Database.GetDbConnection().ConnectionString;
            return View();
        }
    }

    public class TaskCompletionData
    {
        public WorkType Name { get; set; }
        public int TotalRequired { get; set; }
        public int TotalCompleted { get; internal set; }
    }

    public class TaskData
    {
        public int? WorkId { get; set; }
        public string WorkName { get; set; }
        public WorkItemStatus Status { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalFailed { get; set; }
        public int TotalRequired { get; set; }
        public WorkType WorkType { get; internal set; }
        public float CompletionCount { get; internal set; }
        public float FailedCount { get; internal set; }
    }
}
