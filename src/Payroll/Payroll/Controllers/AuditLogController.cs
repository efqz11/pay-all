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
using Newtonsoft.Json;

namespace Payroll.Controllers
{
    [Authorize]
    public class AuditLogController : BaseController
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

        public AuditLogController(PayrollDbContext context, PayrollService payrollService, PayAdjustmentService payAdjustmentService, IHostingEnvironment hostingEnvironment, AccessGrantService accessGrantService, UserResolverService userResolverService, CompanyService companyService, EmployeeService employeeService, ScheduleService scheduleService, Hangfire.IBackgroundJobClient backgroundJobClient, AccountDbContext accountDbContext, AuditLogService auditLogService)
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
        
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string keyId, string modal, DateTime? start = null, DateTime? end = null, int limit = 10)
        {
            ViewBag.Modals = new SelectList(await auditLogService.GetAuditableEntityDropdown(), modal);

            ViewBag.Count = 0;
            if (string.IsNullOrWhiteSpace(keyId))
                return View();

            
            var data = await auditLogService.GetAuditLogs(keyId, modal, start, end, limit);
            ViewBag.Count = data.Item1;

            ViewBag.CurrentRangeDisplay = start.GetDuration(end, userResolverService.GetClaimsPrincipal());
            ViewBag.Start = start;
            ViewBag.End = end;
            ViewBag.KeyId = keyId;
            ViewBag.limit = limit;
            return View(data.Item2);
        }


        public async Task<IActionResult> ViewSummary(string keyId, string modal)
        {
            ViewBag.Modals = new SelectList(await auditLogService.GetAuditableEntityDropdown(), modal);
            
            var data = await auditLogService.GetAuditLogs(keyId, modal, null, null, 5);
            ViewBag.Count = data.Item1;
            ViewBag.Modal = modal;
            ViewBag.KeyId = keyId;
            
            return PartialView("_ViewSummary", data.Item2);
        }

        public async Task<IActionResult> ViewChangedColumns(int id)
        {
            var data = await auditLogService.GetAuditLog(id);
            if (data == null)
                return ThrowJsonError("Log was not found!");

            ViewBag.NewValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.NewValues);
            ViewBag.OldValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.OldValues);
            ViewBag.ChangedColumns = JsonConvert.DeserializeObject<List<string>>(data.ChangedColumns);
            return PartialView("_ViewChangedColumns", data);
        }
    }
}
