
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;

namespace Payroll.Controllers
{
    public class NewsUpdateController : BaseController
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
        private readonly BackgroundJobService backgroundJobService;
        private readonly NotificationService notificationService;

        public NewsUpdateController(PayrollDbContext context, IBackgroundJobClient backgroundJobClient, EmployeeService employeeService, AccessGrantService accessGrantService, UserResolverService userResolverService, FileUploadService fileUploadService, AccountDbContext accountDbContext, UserManager<AppUser> userManager, CompanyService companyService, PayrollService payrollService, BackgroundJobService backgroundJobService, NotificationService notificationService)
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
            this.backgroundJobService = backgroundJobService;
            this.notificationService = notificationService;
        }
        

        public async Task<IActionResult> AllAnnouncements(DateTime? start = null, DateTime? end = null, string query = "", int status = 0, int limit = 10, int page = 1)
        {
            var anns = context.Announcements.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                anns = anns.Where(x => x.Title.ToLower().Contains(query));
            }

            if (start.HasValue && end.HasValue)
                anns = anns.Where(x => x.Start.HasValue && x.End.HasValue && x.Start >= start && x.End <= end);

            if (status > 0)
            {
                switch (status)
                {
                    case 1:
                        anns = anns.Where(a => a.Status == AnnouncementStatus.Draft);
                        break;
                    case 2: // published
                        anns = anns.Where(a => a.Status == AnnouncementStatus.Published);
                        break;
                    case 3: // scheduled
                        anns = anns.Where(a => a.Status == AnnouncementStatus.Scheduled);
                        break;
                    case 4: //expired
                        anns = anns.Where(a => a.Status == AnnouncementStatus.Expired);
                        break;
                    default:
                        break;
                }
            }

            ViewBag.Count = await anns.CountAsync();

            var _data = await anns
             .OrderByDescending(x => x.Id)
             .Skip((page - 1) * limit)
             .Take(limit)
             .Include(a=> a.CreatedEmployee)
             .Include(a=> a.FileDatas)
             .ToListAsync();

            if (Request.IsAjaxRequest())
                return PartialView("_Announcements", _data);

            ViewBag.query = query;
            ViewBag.limit = limit;
            ViewBag.status = status;
            ViewBag.start = start;
            ViewBag.end = end;
            ViewBag.duration = start.GetDuration(end, userResolverService.GetClaimsPrincipal());
            return View(_data);
        }


        public async Task<IActionResult> Announcement(int id)
        {
            var anns = await context.Announcements
                .Where(x=> x.Id == id)
                //.Include(x => x.Employee)
                //.Include(x => x.Department)
                .Include(x => x.CreatedEmployee)
                .Include(x => x.Company)
                .Include(x => x.backgroundJobs)
                .FirstOrDefaultAsync();

            ViewBag.Waiting = anns.TotalInteractionsCount - anns.ViewedCount;
            ViewBag.Viewed = anns.ViewedCount;

            var noti = await notificationService.GetLatestNotificationByTypeAsync(NotificationTypeConstants.PublishAnnouncement, id.ToString(), userResolverService.GetUserId());

            ViewBag.Data = await notificationService.GetNotificationAnalyticsDictionary(NotificationTypeConstants.PublishAnnouncement, id.ToString());
            ViewBag.Noti = noti;

            if (Request.IsAjaxRequest()) {
                if (noti != null)
                    SetTempDataMessage(noti.IsRead ? "This notification is read and completed" : "You still haven't marked this notification as read.", !noti.IsRead ? MsgAlertType.warning : MsgAlertType.success);
                return PartialView("_ViewAnnouncement", anns);
            }

            return View(anns);
        }

        public async Task<IActionResult> ViewAudience(int id)
        {
            var ann  = await context.Announcements
                .Include(x => x.FileDatas)
                //.Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ann.EmployeeSelectorVm == null || !ann.EmployeeSelectorVm.IsValid()) return ThrowJsonError("Audience is not set right!");
            
            var q = context.Employees
                .Where(x => x.Department.CompanyId == userResolverService.GetCompanyId());
            var deptGrp = await q.GroupBy(a => new { a.DepartmentId, a.Department.Name })
                .Select(a => Tuple.Create(a.Key.DepartmentId, a.Key.Name, a.Count())).ToListAsync();

            var locGrp = await q.Where(a => a.LocationId.HasValue)
                .GroupBy(a => new { a.LocationId, a.Location.Name })
                .Select(a => Tuple.Create(a.Key.LocationId, a.Key.Name, a.Count())).ToListAsync();

            var empStatus = await q.Where(a => a.JobType.HasValue)
                .GroupBy(a => new { id = (int?)a.JobType, a.JobType })
                .Select(a => Tuple.Create(a.Key.id, a.Key.JobType, a.Count())).ToListAsync();


            var model = ann.EmployeeSelectorVm;
            var employeeSelectorVm = new EmployeeSelectorVm
            {
                ByLocation = locGrp,
                ByDept = deptGrp,
                ByJobType = empStatus,
                Action = ann.EmployeeSelectorVm.Action ?? "NewAnnouncement",
                Controller = ann.EmployeeSelectorVm.Controller ?? "NewsUpdate",
                Update = ann.EmployeeSelectorVm.Update ?? ".modal__container",
                OnSuccess = ann.EmployeeSelectorVm.OnSuccess,

                ChosenEmployeeDataString = model?.ChosenEmployeeDataString ?? "[]",
                GroupByCategory = model?.GroupByCategory ?? GroupByCategory.ChooseEmployees,
                GroupByCategoryValue = model?.GroupByCategoryValue ?? "",
                TotalMatchedEmployees = model?.TotalMatchedEmployees ?? 0,
                EmployeeIds = model?.EmployeeIds ?? null,
                Summary = model?.Summary ?? "",
                IsEditView = model != null
            };

            return PartialView("_EmployeeSelector", employeeSelectorVm);
        }
        


        public async Task<IActionResult> NewAnnouncement(int? id = null)
        {
            Announcement ann = null;

            var model = GetEmployeeSelectorModal();

            if (id.HasValue && id.Value > 0)
            {
                ann = await context.Announcements
                .Include(x => x.FileDatas)
                //.Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id);
                if (ann == null)
                    return ThrowJsonError("Announcement was not found!");
            }
            else
                ann = new Announcement { EmployeeSelectorVm = model };
            
            return PartialView("_NewAnnouncement", ann);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAnnouncement(int? id = null)
        {
            var notification = await notificationService.GetLatestNotificationByTypeAsync(NotificationTypeConstants.PublishAnnouncement, id.ToString(), userResolverService.GetUserId());
            if (notification == null)
                return ThrowJsonError("Notification was not found!");

            var announcemnt = await context.Announcements
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

            if (announcemnt == null)
                return ThrowJsonError("Oops! That announcement was not found");

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
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewAnnouncement(Announcement model, IFormFile file)
        {
            if (ModelState.IsValid)
            {

                //if (model.IsPublic == false && (model.EmployeeId == null && model.DepartmentId == null))
                //    return ThrowJsonError("Kindly choose your non public audience");

                var ann = new Announcement();
                if (model.Id > 0)
                    ann = await context.Announcements
                        .Include(x => x.FileDatas)
                        .FirstOrDefaultAsync(x => x.Id == model.Id);
                if (model.Id > 0 && ann == null)
                    return ThrowJsonError();


                //ann.IsForDepartment = ann.IsForEmployee = false;

                var _isCreating = model.Id <= 0;

                if (_isCreating)
                {
                    var selector = GetEmployeeSelectorModal();
                    if (selector == null || !selector.IsValid()) return ThrowJsonError("Audience is not set right!");

                    //if (model.IsPublic)
                    //    ann.TotalInteractionsCount = await context.Employees.CountAsync(x => x.Department.CompanyId == userResolverService.GetCompanyId());
                    //else if (model.DepartmentId > 0)
                    //{
                    //    ann.IsForDepartment = true;
                    //    ann.DepartmentId = model.DepartmentId;
                    //    ann.EmployeeIdsData = await context.Employees.Where(x => x.DepartmentId == model.DepartmentId).Select(x => x.Id).ToArrayAsync();
                    //    ann.TotalInteractionsCount = await context.Employees.CountAsync(x => x.DepartmentId == model.DepartmentId);
                    //}
                    //else if (model.EmployeeIdsData != null && model.EmployeeIdsData.Count > 0)
                    //{
                    //    ann.IsForEmployee = true;
                    //    ann.EmployeeIdsData = model.EmployeeIdsData;
                    //    ann.TotalInteractionsCount = model.EmployeeIdsData?.Count() ?? 0;
                    //}

                    ann.CreatedEmployeeId = userResolverService.GetEmployeeId();


                    ann.Title = model.Title;
                    ann.Summary = model.Summary;
                    ann.EmployeeSelectorVm = selector;
                    //ann.IsPublic = model.IsPublic;
                    ann.CompanyId = userResolverService.GetCompanyId();

                    if (model.HasExpiry)
                    {
                        if(!model.Start.HasValue || !model.End.HasValue)
                            return ThrowJsonError("Kindly choose dates for the announcement");

                        ann.Status = AnnouncementStatus.Scheduled;
                        ann.Start = model.Start;
                        ann.End = model.End;
                        if (ann.End < ann.Start)
                            return ThrowJsonError("End date cannot be before start date");

                        if (ann.Start < DateTime.UtcNow)
                            return ThrowJsonError("Start date must be in the future");

                        await backgroundJobService.ScheduleAnnounceMmentPublishDate(ann);
                    }
                    else
                    {
                        ann.Status = AnnouncementStatus.Published;
                        ann.PublishedDate = DateTime.UtcNow;
                        ann.TotalInteractionsCount = ann.EmployeeSelectorVm?.EmployeeIds?.Count() ?? 0;
                    }
                }
                else
                {
                    ann.Title = model.Title;
                    ann.Summary = model.Summary;
                    //ann.IsPublic = model.IsPublic;
                }

                bool hasFiles = fileUploadService.HasFilesReadyForUpload(file);
                if (hasFiles)
                {
                    if (fileUploadService.GetFileSizeInMb(file) >= UploadSetting.MaxFileSizeMb)
                        return ThrowJsonError($"File is too huge, only file size up to {UploadSetting.MaxFileSizeMb}MB are allowed!");

                    if (!fileUploadService.IsAllowedFileType(file, UploadSetting.FileTypes))
                        return ThrowJsonError($"Uploaded file type is not allowed");


                    string fileUrl = await fileUploadService.UploadFles(file);

                    ann.FileDatas.Add(new FileData
                    {
                        CompanyId = userResolverService.GetCompanyId(),
                        Name = file.Name,
                        ContentType = file.ContentType,
                        FileUrl = fileUrl,
                        IsUploaded = true
                        //AnnouncementId = ann.Id;
                    });
                }

                if (model.Id > 0)
                    context.Announcements.Update(ann);
                else
                    context.Announcements.Add(ann);


                
                await context.SaveChangesAsync();
                if(_isCreating && ann.Status == AnnouncementStatus.Published)
                    /// send notifications
                    await notificationService.SendToEmployeesAsync(
                        type: NotificationTypeConstants.PublishAnnouncement,
                        obj: ann,
                        companyAccountId: ann.CompanyId,
                        empIds: ann.EmployeeSelectorVm.EmployeeIds);

                return RedirectToAction(nameof(Announcement), new { Id = ann.Id });
            }

            return ThrowJsonError(ModelState);
        }

        public async Task<IActionResult> GetAnnouncement(int id)
        {
            var anns = await companyService.GetAnnouncement(id);
            return PartialView("_Announcements", new[] { anns });
        }


        public async Task<IActionResult> RemoveAnnouncement(int? id = null)
        {
            var ann = new Announcement();
            ann = await context.Announcements
                        .FirstOrDefaultAsync(x => x.Id == id);


            context.Announcements.Remove(ann);

            return Ok();
        }


        [HttpPost]
        public IActionResult Remove(int id)
        {
            if (ModelState.IsValid)
            {
                var add = context.Employees.FirstOrDefault(x => x.Id == id);
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

