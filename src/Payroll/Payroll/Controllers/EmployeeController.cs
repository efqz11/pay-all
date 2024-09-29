using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IO;
using Payroll.Database;
using Payroll.Filters;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Payroll.Controllers
{
    public class EmployeeController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly EmployeeService employeeService;
        private readonly AccessGrantService accessGrantService;
        private readonly UserResolverService userResolverService;
        private readonly FileUploadService fileUploadService;
        private readonly AccountDbContext accountDbContext;
        private readonly UserManager<AppUser> userManager;
        private readonly CompanyService companyService;
        private readonly PayrollService payrollService;
        private readonly ScheduleService scheduleService;
        private readonly NotificationService notificationService;
        private readonly RequestService requestService;

        public EmployeeController(PayrollDbContext context, IBackgroundJobClient backgroundJobClient, EmployeeService employeeService, AccessGrantService accessGrantService, UserResolverService userResolverService, FileUploadService fileUploadService, AccountDbContext accountDbContext, UserManager<AppUser> userManager, CompanyService companyService, PayrollService payrollService, ScheduleService scheduleService, NotificationService notificationService, RequestService requestService)
        {
            this.context = context;
            this.backgroundJobClient = backgroundJobClient;
            this.employeeService = employeeService;
            this.accessGrantService = accessGrantService;
            this.userResolverService = userResolverService;
            this.fileUploadService = fileUploadService;
            this.accountDbContext = accountDbContext;
            this.userManager = userManager;
            this.companyService = companyService;
            this.payrollService = payrollService;
            this.scheduleService = scheduleService;
            this.notificationService = notificationService;
            this.requestService = requestService;
        }

        private int _empId = 0;
        public void _setEmployeeId(int id)
        {
            if (User.IsInRole(Roles.PayAll.admin))
                _empId = id;
            else
                _empId = userResolverService.GetEmployeeId();
        }

        [ActionPermissions(Permissions.LIST)]
        public async Task<IActionResult> Index(int dept = 0, int page = 1, int limit = 10)
        {
            ViewBag.EmpIdFilter = dept;
            var comapnyId = userResolverService.GetCompanyId();
            var emp = await context.Employees
             .Where(x => x.CompanyId == comapnyId &&
             (dept == 0 || dept == x.DepartmentId))
             .OrderBy(x => x.Department.DisplayOrder)
             .ThenBy(x=> x.EmpID)
             .Skip((page - 1) * limit)
             .Take(limit)
             .Include(x => x.Department)
             //.Include(x => x.Individual)
             .ToListAsync();
            ViewBag.DeptName = context.Departments.Find(dept)?.Name?.ToUpper();
            ViewBag.Count = await context.Employees
             .Where(x => x.Department.CompanyId == comapnyId && (dept == 0 || dept == x.DepartmentId)).CountAsync();
            ViewBag.DeptRouteId = dept;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Listing", emp);

            var departmentStats = 
            ViewData["Departments"] = await context.Departments.Where(x=> x.CompanyId == comapnyId).OrderBy(x=> x.DisplayOrder)
                .Include(x=> x.Employees)
                .Include(x=> x.DepartmentHeads)
                // .AsQueryable()
                // .GroupBy(a=> new { a.Name, a.Id })
                .Select(a=> new DepartmentReportVm
                {
                    Name = a.Name,
                    Id = a.Id,
                    EmployeeCount = a.Employees.Count(),
                    ManagerEmployeesCount = a.DepartmentHeads.Count(),
                    Managers = a.DepartmentHeads.ToArray()
                }).ToListAsync();

            return View(emp);
        }

        [ActionPermissions(Permissions.LIST)]
        public async Task<IActionResult> Structure(int dept = 0, int page = 1, int limit = 10)
        {
            ViewBag.EmpIdFilter = dept;
            var comapnyId = userResolverService.GetCompanyId();

            ViewBag.DepartmentIds = new SelectList(await context.Departments.Where(x => x.CompanyId == comapnyId).OrderBy(x => x.DisplayOrder).ToListAsync(), "Id", "Name", dept);
            ViewBag.Count = await context.Employees
             .CountAsync(x => x.HrStatus == HrStatus.Employed && x.CompanyId == comapnyId);

            return View();
        }

        [ActionPermissions(Permissions.LIST)]
        public async Task<JsonResult> GetOrganChart(int? dept = null)
        {
            var comapnyId = userResolverService.GetCompanyId();


            //var datax =  context.EmployeeJobInfos
            //    .Include(x => x.Department)
            //    .Include(x => x.Employee)
            //    .ThenInclude(a => a.EmployeeDirectReports)
            //        .ThenInclude(a => a.Employee)
            //            .ThenInclude(a => a.EmployeeDirectReports)
            //    .AsEnumerable()
            //    .Where(x => x.Employee.CompanyId == comapnyId && x.ReportingEmployeeId == null)
            //    .ToList();
            //return Json(datax);


            var alChartEmpls = await context.Employees
                 .Where(x => x.Department.CompanyId == comapnyId) //  && (dept == null || x.DepartmentId == dept))
                 .Include(x => x.Department)
                 .Include(x => x.Location)
                 .Include(x => x.Division)
                // .Include(x => x.ReportingEmployee)
                .Select(a => new OrgStructure
                {
                    id = a.Id,
                    //empId = a.EmpID,
                    name = a.GetSystemName(User),
                    department = a.Department != null ? a.Department.Name : "",
                    departmentId = a.DepartmentId,
                    jobId = a.JobId,
                    reportingJobId = a.Job.ReportingJobId,
                    division = a.Division != null ? a.Division.Name : "",
                    location = a.Location != null ? a.Location.Name : "",
                    empstate = a.JobType.GetDisplayName(),
                    title = a.JobTitle,
                    avatar = Url.Content(a.Avatar ?? DefaultPictures.default_user),
                    ReportingEmployeeId = a.ReportingEmployeeId,
                })
                 .ToListAsync();


            var data = alChartEmpls.Where(x=> x.ReportingEmployeeId == null && (dept == null || x.departmentId == dept))
             .ToList();

            if(data == null || data.Count() <= 0) // some cases where dept filter applied
            {    
                // get reporting employees unique list
                var repUniq = alChartEmpls.Select(t => t.ReportingEmployeeId).Distinct().ToList();

                // now check if who has no manager (ce0)

                data = alChartEmpls.Where(x => repUniq.Contains(x.id) && x.ReportingEmployeeId == null).ToList();
            }

            foreach (var item in data)
                item.children = await GetOrhChartChildren(item, dept, alChartEmpls);


            var parentJobs = data.Select(a => a.reportingJobId).Distinct().Select(a =>
                new OrgStructure
                {
                    id = 2,
                    title = "Vacant Post",
                    department = "",
                    name = "Vacant ",
                    children = data
                }).ToList();


            var cmpMd = await accountDbContext.CompanyAccounts.Where(a => a.Id == comapnyId).Select(a => a.ManagingDirector).FirstOrDefaultAsync();
            var _data = new OrgStructure
            {
                id = 1,
                title = "Managing Director",
                department = "",
                name = cmpMd,
                children = parentJobs
            };

            return Json(_data, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });



            async Task<List<OrgStructure>> GetOrhChartChildren(OrgStructure a, int? dept, List<OrgStructure> alChartEmpls)
            {
                if (!alChartEmpls.Any(z => z.ReportingEmployeeId == a.id && (!dept.HasValue || dept == z.departmentId))) return null;

                var data = alChartEmpls
                    // .Include(z=> z.Individual)
                    .Where(e => e.ReportingEmployeeId == a.id && (!dept.HasValue || dept == e.departmentId))
                    .Select(x => new OrgStructure
                    {
                        id = x.id,
                        name = x.name,
                        department = x.department,
                        departmentId = x.departmentId,
                        jobId = x.jobId,
                        location = x.location,
                        division = x.division,
                        empstate = x.empstate,
                        title = x.title,
                        ReportingEmployeeId = x.ReportingEmployeeId,
                        reportingJobId = x.reportingJobId,
                        avatar = x.avatar
                    }).ToList();


                foreach (var item in data)
                    item.children = await GetOrhChartChildren(item, dept, alChartEmpls);

                return data;
            }
        }


        [ActionPermissions(Permissions.LIST)]
        public async Task<IActionResult> Directory(int groupBy = 1, string query = "")
        {
            var comapnyId = userResolverService.GetCompanyId();
            query = query?.ToLower();
            var _emp = await context.Employees
             .Where(x => x.Department.CompanyId == comapnyId
             && (string.IsNullOrWhiteSpace(query) || x.GetSystemName(User).ToLower().Contains(query)))
             .OrderBy(x => x.Department.DisplayOrder)
             .ThenBy(x => x.EmpID)
             .Include(x => x.Location)
             //.Include(x => x.Individual)
             .Include(x => x.Department)
             .Include(x => x.ReportingEmployee)
             .Include(x => x.EmployeeDirectReports)
             //.Include(x => x.Employments)
             //   .ThenInclude(x => x.ReportingEmployee)
                .ToListAsync();
            

            var emp = _emp.GroupBy(g => groupBy == 1 ? g.GetSystemName(User).Length > 0 ? g.GetSystemName(User).Substring(0, 1) : "" : groupBy == 2 ? g.Department.Name : groupBy == 3 ? (g.Location != null ? g.Location.Name : "") : g.HrStatus.ToString())
                .OrderBy(a=>a.Key)
                .ToList();

            ViewBag.Count = _emp.Count;
            ViewBag.Query = query;

            ViewBag.GroupBy = new[] { "Name", "Department", "Location", "Status" }.Select((x, i) => new SelectListItem(x, (i+1).ToString(), (i+1) == groupBy)).ToList();
            return View(emp);
        }

        [HttpPost]
        [ActionPermissions(Permissions.SEARCH)]
        public IActionResult Search(string query)
        {
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
            return BadRequest();
            var comapnyId = userResolverService.GetCompanyId();

            query = query.ToLower();
            var emp = context.Employees
             .Where(x => x.CompanyId == comapnyId)
             .Include(x => x.Department)
             .Where(x=> x.FirstName.ToLower().Contains(query) || (x.MiddleName != null && x.MiddleName.ToLower().Contains(query)) || (x.LastName != null && x.LastName.ToLower().Contains(query)) ||
             // x.GetSystemName(userResolverService.GetClaimsPrincipal()).ToLower().Contains(query) || 
             x.EmpID.ToLower().Equals(query) || x.DepartmentName.ToLower().Contains(query))
             .OrderBy(x => x.Department.DisplayOrder)
             .ThenBy(x=> x.EmpID)
             .Take(11)
             //.Include(x => x.Individual)
             .ToList();
            ViewBag.HasMoreData = emp.Count > 10;

            return PartialView("_Listing", emp);
        }

        [ActionPermissions(Permissions.LIST)]
        public IActionResult Page(int dept = 0, int page = 1, int limit = 10)
        {
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
                return BadRequest();
            var comapnyId = userResolverService.GetCompanyId();
            var emp = context.Employees
             .Where(x => x.Department.CompanyId == comapnyId &&
             (dept == 0 || dept == x.DepartmentId))
             .OrderBy(x => x.EmpID)
             .Skip((page - 1) * limit)
             .Take(limit)
             .Include(x => x.Department)
             .ToList();

            return PartialView("_ListingRow", emp);
        }

        [ActionPermissions(Permissions.READ)]
        public async Task<IActionResult> Detail(int id)
        {
            var emp = await context.Employees
                .Where(x=> x.Id == id)
             .Include(x => x.Department)
             .Include(x => x.Location)
             .Include(x => x.Nationality)
             .Include(x => x.EmergencyContactRelationship)
             //.Include(x => x.Individual)
            //     .ThenInclude(x => x.EmergencyContactRelationship)
            //  .Include(x => x.Individual)
            //     .ThenInclude(x => x.IndividualAddresses)
            //     .Include(x => x.PayrollPeriodEmployees)
            //     .Include(x => x.Location)
            //     .Include(x => x.Job)
            //         .ThenInclude(a=> a.Department)
            //     .Include(x => x.Job)
            //         .ThenInclude(a => a.Location)
            //     .Include(x => x.Job)
            //         .ThenInclude(a => a.Division)
                 .Include(x => x.ReportingEmployee)
            //         .Include(a=> a.Location)
            //         .Include(a=> a.Division)
            //     .Include(x => x.Division)
            //     .Include(x => x.Individual)
            //         .ThenInclude(a=> a.Nationality) 
                .Include(x => x.Company)
                    //.ThenInclude(x => x.Company)
                .FirstOrDefaultAsync();
            //TempData["EmpId"] = new SelectList(context.Employees.OrderBy(x=> x.Department.DisplayOrder)
            //    .ThenBy(x=> x.EmpID).ToList(), "Id", "Name", id);

            //if (emp.PayrollPeriodEmployees != null)
            //    context.Entry(emp).Collection(e => e.PayrollPeriodEmployees)
            //        .Query().OfType<PayrollPeriodEmployee>()
            //        .Include(x => x.VariationKeyValues)
            //        .Include(e => e.Employee)
            //        .Include(e => e.PayrollPeriod)
            //        .Load();

            return View(emp);
        }


        [ActionPermissions(Permissions.READ)]
        public async Task<IActionResult> Welcome(int id)
        {
            var emp = await context.Employees
                .Where(x => x.Id == id)
                .Include(x => x.Department)
                    .ThenInclude(x => x.Company)
                .FirstOrDefaultAsync();

            // profile views


            return PartialView("_Details_General", emp);
        }


        [ActionPermissions(Permissions.UPDATE)]
        [MyProfileOrDirectReportAuthorize]
        public async Task<IActionResult> Change(int id)
        {
            var emp = await context.Employees
                .Where(x => x.Id == id)
                .Include(x => x.PayrollPeriodEmployees)
                .Include(x => x.Location)
                .Include(x => x.JobActionHistories)
                .Include(x => x.Job)
                //.Include(x => x.EmployeeTypes)
                .Include(x => x.Employments)
                //.Include(x => x.Individual)
                .Include(x => x.EmployeePayComponents)
                    .ThenInclude(x => x.PayAdjustment)
                .Include(x => x.Department)
                    .ThenInclude(x => x.Company)
                .FirstOrDefaultAsync();


            ViewBag.activeRequest = await requestService.GetOpenRequests(id, RequestType.Emp_DataChange);

            if (Request.IsAjaxRequest())
                return PartialView("_ChangeOverview", emp);

            return View(emp);
        }

        [ActionPermissions(Permissions.READ)]
        public async Task<IActionResult> EmploymentAndJob(int id, DateTime? start = null, DateTime? end = null)
        {
            var emp = await context.Employees
                .Where(x => x.Id == id)
                .Include(x => x.ReportingEmployee)
                    .ThenInclude(x => x.Job)
                .Include(x => x.ReportingEmployee) // .ThenInclude(x => x.Individual)
                .Include(x => x.Job)
                    .ThenInclude(x => x.Location)
                .Include(x => x.Job)
                    .ThenInclude(x => x.Level)
                .Include(x => x.Job)
                    .ThenInclude(x => x.Department)
                .Include(x => x.EmployeeRoles)
                    .ThenInclude(x => x.EmployeeRole)

                //.Include(x => x.EmployeeTypes)
                //    .ThenInclude(x=> x.TerminationReason)
                .Include(x => x.Employments)
                    .ThenInclude(x => x.Job)
                .Include(x => x.JobActionHistories)
                //     .ThenInclude(x => x.JobActionHistoryChangeSets)
                // .Include(x => x.JobActionHistories)
                //     .ThenInclude(x => x.Job)
                // .Include(x => x.EmployeeActions)
                //     .ThenInclude(x => x.ReportingEmployee)
                .Include(x => x.EmployeeDirectReports)
                .FirstOrDefaultAsync();

            var _start = start ?? DateTime.UtcNow.AddMonths(-3).Date;
            var _end = end ?? DateTime.UtcNow;
            ViewBag.Start = _start;
            ViewBag.End = _end;
            ViewBag.DurationText = start.GetDuration(end, userResolverService.GetClaimsPrincipal());



            return PartialView("_Details_EmploymentAndJob", emp);
        }


        [ActionPermissions(Permissions.READ)]
        public async Task<JsonResult> GetOrganChartEmpl(int id) 
        {
            var emp = await context.Employees
                .Where(x => x.Id == id)
                .Include(x => x.Department)
                //.Include(x => x.Individual)
                .Include(x => x.Division)
                .Include(x => x.Location)
                .Include(x=> x.Job).ThenInclude(a=> a.ReportingJob)
                .Include(x => x.ReportingEmployee)
                    .ThenInclude(x => x.Job)
                .Include(x => x.ReportingEmployee)
                    .ThenInclude(x => x.Department)
                .Include(x => x.ReportingEmployee)
                    .ThenInclude(x => x.Location)
                //.Include(x => x.ReportingEmployee).ThenInclude(x => x.Individual)
                //.Include(x => x.EmployeeDirectReports)
                //    .ThenInclude(x => x.Individual)
                .FirstOrDefaultAsync();

            var data = new List<OrgStructure>() { 
            };


            var _data = new List<OrgStructure>();
            OrgStructure report = null;
            var empOrg = new OrgStructure
            {
                id = emp.Id,
                name = emp.GetSystemName(User),
                department = emp.Department != null ? emp.Department.Name : "",
                jobId = emp.JobId,
                division = emp.Division != null ? emp.Division.Name : "",
                location = emp.Location != null ? emp.Location.Name : "",
                empstate = emp.JobType?.GetDisplayName() ?? "NA",
                title = emp.JobTitle,
                avatar = Url.Content(emp.Avatar ?? DefaultPictures.default_user),
            };
            if (emp.EmployeeDirectReports.Any())
                empOrg.children =
                emp.EmployeeDirectReports.Select(a =>
                new OrgStructure
                {
                    id = a.Id,
                    name = a.GetSystemName(User),
                    department = a.Department != null ? a.Department.Name : "",
                    jobId = a.JobId,
                    division = a.Division != null ? a.Division.Name : "",
                    location = a.Location != null ? a.Location.Name : "",
                    empstate = a.JobType?.GetDisplayName() ?? "NA",
                    title = a.JobTitle,
                    avatar = Url.Content(a.Avatar ?? DefaultPictures.default_user),
                    current = true,
                })
             .ToList();

            if (emp.ReportingEmployee != null)
            {
                report = new OrgStructure
                {
                    id = emp.ReportingEmployee.Id,
                    name = emp.ReportingEmployee.GetSystemName(User),
                    department = emp.ReportingEmployee.Department != null ? emp.Department.Name : "",
                    jobId = emp.ReportingEmployee.JobId,
                    division = emp.ReportingEmployee.Division != null ? emp.ReportingEmployee.Division.Name : "",
                    location = emp.ReportingEmployee.Location != null ? emp.ReportingEmployee.Location.Name : "",
                    empstate = emp.ReportingEmployee.JobType.GetDisplayName(),
                    title = emp.ReportingEmployee.JobTitle,
                    avatar = Url.Content(emp.ReportingEmployee.Avatar ?? DefaultPictures.default_user),
                    children = new List<OrgStructure>() { empOrg }
                };

                _data.Add(report);
            }
            else // if(emp.Job.ReportingJobId.HasValue)
            {
                _data.Add(new OrgStructure
                {
                    id = 0,
                    name = "Vacant Post",
                    department = "",
                    jobId = 00,
                    division =  "",
                    location =  "",
                    title = "Vacant",
                    avatar = Url.Content(DefaultPictures.default_user),
                    children = new List<OrgStructure>() { empOrg }
                });
            }

            return Json(_data.First()); 
            //    new JsonSerializerSettings
            //{
            //    NullValueHandling = NullValueHandling.Ignore
            //});
        }

        [ActionPermissions(Permissions.READ)]
        public async Task<IActionResult> General(int id, DateTime? start = null, DateTime? end = null)
        {
            var emp = await context.Employees
                .Where(x => x.Id == id)
                .Include(x => x.Department)
                    .ThenInclude(x => x.Company)
                .Include(x => x.ReportingEmployee)
                    .ThenInclude(x => x.Job)
                .Include(x => x.EmergencyContactRelationship)
                .Include(x => x.ReportingEmployee)
                .Include(x => x.Nationality)
                    .Include(x => x.EmployeeAddresses)
                .Include(x => x.EmployeeDirectReports)
                //    .ThenInclude(x => x.Individual)
                .FirstOrDefaultAsync();

            var _start = start ?? DateTime.UtcNow.AddMonths(-3).Date;
            var _end = end ?? DateTime.UtcNow;
            ViewBag.Start = _start;
            ViewBag.End = _end;
            ViewBag.DurationText = start.GetDuration(end, userResolverService.GetClaimsPrincipal());

            return PartialView("_Details_General", emp);
        }

        [ActionPermissions(Permissions.READ, Permissions.LIST)]
        public async Task<IActionResult> ViewNotifications(DateTime? start = null, DateTime? end = null, int type= 0, bool showSeen = false, int page = 1, int limit = 10)
        {
            var _start = start.HasValue ? start.Value : scheduleService.thisWeekStart;
            var _end = end.HasValue ? end.Value : scheduleService.thisWeekEnd;
            var empId = userResolverService.GetEmployeeId();
            var vm = new HomeEmployeeVm
            {
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
                }
                //AttedanceSchedule = await scheduleService.GetCurrentSecdule(empId: userResolverService.GetEmployeeId()),
            };

            ViewBag.ShowDetails = true;
            ViewBag.Start = _start;
            ViewBag.End = _end;
            ViewBag.Employee = await employeeService.GetEmployeeById(empId);
            ViewBag.DurationText = scheduleService.GetDurationText(_start, _end, includeDays: false);
            ViewBag.NotificationTypes = new SelectList(await notificationService.GetNotificationTypes(), "Id", "Type", type);
            return PartialView("_Notifications", vm);
        }


        public async Task<IActionResult> ClearEmpCard1_1()
        {
            var employee = await context.Employees
                .Where(x => x.Department.CompanyId == userResolverService.GetCompanyId())
                .Select(x => x.Id)
                .ToArrayAsync();
            var relKpi = await context.KpiValues.Where(x => x.EmployeeId.HasValue && employee.Contains(x.EmployeeId.Value))
                .ToListAsync();
            context.KpiValues.RemoveRange(relKpi);

            var res = await context.SaveChangesAsync();
            return Ok(res+" Removeal successful");
        }
        public async Task<IActionResult> GenerateEmpCard1_1()
        {
            await employeeService.GenEmployeeCardsForAllEmployeeOfCompany(userResolverService.GetCompanyId());
            return Ok("Run successful");
        }

        public async Task<IActionResult> Performance(int id, DateTime? start = null, DateTime? end = null, PerformanceIndicator? ind = null, TimeFrame? tf = null)
        {
            var emp = await context.Employees
                .Where(x => x.Id == id)
                .Include(x => x.KpiValues)
                .Include(x => x.Department)
                    .ThenInclude(x => x.Company)
                .FirstOrDefaultAsync();

            ViewBag.Axes = new SelectList(typeof(EmployeeInteractionAgg).GetProperties()
                            .Select(x => x.Name).ToList());
            var vm = new HomeEmployeeVm
            {
                Employee = emp,
                FilterForm = new FilterForm
                {
                    EmployeeId = id,
                    DurationText = start.GetDuration(end, userResolverService.GetClaimsPrincipal()),
                    Indicator = ind,
                    TimeFrame = tf,
                }
            };
            return PartialView("_Performance", vm);
        }


        [HttpPost]
        public async Task<JsonResult> GetPerformanceChart(int id, DateTime? start = null, DateTime? end = null, TimeFrame? tf = null)
        {
            var _start = start ?? DateTime.UtcNow.AddMonths(-3).Date;
            var _end = end ?? DateTime.UtcNow;
            Dictionary<DateTime, double> data = null;


            var query = context.Attendances.Where(a => a.CompanyId == userResolverService.GetCompanyId());
            if (start.HasValue && end.HasValue)
                query = query.Where(a => a.Date >= _start && a.Date <= _end);
            IQueryable<IGrouping<DateTime, Attendance>> groupedQuery = null;
            switch (tf.Value)
            {
                case TimeFrame.Daily:
                    groupedQuery = query.GroupBy(x =>
                        new DateTime(x.Date.Year, x.Date.Month, x.Date.Day));
                    break;
                case TimeFrame.Weekly:
                    groupedQuery =  query.GroupBy(x => _start.AddDays(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(x.Date,
              CalendarWeekRule.FirstDay, DayOfWeek.Monday) * 7));
                    break;
                case TimeFrame.Monthly:
                    groupedQuery = query.GroupBy(x =>
                        new DateTime(x.Date.Year, x.Date.Month, 1));
                    break;
                case TimeFrame.Yearly:
                    groupedQuery = query.GroupBy(x =>
                        new DateTime(x.Date.Year, 1, 1));
                    break;
                default:
                    break;
            }


            var _data = await groupedQuery
                .OrderBy(a=> a.Key)
                .Select(a => new EmployeeInteractionAgg
            {
                EmployeeId = id,
                OnDate = a.Key,
                DateString = tf == TimeFrame.Yearly ? a.Key.ToString("yyyy") : tf == TimeFrame.Monthly ? a.Key.ToString("yyyy-MMM") : a.Key.ToString("yyyy-MM-dd"),
                LateHours = a.Any(x => x.CurrentStatus == AttendanceStatus.Late) ? Math.Round(a.Where(x => x.CurrentStatus == AttendanceStatus.Late).Sum(x=> x.TotalLateMins)/60f, 2) : 0,

                AbsentDays = a.Any(x => x.CurrentStatus == AttendanceStatus.Absent) ? a.Count(x => x.CurrentStatus == AttendanceStatus.Absent) : 0,

                WorkedHours = a.Any(x => x.HasClockRecords && x.IsOvertime == false && x.CurrentStatus != AttendanceStatus.Created) ? Math.Round(a.Where(x => x.HasClockRecords && x.IsOvertime == false && x.CurrentStatus != AttendanceStatus.Created).Sum(x => x.TotalHoursWorkedPerSchedule),2) : 0,

                OvertimeHours = a.Any(x => x.IsOvertime == true && x.CurrentStatus != AttendanceStatus.Created) ? Math.Round(a.Where(x => x.IsOvertime == true && x.CurrentStatus != AttendanceStatus.Created)
                .Sum(x => x.TotalHoursWorkedPerSchedule), 2) : 0,
                
            }).ToArrayAsync();

            return Json(_data);
        }



        private double GetTotal(IGrouping<object, Attendance> x, PerformanceIndicator value)
        {
            switch (value)
            {
                case PerformanceIndicator.LateMins:
                    return x.Sum(a => a.TotalLateMins);
                case PerformanceIndicator.AbsentDays:
                    return x.Count();
                case PerformanceIndicator.LeaveDays:
                    break;
                case PerformanceIndicator.WorkedHours:
                    break;
                case PerformanceIndicator.OvertimeHours:
                    break;
                case PerformanceIndicator.GradeChanges:
                    break;
                default:
                    break;
            }

            return 0;
        }

        public async Task<IActionResult> Payment(int id, int ppId = 0, DateTime? start = null, DateTime? end = null)
        {
            var emp = await context.Employees
                .Where(x => x.Id == id)
                .Include(x => x.KpiValues)
                .Include(x => x.Department)
                    .ThenInclude(x => x.Company)
                .FirstOrDefaultAsync();

            var _start = start ?? DateTime.UtcNow.AddMonths(-3).Date;
            var _end = end ?? DateTime.UtcNow;
            ViewBag.Start = _start;
            ViewBag.End = _end;
            ViewBag.DurationText = "Year " + DateTime.UtcNow.Year;

            var salaries = await payrollService.GetPayStubsByYear(id, DateTime.Now.Year);
            ViewBag.SalaryHistory = salaries;

            var lastSalary = await payrollService.GetLastPayStub(id);

            var salaryStats = new Dictionary<string, decimal>();

            salaryStats.Add("Net pay", lastSalary.NetSalary);
            salaryStats.Add("Deductions", lastSalary.GrossPay - lastSalary.NetSalary);
            //salaryStats.Add("Deductions", lastSalary.GrossPay - lastSalary.NetSalary);
            ViewBag.LastSalary = lastSalary;
            ViewBag.lastSalaryStats = salaryStats;

            //ViewBag.SalaryHistory = await context.PayrollPeriodEmployees.Where(a => a.EmployeeId == id) // _start >= a.PayrollPeriod.StartDate && _end <= a.PayrollPeriod.EndDate &&
            //    .Select(a => Tuple.Create(a.PayrollPeriod.Name, a.GrossPay, a.NetSalary))
            //    .ToListAsync();
            //ViewBag.PayrolPeriodEmpId = new SelectList(await
            //    context.PayrollPeriods.Where(x => x.CompanyId == userResolverService.GetCompanyId() &&
            //    x.PayrollPeriodEmployees.Any(a => a.IsActive && a.EmployeeId == id))
            //    .OrderBy(a => a.StartDate)
            //    .Select(a => new { a.Id, Name = a.StartDate.GetDuration(a.EndDate, userResolverService.GetClaimsPrincipal(), false) })
            //    .ToListAsync(), "Id", "Name", ppId);
            //if (ppId > 0)
            //{
            //    // single payp period
            //    ViewBag.PayrolPeriodEmployee = await
            //        context.PayrollPeriodEmployees.Where(x => x.PayrollPeriodId == ppId && x.EmployeeId == id)
            //        .Include(a => a.PayrollPeriod)
            //        .FirstOrDefaultAsync();
            //    ViewBag.EmployeeCard = employeeService.GetEmployeeCardFromPayPeriodEmployee(ViewBag.PayrolPeriodEmployee);
            //}
            //else
            //{
            //    // overall card
            //    ViewBag.EmployeeCard = await employeeService.GetEmployeeCardAsync(id,
            //        _start, _end);

            //    var payrolper = new PayrollPeriodEmployee
            //    {
            //        PayrollPeriod = new PayrollPeriod
            //        {
            //            Name = "Active Period",
            //            StartDate = _start,
            //            EndDate = _end
            //        },
            //        Employee = emp,
            //    };

            //    var dates = await companyService.GetPayrollDates(DateTime.UtcNow);
            //    ViewBag.PayrolPeriodEmployeeGen = await payrollService.GetEmployeeInteractionFacts(
            //        companyId: userResolverService.GetCompanyId(),
            //        startDate: dates.Item1,
            //        endDate: dates.Item2,
            //        empId: id);
            //}

            return PartialView("_Details_Payment", emp);
        }


        public async Task<IActionResult> GetPayStubs(int id, DateTime start, DateTime end)
        {
            var data = await payrollService.GetPayStubsByDuration(id, start, end);

            ViewBag.start = start;
            ViewBag.end = end;
            return PartialView("_Details_Payment_PayStubs", data);
        }

        //public async Task<IActionResult> GetPerfromanceAndSalaryChart(DateTime? start = null, DateTime? end = null)
        //{
        //    var _start = start ?? DateTime.UtcNow.AddMonths(-3).Date;
        //    var _end = end ?? DateTime.UtcNow;
        //    var _empId = userResolverService.GetEmployeeId();

        //    var employeeCard = await employeeService.GetEmployeeCardAsync(_empId, _start, _end);
        //    var _salaryChart = await context.PayrollPeriodEmployees.Where(x => x.EmployeeId == _empId && x.PayrollPeriod.StartDate >= _start && x.PayrollPeriod.EndDate <= _end)
        //        .GroupBy(x => new { x.PayrollPeriod.EndDate, x.PayrollPeriod.Name })
        //        .Select(x => new
        //        {
        //            Date = x.Key.EndDate,
        //            Name = x.Key.Name,
        //            GrossPay = x.Sum(p => p.GrossPay),
        //            NetSalary = x.Sum(p => p.NetSalary)
        //        }).ToListAsync();


        //    var salaryChart = new List<Tuple<string, decimal, decimal>>();
        //    var netSalaryChart = new Dictionary<string, decimal>();

        //    for (var sd = _start; sd < _end; sd = sd.AddMonths(1))
        //    {
        //        salaryChart.Add(new Tuple<string, decimal, decimal>(sd.ToString("MMMM yyyy"), _salaryChart.FirstOrDefault(x => x.Date.Month == sd.Month)?.GrossPay ?? 0, _salaryChart.FirstOrDefault(x => x.Date.Month == sd.Month)?.NetSalary ?? 0));
        //        //netSalaryChart.Add(sd.ToString("MMMM yyyy"), _salaryChart.FirstOrDefault(x => x.Date.Month == sd.Month)?.NetSalary ?? 0);
        //    }

        //    ViewBag.DurationText = start.GetDuration(end);

        //    return Json(new
        //    {
        //        Card = employeeCard,
        //        salaryChart,
        //        netSalaryChart
        //    });
        //}       

        public async Task<IActionResult> Compensation(int id)
        {
            var emp = await context.Employees
                .Where(x => x.Id == id)
                .Include(x => x.EmployeePayComponents)
                    .ThenInclude(x => x.PayAdjustment)
                .FirstOrDefaultAsync();


            if (emp == null)
                return BadRequest();

            return PartialView("_Details_Compensation", emp);
        }
        

        public async Task<IActionResult> TimeOffAndLeaves(int id, int year = 0, DateTime? start = null, DateTime? end = null)
        {
            _setEmployeeId(id);
            var _start = start ?? DateTime.UtcNow.AddMonths(-3).Date;
            var _end = end ?? DateTime.UtcNow;

            var years = await context.DayOffEmployees.Where(x => x.EmployeeId == _empId && x.IsActive).Select(a => a.Year).Distinct().OrderBy(a => a).ToArrayAsync();
            if (years == null)
                years = new int[] { DateTime.UtcNow.Year, DateTime.UtcNow.AddYears(1).Year };
            else if (years.Count() == 1)
            {
                years = new int[] { years.First(), years.First() + 1 };
            }

            if (year == 0)
                year = DateTime.UtcNow.Year;

            ViewBag.Years = new SelectList(years, year);
            ViewBag.DurationText = _start.GetDuration(_end, userResolverService.GetClaimsPrincipal());


            var Emplvm = new HomeEmployeeVm
            {
                FilterForm = new FilterForm(id, _start, _end),
                DayOffBalances = await employeeService.GetCurrentEmployeeDayOffBalances(_empId),
                DayOffUsages = await employeeService.GetCurrentEmployeeDayOffUsage(_empId),
                LeaveRequestByStatus = await employeeService.GetLeaveRequestByStatus(_empId),
            };

            return PartialView("_Details_TimeOffSickLeaves", Emplvm);
        }


        [ActionPermissions(Permissions.READ)]
        public async Task<IActionResult> Resources(int id)
        {
            var cmp = await employeeService.GetCompanyFileShares(id);

            var paysliips = await context.PayrollPeriodEmployees.Where(a => a.PayrollPeriod.StartDate.Year == DateTime.Now.Year && a.EmployeeId == id).Select(a => Tuple.Create(a.Id, a.PayrollPeriod.Name, a.PayrollPeriod.StartDate, a.PayrollPeriod.EndDate))
                .ToListAsync();

            ViewBag.files = await context.FileDatas.Where(a =>  a.Request.EmployeeId == id || a.EmployeeId == id)
                .Include(a=> a.Request)
                .ToListAsync();

            ViewBag.Payslips = paysliips;
            ViewBag.CompanyFileShares = cmp;
            ViewBag.Id = id;
            ViewBag.Labels = await companyService.GetLabels();
            return PartialView("_Details_Resources");
        }

        public async Task<IActionResult> AddFile(int id)
        {
            var exists = await context.Employees.FindAsync(id);
            ViewBag.Labels = await companyService.GetLabels();
            if (exists  == null) return ThrowJsonError("Employee not found");
            return PartialView("_AddFile", new FileData { EmployeeId = id, CompanyId = exists.CompanyId  });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFile(FileData model, [FromForm]IFormFile file)
        {
            if(ModelState.IsValid)
            {
                bool hasFiles = fileUploadService.HasFilesReadyForUpload(file);
                if (hasFiles)
                {
                    if (string.IsNullOrWhiteSpace(model.Name))
                        return ThrowJsonError($"Name is required!");

                    if (fileUploadService.GetFileSizeInMb(file) >= UploadSetting.MaxFileSizeMb)
                        return ThrowJsonError($"File is too huge, only file size up to {UploadSetting.MaxFileSizeMb}MB are allowed!");

                    if (!fileUploadService.IsAllowedFileType(file, UploadSetting.FileTypes))
                        return ThrowJsonError($"Uploaded file type is not allowed!");

                    var emp = await context.Employees.FindAsync(model.EmployeeId);
                    if(emp == null)
                        return ThrowJsonError($"Employee was not found!");


                    string fileUrl = await fileUploadService.UploadFles(file, "files");
                    //model.CompanyId = folder.CompanyId;
                    model.ContentLength = (int)file.Length;
                    model.ContentType = file.ContentType;
                    model.FileUrl = fileUrl;

                    model.FileSizeInMb = Math.Round(((int)file.Length / 1024f) / 1024f, 2);
                    model.FileExtension = Path.GetExtension(file.FileName);
                    model.FileName = Path.GetFileName(file.FileName);
                    model.IsSignatureAvailable = model.FileExtension.ToLower().Contains(".pdf");
                    model.Name = model.Name;

                    context.FileDatas.Add(model);
                    await context.SaveChangesAsync();

                    return RedirectToAction(nameof(Resources), new { id = model.EmployeeId });
                }

                return ThrowJsonError("Please upload an image");
            }

            return ThrowJsonError(ModelState);
        }





        [ActionPermissions(Permissions.READ)]
        public async Task<IActionResult> GetDayOffBalanceList(int id, int year = 0)
        {
            _setEmployeeId(id);
            return PartialView("_TimeOffSickLeaves_DayOffBalanceList", await employeeService.GetCurrentEmployeeDayOffs(_empId, year));
        }

        [ActionPermissions(Permissions.READ)]
        public async Task<IActionResult> GetLeaveRequestAndTimeOffs(int id, DateTime? start = null, DateTime? end = null)
        {
            _setEmployeeId(id);
            var _start = start ?? DateTime.UtcNow.AddMonths(-3).Date;
            var _end = end ?? DateTime.UtcNow;

            var requests = await requestService.GetLeaveRequests(userResolverService.GetCompanyId(), new int[] { id }, _start, _end, null, null, 1, int.MaxValue);

                //await employeeService.getl context.DayOffEmployeeItems.Where(x => x.DayOffEmployee.EmployeeId == _empId && x.Request != null && x.Request.ActionTakenDate .HasValue && x.Request.RequestType == RequestType.Leave && x.Request.Start >= _start && x.Request.End <= _end) //  && x.Request.Status == WorkItemStatus.Approved)
                //.Include(x => x.Request)
                //.Include(x => x.DayOffEmployee)
                //    .ThenInclude(x => x.DayOff)
                //.ToListAsync();

            return PartialView("_TimeOffSickLeaves_LeaveRequestAndTimeOffs", requests);
        }


        public async Task<IActionResult> GetScheduleForDates(DateTime start, DateTime end, int? id = null)
        {
            //var _date = start;
            var cmpConfig = await companyService.GetCompanyCalendarSettings();
            ViewBag.cmpConfig = cmpConfig;

            //var start = new DateTime(_date.Year, _date.Month, 1);
            start = start.Date;
            ViewBag.DurationText = scheduleService.GetDurationText(start, end, includeDays: false);


            ViewBag.calendars = Calendars.List;
            ViewBag.WeekStart = start;
            ViewBag.WeekEnd = end;

            var _empId = id ?? userResolverService.GetEmployeeId();

            var vm = new HomeEmployeeVm
            {
                AttedanceSchedule = await scheduleService.GetCurrentSecdule(empId: _empId, start: start, end: end),
                PublicHolidaysUpcoming = await companyService.GetUpComingPublicHolidaysForYear(start.Year, start.Month),
                Employee = await employeeService.GetCurrentEmployeeWithDepartment(_empId),
                WeekScheduleTasks = await scheduleService.GetSecduledTasksForThisWeek(empId: _empId),
                WorkTimes = await companyService.GetWorkTimes(),
                MyRequests = await requestService.GetLeaveRequests(userResolverService.GetCompanyId(), new int[] { _empId }, start, end, null, new WorkItemStatus[] { WorkItemStatus.Approved, WorkItemStatus.Draft, WorkItemStatus.Submitted }, 1, int.MaxValue),
                //defa = await companyService.GetWorkStartDayOfWeek
            };


            return PartialView("_Schedule", vm);
        }

        [ActionPermissions(Permissions.READ)]
        public async Task<IActionResult> Schedule(int? id = null, DateTime? onDate = null, int view = 0)
        {
            ViewBag.Complete = "on";
            if (onDate.HasValue)
                scheduleService.SetBaseDate(onDate);

            var _date = onDate ?? DateTime.Now;
            var cmpConfig = await companyService.GetCompanyCalendarSettings();
            ViewBag.cmpConfig = cmpConfig;

            var start = new DateTime(_date.Year, _date.Month, 1);
            if (view == 1) // monthly view
            {
                ViewBag.start = start;
                await scheduleService.SetDayOfWeeekOnCompany(cmpConfig);
                scheduleService.SetBaseDate(start);
                ViewBag.DurationText = start.ToString("MMMM yyyy");
            }
            else // weekly view
            {
                ViewBag.DurationText = scheduleService.GetDurationText(scheduleService.thisWeekStart, scheduleService.thisWeekEnd, includeDays: false);
            }

            ViewBag.calendars = Calendars.List;
            ViewBag.WeekStart = scheduleService.thisWeekStart;
            ViewBag.WeekEnd = scheduleService.thisWeekEnd;

            var _empId = id ?? userResolverService.GetEmployeeId();
            var _endd = view == 1 ? new DateTime(_date.Year, _date.Month, 1).AddDays(DateTime.DaysInMonth(_date.Year, _date.Month)) : scheduleService.thisWeekEnd;
            var vm = new HomeEmployeeVm
            {
                AttedanceSchedule = await scheduleService.GetCurrentSecdule(empId: _empId, start: ViewBag.start, end: _endd),
                PublicHolidaysUpcoming = await companyService.GetUpComingPublicHolidaysForYear(_date.Year, _date.Month),
                Employee = await employeeService.GetCurrentEmployeeWithDepartment(_empId),
                WeekScheduleTasks = await scheduleService.GetSecduledTasksForThisWeek(empId: _empId),
                WorkTimes = await companyService.GetWorkTimes(),
                MyRequests = await requestService.GetLeaveRequests(userResolverService.GetCompanyId(), new int[] { _empId }, start, _endd, null, new WorkItemStatus[] { WorkItemStatus.Approved, WorkItemStatus.Draft, WorkItemStatus.Submitted }, 1, int.MaxValue),
                //defa = await companyService.GetWorkStartDayOfWeek
            };


            if (view == 1)
            {
                return PartialView("_Monthly", vm);
            }
            return PartialView("_Schedule", vm);
        }


        //{
        //    ViewBag.Complete = "on";
        //    //if (onDate.HasValue)

        //    var _date = onDate ?? DateTime.Now;
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
        //        BirthDaysInMonth = await companyService.GetBirthdayCalendar(_date.Year, _date.Month),
        //        WorkAnniversaries = await companyService.GetWorkAnniversaryCalendar(_date.Year, _date.Month),
        //        MyRequests = await requestService.GetLeaveRequests(userResolverService.GetCompanyId(), new int[] { id }, ViewBag.Start, ViewBag.End, null, new WorkItemStatus[] { WorkItemStatus.Approved, WorkItemStatus.Draft }, 1, int.MaxValue),

        //        PublicHolidaysUpcoming = await companyService.GetUpComingPublicHolidaysForYear(_date.Year, _date.Month),
        //        AttedanceSchedule = await scheduleService.GetCurrentSecdule(empId: userResolverService.GetEmployeeId()),
        //        Employee = await employeeService.GetCurrentEmployeeWithDepartment(),
        //        WeekScheduleTasks = await scheduleService.GetSecduledTasksForThisWeek(empId: userResolverService.GetEmployeeId()),
        //        WorkTimes = await companyService.GetWorkTimes()
        //    };

        //    return PartialView("_Calendar", vm);
        //}

        public async Task<IActionResult> Tasks(int id, DateTime? start = null, DateTime? end = null)
        {
            var _start = start.HasValue ? start.Value : scheduleService.thisWeekStart;
            var _end = end.HasValue ? end.Value : scheduleService.thisWeekEnd;

            var vm = new HomeEmployeeVm
            {
                WeekScheduleTasks = await scheduleService.GetSecduledTasks(empId: id, start: _start, end: _end),
                //AttedanceSchedule = await scheduleService.GetCurrentSecdule(empId: userResolverService.GetEmployeeId()),
            };

            ViewBag.Start = _start;
            ViewBag.End = _end;
            ViewBag.DurationText = scheduleService.GetDurationText(_start, _end, includeDays: false);
            return PartialView("_Tasks", vm);
        }

        public async Task<IActionResult> Requests(int id, DateTime? start = null, DateTime? end = null, RequestType? type = null, WorkItemStatus? status = null)
        {
            var _start = start ?? scheduleService.thisWeekStart;
            var _end = end ?? scheduleService.thisWeekEnd;
            var vm = new HomeEmployeeVm
            {
                Requests = await requestService.GetRequests(id, start, end, type, status, 1, int.MaxValue)
            };

            ViewBag.id = id;
            ViewBag.Start = start;
            ViewBag.End = end;
            ViewBag.Status = status;
            ViewBag.DurationText = scheduleService.GetDurationText(start, end, includeDays: false);
            return PartialView("_Requests", vm.Requests);
        }


        public async Task<IActionResult> PtoAccrurals(int id, DateTime? start = null, DateTime? end = null, int? year = null)
        {
            var _start = start.HasValue ? start.Value : scheduleService.thisWeekStart;
            var _end = end.HasValue ? end.Value : scheduleService.thisWeekEnd;

            ViewBag.Start = _start;
            ViewBag.End = _end;
            ViewBag.DurationText = scheduleService.GetDurationText(_start, _end, includeDays: false);
            ViewBag.Years = Enumerable.Range(0, 10).Select((i, e) => new SelectListItem((2019 + i).ToString(), (2019 + i).ToString(), (2019+i) == year)).ToList();

            return PartialView("_PaidTimeOffAccrurals", await employeeService.GetPaidTimeOffAccrurals(id, year));
        }

        public IActionResult TerminateEmployee(int id)
        {
            var emp = context.Employees.Find(id);
            if (emp == null)
                return BadRequest();
            
            return PartialView("_Terminate", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TerminateEmployee(Employee model)
        {
            var emp = context.Employees.Find(model.Id);
            if (emp == null)
                return ThrowJsonError("Employee was not found");
            if(model.DateOfTermination.HasValue == false)
                return ThrowJsonError("Kindly set termination date");

            if (model.DateOfTermination <= DateTime.UtcNow)
            {
                emp.DateOfTermination = model.DateOfTermination;
                context.SaveChanges();
                return ThrowJsonSuccess("Employee was terminated");
            }
            else
            {

                backgroundJobClient.Schedule(
                    () => employeeService.Terminate(emp.Id, emp.EmpID, emp.GetSystemName(User)),
                    model.DateOfTermination.Value);
                //emp.BackgroundJobs = new Models.BackgroundJob
                //{
                //    TaskType = TaskType.EmployeeTermination,
                //    Name = "Employee Termination",
                //    EndActions = "Change Status to Terminated",
                //    NextRunDate = model.DateOfTermination
                //};
                context.SaveChanges();
                return ThrowJsonSuccess("Employee termination is scheduled to run at "+ model.DateOfTermination.Value.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
            }

            return BadRequest("Please check if these adjsutment variables are filled by employee.");
        }



        public async Task<IActionResult> ViewEmployee(int id)
        {
            var emp = await context.Employees
                .Include(a=> a.Department)
                .FirstOrDefaultAsync(x=> x.Id == id);
            if (emp == null) return BadRequest("Employee was not not found");

            return PartialView("_ViewEmployee", emp);
        }



        #region Create EMployee

        public async Task<IActionResult> NewEmployee()
        {
            var emp = new Employee();
            ViewBag.NewEmployee = true;
            ViewBag.IsCreatingNewEmployee = true;

            //ViewBag.NationalityId = new SelectList(await companyService.GetNationalities(userResolverService.GetCompanyId()), "Id", "Name", emp?.NationalityId);
            return View("Change", emp);
        }

        private async Task<Employee> GetEmployee(int id)
        {
            return await context.Employees.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IActionResult> EditEmployeeProfile(int id)
        {
            var emp = await GetEmployee(id);
            ViewBag.NewEmployee = true;
            ViewBag.IsEditingEmployee = true;
            if (emp.EmployeeSecondaryStatus == EmployeeSecondaryStatus.PersonalInfoMissing)
                ViewBag.Href = "AddOrUpdate";

            return View("Change", emp);
        }

        public async Task<IActionResult> NewEmployeeProfile(int id)
        {
            ViewBag.PrevEmpId = id;
            ViewBag.DepartmentId = new SelectList(await companyService.GetDepartmentsOfCurremtCompany(userResolverService.GetCompanyId()), "Id", "Name");
            ViewBag.LocationId = new SelectList(await companyService.GetLocationsOfCurremtCompany(), "Id", "Name");
            ViewBag.ReportingEmployeeId = await companyService.GetAllEmployeesInMyCompanyForDropdownOptGroups();


            return PartialView("_NewEmployeeProfile", new EmploymentProfileVm { CompanyId = userResolverService.GetCompanyId(), InviteThemToFill = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewEmployeeProfile(EmploymentProfileVm model)
        {
            var cmp = await companyService.GetCompanyAccount(model.CompanyId);
            if (cmp == null) ModelState.AddModelError("", "Company account was not found!");

            if (model.DepartmentId == -1 && string.IsNullOrWhiteSpace(model.NewDepartment))
                ModelState.AddModelError("", "Please enter name of new department!");
            if (ModelState.IsValid)
            {
                var loc = await context.Locations.FindAsync(model.LocationId);
                var mgr = await context.Employees.FindAsync(model.ReportingEmployeeId);
                // var levelId = await companyService.GetOrCreateAndGetClassificationIdAsync(model.Level, model.CompanyId);


                var deptId = (model.DepartmentId == -1) ?
                    await companyService.GetOrCreateAndGetDepartmentIdAsync(model.NewDepartment, model.CompanyId) :
                    model.DepartmentId;
                var dept = await context.Departments.FindAsync(deptId);

                //var Job = new Job
                //{
                //    CompanyId = model.CompanyId,
                //    JobTitle = model.JobTitle,
                //    LevelId = levelId,
                //    DepartmentId = dept.Id,
                //};
                var emp = new Employee
                {
                    //Individual = new Individual
                    //{
                    //    FirstName = model.FirstName,
                    //    LastName = model.LastName,
                        
                    //},
                    EmailPersonal = model.PersonalEmail,
                    CompanyId = model.CompanyId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    JobTitle = model.JobTitle,
                    //Job = Job,

                    DateOfJoined = model.StartDate,
                    LocationId = model.LocationId,
                    LocationName = loc.Name,
                    DepartmentId = dept.Id,
                    DepartmentName = dept.Name,
                    EmailWork = model.PersonalEmail,
                };

                if (mgr != null)
                    emp.ReportingEmployeeId = mgr.Id;

                //if (model.Wages.Any(x => x.IsActive))
                //    emp.EmployeePayComponents = model.Wages.ToList();

                if (model.InviteThemToFill)
                {
                    emp.EmployeeStatus = EmployeeStatus.ActionNeeded;
                    emp.EmployeeSecondaryStatus = EmployeeSecondaryStatus.WaitingSelfOnBoard;
                }
                else
                {
                    emp.EmployeeStatus = EmployeeStatus.Incomplete;
                    emp.EmployeeSecondaryStatus = EmployeeSecondaryStatus.PersonalInfoMissing;
                }

                context.Employees.Add(emp);
                await context.SaveChangesAsync();

                model.Employee = emp;

                return RedirectToAction(nameof(AddOrUpdate), new { id = emp.Id });
            }
            return ThrowJsonError(ModelState);
        }


        public async Task<IActionResult> AddOrUpdate(int id = 0)
        {
            Employee emp = await GetEmployee(id);
            if (emp == null && id > 0) return BadRequest("Employee was not not found");

            ViewBag.NationalityId = new SelectList(await companyService.GetNationalities(userResolverService.GetCompanyId()), "Id", "Name", emp?.NationalityId);

            if (emp == null) emp = new Employee();
            return PartialView("_AddOrUpdate", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditTrailAction("Master data")]
        public async Task<IActionResult> AddOrUpdate(Employee model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id <= 0)
                {
                    return ThrowJsonError("Sorry!");
                    model.CompanyId = userResolverService.GetCompanyId();
                    context.Employees.Add(model);
                }
                else
                {
                    var empInDb = await GetEmployee(model.Id);
                    if (User.IsHrManagerOrAdmin())
                    {
                        // update
                        //empInDb.Initial = model.Initial;
                        //empInDb.FirstName = model.FirstName;
                        //empInDb.MiddleName = model.MiddleName;
                        //empInDb.LastName = model.LastName;

                        empInDb.DateOfBirth = model.DateOfBirth;
                        empInDb.Gender = model.Gender;
                        empInDb.EmpID = model.EmpID;
                        //empInDb.EmailPersonal = model.EmailPersonal;
                        //empInDb.PhonePersonal = model.PhonePersonal;
                        empInDb.IdentityType = model.IdentityType;
                        empInDb.IdentityNumber = model.IdentityNumber;
                        empInDb.Nationality = model.Nationality;

                        //empInDb.JobType = model.JobType;

                        //empInDb.EmailAddress = model.EmailAddress;
                        //empInDb.PhonePersonal = model.PhonePersonal;
                        context.Employees.Update(empInDb);
                    }
                    else
                    {
                        // start request or update
                        var activeRequest = await requestService.GetOpenRequests(empInDb.Id, RequestType.Emp_DataChange);
                        var isCreating = activeRequest == null;
                        if (isCreating)
                        {
                            activeRequest = new Request { EmployeeId = empInDb.Id, CompanyId = empInDb.CompanyId, RequestType = RequestType.Emp_DataChange, Status = WorkItemStatus.Draft, CreationDate = DateTime.UtcNow };
                            activeRequest.RequestDataChanges.Add(await requestService.FillWithExistingData(empInDb.Id));
                            activeRequest = await requestService.GenerateNextNumberAsync(activeRequest);
                        }


                        //activeRequest.RequestDataChanges.First().Initial = model.Initial;
                        //    activeRequest.RequestDataChanges.First().FirstName = model.FirstName;
                        //    activeRequest.RequestDataChanges.First().MiddleName = model.MiddleName?.Trim();
                        //    activeRequest.RequestDataChanges.First().LastName = model.LastName;

                            activeRequest.RequestDataChanges.First().DateOfBirth = model.DateOfBirth;
                            activeRequest.RequestDataChanges.First().Gender = model.Gender;
                            //activeRequest.RequestDataChanges.First().EmailPersonal = model.EmailPersonal;
                            //activeRequest.RequestDataChanges.First().PhonePersonal = model.PhonePersonal;
                            activeRequest.RequestDataChanges.First().IdentityType = model.IdentityType;
                            activeRequest.RequestDataChanges.First().IdentityNumber = model.IdentityNumber;
                        activeRequest.RequestDataChanges.First().NationalityId = model.NationalityId;
                        

                        if(isCreating)
                        context.Requests.Add(activeRequest);
                        else
                            context.Requests.Update(activeRequest);
                    }
                }
                await context.SaveChangesAsync();

                return RedirectToAction(nameof(AddOrUpdateAddressContact), new { id = model.Id });
            }
            return BadRequest(ModelState);
        }

        public async Task<IActionResult> AddOrUpdateBankDetails(int id)
        {
            if (!context.Employees.Any(x => x.Id == id))
                return BadRequest("Employee was not found");

            var model = await context.Employees.FirstOrDefaultAsync(x => x.Id == id);
            await EnsureCanEditEmployeeAsync(model);
            return PartialView("_AddOrUpdateBankDetails", model);
        }

        private async Task EnsureCanEditEmployeeAsync(Employee model)
        {
            if (User.IsHrManagerOrAdmin())
                return;

            if(await requestService.IsRequestSubsmitted(model.Id, RequestType.Emp_DataChange))
                throw new ApplicationException("Requsts already submitted and cannot proceed until action is taken");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditTrailAction("Bank Details")]
        public async Task<IActionResult> AddOrUpdateBankDetails(Employee model)
        {
            if (!context.Employees.Any(x => x.Id == model.Id))
                return BadRequest("Employee was not found");

            var empInDb = await GetEmployee(model.Id);
            if (User.IsHrManagerOrAdmin())
            {
                //empInDb.Bio_About = model.Bio_About;
                //empInDb.EmpID = model.EmpID;
                //empInDb.DateOfJoined = model.DateOfJoined;
                //empInDb.EmailWork = model.EmailWork;
                //empInDb.PhoneWork = model.PhoneWork;
                empInDb.BankAccountName = model.BankAccountName;
                empInDb.BankAccountNumber = model.BankAccountNumber;
                empInDb.BankName = model.BankName;
                empInDb.BankAddress = model.BankAddress;
                empInDb.BankCurrency = model.BankCurrency;
                empInDb.BankIBAN = model.BankIBAN;
                empInDb.BankSwiftCode = model.BankSwiftCode;

                if (empInDb.EmployeeSecondaryStatus == EmployeeSecondaryStatus.PersonalInfoMissing)
                    empInDb.EmployeeSecondaryStatus = EmployeeSecondaryStatus.EmploymentInfoMissing;

                context.Employees.Update(empInDb);
            }
            else
            {
                var activeRequest = await requestService.GetOpenRequests(empInDb.Id, RequestType.Emp_DataChange);
                if (activeRequest == null)
                    return BadRequest("Employee Data change request was not found");
                //activeRequest.RequestDataChanges.First().Bio_About = model.Bio_About;
                //activeRequest.RequestDataChanges.First().DateOfJoined = model.DateOfJoined;
                activeRequest.RequestDataChanges.First().EmpID = model.EmpID;
                activeRequest.RequestDataChanges.First().EmailWork = model.EmailWork;
                activeRequest.RequestDataChanges.First().PhoneWork = model.PhoneWork;
                activeRequest.RequestDataChanges.First().BankAccountName = model.BankAccountName;
                activeRequest.RequestDataChanges.First().BankAccountNumber = model.BankAccountNumber;
                activeRequest.RequestDataChanges.First().BankName = model.BankName;
                activeRequest.RequestDataChanges.First().BankAddress = model.BankAddress;
                activeRequest.RequestDataChanges.First().BankCurrency = model.BankCurrency;
                activeRequest.RequestDataChanges.First().BankIBAN = model.BankIBAN;
                activeRequest.RequestDataChanges.First().BankSwiftCode = model.BankSwiftCode;
                context.Requests.Update(activeRequest);
            }
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(AddEmployment), new { id = model.Id });
        }


        public async Task<IActionResult> AddOrUpdateAddressContact(int id)
        {
            if (!context.Employees.Any(x => x.Id == id))
                return BadRequest("Employee was not found");


            var model = await context.Employees
                .Include(a => a.EmployeeAddresses)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (model.EmployeeAddresses == null)
                model.EmployeeAddresses = new List<Address>();

            if (!model.EmployeeAddresses.Any())
            {
                model.EmployeeAddresses.AddRange(new[]
                {
                    new Address { CountryId = 135, AddressType = AddressType.Present },
                    new Address { CountryId = 135, AddressType = AddressType.Permanant }
                });
            }
            await EnsureCanEditEmployeeAsync(model);

            return PartialView("_AddOrUpdateAddressContact", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditTrailAction("Address details")]
        public async Task<IActionResult> AddOrUpdateAddressContact(Employee model)
        {
            if (!context.Employees.Any(x => x.Id == model.Id))
                return BadRequest("Employee was not found");

            var empInDb = await context.Employees
                .Include(a => a.EmployeeAddresses)
                .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (User.IsHrManagerOrAdmin())
            {
                empInDb.PhoneWork = model.PhoneWork;

                if (empInDb.EmployeeAddresses == null)
                {
                    empInDb.EmployeeAddresses = new List<Address>();
                    empInDb.EmployeeAddresses.AddRange(new[]
                    {
                    new Address { AddressType = AddressType.Present },
                    new Address { AddressType = AddressType.Permanant }
                });
                }

                if (!empInDb.EmployeeAddresses.Any())
                {
                    empInDb.EmployeeAddresses.AddRange(new[]
                    {
                    new Address { AddressType = AddressType.Present },
                    new Address { AddressType = AddressType.Permanant }
                });
                }

                empInDb.EmployeeAddresses.First().Country = model.EmployeeAddresses.First().Country;
                empInDb.EmployeeAddresses.First().Street1  = model.EmployeeAddresses.First().Street1;
                empInDb.EmployeeAddresses.First().State  = model.EmployeeAddresses.First().State;
                empInDb.EmployeeAddresses.First().City  = model.EmployeeAddresses.First().City;
                empInDb.EmployeeAddresses.First().ZipCode  = model.EmployeeAddresses.First().ZipCode;
                empInDb.EmployeeAddresses.First().AddressType = model.EmployeeAddresses.First().AddressType;

                empInDb.EmployeeAddresses.Last().Country = model.EmployeeAddresses.Last().Country;
                empInDb.EmployeeAddresses.Last().Street1 = model.EmployeeAddresses.Last().Street1;
                empInDb.EmployeeAddresses.Last().State = model.EmployeeAddresses.Last().State;
                empInDb.EmployeeAddresses.Last().City = model.EmployeeAddresses.Last().City;
                empInDb.EmployeeAddresses.Last().ZipCode = model.EmployeeAddresses.Last().ZipCode;
                empInDb.EmployeeAddresses.Last().AddressType = model.EmployeeAddresses.Last().AddressType;
                //empInDb.Address = model.Address;
                empInDb.ZipCode = model.ZipCode;
                context.Employees.Update(empInDb);
            }
            else
            {
                var activeRequest = await requestService.GetOpenRequests(empInDb.Id, RequestType.Emp_DataChange);
                if (activeRequest == null)
                    return BadRequest("Employee Data change request was not found");
                //activeRequest.RequestDataChanges.First().Bio_About = model.Bio_About;
                //activeRequest.RequestDataChanges.First().DateOfJoined = model.DateOfJoined;
                activeRequest.RequestDataChanges.First().Country = model.EmployeeAddresses.First().Country;
                activeRequest.RequestDataChanges.First().StreetAddress = model.EmployeeAddresses.First().Street1;
                activeRequest.RequestDataChanges.First().State = model.EmployeeAddresses.First().State;
                activeRequest.RequestDataChanges.First().City = model.EmployeeAddresses.First().City;
                activeRequest.RequestDataChanges.First().ZipCode = model.EmployeeAddresses.First().ZipCode;

                activeRequest.RequestDataChanges.First().Country1 = model.EmployeeAddresses.Last().Country;
                activeRequest.RequestDataChanges.First().StreetAddress1 = model.EmployeeAddresses.Last().Street1;
                activeRequest.RequestDataChanges.First().State1 = model.EmployeeAddresses.Last().State;
                activeRequest.RequestDataChanges.First().City1 = model.EmployeeAddresses.Last().City;
                activeRequest.RequestDataChanges.First().ZipCode1 = model.EmployeeAddresses.Last().ZipCode;
                context.Requests.Update(activeRequest);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(AddOrUpdateEmergencyContact), new { id = model.Id });
        }

        public async Task<IActionResult> AddOrUpdateEmergencyContact(int id)
        {
            if (!context.Employees.Any(x => x.Id == id))
                return ThrowJsonSuccess("Employee was not found");

            var model = await GetEmployee(id); await EnsureCanEditEmployeeAsync(model);

            ViewBag.EmergencyContactRelationshipId = new SelectList(await companyService.GetEmergencyContactRelationships(userResolverService.GetCompanyId()), "Id", "Name", model?.EmergencyContactRelationshipId);
            return PartialView("_AddOrUpdateEmergencyContact", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditTrailAction("Emergency Contact")]
        public async Task<IActionResult> AddOrUpdateEmergencyContact(Employee model)
        {
            if (!context.Employees.Any(x => x.Id == model.Id))
                return ThrowJsonSuccess("Employee was not found");

            var empInDb = await GetEmployee(model.Id);
            if (User.IsHrManagerOrAdmin())
            {
                empInDb.EmergencyContactName = model.EmergencyContactName;
                empInDb.EmergencyContactNumber = model.EmergencyContactNumber;
                empInDb.EmergencyContactRelationshipId = model.EmergencyContactRelationshipId;
                context.Employees.Update(empInDb);
            }
            else
            {
                var activeRequest = await requestService.GetOpenRequests(empInDb.Id, RequestType.Emp_DataChange);
                if (activeRequest == null)
                    return BadRequest("Employee Data change request was not found");
                activeRequest.RequestDataChanges.First().EmergencyContactName = model.EmergencyContactName;
                activeRequest.RequestDataChanges.First().EmergencyContactNumber= model.EmergencyContactNumber;
                activeRequest.RequestDataChanges.First().EmergencyContactRelationshipId = model.EmergencyContactRelationshipId;
                context.Requests.Update(activeRequest);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(AddOrUpdateBankDetails), new { id = model.Id });
        }



        public async Task<IActionResult> AddOrUpdateSocial(int id)
        {
            if (!context.Employees.Any(x => x.Id == id))
                return BadRequest("Employee was not found");

            var model = await GetEmployee(id);
            await EnsureCanEditEmployeeAsync(model);

            return PartialView("_AddOrUpdateSocial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditTrailAction("Social Accounts")]
        public async Task<IActionResult> AddOrUpdateSocial(Employee model)
        {
            ModelState.Remove("Name");
            var keys = new[] { "TwitterId", "LinkedInId", "FacebookId", "InstagramId" };
            if (HasErrorsInModelState(ModelState, keys))
                return ThrowJsonError(ModelState, keys);

            if (!context.Employees.Any(x => x.Id == model.Id))
                return BadRequest("Employee was not found");

            var empInDb = await GetEmployee(model.Id);
            if (User.IsHrManagerOrAdmin())
            {
                empInDb.TwitterId = model.TwitterId;
                empInDb.LinkedInId = model.LinkedInId;
                empInDb.FacebookId = model.FacebookId;
                empInDb.InstagramId = model.InstagramId;
                context.Employees.Update(empInDb);
            }
            else
            {
                var activeRequest = await requestService.GetOpenRequests(empInDb.Id, RequestType.Emp_DataChange);
                if (activeRequest == null)
                    return BadRequest("Employee Data change request was not found");
                activeRequest.RequestDataChanges.First().TwitterId = model.TwitterId;
                activeRequest.RequestDataChanges.First().LinkedInId = model.LinkedInId;
                activeRequest.RequestDataChanges.First().FacebookId = model.FacebookId;
                activeRequest.RequestDataChanges.First().InstagramId = model.InstagramId;
                context.Requests.Update(activeRequest);
            }

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(UploadImage), new { id = model.Id });
        }


        public async Task<IActionResult> UploadImage(int id, string update = "")
        {
            ViewBag.Update = update;
            var emp = await employeeService.GetEmployeeById(id);
            if (emp == null) return ThrowJsonError("Company not found");
            await EnsureCanEditEmployeeAsync(emp);
            return PartialView("_UploadImage", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(Employee model, string base64Image)
        {
            var emp = await context.Employees
                .FirstOrDefaultAsync(a => a.Id == model.Id);
            if (emp == null) return ThrowJsonError("Rmployee was not found");
            string newAvatar = null;
            bool hasFiles = fileUploadService.HasFilesReadyForUpload(base64Image);
            if (hasFiles)
            {
                if (fileUploadService.GetFileSizeInMb(base64Image) >= UploadSetting.MaxImageSizeMb)
                    return ThrowJsonError($"File is too huge, only file size up to {UploadSetting.MaxFileSizeMb}MB are allowed!");

                //if (!fileUploadService.IsAllowedFileType(base64Image, UploadSetting.ImageTypes))
                //    return ThrowJsonError($"Uploaded file type is not allowed!");


                string fileUrl = await fileUploadService.UploadFles(base64Image);
                newAvatar = fileUrl;
            }

            if (User.IsHrManagerOrAdmin())
            {
                if(hasFiles)
                emp.Avatar = newAvatar ?? emp.Avatar;
                emp.Avatar = newAvatar;
                emp.NickName = emp.NickName = model.NickName;
            }
            else
            {
                var activeRequest = await requestService.GetOpenRequests(model.Id, RequestType.Emp_DataChange);
                if (activeRequest == null)
                    return BadRequest("Employee Data change request was not found");
                activeRequest.RequestDataChanges.First().Avatar = newAvatar ?? emp.Avatar;
                activeRequest.RequestDataChanges.First().NickName = model.NickName;
                context.Requests.Update(activeRequest);
            }
            await context.SaveChangesAsync();

            return ThrowJsonSuccess(new { newAvatar });
        }
        //public async Task<IActionResult> AddEmployment(int id)
        //{
        //    var emp = await context.Employees.Where(a=> a.Id == id)
        //        .Include(a=> a.Jobs)
        //        .Include(a=>a.EmployeeTypes)
        //        .FirstOrDefaultAsync();
        //    if (emp == null) return ThrowJsonError("Employee was not not found");
        //    if (emp.EmployeeTypes.Any() || emp.Jobs.Any()) return ThrowJsonError("Employee already has existing Employment(s)");


        //    ViewBag.jobId = new SelectList(await companyService.GetJobs(JobStatus.Vacant), "Id", "Name", empJobInfo?.Id);

        //    return PartialView("_AddEmployment", emp);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[AuditTrailAction("Employment Information")]
        //public async Task<IActionResult> AddEmployment(Employement model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var emp = await context.Employees.FindAsync(model.EmployeeId);
        //        if (emp == null)
        //            return ThrowJsonError();

        //        //if (model.ReportingEmployeeId == model.EmployeeId)
        //        //    return ThrowJsonError("Employee cannot report to themselves");
        //        //if (model._EmploymentStatus == EmploymentStatus.Terminated || model._EmploymentStatus == EmploymentStatus.None)
        //        //    return ThrowJsonError("Please choose any employment status");

        //        //if (model.ReportingEmployeeId <= 0)
        //        //    model.ReportingEmployeeId = null;
        //        context.Jobs.Add(model);
        //        context.EmployeeTypes.Add(new EmployeeType { EffectiveDate = model.EffectiveDate, EmploymentStatus = model._EmploymentStatus, EmployeeId = model.EmployeeId });


        //        emp.LocationId = model.LocationId;
        //        emp.DepartmentId = model.DepartmentId;
        //        emp.JobTitle = model.JobTitle;
        //        emp.EmploymentStatus = model._EmploymentStatus;

        //        context.SaveChanges();
        //        TempData["Message"] = "Employment was completed successfully";
        //        return RedirectToAction(nameof(Change), new { id = model.EmployeeId });
        //    }

        //    return BadRequest(ModelState);
        //}


        [HttpPost]
        public async Task<IActionResult> SendForApproval(int id)
        {
            var request = await requestService.GetOpenRequests(id, RequestType.Emp_DataChange);
            if (request?.EmployeeId != userResolverService.GetEmployeeId())
                return ThrowJsonError("Request was not found or employee mismatch detected");
            if (request?.Status == WorkItemStatus.Submitted)
                return ThrowJsonError("Request is already submitted");

            request.Status = WorkItemStatus.Submitted;
            request.SubmissionDate = DateTime.UtcNow;
            request.DataChangesCount = (await requestService.GetChangedFieldsList(request)).Count();
            if(request.DataChangesCount <= 0)
                return ThrowJsonError("There wern't any data changes found in the request");

            await notificationService.SendAsync(NotificationTypeConstants.RequestEmployeeDataChange,
                obj: request,
                companyAccountId: request.CompanyId);
            await context.SaveChangesAsync();

            SetTempDataMessage("Request successfully sent for approval", MsgAlertType.success);
            return RedirectToAction(nameof(Change), new { id = id });
        }


        public async Task<IActionResult> AddCompensation(int id)
        {
            if (!context.Employees.Any(x => x.Id == id)) return RedirectToAction(nameof(Index));

            var adjustments = await companyService.GetPayAdjustments();
            if (adjustments.Any() == false)
                return ThrowJsonError("Oops! Company does not have pay components configured!");

            var empPayAdjustmentList = adjustments
                      .Select(ajusments => new EmployeePayComponent
                      {
                          EmployeeId = id,
                          Total = 0,
                          PayAdjustment = ajusments,
                          PayAdjustmentId = ajusments.Id
                      }).ToList();


            var oldrecords = context.EmployeePayComponents.Where(x => x.EmployeeId == id).ToList();

            foreach (var item in empPayAdjustmentList)
            {
                if (oldrecords.Any(x => x.PayAdjustmentId == item.PayAdjustmentId))
                {
                    // replcaing toal
                    item.IsActive = true;
                }
            }

            return PartialView("_AddCompensation", empPayAdjustmentList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCompensation(List<EmployeePayComponent> model, DateTime date)
        {
            var first = model.FirstOrDefault();
            if (first == null)
                return RedirectToAction(nameof(Index));

            if (first == null) return ThrowJsonError("Employee pay adjustment was not found");
            if (ModelState.IsValid)
            {
                var hasPrevRecords = await context.EmployeePayComponents.AnyAsync(x => x.EmployeeId == first.EmployeeId);
                if (hasPrevRecords)
                    return ThrowJsonError("Employee already has pay components");

                var newDed = model.Where(a => a.IsActive).ToList();
                if (newDed.Count () <= 0)
                    return ThrowJsonError("Please choose atleast one pay component");

                newDed.ForEach(t => { t.EffectiveDate = date; });

                context.EmployeePayComponents.AddRange(newDed);
                context.SaveChanges();

                TempData["Message"] = "Pay components was updated successfully";
                return RedirectToAction(nameof(Change), new { id = first.EmployeeId });
            }

            return BadRequest(ModelState);
        }


        //public async Task<IActionResult> AddEmployeeReplacement(int id)
        //{
        //    var emp = await context.Employees.Where(a => a.Id == id)
        //        .Include(a => a.Employments)
        //        .Include(a => a.EmployeeTypes)
        //        .FirstOrDefaultAsync();
        //    if (emp == null) return ThrowJsonError("Employee was not not found");
        //    if (emp.EmployeeTypes.Any() || emp.Employments.Any()) return ThrowJsonError("Employee already has existing Employment");


        //    var emps = await companyService.GetEmployeesToReplace(emp.CompanyId);
        //    ViewBag.ReplacingEmployeeId = new SelectList(emps, "Id", "Name");
        //    ViewBag.HasReplacementEmployee = emps.Count() > 0;
        //    ViewBag.Id = id;
        //    return PartialView("_AddEmployeeReplacement");
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddEmployeeReplacement(int id, int ReplacingEmployeeId, DateTime date)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var emp = await context.Employees.Where(a => a.Id == id)
        //            .Include(a => a.Employments)
        //            .Include(a => a.EmployeeTypes)
        //            .FirstOrDefaultAsync();
        //        if (emp == null) return ThrowJsonError("Employee was not not found");
        //        if (emp.EmployeeTypes.Any() || emp.Employments.Any()) return ThrowJsonError("Employee already has existing Employment");


        //        var replacingEmpl = await context.Employees.Where(a => a.Id == ReplacingEmployeeId && a.EmployeeStatus == EmployeeStatus.Terminated)
        //            .Include(a => a.Employments)
        //            .Include(a => a.EmployeeTypes)
        //            .Include(a => a.EmployeePayComponents)
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync();
        //        if (replacingEmpl == null) return ThrowJsonError("Replacing Employee was not not found");
        //        if (!replacingEmpl.EmployeeTypes.Any() && !emp.Employments.Any()) return ThrowJsonError("Replacing Employee conditions are not set correctly");


        //        var newJobInfo = replacingEmpl.Employments.First(x => x.RecordStatus == RecordStatus.Active);
        //        newJobInfo.Id = 0;
        //        newJobInfo.EmployeeId = id;
        //        newJobInfo.EffectiveDate = date;


        //        var newEmplState = replacingEmpl.EmployeeTypes.First(x => x.RecordStatus == RecordStatus.Active);
        //        newEmplState.Id = 0;
        //        newEmplState.EmployeeId = id;
        //        newEmplState.EffectiveDate = date;


        //        var newPayCompns = replacingEmpl.EmployeePayComponents.Where(x => x.IsActive).ToList();
        //        newPayCompns.ForEach(p =>
        //        {
        //            p.Id = 0;
        //            p.EmployeeId = id;
        //            p.EffectiveDate = date;
        //        });

        //        context.EmployeeTypes.Add(newEmplState);
        //        context.Employments.Add(newJobInfo);
        //        context.EmployeePayComponents.AddRange(newPayCompns);
        //        await context.SaveChangesAsync();
        //        TempData["Message"] = "Employee Replacement was completed successfully";
        //        return RedirectToAction(nameof(Change), new { id = id });
        //    }

        //    return BadRequest(ModelState);
        //}

        #endregion

        #region Contract

        #endregion

        #region Add /Update Employee Type and Job Infos

        //public async Task<IActionResult> AddOrUpdateEmployeeType(int eId, int id = 0)
        //{
        //    var emp = await context.Employees.FindAsync(eId);
        //    if (emp == null && id > 0) return BadRequest("Employee was not not found");

        //    ViewBag.TerminationReasonId = new SelectList(await context.TerminationReasons.Where(a => a.CompanyId == emp.CompanyId || a.CompanyId.HasValue == false).ToListAsync(), "Id", "Name");

        //    var type = new EmployeeType { EmployeeId = eId,EffectiveDate = DateTime.UtcNow };
        //    if(id > 0)
        //    {
        //        var _type = await context.EmployeeTypes.FirstOrDefaultAsync(x=> x.Id == id && x.EmployeeId == eId);
        //        if (_type == null && id > 0) return BadRequest("Employee Type was not not found");
        //        else
        //            type = _type;
        //    }
            
        //    return PartialView("_AddOrUpdateEmployeeType", type);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddOrUpdateEmployeeType(EmployeeType model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var emp = await context.Employees.FindAsync(model.EmployeeId);
        //        if (emp == null)
        //            return ThrowJsonError();

        //        if(model.EmploymentStatus == EmployeeStatus.Terminated && model.TerminationReasonId <= 0)
        //            return ThrowJsonError("Kindly enter reason for termination");

        //        if (model.Id <= 0)
        //        {
        //            if (model.EmploymentStatus != EmployeeStatus.Terminated)
        //                model.TerminationReasonId = (int?)null;
        //            context.EmployeeTypes.Add(model);
        //        }
        //        else
        //        {
        //            // update
        //            var empTypeInDb = await context.EmployeeTypes.FindAsync(model.Id);

        //            empTypeInDb.EmploymentStatus = model.EmploymentStatus;
        //            empTypeInDb.EffectiveDate = model.EffectiveDate;


        //            empTypeInDb.TerminationReasonId = model.TerminationReasonId;
        //            if (empTypeInDb.EmploymentStatus != EmployeeStatus.Terminated)
        //                empTypeInDb.TerminationReasonId = (int?)null;
        //        }

        //        emp.EmployeeStatus = model.EmploymentStatus;
        //        context.SaveChanges();
        //        return RedirectToAction(nameof(EmploymentAndJob), new { id = model.EmployeeId });
        //    }
        //    return BadRequest(ModelState);
        //}



        //public async Task<IActionResult> AddEmployment(int id)
        //{
        //    var emp = await context.Employees.Where(a=> a.Id == id)
        //        .Include(a=> a.Jobs)
        //        .Include(a=>a.EmployeeTypes)
        //        .FirstOrDefaultAsync();
        //    if (emp == null) return ThrowJsonError("Employee was not not found");
        //    if (emp.EmployeeTypes.Any() || emp.Jobs.Any()) return ThrowJsonError("Employee already has existing Employment(s)");


        //    ViewBag.jobId = new SelectList(await companyService.GetJobs(JobStatus.Vacant), "Id", "Name", empJobInfo?.Id);

        //    return PartialView("_AddEmployment", emp);
        //}

        public async Task<IActionResult> AddEmployment(int id, int eId = 0)
        {
            var emp = await context.Employees.FindAsync(id);
            if (emp == null) return BadRequest("Employee was not not found");
            
            //var empJobInfo = new Employment { EmployeeId = id, ReportingEmployeeId = emp.ReportingEmployeeId, JobId = emp.JobId ?? 0, EffectiveDate = DateTime.Now };
            //if (eId > 0)
            //{
            //    var _type = await context.Employments.FirstOrDefaultAsync(x => x.Id == eId && x.EmployeeId == id);
            //    if (_type == null && eId > 0) return BadRequest("Employee Job Information was not not found");
            //    else
            //        empJobInfo = _type;
            //}

            var vacantJobs = await companyService.GetJobs(JobStatus.Vacant, id);
            ViewBag.JobId = new SelectList(vacantJobs.ToDictionary(a=> a.Id,a=> $"{a.JobID} · {a.JobTitle}"), "Key", "Value", emp?.JobId);
            //ViewBag.DepartmentId = new SelectList(await companyService.GetDepartmentsOfCurremtCompany(userResolverService.GetCompanyId()), "Id", "Name", empJobInfo?.JobId);
            //ViewBag.LocationId = new SelectList(await companyService.GetLocationsOfCurremtCompany(), "Id", "Name", empJobInfo?.LocationId);
            ViewBag.ReportingEmployeeId = await companyService.GetAllEmployeesInMyCompanyForDropdownOptGroups();
            ViewBag.DepartmentId = new SelectList(await companyService.GetDepartmentsOfCurremtCompany(userResolverService.GetCompanyId()), "Id", "Name");

            if (emp.IsIncomplete && !emp.ContractStartDate.HasValue)
                emp.ContractStartDate = emp.DateOfJoined;

            return PartialView("_AddEmployment", emp);
        }

        public async Task<IActionResult> AddTransfer(int id)
        {
            var emp = await context.Employees.FindAsync(id);
            if (emp == null) return BadRequest("Employee was not not found");


            ViewBag.DepartmentId = new SelectList(await companyService.GetDepartmentsOfCurremtCompany(userResolverService.GetCompanyId()), "Id", "Name", emp?.DepartmentId);
            ViewBag.DivisionId = new SelectList(await companyService.GetDivisionsOfCurremtCompany(), "Id", "Name", emp?.DivisionId);
            ViewBag.LocationId = new SelectList(await companyService.GetLocationsOfCurremtCompany(), "Id", "Name", emp?.LocationId);


            return PartialView("_AddEmployment", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditTrailAction("Change Employment")]
        public async Task<IActionResult> AddEmployment(Employee model)
        {
            if (ModelState.IsValid)
            {
                var emp = await context.Employees
                .Where(x=> x.Id == model.Id)
                .Include(a=> a.ReportingEmployee)
                .FirstOrDefaultAsync();

                if (emp == null)
                    return ThrowJsonError();

                if (model.ReportingEmployeeId == model.Id)
                    return ThrowJsonError("Employee cannot report to themselves");

                if (model.EmploymentType == EmployeeType.None)
                    return ThrowJsonError("Employment type cannot be none");

                if (model.WeeklyWorkingHours <= 0 || model.DailyWorkingHours <= 0 )
                    return ThrowJsonError("Working hours must be greater than 0");
                if (model.DailyWorkingHours > 24)
                    return ThrowJsonError("Daily working hours cannot be greater than 24");

                Job job = null;
                if (!model.IsCreateNewJob)
                {
                    job = await context.Jobs.Where(a => a.Id == model.JobId).Include(a => a.JobPayComponents).FirstOrDefaultAsync();
                    if (job == null || job.JobStatus != JobStatus.Vacant)
                        if (emp.JobId != job.Id)
                            return ThrowJsonError("Job was not found or isn't vacant");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(model.JobTitle))
                        return ThrowJsonError($"Job requires a title!");

                    if (!model.DepartmentId.HasValue)
                        return ThrowJsonError($"New job ${model.JobTitle} needs a department!");

                    if (string.IsNullOrWhiteSpace(model.JobTitle) || string.IsNullOrWhiteSpace(model.Level) || 
                            !model.JobType.HasValue)
                        return ThrowJsonError("Field required to create job was found empty");
                    job = new Job
                    {
                        CompanyId = userResolverService.GetCompanyId(),
                        JobTitle = model.JobTitle,
                        JobType = model.JobType.Value,
                        LevelId = await companyService.GetOrCreateAndGetClassificationIdAsync(model.Level),
                        DepartmentId = model.DepartmentId.Value,
                    };
                }


                if (emp.IsIncomplete || emp.EmployeeStatus == EmployeeStatus.Active)
                {
                    if (model.ReportingEmployeeId <= 0)
                        model.ReportingEmployeeId = null;

                    //if (job.JobPayComponents.Any() && model.UpdateFromJobPayComponents)
                    //    model.EmployeePayComponents.AddRange(job.JobPayComponents.Select(a => new EmployeePayComponent { PayAdjustmentId = a.PayAdjustmentId, Total = a.Total, EmployeeId = model.Id }).ToList());

                    ActionType type = ActionType.TC;
                    if(emp.JobId != model.JobId){
                        
                        type = ActionType.APP;
                        emp.JobId = model.JobId;
                    }
                    
                    emp.ReportingEmployeeId = model.ReportingEmployeeId;
                    emp.WeeklyWorkingHours = model.WeeklyWorkingHours;
                    emp.DailyWorkingHours = model.DailyWorkingHours;
                    //emp.EmployeeStatus = model.EmploymentStatus = EmployeeStatus.Active;
                    model.RecordStatus = RecordStatus.Active;
                    emp.EmploymentType = model.EmploymentType;
                    emp.EmploymentTypeOther = model.EmploymentTypeOther;

                    emp.ContractStartDate = model.ContractStartDate;
                    emp.ContractEndDate = model.ContractEndDate;
                    if (emp.DateOfJoined != emp.ContractStartDate)
                        emp.DateOfJoined = emp.ContractStartDate;
                    if (model.HasProbationEndDate)
                    {
                        emp.ProbationEndDate = model.ProbationEndDate;
                        emp.ProbationStartDate = emp.ContractStartDate;
                    }


                    if (model.IsCreateNewJob)
                    {
                        emp.Job = job;
                    }

                    employeeService.AddEmployeeAction(ref emp, ref job, model.ReportingEmployeeId, type: type);
                    //employeeService.AddJobActionHistory(ref emp, ref job, model.ReportingEmployeeId, type: type);

                    //context.Employments.Add(model);
                }


                emp.JobTitle = job.JobTitle;
                emp.DepartmentId = job.DepartmentId;
                emp.DepartmentName = await context.Departments.Where(t => t.Id == job.DepartmentId).Select(t => t.Name).FirstOrDefaultAsync();
                emp.JobType = job.JobType;


                //else
                //{
                //    // update
                //    var empTypeInDb = await context.Employments.FindAsync(model.Id);
                //    empTypeInDb.EffectiveDate = model.EffectiveDate;

                //    empTypeInDb.JobId = model.JobId;

                //    //empTypeInDb.DivisionId = model.DivisionId;
                //    //empTypeInDb.DepartmentId = model.DepartmentId;
                //    //empTypeInDb.JobTitle = model.JobTitle;
                //    empTypeInDb.RecordStatus = RecordStatus.Active;

                //    empTypeInDb.ReportingEmployeeId = model.ReportingEmployeeId;
                //    if (empTypeInDb.ReportingEmployeeId <= 0)
                //        empTypeInDb.ReportingEmployeeId = null;
                //}


                //emp.LocationId = model.LocationId;
                //emp.DepartmentId = model.DepartmentId;
                //emp.JobTitle = model.JobTitle;
                context.SaveChanges();

                if(emp.IsIncomplete)
                    return RedirectToAction(nameof(AddOrUpdatePayComponents), new { id = model.Id });

                return RedirectToAction(nameof(Change), new { id = model.Id });
                // return RedirectToAction(nameof(EmploymentAndJob), new { id = model.EmployeeId });
            }
            return BadRequest(ModelState);
        }


        public async Task<IActionResult> AddOrUpdatePayComponents(int id)
        {
            var emp = await context.Employees.FindAsync(id);
            if (emp == null) return BadRequest("Employee was not not found");
            ViewBag.Id = id;

            var adjustments = await companyService.GetPayAdjustments();
            if (adjustments.Any() == false)
                return ThrowJsonError("Oops! Company does not have pay components configured!");

            var empPayAdjustmentList = adjustments
                      .Select(ajusments => new EmployeePayComponent
                      {
                          EmployeeId = id,
                          Total = 0,
                          IsActive = false,
                          PayAdjustment = ajusments,
                          PayAdjustmentId = ajusments.Id
                      }).ToList();


            var oldrecords = context.EmployeePayComponents.Where(x => x.EmployeeId == id).ToList();

            foreach (var item in empPayAdjustmentList)
            {
                if (oldrecords.Any(x => x.PayAdjustmentId == item.PayAdjustmentId))
                {
                    // replcaing toal
                    item.IsActive = true;
                    item.Total = oldrecords.First(x => x.PayAdjustmentId == item.PayAdjustmentId).Total;
                }
            }

            emp.EmployeePayComponents = empPayAdjustmentList;
            return PartialView("_AddOrUpdatePayComponents", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdatePayComponents(Employee model)
        {
            var emp = await context.Employees.Include(t=> t.EmployeePayComponents).FirstOrDefaultAsync(t=> t.Id == model.Id);
            if (emp == null) ModelState.AddModelError("", "Employee was not found!");

            var isIcopmenet = emp.IsIncomplete;
            if(isIcopmenet)
            {
                if (!model.EmployeePayComponents.Any(x => x.IsActive))
                    return ThrowJsonError("Employee must be enrolled in atleast one paycomponent");

                if (emp.EmployeePayComponents.Any())
                    context.EmployeePayComponents.RemoveRange(emp.EmployeePayComponents);


                if (model.EmployeePayComponents.Any(x => x.IsActive))
                    await context.EmployeePayComponents.AddRangeAsync(model.EmployeePayComponents.ToList());

                emp.EmployeeStatus = EmployeeStatus.Active;
                emp.EmployeeSecondaryStatus = EmployeeSecondaryStatus.None;
                await context.SaveChangesAsync();
                return PartialView("_EmployeeCreatedUpdated", emp);
            }
            else
            {
                context.EmployeePayComponents.RemoveRange(emp.EmployeePayComponents);

                if (model.EmployeePayComponents.Any(x => x.IsActive))
                    emp.EmployeePayComponents.AddRange(model.EmployeePayComponents.Where(x => x.IsActive).ToList());

                await context.SaveChangesAsync();
                SetTempDataMessage($"{emp.GetSystemName(User)}'s pay components were jsut updated", MsgAlertType.success);
                return RedirectToAction(nameof(Change), new { emp.Id });
            }
        }


        public async Task<IActionResult> UpdateEmployment(int id)
        {
            var emp = await context.Employees.FindAsync(id);
            if (emp == null) return BadRequest("Employee was not not found");

            var vacantJobs = await companyService.GetJobs(JobStatus.Vacant, id);
            ViewBag.JobId = new SelectList(vacantJobs.ToDictionary(a=> a.Id,a=> $"{a.JobID} · {a.JobTitle}"), "Key", "Value", emp?.JobId);
            ViewBag.ReportingEmployeeId = await companyService.GetAllEmployeesInMyCompanyForDropdownOptGroups();


            return PartialView("_AddEmployment", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmployment(Employee model)
        {
            if (ModelState.IsValid)
            {
                var emp = await context.Employees
                .Where(x=> x.Id == model.Id)
                //.Include(a=> a.Individual)
                .Include(a=> a.ReportingEmployee)
                .FirstOrDefaultAsync();

                if (emp == null)
                    return ThrowJsonError();
                if (model.EmploymentType == EmployeeType.None)
                    return ThrowJsonError("Employment type cannot be none");

                if (model.ReportingEmployeeId == model.Id)
                    return ThrowJsonError("Employee cannot report to themselves");


                var job = await context.Jobs.Where(a => a.Id == model.JobId || a.Id != emp.JobId).Include(a=> a.JobPayComponents).FirstOrDefaultAsync();
                if (job == null || job.JobStatus != JobStatus.Vacant)
                    return ThrowJsonError("Job was not found or isn't vacant");


                // direct update for now
                if(emp.JobId != model.JobId){                    
                    emp.JobId = model.JobId;
                    emp.JobIDString = job.JobID;
                    emp.JobTitle= job.JobTitle;
                }

                if(emp.ReportingEmployeeId != model.ReportingEmployeeId)
                    emp.ReportingEmployeeId = model.ReportingEmployeeId;
                
                if(emp.DateOfJoined != model.DateOfJoined)
                    emp.DateOfJoined = model.DateOfJoined;


                if(emp.ContractStartDate != model.ContractStartDate && emp.ContractEndDate != model.ContractEndDate){
                    emp.ContractStartDate = model.ContractStartDate;
                    emp.ContractEndDate = model.ContractEndDate;
                }
                
                
                emp.WeeklyWorkingHours = model.WeeklyWorkingHours;
                emp.DailyWorkingHours = model.DailyWorkingHours;
                emp.EmployeeStatus = model.EmployeeStatus = EmployeeStatus.Active;
                emp.EmploymentType = model.EmploymentType;
                emp.EmploymentTypeOther = model.EmploymentTypeOther;

                if(job.JobPayComponents != null && model.UpdateFromJobPayComponents) {
                    model.EmployeePayComponents.AddRange(job.JobPayComponents.Select(a => new EmployeePayComponent { PayAdjustmentId = a.PayAdjustmentId, Total = a.Total, EmployeeId = model.Id }));
                }
                
                //emp.LocationId = model.LocationId;
                //emp.DepartmentId = model.DepartmentId;
                //emp.JobTitle = model.JobTitle;
                context.SaveChanges();
                return RedirectToAction(nameof(Change), new { id = model.Id });
                // return RedirectToAction(nameof(EmploymentAndJob), new { id = model.EmployeeId });
            }
            return BadRequest(ModelState);
        }


        public async Task<IActionResult> AddOrUpdateCompensation(int eId, int id = 0, string type="addition")
        {
            var emp = await context.Employees.FindAsync(eId);
            if (emp == null && id > 0) return BadRequest("Employee was not not found");

            var empPayComp = new EmployeePayComponent { EmployeeId = eId };
            if (id > 0)
            {
                var _type = await context.EmployeePayComponents.FirstOrDefaultAsync(x => x.Id == id && x.EmployeeId == eId);
                if (_type == null && id > 0) return BadRequest("Employee Pay Component was not not found");
                else
                    empPayComp = _type;
            }

            ViewBag.PayAdjustmentId = new SelectList(await companyService.GetPayAdjustments(type), "Id", "Name", empPayComp?.PayAdjustmentId);


            return PartialView("_AddOrUpdateCompensation", empPayComp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateCompensation(EmployeePayComponent model)
        {
            if (ModelState.IsValid)
            {
                var emp = await context.Employees.FindAsync(model.EmployeeId);
                if (emp == null)
                    return ThrowJsonError();

                if (model.PayAdjustmentId <= 0)
                    return ThrowJsonError("Please choose any pay adjustment");

                if (model.Id <= 0)
                {
                    context.EmployeePayComponents.Add(model);
                }
                else
                {
                    // update
                    var empTypeInDb = await context.EmployeePayComponents.FindAsync(model.Id);

                }
                
                context.SaveChanges();
                return RedirectToAction(nameof(Compensation), new { id = model.EmployeeId });
            }
            return BadRequest(ModelState);
        }


        public async Task<IActionResult> AddDeductions(int id)
        {
            if (!context.Employees.Any(x => x.Id == id)) return RedirectToAction(nameof(Index));

            var empPayAdjustmentList = (await companyService.GetPayAdjustments("deduction"))
                      .Select(ajusments => new EmployeePayComponent
                      {
                          EmployeeId = id,
                          Total = 0,
                          PayAdjustment = ajusments,
                          PayAdjustmentId = ajusments.Id
                      }).ToList();


            var oldrecords = context.EmployeePayComponents.Where(x => x.EmployeeId == id).ToList();

            foreach (var item in empPayAdjustmentList)
            {
                if (oldrecords.Any(x => x.PayAdjustmentId == item.PayAdjustmentId))
                {
                    // replcaing toal
                    item.IsActive = true;
                }
            }

            return PartialView("_AddDeductions", empPayAdjustmentList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDeductions(List<EmployeePayComponent> model, DateTime date)
        {
            var first = model.FirstOrDefault();
            if (first == null)
                return RedirectToAction(nameof(Index));

            if (first == null) return BadRequest("Employee pay adjustment was not found");
            if (ModelState.IsValid)
            {
                var oldrecords = context.EmployeePayComponents.Where(x => x.EmployeeId == first.EmployeeId && x.PayAdjustment.VariationType.ToString().ToLower().Contains("deduction")).ToList();
                if (oldrecords.Count > 0)
                {
                    context.EmployeePayComponents.RemoveRange(oldrecords);
                }

                var newDed = model.Where(a => a.IsActive).ToList();
                newDed.ForEach(t => { t.EffectiveDate = date; });

                context.EmployeePayComponents.AddRange(newDed);
                context.SaveChanges();

                return RedirectToAction(nameof(Compensation), new { id = first.EmployeeId });
            }

            return BadRequest(ModelState);
        }

        #endregion


        public IActionResult MapUser(int id)
        {
            if (!context.Employees.Any(x => x.Id == id))
                return BadRequest("Employee was not found");

            var model = context.Employees.Find(id);

            ViewBag.UserIds = new MultiSelectList(accountDbContext.Users.Select(x => new { x.Id, Name = $"<img src='{Url.Content(x.Avatar ?? DefaultPictures.default_user)}' height='20px' /> {x.NameDisplay}" }).ToList(), "Id", "Name", model?.UserId);

            AppUser user = null;
            if(model.HasUserAccount)
                user = accountDbContext.Users.Find(model.UserId);
            else
            {
                user = new AppUser
                {
                    NickName = model.NickName,
                    Email = model.EmailWork,
                    PhoneNumber = model.PhoneWork,
                    SendOtpAndLoginFirst = true,
                    ChangePasswordOnLogin  = true
                };
            }

            return PartialView("_MapUser", new MapUserToEmployeeVm { Employee = model, EmployeeId = model.Id, HasMapping = model.HasUserAccount, UserId = model.UserId,  UserAvatar = user?.Avatar, UserName = user?.UserName, AppUser = user, IsAlreadyMapped = model.HasUserAccount && user != null  });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MapUser(MapUserToEmployeeVm model)
        {
            var emp = await context.Employees.FindAsync(model.EmployeeId);
            if (emp == null)
                return ThrowJsonError("Employee was not found");

            AppUser newUser = null;
            if (model.CreateNewUser)
            {
                if (string.IsNullOrWhiteSpace(model.AppUser.Email) ||
                    string.IsNullOrWhiteSpace(model.AppUser.PhoneNumber))
                    return ThrowJsonError("Please fill in all the field");

                newUser = new AppUser
                {
                    UserName = model.AppUser.Email,
                    Email = model.AppUser.Email,
                    PhoneNumber = model.AppUser.PhoneNumber,
                    NickName = model.AppUser.NickName,
                    Avatar = emp.Avatar,
                    CompanyAccountId = userResolverService.GetCompanyId(),
                    ChangePasswordOnLogin = true,
                    SendOtpAndLoginFirst  = true,
                    UserType = UserType.Company,
                    AccessGrants = new List<AccessGrant>
                    {
                        new AccessGrant
                        {
                            CompanyAccountId = userResolverService.GetCompanyId(),
                            Roles = Roles.Company.all_employees,
                            Status = AccessGrantStatus.Active,
                        }
                    }
                };

                var result = await userManager.CreateAsync(newUser);
                if (!result.Succeeded)
                    return ThrowJsonError(result.Errors.FirstOrDefault()?.Description);
            }


            emp.UserId = model.CreateNewUser ? newUser.Id : model.UserId;

            var userInDb = await userManager.FindByIdAsync(emp.UserId);
            emp.UserName = userInDb.UserName;
            emp.UserPicture = userInDb.Avatar;
            emp.HasUserAccount = true;

            await context.SaveChangesAsync();

            //empInDb.IdentityType = model.IdentityType;
            //empInDb.IdentityNumber = model.IdentityNumber;
            //empInDb.BankAccountName = model.BankAccountName;
            //empInDb.BankAccountNumber = model.BankAccountNumber;
            //empInDb.BankName = model.BankName;
            //empInDb.Street = model.Street;
            //empInDb.Address = model.Address;
            //empInDb.ZipCode = model.ZipCode;

            //context.Employees.Update(empInDb);
            //context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UnMapUser(int id)
        {
            if (!await context.Employees.AnyAsync(x => x.Id == id))
                return BadRequest("Employee was not found");

            var emp = await context.Employees.FirstOrDefaultAsync(x => x.Id == id);
            if (emp.HasUserAccount)
            { emp.HasUserAccount = false; emp.UserId = ""; emp.UserName = ""; emp.UserPicture = ""; }

            await context.SaveChangesAsync();
            return Ok("Un-Mapped");
        }


        [HttpPost]
        public IActionResult Remove(int id)
        {
            if (ModelState.IsValid)
            {
                var add = context.Employees
                    .FirstOrDefault(x => x.Id == id);
                if (add == null)
                    return BadRequest("Oooh! we didnt find that one");
                //if(context.PayrollPeriodPayAdjustments.Any(x=> x.PayAdjustmentId == payAdjustmentId))
                //    return BadRequest("Ouch! Some items are used as children, please remove them before proceed");
                
                context.Employees.Remove(add);
                context.SaveChanges();
                return RedirectToAction("Index", "Employee");
            }

            return BadRequest();
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

    }

}

