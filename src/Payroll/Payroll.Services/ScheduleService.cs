using Microsoft.AspNetCore.Identity;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payroll.ViewModels;
using Payroll.Database;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Hangfire;
using Microsoft.AspNetCore.Http;

namespace Payroll.Services
{
    public class ScheduleService
    {
        private readonly PayrollDbContext context;
        private readonly UserResolverService userResolverService;
        private readonly AccountDbContext accountDbContext;
        private readonly Hangfire.IBackgroundJobClient backgroundJobClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly CompanyService companyService;
        private string computedstring;

        private DateTime baseDate = DateTime.UtcNow.Date;
        //private DateTime? __EndDate = null;
        //private DateTime? __Start = null;

        public DateTime yesterday => baseDate.AddDays(-1);
        public DayOfWeek _dayOfWeek { get; private set; }
        public DateTime thisWeekStart => StartOfWeek(baseDate, _dayOfWeek); // baseDate.AddDays(-(int)baseDate.DayOfWeek.); // DayOfWeek(baseDate, System.DayOfWeek.Thursday);
        public DateTime thisWeekEnd => thisWeekStart.AddDays(7).AddSeconds(-1);
        public DateTime lastWeekStart => thisWeekStart.AddDays(-7);
        public DateTime lastWeekEnd => thisWeekStart.AddSeconds(-1);
        public DateTime thisMonthStart => StartOfMonth(baseDate, _dayOfWeek); // baseDate.AddDays(1 - baseDate.Day);

        //public DateTime? GetEndDate() => __EndDate;
        public DateTime StartOfMonth(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (30 + (dt.DayOfWeek - startOfWeek)) % 30;
            return dt.AddDays(-1 * diff).Date;
        }

        public DateTime thisMonthEnd => thisMonthStart.AddMonths(1).AddSeconds(-1);


        public async Task AddNewBiometricRecord(int id, BiometricRecord model)
        {
            var attendance = await GetAttendance(id);
            if (attendance == null) throw new ApplicationException("Attendance was not found!");

            attendance.BiometricRecords.Add(model);
            await context.SaveChangesAsync();


            await UpdateAttendanceWorkHours(attendance.Id);
        }

        public DateTime lastMonthStart => thisMonthStart.AddMonths(-1);
        public DateTime lastMonthEnd => thisMonthStart.AddSeconds(-1);



        public ScheduleService(PayrollDbContext context, UserResolverService userResolverService, AccountDbContext accountDbContext, Hangfire.IBackgroundJobClient backgroundJobClient, IHttpContextAccessor httpContextAccessor, CompanyService companyService)
        {
            this.context = context;
            this.userResolverService = userResolverService;
            this.accountDbContext = accountDbContext;
            this.backgroundJobClient = backgroundJobClient;
            this.httpContextAccessor = httpContextAccessor;
            this.companyService = companyService;
        }
        public async Task SetDayOfWeeekOnCompany(int cmpId)
        {
            _dayOfWeek = await accountDbContext.CompanyAccounts.Where(x => x.Id == userResolverService.GetCompanyId()).Select(x => x.WeekStartDay).FirstOrDefaultAsync();
        }

        public async Task SetDayOfWeeekOnCompany(CompanyAccount cmp)
        {
            _dayOfWeek = cmp.WeekStartDay;
        }

        public DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public async Task<Dictionary<DateTime, string>> GetThisWeekTimeSheet(int empId)
        {
            var timeSheetStats = await context.Attendances.Where(a => a.Date >= thisWeekStart && a.Date <= thisWeekEnd && a.IsPublished && a.EmployeeId == empId).AsQueryable().GroupBy(a => a.Date.Date)
                .ToDictionaryAsync(a => a.Key, a => a.Sum(x => x.TotalHoursWorkedPerSchedule).GetHourMinString());

            return Enumerable.Range(0, 7).Select((i, a) => new
            {
                date = thisWeekStart.AddDays(i),
                hours = timeSheetStats.ContainsKey(thisWeekStart.AddDays(i)) ? timeSheetStats[thisWeekStart.AddDays(i)] : "0hrs"
            }).ToDictionary(a=> a.date, a=> a.hours);
        }


        public async Task<Attendance> GetAttendance(int attId)
        {
            return await context.Attendances
                .Include(x => x.Employee)
                .Include(x => x.Requests)
                .Include(x => x.BiometricRecords)
                .FirstOrDefaultAsync(x => x.Id == attId);
        }

        public async Task<List<Attendance>> GetAttendancesAsync(int[] empIds, DateTime start, DateTime end, int page = 1, int limit = 10)
        {
            if (empIds == null) return new List<Attendance>();

            var query = context.Attendances.Where(x => x.Date >= start && x.Date <= end && empIds.Contains(x.EmployeeId) && x.IsActive && x.CompanyId == userResolverService.GetCompanyId())
                .Include(a=> a.BiometricRecords)
                .Skip((page - 1) * limit)
                .Take(limit);

            if(empIds.Length > 1)
            {
                query = query
                .Include(x => x.Employee)
                .ThenInclude(x => x.Department);
                //.OrderBy(x => x.Employee.Department.DisplayOrder).ThenBy(x => x.Employee.EmpID)
                //.ThenBy(x => x.Date);
            }

            query = query
            .Include(x => x.BiometricRecords)
            .OrderBy(a => a.Date);

            return await query
                .ToListAsync();
        }

        public async Task<List<WorkItem>> GetTasksAsync(int[] empIds, DateTime start, DateTime end, int page = 1, int limit = 10)
        {
            if (empIds == null) return new List<WorkItem>();

            var query = context.WorkItems.Where(x => x.Date >= start && x.Date <= end && empIds.Contains(x.EmployeeId) && x.IsActive)
                .Skip((page - 1) * limit)
                .Take(limit);

            if (empIds.Length > 1)
            {
                query = query
                .Include(x => x.Employee)
                .ThenInclude(x => x.Department);
                //.OrderBy(x => x.Employee.Department.DisplayOrder).ThenBy(x => x.Employee.EmpID)
                //.ThenBy(x => x.Date);
            }

            query = query
            .Include(x => x.WorkItemSubmissions)
            .OrderBy(a => a.Date);

            return await query
                .ToListAsync();
        }

        public async Task<List<WeeklyEmployeeShiftVm>> GetCurrentSchedule(int page = 1, int limit = 10, bool showUnScheduled = false, bool showTasks = false, EmployeeSelectorVm selectorVm = null, DateTime? start = null, DateTime? end = null)
        {
            //var weekStartDate = thisWeekStart
            var endDate = end  ?? thisWeekEnd;
            var startDate = start ?? thisWeekStart;
            var allEmpsiInSchedule = await context.Attendances.Where(x => x.Date >= startDate && x.Date <= endDate && x.IsActive &&
            (selectorVm == null || selectorVm.EmployeeIds.Contains(x.EmployeeId)) && x.CompanyId == userResolverService.GetCompanyId())
                .Select(x => x.Employee)
                .Distinct()
                .OrderBy(x => x.Department.DisplayOrder).ThenBy(x => x.EmpID)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Include(x => x.Department)
                .ToListAsync();


            var allEmpsiInScheduleIs = allEmpsiInSchedule.Select(x => x.Id).ToArray();


            if (showUnScheduled)
                allEmpsiInSchedule = await context.Employees.Where(x => x.Department.CompanyId == userResolverService.GetCompanyId()
                    && !allEmpsiInScheduleIs.Contains(x.Id))
                .OrderBy(x => x.Department.DisplayOrder).ThenBy(x => x.EmpID)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Include(x => x.Department)
                .ToListAsync();

            var attendances = await GetAttendancesAsync(allEmpsiInScheduleIs, startDate, endDate, 1, int.MaxValue);
                //context.Attendances.Where(x => x.Date >= startDate && x.Date <= endDate && allEmpsiInScheduleIs.Contains(x.EmployeeId) && x.IsActive && x.CompanyId == userResolverService.GetCompanyId())
                //.Include(x => x.Employee)
                //.ThenInclude(x => x.Department)
                //.OrderBy(x => x.Employee.Department.DisplayOrder).ThenBy(x => x.Employee.EmpID)
                //.ThenBy(x => x.Date)
                //.ToListAsync();


            var list = new List<WeeklyEmployeeShiftVm>();
            foreach (var emp in allEmpsiInSchedule)
            {
                var _ = new WeeklyEmployeeShiftVm { Employee = emp };
                for (DateTime _start = startDate; _start < endDate; _start = _start.AddDays(1))
                {
                    if (attendances.Any(x => x.Date.Date == _start.Date.Date && x.EmployeeId == emp.Id))
                        _.Attendances.AddRange(attendances.Where(x => x.Date.Date == _start.Date.Date && x.EmployeeId == emp.Id).ToList());
                }

                list.Add(_);
            }


            //if (showTasks)
            //{
            //    var workItems = await context.WorkItems.Where(x => x.Date >= thisWeekStart && x.Date <= thisWeekEnd
            //    && allEmpsiInScheduleIs.Contains(x.EmployeeId))
            //        .Include(x => x.Employee)
            //        .ThenInclude(x => x.Department)
            //        .Include(x => x.WorkItemSubmissions)
            //        .OrderBy(x => x.Employee.Department.DisplayOrder)
            //        .ThenBy(x => x.Date)
            //        .ToListAsync();

            //    foreach (var workItem in list)
            //    {
            //        var _ = new WorkItemScheduleVvm();
            //        for (DateTime start = thisWeekStart; start < thisWeekEnd; start = start.AddDays(1))
            //        {
            //            if (workItems.Any(x => x.Date.Date == start.Date.Date && x.WorkId == workItem.Id))
            //                workItem.WorkItems.AddRange(attendances.Where(x => x.Date.Date == start.Date.Date && x.WorkId == workItem.Id && (empId == 0 || empId == x.EmployeeId)).ToArray());
            //        }

            //        list.Add(_);
            //    }
            //}

            return list;
        }

