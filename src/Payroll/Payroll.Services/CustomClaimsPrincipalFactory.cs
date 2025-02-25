using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Payroll.Database;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Payroll.Services
{
    public class CustomClaimsPrincipalFactory :
      UserClaimsPrincipalFactory<AppUser,
      AppRole>
    {
        private readonly AccountDbContext accountDbContext;
        private readonly PayrollDbContext payrolDbContext;
        private readonly EmployeeService employeeService;

        public CustomClaimsPrincipalFactory(
           UserManager<AppUser> userManager,
           RoleManager<AppRole> roleManager,
           IOptions<IdentityOptions> optionsAccessor,
           AccountDbContext accountDbContext,
           PayrollDbContext payrolDbContext,
           EmployeeService employeeService) :
              base(userManager, roleManager, optionsAccessor)
        {
            this.accountDbContext = accountDbContext;
            this.payrolDbContext = payrolDbContext;
            this.employeeService = employeeService;
        }

        public async override Task<ClaimsPrincipal>
           CreateAsync(AppUser user)
        {
            var principal = await base.CreateAsync(user);

            CompanyAccount companyAccount = null;
            Employee emp = null;

            // check if temp switch is Enabled
            if(user.IsTempSwitching.HasValue && user.IsTempSwitching.Value && user.UserType == UserType.Employee)
            {
                emp = await payrolDbContext.Employees
                        .Where(x => x.HasUserAccount && x.UserId == user.Id) // userId <= is Employee User

                        //.Select(x => new EmpForClaimsVm
                        //{
                        //    companyId = x.CompanyId,
                        //    companyLogo = x.Company.LogoUrl,
                        //    companyName = x.Company.Name,
                        //    employeeId = x.Id,
                        //    Avatar = x.Individual.Avatar,
                        //    employeeName = x.Individual.GetSystemName(principal),
                        //    nickName = x.NickName,
                        //    departmentId = x.DepartmentId,
                        //    departmentName = x.Department.Name,
                        //    JobTitle = x.JobTitle
                        //})
                        .FirstOrDefaultAsync();
                companyAccount = await accountDbContext.CompanyAccounts.FindAsync(emp.CompanyId);



                ((ClaimsIdentity)principal.Identity).
                   AddClaims(new[] {
                 //new Claim(CustomClaimTypes.EmployeeId, emp.employeeId.ToString()),
                 //new Claim(CustomClaimTypes.EmployeeName, emp.employeeName.ToString()),
                 //new Claim(CustomClaimTypes.DepartmentId, emp.departmentId?.ToString()),
                 //new Claim(CustomClaimTypes.DepartmentName, emp.departmentName?.ToString()),
                 //new Claim(CustomClaimTypes.JobTitle, emp.JobTitle?.ToString()),

                 //new Claim(CustomClaimTypes.Profile, !string.IsNullOrWhiteSpace(emp.Avatar) ? emp.Avatar : DefaultPictures.default_user),
                 //new Claim(CustomClaimTypes.Nickname, emp.nickName ?? emp.employeeName),
                 new Claim(CustomClaimTypes.EmployeeTempSwitch, "1"),
                 new Claim(CustomClaimTypes.EmployeeTempSwitchOriginalUserId, user.TempSwitchingOriginalUserId),
                 new Claim(CustomClaimTypes.EmployeeTempSwitchOriginalUserName, user.TempSwitchingOriginalUserName),
                });


                // get Employee Roles and add
                var rolesDict = await employeeService.GetEmployeeByRoles(emp.Id);
                if (rolesDict.Any())
                    foreach (var item in rolesDict)
                        ((ClaimsIdentity)principal.Identity).
                   AddClaims(new[] {
                 new Claim(ClaimTypes.Role, item.Value.ToString()),
                 new Claim(CustomClaimTypes.EmployeeRole, item.Value.ToString()),
                 new Claim(CustomClaimTypes.EmployeeRoleId, item.Key.ToString()),
                });

            }
            else
            {
                // normal login for user
                companyAccount = await accountDbContext.AccessGrants
                    .Where(x => x.UserId == user.Id && x.IsDefault)
                    .Select(x => x.CompanyAccount).FirstOrDefaultAsync();

                ((ClaimsIdentity)principal.Identity).
                    AddClaims(new[] {
                         new Claim(CustomClaimTypes.Profile, !string.IsNullOrWhiteSpace(user.Avatar) ? user.Avatar : DefaultPictures.default_user),
                         new Claim(CustomClaimTypes.UserType, UserType.PayAll.ToString()),
                         new Claim(CustomClaimTypes.Nickname, !string.IsNullOrWhiteSpace(user.NickName) ? user.NickName : user.UserName)
                    });

                // Roles will be automatically added
            }


            if(companyAccount != null)
            {
                ((ClaimsIdentity)principal.Identity).
                  AddClaims(new[] {
                 new Claim(CustomClaimTypes.UserType, user.UserType.ToString()),
                 new Claim(CustomClaimTypes.accessgrant_companyName, companyAccount.Name),
                 new Claim(CustomClaimTypes.accessgrant_companyId, companyAccount.Id.ToString()),
                 new Claim(CustomClaimTypes.accessgrant_companyTimeZone, companyAccount?.TimeZone ?? "UTC"),

                 new Claim(CustomClaimTypes.accessgrant_companyLogo, !string.IsNullOrWhiteSpace(companyAccount.LogoUrl) ? companyAccount.LogoUrl : DefaultPictures.default_company),



                    new Claim(CustomClaimTypes.formatter_date, companyAccount?.DateFormat ?? "dd-MMM-yyyy"),
                    new Claim(CustomClaimTypes.formatter_time, companyAccount?.TimeFormat ?? "hh:mm:ss"),
                    new Claim(CustomClaimTypes.formatter_number, companyAccount?.CurrencyFormat ?? "N2"),
                    new Claim(CustomClaimTypes.formatter_name, companyAccount?.NameFormat ?? "{FirstName} {lastName}"),
                      //new Claim(ClaimTypes.GivenName, user.NickName.ToString())
                  });

            }

            if(user.IsTempSwitching.HasValue && user.IsTempSwitching.Value && user.UserType == UserType.Employee && emp !=null)
            {
                ((ClaimsIdentity)principal.Identity).
                   AddClaims(new[] {
                 new Claim(CustomClaimTypes.EmployeeId, emp.Id.ToString()),
                 new Claim(CustomClaimTypes.EmployeeName, emp.GetSystemName(principal)),
                 new Claim(CustomClaimTypes.DepartmentId, emp.DepartmentId?.ToString()),
                 //new Claim(CustomClaimTypes.DepartmentName, emp.Department.Name?.ToString()),
                 new Claim(CustomClaimTypes.JobTitle, emp.JobTitle?.ToString()),

                 new Claim(CustomClaimTypes.Profile, !string.IsNullOrWhiteSpace(emp.Avatar) ? emp.Avatar : DefaultPictures.default_user),
                 new Claim(CustomClaimTypes.Nickname, emp.NickName ?? emp.GetSystemName(principal)),

                });

            }

            //// access grants for company access
            //var userAccessGrant = await accountDbContext.AccessGrants
            //    .Include(x => x.CompanyAccount).FirstOrDefaultAsync(x => x.UserId == user.Id && x.IsDefault);
            //if (userAccessGrant != null)
            //{
            //    ((ClaimsIdentity)principal.Identity).
            //       AddClaims(new[] {
            //     new Claim(CustomClaimTypes.accessgrant_companyName, userAccessGrant.CompanyAccount.Name),
            //     new Claim(CustomClaimTypes.accessgrant_companyId, userAccessGrant.CompanyAccountId.ToString()),
            //           //new Claim(ClaimTypes.GivenName, user.NickName.ToString())
            //       });

            //    ((ClaimsIdentity)principal.Identity).AddClaims(new[] {
            //            new Claim(CustomClaimTypes.accessgrant_companyTimeZone, userAccessGrant.CompanyAccount?.TimeZone ?? "UTC"),
            //    });

            //    ((ClaimsIdentity)principal.Identity).AddClaims(new[] {
            //            new Claim(CustomClaimTypes.accessgrant_companyLogo, !string.IsNullOrWhiteSpace(userAccessGrant.CompanyAccount.LogoUrl) ? userAccessGrant.CompanyAccount.LogoUrl : DefaultPictures.default_company),
            //           });

            //    if (!string.IsNullOrWhiteSpace(userAccessGrant.Roles))
            //        ((ClaimsIdentity)principal.Identity).AddClaims(new[] {
            //            new Claim(CustomClaimTypes.accessgrant_roles, userAccessGrant.Roles),
            //           });


            //    // add formatters
            //    ((ClaimsIdentity)principal.Identity).AddClaims(new[] {
            //            new Claim(CustomClaimTypes.formatter_date, userAccessGrant.CompanyAccount?.DateFormat ?? "dd-MMM-yyyy"),
            //    });
            //    ((ClaimsIdentity)principal.Identity).AddClaims(new[] {
            //            new Claim(CustomClaimTypes.formatter_time, userAccessGrant.CompanyAccount?.TimeFormat ?? "hh:mm:ss"),
            //    });
            //    ((ClaimsIdentity)principal.Identity).AddClaims(new[] {
            //            new Claim(CustomClaimTypes.formatter_number, userAccessGrant.CompanyAccount?.CurrencyFormat ?? "N2"),
            //    });
            //    ((ClaimsIdentity)principal.Identity).AddClaims(new[] {
            //            new Claim(CustomClaimTypes.formatter_name, userAccessGrant.CompanyAccount?.NameFormat ?? "{FirstName} {lastName}"),
            //    });
            //}


            //var emp = 
            //    user.TempSwitchingEmployeeId.HasValue ?
            //    await payrolDbContext.Employees
            //            .Where(x => x.Id == user.TempSwitchingEmployeeId)
            //            .Select(x => new EmpForClaimsVm
            //            {
            //                companyLogo = x.Company.LogoUrl,
            //                companyName = x.Company.Name,
            //                employeeId = x.Id,
            //                Avatar = x.Individual.Avatar,
            //                employeeName = x.Individual.GetSystemName(principal),
            //                nickName = x.NickName,
            //                departmentId = x.DepartmentId,
            //                departmentName = x.Department.Name
            //            })
            //            .FirstOrDefaultAsync()
            //            :
            //            await payrolDbContext.Employees
            //               .Where(x => x.HasUserAccount && x.UserId == user.Id
            //               && (userAccessGrant == null || userAccessGrant.CompanyAccountId == x.Department.CompanyId))
            //               .Select(x => new EmpForClaimsVm
            //               {
            //                   companyLogo = x.Company.LogoUrl,
            //                   companyName = x.Company.Name,
            //                   nickName = x.NickName,
            //                   employeeId = x.Id,
            //                   employeeName = x.Individual.GetSystemName(principal),
            //                   departmentId = x.DepartmentId,
            //                   departmentName = x.Department.Name
            //               })
            //               .FirstOrDefaultAsync();


            //if (emp != null)
            //{
            //    var rolesDict = await employeeService.GetEmployeeByRoles(emp.employeeId);
            //    var _unionRoutes = await employeeService.GetEmployeeByRoleRoutes(emp.employeeId);
            //    //rolesDict.Add(0, Roles.Company.all_employees.ToString());
            //    //if(rolesDict == null)

            //    ((ClaimsIdentity)principal.Identity).
            //       AddClaims(new[] {
            //     new Claim(CustomClaimTypes.EmployeeId, emp.employeeId.ToString()),
            //     new Claim(CustomClaimTypes.EmployeeName, emp.employeeName.ToString()),
            //     new Claim(CustomClaimTypes.DepartmentId, emp.departmentId?.ToString()),
            //     new Claim(CustomClaimTypes.DepartmentName, emp.departmentName?.ToString()),
            //     new Claim(CustomClaimTypes.EmployeeCompanyLogo, emp.companyLogo?.ToString() ?? DefaultPictures.default_company),
            //     new Claim(CustomClaimTypes.EmployeeCompanyName, emp.companyName?.ToString()),

            //     new Claim(CustomClaimTypes.Profile, !string.IsNullOrWhiteSpace(emp.Avatar) ? emp.Avatar : DefaultPictures.default_user),
            //     new Claim(CustomClaimTypes.UserType, UserType.Employee.ToString()),
            //     new Claim(CustomClaimTypes.Nickname, emp.nickName ?? emp.nickName),
            //     new Claim(CustomClaimTypes.EmployeeTempSwitch, "1"),
            //    });


            //    if (_unionRoutes.Any())
            //        foreach (var item in rolesDict)
            //            ((ClaimsIdentity)principal.Identity).
            //       AddClaims(new[] {
            //     new Claim(ClaimTypes.Role, item.Value.ToString()),
            //     new Claim(CustomClaimTypes.EmployeeRoleId, item.Key.ToString()),
            //    });


            //    //((ClaimsIdentity)principal.Identity).Claims.
            //    if (_unionRoutes.Any())
            //        foreach (var item in _unionRoutes)
            //        {
            //            if (item.ContainsKey("scope") && item["scope"].Any())
            //                foreach (var sc in item["scope"])
            //                    ((ClaimsIdentity)principal.Identity).AddClaims(new[] { new Claim(CustomClaimTypes.EmployeeRoleScope, sc) });
            //            if (item.ContainsKey("role") && item["role"].Any())
            //                foreach (var sc in item["role"])
            //                    ((ClaimsIdentity)principal.Identity).AddClaims(new[] { new Claim(ClaimTypes.Role, sc) });
            //        }

            //}

            ////if (user.TempSwitchingEmployeeId.HasValue)
            ////{

            ////    // switch to custom Employee
            ////    var emp = await payrolDbContext.Employees
            ////            .Where(x => x.Id == user.TempSwitchingEmployeeId)
            ////            .Select(x => new
            ////            {
            ////                companyLogo = x.Company.LogoUrl,
            ////                companyName = x.Company.Name,
            ////                employeeId = x.Id,
            ////                Avatar = x.Individual.Avatar,
            ////                employeeName = x.GetSystemName(principal),
            ////                departmentId = x.DepartmentId,
            ////                departmentName = x.Department.Name
            ////            })
            ////            .FirstOrDefaultAsync();


            ////}
            ////else
            ////{

            ////    // add employee access grants
            ////    // (if user access NOT NULL, then get employee belonging to current company access grant)
            ////    var emp = await payrolDbContext.Employees
            ////            .Where(x => x.HasUserAccount && x.UserId == user.Id
            ////            && (userAccessGrant == null || userAccessGrant.CompanyAccountId == x.Department.CompanyId))
            ////            .Select(x => new
            ////            {
            ////                companyLogo = x.Company.LogoUrl,
            ////                companyName = x.Company.Name,
            ////                employeeId = x.Id,
            ////                employeeName = x.GetSystemName(principal),
            ////                departmentId = x.DepartmentId,
            ////                departmentName = x.Department.Name
            ////            })
            ////            .FirstOrDefaultAsync();

            ////    if (emp != null)
            ////    {
            ////        ((ClaimsIdentity)principal.Identity).
            ////           AddClaims(new[] {
            ////     new Claim(CustomClaimTypes.EmployeeId, emp.employeeId.ToString()),
            ////     new Claim(CustomClaimTypes.EmployeeName, emp.employeeName.ToString()),
            ////     new Claim(CustomClaimTypes.DepartmentId, emp.departmentId.ToString()),
            ////     new Claim(CustomClaimTypes.DepartmentName, emp.departmentName.ToString()),
            ////     new Claim(CustomClaimTypes.EmployeeCompanyLogo, emp.companyLogo?.ToString()?? DefaultPictures.default_company),
            ////     new Claim(CustomClaimTypes.EmployeeCompanyName, emp.companyName.ToString()),
            ////        });
            ////    }
            ////}
            //else
            //{

            //    ((ClaimsIdentity)principal.Identity).
            //   AddClaims(new[] {
            //     new Claim(CustomClaimTypes.Profile, !string.IsNullOrWhiteSpace(user.Avatar) ? user.Avatar : DefaultPictures.default_user),
            //     new Claim(CustomClaimTypes.UserType, UserType.PayAll.ToString()),
            //   });


            //    if (!string.IsNullOrWhiteSpace(user.NickName))
            //        ((ClaimsIdentity)principal.Identity).
            //       AddClaims(new[] {
            //     new Claim(CustomClaimTypes.Nickname, user.NickName),
            //    });
            //}

            return principal;
        }
    }

    internal class EmpForClaimsVm
    {
        public string companyLogo { get; set; }
        public string companyName { get; set; }
        public int employeeId { get; set; }
        public string Avatar { get; set; }
        public string employeeName { get; set; }
        public int? departmentId { get; set; }
        public string departmentName { get; set; }
        public string nickName { get; internal set; }
        public int companyId { get; internal set; }
        public string JobTitle { get; internal set; }
    }
}
