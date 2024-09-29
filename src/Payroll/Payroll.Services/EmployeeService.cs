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
    public class EmployeeService
    {
        private readonly Database.PayrollDbContext context;
        private readonly UserResolverService userResolverService;
        private readonly UserManager<AppUser> userManager;

        public EmployeeService(Payroll.Database.PayrollDbContext payrolDbContext, UserResolverService userResolverService,
            UserManager<AppUser> userManager)
        {
            this.context = payrolDbContext;
            this.userResolverService = userResolverService;
            this.userManager = userManager;
        }

        public async Task<EmployeeSummaryVm> GetCurrentEmployee()
        {
            var userId = userResolverService.GetUserId();
            var employee = await context.Employees
                                .Where(x => x.HasUserAccount && x.UserId == userId && x.HrStatus != HrStatus.Terminated &&
                                    x.Id == userResolverService.GetEmployeeId())
                                    .Include(a => a.Department)
                                    .Include(a => a.Location)
                                    .Include(a => a.Division)
                                    .Include(a => a.Employments)
                                        .ThenInclude(a => a.ReportingEmployee)
                                    .Include(a => a.EmployeeDirectReports)
                                    .Select(a=> new EmployeeSummaryVm(a, userResolverService.GetClaimsPrincipal()))
                                .FirstOrDefaultAsync();
            return employee;
        }

        public async Task<List<IGrouping<string, EmployeeSummaryVm>>> GetDirectoryAll(int groupBy = 1, string query = "")
        {
            // "Name", "Department", "Location", "Status" 
            var comapnyId = userResolverService.GetCompanyId();
            var _emp = await context.Employees
             .Where(x => x.Department.CompanyId == comapnyId
             && (string.IsNullOrWhiteSpace(query) || x.GetSystemName(userResolverService.GetClaimsPrincipal()).ToLower().Contains(query)))
             .OrderBy(x => x.Department.DisplayOrder)
             .ThenBy(x => x.EmpID)
             .Include(x => x.Location)
             .Include(x => x.Department)
             .Include(x => x.EmployeeDirectReports)
                .ThenInclude(x => x.Job)
             .Include(x => x.Employments)
                .ThenInclude(x => x.ReportingEmployee)
                .Select(a=> new EmployeeSummaryVm(a, userResolverService.GetClaimsPrincipal()))
                .ToListAsync();


                var emp = _emp.GroupBy(g => groupBy == 1 ? g.FullName.Substring(0, 1) : groupBy == 2 ? g.Department : groupBy == 3 ? g.Location : g.EmployementStatus)
                    .OrderBy(a => a.Key)
                    .ToList();

            return emp; // .ToDictionary(a=> a.Key, a=> a.ToList());
        }

        public async Task<List<WorkItem>> GetPendingTasks(int id)
        {
            var startDate = DateTime.Now.AddDays(-5);
            var datas = await context.WorkItems
                                .Where(x => x.EmployeeId == id && !x.IsCompleted && x.Date >= startDate)
                                .Include(a=> a.Work)
                                .OrderByDescending(a => a.PublishedDate)
                                .ToListAsync();
            return datas;
        }


        public async Task<List<BiometricRecord>> GeTodaysBiometricRecords(int empId)
        {
            var now = DateTime.Today;
            return await context.BiometricRecords.Where(a => a.EmployeeId == empId && a.Attendance.Date == now).ToListAsync();
        }

        public async Task<List<Announcement>> GetAnnouncements(int id)
        {
            var announcements = await context.Announcements
                                .Where(x => x.EmployeeSelectorVm != null && x.EmployeeSelectorVm.EmployeeIds.Contains(id) && x.Status == AnnouncementStatus.Published)
                                .OrderByDescending(a => a.PublishedDate)
                                .ToListAsync();
            return announcements;
        }

        public async Task<List<EmployeePayComponent>> GetCompensations(int id)
        {
            return await context.EmployeePayComponents
                                .Where(x => x.EmployeeId == id)
                                .Include(x=> x.PayAdjustment)
                                .ToListAsync();
        }

        public async Task<List<Employment>> GetEmployeeJobHistory(int id)
        {
            return await context.Employments
                                .Where(x => x.EmployeeId == id)
                                .Include(a=> a.ReportingEmployee)
                                .Include(a=> a.Job)
                                    .ThenInclude(a=> a.Division)
                                .Include(a => a.Job)
                                    .ThenInclude(a => a.Level)
                                .Include(a => a.Job)
                                    .ThenInclude(a => a.Location)
                                .ToListAsync();
        }
        //public async Task<List<EmployeeType>> GetEmploymentHistory(int id)
        //{
        //    return await context.EmployeeTypes
        //                        .Where(x => x.EmployeeId == id)
        //                        .ToListAsync();
        //}

        public async Task<List<Employee>> GetEmployeeDirectReports(int id)
        {
            return await context.Employees
                                .Where(x => x.Id == id)
                                .Include(a => a.EmployeeDirectReports)
                                .ThenInclude(a => a.Job)
                                .SelectMany(a=> a.EmployeeDirectReports)
                                .ToListAsync();
        }

        public async Task<List<DayOffEmployee>> GetDayOffSummary(int? empId = null, int? year = null)
        {
            var _empId = empId ?? userResolverService.GetEmployeeId();
            var _year = year ?? DateTime.UtcNow.Year;
            return await context.DayOffEmployees.AsQueryable()
                                .Where(x => x.EmployeeId == _empId
                                && (_year == x.Year))
                                .Include(a=> a.DayOffEmployeeItems)
                                .Include(x => x.DayOff)
                                .ToListAsync();
        }


        public async Task<List<DayOffEmployeeItem>> GetDayOffUsages(int? empId = null, int? year = null, int page =1 , int limit = 10)
        {
            var _empId = empId ?? userResolverService.GetEmployeeId();
            var _year = year ?? DateTime.UtcNow.Year;
            return await context.DayOffEmployeeItems
                                .Where(x => x.DayOffEmployee.Year == _year)
                                .Include(a => a.DayOffEmployee)
                                    .ThenInclude(a => a.DayOff)
                                .Skip((page - 1) * limit)
                                .Take(limit)
                                .ToListAsync();
        }

        public async Task<List<CompanyFileShare>> GetCompanyFileShares(int? empId = null)
        {
            return await context.CompanyFileShares
                .Where(a => a.EmployeeId == empId)
                .Include(a => a.CompanyFile)
                .ThenInclude(a => a.CompanyFolder)
                .ToListAsync();
        }

        public async Task<IdentityResult> Terminate(int id, string empId = "", string name = "")
        {
            var emp = await context.Employees.FindAsync(id);
            if (emp == null)
                return IdentityResult.Failed(new IdentityError { Description = "Employee was not found" });

            emp.DateOfTermination = DateTime.Now;
            await context.SaveChangesAsync();

            return IdentityResult.Success;
        }


        public async Task<EmployeeSummaryVm> GetCurrentEmployeeSummaryAsync(int id)
        {
            var employee = await context.Employees
                                .Where(x => x.Id == id)
                                    .Select(a=> new EmployeeSummaryVm
                                    {
                                        Id= a.Id,
                                        FirstName = a.FirstName,
                                        LastName = a.LastName,
                                        MiddleName = a.MiddleName,
                                        FullName = a.GetSystemName(userResolverService.GetClaimsPrincipal()),
                                        //Address = a.Address,
                                        //City = a.City,
                                        DateOfBirth = a.DateOfBirth.ToSystemFormat(userResolverService.GetClaimsPrincipal(), false),
                                        JoinedDate = a.DateOfJoined.ToSystemFormat(userResolverService.GetClaimsPrincipal(), false),
                                        Department = a.Department != null ? a.Department.Name : "",
                                        Location = a.Location != null ? a.Location.Name : "",
                                        EmployementStatus = a.EmployeeStatus.GetDisplayName(),
                                        Division = a.Division != null ? a.Division.Name : "",
                                        Designation = a.JobTitle,
                                        Email = a.EmailPersonal,
                                        Phone = a.PhonePersonal,
                                        Street = a.Street,
                                        EmpID = a.EmpID,
                                    })
                                .FirstOrDefaultAsync();


            employee.DateTimeNow = DateTime.UtcNow.ToSystemFormat(userResolverService.GetClaimsPrincipal(), true, false);
            employee.DateNow = DateTime.UtcNow.ToSystemFormat(userResolverService.GetClaimsPrincipal(), false, false);
            employee.TimeNow = DateTime.UtcNow.ToSystemFormat(userResolverService.GetClaimsPrincipal(), false, true);
            return employee;
        }


        public async Task<Employee> GetCurrentEmployeeWithDepartment(int? empId = null)
        {
            var _empId = empId ?? userResolverService.GetEmployeeId();
            var employee = await context.Employees
                                .Where(x => x.Id == _empId)
                                .Include(x=> x.Department)
                                .FirstOrDefaultAsync();
            return employee;
        }

        public async Task<List<DayOffEmployee>> GetCurrentEmployeeDayOffs(int? empId = null, int ? year = null)
        {
            var _empId = empId ?? userResolverService.GetEmployeeId();
            var employee = await context.DayOffEmployees
                                .Where(x => x.EmployeeId == _empId
                                && (year== null || year== x.Year))
                                .Include(x => x.DayOff)
                                .Include(x => x.DayOffEmployeeItems)
                                .ToListAsync();
            return employee;
        }

        public async Task<Dictionary<string, decimal>> GetCurrentEmployeeDayOffBalances(int? empId = null, int? year = null)
        {
            var _empId = empId ?? userResolverService.GetEmployeeId();
            var _year = year ?? DateTime.UtcNow.Year;
            var cmpDayoffs = await context.DayOffs.Where(a => a.CompanyId == userResolverService.GetCompanyId())
                .AsQueryable()
                .ToDictionaryAsync(x => x.Id, x => x.Name);
            var _empDayOffYear = await context.DayOffEmployees
                                .Where(x => x.EmployeeId == _empId
                                && (_year == x.Year))
                                .AsQueryable()
                                .GroupBy(a => a.DayOffId)
                                .Select(a=> new { a.Key, Count = a.Sum(z => z.TotalRemainingHours) })
                                .ToListAsync();
            var empDayOffYear = _empDayOffYear
                                .ToDictionary(a => a.Key, a => a.Count);

            return cmpDayoffs.ToDictionary(a => a.Value, a => empDayOffYear.Any(e => e.Key == a.Key) ? empDayOffYear.First(e => e.Key == a.Key).Value : 0);
        }

        public async Task<Dictionary<string, decimal>> GetCurrentEmployeeDayOffUsage(int? empId = null, int? year = null)
        {
            var _empId = empId ?? userResolverService.GetEmployeeId();
            var _year = year ?? DateTime.UtcNow.Year;
            var cmpDayoffs = await context.DayOffs.Where(a => a.CompanyId == userResolverService.GetCompanyId())
                                .AsQueryable()
                .ToDictionaryAsync(x => x.Id, x => x.Name);
            var empDayOffYear = await context.DayOffEmployees
                                .Where(x => x.EmployeeId == _empId
                                && (_year == x.Year))
                                .AsQueryable()
                                .GroupBy(a => a.DayOffId)
                                .Select(a=> new { a.Key, Count = a.Sum(z => z.TotalRemainingHours) })
                                .ToDictionaryAsync(a => a.Key, a => a.Count);

            return cmpDayoffs.ToDictionary(a => a.Value, a => empDayOffYear.Any(e => e.Key == a.Key) ? empDayOffYear.First(e => e.Key == a.Key).Value : 0);
        }

        public async Task<Dictionary<string, int>> GetLeaveRequestByStatus(int empId, int? year = null)
        {
            var _year = year ?? DateTime.UtcNow.Year;
            var data = await context.Requests.Where(x => x.EmployeeId == empId && x.RequestType == RequestType.Leave && x.CreationDate.Year == _year)
                                    .AsQueryable()
                                .GroupBy(a => a.Status)
                                .Select(a=> new { a.Key, Count = a.Count() })
                                .ToDictionaryAsync(a => a.Key, a => a.Count);

            var dits = new Dictionary<string, int>();
            dits.Add("Submitted", data.Count(a => a.Key == WorkItemStatus.Submitted));
            dits.Add("Approved", data.Count(a => a.Key == WorkItemStatus.Approved));
            dits.Add("Rejected", data.Count(a => a.Key == WorkItemStatus.Rejected));

            return dits;
        }

        public async Task<List<DayOffEmployee>> GetPaidTimeOffAccrurals(int empId, int? year = null)
        {
            return await context.DayOffEmployees
                .Where(x => x.EmployeeId == empId
                && (year == null || year == x.Year))
                .Include(a=> a.DayOff)
                .ToListAsync();
        }

        public async Task<List<Employee>> GetAllEmployeesInMyCompany(List<int> empIds = null)
        {
            var userId = userResolverService.GetUserId();
            var employee = await context.Employees
                .Include(t=> t.Department)
                .Where(x => x.Department.CompanyId == userResolverService.GetCompanyId() && (empIds== null || empIds.Contains(x.Id)))
                .ToListAsync();
            return employee;
        }

        public async Task<List<Employee>> GetAllEmployeesInMyTeam(int teamId)
        {
            var employee = await context.Employees
                .Where(x => x.EmployeeTeams.Any(t=> t.TeamId == teamId))
                .ToListAsync();
            return employee;
        }

        public async Task<List<Employee>> GetAllEmployeesInMyDepartment(Employee emp)
        {
            var employee = await context.Employees
                .Where(x => x.DepartmentId == emp.DepartmentId)
                .ToListAsync();
            return employee;
        }

        public async Task<List<Employee>> GetAllEmployeesInMyCompanyForDropdownOptGroups()
        {
            var userId = userResolverService.GetUserId();
            var employee = await context.Employees
                .Where(x => x.Department.CompanyId == userResolverService.GetCompanyId())
                .Include(x=> x.Department)
                .ToListAsync();
            return employee;
        }

        /// <summary>
        /// Returns employees in company for payroll (returns only employees who have contract between payroll dates)
        /// sameples if to return only 3 mpls for testing adustment config
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="payrollStart"></param>
        /// <param name="payrollEnd"></param>
        /// <returns></returns>
        public async Task<List<Employee>> GetAllEmployeesInMyCompanyForPayroll(bool sample =false, DateTime? payrollStart = null, DateTime? payrollEnd = null, int? companyId = null, int? payAdjustmentId = null, int[] empIds = null)
        {
            var limit = !sample ? int.MaxValue : 3;
            var _cmpId = companyId ?? userResolverService.GetCompanyId();
            if (sample)
            {
                return await context.Employees
                    .Where(x => x.CompanyId == _cmpId && x.EmployeeStatus == EmployeeStatus.Active)
                    .Include(x => x.EmployeePayComponents)
                    .Include(x => x.Department)
                    .Take(limit)
                    .ToListAsync();
            }

            if (payrollEnd == null || payrollStart == null)
                return new List<Employee>();

            return await context.Employees.Where(a => a.CompanyId == _cmpId && a.EmployeeStatus == EmployeeStatus.Active 
            && (empIds == null || empIds.Contains(a.Id))
            //&& (a.EmployeeStatus != EmployeeStatus.Incomplete && a.EmployeeStatus != EmployeeStatus.Terminated)
            && (payAdjustmentId == null || a.EmployeePayComponents.Any(c => c.PayAdjustmentId == payAdjustmentId)))
            //(a.StartDate < pp.PayrollPeriod.EndDate &&  a.EndDate > pp.PayrollPeriod.StartDate
            /* && a.StartDate > pp.PayrollPeriod.StartDate))*/
            .Include(a => a.EmployeePayComponents)
            .ThenInclude(a => a.PayAdjustment)
            .Include(a => a.Department)
            .ToListAsync();

            //var activeCOntractsForThisPayroll = await GetActiveContractsDuring(payrollStart.Value, payrollEnd.Value, companyId: companyId ?? userResolverService.GetCompanyId());
            //return activeCOntractsForThisPayroll.Select(a => a.Employee).Distinct().ToList();
        }

        public async Task<string[]> GetMappedUserIdsAsync(int[] empIds)
        {
            var userIds = await context.Employees.Where(a => a.HasUserAccount && empIds.Contains(a.Id))
                .Select(a => a.UserId).ToArrayAsync();
            var usersInDb = await userManager.Users.Where(a => userIds.Contains(a.Id)).Select(a => a.Id).ToArrayAsync();
            return usersInDb;
        }

        public async Task<List<Employee>> GenEmployeeCardsForAllEmployeeOfCompany(int? cmpId = null)
        {
            var employee = await context.Employees
                .Where(x => x.Department.CompanyId == userResolverService.GetCompanyId())
                .ToListAsync();

            foreach (var emp in employee)
            {
                var kpiAnalysis = await GetEmployeeCardAsync(emp.Id, DateTime.Now.AddMonths(-3), DateTime.Today);
                emp.Grade = kpiAnalysis.Grade;
                emp.CssClass = "_" + emp.Grade.ToLower();
                emp.Percent = emp.Percent;
                emp.PercentStr = kpiAnalysis.PercentStr;


                //kpiAnalysis.KpiValues.ForEach(k => k.EmployeeId = emp.Id);
                emp.KpiValues.AddRange(kpiAnalysis.KpiValues);
                emp.KpiValues.ForEach(t => t.IsActive = true);

            }

            await context.SaveChangesAsync();
            return employee;
        }


        public async Task<KpiAnalysis> GetEmployeeCardAsync(int empId, DateTime _start, DateTime _end)
        {
            var absentDays = await context.Attendances.CountAsync(x => x.EmployeeId == empId && x.CurrentStatus == AttendanceStatus.Absent && x.Date >= _start && x.Date <= _end);
            var lateMins = await context.Attendances.Where(x => x.EmployeeId == empId && x.CurrentStatus == AttendanceStatus.Late && x.Date >= _start && x.Date <= _end).SumAsync(x => x.TotalLateMins);
            var mm = await context.WorkItems.Where(x => x.EmployeeId == empId && x.Date >= _start && x.Date <= _end && !x.IsEmployeeTask)
                .GroupBy(x => x.Work.Type)
                .Select(x => new
                {
                    x.Key,
                    Count = x.Count(),
                    TotalApproved = x.Sum(a => a.TotalApproved),
                    RemainingSubmissions = x.Sum(a => a.RemainingSubmissions),
                    TotalRequired = x.Sum(a => a.Work.TotalRequiredCount),
                    TotalCompleted = x.Count(a => a.Status == WorkItemStatus.Completed),
                }).ToListAsync();

            var submissionOverallCompletionPercent = (decimal)
                mm.Where(x => x.Key == WorkType.RequireSubmissions).Sum(a => a.TotalApproved) /
                 (mm.Where(x => x.Key == WorkType.RequireSubmissions).Sum(a => a.TotalRequired) + 0.1m);
            submissionOverallCompletionPercent = submissionOverallCompletionPercent * 100;

            var clockTasksOverallCompletionPercent = (decimal)
                mm.Where(x => x.Key == WorkType.ExpectClockInRecordsBefore).Sum(a => a.TotalCompleted) /
                 (mm.Where(x => x.Key == WorkType.ExpectClockInRecordsBefore).Sum(a => a.Count) + 0.1m);
            clockTasksOverallCompletionPercent = clockTasksOverallCompletionPercent * 100;

            var disciplinaryActionCount = 2;

            // KPIs
            // absent (3 days), lateMins (20), disciplinaryAction (3)

            // calculate percents
            var absentPercent = (absentDays / 3) * 100;
            var latePercent = (decimal)(lateMins / 20) * 100;
            var disciplinaryActionpecertnt = (disciplinaryActionCount / 3) * 100;

            var kpiAnalysis = new KpiAnalysis();

            //List<(string kpi, int best, int worst, int actual, decimal percent, decimal weight, decimal score, string str)> kpiValues = new List<(string kpi, int best, int worst, int actual, decimal percent, decimal weight, decimal score, string str)>();

            /*
             * matrix   
             * absentDays   35%     .35 
             * lateMins     20%     .20
             * Task_Sub     25%     .25
             * Task_Clock   10%     .10
             * Penaly       10%     .10
             * Total        100%    1.00
             */

            // 
            kpiAnalysis.KpiValues.Add(new KpiValue("Absent Days (*3 days)", 0, 3, absentDays, 0, 0.35m, 0, "Absent Days"));
            kpiAnalysis.KpiValues.Add(new KpiValue("Late Minutes (*20 mins)", 0, 20, (int)lateMins, 0, .20m, 0, "Late Minutes"));

            kpiAnalysis.KpiValues.Add(new KpiValue("Task Submissions", mm.Where(x => x.Key == WorkType.RequireSubmissions).Sum(a => a.TotalRequired), 0, mm.Where(x => x.Key == WorkType.RequireSubmissions).Sum(a => a.TotalApproved), submissionOverallCompletionPercent, .25m, 0, "Tasks Approved"));

            kpiAnalysis.KpiValues.Add(new KpiValue("Task Clock Ins", mm.Where(x => x.Key == WorkType.ExpectClockInRecordsBefore).Sum(a => a.Count), 0, mm.Where(x => x.Key == WorkType.ExpectClockInRecordsBefore).Sum(a => a.TotalCompleted), clockTasksOverallCompletionPercent, .10m, 0, "Clock-In Tasks"));

            kpiAnalysis.KpiValues.Add(new KpiValue("Disciplinary Action (*3 actions)", 0, 3, 0, disciplinaryActionpecertnt, .10m, 0, "Disciplinary Action"));


            //keyValues.Add(new Tuple<string, decimal, string>("Total Weightage", 0, ((totalWeightage).ToString("N0") + "%")));

            //Grade = "";

            CalculateKpiPercentAndScore(kpiAnalysis.KpiValues);

            //var kpiValues =  (string kpi, int best, int worst);

            // KPI, best, worst, ActualValue, Weight, Score 
            //var keyValues = new List<Tuple<string, decimal, string>>();
            //keyValues.Add(new Tuple<string, decimal, string>("Absent Days (*3 days)", absentPercent, absentDays.ToString()));
            //keyValues.Add(new Tuple<string, decimal, string>("Late Minutes (*20 mins)", latePercent, lateMins.ToString()));
            //keyValues.Add(new Tuple<string, decimal, string>("Task Submissions", submissionOverallCompletionPercent, $"{mm.Where(x => x.Key == WorkType.RequireSubmissions).Sum(a => a.TotalApproved)}/{mm.Where(x => x.Key == WorkType.RequireSubmissions).Sum(a => a.TotalRequired)}"));
            //keyValues.Add(new Tuple<string, decimal, string>("Task Clock Ins", clockTasksOverallCompletionPercent, $"{mm.Where(x => x.Key == WorkType.ExpectClockInRecordsBefore).Sum(a => a.TotalCompleted)}/{mm.Where(x => x.Key == WorkType.ExpectClockInRecordsBefore).Sum(a => a.Count)}"));
            //keyValues.Add(new Tuple<string, decimal, string>("Disciplinary Action (*3 actions)", disciplinaryActionpecertnt, disciplinaryActionCount.ToString()));

            //var totalWeightage = (absentDays / .35);
            //totalWeightage += (lateMins / .20);
            //totalWeightage += ((double)submissionOverallCompletionPercent / .25);
            //totalWeightage += ((double)clockTasksOverallCompletionPercent / .10);
            //totalWeightage += (disciplinaryActionpecertnt / .10);

            //var percent = totalWeightage;
            var totalWeightage = kpiAnalysis.KpiValues.Sum(x => x.Score);
            //keyValues.Add(new Tuple<string, decimal, string>("Total Weightage", 0, ((totalWeightage).ToString("N0") + "%")));


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



            //kpiAnalysis.KpiValues.Add(new KpiValue("Total Weightage", 0, 0, 0, 1, 1, totalWeightage, $"Total Percent ({gade})"));
            kpiAnalysis.Grade = gade;
            kpiAnalysis.Percent = totalWeightage;
            kpiAnalysis.PercentStr = $"{totalWeightage.ToString("N0")}%";
            
            //kpiValues.Add(("Class / Grade", 0, 0, 0, 1, 1, 0, gade));
            //keyValues.Add(new Tuple<string, decimal, string>("Class / Grade", 0, gade.ToString()));

            return kpiAnalysis;
        }


        public KpiAnalysis GetEmployeeCardFromPayPeriodEmployee(PayrollPeriodEmployee payrolPeriodEmployee)
        {
            if (payrolPeriodEmployee == null)
                return new KpiAnalysis { IsGraded = false };

            var kpiAnalysis = new KpiAnalysis();
            kpiAnalysis.IsGraded = payrolPeriodEmployee.IsGraded;
            kpiAnalysis.Grade = payrolPeriodEmployee.Grade;
            kpiAnalysis.Percent = payrolPeriodEmployee.Percent;
            kpiAnalysis.PercentStr = payrolPeriodEmployee.PercentStr;
            //kpiAnalysis.CssClass = "_" + payrolPeriodEmployee.Grade.ToLower(); 
            // payrolPeriodEmployee.Css;
            kpiAnalysis.GradeGeneratedDateTime = payrolPeriodEmployee.GradeGeneratedDateTime;

            kpiAnalysis.KpiValues.AddRange(payrolPeriodEmployee.KpiValues);

            return kpiAnalysis;
        }

        public void CalculateKpiPercentAndScore(List<KpiValue> kpiValues)
        {
            kpiValues.ForEach(kpi =>
            {
                if (kpi.Percent > 0)
                {
                    kpi.Score = (kpi.Percent) * kpi.Weight;
                }
                else
                {
                    if (kpi.Actual > kpi.Best && kpi.Actual > kpi.Worst)
                        kpi.Percent = 0;
                    else if (kpi.Actual <= kpi.Best)
                        kpi.Percent = 100;
                    else
                    {
                        kpi.Percent = (kpi.Actual / (kpi.Worst + 0.1m));
                        kpi.Percent = (1 - kpi.Percent) * 100;
                    }

                    kpi.Score = (kpi.Percent) * kpi.Weight;
                }
            });
        }

        /// <summary>
        /// Get list of employees of user company with Active Employment and also filter empIds array
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="empIds"></param>
        /// <returns></returns>
        public async Task<List<Employee>> GetActiveEmploymentsDuring(DateTime startDate, DateTime endDate, int[] empIds = null, int? companyId = null)
        {
            companyId = companyId ?? userResolverService.GetCompanyId();

            return await context.Employees.Where(a => a.CompanyId == companyId
               && (empIds == null || empIds.Contains(a.Id)) &&
               (a.EmployeeStatus != EmployeeStatus.Incomplete && a.EmployeeStatus != EmployeeStatus.Terminated) &&
               (a.DateOfJoined < endDate && a.GetEndDate() > startDate /*&& startDate < a.StartDate*/))
               .Include(a => a.Department)
               //.Include(a => a.Employee)
               //.ThenInclude(a => a.EmployeePayComponents)
               .ToListAsync();
        }
        

        /// <summary>
        /// Update Employee HasUserAccount to new Access Grant
        /// (Does NOT save)
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="newAccess"></param>
        /// <returns></returns>
        public async Task<Employee> GetEmployeeById(int employeeId)
        {
            var employee = await context.Employees
                                .Where(x => employeeId == x.Id)
                                .Include(a=> a.EmployeeRoles)
                                .FirstOrDefaultAsync();
            return employee;

        }

        public async Task<IDictionary<int, string>> GetEmployeeByRoles(int employeeId)
        {
            try
            {
                return await context.EmployeeRoleRelations
                                    .Include(a => a.EmployeeRole)
                                    .Where(x => employeeId == x.EmployeeId)
                                    .ToDictionaryAsync(a => a.EmployeeRoleId, a => a.EmployeeRole.Role);
            }
            catch (Exception)
            {
                return default;
            }
        }

        public async Task<List<IDictionary<string, string[]>>> GetEmployeeByRoleRoutes(int employeeId)
        {
            return await context.EmployeeRoleRelations
                                .Include(a => a.EmployeeRole)
                                .Where(x => employeeId == x.EmployeeId && x.EmployeeRole.UserAccessRights != null)
                                .Select(a => a.EmployeeRole.UserAccessRights)
                                .ToListAsync();

            //                    .ToDictionaryAsync(a => a.EmployeeRoleId, a => a.EmployeeRole.Role);
            //return employee;
        }

        public void AddEmployeeAction(ref Employee emp, ref  Job job, int? reportingEmpId, ActionType type)
        {
            try
            {
                var _a = new EmployeeAction
                {
                    EmployeeId = emp.Id,
                    JobId = job.Id,
                    Job= job,
                    Employee = emp,
                    ActionType = type,
                    ReportingEmployeeId = reportingEmpId,
                    OnDate = DateTime.Now,
                };

                switch (_a.ActionType)
                {
                    case ActionType.NA:
                        break;
                    case ActionType.APP:
                        _a.Message = $"Appointed to Job {job.JobID} {job.JobTitle}";
                        if (job.JobStatus != JobStatus.Vacant)
                            throw new Exception("Job must be vacant to process Appointment");
                        job.JobStatus = JobStatus.Occupied;
                        emp.HrStatus = HrStatus.Employed;
                        emp.JobId = job.Id;
                        break;
                    //case ActionType.Promotion:
                    //    break;
                    //case ActionType.Demotion:
                    //    break;
                    //case ActionType.Transfer:
                    //    break;
                    //case ActionType.Warning_Oral:
                    //    break;
                    //case ActionType.Warning_Written:
                    //    break;
                    //case ActionType.Suspension:
                    //    break;
                    //case ActionType.Termination:
                    //    break;
                    default:
                        break;
                }

                context.EmployeeActions.Add(_a);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error has occured, " + ex.Message);
            }
        }

        
        public void AddJobActionHistory(ref Employee emp, ref  Job job, int? reportingEmpId, ActionType type)
        {
            try
            {
                var _a = new JobActionHistory
                {
                    CompanyId = emp.CompanyId,
                    DepartmentId = emp.DepartmentId.Value,
                    //Individual = emp.Individual,
                    IndividualName = emp.GetSystemName(userResolverService.GetClaimsPrincipal()),
                    EmployeeId = emp.Id,
                    JobId = job.Id,
                    Job= job,
                    JobIdString = job.JobID,
                    Employee = emp,
                    ActionType = type,
                    // ReportingEmployeeId = reportingEmpId,
                    OnDate = DateTime.Now,
                };

                switch (_a.ActionType)
                {
                    case ActionType.NA:
                        break;
                    case ActionType.APP:
                        _a.Remarks = $"Appointed to Job {job.JobID} {job.JobTitle}";
                        if (job.JobStatus != JobStatus.Vacant)
                            throw new Exception("Job must be vacant to process Appointment");
                        job.JobStatus = JobStatus.Occupied;
                        emp.HrStatus = HrStatus.Employed;
                        emp.JobId = job.Id;
                        break;
                    case ActionType.MIG:
                        job.JobStatus = JobStatus.Occupied;
                        emp.EmployeeStatus = EmployeeStatus.Active;
                        emp.HrStatus = HrStatus.Employed;
                        _a.Remarks = "Data Migrated";
                        break;
                    //case ActionType.Promotion:
                    //    break;
                    //case ActionType.Demotion:
                    //    break;
                    //case ActionType.Transfer:
                    //    break;
                    //case ActionType.Warning_Oral:
                    //    break;
                    //case ActionType.Warning_Written:
                    //    break;
                    //case ActionType.Suspension:
                    //    break;
                    //case ActionType.Termination:
                    //    break;
                    default:
                        break;
                }

                context.JobActionHistories.Add(_a);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error has occured, " + ex.Message);
            }
        }
    }
}
