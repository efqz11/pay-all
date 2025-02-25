using GAF;
using GAF.Operators;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payroll.Database;
using Payroll.Filters;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Controllers
{
    [EmployeeRoleAuthorize(Roles = new[] { Roles.Company.all_employees, Roles.Company.supervisor, Roles.Company.hr_manager})]
    public class ScheduleController : BaseController
    {
        private readonly PayrollDbContext payrolDbContext;
        private readonly ILogger<CompanyController> logger;
        private readonly AccessGrantService accessGrantService;
        private readonly AccountDbContext context;
        private readonly FileUploadService fileUploadService;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly ScheduleService scheduleService;
        private readonly UserResolverService userResolverService;
        private readonly CompanyService companyService;
        private readonly EmployeeService employeeService;
        private readonly RequestService requestService;
        private readonly SynchronizationService synchronizationService;

        public ScheduleController(PayrollDbContext context, ILogger<CompanyController> logger, AccessGrantService accessGrantService, AccountDbContext accountDbContext, FileUploadService fileUploadService, IBackgroundJobClient backgroundJobClient, ScheduleService scheduleService, UserResolverService userResolverService, CompanyService companyService, EmployeeService employeeService, RequestService requestService)
        {
            this.payrolDbContext = context;
            this.logger = logger;
            this.accessGrantService = accessGrantService;
            this.context = accountDbContext;
            this.fileUploadService = fileUploadService;
            this.backgroundJobClient = backgroundJobClient;
            this.scheduleService = scheduleService;
            this.userResolverService = userResolverService;
            this.companyService = companyService;
            this.employeeService = employeeService;
            this.requestService = requestService;
        }


        private async Task<IActionResult> GeneticAlgorythm222()
        {
            const double crossoverProbability = 0.65;
            const double mutationProbability = 0.08;
            const int elitismPercentage = 5;

            //create the population
            var population = new Population(100, 44, false, false);

            //create the genetic operators 
            var elite = new Elite(elitismPercentage);

            var crossover = new Crossover(crossoverProbability, true)
            {
                CrossoverType = CrossoverType.SinglePoint
            };

            var mutation = new BinaryMutate(mutationProbability, true);

            //create the GA itself 
            var ga = new GeneticAlgorithm(population, EvaluateFitness);

            //subscribe to the GAs Generation Complete event 
            ga.OnGenerationComplete += ga_OnGenerationComplete;

            //add the operators to the ga process pipeline 
            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutation);

            //run the GA 
            ga.Run(TerminateAlgorithm);

            return View();
        }


        public static double EvaluateFitness(Chromosome chromosome)
        {
            double fitnessValue = -1;
            if (chromosome != null)
            {
                //this is a range constant that is used to keep the x/y range between -100 and +100
                var rangeConst = 200 / (System.Math.Pow(2, chromosome.Count / 2) - 1);

                //get x and y from the solution
                var x1 = Convert.ToInt32(chromosome.ToBinaryString(0, chromosome.Count / 2), 2);
                var y1 = Convert.ToInt32(chromosome.ToBinaryString
                (chromosome.Count / 2, chromosome.Count / 2), 2);

                //Adjust range to -100 to +100
                var x = (x1 * rangeConst) - 100;
                var y = (y1 * rangeConst) - 100;

                //using binary F6 for fitness.
                var temp1 = System.Math.Sin(System.Math.Sqrt(x * x + y * y));
                var temp2 = 1 + 0.001 * (x * x + y * y);
                var result = 0.5 + (temp1 * temp1 - 0.5) / (temp2 * temp2);

                fitnessValue = 1 - result;
            }
            else
            {
                //chromosome is null
                throw new ArgumentNullException("chromosome",
                    "The specified Chromosome is null.");
            }

            return fitnessValue;
        }

        public static bool TerminateAlgorithm(Population population,
        int currentGeneration, long currentEvaluation)
        {
            return currentGeneration > 1000;
        }

        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            //get the best solution 
            var chromosome = e.Population.GetTop(1)[0];

            //decode chromosome

            //get x and y from the solution 
            var x1 = Convert.ToInt32(chromosome.ToBinaryString(0, chromosome.Count / 2), 2);
            var y1 = Convert.ToInt32(chromosome.ToBinaryString(chromosome.Count / 2, chromosome.Count / 2), 2);

            //Adjust range to -100 to +100 
            var rangeConst = 200 / (System.Math.Pow(2, chromosome.Count / 2) - 1);
            var x = (x1 * rangeConst) - 100;
            var y = (y1 * rangeConst) - 100;

            //display the X, Y and fitness of the best chromosome in this generation 
            Console.WriteLine("x:{0} y:{1} Fitness{2}", x, y, e.Population.MaximumFitness);
        }

        public async Task<IActionResult> Index(int page = 1, int limit = 10, DateTime? date = null, DateTime? end = null, bool showUnScheduled = false)
        {
            if (date.HasValue)
                scheduleService.SetBaseDate(date);

            DateTime _endDate = scheduleService.thisWeekEnd;
            if (end.HasValue)
                _endDate = end.Value;
            await SetWorkingWeekDisplayViewBags(scheduleService.thisWeekStart, _endDate);

            ViewBag.showUnScheduled = showUnScheduled;
            ViewBag.WeekStartDay = await context.CompanyAccounts.Where(c => c.Id == userResolverService.GetCompanyId()).Where(MyExpressions.IsCompanyPayrpll)
            .Select(a => a.WeekStartDay)
            .FirstOrDefaultAsync();

            var selector = GetEmployeeSelectorModal();
            var data = await scheduleService.GetCurrentSchedule(page, limit, showUnScheduled, false, selector, scheduleService.thisWeekStart, _endDate);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Weekly", data);
            return View(data);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var scheduleInDb = await payrolDbContext.Schedules.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Id == id)
                .Include(x => x.ParentSchedule)
                .Include(x => x.FololwingSchedules)
                .FirstOrDefaultAsync();
            if (scheduleInDb == null)
                return Unauthorized();

            List<WeeklyEmployeeShiftVm> list = await scheduleService.GetWeeklyEmployeeShiftVm(scheduleInDb);
            ViewBag.CompanyWorkStartDay = await companyService.GetWorkStartDayOfWeek();
            ViewBag.WeekEmployeeShift = list;
            ViewBag.ActiveContracts = await employeeService.GetActiveEmploymentsDuring(scheduleInDb.Start, scheduleInDb.End.Value, scheduleInDb.EmployeeIds);


            ViewBag.WorkItemCount = await payrolDbContext.WorkItems.CountAsync(x => x.ScheduleId == id);
            ViewBag.AttendancesCount = await payrolDbContext.Attendances.CountAsync(x => x.ScheduleId == id);
            //vm.ScheduleInteractions = context.EmployeeInteractions
            //    .Where(a => scheduleIndb.EmployeeIds.Contains(a.EmployeeId)
            //    && a.ScheduleId == id & a.IsActive).ToList();
            //ViewBag.RosterGeneratedDate = scheduleIndb.RosterGeneratedDate;

            return View(scheduleInDb);
        }

        private async Task SetWorkingWeekDisplayViewBags(DateTime start, DateTime end)
        {
            ViewBag.WeekStart = start;
            ViewBag.WeekEnd = end;
            ViewBag.CurrentRangeDisplay = start.ToString("MMM dd") + " - " +
                (end.ToString(end.Month == start.Month ? "dd, yyyy" : "MMM dd, yyyy"));
        }

        public async Task<IActionResult> Tasks(int page = 1, int limit = 10, DateTime? date = null, bool showUnScheduled = false)
        {
            if (date.HasValue)
                scheduleService.SetBaseDate(date);

            await SetWorkingWeekDisplayViewBags(scheduleService.thisWeekStart, scheduleService.thisWeekEnd);

            var data = await scheduleService.GetCurrentSecduledTasks(GetEmployeeSelectorModal(), page, limit);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Tasks", data);
            return View(data);
        }
        
        
        public async Task<IActionResult> Calendar(DateTime? date = null,  string show = "all")
        {
            if (date.HasValue)
                scheduleService.SetBaseDate(new DateTime(date.Value.Year, date.Value.Month, 1));

            await SetWorkingWeekDisplayViewBags(scheduleService.thisMonthStart, scheduleService.thisMonthEnd);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Calendar", await scheduleService.GetWorkingCalendar(GetWorkingEmployeeId(), show));
            return View(await scheduleService.GetWorkingCalendar(GetWorkingEmployeeId(), show));
        }


        public async Task<IActionResult> SelectDates(DateTime? start, DateTime? end, int[] empId = null, int? id = 0, ScheduleFor @for = ScheduleFor.Attendance)
        {
            var vm = new ScheduleCreateVm();
            vm.ShiftDurationStart = start ?? scheduleService.thisWeekStart;
            vm.ShiftDurationEnd = end ?? scheduleService.thisWeekEnd;
            vm.EmployeeIds = empId;
            vm.ScheduleFor = @for;

            return PartialView("_SelectDates", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectDates(ScheduleCreateVm model)
        {
            if ((model.ShiftDurationEnd - model.ShiftDurationStart).HasValue && (model.ShiftDurationEnd - model.ShiftDurationStart).Value.TotalDays > 10)
                return ThrowJsonError("Maximun duration cannot exceed 10 days");

            switch (model.ScheduleFor)
            {
                case ScheduleFor.Attendance:
                    return RedirectToAction(nameof(AddOrUpdateSchedule), new { start = model.ShiftDurationStart, end = model.ShiftDurationEnd, empIds = model.EmployeeIds });
                case ScheduleFor.Work:
                    return RedirectToAction(nameof(CreateWorkItem), new { start = model.ShiftDurationStart, end = model.ShiftDurationEnd, empIds = model.EmployeeIds });
                default:
                    return ThrowJsonError("Unexpected error has occured, please try again");
            }
        }

        public async Task<IActionResult> AddOrUpdateSchedule(DateTime? start, DateTime? end, int[] empIds, int id = 0)
        {
            Schedule scheduleInDb = null;
            if (id > 0)
            {
                 scheduleInDb = await payrolDbContext.Schedules.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Id == id)
                    .FirstOrDefaultAsync();
            }

            var lsitEmps = await accessGrantService.GetEmployeesOfCurremtCompany();
            //ViewBag.EmployeeIds = new MultiSelectList(lsitEmps.Select(x => new { x.Id, Name = $"<img src='{Url.Content(x.Avatar ?? DefaultPictures.default_user)}' height='20px' /> {x.GetSystemName(User)}" }).ToList(), "Id", "Name", scheduleInDb?.EmployeeIdsData?.Select(x=> x.id)?.ToArray() ?? empIds);

            var firstDayOfWeek = await context.CompanyAccounts.Where(x => x.Id == userResolverService.GetCompanyId()).Select(x => x.WeekStartDay).FirstOrDefaultAsync();


            ScheduleCreateVm vm = new ScheduleCreateVm();
            ViewBag.WeekStart = vm.ShiftDurationStart = start ?? scheduleService.thisWeekStart;
            ViewBag.WeekEnd = vm.ShiftDurationEnd = end ?? scheduleService.thisWeekEnd;

            vm.Days = Enumerable.Range(0, (int)(vm.ShiftDurationEnd - vm.ShiftDurationStart).Value.TotalDays)
                .Select((i, x) => new DayVm
                {
                    DayOfWeek = vm.ShiftDurationStart.AddDays(i).DayOfWeek,
                    Date = vm.ShiftDurationStart.AddDays(i).Date,
                    CompanyId = userResolverService.GetCompanyId()
                }).ToList();
            
            vm.Shifts = await payrolDbContext.CompanyWorkTimes.Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .ToDictionaryAsync(x => x.Id, x => x.ShiftName + " (" + x.Duration + ")");

            //await SetWorkingWeekDisplayViewBags();
            return PartialView("_AddOrUpdateSchedule", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateSchedule(ScheduleCreateVm model)
        {
            if (ModelState.IsValid)
            {
                if (model.ShiftDurationStart > model.ShiftDurationEnd)
                    return ThrowJsonError($"End date cannot be greater than Start date");

                if (model.EmployeeIds == null || model.EmployeeIds.Length <= 0)
                    return ThrowJsonError($"Please choose atleast one employee");

                if (model.Days == null) //  && model.Days.Count() != 7)
                    return ThrowJsonError($"Err, you need to add all days!");


                var validationResult = await scheduleService.ValidateDaysForOverlaps(model.Days, model);
                if(validationResult.Succeeded == false)
                    return ThrowJsonError(validationResult.Errors.FirstOrDefault()?.Description);

                //// without overlapping can create
                //// check if employee has more scheudles
                //if (await payrolDbContext.Attendances.AnyAsync(x => x.Date >= model.ShiftDurationStart && x.Date <= model.ShiftDurationEnd && model.EmployeeIds.Contains(x.EmployeeId)))

                //    // yes! then check for overlapping (can't create overlapping betwee work timees)
                //    if (await payrolDbContext.Attendances.AnyAsync(x =>
                //        model.ShiftDurationStart < x.WorkEndTime && x.WorkStartTime < model.ShiftDurationEnd))
                //        return ThrowJsonError("This record(s) will create overlapping attendance records, Kindly check work time");
                ////return ThrowJsonError("Schedules are already created for this selected period");
            }

            if (ModelState.IsValid)
            {

                model.ScheduleFor = ScheduleFor.Attendance;
                var currCompanyId = userResolverService.GetCompanyId();

                var addOrUpdateSchedule = await scheduleService.CreateNewScheduleAsync(model);

                if (model.Id <= 0)
                {
                    payrolDbContext.Schedules.Add(addOrUpdateSchedule);
                }
                else
                {
                    addOrUpdateSchedule.Id = model.Id;
                    payrolDbContext.Schedules.Update(addOrUpdateSchedule);
                }
                
                int recordUpdateCount = await payrolDbContext.SaveChangesAsync();

                logger.LogWarning($"{true} Attendance record(s) created for period from [{model.ShiftDurationStart.Date} - {model.ShiftDurationEnd}] for {model.EmployeeIds.Count()} employee(s). [{recordUpdateCount} records affected]");

                await scheduleService.RunScheduleForAttendance(addOrUpdateSchedule.Id);

                //if (model.SaveAndRun)
                //    return await RunSchedule(addOrUpdateSchedule.Id);

                return await Index(date: model.ShiftDurationStart.Date);
            }

            return ThrowJsonError();
        }

        public async Task<IActionResult> RunSchedule(int id)
        {
            Schedule scheduleInDb = await payrolDbContext.Schedules.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Id == id)
                   .FirstOrDefaultAsync();
            if (scheduleInDb == null) return ThrowJsonError("Schedule was not found!");


            scheduleInDb.EmployeeIds = scheduleInDb.EmployeeIdsData?.Select(x => x.id).ToArray();
            //scheduleInDb.IgnoreDays = scheduleInDb.IgnoreDaysData?.ToArray();

            return PartialView("_RunSchedule", scheduleInDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunSchedule(Schedule model)
        {
            Schedule scheduleInDb = null;
            if (ModelState.IsValid)
            {
                if (model.Repeat != RecurringFrequency.Never
                    && !model.RepeatEndDate.HasValue)
                    //model.IsRepeatEndDateNever = true;
                    return ThrowJsonError("Please provider repeat end date or choose never");

                if (model.Id <= 0)
                    return ThrowJsonError("Schdule was not found");

                scheduleInDb = await payrolDbContext.Schedules.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Id == model.Id)
                   .FirstOrDefaultAsync();
                if (scheduleInDb == null) return ThrowJsonError("Schedule was not found");

                if (model.Start > model.End)
                    return ThrowJsonError($"End date cannot be greater than Start date");

                if (scheduleInDb.EmployeeIds != null)
                    if (await payrolDbContext.Attendances.AnyAsync(x => x.Date >= model.Start && x.Date <= model.End && scheduleInDb.EmployeeIds.Contains(x.EmployeeId) && x.CompanyId == userResolverService.GetCompanyId()))

                        // yes! then check for overlapping (can't create overlapping betwee work timees)
                        if (await payrolDbContext.Attendances.AnyAsync(x => model.Start < x.WorkEndTime && x.WorkStartTime < model.Start && scheduleInDb.EmployeeIds.Contains(x.EmployeeId) && x.CompanyId == userResolverService.GetCompanyId()))
                            return ThrowJsonError("This record(s) will create overlapping attendance records, Kindly check work time");
                        //return ThrowJsonError("Schedules are already created for this selected period");


                if (scheduleInDb.Repeat != RecurringFrequency.Never)
                {
                    if (scheduleInDb.Repeat == RecurringFrequency.Daily && (scheduleInDb.End.Value - scheduleInDb.Start).TotalDays > 1)
                        return ThrowJsonError("Scheduled date range is more than a month, cant repeat schedule");
                    if (scheduleInDb.Repeat == RecurringFrequency.Weekly && (scheduleInDb.End.Value - scheduleInDb.Start).TotalDays > 7)
                        return ThrowJsonError("Scheduled date range is more than a week, cant repeat schedule");
                    if (scheduleInDb.Repeat == RecurringFrequency.Monthly && (scheduleInDb.End.Value - scheduleInDb.Start).TotalDays > 31)
                        return ThrowJsonError("Scheduled date range is more than a month, cant repeat schedule");
                }

                var allEmpIdsInScheudle = scheduleInDb.EmployeeIdsData.Select(e => e.id).ToArray();

                // without overlapping can create
                // check if employee has more scheudles
                if (await payrolDbContext.Attendances.AnyAsync(x => x.Date >= model.Start && x.Date <= model.End && scheduleInDb.EmployeeIdsData != null && allEmpIdsInScheudle.Contains(x.EmployeeId) && x.CompanyId == userResolverService.GetCompanyId()))

                    // yes! then check for overlapping (can't create overlapping betwee work timees)
                    if (await payrolDbContext.Attendances.AnyAsync(x =>
                     model.Start < x.WorkEndTime && x.WorkStartTime < model.End
                     && scheduleInDb.EmployeeIdsData != null && allEmpIdsInScheudle.Contains(x.EmployeeId) && x.CompanyId == userResolverService.GetCompanyId()))
                        return ThrowJsonError("This record(s) will create overlapping attendance records, Kindly check work time");


                //if (await payrolDbContext.Attendances.AnyAsync(x => x.Date >= model.Start && x.Date <= model.End && scheduleInDb.EmployeeIdsData != null && allEmpIdsInScheudle.Contains(x.EmployeeId)))
                //    return ThrowJsonError("Schedules are already created for this selected period");


                // RULE 
                if ((model.End.Value - model.Start).TotalDays >= 32)
                    model.End = model.Start.AddDays(31);
            }

            if (ModelState.IsValid)
            {

                //var workTime = await context.CompanyAccountWorkTimes.Include(x => x.CompanyAccount)
                //    .FirstOrDefaultAsync(x => x.Id == model.ShiftId);
                //if (workTime == null) return ThrowJsonError("Work time was not found");

                model.ScheduleFor = ScheduleFor.Attendance;
                model.CompanyId = userResolverService.GetCompanyId();
                scheduleInDb.Start = model.Start;
                scheduleInDb.End = model.End;
                scheduleInDb.Repeat = model.Repeat;
                scheduleInDb.RepeatEndDate = model.RepeatEndDate;
                scheduleInDb.IsRepeatEndDateNever = model.IsRepeatEndDateNever;
                
                await context.SaveChangesAsync();

                await scheduleService.RunScheduleForAttendance(scheduleInDb.Id);


                return await Index(date: model.Start.Date);
            }

            return ThrowJsonError(ModelState);
        }

        public async Task<IActionResult> ViewSchedule(int id, int wiId)
        {
            var schedule = await payrolDbContext.Schedules
                .Include(x=> x.backgroundJobs)
                .Include(x=> x.Department)
                .FirstOrDefaultAsync(x=> x.Id == id);
            if (schedule == null)
                return ThrowJsonError("Schedule was not found");
            ViewBag.Work = await payrolDbContext.Works.FindAsync(schedule.WorkId);
            ViewBag.WorkItemCount = await payrolDbContext.WorkItems.CountAsync(x => x.ScheduleId == schedule.Id);
            ViewBag.WorkItemId = wiId;
            return PartialView("_ViewSchedule", schedule);
        }


        public async Task<IActionResult> CreateWorkItem(DateTime? start, DateTime? end, int[] workId = null, int[] empId = null)
        {
            var lsitEmps = await accessGrantService.GetEmployeesOfCurremtCompany();
            //ViewBag.EmployeeIds = new MultiSelectList(lsitEmps.Select(x => new { x.Id, Name = $"<img src='{Url.Content(x.Avatar ?? DefaultPictures.default_user)}' height='20px' /> {x.GetSystemName(User)}" }).ToList(), "Id", "Name", empId);

            ViewBag.DepartmentId = new SelectList(await accessGrantService.GetDepartmentsOfCurremtCompany(addEmptyOption: true, emptyOptionLabel: "Choose Department"), "Id", "Name");


            var firstDayOfWeek = await context.CompanyAccounts.Where(x => x.Id == userResolverService.GetCompanyId()).Select(x => x.WeekStartDay).FirstOrDefaultAsync();
            //ViewBag.IgnoreDays = new MultiSelectList(Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().OrderBy(x => (x - firstDayOfWeek + 7) % 7).ToDictionary(x=> (int)x, x=> x.ToString()), "Key", "Value");

            //ViewBag.ShiftId = new SelectList(await context.CompanyAccountWorkTimes.Where(x=> x.CompanyAccountId == userResolverService.GetCompanyId())
            //    .Select(x=> new { x.Id, Shift = x.ShiftName + " (" + x.Duration + ")"})
            //    .ToArrayAsync(), "Id", "Shift");

            ScheduleCreateVm vm = new ScheduleCreateVm();

            ViewBag.WeekStart = vm.ShiftDurationStart = start ?? scheduleService.thisWeekStart;
            ViewBag.WeekEnd = vm.ShiftDurationEnd = end ?? scheduleService.thisWeekEnd;

            vm.Days = Enumerable.Range(0, (int)(vm.ShiftDurationEnd - vm.ShiftDurationStart).Value.TotalDays)
                .Select((i, x) => new DayVm
                {
                    DayOfWeek = vm.ShiftDurationStart.AddDays(i).DayOfWeek,
                    Date = vm.ShiftDurationStart.AddDays(i).Date,
                    CompanyId = userResolverService.GetCompanyId(),
                    WorkIds = workId
                }).ToList();

            var cmpWorks = await scheduleService.GetCompanyWorksAsync();
            vm.Works = cmpWorks.Select(x => new Work { Id = x.Id, Name = x.Name }).ToList();
            vm.Works.Insert(0, new Work { Id = 0, Name = "Choose Work" });
            

            return PartialView("_CreateWorkItem", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWorkItem(ScheduleCreateVm model)
        {
            if (ModelState.IsValid)
            {
                if (model.ShiftDurationEnd.HasValue 
                        && model.ShiftDurationStart > model.ShiftDurationEnd)
                    return ThrowJsonError($"End date cannot be greater than Start date");

                if (!model.IsForDepartment)
                    if (model.EmployeeIds == null || model.EmployeeIds.Length <= 0)
                        return ThrowJsonError($"Please choose atleast one employee");

                if (model.IsForDepartment)
                    if (model.DepartmentId <= 0 ||
                        !await payrolDbContext.Departments
                            .AnyAsync(x => x.CompanyId == userResolverService.GetCompanyId()
                            && x.Id == model.DepartmentId))
                        return ThrowJsonError($"Department was not found");

                if (model.IsRecurring == false)
                {
                    if(model.ShiftDurationEnd.HasValue == false)
                        return ThrowJsonError($"End date need to be set");

                    if (model.Days != null && model.Days.Any(x => x.IsOff == false && (x.WorkIds == null || x.WorkIds.Length <= 0)))
                        return ThrowJsonError($"Please choose work for days that arent off");

                    //if (await payrolDbContext.WorkItems.AnyAsync(x => x.Date >= model.ShiftDurationStart && x.Date <= model.ShiftDurationEnd && model.EmployeeIds.Contains(x.EmployeeId)))
                    //    return ThrowJsonError("Schedules are already created for this selected period");
                }

                if (model.IsRecurring)
                {
                    if(!await payrolDbContext.Works.AnyAsync(x=> x.CompanyId == userResolverService.GetCompanyId() 
                    && x.Id == model.WorkId))
                        return ThrowJsonError("Work item was not found");

                    if(model.RecurringFrequency == RecurringFrequency.Never)
                        return ThrowJsonError("Repeating frequency cannot be never");


                    // return ThrowJsonError("end date>" + model.ShiftDurationEnd);
                }
            }

            if (ModelState.IsValid)
            {

                model.ScheduleFor = ScheduleFor.Work;
                //model.CompanyId = userResolverService.GetCompanyId();


                var newschedule = await scheduleService.CreateNewScheduleAsync(model);
                if (model.Id <= 0)
                {
                    payrolDbContext.Schedules.Add(newschedule);
                }
                else
                {
                    newschedule.Id = model.Id;
                    payrolDbContext.Schedules.Update(newschedule);
                }

                int recordUpdateCount = await payrolDbContext.SaveChangesAsync();

                logger.LogWarning($"{true} Work item record(s) created for period from [{model.ShiftDurationStart.Date} - {model.ShiftDurationEnd}]");
                if (model.EmployeeIds != null)
                    logger.LogWarning($" for {model.EmployeeIds.Count()} employee(s). [{recordUpdateCount} records affected]");
                if (model.IsForDepartment)
                    logger.LogWarning($" for specific department. [{recordUpdateCount} records affected]");
                //if (model.IsForDepartment)
                //    logger.LogWarning($" for specific department. [{recordUpdateCount} records affected]");

                try
                {
                    await scheduleService.RunScheduleForWorkTime(newschedule.Id);

                }
                catch (ApplicationException ex)
                {
                    return ThrowJsonError(ex.Message);
                }
                catch (Exception ex)
                {
                    return ThrowJsonError("Fatal error: " + ex.Message);
                }

                return await Tasks(date: model.ShiftDurationStart.Date);
            }

            return ThrowJsonError();
        }


        public async Task<IActionResult> AddOrUpdateWork(int id = 0)
        {
            Work model = null;
            if (id > 0)
                model = await payrolDbContext.Works.FindAsync(id);

            if (model == null && id > 0)
                return ThrowJsonError("Work was not found!");

            model = model ?? new Work();
            return PartialView("_AddOrUpdateWork", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateWork(Work model)
        {

            if (ModelState.IsValid)
            {
                if (model.IsAdvancedCreate && model.TotalRequiredCount <= 0)
                    return ThrowJsonError($"Work advnaced creation requires complications");

                if (model.Type <= 0)
                    return ThrowJsonError("Please choose task type");

                switch (model.Type)
                {
                    case WorkType.ExpectClockInRecordsBefore:
                        //if(model.MinsBeforeCheckIn <0)
                        //    return ThrowJsonError("Please enter minutes before checkin request");
                        break;
                    case WorkType.RequireSubmissions:
                        if (model.TotalRequiredCount <= 0)
                            return ThrowJsonError("Please enter total submissions required ");
                        break;
                    default:
                        break;
                }


                if (model.LessDeduct <= 0 || model.MoreCredit <= 0)
                    return ThrowJsonError("Sorry! Cannot set 0 values to credit and deduct amounts");
            }


            var work = new Work();

            if(model.Id > 0)
            {
                work = await payrolDbContext.Works.FindAsync(model.Id);

                if (work == null)
                    return ThrowJsonError("Ouch! Item was not found!");
            }


            work.Name = model.Name;
            work.Type = model.Type;
            work.StartTime = model.StartTime;
            work.EndTime = model.EndTime;
            work.LessDeduct = model.LessDeduct;
            work.MoreCredit = model.MoreCredit;
            work.TotalRequiredCount = model.TotalRequiredCount;
            work.MinsBeforeCheckIn = model.MinsBeforeCheckIn;
            work.IsAdvancedCreate = model.Type == WorkType.RequireSubmissions;
            work.HasTime = model.Type == WorkType.ExpectClockInRecordsBefore;

            work.CompanyId = userResolverService.GetCompanyId();
            //work.DepartmentId = await payrolDbContext.Departments.Where(x => x.CompanyId == model.CompanyId)
            //    .Select(x => x.Id).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                if(model.Id > 0)
                        payrolDbContext.Works.Update(work);
                else
                    await payrolDbContext.Works.AddAsync(work);

                await payrolDbContext.SaveChangesAsync();

                return await Tasks();
            }

            return ThrowJsonError();
        }
        

        public async Task<IActionResult> ViewAttendance(int id = 0)
        {
            var emp = await payrolDbContext.Attendances
                .Include(x => x.Employee)
                .Include(x=> x.Requests)
                .Include(x=> x.BiometricRecords)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (emp == null && id != 0) return ThrowJsonError("Attendance was not found");
            if (emp == null) emp = new Attendance { CompanyId = userResolverService.GetCompanyId() };
            return PartialView("_ViewAttendance", emp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">attendance Id</param>
        /// <param name="q"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddBiometricRelate(int id = 0, string q = "")
        {

            var attendance = await payrolDbContext.Attendances.Include(x => x.Employee)
                .Include(x => x.Requests)
                .Include(x => x.BiometricRecords)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (attendance == null && id != 0) return ThrowJsonError("Attendance was not found");
            if (attendance.IsOvertime) return ThrowJsonError("OT requests don't need clock records");

            var _attendance = await payrolDbContext.Attendances
                .Include(x => x.Employee)
                .Include(x => x.BiometricRecords)
                .FirstOrDefaultAsync(x => x.Id == id);
            var timeStartShuift = await payrolDbContext.CompanyWorkTimes.Where(a => a.CompanyId == userResolverService.GetCompanyId())
                .OrderBy(a => a.StartTime)
                .Select(a => a.StartTime).FirstOrDefaultAsync();
            ViewBag.StartDateTime = _attendance.Date.Date.Add(timeStartShuift);
            return PartialView("_AddAttendanceBiometric", _attendance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBiometricRelate(Attendance model, string q = "")
        {

            var attendance = await payrolDbContext.Attendances.Include(x => x.Employee)
                .Include(x => x.Requests)
                .Include(x => x.BiometricRecords)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (attendance == null) return ThrowJsonError("Attendance was not found");


            if (attendance.BiometricRecords.Any())
                payrolDbContext.BiometricRecords.RemoveRange(attendance.BiometricRecords);

            if (model.BiometricRecords.Any())
            {
                var date = attendance.Date;
                payrolDbContext.BiometricRecords.AddRange(model.BiometricRecords.Select((a, i) => new BiometricRecord
                {
                    AttendanceId = attendance.Id,
                    EmployeeId = attendance.EmployeeId,
                    DateTime = attendance.Date.Add(a.Time),
                    Time = a.Time,
                    Date  = attendance.Date,

                    OrderBy = i,
                    Location = a.Location,
                    MachineId = "TECO-LB001|",
                    CompanyId = userResolverService.GetCompanyId(),
                    BiometricRecordType = BiometricRecordType.FingerPrint,
                    BiometricRecordState = a.BiometricRecordState,
                    IsActive = true,
                }).ToList());


                await payrolDbContext.SaveChangesAsync();
                await scheduleService.UpdateAttendanceWorkHours(attendance.Id);
            }
            //{
            //    for (int i = 0; i < attendance.BiometricRecords.Count; i++)
            //        attendance.BiometricRecords.RemoveAt(i);

            //    payrolDbContext.BiometricRecords.RemoveRange(attendance.BiometricRecords);
            //}

            //var loc = new[] { "Lobby Area", "News Room", "Studio Room", "Lift Area" };
            //if (attendance.BiometricRecords.Any() == false && q == "rnd_create")
            //{
            //    var date = attendance.Date;
            //    var rnd = new Random();
            //    var vals = new[] { BiometricRecordState.CheckIn, BiometricRecordState.CheckOut, BiometricRecordState.BreakIn, BiometricRecordState.BreakOut };
            //    var _ = vals.Select((a, i) => new BiometricRecord
            //    {
            //        AttendanceId = attendance.Id,

            //        EmployeeId = attendance.EmployeeId,
            //        DateTime = date,


            //        OrderBy = i,
            //        Location = loc[rnd.Next(0, loc.Length)],
            //        MachineId = "TECO-LB001|",
            //        CompanyId = userResolverService.GetCompanyId(),
            //        BiometricRecordType = BiometricRecordType.FingerPrint,
            //        BiometricRecordState = a,
            //        IsActive = true,
            //    }).ToList();


            //    _.ForEach(r =>
            //    {
            //        switch (r.BiometricRecordState)
            //        {
            //            case BiometricRecordState.CheckIn:
            //                r.DateTime = attendance.WorkStartTime.AddMinutes(rnd.Next(-20, 20)).AddMinutes(rnd.Next(1, 50));
            //                break;
            //            case BiometricRecordState.CheckOut:
            //                r.DateTime = attendance.WorkStartTime.AddHours(rnd.Next(6, 8)).AddMinutes(rnd.Next(1, 50));
            //                break;
            //            case BiometricRecordState.BreakOut:
            //                r.DateTime = attendance.WorkStartTime.AddHours(rnd.Next(2, 3)).AddMinutes(rnd.Next(1, 50));
            //                break;
            //            case BiometricRecordState.BreakIn:
            //                r.DateTime = attendance.WorkStartTime.AddHours(rnd.Next(4, 5)).AddMinutes(rnd.Next(1, 50));
            //                break;
            //            default:
            //                break;
            //        }

            //        r.SetDates();
            //    });

            //    //_.First().Date = attendance.WorkStartTime.AddMinutes(-5);
            //    //_.First().SetDates();

            //    //_.Last().Date = _.First().Date.AddHours(6);
            //    //_.Last().SetDates();

            //    await payrolDbContext.BiometricRecords.AddRangeAsync(_);
            //    await payrolDbContext.SaveChangesAsync();

            //    attendance.BiometricRecords = _.ToList();

            //    await scheduleService.UpdateAttendanceWorkHours(attendance.Id);
            //}


            //var _attendance = await payrolDbContext.Attendances
            //    .Include(x => x.Employee)
            //    .Include(x => x.BiometricRecords)
            //    .FirstOrDefaultAsync(x => x.Id == model.Id);
            //var timeStartShuift = await payrolDbContext.CompanyWorkTimes.Where(a => a.CompanyId == userResolverService.GetCompanyId())
            //    .OrderBy(a => a.StartTime)
            //    .Select(a => a.StartTime).FirstOrDefaultAsync();
            //ViewBag.StartDateTime = _attendance.Date.Date.Add(timeStartShuift);

            return RedirectToAction(nameof(GetEmpDayAttTrStatusClockRecords), new { id = model.EmployeeId, onDate = attendance?.Date });
        }

        public async Task<IActionResult> GetEmpDayAttTrStatusClockRecords(int id, DateTime onDate, string domSelect = "")
        {
            ViewBag.Complete = "on";

            scheduleService.SetBaseDate(onDate);
            var cmpConfig = await companyService.GetCompanyCalendarSettings();
            ViewBag.cmpConfig = cmpConfig;

            ViewBag.WeekStart = scheduleService.thisWeekStart;
            ViewBag.WeekEnd = scheduleService.thisWeekEnd;
            ViewData["calendars"] = Calendars.List;
            ViewData["start"] = onDate;
            ViewData["domSelect"] = domSelect;
            var _empId = id;
            var _endd = onDate.AddDays(1).AddMinutes(-1);
            var vm = new HomeEmployeeVm
            {
                AttedanceSchedule = await scheduleService.GetCurrentSecdule(empId: _empId, start: onDate, end: _endd),
                PublicHolidaysUpcoming = await companyService.GetUpComingPublicHolidays(onDate),
                //Employee = await employeeService.GetCurrentEmployeeWithDepartment(),
                WeekScheduleTasks = await scheduleService.GetSecduledTasksForThisWeek(empId: _empId),
                WorkTimes = await companyService.GetWorkTimes(),
                MyRequests = await requestService.GetLeaveRequests(userResolverService.GetCompanyId(), new int[] { _empId }, onDate, _endd, null, new WorkItemStatus[] { WorkItemStatus.Approved, WorkItemStatus.Draft, WorkItemStatus.Submitted }, 1, int.MaxValue),
                //defa = await companyService.GetWorkStartDayOfWeek
            };

            return PartialView("_EmpDayAttTrStatusClockRecords", vm);
        }

        public async Task<IActionResult> ViewBiometricRelate(int id = 0, string q="")
        {

            var attendance = await payrolDbContext.Attendances.Include(x => x.Employee)
                .Include(x => x.Requests)
                .Include(x=> x.BiometricRecords)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (attendance == null && id != 0) return ThrowJsonError("Attendance was not found");

            //if (attendance.BiometricRecords.Any())
            //{
            //    for (int i = 0; i < attendance.BiometricRecords.Count; i++)
            //        attendance.BiometricRecords.RemoveAt(i);

            //    payrolDbContext.BiometricRecords.RemoveRange(attendance.BiometricRecords);
            //}

            var loc = new[] { "Lobby Area", "News Room", "Studio Room", "Lift Area" };
            if (attendance.BiometricRecords.Any()== false &&  q == "rnd_create")
            {
                var date = attendance.Date;
                var rnd = new Random();
                var vals = new[] { BiometricRecordState.CheckIn, BiometricRecordState.CheckOut, BiometricRecordState.BreakIn, BiometricRecordState.BreakOut };
                var _ = vals.Select((a, i) => new BiometricRecord
                {
                    AttendanceId = attendance.Id,

                    EmployeeId = attendance.EmployeeId,
                    DateTime = date,


                    OrderBy = i,
                    Location = loc[rnd.Next(0, loc.Length)],
                    MachineId = "TECO-LB001|",
                    CompanyId = userResolverService.GetCompanyId(),
                    BiometricRecordType = BiometricRecordType.FingerPrint,
                    BiometricRecordState = a,
                    IsActive= true,
                }).ToList();

                
                _.ForEach(r =>
                {
                    switch (r.BiometricRecordState)
                    {
                        case BiometricRecordState.CheckIn:
                            r.DateTime = attendance.WorkStartTime.AddMinutes(rnd.Next(-20, 20)).AddMinutes(rnd.Next(1, 50));
                            break;
                        case BiometricRecordState.CheckOut:
                            r.DateTime = attendance.WorkStartTime.AddHours(rnd.Next(6,8)).AddMinutes(rnd.Next(1, 50));
                            break;
                        case BiometricRecordState.BreakOut:
                            r.DateTime = attendance.WorkStartTime.AddHours(rnd.Next(2, 3)).AddMinutes(rnd.Next(1, 50));
                            break;
                        case BiometricRecordState.BreakIn:
                            r.DateTime = attendance.WorkStartTime.AddHours(rnd.Next(4, 5)).AddMinutes(rnd.Next(1, 50));
                            break;
                        default:
                            break;
                    }

                    r.SetDates();
                });

                //_.First().Date = attendance.WorkStartTime.AddMinutes(-5);
                //_.First().SetDates();

                //_.Last().Date = _.First().Date.AddHours(6);
                //_.Last().SetDates();

                await payrolDbContext.BiometricRecords.AddRangeAsync(_);
                await payrolDbContext.SaveChangesAsync();

                attendance.BiometricRecords = _.ToList();

                await scheduleService.UpdateAttendanceWorkHours(attendance.Id);
            }


            var _attendance = await payrolDbContext.Attendances
                .Include(x => x.Employee)
                .Include(x => x.BiometricRecords)
                .FirstOrDefaultAsync(x => x.Id == id);
            var timeStartShuift  = await payrolDbContext.CompanyWorkTimes.Where(a => a.CompanyId == userResolverService.GetCompanyId())
                .OrderBy(a => a.StartTime)
                .Select(a=> a.StartTime).FirstOrDefaultAsync();
            ViewBag.StartDateTime = _attendance.Date.Date.Add(timeStartShuift);
            return PartialView("_ViewAttendanceBiometric", _attendance);
        }


        public async Task<IActionResult> AddOrUpdateAttendance(int id = 0)
        {
            var emp = await payrolDbContext.Attendances.Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (emp == null && id != 0) return ThrowJsonError("Attendance was not found");
            if (emp == null) emp = new Attendance { CompanyId = userResolverService.GetCompanyId() };

            var cmpWorkTimesWithDura= await payrolDbContext.CompanyWorkTimes.Where(a => a.CompanyId == userResolverService.GetCompanyId())
                .ToDictionaryAsync(x => x.Id, x => x.ShiftName + " (" + x.Duration + ")"); ;

            ViewBag.ShiftId = new SelectList(cmpWorkTimesWithDura, "Key", "Value", emp.CompanyWorkTimeId);
            return PartialView("_AddOrUpdateAttendance", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateAttendance(Attendance model)
        {
            var attendance = await payrolDbContext.Attendances
                .Include(a=>a.BiometricRecords).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (attendance == null)
                return ThrowJsonError("Attendance record was not found!");

            //if (attendance == null)
            //    return ThrowJsonError("Attendance record was not found!");

            //if (model.WorkStartTime >= model.WorkEndTime)
            //    return ThrowJsonError($"End time cannot be greater than Start time");

            await scheduleService.UpdateAttendanceAsync(model);
        
            return await Index(date: attendance.Date);
        }

        public async Task<IActionResult> RemoveSchedule(int id)
        {
            var scheduleInDb = await payrolDbContext.Schedules.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Id == id)
                 .Include(x => x.Attendances).Include(x => x.WorkItems)
                    .FirstOrDefaultAsync();
            if (scheduleInDb == null)
                return ThrowJsonError("Schedule mwas not found!");

            return PartialView("_RemoveSchedule", scheduleInDb);
        }


        public async Task<IActionResult> AddOrUpdateWorkItem(int id = 0)
        {
            var emp = await payrolDbContext.WorkItems.Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (emp == null && id != 0) return ThrowJsonError("Work Item was not found");
            if (emp == null) emp = new WorkItem { EmployeeId = userResolverService.GetCompanyId() };
            return PartialView("_AddOrUpdateWorkItem", emp);
        }
        
        public async Task<IActionResult> ViewWorkItem(int id, DateTime? date = null)
        {
            var emp = await payrolDbContext.WorkItems
                .Where(x => x.Id == id && (date == null || x.Date.Date == date.Value.Date))
                .Include(x => x.Employee)
                .Include(x => x.WorkItemSubmissions)
                .Include(x => x.Work)
                .Include(x => x.Schedule)
                .Include(x => x.Requests)
                .FirstOrDefaultAsync();

            
            if (emp == null || id == 0) return ThrowJsonError("Work item was not found!");
            return PartialView("_ViewWorkItem", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateWorkItem(WorkItem model)
        {
            var workItem = await payrolDbContext.WorkItems
                .Include(x => x.Employee)
                .Include(x=> x.Work)
                .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (workItem == null)
                return ThrowJsonError("Work Item record was not found!");

            if (model.WorkStartTime > model.WorkEndTime)
                return ThrowJsonError($"End time cannot be greater than Start time");

            if (model.DueDate < model.WorkStartTime && model.DueDate > model.WorkEndTime)
                return ThrowJsonError($"Due date must be betweem start and End date");

            await scheduleService.UpdateWorkItemAsync(model);

            return RedirectToAction(nameof(ViewWorkItem), new { id = model.Id });
        }

        //[HttpPost]
        //public async Task<IActionResult> RemoveScheduledData(int id, bool removeSelf = false)
        //{
        //    string message = "";
        //    if (ModelState.IsValid)
        //    {
        //        var scheduleInDb = await payrolDbContext.Schedules.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Id == id)
        //            .Include(x=> x.Attendances).Include(x=> x.WorkItems)
        //                .FirstOrDefaultAsync();
        //        if (scheduleInDb == null)
        //            return BadRequest("Oooh! we didnt find that one");
        //        switch (scheduleInDb.ScheduleFor)
        //        {
        //            case ScheduleFor.Attendance:
        //                if (scheduleInDb.ScheduleFor == ScheduleFor.Attendance && !scheduleInDb.Attendances.Any() && !removeSelf)
        //                    return ThrowJsonError("Ouch! There are not scheduled attemdamce records to be removed");
        //                payrolDbContext.Attendances.RemoveRange(scheduleInDb.Attendances);
        //                break;
        //            case ScheduleFor.Work:
        //                if (scheduleInDb.ScheduleFor == ScheduleFor.Work && !scheduleInDb.WorkItems.Any() && !removeSelf)
        //                    return ThrowJsonError("Ouch! There are not scheduled work items to be removed");
        //                payrolDbContext.WorkItems.RemoveRange(scheduleInDb.WorkItems);
        //                break;
        //            default:
        //                break;
        //        }
                

        //        if (removeSelf)
        //            payrolDbContext.Schedules.Remove(scheduleInDb);

        //        int changes = payrolDbContext.SaveChanges();

        //        return await Schedules(date: scheduleInDb.Start);
        //    }

        //    return BadRequest();
        //}


        public async Task<IActionResult> SubmitWork(int wiId, int id = 0)
        {
            var workItem = await payrolDbContext.WorkItems
                .Where(x => x.Id == wiId)
                .Include(x => x.Work)
                .FirstOrDefaultAsync();

            if (workItem?.Work == null )
                return ThrowJsonError("Work was not found!");

            WorkItemSubmission model = null;
            if (id > 0)
                model = await payrolDbContext.WorkItemSubmissions.FindAsync(id);

            if (model == null && id > 0)
                return ThrowJsonError("Work Submission was not found!");

            var count = await payrolDbContext.WorkItemSubmissions.CountAsync(x => x.WorkItemId == wiId);
            model = model ?? new WorkItemSubmission
            {
                Name = "Submission " + (count + 1),
                WorkItemId = wiId,
                WorkItem = workItem
            };
            return PartialView("_SubmitWork", model);
        }

        private object GenerateAjaxActionUrl(string action, string @class = "modal__btn modal__btn-back")
        {
            return $@"
            <a class=""{@class}"" data-ajax=""true"" data-ajax-update="".modal__container"" data-ajax-begin=""showModal()"" asp-action=""{action}"" asp-route-id=""@work.Id"">
                <i class=""fa fa-user-shield""></i> Edit
            </a>";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitWork(WorkItemSubmission model)
        {
            WorkItem workItem = null;
            if (ModelState.IsValid)
            {
                workItem = await payrolDbContext.WorkItems
                    .Where(x => x.Id == model.WorkItemId)
                    .Include(x => x.Work)
                    .FirstOrDefaultAsync();

                if (workItem?.Work == null)
                    return ThrowJsonError("Work was not found!");
            }


            if (ModelState.IsValid)
            {

                if (string.IsNullOrWhiteSpace(model.Details) || string.IsNullOrWhiteSpace(model.Name))
                    return ThrowJsonError("Please enter details of work summary");

                if (model.Status != WorkItemStatus.Draft && model.Status != WorkItemStatus.Submitted)
                    return ThrowJsonError("This action doesnt seem to be working");

                if (model.Id == 0 && workItem.Work.IsAdvancedCreate && workItem.RemainingSubmissions <= 0)
                    return ThrowJsonError("Sorry, You have submitted, kindly wait for reply from your supervisor");


                if(model.Status == WorkItemStatus.Submitted)
                {
                    workItem.TotalSubmitted += 1;
                    workItem.RemainingSubmissions -= 1;
                    workItem.PercentSubmitted = workItem.GetSubmittedPercent(workItem.TotalSubmitted);
                    model.SubmissionDate = DateTime.UtcNow;
                }

                if (model.Id > 0)
                    payrolDbContext.WorkItemSubmissions.Update(model);
                else
                {
                    if (workItem.Work.IsAdvancedCreate && workItem.RemainingSubmissions >= 0)
                    {
                        //if(model.Status == WorkItemStatus.Submitted)
                            //workItem.RemainingSubmissions -= 1;

                        await payrolDbContext.WorkItemSubmissions.AddAsync(model);
                    }
                }

                await payrolDbContext.SaveChangesAsync();
                return await ViewWorkItem(model.WorkItemId, workItem.Date);
            }

            return ThrowJsonError(ModelState);
        }


        /// <summary>
        /// Submit single work item
        /// change state from Draft to Submitted
        /// </summary>
        /// <param name="wiId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SubmitSingleWork(int wiId, int id = 0)
        {
            var workItem = await payrolDbContext.WorkItems
                .Where(x => x.Id == wiId)
                .Include(x => x.Work)
                .FirstOrDefaultAsync();

            if (workItem?.Work == null)
                return ThrowJsonError("Work was not found!");

            WorkItemSubmission model = null;
            model = await payrolDbContext.WorkItemSubmissions.FindAsync(id);

            if (model == null)
                return ThrowJsonError("Work Submission was not found!");

            if (model.Status != WorkItemStatus.Draft)
                return ThrowJsonError("Work Submission was found, but not in draft state");

            if (model.Id == 0 && workItem.Work.IsAdvancedCreate && workItem.RemainingSubmissions <= 0)
                return ThrowJsonError("Sorry, You have submitted, kindly wait for reply from your supervisor");



            model.Status = WorkItemStatus.Submitted;

            workItem.TotalSubmitted += 1;
            workItem.RemainingSubmissions -= 1;
            workItem.PercentSubmitted = workItem.GetSubmittedPercent(workItem.TotalSubmitted);

            await payrolDbContext.SaveChangesAsync();

            return await ViewWorkItem(model.WorkItemId, workItem.Date);
        }



        [HttpPost]
        public IActionResult RemoveWork(int id)
        {
            if (ModelState.IsValid)
            {
                var add = payrolDbContext.Works.FirstOrDefault(x => x.Id == id);
                if (add == null)
                    return BadRequest("Oooh! we didnt find that one");
                if (payrolDbContext.WorkItems.Any(x => x.WorkId == id))
                    return BadRequest("Ouch! There are some work items created, kindly remove them first");

                payrolDbContext.Works.Remove(add);
                payrolDbContext.SaveChanges();
                return Ok("Removed");
            }

            return BadRequest();
        }

        [HttpPost]
        public IActionResult RemoveWorkItem(int id)
        {
            if (ModelState.IsValid)
            {
                var add = payrolDbContext.WorkItems.FirstOrDefault(x => x.Id == id);
                if (add == null)
                    return ThrowJsonError("Oooh! we didnt find that one");
                //if (add.IsEmployeeTask == false)
                //    return ThrowJsonError("Ouch! Only employee task cannot be removed");

                payrolDbContext.WorkItems.Remove(add);
                payrolDbContext.SaveChanges();
                return Ok("Removed");
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveWorkItemSubmission(int id)
        {
            if (ModelState.IsValid)
            {
                var workId = 0;
                var add = payrolDbContext.WorkItemSubmissions.FirstOrDefault(x => x.Id == id);
                if (add == null)
                    return ThrowJsonError("Oooh! we didnt find that one");
                if (add.Status != WorkItemStatus.Draft)
                    return ThrowJsonError("Ouch! Cannot remove that one");
                workId = add.WorkItemId;
                //if (add.IsEmployeeTask == false)
                //    return ThrowJsonError("Ouch! Only employee task cannot be removed");

                payrolDbContext.WorkItemSubmissions.Remove(add);

                //var work = await payrolDbContext.WorkItems
                //    .Include(x => x.Work).FirstOrDefaultAsync(x => x.Id == workId);
                //if (work == null)
                //    return ThrowJsonError("Associated work item was not found");
                //work.TotalSubmitted -= 1;
                //work.RemainingSubmissions += 1;
                //work.PercentSubmitted = work.GetSubmittedPercent(work.TotalSubmitted);


                //payrolDbContext.WorkItems.Update(work);
                payrolDbContext.SaveChanges();

                return RedirectToAction(nameof(ViewWorkItem), new { id = workId });
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSchedule(int id, string remove = "")
        {
            if (ModelState.IsValid)
            {
                var schedule = await payrolDbContext.Schedules.Include(x=> x.backgroundJobs)
                    .FirstOrDefaultAsync(x=> x.Id == id);
                if (schedule == null)
                    return BadRequest("Oooh! we didnt find that one");
                //if (payrolDbContext.WorkItems.Any(x => x.WorkId == id))
                //    return BadRequest("Ouch! There are some work items created, kindly remove them first");


                if(remove == "all")
                {
                    var wiItems = await payrolDbContext.WorkItems
                        .Where(x => x.ScheduleId == id)
                        .ToArrayAsync();
                    payrolDbContext.WorkItems.RemoveRange(wiItems);
                    var attens = await payrolDbContext.Attendances
                        .Where(x => x.ScheduleId == id)
                        .ToArrayAsync();
                    payrolDbContext.Attendances.RemoveRange(attens);
                }

                if (remove == "w")
                {
                    var wiItems = await payrolDbContext.WorkItems
                        .Where(x => x.ScheduleId == id)
                        .ToArrayAsync();
                    payrolDbContext.WorkItems.RemoveRange(wiItems);
                }
                else if (remove == "a")
                {
                    var attens = await payrolDbContext.Attendances
                        .Where(x => x.ScheduleId == id)
                        .ToArrayAsync();
                    payrolDbContext.Attendances.RemoveRange(attens);
                }
                else if (remove == "self")
                {
                    schedule.backgroundJobs.ToList().ForEach(t =>
                        backgroundJobClient.Delete(t.HangfireJobId)
                    );

                    payrolDbContext.Schedules.Remove(schedule);
                }
                else
                {
                    return Ok("Removed");
                }

                payrolDbContext.SaveChanges();
                return Ok("Removed");
            }

            return BadRequest();
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            if (ModelState.IsValid)
            {
                var add = payrolDbContext.Attendances.FirstOrDefault(x => x.Id == id);
                if (add == null)
                    return BadRequest("Oooh! we didnt find that one");
                //if (payrolDbContext.Departments.Any(x => x.CompanyId == id))
                //    return BadRequest("Ouch! There are some departments in this company, kindly remove them first");

                payrolDbContext.Attendances.Remove(add);
                payrolDbContext.SaveChanges();
                return Ok("Removed");
            }

            return BadRequest();
        }
    }
}

