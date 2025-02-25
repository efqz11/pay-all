using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
    public class NotificationController : BaseController
    {
        private readonly IHubContext<SignalServer> hubContext;
        private readonly NotificationService notificationService;
        private readonly AccessGrantService accessGrantService;
        private readonly AccountDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<AppRole> roleManager;
        private readonly SignInManager<AppUser> _SignInManager;
        private readonly ILogger<AppUserController> logger;
        private readonly UserResolverService _userResolverService;
        private readonly CompanyService companyService;
        private readonly PayrollDbContext payrollDbContext;

        public NotificationController(AccountDbContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> _signInManager, ILogger<AppUserController> logger, UserResolverService userResolverService, CompanyService companyService, PayrollDbContext payrollDbContext, IHubContext<SignalServer> hubContext, NotificationService notificationService, AccessGrantService accessGrantService)
        {
            this.hubContext = hubContext;
            this.notificationService = notificationService;
            this.accessGrantService = accessGrantService;
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _SignInManager = _signInManager;
            this.logger = logger;
            this._userResolverService = userResolverService;
            this.companyService = companyService;
            this.payrollDbContext = payrollDbContext;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = _userResolverService.GetUserId();
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ThrowJsonError("User not found");
            var userRoles = await userManager.GetRolesAsync(user);
            var agRoles = await accessGrantService.GetMyAccessGrantRolesAsync();


            var notificaitons = await context.Notifications
                .Where(a => (a.ActionTakenUserId == userId || 

                (a.NotificationType.NotificationLevel == UserType.PayAll && a.NotificationType.UsersWithRoles != null && userRoles != null && a.NotificationType.UsersWithRoles.Intersect(userRoles).Count() == a.NotificationType.UsersWithRoles.Count()) || 
                
                (a.NotificationType.NotificationLevel == UserType.Company && a.NotificationType.UsersWithRoles != null && a.NotificationType.UsersWithRoles.Intersect(agRoles).Count() == a.NotificationType.UsersWithRoles.Count())) && a.IsRead == false)

                .OrderByDescending(a => EF.Property<DateTime>(a, AuditFileds.CreatedDate))
                .Take(10)
                .Include(a => a.CompanyAccount)
                .Include(a => a.NotificationType)
                .Include(a => a.User)
                .ToListAsync();

            return PartialView("_ViewNotifications", notificaitons);
        }

        public async Task<IActionResult> GetUserNotifications(string id, int page = 1, int limit = 10)
        {
            var data = await notificationService.GetUserNotifications(id, page: page, limit: limit);

            ViewBag.ShowDetails = true;
            return PartialView("_Listing", data);
        }

        public async Task<IActionResult> GetCompanyNotifications(int id, int page = 1, int limit = 10)
        {
            var data = await context.Notifications
                .Where(a => a.CompanyAccountId == id)
                .OrderByDescending(a => EF.Property<DateTime>(a, AuditFileds.CreatedDate))
                 .Skip((page - 1) * limit)
                 .Take(limit)
                .Include(a => a.CompanyAccount)
                .Include(a => a.NotificationType)
                .Include(a => a.User)
                .ToListAsync();

            ViewBag.ShowDetails = true;
            return PartialView("_Listing", data);
        }

        public async Task<IActionResult> TakeAction(int id, string from = "")
        {
            var notification = await notificationService.GetNotificationAsync(id);
            if (notification == null)
                return ThrowJsonError("Notification was not found!");
            if (notification.IsRead)
            {
                SetTempDataMessage("Attention! This notification is read and action taken. You may view action taken and remarks.", MsgAlertType.warning);
                return PartialView("_ViewNotificationSummary", notification);
            }

            var result = await notificationService.CanTakeAction(notification);

            

            if (result.status == false)
                return ThrowJsonError(result.errorMessage);

            ViewBag.Back = true;
            return PartialView("_TakeAction", notification);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeAction(Notification model)
        {
            var notification = await notificationService.GetNotificationAsync(model.Id);
            if (notification == null)
                return ThrowJsonError("Notification was not found!");

            if (!model.IgnoreChecks)
            {
                var result = await notificationService.CanTakeAction(notification);

                if (result.status == false)
                    return ThrowJsonError(result.errorMessage);
            }


            try
            {
                notification.NotificationActionTakenType = model.NotificationActionTakenType;
                notification.Remarks = model.Remarks;
                await notificationService.TakeActionAsync(notification);
                //return ThrowJsonSuccess();
            }
            catch (ApplicationException ex)
            {
                return ThrowJsonError(ex.Message);
            }
                

            return RedirectToAction(nameof(TakeAction), new { id = notification.Id });
        }


        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await notificationService.GetNotificationAsync(id);
            if (notification == null)
                return ThrowJsonError("Notification was not found!");

            var result = await notificationService.CanTakeAction(notification);

            if (result.status == false)
                return ThrowJsonError(result.errorMessage);

            try
            {
                await notificationService.TakeActionAsync(notification);
                return ThrowJsonSuccess();
            }
            catch (ApplicationException ex)
            {
                return ThrowJsonError(ex.Message);
            }

            return ThrowJsonError("Oops! Your notification cannot be marked as read");
        }
        

        public async Task<IActionResult> ViewActionSummary(int id, int newId)
        {
            var notification = await notificationService.GetNotificationAsync(id);
            if (notification == null)
                return ThrowJsonError("Notification was not found!");

            var NewNotification = await notificationService.GetNotificationAsync(newId);

            ViewBag.NewNotification = NewNotification;
            return PartialView("_ViewNotificationSummary", notification);
        }

        public async Task<IActionResult> ViewNotificationType(int id)
        {
            var item = await context.NotificationTypes.FindAsync(id);
            if (item == null) return ThrowJsonError("Notification type not found");


            return PartialView("_ViewNotificationType", item);
        }


        public async Task<IActionResult> NotificationApproved(int id)
        {
            var interaction = await context.Notifications.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (interaction.IsRead)
                return ThrowJsonError();

            interaction.NotificationActionTakenType = NotificationActionTakenType.Approved;
            interaction.IgnoreChecks = true;
            await TakeAction(interaction);

            return Ok("Marked as Recieved");
        }


        [HttpDelete]
        public async Task<IActionResult> Remove(int id)
        {
            await notificationService.RemoveNotificationAsync(id);
            return Ok();
        }
    }
}