using Microsoft.AspNetCore.Identity;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payroll.ViewModels;
using Payroll.Database;
using Microsoft.EntityFrameworkCore;

namespace Payroll.Services
{
    public class PayrollService
    {
        private readonly PayrollDbContext context;
        private readonly UserResolverService userResolverService;
        private readonly EmployeeService employeeService;
        private readonly CompanyService companyService;
        private string computedstring;

        public PayrollService(PayrollDbContext context, UserResolverService userResolverService, EmployeeService employeeService, CompanyService companyService)
        {
            this.context = context;
            this.userResolverService = userResolverService;
            this.employeeService = employeeService;
            this.companyService = companyService;
        }

        public string GetCalculation() => computedstring;

        public async Task<PayrollPeriodEmployee> GetLastPayStub(int id)
        {
            return await context.PayrollPeriodEmployees.Where(a => a.EmployeeId == id)
                .Include(a => a.PayrollPeriod)
                .LastOrDefaultAsync();
        }


        public async Task<List<PayrollPeriodEmployee>> GetPayStubsByYear(int id, int year)
        {
            return await context.PayrollPeriodEmployees.Where(a => a.EmployeeId == id && a.PayrollPeriod.StartDate.Year == year)
                .OrderByDescending(a => a.PayrollPeriod.StartDate)
                .Include(a => a.PayrollPeriod)
                .ToListAsync();
        }

        public async Task<List<PayrollPeriodEmployee>> GetPayStubsByDuration(int id, DateTime start, DateTime end)
        {
            return await context.PayrollPeriodEmployees.Where(a => a.EmployeeId == id && a.PayrollPeriod.StartDate >= start && a.PayrollPeriod.StartDate <= end)
                .OrderByDescending(a => a.PayrollPeriod.StartDate)
                .Include(a => a.PayrollPeriod)
                .ToListAsync();
        }

        public async Task<List<VariationKeyValue>> GetPayStubVariants(int id, int payrolPeriodEmployeeId)
        {
            return await context.VariationKeyValues.Where(a => a.PayrollPeriodEmployeeId == payrolPeriodEmployeeId && a.PayrollPeriodEmployee.EmployeeId == id)
                //.Include(a => a.PayrollPeriodEmployee)
                .ToListAsync();
        }



