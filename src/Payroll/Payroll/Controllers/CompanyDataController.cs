using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;

namespace Payroll.Controllers
{
    public class CompanyDataController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly UserResolverService useeResolverService;
        private readonly CompanyService cmpanyService;

        public CompanyDataController(PayrollDbContext context, UserResolverService useeResolverService, CompanyService cmpanyService)
        {
            this.context = context;
            this.useeResolverService = useeResolverService;
            this.cmpanyService = cmpanyService;
        }

        //public IActionResult Index()
        //{
        //    var emp = context.Employees
        //        .Include(x => x.PayrollPeriodEmployees)
        //        .ToList();
        //    ViewData["Departments"] = context.Departments.ToList();

        //    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        //        return PartialView("_Listing", emp);
        //    return View(emp);
        //}

        #region Department

        public IActionResult AddOrUpdateDepartment(int cmpId, int id = 0)
        {
            if (id == 0)
                return PartialView("_AddOrUpdateDepartment", new Department() { CompanyId = cmpId });

            var ept = context.Departments.Include(x => x.Employees).FirstOrDefault(x => x.Id == id);
            if (ept == null) return ThrowJsonError();

            if (ept == null) ept = new Department() {  CompanyId = cmpId };
            return PartialView("_AddOrUpdateDepartment", ept);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddOrUpdateDepartment(Department model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    // create
                    context.Departments.Add(model);
                }
                else
                {
                    context.Departments.Update(model);
                }

                context.SaveChanges();
                return RedirectToAction(nameof(ViewDepartments), new { id= model.CompanyId });
            }

