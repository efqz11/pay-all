using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Payroll.Database;
using Payroll.Models;
using Payroll.ViewModels;
using Payroll.Services;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Rotativa.AspNetCore;

namespace Payroll.Controllers
{
    [Authorize]
    public class ManageController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly PayrollService payrollService;
        private readonly PayAdjustmentService payAdjustmentService;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly AccessGrantService accessGrantService;
        private readonly UserResolverService userResolverService;
        private readonly CompanyService companyService;
        private readonly EmployeeService employeeService;
        private readonly ScheduleService scheduleService;
        private readonly Hangfire.IBackgroundJobClient backgroundJobClient;
        private readonly RequestService requestService;

        public ManageController(PayrollDbContext context, PayrollService payrollService, PayAdjustmentService payAdjustmentService, IHostingEnvironment hostingEnvironment, AccessGrantService accessGrantService, UserResolverService userResolverService, CompanyService companyService, EmployeeService employeeService, ScheduleService scheduleService, Hangfire.IBackgroundJobClient backgroundJobClient, RequestService requestService)
        {
            this.context = context;
            this.payrollService = payrollService;
            this.payAdjustmentService = payAdjustmentService;
            this.hostingEnvironment = hostingEnvironment;
            this.accessGrantService = accessGrantService;
            this.userResolverService = userResolverService;
            this.companyService = companyService;
            this.employeeService = employeeService;
            this.scheduleService = scheduleService;
            this.backgroundJobClient = backgroundJobClient;
            this.requestService = requestService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await accessGrantService.GetAllAccessiblePayrollsAsync(overrideCompanyIfAdmin: false);

            return View(data);
        }



        public async Task<IActionResult> OnBoarding(int id, int page = 1, int limit = 10)
        {
            return View();
        }
        public async Task<IActionResult> EmployeeDigital(int id, int page = 1, int limit = 10)
        {
            return View();
        }
        public async Task<IActionResult> AttendanceTracking(int id, int page = 1, int limit = 10)
        {
            return View();
        }

        public async Task<IActionResult> Absence(int id, int page = 1, int limit = 10, int? month = null, int? year = null)
        {
            var _month = month ?? DateTime.Now.Month;
            var _year = year ?? DateTime.Now.Year;
            ViewBag.year = _year;
            ViewBag.month = _month;
            ViewBag.comAcc = await companyService.GetCompanyCalendarSettings();
            ViewBag.dayOffs = await companyService.GetDayOffs();

            var _daysTotals = DateTime.DaysInMonth(_year, _month);
            var start = new DateTime(_year, _month, 1);
            var end = start.AddDays(_daysTotals);


            var data = new List<WeeklyEmployeeShiftVm>();
            if (GetEmployeeSelectorModal() != null)
            {
                var dayOffs = await context.DayOffEmployeeItems.Where(a => GetEmployeeSelectorModal().EmployeeIds.Contains(a.DayOffEmployee.EmployeeId) && a.Status == DayOffEmployeeItemStatus.Approved && a.Start >= start && a.End <= end && a.IsActive)
                    .Select(a => new DayOffEmplVm
                    {
                        UniqeId = a.Id,
                        DayOffId = a.DayOffEmployee.DayOffId,
                        DayOffColor = a.DayOffEmployee.DayOff.Color,
                        Start = a.Start,
                        End = a.End,
                        RequestId = a.RequestId,
                        DayOffName = a.DayOffEmployee.DayOff.Name,
                        EmployeeId = a.DayOffEmployee.EmployeeId
                    })
                    .ToListAsync();
                var waitingRequests = await context.Requests.Where(a => GetEmployeeSelectorModal().EmployeeIds.Contains(a.EmployeeId) && a.Status !=WorkItemStatus.Approved && a.RequestType == RequestType.Leave && a.Start >= start && a.End <= end && a.IsActive)
                    .Include(a=> a.DayOff)
                     .Select(a => new DayOffEmplVm
                     {
                         UniqeId = a.Id,
                         DayOffId = a.DayOffId.Value,
                         DayOffColor = a.DayOff.Color,
                         Start = a.Start.Value,
                         End = a.End.Value,
                         RequestId = a.Id,
                         DayOffName = a.DayOff.Name,
                         EmployeeId = a.EmployeeId,
                         IsPending = true,
                     })
                     .ToListAsync();

                dayOffs.AddRange(waitingRequests);
                data = await context.Employees.Where(a => GetEmployeeSelectorModal().EmployeeIds.Contains(a.Id) && a.CompanyId == userResolverService.GetCompanyId()).Include(a => a.Job).Select(a => new WeeklyEmployeeShiftVm
                {
                    Employee = a,
                    DayOffs = dayOffs.Where(d=> d.EmployeeId == a.Id).ToList()
                }).ToListAsync();

                ViewBag.totalEmpls = GetEmployeeSelectorModal().TotalFoundEmployees;
            }

            return View(data);
        }