        public async Task<List<EmployeeRecord>> GetPayPeriodCalendarAsync(int id, int page = 1, int limit = 10, bool showUnScheduled = false, bool showTasks = false, EmployeeSelectorVm selectorVm = null)
        {

            var payrol = await context.PayrollPeriods
                .Include(x => x.PayrollPeriodEmployees)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (payrol == null)
                throw new Exception("ss");


            var allEmpsiInSchedule = await context.Employees.Where(x => x.Department.CompanyId == userResolverService.GetCompanyId()
            && (selectorVm == null || selectorVm.EmployeeIds.Contains(x.Id)))
            .OrderBy(x => x.Department.DisplayOrder).ThenBy(x => x.EmpID)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Include(x => x.Department)
            .ToListAsync();

            var allEmpsiInScheduleIs = allEmpsiInSchedule.Select(x => x.Id).ToList();
            var attendances = await context.Attendances.Where(x => x.Date.Date >= payrol.StartDate && x.Date.Date <= payrol.EndDate.Date && x.IsActive && x.CurrentStatus != AttendanceStatus.Created
              && allEmpsiInScheduleIs.Contains(x.EmployeeId))
                .Include(x => x.Employee)
                .ThenInclude(x => x.Department)
                .ToListAsync();

            var empDayOffs = await context.DayOffEmployeeItems.Where(x => x.Start.Date >= payrol.StartDate && x.Start.Date <= payrol.EndDate.Date && x.IsActive
            && allEmpsiInScheduleIs.Contains(x.DayOffEmployee.EmployeeId) && x.Status == DayOffEmployeeItemStatus.Approved)
                .Include(x => x.DayOffEmployee)
                    .ThenInclude(x => x.DayOff)
                .ToListAsync();


            var empworkItems = await context.WorkItems.Where(x => x.DueDate.Value.Date >= payrol.StartDate.Date && x.DueDate.Value.Date <= payrol.EndDate.Date && x.IsActive && x.IsCompleted && !x.IsEmployeeTask
            && allEmpsiInScheduleIs.Contains(x.EmployeeId))
                .Include(x => x.WorkItemSubmissions)
                .Include(x => x.Work)
                .ToListAsync();


            var list = new List<EmployeeRecord>();
            foreach (var emp in allEmpsiInSchedule)
            {
                var _ = new EmployeeRecord { Employee = emp };
                for (DateTime start = payrol.StartDate; start < payrol.EndDate; start = start.AddDays(1))
                {
                    if (attendances.Any(x => x.Date.Date == start.Date.Date && x.EmployeeId == emp.Id))
                        _.Attendances.AddRange(attendances.Where(x => x.Date.Date == start.Date.Date && x.EmployeeId == emp.Id).ToList());

                    if (empworkItems.Any(x => x.DueDate.Value.Date == start.Date.Date && x.EmployeeId == emp.Id))
                        _.WorkItems.AddRange(empworkItems.Where(x => x.DueDate.Value.Date == start.Date.Date && x.EmployeeId == emp.Id).ToList());
                }


                if (empDayOffs.Any(x => x.DayOffEmployee.EmployeeId == emp.Id))
                    foreach (var item in empDayOffs.Where(x => x.DayOffEmployee.EmployeeId == emp.Id))
                    {
                        if (item.TotalDays > 1)
                            _.DayOffEmployees.AddRange(Enumerable.Range(0, 1 + item.End.Subtract(item.Start).Days)
                                  .Select(offset => new DayVm { Date = item.Start.AddDays(offset).Date, Color = item.Id.ToString(), DayOffId = item.DayOffEmployee.DayOffId, DayOffName= item.DayOffEmployee.DayOff.Name, CreatedFromRequestId = item.CreatedFromRequestId })
                                  .ToArray());
                        else
                            _.DayOffEmployees.Add(new DayVm { Date = item.Start, Color = item.Id.ToString(), DayOffId = item.DayOffEmployee.DayOffId, DayOffName = item.DayOffEmployee.DayOff.Name, CreatedFromRequestId = item.CreatedFromRequestId });
                    }

                list.Add(_);
            }


            return list;

            ////var weekStartDate = thisWeekStart
            //var allEmpsiInSchedule = await context.Attendances.Where(x => x.Date >= thisWeekStart && x.Date <= thisWeekEnd && x.IsActive &&
            //(empId == 0 || empId == x.EmployeeId))
            //    .Select(x => x.Employee)
            //    .Distinct()
            //    .OrderBy(x => x.Department.DisplayOrder).ThenBy(x => x.EmpID)
            //    .Skip((page - 1) * limit)
            //    .Take(limit)
            //    .Include(x => x.Department)
            //    .ToListAsync();


            //var allEmpsiInScheduleIs = allEmpsiInSchedule.Select(x => x.Id).ToList();


            //if (showUnScheduled)
            //    allEmpsiInSchedule = await context.Employees.Where(x => x.Department.CompanyId == userResolverService.GetCompanyId()
            //        && !allEmpsiInScheduleIs.Contains(x.Id))
            //    .OrderBy(x => x.Department.DisplayOrder).ThenBy(x => x.EmpID)
            //    .Skip((page - 1) * limit)
            //    .Take(limit)
            //    .Include(x => x.Department)
            //    .ToListAsync();

            //var attendances = await context.Attendances.Where(x => x.Date >= thisWeekStart && x.Date <= thisWeekEnd && allEmpsiInScheduleIs.Contains(x.EmployeeId) && x.IsActive)
            //    .Include(x => x.Employee)
            //    .ThenInclude(x => x.Department)
            //    .OrderBy(x => x.Employee.Department.DisplayOrder).ThenBy(x => x.Employee.EmpID)
            //    .ThenBy(x => x.Date)
            //    .ToListAsync();


            //var list = new List<WeeklyEmployeeShiftVm>();
            //foreach (var emp in allEmpsiInSchedule)
            //{
            //    var _ = new WeeklyEmployeeShiftVm { Employee = emp };
            //    for (DateTime start = thisWeekStart; start < thisWeekEnd; start = start.AddDays(1))
            //    {
            //        if (attendances.Any(x => x.Date.Date == start.Date.Date && x.EmployeeId == emp.Id))
            //            _.Attendances.AddRange(attendances.Where(x => x.Date.Date == start.Date.Date && x.EmployeeId == emp.Id).ToList());
            //    }

            //    list.Add(_);
            //}


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
        }

        public async Task<List<KeyValuePair<DateTime, string>>> GetcompanyAndPublicHolidaysCount(PayrollPeriod payrollPeriod)
        {
            var dd = new List<KeyValuePair<DateTime, string>>();
            // ignore public holidays
            var totalPublic = await context.CompanyPublicHolidays.Where(a => a.Id == payrollPeriod.CompanyId && a.Date >= payrollPeriod.StartDate && a.Date <= payrollPeriod.EndDate).Select(t=> new { t.Date, t.Name }).ToListAsync();

            if (totalPublic != null && totalPublic.Any())
                dd.AddRange(totalPublic.Select(t => new KeyValuePair<DateTime, string>(t.Date, t.Name)).ToList());

            var cc = await companyService.GetCompanyCalendarSettings();

            // company public holidays
            var weekends = cc.DayOfWeekHolidays;
            int weekendCount = 0;
            if (weekends != null)
            {
                for (DateTime dt = payrollPeriod.StartDate.Date; dt <= payrollPeriod.EndDate; dt = dt.AddDays(1.0))
                    if (weekends.Contains((int)dt.DayOfWeek))
                        dd.Add(new KeyValuePair<DateTime, string>(dt, "Off Day"));
                //weekendCount++;

                //totalPublic += weekendCount;
            }

            return dd;
        }

