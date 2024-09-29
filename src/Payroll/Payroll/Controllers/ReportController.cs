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
using System.Dynamic;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Payroll.Filters;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Payroll.Controllers
{
    [Authorize]
    public class ReportController : BaseController
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
        private readonly AccountDbContext accountDbContext;
        private readonly AuditLogService auditLogService;

        public ReportController(PayrollDbContext context, PayrollService payrollService, PayAdjustmentService payAdjustmentService, IHostingEnvironment hostingEnvironment, AccessGrantService accessGrantService, UserResolverService userResolverService, CompanyService companyService, EmployeeService employeeService, ScheduleService scheduleService, Hangfire.IBackgroundJobClient backgroundJobClient, AccountDbContext accountDbContext, AuditLogService auditLogService)
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
            this.accountDbContext = accountDbContext;
            this.auditLogService = auditLogService;
        }

        public async Task<IActionResult> Index()
        {
            var reports = await accountDbContext.Reports.ToListAsync();
            return View(reports);
        }


        public async Task<IActionResult> GenderProfile()
        {
            var deptGender = await context.Employees.Where(a => a.CompanyId == userResolverService.GetCompanyId())
                   .GroupBy(a => new { a.Gender, a.Department.Name })
                   .Select(a => new
                   {
                       Gender = a.Key.Gender,
                       Department = a.Key.Name,
                       Count = a.Count()
                   }).ToListAsync();


            var genders = deptGender
                .GroupBy(a => a.Department).Select(a => Tuple.Create
                (
                    a.Key,
                    a.Where(x => x.Gender == Gender.Male).Sum(x=> x.Count),
                    a.Where(x => x.Gender == Gender.Female).Sum(x => x.Count),
                    a.Where(x => x.Gender == Gender.Other).Sum(x => x.Count)
                )).ToList();



            var pie = deptGender
                .GroupBy(a => a.Gender)
                .ToDictionary(a => a.Key.ToString(), a => a.Sum(x=> x.Count));

            ViewBag.Pie = pie;
            ViewBag.ByDept = genders;
            return View();
        }

        public async Task<IActionResult> SetOptions(int id)
        {
            var rpt = await accountDbContext.Reports.FindAsync(id);
            ViewBag.Id = id;
            //ViewBag.Employees = await employeeService.GetAllEmployeesInMyCompanyForDropdownOptGroups();
            ViewBag.EmployeeFields = new MultiSelectList(accessGrantService.GetSelectFieldDisplayNameEmployee(), "Key", "Value");

            if (rpt.Id == ReportTypes.DailyAttendnace)
            {
                ViewBag.Href = "DailyAttendance";
                ViewBag.Titile = "Daily attendance";
                //ViewBag.Employees = await employeeService.GetAllEmployeesInMyCompany();
            }
            else if (rpt.Id == ReportTypes.MonthlyAttendnace)
            {
                ViewBag.Href = "MonthlyAttendance";
                ViewBag.Titile = "Monthly attendance";
            }
            else if (rpt.Id == ReportTypes.OTClaimed)
            {
                ViewBag.Href = "OvertimeClaimed";
                ViewBag.Titile = "Overtime Claimed";
            }
            else if (rpt.Id == ReportTypes.Pension)
            {
                ViewBag.Href = "PensionContribution";
                ViewBag.Titile = "Pension Contribution";
            }

            ViewBag.Report = rpt;

                return View(rpt);
        }


        public async Task<IActionResult> DailyAttendance(List<int> empIds =null, DateTime? dt = null)
        {
            var _date = dt.Value;
            var ddd = await context.Attendances.Where(t => t.CompanyId == userResolverService.GetCompanyId()
                    && empIds.Contains(t.EmployeeId) 
                    && !t.IsOvertime
                    && t.Date == _date)
                .Include(t => t.Employee)
                //.GroupBy(t => new { t.Employee.EmpID, Name = t.Employee.GetSystemName(User) })
                .Select(t=> new
                {
                    t.Employee.EmpID,
                    t.Id,
                    t.EmployeeId,
                    t.TotalWorkedHours,
                    Name =  t.Employee.GetSystemName(User),
                })
                .ToListAsync();

            var bio = await context.BiometricRecords.Where(t => t.CompanyId == userResolverService.GetCompanyId()
        && empIds.Contains(t.EmployeeId)
        && t.Attendance.Date == _date)
    .ToListAsync();
            var fields = new[] { "EmpID", "Employee Name", BiometricRecordState.CheckIn.GetDisplayName(),
            BiometricRecordState.BreakOut.GetDisplayName(),
            BiometricRecordState.BreakIn.GetDisplayName(),
            BiometricRecordState.CheckOut.GetDisplayName(),
            "Total Hours"
            };

            var fieldValueList = ddd
                .Select(t => Tuple.Create
                (
                    t.EmpID,
                    t.Name,
                    bio.FirstOrDefault(q => q.BiometricRecordState == BiometricRecordState.CheckIn && q.EmployeeId == t.EmployeeId)?
                    .DateTime.ToLocalFormat(true) ?? "-",

                    bio.FirstOrDefault(q => q.BiometricRecordState == BiometricRecordState.BreakOut && q.EmployeeId == t.EmployeeId)?
                    .DateTime.ToLocalFormat(true) ?? "-",


                    bio.FirstOrDefault(q => q.BiometricRecordState == BiometricRecordState.BreakIn && q.EmployeeId == t.EmployeeId)?
                    .DateTime.ToLocalFormat(true) ?? "-",


                    bio.FirstOrDefault(q => q.BiometricRecordState == BiometricRecordState.BreakOut && q.EmployeeId == t.EmployeeId)?
                    .DateTime.ToLocalFormat(true) ?? "-",

                    t.TotalWorkedHours.GetHourMinString() ?? "-"

                //a.Where(x => x.Gender == Gender.Male).Sum(x => x.Count),
                //a.Where(x => x.Gender == Gender.Female).Sum(x => x.Count),
                //a.Where(x => x.Gender == Gender.Other).Sum(x => x.Count)
                )).ToList();


            // List[Dictionary[Field,Value]
            var array = new List<Dictionary<String, String>>();
            foreach (var item in fieldValueList)
            {
                var _ = fields.ToDictionary(t => t);

                var result = item
                  .GetType()
                  .GetProperties()
                  .Where(prop => prop.CanRead)
                  .Where(prop => !prop.GetIndexParameters().Any())
                  .Where(prop => Regex.IsMatch(prop.Name, "^Item[0-9]+$"))
                  .ToDictionary(
                      prop => prop.Name,
                      prop => prop.GetValue(item)?.ToString() ?? ""
                  );

                array.Add(result);
            }

            ViewBag.Fields = fields;

            var rpt = await accountDbContext.Reports.FindAsync(ReportTypes.DailyAttendnace);
            ViewBag.Report = rpt;


            if (Request.IsAjaxRequest())
                return PartialView("_GeneratedReport", array);
            return View("GeneratedReport", array);
        }

        public async Task<IActionResult> MonthlyAttendance(List<int> empIds = null, DateTime? dt = null)
        {

            var _date = dt.Value;
            var ddd = await context.Attendances.Where(t => t.CompanyId == userResolverService.GetCompanyId()
                    && empIds.Contains(t.EmployeeId)
                    && !t.IsOvertime
                    && t.Date.Month == _date.Month && t.Date.Year == _date.Year)
                .Include(t => t.Employee)
                //.GroupBy(t => new { t.Employee.EmpID, Name = t.Employee.GetSystemName(User) })
                .Select(t => new
                {
                    t.Employee.EmpID,
                    t.Id,
                    t.EmployeeId,
                    t.TotalWorkedHours,
                    t.Date,
                    Name = t.Employee.GetSystemName(User),
                })
                .ToListAsync();

            var bio = await context.BiometricRecords.Where(t => t.CompanyId == userResolverService.GetCompanyId()
        && empIds.Contains(t.EmployeeId)
        && t.Attendance.Date.Month == _date.Month && t.Attendance.Date.Year == _date.Year)
    .ToListAsync();

            var daysInMonth = DateTime.DaysInMonth(_date.Year, _date.Month);
            var fields = new List<string> { "EmpID", "Employee Name" };

            var daysss = Enumerable.Range(1, daysInMonth).Select(t => t.ToString()).ToList();
            fields.AddRange(daysss);
            fields.Add("Total Hours");



            // List[Dictionary[Field,Value]
            var array = new List<Dictionary<String, String>>();

            var uniqueEmps = ddd.Select(t => new { t.EmployeeId, t.EmpID, t.Name }).Distinct().ToList();
            foreach (var emp in uniqueEmps)
            {
                var dict = fields.ToDictionary(t=> t);
                dict["EmpID"] = emp.EmpID;
                dict["Employee Name"] = emp.Name;

                foreach (var day in daysss)
                    dict[day] = ddd.FirstOrDefault(t=> t.EmployeeId == emp.EmployeeId && t.Date.Date.Day == int.Parse(day) && t.Date.Year == _date.Year)?
                        .TotalWorkedHours.GetHourMinString() ?? "-";

                dict["Total Hours"] = ddd.Where(t => t.EmployeeId == emp.EmployeeId)
                    .Select(t=> t.TotalWorkedHours)?.Sum().GetHourMinString() ?? "-";
                array.Add(dict);
            }



            var rpt = await accountDbContext.Reports.FindAsync(ReportTypes.MonthlyAttendnace);
            ViewBag.Report = rpt;
            ViewBag.Fields = fields.ToArray();
            if (Request.IsAjaxRequest())
                return PartialView("_GeneratedReport", array);
            return View("GeneratedReport", array);
        }

        public async Task<IActionResult> OvertimeClaimed(List<int> empIds = null, DateTime? st = null, DateTime? en = null)
        {
            var _start = st.Value;
            var _end = en.Value;
            var ddd = await context.Attendances.Where(t => t.CompanyId == userResolverService.GetCompanyId()
                    && empIds.Contains(t.EmployeeId)
                    && t.IsOvertime
                    && t.Date >= _start && t.Date <= _end)
                .GroupBy(t => new { t.EmployeeId })
                .Select(t => new
                {
                    t.Key.EmployeeId,
                    Total = t.Sum(z=> z.TotalHoursWorkedPerSchedule),
                    Avg = t.Average(z=> z.TotalHoursWorkedPerSchedule),
                })
                .ToListAsync();

            var fields = new[] { "EmpID", "Employee Name", "Total OT Hours", "Average Daily OT Hours" };

            var emp = await employeeService.GetAllEmployeesInMyCompany(empIds);
            var fieldValueList = emp
                .Select(t => Tuple.Create
                (
                    t.EmpID,
                    t.GetSystemName(User),
                    ddd.Where(z=> z.EmployeeId == t.Id)?.Sum(z=> z.Total).GetHourMinString() ?? "-",
                    ddd.Where(z=> z.EmployeeId == t.Id)?.Sum(z=> z.Avg).GetHourMinString() ?? "-"
                )).ToList();


            // List[Dictionary[Field,Value]
            var array = new List<Dictionary<String, String>>();
            foreach (var item in fieldValueList)
            {
                var _ = fields.ToDictionary(t => t);

                var result = item
                  .GetType()
                  .GetProperties()
                  .Where(prop => prop.CanRead)
                  .Where(prop => !prop.GetIndexParameters().Any())
                  .Where(prop => Regex.IsMatch(prop.Name, "^Item[0-9]+$"))
                  .ToDictionary(
                      prop => prop.Name,
                      prop => prop.GetValue(item)?.ToString() ?? ""
                  );

                array.Add(result);
            }

            ViewBag.Fields = fields;

            var rpt = await accountDbContext.Reports.FindAsync(ReportTypes.OTClaimed);
            ViewBag.Report = rpt;


            if (Request.IsAjaxRequest())
                return PartialView("_GeneratedReport", array);
            return View("GeneratedReport", array);
        }

        public async Task<IActionResult> PensionContribution(List<int> empIds = null, DateTime? st = null, DateTime? en = null)
        {
            var _start = st.Value;
            var _end = en.Value;
            var pensionPayAdjustment = await context.PayAdjustments.FirstOrDefaultAsync(t => t.Name.ToLower().StartsWith("pension") && t.CompanyId == userResolverService.GetCompanyId());
            var ddd = await context.PayrollPeriodPayAdjustments.Where(t => t.PayrollPeriod.CompanyId == userResolverService.GetCompanyId()
                    && empIds.Contains(t.EmployeeId)
                    && t.PayAdjustmentId == pensionPayAdjustment.Id
                    && t.PayrollPeriod.StartDate >= _start && t.PayrollPeriod.EndDate <= _end)
                .GroupBy(t => new { t.EmployeeId, t.PayrollPeriod.Name, t.PayrollPeriodId })
                .Select(t => new
                {
                    t.Key,
                    Total = t.Sum(z => z.Total),
                })
                .ToListAsync();


            var fields = new List<string> { "EmpID", "Employee Name"  };
            var discPp = ddd.Select(t => new { t.Key.PayrollPeriodId, t.Key.Name }).Distinct().ToList();
            fields.AddRange(discPp.Select(t=> t.Name).ToList());
            fields.Add("Total Contribution");

            // List[Dictionary[Field,Value]
            var array = new List<Dictionary<String, String>>();

            var uniqueEmps = await employeeService.GetAllEmployeesInMyCompany(empIds);
            foreach (var emp in uniqueEmps)
            {
                var dict = fields.ToDictionary(t => t);
                dict["EmpID"] = emp.EmpID;
                dict["Employee Name"] = emp.GetSystemName(User);

                foreach (var pp in discPp)
                    dict[pp.Name] = ddd.FirstOrDefault(t => t.Key.EmployeeId == emp.Id && t.Key.PayrollPeriodId == pp.PayrollPeriodId)?
                        .Total.ToSystemFormat(User) ?? "-";

                dict["Total Contribution"] = ddd.Where(t => t.Key.EmployeeId == emp.Id)
                    .Select(t => t.Total)?.Sum().ToSystemFormat(User) ?? "-";
                array.Add(dict);
            }



            var rpt = await accountDbContext.Reports.FindAsync(ReportTypes.Pension);
            ViewBag.Report = rpt;
            ViewBag.Fields = fields.ToArray();


            if (Request.IsAjaxRequest())
                return PartialView("_GeneratedReport", array);
            return View("GeneratedReport", array);
        }

        public async Task<IActionResult> AgeProfile()
        {
            var ageGroupsBrackets = new Dictionary<int, string>()
            {
                { 1, "Less than 20" } ,
                { 2,  "20-29" },
                { 3, "30-39" } ,
                { 4, "40-49" },
                { 5,  "50-59" },
                { 6,  "60-69" } ,
                { 7,  "70 Above" }
            };

            var deptGender = await context.Employees.Where(a => a.CompanyId == userResolverService.GetCompanyId() && a.DateOfBirth.HasValue)
                .GroupBy(a => new { Age = DateTime.Now.Year - a.DateOfBirth.Value.Year, Gender = a.Gender })
                .Select(t=> new { t.Key, Cnt = t.Count() })
                .ToListAsync();

            var data = ageGroupsBrackets.Select(x => Tuple.Create
            (
                x.Value,
                deptGender.Count(a => Convert.ToInt32(x.Key + "0") < a.Key.Age && a.Key.Age <= Convert.ToInt32(x.Key + "9") && a.Key.Gender == Gender.Male),
                deptGender.Count(a => Convert.ToInt32(x.Key + "0") < a.Key.Age && a.Key.Age <= Convert.ToInt32(x.Key + "9") && a.Key.Gender == Gender.Female)
            )).ToList();


            var agePie = ageGroupsBrackets.ToDictionary(x =>
                x.Value,
                x => deptGender.Where(a => Convert.ToInt32(x.Key + "0") < a.Key.Age && a.Key.Age <= Convert.ToInt32(x.Key + "9")).Sum(a=> a.Cnt));
            

            ViewBag.Pie = agePie;
            ViewBag.ByGender = data;
            return View();
        }


        public async Task<IActionResult> YearsOfService()
        {
            var ageGroupsBrackets = new List<Tuple<int, string, int, int>>()
            {
                Tuple.Create<int, string, int, int>(1, "Less than 1", 0, 1),
                Tuple.Create<int, string, int, int>(2, "2-3 years", 2, 3),
                Tuple.Create<int, string, int, int>(3, "3-4 years", 3, 4),
                Tuple.Create<int, string, int, int>(4, "4-5 years", 4, 5),
                Tuple.Create<int, string, int, int>(5, "5-10 years", 5, 10),
                Tuple.Create<int, string, int, int>(6, "10-20 years", 10, 20),
                Tuple.Create<int, string, int, int>(7, "20 plus years", 20, 60),
                //{ 2,  "2-3" },
                //{ 3, "3-4" } ,
                //{ 4, "4-5" },
                //{ 5,  "5-10" },
                //{ 6,  "10-20" } ,
                //{ 7,  "20-30" }
            };

            var yearServiceByUearDept = await context.Employees.Where(a => a.CompanyId == userResolverService.GetCompanyId() && a.DateOfJoined.HasValue)
                .GroupBy(a => new { Age = DateTime.Now.Year - a.DateOfJoined.Value.Year, a.DepartmentName })
                .Select(t => new { t.Key, Cnt = t.Count() })
                .ToListAsync();

            //var data = ageGroupsBrackets.Select(b => Tuple.Create
            //(
            //    b.Item2,
            //    yearServiceByUearDept.Count(a => b.Item4 < a.Key.Age && a.Key.Age <= b.Item3)
            //)).ToList();


            var agePie = ageGroupsBrackets.ToDictionary(x =>
                x.Item2,
                x => yearServiceByUearDept.Where(a => x.Item3 < a.Key.Age && a.Key.Age <= x.Item4).Sum(a => a.Cnt));


            var depts = yearServiceByUearDept
                .Select(a => a.Key.DepartmentName).Distinct().ToList();
            var genders = depts.Select(a => Tuple.Create
                (
                    a,
                    yearServiceByUearDept.Where(b => b.Key.DepartmentName == a && ageGroupsBrackets[0].Item3 < b.Key.Age && b.Key.Age <= ageGroupsBrackets[0].Item4).Sum(x => x.Cnt),
                    yearServiceByUearDept.Where(b => b.Key.DepartmentName == a && ageGroupsBrackets[1].Item3 < b.Key.Age && b.Key.Age <= ageGroupsBrackets[1].Item4).Sum(x => x.Cnt),
                    yearServiceByUearDept.Where(b => b.Key.DepartmentName == a && ageGroupsBrackets[2].Item3 < b.Key.Age && b.Key.Age <= ageGroupsBrackets[2].Item4).Sum(x => x.Cnt),
                    yearServiceByUearDept.Where(b => b.Key.DepartmentName == a && ageGroupsBrackets[3].Item3 < b.Key.Age && b.Key.Age <= ageGroupsBrackets[3].Item4).Sum(x => x.Cnt),
                    yearServiceByUearDept.Where(b => b.Key.DepartmentName == a && ageGroupsBrackets[4].Item3 < b.Key.Age && b.Key.Age <= ageGroupsBrackets[5].Item4).Sum(x => x.Cnt),
                    yearServiceByUearDept.Where(b => b.Key.DepartmentName == a && ageGroupsBrackets[5].Item3 < b.Key.Age && b.Key.Age <= ageGroupsBrackets[5].Item4).Sum(x => x.Cnt),
                    yearServiceByUearDept.Where(b => b.Key.DepartmentName == a && ageGroupsBrackets[6].Item3 < b.Key.Age && b.Key.Age <= ageGroupsBrackets[6].Item4).Sum(x => x.Cnt)
                )).ToList();

            ViewBag.Pie = agePie;
            ViewBag.ByDept = genders;
            ViewBag.Fields = ageGroupsBrackets.Select(t => t.Item2).ToArray();
            //ViewBag.ByGender = data;
            return View();
        }


        public async Task<IActionResult> DataReview(string keyId, string modal, DateTime? start = null, DateTime? end = null, int limit = 10)
        {
            ViewBag.Modals = new SelectList(await auditLogService.GetAuditableEntityDropdown(), modal);

            ViewBag.Count = 0;
            if (string.IsNullOrWhiteSpace(keyId))
                return View();


            var query = context.AuditLogs.Where(a => a.KeyId == keyId && a.ModelName == modal);
            if(start.HasValue && end.HasValue)
                query = context.AuditLogs.Where(a => a.AuditDateTimeUtc >= start && a.AuditDateTimeUtc <= end);

            var data = await auditLogService.GetAuditLogs(keyId, modal, start, end, limit);
            ViewBag.Count = data.Item1;

            ViewBag.CurrentRangeDisplay = start.GetDuration(end, userResolverService.GetClaimsPrincipal());
            ViewBag.Start = start;
            ViewBag.End = end;
            ViewBag.KeyId = keyId;
            return View(data.Item2);
        }


        public async Task<IActionResult> TimeOffTrend(int? year = null)
        {
            var _year = year ?? 2020;
            var query = context.DayOffEmployeeItems.Where(x => x.DayOffEmployee.Year == _year);
            var dayoffs = await query
                .GroupBy(a=> ((DateTime)context.Entry(a).Property<DateTime>(AuditFileds.CreatedDate).CurrentValue).Month)
                .ToDictionaryAsync(a=> a.Key, a=> a.Sum(z=> z.TotalDays));

            var months = Enumerable.Range(1, 12).ToDictionary(x => x, x=> CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(x).Substring(0, 3));
            var dayOffBar = months.ToDictionary(
                x => x.Value, x => dayoffs.ContainsKey(x.Key) ? dayoffs[x.Key] : 0);

            var totalEmplCounts = await companyService.GetEmployeeCount();
            var totalEmplessTakingLeavePercent = Math.Round((decimal)(await query.GroupBy(a => a.DayOffEmployee.EmployeeId).Select(x => x.Key).SumAsync() / totalEmplCounts) * 100, 2);

            var totalTimeOffs = await query.CountAsync();

            return View((dayOffBar, totalEmplessTakingLeavePercent, totalTimeOffs));
        }



        public async Task<IActionResult> ViewChangedColumns(int id)
        {
            var data = await context.AuditLogs.FindAsync(id);
            return View(data);
        }
    }
}
