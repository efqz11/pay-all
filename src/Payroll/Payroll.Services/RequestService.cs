using Microsoft.AspNetCore.Identity;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payroll.ViewModels;
using Payroll.Database;
using Microsoft.EntityFrameworkCore;
using Payroll.Models.ViewModels;

namespace Payroll.Services
{
    public class RequestService
    {
        private readonly UserResolverService userResolverService;
        private readonly PayrollDbContext context;
        private readonly AccountDbContext accountDbContext;
        private readonly ScheduleService scheduleService;
        private readonly NotificationService notificationService;
        private readonly EmployeeService employeeService;
        private readonly AccessGrantService accessGrantService;
        private string computedstring;

        public RequestService(UserResolverService userResolverService, PayrollDbContext context, AccountDbContext accountDbContext, ScheduleService scheduleService, NotificationService notificationService, EmployeeService employeeService, AccessGrantService accessGrantService)
        {
            this.userResolverService = userResolverService;
            this.context = context;
            this.accountDbContext = accountDbContext;
            this.scheduleService = scheduleService;
            this.notificationService = notificationService;
            this.employeeService = employeeService;
            this.accessGrantService = accessGrantService;
        }

        public async Task<Request> GetRequestById(int id)
        {
            return await context.Requests
                    .Include(x => x.DayOff)
                    .Include(x => x.Employee)
                    .Include(x => x.Attendance)
                    .Include(x => x.WorkItem)
                    .ThenInclude(x => x.Work)
                    .Include(x => x.FileDatas)
                    .Include(x => x.TransferredEmployee)
                    .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Request>> GetRequestsCompany(DateTime? start = null, DateTime? end = null, RequestType ? type = null, WorkItemStatus? status = null, int page = 1, int limit = 10)
        {
            return await context.Requests.Where(x => x.CompanyId == userResolverService.GetCompanyId() &&
                (start.HasValue && end.HasValue ? x.CreationDate >= start && x.CreationDate <= end : true) &&
                (type == null || x.RequestType == type) &&
                (status == null || x.Status == status))
                .OrderByDescending(x => x.CreationDate)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Include(x => x.Attendance)
                .Include(x => x.DayOff)
                .Include(x => x.FileDatas)
                .Include(x => x.WorkItem)
                .ToListAsync();
        }


        public async Task<List<DayOffEmplVm>> GetLeaveRequests(int id, int[] empIds, DateTime start, DateTime end, RequestType[] types = null, WorkItemStatus[] statuses = null, int page = 1, int limit = 10)
        {
            return await context.Requests.Where(a => empIds.Contains(a.EmployeeId) && a.RequestType == RequestType.Leave && a.Start >= start && a.End <= end && a.IsActive && (types == null || types.Contains(a.RequestType)) && (statuses == null || statuses.Contains(a.Status)))
                       .Include(a => a.DayOff)
                        .Select(a => new DayOffEmplVm
                        {
                            UniqeId = a.Id,
                            DayOffId = a.DayOffId.Value,
                            DayOffColor = a.DayOff.Color,
                            Start = a.Start.Value,
                            End = a.End.Value,
                            RequestId = a.Id,
                            RequestReference = a.NumberFormat,
                            RequestedDuration = a.GetRequestedDuration(),
                            RequestedStatusString = a.GetStatusString(),
                            RequestedStatusIcon = a.GetIcon(),
                            RequestedStatus = a.Status,
                            DayOffName = a.DayOff.Name,
                            EmployeeId = a.EmployeeId,
                            IsPending = true,
                        })
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Request>> GetRequests(int id, DateTime? start = null, DateTime? end = null, RequestType? type = null, WorkItemStatus? status = null, int page = 1, int limit = 10)
        {
            return await context.Requests.Where(x => x.EmployeeId == id && x.CompanyId == userResolverService.GetCompanyId() &&
                ((start == null || end == null) || (x.CreationDate >= start && x.CreationDate <= end)) &&
                (type == null || x.RequestType == type) &&
                (status == null || x.Status == status))
                .OrderByDescending(x => x.CreationDate)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Include(x => x.Attendance)
                .Include(x => x.DayOff)
                .Include(x => x.FileDatas)
                .Include(x => x.WorkItem)
                .ToListAsync();
        }

        public async Task<bool> IsRequestSubsmitted(int id, RequestType type)
        {
            return await context.Requests.AnyAsync(x => x.EmployeeId == id && x.CompanyId == userResolverService.GetCompanyId() &&
                (x.RequestType == type) &&
                (x.Status == WorkItemStatus.Submitted));
        }

        /// <summary>
        /// Get OPEN request (draft/submitted) for request type
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<Request> GetOpenRequests(int id, RequestType type )
        {
            return await context.Requests.Where(x => x.EmployeeId == id && x.CompanyId == userResolverService.GetCompanyId() &&
                (x.RequestType == type) &&
                (x.Status == WorkItemStatus.Draft || x.Status == WorkItemStatus.Submitted))
                .Include(x => x.Attendance)
                .Include(x => x.Employee)
                .Include(x => x.DayOff)
                .Include(x => x.FileDatas)
                .Include(x => x.WorkItem)
                .Include(x => x.RequestDataChanges)
                .FirstOrDefaultAsync();
        }


        public async Task<List<RequestApprovalConfig>> GetRequestApprovalConfigs(int cmpId, RequestType type, int? dayOffId = null, int? step = null)
        {
            return await context.RequestApprovalConfigs.Where(a => a.CompanyId == cmpId && a.RequestType == type && (dayOffId == null || dayOffId == a.DayOffId) && (step == null || step == a.Step))
                .Include(a=> a.Employee)
                .Include(a=> a.EmployeeRole)
                .Include(a=> a.DayOff)
                .OrderByDescending(a => a.Step)
                .ToListAsync();
        }


        /// <summary>
        /// Process given request
        /// New request creation and update
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Request> ProcessRequest(ProcessRequestVm model)
        {
            model.CompanyId = userResolverService.GetCompanyId();
            bool isSingleDay = false;
            int totalDays = -1;
            if (model.RequestType != RequestType.Document &&
                model.RequestType != RequestType.Work_Change &&
                model.RequestType != RequestType.Attendance_Change)
            {
                // no date validation
                if (model.RequestType == RequestType.Overtime && model.AttendanceId.GetValueOrDefault() > 0) { }
                else
                {
                    if (!model.Start.HasValue || !model.End.HasValue)
                        throw new ApplicationException("Please enter start and end date to proceed");
                    isSingleDay = model.Start == model.End && model.Start != null && model.End != null;
                    totalDays = Convert.ToInt32((model.End.Value.Date - model.Start.Value.Date).TotalDays) + 1;
                }
            }


            Request request = null;
            if (model.Id > 0)
                request = await context.Requests
                    .Include(x => x.FileDatas)
                    .Include(x => x.Employee)
                    .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (request == null && model.Id > 0)
                throw new ApplicationException("Sorry! Request was not found");

            var empRequesting = await context.Employees.FindAsync(model.EmployeeId);
            if (empRequesting == null)
                throw new ApplicationException("Sorry! Requesting Employee was not found");


            bool isNewCreate = request == null;
            request = isNewCreate ? new Request
            {
                Status = WorkItemStatus.Draft,
                EmployeeId = model.EmployeeId,
                CompanyId = userResolverService.GetCompanyId(),
                CreationDate = DateTime.UtcNow,
                Employee= empRequesting,
            } : request;

            if (isNewCreate)
                request = await GenerateNextNumberAsync(request);

            if (model.Status == WorkItemStatus.Submitted)
            {
                request.SubmissionDate = DateTime.UtcNow;
                AddStatusUpdate(request, newStatus: WorkItemStatus.Submitted);
            }

            if (isNewCreate)
                AddStatusUpdate(request, newStatus: WorkItemStatus.Draft);


            request.RequestType = model.RequestType;
            switch (request.RequestType)
            {
                case RequestType.Leave:
                    var empDayOff = await context.DayOffEmployees.Where(x => x.DayOff.CompanyId == userResolverService.GetCompanyId() && x.DayOffId == model.DayOffId && x.Year == DateTime.Now.Year && x.EmployeeId == request.EmployeeId)
                        .Include(x => x.DayOff)
                        .FirstOrDefaultAsync();
                    DayOff dayOff = await context.DayOffs.Where(x => x.Id == model.DayOffId)
                        .FirstOrDefaultAsync();
                    if (dayOff == null)
                        throw new ApplicationException("Requested leave is not found!");
                    if (empDayOff == null)
                        throw new ApplicationException("Employee doesnt have this leave set");

                    if (!dayOff.CanRequestForBackDatedDays && model.Start.Value.Date < DateTime.Now.Date)
                        throw new ApplicationException("Sorry! Cannot request for history date");

                    Employee transferEmpl = null;
                    if (dayOff.RequireSubstitiute && !dayOff.RequireSubstitiuteOptional && !model.TransferredEmployeeId.HasValue)
                        throw new ApplicationException("Sorry! Leave requires tramsfer employee as mandatory");
                    else if (model.TransferredEmployeeId.HasValue)
                    {
                        transferEmpl = await employeeService.GetEmployeeById(model.TransferredEmployeeId.Value);
                        if (transferEmpl == null)
                            throw new ApplicationException("Selected transfer employee was not found!");
                    }

                    //if (!await context.Contracts.AnyAsync(a=> a.EmployeeId == userResolverService.GetEmployeeId() &&
                    //    a.StartDate < model.End && a.EndDate > model.Start && 
                    //    model.Start > a.StartDate && model.End < a.EndDate && a.IsActive))
                    //    throw new ApplicationException("Employee doesn't have an active contract within requested leave period");


                    if (await context.Requests.AnyAsync(a =>
                        a.EmployeeId == request.EmployeeId &&
                        model.Start < a.End && a.Start <= model.End && a.IsActive
                         && (a.Status != WorkItemStatus.Rejected)
                         && (model.Id <= 0 || model.Id != a.Id)))
                        throw new ApplicationException("Ouch! Looks like you have already have leave(s) requested for this period");

                    request.TotalDays = totalDays;
                    //model.DayOff = dayOff;

                    // check if we need to exclude public hollidays and weekend (when calculating total days)
                    if (dayOff.ExcludeForPublicHoliday)
                    {
                        // ignore public holidays
                        request.TotalDays -= await context.CompanyPublicHolidays.CountAsync(a => a.Id == request.CompanyId && a.Date >= model.Start && a.Date <= model.End);


                        // company public holidays
                        var weekends = await accountDbContext.CompanyAccounts.Where(a => a.Id == request.CompanyId).Select(a => a.DayOfWeekHolidays).FirstOrDefaultAsync();
                        int weekendCount = 0;
                        if (weekends != null)
                        {
                            for (DateTime dt = model.Start.Value.Date; dt <= model.End; dt = dt.AddDays(1.0))
                                if (weekends.Contains((int)dt.DayOfWeek))
                                    weekendCount++;

                            request.TotalDays -= weekendCount;
                        }
                    }
                    //throw new ApplicationException("Ouch! Ouch!");

                    if (isSingleDay)
                    {
                        //if (!await context.Attendances.
                        //    AnyAsync(x => x.EmployeeId == userResolverService.GetEmployeeId() &&
                        //    x.Date.Date == model.Start.Value.Date))
                        //    throw new ApplicationException($"Work is not found for the date (range) selected");
                    }

                    bool requireDoc = false;
                    //var attendanceOnStart = await context.Attendances.
                    //    Where(x => x.EmployeeId == userResolverService.GetEmployeeId() &&
                    //    x.Date.Date == model.Start.Value.Date)
                    //    .FirstOrDefaultAsync();
                    //if (attendanceOnStart == null)
                    //    throw new ApplicationException($"Oops! You do not have work on {request.Start.Value.ToString("ddd, dd MMM, yyyy")}");

                    if (request.TotalDays > empDayOff.TotalRemainingHours)
                        throw new ApplicationException("Employee doesn't have the no. of leave days requested");

                    if (dayOff.MustRequestBefore && dayOff.MustRequestBeforeAlert.HasValue)
                    {
                        var totalHrsBeforeMustApply = GetTotalHrsBefore(dayOff.MustRequestBeforeAlert.Value);
                        var actulaTotalHrsBefore = (model.Start.Value - DateTime.Now).TotalHours;

                        if (actulaTotalHrsBefore <= totalHrsBeforeMustApply)
                            throw new ApplicationException($"You cannot request for {dayOff.Name} now, time is already passed. Try request in {dayOff.MustRequestBeforeAlert?.GetDisplayName()}");
                    }


                    var rquiredDocs = dayOff.RequiredDocumentList?.Split(',').ToArray() ?? new string[0];
                    if (rquiredDocs.Any())
                    {
                        if (dayOff.RequiredDocumentForConseqetiveDays && dayOff.ConsquetiveDaysRequire > 0)
                        {
                            if (totalDays >= dayOff.ConsquetiveDaysRequire)
                                foreach (var doc in rquiredDocs)
                                {
                                    if (!request.FileDatas.Any(x => x.Name == doc && x.IsNameChangeable == false))
                                        request.FileDatas.Add(new FileData
                                        {
                                            Name = doc,
                                            IsNameChangeable = false,
                                            CompanyId = userResolverService.GetCompanyId()
                                        });

                                }
                        }
                        requireDoc = false;
                    }

                    request.DayOffId = model.DayOffId;
                    request.Start = model.Start;
                    request.End = model.End;
                    request.Status = model.Status;
                    if (transferEmpl != null)
                    {
                        request.TransferredEmployeeId = model.TransferredEmployeeId;
                        request.TransferredEmployeeName = transferEmpl.GetSystemName(userResolverService.GetClaimsPrincipal());
                    }

                    if (model.Id > 0)
                    {
                        context.Requests.Update(request);
                    }
                    else
                    {
                        //if (requireDoc == true)
                        //{
                        //    if (dayOff.RequiredDocuments && !string.IsNullOrWhiteSpace(dayOff.RequiredDocumentList))
                        //        request.FileDatas = dayOff.RequiredDocumentList.Split(',').Select(x => new FileData
                        //        {
                        //            Name = x,
                        //            IsNameChangeable = false,
                        //            CompanyId = userResolverService.GetCompanyId()
                        //        }).ToList();
                        //}

                        context.Requests.Add(request);
                    }

                    break;

                case RequestType.Overtime:
                    Attendance attn = null;
                    if (model.AttendanceId.HasValue && model.AttendanceId.Value > 0)
                        attn = await context.Attendances.FindAsync(model.AttendanceId);

                    if (attn != null)
                    {
                        // related to attedance
                        request.Start = attn.WorkStartTime;
                        request.End = attn.WorkEndTime;
                        request.Reason = model.Reason;
                        request.AttendanceId = attn.Id;
                        //request.total

                        await EnsureSingleOTRequestPerAttendanceAsync(request);
                        //if (context.Requests.Any(t=> t.Id != request.Id && t.AttendanceId == request.AttendanceId && t.RequestType == RequestType.Overtime))
                        //    throw new ApplicationException("OT request already exists for this attendance record");
                    }
                    else
                    {
                        if (!model._StartTime.HasValue || !model._EndTime.HasValue)
                            throw new ApplicationException("Please enter start and end time");

                        var start = model.Start.Value.Date;
                        var startDateTime = start.Add(model._StartTime.Value);
                        var endDateTime = model.IsOvertime ?
                                start.AddDays(1).Add(model._EndTime.Value) :
                                start.Add(model._EndTime.Value);

                        if (startDateTime > endDateTime)
                            throw new ApplicationException("Final start and end date time was incorrect");
                        //if (startDateTime < DateTime.Now)
                        //    throw new ApplicationException("Cannot make back dated request");

                        request.Start = startDateTime;
                        request.End = endDateTime;
                        request.Reason = model.Reason;
                    }

                    request.Status = model.Status;

                    if (model.Id > 0)
                        context.Requests.Update(request);
                    else
                        context.Requests.Add(request);

                    break;
                case RequestType.Attendance_Change:
                    Employee emp = null;
                    if ((model.NewCheckinTime.HasValue == false || model.NewCheckOutTime.HasValue == false) && model.IsTransferEmployee == false)
                        throw new ApplicationException($"New Start and end times cannot be empty");

                    emp = await context.Employees.FirstOrDefaultAsync(x => x.Department.CompanyId == model.CompanyId && x.Id == model.TransferredEmployeeId);
                    if (model.IsTransferEmployee && emp == null)
                        throw new ApplicationException($"Transferring employee was not found");

                    if (!await context.Attendances.AnyAsync(x => x.Id == model.AttendanceId && x.CompanyId == model.CompanyId))
                        throw new ApplicationException($"Oohhh! Attendance record was not found!");

                    if (context.Requests.Any(x => (model.Id == 0 || model.Id != x.Id) && x.Status == WorkItemStatus.Submitted
                    && x.AttendanceId == model.AttendanceId && x.CompanyId == model.CompanyId && x.RequestType == RequestType.Attendance_Change))
                        throw new ApplicationException($"Ooops! There is already some request sent for approval, please wait until it's completed");

                    //request.Start = startDateTime;
                    //request.End = endDateTime;
                    request.NewCheckinTime = model.NewCheckinTime;
                    request.NewCheckOutTime = model.NewCheckOutTime;
                    request.TransferredEmployeeId = model.TransferredEmployeeId;
                    request.IsTransferEmployee = model.IsTransferEmployee;
                    request.TransferredEmployeeName = emp?.NameDisplay;

                    request.Reason = model.Reason;
                    request.Status = model.Status;
                    if (model.Id > 0)
                        context.Requests.Update(request);
                    else
                    {
                        request.AttendanceId = model.AttendanceId;
                        context.Requests.Add(request);
                    }
                    break;
                case RequestType.Work_Change:
                    Employee empx = null;
                    if ((model.NewCheckinTime.HasValue == false || model.NewCheckOutTime.HasValue == false) && model.IsTransferEmployee == false)
                        throw new ApplicationException($"Checkin and Checkout times cannot be empty");

                    empx = await context.Employees.FirstOrDefaultAsync(x => x.Department.CompanyId == model.CompanyId && x.Id == model.TransferredEmployeeId);
                    if (model.IsTransferEmployee && empx == null)
                        throw new ApplicationException($"Transferring employee was not found");

                    if (!await context.WorkItems.AnyAsync(x => x.Id == model.WorkItemId && x.IsEmployeeTask == false))
                        throw new ApplicationException($"Oohhh! Work iten record was not found!");

                    if (context.Requests.Any(x => (model.Id == 0 || model.Id != x.Id) && x.Status == WorkItemStatus.Submitted
                    && x.AttendanceId == model.AttendanceId && x.CompanyId == model.CompanyId && x.RequestType == RequestType.Attendance_Change))
                        throw new ApplicationException($"Ooops! There is already some request sent for approval, please wait until it's completed");

                    request.NewCheckinTime = model.NewCheckinTime;
                    request.NewCheckOutTime = model.NewCheckOutTime;
                    request.TransferredEmployeeId = model.TransferredEmployeeId;
                    request.IsTransferEmployee = model.IsTransferEmployee;
                    request.TransferredEmployeeName = empx?.NameDisplay;
                    request.Reason = model.Reason;
                    request.Status = model.Status;

                    if (model.Id > 0)
                        context.Requests.Update(request);
                    else
                    {
                        request.WorkItemId = model.WorkItemId;
                        context.Requests.Add(request);
                    }
                    break;
                case RequestType.Work_Submission:
                    break;
                case RequestType.Holiday:
                    break;
                case RequestType.Document:
                    if (string.IsNullOrWhiteSpace(model.Reason))
                        throw new ApplicationException("Kindly specify reason for requesting for documents");

                    if (model.Documents == null)
                        throw new ApplicationException("Plese enter at least one document");
                    var documents = model.Documents.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                    if (documents.Count() <= 0)
                        throw new ApplicationException("Plese enter at least one document");


                    request.DocumentsDataArray = documents;
                    request.Reason = model.Reason;
                    request.Status = model.Status;
                    if (request.Status == WorkItemStatus.Submitted)
                    {
                        if (request.DocumentsData != null && request.DocumentsData.Any())
                            request.FileDatas.AddRange(request.DocumentsDataArray.Select(x => new FileData
                            {
                                Name = x,
                                IsNameChangeable = false,
                                CompanyId = userResolverService.GetCompanyId(),
                                IsUploaded = false
                            }).ToList());
                    }


                    if (model.Id > 0)
                        context.Requests.Update(request);
                    else
                        context.Requests.Add(request);

                    break;
                default:
                    break;
            }

            bool sendNotificatoin = model.Id <= 0;

            var affected = await context.SaveChangesAsync();

            return request;

            //if (sendNotificatoin)
            //{
            //    // send notification
            //    await hubContext.Clients.All.SendAsync("displayNotification");
            //}

            //if (request.RequestType == RequestType.Leave)
            //    return await UploadDocument(request.Id);

            //if (request.RequestType == RequestType.Attendance_Change || request.RequestType == RequestType.Work_Change || request.RequestType == RequestType.Document)
            //{
            //    if (affected > 0 && request.Status == WorkItemStatus.Submitted)
            //    {
            //        await notificationService.SendAsync(
            //            type: NotificationTypeConstants.RequestSubmittedForAction,
            //            obj: request,
            //            companyAccountId: request.CompanyId);

            //        //return RedirectToAction("ViewRequest", "Home", new { id = userResolverService.GetEmployeeId() });
            //    }

            //    return RedirectToAction(nameof(ViewRequest), new { id = request.Id });
            //}

            ////hubContext
            //return RedirectToAction("Requests", "Employee", new { id = request.EmployeeId });
        }

        public async Task EnsureSingleOTRequestPerAttendanceAsync(Request request)
        {
            if (await context.Requests.AnyAsync(t => t.Id != request.Id && t.AttendanceId == request.AttendanceId && t.RequestType == RequestType.Overtime))
                throw new ApplicationException("OT request already exists for this attendance record");
        }

        public async Task ProcessRequestAction(RequestActionVm model)
        {
            var request = await context.Requests
                .Include(x => x.DayOff)
                .Include(x => x.Employee)
                //.ThenInclude(x => x.Individual)
                .Include(x => x.FileDatas)
                .Include(x => x.WorkItem)
                .Include(x => x.Attendance)
                .Include(x => x.RequestDataChanges)
                    .ThenInclude(x => x.Nationality)
                .Include(x => x.RequestDataChanges)
                    .ThenInclude(x => x.EmergencyContactRelationship)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (request == null)
                throw new ApplicationException("Sorry! Request was not found");

            scheduleService.EnsureRequestCanTakeAction(request);

            var latestNotif = await notificationService.GetLatestNotificationByTypeAsync(getNotificaitonTypeForRequestType(request.RequestType), request.Id.ToString());
            var steps = await GetRequestApprovalConfigs(request.CompanyId, request.RequestType, request.DayOffId);

            if (model.IsApproved == false && string.IsNullOrWhiteSpace(model.ActionTakenReasonSummary)) 
                throw new ApplicationException("Please enter reason for the employee");

            var isLastStep = latestNotif.Step == steps?.Max(a => a.Step);
            if (!isLastStep)
            {
                // intermediate person (approving)
                //await notificationService.TakeActionAsync(latestNotif);

                // try
                // {
                    //var noti = await notificationService.GetLatestNotificationByTypeAsync(NotificationTypeConstants.RequestSubmittedForAction, request.Id.ToString());
                    var canProceed = await notificationService.CanTakeAction(latestNotif);
                    if (canProceed.status == false)
                        throw new ApplicationException(canProceed.errorMessage);

                    // if rejected --> then take action and finish approval
                    latestNotif.NotificationActionTakenType = model.IsApproved ? NotificationActionTakenType.Approved : NotificationActionTakenType.Rejected;
                    latestNotif.Remarks = request.Reason;

                    await notificationService.TakeActionAsync(latestNotif, isLastStep);
                // }
                // catch (ApplicationException ex)
                // {
                //     throw ex;
                // }
            }
            else
            {
                // if last step
                request.ActionTakenDate = DateTime.UtcNow;
                request.ActionTakenUserId = userResolverService.GetUserId();
                request.ActionTakenUserName = userResolverService.GetUserName();
                request.ActionTakenReasonSummary = model.ActionTakenReasonSummary;

                if (model.IsApproved == false)
                {
                    if (string.IsNullOrWhiteSpace(model.ActionTakenReasonSummary))
                        throw new ApplicationException("Please enter reason for the employee");

                    request.Status = WorkItemStatus.Rejected;
                    request.Reason = model.ActionTakenReasonSummary;
                    // await context.SaveChangesAsync();
                    // return;
                }
                else{
                    // approved
                    

                    request.Status = WorkItemStatus.Approved;
                    request.IsApproved = true;


                    switch (request.RequestType)
                    {
                        case RequestType.Leave:
                            if (request.DayOff == null)
                                throw new ApplicationException("Please choose leave type");

                            if (!request.Start.HasValue || !request.End.HasValue)
                                throw new ApplicationException("Please enter start and end date to proceed");

                            if (request.TotalDays <= 0)
                                throw new ApplicationException("Requested days does not seem right, cannot approve");

                            var year = request.Start?.Year;
                            var emplDayOffForYear = await context.DayOffEmployees
                                .Include(x => x.DayOffEmployeeItems)
                                .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId && x.Year == year && x.DayOffId == request.DayOffId);
                            if (emplDayOffForYear == null)
                                throw new ApplicationException("Employee does not have " + request.DayOff.Name);
                            if (emplDayOffForYear.TotalRemainingHours < request.TotalDays)
                                throw new ApplicationException("Employee does not have required days to process this leave");

                            // check for overlapping days
                            if (emplDayOffForYear.DayOffEmployeeItems.Any(x => x.Start >= request.Start && x.End <= request.End && x.Status == DayOffEmployeeItemStatus.Approved))
                                throw new ApplicationException("There area days already approved for the date / duration " + request.Duration);


                            emplDayOffForYear.DayOffEmployeeItems.Add(new DayOffEmployeeItem
                            {
                                RequestId = request.Id,
                                Status = DayOffEmployeeItemStatus.Approved,
                                Start = request.Start.Value,
                                End = request.End.Value,
                                IsCreatedFromRequest = true,
                                CreatedFromRequestId = request.Id,
                                TotalDays = request.TotalDays,
                                CreatedManually = false
                            });
                            emplDayOffForYear.TotalCollectedHours += request.TotalDays;
                            emplDayOffForYear.TotalRemainingHours -= request.TotalDays;

                            var affectedAttendances = await context.Attendances.Where(a =>
                                a.EmployeeId == request.EmployeeId &&
                                a.Date >= request.Start && a.Date <= request.End)
                            .ToListAsync();
                            if (affectedAttendances != null)
                            {
                                affectedAttendances.ForEach(a =>
                                {
                                    if (a.StatusUpdates == null)
                                        a.StatusUpdates = new List<AttendanceStatusUpdate>();
                                    a.StatusUpdates.Add(scheduleService.GetNewStatusUpdate(AttendanceStatus.EmployeeOnLeave));
                                    a.CurrentStatus = AttendanceStatus.EmployeeOnLeave;
                                });
                            }

                            //  var affectedAttendances = await context.Attendances.Where(a => a.IsPublished && a.EmployeeId == request.EmployeeId &&
                            //a.Date >= request.Start && a.Date <= request.End).ToListAsync();
                            //  affectedAttendances.ForEach(t=>
                            //  {
                            //      t.CurrentStatus = AttendanceStatus.
                            //  })
                            break;


                        case RequestType.Overtime:
                            if (request.Attendance != null)
                            {
                                await EnsureSingleOTRequestPerAttendanceAsync(request);
                            }
                            else
                            {
                                if (!request.Start.HasValue || !request.End.HasValue)
                                    throw new ApplicationException("Please enter start and end date to proceed");

                                if (request.Start.Value > request.End.Value)
                                    throw new ApplicationException("Start date cannot be greater than end date");
                                // without overlapping can create
                                // check if employee has more scheudles
                                if (await context.Attendances.AnyAsync(x => x.Date >= request.Start && x.Date <= request.End && x.EmployeeId == request.EmployeeId))

                                    // yes! then check for overlapping (can't create overlapping betwee work timees)
                                    if (await context.Attendances.AnyAsync(x =>
                                        request.Start < x.WorkEndTime && x.WorkStartTime < request.End))
                                        throw new ApplicationException("This record(s) will create overlapping attendance records, Kindly check work time");
                            }

                            var newAttn = new Attendance
                            {
                                WorkStartTime = request.Start.Value,
                                WorkEndTime = request.End.Value,
                                IsOvertime = true,
                                IsCreatedFromRequest = true,
                                CreatedFromRequestId = request.Id,
                                CurrentStatus = AttendanceStatus.Published,
                                IsPublished= true,
                                CompanyId = userResolverService.GetCompanyId(),
                                Month = request.Start.Value.Month,
                                Date = request.Start.Value.Date,
                                Year = request.Start.Value.Year,
                                Day = request.Start.Value.Day,
                                EmployeeId = request.EmployeeId,
                                ShiftColor = "overtime",
                                //CompanyWorkType = CompanyWorkType.FixedTime,
                            };

                            if (request.Attendance != null)
                            {
                                newAttn.CompanyWorkTimeId = request.Attendance.CompanyWorkTimeId;
                                newAttn.TotalHoursWorkedPerSchedule =  request.TotalHours =  request.Attendance.TotalHoursWorkedOutOfSchedule;
                                request.Attendance.IsOTAmountClaimed = true;

                            }

                            context.Attendances.Add(newAttn);

                            break;
                        case RequestType.Attendance_Change:
                            var attndance = await context.Attendances.FirstOrDefaultAsync(x => x.Id == request.AttendanceId && x.CompanyId == request.CompanyId);
                            if (attndance == null)
                                throw new ApplicationException($"Oohhh! Attendance record was not found!");

                            if (request.IsTransferEmployee == false)
                            {
                                if (!request.NewCheckinTime.HasValue || !request.NewCheckOutTime.HasValue)
                                    throw new ApplicationException("New clock in time was not found!");

                                if (!attndance.CheckInTime.HasValue)
                                    attndance.CheckInTime = request.CreationDate.Date;
                                if (!attndance.CheckOutTime.HasValue)
                                    attndance.CheckOutTime = request.CreationDate.Date;

                                attndance.IsCheckinUpdated = attndance.CheckInTime.Equals(request.NewCheckinTime.Value);
                                attndance.CheckInTime = attndance.CheckInTime.Value.Date.Add(request.NewCheckinTime.Value);

                                attndance.IsCheckOutUpdated = attndance.CheckOutTime.Equals(request.NewCheckOutTime.Value);
                                attndance.CheckOutTime = attndance.CheckOutTime.Value.Date.Add(request.NewCheckOutTime.Value);
                            }
                            else
                            {
                                if (!await context.Employees.AnyAsync(x => x.Department.CompanyId == request.CompanyId && x.Id == request.TransferredEmployeeId))
                                    throw new ApplicationException("Transferring employee was not found!");

                                attndance.EmployeeId = request.TransferredEmployeeId.Value;
                                attndance.IsTransferred = true;
                            }
                            break;
                        case RequestType.Work_Change:
                            var workItem = await context.WorkItems.FirstOrDefaultAsync(x => x.Id == request.WorkItemId);
                            if (workItem == null)
                                throw new ApplicationException($"Oohhh! Work item record was not found!");

                            if (request.IsTransferEmployee == false)
                            {
                                if (!request.NewCheckinTime.HasValue || !request.NewCheckOutTime.HasValue)
                                    throw new ApplicationException("New clock in time was not found!");

                                if (!workItem.CheckInTime.HasValue)
                                    workItem.CheckInTime = request.CreationDate.Date;
                                if (!workItem.CheckOutTime.HasValue)
                                    workItem.CheckOutTime = request.CreationDate.Date;
                                workItem.CheckInTime = workItem.CheckInTime.Value.Date.Add(request.NewCheckinTime.Value);

                                workItem.IsCheckinUpdated = workItem.CheckInTime.Equals(request.NewCheckinTime.Value);
                                workItem.IsCheckOutUpdated = workItem.CheckOutTime.Equals(request.NewCheckOutTime.Value);
                                workItem.CheckOutTime = workItem.CheckOutTime.Value.Date.Add(request.NewCheckOutTime.Value);
                            }
                            else
                            {
                                if (!await context.Employees.AnyAsync(x => x.Department.CompanyId == request.CompanyId && x.Id == request.TransferredEmployeeId))
                                    throw new ApplicationException("Transferring employee was not found!");

                                workItem.EmployeeId = request.TransferredEmployeeId.Value;
                                workItem.IsTransferred = true;
                            }
                            break;
                        case RequestType.Emp_DataChange:
                            var d =await GetChangedFieldsList(request, true);
                            request.JobActionHistories.Add(new JobActionHistory{
                                CompanyId = request.CompanyId, 
                                EmployeeId = request.EmployeeId,
                                //IndividualId = request.Employee.IndividualId,
                                IndividualName = request.Employee.GetSystemName(userResolverService.GetClaimsPrincipal()),
                                RelatedRequestId = request.Id,
                                RelatedRequestReference = request.NumberFormat,
                                ActionType = ActionType.DC,
                                OnDate = DateTime.UtcNow,
                                JobId = request.Employee.JobId,
                                JobIdString = request.Employee.JobIDString,
                                ChangeSetCount = d.Count,
                                Remarks = "Approval of Employee data change request",
                                JobActionHistoryChangeSets = d.Select(a=> new JobActionHistoryChangeSet{
                                    FieldName = a.Item1,
                                    OldValue = a.Item2?.ToString(),
                                    NewValue = a.Item3?.ToString(),
                                    ChangeState = a.Item4
                                }).ToList()
                            });
                            
                            break;
                        case RequestType.Work_Submission:
                            break;
                        case RequestType.Holiday:
                            break;
                        case RequestType.Document:

                            var totalRequired = request.DocumentsData.Count();

                            foreach (var item in request.DocumentsDataArray)
                            {
                                if (!request.FileDatas.Any(x => x.Name == item && x.IsUploaded && !x.IsNameChangeable))
                                {
                                    throw new ApplicationException("Please upload file for " + item);
                                }
                            }

                            if (request.FileDatas == null && totalRequired > 0)
                                throw new ApplicationException("Oops! there are no files uploaded");


                            var filesUplaoded = request.FileDatas.Select(x => x.Name + x.IsNameChangeable + x.IsUploaded).Distinct().Count();
                            var fileRequired = request.DocumentsData.Select(x => x + "False" + "True").Count();

                            if (filesUplaoded < fileRequired)
                                throw new ApplicationException("Please upload all requested files");

                            var totalUploadedNotNullName = request.FileDatas.Count(x => x.IsUploaded && !string.IsNullOrWhiteSpace(x.Name));
                            if (totalUploadedNotNullName < totalRequired)
                                throw new ApplicationException("Please upload pending documents, also set name for files");

                            break;
                        default:
                            throw new ApplicationException("Request Type was not found");
                            break;
                    }

                }


                try
                {
                    // var noti = await notificationService.GetLatestNotificationByTypeAsync(getNotificaitonTypeForRequestType(request.RequestType), request.Id.ToString());
                    var canProceed = await notificationService.CanTakeAction(latestNotif);
                    if (canProceed.status == false)
                        throw new ApplicationException(canProceed.errorMessage);

                    // take action same as request action and then send
                    latestNotif.NotificationActionTakenType = (NotificationActionTakenType)Enum.Parse(typeof(NotificationActionTakenType), request.Status.ToString());
                    latestNotif.Remarks = request.Reason;

                    await context.SaveChangesAsync();
                    await notificationService.TakeActionAsync(latestNotif, isLastStep: true);
                }
                catch (ApplicationException ex)
                {
                    throw ex;
                }
            }
        }

        public int getNotificaitonTypeForRequestType(RequestType requestType)
        {
            switch (requestType)
            {
                case RequestType.Leave:
                    return NotificationTypeConstants.RequestLeave;

                case RequestType.Overtime:
                case RequestType.Attendance_Change:
                case RequestType.Work_Change:
                case RequestType.Work_Submission:
                case RequestType.Holiday:
                case RequestType.Document:

                    return NotificationTypeConstants.RequestSubmittedForAction;
                    
                case RequestType.Emp_DataChange:
                    return NotificationTypeConstants.RequestEmployeeDataChange;
                    
                default:
                    throw new Exception("Request type is not defined!");
            }
        }

        public async Task<List<(string, string, string, bool?)>> GetChangedFieldsList(Request request, bool update = false)
        {
            if(request.Status == WorkItemStatus.Approved || request.Status == WorkItemStatus.Rejected){
                var dn =  await context.JobActionHistoryChangeSets.Where(x=> x.JobActionHistory.RelatedRequestId == request.Id).ToListAsync();
                var _changedFieldDIcst = new List<(string, string, string, bool?)>();
                foreach (var item in dn)
                    _changedFieldDIcst.Add((item.FieldName, item.OldValue, item.NewValue, item.ChangeState));

                return _changedFieldDIcst;
            }


            var reqDataChanges = request.RequestDataChanges.First();
            var emp = await context.Employees.Include(a => a.EmployeeAddresses)
                .FirstOrDefaultAsync(a => a.Id == request.EmployeeId);
            var empFieldsDict = accessGrantService.GetSelectFieldDisplayNameEmployee();
            //var indSelectFieldDict = accessGrantService.GetSelectFieldNameIndividual();
            var indAddressFIelds = accessGrantService.GetFieldNameDisplayNameDictFromType(typeof(Address));


            var empFieldValues = emp.GetType().GetProperties().ToDictionary(a => a.Name, a => a.GetValue(emp));
            //var indFieldValues = emp.Individual.GetType().GetProperties().ToDictionary(a => a.Name, a => a.GetValue(emp.Individual));
            var dataChangeFields = reqDataChanges.GetType().GetProperties().ToDictionary(a => a.Name, a => a.GetValue(reqDataChanges));
            //                                field,  old  ,  New   , state (add|1, remove|0, edit|null)
            var ChangedFieldDIcst = new List<(string, string, string, bool?)>();

            foreach (var _emF in empFieldsDict) // employee fields
            {
                if (dataChangeFields.ContainsKey(_emF.Key) && empFieldValues[_emF.Key]?.ToString() != dataChangeFields[_emF.Key]?.ToString())
                {
                    ChangedFieldDIcst.Add((empFieldsDict[_emF.Key], empFieldValues[_emF.Key]?.ToString(), dataChangeFields[_emF.Key]?.ToString(), FindChangeState(empFieldValues[_emF.Key], dataChangeFields[_emF.Key])));
                    if (update)

                        if (_emF.Key == nameof(emp._Nationality))
                            emp.GetType().GetProperty("NationalityId").SetValue(emp, dataChangeFields["NationalityId"]);
                    if (_emF.Key == nameof(emp._EmergencyContactRelationship))
                        emp.GetType().GetProperty("EmergencyContactRelationshipId").SetValue(emp, dataChangeFields["EmergencyContactRelationshipId"]);
                    else
                        emp.GetType().GetProperty(_emF.Key).SetValue(emp, dataChangeFields[_emF.Key]);
                }
            }

            //foreach (var _emF in indSelectFieldDict) // individual fields
            //{
            //    if (dataChangeFields.ContainsKey(_emF.Key) && indFieldValues[_emF.Key]?.ToString() != dataChangeFields[_emF.Key]?.ToString())
            //    {
            //        ChangedFieldDIcst.Add((indSelectFieldDict[_emF.Key], indFieldValues[_emF.Key]?.ToString(), dataChangeFields[_emF.Key]?.ToString(), FindChangeState(indFieldValues[_emF.Key], dataChangeFields[_emF.Key])));
            //        if (update)
            //        {
            //            if(_emF.Key == nameof(emp.Individual._Nationality))
            //                emp.Individual.GetType().GetProperty("NationalityId").SetValue(emp.Individual, dataChangeFields["NationalityId"]);
            //            if (_emF.Key == nameof(emp.Individual._EmergencyContactRelationship))
            //                emp.Individual.GetType().GetProperty("EmergencyContactRelationshipId").SetValue(emp.Individual, dataChangeFields["EmergencyContactRelationshipId"]);
            //            else
            //                emp.Individual.GetType().GetProperty(_emF.Key).SetValue(emp.Individual, dataChangeFields[_emF.Key]);
            //        }
            //    }
            //}

            if (!emp.EmployeeAddresses.Any()){
                emp.EmployeeAddresses.AddRange(new[] { new Address { AddressType = AddressType.Present }, new Address { AddressType = AddressType.Permanant } });
                context.EmployeeAddresses.AddRange(emp.EmployeeAddresses);
            }
            else if (emp.EmployeeAddresses.Any(a => a.AddressType == AddressType.NA))
            {
                emp.EmployeeAddresses.First().AddressType = AddressType.Present;
                emp.EmployeeAddresses.Last().AddressType = AddressType.Permanant;
            }

            // address, 1- Present, 2- Permanent
            var presetnt1 = emp.EmployeeAddresses.First().GetType().GetProperties().ToDictionary(a => a.Name, a => a.GetValue(emp.EmployeeAddresses.First()));
            var permenetn2 = emp.EmployeeAddresses.Last().GetType().GetProperties().ToDictionary(a => a.Name, a => a.GetValue(emp.EmployeeAddresses.Last()));

            // create ind addres iff available
            foreach (var _emF in indAddressFIelds)
            {
                if (dataChangeFields.ContainsKey(_emF.Key) && presetnt1[_emF.Key]?.ToString() != dataChangeFields[_emF.Key]?.ToString())
                {
                    ChangedFieldDIcst.Add(("Present " + indAddressFIelds[_emF.Key], presetnt1[_emF.Key]?.ToString(), dataChangeFields[_emF.Key]?.ToString(), FindChangeState(presetnt1[_emF.Key], dataChangeFields[_emF.Key])));
                    if(update)
                        emp.EmployeeAddresses.First().GetType().GetProperty(_emF.Key).SetValue(emp.EmployeeAddresses.First(), dataChangeFields[_emF.Key]);
                }
            }

            // create ind addres iff available
            foreach (var _emF in indAddressFIelds)
            {
                if (dataChangeFields.ContainsKey(_emF.Key + "1") && permenetn2[_emF.Key]?.ToString() != dataChangeFields[_emF.Key + "1"]?.ToString())
                {
                    ChangedFieldDIcst.Add(("Permanent " + indAddressFIelds[_emF.Key], permenetn2[_emF.Key]?.ToString(), dataChangeFields[_emF.Key + "1"]?.ToString(), FindChangeState(permenetn2[_emF.Key], dataChangeFields[_emF.Key])));
                    if (update)
                        emp.EmployeeAddresses.Last().GetType().GetProperty(_emF.Key).SetValue(emp.EmployeeAddresses.Last(), dataChangeFields[_emF.Key + "1"]);
                }
            }

            if (update){
                context.Employees.Update(emp);
                //context.Individuals.Update(emp.Individual);
                context.EmployeeAddresses.UpdateRange(emp.EmployeeAddresses);
            }
            return ChangedFieldDIcst;
        }

        public bool? FindChangeState(object old, object new1){
            if (!string.IsNullOrEmpty(new1?.ToString()) && string.IsNullOrWhiteSpace(old?.ToString()))
            return true;
            else if (string.IsNullOrEmpty(new1?.ToString()) && !string.IsNullOrWhiteSpace(old?.ToString()))
            return false;

            return null;
        }

        public async Task<RequestDataChange> FillWithExistingData(int empId)
        {

            //var reqDataChanges = request.RequestDataChanges.First();
            RequestDataChange reqDataChanges = new RequestDataChange();
            var emp = await context.Employees.Include(a => a.EmployeeAddresses)
                .FirstOrDefaultAsync(a => a.Id == empId);
            var empFieldsDict = accessGrantService.GetSelectFieldDisplayNameEmployee();
            //var indSelectFieldDict = accessGrantService.GetSelectFieldNameIndividual();
            var indAddressFIelds = accessGrantService.GetFieldNameDisplayNameDictFromType(typeof(Address));


            var empFieldValues = emp.GetType().GetProperties().ToDictionary(a => a.Name, a => a.GetValue(emp));
            //var indFieldValues = emp.Individual.GetType().GetProperties().ToDictionary(a => a.Name, a => a.GetValue(emp.Individual));


            foreach (var _emF in empFieldsDict) // employee fields
                if (_emF.Key == nameof(emp._Nationality))
                    reqDataChanges.GetType().GetProperty("NationalityId").SetValue(reqDataChanges, empFieldValues["NationalityId"]);
                else if (_emF.Key == nameof(emp._EmergencyContactRelationship))
                    reqDataChanges.GetType().GetProperty("EmergencyContactRelationshipId").SetValue(reqDataChanges, empFieldValues["EmergencyContactRelationshipId"]);
                else
                    reqDataChanges.GetType().GetProperty(_emF.Key)?.SetValue(reqDataChanges, empFieldValues[_emF.Key]);


            //foreach (var _emF in indSelectFieldDict) // individual fields
            //{
            //    if (_emF.Key == nameof(emp.Individual._Nationality))
            //        reqDataChanges.GetType().GetProperty("NationalityId").SetValue(reqDataChanges, indFieldValues["NationalityId"]);
            //    else if (_emF.Key == nameof(emp.Individual._EmergencyContactRelationship))
            //        reqDataChanges.GetType().GetProperty("EmergencyContactRelationshipId").SetValue(reqDataChanges, indFieldValues["EmergencyContactRelationshipId"]);
            //    else
            //        reqDataChanges.GetType().GetProperty(_emF.Key)?.SetValue(reqDataChanges, indFieldValues[_emF.Key]);
            //}

            if (!emp.EmployeeAddresses.Any())
                emp.EmployeeAddresses.AddRange(new[] { new Address { AddressType = AddressType.Present }, new Address { AddressType = AddressType.Permanant } });
            else if (emp.EmployeeAddresses.Any(a => a.AddressType == AddressType.NA))
            {
                emp.EmployeeAddresses.First().AddressType = AddressType.Present;
                emp.EmployeeAddresses.Last().AddressType = AddressType.Permanant;
            }

            // address, 1- Present, 2- Permanent
            var presetnt1 = emp.EmployeeAddresses.First().GetType().GetProperties().ToDictionary(a => a.Name, a => a.GetValue(emp.EmployeeAddresses.First())?.ToString());
            var permenetn2 = emp.EmployeeAddresses.Last().GetType().GetProperties().ToDictionary(a => a.Name, a => a.GetValue(emp.EmployeeAddresses.Last())?.ToString());

            // create ind addres iff available
            foreach (var _emF in indAddressFIelds)
                reqDataChanges.GetType().GetProperty(_emF.Key)?.SetValue(reqDataChanges, presetnt1[_emF.Key]);

            // create ind addres iff available
            foreach (var _emF in indAddressFIelds)
                reqDataChanges.GetType().GetProperty(_emF.Key + "1")?.SetValue(reqDataChanges, permenetn2[_emF.Key]);

            return reqDataChanges;
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


        public void AddStatusUpdate(Request request, WorkItemStatus newStatus)
        {
            var totalDurationInhrs = 0;
            var now = DateTime.UtcNow;
            if(newStatus != WorkItemStatus.Draft)
            {
                if (request.StatusUpdates != null)
                {
                    var lastStatusUpdate = request.StatusUpdates.OrderByDescending(x => x.ChangedDate).LastOrDefault();
                    if (lastStatusUpdate != null)
                    {
                        lastStatusUpdate.UpdateDate = now;
                        lastStatusUpdate.TotalDurationInHours = (lastStatusUpdate.UpdateDate - lastStatusUpdate.ChangedDate)?.TotalHours ?? 0;
                    }
                }
            }

            if (newStatus == WorkItemStatus.Draft && request.StatusUpdates == null)
                request.StatusUpdates = new List<RequestStatusUpdate>();

            request.StatusUpdates.Add(new RequestStatusUpdate
            {
                Status = newStatus,
                ChangedByName = userResolverService.GetUserName(),
                ChangedByUserId = userResolverService.GetUserId(),
                IsFromScheduler = false,
                ChangedDate = DateTime.UtcNow,
                StatusString = newStatus.ToString(),
            });
        }

        public async Task<Request> GenerateNextNumberAsync(Request request)
        {
            bool isNumberDefined = await accountDbContext.RequestNumberFormats.AnyAsync(a => a.CompanyAccountId == request.CompanyId && a.RequestType == a.RequestType);

            var nextNumber = await context.Requests.Where(x => x.CompanyId == request.CompanyId).OrderByDescending(a => a.CreationDate).Select(a => a.Number).FirstOrDefaultAsync();

            if (nextNumber <= 0)
                nextNumber = 1;
            else
                nextNumber += 1;
            var numberFormat = "";

            if(isNumberDefined)
            {
                var sequenceNumberTypeObj = await accountDbContext.RequestNumberFormats.FirstOrDefaultAsync(a => a.CompanyAccountId == request.CompanyId && a.RequestType == a.RequestType);

                int? year = sequenceNumberTypeObj.IsResetAnnually ? (int?)DateTime.Now.Year : null;

                // Create data to populate the format string.
                var data = new Dictionary<string, object>();
                data.Add("NextNumber", nextNumber);
                data.Add("Now", DateTime.Now);



                try
                {
                    // Format the sequence number.
                    // /{Now:yyyy}/{NextNumber:D5}
                    foreach (var item in sequenceNumberTypeObj.FormatString.Split("/"))
                    {
                        foreach (var d in data)
                        {
                            if(item.Contains(d.Key))
                            {
                                var _ = item.Replace(d.Key, "0");
                                numberFormat += "/" + string.Format(_, d.Value);
                            }
                        }
                    }

                    // add prefix
                    if (!string.IsNullOrEmpty(sequenceNumberTypeObj.Prefix))
                    {
                        numberFormat = sequenceNumberTypeObj.Prefix.Trim() + numberFormat;
                    }
                }
                catch (Exception ex)
                {
                    // ApplicationErrorLogger.Log(ex); TODO
                    throw new Exception("An error occured during sequence number formatting. Error has been logged.");
                }
            }
            else
            {
                numberFormat = "#" + nextNumber;
            }

            request.Number = nextNumber;
            request.NumberFormat = numberFormat;

            return
                request;
        }


        //public async Task<int> SendToNextApprovalLevel(Request obj)
        //{
        //    var requireSubstitute = await context.DayOffs.Where(a => a.Id == obj.DayOffId).Select(a => new { a.RequireSubstitiute, a.RequireSubstitiuteOptional })
        //        .FirstOrDefaultAsync();

        //    if (requireSubstitute.RequireSubstitiute && !obj.TransferredEmployeeId.HasValue)
        //        throw new ApplicationException("Leave requires tramsfer Employee and request doesn't have any!");

        //    var empId = obj.EmployeeId;
        //    var nextStep = await GetRequestApprovalConfigs(obj.CompanyId, obj.RequestType, obj.DayOffId, 1);
        //    if(nextStep != null)
        //    {
        //        var newInteraction = new EmployeeInteraction()
        //        {
        //            CompanyId = obj.CompanyId,
        //            RequestId = obj.Id,
        //            RequestingEmployeeId = obj.EmployeeId,
        //            EmployeeInteractionType = EmployeeInteractionType.RequestApproval,
        //            ToBeReceivedBy = nextStep.First().RequestProceessConfigActionBy,
        //        };

        //        switch (nextStep.First().RequestProceessConfigActionBy)
        //        {
        //            case RequestProceessConfigActionBy.EmployeesWithRole:
        //                newInteraction.EmployeesWithRoles = new[] { nextStep.First().EmployeeRole.Role };
        //                break;
        //            case RequestProceessConfigActionBy.SpecificEmployee:
        //                newInteraction.EmployeeId = nextStep.First().EmployeeId;
        //                break;
        //            case RequestProceessConfigActionBy.Supervisor:
        //                newInteraction.EmployeeId = await context.Employees.Where(a => a.Id == empId).Select(a => a.ReportingEmployeeId).FirstOrDefaultAsync();
        //                break;
        //            case RequestProceessConfigActionBy.SupervisorsSupervisor:
        //                newInteraction.EmployeeId = await context.Employees.Where(a => a.Id == empId).Select(a => a.ReportingEmployee.ReportingEmployeeId).FirstOrDefaultAsync();
        //                break;
        //            case RequestProceessConfigActionBy.AutoActionAfterHours:
        //                break;
        //            default:
        //                break;
        //        }
        //        switch (switch_on)
        //        {
        //            default:
        //        }

        //        context.EmployeeInteractions.Add(newInteraction);
        //        await context.SaveChangesAsync();

        //        // send email to relevant employee for interaction

        //        var emp = await employeeService.GetEmployeeById(obj.EmployeeId);
        //    }

        //}


        //public async Task<List<RequestApprovalConfig>> GetRequestApprovalConfigs(int cmpId, RequestType type, int? dayOffId = null, int? step = null)
        //{
        //    return await payrollDbContext.RequestApprovalConfigs.Where(a => a.CompanyId == cmpId && a.RequestType == type && a.DayOffId == (type ==
        //RequestType.Leave ? dayOffId : a.DayOffId) && (step == null || step == a.Step))
        //        .OrderByDescending(a => a.Step)
        //        .Include(a => a.EmployeeRole)
        //        .ToListAsync();
        //}

    }


}