        //private void SetPeriodStartEndDays(PayrollPeriod payrol)
        //{
        //    if(payrol.EndDate)
        //}

        public async Task RunPayrollAsync(int payrolPeriodId)
        {
            PayrollPeriod payrolPeriod = context.PayrollPeriods.Find(payrolPeriodId);
            context.Entry(payrolPeriod).Collection(e => e.PayrollPeriodPayAdjustments)
                .Query().OfType<PayrollPeriodPayAdjustment>()
                .Include(x => x.PayAdjustment)
                .Include(x => x.PayrollPeriodPayAdjustmentFieldValues)
                .Load();

            // run validate *
            // bool validationResult = payAdjustmentService.Validate(payrolPeriod.PayrollPeriodPayAdjustments.First())

            // update all field values to payPeriodAdjustment.Total
            // take VALUE *returning
            payrolPeriod.PayrollPeriodPayAdjustments.ForEach(adj =>
            {
                adj.Total = 0;
                try
                {
                    adj.Total = adj.PayrollPeriodPayAdjustmentFieldValues
                                .Where(x => x.IsReturn).Sum(x => decimal.Parse(x.Value));
                }
                catch (ArgumentNullException ex)
                {
                }
            });

            // get all adjustemnts lsit (ordered)
            var paydjs = payrolPeriod.PayrollPeriodPayAdjustments.Select(x => x.PayAdjustment).Distinct()
                .OrderByDescending(x => x.CalculationOrder > 0)
                .ThenBy(x => x.CalculationOrder)
                .ToList();


            var allEmployees = await employeeService.GetAllEmployeesInMyCompanyForPayroll(false, payrolPeriod.StartDate, payrolPeriod.EndDate);

            var listPayrolEmplee = new List<PayrollPeriodEmployee>();
            PayrollPeriodEmployee _payEmployeeItem = null;
            foreach (var emp in allEmployees)
            {
                // create employer model
                _payEmployeeItem = new PayrollPeriodEmployee
                {
                    EmployeeId = emp.Id,
                    EmpID = emp.EmpID,
                    Name = emp.GetSystemName(userResolverService.GetClaimsPrincipal()),
                    Designation = emp.JobTitle,
                    //BasicSalary = emp.BasicSalary,
                    PayrollPeriodId = payrolPeriod.Id,
                };

                // add variation key values tp employer records
                foreach (var adj in paydjs)
                {
                    _payEmployeeItem.VariationKeyValues.Add(new VariationKeyValue
                    {
                        KeyId = adj.Id,
                        Value = payrolPeriod.PayrollPeriodPayAdjustments.Where(x=> x.EmployeeId == emp.Id && x.PayAdjustmentId == adj.Id).Sum(s => s.Total),
                        Key = adj.Name,
                        MultiOrder = adj.CalculationOrder,
                        Type = adj.VariationType,
                    });
                }


                // calculate Gross and Net Pay
                _payEmployeeItem.GrossPay = _payEmployeeItem.BasicSalary + payrolPeriod.PayrollPeriodPayAdjustments
                    .Where(x => x.EmployeeId == emp.Id && (x.VariationType == VariationType.VariableAddition || x.VariationType == VariationType.ConstantAddition))
                    .OrderBy(x => x.PayAdjustment?.CalculationOrder)
                    .Sum(x => x.Total);

                _payEmployeeItem.NetSalary = _payEmployeeItem.GrossPay - payrolPeriod.PayrollPeriodPayAdjustments
                    .Where(x => x.EmployeeId == emp.Id && (x.VariationType == VariationType.VariableDeduction || x.VariationType == VariationType.ConstantDeduction))
                    .OrderByDescending(x => x.CalculationOrder > 0)
                    .ThenBy(x => x.CalculationOrder) // order by calcuation start:1
                    .Sum(x => x.Total);

                // running saving performance report
                // _payEmployeeItem

                //payrollService.RunPayroll(ref _payEmployeeItem, paypEriods);

                listPayrolEmplee.Add(_payEmployeeItem);
                _payEmployeeItem = null;
            }

            payrolPeriod.NetSalary = listPayrolEmplee.Sum(x => x.NetSalary);
            payrolPeriod.GrossPay = listPayrolEmplee.Sum(x => x.GrossPay);
            //payrolPeriod.GrossPayLastPeriod = GetLastPayrolPeriodGrossPay(payrolPeriodId)? listPayrolEmplee.Sum(x => x.GrossPay);
            //payrolPeriod.NetSalaryLastPeriod = listPayrolEmplee.Sum(x => x.NetSalary);

            context.PayrollPeriodEmployees.AddRange(listPayrolEmplee);
            context.SaveChanges();
        }

