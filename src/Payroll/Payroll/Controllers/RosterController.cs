

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

    [EmployeeRoleAuthorize(Roles = new[] { Roles.Company.roster })]    
    public class RosterController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly AccountDbContext accountDbContext;
        private readonly CompanyService companyService;
        private readonly AccessGrantService accessGrantService;
        private readonly UserResolverService userResolverService;
        private readonly EmployeeService employeeService;
        private readonly ScheduleService scheduleService;
        private readonly NotificationService notificationService;

        public RosterController(PayrollDbContext context, AccountDbContext accountDbContext, CompanyService companyService, UserResolverService userResolverService, EmployeeService employeeService, ScheduleService scheduleService, NotificationService notificationService)
        {
            this.context = context;
            this.accountDbContext = accountDbContext;
            this.companyService = companyService;
            this.accessGrantService = accessGrantService;
            this.userResolverService = userResolverService;
            this.employeeService = employeeService;
            this.scheduleService = scheduleService;
            this.notificationService = notificationService;
        }

        public async Task<IActionResult> Index(int id)
        {
            var sch = await GetScheduleInDbAsync(id, false);
            if (sch == null)
                return RedirectToAction(nameof(All));

            var vm = GetRosterDataInTemp(sch);
            vm.Status = sch.Status;

            //vm.RecentSchedules = await context.Schedules.Where(x => x.CompanyId == userResolverService.GetCompanyId())
            //    .OrderByDescending(x => ((DateTime)context.Entry(x).Property("CreatedDate").CurrentValue))
            //    .Take(10).ToListAsync();
            //SaveRosterDataInTemp(vm);
            return View(vm);
        }

        public async Task<IActionResult> All(int id = 0, int limit = 10, int empId = 0, DateTime? date = null)
        {
            ViewBag.EmployeeIds = await employeeService.GetAllEmployeesInMyCompanyForDropdownOptGroups();
            var recentSchedules = await context.Schedules.Where(x => x.CompanyId == userResolverService.GetCompanyId()
                && (date == null || (date.Value >= x.Start && date.Value <= x.End))
                && (empId== 0 || x.EmployeeIds.Contains(empId)))
                .OrderByDescending(x => EF.Property<DateTime>(x, AuditFileds.CreatedDate))
                .Take(limit)
                //.Include(a=> a.EmployeeIds)
                //.Include(a=> a.WorkTimeIds)
                .ToListAsync();

            ViewBag.empId = empId;
            ViewBag.limit = limit;
            ViewBag.date = date?.ToString("dd-MMM-yyyy") ?? "";
            return View(recentSchedules);
        }

        public async Task<IActionResult> New(int id = 0)
        {

            //var vm = new RosterVm();
            var sch = new Schedule
            {
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddDays(20),
                SelectedMenu = RosterCreateLineItemType.AddInitialData,
                ScheduleFor = ScheduleFor.Roster,
            };
            TempData["RosterVm"] = null;
            var vm = GetRosterDataInTemp(sch);

            sch.CompanyId = userResolverService.GetCompanyId();
            await context.Schedules.AddAsync(sch);
            await context.SaveChangesAsync();

            vm.ScheduleId = sch.Id;
            SaveRosterDataInTemp(vm);
            return RedirectToAction(nameof(Index), new { id = sch.Id });
        }


        public async Task<IActionResult> CreateNode(int id = 0)
        {
            var sch = await GetScheduleInDbAsync(id);

            var NewSchedule = new Schedule
            {
                Start = sch.End.Value.AddDays(1),
                End = sch.End.Value.AddDays((sch.End.Value - sch.Start).TotalDays),
                DaysData = sch.DaysData.ToList(),
                CompanyId = userResolverService.GetCompanyId(),
                EmployeeIds = sch.EmployeeIds.ToArray(),
                EmployeeIdsData = sch.EmployeeIdsData.ToList(),
                WorkTimeIds = sch.WorkTimeIds.ToArray(),
                WorkTimeIdsData = sch.WorkTimeIdsData.ToList(),
                Slots = sch.Slots,
                ScheduleFor = sch.ScheduleFor,
                SelectedMenu = sch.SelectedMenu,
                ParentScheduleId = id,
                WorkItems = sch.WorkItems.ToList(),

                _ConseqetiveDays = sch._ConseqetiveDays,
                _Patten = sch._Patten,
                _PattenString = sch._PattenString,
                _TotalWorkingHoursPerWeek = sch._TotalWorkingHoursPerWeek
            };
            NewSchedule.CompanyId = userResolverService.GetCompanyId();

            await context.Schedules.AddAsync(NewSchedule);
            await context.SaveChangesAsync();

            //vm.ScheduleId = sch.Id;
            //SaveRosterDataInTemp(NewSchedule);
            return RedirectToAction(nameof(Index), new { id = NewSchedule.Id });
        }

        private void SaveRosterDataInTemp(RosterVm vm)
        {
            TempData["RosterVm"] = JsonConvert.SerializeObject(vm);
            TempData.Keep("RosterVm");
        }

        private RosterVm GetRosterDataInTemp(Schedule sch = null)
        {
            try
            {
                if (sch != null)
                    return new RosterVm(sch);
                return JsonConvert.DeserializeObject<RosterVm>(TempData["RosterVm"].ToString());
            }
            catch (Exception)
            {
                var init = new RosterVm(new Schedule { ScheduleFor = ScheduleFor.Roster });

                //var newSchedule = new Schedule
                //{
                //    SelectedMenu = RosterCreateLineItemType.AddInitialData,
                //    Start = init.StartDate,
                //    End = init.EndDate,
                //    EmployeeIds = init.EmployeeIds,
                //    EmployeeIdsData = init.Employees,

                //    WorkTimeIds = init.WorkTimeIds,
                //    WorkTimeIdsData = init.WorkTimes
                //};

                //newSchedule.CalculateSlots();
                init.CalculateSlots();
                
                SaveRosterDataInTemp(init);
                return init;
            }
        }

        public async Task<IActionResult> Employees(string term)
        {
            term = term.ToLower();
            var items = await context.Departments.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Employees.Any())
                .SelectMany(x => x.Employees)
                .Where(x=> x.GetSystemName(User).ToLower().Contains(term))
                .OrderBy(x => x.Department.DisplayOrder)
                .ThenBy(x => x.EmpID)
                .Select(x => new { x.Id, Text = x.NameDisplay })
                .Take(10)
                .ToArrayAsync();

            //var items = await accountDbContext.Users
            //    .Where(x => x.Email.Contains(term) || x.UserName.Contains(term) || x.PhoneNumber == term)
            //    .Select(x => new { x.Id, Text = $"<img src='{Url.Content(x.Picture ?? DefaultPictures.default_user)}' height='20px' /> {x.NameDisplay}" })
            //    .Take(10)
            //    .ToArrayAsync();

            return Json(items);
        }

        public async Task<IActionResult> AddDays(int schId, int id = 0, int t = 1)
        {
            // throw error
            var scheduleIndb = await GetScheduleInDbAsync(schId);

            var vm = GetRosterDataInTemp(scheduleIndb);
            if (vm == null)
                return ThrowJsonError("Roster was not found");
            var totalDays = (int)(vm.EndDate - vm.StartDate).Value.TotalDays;
            var totalSlots = totalDays * vm.WorkTimeIds.Length;
            vm.CalculateSlots();
            var days = new List<DayVm>();

            var emp = scheduleIndb.EmployeeIdsData.First(x => x.id == id);
            if (emp == null)
                return ThrowJsonError("Employee was not found!");

            ViewBag.Type = t == 1 ? "Requested Slots" : t == 2 ? "Black out Slots" : "Non working Shifts";
            ViewBag.TypeT = t;
            ViewBag.Emp = id;
            ViewBag.SchId = schId;
            ViewBag.Employee = emp.Name;



            int indz = 0;
            for (var start = vm.StartDate.AddDays(1); start <= vm.EndDate; start = start.AddDays(1))
            {
                days.Add(new DayVm
                {
                    DayOfWeek = start.DayOfWeek,
                    Date = start,
                    Day = start.Date.Day,

                    // each day 3 slot
                    WorkTimes = vm.WorkTimes.Select(w=> new _WorkTime {
                        IsSelected = t==1 ? emp.RequestedSlots[indz++] : emp.BlackOutSlots[indz++],
                        Name = w.Name})
                    .ToList()
                });
            }

            //switch (t)
            //{
            //    case 1:  // Requested Slots
            //        days.ForEach(day =>
            //        {
            //            day.WorkTimes.ForEach(wt => { wt.IsSelected = emp.RequestedSlots[indz++]; });
            //        });
            //        break;
            //    case 2:  // Black out Slots
            //        days.ForEach(day =>
            //        {
            //            day.WorkTimes.ForEach(wt => { wt.IsSelected = emp.BlackOutSlots[indz++]; });
            //        });
            //        break;
            //    case 3:  // Non working Shifts
            //        break;
            //    default:
            //        break;
            //}

            return PartialView("_AddDays", days);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDays(DayVm[] model)
        {
            var sId = Request.Form["sId"].ToString();
            var type = Request.Form["t"].ToString();
            var emp = Request.Form["emp"].ToString();

            int schId = 0;
            int.TryParse(sId, out schId);

            // throw error
            var scheduleIndb = await GetScheduleInDbAsync(schId);


            if (scheduleIndb.Slots != model.SelectMany(x => x.WorkTimes).Count())
                return ThrowJsonError("Provided data is invalid, Please try again");

            int empId = 0;
            int.TryParse(emp, out empId);
            var empRecord = scheduleIndb.EmployeeIdsData.FirstOrDefault(x => x.id == empId);
            var modelSlots= model.SelectMany(a => a.WorkTimes).Select(w => w.IsSelected).ToArray();
            switch (type)
            {
                case "1": // Requested Slots
                    empRecord.RequestedSlots = new bool[scheduleIndb.Slots];
                    for (int i = 0; i < modelSlots.Length; i++)
                    {
                        empRecord.RequestedSlots[i] = modelSlots[i];
                    }
                    break;
                case "2": // Black Out Slots
                    empRecord.BlackOutSlots = new bool[scheduleIndb.Slots];
                    for (int i = 0; i < modelSlots.Length; i++)
                    {
                        empRecord.BlackOutSlots[i] = modelSlots[i];
                    }
                    break;
                default:
                    break;
            }

            context.Schedules.Update(scheduleIndb);

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Process), new { id= sId, t = RosterCreateLineItemType.SetupEmployee });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveEmployee(int id, int schId)
        {
            var scheduleIndb = await GetScheduleInDbAsync(schId);

            var empIds = scheduleIndb.EmployeeIds.ToList();
            var emData = scheduleIndb.EmployeeIdsData.ToList();

            if (empIds.Any(x => x == id))
            {
                empIds.RemoveAll(x => x == id);
                scheduleIndb.EmployeeIds = empIds.ToArray();
            }

            if (emData.Any(x => x.id == id))
            {
                emData.RemoveAll(x => x.id == id);
                scheduleIndb.EmployeeIdsData = emData;
            }


            context.Schedules.Update(scheduleIndb);
            await context.SaveChangesAsync();

            return Ok();
        }

        #region Setup day week Patter

        public async Task<IActionResult> AddPatternWeek(int id)
        {
            // throw error
            var scheduleIndb = await GetScheduleInDbAsync(id, throwOnError: true);
            var _data = scheduleIndb.DaysData?.ToList();
            if (_data == null || _data.Any() == false)
            {
                _data= Enumerable.Range(1, 7)
                    .Select(a => new DayVm { Day = a })
                    .ToList();
            }
            else
            {
                var max = _data.Max(d => d.Day);
                _data.AddRange(Enumerable.Range((max + 1), 7)
                    .Select(a => new DayVm { Day = a })
                    .ToList());
            }
            scheduleIndb.DaysData = _data;

            await context.SaveChangesAsync();

            return await Process(id, RosterCreateLineItemType.SetRotatingPattern);
        }

        [HttpPost]
        public async Task<IActionResult> RemovePatternWeek(int id, int week)
        {
            // throw error
            var scheduleIndb = await GetScheduleInDbAsync(id, throwOnError: true);
            var _data = scheduleIndb.DaysData.ToList();
            var totalweeks = _data.Count() / 2;
            if (week > totalweeks)
                return ThrowJsonError("Week was not found");

            int skip = (week * 7) - 7;
            _data.RemoveRange(skip, 7);


            scheduleIndb.DaysData = _data;

            await context.SaveChangesAsync();

            return await Process(id, RosterCreateLineItemType.SetRotatingPattern);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveWholePattern(int id, int week)
        {
            // throw error
            var scheduleIndb = await GetScheduleInDbAsync(id, throwOnError: true);

            scheduleIndb.DaysData = null;

            await context.SaveChangesAsync();

            return await Process(id, RosterCreateLineItemType.SetRotatingPattern);
        }

        public async Task<IActionResult> AddShiftOnDay(int id, int day)
        {
            var scheduleIndb = await GetScheduleInDbAsync(id);
            ViewBag.sId = id;


            ViewBag.WorkTime = scheduleIndb.WorkTimeIdsData.ToList();
            ViewBag.WorkTimeCompany = await companyService.GetWorkTimes();
            

            var _day = scheduleIndb.DaysData.FirstOrDefault(a => a.Day == day);

            return PartialView("_AddShiftOnDay", _day);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddShiftOnDay(DayVm model)
        {
            var sId = Request.Form["sId"].ToString();

            int schId = 0;
            int.TryParse(sId, out schId);

            // throw error
            var scheduleIndb = await GetScheduleInDbAsync(schId);
            var _data = scheduleIndb.DaysData.ToList();

            var day = _data.FirstOrDefault(a => a.Day == model.Day);
            if (day != null)
                day.ShiftId = model.ShiftId;

            scheduleIndb.DaysData = _data;

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Process), new { id = sId, t = RosterCreateLineItemType.SetRotatingPattern });
        }


        [HttpPost]
        public async Task<IActionResult> RemoveWorkToShift(int id, int wtId, int wid)
        {

            var scheduleIndb = await GetScheduleInDbAsync(id);
            var _data = scheduleIndb.WorkTimeIdsData.ToList();

            var workTime = _data.FirstOrDefault(a => a.id == wtId);
            if (workTime == null) return ThrowJsonError("Work time was not found!");

            if (!_data.Any(a => a.id == wtId && a.WorkTimeWorkItems != null && a.WorkTimeWorkItems.Any(w => w.WorkId == wid)))
                return ThrowJsonError("Selected work to remove was not found!");

            var toRemove = workTime.WorkTimeWorkItems.FirstOrDefault(a => a.WorkId == wid);
            workTime.WorkTimeWorkItems.Remove(toRemove);


            scheduleIndb.WorkTimeIdsData = _data;

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Process), new { id = id, t = RosterCreateLineItemType.SetupShift });
        }

        public async Task<IActionResult> AddWorkToShift(int id, int wtId)
        {

            var scheduleIndb = await GetScheduleInDbAsync(id);
            var workTime = scheduleIndb.WorkTimeIdsData.FirstOrDefault(a => a.id == wtId);
            if (workTime == null) return ThrowJsonError("Work time was not found!");

            ViewBag.sId = id;
            ViewBag.wtId = wtId;
            ViewBag.WorkItem = workTime;

            var workItens = await companyService.GetCompanyWorks();
            ViewBag.WorkItems = new SelectList(workItens
                .ToDictionary(a => a.Id, a => a.Name + (a.Type == WorkType.ExpectClockInRecordsBefore ? "(c)" : "(s)")), "Key", "Value");

            ViewBag.Days = new SelectList(Enumerable.Range(0,6)
                .ToDictionary(a => a, a => (DayOfWeek)a), "Key", "Value");

            var lsitEmps = await companyService.GetEmployeesOfCurremtCompany();
            ViewBag.EmployeeIds = new MultiSelectList(lsitEmps
                .Where(a => scheduleIndb.EmployeeIds.Contains(a.Id)).Select(x => new { x.Id, Name = $"<img src='{Url.Content(x.Avatar ?? DefaultPictures.default_user)}' height='20px' /> {x.GetSystemName(User)}" }).ToList(), "Id", "Name");

            return PartialView("_AddWorkToShift", new WorkTimeWorkItem { WorkTimeId = wtId  });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorkToShift(WorkTimeWorkItem model)
        {
            var sId = Request.Form["sId"].ToString();

            int schId = 0, workTimeId = model.WorkTimeId;
            int.TryParse(sId, out schId);

            // throw error
            var scheduleIndb = await GetScheduleInDbAsync(schId);
            var _data = scheduleIndb.WorkTimeIdsData.ToList();

            var workTime = _data.FirstOrDefault(a => a.id == workTimeId);
            if (workTime == null) return ThrowJsonError("Work time was not found!");
            if (_data.Any(a=> a.WorkTimeWorkItems != null && a.WorkTimeWorkItems.Any(w=> w.WorkId == model.WorkId)))
                return ThrowJsonError("Selected work is already assigned to another shift or work time!");

            if (model.WorkableEmployeeIds == null || model.WorkableEmployeeIds.Length <= 0)
                return ThrowJsonError("Atleast one workable employee is required");

            var day = _data.FirstOrDefault(a => a.id == workTimeId);
            if (day != null)
            {
                if (day.WorkTimeWorkItems != null)
                    day.WorkTimeWorkItems.Add(model);
                else
                {
                    day.WorkTimeWorkItems = new List<WorkTimeWorkItem>();
                    day.WorkTimeWorkItems.Add(model);
                }

                day.MinEmployees = model.MinEmployees;
                day.MaxEmployees = model.MaxEmployees;

                if (model.MinEmployees <= 0)
                    day.MinEmployees = 1;
                if (model.MaxEmployees <= 0)
                    day.MaxEmployees = model.WorkableEmployeeIds.Length;

            }


            scheduleIndb.WorkTimeIdsData = _data;

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Process), new { id = sId, t = RosterCreateLineItemType.SetupShift });
        }

        [HttpPost]
        public async Task<IActionResult> ClearRosterGenereatedItems(int id)
        {
            var scheduleIndb = await GetScheduleInDbAsync(id);
            var attendances = await context.Attendances.Where(a => a.ScheduleId == id).ToListAsync();
            var workItgems = await context.WorkItems.Where(a => a.ScheduleId == id).ToListAsync();
            context.Attendances.RemoveRange(attendances);
            context.WorkItems.RemoveRange(workItgems);

            //scheduleIndb.IsGenerated = false;
            scheduleIndb.Status = ScheduleStatus.Draft;

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Process), new { id = id, t = RosterCreateLineItemType.Complete });
        }
        
        public async Task<IActionResult> GetShiftDetails(int id)
        {
            var scheduleIndb = await GetScheduleInDbAsync(id);
            if (scheduleIndb.Status == ScheduleStatus.Draft)
                return ThrowJsonError("Schedule was not found!");


            var attendances = await context.Attendances.Where(x => x.ScheduleId == scheduleIndb.Id && x.IsActive)
                .Include(a => a.Employee)
                .ThenInclude(a => a.Department)
                .GroupBy(a => new { a.CompanyWorkTimeId, a.Date })
                .Select(a => new
                {
                    ShiftId = a.Key.CompanyWorkTimeId,
                    Date = a.Key.Date,
                    //TotalAttendanceRecords = a.Count(),
                    Attedances = a.ToList(),
                    //TotalEmployees = a.Select(w => w.EmployeeId).Distinct().Count(),
                    //TotalEmployeesList = a.Select(w => w.Employee).ToList()
                })
                .ToListAsync();

            var list = new List<WeeklyEmployeeShiftVm>();


            foreach (var shift in scheduleIndb.WorkTimeIdsData)
            {
                var _ = new WeeklyEmployeeShiftVm { WorkTime = new CompanyWorkTime { Id= shift.id, ShiftName = shift.Name, ColorCombination = shift.Color } };
                for (DateTime start = scheduleIndb.Start; start < scheduleIndb.End; start = start.AddDays(1))
                {
                    if (attendances.Any(x => x.ShiftId== shift.id && x.Date == start.Date.Date))
                        _.Attendances.AddRange(attendances.FirstOrDefault(x => x.ShiftId == shift.id && x.Date == start.Date.Date).Attedances);


                    //if (attendances.Any(x => x.ShiftId == shift.id && x.Date == start.Date.Date)
                    //    _.Attendances.AddRange(attendances.FirstOrDefault(x => x.ShiftId == shift.id && x.Date == start.Date.Date).Attedances);

                    //if (workItems.Any(x => x.Date.Date == start.Date.Date && x.EmployeeId == shift.id))
                    //    _.WorkItems.AddRange(workItems.Where(x => x.Date.Date == start.Date.Date && x.EmployeeId == shift.id).ToList());
                }

                list.Add(_);
            }
            var vm = GetRosterDataInTemp(scheduleIndb);

            vm.WeekEmployeeShift = list;

            return PartialView("_Step6_ShiftDetail", vm);
        }

        public async Task<IActionResult> GetTaskDetails(int id)
        {
            var scheduleIndb = await GetScheduleInDbAsync(id);
            if (scheduleIndb.Status == ScheduleStatus.Draft)
                return ThrowJsonError("Schedule was not found!");

            var worktimes = await context.WorkItems.Where(x => x.ScheduleId == scheduleIndb.Id && x.IsActive)
                .Include(a => a.Work)
                .GroupBy(a => new { a.WorkId, a.Work.Name, a.Date })
                .Select(a => new
                {
                    Date = a.Key.Date,
                    WorkId = a.Key.WorkId,
                    WorkName = a.Key.Name,
                    //TotalAttendanceRecords = a.Count(),
                    WorkItems = a.ToList(),
                    //TotalEmployees = a.Select(w => w.EmployeeId).Distinct().Count(),
                    //TotalEmployeesList = a.Select(w => w.Employee).ToList()
                })
                .ToListAsync();

            var works = worktimes.SelectMany(a => a.WorkItems).Select(w => w.WorkId).Distinct().ToList();

            var list = new List<WorkItemScheduleVvm>();
            var cmpWorks = await companyService.GetCompanyWorks();


            foreach (var work in cmpWorks.Where(a=> works.Contains(a.Id)))
            {
                var _ = new WorkItemScheduleVvm { Works =new Work { Id = work.Id, Name= work.Name, ColorCombination = work.ColorCombination } };
                for (DateTime start = scheduleIndb.Start; start < scheduleIndb.End; start = start.AddDays(1))
                {
                    if (worktimes.Any(x => x.WorkId == work.Id && x.Date == start.Date.Date))
                        _.WorkItems.AddRange(worktimes.FirstOrDefault(x => x.WorkId == work.Id && x.Date == start.Date.Date).WorkItems);


                    //if (attendances.Any(x => x.ShiftId == shift.id && x.Date == start.Date.Date)
                    //    _.Attendances.AddRange(attendances.FirstOrDefault(x => x.ShiftId == shift.id && x.Date == start.Date.Date).Attedances);

                    //if (workItems.Any(x => x.Date.Date == start.Date.Date && x.EmployeeId == shift.id))
                    //    _.WorkItems.AddRange(workItems.Where(x => x.Date.Date == start.Date.Date && x.EmployeeId == shift.id).ToList());
                }

                list.Add(_);
            }
            var vm = GetRosterDataInTemp(scheduleIndb);

            vm.WeekEmployeeTask = list;

            return PartialView("_Step6_TaskDetail", vm);
        }
        #endregion

        #region Process - 
        
        [HttpPost]
        public async Task<IActionResult> NotifyEveryone(int id)
        {
            var scheduleInDb = await GetScheduleInDbAsync(id);
            if (scheduleInDb.Status == ScheduleStatus.Draft)
                return ThrowJsonError("Schedule was not generated");

            var status = await notificationService.SendToEmployeesAsync(NotificationTypeConstants.ScheduleSubmittedForVerification, scheduleInDb, scheduleInDb.CompanyId, scheduleInDb.EmployeeIds);

            if(!status)
                return ThrowJsonError("Unexpected error has occured while sending out notifications");

            var countOfMappedUserIds = await context.Employees.CountAsync(a => a.HasUserAccount && scheduleInDb.EmployeeIds.Contains(a.Id));

            var msg = "Notifications were sent out successfully";
            if (countOfMappedUserIds == 0)
            {
                msg = "No employees have user account associated with them, hence roster is approved";
            }
            else if (countOfMappedUserIds < scheduleInDb.EmployeeIds.Count())
                msg = "We could only send notifications to few of the employees as some employees do not have user account associated with them";

            SetTempDataMessage(msg);

            //var oldInteractions = scheduleInDb.Interactions.Where(a => scheduleInDb.EmployeeIds.Contains(a.EmployeeId)
            //    && a.ScheduleId == id & a.IsActive).ToList();
            //var _oldInterList = oldInteractions;
            //var newInteractions = scheduleInDb.EmployeeIds
            //    .Select(x => new EmployeeInteraction
            //    {
            //        ScheduleId = id,
            //        EmployeeId = x,
            //        IsActive = true,
            //        SentDate = DateTime.UtcNow,
            //        TriggerSource = NotificationTriggerSource.ScheduleVerification
            //    }).ToList();

            //// Ignore empls who not marked as recived
            //// basically add only those marked or not sent
            //if(_oldInterList.Count() > 0)
            //{
            //    foreach (var intera in _oldInterList)
            //    {
            //        if (intera.IsRecieved == false)
            //        {
            //            oldInteractions.Remove(intera);
            //            newInteractions.RemoveAll(a => a.EmployeeId == intera.EmployeeId);
            //        }
            //    }
            //}

            //if (newInteractions.Count() > 0)
            //    context.EmployeeInteractions.AddRange(newInteractions);
            //if (oldInteractions.Count > 0)
            //    context.EmployeeInteractions.RemoveRange(oldInteractions);

            //await context.SaveChangesAsync();

            return RedirectToAction(nameof(ViewInteractions), new { id = scheduleInDb.Id });

        }

        public async Task<IActionResult> ViewInteractions(int id)
        {
            var scheduleInDb = await GetScheduleInDbAsync(id);
            if (scheduleInDb.Status == ScheduleStatus.Draft)
                return ThrowJsonError("Schedule was not generated");

            var notifications = await notificationService.GetNotificationByTypeAsync(NotificationTypeConstants.ScheduleSubmittedForVerification, id.ToString());
            var data = notificationService.GetNotificationAnalyticsDictionary(notifications);
            ViewBag.Data = data;

            var rosterVm = GetRosterDataInTemp(scheduleInDb);
            rosterVm.Notifications = notifications;
            

            return PartialView("_ScheduleEmployeeInteractions", rosterVm);
        }

        [HttpPost]
        public async Task<IActionResult> PublishSchedule(int id)
        {
            var scheduleInDb = await GetScheduleInDbAsync(id);
            if (scheduleInDb.Status == ScheduleStatus.Draft)
                return ThrowJsonError("Schedule was not generated");


            var notifications = await notificationService.GetNotificationByTypeAsync(NotificationTypeConstants.ScheduleSubmittedForVerification, id.ToString());

            if (notifications.Count(x=> x.IsRead && x.NotificationActionTakenType == NotificationActionTakenType.Approved) != notifications.Count())
                return ThrowJsonError("All members do not consent with this schedule yet!");

            var attendances = await context.Attendances
                .Where(a => a.ScheduleId == id & a.IsActive)
                .ToListAsync();
            var WorkTimes = await context.WorkItems
                .Where(a => a.ScheduleId == id & a.IsActive)
                .ToListAsync();

            attendances.ForEach(a => { a.IsPublished = true; a.PublishedDate = DateTime.UtcNow; });
            WorkTimes.ForEach(a => { a.IsPublished = true; a.PublishedDate = DateTime.UtcNow; });
            scheduleInDb.Status = ScheduleStatus.Published;
            await context.SaveChangesAsync();
            return Ok("Published");
        }
        #endregion

        public async Task<IActionResult> Process(int id, RosterCreateLineItemType t 
            = RosterCreateLineItemType.AddInitialData)
        {
            //var scheduleIndb = await GetScheduleInDbAsync(id);

            var scheduleIndb = await GetScheduleInDbAsync(id, false);
            if (scheduleIndb == null && t != RosterCreateLineItemType.AddInitialData)
                return ThrowJsonError("Schedule was not found");


            var vm = GetRosterDataInTemp(scheduleIndb);


            vm.SelectedMenu = t;
            vm.Header = scheduleIndb.GetHeader(vm.SelectedMenu);

            switch (t)
            {
                case RosterCreateLineItemType.AddInitialData:
                    vm.Header = scheduleIndb.GetHeader(vm.SelectedMenu);

                    var lsitEmps = await companyService.GetEmployeesOfCurremtCompany();

                    var _empList = lsitEmps.Select(x => new { x.Id, Name = $"<img src='{Url.Content(x.Avatar ?? DefaultPictures.default_user)}' height='20px' /> {x.GetSystemName(User)}" }).ToList();

                    ViewBag.EmployeeIds = new MultiSelectList(_empList, "Id", "Name", vm?.EmployeeIds);

                    var departs = await context.Departments.Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .OrderBy(x => x.DisplayOrder)
                .Select(a => new { Id = a.Id, Name = a.Name, Empls = a.Employees.Select(b => b.Id).ToArray() })
                .ToListAsync();


                    ViewBag.WorkTimes = new MultiSelectList(await companyService.GetWorkTimes(), "Id", "ShiftName", vm?.WorkTimeIds);
                    ViewBag.Departments = departs;

                    return PartialView("_Step1", vm);

                case RosterCreateLineItemType.SetupEmployee:
                    vm.ActiveContracts = await employeeService.GetActiveEmploymentsDuring(scheduleIndb.Start, scheduleIndb.End.Value, scheduleIndb.EmployeeIds);

                    return PartialView("_Step3", vm);

                case RosterCreateLineItemType.SetupShift:
                    vm.Works = await companyService.GetCompanyWorks();

                    vm.EmployeeActiveContractFlag = scheduleIndb.HaveConflicts;
                    return PartialView("_Step4", vm);

                case RosterCreateLineItemType.SetRotatingPattern:
                    vm._Patten = scheduleIndb._Patten;
                    vm._PattenString = scheduleIndb._PattenString;
                    vm.DaysData = scheduleIndb.DaysData?.ToList() ?? new List<DayVm>();


                    //DateTime _start = date ?? DateTime.UtcNow.AddDays(-10);
                    //DateTime _endDate = end ?? DateTime.UtcNow.AddDays(20);


                    ViewBag.WorkTime = (await companyService.GetWorkTimes()).ToDictionary(a=> a.Id, a=> a);
                    //ViewBag.WeekStart = _start;
                    //ViewBag.WeekEnd = _endDate;

                    return PartialView("_Step2", vm);
                    
                case RosterCreateLineItemType.Summary:

                    ViewBag.DayOffs = await context.DayOffEmployeeItems.Where(a => a.Start >= scheduleIndb.Start &&
                    a.End <= scheduleIndb.End && a.IsActive && a.Status == DayOffEmployeeItemStatus.Approved
                    && scheduleIndb.EmployeeIds.Contains(a.DayOffEmployee.EmployeeId))
                    .ToListAsync();

                    ViewBag.AttendanceErrors = await context.Attendances.Where(x => // x.ScheduleId != scheduleIndb.Id &&
                    x.CompanyId == scheduleIndb.CompanyId &&
                    x.Date >= scheduleIndb.Start && x.Date <= scheduleIndb.End &&
                    scheduleIndb.EmployeeIds.Contains(x.EmployeeId))
                    .Include(x=> x.Employee)
                    .Include(x=> x.CompanyWorkTime)
                    .ToListAsync();

                    vm.DaysData = scheduleIndb.DaysData.ToList();
                    vm._ConseqetiveDays = scheduleIndb._ConseqetiveDays;
                    vm._TotalWorkingHoursPerWeek = scheduleIndb._TotalWorkingHoursPerWeek;
                    vm._Patten = scheduleIndb._Patten;
                    vm._PattenString = scheduleIndb._PattenString;


                    var _activeContracts = await employeeService.GetActiveEmploymentsDuring(scheduleIndb.Start, scheduleIndb.End.Value, scheduleIndb.EmployeeIds);
                    vm.EmployeeActiveContractFlag = scheduleIndb.EmployeeIdsData.ToList().TrueForAll(e =>
                        _activeContracts.Any(c => c.EmployeeId == e.id && c.IsActive));
                    return PartialView("_Overview", vm);

                case RosterCreateLineItemType.Complete:
                    //vm.IsRoseterGenerated = scheduleIndb.IsGenerated;
                  
                    if (true ) // scheduleIndb.IsGenerated)
                    {
                        List<WeeklyEmployeeShiftVm> list = await scheduleService.GetWeeklyEmployeeShiftVm(scheduleIndb);
                        vm.Notifications = await notificationService.GetNotificationByTypeAsync(NotificationTypeConstants.ScheduleSubmittedForVerification, id.ToString());
                        vm.CompanyWorkStartDay = await companyService.GetWorkStartDayOfWeek();
                        vm.WeekEmployeeShift = list;
                        vm.RosterGeneratedDate = scheduleIndb.RosterGeneratedDate;
                    }
                    return PartialView("_Step6", vm);

                case RosterCreateLineItemType.Error:
                    break;
                default:
                    break;
            }

            return ThrowJsonError();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(RosterVm model)
        {
            var scheduleIndb = await GetScheduleInDbAsync(model.ScheduleId, false);
            if (scheduleIndb == null)
                return ThrowJsonError("Schedule was not found");

            var rosterVm = GetRosterDataInTemp(scheduleIndb);

            switch (model.SelectedMenu)
            {
                case RosterCreateLineItemType.AddInitialData:
                    #region Add Initial Data 

                    if (!model.EndDate.HasValue || model.StartDate > model.EndDate)
                        return ThrowJsonError("Time period is invalid, Kindly check the input valies");

                    if (model.EmployeeIds == null || model.EmployeeIds.Count() < 0)
                        return ThrowJsonError("Please choose at least one Employee");

                    if (model.WorkTimeIds == null || model.WorkTimeIds.Count() < 0)
                        return ThrowJsonError("Please choose at least one Work Time");
                    if (string.IsNullOrWhiteSpace(model.Name))
                        return ThrowJsonError("Please enter name for your roster");

                    //var lsitEmps = await companyService.GetEmployeesOfCurremtCompany();
                    //ViewBag.EmployeeIds = new MultiSelectList(lsitEmps.Select(x => new { x.Id, Name = x.NameDisplay }).ToList(), "Id", "Name", scheduleIndb.EmployeeIds);

                    //ViewBag.WorkTimes = new MultiSelectList(await companyService.GetWorkTimes(), "Id", "ShiftName", scheduleIndb.WorkTimeIds);
                    var workTimes = await companyService.GetWorkTimes();
                    scheduleIndb.Start = model.StartDate.Date;
                    scheduleIndb.End = model.EndDate.Value.Date;

                    scheduleIndb.EmployeeIds = model.EmployeeIds;
                    scheduleIndb.WorkTimeIds = model.WorkTimeIds;
                    var _workTimedata = workTimes.Where(x => model.WorkTimeIds.Contains(x.Id))
                                    .Select((x, i) => new _WorkTime(i, x.Id, x.ShiftName, x.ColorCombination))
                                    .ToList();
                    var _empls = await context.Employees
                                    .Where(x => model.EmployeeIds.Contains(x.Id))
                                    .Select(x => new EmployeeSummaryVm(x.Id, x.GetSystemName(User), x.Avatar, x.Department.Name))
                                    .ToArrayAsync();

                    var activeContracts = await employeeService.GetActiveEmploymentsDuring(scheduleIndb.Start, scheduleIndb.End.Value, scheduleIndb.EmployeeIds);


                    var _empData = new List<_Employee>();
                    for (int i = 0; i < _empls.Length; i++)
                        _empData.Add(new _Employee(i, _empls[i].Id, _empls[i].FullName, _empls[i].Avatar, _empls[i].Department));



                    scheduleIndb.HaveConflicts = await context.Attendances.Where(x => x.ScheduleId != scheduleIndb.Id &&
                      x.CompanyId == userResolverService.GetCompanyId() &&
                      x.Date >= scheduleIndb.Start && x.Date <= scheduleIndb.End &&
                      scheduleIndb.EmployeeIds.Contains(x.EmployeeId))
                      .Include(x => x.Employee)
                      .CountAsync() > 0;


                    scheduleIndb.EmployeeIdsData = _empData.ToList();
                    scheduleIndb.WorkTimeIdsData = _workTimedata.ToList();
                    //rosterVm.CalculateSlots();


                    scheduleIndb.Slots = scheduleIndb.GetCalculateSlots();
                    scheduleIndb.SelectedMenu = RosterCreateLineItemType.SetRotatingPattern;

                    var activeEmployments = await employeeService.GetActiveEmploymentsDuring(scheduleIndb.Start, scheduleIndb.End.Value, scheduleIndb.EmployeeIds);
                    scheduleIndb.HaveConflicts = scheduleIndb.EmployeeIds.ToList().TrueForAll(e =>
                        activeEmployments.Any(c => c.EmployeeId == e && c.IsActive));

                    scheduleIndb.Name = model.Name;
                    context.Schedules.Update(scheduleIndb);

                    await context.SaveChangesAsync();

                    //rosterVm.ScheduleId = scheduleIndb.Id;
                    SaveRosterDataInTemp(rosterVm);

                    return RedirectToAction(nameof(Process), new { id = scheduleIndb.Id, t = scheduleIndb.SelectedMenu });


                #endregion

                case RosterCreateLineItemType.SetupEmployee:

                    scheduleIndb.SelectedMenu = RosterCreateLineItemType.SetupShift;

                    context.Schedules.Update(scheduleIndb);
                    await context.SaveChangesAsync();
                    ////scheduleIndb.End = model.WorkTimes;

                    //SaveRosterDataInTemp(rosterVm);
                    return RedirectToAction(nameof(Process), new { id = model.ScheduleId, t = scheduleIndb.SelectedMenu });

                case RosterCreateLineItemType.SetRotatingPattern:
                    if (scheduleIndb.DaysData.Count() < 7)
                        return ThrowJsonError("Minimun of one week pattern is required");

                    // loop throw form body (sent from client)
                    var daysData = scheduleIndb.DaysData.ToList();
                    var totalWeeks = daysData.Count / 7;
                    char[] patternWeek = new char[(daysData.Count + (totalWeeks - 1))];
                    var cmpWtimes = await companyService.GetWorkTimes();
                    var ii = 0;
                    double totalWorkingHoursPerWeek = 0;
                    for (int i = 0; i < daysData.Count; i++)
                    {
                        if (i % 7 == 0 && i != 0)
                            patternWeek[ii++] = "|".ToCharArray()[0];

                        if (daysData[i].ShiftId == 0)
                            patternWeek[ii++] = "-".ToCharArray()[0];
                        else
                        {
                            patternWeek[ii++] = cmpWtimes.First(a => a.Id == daysData[i].ShiftId).ShiftName.ToUpper().ToCharArray(0, 1)[0];
                            totalWorkingHoursPerWeek += cmpWtimes.First(a => a.Id == daysData[i].ShiftId).TotalHours;
                        }
                    }

                    var pattern = patternWeek.ToString();
                    scheduleIndb._Patten = patternWeek;
                    scheduleIndb._PattenString = new string(patternWeek);

                    var totalWorkingDays = daysData.Count(a => a.ShiftId != 0);
                    scheduleIndb._ConseqetiveDays = totalWorkingDays / totalWeeks;
                    scheduleIndb._TotalWorkingHoursPerWeek = totalWorkingHoursPerWeek / totalWeeks;

                    scheduleIndb.SelectedMenu = RosterCreateLineItemType.SetupEmployee;

                    context.Schedules.Update(scheduleIndb);
                    await context.SaveChangesAsync();
                    //scheduleIndb.End = model.WorkTimes;

                    SaveRosterDataInTemp(rosterVm);
                    return RedirectToAction(nameof(Process), new { id = model.ScheduleId, t = scheduleIndb.SelectedMenu });

                case RosterCreateLineItemType.SetupShift:
                    //if (model.WorkTimes == null || model.WorkTimes.Count() <= 0)
                    //    return ThrowJsonError("No data was received from client");

                    //if (scheduleIndb.WorkTimeIdsData?.Any() ?? false)
                    //{
                    //    var allworkTimes = scheduleIndb.WorkTimeIdsData.Where(a => a.WorkTimeWorkItems.Any())
                    //        .SelectMany(a => a.WorkTimeWorkItems).ToList();


                    //    // take in all empls
                    //    var empls = scheduleIndb.EmployeeIdsData.ToList();
                    //    empls.ForEach(emp =>
                    //    {
                    //        if (allworkTimes.Any(a => a.WorkableEmployeeIds.Contains(emp.id)))
                    //        {
                    //            emp.ShiftWorkPointers = allworkTimes.Where(a => a.WorkableEmployeeIds.Contains(emp.id)).Select(a => a.guid).Distinct().ToArray();
                    //            emp.WorkableTaskIds = allworkTimes.Where(a => a.WorkableEmployeeIds.Contains(emp.id)).Select(a => a.WorkId).Distinct().ToArray();
                    //        }
                    //    });

                    //    scheduleIndb.EmployeeIdsData = empls;
                    //}


                    //// loop throw form body (sent from client)
                    //model.WorkTimes.ForEach(emp =>
                    //{
                    //    var schRecord = scheduleIndb.WorkTimeIdsData.FirstOrDefault(x => x.id == emp.id);
                    //    if (schRecord != null)
                    //    {
                    //        schRecord.MinEmployees = emp.MinEmployees;
                    //        schRecord.MaxEmployees = emp.MaxEmployees;
                    //    }
                    //});


                    scheduleIndb.SelectedMenu = RosterCreateLineItemType.Summary;

                    context.Schedules.Update(scheduleIndb);
                    await context.SaveChangesAsync();


                    SaveRosterDataInTemp(rosterVm);
                    return RedirectToAction(nameof(Process), new { id = model.ScheduleId, t = scheduleIndb.SelectedMenu });
                    

                //case RosterCreateLineItemType.SetupContracts:
                //    break;
                //case RosterCreateLineItemType.Penalties:
                //    break;

                case RosterCreateLineItemType.Summary:
                    scheduleIndb.SelectedMenu = RosterCreateLineItemType.Complete;
                    context.Schedules.Update(scheduleIndb);
                    await context.SaveChangesAsync();
                    return RedirectToAction(nameof(Process), new { id = model.ScheduleId, t = scheduleIndb.SelectedMenu });

                case RosterCreateLineItemType.Error:
                    break;
                default:
                    break;
            }


            return ThrowJsonError();
        }

        private async Task<Schedule> GetScheduleInDbAsync(int id, bool throwOnError = true)
        {
            var scheduleInDb = await context.Schedules.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Id == id)
                .Include(x=> x.ParentSchedule)
                .Include(x=> x.FololwingSchedules)
                //.Include(x=> x.WorkTimeIdsData)
          .FirstOrDefaultAsync();
            if (scheduleInDb == null && throwOnError) throw new ApplicationException("Schedule was not found!");

            return scheduleInDb;
        }

        /// <summary>
        /// Generate roster for all employees and shifts taking care of employee contracts, holidays, day offs 
        /// and also rules set by ROSTER Config
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> StartRosteringDone(int id)
        {
            var scheduleInDb = await GetScheduleInDbAsync(id);
            try
            {
                scheduleInDb.EnsureCanProceedRostering();

                var canGenerate = await context.Attendances.Where(x => // x.ScheduleId != scheduleInDb.Id &&
                  x.CompanyId == userResolverService.GetCompanyId() &&
                  x.Date >= scheduleInDb.Start && x.Date <= scheduleInDb.End &&
                  scheduleInDb.EmployeeIds.Contains(x.EmployeeId))
                  .Include(x => x.Employee)
                  .CountAsync() <= 0;
                if (canGenerate == false)
                {
                    if (scheduleInDb.HaveConflicts == false)
                    {
                        scheduleInDb.HaveConflicts = true;
                        await context.SaveChangesAsync();
                    }


                    return ThrowJsonError("Conflicts are found, Please check summary");
                }

            }
            catch (ApplicationException e)
            {
                return ThrowJsonError(e.Message);
            }


            var workTimesData = scheduleInDb.WorkTimeIdsData;
            var workTimeWorkItems = workTimesData.Where(a=> a.WorkTimeWorkItems != null)
                .SelectMany(a => a.WorkTimeWorkItems).ToDictionary(a=> a.guid, a=> a);

            var cmpWorkTimes = await companyService.GetWorkTimes();
            var empls = scheduleInDb.EmployeeIdsData.ToList();
            var totalDays = (scheduleInDb.End - scheduleInDb.Start).Value.TotalDays;
            var weeks = scheduleInDb._PattenString.Split('|').Count();
            var totalPatternLen = scheduleInDb.DaysData.Count();
            var indxByWeek = new int[7];
            var ii = 0;


            // get all employee contracts
            var _activeEmpls = await employeeService.GetActiveEmploymentsDuring(scheduleInDb.Start, scheduleInDb.End.Value, scheduleInDb.EmployeeIds);
            bool DoesEmplHasActiveContract = scheduleInDb.EmployeeIdsData.ToList().TrueForAll(e =>
                _activeEmpls.Any(c => c.EmployeeId == e.id && c.IsActive));

            var _emplsWithActiveContracts = empls.Where(e => _activeEmpls.Any(c => c.EmployeeId == e.id)).ToList();

            // set employee start index of pattern
            // for this schdule selected employees
            if (scheduleInDb.ParentSchedule != null)
            {
                foreach (var emp in empls)
                {
                    if (!_activeEmpls.Any(c => c.EmployeeId == emp.id))
                        continue;


                    if (scheduleInDb.ParentSchedule.EmployeeIdsData.Any(a => a.id == emp.id))
                        emp._PatternStartIndex = GetNextPatternIndex(scheduleInDb.ParentSchedule.EmployeeIdsData.FirstOrDefault(a => a.id == emp.id)._PatternEndIndex,
                            totalPatternLen);
                    else { 
                        emp._PatternStartIndex = ii;
                    ii = GetPatternStart(ii, scheduleInDb.DaysData);}

                }
            }
            else
            {
                foreach (var emp in empls)
                {
                    if (!_activeEmpls.Any(c => c.EmployeeId == emp.id))
                        continue;
                    emp._PatternStartIndex = ii;
                    ii = GetPatternStart(ii, scheduleInDb.DaysData);
                }
            }


            var mIndx = 0;
            List<Attendance> list = new List<Attendance>();
            List<WorkItem> listWorkTimes = new List<WorkItem>();
            var employeeLastIndex = scheduleInDb.EmployeeIds.ToDictionary(x => x, x => 0);
            foreach (var empl in empls)
            {
                if (!_activeEmpls.Any(c => c.EmployeeId == empl.id))
                    continue;

                mIndx = empl._PatternStartIndex;
                //var emplPointers = empl.ShiftWorkPointers;
                for (DateTime start = scheduleInDb.Start.Date; start.Date <= scheduleInDb.End.Value.Date; start = start.AddDays(1))
                {
                    var workTime = scheduleInDb.DaysData[mIndx];
                    var cmpWorkTime = cmpWorkTimes.FirstOrDefault(a => a.Id == workTime.ShiftId);

                    employeeLastIndex[empl.id] = mIndx;

                    if (workTime.ShiftId != 0 && _activeEmpls.Any(c=> c.EmployeeId == empl.id && start.Date > c.EffectiveDate && start.Date < c.GetEndDate()))
                    {
                        var workEndDateCalc = cmpWorkTime.EndTime.Hours < cmpWorkTime.StartTime.Hours ? start.AddDays(1) : start;

                        list.Add(new Attendance
                        {
                            ScheduleId = scheduleInDb.Id,
                            EmployeeId = empl.id,
                            //CompanyWorkType = cmpWorkTimes.FirstOrDefault(a=> a.Id == workTime.ShiftId).,
                            CompanyWorkTimeId = workTime.ShiftId,
                            ShiftId = workTime.ShiftId,
                            ShiftName = cmpWorkTimes.FirstOrDefault(a => a.Id == workTime.ShiftId).ShiftName,
                            ShiftColor = cmpWorkTimes.FirstOrDefault(a => a.Id == workTime.ShiftId).ColorCombination,
                            CurrentStatus = AttendanceStatus.Created,
                            Date = start.Date,
                            Day = start.Day,
                            Month = start.Month,
                            Year = start.Year,
                            //Week = GetWeeekOfYear(start),
                            WorkStartTime = new DateTime(start.Year, start.Month, start.Day, cmpWorkTime.StartTime.Hours, cmpWorkTime.StartTime.Minutes, cmpWorkTime.StartTime.Seconds),
                            WorkEndTime = new DateTime(workEndDateCalc.Year, workEndDateCalc.Month, workEndDateCalc.Day, cmpWorkTime.EndTime.Hours, cmpWorkTime.EndTime.Minutes, cmpWorkTime.EndTime.Seconds),
                            CompanyId = userResolverService.GetCompanyId(),
                            StatusUpdates = new List<AttendanceStatusUpdate> { scheduleService.GetNewStatusUpdate(AttendanceStatus.Created) },
                            bySchduler = false,
                            IsPublished = false,
                        });
                    }
                    
                    mIndx = GetNextPatternIndex(mIndx, totalPatternLen);
                }
            }



            // generate tasks
            var cmpWorks = await companyService.GetCompanyWorks();
            var rnd = new Random();
            foreach (var shift in workTimesData)
            {
                if (shift.WorkTimeWorkItems == null)
                    continue;

                // loop each shift rules for assigning tasks
                foreach (var work in shift.WorkTimeWorkItems)
                {
                    var _work = cmpWorks.FirstOrDefault(x => x.Id == work.WorkId);
                    var _list = list.Where(a => a.CompanyWorkTimeId == shift.id && work.WorkableEmployeeIds.Contains(a.EmployeeId)).ToList();
                    if (work.IsForWholeWeek == false && work.OnDays != null && work.OnDays.Count > 0)
                        _list = _list.Where(a => work.OnDays.Contains(a.Date.DayOfWeek)).ToList();

                    foreach (var attn in _list)
                    {
                        var workEndDateCalc = _work.EndTime.Hours < _work.StartTime.Hours ? attn.Date.AddDays(1) : attn.Date;
                        try
                        {
                            var _ = rnd.Next(0, work.MaxEmployees - work.MinEmployees);
                            if (_ == 0)
                                listWorkTimes.Add(new WorkItem
                                {
                                    ScheduleId = scheduleInDb.Id,
                                    EmployeeId = attn.EmployeeId,
                                    HasAttendance = false,
                                    Status = WorkItemStatus.Draft,
                                    Date = attn.Date,
                                    Day = attn.Day,
                                    Month = attn.Month,
                                    Year = attn.Year,
                                    //Week = GetWeeekOfYear(start),
                                    WorkId = work.WorkId,
                                    TaskName = _work.Name,
                                    //WorkColor = work.ColorCombination,
                                    RemainingSubmissions = _work.TotalRequiredCount,

                                    WorkStartTime = new DateTime(attn.Date.Year, attn.Date.Month, attn.Date.Day, _work.StartTime.Hours, _work.StartTime.Minutes, _work.StartTime.Seconds),
                                    WorkEndTime = new DateTime(attn.Date.Year, attn.Date.Month, attn.Date.Day, _work.EndTime.Hours, _work.EndTime.Minutes, _work.EndTime.Seconds),

                                    DueDate = new DateTime(workEndDateCalc.Year, workEndDateCalc.Month, workEndDateCalc.Day, 23, 59, 59),
                                    //TotalAllowance = workTime.TotalRequiredCount * workTime.de
                                    //WorkEndTime = new DateTime(start.Year, start.Month, start.Day, workTime.EndTime.Hours, workTime.EndTime.Minutes, workTime.EndTime.Seconds),
                                });
                        }
                        catch (Exception)
                        {
                        }
                    }

                    //var selectedEmpls = rnd.Next(work.MinEmployees, work.MaxEmployees);
                    //var listSeel = work.WorkableEmployeeIds.OrderBy(a => rnd.Next()).Take(selectedEmpls).ToArray();
                    //for (int i = 0; i < listSeel.Length; i++)
                    //{
                    //    list.Where(a=> a.s)
                    //}
                }
            }

            //scheduleInDb.IsGenerated = true;
            scheduleInDb.RosterGeneratedDate = DateTime.UtcNow;
            context.Attendances.AddRange(list);
            context.WorkItems.AddRange(listWorkTimes);

            var _data = scheduleInDb.EmployeeIdsData.ToList();

            // set employee End index of pattern
            foreach (var emp in empls)
            {
                if (!_activeEmpls.Any(c => c.EmployeeId == emp.id))
                    continue;

                emp._PatternEndIndex = employeeLastIndex[emp.id];
            }

            scheduleInDb.Status = ScheduleStatus.Generated;
            scheduleInDb.SelectedMenu = RosterCreateLineItemType.Complete;
            scheduleInDb.EmployeeIdsData = empls.ToList();
            context.Schedules.Update(scheduleInDb);

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Process), new { id = scheduleInDb.Id, t = RosterCreateLineItemType.Complete });
        }

        private int GetPatternStart(int ii, IList<DayVm> daysData)
        {
            var nextIndex = ii  + (7 + 1);
            if (nextIndex >= daysData.Count())
                nextIndex = nextIndex - daysData.Count();

            return nextIndex;
        }

        /// <summary>
        /// Uses rotating index > if goes out of bounds
        /// </summary>
        /// <param name="ii"></param>
        /// <param name="totalPattern"></param>
        /// <returns></returns>
        private int GetNextPatternIndex(int ii, int totalPattern)
        {
            var nextIndex = ii + (1);
            if (nextIndex >= totalPattern)
                nextIndex = nextIndex - totalPattern;

            return nextIndex;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            TempData.Keep("RosterVm");
            base.OnActionExecuted(context);
        }
    }
}
