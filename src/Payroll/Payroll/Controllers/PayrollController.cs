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
    [EmployeeRoleAuthorize(Roles = new[] { Roles.Company.payroll })]    
    public class PayrollController : BaseController
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

        public PayrollController(PayrollDbContext context, PayrollService payrollService, PayAdjustmentService payAdjustmentService, IHostingEnvironment hostingEnvironment, AccessGrantService accessGrantService, UserResolverService userResolverService, CompanyService companyService, EmployeeService employeeService, ScheduleService scheduleService, Hangfire.IBackgroundJobClient backgroundJobClient)
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
        }

        public async Task<IActionResult> Index()
        {
            var data = await accessGrantService.GetAllAccessiblePayrollsAsync(overrideCompanyIfAdmin: false);

            return View(data);
        }

        public IActionResult AddOrUpdate(int id = 0)
        {
            if(id > 0)
            {
                var payrol = context.PayrollPeriods.Find(id);
                if (payrol == null)
                    return BadRequest("Payrol for edit was not found");

                return View(payrol);
            }

            ViewBag.IsFirstPayrol = false;
            ViewBag.PayAdjustmentCreating = context.PayAdjustments
                        .Where(x => (x.VariationType == VariationType.ConstantAddition || x.VariationType == VariationType.ConstantDeduction) && x.CompanyId == userResolverService.GetCompanyId()).ToList();
            var payrolPeriod = new PayrollPeriod { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) };
            var lastPayrol = context.PayrollPeriods.OrderByDescending(x => x.EndDate).FirstOrDefault();
            if (lastPayrol == null)
                ViewBag.IsFirstPayrol = true;
            else
            {
                payrolPeriod.StartDate = lastPayrol.EndDate.AddDays(1);
                payrolPeriod.EndDate = lastPayrol.EndDate.AddDays(30);
            }
            payrolPeriod.CompanyId = userResolverService.GetCompanyId();


            return View(payrolPeriod);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdate(PayrollPeriod model)
        {
            if (ModelState.IsValid)
            {
                if (userResolverService.GetCompanyId() != model.CompanyId)
                    ModelState.AddModelError("", "Selected Company was invalid");

                if (model.PayDate <= model.StartDate || model.PayDate > model.EndDate.AddDays(7))
                    ModelState.AddModelError("PayDate", "Pay date must be between payroll period or within a week after!");

                if (await context.PayrollPeriods.AnyAsync(x=> x.CompanyId == userResolverService.GetCompanyId() &&
                    x.StartDate > model.StartDate && x.EndDate <= model.EndDate && (model.Id > 0 ? x.Id != model.Id : true)))
                    ModelState.AddModelError("", "Payrol period has conflicts between dates, kindly changes the dates!");
            }


            if (ModelState.IsValid)
            {
                if (model.Id <= 0)
                {
                    // Create
                    context.PayrollPeriods.Add(model);
                    context.SaveChanges();

                    try
                    {

                        if (model.GenerateFieldsForConstantPayAdjustments)
                        {
                            var constantPayadjustments = context.PayAdjustments
                                .Where(x => (x.VariationType == VariationType.ConstantAddition || x.VariationType == VariationType.ConstantDeduction) && x.CompanyId == userResolverService.GetCompanyId())
                                .Include(x => x.Fields).ToArray();
                            foreach (var adj in constantPayadjustments)
                            {
                                var newList = await payAdjustmentService.GeneratePayAdjustmentsAndFieldValues(adj.Fields, adj, payrolId: model.Id, isSample: false);
                                context.PayrollPeriodPayAdjustments.AddRange(newList);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        TempData["Msg"] = "Unable to create values for selected constant adjustments, but Payrol was created";
                        TempData.Keep("Msg");
                        return RedirectToAction(nameof(View), new { id = model.Id });
                    }
                }
                else
                {
                    // update
                    context.PayrollPeriods.Update(model);
                }

                context.SaveChanges();
                return RedirectToAction("View", new { id = model.Id });
            }


            ViewBag.PayAdjustmentCreating = context.PayAdjustments
                        .Where(x => (x.VariationType == VariationType.ConstantAddition || x.VariationType == VariationType.ConstantDeduction) && x.CompanyId == userResolverService.GetCompanyId()).ToList();
            return View(model);
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            if (ModelState.IsValid)
            {
                var payrolPeroid = context.PayrollPeriods
                    .Include(x=> x.PayrollPeriodPayAdjustments).FirstOrDefault(x=> x.Id == id);
                if (payrolPeroid == null)
                    return BadRequest("Oooh! we didnt find that one");
                //if (payrolPeroid.PayrollPeriodPayAdjustments.Any())
                //    return BadRequest("Ouch! Some items are used as children, please remove them before proceed");

                context.PayrollPeriods.Remove(payrolPeroid);
                context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return BadRequest();
        }

        public async Task<IActionResult> View(int id)
        {
            var empId = GetWorkingEmployeeId();
            var pp = new PayrollVm
            {
                PayrollPeriod = await context.PayrollPeriods.FindAsync(id),
                ActiveTab = ParolTabs.PayPeriod,
                PayrolPeriodId = id,
                //RecentPeriods = await context.PayrollPeriods.Where(x=> x.CompanyId == userResolverService.GetCompanyId())
                //.OrderByDescending(x=> context.Entry(x).Property(AuditFileds.CreatedDate).CurrentValue)
                //.Take(10)
                //.ToDictionaryAsync(x=> x.Id, x=> x.StartDate.GetDuration(x.EndDate))
            };
            
            return View(pp);
        }


        public async Task<IActionResult> Dashboard(int id)
        {
            ViewBag.Header = "Employees working";

            var pp = new PayrollVm
            {
                PayrollPeriod = await context.PayrollPeriods.FindAsync(id),
                PayrolPeriodId = id,
            };
            pp.PublicHolidaysDuringPeriod = await payrollService.GetcompanyAndPublicHolidaysCount(pp.PayrollPeriod);

            ViewBag.TotalDays = (pp.PayrollPeriod.EndDate - pp.PayrollPeriod.StartDate).TotalDays;
            
            return PartialView("_Dashboard", pp);
        }


        public async Task<IActionResult> Perforrmance(int id, int page = 1, int limit = 10)
        {
            var empRecorsd = await payrollService.GetPayPeriodCalendarAsync(id, page, limit, selectorVm: GetEmployeeSelectorModal());
            ViewBag.Header = "Employees working";

            var pp = new PayrollVm
            {
                PayrollPeriod = await context.PayrollPeriods.FindAsync(id),
                EmployeeRecords = empRecorsd,
                PayrolPeriodId = id
            };

            var empSelector = GetEmployeeSelectorModal();
            var _start = pp.PayrollPeriod.StartDate.Date;
            var _end = pp.PayrollPeriod.EndDate.Date.AddDays(1).AddMinutes(-1);
            var _activeEmployments = await employeeService.GetAllEmployeesInMyCompanyForPayroll(false, _start, _end, empIds: empSelector?.EmployeeIds);
            var _activeEmployeeIds = _activeEmployments.Select(e => e.Id).ToArray();

            pp._ActiveContracts = _activeEmployments;

            var attendanceStatusGroup = await context.Attendances.Where(a => a.CompanyId == userResolverService.GetCompanyId() && a.IsActive 
                    && a.IsPublished && a.Date >= _start && a.Date <= _end && _activeEmployeeIds.Contains(a.EmployeeId))
                .AsQueryable()
                .GroupBy(a => new { a.CurrentStatus, a.EmployeeId, a.IsOvertime })
                .Select(x=> new {
                    x.Key,
                    CurrentStatus = x.Key.CurrentStatus,
                    _lateMins = x.Where(a => x.Key.CurrentStatus == AttendanceStatus.Late).Sum(a => a.TotalLateMins),
                     _lateDays = x.Where(a => x.Key.CurrentStatus == AttendanceStatus.Late).Select(z => z.Date.Date).Distinct().Count(),
                     //x.Key,

                    _totalWorkedHrsPerSchedule = x.Sum(a => a.TotalHoursWorkedPerSchedule),
                    _totalWorkedHrs = x.Sum(a => a.TotalWorkedHours),
                })
                .ToListAsync();

            var mm = await context.WorkItems.Where(x => _activeEmployeeIds.Contains(x.EmployeeId) && x.Date >= _start && x.Date <= _end && !x.IsEmployeeTask &&      _activeEmployeeIds.Contains(x.EmployeeId))
                .AsQueryable()
                .Include(x=> x.Work)
                .GroupBy(x => x.Work.Type)
                .Select(x => new
                {
                    x.Key,
                    Count = x.Count(),
                    TotalApproved = x.Sum(a => a.TotalApproved),
                    RemainingSubmissions = x.Sum(a => a.RemainingSubmissions),
                    // TotalRequired = x.Sum(a => a.Work.TotalRequiredCount),
                    TotalCompleted = x.Count(a => a.Status == WorkItemStatus.Completed),
                }).ToListAsync();



            pp._ActiveContracts.ForEach(c =>
            {
                c._lateMins = attendanceStatusGroup.Where(a => a.Key.EmployeeId == c.Id && !a.Key.IsOvertime && a.CurrentStatus == AttendanceStatus.Late).Sum(z => z._lateMins);
                c._lateDays = attendanceStatusGroup.Where(a => a.Key.EmployeeId == c.Id && !a.Key.IsOvertime &&  a.CurrentStatus == AttendanceStatus.Late).Distinct().Count();

                c._otHrs = attendanceStatusGroup.Where(a => a.Key.EmployeeId == c.Id && a.Key.IsOvertime).Sum(z => z._totalWorkedHrsPerSchedule);

                c._AbsentDays = attendanceStatusGroup.Where(a => a.Key.EmployeeId == c.Id && a.CurrentStatus == AttendanceStatus.Absent).Distinct().Count();
                c._totalWorkedHrs = attendanceStatusGroup.Where(a => a.Key.EmployeeId == c.Id && (a.CurrentStatus == AttendanceStatus.OnTime || a.CurrentStatus == AttendanceStatus.Early || a.CurrentStatus == AttendanceStatus.Late)).Sum(a => a._totalWorkedHrs);

                c._totalWorkedHrsPerSchedule = attendanceStatusGroup.Where(a => a.Key.EmployeeId == c.Id && !a.Key.IsOvertime).Sum(z => z._totalWorkedHrsPerSchedule);
                // c._submissionOverallCompletionPercent = ((decimal)
                // mm.Where(x => x.Key == WorkType.RequireSubmissions).Sum(a => a.TotalApproved) /
                //  (mm.Where(x => x.Key == WorkType.RequireSubmissions).Sum(a => a.TotalRequired) + 0.1m)) * 100;
                c._clockTasksOverallCompletionPercent = ((decimal)
                mm.Where(x => x.Key == WorkType.ExpectClockInRecordsBefore).Sum(a => a.TotalCompleted) /
                 (mm.Where(x => x.Key == WorkType.ExpectClockInRecordsBefore).Sum(a => a.Count) + 0.1m)) * 100;
            });
            
            return PartialView("_PayrolPeriodDays", pp);
        }

        public async Task<IActionResult> GetStatDetails(int id, int empId, string type)
        {
            var pp = await context.PayrollPeriods.FindAsync(id);
            var emp = await context.Employees.FindAsync(empId);
            var _start = pp.StartDate;
            var _end = pp.EndDate;
            List<Attendance> dates = null;
            ViewBag.Type = type;
            ViewBag.emp = emp;
            ViewBag.pp = pp;

            switch (type)
            {
                case "late":
                    dates = await context.Attendances.Where(a => a.CompanyId == userResolverService.GetCompanyId() && a.EmployeeId == empId && a.IsActive && a.IsPublished && a.Date >= _start && a.Date <= _end && a.CurrentStatus == AttendanceStatus.Late)
                        .ToListAsync();
                    break;
                case "workedHrs":
                    dates = await context.Attendances.Where(a => a.CompanyId == userResolverService.GetCompanyId() && a.EmployeeId == empId && a.IsActive && a.IsPublished && a.Date >= _start && a.Date <= _end && (a.CurrentStatus == AttendanceStatus.OnTime || a.CurrentStatus == AttendanceStatus.Early || a.CurrentStatus == AttendanceStatus.Late))
                        .ToListAsync();
                    break;
                case "ot":
                    dates = await context.Attendances.Where(a => a.IsOvertime && a.CompanyId == userResolverService.GetCompanyId() && a.EmployeeId == empId && a.IsActive && a.IsPublished && a.Date >= _start && a.Date <= _end)
                        .ToListAsync();
                    break;
                case "absent":
                    dates = await context.Attendances.Where(a => a.CompanyId == userResolverService.GetCompanyId() && a.EmployeeId == empId && a.IsActive && a.IsPublished && a.Date >= _start && a.Date <= _end && a.CurrentStatus == AttendanceStatus.Absent)
                        .ToListAsync();
                    break;
                default:
                    return ThrowJsonError();
            }

            return PartialView("_StatDetails", dates);
        }

        #region Attendance
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
                        TotalHoursWorkedPerSchedule = a.Sum(x=> x.TotalHoursWorkedPerSchedule),
                        TotalWorkedHours = a.Sum(x => x.TotalWorkedHoursCalculated),
                        TotalLateMins = a.Sum(x=> x.TotalLateMins)
                    }).ToListAsync();
            var empls = context.Employees.Where(a => lateEmplsMax.Select(x => x.EmployeeId).Distinct().Contains(a.Id))
                .Include(a => a.Department)
                .ToList();

            lateEmplsMax.ForEach(t => t.Employee = empls.First(a => a.Id == t.EmployeeId));
            pp.EmployeeStatusDays = lateEmplsMax;

            return PartialView("_AttendanceView", pp);
        }

        public async Task<IActionResult> AttendanceDayView(int id, int? empId = null, DateTime? date = null, int page = 1, int limit = 10)
        {
            //var empRecorsd = await payrollService.GetPayPeriodCalendarAsync(id, page, limit, empId: empId);
            ViewBag.Header = "All Employee Attendance Summary";

            var pp = new PayrollVm
            {
                PayrollPeriod = await context.PayrollPeriods.FindAsync(id),
                //EmployeeRecords = empRecorsd,
                //EmployeeInteractions = await payAdjustmentService.GetEmployeePayPeriodInteractionsAsync(id, empId: empId)
            };
            pp.RelatedAttendanceChart = new Dictionary<string, int>();
            pp.PayrolPeriodId = pp.PayrollPeriod.Id;
            if (date.HasValue && date >= pp.PayrollPeriod.StartDate && date <= pp.PayrollPeriod.EndDate)
            {
                pp.SelectedDate = date;
                pp.RelatedAttendanceChart = await context.Attendances.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Date.Date == date.Value.Date).GroupBy(a => a.CurrentStatus)
                    .ToDictionaryAsync(a => a.Key.ToString(), a => a.Count());
            }

            if (empId.HasValue && empId.Value > 0)
            {
                var employee = await context.Employees.FirstOrDefaultAsync(a => a.Id == empId);
                if (employee != null)
                {

                    ViewBag.Header = employee?.NameDisplay + " Time Sheet";
                    pp.RelatedTimeSheet = await context.BiometricRecords.Where
                        (b => b.Date.Date == date && b.EmployeeId == empId)
                        .OrderBy(a => a.Date)
                        .Include(a=> a.Attendance)
                        .ToListAsync();
                    ViewBag.Employee = employee;

                    if (pp.RelatedTimeSheet.Any())
                    {
                        
                        // if time sheet records contains attendance for today
                        if (pp.RelatedTimeSheet.Any(x => x.Attendance.Date.Date == date.Value.Date && x.EmployeeId == empId))
                        {
                            var attnIds = pp.RelatedTimeSheet.Select(a => a.AttendanceId).Distinct().ToArray();
                            pp.RelatedAttendances = await context.Attendances.Where(x => x.CompanyId == userResolverService.GetCompanyId() && attnIds.Contains(x.Id)
                                /*&& x.Date.Date == date.Value.Date && x.EmployeeId == empId*/)
                                .Include(x => x.Employee)
                                .Include(x => x.BiometricRecords)
                                .ToListAsync();
                        }
                        else
                        {
                            pp.RelatedAttendances = await context.Attendances.Where(x => x.CompanyId == userResolverService.GetCompanyId()
                                && x.Date.Date == date.Value.Date && x.EmployeeId == empId)
                                .Include(x => x.Employee)
                                .Include(x => x.BiometricRecords)
                                .ToListAsync();
                        }



                        // if night duty or start-end on different days *then fix 
                        if (pp.RelatedAttendances.FirstOrDefault() != null)
                            if (pp.RelatedAttendances.First().WorkStartTime.Date != pp.RelatedAttendances.First().WorkEndTime.Date)
                                pp.RelatedTimeSheet.AddRange(
                                        pp.RelatedAttendances.First().BiometricRecords
                                        .Except(pp.RelatedTimeSheet)
                                        .ToList()
                                    );
                    }
                    else
                    {
                        // if no time sheeet, still fetch matching attendance/shift record on date
                        pp.RelatedAttendances = await context.Attendances.Where(x => x.CompanyId == userResolverService.GetCompanyId() 
                            && x.Date.Date == date.Value.Date && x.EmployeeId == empId)
                            .Include(x => x.Employee)
                            .Include(x => x.BiometricRecords)
                            .ToListAsync();
                    }

                    if (pp.RelatedAttendances.Any())
                        ViewBag.StartDateTime = await companyService.GetCompanyWokTimeStartOrShiftStart(pp.RelatedAttendances.First().Date.Date);
                }
            }
            else if (date.HasValue)
            {
                // get attendances
                var empSelectorVm = GetEmployeeSelectorModal();
                pp.RelatedAttendances = await context.Attendances.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Date.Date == date.Value.Date && (empSelectorVm == null || empSelectorVm.EmployeeIds.Contains(x.EmployeeId)))
                    .Include(x => x.Employee)
                    .Include(x => x.BiometricRecords)
                    .ToListAsync();
            }


            return PartialView("_AttendanceDayView", pp);
        }

        public async Task<IActionResult> AddBioMetricRecord(int empId, DateTime onDate, int pId, int id = 0, int attnId = 0)
        {
            var emp = await context.Employees.FindAsync(empId);
            if (emp == null) return BadRequest("Employee was not not found");



            ViewBag.EmployeeId = await employeeService.GetAllEmployeesInMyCompanyForDropdownOptGroups();
            //ViewBag.AnnouncementId = new SelectList(announcemtss
            //    .OrderByDescending(x => x.PublishedDate).ToList(), "Id", "Name", emp?.AnnouncementId ?? annId);

            //TempData["AnnouncementId"] = annId;
            //TempData.Keep("AnnouncementId");
            
            var rec = new BiometricRecord
            {
                BiometricRecordType = BiometricRecordType.FingerPrint,
                CompanyId = userResolverService.GetCompanyId(),
                Date = onDate,
                Time = onDate.TimeOfDay,
                EmployeeId = empId,
                AttendanceId = attnId,
            };
            if (id > 0)
            {
                rec = await context.BiometricRecords
                    .Include(x=> x.Employee)
                    .FirstOrDefaultAsync(x=> x.Id == id);
            }
            ViewBag.pId = pId;

            return PartialView("_AddBiometricRecord", rec);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBioMetricRecord(BiometricRecord model)
        {
            try
            {
                model.DateTime = model.Date.Date;
                model.DateTime = model.DateTime.Add(model.Time);

                if(model.AttendanceId <= 0) return BadRequest("Attendance was not not found");
                if(model.EmployeeId <= 0 && model.Id <=0) return BadRequest("Employee was not not found");

                if (model.Id <= 0)
                {
                    var nreC = new BiometricRecord
                    {
                        BiometricRecordType = BiometricRecordType.FingerPrint,
                        BiometricRecordState = model.BiometricRecordState,
                        CompanyId = userResolverService.GetCompanyId(),
                        Date = model.Date,
                        Time = model.Time,
                        DateTime = model.DateTime,
                        EmployeeId = model.EmployeeId,
                        Location = model.Location,
                        AttendanceId = model.AttendanceId,
                    };

                    await context.BiometricRecords.AddAsync(nreC);
                    await context.SaveChangesAsync();

                    await scheduleService.UpdateAttendanceWorkHours(nreC.AttendanceId);
                }
                else
                {
                    var biorecord = await context.BiometricRecords.FindAsync(model.Id);
                    if (biorecord == null)
                        return ThrowJsonError("Record was not found");

                    biorecord.Location = model.Location;
                    biorecord.BiometricRecordState = model.BiometricRecordState;
                    biorecord.Date = model.Date;
                    biorecord.Time = model.Time;
                    biorecord.DateTime = model.DateTime;
                    biorecord.IsActive = model.IsActive;

                    context.BiometricRecords.Update(biorecord);
                    await context.SaveChangesAsync();

                    await scheduleService.UpdateAttendanceWorkHours(model.AttendanceId);
                    //return ThrowJsonError("Working hours are calculated");
                    //return ThrowJsonError("Sorry! you dont have credentials to make changes");

                    // redirect to empl _view
                    model.EmployeeId = biorecord.EmployeeId;
                }

            }
            catch (Exception x)
            {
                return BadRequest($"{x.GetType().ToString()}: {x.Message}");
            }

            
            return RedirectToAction(nameof(AttendanceDayView) ,
                new{ date=model.Date, empId= model.EmployeeId,
             id = Request.Form["pId"]});
        }


        [HttpPost]
        public async Task<IActionResult> RemoveBioMetricRecord(int id, int pId)
        {
            var rec = await context.BiometricRecords
                .Include(a => a.Attendance)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (rec == null) return BadRequest("Record was not not found");

            context.BiometricRecords.Remove(rec);

            await context.SaveChangesAsync();

            try
            {
                if(rec.Attendance != null)
                await scheduleService.UpdateAttendanceWorkHours(rec.AttendanceId);
            }
            catch (Exception)
            {
                
            }

            return RedirectToAction(nameof(AttendanceDayView),
                new
                {
                    date = rec.Attendance.Date,
                    empId = rec.EmployeeId,
                    id = pId
                });
        }


        public async Task<IActionResult> GetPayrollAttendanceOverview(int id)
        {
            var payrollPeriod = await context.PayrollPeriods.FindAsync(id);
            if (payrollPeriod == null)
                return null;

            var empId = GetWorkingEmployeeId();
            var empSelectorVm = GetEmployeeSelectorModal();
            var start = payrollPeriod.StartDate.Date;
            var end = payrollPeriod.EndDate.Date;
            var totalDays = (end - start).TotalDays;
            var chartdata = await context.Attendances
                .Where(x => (empSelectorVm == null || empSelectorVm.EmployeeIds.Contains(x.EmployeeId)) &&
                    x.Date >= start && x.Date <= end && x.CompanyId == userResolverService.GetCompanyId())
                .GroupBy(x => new { x.Date })
                .Select(x => new
                {
                    Date = x.Key,
                    DateString = x.Key.Date.ToString("yyyy-MM-dd"),
                    TotalScheduledHours = x.Sum(a => (a.WorkEndTime - a.WorkStartTime).TotalHours),
                    ActualWorkedHours = x.Sum(a => a.TotalWorkedHours),
                    ActualWorkedHoursPerSchedule = x.Sum(a => a.TotalHoursWorkedPerSchedule),
                    TotalScheduledEmployees = x.Count(),
                    TotalAbsentEmployeeCount = x.Count(a => a.CurrentStatus == AttendanceStatus.Absent),
                    LateEmployeeCount = x.Count(a => a.CurrentStatus == AttendanceStatus.Late),
                    TotalLateHours = x.Sum(a => (int)(a.TotalLateMins / 60)),
                    TotalLateMins = x.Sum(a => a.TotalLateMins),
                }).ToListAsync();

            var data = Enumerable.Range(0, (int)(end - start).TotalDays).Select(i => new
            {
                Date = start.AddDays(i),
                DateString = start.AddDays(i).Date.ToString("yyyy-MM-dd"),
                TotalScheduledHours = chartdata.FirstOrDefault(c=> c.Date.Date == start.AddDays(i).Date)?.TotalScheduledHours ?? 0,
                ActualWorkedHours = chartdata.FirstOrDefault(c=> c.Date.Date == start.AddDays(i).Date)?.ActualWorkedHours ?? 0,
                ActualWorkedHoursPerSchedule = chartdata.FirstOrDefault(c=> c.Date.Date == start.AddDays(i).Date)?.ActualWorkedHoursPerSchedule ?? 0,
                TotalScheduledEmployees = chartdata.FirstOrDefault(c=> c.Date.Date == start.AddDays(i).Date)?.TotalScheduledEmployees ?? 0,
                TotalAbsentEmployeeCount = chartdata.FirstOrDefault(c=> c.Date.Date == start.AddDays(i).Date)?.TotalAbsentEmployeeCount ?? 0,
                LateEmployeeCount = chartdata.FirstOrDefault(c=> c.Date.Date == start.AddDays(i).Date)?.LateEmployeeCount ?? 0,
                TotalLateHours = chartdata.FirstOrDefault(c=> c.Date.Date == start.AddDays(i).Date)?.TotalLateHours ?? 0,
                TotalLateMins = chartdata.FirstOrDefault(c=> c.Date.Date == start.AddDays(i).Date)?.TotalLateMins ?? 0,
            }).ToList();

            return Json(new
            {
                chartdata = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWorkingEmployee(int id, string text)
        {
            UpdateWorkingEmployeeId(id, text);
            return ThrowJsonSuccess();
        }


        #endregion

        public async Task<IActionResult> AdjustmentsView(int id)
        {
            var empSelectorVm = GetEmployeeSelectorModal();
            var pp = new PayrollVm
            {
                PayrolPeriodId = id,
                AllPayAdjustments = await context.PayAdjustments
                 .Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.IsActive)
                 .OrderBy(x => x.CalculationOrder)
                 .ToListAsync(),
                //PayrolPayAdjustments = await context.PayrollPeriodPayAdjustments
                // .Where(x => x.PayrollPeriodId == id && (empId == 0 || empId == x.EmployeeId))
                // .Select(x => x.PayAdjustment)
                // .Distinct()
                // .OrderBy(x => x.CalculationOrder)
                // .ToListAsync(),
                EmployeePayAdjustmentTotals = await context.PayrollPeriodPayAdjustments
                 .Where(x => x.PayrollPeriodId == id && (empSelectorVm == null || empSelectorVm.EmployeeIds.Contains(x.EmployeeId)))
                 .GroupBy(x => new { x.PayAdjustmentId, x.PayAdjustment.Name })
                 .Select(x => new EmployeePayAdjustmentTotal
                 {
                     PayAdjustmentId = x.Key.PayAdjustmentId,
                     PayAdjustmentName = x.Key.Name,
                     Total = x.Sum(z => z.Total)
                 })
                 .ToListAsync()
            };

            return PartialView("_AdjustmentsSummary", pp);

            //ViewBag.PayrolPeriodId = id;
            //var empRecords = await context.PayrollPeriodPayAdjustments
            // .Where(x => x.PayrollPeriodId == id)
            // .Select(x => x.PayAdjustment)
            // .Distinct()
            // .OrderBy(x => x.CalculationOrder)
            // .ToListAsync();
            //ViewBag.PayAdjustments = await context.PayAdjustments
            // .Where(x => x.CompanyId == userResolverService.GetCompanyId())
            // .OrderBy(x => x.CalculationOrder)
            // .ToListAsync();

            //return PartialView("_AdjustmentsView", empRecords);
        }

        public async Task<IActionResult> Finals(int id, string query = "", int limit = 10, int page = 1)
        {
            var empSelectorVm = GetEmployeeSelectorModal();
            var payrol = await context.PayrollPeriods.FindAsync(id);
            if (payrol == null)
                return BadRequest("Payrol was not found");

            query = query.ToLower();
            var pagedEmps = await context.PayrollPeriodEmployees
                .Where(x => x.PayrollPeriodId == id
                 && (query == "" || x.Employee.GetSystemName(userResolverService
                .GetClaimsPrincipal()).ToLower().Contains(query))
                 && (query == "" || x.Employee.EmpID.ToString().Contains(query))
                 && (empSelectorVm == null || empSelectorVm.EmployeeIds.Contains(x.EmployeeId)))
                .OrderBy(x => x.EmpID)
                    .ThenBy(x => x.Employee.Department)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Include(x => x.VariationKeyValues)
                .Include(x => x.Employee)
                    .ThenInclude(x => x.Department)
                .ToListAsync();

            payrol.PayrollPeriodEmployees = pagedEmps;

            //if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            //    return PartialView("_PayrolMasterSheet", pagedEmps);

            //TempData["NextItem"] = await context.PayrollPeriods.Where(x => x.Id > id).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            //TempData["PrevItem"] = await context.PayrollPeriods.Where(x => x.Id < id).OrderBy(x => x.Id).FirstOrDefaultAsync();

            return PartialView("_Finals", new PayrollVm { PayrolPeriodId = payrol.Id, PayrollPeriod = payrol });
        }

        public async Task<IActionResult> Report(int id, int empId = 0, string query = "", int limit = 10, int page = 1)
        {
            var payrol = await context.PayrollPeriods.FindAsync(id);
            if (payrol == null)
                return BadRequest("Payrol was not found");

            List<(string, VariationType, decimal, decimal)> data = new List<(string, VariationType, decimal, decimal)>();
            var comaprePeiodEmployees = await context.PayrollPeriodEmployees
                .Where(x => x.PayrollPeriodId == id)
                .Include(x => x.VariationKeyValues)
                .Include(x => x.PayrollPeriod)
                .Include(x => x.Employee)
                .ThenInclude(a => a.Department)
                .ToListAsync();
            if(comaprePeiodEmployees.Count() <= 7)
            {
                ViewBag.IsError = true;
                ViewBag.InsufficientData = true;

                return PartialView("_Error", ("text-warning", "fa-exclamation-circle", "Oops! Insufficient data for reports", "Our reports requires minimun of 7 employees and minimum of 2 or more variable adjustments components"));
            }


            if (comaprePeiodEmployees == null)
            {
                ViewBag.IsError = true;
                return PartialView("_Error", ("text-danger", "fa-exclamation-triangle", "Sorry, Payroll is incomplete", "Payroll needs to be calculated to view reports"));

                return PartialView("_Report", (new List<EmployeePayCompare2D>(), data));
            }

            var topListed = comaprePeiodEmployees.OrderByDescending(x => x.VariationKeyValues.Where(v => v.Type == VariationType.VariableAddition).Sum(w => w.Value))
                     .Take(3).Select(x => new EmployeePayCompare2D
                     {
                         Id = x.EmployeeId,
                         Employee = new EmployeeSummaryVm(x.EmployeeId, x.Employee.NameDisplay, x.Employee.Avatar, x.Employee.Department.Name, x.Employee.CssClass, x.Employee.Grade, x.Employee.EmailWork, x.Employee.JobTitle),
                         EmpName = x.Employee.GetSystemName(User),
                         Avatar = x.Employee.Avatar,
                         NetPay = x.NetSalary,
                         TopAddAmnt = x.VariationKeyValues.Where(v => v.Type == VariationType.VariableAddition).Sum(w => w.Value),
                         AddArray = x.VariationKeyValues.Where(v => v.Type == VariationType.VariableAddition).OrderByDescending(w => w.Value).Take(3).ToArray(),
                         isAdd = true
                     }).ToList();

            var blackListed = comaprePeiodEmployees.OrderByDescending(x => x.VariationKeyValues.Where(v => v.Type == VariationType.VariableDeduction).Sum(w => w.Value))
                                .Take(3).Select(x => new EmployeePayCompare2D
                                {
                                    Id = x.EmployeeId,
                                    Employee = new EmployeeSummaryVm(x.EmployeeId, x.Employee.NameDisplay, x.Employee.Avatar, x.Employee.Department.Name, x.Employee.CssClass, x.Employee.Grade, x.Employee.EmailWork, x.Employee.JobTitle),
                                    EmpName = x.Employee.GetSystemName(User),
                                    Avatar= x.Employee.Avatar,
                                    NetPay = x.NetSalary,
                                    TopAddAmnt = x.VariationKeyValues.Where(v => v.Type == VariationType.VariableDeduction).Sum(w => w.Value),
                                    AddArray = x.VariationKeyValues.Where(v => v.Type == VariationType.VariableDeduction).OrderByDescending(w => w.Value).Take(3).ToArray(),
                                    isAdd = false
                                }).ToList();




            var _compareData = comaprePeiodEmployees.SelectMany(a => a.VariationKeyValues).ToList();
            var topPerformingVariants = topListed.SelectMany(a => a.AddArray).Select(a=> new { a.KeyId, a.Key })
                .Distinct().ToArray();
            var _top = topPerformingVariants.Select(x => new Tuple<string, int, decimal, decimal>
            (
                x.Key,
                _compareData.Where(a => a.KeyId == x.KeyId && a.Value > 0).Select(a=> a.PayrollPeriodEmployeeId).Distinct().Count(), // TotalEmpls
                _compareData.Where(c => c.KeyId == x.KeyId && c.Value > 0).Sum(z => z.Value),
                _compareData.Where(c => c.KeyId == x.KeyId && c.Value > 0).Average(z => z.Value)
            )).ToList();

            var bottomPerformingVariants = blackListed.SelectMany(a => a.AddArray).Select(a => new { a.KeyId, a.Key })
                .Distinct().ToArray();
            var _bottom = bottomPerformingVariants.Select(x => new Tuple<string, int, decimal, decimal>
            (
                x.Key,
                _compareData.Where(a => a.KeyId == x.KeyId && a.Value > 0).Select(a => a.PayrollPeriodEmployeeId).Distinct().Count(), // TotalEmpls
                _compareData.Where(c => c.KeyId == x.KeyId && c.Value > 0).Sum(z => z.Value),
                _compareData.Where(c => c.KeyId == x.KeyId && c.Value > 0).Average(z => z.Value)
            )).ToList();


            var employeePayCompar = new List<EmployeePayCompare2D>();
            employeePayCompar.AddRange(topListed);
            employeePayCompar.AddRange(blackListed);


            //List<(string, VariationType, decimal, decimal)> data;

            var payPeriodAdjustments = comaprePeiodEmployees.SelectMany(x => x.VariationKeyValues.Where(a => a.Value > 0m))
                .GroupBy(x => new { x.Type, x.Key, x.KeyId })
                .Where(v => (v.Key.Type == VariationType.VariableDeduction || v.Key.Type == VariationType.VariableAddition))
                .Select(x => new Tuple<string, VariationType, decimal, decimal>
                (
                    x.Key.Key,
                    x.Key.Type,
                    x.Max(a => a.Value),
                    x.Min(a => a.Value)
                )).ToList();


            ViewBag.PayAdjustments = context.PayAdjustments.Where(x => x.CompanyId == userResolverService.GetCompanyId()).ToDictionary(x => x.Id, x => x.Name);

            //if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            //    return PartialView("_PayrolMasterSheet", pagedEmps);

            //TempData["NextItem"] = await context.PayrollPeriods.Where(x => x.Id > id).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            //TempData["PrevItem"] = await context.PayrollPeriods.Where(x => x.Id < id).OrderBy(x => x.Id).FirstOrDefaultAsync();

            return PartialView("_Report", (employeePayCompar, payPeriodAdjustments, _top, _bottom));
        }

        public async Task<IActionResult> GetPayAdjustmentFieldValues(int id, int addId = 0, int page = 1, int tab = 1, int limit = 10, string query = null)
        {
            var empSelectorVm = GetEmployeeSelectorModal();
            ViewBag.FollowUpIndex = ((page - 1) * limit);

            if (addId > 0 && !context.PayAdjustments.Any(x => x.CompanyId == userResolverService.GetCompanyId() && x.Id == addId))
                return BadRequest("Item was not found");
            

            var _adds = await context.PayAdjustments
                .Where(x => x.CompanyId == userResolverService.GetCompanyId()) 
                .Include(x => x.Fields)
                .OrderByDescending(x => x.CalculationOrder > 0)
                .ThenBy(x => x.CalculationOrder)
                .ToListAsync();

            var _payperiods = context.PayrollPeriods.ToList();

            var adjustmentDropdown = new List<PayAdjustment> { new PayAdjustment { Id = 0, Name = "View all adjustments" } };
            //adjustmentDropdown.AddRange(_adds);

            ViewBag.PayrolPediodText = _payperiods.Find(x => x.Id == id).Name;
            ViewBag.ItemName = _adds.Find(x => x.Id == addId)?.Name;
            ViewBag.ItemType = _adds.Find(x => x.Id == addId)?.VariationType;
            ViewBag.ItemTypeCssClass = _adds.Find(x => x.Id == addId)?.VariationType.ToString().Contains("Addition") ?? false ? "text-success" : "text-danger";

            var index = _adds.FindIndex(x => x.Id == addId);
            if (index - 1 > -1)
                ViewBag.PrevItemId = _adds[index - 1].Id;
            if (index + 1 < _adds.Count())
                ViewBag.NextItemId = _adds[index + 1].Id;
            //ViewBag.ItemId = new SelectList(adjustmentDropdown, "Id", "Name", addId);
            //ViewBag.PayrolPeriodId = new SelectList(_payperiods, "Id", "Name", id);


            if (addId == 0)
                return RedirectToAction(nameof(AdjustmentsView), new { id });

            if (!string.IsNullOrWhiteSpace(query))
                query = query.ToLower();

            var payrolPayAdjustments = await context.PayrollPeriodPayAdjustments
                .Where(x => (addId == 0 || x.PayAdjustmentId == addId) && x.PayrollPeriodId == id &&
                (string.IsNullOrWhiteSpace(query) || x.Employee.GetSystemName(userResolverService
                .GetClaimsPrincipal()).ToLower().Contains(query)) &&
                (string.IsNullOrWhiteSpace(query) || x.Employee.EmpID.ToString() == query)
                && (empSelectorVm == null || empSelectorVm.EmployeeIds.Contains(x.EmployeeId)))
                .OrderBy(x => x.Employee.Department.DisplayOrder)
                .ThenBy(x => x.Employee.EmpID)
                .Skip((page - 1) * limit)
                .Take(limit)
                //.Include(x => x.PayrollPeriod)
                .Include(x => x.Employee)
                    .ThenInclude(x => x.Department)
                .Include(x => x.PayrollPeriodPayAdjustmentFieldValues)
                .ToListAsync();

            //payrolPayAdjustments.ForEach(t => t.PayrollPeriodPayAdjustmentFieldValues = t.PayrollPeriodPayAdjustmentFieldValues.OrderBy(x => x.CalculationOrder).ToList());
            ViewBag.FollowUpTabIndex = tab + 1;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var data = new PayItemVm
                {
                    ItemId = addId,
                    ItemName = ViewBag.ItemName,
                    PayrolPeriodId = id,
                    PayrolPayAdjustments = payrolPayAdjustments,
                };
                if (page > 1)
                    return PartialView("_ListingRow", data);
                else
                    return PartialView("_Listing", data);
            }



            //if (addId > 0)
            {
                return View(new PayItemVm
                {
                    ItemId = addId,
                    ItemName = ViewBag.ItemName,
                    PayrolPeriodId = id,
                    PayrolPeriod = _payperiods.Find(x => x.Id == id),
                    PayrolPayAdjustments = payrolPayAdjustments,
                });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> RemovePayAdjustmentFieldValues(int id, int addId = 0)
        {
            var payrolPeriodEmpl = await context.PayrollPeriodPayAdjustments.Where(x => x.PayrollPeriodId == id && x.PayAdjustmentId == addId)
                .Include(x => x.PayrollPeriodPayAdjustmentFieldValues)
                .ToListAsync();

            if (payrolPeriodEmpl != null)
            {
                context.PayrollPeriodPayAdjustmentFieldValues.RemoveRange(payrolPeriodEmpl.SelectMany(x => x.PayrollPeriodPayAdjustmentFieldValues));
                context.PayrollPeriodPayAdjustments.RemoveRange(payrolPeriodEmpl);
                context.SaveChanges();
                return Ok();
            }

            return ThrowJsonError();
        }


        public async Task<IActionResult> ViewPaySlip(int id)
        {
            var payrollEmp = await context.PayrollPeriodEmployees
                .Where(x => x.Id == id)
                .Include(x => x.VariationKeyValues)
                .Include(x => x.PayrollPeriod)
                    .ThenInclude(x => x.Company)
                .FirstOrDefaultAsync();

            if (payrollEmp == null)
                return ThrowJsonError("Payrol was not found");
            ViewBag.ShowAll = false;

            if (Request.IsAjaxRequest())
                return PartialView("_ViewPaySlip", payrollEmp);

            ViewData["ShowAll"] = true;
            return View(payrollEmp);
        }

        public async Task<IActionResult> DowmloadPaySlip(int id)
        {
            var payrollEmp = await context.PayrollPeriodEmployees
                .Where(x => x.Id == id)
                .Include(x => x.VariationKeyValues)
                .Include(x => x.PayrollPeriod)
                    .ThenInclude(x => x.Company)
                .FirstOrDefaultAsync();

            if (payrollEmp == null)
                return ThrowJsonError("Payrol was not found");
            ViewData["ShowAll"] = true;


            return new 
                ViewAsPdf("ViewPaySlip", payrollEmp, ViewData)
            {
                PageMargins = { Left = 10, Bottom = 10, Right = 10, Top = 10 },
                FileName = $"Payslip-{payrollEmp.PayrollPeriod.Name.GenerateSlug()}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                //CustomSwitches = "--footer-center \"  Created Date: " + DateTime.UtcNow.ToSystemFormat(User, true) + " \""
            };
        }

        public async Task<IActionResult> Detail(int id, string query = "", int limit = 10,int page = 1)
        {
            var payrol = await context.PayrollPeriods.FindAsync(id);
            if (payrol == null)
                return BadRequest("Payrol was not found");

            query = query.ToLower();
            var pagedEmps = await context.PayrollPeriodEmployees
                .Where(x => x.PayrollPeriodId == id
                 && (query == "" || x.Employee.GetSystemName(userResolverService
                .GetClaimsPrincipal()).ToLower().Contains(query))
                 && (query == "" || x.Employee.EmpID.ToString().Contains(query)))
                .OrderBy(x => x.EmpID)
                    .ThenBy(x => x.Employee.Department)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Include(x => x.VariationKeyValues)
                .Include(x => x.Employee)
                    .ThenInclude(x => x.Department)
                .ToListAsync();

            payrol.PayrollPeriodEmployees = pagedEmps;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_PayrolMasterSheet", pagedEmps);

            TempData["NextItem"] = await context.PayrollPeriods.Where(x=> x.Id > id).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            TempData["PrevItem"] = await context.PayrollPeriods.Where(x=> x.Id < id).OrderBy(x => x.Id).FirstOrDefaultAsync();

            return View(payrol);
        }

        /// <summary>
        /// Re-create all payrol period employees
        /// Remove old emoplyess, but copy values where same key
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Recalculate(int id)
        {
            var payrolPeriodEmpl = context.PayrollPeriodEmployees.Where(x => x.PayrollPeriodId == id);
            if (payrolPeriodEmpl != null)
            {
                context.PayrollPeriodEmployees.RemoveRange(payrolPeriodEmpl);
                context.SaveChanges();
            }

            /// ---------------------------

            var paypEriods = context.PayrollPeriods.Find(id);
            if (paypEriods == null)
                return BadRequest();

            await payrollService.RunPayrollAsync(payrolPeriodId: id);


            backgroundJobClient.Enqueue(() => payrollService.RunPerformanceAsync(id, paypEriods.Name));
            

            return Json(new { Message = "Success" });
        }

        public async Task<IActionResult> ExportPayroll(int id)
        {
            var paypEriods = await context.PayrollPeriods.FindAsync(id);
            if (paypEriods == null)
                return BadRequest();
            return PartialView("_ExportPayroll", paypEriods);
        }

        [HttpPost]
        public IActionResult ClearAll(int id)
        {
            var payrolPeriodEmpl = context.PayrollPeriodEmployees.Where(x => x.PayrollPeriodId == id);
            if (payrolPeriodEmpl != null)
            {
                context.PayrollPeriodEmployees.RemoveRange(payrolPeriodEmpl);
                context.SaveChanges();
            }
            
            return Json(new { Message = "Success" });
        }


        #region PayAdjustment CRUD
        public async Task<IActionResult> PayAdjustment(int payrolId, int addId = 0, int page = 1, int tab = 1, int limit = 10, string query = null)
        {
            ViewBag.FollowUpIndex = ((page - 1) * limit);

            if (addId > 0 && !context.PayAdjustments.Any(x => x.CompanyId == userResolverService.GetCompanyId() && x.Id == addId))
                return BadRequest("Item was not found");



            var _adds = await context.PayAdjustments
                .Where(x=> x.CompanyId == userResolverService.GetCompanyId())
                .Include(x => x.Fields)
                .OrderByDescending(x => x.CalculationOrder > 0)
                .ThenBy(x => x.CalculationOrder)
                .ToListAsync();

            var _payperiods = context.PayrollPeriods.ToList();

            var adjustmentDropdown = new List<PayAdjustment> { new PayAdjustment { Id = 0, Name = "View all adjustments" } };
            adjustmentDropdown.AddRange(_adds);

            ViewBag.PayrolPediodText = _payperiods.Find(x => x.Id == payrolId).Name;
            ViewBag.ItemName = _adds.Find(x => x.Id == addId)?.Name;
            ViewBag.ItemType = _adds.Find(x => x.Id == addId)?.VariationType;
            ViewBag.ItemTypeCssClass = _adds.Find(x => x.Id == addId)?.VariationType.ToString().Contains("Addition") ?? false ? "text-success" : "text-danger";


            ViewBag.ItemId = new SelectList(adjustmentDropdown, "Id", "Name", addId);
            ViewBag.PayrolPeriodId = new SelectList(_payperiods, "Id", "Name", payrolId);


            if (addId == 0)
            {
                ViewBag.PayAdjustments = await context.PayAdjustments
                .Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .Include(x => x.Fields)
                .OrderByDescending(x => x.CalculationOrder > 0)
                .ThenBy(x => x.CalculationOrder)
                .ToListAsync(); 



                //ViewBag.FieldCounts = await context.PayrollPeriodPayAdjustments.CountAsync(x => x.PayrollPeriodId == payrolId);
                ViewBag.EmployeeCount = await context.Employees.CountAsync();
                ViewBag.IsLoading = true;
                return View(new PayItemVm
                {
                    ItemId = addId,
                    ItemName = ViewBag.ItemName,
                    PayrolPeriodId = payrolId,
                    PayrolPeriod = await context.PayrollPeriods.FindAsync(payrolId),
                    //PayrolPayAdjustments = payrolPayAdjustments,
                });
            }

            if(!string.IsNullOrWhiteSpace(query))
                query = query.ToLower();

            var payrolPayAdjustments = await context.PayrollPeriodPayAdjustments
                .Where(x => (addId == 0 || x.PayAdjustmentId == addId) && x.PayrollPeriodId == payrolId && 
                (string.IsNullOrWhiteSpace(query) || x.Employee.GetSystemName(userResolverService
                .GetClaimsPrincipal()).ToLower().Contains(query)) &&
                (string.IsNullOrWhiteSpace(query) || x.Employee.EmpID.ToString() == query))
                .Skip((page - 1) * limit)
                .Take(limit)
                .OrderBy(x => x.Employee.Department.DisplayOrder)
                .ThenBy(x => x.Employee.EmpID)
                .Include(x => x.Employee)
                    .ThenInclude(x => x.Department)
                //.Include(x => x.PayrollPeriod)
                //.Include(x => x.PayAdjustment)
                .Include(x => x.PayrollPeriodPayAdjustmentFieldValues)
                .ToListAsync();

            //payrolPayAdjustments.ForEach(t => t.PayrollPeriodPayAdjustmentFieldValues = t.PayrollPeriodPayAdjustmentFieldValues.OrderBy(x => x.CalculationOrder).ToList());

            ViewBag.FollowUpTabIndex = tab + 1;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var data =  new PayItemVm
                {
                    ItemId = addId,
                    ItemName = ViewBag.ItemName,
                    PayrolPeriodId = payrolId,
                    PayrolPayAdjustments = payrolPayAdjustments,
                };
                if (page > 1)
                    return PartialView("_ListingRow", data);
                else
                    return PartialView("_Listing", data);
            }



            //if (addId > 0)
            {
                return View(new PayItemVm
                {
                    ItemId = addId,
                    ItemName = ViewBag.ItemName,
                    PayrolPeriodId = payrolId,
                    PayrolPeriod = _payperiods.Find(x => x.Id == payrolId),
                    PayrolPayAdjustments = payrolPayAdjustments,
                });
            }


            //var allEmpIdInPayrols = payrolPayAdjustments.Select(a => a.EmployeeId).Distinct().ToArray();
            //ViewBag.MissingEmployeeCount = await context.Employees.CountAsync(x => !allEmpIdInPayrols.Contains(x.Id));

            ////context.Entry(adjustments.Select(z=> z.PayrollPeriodPayAdjustmentFieldValues)).Collection(e => e.PayrollPeriodPayAdjustmentFieldValues)
            ////    .Query().OfType<PayrollPeriodPayAdjustmentFieldValues>()
            ////    .Include(x => x.VariationKeyValues)
            ////    .Load();

            //_adds.ForEach(x => x._fieldsCount = x.VariationType == VariationType.ConstantAddition || x.VariationType == VariationType.ConstantDeduction ? 2 : x.Fields.Count(z => z.IsActive));
            //ViewBag.PayAdjustments = _adds;



            ////ViewBag.FieldCounts = await context.PayrollPeriodPayAdjustments.CountAsync(x => x.PayrollPeriodId == payrolId);
            //ViewBag.EmployeeCount = await context.Employees.CountAsync();
            //ViewBag.IsLoading = true;
            //return View(new PayItemVm
            //{
            //    ItemId = addId,
            //    ItemName = ViewBag.ItemName,
            //    PayrolPeriodId = payrolId,
            //    PayrolPeriod = _payperiods.Find(x => x.Id == payrolId),
            //    PayrolPayAdjustments = payrolPayAdjustments,
            //});
        }

        [HttpPost]
        public async Task<IActionResult> OverviewPayAdjustments(int payrolId)
        {
            
            var _adds = await context.PayAdjustments
                .Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .Include(x => x.Fields)
                .OrderByDescending(x => x.CalculationOrder > 0)
                .ThenBy(x => x.CalculationOrder)
                .ToListAsync();

            _adds.ForEach(x => x._fieldsCount = x.VariationType == VariationType.ConstantAddition || x.VariationType == VariationType.ConstantDeduction ? 2 : x.Fields.Count(z => z.IsActive));
            ViewBag.PayAdjustments = _adds;

            var adFieldCounts = await context.PayrollPeriodPayAdjustments.Where(x => x.PayrollPeriodId == payrolId).GroupBy(x => x.PayAdjustmentId)
                .Select(x => new Tuple<int, int, int, decimal>
                (
                    x.Key,
                    x.Count(),
                    x.Sum(z => z.PayrollPeriodPayAdjustmentFieldValues.Count),
                    0m
                //x.SelectMany(z => z.PayrollPeriodPayAdjustmentFieldValues)
                //.Where(f => f.IsReturn).Sum(q=> decimal.Parse(q.Value))
                //x.SelectMany(w => w.PayrollPeriodPayAdjustmentFieldValues.Where(z => z.IsReturn)).Any() ? x.SelectMany(z => z.PayrollPeriodPayAdjustmentFieldValues.Where(q=> q.IsReturn)).Sum(r=> decimal.Parse(r.Value)) : 0m
                //x.Any(z=> z.PayrollPeriodPayAdjustmentFieldValues.Any()) ? x.SelectMany(z => z.PayrollPeriodPayAdjustmentFieldValues.Where(q => q.IsReturn)).Average(r => decimal.Parse(r.Value)) : 0m
                ))
                .ToListAsync();

            ViewBag.EmployeeCount = await context.Employees.CountAsync();
            ViewBag.FieldCounts = adFieldCounts;
            ViewBag.IsLoading = false;
            return PartialView("_OverviewPayAdjustments", new PayItemVm
            {
                PayrolPeriodId = payrolId,
            });
        }

        [HttpPost]
        [RequestFormLimits(ValueCountLimit = int.MaxValue)]
        public IActionResult SavePayAdjustments(PayItemVm model)
        {
            //if (ModelState.IsValid)
            //{
            var add_model = model.PayrolPayAdjustments;
            var addFieldValues = add_model.SelectMany(x => x.PayrollPeriodPayAdjustmentFieldValues);
            //add.ForEach(t =>
            //{
            //    t.PayrollPeriodId = model.PayrolPeriodId;
            //    t.PayAdjustmentId = model.ItemId;
            //    t.Adjustment = model.ItemName;
            //    t.EmployeeName = model.ItemName;
            //    // t.CalculationOrder = model.CalculationOrder
            //});

            var updatingPayrolAdju = model.PayrolPayAdjustments.Select(x => x.Id).ToArray();
            var inDbPayrolPayAdjs = context.PayrollPeriodPayAdjustments.Where(x => updatingPayrolAdju.Contains(x.Id))
                .Include(x=> x.PayrollPeriodPayAdjustmentFieldValues).ToList();
            foreach (var item in inDbPayrolPayAdjs)
            {
                if (add_model.Any(x => x.Id == item.Id) == false) continue;

                foreach (var fieldValue in item.PayrollPeriodPayAdjustmentFieldValues)
                {
                    if (add_model.Any(x => x.PayrollPeriodPayAdjustmentFieldValues.Any(z => z.Id == fieldValue.Id)) == false)
                    continue;

                    var confict = addFieldValues.FirstOrDefault(a => a.Id == fieldValue.Id);
                    if (fieldValue.Value != confict.Value)
                        fieldValue.Value = confict.Value;

                    //if (fieldValue.ListSelect != confict.ListSelect)
                    //    fieldValue.ListSelect = confict.ListSelect;
                }
            }
            

            inDbPayrolPayAdjs.ForEach(adj =>
            {
                var deci = 0m;
                try
                {
                    adj.Total = decimal.Parse(adj.PayrollPeriodPayAdjustmentFieldValues.FirstOrDefault(x => x.IsReturn)?.Value);
                }
                catch (Exception)
                {
                    adj.Total = deci;
                }
            });

            context.PayrollPeriodPayAdjustmentFieldValues.UpdateRange(inDbPayrolPayAdjs.SelectMany(x=> x.PayrollPeriodPayAdjustmentFieldValues));
            context.SaveChanges();

            return Ok();
            //return PartialView("_Listing", GetNewList(
            //    payrolId: add.FirstOrDefault().PayrollPeriodId, 
            //    addId: add.FirstOrDefault().PayAdjustmentId));
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePayAdjustmentFields(int id, int addId)
        {
            // return BadRequest("No Specified payrol was found");
            var _per = await context.PayrollPeriods.FindAsync(id);
            if (_per == null) return BadRequest("No Specified payrol was found");

            var adjustmentFieldConfigs = context.PayAdjustmentFieldConfigs
                    .Where(x => x.PayAdjustmentId == addId)
                    .ToList();
            //if ( adjustmentFieldConfigs == null || adjustmentFieldConfigs.Count <= 0)
            //    return BadRequest("No records are created, kindly check payadjustment configuration");

            var allempIdsInCurrent = 
                context.PayrollPeriodPayAdjustments
                .Include(x=> x.PayrollPeriodPayAdjustmentFieldValues)
                .Include(x=> x.Employee)
                .Where(x => x.PayrollPeriodId == id && x.PayAdjustmentId == addId)
                .ToList();



            // take in all master adjustment *incl field config
            var masterAddition = context.PayAdjustments.Find(addId);
            if(masterAddition == null) return BadRequest("No Specified Pay Adjustment was found");

            //if (allempIdsInCurrent.Count() <= 0) return BadRequest("No employees were assigned to " + masterAddition.Name);

            if (ModelState.IsValid)
            {
                var newList = await payAdjustmentService.GeneratePayAdjustmentsAndFieldValues(adjustmentFieldConfigs, masterAddition, payrolId: id, isSample: false);
                foreach (var item in newList)
                {
                    if(allempIdsInCurrent.Any(x=> x.EmployeeId == item.EmployeeId && x.PayAdjustmentId == item.PayAdjustmentId && x.PayrollPeriodId == item.PayrollPeriodId))
                    {
                        if (masterAddition.VariationType != VariationType.ConstantAddition && masterAddition.VariationType != VariationType.ConstantDeduction)
                        {
                            // update ONLY for ADDITIONS AND DEDUCTIONS (ignore constants => need to be updated always to employee entered value)
                            // and not aggregated values
                            var oldPayAdj = allempIdsInCurrent.FirstOrDefault(x => x.EmployeeId == item.EmployeeId && x.PayAdjustmentId == item.PayAdjustmentId && x.PayrollPeriodId == item.PayrollPeriodId);
                            foreach (var column in item.PayrollPeriodPayAdjustmentFieldValues.Where(x=> !x.IsAggregated))
                            {
                                if (oldPayAdj.PayrollPeriodPayAdjustmentFieldValues.Any(x => x.Key == column.Key) && oldPayAdj.PayrollPeriodPayAdjustmentFieldValues.FirstOrDefault(x => x.Key == column.Key)?.Value  != column.Value)
                                    column.Value = oldPayAdj.PayrollPeriodPayAdjustmentFieldValues.FirstOrDefault(x => x.Key == column.Key)?.Value;
                            }
                        }
                    }
                }

                context.PayrollPeriodPayAdjustmentFieldValues.RemoveRange(allempIdsInCurrent.SelectMany(x=> x.PayrollPeriodPayAdjustmentFieldValues));
                context.PayrollPeriodPayAdjustments.RemoveRange(allempIdsInCurrent);

                context.PayrollPeriodPayAdjustments.AddRange(newList);
                context.SaveChanges();

                return Ok();
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> RemovePayAdjustment(int payAdjustmentId = 0)
        {
            if (ModelState.IsValid)
            {
                var add = await context.PayrollPeriodPayAdjustments.Include(x=> x.PayrollPeriodPayAdjustmentFieldValues)
                    .FirstOrDefaultAsync(x=> x.Id == payAdjustmentId);
                if (add == null)
                    return BadRequest("Oooh! we didnt find that one");

                var payrolId = add.PayrollPeriodId;
                var _addId = add.PayAdjustmentId;


                context.PayrollPeriodPayAdjustmentFieldValues.RemoveRange(add.PayrollPeriodPayAdjustmentFieldValues);
                context.PayrollPeriodPayAdjustments.Remove(add);
                context.SaveChanges();

                return Ok();
            }

            return BadRequest();
        }
        #endregion

        #region Export 
        public async Task<IActionResult> GenerateBankSheet(int payrolId)
        {
            byte[] fileContents;
            var payrol = await context.PayrollPeriods
                .Where(x => x.Id == payrolId)
                .Include(x=> x.Company)
                .FirstOrDefaultAsync();
            var payrolEmpls = await context.PayrollPeriodEmployees
                .Include(x=> x.Employee)
                .Where(x=> x.PayrollPeriodId == payrolId)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Bank Sheet");

                // Put whatever you want here in the sheet
                // For example, for cell on row1 col1
                worksheet.Cells[1, 1].Value = "Raajje Television Pvt Ltd";
                worksheet.Cells[2, 1].Value = "Bank: BML MVR account";

                worksheet.Cells[3, 1].Value = "SL NO";
                worksheet.Cells[3, 2].Value = "Account";
                worksheet.Cells[3, 3].Value = "Account Name";
                worksheet.Cells[3, 4].Value = "Amount";
                worksheet.Cells[3, 1, 3, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[3, 1, 3, 4].Style.Border.Right.Style = (ExcelBorderStyle.Thin);

                int i = 4;
                foreach (var item in payrolEmpls)
                {
                    worksheet.Cells[i, 1].Value = i - 3;
                    worksheet.Cells[i, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i, 2].Value = item.Employee.BankAccountNumber;
                    worksheet.Cells[i, 3].Value = item.Employee.BankAccountName;
                    worksheet.Cells[i, 4].Value = item.NetSalary;
                    worksheet.Cells[i, 1, i, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[i, 1, i, 4].Style.Border.Right.Style = (ExcelBorderStyle.Thin);
                    i++;
                }


                worksheet.Cells[i, 3].Value = "SALARY COMMISSION";
                worksheet.Cells[i, 4].Value = 0m;
                worksheet.Cells[i, 3, i, 4].Style.Font.Bold = true;
                worksheet.Cells[i, 3, i, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[i, 3, i, 4].Style.Border.Right.Style = (ExcelBorderStyle.Thin);
                i++;
                worksheet.Cells[i, 3].Value = "G.Total";
                worksheet.Cells[i, 4].Value = payrolEmpls.Sum(x => x.NetSalary);
                worksheet.Cells[i, 3, i, 4].Style.Font.Bold = true;
                worksheet.Cells[i, 3, i, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[i, 3, i, 4].Style.Border.Right.Style = (ExcelBorderStyle.Thin);


                i += 2;
                worksheet.Cells[i, 2].Value = "Contact Name";
                worksheet.Cells[i, 3].Value = "Aiminath Reesha";
                i++;
                worksheet.Cells[i, 2].Value = "Contact Number";
                worksheet.Cells[i, 3].Value = "7783212";
                i++;
                worksheet.Cells[i, 2].Value = "* NOTE: Salary Com MRF5.00 & USD 0.39 per BML account";


                i += 3;
                worksheet.Cells[i, 2].Value = "Approved By";
                worksheet.Cells[i, 2].Style.Font.Bold = true;
                worksheet.Cells[i, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[i, 2, i, 3].Merge = true;

                i += 3;
                worksheet.Cells[i, 2].Value = payrol.Company.ManagingDirector; // "Ahmed Saleem";
                worksheet.Cells[i, 2].Style.Font.Bold = true;
                worksheet.Cells[i, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[i, 2, i, 3].Merge = true;
                i += 1;
                worksheet.Cells[i, 2].Value = "Managing Directory";
                worksheet.Cells[i, 2].Style.Font.Bold = true;
                worksheet.Cells[i, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[i, 2, i, 3].Merge = true;
                //  SALARY COMMISSION 
                // G.Total

                //worksheet.Cells[1, 1].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                // So many things you can try but you got the idea.

                // Finally when you're done, export it to byte array.
                fileContents = package.GetAsByteArray();
            }

            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: $"bank-sheet-{payrol.Name.GenerateSlug()}.xlsx"
            );
        }
        public async Task<IActionResult> GeneratePensionSheet300(int payrolId)
        {
            byte[] fileContents;
            var payrol = await context.PayrollPeriods
                .Where(x => x.Id == payrolId)
                .Include(a=> a.Company)
                .FirstOrDefaultAsync();
            
            // all emps where status = "Just Enrolled" < 1 month and (This month)
            var payrolEmpls = await context.PayrollPeriodEmployees
                .Include(x => x.VariationKeyValues)
                .Include(x => x.Employee)
                    .ThenInclude(x => x.EmployeePayComponents)
                .Where(x => x.PayrollPeriodId == payrolId && x.Employee.DateOfJoined >= x.PayrollPeriod.StartDate && x.Employee.DateOfJoined < x.PayrollPeriod.EndDate)
                .ToListAsync();
            var payAdju_BasicSalaryId = await context.PayAdjustments.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Name == "Basic Salary").Select(x => x.Id).FirstOrDefaultAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("MPS300");
                worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                // Put whatever you want here in the sheet
                // For example, for cell on row1 col1
                worksheet.Cells[1, 1].Value = "T"; // "Raajje Television Pvt Ltd";
                worksheet.Cells[1, 2].Value = "SECTION I   Pension Contribution Identification";
                worksheet.Cells[1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[1, 2].Style.Font.Bold = true;
                worksheet.Cells[1, 2, 1, 4].Merge = true;

                // row 1 - 4 [SECTION I   Pension Contribution Identification]
                worksheet.Cells[2, 1].Value = "T";
                worksheet.Cells[2, 2].Value = "Month";
                worksheet.Cells[2, 3].Value = "Year";
                worksheet.Cells[2, 4].Value = "Type of Statement";
                worksheet.Cells[3, 1].Value = "1";
                worksheet.Cells[3, 2].Value = payrol.StartDate.Date.Month; // "7";
                worksheet.Cells[3, 3].Value = payrol.StartDate.Date.Year; // "Year";
                worksheet.Cells[3, 4].Value = 300; // "Type of Statement";
                worksheet.Cells[4, 1].Value = "T";
                worksheet.Cells[4, 2].Value = 1.1;
                worksheet.Cells[4, 3].Value = 1.2;
                worksheet.Cells[4, 4].Value = 1.3;

                for (int i = 2; i <= 4; i++)
                {
                    worksheet.Cells[i, 2, i, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[i, 3, i, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[i, 4, i, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                worksheet.Cells[2, 2, 2, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[2, 2, 2, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[4, 2, 4, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[4, 2, 4, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // right
                worksheet.Cells[2, 8].Value = "Total Employees";
                worksheet.Cells[2, 10].Value = "Total Contributions";
                worksheet.Cells[2, 12].Value = "Last Payment Date of Salaries";
                worksheet.Cells[2, 14].Value = "SPC Version";
                worksheet.Cells[3, 8].Value = payrolEmpls.Count();
                worksheet.Cells[3, 10].Value = (payrolEmpls.Sum(z => z.VariationKeyValues.Where(x => x.Key == "Pension Charge").Sum(x => x.Value)) * 2);
                worksheet.Cells[3, 10].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[3, 12].Value = "31.07.2019";
                worksheet.Cells[3, 14].Value = "MRPS-02-10-01";
                worksheet.Cells[4, 8].Value = 1.4;
                worksheet.Cells[4, 10].Value = 1.5;
                worksheet.Cells[4, 12].Value = 1.6;
                worksheet.Cells[4, 14].Value = 1.7;
                worksheet.Cells[2, 8, 2, 9].Merge = true;
                worksheet.Cells[2, 10, 2, 11].Merge = true;
                worksheet.Cells[2, 12, 2, 13].Merge = true;
                worksheet.Cells[3, 8, 3, 9].Merge = true;
                worksheet.Cells[3, 10, 3, 11].Merge = true;
                worksheet.Cells[3, 12, 3, 13].Merge = true;
                worksheet.Cells[4, 8, 4, 9].Merge = true;
                worksheet.Cells[4, 10, 4, 11].Merge = true;
                worksheet.Cells[4, 12, 4, 13].Merge = true;

                worksheet.Cells[4, 2, 4, 14].Style.Font.Bold = true;


                worksheet.Cells[2, 8, 4, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                for (int i = 2; i <= 4; i++)
                {
                    worksheet.Cells[i, 14].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }


                worksheet.Cells[2, 8, 2, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[4, 8, 4, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[3, 8, 3, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Transparent);
                //worksheet.Cells[3, 13].Style.Border.Right.Style = (ExcelBorderStyle.Thin);
                //worksheet.Cells[3, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);



                //--------------- row 6 - 9 [SECTION II    Employer Identification]
                worksheet.Cells[6, 1].Value = "T"; // "Raajje Television Pvt Ltd";
                worksheet.Cells[6, 2].Value = "SECTION II    Employer Identification";
                worksheet.Cells[6, 2, 6, 7].Merge = true;
                worksheet.Cells[6, 2].Style.Font.Bold = true;
                worksheet.Cells[6, 2, 6, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                worksheet.Cells[7, 1].Value = "T";
                worksheet.Cells[7, 2].Value = "Identifier Type";
                worksheet.Cells[7, 3].Value = "Employer ID";
                worksheet.Cells[7, 4].Value = "Name of Employer";
                worksheet.Cells[8, 1].Value = 2;
                worksheet.Cells[8, 2].Value = "10";
                worksheet.Cells[8, 3].Value = "20110225300";
                worksheet.Cells[8, 4].Value = payrol.Company.Name; // "Raajje Television Pvt. Ltd.";
                worksheet.Cells[9, 1].Value = "T";
                worksheet.Cells[9, 2].Value = 2.1;
                worksheet.Cells[9, 3].Value = 2.2;
                worksheet.Cells[9, 4].Value = 2.3;
                worksheet.Cells[7, 4, 7, 6].Merge = true;
                worksheet.Cells[8, 4, 8, 6].Merge = true;
                worksheet.Cells[9, 4, 9, 6].Merge = true;
                worksheet.Cells[9, 2, 9, 14].Style.Font.Bold = true;

                for (int row = 7; row <= 9; row++)
                {
                    for (int col = 2; col <= 6; col++)
                    {
                        worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        if(row != 8)
                        {
                            worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }
                    }
                }

                //worksheet.Cells[7, 2, 9, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                //worksheet.Cells[7, 2, 7, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                //worksheet.Cells[7, 2, 7, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); //  =  (ExcelBorderStyle.Thin);
                //worksheet.Cells[9, 2, 9, 4].Style.Border.Right.Style = (ExcelBorderStyle.Thin);
                //worksheet.Cells[9, 2, 9, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                //worksheet.Cells[8, 2, 8, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Transparent);


                // right
                worksheet.Cells[6, 8].Value = "Person Responsible for Pension Calculation ";
                worksheet.Cells[6, 8].Style.Font.Bold = true;
                worksheet.Cells[6, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                worksheet.Cells[7, 8].Value = "Contact Name";
                worksheet.Cells[7, 11].Value = "Telephone";
                worksheet.Cells[7, 12].Value = "Mobile";
                worksheet.Cells[7, 13].Value = "Email Address";
                worksheet.Cells[8, 8].Value = "RIFQA ABDUL WAHHAB";
                worksheet.Cells[8, 11].Value = "3007773";
                worksheet.Cells[8, 12].Value = "7722774";
                worksheet.Cells[8, 13].Value = "rifqa.wahhab@raajjetv.tv";
                worksheet.Cells[9, 8].Value = 2.4;
                worksheet.Cells[9, 11].Value = 2.5;
                worksheet.Cells[9, 12].Value = 2.6;
                worksheet.Cells[9, 13].Value = 2.7;

                worksheet.Cells[6, 8, 6, 10].Merge = true;
                worksheet.Cells[7, 8, 7, 10].Merge = true;
                worksheet.Cells[8, 8, 8, 10].Merge = true;
                worksheet.Cells[9, 8, 9, 10].Merge = true;

                worksheet.Cells[7, 13, 7, 14].Merge = true;
                worksheet.Cells[8, 13, 8, 14].Merge = true;
                worksheet.Cells[9, 13, 9, 14].Merge = true;


                for (int row = 7; row <= 9; row++)
                {
                    for (int col = 8; col <= 14; col++)
                    {
                        worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        if (row != 8)
                        {
                            worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }
                    }
                }

                //worksheet.Cells[7, 7, 9, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                //worksheet.Cells[7, 7, 7, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                //worksheet.Cells[7, 7, 7, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray); //  =  (ExcelBorderStyle.Thin);
                //worksheet.Cells[9, 7, 9, 12].Style.Border.Right.Style = (ExcelBorderStyle.Thin);
                //worksheet.Cells[9, 7, 9, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);



                //---------- row 11 [SECTION III   Employee Identification and Calcultaion]
                worksheet.Cells[11, 1].Value = "T";
                worksheet.Cells[11, 2].Value = "SECTION III   Employee Identification and Calcultaion";
                worksheet.Cells[11, 2, 11, 5].Merge = true;
                worksheet.Cells[11, 2].Style.Font.Bold = true;
                worksheet.Cells[11, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                //worksheet.Cells[12, 2, 12, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[12, 1].Value = "T";
                worksheet.Cells[12, 2].Value = "Worker Identification";
                worksheet.Cells[12, 2, 12, 6].Merge = true;
                worksheet.Cells[12, 2, 12, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[12, 7].Value = "Employment Information";
                worksheet.Cells[12, 7, 12, 8].Merge = true;
                worksheet.Cells[12, 7, 12, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[12, 9].Value = "Base for Calculation";
                worksheet.Cells[12, 9, 12, 10].Merge = true;
                worksheet.Cells[12, 9, 12, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[12, 11].Value = "Calculated Pension Contribution Amounts";
                worksheet.Cells[12, 11, 12, 14].Merge = true;
                worksheet.Cells[12, 11, 12, 14].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                worksheet.Cells[13, 1].Value = "T";
                worksheet.Cells[14, 1].Value = "T";
                worksheet.Cells[15, 1].Value = "T";
                worksheet.Cells[13, 2].Value = "Type Identifier";
                worksheet.Cells[13, 2, 15, 2].Merge = true;
                worksheet.Cells[13, 3].Value = "Identifier";
                worksheet.Cells[13, 3, 15, 3].Merge = true;
                worksheet.Cells[13, 4].Value = "Name of worker";
                worksheet.Cells[13, 4, 15, 5].Merge = true;
                //worksheet.Cells[13, 4, 13, 5].Merge = true;
                worksheet.Cells[14, 6].Value = "Date of Birth";
                worksheet.Cells[15, 6].Value = "dd.mm.yyyy";
                worksheet.Cells[14, 7].Value = "Commencing date";
                worksheet.Cells[15, 7].Value = "dd.mm.yyyy";
                worksheet.Cells[14, 8].Value = "Termination date";
                worksheet.Cells[15, 8].Value = "dd.mm.yyyy";
                worksheet.Cells[13, 9].Value = "Reduced Basic Salary codes and Contribution Exemption Code";
                worksheet.Cells[13, 9].Style.WrapText = true;
                
                worksheet.Cells[13, 9, 15, 9].Merge = true;
                worksheet.Cells[13, 10].Value = "Pensionable Wage";
                worksheet.Cells[13, 10, 15, 10].Merge = true;

                worksheet.Cells[13, 11].Value = "Mandatory Contributions";
                worksheet.Cells[13, 11, 13, 12].Merge = true;
                worksheet.Cells[14, 11].Value = "Employee";
                worksheet.Cells[14, 11, 15, 11].Merge = true;
                worksheet.Cells[14, 12].Value = "Employer";
                worksheet.Cells[14, 12, 15, 12].Merge = true;


                worksheet.Cells[13, 13].Value = "Voluntary contributions";
                worksheet.Cells[13, 13, 13, 14].Merge = true;
                worksheet.Cells[14, 13].Value = "Employee";
                worksheet.Cells[14, 13, 15, 13].Merge = true;
                worksheet.Cells[14, 14].Value = "Employer";
                worksheet.Cells[14, 14, 15, 14].Merge = true;



                for (int row = 12; row <= 15; row++)
                {
                    for (int col = 2; col <= 14; col++)
                    {
                        worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        if (row != 8)
                        {
                            worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }
                    }
                }

                // remove borders from dob,comdate,term
                for (int col = 6; col <= 8; col++)
                {
                    worksheet.Cells[13, col].Style.Border.BorderAround(ExcelBorderStyle.None);
                    worksheet.Cells[14, col].Style.Border.BorderAround(ExcelBorderStyle.None);
                    worksheet.Cells[15, col].Style.Border.BorderAround(ExcelBorderStyle.None);

                    worksheet.Cells[13, col, 15, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }


                worksheet.Cells.Where(x => x.Merge).ToList()
                    .ForEach(t => t.Style.Border.BorderAround(ExcelBorderStyle.Thin));

                worksheet.Cells[11, 2, 11, 5].Style.Border.BorderAround(ExcelBorderStyle.None);
                worksheet.Cells[6, 8].Style.Border.BorderAround(ExcelBorderStyle.None);
                worksheet.Cells[6, 2, 6, 4].Style.Border.BorderAround(ExcelBorderStyle.None);
                //worksheet.Cells.Where(x => x.Merge && x.Rows == 6 && x.Rows == 11).ToList()
                //    .ForEach(t => t.Style.Border.BorderAround(ExcelBorderStyle.None));


                // ***** add 3.1,3.2,....3.12 rows
                for (int cell = 1; cell <= 14; cell++)
                {
                    if (cell == 1)
                    {
                        worksheet.Cells[16, cell].Value = "T";
                        continue;
                    }
                    if (cell == 5)
                    {
                        worksheet.Cells[16, cell-1, 16, cell].Merge = true;
                        worksheet.Cells[16, cell].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        continue;
                    }
                    int decPoint = cell > 9 ? 2: 1;
                    worksheet.Cells[16, cell].Value = Math.Round(decimal.Parse("3." + (cell >= 5 ? cell - 2 : cell-1)), decPoint);
                    worksheet.Cells[16, cell].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[16, cell].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[16, cell].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }


                int rowIndx = 17;
                foreach (var emp in payrolEmpls)
                {
                    for (int cell = 1; cell <= 14; cell++)
                    {
                        if(cell == 1)
                        {
                            worksheet.Cells[rowIndx, cell].Value = 3;
                            continue;
                        }
                        if(cell == 2)
                            worksheet.Cells[rowIndx, cell].Value = 1;
                        if (cell == 3)
                            worksheet.Cells[rowIndx, cell].Value = emp.Employee?.IdentityNumber;
                        if (cell == 4)
                            worksheet.Cells[rowIndx, cell].Value = emp.Employee.GetSystemName(User);
                        if (cell == 5)
                        {
                            worksheet.Cells[rowIndx, cell - 1, rowIndx, cell].Merge = true;
                            worksheet.Cells[rowIndx, cell].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            continue;
                        }
                        if (cell == 6)
                            worksheet.Cells[rowIndx, cell].Value = emp.Employee.DateOfJoined?.ToString("dd.MM.yyyy");
                        if (cell == 7)
                            worksheet.Cells[rowIndx, cell].Value = emp.Employee.DateOfJoined?.ToString("dd.MM.yyyy");
                        if (cell == 10)
                            worksheet.Cells[rowIndx, cell].Value = emp.Employee.EmployeePayComponents.FirstOrDefault(x => x.PayAdjustmentId == payAdju_BasicSalaryId)?.Total ?? 0;
                        if (cell == 11)
                            worksheet.Cells[rowIndx, cell].Value = emp.VariationKeyValues.FirstOrDefault(x => x.Key == "Pension Charge")?.Value ?? 0;
                        if (cell == 12)
                            worksheet.Cells[rowIndx, cell].Value = emp.VariationKeyValues.FirstOrDefault(x => x.Key == "Pension Charge")?.Value ?? 0;

                        int decPoint = cell >= 9 ? 2 : 1;
                        worksheet.Cells[rowIndx, cell].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    worksheet.Cells[rowIndx, 10, rowIndx, 12].Style.Numberformat.Format = "#,##0.00";
                    rowIndx++;
                }

                worksheet.Cells[rowIndx, 2].Value = payrolEmpls.Count();
                worksheet.Cells[rowIndx, 11].Value = payrolEmpls.Sum(z => z.VariationKeyValues.Where(x => x.Key == "Pension Charge").Sum(x => x.Value));
                worksheet.Cells[rowIndx, 12].Value = payrolEmpls.Sum(z => z.VariationKeyValues.Where(x => x.Key == "Pension Charge").Sum(x => x.Value));
                worksheet.Cells[rowIndx, 11, rowIndx, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[rowIndx, 11, rowIndx, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[rowIndx, 11, rowIndx, 12].Style.Numberformat.Format = "#,##0.00";

                fileContents = package.GetAsByteArray();

            }

            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: $"MPS300-{payrol.Name.GenerateSlug()}.xlsx"
            );
        }
        public async Task<IActionResult> GeneratePensionSheet101(int payrolId)
        {
            byte[] fileContents;
            var payrol = await context.PayrollPeriods
                .Where(x => x.Id == payrolId)
                .FirstOrDefaultAsync();

            var payrolEmpls = await context.PayrollPeriodEmployees
                .Include(x => x.VariationKeyValues)
                .Include(x => x.Employee)
                    .ThenInclude(x => x.EmployeePayComponents)
                .Where(x => x.PayrollPeriodId == payrolId)
                .ToListAsync();
            var payAdju_BasicSalaryId = await context.PayAdjustments.Where(x => x.Name == "Basic Salary" && x.CompanyId == userResolverService.GetCompanyId()).Select(x => x.Id).FirstOrDefaultAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("MPS101");
                worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                // Put whatever you want here in the sheet
                // For example, for cell on row1 col1
                worksheet.Cells[1, 1].Value = "T"; // "Raajje Television Pvt Ltd";
                worksheet.Cells[1, 2].Value = "SECTION I   Pension Contribution Identification";
                worksheet.Cells[1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[1, 2].Style.Font.Bold = true;
                worksheet.Cells[1, 2, 1, 4].Merge = true;

                // row 1 - 4 [SECTION I   Pension Contribution Identification]
                worksheet.Cells[2, 1].Value = "T";
                worksheet.Cells[2, 2].Value = "Month";
                worksheet.Cells[2, 3].Value = "Year";
                worksheet.Cells[2, 4].Value = "Type of Statement";
                worksheet.Cells[3, 1].Value = "1";
                worksheet.Cells[3, 2].Value = payrol.StartDate.Date.Month; // "7";
                worksheet.Cells[3, 3].Value = payrol.StartDate.Date.Year; // "Year";
                worksheet.Cells[3, 4].Value = 101; // "Type of Statement";
                worksheet.Cells[4, 1].Value = "T";
                worksheet.Cells[4, 2].Value = 1.1;
                worksheet.Cells[4, 3].Value = 1.2;
                worksheet.Cells[4, 4].Value = 1.3;

                for (int i = 2; i <= 4; i++)
                {
                    worksheet.Cells[i, 2, i, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[i, 3, i, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[i, 4, i, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                worksheet.Cells[2, 2, 2, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[2, 2, 2, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[4, 2, 4, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[4, 2, 4, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // right
                worksheet.Cells[2, 8].Value = "Total Employees";
                worksheet.Cells[2, 10].Value = "Total Contributions";
                worksheet.Cells[2, 12].Value = "Last Payment Date of Salaries";
                worksheet.Cells[2, 14].Value = "SPC Version";
                worksheet.Cells[3, 8].Value = payrolEmpls.Count();
                worksheet.Cells[3, 10].Value = (payrolEmpls.Sum(z => z.VariationKeyValues.Where(x => x.Key == "Pension Charge").Sum(x => x.Value)) * 2);
                worksheet.Cells[3, 10].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[3, 12].Value = "31.07.2019";
                worksheet.Cells[3, 14].Value = "MRPS-02-10-01";
                worksheet.Cells[4, 8].Value = 1.4;
                worksheet.Cells[4, 10].Value = 1.5;
                worksheet.Cells[4, 12].Value = 1.6;
                worksheet.Cells[4, 14].Value = 1.7;
                worksheet.Cells[2, 8, 2, 9].Merge = true;
                worksheet.Cells[2, 10, 2, 11].Merge = true;
                worksheet.Cells[2, 12, 2, 13].Merge = true;
                worksheet.Cells[3, 8, 3, 9].Merge = true;
                worksheet.Cells[3, 10, 3, 11].Merge = true;
                worksheet.Cells[3, 12, 3, 13].Merge = true;
                worksheet.Cells[4, 8, 4, 9].Merge = true;
                worksheet.Cells[4, 10, 4, 11].Merge = true;
                worksheet.Cells[4, 12, 4, 13].Merge = true;

                worksheet.Cells[4, 2, 4, 14].Style.Font.Bold = true;


                worksheet.Cells[2, 8, 4, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                for (int i = 2; i <= 4; i++)
                {
                    worksheet.Cells[i, 14].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }


                worksheet.Cells[2, 8, 2, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[4, 8, 4, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[3, 8, 3, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Transparent);
                //worksheet.Cells[3, 13].Style.Border.Right.Style = (ExcelBorderStyle.Thin);
                //worksheet.Cells[3, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);



                //--------------- row 6 - 9 [SECTION II    Employer Identification]
                worksheet.Cells[6, 1].Value = "T"; // "Raajje Television Pvt Ltd";
                worksheet.Cells[6, 2].Value = "SECTION II    Employer Identification";
                worksheet.Cells[6, 2, 6, 7].Merge = true;
                worksheet.Cells[6, 2].Style.Font.Bold = true;
                worksheet.Cells[6, 2, 6, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                worksheet.Cells[7, 1].Value = "T";
                worksheet.Cells[7, 2].Value = "Identifier Type";
                worksheet.Cells[7, 3].Value = "Employer ID";
                worksheet.Cells[7, 4].Value = "Name of Employer";
                worksheet.Cells[8, 1].Value = 2;
                worksheet.Cells[8, 2].Value = "10";
                worksheet.Cells[8, 3].Value = "20110225300";
                worksheet.Cells[8, 4].Value = "Raajje Television Pvt. Ltd.";
                worksheet.Cells[9, 1].Value = "T";
                worksheet.Cells[9, 2].Value = 2.1;
                worksheet.Cells[9, 3].Value = 2.2;
                worksheet.Cells[9, 4].Value = 2.3;
                worksheet.Cells[7, 4, 7, 6].Merge = true;
                worksheet.Cells[8, 4, 8, 6].Merge = true;
                worksheet.Cells[9, 4, 9, 6].Merge = true;
                worksheet.Cells[9, 2, 9, 14].Style.Font.Bold = true;

                for (int row = 7; row <= 9; row++)
                {
                    for (int col = 2; col <= 6; col++)
                    {
                        worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        if (row != 8)
                        {
                            worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }
                    }
                }

                //worksheet.Cells[7, 2, 9, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                //worksheet.Cells[7, 2, 7, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                //worksheet.Cells[7, 2, 7, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); //  =  (ExcelBorderStyle.Thin);
                //worksheet.Cells[9, 2, 9, 4].Style.Border.Right.Style = (ExcelBorderStyle.Thin);
                //worksheet.Cells[9, 2, 9, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                //worksheet.Cells[8, 2, 8, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Transparent);


                // right
                worksheet.Cells[6, 8].Value = "Person Responsible for Pension Calculation ";
                worksheet.Cells[6, 8].Style.Font.Bold = true;
                worksheet.Cells[6, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                worksheet.Cells[7, 8].Value = "Contact Name";
                worksheet.Cells[7, 11].Value = "Telephone";
                worksheet.Cells[7, 12].Value = "Mobile";
                worksheet.Cells[7, 13].Value = "Email Address";
                worksheet.Cells[8, 8].Value = "RIFQA ABDUL WAHHAB";
                worksheet.Cells[8, 11].Value = "3007773";
                worksheet.Cells[8, 12].Value = "7722774";
                worksheet.Cells[8, 13].Value = "rifqa.wahhab@raajjetv.tv";
                worksheet.Cells[9, 8].Value = 2.4;
                worksheet.Cells[9, 11].Value = 2.5;
                worksheet.Cells[9, 12].Value = 2.6;
                worksheet.Cells[9, 13].Value = 2.7;

                worksheet.Cells[6, 8, 6, 10].Merge = true;
                worksheet.Cells[7, 8, 7, 10].Merge = true;
                worksheet.Cells[8, 8, 8, 10].Merge = true;
                worksheet.Cells[9, 8, 9, 10].Merge = true;

                worksheet.Cells[7, 13, 7, 14].Merge = true;
                worksheet.Cells[8, 13, 8, 14].Merge = true;
                worksheet.Cells[9, 13, 9, 14].Merge = true;


                for (int row = 7; row <= 9; row++)
                {
                    for (int col = 8; col <= 14; col++)
                    {
                        worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        if (row != 8)
                        {
                            worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }
                    }
                }

                //worksheet.Cells[7, 7, 9, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                //worksheet.Cells[7, 7, 7, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                //worksheet.Cells[7, 7, 7, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray); //  =  (ExcelBorderStyle.Thin);
                //worksheet.Cells[9, 7, 9, 12].Style.Border.Right.Style = (ExcelBorderStyle.Thin);
                //worksheet.Cells[9, 7, 9, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);



                //---------- row 11 [SECTION III   Employee Identification and Calcultaion]
                worksheet.Cells[11, 1].Value = "T";
                worksheet.Cells[11, 2].Value = "SECTION III   Employee Identification and Calcultaion";
                worksheet.Cells[11, 2, 11, 5].Merge = true;
                worksheet.Cells[11, 2].Style.Font.Bold = true;
                worksheet.Cells[11, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                //worksheet.Cells[12, 2, 12, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[12, 1].Value = "T";
                worksheet.Cells[12, 2].Value = "Worker Identification";
                worksheet.Cells[12, 2, 12, 6].Merge = true;
                worksheet.Cells[12, 2, 12, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[12, 7].Value = "Employment Information";
                worksheet.Cells[12, 7, 12, 8].Merge = true;
                worksheet.Cells[12, 7, 12, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[12, 9].Value = "Base for Calculation";
                worksheet.Cells[12, 9, 12, 10].Merge = true;
                worksheet.Cells[12, 9, 12, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[12, 11].Value = "Calculated Pension Contribution Amounts";
                worksheet.Cells[12, 11, 12, 14].Merge = true;
                worksheet.Cells[12, 11, 12, 14].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                worksheet.Cells[13, 1].Value = "T";
                worksheet.Cells[14, 1].Value = "T";
                worksheet.Cells[15, 1].Value = "T";
                worksheet.Cells[13, 2].Value = "Type Identifier";
                worksheet.Cells[13, 2, 15, 2].Merge = true;
                worksheet.Cells[13, 3].Value = "Identifier";
                worksheet.Cells[13, 3, 15, 3].Merge = true;
                worksheet.Cells[13, 4].Value = "Name of worker";
                worksheet.Cells[13, 4, 15, 5].Merge = true;
                //worksheet.Cells[13, 4, 13, 5].Merge = true;
                worksheet.Cells[14, 6].Value = "Date of Birth";
                worksheet.Cells[15, 6].Value = "dd.mm.yyyy";
                worksheet.Cells[14, 7].Value = "Commencing date";
                worksheet.Cells[15, 7].Value = "dd.mm.yyyy";
                worksheet.Cells[14, 8].Value = "Termination date";
                worksheet.Cells[15, 8].Value = "dd.mm.yyyy";
                worksheet.Cells[13, 9].Value = "Reduced Basic Salary codes and Contribution Exemption Code";
                worksheet.Cells[13, 9].Style.WrapText = true;

                worksheet.Cells[13, 9, 15, 9].Merge = true;
                worksheet.Cells[13, 10].Value = "Pensionable Wage";
                worksheet.Cells[13, 10, 15, 10].Merge = true;

                worksheet.Cells[13, 11].Value = "Mandatory Contributions";
                worksheet.Cells[13, 11, 13, 12].Merge = true;
                worksheet.Cells[14, 11].Value = "Employee";
                worksheet.Cells[14, 11, 15, 11].Merge = true;
                worksheet.Cells[14, 12].Value = "Employer";
                worksheet.Cells[14, 12, 15, 12].Merge = true;


                worksheet.Cells[13, 13].Value = "Voluntary contributions";
                worksheet.Cells[13, 13, 13, 14].Merge = true;
                worksheet.Cells[14, 13].Value = "Employee";
                worksheet.Cells[14, 13, 15, 13].Merge = true;
                worksheet.Cells[14, 14].Value = "Employer";
                worksheet.Cells[14, 14, 15, 14].Merge = true;



                for (int row = 12; row <= 15; row++)
                {
                    for (int col = 2; col <= 14; col++)
                    {
                        worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        if (row != 8)
                        {
                            worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }
                    }
                }

                // remove borders from dob,comdate,term
                for (int col = 6; col <= 8; col++)
                {
                    worksheet.Cells[13, col].Style.Border.BorderAround(ExcelBorderStyle.None);
                    worksheet.Cells[14, col].Style.Border.BorderAround(ExcelBorderStyle.None);
                    worksheet.Cells[15, col].Style.Border.BorderAround(ExcelBorderStyle.None);

                    worksheet.Cells[13, col, 15, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }


                worksheet.Cells.Where(x => x.Merge).ToList()
                    .ForEach(t => t.Style.Border.BorderAround(ExcelBorderStyle.Thin));

                worksheet.Cells[11, 2, 11, 5].Style.Border.BorderAround(ExcelBorderStyle.None);
                worksheet.Cells[6, 8].Style.Border.BorderAround(ExcelBorderStyle.None);
                worksheet.Cells[6, 2, 6, 4].Style.Border.BorderAround(ExcelBorderStyle.None);
                //worksheet.Cells.Where(x => x.Merge && x.Rows == 6 && x.Rows == 11).ToList()
                //    .ForEach(t => t.Style.Border.BorderAround(ExcelBorderStyle.None));


                // ***** add 3.1,3.2,....3.12 rows
                for (int cell = 1; cell <= 14; cell++)
                {
                    if (cell == 1)
                    {
                        worksheet.Cells[16, cell].Value = "T";
                        continue;
                    }
                    if (cell == 5)
                    {
                        worksheet.Cells[16, cell - 1, 16, cell].Merge = true;
                        worksheet.Cells[16, cell].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        continue;
                    }
                    int decPoint = cell >= 9 ? 2 : 1;
                    worksheet.Cells[16, cell].Value = Math.Round(decimal.Parse("3." + (cell >= 5 ? cell - 2 : cell - 1)), decPoint);
                    worksheet.Cells[16, cell].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[16, cell].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[16, cell].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }


                int rowIndx = 17;
                foreach (var emp in payrolEmpls)
                {
                    for (int cell = 1; cell <= 14; cell++)
                    {
                        if (cell == 1)
                        {
                            worksheet.Cells[rowIndx, cell].Value = 3;
                            continue;
                        }
                        if (cell == 2)
                            worksheet.Cells[rowIndx, cell].Value = 1;
                        if (cell == 3)
                            worksheet.Cells[rowIndx, cell].Value = emp.Employee?.IdentityNumber;
                        if (cell == 4)
                            worksheet.Cells[rowIndx, cell].Value = emp.Employee.GetSystemName(userResolverService
                .GetClaimsPrincipal());
                        if (cell == 5)
                        {
                            worksheet.Cells[rowIndx, cell - 1, rowIndx, cell].Merge = true;
                            worksheet.Cells[rowIndx, cell].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            continue;
                        }
                        if (cell == 6)
                            worksheet.Cells[rowIndx, cell].Value = emp.Employee.DateOfJoined?.ToString("dd.MM.yyyy");
                        if (cell == 7)
                            worksheet.Cells[rowIndx, cell].Value = emp.Employee.DateOfJoined?.ToString("dd.MM.yyyy");
                        if (cell == 10)
                            worksheet.Cells[rowIndx, cell].Value = emp.Employee.EmployeePayComponents.FirstOrDefault(x => x.PayAdjustmentId == payAdju_BasicSalaryId)?.Total ?? 0;
                        if (cell == 11)
                            worksheet.Cells[rowIndx, cell].Value = emp.VariationKeyValues.FirstOrDefault(x => x.Key == "Pension Charge")?.Value ?? 0;
                        if (cell == 12)
                            worksheet.Cells[rowIndx, cell].Value = emp.VariationKeyValues.FirstOrDefault(x => x.Key == "Pension Charge")?.Value ?? 0;

                        worksheet.Cells[rowIndx, cell].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        //worksheet.Cells[rowIndx, cell].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //worksheet.Cells[rowIndx, cell].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    worksheet.Cells[rowIndx, 10, rowIndx, 12].Style.Numberformat.Format = "#,##0.00";
                    rowIndx++;
                }
                
                worksheet.Cells[rowIndx, 2].Value = payrolEmpls.Count();
                worksheet.Cells[rowIndx, 11].Value = payrolEmpls.Sum(z => z.VariationKeyValues.Where(x => x.Key == "Pension Charge").Sum(x => x.Value));
                worksheet.Cells[rowIndx, 12].Value = payrolEmpls.Sum(z => z.VariationKeyValues.Where(x => x.Key == "Pension Charge").Sum(x => x.Value));
                worksheet.Cells[rowIndx, 11, rowIndx, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[rowIndx, 11, rowIndx, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[rowIndx, 11, rowIndx, 12].Style.Numberformat.Format = "#,##0.00";


                fileContents = package.GetAsByteArray();

            }

            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: $"MPS101-{payrol.Name.GenerateSlug()}.xlsx"
            );
        }

        #endregion


        private PayItemVm GetNewList(int payrolId, int addId, VariationType? type = null)
        {
            int page = 1;
            int limit = 10;
            var adjustments = context.PayrollPeriodPayAdjustments
                .Where(x => x.PayAdjustmentId == addId && x.PayrollPeriodId == payrolId
                && (type == null || type == x.VariationType))
                .Skip((page - 1) * limit)
                .Take(limit)
                .OrderBy(x => x.Employee.Department.DisplayOrder)
                .ThenBy(x=> x.Employee.EmpID)
                .Include(x => x.PayrollPeriod)
                .Include(x => x.PayAdjustment)
                .Include(x => x.PayrollPeriodPayAdjustmentFieldValues)
                .ToList();

            adjustments.ForEach(t => t.PayrollPeriodPayAdjustmentFieldValues = t.PayrollPeriodPayAdjustmentFieldValues.OrderBy(x => x.CalculationOrder).ToList());

            //ViewBag.ItemId = new SelectList(context.PayAdjustments.ToList(), "Id", "Name", addId);
            //ViewBag.PayrolPeriodId = new SelectList(context.PayrollPeriods.ToList(), "Id", "Name", payrolId);
            //ViewBag.Employees = context.Employees.ToList();
            return new PayItemVm
            {
                ItemId = addId,
                PayrolPeriodId = payrolId,
                PayrolPayAdjustments = adjustments
            };
        }

        //private PayItemVm GetNewListDeduction(int payrolId, int addId)
        //{
        //    var additions = context.PayrollPeriodDeductions
        //        .Where(x => x.DeductionId == addId && x.PayrollPeriodId == payrolId)
        //        .OrderBy(x => x.EmployeeName)
        //        .Include(x => x.PayrollPeriod)
        //        .Include(x => x.OriginalDeduction)
        //        .ToList();

        //    ViewBag.ItemId = new SelectList(context.Deductions.ToList(), "Id", "Name", addId);
        //    ViewBag.PayrolPeriodId = new SelectList(context.PayrollPeriods.ToList(), "Id", "Name", payrolId);
        //    ViewBag.Employees = context.Employees.ToList();
        //    return new PayItemVm
        //    {
        //        ItemId = addId,
        //        PayrolPeriodId = payrolId,
        //        PayrollPeriodDeductions = additions,
        //        VariationType = VariationType.Deduction
        //    };
        //}


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }

}