        //private bool GetLastPayrolPeriodGrossPay(int payrolPeriodId)
        //{

        //}

        public void RunPayroll(ref PayrollPeriodEmployee _payEmployeeItem, PayrollPeriod payrolPeriod)
        {
            // run validate *
            // bool validationResult = payAdjustmentService.Validate(payrolPeriod.PayrollPeriodPayAdjustments.First())

            // update all field values to payPeriodAdjustment.Total
            // take VALUE *returning
            payrolPeriod.PayrollPeriodPayAdjustments.ForEach(adj =>
            {
                adj.Total = 0;
                try
                {
                    adj.Total = adj.PayrollPeriodPayAdjustmentFieldValues
                                .Where(x => x.IsReturn).Sum(x => decimal.Parse(x.Value));
                }
                catch (ArgumentNullException ex)
                {
                }
            });



            var empId = _payEmployeeItem.EmployeeId;

            // calculate Gross and Net Pay
            _payEmployeeItem.GrossPay = _payEmployeeItem.BasicSalary + payrolPeriod.PayrollPeriodPayAdjustments
                .Where(x => x.EmployeeId == empId && (x.VariationType == VariationType.VariableAddition || x.VariationType == VariationType.ConstantAddition))
                .OrderBy(x => x.PayAdjustment?.CalculationOrder)
                .Sum(x => x.Total);

            _payEmployeeItem.NetSalary = _payEmployeeItem.GrossPay - payrolPeriod.PayrollPeriodPayAdjustments
                .Where(x => x.EmployeeId == empId && (x.VariationType == VariationType.VariableDeduction || x.VariationType == VariationType.ConstantDeduction))
                .OrderByDescending(x => x.CalculationOrder > 0)
                .ThenBy(x => x.CalculationOrder) // order by calcuation start:1
                .Sum(x => x.Total);


            // add varition key values (pay adjustments) 
            // in order ^@@
            var allpayPeriodAdjustments = payrolPeriod.PayrollPeriodPayAdjustments
                .Where(x => x.EmployeeId == empId).ToList();
            allpayPeriodAdjustments.ForEach(t => 
                t.PayrollPeriodPayAdjustmentFieldValues = 
                    t.PayrollPeriodPayAdjustmentFieldValues.OrderBy(x => x.CalculationOrder).ToList());

            foreach (var item in payrolPeriod.PayrollPeriodPayAdjustments.Where(x => x.EmployeeId == empId))
            {
            }
            _payEmployeeItem.VariationKeyValues.AddRange(
                allpayPeriodAdjustments
                .GroupBy(x => new { x.Adjustment, x.PayAdjustmentId, x.VariationType, x.CalculationOrder })
                .Select(x => new VariationKeyValue
                {
                    KeyId = x.Key.PayAdjustmentId,
                    Value = x.Sum(s => s.Total),
                    Key = x.Key.Adjustment,
                    MultiOrder = x.Key.CalculationOrder,
                    Type = x.Key.VariationType,
                }));

            // add varition key values (deduction)  ^^ MERGED WITH... ^^
            //_payEmployeeItem.VariationKeyValues.AddRange(
            //    payrolPeriod.PayrollPeriodDeductions
            //    .Where(x => x.EmployeeId == emp.Id)
            //    .OrderBy(x => x.CalculationOrder)
            //    .GroupBy(x => new { x.Deduction, x.DeductionId, x.CalculationOrder })
            //    .Select(x => new VariationKeyValue
            //    {
            //        KeyId = x.Key.DeductionId,
            //        Value = x.Sum(z => z.Total),
            //        Key = x.Key.Deduction,
            //        MultiOrder = x.Key.CalculationOrder,
            //        Type = VariationType.Deduction
            //    }));


        }


