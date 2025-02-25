using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Payroll.Database;
using Payroll.Filters;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;

namespace Payroll.Controllers
{
    public class AppUserController : BaseController
    {
        private readonly AccountDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<AppRole> roleManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ILogger<AppUserController> logger;
        private readonly UserResolverService _userResolverService;
        private readonly CompanyService companyService;
        private readonly PayrollDbContext payrollDbContext;
        private readonly IUserStore<AppUser> _userStore;
        private readonly AccessGrantService accessGrantService;

        public AppUserController(AccountDbContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> _signInManager, ILogger<AppUserController> logger, UserResolverService userResolverService, CompanyService companyService, PayrollDbContext payrollDbContext, IUserStore<AppUser> _userStore, AccessGrantService accessGrantService)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            signInManager = _signInManager;
            this.logger = logger;
            this._userResolverService = userResolverService;
            this._userStore = _userStore;
            this.accessGrantService = accessGrantService;
            this.companyService = companyService;
            this.payrollDbContext = payrollDbContext;
        }

        [ActionPermissions(Permissions.LIST)]
        public async Task<IActionResult> Index(int page = 1, int limit = 10, int cmpId = 0, string role = "", string search = "", ContractSort? sort = null)
        {
            var empQuery = context.AppUsers
                .Where(a => cmpId == 0 || a.AccessGrants.Any(z => (z.CompanyAccountId == cmpId)));
            ViewBag.CmpIds = new SelectList(await companyService.GetAllCompanies(), "Id", "Name", cmpId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                empQuery = empQuery.Where(x => x.UserName.ToLower().Contains(search) ||
                 (x.NickName != null && x.NickName.ToLower().Contains(search)) ||
                 (x.PhoneNumber != null && x.PhoneNumber.ToString() == search) ||
                 (x.Email != null && x.Email.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                empQuery = empQuery.Where(u => u.AppUserRoles.Any(r => role == "" || role == r.AppRole.Name));
            }

            var watch = new Stopwatch();
            watch.Start();

            var emp = await empQuery.OrderBy(x => x.UserName)
             .Skip((page - 1) * limit)
             .Take(limit)
             .Include(x => x.AppUserLogins)
             .Include(x => x.AccessGrants)
             .Include(x => x.AppUserRoles)
                .ThenInclude(ur => ur.AppRole)
             .ToListAsync();

            ViewBag.cmpId = cmpId;
            ViewBag.limit = limit;
            //ViewBag.type = (int)(type ?? 0);
            //ViewBag.desgId = desgId;
            ViewBag.sort = sort;
            ViewBag.role = role;

            if (Request.IsAjaxRequest())
            {
                watch.Stop();
                return PartialView("_Listing", emp);
            } 
            else
            {
                ViewBag.Count = await empQuery.CountAsync();
                watch.Stop();
                ViewBag.TimeSec = watch.Elapsed.TotalSeconds.ToString("N2");

                ViewBag.Roles = new SelectList(typeof(Roles)
                                .GetFields(BindingFlags.Public | BindingFlags.Static)
                                .Where(f => f.FieldType == typeof(string))
                                .Select(f=> (string)f.GetValue(null))
                                .ToArray(), role);
            }
            
            return View(emp);
        }

        [HttpPost]
        [ActionPermissions(Permissions.SEARCH)]
        public async Task<IActionResult> Search(string query)
        {
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
                return BadRequest();
            
            query = query.ToLower();
            var emp = userManager.Users
             .Where(x => x.UserName.ToLower().Contains(query) ||
             (x.NickName != null && x.NickName.ToLower().Contains(query)) ||
             (x.PhoneNumber != null && x.PhoneNumber.ToString() == query) ||
             (x.Email != null && x.Email.Contains(query)))
             .OrderBy(x => x.UserName)
             .Take(11)
             .Include(x => x.AccessGrants)
             .Include(x => x.AppUserLogins)
             .Include(x => x.AppUserRoles)
                .ThenInclude(ur => ur.AppRole)
             .ToList();


            return PartialView("_Listing", emp);
        }


        [ActionPermissions(Permissions.READ)]
        public async Task<IActionResult> Detail(string id)
        {
            ViewBag.RolesDict = await context.Roles.ToDictionaryAsync(a => a.Id, a => a.Name);
            var emp = await userManager.Users
                .Include(x => x.AppUserRoles)
                .Include(x => x.AppUserLogins)
                .Include(x => x.AccessGrants)
                    .ThenInclude(ur => ur.CompanyAccount)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (emp == null) return BadRequest("User not found");

            var userRoles = emp.GetUserRoles();


            if (Request.IsAjaxRequest())
                return PartialView("_Detail", emp);

            var usr = await userManager.GetUserAsync(User);
            var CurrentLogins = await userManager.GetLoginsAsync(usr);
            emp.CurrentLogins = CurrentLogins;
            emp.OtherLogins = (await signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            string passwordHash = null;
            if (_userStore is IUserPasswordStore<AppUser> userPasswordStore)
            {
                passwordHash = await userPasswordStore.GetPasswordHashAsync(emp, HttpContext.RequestAborted);
            }

            emp.ShowRemoveButton = passwordHash != null || CurrentLogins.Count > 1;

            ViewBag.AccessGrantRoles = await context.AccessGrantRoles.ToListAsync();
            ViewBag.Employess = await payrollDbContext.Employees.Where(a => a.HasUserAccount && a.UserId == id)
                .Include(d => d.Department).ToListAsync();

            return View(emp);
        }


        [HttpPost]
        public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            var result = await userManager.RemoveLoginAsync(user, loginProvider, providerKey);
            if (!result.Succeeded)
            {
                var userId = await userManager.GetUserIdAsync(user);
                throw new InvalidOperationException($"Unexpected error occurred removing external login for user with ID '{userId}'.");
            }

            await signInManager.RefreshSignInAsync(user);
            SetTempDataMessage( "The external login was removed.", MsgAlertType.success);
            return Ok();
        }

        [ActionPermissions(Permissions.CREATE)]
        public async Task<IActionResult> AddOrUpdate(string id = "")
        {
            var emp = await userManager.Users.Include(x=> x.AppUserRoles).FirstOrDefaultAsync(x=> x.Id ==id);
            if (emp == null && id != "") return BadRequest("User not found");
            if (emp == null) emp = new AppUser { Id = "" };


            //await EnsureUserIsAuthorizedAsync(_userResolverService.GetUserId())
            return PartialView("_AddOrUpdate", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionPermissions(Permissions.CREATE)]
        public async Task<IActionResult> AddOrUpdate(AppUser model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(model.PasswordHash) && model.Id == "Model.Id")
                    return ThrowJsonError("Please enter password");
            }
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(model.Id) || model.Id == "Model.Id")
                {
                    model.Id = Guid.NewGuid().ToString();
                    model.SecurityStamp = await userManager.GetSecurityStampAsync(model);
                    var result = await userManager.CreateAsync(model, model.PasswordHash);
                    if(result.Succeeded)
                        return await Index();
                    else
                    {
                        return ThrowJsonError(result.Errors);
                    }
                }
                else
                {
                    //await userManager.UpdateSecurityStampAsync(model);
                    var user = await userManager.FindByIdAsync(model.Id);
                    user.NickName = model.NickName;
                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.LockoutEnabled = model.LockoutEnabled;
                    user.TwoFactorEnabled = model.TwoFactorEnabled;
                    user.ChangePasswordOnLogin = model.ChangePasswordOnLogin;
                    user.SendOtpAndLoginFirst = model.SendOtpAndLoginFirst;

                    await userManager.UpdateAsync(user);

                    return RedirectToAction(nameof(Detail), new { id = model.Id });
                }
                
            }

