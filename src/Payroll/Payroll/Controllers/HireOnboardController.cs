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
    public class HireOnboardController : BaseController
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

        public HireOnboardController(PayrollDbContext context, IBackgroundJobClient backgroundJobClient, EmployeeService employeeService, AccessGrantService accessGrantService, UserResolverService userResolverService, FileUploadService fileUploadService, AccountDbContext accountDbContext, UserManager<AppUser> userManager, CompanyService companyService, PayrollService payrollService, ScheduleService scheduleService, NotificationService notificationService, RequestService requestService)
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
             (x.EmployeeStatus == EmployeeStatus.Incomplete || x.EmployeeStatus == EmployeeStatus.ActionNeeded) &&
             (dept == 0 || dept == x.DepartmentId))

             .OrderBy(x => x.Department.DisplayOrder)
             .ThenBy(x=> x.EmpID)
             .Skip((page - 1) * limit)
             .Take(limit)
             .Include(x => x.Department)
             .ToListAsync();
            ViewBag.Id = comapnyId;
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
        public async Task<IActionResult> Detail(int id)
        {
            var emp = await context.Employees.FindAsync(id);
            if (emp == null)
                return ThrowJsonError();

            if (emp.IsSelfOnBoarding)
                return View("_SelfOnBoarding", emp);

            //var comapnyId = userResolverService.GetCompanyId();

            //ViewBag.DepartmentIds = new SelectList(await context.Departments.Where(x => x.CompanyId == comapnyId).OrderBy(x => x.DisplayOrder).ToListAsync(), "Id", "Name", dept);
            //ViewBag.Count = await context.Employees
            // .CountAsync(x => x.HrStatus == HrStatus.Employed && x.CompanyId == comapnyId);

            return View();
        }

        public async Task<IActionResult> ContinueSelfOnboard()
        {
            var emp = await context.Employees.FindAsync(userResolverService.GetEmployeeId());
            if (emp == null)
                return ThrowJsonError();

            if (!emp.IsSelfOnBoarding)
                return ThrowJsonError("Employee needs to be self-onboarding");

            //var comapnyId = userResolverService.GetCompanyId();

            //ViewBag.DepartmentIds = new SelectList(await context.Departments.Where(x => x.CompanyId == comapnyId).OrderBy(x => x.DisplayOrder).ToListAsync(), "Id", "Name", dept);
            //ViewBag.Count = await context.Employees
            // .CountAsync(x => x.HrStatus == HrStatus.Employed && x.CompanyId == comapnyId);

            return View("UpdateEmployeeInfo", emp);
        }
    }

}