        public async Task<IdentityResult> RunPerformanceAsync(int payrollId, string name)
        {
            var payrollPeriod = await context.PayrollPeriods.FindAsync(payrollId);
            if (payrollPeriod == null)
                return IdentityResult.Failed(new IdentityError { Description = "Payroll not found" });

            var payrolPeriodEmpl = await context.PayrollPeriodEmployees.Where(x => x.PayrollPeriodId == payrollId).ToListAsync();

            //var allEmployees = await employeeService.GetAllEmployeesInMyCompanyForPayroll(false, payrollPeriod.StartDate, payrollPeriod.EndDate);
            var _empIdsEligible = payrolPeriodEmpl.Select(a => a.EmployeeId).ToArray();

            var interactions = await GetEmployeePayPeriodInteractionsAsync(payrollId, page: 1, limit: int.MaxValue);

            var isKpiConfigured = await companyService.IsCompanyKpiConfigured(payrollPeriod.CompanyId);
            var cmpKpiConfig = isKpiConfigured ? await companyService.GetCompanyKpiConfig(payrollPeriod.CompanyId) : null;

            var chartdata = await GetEmployeeInteraction2DByDateBetweenPeriod(payrollPeriod.CompanyId, payrollPeriod.StartDate, payrollPeriod.EndDate, _empIdsEligible);


            payrolPeriodEmpl.ForEach(pe =>
            {
                var _ = interactions.FirstOrDefault(i => i.EmployeeId == pe.EmployeeId);
                if (_ != null)
                {
                    // fact data (2D)
                    pe.AbsentDays = _.AbsentDays;
                    pe.LateDays = _.LateDays;
                    pe.LateHours = _.LateHours;
                    pe.LateMins = _.LateMins;
                    pe.LeaveDays = _.LeaveDays;
                    pe.OvertimeCount = _.OvertimeCount;
                    pe.OvertimeHours = _.OvertimeHours;
                    pe.OvertimeMins = _.OvertimeMins;
                    pe.TaskCompletedCount = _.TaskCompletedCount;
                    pe.TaskCompletedCount = _.TaskCompletedCount;
                    pe.TaskDebitSum = _.TaskDebitSum;
                    pe.TaskFailedCount = _.TaskFailedCount;
                    pe.TaskRemainingCount = _.TaskRemainingCount;
                    pe.TaskSubmissionsCount = _.TaskSubmissionsCount;
                    pe.WorkedHours = _.WorkedHours;
                    pe.WorkedMins = _.WorkedMins;
                    pe.WorkedRecordsCount = _.WorkedRecordsCount;

                }

                // chart data
                if (chartdata.Any(c => c.EmployeeId == pe.EmployeeId))
                {
                    pe.ChartDataX = chartdata
                    .Where(c => c.EmployeeId == pe.EmployeeId)
                    .Select(c => new ChartDataX
                    {
                        EmployeeId = c.EmployeeId,
                        PayrollPeriodEmployeeId = pe.Id,
                        Date = c.Date,
                        DateString = c.DateString,
                        ActualWorkedHours = c.ActualWorkedHours,
                        ActualWorkedHoursPerSchedule = c.ActualWorkedHoursPerSchedule,
                        LateEmployeeCount = c.LateEmployeeCount,
                        OvertimeWorkedHoursPerSchedule = c.OvertimeWorkedHoursPerSchedule,
                        TotalAbsentCount = c.TotalAbsentCount,
                        TotalLateDays = c.TotalLateDays,
                        TotalLateHours = c.TotalLateHours,
                        TotalLateMins = c.TotalLateMins,
                        TotalScheduledHours = c.TotalScheduledHours,
                    }).ToList();
                }

                // KPI data
                if (isKpiConfigured)
                {
                    if (pe.KpiValues.Any())
                        pe.KpiValues.ForEach(a => a.IsActive = false);

                    var _newKpiVal = new List<KpiValue>();
                    foreach (var config in cmpKpiConfig)
                    {
                        if(_.GetType().GetProperties().Any(a=> a.Name == config.Kpi))
                        {
                            var actual = _.GetType().GetProperty(config.Kpi).GetValue(_, null)?.ToString();
                            int _actual = 0;
                            int.TryParse(actual, out _actual);

                            // pe.KpiValues.Add(new KpiValue("Absent Days (*3 days)", 0, 3, absentDays, 0, 0.35m, 0, "Absent Days"))
                            _newKpiVal.Add(new KpiValue(
                                config.DisplayName,
                                config.Best,
                                config.Worst,
                                _actual,
                                0,
                                config.Weight,
                                0,
                                config.DisplayName));


                        }
                    }

                    if (_newKpiVal.Any())
                    {

                        employeeService.CalculateKpiPercentAndScore(_newKpiVal);

                        var totalWeightage = _newKpiVal.Sum(x => x.Score);

                        var gade = "";
                        if (totalWeightage >= 80)
                            gade = "A";
                        else if (totalWeightage >= 70)
                            gade = "B";
                        else if (totalWeightage >= 60)
                            gade = "C";
                        else if (totalWeightage >= 50)
                            gade = "D";
                        else if (totalWeightage >= 30)
                            gade = "E";
                        else
                            gade = "F";

                        pe.Grade = gade;
                        //pe.GradeCss = 
                        pe.Percent = totalWeightage;
                        pe.PercentStr = $"{totalWeightage.ToString("N0")}%";
                        pe.GradeGeneratedDateTime = DateTime.UtcNow;
                        pe.IsGraded = true;

                        _newKpiVal.ForEach(a =>
                        {
                            a.PayrollPeriodEmployeeId = pe.Id;
                            a.EmployeeId = pe.EmployeeId;
                        });
                        pe.KpiValues.AddRange(_newKpiVal);

                        _newKpiVal = null;
                    }

                }
            });

            context.PayrollPeriodEmployees.UpdateRange(payrolPeriodEmpl);
            await context.SaveChangesAsync();

            //var start = payrollPeriod.StartDate.Date;
            //var end = payrollPeriod.EndDate.Date;
            //var totalDays = (end - start).TotalDays;
            //var chartdata = await context.Attendances
            //    .Where(x => x.Date >= start && x.Date <= end && x.CompanyId == userResolverService.GetCompanyId())
            //    .GroupBy(x => new { x.Date })
            //    .Select(x => new
            //    {
            //        Date = x.Key,
            //        DateString = x.Key.Date.ToString("yyyy-MM-dd"),
            //        TotalScheduledHours = x.Sum(a => (a.WorkEndTime - a.WorkStartTime).TotalHours),
            //        ActualWorkedHours = x.Sum(a => a.TotalWorkedHours),
            //        ActualWorkedHoursPerSchedule = x.Sum(a => a.TotalHoursWorkedPerSchedule),
            //        TotalScheduledEmployees = x.Count(),
            //        TotalAbsentEmployeeCount = x.Count(a => a.CurrentStatus == AttendanceStatus.Absent),
            //        LateEmployeeCount = x.Count(a => a.CurrentStatus == AttendanceStatus.Late),
            //        TotalLateHours = x.Sum(a => (int)(a.TotalLateMins / 60)),
            //    }).ToListAsync();



            return IdentityResult.Success;
        }


