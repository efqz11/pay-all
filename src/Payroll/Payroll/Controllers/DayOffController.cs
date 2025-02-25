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
    public class DayOffController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly UserResolverService useeResolverService;
        private readonly AccessGrantService accessGrantService;
        private readonly CompanyService companyService;

        public DayOffController(PayrollDbContext context, UserResolverService useeResolverService,
            AccessGrantService accessGrantService, CompanyService companyService)
        {
            this.context = context;
            this.useeResolverService = useeResolverService;
            this.accessGrantService = accessGrantService;
            this.companyService = companyService;
        }

        public async Task<IActionResult> Index(int? id = null)
        {
            int _id =  id ?? useeResolverService.GetCompanyId();
            var emp = context.DayOffs
                .Where(a=> a.CompanyId == _id)
                .Include(x => x.DayOffEmployees)
                .ToList();

            ViewBag.Year = DateTime.Now.Year;
            ViewBag.CompanyId = _id;
            ViewBag.CompanyPublicHolidays = await context.CompanyPublicHolidays.Where(x => x.CompanyId == _id && x.Date.Year == DateTime.Now.Year).ToListAsync();
            ViewBag.Id = _id;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Listing", emp);
            return View(emp);
        }

        public IActionResult Detail(int id = 0)
        {
            var ept = context.DayOffs
                .Include(x => x.DayOffEmployees)
                .ThenInclude(x => x.Employee)
                .FirstOrDefault(x => x.Id == id);
            EnsureDayOffIsValid(ept);

            if(Request.IsAjaxRequest())
                return PartialView("_Detail", ept);
            return View(ept);
        }
        public IActionResult AddOrUpdate(int cmpId, int id = 0)
        {
            //if (id == 0)
            //    return PartialView("_AddOrUpdate");
            var ept = context.DayOffs.Include(x => x.DayOffEmployees).FirstOrDefault(x => x.Id == id);
            if (ept == null && id > 0) return BadRequest();
            if(ept != null && !ept.IsActive)
                return RedirectToAction(nameof(AddOrUpdateSettings), new { id = ept.Id });
                
            if (ept == null) ept = new DayOff() { CompanyId = cmpId };

            ViewBag.Colors = new SelectList(BootstrapColors.GetColors(), ept.Color);
            return PartialView("_AddOrUpdate", ept);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdate(DayOff model)
        {
            if (ModelState.IsValid)
            {
                if (model.MustRequestBefore && model.MustRequestBeforeAlert.HasValue == false)
                    return ThrowJsonError("Please choose request days/hours before value");
                if (model.IsForSpecificGender && model.Gender.HasValue == false)
                    return ThrowJsonError("Please specify gender");
                if (model.RequiredDocumentForConseqetiveDays && model.ConsquetiveDaysRequire <= 0)
                    return ThrowJsonError("Please enter conseqetive days");
                if (model.RequiredDocuments && string.IsNullOrWhiteSpace(model.RequiredDocumentList))
                    return ThrowJsonError("Please enter required documents list in csv format");

                if (model.RequiredDocumentForConseqetiveDays && (!model.RequiredDocuments && !string.IsNullOrWhiteSpace(model.RequiredDocumentList)))
                    return ThrowJsonError("Please choose required documents if consecutive days are selected");

                if (model.IsThereLimit)
                {
                    if (!model.AccrualMethod.HasValue)
                        return ThrowJsonError("Please choose accrual method.");

                    if (model.AccrualMethod == AccrualMethod.Fixed && model.TotalHoursPerYear.GetValueOrDefault() <= 0)
                        return ThrowJsonError("Please enter number of hours that will be earned each year.");
                    //if (model.AccrualMethod == AccrualMethod.Fixed && (!model.TotalPerYear.HasValue || model.TotalPerYear <= 0))
                    //    return ThrowJsonError("Please enter number of hours that will be earned each year.");


                    if (model.AccrualMethod == AccrualMethod.Hourly && !model.AccureTimeBasedOn.HasValue)
                        return ThrowJsonError("Please choose accrual time base on option.");
                    if (model.AccrualMethod == AccrualMethod.Hourly && (!model.HoursEarned.HasValue || model.HoursEarned <= 0) && (!model.PerHoursWorked.HasValue || model.PerHoursWorked <= 0))
                        return ThrowJsonError("Please enter the accural hourly rate.");

                }
                //if (!model.ResetEveryYear)
                //    return ThrowJsonError("Please choose your preferred reset option");


                if (await context.DayOffs.AnyAsync(a=> a.Name.ToLower().Equals(model.Name.ToLower()) && a.CompanyId == model.CompanyId && (model.Id == 0 || model.Id != a.Id)))
                    return ThrowJsonError($"Oh! We already have {model.Name} configured in this company");
            }

            var cmp = await companyService.GetCompanyAccount(model.CompanyId);
            if (cmp == null) ModelState.AddModelError("", "Company account was not found!");

            if (ModelState.IsValid)
            {
                model.CompanyId = model.CompanyId;
                model.TotalPerYear = model.TotalHoursPerYear.GetValueOrDefault();
                model.RequireSubstitiute = model.RequireSubsituteEnum != RequireSubsitute.No;
                model.RequireSubstitiuteOptional = model.RequireSubsituteEnum == RequireSubsitute.Optional;
                if (model.Id == 0)
                {
                    // create
                    model.IsActive = false;
                    context.DayOffs.Add(model);
                }
                else
                {
                    context.DayOffs.Update(model);
                }
                
                await context.SaveChangesAsync();

                if(model.IsThereLimit)
                    return RedirectToAction(nameof(AddOrUpdateSettings), new { id = model.Id });
                else
                    return RedirectToAction(nameof(AddOrRemoveEmployees), new { id = model.Id });
            }

            return ThrowJsonError(ModelState);
        }

        public IActionResult AddOrUpdateSettings(int id)
        {
            //if (id == 0)
            //    return PartialView("_AddOrUpdate");
            var ept = context.DayOffs.FirstOrDefault(x => x.Id == id);
            if (ept == null && id > 0) return BadRequest();

            return PartialView("_AddOrUpdateSettings", ept);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateSettings(DayOff model)
        {
            if (ModelState.IsValid)
            {
                if (model.IsThereWaitingPeriodForAccrue && model.LengthWaitingPeriodForAccrue <= 0)
                    return ThrowJsonError("Please enter length of waiting period before they begin accruing time off");
                if (model.IsThereWaitingPeriodForRequest && model.LengthWaitingPeriodForRequest <= 0)
                    return ThrowJsonError("Please enter length of waiting period before they can request time off");
                if (model.IsThereCarryOverLimit && model.CarryOverLimit <= 0)
                    return ThrowJsonError("Please specify how many hours an employee can carry over from one calendar year to the next.");


            }

            var dayOff = await context.DayOffs.FindAsync(model.Id);
            if (dayOff == null) ModelState.AddModelError("", "Time off policy was not found!");

            if (ModelState.IsValid)
            {
                dayOff.IsOutstandingBalancePaidUponDismissial = model.IsOutstandingBalancePaidUponDismissial;
                dayOff.CarryOverLimit = model.CarryOverLimit;
                dayOff.IsThereCarryOverLimit = model.IsThereCarryOverLimit;
                dayOff.MaxBalance = model.MaxBalance;
                dayOff.MaxAccuredHoursPerYear = model.MaxAccuredHoursPerYear;
                dayOff.IsThereWaitingPeriodForAccrue = model.IsThereWaitingPeriodForAccrue;
                dayOff.IsThereWaitingPeriodForRequest = model.IsThereWaitingPeriodForRequest;
                dayOff.LengthWaitingPeriodForAccrue = model.LengthWaitingPeriodForAccrue;
                dayOff.LengthWaitingPeriodForRequest = model.LengthWaitingPeriodForRequest;

                //context.DayOffs.Update(model);

                //if (!dayOff.IsActive)
                //{
                //    //  has Active EMploes
                //    if (await companyService.GetEmployeeCount() > 0)
                //    {
                        await context.SaveChangesAsync();
                //        return RedirectToAction(nameof(AddOrRemoveEmployees), new { id = dayOff.Id });
                //    }
                //    else
                //    {
                //        dayOff.IsActive = true;
                //        await context.SaveChangesAsync();
                //        await companyService.IsCompanyStepDone(model.CompanyId, 8);
                //        return PartialView("_PolicyCreatedAndActive", dayOff);
                //        //return RedirectToAction(nameof(AddOrRemoveEmployees), new { id = dayOff.Id });
                //    }
                //}


                return RedirectToAction(nameof(AddOrRemoveEmployees), new { id = dayOff.Id });
            }

            return ThrowJsonError(ModelState);
        }


        public async Task<IActionResult> AddOrRemoveEmployees(int id)
        {
            //if (id == 0)
            //    return PartialView("_AddOrUpdate");
            var emps = await companyService.GetAllEmployeesInMyCompanyForDropdownOptGroups(EmployeeStatus.Active);
            var alreadyEntrolled = await context.DayOffEmployees.Where(x => x.DayOffId == id).Select(x=> x.Employee).ToListAsync();
            var dayOff = await context.DayOffs.FindAsync(id);
            if (dayOff == null) return BadRequest();


            if (emps.Count <= 0 && !dayOff.IsActive)
            {
                dayOff.IsActive = true;
                await companyService.UpdateProgressAndSaveAsync(dayOff.CompanyId, 8);
                await context.SaveChangesAsync();
                return PartialView("_PolicyCreatedAndActive", dayOff);
            }

            ViewBag.Id = id;
            ViewBag.Name = dayOff.Name;

            var model = emps.Except(alreadyEntrolled).ToList();
            model.ForEach(t => t.IsActive = false);
            alreadyEntrolled.ForEach(t => t.IsActive = true);
            model.AddRange(alreadyEntrolled);

            return PartialView("_AddOrRemoveEmployees", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrRemoveEmployees(int id, List<Employee> model)
        {
            var dayOff = await context.DayOffs.Include(t=> t.DayOffEmployees).FirstOrDefaultAsync(t=> t.Id== id);
            EnsureDayOffIsValid(dayOff);

            var selectedEmpIds = model.Where(t => t.IsActive).Select(t => t.Id).ToList();
            var newEnrolledEmpIds = selectedEmpIds.Except(dayOff.DayOffEmployees.Select(t => t.EmployeeId)).ToList();
            bool hasNewEnrollments = selectedEmpIds.Any() && newEnrolledEmpIds.Any();
            if (hasNewEnrollments)
            {
                // add kickoff 
                ViewBag.Id = dayOff.Id;
                var kickOff = await context.Employees.Where(t => t.CompanyId == dayOff.CompanyId && newEnrolledEmpIds.Contains(t.Id))
                    .Select(t => new DayOffEmployee { DayOffId = id, EmployeeId = t.Id, Employee = t }).ToListAsync();
                return PartialView("_KickOff", kickOff);
            }
            else
            {

                if (dayOff.IsActive == false)
                {
                    dayOff.IsActive = true;
                    await context.SaveChangesAsync();
                    return PartialView("_PolicyCreatedAndActive", dayOff);
                }

                else
                    SetTempDataMessage($"{dayOff} was just updated");

                return RedirectToAction(nameof(Detail), "DayOff", new { id = dayOff.Id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KickOffConfirm(int id, List<DayOffEmployee> model)
        {
            var dayOff = await context.DayOffs.Include(t => t.DayOffEmployees).FirstOrDefaultAsync(t => t.Id == id);
            EnsureDayOffIsValid(dayOff);

            var selectedEmpIds = model.Select(t => t.EmployeeId).ToList();
            var newEnrolledEmpIds = selectedEmpIds.Except(dayOff.DayOffEmployees.Select(t => t.EmployeeId)).ToList();
            bool hasNewEnrollments = selectedEmpIds.Any() && newEnrolledEmpIds.Any();
            if (hasNewEnrollments)
            {
                // always will have new enrollments here
                var newAdd = newEnrolledEmpIds.Select(x => new DayOffEmployee
                {
                    EmployeeId = x,
                    DayOffId = dayOff.Id,
                    TotalHours = 0,
                    TotalCollectedHours = 0,
                    TotalRemainingHours = 0
                }).ToList();

                if(model.Any(t=> t.TotalHours > 0))
                {
                    var trackerAdds = model.Where(t => t.TotalHours > 0)
                    .Select(t => new DayOffTracker
                    {
                        EmployeeId = t.EmployeeId,
                        DayOffId = dayOff.Id,
                        TotalHours = t.TotalHours,
                        RemainingBalance = t.TotalHours,
                        IsAddOrSubState = true,
                        IsCreateByDuringKickOff = true,
                        Summary = "Created on initial kick-off"
                    }).ToList();

                    // add to dayoffEmployee TrackerList
                    foreach (var item in trackerAdds)
                    {
                        newAdd.First(t => t.EmployeeId == item.EmployeeId).DayOffTrackers.Add(item);
                        newAdd.First(t => t.EmployeeId == item.EmployeeId).TotalHours = newAdd.First(t => t.EmployeeId == item.EmployeeId).TotalRemainingHours = item.TotalHours;
                    }
                }

                if (dayOff.IsActive == false)
                {
                    dayOff.IsActive = true;
                    await context.DayOffEmployees.AddRangeAsync(newAdd);
                    await context.SaveChangesAsync();
                    return PartialView("_PolicyCreatedAndActive", dayOff);
                }
                else
                {
                    SetTempDataMessage($"{newEnrolledEmpIds.Count} employee(s) enrolled successfullt");
                    return RedirectToAction(nameof(Detail), "DayOff", new { id = dayOff.Id });
                }
            }

            return ThrowJsonError("Sorry! you've reached dead end here!");
        }

        public async Task<IActionResult> Tracker(int id, int dayOffEmpId)
        {
            var d = await context.DayOffEmployees
                .Include(t => t.DayOffTrackers)
                .FirstOrDefaultAsync(e => e.Id == dayOffEmpId && e.DayOffId == id);

            return View(d);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveEmployee(int id, int dayOffEmpId)
        {
            var d = await context.DayOffEmployees
                .Include(t=> t.DayOffTrackers)
                .FirstOrDefaultAsync(e => e.Id == dayOffEmpId && e.DayOffId == id);
            if (d != null)
            {
                context.DayOffTrackers.RemoveRange(d.DayOffTrackers);
                context.DayOffEmployees.Remove(d);
                await context.SaveChangesAsync();
            }
            return Ok();
        }

        private async Task<IActionResult> EnrollEmployee(int dayOffId, int empId, int? rem = null)
        {
            var dayOff = await context.DayOffs.FindAsync(dayOffId);
            EnsureDayOffIsValid(dayOff);

            ViewBag.Id = dayOffId;
            ViewBag.Name = dayOff.Name;

            var d = await context.DayOffEmployees.FirstOrDefaultAsync(e => e.EmployeeId == empId && e.DayOffId == dayOffId);
            if (rem.HasValue && rem == -1) // remove
            {
                context.DayOffEmployees.Remove(d);
                await context.SaveChangesAsync();
                return Ok();
            }
            else if (rem.HasValue && rem == 0 && empId == 0) // add all
            {
                var emps11 = await companyService.GetAllEmployeesInMyCompanyForDropdownOptGroups(EmployeeStatus.Active);
                var alreadyEntrolled1 = await context.DayOffEmployees.Where(x => x.DayOffId == dayOffId).Select(x => x.Employee).ToListAsync();

                var avaiblableEmpls = emps11.Except(alreadyEntrolled1).ToList();
                if(avaiblableEmpls.Count <= 0)
                    return ThrowJsonError("All employees are enrolled");

                var newAdd = avaiblableEmpls.Select(x => new DayOffEmployee
                {
                    EmployeeId = x.Id,
                    DayOffId = dayOffId,
                    TotalHours = dayOff.TotalPerYear.GetValueOrDefault(),
                    TotalCollectedHours = 0,
                    TotalRemainingHours = dayOff.TotalPerYear.GetValueOrDefault()
                }).ToList();

                context.DayOffEmployees.AddRange(newAdd);
                await context.SaveChangesAsync();

                emps11.ForEach(t => t.IsActive = false);
                return PartialView("_AddOrRemoveEmployees", emps11);
            }
            else
            {
                if (d != null)
                    return ThrowJsonError("Employee already exists");

                context.DayOffEmployees.Add(new DayOffEmployee
                {
                    EmployeeId = empId,
                    DayOffId = dayOffId,
                    TotalHours = dayOff.TotalPerYear.GetValueOrDefault(),
                    TotalCollectedHours = 0,
                    TotalRemainingHours = dayOff.TotalPerYear.GetValueOrDefault()
                });
                await context.SaveChangesAsync();
            }


            var emps = await companyService.GetAllEmployeesInMyCompanyForDropdownOptGroups(EmployeeStatus.Active);
            var alreadyEntrolled = await context.DayOffEmployees.Where(x => x.DayOffId == dayOffId).Select(x => x.Employee).ToListAsync();

            var model = emps.Except(alreadyEntrolled).ToList();
            model.ForEach(t => t.IsActive = false);
            alreadyEntrolled.ForEach(t => t.IsActive = true);
            model.AddRange(alreadyEntrolled);

            return PartialView("_AddOrRemoveEmployees", model);
        }

        private JsonResult EnsureDayOffIsValid(DayOff dayOff)
        {
            if(dayOff==null)
                return ThrowJsonError("Day off was not found!");
            if (dayOff.CompanyId != useeResolverService.GetCompanyId())
                return ThrowJsonError("Unauthorized access detected!");


            return default;
        }


        public async Task<IActionResult> GetDayOffEmployees(int? dayOffId = null, int limit = 5, int? year = null)
        {
            year = year == null ? DateTime.Now.Year : year;

            int[] yearList = new int[limit];
            for (int i = 0; i < limit; i++)
            {
                yearList[i] = (year.Value + i);
            }
            year = year == null ? DateTime.Now.Year : year;

            var dayOff = await context.DayOffs.Where(x => x.Id == dayOffId).FirstOrDefaultAsync();
            var dayOfEmps = await context.DayOffEmployees.Where(x => x.DayOff.CompanyId == dayOff.CompanyId
            && x.DayOffId == dayOffId
                && yearList.Contains(x.Year))
                .GroupBy(x => new { x.DayOffId, x.Year })
                .Select(x => new DayOffEmployee
                {
                    DayOffId = x.Key.DayOffId,
                    Year = x.Key.Year,
                    TotalCollectedHours = x.Count(),
                    TotalHours = x.Sum(z=> z.DayOffEmployeeItems.Count(i=>i.Status == DayOffEmployeeItemStatus.Approved)),
                })
                .ToListAsync();

            ViewBag.DayOffId = dayOffId;
            ViewBag.CmpId = dayOff.CompanyId;
            ViewBag.Limit = limit;
            ViewBag.Year = year;
            return PartialView("_DayOffEmployeeByYear", dayOfEmps);
        }


        public async Task<IActionResult> UpdateForEmployees(int id = 0, int? dayOffId = null, int? year = null, int limit = 5)
        {
            year = year == null ? DateTime.Now.Year : year;
            
            var dayOffs = await context.DayOffs.Where(x => x.CompanyId == id).ToListAsync();
            var dayOfEmps = await context.DayOffEmployees.Where(x => x.DayOff.CompanyId == id
                && x.Year == year)
                .Select(x => new DayOffEmployee
                {
                    Employee = x.Employee,
                    DayOff = x.DayOff,
                    Year = x.Year,
                    TotalCollectedHours = x.DayOffEmployeeItems.Count()
                })
                .ToListAsync();

            
            var vm = new DayOffToEmployeeVm
            {
                Years = new int[] { 2020, 2021, 2022, 2023, 2024, 2025 },
                Year = year,
                dayOffs = dayOffs,
                DayOffEmplsInYear = dayOfEmps,
                DayOffId = dayOffId,
            };

            ViewBag.DayOffId = dayOffId;
            ViewBag.YearsSElectList = new SelectList(vm.Years, year);
            ViewBag.DayOffsSelectList = new SelectList(vm.dayOffs
                   , "Id", "Name", dayOffId);
            return PartialView("_UpdateForEmployees", vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateDayOffEmployee(int cmpId, int? dayOffId = null, int? year = null, int? force = null, int? remove = null)
        {
            year = year == null ? DateTime.Now.Year : year;
            
            var dayOfEmps = await context.DayOffEmployees.Where(x => x.DayOff.CompanyId == cmpId
                && x.Year == year && x.DayOffId == dayOffId)
                .Select(x => new DayOffEmployee
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    Year = x.Year,
                })
                .ToListAsync();

            if(remove == 1)
            {
                var query = $"UPDATE [{nameof(DayOffEmployee)}s] SET [IsDeleted] = 1 WHERE Id IN ({string.Join(",", dayOfEmps.Select(x => x.Id))})";
                await context.Database.ExecuteSqlRawAsync(query);
                //await context.SaveChangesAsync();
                //return RedirectToAction("UpdateForEmployees", new { id = useeResolverService.GetCompanyId(), dayOffId, year });
            }


            var empIdList = dayOfEmps.Select(x => x.EmployeeId).Distinct().ToArray();

            var dayOff = await
                    context.DayOffs.Where(x => x.CompanyId == cmpId && x.Id == dayOffId).FirstOrDefaultAsync();
            if (dayOff == null)
                return ThrowJsonError("Day off was not found!");

            var allEmps = await 
                    context.Employees.Where(x => x.Department.CompanyId == cmpId)
                    .Select(x => new { x.Id, x.DateOfJoined, x.Gender }).ToArrayAsync();

            if (dayOff.IsForSpecificGender)
                allEmps = allEmps.Where(x => x.Gender == dayOff.Gender).ToArray();

            if(force==1)
            {
                context.DayOffEmployees.RemoveRange(dayOfEmps);
                var newrecords = allEmps.Select(x => new DayOffEmployee
                {
                    Year = year.Value,
                    EmployeeId = x.Id,
                    TotalHours = dayOff.TotalPerYear ?? 0m,
                    DayOffId = dayOff.Id,
                    TotalRemainingHours = dayOff.TotalPerYear ?? 0m,
                    NextRefreshDate = new DateTime((year.Value+1), 1, 1)
                }).ToList();
                await context.DayOffEmployees.AddRangeAsync(newrecords);
            }
            else
            {
                var newrecords = allEmps
                    .Where(x => !empIdList.Any(z=> z == x.Id))
                    .Select(x => new DayOffEmployee
                    {
                        Year = year.Value,
                        EmployeeId = x.Id,
                        TotalHours = dayOff.TotalPerYear ?? 0m,
                        DayOffId = dayOff.Id,
                        TotalRemainingHours = dayOff.TotalPerYear ?? 0m,
                        NextRefreshDate = new DateTime((year.Value + 1), 1, 1),
                        TotalCollectedHours = 0,
                    }).ToList();

                await context.DayOffEmployees.AddRangeAsync(newrecords);
            }

            await context.SaveChangesAsync();

            return RedirectToAction("UpdateForEmployees", new { id = cmpId, dayOffId, year });
        }


        [HttpPost]
        public IActionResult Remove(int id)
        {
            if (ModelState.IsValid)
            {
                var add = context.DayOffs.FirstOrDefault(x => x.Id == id);
                if (add == null)
                    return BadRequest("Oooh! we didnt find that one");
                if (context.DayOffEmployees.Any(x => x.DayOffId == id))
                    return BadRequest("Ouch! Some employees have this day of already set, please remove them to proceed");

                context.DayOffs.Remove(add);
                context.SaveChanges();
                return RedirectToAction("Index", "DayOff", new { id = add.CompanyId });
            }

            return BadRequest();
        }


    }
}

