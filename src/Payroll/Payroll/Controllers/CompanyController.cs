
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExcelDataReader;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;
using RadPdf.Web.UI;

namespace Payroll.Controllers
{
    //[Authorize(Roles = Roles.PayAll.admin)]
    public class CompanyController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly AccountDbContext accDbcontext;

        private readonly ILogger<CompanyController> logger;
        private readonly AccessGrantService accessGrantService;
        private readonly FileUploadService fileUploadService;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly SynchronizationService synchronizationService;
        private readonly CompanyService companyService;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly UserManager<AppUser> userManager;
        private readonly NotificationService notificationService;
        private readonly UserResolverService userResolverService;

        public CompanyController(PayrollDbContext context, ILogger<CompanyController> logger, AccessGrantService accessGrantService, AccountDbContext accountDbContext, FileUploadService fileUploadService, IBackgroundJobClient backgroundJobClient, SynchronizationService synchronizationService, CompanyService companyService, IHostingEnvironment env, UserManager<AppUser> userManager, NotificationService notificationService, UserResolverService userResolverService)
        {
            this.context = context;
            this.logger = logger;
            this.accessGrantService = accessGrantService;
            this.accDbcontext = accountDbContext;
            this.fileUploadService = fileUploadService;
            this.backgroundJobClient = backgroundJobClient;
            this.synchronizationService = synchronizationService;
            this.companyService = companyService;
            this.hostingEnvironment = env;
            this.userManager = userManager;
            this.notificationService = notificationService;
            this.userResolverService = userResolverService;
        }

