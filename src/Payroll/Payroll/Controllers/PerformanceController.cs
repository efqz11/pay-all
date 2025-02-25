using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;

namespace Payroll.Controllers
{
    public class PerformanceController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly AccountDbContext accountDbContext;
        private readonly CompanyService companyService;
        private readonly UserResolverService userResolverService;
        private readonly EmployeeService employeeService;
        private readonly ScheduleService scheduleService;
        private readonly FileUploadService fileUploadService;
        private readonly AccessGrantService accessGrantService;
        private readonly UserManager<AppUser> userManager;
        private readonly PayAdjustmentService payAdjustmentService;
        private readonly PayrollService payrollService;

        public PerformanceController(PayrollDbContext context, AccountDbContext accountDbContext, CompanyService companyService, UserResolverService userResolverService, EmployeeService employeeService, ScheduleService scheduleService, FileUploadService fileUploadService, AccessGrantService accessGrantService, UserManager<AppUser> userManager, PayAdjustmentService payAdjustmentService, PayrollService payrollService)
        {
            this.context = context;
            this.accountDbContext = accountDbContext;
            this.companyService = companyService;
            this.userResolverService = userResolverService;
            this.employeeService = employeeService;
            this.scheduleService = scheduleService;
            this.fileUploadService = fileUploadService;
            this.accessGrantService = accessGrantService;
            this.userManager = userManager;
            this.payAdjustmentService = payAdjustmentService;
            this.payrollService = payrollService;
        }

        public async Task<IActionResult> Index(int id = 0, int limit = 10, int empId = 0, int desgId = 0, ContractType? type = null, ContractSort? sort = null, int page = 1)
        {
            //ViewBag.DesgnationIds = new SelectList(await companyService.GetDesignationDropdown(), "Id", "Name", desgId);
            ViewBag.EmployeeIds = await employeeService.GetAllEmployeesInMyCompanyForDropdownOptGroups();

            var payrolPeriodEmplsCount =  context.PayrollPeriodEmployees.Where(x => x.PayrollPeriod.CompanyId == userResolverService.GetCompanyId()
                //&& (date == null || (date.Value >= x.StartDate && date.Value <= x.EndDate))
                && (empId == 0 || x.EmployeeId == empId));
                //&& (desgId == 0 || x.JobTitleId == desgId)
                //&& (type == null || type == 0 || x.ContractType == type));

            //switch (sort)
            //{
            //    case ContractSort.Expiring:
            //        recentSchedules = recentSchedules
            //            .OrderBy(c => (c.EndDate - DateTime.UtcNow).TotalDays);
            //        break;
            //    case ContractSort.Recent:
            //        recentSchedules = recentSchedules
            //            .OrderBy(c => (DateTime.UtcNow - c.StartDate).TotalDays);
            //        break;
            //    case ContractSort.Short:
            //        recentSchedules = recentSchedules
            //            .OrderBy(c => (c.EndDate - c.StartDate).TotalDays);
            //        break;
            //    case ContractSort.Long:
            //        recentSchedules = recentSchedules
            //            .OrderByDescending(c => (c.EndDate - c.StartDate).TotalDays);
            //        break;
            //    default:
            //        break;
            //}


            var watch = new Stopwatch();
            watch.Start();

            var rec = await  payrolPeriodEmplsCount
                // .OrderByDescending(x => EF.Property<DateTime>(x, AuditFileds.CreatedDate))
                .Skip((page - 1)* limit)
                .Take(limit)
                //.Include(a => a.JobTitle)
                .Include(a => a.Employee)
                    .ThenInclude(a => a.Department)
                //.Include(a=> a.EmployeeIds)
                //.Include(a=> a.WorkTimeIds)
                .OrderByDescending(a=> a.PayrollPeriod.StartDate)
                .ToListAsync();

            ViewBag.empId = empId;
            ViewBag.limit = limit;
            ViewBag.type = (int)(type ?? 0);
            ViewBag.desgId = desgId;
            ViewBag.sort = sort;
            if (Request.IsAjaxRequest())
            {
                watch.Stop();
                return PartialView("Index_Listing", rec);
            }
            else
            {
                ViewBag.Count = await payrolPeriodEmplsCount.CountAsync();
                watch.Stop();
                ViewBag.TimeSec = watch.Elapsed.TotalSeconds.ToString("N2");
            }

            return View(rec);
        }

