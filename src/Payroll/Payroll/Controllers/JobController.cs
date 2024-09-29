using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Payroll.Database;
using Payroll.Models;
using Payroll.Models.JobScrape;
using Payroll.Services;
using Payroll.ViewModels;

namespace Payroll.Controllers
{
    public class JobController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly PayAdjustmentService payAdjustmentService;
        private readonly AccessGrantService accessGrantService;
        private readonly IHttpClientFactory clientFactory;
        private readonly IHostingEnvironment environment;
        private readonly CompanyService companyService;
        private readonly AccountDbContext accountDbContext;
        private readonly UserResolverService userResolverService;
        public int cnt_jobs_updated = 0;
        public int cnt_jobs_created = 0;
        public int cnt_comps_created = 0;
        public int cnt_comps_updated = 0;

        public class Meta
        {
            public int current_page { get; set; }
            public int from { get; set; }
            public int last_page { get; set; }
            public int per_page { get; set; }
            public int to { get; set; }
            public int total { get; set; }
        }
        public class MorePageVm
        {
            public List<Jobsicle_Job> data { get; set; }
            public Meta meta { get; set; }
        }

        public JobController(PayrollDbContext context, PayAdjustmentService payAdjustmentService, AccessGrantService accessGrantService, IHttpClientFactory clientFactory, IHostingEnvironment environment, UserResolverService userResolverService, CompanyService companyService, AccountDbContext accountDbContext)
        {
            this.context = context;
            this.payAdjustmentService = payAdjustmentService;
            this.accessGrantService = accessGrantService;
            this.clientFactory = clientFactory;
            this.environment = environment;
            this.companyService = companyService;
            this.accountDbContext = accountDbContext;
            this.userResolverService = userResolverService;
        }


        public async  Task<IActionResult> Index(int dept = 0, int loc = 0, int c = 0, int page = 1, int limit = 10)
        {
            ViewBag.EmpIdFilter = dept;
            ViewBag.ClsIdFilter = c;
            var comapnyId = userResolverService.GetCompanyId();
            var q = context.Jobs
             .Where(x => x.CompanyId == comapnyId &&
             (dept == 0 || dept == x.DepartmentId) &&
             (loc == 0 || loc == x.LocationId) &&
             (c == 0 || c == x.LevelId));

            var emp = await q
             .OrderBy(x => x.Department.DisplayOrder)
             .ThenBy(x => x.JobID)
             .Skip((page - 1) * limit)
             .Take(limit)
             .Include(x => x.Department)
             .Include(x => x.Employees)
             .Include(x => x.Location)
             .Include(x => x.Level)
             .ToListAsync();
            ViewBag.DeptName = context.Departments.Find(dept)?.Name?.ToUpper();
            ViewBag.Count = await q.CountAsync();
            ViewBag.DeptRouteId = dept;

            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Listing", emp);
                
            ViewData["Classifications"] = context.Levels.Where(x => x.CompanyId == comapnyId).OrderBy(x => x.DisplayOrder)
                // .GroupBy(a => new { a.Name, a.Id })
                .Select(a => new DepartmentReportVm
                {
                    Name = a.Name,
                    Id = a.Id,
                    EmployeeCount = a.Jobs.Count(z=> z.JobStatus==JobStatus.Vacant),
                    ManagerEmployeesCount = a.Jobs.Count(z => z.JobStatus != JobStatus.Vacant && z.JobStatus != JobStatus.Abolished),
                }).ToList();
            //ViewData["Departments"] = context.Departments.Where(x => x.CompanyId == comapnyId).OrderBy(x => x.DisplayOrder)
            //    .GroupBy(a => new { a.Name, a.Id })
            //    .Select(a => new DepartmentReportVm
            //    {
            //        Name = a.Key.Name,
            //        Id = a.Key.Id,
            //        EmployeeCount = a.Sum(d => d.Employees.Count()),
            //        ManagerEmployeesCount = a.Sum(d => d.DepartmentHeads.Count),
            //        Managers = a.SelectMany(z => z.DepartmentHeads).ToArray()
            //    }).ToList();

            return View(emp);
        }