        public async Task<IActionResult> TimeTrackingApprovals(int id, int page = 1, int limit = 10, int? month = null, int? year = null)
        {
            var _month = month ?? DateTime.Now.Month;
            var _year = year ?? DateTime.Now.Year;
            ViewBag.year = _year;
            ViewBag.month = _month;
            ViewBag.comAcc = await companyService.GetCompanyCalendarSettings();
            ViewBag.dayOffs = await companyService.GetDayOffs();

            var _daysTotals = DateTime.DaysInMonth(_year, _month);
            var start = new DateTime(_year, _month, 1);
            var end = start.AddDays(_daysTotals);


            var data = new List<WeeklyEmployeeShiftVm>();
            var attendances = await context.Attendances.Where(a => a.CompanyId == userResolverService.GetCompanyId() && !a.IsPublished && a.IsActive)
                .ToListAsync();

            var selectedEmplUIds = attendances.Select(a => a.EmployeeId).Distinct().ToArray();
            if (attendances != null)
            {
                data = await context.Employees.Where(a => selectedEmplUIds.Contains(a.Id) && a.CompanyId == userResolverService.GetCompanyId()).Include(a => a.Job).Select(a => new WeeklyEmployeeShiftVm
                {
                    Employee = a,
                    Attendances = attendances
                }).ToListAsync();

            }

            return View(data);
        }

        public async Task<IActionResult> EmployeeTimeSheet(int id)
        {
            var attendances = await context.Attendances.Where(a => a.EmployeeId == id && !a.IsPublished && a.IsActive)
                .OrderBy(a=> a.Date)
                .ToListAsync();
            ViewBag.empl = await employeeService.GetEmployeeById(id);
            return PartialView("_EmployeeTimeSheet", attendances);
        }
        public async Task<IActionResult> EmployeeDayBiometricRecords(int id, int aId)
        {
            var attendances = await context.BiometricRecords.Where(a => a.EmployeeId == id && a.AttendanceId == aId)
                .Include(a=>a.Attendance)
                .OrderBy(a => a.DateTime)
                .ToListAsync();
            ViewBag.empl = await employeeService.GetEmployeeById(id);
            return PartialView("_EmplpyeeDayBiometric", attendances);
        }

        [HttpPost]
        public async Task<IActionResult> TimeTrackingActionEmployee(int id, bool? approve = null)
        {
            var attendance = await context.Attendances
                    .Where(x => x.EmployeeId == id && !x.IsPublished && x.TotalWorkedHours > 0)
                    .ToListAsync();
            if(attendance == null || !attendance.Any())
                return ThrowJsonError("attendance was not found or was already published");

                attendance.ForEach  (a=> {
                    a.IsPublished = true;
                    a.PublishedDate = DateTime.UtcNow;
                });
                await context.SaveChangesAsync();
                return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> TimeTrackingAction(int id, bool? approve = null)
        {
            var attendance = await context.Attendances
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsPublished);
            if(attendance == null)
                return ThrowJsonError("attendance was not found or was already published");

            if(attendance.TotalWorkedHours <= 0)
                return ThrowJsonError("attendance does not have clock records and tracked time is 0");

                attendance.IsPublished = true;
                attendance.PublishedDate = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return Ok();
        }

        //public async Task<IActionResult> Calendar(int id, DateTime? onDate = null)
        //{
        //    ViewBag.Complete = "on";
        //    //if (onDate.HasValue)

        //    var _date = onDate ?? DateTime.Now;
        //    var cmpConfig = await companyService.GetCompanyCalendarSettings();
        //    ViewBag.cmpConfig = cmpConfig;
        //    scheduleService.SetDayOfWeeekOnCompany(cmpConfig);
        //    scheduleService.SetBaseDate(new DateTime(_date.Year, _date.Month, 1));

        //    ViewBag.Start = new DateTime(_date.Year, _date.Month, 1);
        //    ViewBag.End = new DateTime(_date.Year, _date.Month, 1).AddDays(DateTime.DaysInMonth(_date.Year, _date.Month));
        //    ViewBag.WeekStart = scheduleService.thisWeekStart;
        //    ViewBag.WeekEnd = scheduleService.thisWeekEnd;
        //    ViewBag.DurationText = _date.ToString("MMMM yyyy");
        //    ViewBag.Date = _date;
        //    ViewBag.calendars = Calendars.List;

        //    var vm = new HomeEmployeeVm
        //    {

        //        MyRequests = await requestService.GetLeaveRequests(userResolverService.GetCompanyId(), new int[] { userResolverService.GetEmployeeId() }, ViewBag.Start, ViewBag.End, null, new WorkItemStatus[] { WorkItemStatus.Approved, WorkItemStatus.Draft }, 1, int.MaxValue),
        //        BirthDaysInMonth = await companyService.GetBirthdayCalendar(_date.Year, _date.Month),
        //        WorkAnniversaries = await companyService.GetWorkAnniversaryCalendar(_date.Year, _date.Month),
        //        PublicHolidaysUpcoming = await companyService.GetUpComingPublicHolidaysForYear(_date.Year, _date.Month),
        //        AttedanceSchedule = await scheduleService.GetCurrentSecdule(empId: userResolverService.GetEmployeeId()),
        //        Employee = await employeeService.GetCurrentEmployeeWithDepartment(),
        //        WeekScheduleTasks = await scheduleService.GetSecduledTasksForThisWeek(empId: userResolverService.GetEmployeeId()),
        //        WorkTimes = await companyService.GetWorkTimes()
        //    };

