using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;

namespace Payroll.Controllers
{
    public class ScheduledSystemTaskController : BaseController
    {
        private readonly IHubContext<SignalServer> hubContext;
        private readonly NotificationService notificationService;
        private readonly AccessGrantService accessGrantService;
        private readonly ScheduledSystemTaskService scheduledSystemTaskService;
        private readonly EventLogService eventLogService;
        private readonly AccountDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<AppRole> roleManager;
        private readonly SignInManager<AppUser> _SignInManager;
        private readonly ILogger<AppUserController> logger;
        private readonly UserResolverService _userResolverService;
        private readonly CompanyService companyService;
        private readonly PayrollDbContext payrollDbContext;

        public ScheduledSystemTaskController(AccountDbContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> _signInManager, ILogger<AppUserController> logger, UserResolverService userResolverService, CompanyService companyService, PayrollDbContext payrollDbContext, IHubContext<SignalServer> hubContext, NotificationService notificationService, AccessGrantService accessGrantService, ScheduledSystemTaskService scheduledSystemTaskService, EventLogService eventLogService)
        {
            this.hubContext = hubContext;
            this.notificationService = notificationService;
            this.accessGrantService = accessGrantService;
            this.scheduledSystemTaskService = scheduledSystemTaskService;
            this.eventLogService = eventLogService;
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _SignInManager = _signInManager;
            this.logger = logger;
            this._userResolverService = userResolverService;
            this.companyService = companyService;
            this.payrollDbContext = payrollDbContext;
        }


        public async Task<IActionResult> Index()
        {
            var jobs = await context.ScheduledSystemTasks.ToListAsync();
            foreach (var item in jobs)
            {
                if (item.IsActive)
                {
                    RecurringJob.AddOrUpdate(item.HangfireIdentifier, () => scheduledSystemTaskService.DoScheduledSystemTask(item.Id), item.CronExpression);
                }
                else
                {
                    RecurringJob.RemoveIfExists(item.HangfireIdentifier);
                }
            }
            return View(await context.ScheduledSystemTasks.ToListAsync());
        }

        public async Task<IActionResult> Events(int id, DateTime? start = null, DateTime? end = null, int limit = 10, int page = 1)
        {
            var task = await context.ScheduledSystemTasks.FindAsync(id);
            if (task == null)
                return BadRequest();

            ViewBag.SystemTask = task;
            var data = await eventLogService.GetEventsAsync(EventLogTypes.EXECUTE_SCHEDULED_TASK, id.ToString(), start, end, limit, page);

            if (start.HasValue && end.HasValue)
            {
                ViewBag.duration = start.GetDuration(end, _userResolverService.GetClaimsPrincipal());
                ViewBag.Start = start;
                ViewBag.End = end;
            }
            ViewBag.limit = limit;

            return View(data);
        }

        public async Task<IActionResult> GetEventActionSummary(int id)
        {
            var data = await eventLogService.GetEventAsync(id);
            return PartialView("_ViewEventActionSummary", data);
        }
    }
}