        [HttpPost]
        public async Task<IActionResult> Search(string query)
        {
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
                return BadRequest();
            var comapnyId = userResolverService.GetCompanyId();

            query = query.ToLower();
            var emp = await context.Jobs
             .Where(x => x.CompanyId == comapnyId
             && (x.JobTitle.ToLower().Contains(query) || x.JobID.Contains(query) || x.Department.Name.Contains(query)))
             .OrderBy(x => x.Department.DisplayOrder)
             .ThenBy(x => x.JobID)
             .Take(11)
             .Include(x => x.Department)
             .Include(x => x.Location)
             .Include(x => x.Level)
             .ToListAsync();
            ViewBag.HasMoreData = emp.Count > 10;

            return PartialView("_Listing", emp);
        }

        public async Task<IActionResult> Structure(int dept = 0, int page = 1, int limit = 10)
        {
            ViewBag.EmpIdFilter = dept;
            var comapnyId = userResolverService.GetCompanyId();

            ViewBag.Count = await context.Jobs
             .CountAsync(x=>  x.CompanyId == comapnyId);
            ViewBag.DepartmentIds = new SelectList(await context.Departments.Where(x => x.CompanyId == comapnyId).OrderBy(x => x.DisplayOrder).ToListAsync(), "Id", "Name", dept);
            return View();
        }