        public async Task<IActionResult> Employee(int id)
        {
            var empls = await context.PayrollPeriodEmployees.Where(x => x.PayrollPeriod.CompanyId == userResolverService.GetCompanyId() && x.EmployeeId == id)
                .Include(x=> x.PayrollPeriod)
                .OrderByDescending(a => a.PayrollPeriod.StartDate)
                .ToListAsync();

            ViewBag.Employee = await context.Employees
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x=> x.Id == id);
            return View(empls);
        }

        /// <summary>
        /// Display employee performance for current period
        /// :: Real=time calculation of values
        /// </summary>
        /// <param name="emp"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetChartForActivePeriod(int empId)
        {
            var dates = await companyService.GetPayrollDates(DateTime.UtcNow);

            var chartData = await payrollService.GetEmployeeInteraction2DByDateBetweenPeriod(
                companyId: userResolverService.GetCompanyId(),
                startDate: dates.Item1,
                endDate: dates.Item2,
                empIds: new[] { empId });

            return Json(new { data = chartData });
        }

        /// <summary>
        /// Display employee performance for single period
        /// :: from saved payrol period employee (table)
        /// </summary>
        /// <param name="emp"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetChartForSinglePeriod(int ppempId)
        {
            var chartData = await context.PayrollPeriodEmployees.Where(x => x.Id == ppempId)
                .Where(x=> x.ChartDataX != null)
                .SelectMany(x => x.ChartDataX)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return Json(new { data = chartData });
        }

        /// <summary>
        /// Get overall employee performance (breakdown by pay period)
        /// :: from saved payrol period employee (table)
        /// </summary>
        /// <param name="emp"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetChartForEmployee(int emp)
        {
            var chartData = await context.PayrollPeriodEmployees.Where(x => x.PayrollPeriod.CompanyId == userResolverService.GetCompanyId() && x.EmployeeId == emp && x.ChartDataX != null)
                .Select(x => new { Id = x.Id, Chart = x.ChartDataX.OrderBy(a => a.Date), StartDate = x.PayrollPeriod.StartDate })
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();


            //var payrolsIncluded = context.PayrollPeriodEmployees.Where(x => x.PayrollPeriod.CompanyId == userResolverService.GetCompanyId() && x.EmployeeId == emp)
            //    .Select(x => x.PayrollPeriod).Distinct()
            //    .OrderBy(x => x.StartDate)
            //    .ToList();


            return Json(new { data = chartData });
        }



        public async Task<IActionResult> NewContractEmployee()
        {
            ViewBag.EmployeeIds = await employeeService.GetAllEmployeesInMyCompanyForDropdownOptGroups();
            return PartialView("_NewContractEmployee");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> NewContractEmployee(int empId)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        return RedirectToAction(nameof(AddOrUpdateContractTerms), new { id = empId });
        //    }

        //    return ThrowJsonError(ModelState);
        //}


        //public async Task<IActionResult> Detail(int id)
        //{
        //    Contract contract = await GetContractInDb(id);
        //    if (contract == null) return ThrowJsonError("Contract was not found!");

        //    if (contract.Employee.HasUserAccount)
        //        ViewBag.Access = await accessGrantService.GetContractEmployeeAccessAsync(contract.Employee.UserId);

        //    ViewBag.ContractId = id;
        //    ViewBag.WorkTimes = await companyService.GetWorkTimes();
        //    return View(contract);
        //}


    }

}
