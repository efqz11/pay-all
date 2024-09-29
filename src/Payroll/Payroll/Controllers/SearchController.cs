using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Controllers
{
    public class SearchController : Controller
    {
        private readonly PayrollDbContext context;
        private readonly AccountDbContext accountDbContext;
        private readonly AccessGrantService accessGrantService;
        private readonly UserResolverService userResolverService;

        public SearchController(PayrollDbContext context, AccountDbContext accountDbContext, AccessGrantService accessGrantService, UserResolverService userResolverService)
        {
            this.context = context;
            this.accountDbContext = accountDbContext;
            this.accessGrantService = accessGrantService;
            this.userResolverService = userResolverService;
        }

        public async Task<IActionResult> Users(string term)
        {
            term = term.ToLower();
            var items = await GetSearchResultAsync(EntityType.AppUser, term);
            

            return Json(items);
        }

        public async Task<IActionResult> Employees(string term)
        {
            term = term.ToLower();
            var items = await GetSearchResultAsync(EntityType.Employee, term);

            //var items = await accountDbContext.Users
            //    .Where(x => x.Email.Contains(term) || x.UserName.Contains(term) || x.PhoneNumber == term)
            //    .Select(x => new { x.Id, Text = $"<img src='{Url.Content(x.Picture ?? DefaultPictures.default_user)}' height='20px' /> {x.NameDisplay}" })
            //    .Take(10)
            //    .ToArrayAsync();

            return Json(items);
        }

        public async Task<IActionResult> Jobs(string term)
        {
            term = term.ToLower();
            var items = await GetSearchResultAsync(EntityType.Job, term);
            
            

            //var items = await accountDbContext.Users
            //    .Where(x => x.Email.Contains(term) || x.UserName.Contains(term) || x.PhoneNumber == term)
            //    .Select(x => new { x.Id, Text = $"<img src='{Url.Content(x.Picture ?? DefaultPictures.default_user)}' height='20px' /> {x.NameDisplay}" })
            //    .Take(10)
            //    .ToArrayAsync();

            return Json(items);
        }

        public async Task<IActionResult> PayrolPeriod(string term)
        {
            var items = await GetSearchResultAsync(EntityType.PayrolPeriod, term);

            //var items = await accountDbContext.Users
            //    .Where(x => x.Email.Contains(term) || x.UserName.Contains(term) || x.PhoneNumber == term)
            //    .Select(x => new { x.Id, Text = $"<img src='{Url.Content(x.Picture ?? DefaultPictures.default_user)}' height='20px' /> {x.NameDisplay}" })
            //    .Take(10)
            //    .ToArrayAsync();

            return Json(items);
        }

        public async Task<IActionResult> Comapanies(string term)
        {
            term = term.ToLower();
            var items = await GetSearchResultAsync(EntityType.Company, term);

            //var items = await accountDbContext.Users
            //    .Where(x => x.Email.Contains(term) || x.UserName.Contains(term) || x.PhoneNumber == term)
            //    .Select(x => new { x.Id, Text = $"<img src='{Url.Content(x.Picture ?? DefaultPictures.default_user)}' height='20px' /> {x.NameDisplay}" })
            //    .Take(10)
            //    .ToArrayAsync();

            return Json(items);
        }

        public async Task<IActionResult> AppUsers(string term)
        {
            term = term.ToLower();
            var items = await accountDbContext.Users
                .Where(x => x.UserName.ToLower().Contains(term) || x.NickName.ToLower().Contains(term))
                .Select(x => new SearchResult($"{x.UserName}", "fa fa-users", EntityType.AppUser, Url.Action("Detail", "AppUser", new { id = x.Id })))
                //.Select(x => new { x.Id, Text = $"<img src='{Url.Content(x.Avatar ?? DefaultPictures.default_company)}' height='20px' /> {x.UserName}" })
                .Take(10)
                .ToArrayAsync();

            return Json(items);
        }


        public async Task<IActionResult> SearchModal(EntityType what)
        {
            ViewBag.Header = "Search for " + what.ToString().ToLower();
            return PartialView("_SearchModal", what);
        }

        [HttpPost]
        public async Task<IActionResult> SearchModal(EntityType what, string term)
        {
            ViewBag.Header = "Search for " + what.ToString().ToLower();
            switch (what)
            {
                case EntityType.Announcement:
                    break;
                case EntityType.PayrolPeriod:
                    return RedirectToAction(nameof(PayrolPeriod), new { term });
                case EntityType.Roster:
                    break;
                case EntityType.Request:
                    break;
                case EntityType.Employee:
                    return RedirectToAction(nameof(Employees), new { term });

                case EntityType.Job:
                    return RedirectToAction(nameof(Jobs), new { term });

                case EntityType.Company:
                    return RedirectToAction(nameof(Comapanies), new { term });

                case EntityType.AppUser:
                    return RedirectToAction(nameof(Users), new { term });

                case EntityType.Anything:
                default:
                    return RedirectToAction(nameof(All), new { term });
            }
            return PartialView("_SearchModal", what);
        }


        public async Task<IActionResult> All(string term)
        {
            term = term.ToLower();
            var items = new List<SearchResult>();
            var matcdFeatures = FeatureMenus.FeatureSearchList.Where(a => a.Name.ToLower().Contains(term))
                .ToList();
                if(matcdFeatures.Any())
                items.AddRange(matcdFeatures);

                var matchEmpls = await GetSearchResultAsync(EntityType.Employee, term);
                if(matchEmpls.Any())
                items.AddRange(matchEmpls);


                var jobs = await GetSearchResultAsync(EntityType.Job, term);
                if(jobs.Any())
                items.AddRange(jobs);

            //await context.Companies
            //.Where(x => x.Name.ToLower().Contains(term))
            //.Select(x => new { x.Id, Text = $"<img src='{Url.Content(x.LogoUrl ?? DefaultPictures.default_company)}' height='20px' /> {x.Name}" })
            //.Take(10)
            //.ToArrayAsync();
            //var items = await accountDbContext.Users
            //    .Where(x => x.Email.Contains(term) || x.UserName.Contains(term) || x.PhoneNumber == term)
            //    .Select(x => new { x.Id, Text = $"<img src='{Url.Content(x.Picture ?? DefaultPictures.default_user)}' height='20px' /> {x.NameDisplay}" })
            //    .Take(10)
            //    .ToArrayAsync();

            return Json(items);
        }

        public async Task<SearchResult[]> GetSearchResultAsync(EntityType type, string term)
        {
            term = term.ToLower();
            switch (type)
            {
                // case EntityType.AppUser:
                //     return await accountDbContext.Users
                //     .Where(x => x.UserName.ToLower().Contains(term) || x.NickName.ToLower().Contains(term))
                //     .Select(x => new SearchResult($"{x.UserName}", "fa fa-users", EntityType.AppUser, Url.Action("Detail", "AppUser", new { id = x.Id })))
                //     //.Select(x => new { x.Id, Text = $"<img src='{Url.Content(x.Avatar ?? DefaultPictures.default_company)}' height='20px' /> {x.UserName}" })
                //     .Take(10)
                //     .ToArrayAsync();
                // default:

                case EntityType.PayrolPeriod:
                    return await context.PayrollPeriods.Where(x => x.CompanyId == userResolverService.GetCompanyId()
                   && (x.Name.ToLower().Contains(term) || x.StartDate.GetDuration(x.EndDate, userResolverService.GetClaimsPrincipal(), true).ToLower().Contains(term)))
                        .OrderBy(x => x.StartDate)
                        .Select(x => new SearchResult(x.Name + " · " + x.StartDate.GetDuration(x.EndDate, userResolverService.GetClaimsPrincipal(), true), FeatureMenus.GetFeatureMenuItem(FeatureMenus.MenuItem.Payroll), type, Url.Action("View", "Payroll", new { id = x.Id })))
                        .Take(10)
                        .ToArrayAsync();

                case EntityType.Job:
                    return await context.Jobs.Where(x => x.CompanyId == userResolverService.GetCompanyId())
                    .Where(x => x.JobTitle.ToLower().Contains(term) || x.JobID == term)
                    .OrderBy(x => x.JobTitle)
                    .ThenBy(x => x.JobID)

                    .Select(x => new SearchResult($"{x.JobID} {x.JobTitle}", FeatureMenus.GetFeatureMenuItem(FeatureMenus.MenuItem.Jobs), EntityType.Job, Url.Action("Detail", "Job", new { id = x.Id })))
                    .Take(10)
                    .ToArrayAsync();
                case EntityType.Company:
                    return await context.Companies
                                    .Where(x => x.Name.ToLower().Contains(term))
                                    .Select(x => new SearchResult(x.Id, Url.Content(x.LogoUrl ?? DefaultPictures.default_company), x.Name, Url.Action("Detail", "Company", new { id = x.Id }), x.ManagingDirector, EntityType.Employee))
                                    //.Select(x => new { x.Id, Text = $"<img src='{Url.Content(x.LogoUrl ?? DefaultPictures.default_company)}' height='20px' /> {x.Name}" })
                                    .Take(10)
                                    .ToArrayAsync();

                case EntityType.Employee:
                    return await context.Employees
                                    .Where(x => x.CompanyId == userResolverService.GetCompanyId())
                                     .Where(x => x.FirstName.ToLower().Contains(term) || (x.MiddleName != null && x.MiddleName.ToLower().Contains(term)) || (x.LastName != null && x.LastName.ToLower().Contains(term)) ||
                                     // x.GetSystemName(userResolverService.GetClaimsPrincipal()).ToLower().Contains(query) || 
                                     x.EmpID.ToLower().Equals(term) || x.DepartmentName.ToLower().Contains(term))
                                    .OrderBy(x => x.Department.DisplayOrder)
                                    .ThenBy(x => x.EmpID)
                                    .Select(x => new SearchResult(x.Id, Url.Content(x.Avatar ?? DefaultPictures.default_user), x.GetSystemName(User), Url.Action("Detail", "Employee", new { id = x.Id }), x.JobTitle, EntityType.Employee))
                                    .Take(10)
                                    .ToArrayAsync();

                case EntityType.AppUser:
                    return await accountDbContext.Users
                    .Where(x => x.Email.Contains(term) || x.UserName.Contains(term) || x.PhoneNumber == term)
                      .Select(x => new SearchResult(x.Id, Url.Content(x.Avatar ?? DefaultPictures.default_user), x.NameDisplay, Url.Action("Detail", "User", new { id = x.Id }), x.Email, EntityType.AppUser))

                    //.Select(x => new { x.Id, Text = $"<img src='{Url.Content(x.Avatar ?? DefaultPictures.default_user)}' height='20px' /> {x.NameDisplay}" })
                    .Take(10)
                    .ToArrayAsync();
                default:
                    return default;
            }
        }

    }
}