        public async Task<List<WeeklyEmployeeShiftVm>> GetCurrentSecdule(int page = 1, int limit = 10, bool showUnScheduled = false, bool showTasks = false, int empId = 0, DateTime? start = null, DateTime? end = null)
            {
            return await GetCurrentSchedule(page, limit, showUnScheduled, showTasks, new EmployeeSelectorVm { EmployeeIds = new[] { empId } }, start, end);
        }

        public async Task<List<Work>> GetCurrentSecduledTasks(EmployeeSelectorVm selectorVm, int page = 1, int limit = 10)
        {
            List<Work> allWorksInCmp = await GetCompanyWorksAsync();

            var attendances = await GetTasksAsync(selectorVm.EmployeeIds, thisWeekStart, thisWeekEnd, page, limit);

            //    await context.WorkItems.Where(x => x.Date >= thisWeekStart && x.Date <= thisWeekEnd
            //&& (selectorVm == null || selectorVm.EmployeeIds.Contains(x.EmployeeId)))
            //    .Include(x => x.Schedule)
            //    .Include(x => x.Employee)
            //    .ThenInclude(x => x.Department)
            //    .Include(x => x.WorkItemSubmissions)
            //    .OrderBy(x => x.Employee.Department.DisplayOrder)
            //    .ThenBy(x => x.Date)
            //    .ToListAsync();

            allWorksInCmp.ForEach(x => x.WorkItems = new List<WorkItem>());
            var list = new List<WorkItemScheduleVvm>();
            foreach (var workItem in allWorksInCmp)
            {
                var _ = new WorkItemScheduleVvm();
                for (DateTime start = thisWeekStart; start < thisWeekEnd; start = start.AddDays(1))
                {
                    if (attendances.Any(x => x.Date.Date == start.Date.Date && x.WorkId == workItem.Id && (selectorVm == null || selectorVm.EmployeeIds.Contains(x.EmployeeId))))
                        workItem.WorkItems.AddRange(attendances.Where(x => x.Date.Date == start.Date.Date && x.WorkId == workItem.Id && (selectorVm == null || selectorVm.EmployeeIds.Contains(x.EmployeeId))).ToArray());
                }

                list.Add(_);
            }


            return allWorksInCmp;
        }

        public async Task<List<Work>> GetCurrentSecduledTasks(int page = 1, int limit = 10, int empId = 0)
            => await GetCurrentSecduledTasks(new EmployeeSelectorVm { EmployeeIds = new[] { empId } }, page, limit);

        public async Task<List<WeeklyEmployeeShiftVm>> GetWeeklyEmployeeShiftVm(Schedule scheduleIndb)
        {
            var attendances = await context.Attendances.Where(x => x.ScheduleId == scheduleIndb.Id && x.IsActive)
                                        .ToListAsync();
            var workItems = await context.WorkItems.Where(x => x.ScheduleId == scheduleIndb.Id && x.IsActive)
                .Include(x => x.Work)
                .ToListAsync();
            var list = new List<WeeklyEmployeeShiftVm>();
            var emps = await context.Employees.Where(a => scheduleIndb.EmployeeIds.Contains(a.Id))
                .Include(a=> a.Department)
                .ToListAsync();
            foreach (var emp in emps)
            {
                var _ = new WeeklyEmployeeShiftVm { Employee = emp };
                for (DateTime start = scheduleIndb.Start; start <= scheduleIndb.End; start = start.AddDays(1))
                {
                    if (attendances.Any(x => x.Date.Date == start.Date.Date && x.EmployeeId == emp.Id))
                        _.Attendances.AddRange(attendances.Where(x => x.Date.Date == start.Date.Date && x.EmployeeId == emp.Id).ToList());

                    if (workItems.Any(x => x.Date.Date == start.Date.Date && x.EmployeeId == emp.Id))
                        _.WorkItems.AddRange(workItems.Where(x => x.Date.Date == start.Date.Date && x.EmployeeId == emp.Id).ToList());
                }

                list.Add(_);
            }

            return list;
        }

        public async Task<List<DayVm>> GetWorkingCalendar(int empId, string show = "all")
        {
            List<Work> allWorksInCmp = await GetCompanyWorksAsync();

            var workItems = await context.WorkItems.Where(x => (empId == 0 || empId == x.EmployeeId) && x.Date.Date >= thisMonthStart && x.Date.Date <= this.thisMonthEnd)
            .AsQueryable()
                .GroupBy(x => x.Date.Date)
                .ToDictionaryAsync(x => x.Key.Date, x => x.Count());

            var attendances = await context.Attendances.Where(x => (empId == 0 || empId == x.EmployeeId) && x.Date.Date >= thisMonthStart && x.Date.Date <= this.thisMonthEnd && x.CompanyId == userResolverService.GetCompanyId())
                .AsQueryable()
                .GroupBy(x => x.Date.Date)
                .ToDictionaryAsync(x => x.Key.Date, x => x.Count());

            var days = new List<DayVm>();
            for (var start = thisMonthStart; start < this.thisMonthEnd; start = start.AddDays(1))
            {
                days.Add(new DayVm
                {
                    DayOfWeek = start.DayOfWeek,
                    Date = start,
                    Day = start.Date.Day,
                    _TotalWorks = workItems.GetValueOrDefault(start.Date),
                    _TotalAttendance = attendances.GetValueOrDefault(start.Date),
                });
            }
            //    .Include(x => x.Schedule)
            //    .Include(x => x.Employee)
            //    .ThenInclude(x => x.Department)
            //    .Include(x => x.WorkItemSubmissions)
            //    .OrderBy(x => x.Employee.Department.DisplayOrder)
            //    .ThenBy(x => x.Date)
            //    .ToListAsync();

            //allWorksInCmp.ForEach(x => x.WorkItems = new List<WorkItem>());
            //var list = new List<WorkItemScheduleVvm>();
            //foreach (var workItem in allWorksInCmp)
            //{
            //    var _ = new WorkItemScheduleVvm();
            //    for (DateTime start = thisWeekStart; start < thisWeekEnd; start = start.AddDays(1))
            //    {
            //        if (attendances.Any(x => x.Date.Date == start.Date.Date && x.WorkId == workItem.Id && (empId == 0 || empId == x.EmployeeId)))
            //            workItem.WorkItems.AddRange(attendances.Where(x => x.Date.Date == start.Date.Date && x.WorkId == workItem.Id && (empId == 0 || empId == x.EmployeeId)).ToArray());
            //    }

            //    list.Add(_);
            //}


            return days;
        }


