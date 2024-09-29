using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll.Database;
using Payroll.Models;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Payroll.Services
{
    internal static class DbSetExtensions
    {
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this DbSet<T> dbSet)
            where T : class
        {
            return dbSet;
        }

        public static IQueryable<T> AsQueryable<T>(this DbSet<T> dbSet)
            where T : class
        {
            return dbSet;
        }
    }



    public class CompanyService
    {
        private readonly AccountDbContext accDbContext;
        private readonly PayrollDbContext context;
        private readonly UserResolverService userResolverService;

        public CompanyService(AccountDbContext accDbContext, PayrollDbContext context, UserResolverService userResolverService)
        {
            this.context = context;
            this.accDbContext = accDbContext;
            this.userResolverService = userResolverService;
        }


        public async Task<List<Company>> GetAllCompanies()
        {
            var employee = await context.Companies
                .ToListAsync();
            return employee;
        }


        public async Task<Announcement> GetAnnouncement(int id)
        {
            var anns = await context.Announcements.Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Id == id)
                .Include(x => x.CreatedEmployee)
                .Include(x => x.FileDatas)
                .OrderByDescending(x => x.Id).Take(6)
                .FirstOrDefaultAsync();
            return anns;
        }

        public async Task<List<Job>> GetJobs(JobStatus? status = null, int? empId = null)
        {
            var anns = await context.Jobs.Where(x => x.CompanyId == userResolverService.GetCompanyId() && (status == null || x.JobStatus == status))
                .Include(x => x.Employees)
                .Include(x => x.Level)
                .OrderBy(x => x.JobID)
                .ThenBy(x => x.JobTitle)
                .ToListAsync();
                
            if(empId.HasValue)
            {
                var empJob = await context.Employees
                .Where(a=> a.Id == empId)
                .Include(a=>a.Job)
                .Select(a=> a.Job)
                .FirstOrDefaultAsync();
                if(empJob != null)
                anns.Add(empJob);
            }
            return anns;
        }


        public async Task<List<Announcement>> GetAnnouncements(int page = 1, int limit = 10)
        {
            var anns = await context.Announcements.Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .Skip((page - 1) * limit)
                .Take(limit)
                .Include(x => x.CreatedEmployee)
                .Include(x => x.FileDatas)
                .OrderByDescending(x => x.Id).Take(6)
                .ToListAsync();
            return anns;
        }

        public async Task<Dictionary<WorkItemStatus, EmployeeSummaryVm>> GetWhosOut(DateTime date)
        {
            return await context.Requests.Where(a => a.CompanyId == userResolverService.GetCompanyId()
            && a.RequestType == RequestType.Leave && a.Status != WorkItemStatus.Rejected && a.Start.Value.Date >= date && a.End.Value.Date <= date)
            .GroupBy(a=> new {a.Status, a.Employee })
            .ToDictionaryAsync(a => a.Key.Status, a => new EmployeeSummaryVm(a.Key.Employee, userResolverService.GetClaimsPrincipal()));
        }

        public async Task<Dictionary<string, List<EmployeeSummaryVm>>> GetUpcomingBirthdays(DateTime date)
        {
            var now = DateTime.Today;
            return await context.Employees.Where(a => a.CompanyId == userResolverService.GetCompanyId()
            && a.DateOfBirth.HasValue && a.DateOfBirth.Value.Month >= now.Month)
            .Select(a=> new
            {
                dob = new DateTime((a.DateOfBirth.Value.Month < now.Month ? now.AddYears(1).Year : now.Year),
                    a.DateOfBirth.Value.Month, a.DateOfBirth.Value.Day),
                Emp = a
            })
            .GroupBy(a=> a.dob.ToString("MMMM yyyy"))
            .ToDictionaryAsync(a => a.Key, a=> a.Select(x=> new EmployeeSummaryVm(x.Emp, userResolverService.GetClaimsPrincipal())).ToList());
        }

        public async Task<Dictionary<DateTime, List<EmployeeSummaryVm>>> GetBirthdayCalendar(int year, int month)
        {
            var now = DateTime.Today;
            var _ = await context.Employees.Where(a => a.CompanyId == userResolverService.GetCompanyId()
            && a.DateOfBirth.HasValue && a.DateOfBirth.Value.Month == month && a.DateOfBirth.Value.Day >= 1 && a.DateOfBirth.Value.Day <= DateTime.DaysInMonth(year, month))

            .Select(a => new
            {
                dob = new DateTime(now.Year, // (a.DateOfBirth.Value.Month < now.Month ? now.AddYears(1).Year : now.Year),
                    a.DateOfBirth.Value.Month, a.DateOfBirth.Value.Day),
                Emp = a
            })
            .ToListAsync();


            return _.GroupBy(a => a.dob) // .ToString("MMMM yyyy"))
            //.ToDictionaryAsync(a => a.Key, a => a.Select(x => new EmployeeSummaryVm(x.Emp, userResolverService.GetClaimsPrincipal())).ToList());
            .ToDictionary(a => a.Key, a => a.Select(x => new EmployeeSummaryVm(x.Emp, userResolverService.GetClaimsPrincipal())).ToList());
        }

        public async Task<Dictionary<DateTime, List<EmployeeSummaryVm>>> GetWorkAnniversaryCalendar(int year, int month)
        {
            var now = DateTime.Today;
            var _ = await context.Employees.Where(a => a.CompanyId == userResolverService.GetCompanyId()
            && a.DateOfJoined.Value.Year != now.Year && a.DateOfJoined.HasValue && a.DateOfJoined.Value.Month == month)

            .Select(a => new
            {
                dob = new DateTime(now.Year, //new DateTime((a.DateOfJoined.Value.Month < now.Month ? now.AddYears(1).Year : now.Year),
                    a.DateOfJoined.Value.Month, a.DateOfJoined.Value.Day),
                Emp = a
            }).ToListAsync();

            return  _.GroupBy(a => a.dob) // .ToString("MMMM yyyy"))
            //.ToDictionaryAsync(a => a.Key, a => a.Select(x => new EmployeeSummaryVm(x.Emp, userResolverService.GetClaimsPrincipal())).ToList());
            .ToDictionary(a => a.Key, a => a.Select(x => new EmployeeSummaryVm(x.Emp, userResolverService.GetClaimsPrincipal())).ToList());
        }

        //public async Task<List<Announcement>> GetAnnouncementsForEmployee(int page = 1, int limit = 10)
        //{
        //    var anns = await context.EmployeeInteractions.Where(x => x.EmployeeId == userResolverService.GetEmployeeId() && x.IsActive  && x.Announcement != null)
        //        .Skip((page - 1) * limit)
        //        .Take(limit)
        //        .Include(x => x.Announcement)
        //            .ThenInclude(x => x.FileDatas)
        //        .OrderByDescending(x => x.Id).Take(6)
        //        .ToListAsync();

        //    return anns.Select(x=> x.Announcement).Distinct().ToList();
        //}

        public async Task<List<KpiConfig>> GetCompanyKpiConfig(int companyId)
        {
            return await accDbContext.CompanyAccounts.Where(a => a.Id == companyId)
                .SelectMany(a => a.KpiConfig)
                .ToListAsync();
        }

        public async Task<bool> IsCompanyKpiConfigured(int companyId)
        {
            return await accDbContext.CompanyAccounts.AnyAsync(a => a.Id == companyId && a.IsKpiConfigured);
        }


        public async Task<List<Work>> GetCompanyWorks(bool addEmptyOption = false)
        {
            var currCompanyId = userResolverService.GetCompanyId();
            var items = await context.Works.Where(x => x.CompanyId == currCompanyId)
                .OrderBy(x => x.StartTime)
                .ToListAsync();

            return items;
        }

        //public async Task<List<Designation>> GetDesignationDropdown(bool addEmptyOption = false)
        //{
        //    var currCompanyId = userResolverService.GetCompanyId();
        //    var items = await context.Designations.Where(x => x.CompanyId == currCompanyId)
        //        .OrderBy(x => x.Name)
        //        .ToListAsync();

        //    if (addEmptyOption)
        //    {
        //        var viewAll = new Designation { Id = 0, Name = "All Designations" };
        //        items.Insert(0, viewAll);
        //    }

        //    return items;
        //}

        public async Task<List<CompanyWorkTime>> GetWorkTimes(bool addEmptyOption = false)
        {
            var currCompanyId = userResolverService.GetCompanyId();
            var items = await context.CompanyWorkTimes.Where(x => x.CompanyId == currCompanyId)
                .OrderBy(x => x.StartTime)
                .ToListAsync();

            if (addEmptyOption)
            {
                var viewAll = new CompanyWorkTime { Id = 0, ShiftName = "All Shifts" };
                items.Insert(0, viewAll);
            }

            return items;
        }

        public async Task<int> GetEmployeeCount(int? deptId = null)
        {
            var currCompanyId = userResolverService.GetCompanyId();
            return await context.Employees
                .Where(a => (deptId == null || a.DepartmentId == deptId) && a.EmployeeStatus == EmployeeStatus.Active)
                .CountAsync();
        }

        public async Task<List<Employee>> GetEmployeesOfCurremtCompany(bool addEmptyOption = false, int? deptId = null)
        {
            var currCompanyId = userResolverService.GetCompanyId();
            return await GetEmployeesOfCurremtCompany(currCompanyId, addEmptyOption, deptId);
        }

        public async Task<List<Employee>> GetAllEmployeesInMyCompanyForDropdownOptGroups(EmployeeStatus? filterStatus = null)
        {
            var userId = userResolverService.GetUserId();
            var employee = await context.Employees
                .Where(x => x.CompanyId == userResolverService.GetCompanyId() && (filterStatus == null || x.EmployeeStatus == filterStatus))
                .Include(x => x.Department).ToListAsync();
            return employee;
        }

        public async Task<List<Employee>> GetEmployeesOfCurremtCompany(int cmpId, bool addEmptyOption = false, int? deptId = null)
        {
            var currCompanyId = cmpId;
            var items = await context.Departments.Where(x => x.CompanyId == currCompanyId && x.Employees.Any() && (deptId == null || deptId == x.Id))
                .SelectMany(x => x.Employees)
                .OrderBy(x => x.Department.DisplayOrder)
                .ThenBy(x => x.EmpID)
                .Include(x => x.Department)
                .Select(a => new Employee { Id = a.Id, Name = a.GetSystemName(userResolverService.GetClaimsPrincipal()), Department = a.Department })
                .ToListAsync();

            if (addEmptyOption)
            {
                var viewAll = new Employee { Id = 0, Name = "All Employees" };
                items.Insert(0, viewAll);
            }

            return items;
        }
        public async Task<List<EmployeeRole>> GetEmployeesRolesOfCurremtCompany(int cmpId)
        {
            return await context.EmployeeRoles.Where(x => x.CompanyId == cmpId)
                .ToListAsync();
        }

        public async Task<List<EmployeeSummaryVm>> GetWhosIsOutList(int? cmpId = null)
        {
               var _Id = cmpId ?? userResolverService.GetCompanyId();
            var _now = DateTime.Now.Date;
            var employee = await context.Requests
                .Where(x => x.DayOffId.HasValue && x.DayOff.CompanyId == _Id && x.RequestType == RequestType.Leave && x.Status == WorkItemStatus.Approved && _now >= x.Start && _now <= x.End)
                .GroupBy(a => new { a.Employee.Id, Name = a.Employee.GetSystemName(userResolverService.GetClaimsPrincipal()), a.Employee.Avatar })
                .Select(a => new EmployeeSummaryVm
                {
                    Id = a.Key.Id,
                    FullName = a.Key.Name,
                    Avatar = a.Key.Avatar,
                    BackInDays = a.Sum(z => (z.End - _now).Value.TotalDays)
                })
                .ToListAsync();
            return employee;
        }

        public async Task<List<Employee>> GetEmployeesToReplace(int cmpId, bool addEmptyOption = false)
        {
            var currCompanyId = cmpId;
            var items = await context.Employees.Where(x => x.CompanyId == currCompanyId && x.Employments.Any() &&  x.EmployeeStatus == EmployeeStatus.Terminated)
                .OrderBy(x => x.Department.DisplayOrder)
                .ThenBy(x => x.EmpID)
                .Include(x => x.Department)
                .Select(a => new Employee { Id = a.Id, Name = a.GetSystemName(userResolverService.GetClaimsPrincipal()) })
                .ToListAsync();

            if (addEmptyOption)
            {
                var viewAll = new Employee { Id = 0, Name = "All Employees" };
                items.Insert(0, viewAll);
            }

            return items;
        }

        public async Task<List<Location>> GetLocationsOfCurremtCompany(int? cmpId = null, bool addEmptyOption = false, string emptyOptionLabel = null)
        {
            var currCompanyId = cmpId ?? userResolverService.GetCompanyId();
            return await context.Locations.Where(x => x.CompanyId == currCompanyId)
                //.OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }
        public async Task<int> GetCompanyCount(int? cmpId = null)
        {
            var currCompanyId = cmpId ?? userResolverService.GetCompanyId();
            return await context.Locations.Where(x => x.CompanyId == currCompanyId)
                //.OrderBy(x => x.DisplayOrder)
                .CountAsync();
        }

        public async Task<List<Division>> GetDivisionsOfCurremtCompany(bool addEmptyOption = false, string emptyOptionLabel = null)
        {
            var currCompanyId = userResolverService.GetCompanyId();
            return await context.Divisions.Where(x => x.CompanyId == currCompanyId)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<List<Department>> GetDepartmentsOfCurremtCompany(bool addEmptyOption = false, string emptyOptionLabel = null)
        {
            var currCompanyId = userResolverService.GetCompanyId();
            return await GetDepartmentsOfCurremtCompany(currCompanyId, addEmptyOption, emptyOptionLabel);
        }

        public async Task<List<Department>> GetDepartmentsOfCurremtCompany(int cmpId, bool addEmptyOption = false, string emptyOptionLabel = null)
        {
            var currCompanyId = cmpId;
            var items = await context.Departments.Where(x => x.CompanyId == currCompanyId)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            if (addEmptyOption)
            {
                var viewAll = new Department { Id = 0, Name = emptyOptionLabel ?? "All Departments" };
                items.Insert(0, viewAll);
            }

            return items;
        }

        public async Task<List<Nationality>> GetNationalities(int cmpId)
        {
            var currCompanyId = cmpId;
            return await context.Nationalities.Where(x => x.CompanyId == currCompanyId || x.CompanyId.HasValue == false)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<List<EmergencyContactRelationship>> GetEmergencyContactRelationships(int cmpId)
        {
            var currCompanyId = cmpId;
            return await context.EmergencyContactRelationships.Where(x => x.CompanyId == currCompanyId || x.CompanyId.HasValue == false)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<DayOfWeek> GetWorkStartDayOfWeek()
        {
            return await accDbContext.CompanyAccounts.Where(c => c.Id == userResolverService.GetCompanyId()
               && c.IsConfigured)
               .Select(a => a.WeekStartDay)
               .FirstOrDefaultAsync();
        }
        public async Task<List<DayOff>> GetDayOffs(int? id = null, int[] selected = null)
        {
            return await context.DayOffs.Where(c => (selected == null || selected.Contains(c.Id)) && c.CompanyId == (id ?? userResolverService.GetCompanyId()))
               .ToListAsync();
        }

        public async Task<List<PayAdjustment>> GetPayAdjustments(string type = "")
        {
            return await context.PayAdjustments.Where(c => (type == "" || c.VariationType.ToString().ToLower().Contains(type)) && c.CompanyId == userResolverService.GetCompanyId())
               .ToListAsync();
        }        
        
        public async Task<List<Label>> GetLabels()
        {
            return await context.Labels.Where(c => (c.CompanyId.HasValue && c.CompanyId == userResolverService.GetCompanyId()) || !c.CompanyId.HasValue)
               .ToListAsync();
        }

        public async Task<DateTime> GetCompanyWokTimeStartOrShiftStart(DateTime attendanceDate)
        {
            var timeStartShuift = await context.CompanyWorkTimes.Where(a => a.CompanyId == userResolverService.GetCompanyId())
                           .OrderBy(a => a.StartTime)
                           .Select(a => a.StartTime).FirstOrDefaultAsync();
            return attendanceDate.Date.Add(timeStartShuift);
        }

        public async Task<(DateTime, DateTime)> GetPayrollDates(DateTime? utcNow)
        {
            var today = utcNow?.Date ?? DateTime.UtcNow.Date;

            if (await context.PayrollPeriods.AnyAsync(a => a.CompanyId == userResolverService.GetCompanyId() && a.StartDate >= today && a.EndDate < today))
            {
                var _ = await context.PayrollPeriods.Where(a => a.CompanyId == userResolverService.GetCompanyId() && a.StartDate >= today && a.EndDate < today).Select(a => new { a.StartDate, a.EndDate }).FirstOrDefaultAsync();

                return (_.StartDate, _.EndDate);
            }
            else
            {
                var cmp = await accDbContext.CompanyAccounts.FirstOrDefaultAsync(a => a.Id == userResolverService.GetCompanyId());

                if (cmp?.IsConfigured ?? false)
                {
                    var _start = new DateTime(today.Year, today.Month, cmp.PayrolPeriodStartDate.Value);
                    return (_start, _start.AddDays(cmp.PayrolPeriodDays ?? 30));
                }
                return (today.AddDays(-15), today.AddDays(15));
            }
        }

        public async Task<CompanyAccount> GetCompanyAccount(int id)
        {
            return await accDbContext.CompanyAccounts.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<bool> IsCompanyStepDone(int id, int step)
        {
            var data = await accDbContext.CompanyAccounts.Where(x => x.Id == id)
                .Select(x => x.ProgressBySteps)
                .FirstOrDefaultAsync();

            if (data != null && data.ContainsKey(step) && data[step])
                return true;

            return false;
        }

        //public async Task UpdateProgressAndSaveAsync(int id, int step = 0)
        //{
        //    var cox = await GetCompanyAccount(id);
        //    if(cox != null)
        //    {
        //        UpdateProgress(cox, step);
        //        accDbContext.CompanyAccounts.Update(cox);
        //        await accDbContext.SaveChangesAsync();
        //    }
        //}

        public async Task<CompanyAccount> UpdateProgressAndSaveAsync(int companyId, int step)
        {
            var cmpAccount = await accDbContext.CompanyAccounts.FindAsync(companyId);
            if (cmpAccount == null) return null;

            return await UpdateProgressAndSaveAsync(cmpAccount, step);
        }


        public async Task<CompanyAccount> UpdateProgressAndSaveAsync(CompanyAccount model, int step)
        {
            UpdateProgress(model, step);
            accDbContext.CompanyAccounts.Update(model);
            await accDbContext.SaveChangesAsync();
            return model;
        }

        public void UpdateProgress(CompanyAccount model, int step = 0)
        {
            if (model.ProgressBySteps == null || model.ProgressBySteps.Count() == 0 || model.ProgressBySteps.Count() != 9)
            {
                model.ProgressBySteps = new Dictionary<int, bool>();
                model.ProgressBySteps.Add(1, false);
                model.ProgressBySteps.Add(2, false);
                model.ProgressBySteps.Add(3, false);
                model.ProgressBySteps.Add(4, false);
                model.ProgressBySteps.Add(5, false);
                model.ProgressBySteps.Add(6, false);
                model.ProgressBySteps.Add(7, false);
                model.ProgressBySteps.Add(8, false);
                model.ProgressBySteps.Add(9, false);
                //model.ProgressBySteps.Add(10, false);
                //model.ProgressBySteps.Add(11, false);
                //model.ProgressBySteps.Add(12, false);
            }

            var _progres = model.ProgressBySteps;
            if(!_progres.ContainsKey(step))
                _progres.Add(step, false);

            try
            {
                _progres[step] = true;
            }
            catch (Exception)
            {
            }
            finally
            {
                model.ProgressBySteps = _progres;
                var progress = (double)model.ProgressBySteps.Count(a => a.Value) / model.ProgressBySteps.Count();
                model.ProgressPercent = Convert.ToInt32(progress * 100);
                model.NextStep = _progres.Any(x => x.Value == false) ? _progres.Where(x => x.Value == false).First().Key.ToString() : 0.ToString(); 
                // (step + 1).ToString();
            }

        }



        public async Task<int> GetOrCreateGetConstantAdditionPayAdjustmentAsync(string name, int cmpId)
        {
            var _ = name.ToLower(); ;
            if (await context.PayAdjustments.AnyAsync(a => a.CompanyId == cmpId && a.Name.ToLower() == _))
                return await context.PayAdjustments.Where(a => a.CompanyId == cmpId && a.Name.ToLower() == _).Select(a => a.Id).FirstOrDefaultAsync();
            else
            {
                var _des = new PayAdjustment { Name = name, CompanyId = cmpId, VariationType = VariationType.ConstantAddition };
                context.PayAdjustments.Add(_des);
                await context.SaveChangesAsync();
                return _des.Id;
            }
        }        

        public async Task<int> GetOrCreateAndGetDepartmentIdAsync(string departmentName, int cmpId)
        {
            var _ = departmentName.ToLower(); ;
            if (await context.Departments.AnyAsync(a => a.CompanyId == cmpId && a.Name.ToLower() == _))
                return await context.Departments.Where(a => a.CompanyId == cmpId && a.Name.ToLower() == _).Select(a => a.Id).FirstOrDefaultAsync();
            else
            {
                var _des = new Department { Name = departmentName, CompanyId = cmpId };
                context.Departments.Add(_des);
                await context.SaveChangesAsync();
                return _des.Id;
            }
        }

        public async Task<int?> GetJobIdAsync(string _, int cmpId)
        {
            if (String.IsNullOrWhiteSpace(_))
                return (int?)null;
            //var _ = _.JobTitle; //.ToLower(); ;

            if (await context.Jobs.AnyAsync(a => a.CompanyId == cmpId && a.JobTitle == _))
                return await context.Jobs.Where(a => a.CompanyId == cmpId && a.JobTitle == _).Select(a => a.Id).FirstOrDefaultAsync();
            else
            {
                return (int ?)null;
                //var _des = new Job
                //{
                //    CompanyId = cmpId,
                //    LevelId = await GetOrCreateAndGetClassificationIdAsync(_.Classification, cmpId),
                //    LocationId = await GetOrCreateAndGetLocationIdAsync(_.Location, cmpId),
                //    DepartmentId = await GetOrCreateAndGetDepartmentIdAsync(_.Department, cmpId),
                //    ReportingJobId = await GetOrCreateAndGeJobIdAsync(_, cmpId),
                //    JobID = _.JobID,
                //    JobTitle = _.JobTitle,
                //    JobType = _.JobType,
                //});

                //await context.SaveChangesAsync();
                //return _des.Id;
            }
        }

        public async Task<int> GetOrCreateAndGetLocationIdAsync(string departmentName, int cmpId)
        {
            var _ = departmentName.ToLower(); ;
            if (await context.Locations.AnyAsync(a => a.CompanyId == cmpId && a.Name.ToLower() == _))
                return await context.Locations.Where(a => a.CompanyId == cmpId && a.Name.ToLower() == _).Select(a => a.Id).FirstOrDefaultAsync();
            else
            {
                var _des = new Location { Name = departmentName, CompanyId = cmpId };
                context.Locations.Add(_des);
                await context.SaveChangesAsync();
                return _des.Id;
            }
        }

        public async Task<int> GetOrCreateAndGetClassificationIdAsync(string departmentName, int? _cmpId = null)
        {
            var cmpId = _cmpId ?? userResolverService.GetCompanyId();
            var _ = departmentName.ToLower(); 
            if (await context.Levels.AnyAsync(a => a.CompanyId == cmpId && a.Name.ToLower() == _))
                return await context.Levels.Where(a => a.CompanyId == cmpId && a.Name.ToLower() == _).Select(a => a.Id).FirstOrDefaultAsync();
            else
            {
                var _des = new Level { Name = departmentName, CompanyId = cmpId };
                context.Levels.Add(_des);
                await context.SaveChangesAsync();
                return _des.Id;
            }
        }

        public async Task<int> GetOrCreateAndGetCountryAsync(string departmentName, int cmpId)
        {
            var _ = departmentName.ToLower(); ;
            if (await accDbContext.Countries.AnyAsync(a => a.Id == cmpId && a.Name.ToLower() == _))
                return await accDbContext.Countries.Where(a => a.Id == cmpId && a.Name.ToLower() == _).Select(a => a.Id).FirstOrDefaultAsync();
            else
            {
                var _des = new Country { Name = departmentName };
                accDbContext.Countries.Add(_des);
                await accDbContext.SaveChangesAsync();
                return _des.Id;
            }
        }


        public async Task<int?> GetOrCreateAndGetEmergencyContactRelationAsync(string departmentName, int cmpId)
        {
            if (string.IsNullOrWhiteSpace(departmentName))
                return (int?)null;
            var _ = departmentName?.ToLower() ?? "";
            if (await context.EmergencyContactRelationships.AnyAsync(a => (!a.CompanyId.HasValue || a.CompanyId.Value == cmpId) && a.Name.ToLower() == _))
                return await context.EmergencyContactRelationships.Where(a => (!a.CompanyId.HasValue || a.CompanyId.Value == cmpId) && a.Name.ToLower() == _).Select(a => a.Id).FirstOrDefaultAsync();
            else
            {
                var _des = new EmergencyContactRelationship { Name = departmentName, CompanyId = cmpId };
                context.EmergencyContactRelationships.Add(_des);
                await context.SaveChangesAsync();
                return _des.Id;
            }
        }

        public async Task<int> GetOrCreateAndGetStateAsync(string departmentName, int cmpId, string countryName = null)
        {
            var _ = departmentName.ToLower(); ;
            if (await accDbContext.States.AnyAsync(a => a.Id == cmpId && a.Name.ToLower() == _))
                return await accDbContext.States.Where(a => a.Id == cmpId && a.Name.ToLower() == _).Select(a => a.Id).FirstOrDefaultAsync();
            else
            {
                var _des = new State { Name = departmentName };
                if (!string.IsNullOrWhiteSpace(countryName))
                    _des.CountryId = await GetOrCreateAndGetCountryAsync(countryName, cmpId);

                accDbContext.States.Add(_des);
                await accDbContext.SaveChangesAsync();
                return _des.Id;
            }
        }

        public async Task<int> GetOrCreateAndGetNationalityAsync(string departmentName, int cmpId, string countryName = null)
        {
            var _ = departmentName.ToLower(); ;
            if (await context.Nationalities.AnyAsync(a => (!a.CompanyId.HasValue || a.CompanyId.Value == cmpId) && a.Name.ToLower() == _))
                return await context.Nationalities.Where(a => (!a.CompanyId.HasValue || a.CompanyId.Value == cmpId) && a.Name.ToLower() == _).Select(a => a.Id).FirstOrDefaultAsync();
            else
            {
                var _des = new Nationality { Name = departmentName, CompanyId = cmpId };

                context.Nationalities.Add(_des);
                await context.SaveChangesAsync();
                return _des.Id;
            }
        }


        public async Task<int> GetOrCreateAndGetCityAsync(string departmentName, int cmpId, string stateName)
        {
            var _ = departmentName.ToLower(); ;
            if (await accDbContext.Cities.AnyAsync(a => a.Id == cmpId && a.Name.ToLower() == _))
                return await accDbContext.Cities.Where(a => a.Id == cmpId && a.Name.ToLower() == _).Select(a => a.Id).FirstOrDefaultAsync();
            else
            {
                var _des = new City { Name = departmentName, StateId = await GetOrCreateAndGetStateAsync(stateName, cmpId) };
                accDbContext.Cities.Add(_des);
                await accDbContext.SaveChangesAsync();
                return _des.Id;
            }
        }

        public async Task<int?> GetReportingEmmployeeAsync(string EmpID, int cmpId)
        {
            if (await context.Employees.AnyAsync(a => a.CompanyId == cmpId && a.EmpID == EmpID))
                return await context.Employees.Where(a => a.CompanyId == cmpId && a.EmpID == EmpID).Select(a => a.Id).FirstOrDefaultAsync();
            else
            {
                return (int?)null;
            }
        }

        public async Task<CompanyAccount> GetCompanyCalendarSettings()
        {
            return await accDbContext   .CompanyAccounts
                .Where(x => x.Id == userResolverService.GetCompanyId())
                .Select(a=> new CompanyAccount
                {
                    Id = a.Id,
                    DayOfWeekHolidays = a.DayOfWeekHolidays,
                    PayrolPeriodDays = a.PayrolPeriodDays,
                    DayOfWeekOffDays = a.DayOfWeekOffDays,
                    WeekStartDay = a.WeekStartDay,
                })
                //.Take(limit)
                .FirstOrDefaultAsync();
        }

        public async Task<List<CompanyPublicHoliday>> GetUpComingPublicHolidays(int limit)
        {
            return await context.CompanyPublicHolidays
                .Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Date >= DateTime.Now.Date)
                .OrderBy(x => x.Date)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<CompanyPublicHoliday>> GetUpComingPublicHolidaysForYear(int year, int? month = null)
        {
            return await context.CompanyPublicHolidays
                .Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Date.Year == year && (!month.HasValue || month == x.Date.Month))
                .OrderBy(x => x.Date)
                .ToListAsync();
        }
        public async Task<List<CompanyPublicHoliday>> GetUpComingPublicHolidays(DateTime date)
        {
            return await context.CompanyPublicHolidays
                .Where(x => x.CompanyId == userResolverService.GetCompanyId() && x.Date == date)
                .OrderBy(x => x.Date)
                .ToListAsync();
        }
    }
}