        public async Task<JsonResult> GetOrganChart(int? dept = null)
        {
            var comapnyId = userResolverService.GetCompanyId();


            //var datax =  context.EmployeeJobInfos
            //    .Include(x => x.Department)
            //    .Include(x => x.Employee)
            //    .ThenInclude(a => a.EmployeeDirectReports)
            //        .ThenInclude(a => a.Employee)
            //            .ThenInclude(a => a.EmployeeDirectReports)
            //    .AsEnumerable()
            //    .Where(x => x.Employee.CompanyId == comapnyId && x.ReportingEmployeeId == null)
            //    .ToList();
            //return Json(datax);
            var data = new List<OrgStructure>();
            if (dept.HasValue)
                data = await context.Jobs
                 .Where(x => x.Department.CompanyId == comapnyId && (x.DepartmentId == dept && x.ReportingJob.DepartmentId != dept && x.ReportingJob ==null)) //  && x.ReportingJobs.Any())) //x.ReportingJob != null && x.ReportingJob.DepartmentId != dept && 
                 // .Select(a => a.ReportingJob)
                 // .Include(x => x.ReportingJob)
                 .Include(x => x.Department)
                 .Include(x => x.Location)
                 .Include(x => x.Division)
                 .Include(x => x.ReportingJobs)
                    .ThenInclude(x => x.ReportingJob)
                .Select(a => new OrgStructure
                {
                    id = a.Id,
                    name = a.JobID,
                    department = a.Department != null ? a.Department.Name : "",
                    jobId = a.Id,
                    division = a.Division != null ? a.Division.Name : "",
                    location = a.Location != null ? a.Location.Name : "",
                    empstate = a.JobStatus.GetDisplayName(),
                    title = a.JobTitle,
                })
                 .ToListAsync();
            else
                data = await context.Jobs
             .Where(x => x.Department.CompanyId == comapnyId && x.ReportingJob == null && x.ReportingJobs.Any())
             .Select(a => dept.HasValue ? a.ReportingJob : a)
             .Include(x => x.ReportingJob)
             .Include(x => x.Department)
             .Include(x => x.Location)
             .Include(x => x.Division)
             .Include(x => x.ReportingJobs)
                .ThenInclude(x => x.ReportingJob)
            .Select(a => new OrgStructure
            {
                id = a.Id,
                name = a.JobID,
                department = a.Department != null ? a.Department.Name : "",
                jobId = a.Id,
                division = a.Division != null ? a.Division.Name : "",
                location = a.Location != null ? a.Location.Name : "",
                empstate = a.JobStatus.GetDisplayName(),
                title = a.JobTitle,
            })
             .ToListAsync();
        

            foreach (var item in data)
                item.children = await GetOrhChartChildren(item, dept);

            var cmpMd = await accountDbContext.CompanyAccounts.Where(a => a.Id == comapnyId).Select(a => a.ManagingDirector).FirstOrDefaultAsync();
            var _data = new OrgStructure
            {
                //id = 1,
                title = "Managing Director",
                department = "",
                name = cmpMd,
                children = data
            };

            return Json(_data, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            //async Task<IActionResult> getEmployee(OrgStructure item)
            //{
            //    if(item.rep)
            //}
        }

        private async Task<List<OrgStructure>> GetOrhChartChildren(OrgStructure a, int? dept)
        {
            if (!await context.Jobs.AnyAsync(z => z.ReportingJobId == a.id && (!dept.HasValue || dept == z.DepartmentId))) return null;

            var data = await context.Jobs.Where(z => z.ReportingJobId == a.id && (!dept.HasValue || dept == z.DepartmentId))
                .Select(x => new OrgStructure
                {
                    id = x.Id,
                    name = x.JobID,
                    department = x.Department != null ? x.Department.Name : "",
                    //jobId = x.Id,
                    division = x.Division != null ? x.Division.Name : "",
                    location = x.Location != null ? x.Location.Name : "",
                    empstate = x.JobStatus.GetDisplayName(),
                    title = x.JobTitle,
                }).ToListAsync();


            foreach (var item in data)
                item.children = await GetOrhChartChildren(item, dept);

            return data;
        }


        public async Task<IActionResult> Directory(int groupBy = 1, string query = "")
        {
            var comapnyId = userResolverService.GetCompanyId();
            query = query?.ToLower();
            var _emp = await context.Jobs
             .Where(x => x.CompanyId == comapnyId
             && (string.IsNullOrWhiteSpace(query) || x.JobID.Equals(query)))
             .OrderBy(x => x.JobID)
             //.ThenBy(x => x.Nam)
             .Include(x => x.Level)
             .Include(x => x.Location)
             .Include(x => x.Department)
             //.Include(x => x.Employees.First())
             //.Include(x => x.Employments)
             //   .ThenInclude(x => x.ReportingEmployee)
                .ToListAsync();


            var emp = _emp.GroupBy(g => 
            groupBy == 1 ? g.JobID : 
            groupBy == 2 ? g.JobTitle :
            groupBy == 3 ? g.Level.Name :
            groupBy == 4 ? g.Department.Name :
            groupBy == 5 ? (g.Location != null ? g.Location.Name : "")
            : g.JobStatus.ToString())
                .OrderBy(a => a.Key)
                .ToList();

            ViewBag.Count = _emp.Count;
            ViewBag.Query = query;

            ViewBag.GroupBy = new[] { "ID", "Titile", "Level", "Department", "Location", "Status" }.Select((x, i) => new SelectListItem(x, (i + 1).ToString(), (i + 1) == groupBy)).ToList();
            return View(emp);
        }


        public async Task<JsonResult> GetOrganChartJobs(int id)
        {
            var emp = await context.Jobs
                .Where(x => x.Id == id)
                .Include(x => x.Department)
                .Include(x => x.Division)
                .Include(x => x.Location)
                //.Include(x => x.Employees)
                .Include(x => x.ReportingJob)
                    .ThenInclude(x => x.Department)
                .Include(x => x.ReportingJob)
                    .ThenInclude(x => x.Department)
                .Include(x => x.ReportingJob)
                    .ThenInclude(x => x.Location)
                .Include(x => x.ReportingJobs)
                .FirstOrDefaultAsync();


            var _data = new List<OrgStructure>();
            OrgStructure report = null;
            var empOrg = new OrgStructure
            {
                id = emp.Id,
                name = emp.JobID,
                department = emp.Department != null ? emp.Department.Name : "",
                jobId = emp.Id,
                division = emp.Division != null ? emp.Division.Name : "",
                location = emp.Location != null ? emp.Location.Name : "",
                empstate = emp.JobStatus.GetDisplayName(),
                title = emp.JobTitle,
            };
            if (emp.ReportingJobs != null && emp.ReportingJobs.Any())
                empOrg.children =
                emp.ReportingJobs.Select(a =>
                new OrgStructure
                {
                    id = a.Id,
                    name = a.JobID,
                    department = a.Department != null ? a.Department.Name : "",
                    jobId = a.Id,
                    division = a.Division != null ? a.Division.Name : "",
                    location = a.Location != null ? a.Location.Name : "",
                    empstate = a.JobStatus.GetDisplayName(),
                    title = a.JobTitle,
                    current = true,
                })
             .ToList();

            if (emp.ReportingJob != null)
            {
                report = new OrgStructure
                {
                    id = emp.ReportingJob.Id,
                    name = emp.ReportingJob.JobID,
                    department = emp.ReportingJob.Department != null ? emp.ReportingJob.Department.Name : "",
                    jobId = emp.ReportingJob.Id,
                    division = emp.ReportingJob.Division != null ? emp.ReportingJob.Division.Name : "",
                    location = emp.ReportingJob.Location != null ? emp.ReportingJob.Location.Name : "",
                    empstate = emp.ReportingJob.JobStatus.GetDisplayName(),
                    title = emp.ReportingJob.JobTitle,
                    children = new List<OrgStructure>() { empOrg }
                };

                _data.Add(report);
            }
            else // if(emp.Job.ReportingJobId.HasValue)
            {
                _data.Add(new OrgStructure
                {
                    id = 0,
                    name = "Vacant Post",
                    department = "",
                    jobId = 00,
                    division = "",
                    location = "",
                    title = "Vacant",
                    children = new List<OrgStructure>() { empOrg }
                });
            }

            return Json(_data.First(), new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }


        public async Task<IActionResult> UpdateBulk()
        {
            var comapnyId = userResolverService.GetCompanyId();

            var data = await context.Jobs
             .Where(x => x.CompanyId == comapnyId)
             .GroupBy(a=> a.JobTitle)
             .Select(a=> new BulkJobUpdateVm{
                 JobTitle = a.Key,
                 TotalJobs = a.Count(),
             })
             .ToListAsync();
            var titles = data.Select(a=> a.JobTitle).ToArray();
             
            var jobpayADjis = await context.JobPayComponents
             .Where(x => x.Job.CompanyId == comapnyId && x.IsActive && titles.Contains(x.Job.JobTitle))
             .Select(a=> new {
                 a.JobId,
                 a.Job.JobTitle,
                 a.PayAdjustmentId,
                 a.Total,
             })
             .ToListAsync();

             var adjustments = await context.PayAdjustments
             .Where(x => x.CompanyId == comapnyId && x.IsActive)
             .Select(a=> new PayAdjustmentValue {Id= a.Id, Name =  a.Name, VariationType = a.VariationType})
             .ToListAsync();
            
            data.ForEach(d => {
                d.PayAdjustmentValues = adjustments
             .Select(a=> new PayAdjustmentValue {Id= a.Id, Name =  a.Name, VariationType = a.VariationType, Value =jobpayADjis.FirstOrDefault(x=>x.PayAdjustmentId == a.Id && x.JobTitle == d.JobTitle)?.Total ?? 0, DifferentValues = string.Join(",", jobpayADjis.Where(x=>x.PayAdjustmentId == a.Id && x.JobTitle == d.JobTitle).Select(z=> z.Total)) })
             .ToList();

                // d.PayAdjustmentValues.ForEach(ePay => {
                //     if(jobpayADjis.Any(x=>x.PayAdjustmentId == ePay.Id && x.JobTitle == d.JobTitle)){
                //         ePay.Value = jobpayADjis.First(x=>x.PayAdjustmentId == ePay.Id && x.JobTitle == d.JobTitle).Total;
                //         ePay.DifferentValues = string.Join(",", jobpayADjis.Where(x=>x.PayAdjustmentId == ePay.Id && x.JobTitle == d.JobTitle).Select(a=> a.Total));
                //     }
                // });
            });

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBulk(List<BulkJobUpdateVm> model)
        {
            // if (ModelState.IsValid)
            {
                if(model.Any(a=>a.MustUpdate && a.PayAdjustmentValues.Any(x=> x.VariationType == VariationType.ConstantAddition && x.Value <= 0)))
                    return ThrowJsonError("Please fill in all the values");

                foreach (var item in model.Where(a=>a.MustUpdate))
                {
                    var jobs = await context.Jobs
                    .Where(x=> x.JobTitle == item.JobTitle)
                    .Include(a=> a.JobPayComponents)
                    .ToListAsync();
                    foreach (var job in jobs)
                    if(job != null){
                        if(job.JobPayComponents.Any())
                            context.JobPayComponents.RemoveRange(job.JobPayComponents);

                        var pc = item.PayAdjustmentValues.Select(x=> new JobPayComponent{
                            PayAdjustmentId = x.Id,
                            EffectiveDate = DateTime.UtcNow,
                            Total = x.Value,
                            RecordStatus = RecordStatus.Active
                        }).ToList();

                        foreach (var p in pc)
                            job.JobPayComponents.Add(p);
                    }
                }

                context.SaveChanges();
                return Ok("Job pay components were just updated");
            }
            return BadRequest(ModelState);
        }


        public async Task<IActionResult> Detail(int id)
        {
            var comapnyId = userResolverService.GetCompanyId();

            var data = await context.Jobs
             .Where(x => x.CompanyId == comapnyId
             && x.Id == id)
             .Include(x => x.ReportingJobs)
             .Include(x => x.Employees)
                .Include(x => x.ReportingJob)
                .ThenInclude(a=> a.Department)
                .Include(x => x.ReportingJob)
                .ThenInclude(a => a.Level)
             .Include(x => x.Department)
             .Include(x => x.Location)
             .Include(x => x.Level)
             .FirstOrDefaultAsync();

            return View(data);
        }


        public async Task<IActionResult> AddOrUpdateJob(int cId, int id = 0)
        {
            var emp = await context.Companies.FindAsync(cId);
            if (emp == null && id > 0) return BadRequest("Company was not not found");

            var empJobInfo = new Job { CompanyId = cId };
            if (id > 0)
            {
                var _type = await context.Jobs.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == cId);
                if (_type == null && id > 0) return BadRequest("Job was not not found");
                else
                    empJobInfo = _type;
            }

            ViewBag.DepartmentId = new SelectList(await companyService.GetDepartmentsOfCurremtCompany(userResolverService.GetCompanyId()), "Id", "Name", empJobInfo?.DepartmentId);
            ViewBag.LocationId = new SelectList(await companyService.GetLocationsOfCurremtCompany(), "Id", "Name", empJobInfo?.LocationId);
            //ViewBag.ReportingEmployeeId = new SelectList(await companyService.GetEmployeesOfCurremtCompany(), "Id", "Name", empJobInfo?.ReportingEmployeeId);


            return PartialView("_AddOrUpdateJobInformation", empJobInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateJob(Job model)
        {
            if (ModelState.IsValid)
            {
                //var emp = await context.Employees.FindAsync(model.EmployeeId);
                //if (emp == null)
                //    return ThrowJsonError();

                //if (model.ReportingEmployeeId == model.EmployeeId)
                //    return ThrowJsonError("Employee cannot report to themselves");

                if (model.Id <= 0)
                {
                    //if (model.ReportingEmployeeId <= 0)
                    //    model.ReportingEmployeeId = null;
                    context.Jobs.Add(model);
                }
                else
                {
                    // update
                    var empTypeInDb = await context.Jobs.FindAsync(model.Id);
                    if (empTypeInDb == null)
                        return ThrowJsonError();
                    //empTypeInDb.EffectiveDate = model.EffectiveDate;
                    empTypeInDb.LocationId = model.LocationId;
                    empTypeInDb.JobID = model.JobID;
                    empTypeInDb.LevelId = model.LevelId;
                    //empTypeInDb.DivisionId = model.DivisionId;
                    empTypeInDb.DepartmentId = model.DepartmentId;
                    empTypeInDb.JobTitle = model.JobTitle;

                    //empTypeInDb.ReportingEmployeeId = model.ReportingEmployeeId;
                    //if (empTypeInDb.ReportingEmployeeId <= 0)
                    //    empTypeInDb.ReportingEmployeeId = null;
                }

                //emp.LocationId = model.LocationId;
                //emp.DepartmentId = model.DepartmentId;
                //emp.JobTitle = model.JobTitle;
                context.SaveChanges();
                return Ok();
            }
            return BadRequest(ModelState);
        }



        public async Task<IActionResult> ViewPayComponents(int id)
        {
            var emp = await context.Jobs
                .Where(x => x.Id == id)
                .Include(x => x.JobPayComponents)
                    .ThenInclude(x => x.PayAdjustment)
                .FirstOrDefaultAsync();


            if (emp == null)
                return BadRequest();

            return PartialView("_ViewPayComponents", emp);
        }

        public async Task<IActionResult> AddDefaultPayComponents(int id)
        {
            if (!context.Jobs.Any(x => x.Id == id)) return ThrowJsonError();
            var jobData = await context.Jobs
            .Include(a=> a.JobPayComponents)
            .FirstOrDefaultAsync(x=> x.Id == id);

            var adjustments = await companyService.GetPayAdjustments();
            if (adjustments.Any() == false)
                return ThrowJsonError("Oops! Company does not have pay components configured!");

            var empPayAdjustmentList = adjustments
                      .Select(ajusments => new JobPayComponent
                      {
                          JobId = id,
                          Total = 0,
                          PayAdjustment = ajusments,
                          PayAdjustmentId = ajusments.Id
                      }).ToList();


            var oldrecords = context.JobPayComponents.Where(x => x.JobId == id).ToList();

            foreach (var item in jobData.JobPayComponents)
            {
                if (oldrecords.Any(x => x.PayAdjustmentId == item.PayAdjustmentId))
                {
                    // replcaing toal
                    item.IsActive = true;
                    item.Total = oldrecords.First(x => x.PayAdjustmentId == item.PayAdjustmentId).Total;
                }
            }

            return PartialView("_AddDefaultPayComponents", empPayAdjustmentList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDefaultPayComponents(List<JobPayComponent> model, DateTime date)
        {
            var first = model.FirstOrDefault();
            if (first == null)
                return RedirectToAction(nameof(Index));

            if (first == null) return ThrowJsonError("Employee pay adjustment was not found");
            if (ModelState.IsValid)
            {
                var hasPrevRecords = await context.JobPayComponents.AnyAsync(x => x.JobId == first.JobId);
                if (hasPrevRecords)
                    return ThrowJsonError("Employee already has pay components");

                var newDed = model.Where(a => a.IsActive).ToList();
                if (newDed.Count() <= 0)
                    return ThrowJsonError("Please choose atleast one pay component");

                newDed.ForEach(t => { t.EffectiveDate = date; });

                context.JobPayComponents.AddRange(newDed);
                context.SaveChanges();

                SetTempDataMessage("Default pay componenets for job were just updated");
                return RedirectToAction(nameof(Detail), new { id = first.JobId });
            }

            return BadRequest(ModelState);
        }


        //public async Task<IActionResult> ViewLeaves(int id)
        //{
        //    var emp = await context.Jobs
        //        .Where(x => x.Id == id)
        //        .FirstOrDefaultAsync();
        //    var leaves = await context.DayOffs.ToListAsync();
        //    if (leaves.Any() == false)
        //        return ThrowJsonError("Oops! Company does not have pay components configured!");

        //    ViewBag.job = emp;

        //    if (emp == null)
        //        return BadRequest();

        //    return PartialView("_AddLeaves", leaves);
        //}


        public async Task<IActionResult> ViewLeaves(int id)
        {
            if (!context.Jobs.Any(x => x.Id == id)) return ThrowJsonError();
            var jobData = await context.Jobs.FindAsync(id);
            ViewBag.job = jobData;

            var dayOffs = await companyService.GetDayOffs();
            if (dayOffs.Any() == false)
                return ThrowJsonError("Oops! Company does not have pay components configured!");

            return PartialView("_ViewLeaves", dayOffs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDayOffs(List<DayOff> model, int id)
        {
            var jobData = await context.Jobs.FindAsync(id);
            if (jobData == null) return ThrowJsonError("Job was not found");

            if (ModelState.IsValid)
            {
                var newDed = model.Where(a => a.IsActive).Select(a=>a.Id).ToArray();
                if (newDed.Count() <= 0)
                    return ThrowJsonError("Please choose atleast one leave policy");

                jobData.DayOffIds = newDed;

                context.Jobs.Update(jobData);
                context.SaveChanges();

                SetTempDataMessage("Associated leaves were just updated");
                return RedirectToAction(nameof(ViewLeaves), new { id = id });
            }

            return BadRequest(ModelState);
        }


        //public async Task<IActionResult> Index()
        //{
        //    int page = 1;
        //    var emp = await context.Jobsicle_Jobs.OrderByDescending(x=> x.created_at).Include(x=>x.company)
        //        .Take(20)
        //        .ToListAsync();

        //        //await context.PayAdjustments
        //        //.Include(x => x.Fields)
        //        //.OrderByDescending(x => x.CalculationOrder > 0)
        //        //.ThenBy(x => x.CalculationOrder)
        //        //.ToListAsync();

        //    //if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        //    //    return PartialView("_Listing", GetNewList());

        //    return View(emp);
        //}

        //public async Task<IActionResult> Manage()
        //{
        //    int page = 1;
        //    var com = context.Jobsicle_Companies.Take(10).ToList();
        //    //var emp = await accessGrantService.GetAllAccessiblePayAdjustmentsAsync(page, int.MaxValue);

        //    //await context.PayAdjustments
        //    //.Include(x => x.Fields)
        //    //.OrderByDescending(x => x.CalculationOrder > 0)
        //    //.ThenBy(x => x.CalculationOrder)
        //    //.ToListAsync();

        //    //if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        //    //    return PartialView("_Listing", GetNewList());

        //    return View();
        //}

        //public async Task<IActionResult> UpdateJobsicle()
        //{
        //    int page = 1;


        //    var url = "https://jobsicle.mv/jobs/1816/json";
        //    var urlPaged = "https://jobsicle.mv/jobs/more?page=1";
        //    IEnumerable<Models.JobScrape.Jobsicle_Job> jobs = null;
        //    MorePageVm pagedVm = null;

        //    var request = new HttpRequestMessage(HttpMethod.Get, url);
        //    request.Headers.Add("Accept", "application/vnd.github.v3+json");
        //    //request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

        //    string sampleRespose = await System.IO.File.ReadAllTextAsync(environment.ContentRootPath + "/Filters/sampleResponseJson_Jobscicle_MorePage.json");
        //    //using (var client = clientFactory.CreateClient())
        //    {
        //        //var response = await client.SendAsync(request);

        //        if (true) //response.IsSuccessStatusCode)
        //        {
        //            string responseBody = sampleRespose; // await response.Content.ReadAsStringAsync();
        //            pagedVm = JsonConvert.DeserializeObject
        //                <MorePageVm>(responseBody);

        //            await AddOrUpdateJobsIncomingJobs(pagedVm);
        //        }
        //        else
        //        {
        //            jobs = Array.Empty<Models.JobScrape.Jobsicle_Job>();
        //        }
        //    }


        //    return Content
        //        ($@"Total Recieved: {pagedVm.data.Count()}\n
        //            Jobs Created: {cnt_jobs_created} \n
        //            Jobs Updated: {cnt_jobs_updated}\n
        //            Company Created: {cnt_comps_created}\n
        //            Company Updated: {cnt_comps_updated}");
        //}

        //private async Task<bool> AddOrUpdateJobsIncomingJobs(MorePageVm pagedVm)
        //{
        //    var listJobsToCreate = new List<Jobsicle_Job>();
        //    foreach (var item in pagedVm.data)
        //    {
        //        if (await context.Jobsicle_Jobs.AnyAsync(x => x.jobsicleId == item.id))
        //        {
        //            cnt_jobs_updated++;
        //        }
        //        else
        //        {
        //            var newJob = item;
        //            newJob.companyId = await AddOrUpdateCompany(item.company);
        //            newJob.company = null;
        //            newJob.jobsicleId = newJob.id;
        //            newJob.id = 0;

        //            listJobsToCreate.Add(newJob);

        //            cnt_jobs_created++;
        //        }
        //    }

        //    if (listJobsToCreate.Any())
        //    {
        //        await context.Jobsicle_Jobs.AddRangeAsync(listJobsToCreate);
        //        await context.SaveChangesAsync();
        //    }

        //    return true;
        //}

        //private async Task<int?> AddOrUpdateCompany(Jobsicle_Company item)
        //{
        //    if (item == null)
        //        return (int?)null;

        //    var inDb = await context.Jobsicle_Companies.Where(x => x.jobsicleId == item.id)
        //        .Select(x => x.id).FirstOrDefaultAsync();
        //    if (inDb > 0)
        //    {
        //        cnt_comps_updated++;
        //        return inDb;
        //    }

        //    var newCmp = item;

        //    if (newCmp.rating != null)
        //    {
        //        newCmp.rating.jobsicleId = newCmp.rating.id;
        //        newCmp.rating.id = 0;
        //    }

        //    if (newCmp.office_hours != null)
        //    {
        //        newCmp.office_hours.jobsicleId = newCmp.office_hours.id;
        //        newCmp.office_hours.id = 0;
        //    }

        //    if(newCmp.photos != null)
        //    newCmp.photos.ToList()?.ForEach(x => { x.jobsicleId = x.id; x.id = 0; });


        //    newCmp.jobsicleId = newCmp.id;
        //    newCmp.id = 0;
        //    context.Jobsicle_Companies.Add(newCmp);
        //    await context.SaveChangesAsync();

        //    cnt_comps_created++;
        //    return newCmp.id;
        //}
    }
}