        public async Task<Dictionary<string, int>> GetSecduledTasksForThisWeekByStatus(int? empId = null)
        {
            List<Work> allWorksInCmp = await GetCompanyWorksAsync();

            var statusCounts =  await context.WorkItems.Where(x => x.Date.Date >= thisWeekStart && x.Date.Date <= thisWeekEnd
            && (empId == null || empId == x.EmployeeId))
            .AsQueryable()
            .GroupBy(x => x.Status)
            .Select(x=> new  {x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

            var dits = new Dictionary<string, int>();
            dits.Add("Pending", statusCounts.Count(a => a.Key == WorkItemStatus.Draft));
            dits.Add("Completed", statusCounts.Count(a => a.Key == WorkItemStatus.Approved && a.Key == WorkItemStatus.Submitted && a.Key == WorkItemStatus.Completed));
            dits.Add("UpComing", await context.WorkItems.CountAsync(x => x.Date.Date >= thisWeekStart.AddDays(7) && x.Date.Date <= thisWeekEnd.AddDays(7)
            && (empId == null || empId == x.EmployeeId)));
            return dits;
        }

        public async Task<List<WorkItem>> GetSecduledTasksForToday(int page = 1, int limit = 10, int empId = 0)
        {
            List<Work> allWorksInCmp = await GetCompanyWorksAsync();

            return await context.WorkItems.Where(x => x.Date.Date >= DateTime.Now.Date && x.Date.Date <= DateTime.Now.Date
            && (empId == 0 || empId == x.EmployeeId))
                .Include(x => x.Employee)
                .ThenInclude(x => x.Department)
                .Include(x => x.Work)
                .Include(x => x.WorkItemSubmissions)
                .OrderBy(x => x.Employee.Department.DisplayOrder)
                .ThenBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<List<WorkItem>> GetSecduledTasksForThisWeek(int page = 1, int limit = 10, int? empId = null, string complete = "on")
        {
            List<Work> allWorksInCmp = await GetCompanyWorksAsync();

            return await context.WorkItems.Where(x => x.Date.Date >= thisWeekStart && x.Date.Date <= thisWeekEnd
            && (empId == null || empId == x.EmployeeId)
            && (complete == "off" ? x.Status != WorkItemStatus.Completed : true))
                .Include(x => x.Employee)
                .ThenInclude(x => x.Department)
                .Include(x => x.Work)
                .Include(x => x.WorkItemSubmissions)
                .OrderBy(x => x.Employee.Department.DisplayOrder)
                .ThenBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<List<WorkItem>> GetSecduledTasks(int page = 1, int limit = 10, int? empId = null, DateTime? start = null, DateTime? end = null)
        {
            List<Work> allWorksInCmp = await GetCompanyWorksAsync();

            return await context.WorkItems.Where(x => (start == null || end == null) || x.Date.Date >= start.Value.Date && x.Date.Date <= end.Value.Date
            && (empId == null || empId == x.EmployeeId))
                .Include(x => x.Employee)
                .ThenInclude(x => x.Department)
                .Include(x => x.Work)
                .Include(x => x.WorkItemSubmissions)
                .OrderBy(x => x.Employee.Department.DisplayOrder)
                .ThenBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<List<Work>> GetCompanyWorksAsync()
        {
            //var weekStartDate = thisWeekStart
            return await context.Works
                .Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public void EnsureRequestCanTakeAction(Request request)
        {
            if (request.Status != WorkItemStatus.Submitted)
                throw new ApplicationException("Sorry! This Request just cant take action");
        }

        //internal void SetStartDate(DateTime? date)
        //{
        //    __Start = date.Value;
        //}

        public DateTime GetBaseDate()
        {
            return baseDate;
        }

        public void SetBaseDate(DateTime? date)
        {
            baseDate = date.Value;
        }


        //internal void SetEndDate(DateTime? date)
        //{
        //    __EndDate = date.Value;
        //}


        // https://stackoverflow.com/questions/11154673/get-the-correct-week-number-of-a-given-date
        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        // GetIso8601WeekOfYear
        public int GetWeeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public string GetDurationText(DateTime? start, DateTime? end, bool includeDays = true)
        {
            try
            {
                if (start.HasValue && end.HasValue)
                {
                    var days = (end?.Date - start?.Date)?.Days;
                    if (days <= 0)
                        return start?.ToString("ddd, MMM dd, yyyy");
                    else
                        return start?.ToString("MMM dd") + " - " + end?.ToString(start?.Month != end?.Month ? "MMM dd, yyyy" : "dd, yyyy") + (includeDays ? " (" + days + " days)" : "");
                }
                else
                {
                    return "NA";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        
        public async Task<IdentityResult> ValidateDaysForOverlaps(List<DayVm> days, ScheduleCreateVm model)
        {
            // without overlapping can create
            // check if employee has more scheudles
            if (await context.Attendances.AnyAsync(x => x.Date >= model.ShiftDurationStart && x.Date <= model.ShiftDurationEnd && model.EmployeeIds.Contains(x.EmployeeId) && x.CompanyId == userResolverService.GetCompanyId()))

                // yes! then check for overlapping (can't create overlapping betwee work timees)
                if (await context.Attendances.AnyAsync(x =>
                    model.ShiftDurationStart < x.WorkEndTime && x.WorkStartTime < model.ShiftDurationEnd && x.CompanyId == userResolverService.GetCompanyId()))
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "This record(s) will create overlapping attendance records, Kindly check work time"
                    });
            //foreach (var item in model.Days.Where(x => x.IsOff == false))
            //    {
            //    }

            return IdentityResult.Success;
        }

        public async Task<Schedule> CreateNewScheduleAsync(ScheduleCreateVm model)
        {
            List<Work> works = null;
            List<CompanyWorkTime> workTimes = null;

            if (model.ScheduleFor == ScheduleFor.Attendance)
                workTimes = await context.CompanyWorkTimes
                    .Where(x => x.CompanyId == userResolverService.GetCompanyId())
                    .Include(x => x.Company)
               .ToListAsync();
            else
                works = await context.Works
                    .Where(x => x.CompanyId == userResolverService.GetCompanyId())
                    .ToListAsync();


            if (model.ScheduleFor == ScheduleFor.Attendance && workTimes.Count <= 0) throw new Exception("Work times were NOT found to create attendance schedule");
            if (model.ScheduleFor == ScheduleFor.Work && works.Count <= 0) throw new Exception("Works were NOT found to create attendance schedule");

            var sch = new Schedule
            {
                Name = model.Name,
                DepartmentId = model.DepartmentId <= 0 ? (int?)null : model.DepartmentId,
                IsForAllEmployees = model.IsForAllEmployees,
                IsForDepartment = model.IsForDepartment,
                WorkId = model.WorkId,
                WorkName = works?.FirstOrDefault(x => x.Id == model.WorkId)?.Name ?? "",
                ScheduleFor = model.ScheduleFor,
                //IgnoreDays = model.IgnoreDays,
                //ShiftName = workTimes = works.FirstOrDefault(x => x.Id == model.)?.Name,,
                Start = model.ShiftDurationStart,
                End = model.ShiftDurationEnd ?? (DateTime?)null,
                //WorkId = model.WorkId,
                //ShiftId = model.ShiftId,
                IsEffectiveImmediately = model.IsEffectiveImmediately,
                EffectiveDate = model.EffectiveDate,
                //MinHours = model.MinHours,
                Repeat = model.RecurringFrequency,
                EmployeeIds = model.EmployeeIds,
            };

            var _days = model.Days;
            if (sch.ScheduleFor == ScheduleFor.Work)
            {
                if (_days != null && _days.Any())
                {
                    foreach (var day in _days)
                    {
                        if (day.IsOff) continue;
                        day._Work = day.WorkIds.Select(id => new _Work
                        {
                            id = id,
                            Name = works.First(a => a.Id == id).Name,
                            Type = works.First(a => a.Id == id).Type,
                        }).ToList();
                    }
                }
                sch.DaysData = _days;
            }
            else if (sch.ScheduleFor == ScheduleFor.Attendance)
            {
                if (_days != null && _days.Any())
                {
                    foreach (var day in _days)
                    {
                        day.ShiftName = workTimes.First(a => a.Id == day.ShiftId).ShiftName;
                        day.Color = workTimes.First(a => a.Id == day.ShiftId).ColorCombination;
                    }
                }

                sch.DaysData = _days;
            }
            else
            {
                sch.DaysData = model.Days;
            }
            //foreach (var day in model.Days)
            //{
            //    if (model.ScheduleFor == ScheduleFor.Attendance)
            //    {
            //        day.ShiftName = workTimes.FirstOrDefault(x=> x.Id == day.ShiftId)?.ShiftName;
            //        day.WorkStart = workTimes.FirstOrDefault(x => x.Id == day.ShiftId)?.StartTime;
            //        day.WorkEnd = workTimes.FirstOrDefault(x => x.Id == day.ShiftId)?.EndTime;
            //        day.Color = workTimes.FirstOrDefault(x => x.Id == day.ShiftId)?.ColorCombination;
            //    }
            //    else
            //    {
            //        day.WorkName = works.FirstOrDefault(x => x.Id == day.WorkId)?.Name;
            //        day.WorkStart = works.FirstOrDefault(x => x.Id == day.WorkId)?.StartTime;
            //        day.WorkEnd = works.FirstOrDefault(x => x.Id == day.WorkId)?.EndTime;
            //        day.Color = works.FirstOrDefault(x => x.Id == day.WorkId)?.ColorCombination;
            //    }
            //}

            //sch.IgnoreDaysData = model.IgnoreDays?.ToList();
            if (model.EmployeeIds != null)
                sch.EmployeeIdsData = await context.Departments
                    .Where(x => x.CompanyId == userResolverService.GetCompanyId())
                    .SelectMany(x => x.Employees)
                    .Where(z => model.EmployeeIds.Contains(z.Id))
                    .OrderBy(x => x.Department.DisplayOrder)
                    .ThenBy(x => x.EmpID)
                    .Select(x => new _Employee
                    {
                        Photo = x.Avatar,
                        id = x.Id,
                        //City = x.City,
                        Department = x.Department.Name,
                        Name = x.GetSystemName(userResolverService.GetClaimsPrincipal()),
                        Designation = x.JobTitle,
                        Gender = x.Gender,
                    })
                    .ToListAsync();

            sch.IsRepeatEndDateNever = !sch.End.HasValue;
            sch.IsRepeating = sch.Repeat != RecurringFrequency.Never;
            sch.CompanyId = userResolverService.GetCompanyId();
            return sch;
        }


        public async Task<List<Schedule>> GetSchedule(int? companyId = null, int page = 1, int limit = 10, string searchKey = null)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey?.ToLower();
                return await context.Schedules
                         //   .Where(x => x.Name.ToLower().Contains(searchKey) ||
                         //(x.Hotline != null && x.Hotline.ToLower().Contains(searchKey)) ||
                         //(x.ManagingDirector != null && x.ManagingDirector.ToString() == searchKey) ||
                         //(x.Address != null && x.Address.Contains(searchKey)))

                         //.OrderBy(x => x.Name)
                         .Skip((page - 1) * limit)
                         .Take(limit)
                         //.Select(x=> new Schedule { Company = x.Company, EffectiveDate = x.EffectiveDate, Name = x.Name, ScheduleFor = x.ScheduleFor })
                         .ToListAsync();
            }


            return await context.Schedules
                 //.OrderBy(x => x.Name)
                 .Skip((page - 1) * limit)
                 .Take(limit)
                 //.Select(x => new Schedule { Company = x.Company, EffectiveDate = x.EffectiveDate, Name = x.Name, ScheduleFor = x.ScheduleFor })
                 .ToListAsync();
        }

        public async Task UpdateAttendanceAsync(Attendance model)
        {
            var attendance = await context.Attendances.FirstOrDefaultAsync(x => x.Id == model.Id);
            attendance.CurrentStatus = model.CurrentStatus;

            if (attendance.StatusUpdates == null)
                attendance.StatusUpdates = new List<AttendanceStatusUpdate>();


            var date = attendance.Date.Date;

            // update shift (also update work start/end time)
            if (model.CompanyWorkTimeId != attendance.CompanyWorkTimeId && model.CompanyWorkTimeId != 0)
            {
                var cmWorktimes = await companyService.GetWorkTimes();
                attendance.CompanyWorkTimeId = model.CompanyWorkTimeId;

                var newShift = cmWorktimes.FirstOrDefault(a => a.Id == model.CompanyWorkTimeId);
                attendance.ShiftId = model.CompanyWorkTimeId ?? 0;
                attendance.ShiftColor = newShift.ColorCombination;
                attendance.ShiftName = newShift.ShiftName;

                attendance.WorkStartTime = attendance.WorkStartTime.Date.Add(newShift.StartTime);

                var workEndDateCalc = newShift.EndTime.Hours < newShift.StartTime.Hours ? date.AddDays(1) : date;
                attendance.WorkEndTime = new DateTime(workEndDateCalc.Year, workEndDateCalc.Month, workEndDateCalc.Day, newShift.EndTime.Hours, newShift.EndTime.Minutes, newShift.EndTime.Seconds);
                
            }



            if (model.CheckInTime.HasValue)
                attendance.CheckInTime = date.Add(model.CheckInTime.Value.TimeOfDay);
            else
                attendance.CheckInTime = null;

            if (model.CheckOutTime.HasValue && model.CheckInTime.HasValue)
            {
                if (model.CheckOutTime.Value.TimeOfDay < model.CheckInTime.Value.TimeOfDay)
                    date = date.AddDays(1).Date;
                attendance.CheckOutTime = date.Add(model.CheckOutTime.Value.TimeOfDay);
            }
            else
                attendance.CheckOutTime = null;
            
            if (attendance.CheckInTime.HasValue == false || attendance.CheckOutTime.HasValue == false)
            {
                attendance.StatusUpdates.Add(GetNewStatusUpdate(AttendanceStatus.Absent));
                attendance.CurrentStatus = LastUpdatedAttendanceStatus;
                attendance.HasClockRecords = false;
                context.Attendances.Update(attendance);
                await context.SaveChangesAsync();
                return;
            }


            if (attendance.CheckOutTime.HasValue)
            {
                attendance.TotalEarlyMins = attendance.CheckInTime < attendance.WorkStartTime ? (attendance.WorkStartTime - attendance.CheckInTime.Value).TotalMinutes : 0f;
                attendance.TotalLateMins = attendance.CheckInTime > attendance.WorkStartTime ? (attendance.CheckInTime.Value - attendance.WorkStartTime).TotalMinutes : 0f;
                attendance.TotalAfterWorkMins = attendance.CheckOutTime > attendance.WorkEndTime ? (attendance.CheckOutTime.Value - attendance.WorkEndTime).TotalMinutes : 0f;
                attendance.TotalWorkedHours = (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
                attendance.CurrentStatus = AttendanceStatus.Recieved;
            }
            

            if (attendance.StatusUpdates == null)
                attendance.StatusUpdates = new List<AttendanceStatusUpdate>();

            

            if (attendance.TotalEarlyMins > 0 && attendance.TotalEarlyMins > 5)
                attendance.StatusUpdates.Add(GetNewStatusUpdate(AttendanceStatus.Early));

            else if (attendance.TotalLateMins <= 0 && (attendance.TotalLateMins <= 1 || attendance.TotalEarlyMins <= 5))
            {
                attendance.StatusUpdates.Add(GetNewStatusUpdate(AttendanceStatus.OnTime));

            }
            else
                attendance.StatusUpdates.Add(GetNewStatusUpdate(AttendanceStatus.Late));



            //attendance.WorkedOverTime = false;
            attendance.TotalHoursWorkedOutOfSchedule = 0;

            if (attendance.TotalAfterWorkMins > 0)
            {
                attendance.TotalHoursWorkedOutOfSchedule = (attendance.TotalAfterWorkMins / 60) + (attendance.TotalEarlyMins / 60);
            }
            attendance.WorkedOverTime = attendance.TotalHoursWorkedOutOfSchedule > 0;

            var totalWorkHours = (attendance.WorkEndTime - attendance.WorkStartTime).TotalHours;


            // calculate total worked per schedule
            var _checkIn = (attendance.CheckInTime.Value >= attendance.WorkStartTime ? attendance.CheckInTime.Value : attendance.WorkStartTime);
            var _checkOut = (attendance.CheckOutTime.Value >= attendance.WorkEndTime ? attendance.WorkEndTime : attendance.CheckOutTime.Value);

            attendance.TotalHoursWorkedPerSchedule = (_checkOut - _checkIn).TotalHours;


            attendance.TotalWorkedHoursCalculated = attendance.TotalHoursWorkedPerSchedule;
            attendance.HasClockRecords = attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue;
            attendance.CurrentStatus = LastUpdatedAttendanceStatus;
            //attendance.DataSource = model.DataSource;
            //attendance.InitialCatalog = model.InitialCatalog;
            //attendance.UserId = model.UserId;
            //attendance.IntegratedSecurity = model.IntegratedSecurity;
            context.Attendances.Update(attendance);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAttendanceWorkHours(int? attendanceId)
        {
            //Attendance model = new Attendance();

            var attendance = await context.Attendances
            .Include(a => a.BiometricRecords)
            .FirstOrDefaultAsync(x => x.Id == attendanceId);

            attendance.HasError = false;
            attendance.ErroMsg = "";

            if (attendance.BiometricRecords.Count() % 2 != 0)
            {
                attendance.HasError = true;
                attendance.ErroMsg = "Insufficient records";

            }
            else
            {
                var _ = attendance.BiometricRecords.AsQueryable()
                .GroupBy(x => x.BiometricRecordState)
                    .Select(a => new { a.Key, Cnt = a.Count() })
                    .ToList();
                if (_.Select(a => a.Key).Count() % 2 != 0)
                {
                    attendance.HasError = true;
                    attendance.ErroMsg = "Duplicate states found";
                }
                //else if(_.Sum(a => a.Cnt) % 2 != 0)
                //{
                //    attendance.HasError = true;
                //    attendance.ErroMsg = "Incorrect order of records";
                //}
            }

            if (attendance.HasError)
            {
                await context.SaveChangesAsync();
                return;
            }

            //attendance.CurrentStatus = model.CurrentStatus;


            var orderedRecds = attendance.BiometricRecords.OrderBy(a => a.DateTime)
                .ToArray();
            var ompanyAccountData = await context.Companies.Where(a => a.Id == userResolverService.GetCompanyId())
                .Select(a => new { a.FlexibleBreakHourCount, a.IsBreakHourStrict, a.EarlyOntimeMinutes })
                .FirstOrDefaultAsync();

            TimeSpan timeWorking = new TimeSpan();
            TimeSpan timeWorkingPerSchedule = new TimeSpan();
            TimeSpan timeBreak = new TimeSpan();
            BiometricRecordState? prevState = null;

            attendance.TotalLateMins = 0;
            attendance.TotalEarlyMins = 0;
            attendance.TotalAfterWorkMins = 0;

            for (int i = 0; i < orderedRecds.Length; i++)
            {

                switch (orderedRecds[i].BiometricRecordState)
                {
                    case BiometricRecordState.CheckIn:
                        // EARLY MINS
                        attendance.TotalEarlyMins = orderedRecds[i].DateTime < attendance.WorkStartTime ? (attendance.WorkStartTime - orderedRecds[i].DateTime).TotalMinutes : 0f;

                        // LATE 
                        if (orderedRecds[i].DateTime > attendance.WorkStartTime)
                            attendance.TotalLateMins += (orderedRecds[i].DateTime - attendance.WorkStartTime).TotalMinutes;


                        //if (!prevState.HasValue)
                        //timeWorking = 0;
                        break;
                    case BiometricRecordState.CheckOut:
                        if (prevState.HasValue && prevState == BiometricRecordState.CheckIn || prevState == BiometricRecordState.BreakIn)
                            timeWorking += (orderedRecds[i].DateTime - orderedRecds[i - 1].DateTime);

                        if (prevState.HasValue && prevState == BiometricRecordState.CheckIn || prevState == BiometricRecordState.BreakIn)
                        {
                            // calculate total worked per schedule
                            var _checkIn = (orderedRecds[i - 1].DateTime >= attendance.WorkStartTime ? orderedRecds[i - 1].DateTime : attendance.WorkStartTime);
                            var _checkOut = (orderedRecds[i].DateTime >= attendance.WorkEndTime ? attendance.WorkEndTime : orderedRecds[i].DateTime);

                            timeWorkingPerSchedule += (_checkOut - _checkIn);
                        }

                        // AFTER WORKS (can be overtime)
                        attendance.TotalAfterWorkMins = orderedRecds[i].DateTime > attendance.WorkEndTime ? (orderedRecds[i].DateTime - attendance.WorkEndTime).TotalMinutes : 0f;

                        break;
                    case BiometricRecordState.BreakIn:
                        if (prevState.HasValue && prevState == BiometricRecordState.BreakOut)
                            timeBreak += (orderedRecds[i].DateTime - orderedRecds[i - 1].DateTime);

                        // if break hour is more than company set hour;
                        if ((orderedRecds[i].DateTime - orderedRecds[i - 1].DateTime) > TimeSpan.FromHours(1) && ompanyAccountData.FlexibleBreakHourCount > 0 & ompanyAccountData.IsBreakHourStrict)
                            attendance.TotalLateMins += ((orderedRecds[i].DateTime - orderedRecds[i - 1].DateTime) - TimeSpan.FromHours(ompanyAccountData.FlexibleBreakHourCount)).TotalMinutes;
                        break;
                    case BiometricRecordState.BreakOut:
                        // going break out (prev check-in)
                        if (prevState.HasValue && prevState == BiometricRecordState.CheckIn)
                            timeWorking += (orderedRecds[i].DateTime - orderedRecds[i - 1].DateTime);
                        if (prevState.HasValue && prevState == BiometricRecordState.CheckIn)
                        {
                            // calculate total worked per schedule
                            var _checkIn = (orderedRecds[i - 1].DateTime >= attendance.WorkStartTime ? orderedRecds[i - 1].DateTime : attendance.WorkStartTime);
                            var _checkOut = (orderedRecds[i].DateTime >= attendance.WorkEndTime ? attendance.WorkEndTime : orderedRecds[i].DateTime);

                            timeWorkingPerSchedule += (_checkOut - _checkIn);
                        }
                        break;
                    default:
                        break;
                }

                prevState = orderedRecds[i].BiometricRecordState;
            }

            if (attendance.TotalEarlyMins > 0 && attendance.TotalEarlyMins > ompanyAccountData.EarlyOntimeMinutes && attendance.TotalLateMins <=0)
                attendance.StatusUpdates.Add(GetNewStatusUpdate(AttendanceStatus.Early));

            else if (attendance.TotalLateMins <= 0 && (attendance.TotalLateMins <= 1 || attendance.TotalEarlyMins <= ompanyAccountData.EarlyOntimeMinutes))
            {
                attendance.StatusUpdates.Add(GetNewStatusUpdate(AttendanceStatus.OnTime));

            }
            else
                attendance.StatusUpdates.Add(GetNewStatusUpdate(AttendanceStatus.Late));


            //attendance.WorkedOverTime = false;
            attendance.TotalHoursWorkedOutOfSchedule = 0;

            if (attendance.TotalAfterWorkMins > 0)
            {
                attendance.TotalHoursWorkedOutOfSchedule = (attendance.TotalAfterWorkMins / 60) + (attendance.TotalEarlyMins / 60);
            }
            attendance.WorkedOverTime = attendance.TotalHoursWorkedOutOfSchedule > 0;

            var totalWorkHours = (attendance.WorkEndTime - attendance.WorkStartTime).TotalHours;


            // calculate total worked per schedule
            attendance.TotalHoursWorkedPerSchedule = timeWorkingPerSchedule.TotalHours;
            attendance.TotalWorkedHours = timeWorking.TotalHours;
            attendance.TotalBreakHours = timeBreak.TotalHours;


            attendance.TotalWorkedHoursCalculated = attendance.TotalHoursWorkedPerSchedule;
            attendance.HasClockRecords = attendance.BiometricRecords?.Any()??false;
            attendance.CurrentStatus = LastUpdatedAttendanceStatus;

            context.Attendances.Update(attendance);
            await context.SaveChangesAsync();




            //double totalHours = 0;
            //for (DateTime start = attendance.WorkStartTime; start <= attendance.WorkEndTime; start = start.Add(TimeSpan.FromHours(1)))
            //{
            //    if (start.Hour % 2 == 1)
            //    {
            //        if (model.BiometricRecords.Any(r => start <= r.DateTime &&
            //      start.Add(TimeSpan.FromHours(2)) >= r.DateTime))
            //        {
            //            foreach (var item in model.BiometricRecords.Where(r => start <= r.DateTime &&
            //            start.Add(TimeSpan.FromHours(2)) >= r.DateTime))
            //            {
            //                var left = item.BiometricRecordState == BiometricRecordState.CheckIn | item.BiometricRecordState == BiometricRecordState.BreakIn ?
            //                ((start.Add(TimeSpan.FromHours(2)) - item.DateTime).TotalHours / 2) * 100 : ((item.DateTime - start).TotalHours / 2) * 100;

            //                var css = (item.BiometricRecordState == BiometricRecordState.CheckIn || item.BiometricRecordState == BiometricRecordState.BreakIn) ? "st" : "en";
            //                if (css == "st")
            //                    totalHours += (item.Date - start).TotalHours;
            //                else
            //                    totalHours += (start - item.Date).TotalHours;
            //                //<div class="item item-checkInOut sch item- sch-checkInOut @css" style="width:@left%" onclick="$('p.recrd').removeClass('font-weight-bold');$('.recrd_@item.Id').toggleClass('font-weight-bold')">
            //                //    <span title = "@item.DateTime.TimeOfDay - @item.BiometricRecordState" >
            //                //        &nbsp;
            //                //        @*<i class="fa fa-clock"></i>*@
            //                //    </span>
            //                //</div>
            //            }
            //        }
            //        // Middle spaces
            //        else if ((start > model.BiometricRecords?.FirstOrDefault(z => z.BiometricRecordState == BiometricRecordState.CheckIn)?.DateTime &&
            //                      start.Add(TimeSpan.FromHours(2)) < model.BiometricRecords?.FirstOrDefault(z => z.BiometricRecordState == BiometricRecordState.BreakOut)?.DateTime)
            //              ||
            //              (start > model.BiometricRecords?.FirstOrDefault(z => z.BiometricRecordState == BiometricRecordState.CheckIn)?.DateTime &&
            //                      start.Add(TimeSpan.FromHours(2)) < model.BiometricRecords?.FirstOrDefault(z => z.BiometricRecordState == BiometricRecordState.CheckOut)?.DateTime))
            //        {
            //            totalHours += 2;
            //        }
            //    }
            //}

        }

        public async Task UpdateWorkItemAsync(WorkItem model)
        {
            var workItem = await context.WorkItems
                .Include(x=> x.WorkItemSubmissions)
                .Include(x=> x.Work)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            var date = workItem.Date.Date;
            if (model.CheckInTime.HasValue)
                workItem.CheckInTime = date.Add(model.CheckInTime.Value.TimeOfDay);
            else
                workItem.CheckInTime = null;

            if (model.CheckOutTime.HasValue && model.CheckInTime.HasValue)
            {
                if (model.CheckOutTime.Value.TimeOfDay < model.CheckInTime.Value.TimeOfDay)
                    date = date.AddDays(1).Date;
                workItem.CheckOutTime = date.Add(model.CheckOutTime.Value.TimeOfDay);
            }
            else
                workItem.CheckOutTime = null;


            if (workItem.CheckInTime.HasValue)
            {
                workItem.TotalEarlyMins = workItem.CheckInTime < workItem.WorkStartTime ? (workItem.WorkStartTime - workItem.CheckInTime.Value).TotalMinutes : 0;
                workItem.TotalLateMins = workItem.CheckInTime > workItem.WorkStartTime ? (workItem.CheckInTime.Value - workItem.WorkStartTime).TotalMinutes : 0;

                //workItem.CurrentStatus = AttendanceStatus.Recieved;
            }

            workItem.Date = model.Date;
            workItem.DueDate = model.DueDate;
            workItem.WorkStartTime = model.WorkStartTime;
            workItem.WorkEndTime = model.WorkEndTime;

            // change status to completed if all task approve (required)
            AfterApproveWorkItem(workItem);

            context.WorkItems.Update(workItem);
            await context.SaveChangesAsync();
        }

        public void AfterApproveWorkItem(WorkItem workItem)
        {
            var totalApproved = workItem.WorkItemSubmissions.Count(x => x.Status == WorkItemStatus.Approved);
            var totalNotApproved = workItem.WorkItemSubmissions.Count(x => x.Status != WorkItemStatus.Approved);

            workItem.IsCompleted = false;



            if (workItem.Work.Type == WorkType.ExpectClockInRecordsBefore)
            {
                if (workItem.CheckInTime.HasValue)
                {
                    var diff = (workItem.WorkStartTime.TimeOfDay - workItem.CheckInTime.Value.TimeOfDay).TotalMinutes;
                    if(diff >= workItem.Work.MinsBeforeCheckIn)
                    {
                        workItem.Status = WorkItemStatus.Completed;
                        workItem.TotalAmountCredited = workItem.Work.MoreCredit;
                        workItem.TotalAmountDeducted = 0;
                        workItem.IsCompleted = true;
                    }
                    else
                    {
                        workItem.Status = WorkItemStatus.FailedWithDeduction;
                        workItem.TotalAmountDeducted = workItem.Work.LessDeduct;
                        workItem.TotalAmountCredited = 0;
                        workItem.IsCompleted = true;
                    }
                }
                else
                {
                    workItem.Status = WorkItemStatus.FailedWithDeduction;
                    workItem.TotalAmountDeducted = workItem.Work.LessDeduct;
                    workItem.TotalAmountCredited = 0;
                    workItem.IsCompleted = true;
                }
            }
            else
            {
                // completed
                if (totalApproved >= workItem.Work.TotalRequiredCount)
                {
                    workItem.Status = WorkItemStatus.Completed;
                    workItem.TotalAmountCredited = totalApproved * workItem.Work.MoreCredit;
                    workItem.TotalAmountDeducted = 0;
                    workItem.IsCompleted = true;
                }
                else if (workItem.DueDate < DateTime.Now.Date)
                {
                    workItem.TotalAmountCredited = workItem.TotalAmountDeducted = 0;

                    if (totalApproved > 0)
                        workItem.TotalAmountCredited = totalApproved * workItem.Work.MoreCredit;

                    var rem = workItem.Work.TotalRequiredCount - totalApproved;
                    if (rem > 0)
                        workItem.TotalAmountDeducted = (workItem.Work.TotalRequiredCount * workItem.Work.LessDeduct);

                    workItem.Status = WorkItemStatus.FailedWithDeduction;
                    workItem.IsCompleted = true;
                }
            }
            //{
            //    workItem.Status = WorkItemStatus.Draft;

            //    workItem.TotalAmountCredited = (workItem.WorkItemSubmissions.Count(x => x.Status == WorkItemStatus.Approved)) * workItem.Work.MoreCredit;
            //    workItem.TotalAmountDeducted = 0;
            //}
        }

        private AttendanceStatus LastUpdatedAttendanceStatus = AttendanceStatus.Created;

        public AttendanceStatusUpdate GetNewStatusUpdate(AttendanceStatus early)
        {
            LastUpdatedAttendanceStatus = early;
            return new AttendanceStatusUpdate
            {
                ChangedByName = userResolverService.GetUserName(),
                ChangedByUserId = userResolverService.GetUserId(),
                Status = early
            };
        }


        /// <summary>
        /// RUNs Schedule for attendance + Creates next run jobs (repeating)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> RunScheduleForAttendance(int id, bool bySchduler = false)
        {
            var sch = await context.Schedules.FindAsync(id);
            var days = sch.DaysData;
            var workTimes = await companyService.GetWorkTimes();

            if (workTimes.Count <= 0) throw new Exception("Work time or shift was not found");

            // set end date to 1 month if greater AND Schedule to run on yesterday
            // midnight to create more (until enddate is reached)
            bool needMoreSchedule = sch.End > DateTime.Now.AddDays(60);
            var calcEndDate = needMoreSchedule ? DateTime.Now.AddDays(60) : sch.End.Value;
            var nextRunDate = calcEndDate.AddDays(-1);

            List<Attendance> list = new List<Attendance>();
            for (DateTime start = sch.Start.Date; start <= calcEndDate; start = start.AddDays(1))
            {
                if (days.Any(x => x.Date == start.Date))
                {
                    if (days.FirstOrDefault(x => x.Date == start).IsOff)
                        continue;

                    var workTime = workTimes.FirstOrDefault(x => x.Id == days.FirstOrDefault(z => z.Date == start).ShiftId);
                    if (workTime == null) continue;

                    var workEndDateCalc = workTime.EndTime.Hours < workTime.StartTime.Hours ? start.AddDays(1) : start;

                    list.AddRange(sch.EmployeeIdsData.Select(x => new Attendance
                    {
                        ScheduleId = sch.Id,
                        EmployeeId = x.id,
                        CompanyWorkTimeId = workTime.Id,
                        ShiftId = workTime.Id,
                        ShiftColor = workTime.ColorCombination,
                        ShiftName = workTime.ShiftName,
                        CurrentStatus = AttendanceStatus.Created,
                        Date = start.Date,
                        Day = start.Day,
                        Month = start.Month,
                        Year = start.Year,
                        Week = GetWeeekOfYear(start),
                        WorkStartTime = new DateTime(start.Year, start.Month, start.Day, workTime.StartTime.Hours, workTime.StartTime.Minutes, workTime.StartTime.Seconds),
                        WorkEndTime = new DateTime(workEndDateCalc.Year, workEndDateCalc.Month, workEndDateCalc.Day, workTime.EndTime.Hours, workTime.EndTime.Minutes, workTime.EndTime.Seconds),
                        CompanyId = userResolverService.GetCompanyId(),
                        StatusUpdates = new List<AttendanceStatusUpdate> { GetNewStatusUpdate(AttendanceStatus.Created) },
                        bySchduler = bySchduler
                    }).ToList());
                }
            }

            //// EndDate =  new DateTime(2019, 11, (EndTime.Hours < StartTime.Hours ? 18 : 17), EndTime.Hours, EndTime.Minutes, EndTime.Seconds);
            //list.Where(x => x.WorkEndTime.Hour < x.WorkStartTime.Hour).ToList()
            //    .ForEach(x => x.WorkEndTime.AddDays(1));

            //if(needMoreSchedule)
            //{
            //    var jobId = backgroundJobClient.Schedule(() => ScheduleBackgroundJobAsync(sch.Id, "Creating recurring schedule first time"), nextRunDate);

            //    var backgroundJob = new Models.BackgroundJob
            //    {
            //        ScheduleId = sch.Id,
            //        TaskType = TaskType.ScheduleNextRun,
            //        Name = "Schudule: " + sch.Summary,
            //        Details = "Schdeule recurring background job for creating attendance/worktime",
            //        TaskStatus = Models.TaskStatus.Scheduled,
            //        RunDate = nextRunDate
            //    };
            //    backgroundJob.HangfireJobId = jobId;
            //    backgroundJob.NextRunDate = nextRunDate;

            //    sch.HangfireJobId = jobId;
            //    sch.BackgroundJob = backgroundJob;

            //    //backgroundJobClient.Enqueue(() => ScheduleBackgroundJobAsync(sch.Id, "Initiating scheduele " + sch.Repeat + " repearting schedule"));
            //}

            context.Attendances.AddRange(list);
            await context.SaveChangesAsync();

            //int recordUpdateCount = await payrolDbContext.SaveChangesAsync();

            //logger.LogWarning($"{list.Count} Attendance record(s) created for period from [{model.ShiftDurationStart.Date} - {model.ShiftDurationEnd}] for {model.EmployeeIds.Count()} employee(s). [{recordUpdateCount} records affected]");
            return true;
        }


        /// <summary>
        /// Method is enqueued after successful schedul rumc (will created bbackground job if necesasry and updates next run)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<IdentityResult> ScheduleBackgroundJobAsync(int id, string msg)
        {
            var scheduleInDb = await context.Schedules.FindAsync(id);
            if (scheduleInDb == null) return IdentityResult.Failed(new IdentityError { Description = "Item was not found" });

            if (!scheduleInDb.IsRepeating) return IdentityResult.Failed(new IdentityError { Description = "Ite was found, but its not repeating" });

            var runningJobId = scheduleInDb.BackgroundJob;
            scheduleInDb.BackgroundJob.TaskStatus = Models.TaskStatus.Ended;
            scheduleInDb.BackgroundJob.EndedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();

            // runn schedule and create any more background job ()
            await RunScheduleForAttendance(scheduleInDb.Id, bySchduler: true);
            return IdentityResult.Success;



            //if (scheduleInDb.Repeat != RecurringFrequency.Never)
            //{
            //    var backgroundJob = new Models.BackgroundJob
            //    {
            //        TaskType = TaskType.ScheduleNextRun,
            //        Name = "Schudule: " + scheduleInDb.Summary,
            //        Details = "Schdeule recurring background job for creating attendance/worktime",
            //        TaskStatus = Models.TaskStatus.Scheduled
            //    };

            //    switch (scheduleInDb.Repeat)
            //    {
            //        //case RecurringFrequency.Daily:
            //        //    backgroundJob.NextRunDate = 
            //        //    break;
            //        case RecurringFrequency.Weekly:
            //            backgroundJob.NextRunDate = thisWeekEnd;
            //            break;
            //        case RecurringFrequency.Monthly:
            //            backgroundJob.NextRunDate = thisMonthEnd;
            //            break;
            //        default:
            //            break;
            //    }

            //    if (backgroundJob.NextRunDate.HasValue)
            //        backgroundJob.NextRunDate.Value.AddHours(9);

            //    var jobId = backgroundJobClient.Schedule(() => RunScheduleForAttendance( scheduleInDb.Id, "Creating recurring schedule first time"), backgroundJob.NextRunDate);
            //    backgroundJob.HangfireJobId = jobId;

            //    scheduleInDb.HangfireJobId = jobId;
            //    scheduleInDb.BackgroundJob = backgroundJob;
            //    scheduleInDb.back
            //}

        }

        public async Task<bool> RunScheduleForWorkTime(int id)
        {
            var sch = await context.Schedules.FindAsync(id);
            if (sch == null) throw new ApplicationException("Schedule was not found");
            return await RunScheduleForWorkTime(sch);
        }

        public async Task<bool> RunScheduleForWorkTime(Schedule sch)
        {
            if (sch== null) throw new ApplicationException("Schedule was not found");
            var days = sch.DaysData;

            var allWorks = await context.Works
                .Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .ToListAsync();

            if (allWorks.Count <= 0) throw new ApplicationException("Work was not found");

            List<WorkItem> list = new List<WorkItem>();

            // filling out employee list (override if other than specific empl)
            if (sch.IsForAllEmployees)
                sch.EmployeeIds = (await companyService.GetEmployeesOfCurremtCompany()).Select(x => x.Id).ToArray();
            if (sch.IsForDepartment)
                sch.EmployeeIds = (await companyService.GetEmployeesOfCurremtCompany(deptId: sch.DepartmentId)).Select(x => x.Id).ToArray();

            if (sch.EmployeeIdsData != null && !sch.IsForAllEmployees && !sch.IsForDepartment)
                sch.EmployeeIds = sch.EmployeeIdsData.Select(x => x.id).ToArray();

            if (sch.EmployeeIds == null)
                throw new ApplicationException("Fatal error occured while running work item schedule");

            if (sch.End.HasValue && sch.Start.Date > sch.End.Value) throw new ApplicationException("Start date cannot be greater than today");

            if (sch.IsRepeating)
            {
                var loopCount = 1;
                DateTime? nextRunDate = sch.Start;

                // starting in past
                double daysFromStartToToday = 1;
                if (sch.IsRepeatEndDateNever == false)
                {
                    loopCount = GetLoopCount(sch.Repeat, sch.Start, sch.End.Value);

                    list.AddRange(
                        GenerateWorkItemsForLoopRepeating
                            (loopCount, allWorks, sch, out nextRunDate));
                }
                else
                {
                    // today inbetween dates (then be careful)
                    if (sch.Start >= DateTime.Now && sch.End < DateTime.Now)
                    {
                        loopCount = GetLoopCount(sch.Repeat, sch.Start, DateTime.Now);

                        list.AddRange(
                            GenerateWorkItemsForLoopRepeating
                                (loopCount, allWorks, sch, out nextRunDate, overriddenEndDate: DateTime.Now.Date));

                        if (sch.IsRepeatEndDateNever && nextRunDate.HasValue)
                        {
                            // Repeat end date is never 
                            // => then schedule background job to run
                            var backJob = await CreateNewBackgroundJobAsync(sch, nextRunDate.Value);

                            var svc = (BackgroundJobService)httpContextAccessor.HttpContext.RequestServices.GetService(typeof(BackgroundJobService));
                            var hangfireJobId = backgroundJobClient
                                    .Schedule(() => svc
                                                .CreateNewWorkItem(backJob.Identifier, backJob.TaskType),
                                                backJob.RunDate);

                            backJob.HangfireJobId = hangfireJobId;
                            sch.backgroundJobs.Add(backJob);
                            sch.HasBackgroundJob = true;
                            sch.HangfireJobId = hangfireJobId;
                            //sch.BackgroundJob = backJob;
                            sch.NextRunDate = nextRunDate;
                        }
                        else
                        {
                            // create for remaining days
                            loopCount = GetLoopCount(sch.Repeat, DateTime.Now, sch.End.Value);

                            if (nextRunDate.HasValue && nextRunDate.Value > DateTime.Now)
                                list.AddRange(
                                    GenerateWorkItemsForLoopRepeating
                                        (loopCount, allWorks, sch, out nextRunDate, overriddenStartDate: nextRunDate.Value));
                        }
                    }
                    else
                    {
                        loopCount = GetLoopCount(sch.Repeat, sch.Start, sch.End.Value);

                        list.AddRange(
                            GenerateWorkItemsForLoopRepeating
                                (loopCount, allWorks, sch, out nextRunDate));
                    }
                    
                }
            }
            else
            {

                list.AddRange(
                    GenerateWorkAttendanceRecordsForWeek(sch, allWorks));
            }


            //list.Where(x => x.WorkEndTime.Hour < x.WorkStartTime.Hour).ToList()
            //    .ForEach(x => x.WorkEndTime.AddDays(1));


            context.WorkItems.AddRange(list);
            context.Schedules.Update(sch);

            await context.SaveChangesAsync();

            return true;
        }

        public async Task<Models.BackgroundJob> CreateNewBackgroundJobAsync(Schedule sch, DateTime nextRunDate)
        {
            if(await context.BackgroundJobs.AnyAsync(x=> x.IsActive && x.TaskStatus == Models.TaskStatus.Scheduled && x.ScheduleId == sch.Id))
                if (sch.Start.Date > DateTime.UtcNow.Date) throw new ApplicationException("There are already schedules set for repeating in this schedule. Please wait until it has ended");

            return new Models.BackgroundJob
            {
                Name = TaskType.ScheduleWorkItemNextRun.ToString(),
                Identifier = Guid.NewGuid(),
                ScheduleId = sch.Id,
                RunDate = nextRunDate,
                TaskType = TaskType.ScheduleWorkItemNextRun,
                TaskStatus = Models.TaskStatus.Scheduled,
                CompanyAccountId = userResolverService.GetCompanyId()
            };


        }

        private List<WorkItem> GenerateWorkAttendanceRecordsForWeek(Schedule sch, List<Work> allWorks)
        {
            var days = sch.DaysData;
            List<WorkItem> workItems = new List<WorkItem>();


            for (DateTime start = sch.Start.Date; start <= sch.End; start = start.AddDays(1))
            {
                if (!days.Any(x => x.Date == start))
                    continue;

                if (days.FirstOrDefault(x => x.Date == start).IsOff)
                    continue;

                var workIds = days.FirstOrDefault(z => z.Date == start).WorkIds;
                if (workIds.Length <= 0) continue;

                foreach (var wId in workIds)
                {
                    var workTime = allWorks.FirstOrDefault(x => x.Id == wId);
                    var workEndDateCalc = workTime.EndTime.Hours < workTime.StartTime.Hours ? start.AddDays(1) : start;


                    workItems.AddRange(sch.EmployeeIds.Select(x => new WorkItem
                    {
                        ScheduleId = sch.Id > 0 ? sch.Id  : (int?)null,
                        EmployeeId = x,
                        HasAttendance = false,
                        Status = WorkItemStatus.Draft,
                        Date = start.Date,
                        Day = start.Day,
                        Month = start.Month,
                        Year = start.Year,
                        Week = GetWeeekOfYear(start),
                        WorkId = workTime.Id,
                        //ShiftColor = workTime.ColorCombination,
                        RemainingSubmissions = workTime.TotalRequiredCount,
                        WorkStartTime = new DateTime(start.Year, start.Month, start.Day, workTime.StartTime.Hours, workTime.StartTime.Minutes, workTime.StartTime.Seconds),
                        WorkEndTime = new DateTime(workEndDateCalc.Year, workEndDateCalc.Month, workEndDateCalc.Day, workTime.EndTime.Hours, workTime.EndTime.Minutes, workTime.EndTime.Seconds),
                        DueDate = new DateTime(workEndDateCalc.Year, workEndDateCalc.Month, workEndDateCalc.Day, 23, 59, 59),
                        //WorkEndTime = new DateTime(start.Year, start.Month, start.Day, workTime.EndTime.Hours, workTime.EndTime.Minutes, workTime.EndTime.Seconds),
                    }).ToList());
                }
            }

            return workItems;
        }

        private static int GetLoopCount(RecurringFrequency recurringFrequency, DateTime start, DateTime end)
        {

            // 27/7 = 3.85  ==< both is 4 
            // 28/7 = 4     ==< both is 4
            var daysFromStartToToday = Math.Ceiling((end.Date - start.Date).TotalDays);
            int loopCount = 1;

            switch (recurringFrequency)
            {
                case RecurringFrequency.Never:
                    throw new ApplicationException("repeating frequency cannot be never");
                case RecurringFrequency.Daily:
                    loopCount = Convert.ToInt32(daysFromStartToToday);
                    if (loopCount == 0)
                        loopCount = 1;
                    if (daysFromStartToToday > 90)
                        throw new ApplicationException("Days from start date until today cannot be more than 7");
                    break;
                case RecurringFrequency.Weekly:
                    loopCount = Convert.ToInt32(daysFromStartToToday / 7);
                    break;
                case RecurringFrequency.Every2Weeks:
                    loopCount = Convert.ToInt32(daysFromStartToToday / (7 * 2));
                    break;
                case RecurringFrequency.Monthly:
                    loopCount = Convert.ToInt32(daysFromStartToToday / 30);
                    break;
                default:
                    break;
            }

            return loopCount;
        }

        public List<WorkItem> GenerateWorkItemsForLoopRepeating(int loopCount, List<Work> allWorks, Schedule sch, out DateTime? nextRunDate, DateTime? overriddenStartDate = null, DateTime? overriddenEndDate = null)
        {
            if (overriddenStartDate.HasValue && !overriddenEndDate.HasValue &&  overriddenStartDate.Value > sch.End)
                throw new ApplicationException("Overridden start date cannot be before end date");

            if (overriddenEndDate.HasValue && !overriddenStartDate.HasValue && sch.Start > overriddenEndDate.Value)
                throw new ApplicationException("start date cannot be before Overridden  date");

            if (overriddenStartDate.HasValue && overriddenEndDate.HasValue && overriddenStartDate.Value > overriddenEndDate.Value)
                throw new ApplicationException("Overridden start date cannot be before overridden end date");

            var start = overriddenStartDate ?? sch.Start;
            var end = overriddenEndDate ?? sch.End;
            List<WorkItem> workItems = new List<WorkItem>();
            nextRunDate = null;
            var addDays = 1;
            var addDaysFirst = 0;
            bool updateOnNextRecurring = false;


            var workTime = allWorks.FirstOrDefault(x => x.Id == sch.WorkId);
            for (int i = 0; i < loopCount; i++)
            {
                var workEndDateCalc = workTime.EndTime.Hours < workTime.StartTime.Hours ? start.AddDays(1) : start;

                workItems.AddRange(sch.EmployeeIds.Select(x => new WorkItem
                {
                    ScheduleId = sch.Id,
                    EmployeeId = x,
                    HasAttendance = false,
                    Status = WorkItemStatus.Draft,
                    Date = start.Date,
                    Day = start.Day,
                    Month = start.Month,
                    Year = start.Year,
                    Week = GetWeeekOfYear(start),
                    WorkId = workTime.Id,
                    //ShiftColor = workTime.ColorCombination,
                    RemainingSubmissions = workTime.TotalRequiredCount,
                    WorkStartTime = new DateTime(start.Year, start.Month, start.Day, workTime.StartTime.Hours, workTime.StartTime.Minutes, workTime.StartTime.Seconds),
                    WorkEndTime = new DateTime(workEndDateCalc.Year, workEndDateCalc.Month, workEndDateCalc.Day, workTime.EndTime.Hours, workTime.EndTime.Minutes, workTime.EndTime.Seconds),
                    DueDate = new DateTime(workEndDateCalc.Year, workEndDateCalc.Month, workEndDateCalc.Day, 23, 59, 59),
                    //TotalAllowance = workTime.TotalRequiredCount * workTime.de
                    //WorkEndTime = new DateTime(start.Year, start.Month, start.Day, workTime.EndTime.Hours, workTime.EndTime.Minutes, workTime.EndTime.Seconds),
                }).ToList());


                if (sch.Repeat == RecurringFrequency.Daily)
                    start = start.AddDays(1);
                else if (sch.Repeat == RecurringFrequency.Weekly)
                    start = start.AddDays(7);
                else if (sch.Repeat == RecurringFrequency.Every2Weeks)
                    start = start.AddDays(14);
                else if (sch.Repeat == RecurringFrequency.Monthly)
                    start = start.AddMonths(1);
                else
                    start = start.AddDays(1);

                // break if next date is greater than end date
                if (end.HasValue && start > end.Value)
                    break;
            }

            if (sch.IsRepeating)
                nextRunDate = start;

            return workItems;
        }
    }
}