            return ThrowJsonError(ModelState);
        }


        public async Task<IActionResult> ViewDepartments(int id)
        {
            ViewBag.CountData = await context.Employees.Where(a => a.CompanyId == id && a.DepartmentId.HasValue)
                .AsQueryable()
                .GroupBy(a => a.DepartmentId)
                .Select(a=> new { a.Key, Count = a.Count() })
                .ToDictionaryAsync(a => a.Key, a => a.Count );

            //var cmpId = dept.CompanyId;
            return PartialView("_ViewDepartments", new Company
            {
                Id = id,
                Departments = await context.Departments.Where(a => a.CompanyId == id)
                .Include(x => x.DepartmentHeads)
                .ThenInclude(x=> x.Employee)
                //.ThenInclude(x=> x.Individual)
                .ToListAsync()
            });
        }


        [HttpPost]
        public IActionResult RemoveDepartment(int id)
        {
            if (ModelState.IsValid)
            {
                var add = context.Departments.FirstOrDefault(x => x.Id == id);
                if (add == null)
                    return ThrowJsonError("Oooh! we didnt find that one");
                if (context.Employees.Any(x => x.DepartmentId == add.Id))
                    return ThrowJsonError("Ouch! Please remove all employees in this department first!");

                context.Departments.Remove(add);
                context.SaveChanges();
                return RedirectToAction("Index", "Employee");
            }

            return ThrowJsonError();
        }

        public async Task<IActionResult> AddOrUpdateManager(int cmpId, int deptId = 0, int id= 0)
        {
            DepartmentHead dh = null;
            if (id == 0)
                dh = new DepartmentHead { DepartmentId = deptId, CompanyId = cmpId };
            else
            {
                dh = await context.DepartmentHeads.FindAsync(id);
                if (dh == null)
                    return ThrowJsonError("Manager was not found");
            }

            var dpts = await cmpanyService.GetDepartmentsOfCurremtCompany(cmpId);
            ViewBag.DepartmentId = new SelectList(dpts, "Id", "Name", dh.DepartmentId);

            //var empls = await cmpanyService.GetEmployeesOfCurremtCompany(cmpId);
            //ViewBag.EmployeeIds = empls;


            return PartialView("_AddOrUpdateManager", dh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateManager(DepartmentHead model)
        {
            if (ModelState.IsValid)
            {
                if (await context.DepartmentHeads.AnyAsync(a => a.DepartmentId == model.DepartmentId &&
                     (model.Id == 0 || model.Id != a.Id) && a.EmployeeId == model.EmployeeId))
                    return ThrowJsonError("Employee already is assigned as manager to this department");

                if (!await context.Employees.AnyAsync(a => a.Department.CompanyId == model.CompanyId &&
                     a.Id == model.EmployeeId))
                    return ThrowJsonError("Employee doesn't belong to this Company");


                if (model.Id == 0)
                {
                    // create
                    context.DepartmentHeads.Add(model);
                }
                else
                {
                    context.DepartmentHeads.Update(model);
                }

                context.SaveChanges();
                return RedirectToAction(nameof(ViewDepartments), new { id = model.CompanyId });
            }

            return ThrowJsonError(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveManager(int cmpId, int id)
        {
            if(ModelState.IsValid)
            {
                var dh = await context.DepartmentHeads.FirstOrDefaultAsync(a=> a.Department.CompanyId == cmpId && a.Id == id);
                if (dh == null)
                    return ThrowJsonError("Oooh! we didnt find that one");

                var deptId = dh.DepartmentId;
                context.DepartmentHeads.Remove(dh);
                context.SaveChanges();
                return RedirectToAction(nameof(ViewDepartments), new { cmpId });
            }

            return ThrowJsonError();
        }


        #endregion

        #region division

        public async Task<IActionResult> ViewDivisions(int id)
        {
            ViewBag.CountData = await context.Employees.Where(a => a.CompanyId == id && a.DivisionId.HasValue)
                .GroupBy(a => a.DivisionId)
                .ToDictionaryAsync(a => a.Key, a => a.Count());
            return PartialView("_ViewDivisions", new Company
            {
                Id = id,
                Divisions = await context.Divisions.Where(a => a.CompanyId == id).ToListAsync()
            });
        }

        public async Task<IActionResult> AddOrUpdateDivision(int cmpId, int id = 0)
        {
            var data = new Division() { CompanyId = cmpId };
            if (id > 0)
            {
                data = await context.Divisions.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null) return ThrowJsonError();
            }
            
            return PartialView("_AddOrUpdateDivision", data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddOrUpdateDivision(Division model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                    context.Divisions.Add(model);
                else
                    context.Divisions.Update(model);

                context.SaveChanges();
                return RedirectToAction(nameof(ViewDivisions), new { id = model.CompanyId });
            }

            return ThrowJsonError(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveDivision(int id)
        {
            if (ModelState.IsValid)
            {
                var data = await context.Divisions.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null)
                    return ThrowJsonError("Oooh! we didnt find that one");
                if (context.Employees.Any(x => x.DivisionId == data.Id))
                    return ThrowJsonError("Ouch! Please remove all employees in this division first!");

                context.Divisions.Remove(data);
                context.SaveChanges();
                return RedirectToAction(nameof(ViewDivisions), new { id = data.CompanyId });
            }

            return ThrowJsonError();
        }

        #endregion


        #region teams

        public async Task<IActionResult> ViewTeams(int id)
        {
            ViewBag.CountData = await context.EmployeeTeams.Where(a => a.Team.CompanyId == id)
                .GroupBy(a => a.TeamId)
                .ToDictionaryAsync(a => a.Key, a => a.Count());
            return PartialView("_ViewTeams", new Company
            {
                Id = id,
                Teams = await context.Teams.Where(a => a.CompanyId == id).ToListAsync()
            });
        }

        public async Task<IActionResult> AddOrUpdateTeam(int cmpId, int id = 0)
        {
            var data = new Team() { CompanyId = cmpId };
            if (id > 0)
            {
                data = await context.Teams.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null) return ThrowJsonError();
            }

            return PartialView("_AddOrUpdateTeam", data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddOrUpdateTeam(Team model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                    context.Teams.Add(model);
                else
                    context.Teams.Update(model);

                context.SaveChanges();
                return RedirectToAction(nameof(ViewTeams), new { id = model.CompanyId });
            }

            return ThrowJsonError(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTeam(int id)
        {
            if (ModelState.IsValid)
            {
                var data = await context.Teams.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null)
                    return ThrowJsonError("Oooh! we didnt find that one");
                if (context.Employees.Any(x => x.DivisionId == data.Id))
                    return ThrowJsonError("Ouch! Please remove all employees in this division first!");

                context.Teams.Remove(data);
                context.SaveChanges();
                return RedirectToAction(nameof(ViewDivisions), new { id = data.CompanyId });
            }

            return ThrowJsonError();
        }

        #endregion


        #region Location

        public async Task<IActionResult> ViewLocations(int id)
        {
            ViewBag.CountData = await context.Employees.Where(a => a.CompanyId == id && a.LocationId.HasValue)
                .GroupBy(a => a.LocationId)
                .ToDictionaryAsync(a => a.Key, a => a.Count());
            return PartialView("_ViewLocations", new Company
            {
                Id = id,
                Locations = await context.Locations.Where(a => a.CompanyId == id).ToListAsync()
            });
        }

        public async Task<IActionResult> AddOrUpdateLocation(int cmpId, int id = 0)
        {
            var data = new Location() { CompanyId = cmpId };
            if (id > 0)
            {
                data = await context.Locations.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null) return ThrowJsonError();
            }

            return PartialView("_AddOrUpdateLocation", data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddOrUpdateLocation(Location model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                    context.Locations.Add(model);
                else
                    context.Locations.Update(model);

                context.SaveChanges();
                return RedirectToAction(nameof(ViewLocations), new { id = model.CompanyId });
            }

            return ThrowJsonError(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveLocation(int id)
        {
            if (ModelState.IsValid)
            {
                var data = await context.Locations.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null)
                    return ThrowJsonError("Oooh! we didnt find that one");
                if (context.Employees.Any(x => x.LocationId == data.Id))
                    return ThrowJsonError("Ouch! Please remove all employees in this location first!");

                context.Locations.Remove(data);
                context.SaveChanges();
                return RedirectToAction(nameof(ViewLocations), new { id = data.CompanyId });
            }

            return ThrowJsonError();
        }

        #endregion




        #region ViewEmergencyContactRelationships

        public async Task<IActionResult> ViewEmergencyContactRelationships(int id)
        {
            ViewBag.CountData = await context.Employees.Where(a => a.CompanyId == id && a.EmergencyContactRelationshipId.HasValue)
                .GroupBy(a => a.EmergencyContactRelationshipId)
                .ToDictionaryAsync(a => a.Key, a => a.Count());
            ViewBag.CmpId = id;
            return PartialView("_ViewEmergencyContactRelationships", await context.EmergencyContactRelationships.Where(a => a.CompanyId == id || a.CompanyId.HasValue == false).ToListAsync());
        }


        public async Task<IActionResult> AddOrUpdateEmergencyContactRelationship(int cmpId, int id = 0)
        {
            var data = new EmergencyContactRelationship() { CompanyId = cmpId };
            if (id > 0)
            {
                data = await context.EmergencyContactRelationships.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null) return ThrowJsonError();
                if (data.CompanyId != cmpId ) return ThrowJsonError("Sorry! you cannot edit this one");
            }

            return PartialView("_AddOrUpdateEmergencyContactRelationship", data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddOrUpdateEmergencyContactRelationship(EmergencyContactRelationship model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                    context.EmergencyContactRelationships.Add(model);
                else
                    context.EmergencyContactRelationships.Update(model);

                context.SaveChanges();
                return RedirectToAction(nameof(ViewEmergencyContactRelationships), new { id = model.CompanyId });
            }

            return ThrowJsonError(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveEmergencyContactRelationship(int id)
        {
            if (ModelState.IsValid)
            {
                var data = await context.EmergencyContactRelationships.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null)
                    return ThrowJsonError("Oooh! we didnt find that one");
                if (context.Employees.Any(x => x.EmergencyContactRelationshipId == data.Id))
                    return ThrowJsonError("Ouch! Please remove all employees in this relationship first!");

                context.EmergencyContactRelationships.Remove(data);
                context.SaveChanges();
                return RedirectToAction(nameof(ViewEmergencyContactRelationships), new { id = data.CompanyId });
            }

            return ThrowJsonError();
        }

        #endregion




        #region Nationality

        public async Task<IActionResult> ViewNationalities(int id)
        {
            ViewBag.CountData = await context.Employees.Where(a => a.CompanyId == id && a.NationalityId.HasValue)
            //.Select(x=> x.Individual)
                .GroupBy(a => a.NationalityId)
                .ToDictionaryAsync(a => a.Key, a => a.Count());
            ViewBag.CmpId = id;
            return PartialView("_ViewNationalities", await context.Nationalities.Where(a => a.CompanyId == id || a.CompanyId.HasValue == false).ToListAsync());
        }


        public async Task<IActionResult> AddOrUpdateNationality(int cmpId, int id = 0)
        {
            var data = new Nationality() { CompanyId = cmpId };
            if (id > 0)
            {
                data = await context.Nationalities.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null) return ThrowJsonError();
                if (data.CompanyId != cmpId) return ThrowJsonError("Sorry! you cannot edit this one");
            }

            return PartialView("_AddOrUpdateNationality", data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateNationality(Nationality model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                    context.Nationalities.Add(model);
                else
                {
                    var data = await context.Nationalities.FirstOrDefaultAsync(x => x.Id == model.Id);
                    if (data.CompanyId.HasValue == false) return ThrowJsonError("Sorry! you cannot edit this one");
                    data.Name = model.Name;
                    context.Nationalities.Update(data);
                }

                context.SaveChanges();
                return RedirectToAction(nameof(ViewNationalities), new { id = model.CompanyId });
            }

            return ThrowJsonError(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveNationality(int id)
        {
            if (ModelState.IsValid)
            {
                var data = await context.Nationalities.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null)
                    return ThrowJsonError("Oooh! we didnt find that one");
                if (context.Employees.Any(x => x.NationalityId == data.Id))
                    return ThrowJsonError("Ouch! Please remove all employees in this nationality first!");

                context.Nationalities.Remove(data);
                context.SaveChanges();
                return RedirectToAction(nameof(ViewNationalities), new { id = data.CompanyId });
            }

            return ThrowJsonError();
        }

        #endregion


        #region Nationality

        public async Task<IActionResult> ViewTerminationReasons(int id)
        {
            ViewBag.CmpId = id;
            return PartialView("_ViewTerminationReasons", await context.TerminationReasons.Where(a => a.CompanyId == id || a.CompanyId.HasValue == false).ToListAsync());
        }


        public async Task<IActionResult> AddOrUpdateTerminationReason(int cmpId, int id = 0)
        {
            var data = new TerminationReason() { CompanyId = cmpId };
            if (id > 0)
            {
                data = await context.TerminationReasons.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null) return ThrowJsonError();
                if (data.CompanyId != cmpId) return ThrowJsonError("Sorry! you cannot edit this one");
            }

            return PartialView("_AddOrUpdateTerminationReason", data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateTerminationReason(TerminationReason model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                    context.TerminationReasons.Add(model);
                else
                {
                    var data = await context.TerminationReasons.FirstOrDefaultAsync(x => x.Id == model.Id);
                    if (data.CompanyId.HasValue == false) return ThrowJsonError("Sorry! you cannot edit this one");
                    data.Name = model.Name;
                    context.TerminationReasons.Update(data);
                }

                context.SaveChanges();
                return RedirectToAction(nameof(ViewTerminationReasons), new { id = model.CompanyId });
            }

            return ThrowJsonError(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTerminationReason(int id)
        {
            if (ModelState.IsValid)
            {
                var data = await context.TerminationReasons.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null)
                    return ThrowJsonError("Oooh! we didnt find that one");
                //if (context.EmployeeTypes.Any(x => x.TerminationReasonId == data.Id))
                //    return ThrowJsonError("Ouch! Please remove all employees in this termination reason first!");

                context.TerminationReasons.Remove(data);
                context.SaveChanges();
                return RedirectToAction(nameof(ViewTerminationReasons), new { id = data.CompanyId });
            }

            return ThrowJsonError();
        }

        #endregion
    }
}

