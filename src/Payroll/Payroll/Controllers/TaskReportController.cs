using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using Microsoft.AspNetCore.Http;
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
    public class TaskReportController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly AccountDbContext accountDbContext;
        private readonly CompanyService companyService;
        private readonly UserResolverService userResolverService;
        private readonly EmployeeService employeeService;
        private readonly ScheduleService scheduleService;
        private readonly FileUploadService fileUploadService;

        public TaskReportController(PayrollDbContext context, AccountDbContext accountDbContext, CompanyService companyService, UserResolverService userResolverService, EmployeeService employeeService, ScheduleService scheduleService, FileUploadService fileUploadService)
        {
            this.context = context;
            this.accountDbContext = accountDbContext;
            this.companyService = companyService;
            this.userResolverService = userResolverService;
            this.employeeService = employeeService;
            this.scheduleService = scheduleService;
            this.fileUploadService = fileUploadService;
        }
        
        public async Task<IActionResult> Index(int id = 0, int limit = 10, TaskReportType? type = null)
        {
            var recentSchedules = await accountDbContext.TaskRunReports.Where(x => 
                //&& (date == null || (date.Value >= x.StartDate && date.Value <= x.EndDate))
                (type== null || x.TaskReportType == type))
                .OrderByDescending(x => EF.Property<DateTime>(x, AuditFileds.CreatedDate))
                .Take(limit)
                .ToListAsync();
            
            ViewBag.limit = limit;
            ViewBag.type = (int)(type ?? 0);

            return View(recentSchedules);
        }
        
        
        public async Task<IActionResult> ViewReport(int id)
        {
            if (!accountDbContext.TaskRunReports.Any(x => x.Id == id)) return ThrowJsonError("Report was not found!");

            var report = await accountDbContext.TaskRunReports.FindAsync(id);
            return PartialView("_ViewReport", report);
        }
        
    }
}
