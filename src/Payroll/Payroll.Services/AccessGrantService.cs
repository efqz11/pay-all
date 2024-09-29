using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Payroll.Database;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Payroll.Services
{
    public class AccessGrantService
    {
        private readonly AccountDbContext context;
        private readonly PayrollDbContext payrolDbContext;
        private readonly UserResolverService userResolverService;
        private readonly SignInManager<AppUser> signInManager;

        public AccessGrantService(AccountDbContext context, PayrollDbContext payrolDbContext, UserResolverService userResolverService, SignInManager<AppUser> signInManager)
        {
            this.context = context;
            this.payrolDbContext = payrolDbContext;
            this.userResolverService = userResolverService;
            this.signInManager = signInManager;
        }
       
        public async Task<AccessGrant> GetFirstAccessGrantAsync(string userId)
        {
            return await context.AccessGrants
                .Include(x=> x.CompanyAccount).FirstOrDefaultAsync(x => x.UserId == userId);
        }
        public async Task<string[]> GetMyAccessGrantRolesAsync()
        {
            var roles = await context.AccessGrants.Where(x => x.CompanyAccountId == userResolverService.GetCompanyId() && x.UserId == userResolverService.GetUserId()).Select(a=> a.Roles).FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(roles))
                return roles.Split(",").ToArray();

            return default(string[]);
        }

        public async Task<List<PayAdjustment>> GetAllAccessiblePayAdjustmentsAsync(int page, int maxValue)
        {
            return await payrolDbContext.PayAdjustments
                .Where(x => x.CompanyId == userResolverService.GetCompanyId())
                   .Include(x => x.Fields)
                   .OrderByDescending(x => x.CalculationOrder > 0)
                   .ThenBy(x => x.CalculationOrder)
                   .ToListAsync();
        }

        public async Task<List<AccessGrant>> GetAllAccessGrantsAsync(string userId)
        {
            return await context.AccessGrants
                .Include(x => x.CompanyAccount).Where(x => x.UserId == userId).ToListAsync();
        }

        // change user access grant (to give company)
        public async Task ChangeUserAccessGrant(int id)
        {
            var userId = userResolverService.GetUserId();
            var user = context.Users.Find(userId);
            if (user == null) throw new ApplicationException("User not not found");
            if (!await context.AccessGrants.AnyAsync(x => x.Id == id && x.UserId == userId))
                throw new ApplicationException("Access Grant was not not found");

            var emp = await context.AccessGrants.
                Include(x => x.CompanyAccount)
                .Where(x => x.UserId == userId && x.Id == id)
                .FirstOrDefaultAsync();

            var reset = await context.AccessGrants.Where(x => x.UserId == userId && x.IsDefault == true).ToListAsync();
            reset.ForEach(a => a.IsDefault = false);
            emp.IsDefault = true;
            await context.SaveChangesAsync();

            // update claims
            await signInManager.RefreshSignInAsync(user);

            //ViewBag.User = user;
            return; // true();
        }

        /// <summary>
        /// Get all Company Accounts for admin user 
        /// TODO: (need to restrict for each company)
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<List<CompanyAccount>> GetAllAccessibleCompanyAccountsAsync(int page = 1, int limit = 10, string searchKey = null)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey?.ToLower();
                return await context.CompanyAccounts

                            .Where(x => x.Name.ToLower().Contains(searchKey) ||
                         (x.Hotline != null && x.Hotline.ToLower().Contains(searchKey)) ||
                         (x.ManagingDirector != null && x.ManagingDirector.ToString() == searchKey) ||
                         (x.Address != null && x.Address.Contains(searchKey)))

                         .OrderBy(x => x.Name)
                         .Skip((page - 1) * limit)
                         .Include(x=> x.AccessGrants)
                         .Take(limit)
                         .ToListAsync();
            }


            return await context.CompanyAccounts
                 .OrderBy(x => x.Name)
                 .Skip((page - 1) * limit)
                 .Take(limit)
                 .Include(x => x.AccessGrants)
                 .ToListAsync();
        }

        /// <summary>
        /// Create company (accounDb) and access grant for owner with 'admin' role
        /// </summary>
        /// <param name="usr"></param>
        /// <returns></returns>
        public async Task<CompanyAccount> CreateCompanyForUserAsync(AppUser usr)
        {
            var data = new CompanyAccount
            {
                Name = usr.CompanyName,
                AccessGrants = new List<AccessGrant> {
                new AccessGrant { IsActive = true, Roles = Roles.Company.administrator, UserId = userResolverService.GetUserId(), Status = AccessGrantStatus.Active } }
            };
            context.CompanyAccounts.Add(data);
            await context.SaveChangesAsync();
            return data;
        }

        public async Task<List<Company>> GetAllAccessibleCompanyIdsAsync(int page = 1, int limit = 10)
        {
            if(userResolverService.IsAdmin())
                return await payrolDbContext.Companies
                 .OrderBy(x => x.Name)
                 .Skip((page - 1) * limit)
                 .Take(limit)
                 .Include(x => x.Departments)
                 .ToListAsync();


            string userId = userResolverService.GetUserId();
            var allIds = await context.AccessGrants
                .Include(x => x.CompanyAccount).Where(x => x.UserId == userId && x.IsActive
                && x.Roles.Contains(Roles.Company.administrator))
                .Select(x=> x.CompanyAccountId)
                .ToListAsync();
            if(allIds.Count > 0)
            {
                return await payrolDbContext.Companies
                     .Where(x => allIds.Contains(x.Id))
                     .OrderBy(x => x.Name)
                     .Skip((page - 1) * limit)
                     .Take(limit)
                     .Include(x => x.Departments)
                     .ToListAsync();
            }

            return new List<Company>();
        }

        public async Task<AccessGrant> GetContractEmployeeAccessAsync(string UserId)
        {
            return await context.AccessGrants
                .Include(a=> a.User)
                .FirstOrDefaultAsync(a => a.UserId == UserId);
        }

        public async Task<List<PayrollPeriod>> GetAllAccessiblePayrollsAsync(bool overrideCompanyIfAdmin =  false, int page = 1, int limit = 10)
        {
            if (userResolverService.IsAdmin() && overrideCompanyIfAdmin)
                return await payrolDbContext.PayrollPeriods
                 .OrderBy(x => x.Name)
                 .Skip((page - 1) * limit)
                 .Take(limit)
                 .Include(x => x.PayrollPeriodEmployees)
                 .ToListAsync();


            int companyId = userResolverService.GetCompanyId();
            return await payrolDbContext.PayrollPeriods
                 .Where(x => x.CompanyId == companyId)
                 .OrderBy(x => x.Name)
                 .Skip((page - 1) * limit)
                 .Take(limit)
                 .Include(x => x.PayrollPeriodEmployees)
                 .ToListAsync();
        }

        //internal async Task<(bool status, string error)> CheckConnectionAsync(string connectionString)
        //{
        //    (bool status, string error) obj = (false, "");
        //    var myConn = context.Database.GetDbConnection().ConnectionString;
        //    try
        //    {

        //        using (var ctx = payrolDbContextFactory.CreateApplicationDbContext(connectionString))
        //        {
        //            obj.status = ctx.Database.CanConnect();
        //        }

        //        ChangeConnectionAsync(connectionString).Wait();

        //        //    var optionsBuilder = new DbContextOptionsBuilder();
        //        //    optionsBuilder.UseSqlServer(connectionString);
        //        //    var _ = new PayrolDbContext(optionsBuilder, userResolverService);

        //        //await _.Database.OpenConnectionAsync();
        //        //obj.status = true;
        //        //await context.Database.OpenConnectionAsync();
        //    }
        //    catch (Exception e) { obj.error = e.Message; }
        //    finally
        //    {
        //        ChangeConnectionAsync(myConn).Wait();
        //    }

        //    return obj;
        //}


        public async Task<List<Employee>> GetEmployeesOfCurremtCompany(bool addEmptyOption = false, int? deptId = null)
        {
            var currCompanyId = userResolverService.GetCompanyId();
            var items = await payrolDbContext.Departments.Where(x => x.CompanyId == currCompanyId && x.Employees.Any() && (deptId == null || deptId == x.Id))
                .SelectMany(x => x.Employees)
                .OrderBy(x=> x.Department.DisplayOrder)
                .ThenBy(x=> x.EmpID)
                .ToListAsync();

            if (addEmptyOption)
            {
                var viewAll = new Employee { Id = 0, Name = "All Employees" };
                items.Insert(0, viewAll);
            }

            return items;
        }

        public async Task<List<Department>> GetDepartmentsOfCurremtCompany(bool addEmptyOption = false, string emptyOptionLabel = null)
        {
            var currCompanyId = userResolverService.GetCompanyId();
            var items = await payrolDbContext.Departments.Where(x => x.CompanyId == currCompanyId)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            if (addEmptyOption)
            {
                var viewAll = new Department { Id = 0, Name = emptyOptionLabel ?? "All Departments" };
                items.Insert(0, viewAll);
            }

            return items;
        }

        public async Task<List<Industry>> GetIndustries()
        {
            return await context.Industries.ToListAsync();
        }


        private async Task ChangeConnectionAsync(string connectionString)
        {
            context.Database.CloseConnection();
            context.Database.GetDbConnection().ConnectionString = connectionString;

            context.Database.OpenConnection();
        }

        /// <summary>
        /// Emp "Employee ID"
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetSelectFieldDisplayNameEmployee()
        {
            return typeof(Employee).GetProperties()
                               .Where(x => Attribute.IsDefined(x, typeof(Filters.SelectableFieldAttribute)))
                               .ToDictionary(x=> x.Name, x=> x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name);
        }
        //public Dictionary<string, string>  GetSelectFieldNameIndividual()
        //{
        //    return typeof(Individual).GetProperties()
        //                       .Where(x => Attribute.IsDefined(x, typeof(Filters.SelectableFieldAttribute)))
        //                       .ToDictionary(x => x.Name, x => x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name);
        //}


        /// <summary>
        /// Key Value
        /// FirstName "First Name"
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetFieldNameDisplayNameDictFromType(Type type)
        {
            return type.GetProperties()
                               .Where(x => Attribute.IsDefined(x, typeof(Filters.SelectableFieldAttribute)))
                               .ToDictionary(x => x.Name, x => x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name);
        }
    }

}