        //    return View(vm);
        //}

        public async Task<IActionResult> Calendar(int? empId = null, DateTime? onDate = null)
        {
            ViewBag.Complete = "on";
            //if (onDate.HasValue)

            var _date = onDate ?? DateTime.Now;
            var cmpConfig = await companyService.GetCompanyCalendarSettings();
            ViewBag.cmpConfig = cmpConfig;
            scheduleService.SetDayOfWeeekOnCompany(cmpConfig);
            scheduleService.SetBaseDate(new DateTime(_date.Year, _date.Month, 1));

            ViewBag.Start = new DateTime(_date.Year, _date.Month, 1);
            ViewBag.End = new DateTime(_date.Year, _date.Month, 1).AddDays(DateTime.DaysInMonth(_date.Year, _date.Month));
            ViewBag.WeekStart = scheduleService.thisWeekStart;
            ViewBag.WeekEnd = scheduleService.thisWeekEnd;
            ViewBag.DurationText = _date.ToString("MMMM yyyy");
            ViewBag.Date = _date;
            ViewBag.calendars = Calendars.List;
            ViewBag.empId = empId;

            var _empIds = new int[] { empId ?? userResolverService.GetEmployeeId() };
            if (GetEmployeeSelectorModal() != null)
            {
                _empIds = GetEmployeeSelectorModal().EmployeeIds;
            }
            var vm = new HomeEmployeeVm
            {
                MyRequests = await requestService.GetLeaveRequests(userResolverService.GetCompanyId(), _empIds, ViewBag.Start, ViewBag.End, null, new WorkItemStatus[] { WorkItemStatus.Approved, WorkItemStatus.Draft }, 1, int.MaxValue),
                BirthDaysInMonth = await companyService.GetBirthdayCalendar(_date.Year, _date.Month),
                WorkAnniversaries = await companyService.GetWorkAnniversaryCalendar(_date.Year, _date.Month),
                PublicHolidaysUpcoming = await companyService.GetUpComingPublicHolidaysForYear(_date.Year, _date.Month),
                //AttedanceSchedule = await scheduleService.GetCurrentSecdule(empId: _empId),
                Employee = await employeeService.GetCurrentEmployeeWithDepartment(),
                WeekScheduleTasks = await scheduleService.GetTasksAsync(_empIds, ViewBag.Start, ViewBag.End),
                WorkTimes = await companyService.GetWorkTimes()
            };

            if(Request.IsAjaxRequest())
            return PartialView("_Calendar", vm);

               return View(vm);
        }


        public async Task<IActionResult> AttendanceView(int id, int page = 1, int limit = 10)
        {
            var empId = GetWorkingEmployeeId();
            //var empRecorsd = await payrollService.GetPayPeriodCalendarAsync(id, page, limit, empId: empId);
            ViewBag.Header = "Employee Attendance Summary";

            var pp = new PayrollVm
            {
                PayrollPeriod = await context.PayrollPeriods.FindAsync(id),
                //EmployeeRecords = empRecorsd,
                //EmployeeInteractions = await payAdjustmentService.GetEmployeePayPeriodInteractionsAsync(id, empId: empId)
            };

            var selectorVm = GetEmployeeSelectorModal();
            var lateEmplsMax = await context.Attendances
                .Where(x => x.CompanyId == userResolverService.GetCompanyId() &&
                    (selectorVm == null || selectorVm.EmployeeIds.Contains(x.EmployeeId)) &&
                    x.Date >= pp.PayrollPeriod.StartDate && x.Date <= pp.PayrollPeriod.EndDate &&
                    (x.CurrentStatus == AttendanceStatus.Late || x.CurrentStatus == AttendanceStatus.OnTime ||
                    x.CurrentStatus == AttendanceStatus.Absent))
                .GroupBy(x => new { x.CurrentStatus, x.EmployeeId })
                .OrderByDescending(x => x.Count())
                .Take(10)
                .Select(a => new EmployeeStatusDays
                {
                    EmployeeId = a.Key.EmployeeId,
                    CurrentStatus = a.Key.CurrentStatus,
                    DaysCount = a.Select(z => z.Date.Date).Distinct().Count(),
                    TotalHoursWorkedPerSchedule = a.Sum(x => x.TotalHoursWorkedPerSchedule),
                    TotalWorkedHours = a.Sum(x => x.TotalWorkedHoursCalculated),
                    TotalLateMins = a.Sum(x => x.TotalLateMins)
                }).ToListAsync();
            var empls = context.Employees.Where(a => lateEmplsMax.Select(x => x.EmployeeId).Distinct().Contains(a.Id))
                .Include(a => a.Department)
                .ToList();

            lateEmplsMax.ForEach(t => t.Employee = empls.First(a => a.Id == t.EmployeeId));
            pp.EmployeeStatusDays = lateEmplsMax;

            return PartialView("_AttendanceView", pp);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }

}