        private string GetComputedString(PayAdjustmentFieldConfig model)
        {
            var str = "";

            str = GetKeyValuePair(nameof(model.BaseType), model.BaseType.ToString());
            switch (model.BaseType)
            {
                case BaseType.ComputedList:
                    switch (model.ListType)
                    {
                        case ListType.None:
                            break;
                        case ListType.Employee:
                        case ListType.Department:
                            str = GetKeyValuePair(nameof(model.ListType), model.ListType.ToString());

                            str += GetKeyValuePair(nameof(model.ListSelect), model.ListSelect);
                            break;
                        //case ListType.Additions:
                        //    break;
                        //case ListType.Deductions:
                        //    break;
                        default:
                            break;
                    }
                    break;
                case BaseType.Calculated:
                    str = model.Calculation;
                    break;
                case BaseType.ManualEntry:
                    str = GetKeyValuePair(nameof(model.FieldType), model.FieldType.ToString());
                    break;
                default:
                    break;
            }

            return str;
        }

        private string GetKeyValuePair(string v, string d)
        {
            // if (v == d) return "";
            return $"{v}={d};";
        }


        /// <summary>
        /// Calculate and return interactions for active data (start,end,company) in param
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="showUnScheduled"></param>
        /// <param name="showTasks"></param>
        /// <param name="empId"></param>
        /// <returns></returns>
        public async Task<List<EmployeeInteractionAgg>> GetEmployeeActivePeriodInteractionsAsync(int companyId, DateTime startDate, DateTime endDate, int page = 1, int limit = 10, bool showUnScheduled = false, bool showTasks = false, int? empId = null)
        {

            var allEmpsiInSchedule = empId.HasValue ? new List<Employee> { new Employee { Id = empId.Value } } :
                await employeeService.GetAllEmployeesInMyCompanyForPayroll(false, startDate, endDate, companyId);

            return await GetEmployeeInteractionsAggregateData(companyId, startDate, endDate, allEmpsiInSchedule);
        }


        public async Task<PayrollPeriodEmployee> GetEmployeeInteractionFacts(int companyId, DateTime startDate, DateTime endDate, int empId)
        {
            var emplData = await context.Employees.FindAsync(empId);
            var data = await GetEmployeeInteractionsAggregateData(companyId, startDate, endDate, new List<Employee> { emplData });

            if (data != null && data.Any())
                return new PayrollPeriodEmployee
                {
                    PayrollPeriod = new PayrollPeriod
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        Status = PayrollStatus.OnGoing
                    }
                };


            var x = data.Select(_ => new PayrollPeriodEmployee
            {
                PayrollPeriod = new PayrollPeriod
                {
                    StartDate= startDate,
                    EndDate = endDate,
                    Status = PayrollStatus.OnGoing
                },
                Employee = emplData,
                EmployeeId = emplData.Id,
                AbsentDays = _.AbsentDays,
                LateDays = _.LateDays,
                LateHours = _.LateHours,
                LateMins = _.LateMins,
                LeaveDays = _.LeaveDays,
                OvertimeCount = _.OvertimeCount,
                OvertimeHours = _.OvertimeHours,
                OvertimeMins = _.OvertimeMins,
                TaskCompletedCount = _.TaskCompletedCount,
                //TaskCompletedCount = _.TaskCompletedCount,
                TaskDebitSum = _.TaskDebitSum,
                TaskFailedCount = _.TaskFailedCount,
                TaskRemainingCount = _.TaskRemainingCount,
                TaskSubmissionsCount = _.TaskSubmissionsCount,
                WorkedHours = _.WorkedHours,
                WorkedMins = _.WorkedMins,
                WorkedRecordsCount = _.WorkedRecordsCount,
            }).FirstOrDefault();

