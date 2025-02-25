using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Payroll.Models;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Services
{
    public class NotificationService
    {
        private readonly Database.PayrollDbContext payrollDbContext;

        //private readonly Database.PayrollDbContext context;
        private readonly UserResolverService userResolverService;
        private readonly UserManager<AppUser> userManager;
        private readonly Database.AccountDbContext context;
        private readonly AccessGrantService accessGrantService;
        private readonly EmployeeService employeeService;

        public NotificationService(Payroll.Database.PayrollDbContext payrolDbContext, UserResolverService userResolverService,
            UserManager<AppUser> userManager, Database.AccountDbContext context, AccessGrantService accessGrantService, EmployeeService employeeService)
        {
            this.payrollDbContext = payrolDbContext;
            //this.context = payrolDbContext;
            this.userResolverService = userResolverService;
            this.userManager = userManager;
            this.context = context;
            this.accessGrantService = accessGrantService;
            this.employeeService = employeeService;
        }

        public async Task<List<Notification>> GetUserNotifications(string id, DateTime? start = null, DateTime? end = null, bool showSeen = false, int type = 0, int page = 1, int limit = 10)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) throw new Exception("User not found");
            var userRoles = await userManager.GetRolesAsync(user);

            var q = context.Notifications
                .Where(a => a.CompanyAccountId == userResolverService.GetCompanyId() && (a.ActionTakenUserId == id || (a.NotificationType.UsersWithRoles != null && userRoles != null && a.NotificationType.UsersWithRoles.Intersect(userRoles).Count() == a.NotificationType.UsersWithRoles.Count())));
            if (type > 0)
                q = q.Where(a => a.NotificationTypeId == type);
            if (showSeen)
                q = q.Where(a => a.IsRead);
            if (start.HasValue && end.HasValue)
                q = q.Where(a => a.SentDate >= start.Value && a.SentDate <= end.Value);

            return await q
                .OrderByDescending(a => EF.Property<DateTime>(a, AuditFileds.CreatedDate))
                 .Skip((page - 1) * limit)
                 .Take(limit)
                .Include(a => a.CompanyAccount)
                .Include(a => a.NotificationType)
                .Include(a => a.User)
                .ToListAsync(); 
        }
        public async Task<List<Notification>> GetEmployeeNotifications(int id, DateTime? start = null, DateTime? end = null, bool showSeen = false, int type = 0, int page = 1, int limit = 10)
        {
            var emp = await employeeService.GetEmployeeById(id);
            if (emp == null) throw new Exception("Employee not found");

            var roleIds = emp.EmployeeRoles.Select(a => a.EmployeeRoleId).ToArray();
            var userRoles = userResolverService.GetEmployeeRoles(); //  await payrollDbContext.EmployeeRoles.Where(a=>  roleIds.Contains(a.Id) && a.CompanyId == emp.CompanyId).Select(a=> a.Role).ToListAsync();
            var empId = userResolverService.GetEmployeeId();

            var _q = context.Notifications
                .Where(a => a.CompanyAccountId == userResolverService.GetCompanyId());
            if (type > 0)
                _q = _q.Where(a => a.NotificationTypeId == type);
            //if (showSeen)
                _q = _q.Where(a => a.IsRead == showSeen);
            if (start.HasValue && end.HasValue)
                _q = _q.Where(a => a.SentDate >= start.Value && a.SentDate <= end.Value);



            var q = await _q
                .OrderByDescending(a => EF.Property<DateTime>(a, AuditFileds.CreatedDate))
                .Include(a => a.CompanyAccount)
                .Include(a => a.NotificationType)
                .Include(a => a.User)
                .ToListAsync();

            return  q
                .Where(a => a.CompanyAccountId == userResolverService.GetCompanyId() && (a.EmployeeId == empId || (a.EmployeesWithRoles != null && userRoles != null && a.EmployeesWithRoles.Intersect(userRoles).Count() == a.EmployeesWithRoles.Count())))
                
                 .Skip((page - 1) * limit)
                 .Take(limit)
                .ToList();
        }

        private async Task<Notification> GenerateNotificationAsync(NotificationType notiType, object obj, Notification parentNotification = null)
        {
            var fieldValues = obj.GetType().GetProperties()
                .ToDictionary(x => x.Name, x => x.GetValue(obj, null)?.ToString());
            //try
            {
                if (obj.GetType() == typeof(Request))
                {
                    fieldValues.Add("GetRequestedDuration", (string)obj.GetType().GetMethod("GetRequestedDuration").Invoke(obj, new object[] { }));
                    // fieldValues.Add("DataChangesCount", (((Request)obj).RequestDataChanges?.Count() ?? 0).ToString());
                    fieldValues.Add("DayOffName", ((Request)obj).DayOff?.Name);
                    fieldValues.Add("Start.ToSystemFormat", ((Request)obj).Start.ToSystemFormat(userResolverService.GetClaimsPrincipal()));
                    fieldValues.Add("End.ToSystemFormat", ((Request)obj).End.ToSystemFormat(userResolverService.GetClaimsPrincipal()));
                    //fieldValues.Add("TotalDays", ((Request)obj).TotalDays);
                }

                fieldValues.Add(nameof(AuditFileds.CreatedByName), GetShadowFieldFromModal(obj, AuditFileds.CreatedByName));
                var empField = obj.GetType().GetProperty(nameof(Employee)).GetValue(obj, null);
                    // empField = obj.GetType().GetProperty(nameof(Employee)).;
                if (empField != null) {
                    var empFieldValues = empField.GetType().GetProperties()
                        .ToDictionary(x => x.Name, x => x.GetValue(empField, null)?.ToString());


                    fieldValues.Add(nameof(Employee) + ".GetSystemName", (string)empField.GetType().GetMethod("GetSystemName").Invoke(empField, new object[] { userResolverService.GetClaimsPrincipal() }));

                    foreach (var item in empFieldValues)
                        fieldValues.Add(nameof(Employee) + "." + item.Key, item.Value);
                }
            }
            //catch (Exception)
            //{
            //}

            // add Approve / Reject to fieldvalue as it is used in summary
            if (parentNotification != null)
                fieldValues.Add(nameof(parentNotification.NotificationActionTakenType), parentNotification.NotificationActionTakenType.ToString());

            var avatar = obj.GetType().GetProperties()
                .FirstOrDefault(x => Attribute.IsDefined(x, typeof(Filters.NotificaitonAvatarAttribute)))
                ?.GetValue(obj, null)?.ToString() ?? "";

            string _id = "";
            fieldValues.TryGetValue("Id", out _id);


            var nn = new Notification
            {
                NotificationType = notiType,
                EntityId = _id,
                CompanyAccountId = userResolverService.GetCompanyId(),
                Url = notiType.UrlWithPlaceholder.StringFormat(fieldValues),
                IsXhrRequest = notiType.IsXhrRequest,
                Summary = notiType.SummaryTextWithPlaceholder.StringFormat(fieldValues),
                Avatar = avatar,
                SentDate = DateTime.UtcNow
            };

            try
            {
                var empId = (int?)obj.GetType().GetProperty("EmployeeId").GetValue(obj, null);
                if (empId.HasValue)
                {
                    var emp = await employeeService.GetEmployeeById(empId.Value);
                    nn.RequestingEmployeeId = emp.Id;
                    nn.RequestingEmployeeAvatar = emp.Avatar;
                    nn.RequestingEmployeeName = emp.GetSystemName(userResolverService.GetClaimsPrincipal());
                }
            }
            catch (Exception)
            {

                throw;
            }

            if (parentNotification != null)
            {
                nn.ParentNotification = parentNotification;
                    fieldValues.Add("RejectedReason", parentNotification.Remarks);

                // override summary text (if parent notifcation and action is action taken && Rejected)
                if(notiType.Id == NotificationTypeConstants.CompanyActionTaken)
                {
                    if (parentNotification.NotificationActionTakenType == NotificationActionTakenType.Rejected)
                        nn.Summary += ". " + parentNotification.Remarks;
                }
                else if(notiType.Id == NotificationTypeConstants.RequestActionTaken)
                {
                    if (parentNotification.NotificationActionTakenType == NotificationActionTakenType.Rejected && !string.IsNullOrWhiteSpace(parentNotification.NotificationType.RejectedTextWithPlaceholder))
                        nn.Summary = parentNotification.NotificationType.RejectedTextWithPlaceholder?.StringFormat(fieldValues);
                    else if (!string.IsNullOrWhiteSpace(parentNotification.NotificationType.ApprovedTextWithPlaceholder))
                        nn.Summary = parentNotification.NotificationType.ApprovedTextWithPlaceholder.StringFormat(fieldValues);
                }
            }

            return nn;
        }

        public async Task<int> SendAsync(int type, object obj, int companyAccountId, Notification parentNotification = null)
        {
            var notiType = await context.NotificationTypes.FirstOrDefaultAsync(x => x.Id == type);
            if (notiType == null)
                return 0;


            var noti = await GenerateNotificationAsync(notiType, obj, parentNotification);
            noti.CompanyAccountId = companyAccountId;

            switch (notiType.NotificationReceivedBy)
            {
                case NotificationReceivedBy.UsersWithRoles:
                    break;
                case NotificationReceivedBy.SpecificUsers:
                    break;
                case NotificationReceivedBy.OwnerOfEntity:
                    noti.ActionTakenUserId = GetShadowFieldFromModal(obj);
                    noti.EmployeeId = (int?)obj.GetType().GetProperty("EmployeeId").GetValue(obj, null);
                    break;

                case NotificationReceivedBy.RequestApprovalConfig:
                    // if step == 0 and has transfer (substitute) employee => then first send to subs empl
                    var subsEMpl = (int?)obj.GetType().GetProperty("TransferredEmployeeId").GetValue(obj, null);
                    bool isNotifToSubstitutingEMpl = subsEMpl.HasValue && (parentNotification == null);

                    var step = ((parentNotification?.Step ?? 0) + 1);
                    var reqType = (RequestType)Enum.Parse(typeof(RequestType), obj.GetType().GetProperty(nameof(RequestType)).GetValue(obj, null)?.ToString());
                    var dayOffId = (int?)obj.GetType().GetProperty("DayOffId").GetValue(obj, null);
                    var empId = (int?)obj.GetType().GetProperty("EmployeeId").GetValue(obj, null);
                    var reqApprovalConfig = await GetRequestApprovalConfigs(noti.CompanyAccountId, reqType, dayOffId, step);
                    //if (!empId.HasValue)
                    //    throw new ApplicationException("Sorry! Requesting Employee was not found was not found");

                    //var emp = await employeeService.GetEmployeeById(empId.Value);


                    //noti.RequestingEmployeeId = emp.Id;
                    //noti.RequestingEmployeeAvatar = emp.Avatar;
                    //noti.RequestingEmployeeName = emp.GetSystemName(userResolverService.GetClaimsPrincipal());
                    noti.NotificationActionTypeEnum = EmployeeNotificationType.Approval;

                    if (isNotifToSubstitutingEMpl)
                    {
                        noti.ToBeReceivedBy = RequestProceessConfigActionBy.SpecificEmployee;
                        noti.Step = 0;
                        noti.EmployeeId = subsEMpl;
                    }
                    else if (reqApprovalConfig.Any())
                    {
                        noti.ToBeReceivedBy = reqApprovalConfig.First().RequestProceessConfigActionBy;
                        noti.Step = reqApprovalConfig.First().Step;

                        switch (noti.ToBeReceivedBy)
                        {
                            case RequestProceessConfigActionBy.EmployeesWithRole:
                                noti.EmployeesWithRoles = new[] { reqApprovalConfig.First().EmployeeRole.Role };
                                break;
                            case RequestProceessConfigActionBy.SpecificEmployee:
                                break;
                            case RequestProceessConfigActionBy.Supervisor:
                                noti.EmployeeId = await payrollDbContext.Employees.Where(a => a.Id == empId).Select(a => a.ReportingEmployeeId).FirstOrDefaultAsync();

                                // if (supervisor) null -> fallback to hr_manager role
                                if (!noti.EmployeeId.HasValue)
                                {
                                    noti.EmployeesWithRoles = new[] { Roles.Company.hr_manager };
                                    noti.ToBeReceivedBy = RequestProceessConfigActionBy.EmployeesWithRole;
                                    noti.SetFallbackSummary("Employee's supervisor was not found");
                                }
                                break;
                            case RequestProceessConfigActionBy.SupervisorsSupervisor:
                                noti.EmployeeId = await payrollDbContext.Employees.Where(a => a.Id == empId).Select(a => a.ReportingEmployee.ReportingEmployeeId).FirstOrDefaultAsync();
                                if (!noti.EmployeeId.HasValue)
                                {
                                    noti.EmployeesWithRoles = new[] { Roles.Company.hr_manager };
                                    noti.ToBeReceivedBy = RequestProceessConfigActionBy.EmployeesWithRole;
                                    noti.SetFallbackSummary("Employee's supervisor's supervisor was not found");
                                }
                                break;
                            case RequestProceessConfigActionBy.AutoActionAfterHours:
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        throw new Exception("Request approval config is not defined!");
                    }
                    break;
                default:
                    break;
            }

            context.Notifications.Add(noti);
            await context.SaveChangesAsync();
            return noti.Id;
        }

        public async Task<List<RequestApprovalConfig>> GetRequestApprovalConfigs(int cmpId, RequestType type, int? dayOffId = null, int? step = null)
        {
            return await payrollDbContext.RequestApprovalConfigs.Where(a => a.CompanyId == cmpId && a.RequestType == type &&
            (dayOffId == null || dayOffId == a.DayOffId) &&
            //a.DayOffId == (type == RequestType.Leave ? dayOffId : a.DayOffId) && 
            (step == null || step == a.Step))
                .OrderByDescending(a => a.Step)
                .Include(a=> a.EmployeeRole)
                .ToListAsync();
        }


        private string GetShadowFieldFromModal(object obj, string field = AuditFileds.CreatedById)
        {
            try
            {
                return context.Entry(obj).Property(field).CurrentValue.ToString();
            }
            catch (InvalidOperationException)
            {
                return payrollDbContext.Entry(obj).Property(field).CurrentValue.ToString();
            }
            catch(Exception)
            {
                throw new ApplicationException("Fatal error has occured");
            }
        }

        public async Task<bool> SendToEmployeesAsync(int type, object obj, int companyAccountId, int[] empIds)
        {
            if (!empIds.Any()) return false;

            var notiType = await context.NotificationTypes.FirstOrDefaultAsync(x => x.Id == type);
            if (notiType == null)
                return false;

            var mappedUserIds = await employeeService.GetMappedUserIdsAsync(empIds); 
            if (!mappedUserIds.Any()) return false;

            var notiList = new List<Notification>();
            var _noti = new Notification();
            foreach (var userId in mappedUserIds)
            {
                _noti = await GenerateNotificationAsync(notiType, obj);
                _noti.CompanyAccountId = companyAccountId;

                switch (notiType.NotificationReceivedBy)
                {
                    case NotificationReceivedBy.UsersWithRoles:
                        break;
                    case NotificationReceivedBy.SpecificUsers:
                        _noti.ActionTakenUserId = userId;
                        break;
                    case NotificationReceivedBy.OwnerOfEntity:
                        _noti.ActionTakenUserId = GetShadowFieldFromModal(obj);
                        break;
                    default:
                        break;
                }

                notiList.Add(_noti);
                _noti = null;
            }

            context.Notifications.AddRange(notiList);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TakeActionAsync(Notification model, bool isLastStep = false)
        {
            if (model.NotificationType.RequireApproveRejectAction == false && model.NotificationType.RequireReceivedNotes == false)
            {
                await MarkAsReadAsync(model);
                context.Notifications.Update(model);
                await context.SaveChangesAsync();
                return true;
            }

            if (model.NotificationActionTakenType == NotificationActionTakenType.Rejected && string.IsNullOrWhiteSpace(model.Remarks))
                throw new ApplicationException("Please enter reason for rejection in the remarks field provided!");

            var _newId = 0;

            if (!model.ReceivedDate.HasValue)
                model.ReceivedDate = DateTime.UtcNow;


            model.IsRead = true;
            model.ActionTakenUserId = userResolverService.GetUserId();
            model.ActionTakenEmployeeAvatar = userResolverService.Get(CustomClaimTypes.Profile);
            model.ActionTakenEmployeeName = userResolverService.Get(CustomClaimTypes.EmployeeName);
            model.ActionTakenEmployeeId = userResolverService.GetEmployeeId();
            model.ActionTakenDate = DateTime.UtcNow;
            model.Remarks = model.Remarks;

            if (model.NotificationType.RequireApproveRejectAction)
            {
                model.NotificationActionTakenType = model.NotificationActionTakenType;

                if (model.NotificationActionTakenType == NotificationActionTakenType.NoAction)
                    throw new ApplicationException("Whoops! There was no action taken!");


                switch (model.NotificationTypeId)
                {
                    case NotificationTypeConstants.CompanySubmittedForAction:

                        // get company and update status
                        var cmp = await context.CompanyAccounts.FindAsync(Convert.ToInt32(model.EntityId));
                        cmp.Status = (CompanyStatus)Enum.Parse(typeof(CompanyStatus), model.NotificationActionTakenType.ToString());

                        await context.SaveChangesAsync();

                        // send Action Taken Notification
                        _newId = await SendAsync(NotificationTypeConstants.CompanyActionTaken, cmp, cmp.Id, model);
                        break;

                    case NotificationTypeConstants.ScheduleSubmittedForVerification:

                        var schedule = await payrollDbContext.Schedules.FindAsync(Convert.ToInt32(model.EntityId));
                        //cmp.Status = (CompanyStatus)Enum.Parse(typeof(CompanyStatus), model.NotificationActionTakenType.ToString());

                        await context.SaveChangesAsync();

                        // send Action Taken Notification
                        _newId = await SendAsync(NotificationTypeConstants.ScheduleVerificationUpdate, schedule, schedule.CompanyId, model);
                        break;


                    case NotificationTypeConstants.RequestSubmittedForAction:

                        var request = await payrollDbContext.Requests.FindAsync(Convert.ToInt32(model.EntityId));

                        // if rejected or if it's last step --> then take action and finish approval
                        if (model.NotificationActionTakenType == NotificationActionTakenType.Rejected || isLastStep)
                        {
                            _newId = await SendAsync(NotificationTypeConstants.RequestActionTaken, request, request.CompanyId, model);
                        }
                        else
                        {
                            // then (not last) create notification and send to next level
                            _newId = await SendAsync(NotificationTypeConstants.RequestSubmittedForAction, request, request.CompanyId, model);
                        }

                        // send Action Taken Notification
                        break;
                    default:
                        // all other routes (specific request type)
                        var _request = await payrollDbContext.Requests.FindAsync(Convert.ToInt32(model.EntityId));

                        // if rejected or if it's last step --> then take action and finish approval
                        if (model.NotificationActionTakenType == NotificationActionTakenType.Rejected || isLastStep)
                        {
                            _newId = await SendAsync(NotificationTypeConstants.RequestActionTaken, _request, _request.CompanyId, model);
                        }
                        else
                        {
                            // then (not last) create notification and send to next level
                            _newId = await SendAsync(model.NotificationTypeId, _request, _request.CompanyId, model);
                        }
                        break;
                }


                switch (model.NotificationActionTakenType)
                {
                    case NotificationActionTakenType.Approved:
                        break;
                    case NotificationActionTakenType.Rejected:
                        break;
                    default:
                        break;
                }
            }

            if (model.NotificationType.RequireReceivedNotes)
            {
                switch (model.NotificationTypeId)
                {
                    case NotificationTypeConstants.PublishAnnouncement:
                        var announcement = await payrollDbContext.Announcements.FindAsync(Convert.ToInt32(model.EntityId));

                        announcement.ViewedCount++;
                        await payrollDbContext.SaveChangesAsync();

                        // send Action Taken Notification
                        _newId = await SendAsync(NotificationTypeConstants.AnnouncementViewed, announcement, announcement.CompanyId, model);
                        break;
                    default:
                        break;
                }

            }

            return true;
        }

        public async Task MarkAsReadAsync(Notification notification)
        {
            notification.IsRead = true;
            if (notification.NotificationType.NotificationReceivedBy == NotificationReceivedBy.UsersWithRoles)
                notification.ActionTakenUserId = userResolverService.GetUserId();

            if (!notification.ReceivedDate.HasValue)
                notification.ReceivedDate = DateTime.UtcNow;

            notification.ActionTakenDate = DateTime.UtcNow;
        }

        private string GenerateUrl(string urlWithPlaceholder, Dictionary<string, string> fieldValues)
        {
            return urlWithPlaceholder.StringFormat(fieldValues);
        }

        /// <summary>
        /// Get latest read notification summary returns child notificarion summary after reading current notification
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> GetLatestReadNotificationSummaryAsync(int typeId, int id)
        {
            return await context.Notifications.Where(a => a.NotificationTypeId == typeId && a.IsRead && a.EntityId == id.ToString() && a.ChildNotifications.Any())
                .OrderByDescending(a => a.SentDate)
                .Select(a => a.ChildNotifications.First().Summary).FirstOrDefaultAsync();
        }

        public async Task<Notification> GetNotificationAsync(int id)
        {
            return await context.Notifications
                .Include(a=> a.NotificationType)
                .Include(a=> a.CompanyAccount)
                .Include(a=> a.User)
                .Include(a=> a.ChildNotifications)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Notification>> GetNotificationByTypeAsync(int typeId, string entityId)
        {
            return await context.Notifications
                .Where(a => a.NotificationTypeId == typeId && a.EntityId == entityId)
                .Include(a => a.NotificationType)
                .Include(a => a.CompanyAccount)
                .Include(a => a.User)
                .Include(a => a.ChildNotifications)
                .OrderByDescending(a => a.SentDate)
                .ToListAsync();
        }
        public async Task<List<Notification>> GetNotificationByTypeAsync(int typeId, string entityId, string userId)
        {
            return await context.Notifications
                .Where(a => a.NotificationTypeId == typeId && a.EntityId == entityId && a.ActionTakenUserId == userId)
                .Include(a => a.NotificationType)
                .Include(a => a.CompanyAccount)
                .Include(a => a.User)
                .Include(a => a.ChildNotifications)
                .OrderByDescending(a => a.SentDate)
                .ToListAsync();
        }
        public async Task<Notification> GetLatestNotificationByTypeAsync(int typeId, string entityId, string userId = "")
        {
            return await context.Notifications
                .Where(a => a.NotificationTypeId == typeId && a.EntityId == entityId && (string.IsNullOrWhiteSpace(userId) || a.ActionTakenUserId == userId))
                .Include(a => a.NotificationType)
                .Include(a => a.CompanyAccount)
                .Include(a => a.User)
                .Include(a => a.ChildNotifications)
                .OrderByDescending(a=> a.SentDate)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<string, int>> GetNotificationAnalyticsDictionary (int typeId, string entityId)
        {
            var notifications = await GetNotificationByTypeAsync(typeId, entityId);
            return GetNotificationAnalyticsDictionary(notifications);
        }

        public Dictionary<string, int> GetNotificationAnalyticsDictionary(List<Notification> notifications)
        {
            var data = new Dictionary<string, int>();
            bool approveReject = notifications.FirstOrDefault()?.NotificationType.RequireApproveRejectAction ?? false;

            if (approveReject)
            {
                data.Add("Approved", notifications.Count(x => x.IsRead && x.NotificationActionTakenType == NotificationActionTakenType.Approved));
                data.Add("Rejected", notifications.Count(x => x.IsRead && x.NotificationActionTakenType == NotificationActionTakenType.Rejected));
                data.Add("No Action", notifications.Count(x => x.NotificationActionTakenType == NotificationActionTakenType.NoAction));
            }
            else
            {
                data.Add("Waiting", notifications.Count(x => !x.IsRead));
                data.Add("Viewed", notifications.Count(x => x.IsRead));
            }
            return data;
        }

        public async Task<(bool status, string errorMessage)> CanTakeAction(Notification noti)
        {
            if (noti == null)
                return (false, "Notification was not found");

            var userId = userResolverService.GetUserId();
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return (false, "User not found");
            var userRoles = await userManager.GetRolesAsync(user);
            var agRoles = await accessGrantService.GetMyAccessGrantRolesAsync();
            var empRoles = userResolverService.GetEmployeeRoles();

            if ((noti.NotificationType.NotificationLevel == UserType.PayAll && noti.ActionTakenUserId == userId)
                ||
                (noti.NotificationType.NotificationLevel == UserType.PayAll && noti.NotificationType.UsersWithRoles != null && noti.NotificationType.UsersWithRoles.Intersect(userRoles).Count() == noti.NotificationType.UsersWithRoles.Count())
                ||
                (noti.NotificationType.NotificationLevel == UserType.Company && noti.EmployeeId == userResolverService.GetEmployeeId())
                ||
                ((noti.NotificationType.NotificationLevel == UserType.Company && noti.EmployeesWithRoles != null && noti.EmployeesWithRoles.Intersect(empRoles).Count() == noti.EmployeesWithRoles.Count())))
            {
                if(!noti.IsRead)
                    return (true, "");
                else
                    return (false, "Notification is already read and action taken");
            }
            else
                return (false, "Notification is not assigned to current user or user is not authorized to take action");
        }



        public async Task<bool> RemoveNotificationAsync(int id)
        {
            var n = await GetNotificationAsync(id);
            if(n != null)
            {
                context.Notifications.Remove(n);
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> RemoveNotificationAsync(EntityType entityType, string entityId)
        {
            var n = await context.Notifications.Where(a => a.EntityId == entityId && a.NotificationType.EntityType == entityType).ToListAsync();
            if (n != null)
            {
                context.Notifications.RemoveRange(n);
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }


        public async Task<List<NotificationType>> GetNotificationTypes()
        {
            return await context.NotificationTypes
                .ToListAsync();
        }
    }
}
