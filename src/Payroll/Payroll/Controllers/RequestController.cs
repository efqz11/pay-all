using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payroll.Database;
using Payroll.Models;
using Payroll.Models.ViewModels;
using Payroll.Services;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Payroll.Controllers
{

    public class RequestController : BaseController
    {
        private readonly PayrollDbContext payrolDbContext;
        private readonly ILogger<CompanyController> logger;
        private readonly AccessGrantService accessGrantService;
        private readonly AccountDbContext context;
        private readonly FileUploadService fileUploadService;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly ScheduleService scheduleService;
        private readonly UserResolverService userResolverService;
        private readonly IHubContext<SignalServer> hubContext;
        private readonly RequestService requestService;
        private readonly NotificationService notificationService;
        private readonly SynchronizationService synchronizationService;

        public RequestController(PayrollDbContext context, ILogger<CompanyController> logger, AccessGrantService accessGrantService, AccountDbContext accountDbContext, FileUploadService fileUploadService, IBackgroundJobClient backgroundJobClient, ScheduleService scheduleService, UserResolverService userResolverService, IHubContext<SignalServer> hubContext, RequestService requestService, NotificationService notificationService)
        {
            this.payrolDbContext = context;
            this.logger = logger;
            this.accessGrantService = accessGrantService;
            this.context = accountDbContext;
            this.fileUploadService = fileUploadService;
            this.backgroundJobClient = backgroundJobClient;
            this.scheduleService = scheduleService;
            this.userResolverService = userResolverService;
            this.hubContext = hubContext;
            this.requestService = requestService;
            this.notificationService = notificationService;
        }

        public async Task<IActionResult> Index(DateTime? start = null, DateTime? end = null)
        {
            var _start = start.HasValue ? start.Value : scheduleService.thisWeekStart;
            var _end = end.HasValue ? end.Value : scheduleService.thisWeekEnd;

            //var vm = new HomeEmployeeVm
            //{
            //    Requests = await payrolDbContext.Requests.Where(x => x.EmployeeId == userResolverService.GetEmployeeId() && x.CompanyId == userResolverService.GetCompanyId() &&
            //     (x.CreationDate >= _start && x.CreationDate <= _end))
            //    .Include(x => x.Attendance)
            //    .Include(x => x.DayOff)
            //    .Include(x => x.WorkItem)
            //    .Include(x => x.FileDatas)
            //    .OrderByDescending(x => x.CreationDate)
            //    .ToListAsync()
            //};

            ViewBag.Start = _start;
            ViewBag.End = _end;
            ViewBag.DurationText = scheduleService.GetDurationText(_start, _end, includeDays: false);
            

            var q = payrolDbContext.Requests.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Status == WorkItemStatus.Submitted).GroupBy(z => z.RequestType)
                .Select(x => new { x.Key, Count = x.Count() });
            var item = new HomeAdminVm
            { RequestNotificationsDictionry = await q.ToDictionaryAsync(a => a.Key, a => a.Count) };
        
            return View(item);
        }

        public async Task<IActionResult> AttendanceChangeRequest(int id)
        {
            var attendance = await payrolDbContext.Attendances
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == userResolverService.GetCompanyId());

            if (attendance == null)
                return ThrowJsonError("Attendance record was not found");

            if (payrolDbContext.Requests.Any(x => x.Status == WorkItemStatus.Submitted
            && x.AttendanceId == id && x.CompanyId == userResolverService.GetCompanyId() && x.RequestType == RequestType.Attendance_Change))
                return ThrowJsonError($"Ooops! Cannot create any more, please wait until current request is completed");

            var req = new Request
            {
                Id = 0,
                RequestType = RequestType.Attendance_Change,
                Start = attendance.CheckInTime,
                End = attendance.CheckOutTime,
                Attendance = attendance,
                AttendanceId = attendance.Id,
                Employee = attendance.Employee,
                EmployeeId = attendance.EmployeeId
            };

            return PartialView("_AttendanceWorkChangeRequest", req);
        }

        public async Task<IActionResult> WorkItemChangeRequest(int id)
        {
            var workItem = await payrolDbContext.WorkItems
                .Include(x => x.Employee)
                .Include(x => x.Work)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsEmployeeTask == false);
            // && x.CompanyId == userResolverService.GetCompanyId()


            if (workItem == null)
                return ThrowJsonError("Work item was not found");

            if (payrolDbContext.Requests.Any(x => x.Status == WorkItemStatus.Submitted
            && x.WorkItemId == id && x.CompanyId == userResolverService.GetCompanyId() && x.RequestType == RequestType.Work_Change))
                return ThrowJsonError($"Ooops! Cannot create any more, please wait until current request is completed");

            var req = new Request
            {
                Id = 0,
                RequestType = RequestType.Work_Change,
                Start = workItem.CheckInTime,
                End = workItem.CheckOutTime,
                WorkItem = workItem,
                WorkItemId = workItem.Id,
                Employee = workItem.Employee,
                EmployeeId = workItem.EmployeeId
            };

            return PartialView("_AttendanceWorkChangeRequest", req);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AttendanceChangeRequest(Request model)
        //{
        //    model.CompanyId = userResolverService.GetCompanyId();
        //    if (ModelState.IsValid)
        //    {
        //        if (model.RequestType != RequestType.Attendance_Change)
        //            return ThrowJsonError($"Fatal error, this request cannot be served");

        //        if (model.NewCheckinTime.HasValue == false || model.NewCheckOutTime.HasValue == false)
        //            return ThrowJsonError($"Checkin and Checkout times cannot be empty");

        //        if (! await payrolDbContext.Attendances.AnyAsync(x => x.Id == model.AttendanceId && x.CompanyId == model.CompanyId))
        //            return ThrowJsonError($"Oohhh! Attendance record was not found!");

        //        if (payrolDbContext.Requests.Any(x=> (model.Id == 0 || model.Id != x.Id) && x.Status == WorkItemStatus.Submitted
        //        && x.AttendanceId == model.AttendanceId && x.CompanyId == model.CompanyId))
        //            return ThrowJsonError($"Ooops! There is already some request sent for approval, please wait until it's completed");
        //    }


        //    if (ModelState.IsValid)
        //    {
        //        if (model.Id <= 0)
        //        {
        //            model.Status = WorkItemStatus.Submitted;
        //            model.SubmissionDate = DateTime.UtcNow;
        //            await payrolDbContext.Requests.AddAsync(model);
        //        }
        //        else
        //        {
        //            payrolDbContext.Requests.Update(model);
        //        }

        //        int recordUpdateCount = await payrolDbContext.SaveChangesAsync();

        //        return RedirectToAction("ViewAttendance", "Schedule", new { id = model.AttendanceId });
        //    }

        //    return ThrowJsonError();
        //}

        public async Task<IActionResult> NewRequest(int? id = null)
        {
            //await hubContext.Clients.All.SendAsync("ReceiveMessage", userResolverService.GetUserId(), "message");

            //var userId = "1b92e89b-aab4-417e-8abf-1bff855061bf"; //  userResolverService.GetUserId();
            //var message = $"Send message to you with user id {userId}";
            //await hubContext.Clients.Client(userId).SendAsync("ReceiveMessage", userResolverService.GetUserId(), message);

            return PartialView("_NewRequest", new Request { EmployeeId = id ?? userResolverService.GetEmployeeId() });
        }



        [HttpPost]
        public async Task<IActionResult> NewRequestOfType(RequestType type, int? empId = null, string from = "", string to = "", int? attn = null)
        {
            var _empId = empId ?? userResolverService.GetEmployeeId();
            try
            {
                var dtF = DateTime.ParseExact(from, "yyMMdd", null);
                var dtT = DateTime.ParseExact(to, "yyMMdd", null);
                return await ViewRequest(type, empId: _empId, from: dtF, to: dtT, attnId: attn);
            }
            catch (Exception)
            {
                return await ViewRequest(type, empId: _empId, attnId: attn);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewRequest(Request request)
        {
            ViewBag.UpcomingAttendanceRecords = await payrolDbContext.Attendances.Where(x => x.EmployeeId == request.EmployeeId && x.Date.Date >= DateTime.Now.Date && x.Date.Date < DateTime.Now.Date.AddDays(7))
                //.Include(x => x)
                .ToListAsync();


            return await ViewRequest(request.RequestType, empId: request.EmployeeId);
        }


        public async Task<IActionResult> ViewRequest(RequestType type, int empId, int id = 0, int edit = 0, DateTime? from = null, DateTime? to = null, int? attnId = null)
        {
            var request = new Request { RequestType = type, EmployeeId = empId, Start = from, End = to, AttendanceId = attnId, CompanyId = userResolverService.GetCompanyId() };
            if (id > 0)
                request = await payrolDbContext.Requests
                    .Include(x => x.DayOff)
                    .Include(x => x.Employee)
                    .Include(x => x.Attendance)
                    .Include(x => x.WorkItem)
                    .ThenInclude(x => x.Work)
                    .Include(x => x.FileDatas)
                .Include(x => x.RequestDataChanges)
                    .ThenInclude(x => x.Nationality)
                .Include(x => x.RequestDataChanges)
                    .ThenInclude(x => x.EmergencyContactRelationship)
                    .Include(x => x.TransferredEmployee)
                    .FirstOrDefaultAsync(x => x.Id == id);


            ViewBag.ProcessConfig = await requestService.GetRequestApprovalConfigs(request.CompanyId, request.RequestType, request.DayOffId);
            if (edit != 1)
            {
                if ((request != null && id > 0 && request.Status != WorkItemStatus.Draft) || request.RequestType == RequestType.Emp_DataChange)
                {
                    var notifs = await notificationService.GetNotificationByTypeAsync(requestService.getNotificaitonTypeForRequestType(request.RequestType), id.ToString());
                    ViewBag.Notifications = notifs;
                    if (this.Request.Query.ContainsKey("noti"))
                    {
                        var latestNotif = notifs.LastOrDefault(a => !a.IsRead);
                        // await notificationService.GetLatestNotificationByTypeAsync(NotificationTypeConstants.RequestSubmittedForAction, id.ToString());
                        var ss = await notificationService.CanTakeAction(latestNotif);
                        ViewBag.CanTakeAction = ss.status;
                    }

                    //if(!notifs.Any())
                    //{
                    //    // try to send if notificaion was not found!
                    //    await notificationService.SendAsync(NotificationTypeConstants.RequestSubmittedForAction,
                    //        obj: request,
                    //        companyAccountId: request.CompanyId);
                    //    await context.SaveChangesAsync();
                    //}

                    if (request.RequestType == RequestType.Emp_DataChange)
                    {
                        var _empType = typeof(Employee);
                        //var _ind = typeof(Individual);

                        List<(string, string, string, bool?)> ChangedFieldDIcst = await requestService.GetChangedFieldsList(request);

                        //foreach (var dCh in dataChangeFields.Keys)
                        //{
                        //    // if value is changed on Employee

                        //    if (empFieldsDict[dCh] != dataChangeFields[dCh])
                        //        ChangedFieldDIcst.Add(_empType.GetField(dCh).GetCustomAttribute<DisplayAttribute>().Name, dataChangeFields[dCh]?.ToString());
                        //}

                        ViewBag.DataChangeListCompare = ChangedFieldDIcst;
                    }
                    return PartialView("~/Views/Request/_ViewRequest.cshtml", request);
                }
            }

            switch (type)
            {
                case RequestType.Leave:
                    ViewBag.DayOffs = await payrolDbContext.DayOffEmployees.Where(x => x.DayOff.CompanyId == userResolverService.GetCompanyId() && x.Year == DateTime.Now.Year && x.EmployeeId == request.EmployeeId)
                        .Include(x => x.DayOff)
                        .ToListAsync();
                    //request.Start = DateTime.Now;
                    return PartialView("~/Views/Request/_LeaveRequest.cshtml", request);
                case RequestType.Overtime:
                    request._StartTime = request.Start?.TimeOfDay;
                    request._EndTime = request.End?.TimeOfDay;
                    if (attnId.HasValue && attnId.Value > 0)
                    {
                        request.AttendanceId = attnId;
                        request.Attendance = await payrolDbContext.Attendances.FindAsync(attnId);
                    }
                    return PartialView("~/Views/Request/_OTRequest.cshtml", request);
                case RequestType.Attendance_Change:
                case RequestType.Work_Change:
                    return PartialView("~/Views/Request/_AttendanceWorkChangeRequest.cshtml", request);
                case RequestType.Work_Submission:
                    return PartialView("~/Views/Request/_AttendanceWorkChangeRequest.cshtml", request);
                case RequestType.Holiday:
                    break;
                case RequestType.Document:
                    return PartialView("~/Views/Request/_DocumentRequest.cshtml", request);
                default:
                    break;
            }

            return ThrowJsonError("Unrecognized");
        }


        public async Task<IActionResult> ViewRequestAction(int id, bool? approve = null)
        {
            var request = await payrolDbContext.Requests
                    .Include(x => x.DayOff)
                    .Include(x => x.Employee)
                    .Include(x => x.FileDatas)
                    .Include(x => x.Attendance)
                    .Include(x => x.WorkItem)
                        .ThenInclude(x => x.Work)
                    .FirstOrDefaultAsync(x => x.Id == id);
            var year = request.Start?.Year;

            if (request.RequestType == RequestType.Leave)
            {
                ViewBag.EmplDayOffForYear = await payrolDbContext.DayOffEmployees
                            .Include(x => x.DayOffEmployeeItems)
                            .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId && x.Year == year && x.DayOffId == request.DayOffId);
                ViewBag.AffectedAttendances = await payrolDbContext.Attendances.CountAsync(a => a.IsPublished && a.EmployeeId == request.EmployeeId &&
                  a.Date >= request.Start && a.Date <= request.End);
            }
            else if (request.RequestType == RequestType.Document)
            {
                foreach (var item in request.DocumentsDataArray)
                {
                    if (!request.FileDatas.Any(x => x.Name == item && x.IsUploaded && !x.IsNameChangeable))
                    {
                        ViewBag.RequireFileUpload = true;
                        break;
                    }
                }
            }
            request.IsApproved = approve;


            var latestNotif = await notificationService.GetLatestNotificationByTypeAsync(requestService.getNotificaitonTypeForRequestType(request.RequestType), id.ToString());
            var steps = await requestService.GetRequestApprovalConfigs(request.CompanyId, request.RequestType, request.DayOffId);
            if (latestNotif == null)
                return ThrowJsonError("Notification's were not found and cannot be approved");

            ViewBag.IsLastStep = latestNotif.Step == steps.Max(a => a.Step);
            if (latestNotif.Step != steps.Max(a => a.Step))
            {
                ViewBag.Summary = "On approval, this request will be escalated to " + steps.First(a => a.Step == latestNotif.Step + 1).GetApprovalPersonHtml(User);
            }
            else
            {
                ViewBag.Summary = "This is the last step of the approval process";
            }

            return PartialView("_ViewRequestAction", request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeRequestAction(RequestActionVm model)
        {
            try
            {
                await requestService.ProcessRequestAction(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return ThrowJsonError(ex.Message);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessRequest(ProcessRequestVm model)
        {
            Request request = null;
            try
            {
                request = await requestService.ProcessRequest(model);
            }
            catch (Exception ex)
            {
                logger.LogError(1, ex, "Error while processing request ");
                return ThrowJsonError(ex.Message);
            }


            if (request.RequestType == RequestType.Leave)
                return await UploadDocument(request.Id);

            if (request.RequestType == RequestType.Attendance_Change || request.RequestType == RequestType.Work_Change || request.RequestType == RequestType.Document || request.RequestType == RequestType.Overtime)
            {
                if (request.Status == WorkItemStatus.Submitted)
                {
                    await notificationService.SendAsync(NotificationTypeConstants.RequestSubmittedForAction,
                        obj: request,
                        companyAccountId: request.CompanyId);

                    //return RedirectToAction("ViewRequest", "Home", new { id = userResolverService.GetEmployeeId() });
                }

                return RedirectToAction(nameof(ViewRequest), new { id = request.Id });
            }

            //hubContext
            return RedirectToAction("Requests", "Employee", new { id = request.EmployeeId });
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> FindAttendance(Request request)
        //{
        //    if (request.Start.HasValue == false)
        //        return ThrowJsonError("Request start date is required");
        //    var attendances = await payrolDbContext.Attendances.Where(x => x.Date.Date == request.Start.Value.Date && x.EmployeeId == request.EmployeeId
        //    && x.CompanyId == userResolverService.GetCompanyId()).ToListAsync();

        //    return PartialView("_FindAttendance", attendances);
        //}


        //public async Task<IActionResult> ViewRequest(RequestType type, int id = 0)
        //{
        //    var request = new Request { RequestType = type };
        //    if (id > 0)
        //        request = await payrolDbContext.Requests.FindAsync(id);
        //    switch (type)
        //    {
        //        case RequestType.Leave:
        //            ViewBag.DayOffs = await payrolDbContext.DayOffEmployees.Where(x => x.DayOff.CompanyId == userResolverService.GetCompanyId() && x.Year == DateTime.Now.Year)
        //                .Include(x => x.DayOff)
        //                .ToListAsync();
        //            //request.Start = DateTime.Now;
        //            return PartialView("_LeaveRequest", request);
        //        case RequestType.Overtime:
        //            return PartialView("_OTRequest", request);
        //        case RequestType.Attendance_Change:
        //            return PartialView("_FindAttendance", request);
        //            break;
        //        case RequestType.Work_Change:
        //            break;
        //        case RequestType.Work_Submission:
        //            break;
        //        case RequestType.Holiday:
        //            break;
        //        case RequestType.Document:
        //            break;
        //        default:
        //            break;
        //    }

        //    return ThrowJsonError("Unrecognized");
        //}

        [HttpPost]
        public async Task<IActionResult> AddDocument(int id)
        {
            var request = await payrolDbContext.Requests
                .Include(x => x.DayOff)
                .Include(x => x.FileDatas)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (request == null)
                return ThrowJsonError("Request was not found");

            request.FileDatas.Add(new FileData { CompanyId = userResolverService.GetCompanyId() });

            await payrolDbContext.SaveChangesAsync();
            return PartialView("_UploadDocument", request);
        }

        public async Task<IActionResult> RemoveDocument(int id, int rId)
        {
            var file = await payrolDbContext.FileDatas
                .FirstOrDefaultAsync(x => x.Id == id && x.RequestId == rId);
            if (file == null)
                return ThrowJsonError("Document was not found");

            payrolDbContext.FileDatas.Remove(file);
            await payrolDbContext.SaveChangesAsync();

            return RedirectToAction(nameof(UploadDocument), new { id = rId });
        }

        public async Task<IActionResult> UploadDocument(int id)
        {
            var request = await payrolDbContext.Requests
                .Include(x => x.DayOff)
                .Include(x => x.FileDatas)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (request == null)
                return ThrowJsonError("Request was not found");

            return PartialView("_UploadDocument", request);
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(IFormFile file, int id, string name = "")
        {
            if (file == null)
                return ThrowJsonError("Please upload file");
            var fileData = await payrolDbContext.FileDatas.FirstOrDefaultAsync(x => x.Id == id);
            if (fileData == null)
                return ThrowJsonError("File data was not found");
            if (!fileData.RequestId.HasValue)
                return ThrowJsonError("Request was not found");

            try
            {
                string fileUrl = await fileUploadService.UploadFles(file, "files");
                fileData.FileName = System.IO.Path.GetFileName(file.FileName);
                fileData.FileUrl = fileUrl;
                fileData.Name = name;
                fileData.IsUploaded = true;
            }
            catch (Exception ex)
            {
                return ThrowJsonError("Upload failed. " + ex.Message);
            }

            //var dayOff = await payrolDbContext.DayOffs.FirstOrDefaultAsync(x => x.Id == fileData.DayOffId);
            //if (dayOff == null)
            //    return ThrowJsonError("Please choose leave type");

            //if(dayOff.RequiredDocuments)
            //{
            //    var totalRequired = dayOff.RequiredDocumentList.Split(',').Count();

            //    if (model.FileDatas == null && totalRequired > 0)
            //        return ThrowJsonError("Oops! there are no files uploaded");

            //    if (model.FileDatas.Count(x=> x.IsActive) < dayOff.RequiredDocumentList.Split(',').Count())
            //        return ThrowJsonError("Please upload pending documents");
            //}

            //fileData.Status = WorkItemStatus.Submitted;
            await payrolDbContext.SaveChangesAsync();

            return await UploadDocument(fileData.RequestId.Value);
        }


        [HttpPost]
        public async Task<IActionResult> SaveDocument(int id, int approve = 0)
        {
            var reqeust = await payrolDbContext.Requests
                .Include(x => x.FileDatas)
                .Include(x => x.DayOff)
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (reqeust == null)
                return ThrowJsonError("Request was not found");

            if (reqeust.RequestType == RequestType.Leave && reqeust.FileDatas.Count > 0)
            {
                List<FileData> filesDatas = new List<FileData>();
                int files = Request.Form.Count / 3;
                var ix = 0;
                for (int j = 0; j < files; j++)
                {
                    filesDatas.Add(new FileData
                    {
                        Id = Convert.ToInt32(Request.Form[$"[{ix}].Id"]),
                        Name = Request.Form[$"[{ix}].Name"],
                        IsUploaded = !string.IsNullOrWhiteSpace(Request.Form[$"[{ix}].IsUploaded"]) ? Convert.ToBoolean(Request.Form[$"[{ix}].IsUploaded"]) : false
                    });

                    ix += 1;
                }

                reqeust.FileDatas.ForEach(t =>
                {
                    t.Name = filesDatas.Any(x => x.Id == t.Id) ? filesDatas.FirstOrDefault(x => x.Id == t.Id)?.Name : t.Name;
                });
                if (filesDatas.Count <= 0)
                    return ThrowJsonError("Please upload all the files or remove them");

                var dayOff = await payrolDbContext.DayOffs.FirstOrDefaultAsync(x => x.Id == reqeust.DayOffId);
                var totalUploadedNotNullName = reqeust.FileDatas.Count(x => x.IsUploaded && !string.IsNullOrWhiteSpace(x.Name));
                if (dayOff != null)
                {

                    if (dayOff.RequiredDocuments)
                    {
                        var totalRequired = dayOff.RequiredDocumentList.Split(',').Count();

                        if (reqeust.FileDatas == null && totalRequired > 0)
                            return ThrowJsonError("Oops! there are no files uploaded");

                        if (totalUploadedNotNullName < dayOff.RequiredDocumentList.Split(',').Count())
                            return ThrowJsonError("Please upload pending documents, also set name for files");
                    }
                }
            }


            bool _sendNotif = false;
            if (approve == 1)
            {
                reqeust.SubmissionDate = DateTime.UtcNow;
                reqeust.Status = WorkItemStatus.Submitted;
                _sendNotif = true;
            }

            ////fileData.Status = WorkItemStatus.Submitted;
            var affected = await payrolDbContext.SaveChangesAsync();

            if (affected > 0 && _sendNotif)
            {
                await notificationService.SendAsync(requestService.getNotificaitonTypeForRequestType(reqeust.RequestType),
                    obj: reqeust,
                    companyAccountId: reqeust.CompanyId);

                return RedirectToAction(nameof(ViewRequest), new { id = reqeust.Id });
            }

            return RedirectToAction(nameof(UploadDocument), new { id = reqeust.Id });
        }


        private double GetTotalHrsBefore(AlertType alert)
        {
            switch (alert)
            {
                case AlertType.Before_5_Min:
                    return TimeSpan.FromMinutes(5).TotalHours;
                case AlertType.Before_10_Min:
                    return TimeSpan.FromMinutes(10).TotalHours;
                case AlertType.Before_15_Min:
                    return TimeSpan.FromMinutes(15).TotalHours;
                case AlertType.Before_30_Min:
                    return TimeSpan.FromMinutes(30).TotalHours;
                case AlertType.Before_1_Hour:
                    return TimeSpan.FromHours(1).TotalHours;
                case AlertType.Before_2_Hour:
                    return TimeSpan.FromMinutes(2).TotalHours;
                case AlertType.Before_1_Day:
                    return TimeSpan.FromDays(1).TotalHours;
                case AlertType.Before_2_Day:
                    return TimeSpan.FromDays(2).TotalHours;
                case AlertType.Before_1_Week:
                    return TimeSpan.FromDays(7).TotalHours;
                default:
                    return 0;
            }
        }


        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            if (ModelState.IsValid)
            {
                var add = payrolDbContext.Requests.FirstOrDefault(x => x.Id == id);
                if (add == null)
                    return BadRequest("Oooh! we didnt find that one");
                //if(context.PayrollPeriodPayAdjustments.Any(x=> x.PayAdjustmentId == payAdjustmentId))
                //    return BadRequest("Ouch! Some items are used as children, please remove them before proceed");
                var start = add.CreationDate;

                payrolDbContext.Requests.Remove(add);
                payrolDbContext.SaveChanges();
                await notificationService.RemoveNotificationAsync(EntityType.Request, id.ToString());
                return Ok();
            }

            return ThrowJsonError();
        }

    }
}