            x.NetSalary = await context.PayrollPeriodEmployees.Where(a => a.PayrollPeriod.CompanyId == companyId && a.EmployeeId == empId)
                .Select(a => a.NetSalary).AverageAsync();
            return x;
        }


        /// <summary>
        /// Calculate and return interactions for data between Payroll Period dates
        /// </summary>
        /// <param name="payrollId"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="showUnScheduled"></param>
        /// <param name="showTasks"></param>
        /// <param name="empId"></param>
        /// <returns></returns>
        public async Task<List<EmployeeInteractionAgg>> GetEmployeePayPeriodInteractionsAsync(int payrollId, int page = 1, int limit = 10, bool showUnScheduled = false, bool showTasks = false)
        {
            var payrol = await context.PayrollPeriods
                .Include(x => x.PayrollPeriodEmployees)
                .FirstOrDefaultAsync(x => x.Id == payrollId);
            if (payrol == null)
                return new List<EmployeeInteractionAgg>();


            var allEmpsiInSchedule = await employeeService.GetAllEmployeesInMyCompanyForPayroll(false, payrol.StartDate, payrol.EndDate, payrol.CompanyId);

            //    await context.Employees.Where(x => x.Department.CompanyId == userResolverService.GetCompanyId()
            //&& (empId == null || empId == x.Id))
            //.OrderBy(x => x.Department.DisplayOrder).ThenBy(x => x.EmpID)
            //.Skip((page - 1) * limit)
            //.Take(limit)
            //.Include(x => x.Department)
            //.ToListAsync();
            return await GetEmployeeInteractionsAggregateData(payrol.CompanyId, payrol.StartDate, payrol.EndDate, allEmpsiInSchedule);
        }