        public void OnGet()
        {
            logger.LogInformation("Requested the Index Page");
            int count;
            try
            {
                for (count = 0; count <= 5; count++)
                {
                    if (count == 3)
                    {
                        throw new Exception("RandomException");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception Caught");
            }
        }

        
        public async Task<IActionResult> Index(int page = 1, int limit = 10)
        {
            var accessibleCompanies = await accessGrantService.GetAllAccessibleCompanyAccountsAsync();

            ViewBag.Count = accDbcontext.CompanyAccounts.Count();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Listing", accessibleCompanies);
            return View(accessibleCompanies);
        }

        public async Task<IActionResult> Change(int id)
        {
            var emp = accDbcontext.CompanyAccounts
                .Where(x => x.Id == id)
                .Include(x => x.RequestProcessConfigs)
                .Include(x => x.AccessGrants)
                    .ThenInclude(x => x.User)
                .First();

            var payrolComp = await context.Companies
                .Where(x => x.Id == id)
                .Include(x => x.WorkTimes)
                .Include(a => a.Departments)
                .Include(a => a.Divisions)
                .Include(a => a.Locations)
                .FirstOrDefaultAsync();

            ViewBag.PayrolCompany = payrolComp;

            if (emp.ProgressBySteps == null)
                emp.ProgressBySteps = null;

            if(emp.Status == CompanyStatus.Rejected || emp.Status == CompanyStatus.Approved)
            {
                SetTempDataMessage(await notificationService.GetLatestReadNotificationSummaryAsync(NotificationTypeConstants.CompanySubmittedForAction, id), emp.Status == CompanyStatus.Rejected ? MsgAlertType.danger : MsgAlertType.success);
            }

            if (Request.IsAjaxRequest())
                return PartialView("_ChangeOverview", emp);

            return View(emp);
        }
        
        public async Task<IActionResult> NewCompany()
        {
            var emp = new CompanyAccount();
            ViewBag.NewEmployee = true;

            ViewBag.IndustryId = new SelectList(await accDbcontext.Industries.ToListAsync(), "Id", "Name", emp.IndustryId);
            ViewBag.TimeZone = new SelectList(TimeZoneInfo.GetSystemTimeZones(), "Id", "DisplayName", emp.TimeZone);
            return View("Change", emp);
        }

        public async Task<IActionResult> Detail(int id, string tab = null)
        {
            if (tab == null)
            {
                var emp = await accDbcontext.CompanyAccounts
                    .Where(x => x.Id == id)
                    .Include(x => x.RequestProcessConfigs)
                    .Include(x => x.Industry)
                    .Include(x => x.AccessGrants)
                        .ThenInclude(x => x.User)
                    .FirstOrDefaultAsync();

                var payrolComp = await context.Companies
                    .Where(x => x.Id == id)
                    .Include(x => x.Departments)
                    .ThenInclude(x => x.DepartmentHeads)
                    .ThenInclude(x => x.Employee)
                    .Include(x => x.PayrollPeriods)
                    .Include(x => x.PayAdjustments)
                    .Include(x => x.DayOffs)
                    .Include(x => x.WorkTimes)
                    .Include(x => x.CompanyPublicHolidays)
                    .Include(x => x.RequestApprovalConfigs)
                    .ThenInclude(x => x.Employee)
                    .Include(x => x.RequestApprovalConfigs)
                    .ThenInclude(x => x.DayOff)
                    .Include(x => x.RequestApprovalConfigs)
                    .ThenInclude(x => x.EmployeeRole)
                    .Include(x => x.EmployeeRoles)
                    .ThenInclude(x => x.AssignedEmployees)
                    .Include(x => x.EmployeeRoles)
                    .ThenInclude(x => x.Reminders)
                    .ThenInclude(x => x.EmployeeRole)
                .Include(a => a.CompanyFolders)
                .ThenInclude(a => a.CompanyFiles)
                    .FirstOrDefaultAsync();

                ViewBag.empDdList = await companyService.GetAllEmployeesInMyCompanyForDropdownOptGroups();

                // all days of week
                var daysOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();

                // get first day of week from current culture
                var firstDayOfWeek = emp.WeekStartDay;

                // all days of week ordered from first day of week
                var list = daysOfWeek.OrderBy(x => (x - firstDayOfWeek + 7) % 7).ToDictionary(x => x.ToString(), x => (emp.DayOfWeekHolidays?.Contains(Convert.ToInt32(x)) ?? false));
                ViewBag.Week = list;



                var dayOffGroupByYear = await context.DayOffEmployees.Where(x => x.DayOff.CompanyId == id && x.Year == DateTime.Now.Year)
                    .GroupBy(x => new { x.DayOffId, x.Year })
                    .Select(x => new DayOffEmployee { DayOffId = x.Key.DayOffId, Year = x.Key.Year, TotalHours = x.Count() })
                    .ToListAsync();


                foreach (var item in payrolComp.DayOffs)
                    item.DayOffEmployees = dayOffGroupByYear.Where(x => x.DayOffId == item.Id).ToList();


                ViewBag.PayrolCompany = payrolComp;
                ViewBag.dayOffs = await companyService.GetDayOffs(id);
                ViewBag.CompanyId = id;
                ViewBag.Year = DateTime.Now.Year;
                ViewBag.LastNotification = await accDbcontext.Notifications.Where(a => a.NotificationTypeId == NotificationTypeConstants.CompanySubmittedForAction && a.EntityId == emp.Id.ToString())
                    .OrderByDescending(a=> a.SentDate).FirstOrDefaultAsync();

                if (emp.ProgressBySteps == null)
                    emp.ProgressBySteps = new Dictionary<int, bool>();

                ViewBag.AccessGrantRoles = await accDbcontext.AccessGrantRoles.ToListAsync();

                return View(emp);
            }
            else
            {
                switch (tab)
                {
                    case CompanyDetailTabs.Departments:

                    default:
                        break;
                }
            }

            return View();
        }


        public async Task<IActionResult> View(int id, string tab = null)
        {
            if (tab == null)
            {
                var emp = await accDbcontext.CompanyAccounts
                    .Where(x => x.Id == id)
                    .Include(x => x.RequestProcessConfigs)
                    .Include(x => x.Industry)
                    .Include(x => x.AccessGrants)
                        .ThenInclude(x => x.User)
                    .FirstOrDefaultAsync();

                var payrolComp = await context.Companies
                    .Where(x => x.Id == id)
                    .Include(x => x.Departments)
                    .ThenInclude(x => x.DepartmentHeads)
                    .ThenInclude(x => x.Employee)
                    .Include(x => x.PayrollPeriods)
                    .Include(x => x.PayAdjustments)
                    .Include(x => x.DayOffs)
                    .Include(x => x.WorkTimes)
                    .Include(x => x.CompanyPublicHolidays)
                    .Include(x => x.RequestApprovalConfigs)
                    .ThenInclude(x => x.Employee)
                    .Include(x => x.RequestApprovalConfigs)
                    .ThenInclude(x => x.DayOff)
                    .Include(x => x.RequestApprovalConfigs)
                    .ThenInclude(x => x.EmployeeRole)
                    .Include(x => x.EmployeeRoles)
                    .ThenInclude(x => x.AssignedEmployees)
                    .Include(x => x.EmployeeRoles)
                    .ThenInclude(x => x.Reminders)
                    .ThenInclude(x => x.EmployeeRole)
                .Include(a => a.CompanyFolders)
                .ThenInclude(a => a.CompanyFiles)
                    .FirstOrDefaultAsync();

                ViewBag.empDdList = await companyService.GetAllEmployeesInMyCompanyForDropdownOptGroups();

                // all days of week
                var daysOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();

                // get first day of week from current culture
                var firstDayOfWeek = emp.WeekStartDay;

                // all days of week ordered from first day of week
                var list = daysOfWeek.OrderBy(x => (x - firstDayOfWeek + 7) % 7).ToDictionary(x => x.ToString(), x => (emp.DayOfWeekHolidays?.Contains(Convert.ToInt32(x)) ?? false));
                ViewBag.Week = list;



                var dayOffGroupByYear = await context.DayOffEmployees.Where(x => x.DayOff.CompanyId == id && x.Year == DateTime.Now.Year)
                    .GroupBy(x => new { x.DayOffId, x.Year })
                    .Select(x => new DayOffEmployee { DayOffId = x.Key.DayOffId, Year = x.Key.Year, TotalHours = x.Count() })
                    .ToListAsync();


                foreach (var item in payrolComp.DayOffs)
                    item.DayOffEmployees = dayOffGroupByYear.Where(x => x.DayOffId == item.Id).ToList();


                ViewBag.PayrolCompany = payrolComp;
                ViewBag.dayOffs = await companyService.GetDayOffs(id);
                ViewBag.CompanyId = id;
                ViewBag.Year = DateTime.Now.Year;
                ViewBag.LastNotification = await accDbcontext.Notifications.Where(a => a.NotificationTypeId == NotificationTypeConstants.CompanySubmittedForAction && a.EntityId == emp.Id.ToString())
                    .OrderByDescending(a => a.SentDate).FirstOrDefaultAsync();

                if (emp.ProgressBySteps == null)
                    emp.ProgressBySteps = new Dictionary<int, bool>();

                ViewBag.AccessGrantRoles = await accDbcontext.AccessGrantRoles.ToListAsync();
                ViewBag.UserPermissionCount = await accDbcontext.AccessGrants.Where(x => x.CompanyAccountId == userResolverService.GetCompanyId())
                .Select(x => x.User)
                .AsQueryable()
                .GroupBy(t => t.UserType)
                .Select(a => new { a.Key, Count = a.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

                return View(emp);
            }
            else
            {
                switch (tab)
                {
                    case CompanyDetailTabs.Departments:

                    default:
                        break;
                }
            }

            return View();
        }


        public async Task<IActionResult> PermissionSummary(UserType type)
        {
            var users = await accDbcontext.AccessGrants.Where(x => x.User.UserType == type && x.CompanyAccountId == userResolverService.GetCompanyId())
                .Select(x => x.User)
                .AsQueryable()
                .GroupBy(t => t.UserType)
                .Select(a => new { a.Key, Count = a.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
            return PartialView("_PermissionSummary", users);
        }

        public async Task<IActionResult> GetPermissionDetails(UserType type)
        {
            var users = await accDbcontext.AccessGrants.Where(x => x.User.UserType == type && x.CompanyAccountId == userResolverService.GetCompanyId())
                .Select(x=> x.User).ToListAsync();
            ViewBag.type = type;
            return PartialView("_GetPermissionDetails", users);
        }


        public async Task<IActionResult> Header(int id)
        {
            var modelInDb = await accDbcontext.CompanyAccounts.FindAsync(id);


            // all days of week
            var daysOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();

            // get first day of week from current culture
            var firstDayOfWeek = modelInDb.WeekStartDay;

            // all days of week ordered from first day of week
            var list = daysOfWeek.OrderBy(x => (x - firstDayOfWeek + 7) % 7).ToDictionary(x => x.ToString(), x => (modelInDb.DayOfWeekHolidays?.Contains(Convert.ToInt32(x)) ?? false));
            ViewBag.Week = list;


            return PartialView("_Header", modelInDb);
        }

        public async Task<IActionResult> Search(string query)
        {
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
                return BadRequest();

            var emp = await accessGrantService.GetAllAccessibleCompanyAccountsAsync(searchKey: query);

            //query = query.ToLower();
            //var emp = ()
            // .Where(x => x.Name.ToLower().Contains(query) ||
            // (x.Hotline !=null && x.Hotline.ToLower().Contains(query)) ||
            // (x.ManagingDirector != null && x.ManagingDirector.ToString() == query) || 
            // (x.Address != null && x.Address.Contains(query)))
            // .OrderBy(x => x.Name)
            // .Take(11)
            // .ToList();

            return PartialView("_Listing", emp);
        }

        public bool IsCreating()
        {
            SetIsCreatingTempData();
            return TempData["IsCreating"].ToString() == "True";
        }

        public void SetIsCreatingTempData(bool? value = null)
        {
            if (value.HasValue)
                TempData["IsCreating"] = value.Value;
            else
                TempData["IsCreating"] = TempData["IsCreating"] ?? false;

            TempData.Keep("IsCreating");
        }
        
        public async Task<IActionResult> AddOrUpdate(int id = 0)
        {
            if (id == 0) SetIsCreatingTempData(true);
            else SetIsCreatingTempData();
            var emp = await accDbcontext.CompanyAccounts.FindAsync(id);
            if (emp == null && id != 0) return ThrowJsonError("Company not found");
            if (emp == null) emp = new CompanyAccount { };

            ViewBag.IndustryId = new SelectList(await accDbcontext.Industries.ToListAsync(), "Id", "Name", emp.IndustryId);
            ViewBag.TimeZone = new SelectList(TimeZoneInfo.GetSystemTimeZones(), "Id", "DisplayName", emp.TimeZone);

            return PartialView("_AddOrUpdate", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdate(CompanyAccount model)
        {
            if (ModelState.IsValid)
            {
                if(accDbcontext.CompanyAccounts.Any(x=> x.Name.ToLower() == model.Name.ToLower() && 
                (model.Id == 0 || model.Id != model.Id)))
                    return ThrowJsonError($"Name '{model.Name}' is alreay taken. Please choose another name");
                if (model.Id == 0 && string.IsNullOrWhiteSpace(model.Street1))
                    return ThrowJsonError($"Please enter location details");
            }

            if (ModelState.IsValid)
            {
                if (model.Id <= 0)
                {
                    // create first
                    model.Status = CompanyStatus.Draft;
                    companyService.UpdateProgress(model, 1);
                    accDbcontext.CompanyAccounts.Add(model);
                }
                else
                {
                    var modelInDb = await accDbcontext.CompanyAccounts.FindAsync(model.Id);
                    modelInDb.Name = model.Name;
                    modelInDb.IndustryId = model.IndustryId;
                    modelInDb.TimeZone = model.TimeZone;
                    modelInDb.Website = model.Website;
                    modelInDb.ManagingDirector = model.ManagingDirector;
                    modelInDb.CompanyRegistrationNo = model.CompanyRegistrationNo;

                    modelInDb.Address = model.Address;
                    modelInDb.Hotline = model.Hotline;
                    modelInDb.Email = model.Email;

                    //if(!string.IsNullOrWhiteSpace(model.Street1))
                    //{
                    //    var loc = await context.Locations.FirstOrDefaultAsync(x => x.CompanyId == model.Id && x.Name.ToLower().Equals(model.Street1));
                    //    if (loc == null)
                    //    {
                    //        context.Locations.Add(new Location { Name = model.Street1, CompanyId = modelInDb.Id });
                    //        await context.SaveChangesAsync();
                    //    }
                    //}

                    companyService.UpdateProgress(modelInDb, 1);
                    accDbcontext.CompanyAccounts.Update(modelInDb);
                }

                // if only have one location
                var locCOunt = await companyService.GetCompanyCount();
                var toUpdateAddress = locCOunt <= 1 ? new Address
                {
                    Street1 = model.Street1,
                    Street2 = model.Street2,
                    ZipCode = model.Zip,
                    State = model.State,
                    City = model.City,
                    //ContactNumber = model.Hotline,
                } : null;
               bool isSaved = await accDbcontext.SaveChangesAsync() > 0;
                if (isSaved)
                {
                    backgroundJobClient.Enqueue(() => synchronizationService.SyncCompanyOnMasterDatabase(model.Id, model.Name, toUpdateAddress));
                    //backgroundJobClient.Enqueue(() => synchronizationService.SyncCompanyOnMasterPayrolDb(model.Id, model.Name));
                    return await ReturnStepDone(model.Id, 1);
                    //return PartialView("_CreatedAndReadyForSetup", model);
                } else { return ThrowJsonError();  }

            }

            return ThrowJsonError(ModelState);
        }

        public async Task<IActionResult> ReturnStepDone(int id, int step) => RedirectToAction("GetSetupOverviewItemDone", "Account", new { id, step });

        [HttpPost]
        public async Task<IActionResult> SendForVerification(int id)
        {
            if (ModelState.IsValid)
            {
                var cmp = await accDbcontext.CompanyAccounts.FindAsync(id);
                if (cmp == null)
                    return ThrowJsonError("Company was not found");

                if (cmp.Status != CompanyStatus.Draft && cmp.Status != CompanyStatus.Rejected)
                    return ThrowJsonError("Company must be in Draft Status for Verification");

                if (cmp.ProgressPercent != 100)
                    return ThrowJsonError("Company progress is not yet completed!");

                var isSent = await notificationService.SendAsync(NotificationTypeConstants.CompanySubmittedForAction, cmp, id) > 0;
                

                if (isSent)
                {
                    cmp.Status = CompanyStatus.Pending;
                    await accDbcontext.SaveChangesAsync();
                    SetTempDataMessage("Great! Your Company was just sent for verificaiton. Our freindly staffs will verify your account and will be in touch with you soon.");

                    return RedirectToAction("Change", new { id });
                }
                else
                    return ThrowJsonError("Failed to send notification");
            }

            return ThrowJsonError("Unexpected error has occured!");
        }


        public async Task<IActionResult> AddOrUpdateAddressContact(int id)
        {
            var emp = await accDbcontext.CompanyAccounts.FindAsync(id);
            if (emp == null) return ThrowJsonError("Company not found");

            return PartialView("_AddOrUpdateAddressContact", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateAddressContact(CompanyAccount model)
        {
            var modelInDb = await accDbcontext.CompanyAccounts.FindAsync(model.Id);
            if (string.IsNullOrWhiteSpace(model.Address)) return ThrowJsonError("Please enter address to continue");

            modelInDb.Address = model.Address;
            modelInDb.Hotline = model.Hotline;
            modelInDb.Email = model.Email;

            companyService.UpdateProgress(modelInDb, 1);
            accDbcontext.CompanyAccounts.Update(modelInDb);

            bool isSaved = await accDbcontext.SaveChangesAsync() > 0;
            if (isSaved)
            {
                backgroundJobClient.Enqueue(() => synchronizationService.SyncCompanyOnMasterDatabase(model.Id, model.Name, null));
                return await Change(model.Id);
            }
            else 
                return ThrowJsonError();
        }

        public async Task<IActionResult> UpdateFormatSettings(int id)
        {
            if (id == 0) SetIsCreatingTempData(true);
            else SetIsCreatingTempData();
            var emp = await accDbcontext.CompanyAccounts.FindAsync(id);
            if (emp == null) return ThrowJsonError("Company not found");

            var date = new DateTime(2017, 08, 03, 21, 55, 05);
            ViewBag.DateFormatTest = date;
            ViewBag.DateFormat = Formatters.DateFormats.Select(x => new SelectListItem(date.ToString(x), x, x.Equals(emp.DateFormat))).ToList();

            ViewBag.TimeFormat = Formatters.TimeFormats.Select(x => new SelectListItem(date.ToString(x), x, x.Equals(emp.TimeFormat))).ToList();


            var dec = 1234567.890;
            ViewBag.CurrencyFormatTest = dec;
            ViewBag.CurrencyFormat = Formatters.CurrencyFormats.Select(x => new SelectListItem(string.Format(x, dec), x, x.Equals(emp.CurrencyFormat))).ToList();

            var empTss = new Employee { Initial = Initial.Ms, FirstName = "Alfonso", MiddleName = "Ernesto", LastName = "Hernández" };
            var dictionary = new Dictionary<string, string>
            {
                { "Initial", empTss.Initial.ToString() },
                { "FirstName", empTss.FirstName },
                { "MiddleName", empTss.MiddleName },
                { "LastName", empTss.LastName }
            };
            ViewBag.NameFormat = Formatters.NameFormats.Select(x => new SelectListItem(x.StringFormat(dictionary), x, x.Equals(emp.NameFormat))).ToList();
            ViewBag.NameFormatTest = empTss;

            return PartialView("_UpdateFormatSettings", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFormatSettings(CompanyAccount model)
        {
            var modelInDb = await accDbcontext.CompanyAccounts.FindAsync(model.Id);
            modelInDb.DateFormat = model.DateFormat;
            modelInDb.TimeFormat = model.TimeFormat;
            modelInDb.NameFormat = model.NameFormat;
            modelInDb.CurrencyFormat = model.CurrencyFormat;

            try
            {
                if(!Formatters.DateFormats.Any(x=> x.Equals(modelInDb.DateFormat)) || !Formatters.TimeFormats.Any(x => x.Equals(modelInDb.TimeFormat)) || !Formatters.NameFormats.Any(x => x.Equals(modelInDb.NameFormat)) || !Formatters.CurrencyFormats.Any(x => x.Equals(modelInDb.CurrencyFormat)))
                    return ThrowJsonError("Kindly choose any item from format settings");
            }
            catch (Exception)
            {

                throw;
            }

            companyService.UpdateProgress(modelInDb, 5);
            accDbcontext.CompanyAccounts.Update(modelInDb);

            bool isSaved = await accDbcontext.SaveChangesAsync() > 0;
            if (isSaved)
                return await ReturnStepDone(model.Id, 5);
            //return await Change(model.Id);
            else { return ThrowJsonError(); }
        }


        public async Task<IActionResult> UpdateDeptDivisionLocations(int id)
        {
            if (id == 0) SetIsCreatingTempData(true);
            else SetIsCreatingTempData();
            var emp = await context.Companies
                .Include(a=> a.Departments)
                .Include(a=> a.Divisions)
                .Include(a=> a.Locations)
                .FirstOrDefaultAsync(x=> x.Id == id);
            if (emp == null) return ThrowJsonError("Company not found");



            return PartialView("_UpdateDeptDivisionLocations", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDeptDivisionLocations(Company model)
        {
            var modelInDb = await context.Companies
                .Include(a => a.Departments)
                .Include(a => a.Divisions)
                .Include(a => a.Locations)
                .FirstOrDefaultAsync(x => x.Id == model.Id);
            var accDnModel = await accDbcontext.CompanyAccounts.FindAsync(model.Id);
            //if(accDnModel.Status == CompanyStatus.Approved || accDnModel.Status == CompanyStatus.Pending)
            //    return ThrowJsonError("Sorry, Unable to change due to current status");


            var toCreate = model.Departments.Where(a => !string.IsNullOrWhiteSpace(a.Name) && a.IsActive && a.Id < 0);
            var toUpdate = model.Departments.Where(x => x.IsActive && x.Id > 0);
            var toRemove = model.Departments.Where(a => a.IsActive == false && a.Id > 0);

            try
            {
                // dept
                foreach (var item in model.Departments.Where(x => x.Id > 0))
                {
                    var first = modelInDb.Departments.First(a => a.Id == item.Id);
                    if (item.IsActive)
                    {
                        first.Name = item.Name;
                        first.DeptCode = item.DeptCode;
                    }
                    else
                    {
                        modelInDb.Departments.Remove(first);
                    }
                }
                foreach (var item in model.Departments.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.IsActive && x.Id <= 0))
                    modelInDb.Departments.Add(new Department { Name = item.Name, DeptCode = item.DeptCode });


                // location
                if (model.Locations != null && model.Locations.Any())
                {
                    foreach (var item in model.Locations.Where(x => x.Id > 0))
                    {
                        var first = modelInDb.Locations.First(a => a.Id == item.Id);
                        if (item.IsActive)
                        {
                            first.Name = item.Name;
                        }
                        else
                        {
                            modelInDb.Locations.Remove(first);
                        }
                    }
                    foreach (var item in model.Locations.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.IsActive && x.Id <= 0))
                        modelInDb.Locations.Add(new Location { Name = item.Name });
                }

                // division
                if (model.Divisions != null && model.Divisions.Any())
                {
                    foreach (var item in model.Divisions.Where(x => x.Id > 0))
                    {
                        var first = modelInDb.Divisions.First(a => a.Id == item.Id);
                        if (item.IsActive)
                        {
                            first.Name = item.Name;
                        }
                        else
                        {
                            modelInDb.Divisions.Remove(first);
                        }
                    }
                    foreach (var item in model.Divisions.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.IsActive && x.Id <= 0))
                        modelInDb.Divisions.Add(new Division { Name = item.Name });
                }

                companyService.UpdateProgress(accDnModel, 6);
                accDbcontext.CompanyAccounts.Update(accDnModel);

                await accDbcontext.SaveChangesAsync();

                bool isSaved = await context.SaveChangesAsync() > 0;
                if (isSaved)
                    return await ReturnStepDone(model.Id, 6);
                    //return await Change(model.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update company location, department, division and teams");
                return ThrowJsonError(ex.Message);
            }

            return ThrowJsonError();
        }

        [HttpPost]
        public async Task<IActionResult> SetupRolesForCompany(int id, IList<string> roles)
        {
            var cmp = await accDbcontext.CompanyAccounts.FindAsync(id);
            if (cmp == null) return ThrowJsonError("Company not found");

            var d = await context.EmployeeRoles.Where(a => a.CompanyId == id).Include(a => a.Reminders).ToListAsync();
            if (d != null)
            {
                return ThrowJsonError("Company already has roles defined");
                //context.Reminders.RemoveRange(d.SelectMany(a => a.Reminders));
                //context.EmployeeRoles.RemoveRange(d);
            }

            var defaultRoles = typeof(Roles.Company).GetFields()
                .Where(x => roles.Contains(x.Name))
                .Select(r => new EmployeeRole
                {
                    Role = r.Name,
                    CalendarDefaults = Calendars.List.ToDictionary(a => a.Item1, a => EmployeeSelectorRole.None),
                    CompanyId = cmp.Id,
                    Description = ""
                }).ToList(); ;

            await context.EmployeeRoles.AddRangeAsync(defaultRoles);
            var _t = await context.SaveChangesAsync();

            companyService.UpdateProgress(cmp, 9);
            accDbcontext.CompanyAccounts.Update(cmp);
            await accDbcontext.SaveChangesAsync();

            return await ReturnStepDone(cmp.Id, 9);
            //return ThrowJsonSuccess(_t + " Roles Created successfully");
        }


        public async Task<IActionResult> ChoosePlan(int id)
        {
            var emp = await context.Companies.FirstOrDefaultAsync(x => x.Id == id);
            if (emp == null) return ThrowJsonError("Company not found");
            ViewBag.Id = id;
            return PartialView("_ChangePlan", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChoosePlan(Company model)
        {
            var cmp = await accDbcontext.CompanyAccounts.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (cmp == null) return ThrowJsonError("Company not found");

            companyService.UpdateProgress(cmp, 9);
            accDbcontext.CompanyAccounts.Update(cmp);

            cmp.Status = CompanyStatus.Approved;
            await accDbcontext.SaveChangesAsync();
            return PartialView("_CompanyApproved", cmp);
        }

        public async Task<IActionResult> PayComponents(int id)
        {
            var emp = await context.PayAdjustments.Where(x => x.CompanyId == id).ToListAsync();
            if (emp == null) return ThrowJsonError("Company not found");
            ViewBag.Id = id;
            return PartialView("_PayComponentsListing", emp);
        }

        [HttpPost]
        public async Task<IActionResult> RemovePayComponent(int id)
        {
            var emp = await context.PayAdjustments.FirstOrDefaultAsync(x => x.CompanyId == userResolverService.GetCompanyId() && x.Id == id);
            if (emp == null) return ThrowJsonError($"Company's pay component was not found!");

            context.PayAdjustments.Remove(emp);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(PayComponents), new { id = emp.CompanyId });
        }

        [HttpPost]
        public async Task<IActionResult> AddPayComponent(int id, string n)
        {
            var cmp = await companyService.GetCompanyAccount(id);
            if (cmp == null) return ThrowJsonError("Company account was not found!");

            if (n.IsMissing()) return ThrowJsonError("Pay component was not found!");
            n = n.ToLower();
            var emp = await context.PayAdjustments.FirstOrDefaultAsync(x => x.CompanyId == id && x.Name.ToLower() == n);
            if (emp != null) return ThrowJsonError($"Company already has '{n}' pay component");

            if (!PreConfigurePayComponents.List.Any(x => x.title.ToLower() == n)) return ThrowJsonError("Chosen pay component is invalid!");
            var data = PreConfigurePayComponents.List.First(x => x.title.ToLower() == n);
            data.adj.CompanyId = id;
            context.PayAdjustments.Add(data.adj);


            if (await context.SaveChangesAsync() > 0)
                if (!await companyService.IsCompanyStepDone(id, 6))
                {
                    var d = await companyService.UpdateProgressAndSaveAsync(cmp, 6);
                    TempData["ProgressPercent"] = d.ProgressPercent;
                    TempData["Step"] = 6;
                }

            return RedirectToAction(nameof(PayComponents), new { id });
        }


        public async Task<IActionResult> NewEmployeeProfile(int id)
        {
            ViewBag.Id = id;
            ViewBag.DepartmentId = new SelectList(await companyService.GetDepartmentsOfCurremtCompany(userResolverService.GetCompanyId()), "Id", "Name");
            ViewBag.LocationId = new SelectList(await companyService.GetLocationsOfCurremtCompany(), "Id", "Name");
            ViewBag.ReportingEmployeeId = await companyService.GetAllEmployeesInMyCompanyForDropdownOptGroups();


            var adjustments = await companyService.GetPayAdjustments();
            if (adjustments.Any() == false)
                return ThrowJsonError("Oops! Company does not have pay components configured!");

            var empPayAdjustmentList = adjustments
                      .Select(ajusments => new EmployeePayComponent
                      {
                          EmployeeId = id,
                          Total = 0,
                          PayAdjustment = ajusments,
                          PayAdjustmentId = ajusments.Id
                      }).ToList();


            var oldrecords = context.EmployeePayComponents.Where(x => x.EmployeeId == id).ToList();

            foreach (var item in empPayAdjustmentList)
            {
                if (oldrecords.Any(x => x.PayAdjustmentId == item.PayAdjustmentId))
                {
                    // replcaing toal
                    item.IsActive = true;
                }
            }
            return PartialView("_NewEmployeeProfile", new EmploymentProfileVm { CompanyId = id, Wages = empPayAdjustmentList, InviteThemToFill = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewEmployeeProfile(EmploymentProfileVm model)
        {
            var cmp = await companyService.GetCompanyAccount(model.CompanyId);
            if (cmp == null) ModelState.AddModelError("", "Company account was not found!");

            if (model.DepartmentId == -1 && string.IsNullOrWhiteSpace(model.NewDepartment)) 
                ModelState.AddModelError("", "Please enter name of new department!");
            if (ModelState.IsValid)
            {
                var loc = await context.Locations.FindAsync(model.LocationId);
                var mgr = await context.Employees.FindAsync(model.ReportingEmployeeId);
                var levelId = await companyService.GetOrCreateAndGetClassificationIdAsync(model.Level, model.CompanyId);


                var deptId = (model.DepartmentId == -1) ?
                    await companyService.GetOrCreateAndGetDepartmentIdAsync(model.NewDepartment, model.CompanyId) :
                    model.DepartmentId;
                var dept = await context.Departments.FindAsync(deptId);

                var Job = new Job 
                {
                    CompanyId = model.CompanyId,
                    JobTitle = model.JobTitle,
                    LevelId = levelId,
                    DepartmentId = dept.Id,
                };
                var emp = new Employee
                {
                    //Individual = new  Individual
                    //{
                    //    FirstName = model.FirstName,
                    //    LastName = model.LastName,
                    //    EmailPersonal = model.PersonalEmail,
                    //},
                    EmailPersonal = model.PersonalEmail,
                    CompanyId = model.CompanyId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    JobTitle = model.JobTitle,
                    Job = Job,
                    
                    DateOfJoined = model.StartDate,
                    LocationId = model.LocationId,
                    LocationName = loc.Name,
                    DepartmentId = dept.Id,
                    DepartmentName = dept.Name,
                    EmailWork = model.PersonalEmail,
                };

                if (mgr != null)
                    emp.ReportingEmployeeId = mgr.Id;

                if (model.Wages.Any(x => x.IsActive))
                    emp.EmployeePayComponents = model.Wages.ToList();

                if(model.InviteThemToFill)
                {
                    emp.EmployeeStatus = EmployeeStatus.ActionNeeded;
                    emp.EmployeeSecondaryStatus = EmployeeSecondaryStatus.WaitingSelfOnBoard;
                }
                else
                {
                    emp.EmployeeStatus = EmployeeStatus.Incomplete;
                    emp.EmployeeSecondaryStatus = EmployeeSecondaryStatus.PersonalInfoMissing;
                }

                context.Employees.Add(emp);


                if (await context.SaveChangesAsync() > 0)
                {
                    if (!await companyService.IsCompanyStepDone(model.CompanyId, 7))
                    {
                        companyService.UpdateProgress(cmp, 7);
                        accDbcontext.CompanyAccounts.Update(cmp);
                        await accDbcontext.SaveChangesAsync();
                    }
                    else
                    {
                        // creating new employee here (after setup process)
                        return RedirectToAction();
                    }
                }

                model.Employee = emp;
                //return PartialView("_NewEmployeeProfileDone", model);

                return await ReturnStepDone(model.CompanyId, 7);
            }
            return ThrowJsonError(ModelState);

            //if (n.IsMissing()) return ThrowJsonError("Pay component was not found!");
            //n = n.ToLower();
            //var emp = await context.PayAdjustments.FirstOrDefaultAsync(x => x.CompanyId == id && x.Name.ToLower() == n);
            //if (emp != null) return ThrowJsonError($"Company already has '{n}' pay component");

            //if (!PreConfigurePayComponents.List.Any(x => x.title.ToLower() == n)) return ThrowJsonError("Chosen pay component is invalid!");
            //var data = PreConfigurePayComponents.List.First(x => x.title.ToLower() == n);
            //data.adj.CompanyId = id;
            //context.PayAdjustments.Add(data.adj);
            //await context.SaveChangesAsync();

            //return RedirectToAction(nameof(PayComponents), new { id });
        }


        public async Task<IActionResult> EmployeeRoles(int id)
        {
            var emp = await context.EmployeeRoles.Where(x => x.CompanyId == id).ToListAsync();
            if(emp.Any())  return ThrowJsonError("Company already has roles configured");
            ViewBag.Id = id;
            return PartialView("_EmployeeRoles", emp);
        }

        public async Task<IActionResult> UploadImage(int id, string update = "")
        {
            SetIsCreatingTempData();
            ViewBag.Update = update;
            var emp = await accDbcontext.CompanyAccounts.FindAsync(id);
            if (emp == null) return ThrowJsonError("Company not found");
            return PartialView("_UploadImage", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(Company model, string base64Image)
        {
            bool hasFiles = fileUploadService.HasFilesReadyForUpload(base64Image);
            if (hasFiles)
            {
                if (fileUploadService.GetFileSizeInMb(base64Image) >= UploadSetting.MaxImageSizeMb)
                    return ThrowJsonError($"File is too huge, only file size up to {UploadSetting.MaxFileSizeMb}MB are allowed!");

                // only Jph files are posted
                //if (!fileUploadService.IsAllowedFileType(base64Image, UploadSetting.ImageTypes))
                //    return ThrowJsonError($"Uploaded file type is not allowed!");


                var companyFrmDb = await accDbcontext.CompanyAccounts.FindAsync(model.Id);
                if (companyFrmDb == null) return ThrowJsonError("Company was not found");
                try
                {
                    string fileUrl = await fileUploadService.UploadFles(base64Image);

                    companyFrmDb.LogoUrl = fileUrl;
                }
                catch (Exception ex)
                {
                    return ThrowJsonError(ex.Message);
                }
                companyService.UpdateProgress(companyFrmDb, 3);

                accDbcontext.CompanyAccounts.Update(companyFrmDb);
                await accDbcontext.SaveChangesAsync();

                //backgroundJobClient.Enqueue(() => synchronizationService.SyncCompanyOnMasterPayrolDb(model.Id, model.Name));

                //return await Change(model.Id);
                return await ReturnStepDone(model.Id, 3);
            }

            return ThrowJsonError("Please upload an image");
        }


        public async Task<IActionResult> SetupPayrolPeriod(int id = 0)
        {
            SetIsCreatingTempData();
            var emp = await accDbcontext.CompanyAccounts.FindAsync(id);
            if (emp == null && id != 0) return ThrowJsonError("Company not found");
            if (emp == null) emp = new CompanyAccount { };

            var listDays = Enumerable.Range(1, 31).ToDictionary(x => x.AddOrdinal(true), x => x);
            ViewBag.PayrolPeriodStartDate = new SelectList(listDays,  "Value", "Key", emp?.PayrolPeriodStartDate ?? 21);
            ViewBag.PayrolPeriodEndDate = new SelectList(listDays, "Value" , "Key", emp?.PayrolPeriodEndDate ?? 20);


            var daysOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();
            var firstDayOfWeek = emp.WeekStartDay;
            var list = daysOfWeek.OrderBy(x => (x - firstDayOfWeek + 7) % 7).ToDictionary(x=> (int)x, x=> x);
            ViewBag.DayOfWeekHolidays = new MultiSelectList(list, "Key", "Value", emp.DayOfWeekHolidays);
            ViewBag.DayOfWeekOffDays = new MultiSelectList(list, "Key", "Value", emp.DayOfWeekOffDays);
            ViewBag.Week = list;

            return PartialView("_SetupPayrolPeriod", emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetupPayrolPeriod(CompanyAccount model)
        {
            var ompanny = await accDbcontext.CompanyAccounts.FindAsync(model.Id);
            if (ompanny == null)
                return ThrowJsonError("Company was not found!");

            var days = 30;
            var start = model.PayrolPeriodStartDate;
            if (model.IncludeStartDate ?? false)
                start += 1;

            var totalDays = (30 - start);

            var daysLeft1mos = days - start;

            var enddate = days - daysLeft1mos;
            if (!model.IncludeEndDate ?? false) enddate++;


            //if (days == (daysLeft1mos + enddate))
            //    return ThrowJsonError("Total days of payrol duration must be eqaul to 30");

            ompanny.PayrolPeriodStartDate = model.PayrolPeriodStartDate;
            ompanny.PayrolPeriodEndDate = model.PayrolPeriodEndDate;
            ompanny.IncludeEndDate = model.IncludeEndDate;
            ompanny.IncludeStartDate = model.IncludeStartDate;
            ompanny.PayrolPeriodDays = daysLeft1mos + enddate;
            ompanny.WeekStartDay = model.WeekStartDay;
            ompanny.WhenToApplyPaidTimeOffPolicyAfterJoining = model.WhenToApplyPaidTimeOffPolicyAfterJoining;

            if (model.DayOfWeekHolidays != null && model.DayOfWeekHolidays.Count > 0)
                ompanny.DayOfWeekHolidays = model.DayOfWeekHolidays;
            else
                ompanny.DayOfWeekHolidays = null;

            if (model.DayOfWeekOffDays != null && model.DayOfWeekOffDays.Count > 0)
                ompanny.DayOfWeekOffDays = model.DayOfWeekOffDays;
            else
                ompanny.DayOfWeekOffDays = null;


            if (ompanny.IsConfigured)
                companyService.UpdateProgress(ompanny, 2);

            accDbcontext.CompanyAccounts.Update(ompanny);
            await accDbcontext.SaveChangesAsync();

            return await ReturnStepDone(model.Id, 2);
            //backgroundJobClient.Enqueue(() => synchronizationService.SyncCompanyOnMasterPayrolDb(model.Id, model.Name));
            //return await Change(model.Id);
        }


        public async Task<IActionResult> AddOrUpdateConnection(int id = 0)
        {
            SetIsCreatingTempData();
            var emp = await accDbcontext.CompanyAccounts.FindAsync(id);
            if (emp == null && id != 0) return ThrowJsonError("Company not found");
            if (emp == null) emp = new CompanyAccount { };
            return PartialView("_AddOrUpdateConnection", emp);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddOrUpdateConnection(CompanyAccount model)
        //{
        //    var ompanny = await accDbcontext.CompanyAccounts.FindAsync(model.Id);
        //    if (ompanny == null)
        //        return ThrowJsonError("Company was not found!");
            
        //    ompanny.ConfigConnectionStringName = model.ConfigConnectionStringName;
        //    ompanny.DataSource = model.DataSource;
        //    ompanny.InitialCatalog = model.InitialCatalog;
        //    ompanny.UserId = model.UserId;
        //    ompanny.IntegratedSecurity = model.IntegratedSecurity;
        //    await accDbcontext.SaveChangesAsync();

        //    backgroundJobClient.Enqueue(() => synchronizationService.SyncCompanyOnMasterPayrolDb(model.Id, model.Name));

        //    return await Index(model.Id);
        //}



        public async Task<IActionResult> ViewAccessGrants(int id = 0)
        {
            ViewBag.Company = await accDbcontext.CompanyAccounts.Where(x => x.Id == id).FirstOrDefaultAsync();
            var list = await accDbcontext.AccessGrants
                .Where(x => x.CompanyAccountId == id)
                .Include(x => x.User)
                .ToListAsync();

            return PartialView("_AccessGrants", list);
        }

        //[HttpPost]
        //public async Task<IActionResult> CheckConnection(CompanyAccount model)
        //{
        //    try
        //    {
        //        var intgSec = model.IntegratedSecurity ?? true;
        //        var connectionString = $"Server={model?.DataSource};Initial Catalog={model?.InitialCatalog};Integrated Security={intgSec.ToString()};MultipleActiveResultSets=True";
        //        if (!intgSec)
        //            connectionString += $";User Id={model.UserId};Password={model.Password}";

        //        logger.LogWarning("Checking ConnctionString:"+ connectionString);
        //        return Json(await accessGrantService.CheckConnectionAsync(connectionString));
        //    }
        //    catch (Exception ex)
        //    {
        //        return ThrowJsonError("Error: " + ex.Message);
        //    }
        //}

        public async Task<IActionResult> ApproveCompany(int id = 0)
        {
            SetIsCreatingTempData();
            var emp = await accDbcontext.CompanyAccounts.FindAsync(id);
            if (emp == null && id != 0) return ThrowJsonError("Company not found");
            if (emp == null) emp = new CompanyAccount { };
            return PartialView("_AddOrUpdateConnection", emp);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ApproveCompany(CompanyAccount model)
        //{
        //    var ompanny = await accDbcontext.CompanyAccounts.FindAsync(model.Id);
        //    if (ompanny == null)
        //        return ThrowJsonError("Company was not found!");

        //    ompanny.ConfigConnectionStringName = model.ConfigConnectionStringName;
        //    ompanny.DataSource = model.DataSource;
        //    ompanny.InitialCatalog = model.InitialCatalog;
        //    ompanny.UserId = model.UserId;
        //    ompanny.IntegratedSecurity = model.IntegratedSecurity;
        //    ompanny.ConnectionStatus = model.ConnectionStatus;
        //    await accDbcontext.SaveChangesAsync();

        //    backgroundJobClient.Enqueue(() => synchronizationService.SyncCompanyOnMasterPayrolDb(model.Id, model.Name));

        //    return await Index(model.Id);
        //}

        //public async Task<IActionResult> ChangeWorkType(int id = 0)
        //{
        //    SetIsCreatingTempData();
        //    var emp = await context.CompanyAccounts.FindAsync(id);
        //    if (emp == null && id != 0) return ThrowJsonSuccess("Company not found");
        //    if (emp == null)
        //    {
        //        emp = new CompanyAccount();
        //        if (emp.CompanyWorkTimes.Any())
        //            emp.CompanyWorkTimes.Add(new CompanyWorkTime { })
        //    }
        //    return PartialView("_ChangeWorkType", emp);
        //}

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


        #region Company Werk TImings
            

        public async Task<IActionResult> RemoveBreakTime(int cmpId, int id = 0)
        {
            var cmp = await context.Companies.Where(x => x.Id == cmpId).FirstOrDefaultAsync();
            if (cmp == null) return ThrowJsonError("Company not found");

            var cmpBreakTime = await context.CompanyWorkBreakTimes.FirstOrDefaultAsync(x => x.CompanyId == cmpId && x.Id == id);
            if (cmpBreakTime == null)
                return ThrowJsonError("Company Break time not found");

            cmpBreakTime.IsActive = false;
            context.CompanyWorkBreakTimes.Remove(cmpBreakTime);
            await context.SaveChangesAsync();

            return Ok();
        }

        public async Task<IActionResult> RemoveWorkTime(int cmpId, int id = 0)
        {
            var cmp = await context.Companies.Where(x => x.Id == cmpId).FirstOrDefaultAsync();
            if (cmp == null) return ThrowJsonError("Company not found");

            var cmpWorkTime = await context.CompanyWorkTimes.FirstOrDefaultAsync(x => x.CompanyId == cmpId && x.Id == id);
            if (cmpWorkTime == null)
                return ThrowJsonError("Company work time not found");

            cmpWorkTime.IsActive = false;
            context.CompanyWorkTimes.Remove(cmpWorkTime);
            await context.SaveChangesAsync();

            return Ok();
        }


        public async Task<IActionResult> ChangeWorkType(int id = 0)
        {
            SetIsCreatingTempData();
            var ompanny = await context.Companies.Where(x => x.Id == id)
                .Include(x => x.WorkTimes)
                .Include(x => x.BreakTimes)
                .FirstOrDefaultAsync();
            if (ompanny == null && id != 0) return ThrowJsonError("Company not found");

            if (ompanny.BreakTimes != null)
                ompanny.BreakTimes.RemoveAll(x => x.IsActive == false);
            if (ompanny.WorkTimes != null)
                ompanny.WorkTimes.RemoveAll(x => x.IsActive == false);

            ViewBag.EnableUpdateType = await accDbcontext.CompanyAccounts.Where(a => a.Id == id)
                .CountAsync(a => a.Status == CompanyStatus.Approved) > 0;

            ompanny.CreateNewWorkHour = ompanny.CreateNewBreakHour = false;
            return PartialView("_ChangeWorkType", ompanny);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeWorkType(Company model)
        {
            if (model.WorkTimes != null && model.WorkTimes.Any())
                if (model.WorkTimes.Count(a => string.IsNullOrWhiteSpace(a.ShiftName) || string.IsNullOrWhiteSpace(a.ColorCombination)) > 0) return ThrowJsonError("Please enter work time name or color!");


            var ompanny = await context.Companies.Where(x => x.Id == model.Id)
                .Include(x => x.WorkTimes)
                .Include(x => x.BreakTimes)
                .FirstOrDefaultAsync();
            if (ompanny == null)
                return ThrowJsonError("Company was not found!");

            var cmpAccount = await accDbcontext.CompanyAccounts.FindAsync(model.Id);

            ompanny.WorkType = model.WorkType;

            var breakHourErr = false;
            var stringBrekHr = "";
            if (model.WorkType == CompanyWorkType.FixedTime)
            {
                if (ompanny.WorkTimes != null && ompanny.WorkTimes.Any())
                    if (ompanny.WorkTimes.Count > 1)
                        ompanny.WorkTimes.Skip(1).ToList().ForEach(t => context.CompanyWorkTimes.Remove(t));

                ompanny.WorkTimes.ForEach(x =>
                {
                    x.ShiftName = model.WorkTimes != null ? model.WorkTimes.FirstOrDefault(w => x.Id == w.Id)?.ShiftName : x.ShiftName;
                    x.StartTime = model.WorkTimes != null ? model.WorkTimes.FirstOrDefault(w => x.Id == w.Id)?.StartTime ?? x.StartTime : x.StartTime;
                    x.EndTime = model.WorkTimes != null ? model.WorkTimes.FirstOrDefault(w => x.Id == w.Id)?.EndTime ?? x.EndTime : x.EndTime;
                    x.ColorCombination = model.WorkTimes != null ? model.WorkTimes.FirstOrDefault(w => x.Id == w.Id)?.ColorCombination ?? x.ColorCombination : x.ColorCombination;

                    //workTimes += (x.EndTime - x.StartTime).TotalHours;
                });


                if (ompanny.BreakTimes != null && ompanny.BreakTimes.Any())
                    if (ompanny.BreakTimes.Count > 1)
                        ompanny.BreakTimes.Skip(1).ToList().ForEach(t => context.CompanyWorkBreakTimes.Remove(t));
                ompanny.BreakTimes.ForEach(x =>
                {
                    x.StartTime = model.BreakTimes != null ? model.BreakTimes.FirstOrDefault(w => x.Id == w.Id)?.StartTime ?? x.StartTime : x.StartTime;
                    x.EndTime = model.BreakTimes != null ? model.BreakTimes.FirstOrDefault(w => x.Id == w.Id)?.EndTime ?? x.EndTime : x.EndTime;

                    if (!model.WorkTimes.Any(w => w.StartTime <= x.StartTime && w.EndTime >= x.EndTime) && !breakHourErr)
                    {
                        if (!model.WorkTimes.Any(w => w._StartDateTime <= x._StartTime && w._EndDateTime >= x._EndTime) && !breakHourErr)
                        {
                            breakHourErr = true;
                            stringBrekHr = x.StartTime.ToString("hh\\:mm") + " to " + x.EndTime.ToString("hh\\:mm");
                        }
                    }
                });
            }
            else
            {
                if (ompanny.WorkTimes != null && ompanny.WorkTimes.Any())
                    ompanny.WorkTimes.ForEach(x =>
                    {
                        x.ShiftName = model.WorkTimes != null ? model.WorkTimes.FirstOrDefault(w => x.Id == w.Id)?.ShiftName : x.ShiftName;
                        x.StartTime = model.WorkTimes != null ? model.WorkTimes.FirstOrDefault(w => x.Id == w.Id)?.StartTime ?? x.StartTime : x.StartTime;
                        x.EndTime = model.WorkTimes != null ? model.WorkTimes.FirstOrDefault(w => x.Id == w.Id)?.EndTime ?? x.EndTime : x.EndTime;
                        x.ColorCombination = model.WorkTimes != null ? model.WorkTimes.FirstOrDefault(w => x.Id == w.Id)?.ColorCombination ?? x.ColorCombination : x.ColorCombination;

                        //workTimes += (x.EndTime - x.StartTime).TotalHours;
                    });

                if (ompanny.BreakTimes != null && ompanny.BreakTimes.Any())
                    ompanny.BreakTimes.ForEach(x =>
                    {
                        x.StartTime = model.BreakTimes != null ? model.BreakTimes.FirstOrDefault(w => x.Id == w.Id)?.StartTime ?? x.StartTime : x.StartTime;
                        x.EndTime = model.BreakTimes != null ? model.BreakTimes.FirstOrDefault(w => x.Id == w.Id)?.EndTime ?? x.EndTime : x.EndTime;

                        if (!model.WorkTimes.Any(w => w.StartTime <= x.StartTime && w.EndTime >= x.EndTime) && !breakHourErr)
                        {
                            if (!model.WorkTimes.Any(w => w._StartDateTime <= x._StartTime && w._EndDateTime >= x._EndTime) && !breakHourErr)
                            {
                                breakHourErr = true;
                                stringBrekHr = x.StartTime.ToString("hh\\:mm") + " to " + x.EndTime.ToString("hh\\:mm");
                            }
                        }
                    });
            }

            if (breakHourErr && model.CreateNewBreakHour)
                return ThrowJsonError($"Break hour [{stringBrekHr}] is not within any work hour");

            if (ompanny.WorkTimes != null && ompanny.WorkTimes.Any())
            {
                if (ompanny.WorkTimes.Any(x => x._StartDateTime >= x._EndDateTime))
                    return ThrowJsonError("Work Start time cannot be greater than End time");
            }
            if (ompanny.BreakTimes != null && ompanny.BreakTimes.Any())
            {
                if (ompanny.BreakTimes.Any(x => x._StartTime >= x._EndTime))
                    return ThrowJsonError("Break Start time cannot be greater than End time");
            }
            //ompanny.DataSource = model.DataSource;
            //ompanny.InitialCatalog = model.InitialCatalog;
            //ompanny.UserId = model.UserId;
            //ompanny.IntegratedSecurity = model.IntegratedSecurity;
            //ompanny.ConnectionStatus = model.ConnectionStatus;

            if (model.CreateNewBreakHour)
            {
                if (ompanny.BreakTimes == null)
                    ompanny.BreakTimes = new List<CompanyWorkBreakTime>();
                if (ompanny.WorkTimes == null || ompanny.WorkTimes.Any() == false)
                    return ThrowJsonError("Please create work time first");
                ompanny.BreakTimes.Add(new CompanyWorkBreakTime());
                model.CreateAnotherShift = true;
            }
            if (model.CreateNewWorkHour)
            {
                if (ompanny.WorkTimes == null)
                    ompanny.WorkTimes = new List<CompanyWorkTime>();
                ompanny.WorkTimes.Add(new CompanyWorkTime
                { StartTime = ompanny.WorkTimes.Any() ? ompanny.WorkTimes.Max(x => x.EndTime) : new TimeSpan(18, 0, 0) });
            }


            // if -master-save-button- => run validate all work and break times
            if (!model.CreateNewWorkHour && !model.CreateNewBreakHour && model.WorkType == CompanyWorkType.Shifts && !model.IsChangeWorkType)
            {
                var sum = ompanny.WorkTimes.Sum(x => x.TotalHours);
                logger.LogWarning("Total sum of hours work time: " + sum);
                if (sum < 0)
                    return ThrowJsonError("Work times could not be calculated, make sure timings are ordered as readable");
                //if (sum != 24.00f)
                //    return ThrowJsonError("Total working hours must be equal to 24 hours");
            }

            ompanny.FlexibleBreakHourCount = model.FlexibleBreakHourCount;
            ompanny.EarlyOntimeMinutes = model.EarlyOntimeMinutes;
            ompanny.IsBreakHourStrict = model.IsBreakHourStrict;
            //context.CompanyWorkTimes.UpdateRange(ompanny.WorkTimes);
            if (model.CreateNewWorkHour || model.CreateNewBreakHour || model.IsSaveOtf)
            {
                await context.SaveChangesAsync();
                ViewBag.EnableUpdateType = cmpAccount.Status == CompanyStatus.Approved;

                if (model.CreateNewBreakHour)
                    ViewData["isBreak"] = true;

                return PartialView("_ChangeWorkTypeHoursTable", ompanny);
                //return await ChangeWorkType(ompanny.Id);
            }

            if (ompanny.WorkTimes.Any())
            {
                companyService.UpdateProgress(cmpAccount, 4);
                accDbcontext.CompanyAccounts.Update(cmpAccount);
                await accDbcontext.SaveChangesAsync();
                context.Companies.Update(ompanny);
            }

            await context.SaveChangesAsync();
            if(cmpAccount.Status == CompanyStatus.Approved && model.UpdateHistory)
                backgroundJobClient.Enqueue(() => synchronizationService.SyncSchedulesAttendanceOnCompanyWorkTimeUpdate(model.Id, model.Name));

            //if (cmpAccount.Status != CompanyStatus.Draft)
            //    backgroundJobClient.Enqueue(() => synchronizationService.SyncCompanyOnMasterPayrolDb(model.Id, model.Name));

            return await ReturnStepDone(model.Id, 4);
            //return RedirectToAction("change", new { id = model.Id });
            //return await ChangeWorkType(ompanny.Id);
        }

        #endregion


        public async Task<IActionResult> GetFolders(int id)
        {
            var cmp = await context.Companies
                .Include(a=>a.CompanyFolders)
                .ThenInclude(a=> a.CompanyFiles)
                .FirstOrDefaultAsync(a=> a.Id == id);

            if (cmp == null) return ThrowJsonError("Company not found");
            return PartialView("_Folders", cmp);
        }
        public async Task<IActionResult> GetFiles(int id)
        {
            var cmp = await context.CompanyFiles
                .Where(a => a.CompanyFolderId == id)
                .Include(a => a.CompanyFolder)
                .ThenInclude(a => a.CompanyFiles)
                .ToListAsync();

            ViewBag.FolderId = id;
            if (cmp == null) return ThrowJsonError("Company Filess were not found");
            return PartialView("_Files", cmp);
        }

        public async Task<IActionResult> AddOrUpdateFolder(int id, int fId = 0)
        {
            var cmp = await accDbcontext.CompanyAccounts.FindAsync(id);
            if (cmp == null) return ThrowJsonError("Company not found");


            var folder = new CompanyFolder { CompanyId = id };
            if(fId> 0)
                folder = await context.CompanyFolders.FindAsync(fId);
            if (fId > 0 && folder == null) return ThrowJsonError("Company Folder was not found");
            

            return PartialView("_AddOrUpdateFolder", folder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateFolder(CompanyFolder model)
        {
            var cmp = await context.Companies.FindAsync(model.CompanyId);
            if (cmp == null) return ThrowJsonError("Company not found");
            if (model.Id <= 0)
                context.CompanyFolders.Add(new CompanyFolder { CompanyId = model.CompanyId, Name = model.Name, Color = model.Color });
            else
                context.CompanyFolders.Update(model);

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(GetFolders), new { id = model.CompanyId });
        }

        public async Task<IActionResult> AddFile(int id)
        {
            var exists = await context.CompanyFolders.AnyAsync(x=> x.Id == id);
            if (!exists) return ThrowJsonError("Company Folder not found");
            return PartialView("_AddFile", new CompanyFile { CompanyFolderId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFile(CompanyFile model, [FromForm]IFormFile file)
        {
            if(ModelState.IsValid)
            {
                bool hasFiles = fileUploadService.HasFilesReadyForUpload(file);
                if (hasFiles)
                {
                    if (string.IsNullOrWhiteSpace(model.Name))
                        return ThrowJsonError($"Name is required!");

                    if (fileUploadService.GetFileSizeInMb(file) >= UploadSetting.MaxFileSizeMb)
                        return ThrowJsonError($"File is too huge, only file size up to {UploadSetting.MaxFileSizeMb}MB are allowed!");

                    if (!fileUploadService.IsAllowedFileType(file, UploadSetting.FileTypes))
                        return ThrowJsonError($"Uploaded file type is not allowed!");

                    var folder = await context.CompanyFolders.FindAsync(model.CompanyFolderId);
                    if(folder == null)
                        return ThrowJsonError($"Folder was not found!");


                    string fileUrl = await fileUploadService.UploadFles(file, "files");
                    //model.CompanyId = folder.CompanyId;
                    model.ContentLength = (int)file.Length;
                    model.ContentType = file.ContentType;
                    model.FileUrl = fileUrl;

                    model.FileSizeInMb = Math.Round(((int)file.Length / 1024f) / 1024f, 2);
                    model.FileExtension = Path.GetExtension(file.FileName);
                    model.FileName = Path.GetFileName(file.FileName);
                    model.IsSignatureAvailable = model.FileExtension.ToLower().Contains(".pdf");
                    model.Name = model.Name;

                    context.CompanyFiles.Add(model);
                    await context.SaveChangesAsync();

                    return RedirectToAction(nameof(GetFiles), new { id = model.CompanyFolderId });
                }

                return ThrowJsonError("Please upload an image");
            }

            return ThrowJsonError(ModelState);
        }


        #region Public Holiday


        public async Task<IActionResult> GetPublicHoliday(int cmpId, int year)
        {
            ViewBag.CompanyId = cmpId;
            ViewBag.Year = year;
            return PartialView("_PublicHolidays", await context.CompanyPublicHolidays.Where(a => a.CompanyId == cmpId && a.Year == year.ToString()).OrderBy(a=> a.Date).ToListAsync());
        }

        public async Task<IActionResult> AddOrUpdatePublicHoliday(int cmpId, int id = 0)
        {
            var cmp = await context.Companies.Where(x => x.Id == cmpId)
                .Include(x => x.CompanyPublicHolidays).FirstOrDefaultAsync();
            if (cmp == null) return ThrowJsonError("Company not found");

            var pubHoli = new CompanyPublicHoliday { CompanyId = cmpId,Date = DateTime.Today };
            if (id > 0)
            {
                pubHoli = cmp.CompanyPublicHolidays.FirstOrDefault(a => a.Id == id);
                if (pubHoli == null) return ThrowJsonError("Company not found");
            }

            var start = DateTime.Now.Year - 5;
            var end = DateTime.Now.Year + 5;
            ViewBag.Years = new SelectList(Enumerable.Range(1, (end - start)).Select(a=> start + a).ToList(), pubHoli?.Year);

            return PartialView("_AddOrUpdatePublicHoliday", pubHoli);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdatePublicHoliday(CompanyPublicHoliday model)
        {
            if (!ModelState.IsValid) return ThrowJsonError(ModelState);


            var pubHoli = new CompanyPublicHoliday();
            if (model.Id > 0)
            {
                pubHoli = await context.CompanyPublicHolidays.FirstOrDefaultAsync(a => a.Id == model.Id);
                if (pubHoli == null) return ThrowJsonError("Company not found");

                pubHoli.Name = model.Name;
                pubHoli.Description = model.Description;
                pubHoli.Year = model.Date.Year.ToString();
                pubHoli.Date = model.Date;
                context.CompanyPublicHolidays.Update(pubHoli);
            }
            else
            {
                if (model.HasRange && model.Date2.HasValue)
                {
                    var dadd = new List<CompanyPublicHoliday>();
                    for (var d = model.Date; d <= model.Date2; d = d.AddDays(1))
                    {
                        dadd.Add(new CompanyPublicHoliday
                        {
                            CompanyId = model.CompanyId,
                            Date = d,
                            Name = model.Name,
                            Description = model.Description,
                            Year = d.Year.ToString(),
                        });
                    }
                    if (dadd.Any())
                        context.CompanyPublicHolidays.AddRange(dadd);
                }
                else
                {
                    model.Year = model.Date.Year.ToString();
                    context.CompanyPublicHolidays.Add(model);
                }
            }

            var cmpId = model.Id > 0 ? pubHoli.CompanyId : model.CompanyId;
            var id = model.Id > 0 ? pubHoli.Id : 0;;

            await context.SaveChangesAsync();
            if (id == 0)
                id = model.Id;

            return RedirectToAction(nameof(GetPublicHoliday), new { cmpId, year = DateTime.Now.Year });
        }

        [HttpPost]
        public async Task<IActionResult> RemovePublicHoliday(int cmpId, int id)
        {
            var pubHoli = await context.CompanyPublicHolidays.FirstOrDefaultAsync(a => a.Id == id && a.CompanyId == cmpId);
            if (pubHoli == null) return ThrowJsonError("Company not found");
            context.CompanyPublicHolidays.Remove(pubHoli);

            return Ok("Removed");
        }

        #endregion
        


        public async Task<IActionResult> AddOrUpdateKpiConfig(int id, string kpi = null)
        {
            var emp = await accDbcontext.CompanyAccounts.FindAsync(id);
            if (emp == null) return ThrowJsonError("Company not found");

            var data = new KpiConfig() {  CmpId = id };
            if (kpi != null)
                data = emp.KpiConfig.FirstOrDefault(a => a.Kpi == kpi);

            ViewBag.Kpi = new SelectList(typeof(EmployeeInteractionAgg).GetProperties()
                            .Select(x => x.Name).ToList(), data.Kpi);
            return PartialView("_AddOrUpdateKpiConfig", data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateKpiConfig(KpiConfig model)
        {
            if (ModelState.IsValid)
            {
                if (model.Best == model.Worst) return ThrowJsonError("Best and Worst values cannot be same");
                if (model.Weight < 0 || model.Weight > 1) return ThrowJsonError("Weihtage must be between 0 and 1");
            }

            if (ModelState.IsValid)
            {
                var cmp = await accDbcontext.CompanyAccounts.FindAsync(model.CmpId);
                if (cmp == null) return ThrowJsonError("Company not found");
                var curreConfig = cmp.KpiConfig?.ToList() ?? new List<KpiConfig>();

                if (curreConfig.Any(a => a.Kpi == model.Kpi))
                {
                    var _ = curreConfig.First(a => a.Kpi == model.Kpi);
                    _.Weight = model.Weight;
                    _.WorkType = model.WorkType;
                    _.Best = model.Best;
                    _.Worst = model.Worst;
                    _.DisplayName = !string.IsNullOrWhiteSpace(model.DisplayName) ?  model.DisplayName : model.Kpi;
                    _.DisplayOrder = model.DisplayOrder;
                }
                else
                {
                    curreConfig.Add(model);
                }

                cmp.KpiConfig = curreConfig.ToList();
                cmp.IsKpiConfigured = curreConfig.Sum(a => a.Weight) == 1;
                bool isSaved = await accDbcontext.SaveChangesAsync() > 0;


                return PartialView("_ListingKpi", await accDbcontext.CompanyAccounts.FindAsync(model.CmpId));
            }

            return ThrowJsonError(ModelState);
        }





        public async Task<IActionResult> AddOrUpdateReminder(int id, int? remId = null)
        {
            // check has role
            EmployeeRole empRole = null;
            var rem = await context.Reminders.FirstOrDefaultAsync(a => a.Id == remId && a.EmployeeRoleId == id);
            if (rem == null && remId.HasValue) return ThrowJsonError("Reminder was not found");

            //if(empRole == null)
            //{
            //    foreach (var r in typeof(Roles.Company).GetFields())
            //    {
            //        context.EmployeeRoles.Add(new EmployeeRole
            //        {
            //            Role = r.Name,
            //            CalendarDefaults = Calendars.List.ToDictionary(a => a.Item1, a => EmployeeSelectorRole.None),
            //            CompanyId = cmp.Id,
            //            Description = ""
            //        });
            //    } 
            //    //await context.EmployeeRoles.AddRangeAsync(defaultRoles);
            //    await context.SaveChangesAsync();

            //    empRole = await context.EmployeeRoles.FirstOrDefaultAsync(a => a.CompanyId == cmp.Id && a.Role.Equals(role));
            //}
            if (rem == null)
                rem = new Reminder() { EmployeeRoleId = id, In = RemindIn.weeks, When = 1 };

            return PartialView("_AddOrUpdateReminder", rem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateReminder(Reminder model)
        {
            if (ModelState.IsValid)
            {
                if (model.When <= 0) return ThrowJsonError("Please enter when days or week value");
                //if (model.Weight < 0 || model.Weight > 1) return ThrowJsonError("Weihtage must be between 0 and 1");
            }

            if (ModelState.IsValid)
            {
                var cmp = await context.EmployeeRoles.FindAsync(model.EmployeeRoleId);
                if (cmp == null) return ThrowJsonError("Role not found");

                if (model.Id > 0)
                {
                    var _ = await context.Reminders.FirstOrDefaultAsync(x => x.Id == model.Id);
                    if (_ == null) return ThrowJsonError("Reminder not found");
                    _.About = model.About;
                    _.Of = model.Of;
                    _.When = model.When;
                    _.BeforeOrAfter = model.BeforeOrAfter;
                    _.In = model.In;
                    _.RemindAction = model.RemindAction;
                    _.Note = model.Note;

                    context.Reminders.Update(_);
                }
                else
                {
                    context.Reminders.Add(model);
                }


                bool isSaved = await context.SaveChangesAsync() > 0;


                return RedirectToAction(nameof(ViewReminders), new { id = model.EmployeeRoleId });
            }

            return ThrowJsonError(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrRemoveEmployeeToRole(int cmpId, int empId, int roleId)
        {
            //if (inrol)
            //{
            //    if (model.When <= 0) return ThrowJsonError("Please enter when days or week value");
            //    //if (model.Weight < 0 || model.Weight > 1) return ThrowJsonError("Weihtage must be between 0 and 1");
            //}

            if (ModelState.IsValid)
            {
                var cmp = await context.EmployeeRoleRelations.FindAsync(empId, roleId);

                if (cmp == null)
                {
                    await context.EmployeeRoleRelations.AddAsync(new EmployeeRoleRelation {
                        EmployeeId = empId,
                        EmployeeRoleId = roleId
                    });
                }
                else
                {
                    context.EmployeeRoleRelations.Remove(cmp);
                }


                bool isSaved = await context.SaveChangesAsync() > 0;
                return Ok(isSaved + " saved");
            }

            return ThrowJsonError(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrRemoveEmpRole(EmployeeRole model)
        {
            //if (inrol)
            //{
            //    if (model.When <= 0) return ThrowJsonError("Please enter when days or week value");
            //    //if (model.Weight < 0 || model.Weight > 1) return ThrowJsonError("Weihtage must be between 0 and 1");
            //}
            var cmp = await context.EmployeeRoles.FindAsync(model.Id);
            if (cmp == null) return ThrowJsonError("Role not found");

            if (model.CalendarDefaults != null)
                cmp.CalendarDefaults = model.CalendarDefaults;

            if (model.UserAccessRights != null && model.UserAccessRights.Any())
                cmp.UserAccessRights = model.UserAccessRights; //.ToDictionary(a=> a.Key, a=>a.Value);

            if (!string.IsNullOrWhiteSpace(model.Role) && !model.Role.Equals(cmp.Role))
                cmp.Role = model.Role;

            if (!string.IsNullOrWhiteSpace(model.Description) && !model.Description.Equals(cmp.Description))
                cmp.Description = model.Description;

            cmp.Enable2fa = model.Enable2fa;

            var isSaved = await context.SaveChangesAsync();
            return Ok(isSaved + " saved");
        }

        public async Task<IActionResult> ViewReminders(int id)
        {
            return PartialView("_ViewReminders", await context.Reminders.Where(a => a.EmployeeRoleId == id).Include(a=> a.EmployeeRole).ToListAsync());
        }


        public async Task<IActionResult> AddOrUpdateProcessConfig(int cmpId, RequestType type, int id = 0, int? dayOffId= null)
        {
            RequestApprovalConfig dh = null;
            var nextStep = await context.RequestApprovalConfigs.Where(a => a.CompanyId == cmpId && a.RequestType == type && a.DayOffId == (type ==
        RequestType.Leave ? dayOffId : a.DayOffId))
                .OrderByDescending(a => a.Step)
                .Select(a=> a.Step)
                .FirstOrDefaultAsync();
            if (id == 0)
                dh = new RequestApprovalConfig { CompanyId = cmpId, RequestType = type, Step = ++nextStep, DayOffId = dayOffId };
            else
            {
                dh = await context.RequestApprovalConfigs.FindAsync(id);
                if (dh == null)
                    return ThrowJsonError("Process config was not found");
            }

            //var dpts = await cmpanyService.GetDepartmentsOfCurremtCompany(cmpId);
            //ViewBag.DepartmentId = new SelectList(dpts, "Id", "Name", dh.DepartmentId);

            var empls = await companyService.GetEmployeesOfCurremtCompany(cmpId);
            ViewBag.EmployeeIds = new SelectList(empls, "Id", "Name", dh.EmployeeId);
            ViewBag.EmployeeRoleId = new SelectList(await companyService.GetEmployeesRolesOfCurremtCompany(cmpId), "Id", "Role", dh.EmployeeRoleId);

            if(dh.RequestType == RequestType.Leave)
                ViewBag.DayOffId = new SelectList(await companyService.GetDayOffs(cmpId), "Id", "Name", dh.DayOffId);

            return PartialView("_AddOrUpdateProcessConfig", dh);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateProcessConfig(RequestApprovalConfig model)
        {
            if (ModelState.IsValid)
            {
                if (model.RequestProceessConfigActionBy == RequestProceessConfigActionBy.EmployeesWithRole && model.EmployeeRoleId <= 0) return ThrowJsonError("Please choose employee role");
                if (model.RequestProceessConfigActionBy == RequestProceessConfigActionBy.SpecificEmployee && model.EmployeeId <= 0) return ThrowJsonError("Please choose a sepcific employee");

                if (model.RequestType == RequestType.Leave  && model.DayOffId <= 0) return ThrowJsonError("Please choose a sepcific dayoff or leave");


                if (model.RequestProceessConfigActionBy == RequestProceessConfigActionBy.AutoActionAfterHours)
                {
                    if(!model.AutoAction.HasValue) return ThrowJsonError("Auto action is required");
                    if (model.AutoAction.Value != WorkItemStatus.Approved &&
                        model.AutoAction.Value != WorkItemStatus.Rejected) return ThrowJsonError("Auto action is invalid");

                    if(model.AutoActionAfterHours <= 0) return ThrowJsonError("Auto action after hours must be greater than zero");

                    if (model.AutoAction.Value == WorkItemStatus.Rejected && string.IsNullOrWhiteSpace(model.AutoActionSummary)) return ThrowJsonError("Auto action summary is required if action is rejected");
                }
                //if (await context.DepartmentHeads.AnyAsync(a => a.DepartmentId == model.DepartmentId &&
                //     (model.Id == 0 || model.Id != a.Id) && a.EmployeeId == model.EmployeeId))
                //    return ThrowJsonError("Employee already is assigned as manager to this department");

                //if (!await context.Employees.AnyAsync(a => a.Department.CompanyId == model.CompanyId &&
                //     a.Id == model.EmployeeId))
                //    return ThrowJsonError("Employee doesn't belong to this Company");


                if (model.Id == 0)
                {
                    // create
                    if (model.DayOffId.HasValue)
                    {
                        var nextStep = await context.RequestApprovalConfigs.Where(a => a.CompanyId == model.CompanyId && a.RequestType == model.RequestType && a.DayOffId == (model.RequestType ==
                    RequestType.Leave ? model.DayOffId : a.DayOffId))
                            .OrderByDescending(a => a.Step)
                            .Select(a => a.Step)
                            .FirstOrDefaultAsync();
                        model.Step = ++nextStep;
                    }
                    context.RequestApprovalConfigs.Add(model);
                }
                else
                {
                    context.RequestApprovalConfigs.Update(model);
                }

                context.SaveChanges();
                return RedirectToAction(nameof(ViewCompanyRequestProcessConfigs), new { cmpId = model.CompanyId });
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveProcessConfig(int cmpId, int id, RequestType type)
        {
            var currStep = await context.RequestApprovalConfigs.FirstOrDefaultAsync(a => a.Id == id && a.CompanyId == cmpId && a.RequestType == type);
            if (currStep == null)
                return ThrowJsonError("oops! This step was not found!");


            var lastStepInType = await context.RequestApprovalConfigs.Where(a => a.CompanyId == cmpId && a.RequestType == type && a.DayOffId == (type ==
        RequestType.Leave ? currStep.DayOffId : a.DayOffId)).OrderByDescending(a => a.Step).FirstOrDefaultAsync();

            if (lastStepInType.Id != currStep.Id)
                return ThrowJsonError("You should first remove the last step!");

            context.RequestApprovalConfigs.Remove(lastStepInType);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(ViewCompanyRequestProcessConfigs), new { cmpId = cmpId });
        }

        public async Task<IActionResult> ImportEmployees(int id)
        {
            ViewBag.Id = id;
            return View();
        }


        public async Task<IActionResult> ImportJobs(int id)
        {
            ViewBag.Id = id;
            return View();
        }

        public async Task<IActionResult> ImportMasterData(int? id = null)
        {
            ViewBag.Id = id ?? userResolverService.GetCompanyId();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InportJobsData(int id, [FromForm] IFormFile file)
        {
            bool hasFiles = fileUploadService.HasFilesReadyForUpload(file);
            ViewBag.Id = id;

            if (hasFiles)
            {
                if (!Path.GetExtension(file.FileName).ToLower().Equals(".xlsx"))
                    return ThrowJsonError("Files type is invalid!");

                int line = 1;
                int col = 0;

                ViewBag.IsError = false;
                System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                string _rootPath = hostingEnvironment.WebRootPath;
                var _filePath = Path.Combine(_rootPath, file.FileName);

                using (var fileStream = new FileStream(_filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var outFilePath = Path.Combine(_rootPath, "converted.json");
                var inFilePath = _filePath;
                var listData = new List<BulkImportJobVm>();
                var erroorDict = new Dictionary<string, string>();
                BulkImportJobVm _dataItem = null;
                var total = 0;
                using (var inFile = System.IO.File.Open(inFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var outFile = System.IO.File.CreateText(outFilePath))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(inFile, new ExcelReaderConfiguration()
                        { FallbackEncoding = Encoding.GetEncoding(1252) }))
                        {
                            reader.Read(); //SKIP FIRST ROW, it's TITLES.
                            //do
                            //{
                            while (reader.Read())
                            {
                                try
                                {

                                    line++;
                                    _dataItem = new BulkImportJobVm();


                                    _dataItem.Location = reader.GetString(col++);
                                    _dataItem.Department = reader.GetString(col++);
                                    _dataItem.JobTitle = reader.GetString(col++);

                                    //var empId = reader.GetString(col++);
                                    _dataItem.JobID = reader.GetString(col++);

                                    _dataItem.JobType = (JobType)Enum.Parse(typeof(JobType), reader.GetString(col++));

                                    _dataItem.Classification = reader.GetString(col++);
                                    _dataItem.Total = Convert.ToInt32(reader.GetDouble(col++));
                                    total += _dataItem.Total;

                                    listData.Add(_dataItem);
                                }
                                catch (Exception ex)
                                {
                                    erroorDict.Add($"row {line} column '{_dataItem.GetType().GetProperties()[col - 1].Name}' ({col})", ex.Message);
                                }
                                finally
                                {
                                    col = 0;
                                }
                            }
                            // } while (reader.NextResult()); //Move to NEXT SHEET
                        }
                    }
                }


                ViewBag.Data = listData;
                ViewBag.Total = total;
                ViewBag.ErroorDict = erroorDict;
                ViewBag.DataString = JsonConvert.SerializeObject(listData);

                if (erroorDict.Any())
                {

                    ViewBag.IsError = true;
                    return PartialView("_InportJobDataConfirm", id);
                }


                return PartialView("_InportJobDataConfirm", id);
            }

            return ThrowJsonError("Please upload the filled template file");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InportJobDataUpdate(int id, string data)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (!await context.Companies.AnyAsync(x => x.Id == id))
                        return ThrowJsonError("Company was not found!");

                    var _data = JsonConvert.DeserializeObject<List<BulkImportJobVm>>(data);
                    if (data == null)
                        return ThrowJsonError("data was not found!");

                    var jobs = new List<Job>();
                    //var contracts = new List<Contract>();
                    for (int i = 0; i < _data.Count(); i++)
                    {
                        for (int j = 0; j < _data[i].Total; j++)
                        {
                            jobs.Add(new Job
                            {
                                CompanyId = id,
                                LevelId = await companyService.GetOrCreateAndGetClassificationIdAsync(_data[i].Classification, id),
                                LocationId = await companyService.GetOrCreateAndGetLocationIdAsync(_data[i].Location, id),
                                DepartmentId = await companyService.GetOrCreateAndGetDepartmentIdAsync(_data[i].Department, id),
                                JobID = _data[i].JobID,
                                JobTitle = _data[i].JobTitle,
                                JobType = _data[i].JobType,
                            });
                        }
                    }

                    await context.Jobs.AddRangeAsync(jobs);
                    await context.SaveChangesAsync();


                    return Ok();
                }
                catch (Exception ex)
                {
                    return ThrowJsonError("An Error occured. " + ex.Message);
                }

            }

            return ThrowJsonError("Please upload the filled template file");
        }


        public async Task<IActionResult> InportEmployeeData(int id, string update = "")
        {
            ViewBag.Id = id;
            return PartialView("_ImportEmployeeData");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InportEmployeesData(int id, [FromForm]IFormFile file)
        {
            bool hasFiles = fileUploadService.HasFilesReadyForUpload(file);
            ViewBag.Id = id;

            if (hasFiles)
            {
                if (!Path.GetExtension(file.FileName).ToLower().Equals(".xlsx"))
                    return ThrowJsonError("Files type is invalid!");

                //if (fileUploadService.GetFileSizeInMb(file) >= UploadSetting.MaxImageSizeMb)
                //    return ThrowJsonError($"File is too huge, only file size up to {UploadSetting.MaxFileSizeMb}MB are allowed!");

                //if (!fileUploadService.IsAllowedFileType(file, UploadSetting.ImageTypes))
                //    return ThrowJsonError($"Uploaded file type is not allowed!");

                //if (fileUploadService.GetFileSizeInMb(file) >= 20m)
                //    return ThrowJsonError("File is too huge, only file size up to 1MB are allowed!");

                int line = 1;
                int col = 0;

                //try
                //{
                    ViewBag.IsError = false;
                    System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    string _rootPath = hostingEnvironment.WebRootPath;
                    var _filePath = Path.Combine(_rootPath, file.FileName);

                    using (var fileStream = new FileStream(_filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    var outFilePath = Path.Combine(_rootPath, "converted.json");
                    var inFilePath = _filePath;
                    var listData = new List<BulkImportEmployeeVm>();
                    var erroorDict = new Dictionary<string, string>();
                    BulkImportEmployeeVm _dataItem = null;
                    using (var inFile = System.IO.File.Open(inFilePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var outFile = System.IO.File.CreateText(outFilePath))
                        {
                        using (var reader = ExcelReaderFactory.CreateReader(inFile, new ExcelReaderConfiguration()
                        { FallbackEncoding = Encoding.GetEncoding(1252) }))
                        {
                            reader.Read(); //SKIP FIRST ROW, it's TITLES.
                            //do
                            //{
                            while (reader.Read())
                            {
                                try
                                {

                                        line++;
                                        _dataItem = new BulkImportEmployeeVm();
                                        //peek ahead? Bail before we start anything so we don't get an empty object
                                        var empId = reader.GetString(col++);
                                        if (string.IsNullOrEmpty(empId)) break;
                                        _dataItem.EmpID = empId;

                                        _dataItem.FirstName = reader.GetString(col++);
                                        _dataItem.MiddleName = reader.GetString(col++);
                                        _dataItem.LastName = reader.GetString(col++);


                                        _dataItem.NickName = reader.GetString(col++);
                                        _dataItem.EmailAddress = reader.GetString(col++);
                                        _dataItem.PhoneWork = reader.GetString(col++);
                                        _dataItem.PhonePersonal = reader.GetString(col++);
                                        _dataItem.EmergencyContactName = reader.GetString(col++);
                                        _dataItem.EmergencyContactNumber = reader.GetString(col++);
                                        _dataItem.EmergencyContactRelation = reader.GetString(col++);
                                        _dataItem.Address = reader.GetString(col++);
                                        _dataItem.Street = reader.GetString(col++);
                                        _dataItem.ZipCode = reader.GetString(col++);

                                        _dataItem.Gender = (Gender)Enum.Parse(typeof(Gender), reader.GetString(col++));

                                        _dataItem.DepartmentName = reader.GetString(col++);
                                    if (string.IsNullOrWhiteSpace(_dataItem.DepartmentName))
                                        erroorDict.Add($"row {line} column '{nameof(_dataItem.DepartmentName)}' ({col})", "Department Name is required");

                                    _dataItem.DateOfBirth = reader.GetDateTime(col++);
                                        // DateTime.ParseExact(reader.GetString(col++), "dd-MM-yyyy", null);
                                        _dataItem.IdentityType = (IdentityType)Enum.Parse(typeof(IdentityType), reader.GetString(col++));
                                        _dataItem.IdentityNumber = reader.GetString(col++);
                                        _dataItem.DateOfJoined = reader.GetDateTime(col++);
                                        //_dataItem.DateOfJoined = DateTime.ParseExact(reader.GetString(col++), "dd-MM-yyyy", null);

                                        _dataItem.BankName = reader.GetString(col++);
                                        _dataItem.BankAccountName = reader.GetString(col++);
                                        _dataItem.BankAccountNumber = reader.GetString(col++);
                                        _dataItem.TwitterId = reader.GetString(col++);
                                        _dataItem.FacebookId = reader.GetString(col++);
                                        _dataItem.InstagramId = reader.GetString(col++);
                                        _dataItem.LinkedInId = reader.GetString(col++);


                                        _dataItem.Nationality = reader.GetString(col++);
                                        //_dataItem.Passport = reader.GetString(col++);
                                        _dataItem.VisaType = (VisaType)Enum.Parse(typeof(VisaType), reader.GetString(col++));
                                        //_dataItem.AgreementStart = DateTime.ParseExact(reader.GetString(col++), "dd-MM-yyyy", null);
                                        //_dataItem.AgreementEnd = DateTime.ParseExact(reader.GetString(col++), "dd-MM-yyyy", null);
                                        //_dataItem.AgreementType = (ContractType)Enum.Parse(typeof(ContractType), reader.GetString(col++));
                                        //_dataItem.Designation = reader.GetString(col++);

                                        listData.Add(_dataItem);
                                    }
                                    catch (Exception ex)
                                    {
                                        erroorDict.Add($"row {line} column '{_dataItem.GetType().GetProperties()[col - 1].Name}' ({col})", ex.Message);
                                    }
                                    finally
                                    {
                                        col = 0;
                                    }
                                }
                            // } while (reader.NextResult()); //Move to NEXT SHEET
                        }
                        }
                    }


                    ViewBag.Data = listData;
                    ViewBag.ErroorDict = erroorDict;
                    ViewBag.DataString =  JsonConvert.SerializeObject(listData);

                    if (erroorDict.Any())
                    {

                        ViewBag.IsError = true;
                        return PartialView("_InportEmployeeDataConfirm", id);
                    }


                //}
                //catch (Exception err)
                //{
                //    ViewBag.IsError = true;
                //    ViewBag.Line = line;
                //    ViewBag.Col = col;
                //    ViewBag.Error = err.Message;

                //    return PartialView("_InportEmployeeDataConfirm", id);
                //}

                return PartialView("_InportEmployeeDataConfirm", id);
                //string fileUrl = await fileUploadService.UploadFles(file);
                //var companyFrmDb = await context.CompanyAccounts.FindAsync(model.Id);
                //if (companyFrmDb == null) return ThrowJsonError("Company was not found");

                //companyFrmDb.LogoUrl = fileUrl;
                //await context.SaveChangesAsync();
                //backgroundJobClient.Enqueue(() => synchronizationService.SyncCompanyOnMasterPayrolDb(model.Id, model.Name));


            }

            return ThrowJsonError("Please upload an image");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> InportEmployeeDataUpdate(int id, string data)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            if(!await context.Companies.AnyAsync (x=> x.Id == id))
        //                    return ThrowJsonError("Company was not found!");

        //            var _data = JsonConvert.DeserializeObject<List<BulkImportEmployeeVm>>(data);
        //            if (data == null)
        //                return ThrowJsonError("data was not found!");

        //            var employees = new List<Employee>();
        //            //var contracts = new List<Contract>();
        //            for (int i = 0; i < _data.Count(); i++)
        //            {
        //                employees.Add(new Employee
        //                {
        //                    //EmpID = _data[i].EmpID,
        //                    FirstName = _data[i].FirstName,
        //                    LastName = _data[i].LastName,
        //                    MiddleName = _data[i].MiddleName,
        //                    NickName = _data[i].NickName,
        //                    EmailPersonal = _data[i].EmailAddress,
        //                    PhoneWork = _data[i].PhoneWork,
        //                    PhonePersonal = _data[i].PhonePersonal,
        //                    EmergencyContactName = _data[i].EmergencyContactName,
        //                    EmergencyContactNumber = _data[i].EmergencyContactNumber,
        //                    //Address = _data[i].Address,
        //                    Street = _data[i].Street,
        //                    ZipCode = _data[i].ZipCode,

        //                    Gender = _data[i].Gender,
        //                    DepartmentId = await companyService.GetOrCreateAndGetDepartmentIdAsync(_data[i].DepartmentName, id),
        //                    //DepartmentName = _data[i].DepartmentName
        //                    DateOfBirth = _data[i].DateOfBirth,
        //                    IdentityType = _data[i].IdentityType,
        //                    IdentityNumber = _data[i].IdentityNumber,
        //                    BankName = _data[i].BankName,
        //                    BankAccountName = _data[i].BankAccountName,
        //                    BankAccountNumber = _data[i].BankAccountNumber,
        //                    TwitterId = _data[i].TwitterId,
        //                    FacebookId = _data[i].FacebookId,
        //                    InstagramId = _data[i].InstagramId,
        //                    LinkedInId = _data[i].LinkedInId,
        //                    index = i,
        //                });

        //                //contracts.Add(new Contract
        //                //{
        //                //    index = i,
        //                //    StartDate = _data[i].AgreementStart,
        //                //    EndDate = _data[i].AgreementEnd,
        //                //    ContractType = _data[i].AgreementType,
        //                //    DesignationId = await companyService.GetOrCreateAndGetDesignationIdAsync(_data[i].Designation, id),
        //                //});
        //            }

        //            // create and save employees
        //            await context.Employees.AddRangeAsync(employees);
        //            await context.SaveChangesAsync();


        //            //contracts.ForEach(t => t.CompanyId = id);
        //            //var userValidator = new UserValidator<AppUser>();
        //            //for (int i = 0; i < contracts.Count(); i++)
        //            //{
        //            //    contracts[i].EmployeeId = employees.First(a => a.index == i).Id;
        //            //    payrolDbContext.Contracts.Add(contracts[i]);
        //            //    _data[contracts[i].index]._ContractCreated = await payrolDbContext.SaveChangesAsync() > 0;


        //            //    if (_data[contracts[i].index]._ContractCreated)
        //            //    {
        //            //        await companyService.UpdateEmployeeContractMapping(contracts[i].Id);


        //            //        _data[contracts[i].index]._UserAccessGrantCreated = false;
        //            //        if (DateTime.Now >= contracts[i].StartDate && DateTime.Now <= contracts[i].EndDate)
        //            //        {
                                

        //            //            // create 
        //            //            var newUser = new AppUser
        //            //            {
        //            //                UserName = _data[contracts[i].index].EmailAddress,
        //            //                Email = _data[contracts[i].index].EmailAddress,
        //            //                SendOtpAndLoginFirst = true,
        //            //                ChangePasswordOnLogin = true
        //            //                //PhoneNumber = model.AppUser.PhoneNumber,
        //            //                //NickName = model.AppUser.NickName,
        //            //                //Picture = _empl.PhotoLink
        //            //            };

        //            //            var res = await userValidator.ValidateAsync(userManager, newUser);
        //            //            if(!res.Succeeded)
        //            //            {
        //            //                _data[contracts[i].index]._UserCreated = false;
        //            //                continue;
        //            //            }

        //            //            var result = await userManager.CreateAsync(newUser, Guid.NewGuid().ToString());
        //            //            _data[contracts[i].index]._UserCreated = result.Succeeded;

        //            //            if (!result.Succeeded)
        //            //                return ThrowJsonError(result.Errors.FirstOrDefault()?.Description);

        //            //            // acesss to company
        //            //            var newAccess = new AccessGrant
        //            //            {
        //            //                CompanyAccountId = id,
        //            //                Status = AccessGrantStatus.Active,
        //            //                ApplyOnDate = contracts[i].StartDate,
        //            //                ExpiryDate = contracts[i].EndDate,
        //            //                //ContractId = model.ContractId,
        //            //                IsDefault = true,
        //            //                UserId = newUser.Id,
        //            //            };
        //            //            _data[contracts[i].index]._UserAccessGrantCreated = true;


        //            //            if (employees.Any(a => a.index == i)) {
        //            //                var _empl = await payrolDbContext.Employees.FindAsync(employees.First(a => a.index == i).Id);

        //            //                if (_empl != null && newAccess.Status == AccessGrantStatus.Active)
        //            //                {
        //            //                    _empl.HasUserAccount = true;
        //            //                    _empl.UserId = newUser.Id;
        //            //                    _empl.UserName = newUser.UserName;
        //            //                    _empl.UserPicture = _empl.Avatar;
        //            //                }
        //            //                else
        //            //                    _empl.HasUserAccount = false;


        //            //                context.AccessGrants.Add(newAccess);
        //            //                _data[contracts[i].index]._UserAccessGrantCreated = await context.SaveChangesAsync() > 0;

        //            //                _data[contracts[i].index]._EmployeeRecordUpdated = await payrolDbContext.SaveChangesAsync() > 0;
        //            //            }
        //            //        }
        //            //        else
        //            //        {
        //            //            _data[contracts[i].index]._UserAccessGrantCreated = false;
        //            //        }
        //            //    }
        //            //}

        //            return PartialView("_ImportDataSummary", _data);
        //        }
        //        catch (Exception ex)
        //        {
        //            return ThrowJsonError("An Error occured. " + ex.Message);
        //        }

        //    }

        //    return ThrowJsonError("Please upload the filled template file");
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportMasterData(int id, [FromForm] IFormFile file)
        {
            bool hasFiles = fileUploadService.HasFilesReadyForUpload(file);
            ViewBag.Id = id;

            if (hasFiles)
            {
                if (!Path.GetExtension(file.FileName).ToLower().Equals(".xlsx"))
                    return ThrowJsonError("Files type is invalid!");

                int line = 1;
                int col = 0;

                //try
                //{
                ViewBag.IsError = false;
                System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                string _rootPath = hostingEnvironment.WebRootPath;
                var _filePath = Path.Combine(_rootPath, "imports");
                if(!Directory.Exists(_filePath))
                    Directory.CreateDirectory(_filePath);

                _filePath = Path.Combine(_filePath, file.FileName);
                ViewBag.FileName = file.FileName;

                using (var fileStream = new FileStream(_filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var outFilePath = Path.Combine(_rootPath, "converted.json");
                var inFilePath = _filePath;
                String _gen = "";
                var listData = new List<BulkImportMasterVm>();
                var data =typeof(BulkImportMasterVm).GetProperties().ToDictionary(x=> x.Name, x=> ((DisplayAttribute)x.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault())?.Name ?? x.Name);

                var erroorDict = new Dictionary<string, string>();

                var fieldMetaConfig = new List<BulkImportMasterPostDataVm>();
                BulkImportMasterVm _dataItem = null;
                using (var inFile = System.IO.File.Open(inFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var outFile = System.IO.File.CreateText(outFilePath))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(inFile, new ExcelReaderConfiguration()
                        { FallbackEncoding = Encoding.GetEncoding(1252) }))
                        {
                            //reader.Read(); //SKIP FIRST ROW, it's TITLES.
                            reader.Read();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                fieldMetaConfig.Add(new BulkImportMasterPostDataVm {
                                    sxColumnIndx = i,
                                    xColumnName =reader.GetValue(i)?.ToString() ?? "",
                                    // mappedFieldName = data.Contains(reader.GetValue(i)?.ToString() ?? "") ? reader.GetValue(i)?.ToString() ?? "" : "None"
                                });
                            }

                            // while (reader.Read()){
                            //     //foreach(var d in data){
                            //         fieldMetaConfig.Add((reader.GetValue(o)?.ToString() ?? "", data.Contains(reader.GetValue(o)?.ToString() ?? "") ? reader.GetValue(o)?.ToString() ?? "" : "", default));
                            //         o++;
                            //     //}
                            // }
                            

                            //do
                            //{
                            while (reader.Read())
                            {
                                try
                                {
                                    if(line <=5){
                                        for (int i = 0; i < reader.FieldCount; i++)
                                            fieldMetaConfig.Find(x=> x.sxColumnIndx == i).sampleData.Add(reader.GetValue(i)?.ToString() ?? "");
                                    }

                                    line++;
                                    // _dataItem = new BulkImportMasterVm();
                                    // //peek ahead? Bail before we start anything so we don't get an empty object

                                    // // JOB data
                                    // _dataItem.JobID = reader.GetString(col++);
                                    // _dataItem.JobType = (JobType)Enum.Parse(typeof(JobType), reader.GetString(col++));
                                    // _dataItem.Level = reader.GetString(col++);

                                    // _dataItem.Location = reader.GetString(col++);
                                    // _dataItem.Department = reader.GetString(col++);
                                    // if (string.IsNullOrWhiteSpace(_dataItem.Department))
                                    //     erroorDict.Add($"row {line} column '{nameof(_dataItem.Department)}' ({col})", "Department Name is required");
                                    // _dataItem.JobTitle = reader.GetString(col++);
                                    // _dataItem.Reporting = reader.GetString(col++);

                                    // // EMPLOYEE id? data
                                    // var empId = reader.GetString(col++);
                                    // if (string.IsNullOrWhiteSpace(empId))
                                    //     erroorDict.Add($"row {line} column '{nameof(_dataItem.EmpID)}' ({col})", "Employee ID is required");
                                    // _dataItem.EmpID = empId;

                                    // _dataItem.FirstName = reader.GetString(col++);
                                    // _dataItem.MiddleName = reader.GetString(col++);
                                    // _dataItem.LastName = reader.GetString(col++);


                                    // _dataItem.NickName = reader.GetString(col++);

                                    // _gen = reader.GetString(col++);
                                    // if (Enum.IsDefined(typeof(Gender), _gen))
                                    //     _dataItem.Gender = (Gender)Enum.Parse(typeof(Gender), _gen);


                                    // if (!string.IsNullOrWhiteSpace(empId))
                                    //     _dataItem.DateOfJoined = reader.GetDateTime(col++);
                                    // else
                                    //     col++;

                                    // _dataItem.IdentityType = (string.IsNullOrWhiteSpace(empId)) ? IdentityType.TBD : (IdentityType)Enum.Parse(typeof(IdentityType), reader.GetString(col++));
                                    // _dataItem.IdentityNumber = reader.GetString(col++);

                                    // if (!string.IsNullOrWhiteSpace(empId))
                                    //     _dataItem.DateOfBirth = reader.GetDateTime(col++);
                                    // else
                                    //     col++;

                                    // _dataItem.EmailPersonal = reader.GetString(col++);
                                    // _dataItem.EmailWork = reader.GetString(col++);
                                    // _dataItem.PhonePersonal = reader.GetString(col++);
                                    // _dataItem.PhoneWork = reader.GetString(col++);

                                    // _dataItem.PermanentAddress = reader.GetString(col++);
                                    // _dataItem.PermanentState = reader.GetString(col++);
                                    // _dataItem.PermanentCity = reader.GetString(col++);
                                    // _dataItem.PresentAddress = reader.GetString(col++);
                                    // _dataItem.PresentState = reader.GetString(col++);
                                    // _dataItem.PresentCity = reader.GetString(col++);

                                    // _dataItem.EmergencyContactName = reader.GetString(col++);
                                    // _dataItem.EmergencyContactNumber = reader.GetString(col++);
                                    // _dataItem.EmergencyContactRelation = reader.GetString(col++);



                                    // _dataItem.BankAccountNumber = reader.GetString(col++);
                                    // _dataItem.BankAccountName = reader.GetString(col++);
                                    // _dataItem.BankName = reader.GetString(col++);
                                    // _dataItem.TwitterId = reader.GetString(col++);
                                    // _dataItem.FacebookId = reader.GetString(col++);
                                    // _dataItem.InstagramId = reader.GetString(col++);
                                    // _dataItem.LinkedInId = reader.GetString(col++);


                                    // _dataItem.Nationality = reader.GetString(col++);
                                    // _dataItem.VisaType = (string.IsNullOrEmpty(empId)) ? VisaType.Local : (VisaType)Enum.Parse(typeof(VisaType), reader.GetString(col++));
                                    // //_dataItem.Passport = reader.GetString(col++);
                                    // //_dataItem.AgreementStart = DateTime.ParseExact(reader.GetString(col++), "dd-MM-yyyy", null);
                                    // //_dataItem.AgreementEnd = DateTime.ParseExact(reader.GetString(col++), "dd-MM-yyyy", null);
                                    // //_dataItem.AgreementType = (ContractType)Enum.Parse(typeof(ContractType), reader.GetString(col++));
                                    // //_dataItem.Designation = reader.GetString(col++);

                                    // listData.Add(_dataItem);
                                }
                                catch (Exception ex)
                                {
                                    erroorDict.Add($"row {line} column '{_dataItem.GetType().GetProperties()[col - 1].Name}' ({col})", ex.Message);
                                }
                                finally
                                {
                                    col = 0;
                                }
                            }
                            // } while (reader.NextResult()); //Move to NEXT SHEET
                        }
                    }
                }

                var oldMapping = GetDataSavedInSesssion<List<BulkImportMasterPostDataVm>>();
                if(oldMapping != null){
                    fieldMetaConfig.ForEach(t=> {
                        if(oldMapping.Any(x=> x.IsMapped && x.xColumnName == t.xColumnName))
                            t.mappedFieldName = oldMapping.First(x=> x.IsMapped && x.xColumnName == t.xColumnName).mappedFieldName;
                    });
                }
                else{
                    fieldMetaConfig.ForEach(t=> {
                        if(data.Keys.Any(x=>  t.xColumnName.Equals(x)))
                            t.mappedFieldName = data.Keys.First(x=>  t.xColumnName.Equals(x));
                        else if(data.Values.Any(x=>  t.xColumnName.Equals(x)))
                            t.mappedFieldName = data.First(d=> t.xColumnName.Equals(d.Value)).Key;
                        else if(data.Values.Any(x=> x.Split(" ").Intersect(t.xColumnName.Split(" ")).Count() >= t.xColumnName.Split(" ").Length))
                            t.mappedFieldName = data.First(d=> data.Values.First(x=> x.Split(" ").Intersect(t.xColumnName.Split(" ")).Count() >= t.xColumnName.Split(" ").Length) == d.Value).Key;
                    });
                }
                // if(fieldMetaConfig.Any()){
                //     fieldMetaConfig.ForEach(t=> {
                //         var _ = listData.Select(x=> x.GetType().GetProperty(t.xColumnName).GetValue(x)?.ToString() ).Take(5).ToList();
                //         t.sampleData = _;
                //     });
                // }

                //ViewBag.JobsCount = listData.Where(a => !string.IsNullOrWhiteSpace(a.JobID)).Select(a => a.JobID).Distinct().Count();
                //ViewBag.EmplsCount Count = listData.Where(a=>!string.IsNullOrWhiteSpace(a.EmpID)).Select(a => a.EmpID).Distinct().Count();
                ViewBag.fieldMetaConfig = fieldMetaConfig;
                // ViewBag.Data = listData;
                ViewBag.RecordCount = line;
                ViewBag.CanContinueWithErrors = erroorDict.Count(a => a.Value == "Employee ID is required") == erroorDict.Count();
                ViewBag.ErroorDict = erroorDict;
                // ViewBag.DataString = JsonConvert.SerializeObject(listData);
                

                if (erroorDict.Any())
                {

                    ViewBag.IsError = true;
                    return PartialView("_InportMasterDataConfirm", id);
                }


                //}
                //catch (Exception err)
                //{
                //    ViewBag.IsError = true;
                //    ViewBag.Line = line;
                //    ViewBag.Col = col;
                //    ViewBag.Error = err.Message;

                //    return PartialView("_InportEmployeeDataConfirm", id);
                //}

                return PartialView("_InportMasterDataConfirm", id);
                //string fileUrl = await fileUploadService.UploadFles(file);
                //var companyFrmDb = await context.CompanyAccounts.FindAsync(model.Id);
                //if (companyFrmDb == null) return ThrowJsonError("Company was not found");

                //companyFrmDb.LogoUrl = fileUrl;
                //await context.SaveChangesAsync();
                //backgroundJobClient.Enqueue(() => synchronizationService.SyncCompanyOnMasterPayrolDb(model.Id, model.Name));


            }

            return ThrowJsonError("Please upload an image");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportMasterDataUpdate(int id, string data, string mapData, string fileName)
        {
            if(ModelState.IsValid){
                List<BulkImportMasterPostDataVm> mapping = JsonConvert.DeserializeObject<List<BulkImportMasterPostDataVm>>(mapData);
                SaveInSession(mapData);
                List<BulkImportMasterVm> _data = new List<BulkImportMasterVm>();

                // validate required fields
                var requFields = typeof(BulkImportMasterVm).GetProperties().Where(a=> a.IsDefined(typeof(RequiredAttribute), false)).Select(x=> x.Name).ToList();
                var mappedFieldNames = mapping.Where(x=>x.IsMapped).Select(x=> x.mappedFieldName).ToList();
                if(requFields.Intersect(mappedFieldNames).Count() < requFields.Count()){
                    // either fullName or firstName
                    var reqFirlds = requFields.Except(requFields.Intersect(mappedFieldNames));
                    if(reqFirlds.Count() == 1 && (reqFirlds.First() ==nameof(BulkImportMasterVm.FirstName) ||  reqFirlds.First() ==nameof(BulkImportMasterVm.FullName))){

                    }
                    else{
                        var emptyRequredFields = string.Join(", ", reqFirlds);
                        return ThrowJsonError("Required fields are missing, " + emptyRequredFields);
                    }
                }

                string _rootPath = hostingEnvironment.WebRootPath;
                var fromWebrootPath = Path.Combine("imports", fileName);
                var filePath = Path.Combine(_rootPath, fromWebrootPath);
                var errorList = new List<string>();
                int row = 1;
                int currIndx = 0;
                
       
                using (var inFile = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(inFile, 
                        new ExcelReaderConfiguration(){ FallbackEncoding = Encoding.GetEncoding(1252) }))
                    {
                        //reader.Read(); //SKIP FIRST ROW, it's TITLES.
                        reader.Read();

                        BulkImportMasterVm _dataItem = null;
                        bool _b = false;
                        while (reader.Read())
                        {
                            try
                            {
                                _dataItem = new BulkImportMasterVm();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    currIndx = i;
                                    if(mapping.Any(x=> x.IsMapped && x.sxColumnIndx == i)){
                                        // field is mapped
                                        var _mappedField = mapping.First(x=> x.IsMapped && x.sxColumnIndx == i);
                                        var _mappedFieldType = _dataItem.GetType().GetProperty(_mappedField.mappedFieldName);

                                        if(_mappedFieldType.Name == nameof(BulkImportMasterVm.EmpID) && (reader.GetValue(i) ?.ToString() ??"").IsMissing())
                                        {
                                            // increment and to the next row
                                            _b = true;
                                            break;
                                        }
                                        else if(_mappedFieldType.PropertyType == typeof(DateTime))
                                           _mappedFieldType.SetValue(_dataItem, reader.GetDateTime(i));
                                        else if(_mappedFieldType.PropertyType == typeof(DateTime?))
                                        {
                                            DateTime dt = DateTime.Now;
                                            if(DateTime.TryParse(reader.GetValue(i)?.ToString(), out dt))
                                               _mappedFieldType.SetValue(_dataItem, dt);
                                        }
                                        else if(_mappedFieldType.PropertyType == typeof(decimal))
                                        {
                                            var d = 0m;
                                            decimal.TryParse(reader.GetValue(i)?.ToString(), out d);
                                           _mappedFieldType.SetValue(_dataItem, d);
                                        }
                                        // else if(!reader.IsDBNull(i) && reader.GetFieldType(i).IsEnum && 
                                        //     Enum.IsDefined(reader.GetFieldType(i), reader.GetString(i)))
                                        //    _mappedFieldType.SetValue(_dataItem, Enum.Parse(reader.GetFieldType(i), reader.GetString(i)));
                                        else if(_mappedFieldType.PropertyType.IsEnum) 
                                        {
                                            if(reader.IsDBNull(i))
                                                errorList.Add($"Error in row {row} column '{mapping.First(x=> x.sxColumnIndx == currIndx).mappedFieldName}', value is empty!");
                                            else if (!Enum.IsDefined(_mappedFieldType.PropertyType, reader.GetValue(i)?.ToString()))
                                                errorList.Add($"Error in row {row} column '{mapping.First(x=> x.sxColumnIndx == currIndx).mappedFieldName}', Value '{reader.GetValue(i)?.ToString()}' is not defined, please chcek allowed list of values!");
                                            else
                                            _mappedFieldType.SetValue(_dataItem, Enum.Parse(_mappedFieldType.PropertyType, reader.GetValue(i)?.ToString()));
                                        }
                                        else if(!reader.IsDBNull(i) && _mappedFieldType.PropertyType == typeof(int))
                                           _mappedFieldType.SetValue(_dataItem, Convert.ToInt32(reader.GetValue(i)?.ToString() ?? "0"));
                                        else
                                           _mappedFieldType.SetValue(_dataItem, (reader.GetValue(i) ?? "")?.ToString()?.Trim() ?? "");
                                    }
                                }

                                if (_b)
                                    _b = false;
                                else
                                    _data.Add(_dataItem);

                            }
                            catch (System.Exception ex)
                            {
                                errorList.Add($"Error in row {row} column '{mapping.First(x=> x.sxColumnIndx == currIndx).mappedFieldName}', Err " + ex.Message);
                            }
                            finally { 
                                row++;
                            }
                        }
                        
                        // return ThrowJsonError("OK! You have passed");
                    }
                }

                // find existing datas
                var currentEmpIDs = await context.Employees.Where(x=> x.CompanyId == id).Select(x=> x.EmpID).ToListAsync();
                var currentJobIDs = await context.Jobs.Where(x=> x.CompanyId == id).Select(x=> x.JobID).ToListAsync();
                var payAdjsDict = new Dictionary<string ,int>();
                // loop in payAdjustment field and find
                foreach(var item in typeof(BulkImportMasterVm).GetProperties().Where(p=> p.IsDefined(typeof(PayAdjustmentFieldAttribute), false))){
                    if(mapping.Any(x=> x.IsMapped && x.mappedFieldName == item.Name)){ 
                        var name =  item.IsDefined(typeof(DisplayAttribute), false) ?
                            item.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().Single().Name : item.Name;
                        payAdjsDict.Add(item.Name, await companyService.GetOrCreateGetConstantAdditionPayAdjustmentAsync(name, id));
                    }
                }

                if(errorList.Any())
                    return ThrowJsonError(JsonConvert.SerializeObject(errorList));
                
                var t = 0;

                // craete data
                // var _createdJobIdsHere = new Dictionary<string, Job>();
                var addedIndex = 0;
                // from : distinct Reporting
                var distinctReportingJobs = _data.Where(a => !string.IsNullOrWhiteSpace(a.Reporting))
                .Select(a => a.Reporting.Trim()).Distinct().ToList();
                foreach (var jd in distinctReportingJobs)
                    foreach (var _ in _data.Where(a => a.JobTitle.Trim() == jd)) {
                            t += await _processDataCreate(id, _data, _, currentEmpIDs, payAdjsDict);
                        // _createdJobIdsHere.Add(_zz.jobId, _zz.job);
                    }
                    

                    //var _ = _data.Find(a => a.JobTitle == jd);

                    //_createdJobIdsHere.Add(jd, );

                    //_jobsDataDict.Add(jd, await companyService.GetOrCreateAndGeJobIdAsync(_.Reporting, id);
                
                // var addedJobIDs = _createdJobIdsHere.Keys.ToArray();
                // foreach (var jd in _data.Where(a => !addedJobIDs.Contains(a.JobID))
                //     .Select(a => a.JobTitle.Trim()).Distinct().ToList())
                //     foreach (var _ in _data.Where(a => a.JobTitle.Trim() == jd))
                //         await _processDataCreate(id, _data, _);

                var dictinctJobTitles = _data.Where(a => !string.IsNullOrWhiteSpace(a.JobTitle))
                .Select(a => a.JobTitle.Trim()).Distinct().ToList();
                foreach (var jd in dictinctJobTitles.Where(x=> !distinctReportingJobs.Contains(x)))
                    foreach (var _ in _data.Where(a => a.JobTitle.Trim() == jd)){
                        t += await _processDataCreate(id, _data, _, currentEmpIDs, payAdjsDict);
                        // _createdJobIdsHere.Add(_zz.jobId, _zz.job);
                    }


                // adding department heads (HOD's)
                var disticntHodEmplIds = _data.Where(a=> !a.HOD.IsMissing()).Select(x=> x.HOD).Distinct().ToList();
                foreach (var item in disticntHodEmplIds)
                    foreach (var emp in _data.Where(a=> a.EmpID == item))
                       context.DepartmentHeads.Add(await _addDepartmentHead(id, emp));


                t += await context.SaveChangesAsync();
                ViewBag.TotalRowsEffected = t;

                return PartialView("_InportMasterDataComplete");
            }

            // if (ModelState.IsValid)
            // {
            //     //try
            //     //{
                    
            //         if (!await context.Companies.AnyAsync(x => x.Id == id))
            //             return ThrowJsonError("Company was not found!");

            //         var _data = JsonConvert.DeserializeObject<List<BulkImportMasterVm>>(data);
            //         if (data == null)
            //             return ThrowJsonError("data was not found!");

            //         var _createdJobIdsHere = new Dictionary<string, Job>();
            //         var addedIndex = 0;
            //         // from : distinct Reporting
            //         var distinctReportingJobs = _data.Where(a=> !string.IsNullOrWhiteSpace(a.Reporting))
            //         .Select(a => a.Reporting.Trim()).Distinct().ToList();
            //     foreach (var jd in distinctReportingJobs)
            //     {
            //         foreach (var _ in _data.Where(a => a.JobTitle.Trim() == jd))
            //         {

            //             // 2nd level report
            //             var level2Report = string.IsNullOrEmpty(_.Reporting) ? null : _data.FirstOrDefault(a => a.JobTitle.Trim() == _.Reporting.Trim());

            //             //if (level2Report == 0)
            //             var job = await _getJob(id, _);
            //             if (!string.IsNullOrEmpty(_.EmpID))
            //             {
            //                 var _emp = await _getEmployee(id, _, job);

            //                 if (!string.IsNullOrEmpty(_.Reporting) && level2Report != null) // Reporting is NOT NULL *findl report employee)
            //                     _emp.ReportingEmployeeId = await companyService.GetReportingEmmployeeAsync(level2Report.EmpID, id);

            //                 if (!string.IsNullOrEmpty(_.PresentAddress))
            //                     _emp.Individual.IndividualAddresses.Add(await _getAddress(id, _, AddressType.Present));
            //                 if (!string.IsNullOrEmpty(_.PermanentAddress))
            //                     _emp.Individual.IndividualAddresses.Add(await _getAddress(id, _, AddressType.Permanant));

            //                 _emp.Job = job;
            //                 //_emp.DepartmentName  = job.Department.Name

            //                 _emp.HrStatus = HrStatus.Employed;
            //                 _emp.JobActionHistories.Add(new JobActionHistory
            //                 {
            //                     Job = job,
            //                     DepartmentId = _emp.DepartmentId.Value,
            //                     Individual = _emp.Individual,
            //                     ActionType = ActionType.MIG,
            //                     CompanyId = id,
            //                     OnDate = DateTime.Now,
            //                     Remarks = "Data Migrated",
            //                 });

            //                 job.JobStatus = JobStatus.Occupied;
            //                 context.Employees.Add(_emp);
            //             }

            //             context.Jobs.Add(job);
            //             _createdJobIdsHere.Add(_.JobID, job);
            //             await context.SaveChangesAsync();
            //         }

            //         //var _ = _data.Find(a => a.JobTitle == jd);

            //         //_createdJobIdsHere.Add(jd, );

            //         //_jobsDataDict.Add(jd, await companyService.GetOrCreateAndGeJobIdAsync(_.Reporting, id);
            //     }



            //         // rest of the jobs (Job Title)
            //         var addedJobIDs = _createdJobIdsHere.Keys.ToArray();
            //     foreach (var jd in _data
            //         .Where(a => !addedJobIDs.Contains(a.JobID))
            //         .Select(a => a.JobTitle.Trim()).Distinct().ToList())
            //     {
            //         foreach (var _ in _data.Where(a => a.JobTitle.Trim() == jd))
            //         {
            //             // 2nd level report
            //             var level2Report = _data.Find(a => a.JobTitle.Trim() == _.Reporting.Trim());

            //             //if (level2Report == 0)
            //             var job = await _getJob(id, _);
            //             if (!string.IsNullOrEmpty(_.EmpID))
            //             {
            //                 var _emp = await _getEmployee(id, _, job);

            //                 if (!string.IsNullOrEmpty(_.Reporting) && level2Report != null) // Reporting is NOT NULL *findl report employee)
            //                     _emp.ReportingEmployeeId = await companyService.GetReportingEmmployeeAsync(level2Report.EmpID, id);

            //                 if (!string.IsNullOrEmpty(_.PresentAddress))
            //                     _emp.Individual.IndividualAddresses.Add(await _getAddress(id, _, AddressType.Present));
            //                 if (!string.IsNullOrEmpty(_.PermanentAddress))
            //                     _emp.Individual.IndividualAddresses.Add(await _getAddress(id, _, AddressType.Permanant));

            //                 _emp.Job = job;
            //                 _emp.HrStatus = HrStatus.Employed;
            //                 _emp.JobActionHistories.Add(new JobActionHistory
            //                 {
            //                     Job = job,
            //                     DepartmentId = _emp.DepartmentId.Value,
            //                     Individual = _emp.Individual,
            //                     ActionType = ActionType.MIG,
            //                     CompanyId = id,
            //                     //ReportingEmployeeId = _createdJobIdsHere[e.JobTitle].ReportingEmployeeId,
            //                     OnDate = DateTime.Now,
            //                     Remarks = "Data Migrated",
            //                 });

            //                 job.JobStatus = JobStatus.Occupied;
            //                 context.Employees.Add(_emp);
            //             }
            //             context.Jobs.Add(job);
            //             _createdJobIdsHere.Add(_.JobID, job);
            //             await context.SaveChangesAsync();

            //             //var _ = _data.Find(a => a.Reporting == jd);
            //             //var _job = await _getJob(id, _);

            //             //_createdJobIdsHere.Add(jd, _job);
            //             //context.Jobs.Add(_job);
            //         }
            //     }

            //     //var emIds = _data.Select(a => a.EmpID).ToArray();
            //     //var empls = await context.Employees.Where(a => a.CompanyId == id && emIds.Contains(a.EmpID)).Include(a=> a.Job).ToListAsync();
            //     //foreach (var e in empls)
            //     //{
            //     //    e.Status = EmployeeStatus.Employed;
            //     //    e.EmployeeActions.Add(new EmployeeAction
            //     //    {
            //     //        JobId =  _createdJobIdsHere[e.JobTitle].Id,
            //     //        ActionType = ActionType.Migration,
            //     //        RecordStatus = RecordStatus.Active,
            //     //        //ReportingEmployeeId = _createdJobIdsHere[e.JobTitle].ReportingEmployeeId,
            //     //        OnDate = DateTime.Now,
            //     //        Message = "Data Migrated",
            //     //    });
            //     //}

            //     await context.SaveChangesAsync();
            //     //await context.SaveChangesAsync();

            //     //// adding employees 
            //     //var _empls = new List<Employee>();
            //     //foreach (var empl in _data
            //     //    .Where(a => !string.IsNullOrWhiteSpace(a.EmpID)))
            //     //{
            //     //    var _emp = await _getEmployee(id, empl, job);
            //     //    _emp.JobId = _createdJobIdsHere[_emp.JobTitle].Id;
            //     //    _emp.JobTitle = _createdJobIdsHere[_emp.JobTitle].JobTitle;
            //     //    _emp.DepartmentId = _createdJobIdsHere[_emp.JobTitle].DepartmentId;
            //     //    _emp.DepartmentId = _createdJobIdsHere[_emp.JobTitle].LocationId;
            //     //    _emp.ReportingEmployeeId = _createdJobIdsHere[_emp.JobTitle].ReportingJobId;

            //     //}
            //     //await context.SaveChangesAsync();
            //     return Ok();
            //         //return PartialView("_ImportDataSummary", _data);
            //     //}
            //     //catch (Exception ex)
            //     //{
            //     //    throw ex;

            //     //    return ThrowJsonError("An Error occured. " + ex.Message);
            //     //}

            // }

            return ThrowJsonError("Please upload the filled template file");

            async Task<DepartmentHead> _addDepartmentHead(int _id, BulkImportMasterVm _)
            {
                var dh = new DepartmentHead
                {
                    CompanyId = _id,
                    EmployeeId = await context.Employees.Where(x=> x.EmpID == _.EmpID).Select(x=> x.Id).FirstOrDefaultAsync(),
                    DepartmentId = await companyService.GetOrCreateAndGetDepartmentIdAsync(_.Department, _id),
                };
                return dh;
            }

            async Task<Job> _getJob(int _id, BulkImportMasterVm _, Dictionary<string, int> _payAdjsDict)
            {
                var job = new Job
                {
                    CompanyId = _id,
                    LevelId = await companyService.GetOrCreateAndGetClassificationIdAsync(_.Level, _id),
                    LocationId = await companyService.GetOrCreateAndGetLocationIdAsync(_.Location, _id),
                    DepartmentId = await companyService.GetOrCreateAndGetDepartmentIdAsync(_.Department, _id),
                    ReportingJobId = await companyService.GetJobIdAsync(_.Reporting, _id),
                    JobID = _.JobID,
                    JobTitle = _.JobTitle,
                    JobType = _.JobType
                };
                if(_payAdjsDict.Any())
                    foreach(var item in _payAdjsDict)
                        job.JobPayComponents.Add(new JobPayComponent{ PayAdjustmentId = item.Value, Total = decimal.Parse(_.GetType().GetProperty(item.Key).GetValue(_)?.ToString()) });
                return job;
            }

            async Task<Employee> _getEmployee(int _id, BulkImportMasterVm _, Job job)
            {
                var d = string.IsNullOrWhiteSpace(_.FullName) ? new { _.FirstName, _.MiddleName, _.LastName, _.Initial } : 
                new {  FirstName = _.FullName.Split(" ").ElementAt(0).Trim(), 
                    MiddleName = _.FullName.Split(" ").Length > 1 ? _.FullName.Split(" ").ElementAt(1).Trim() : "", 
                    LastName = _.FullName.Split(" ").Length > 2 ? _.FullName.Split(" ").ElementAt(2).Trim() : "", 
                    _.Initial };

                var _a = new Employee
                {
                    //Individual = new Individual
                    //{
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    MiddleName = d.MiddleName,
                    Initial = d.Initial,
                    Gender = _.Gender,
                    MaritialStatus = _.MaritialStatus,
                    IdentityType = _.IdentityType,
                    IdentityNumber = _.IdentityNumber,
                    DateOfBirth = _.DateOfBirth,
                    EmailPersonal = _.EmailPersonal,
                    PhonePersonal = _.PhonePersonal,
                    EmergencyContactName = _.EmergencyContactName,
                    EmergencyContactNumber = _.EmergencyContactNumber,


                    TwitterId = _.TwitterId,
                    FacebookId = _.FacebookId,
                    InstagramId = _.InstagramId,
                    LinkedInId = _.LinkedInId,

                    //DepartmentName = _.DepartmentName
                    BankName = _.BankName,
                    BankAccountName = _.BankAccountName,
                    BankAccountNumber = _.BankAccountNumber,
                    BankIBAN = _.BankIBAN,
                    BankCurrency = _.BankCurrency,
                    BankSwiftCode = _.BankSwiftCode,
                    //NickName = _.NickName,
                    BankAddress = _.BankAddress,
                    //},
                    //FirstName = d.FirstName,
                    //LastName = d.LastName,
                    //MiddleName = d.MiddleName,
                    //Initial = d.Initial,

                    NickName = _.NickName,
                    EmpID = _.EmpID,
                    DateOfJoined = _.DateOfJoined,
                    EmailWork = _.EmailWork,
                    PhoneWork = _.PhoneWork,

                    //WeeklyWorkingHours = _.WeeklyWorkingHours,
                    //DailyWorkingHours = _.DailyWorkingHours,
                    //DaysWorkingInWeek = _.WeeklyWorkingHours / _.DailyWorkingHours,


                    DepartmentId = job.DepartmentId,
                    DepartmentName = _.Department,
                    LocationId = job.LocationId,
                    LocationName = _.Location,
                    JobTitle = job.JobTitle,
                    JobType = job.JobType,
                    JobIDString = job.JobID,
                    EmployeeStatus = _.EmploymentStatus <= 0 ? EmployeeStatus.Active : _.EmploymentStatus,
                    // DepartmentName = job.Department.Name,
                    //Location = job.Location.Name,

                    //VisaType = 
                    CompanyId = id,
                    ContractStartDate = _.DateOfJoined,
                    ContractEndDate = _.ContractEndDate,
                    ProbationEndDate = _.ProbationEndDate,
                    EmployeeAddresses = new List<Address>()
                    //index = i,
                };

                if (!_.Nationality.IsMissing())
                    _a.NationalityId = await companyService.GetOrCreateAndGetNationalityAsync(_.Nationality, _id);
                if (!_.EmergencyContactRelation.IsMissing())
                    _a.EmergencyContactRelationshipId = await companyService.GetOrCreateAndGetEmergencyContactRelationAsync(_.EmergencyContactRelation, _id);
                if (_a.DailyWorkingHours > 0 && _.WeeklyWorkingHours > 0)
                {
                    _a.WeeklyWorkingHours = _.WeeklyWorkingHours;
                    _a.DailyWorkingHours = _.DailyWorkingHours;
                    _a.DaysWorkingInWeek = _.WeeklyWorkingHours / _.DailyWorkingHours;
                }
                    //_a.EmergencyContactRelationshipId = await companyService.GetOrCreateAndGetEmergencyContactRelationAsync(_.EmergencyContactRelation, _id);

                return _a;
            }

            async Task<Address> _getAddress(int _ied, BulkImportMasterVm _, AddressType type)
            {
                switch (type)
                {
                    //case AddressType.NA:
                    //    break;
                    case AddressType.Present:
                        return new Address
                        {
                            //ZipCode = _.PresentZipCode,
                            //CountryId = await companyService.GetOrCreateAndGetCountryAsync(_.EmergencyContactName, _id),
                            State = _.PresentState,
                            City = _.PresentCity,
                            //StateId = await companyService.GetOrCreateAndGetStateAsync(_.PresentState, _ied),
                            //CityId = await companyService.GetOrCreateAndGetCityAsync(_.PresentCity, _ied, _.PresentState),
                            RecordStatus = RecordStatus.Active,
                            AddressType = AddressType.Permanant,
                            Street1 = _.PresentAddress,
                        };
                    case AddressType.Permanant:
                        return new Address
                        {
                            //ZipCode = _.PresentZipCode,
                            //CountryId = await companyService.GetOrCreateAndGetCountryAsync(_.EmergencyContactName, _id),
                            State = _.PermanentState,
                            City = _.PermanentCity,
                            //StateId = await companyService.GetOrCreateAndGetStateAsync(_.PermanentState, _ied),
                            //CityId = await companyService.GetOrCreateAndGetCityAsync(_.PermanentCity, _ied, _.PermanentState),
                            RecordStatus = RecordStatus.Active,
                            AddressType = AddressType.Permanant,
                            Street1 = _.PermanentAddress,
                        };
                    default:
                        return null;
                }
            }

            async Task<int> _processDataCreate(int _id, List<BulkImportMasterVm> _data, BulkImportMasterVm _, List<string> _empIds, Dictionary<string, int> _payAdjsDict)
            {
                // 2nd level report
                var level2Report = string.IsNullOrEmpty(_.Reporting) ? null : _data.FirstOrDefault(a => a.JobTitle == _.Reporting);
                
                if(_empIds.Contains(_.EmpID))
                return 0;

                //if (level2Report == 0)
                var job = await _getJob(_id, _, _payAdjsDict);
                if (!string.IsNullOrEmpty(_.EmpID))
                {
                    var _emp = await _getEmployee(_id, _, job);

                    if (!string.IsNullOrEmpty(_.Reporting) && level2Report != null) // Reporting is NOT NULL *findl report employee)
                        _emp.ReportingEmployeeId = await companyService.GetReportingEmmployeeAsync(level2Report.EmpID, _id);

                    if (!string.IsNullOrEmpty(_.PresentAddress))
                        _emp.EmployeeAddresses.Add(await _getAddress(_id, _, AddressType.Present));
                    if (!string.IsNullOrEmpty(_.PermanentAddress))
                        _emp.EmployeeAddresses.Add(await _getAddress(_id, _, AddressType.Permanant));

                    _emp.Job = job;
                    //_emp.DepartmentName  = job.Department.Name

                    _emp.HrStatus = HrStatus.Employed;
                    _emp.EmployeeStatus = EmployeeStatus.Active;
                    _emp.WeeklyWorkingHours = _.WeeklyWorkingHours;
                    _emp.RecordStatus = RecordStatus.Active;
                    _emp.ReportingEmployeeId = _emp.ReportingEmployeeId;
                    //_emp.EmploymentStatus = EmployeeStatus.Active;
                        _emp.EmploymentType = _.EmploymentType;
                    //_emp.EffectiveDate = _emp.DateOfJoined.Value;
                    _emp.DailyWorkingHours = _.WeeklyWorkingHours / 7;
                    _emp.Job = _emp.Job;
                        _emp.EmployeePayComponents = job.JobPayComponents.Select(x => new EmployeePayComponent
                        {
                            EffectiveDate = x.EffectiveDate,
                            PayAdjustmentId = x.PayAdjustmentId,
                            Employee = _emp,
                            Total = x.Total,
                            RecordStatus = RecordStatus.Active,
                        }).ToList();

                    //_emp.Employments.Add(new Employment{
                    //    WeeklyWorkingHours = _.WeeklyWorkingHours,
                    //    RecordStatus = RecordStatus.Active,
                    //    ReportingEmployeeId = _emp.ReportingEmployeeId,
                    //    EmploymentStatus = EmployeeStatus.Active,
                    //    EmploymentType = _.EmploymentType,
                    //    EffectiveDate = _emp.DateOfJoined.Value,
                    //    DailyWorkingHours = _.WeeklyWorkingHours / 7,
                    //    Job  = _emp.Job,
                    //    EmployeePayComponents = job.JobPayComponents.Select(x=> new EmployeePayComponent{
                    //        EffectiveDate = x.EffectiveDate,
                    //        PayAdjustmentId = x.PayAdjustmentId,
                    //        Employee = _emp,
                    //        Total = x.Total,
                    //        RecordStatus =  RecordStatus.Active,
                    //    }).ToList()
                    //});

                    // _emp.JobActionHistories.Add(new JobActionHistory
                    // {
                    //     Job = job,
                    //     DepartmentId = _emp.DepartmentId.Value,
                    //     Individual = _emp.Individual,
                    //     ActionType = ActionType.MIG,
                    //     CompanyId = _id,
                    //     OnDate = DateTime.Now,
                    //     Remarks = "Data Migrated",
                    // });

                    job.JobStatus = JobStatus.Occupied;
                    context.Employees.Add(_emp);
                }

                context.Jobs.Add(job);
                // _createdJobIdsHere.Add(_.JobID, job);
                return await context.SaveChangesAsync();
                // return (_.JobID, job);
            }
        }

        
        public async Task<IActionResult> RemoveData(int id)
        {
            if(id == userResolverService.GetCompanyId())
            {
                var t = await context.Database.ExecuteSqlRawAsync($@"
                  DECLARE @cmpId int = {id};
                    delete from [Payroll.Master.Test].dbo.JobActionHistories where EmployeeId in (select Id from [Payroll.Master.Test].dbo.Employees where CompanyId = @cmpId);
                    delete from [Payroll.Master.Test].dbo.Employments where EmployeeId in (select Id from [Payroll.Master.Test].dbo.Employees where CompanyId = @cmpId);
                    DELETE FROM [Payroll.Master.Test].[dbo].[DepartmentHeads] where DepartmentId in (select Id FROM [Payroll.Master.Test].[dbo].[Departments] where CompanyId = @cmpId);
                    delete from [Payroll.Master.Test].dbo.EmployeeActions where JobId in (select id from [Payroll.Master.Test].dbo.Jobs where CompanyId = @cmpId);
                    delete from [Payroll.Master.Test].dbo.Employees where CompanyId = @cmpId;
                    delete from [Payroll.Master.Test].dbo.Jobs where CompanyId = @cmpId;
                    DELETE FROM [Payroll.Master.Test].[dbo].[Departments] where CompanyId = @cmpId;");
                    return Ok("Data removed!");
            }
            return Ok("Data removed");
        }

        public async Task<IActionResult> ViewCompanyRequestProcessConfigs(int cmpId)
        {
            var dept = await context.Companies
                    .Include(x => x.RequestApprovalConfigs)
                    .ThenInclude(x => x.Employee)
                    .Include(x => x.RequestApprovalConfigs)
                    .ThenInclude(x => x.DayOff)
                    .Include(x => x.RequestApprovalConfigs)
                    .ThenInclude(x => x.EmployeeRole)
                    .FirstOrDefaultAsync(x=> x.Id == cmpId);
            if (dept == null)
                return ThrowJsonError();

            ViewBag.dayOffs = await companyService.GetDayOffs(cmpId);

            return PartialView("_ListingRequestProcessConfig", dept);
        }


        #region Bulk Import Data
        public async Task<IActionResult> DownloadImportTemplate(int jobs = 0)
        {
            byte[] fileContents;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Put whatever you want here in the sheet
                // For example, for cell on row1 col1
                var t = jobs == 0 ? typeof(BulkImportEmployeeVm) : jobs == 2 ? typeof(BulkImportMasterVm) : typeof(BulkImportJobVm);
                var d = jobs == 0 ? new BulkImportEmployeeVm
                {
                    EmpID = "123",
                    FirstName = "Ahmed",
                    MiddleName = "Mohamed",
                    LastName = "Rasheed",
                    NickName = "Ahmed",
                    DepartmentName = "Finance",
                    EmailAddress = "rashyd14@gmail.com",
                    PhoneWork = "3335642",
                    PhonePersonal = "7457857",
                    Address = "Lot 103365 Hulhumale",
                    Street = "Nirolhu Magu",
                    ZipCode = "10035",
                    Gender = Gender.Male,
                    DateOfBirth = DateTime.Now.AddYears(-29),
                    DateOfJoined = DateTime.Now.AddYears(-1),
                    Nationality = "Maldivian",
                    BankName = "MIB",
                    BankAccountName = "Ahmed Hassan",
                    BankAccountNumber = "770412378234",
                    EmergencyContactName = "Ahmed Hassan",
                    EmergencyContactNumber = "7770770",
                    EmergencyContactRelation = "Brother",
                    LinkedInId = "hassan",
                    FacebookId = "",
                    InstagramId = "i_newAne",
                    TwitterId = "hassan_rasheed",
                    IdentityType = IdentityType.NationalID,
                    IdentityNumber = "A000000",
                    //Passport = "",
                    VisaType = VisaType.Local,
                }.GetType().GetProperties() : jobs == 2 ? 
                new BulkImportMasterVm
                {
                    EmpID = "123",
                    FirstName = "Ahmed",
                    MiddleName = "Mohamed",
                    LastName = "Rasheed",
                    NickName = "Ahmed",
                    Department = "Finance",
                    EmailPersonal = "rashyd14@gmail.com",
                    EmailWork = "rashyd14@mira.com",
                    PhoneWork = "3335642",
                    PhonePersonal = "7457857",
                    PermanentAddress = "Lot 103365 Hulhumale, Nirolhu Magu",
                    PermanentCity = "Hulhumale",
                    PermanentState = "K",
                    PresentAddress = "Lot 103365 Hulhumale, Nirolhu Magu",
                    PresentCity = "Hulhumale",
                    PresentState = "K",
                    Gender = Gender.Male,
                    DateOfBirth = DateTime.Now.AddYears(-29),
                    DateOfJoined = DateTime.Now.AddYears(-1),
                    Nationality = "Maldivian",
                    BankName = "MIB",
                    BankAccountName = "Ahmed Hassan",
                    BankAccountNumber = "770412378234",
                    EmergencyContactName = "Ahmed Hassan",
                    EmergencyContactNumber = "7770770",
                    EmergencyContactRelation = "Brother",
                    LinkedInId = "hassan",
                    FacebookId = "",
                    InstagramId = "i_newAne",
                    TwitterId = "hassan_rasheed",
                    IdentityType = IdentityType.NationalID,
                    IdentityNumber = "A000000",
                    //Passport = "",
                    VisaType = VisaType.Local,
                    Level = "Manager",
                    JobID = "JD001",
                    JobType = JobType.FullTime,
                    JobTitle = "Assistant Manager",
                    Location = "Area 1",
                    Reporting = "Manager"
                }.GetType().GetProperties()
                : new BulkImportJobVm
                {
                    Classification = "Manager",
                    Department = "Finance",
                    JobID = "JD001",
                    JobType = JobType.FullTime,
                    JobTitle = "Assistant Manager",
                    Location = "Area 1",
                    Total = 4
                }.GetType().GetProperties();


                for (int i = 0; i < d.Count(); i++)
                {
                    worksheet.Cells[1, i+1].Value = d[i].Name;
                }
                try
                {
                    for (int i = 0; i < d.Count(); i++)
                    {
                        if (d[i].PropertyType == typeof(DateTime))
                            worksheet.Cells[2, i + 1].Value = ((DateTime)d[i].GetValue(d)).ToString("dd/MM/yyyy");
                        else
                            worksheet.Cells[2, i + 1].Value = d[i].GetValue(d)?.ToString() ?? "";
                    }
                }
                catch (Exception)
                {
                }

                // Finally when you're done, export it to byte array.
                fileContents = package.GetAsByteArray();
            }

            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: jobs == 2 ? "bulk-import-master.xlsx" : jobs == 0 ? "bulk -import-employee.xlsx" : "bulk-import-jobs.xlsx"
            );
        }

        #endregion


        [HttpPost]
        public IActionResult Remove(int id)
        {
            if (ModelState.IsValid)
            {
                var add = context.Companies.FirstOrDefault(x => x.Id == id);
                if (add == null)
                    return BadRequest("Oooh! we didnt find that one");
                if (context.Departments.Any(x => x.CompanyId == id))
                    return BadRequest("Ouch! There are some departments in this company, kindly remove them first");

                context.Companies.Remove(add);
                context.SaveChanges();
                return RedirectToAction("Index");
            }

            return BadRequest();
        }
    }
}

