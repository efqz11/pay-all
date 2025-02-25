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

namespace Payroll.Controllers
{
    [EmployeeRoleAuthorize(Roles = new[] { Roles.Company.payroll, Roles.Company.hr_manager, Roles.Company.management })]    
    public class PayAdjustmentController : Controller
    {
        private readonly PayrollDbContext context;
        private readonly PayAdjustmentService payAdjustmentService;
        private readonly AccessGrantService accessGrantService;
        private readonly UserResolverService userResolverService;

        public PayAdjustmentController(PayrollDbContext context, PayAdjustmentService payAdjustmentService, AccessGrantService accessGrantService, UserResolverService userResolverService)
        {
            this.context = context;
            this.payAdjustmentService = payAdjustmentService;
            this.accessGrantService = accessGrantService;
            this.userResolverService = userResolverService;
        }

        public async Task<IActionResult> Index()
        {
            int page = 1;
            var emp = await accessGrantService.GetAllAccessiblePayAdjustmentsAsync(page, int.MaxValue);

                //await context.PayAdjustments
                //.Include(x => x.Fields)
                //.OrderByDescending(x => x.CalculationOrder > 0)
                //.ThenBy(x => x.CalculationOrder)
                //.ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Listing", await GetNewList());

            return View(emp);
        }

        public async Task<IActionResult> SaveFields(List<PayAdjustmentFieldConfig> additionDeductionFields)
        {
            context.PayAdjustmentFieldConfigs.UpdateRange(additionDeductionFields);
            await context.SaveChangesAsync();

            return RedirectToAction("Change");
        }
        
        [HttpPost]
        public IActionResult UpdateOrder(string modelsJson)
        {
            var models = JsonConvert.DeserializeObject<List<PayAdjustmentFieldConfig>>(modelsJson);

            var payADjusmtns = context.PayAdjustments.ToList();
            foreach (var item in payADjusmtns)
            {
                if (models.Any(x => x.Id == item.Id))
                {
                    item.CalculationOrder = models.First(x => x.Id == item.Id).CalculationOrder;
                    context.PayAdjustments.Update(item);
                }
                
            }
            context.SaveChanges();
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> Index(PayAdjustment[] model)
        {
            if (ModelState.IsValid)
            {
                for (int i = 0; i < model.Length; i++)
                    model[i].CompanyId = userResolverService.GetCompanyId();

                context.PayAdjustments.UpdateRange(model);
                context.SaveChanges();
            }

            return PartialView("_Listing", await GetNewList());
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            if (ModelState.IsValid)
            {
                var newPay = new PayAdjustment
                {
                    Name = "New Adjustment",
                    CompanyId = userResolverService.GetCompanyId(),
                };
                context.PayAdjustments.Add(newPay);
                context.SaveChanges();
                ViewBag.IsNewRow = true;
                return PartialView("_Listing", await GetNewList(true, newPay.Id));
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int payAdjustmentId = 0)
        {
            if (ModelState.IsValid)
            {
                var add = context.PayAdjustments.Find(payAdjustmentId);
                if (add == null)
                    return BadRequest("Oooh! we didnt find that one");
                //if(context.PayrollPeriodPayAdjustments.Any(x=> x.PayAdjustmentId == payAdjustmentId))
                //    return BadRequest("Ouch! Some items are used as children, please remove them before proceed");

                context.PayAdjustments.Remove(add);
                context.SaveChanges();
                return PartialView("_Listing", await GetNewList());
            }

            return BadRequest();
        }

        private async Task<List<PayAdjustment>> GetNewList(bool orderByIdFirstRow = false, int id = 0)
        {
            //var list =  await accessGrantService.GetAllAccessiblePayAdjustmentsAsync(1, int.MaxValue);


            var list = context.PayAdjustments
                .Where(a=> a.CompanyId == userResolverService.GetCompanyId())
                .Include(x => x.Fields)
                .OrderByDescending(x => x.CalculationOrder > 0)
                .ThenBy(x => x.CalculationOrder)
                .ToList();
            if (orderByIdFirstRow)
            {
                var first = context.PayAdjustments.Find(id);
                var newList = new List<PayAdjustment> { first };

                newList.AddRange(list.Where(x => x.Id != id));
                return newList;
            }

            return list;
        }
    }
}