        public async Task<List<EmployeeInteractionAgg>> GetEmployeeInteractionsAggregateData(int companyId, DateTime startDate, DateTime endDate, List<Employee> allEmpsiInSchedule)
        {
            var _attendances = await context.Attendances.Where(a => a.Date.Date >= startDate && a.Date.Date <= endDate.Date && a.IsActive).ToListAsync();
            var _dayoffds = await context.DayOffEmployees
                .Where(d => d.DayOffEmployeeItems.Any(a => a.Start.Date <= endDate.Date && a.IsActive))
                .Include(t => t.DayOffEmployeeItems).ToListAsync();
            var _wrokItems = await context.WorkItems.Where(a => a.DueDate.Value.Date >= startDate && a.DueDate.Value.Date <= endDate.Date && a.IsActive).ToListAsync();
            var allEmpsiInScheduleIs = allEmpsiInSchedule.Select(x => x.Id).ToList();
            var _xx = await context.Employees
                .Where(x => allEmpsiInScheduleIs.Contains(x.Id) && x.Department.CompanyId == companyId)
                .Select(x => new EmployeeInteractionAgg
                {
                    EmployeeId = x.Id
                }).ToListAsync();

            var xx = _xx
                .Select(x => new EmployeeInteractionAgg
                {
                    EmployeeId = x.EmployeeId,
                    WorkedRecordsCount = _attendances.Count(a => a.EmployeeId == x.EmployeeId && a.HasClockRecords && a.IsOvertime == false && a.CurrentStatus != AttendanceStatus.Created),

                    WorkedHours = _attendances.Where(a => a.EmployeeId == x.EmployeeId && a.HasClockRecords && a.IsOvertime == false && a.CurrentStatus != AttendanceStatus.Created).Sum(a => a.TotalHoursWorkedPerSchedule),

                    AbsentDays = _attendances.Count(a => a.EmployeeId == x.EmployeeId && a.CurrentStatus == AttendanceStatus.Absent),

                    LateMins = _attendances.Where(a => a.EmployeeId == x.EmployeeId && a.HasClockRecords && a.CurrentStatus == AttendanceStatus.Late).Sum(a => a.TotalLateMins),

                    LateDays = _attendances.Count(a => a.EmployeeId == x.EmployeeId && a.HasClockRecords && a.CurrentStatus == AttendanceStatus.Late),

                    OvertimeCount = _attendances.Count(a => a.EmployeeId == x.EmployeeId && a.IsOvertime == true),

                    OvertimeHours = _attendances.Where(a => a.EmployeeId == x.EmployeeId && a.IsOvertime == true).Sum(a => a.TotalHoursWorkedPerSchedule),

                    LeaveDays = _dayoffds
                    .Where(d => d.EmployeeId == x.EmployeeId)
                    .Sum(d => d.DayOffEmployeeItems.Where(a => a.Start.Date <= endDate.Date && a.IsActive && a.Status == DayOffEmployeeItemStatus.Approved).Sum(a => a.TotalDays)),


                    TaskCompletedCount = _wrokItems.Count(a => a.EmployeeId == x.EmployeeId && a.IsActive && a.Status == WorkItemStatus.Completed && a.IsCompleted && !a.IsEmployeeTask),

                    TaskSubmissionsCount = _wrokItems.Where(a => a.EmployeeId == x.EmployeeId && a.IsActive && a.Status == WorkItemStatus.Completed && a.IsCompleted && !a.IsEmployeeTask && a.Work.Type == WorkType.RequireSubmissions).Sum(a => a.TotalApproved),

                    TaskFailedCount = _wrokItems.Count(a => a.EmployeeId == x.EmployeeId && a.IsActive && a.Status == WorkItemStatus.FailedWithDeduction && a.IsCompleted && !a.IsEmployeeTask),

                    TaskRemainingCount = _wrokItems.Where(a => a.EmployeeId == x.EmployeeId && a.IsActive && a.Status == WorkItemStatus.FailedWithDeduction && a.IsCompleted && !a.IsEmployeeTask && a.Work.Type == WorkType.RequireSubmissions).Sum(a => a.RemainingSubmissions),

                    //TaskAggByWorkIds = _wrokItems.Where(a => a.EmployeeId == x.Id && a.IsActive && a.IsCompleted && !a.IsEmployeeTask).GroupBy(a => a.WorkId).Select(a => new TaskAggByWorkId
                    //{
                    //    WorkId = a.Key.Value,
                    //    TaskCompletedCount = a.Count(b => b.Status == WorkItemStatus.Completed),
                    //    TaskFailedCount = a.Count(b => b.Status == WorkItemStatus.FailedWithDeduction),
                    //    TaskCreditSum = a.Where(b => b.Status == WorkItemStatus.Completed).Sum(b => b.TotalAmountCredited),
                    //    TaskDebitSum = a.Where(b => b.Status == WorkItemStatus.FailedWithDeduction).Sum(b => b.TotalAmountDeducted)
                    //}).ToArray(),

                    TaskCreditSum = _wrokItems.Where(a => a.EmployeeId == x.EmployeeId && a.IsActive && a.Status == WorkItemStatus.Completed).Sum(a => a.TotalAmountCredited),
                    TaskDebitSum = _wrokItems.Where(a => a.EmployeeId == x.EmployeeId && a.IsActive && a.Status == WorkItemStatus.FailedWithDeduction).Sum(a => a.TotalAmountDeducted),
                }).ToList();

            xx.ForEach(x =>
            {
                x.WorkedMins = x.WorkedHours * 60;
                x.LateHours = x.LateMins / 60;

                x.OvertimeMins = x.OvertimeHours * 60;
            });


            return xx;
        }

        public async Task<List<ChartDataX>> GetEmployeeInteraction2DByDateBetweenPeriod(int companyId, DateTime startDate, DateTime endDate, int[] empIds)
        {
            return await context.Attendances
                .Where(x => x.Date >= startDate && x.Date <= endDate && x.CompanyId == companyId && empIds.Contains(x.EmployeeId) && x.IsPublished)
                .GroupBy(x => new { x.Date.Date, x.EmployeeId })
                .Select(x => new ChartDataX
                {
                    Date = x.Key.Date,
                    EmployeeId = x.Key.EmployeeId,
                    DateString = x.Key.Date.Date.ToString("yyyy-MM-dd"),
                    TotalScheduledHours = x.Sum(a => (a.WorkEndTime - a.WorkStartTime).TotalHours),
                    ActualWorkedHours = x.Sum(a => a.TotalWorkedHours),
                    ActualWorkedHoursPerSchedule = x.Sum(a => a.TotalHoursWorkedPerSchedule),

                    OvertimeWorkedHoursPerSchedule = x.Where(a => a.IsOvertime).Sum(a => a.TotalHoursWorkedPerSchedule),
                    //LeaveCount = x.Count(a => a.CurrentStatus == AttendanceStatus.Absent),

                    TotalAbsentCount = x.Count(a => a.CurrentStatus == AttendanceStatus.Absent),
                    LateEmployeeCount = x.Count(a => a.CurrentStatus == AttendanceStatus.Late),
                    TotalLateHours = x.Where(a => a.CurrentStatus == AttendanceStatus.Late).Sum(a => (int)(a.TotalLateMins / 60)),
                    TotalLateMins = x.Where(a => a.CurrentStatus == AttendanceStatus.Late).Sum(a => (a.TotalLateMins)),
                    TotalLateDays = x.Count(a => a.CurrentStatus == AttendanceStatus.Late),
                }).ToListAsync();
        }


    }
}