            return ThrowJsonError("Please enter all the fields");
        }

        [HttpPost]

        [ActionPermissions(Permissions.SWITCH)]
        public async Task<IActionResult> UpdateRoles(int id, string roles)
        {
            var accessGrant = await context.AccessGrants.FirstOrDefaultAsync(a => a.Id == id);
            if (accessGrant == null)
                return BadRequest("ERR");

            if(!User.IsInRole(Roles.PayAll.admin) && accessGrant.CompanyAccountId != _userResolverService.GetCompanyId())
                return BadRequest("ERR");

            if (!string.IsNullOrEmpty(roles))
            {
                accessGrant.Roles = roles;
                await context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Failed");
        }


        public async Task<IActionResult> AddOrUpdateOther(string id)
        {
            var emp = await userManager.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == id);
            if (emp == null && id != "") return BadRequest("User not found");
            if (emp == null) emp = new AppUser { Id = "" };

            return PartialView("_AddOrUpdateOther", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateOther(AppUser model)
        {
            var userX = await userManager.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (userX == null)
                return ThrowJsonError("User was not found");

            userX.NickName = model.NickName ?? userX.NickName;

            if (model.Roles.Length > 0)
            {
                var userCurretnRoles = await userManager.GetRolesAsync(userX);
                var removeRolesResult = await userManager.RemoveFromRolesAsync(userX, userCurretnRoles);

                if (removeRolesResult.Succeeded)
                {
                    await userManager.AddToRolesAsync(userX, model.Roles.Split(',').ToArray());
                    return await Index();
                }
            }

            return ThrowJsonError("Please enter all the fields");
        }

        public async Task<IActionResult> AddOrUpdatePayAllRoles(string id)
        {
            var emp = await userManager.Users.Include(x => x.AppUserRoles).FirstOrDefaultAsync(x => x.Id == id);
            if (emp.UserType == UserType.Company) return ThrowJsonError("Roles can be modified for PayAll users only!");
            ViewBag.Roles = await roleManager.Roles.ToListAsync();

            if (emp == null && id != "") return ThrowJsonError("User not found");
            if (emp == null) emp = new AppUser { Id = "" };

            return PartialView("_AddOrUpdatePayAllRoles", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdatePayAllRoles(AppUser model)
        {
            var userX = await userManager.Users.Include(x => x.AppUserRoles).ThenInclude(x => x.AppRole).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (userX == null)
                return ThrowJsonError("User was not found");

            var roles = userX.AppUserRoles.Select(a => a.AppRole.Name).ToArray();
            await userManager.RemoveFromRolesAsync(userX, roles);

            var _roles = model.Roles.Split(',').ToArray();
            if (_roles.Length > 0)
                await userManager.AddToRolesAsync(userX, _roles);

            return RedirectToAction(nameof(Detail), new { id = model.Id });
        }
        //public IActionResult ChangeUserAccessGrant(string id)
        //{
        //    var emp = context.AccessGrants.
        //        Include(x => x.Company)
        //        .Where(x => x.UserId == id)
        //        .ToList();

        //    if (emp == null || emp.Count <= 0) return ThrowJsonError("Access Grant was not not found");

        //    var user = context.Users.Find(id);
        //    if (user == null) return ThrowJsonError("User not not found");

        //    ViewBag.User = user;
        //    ViewBag.IsChangeUserAccessGrant = true;
        //    if (emp == null) emp = new List<AccessGrant>();
        //    return PartialView("_AccessGrants", emp);
        //}

        /// <summary>
        /// Update access grant aka change company
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ChangeUserAccessGrant(int id, string userId)
        {
            try
            {
                ViewBag.User = _userResolverService.GetUser();
                await accessGrantService.ChangeUserAccessGrant(id);
                return ThrowJsonSuccess();
            }
            catch (Exception ex)
            {
                return ThrowJsonError(ex.Message);
            }
            //var user = context.Users.Find(userId);
            //if (user == null) return ThrowJsonError("User not not found");
            //if (!await context.AccessGrants.AnyAsync(x=> x.Id == id && x.UserId == userId))
            //    return ThrowJsonError("Access Grant was not not found");

            //var emp = await context.AccessGrants.
            //    Include(x => x.CompanyAccount)
            //    .Where(x => x.UserId == userId && x.Id == id)
            //    .FirstOrDefaultAsync();

            //var reset = await context.AccessGrants.Where(x => x.UserId == userId && x.IsDefault == true).ToListAsync();
            //reset.ForEach(a => a.IsDefault = false);
            //emp.IsDefault = true;
            //await context.SaveChangesAsync();

            //// update claims
            //await signInManager.RefreshSignInAsync(user);

            //ViewBag.User = user;
            //return ThrowJsonSuccess();
        }


        [HttpPost]
        [ActionPermissions(Permissions.SWITCH)]
        public async Task<IActionResult> SwitchToEmployee(int id)
        {
            var rmpl = payrollDbContext.Employees.Find(id);
            if (rmpl == null) return ThrowJsonError("Employee was not not found");

            // create or get user for the employee
            AppUser user = null;
            if (!rmpl.HasUserAccount)
            {
                var _username = rmpl.EmailWork.Trim();
                user = new AppUser { UserName = _username, Email = _username, UserType = UserType.Employee };
                var result = await userManager.CreateAsync(user, rmpl.EmailWork);
                if (result.Succeeded)
                {
                    rmpl.UserId = user.Id;
                    rmpl.HasUserAccount = true;
                    await payrollDbContext.SaveChangesAsync();
                }
                else
                    return ThrowJsonError("Error! " + result.Errors.First().Description);
            }
            else
            {
                user = await context.Users.FindAsync(rmpl.UserId);
            }


            if (_userResolverService.GetUserId() == user.Id) return ThrowJsonError("Can't switch to same user. thanks!");
            //var user = await context.Users.FindAsync(_userResolverService.GetUserId());
            //TempData["SwitchToEmployeeId"] = _userResolverService.GetUserId();
            //TempData["SwitchToEmployeeName"] = User.Identity.Name;
            //TempData["SwitchToEmployeeName"] = User.Identity.Name;
            user.IsTempSwitching = true;
            user.TempSwitchingOriginalUserId = _userResolverService.GetUserId();
            user.TempSwitchingOriginalUserName = User.Identity.Name;

            //if (!await context.AccessGrants.AnyAsync(x => x.Id == id && x.UserId == userId))
            //    return ThrowJsonError("Access Grant was not not found");

            //var emp = await context.AccessGrants.
            //    Include(x => x.CompanyAccount)
            //    .Where(x => x.UserId == userId && x.Id == id)
            //    .FirstOrDefaultAsync();

            //var reset = await context.AccessGrants.Where(x => x.UserId == userId && x.IsDefault == true).ToListAsync();
            //reset.ForEach(a => a.IsDefault = false);
            //emp.IsDefault = true;
            //await context.SaveChangesAsync();

            // update claims
            await signInManager.RefreshSignInAsync(user);

            ViewBag.User = rmpl;
            return ThrowJsonSuccess();
        }


        [HttpPost]
        [ActionPermissions(Permissions.SWITCH)]
        public async Task<IActionResult> SwitchBack()
        {
            var tempStichVal = _userResolverService.Get(CustomClaimTypes.EmployeeTempSwitch);
            if (tempStichVal== "1")
            {
                var originalUserId = _userResolverService.Get(CustomClaimTypes.EmployeeTempSwitchOriginalUserId);
                var user = await context.Users.FindAsync(originalUserId);


                //TempData["SwitchToEmployeeId"] = null;
                user.IsTempSwitching = null;

                await signInManager.RefreshSignInAsync(user);
                return ThrowJsonSuccess($"Now Refreshing as User {user.NameDisplay}");
            }

            return ThrowJsonError("Employee was not not found");

        }

        public IActionResult ViewAccessGrant(string id = "", bool layout = false)
        {
            var emp = context.AccessGrants.
                Include(x => x.CompanyAccount)
                .Where(x => x.UserId == id)
                .ToList();

            if (emp == null && id != "") return ThrowJsonError("Access Grant was not not found");

            var user= context.Users.Find(id);
            if (user == null) return ThrowJsonError("User not not found");

            ViewBag.User = user;
            ViewBag.IsChangeUserAccessGrant = layout;
            if (emp == null) emp = new List<AccessGrant>();
            return PartialView("_AccessGrants", emp);
        }


        [ActionPermissions(Permissions.CREATE)]
        public async Task<IActionResult> AddOrUpdateAccessGrant(string userId = "", int id = 0)
        {
            var emp = await context.AccessGrants.FirstOrDefaultAsync(x=> x.Id == id && x.UserId == userId);
            if (emp == null && id>0) return BadRequest("Access Grant not found");
            if (emp == null) emp = new AccessGrant { UserId = userId };

            ViewBag.CompanyAccountId = new SelectList(context.CompanyAccounts.OrderBy(x => x.Name).ToList(), "Id", "Name", emp?.CompanyAccountId);

            ViewBag.AccessGrantRoles = await context.AccessGrantRoles.ToListAsync();

            return PartialView("_AddOrUpdateAccessGrant", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionPermissions(Permissions.CREATE)]
        public async Task<IActionResult> AddOrUpdateAccessGrant(AccessGrant model)
        {
            if (ModelState.IsValid)
            {
                if (!await context.Users.AnyAsync(x => x.Id == model.UserId))
                    return ThrowJsonError("User was not found");

                if (!await context.CompanyAccounts.AnyAsync(x => x.Id == model.CompanyAccountId))
                    return ThrowJsonError("Company was not found");

                if (await context.AccessGrants.AnyAsync(x => x.UserId == model.UserId && x.CompanyAccountId == model.CompanyAccountId))
                    return ThrowJsonError("User already has the same access grant");
            }
            if (ModelState.IsValid)
            {
                if (model.Id <= 0)
                {
                    context.AccessGrants.Add(model);
                }
                else
                {
                    context.AccessGrants.Update(model);
                }
                await context.SaveChangesAsync();

                return ViewAccessGrant(model.UserId);
            }

            return ThrowJsonError("Please enter all the fields");
        }


        [HttpPost]
        [ActionPermissions(Permissions.DELETE)]
        public IActionResult RemoveAccessGrant(int id)
        {
            if (ModelState.IsValid)
            {
                var add = context.AccessGrants.FirstOrDefault(x => x.Id == id);
                if (add == null)
                    return BadRequest("Oooh! we didnt find that one");
                //if(context.PayrollPeriodPayAdjustments.Any(x=> x.PayAdjustmentId == payAdjustmentId))
                //    return BadRequest("Ouch! Some items are used as children, please remove them before proceed");

                context.AccessGrants.Remove(add);
                context.SaveChanges();
                return ViewAccessGrant(add.UserId);
            }

            return BadRequest();
        }


        private IActionResult ThrowJsonError(IEnumerable<IdentityError> errors)
        {
            return ThrowJsonError(string.Join(", ", errors.Select(x => x.Code + " - " + x.Description)));
            // Bthrow new NotImplementedException();
        }

        //public IActionResult Page(int dept = 0, int page = 1, int limit = 10)
        //{
        //    if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
        //        return BadRequest();
        //    var emp = context.AppUsers
        //     .Where(x => dept == 0 || dept == x.DepartmentId)
        //     .OrderBy(x => x.EmpID)
        //     .Skip((page - 1) * limit)
        //     .Take(limit)
        //     .Include(x => x.Department)
        //     .ToList();

        //    return PartialView("_ListingRow", emp);
        //}

            
        

        [HttpPost]
        [ActionPermissions(Permissions.DELETE)]
        public IActionResult Remove(string id)
        {
            if (ModelState.IsValid)
            {
                var add = context.AppUsers.FirstOrDefault(x => x.Id == id);
                if (add == null)
                    return BadRequest("Oooh! we didnt find that one");
                //if(context.PayrollPeriodPayAdjustments.Any(x=> x.PayAdjustmentId == payAdjustmentId))
                //    return BadRequest("Ouch! Some items are used as children, please remove them before proceed");

                context.AppUsers.Remove(add);
                context.SaveChanges();
                return RedirectToAction("Index");
            }

            return BadRequest();
        }
    }
}

