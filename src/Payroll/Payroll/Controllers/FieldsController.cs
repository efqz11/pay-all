using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;

namespace Payroll.Controllers
{
    public class FieldsController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly PayAdjustmentService payAdjustmentService;

        public FieldsController(PayrollDbContext context, PayAdjustmentService payAdjustmentService)
        {
            this.context = context;
            this.payAdjustmentService = payAdjustmentService;
        }


        public async Task<IActionResult> AddOrUpdate(int addId, VariationType type, int id = 0)
        {
            var item = await context.PayAdjustmentFieldConfigs
                .Where(x => x.Id == id && x.PayAdjustmentId == addId).FirstOrDefaultAsync();
            // if (item == null) return BadRequest();

            if (id == 0)
                item = new PayAdjustmentFieldConfig
                {
                    DisplayName = "",
                    ListSelect = "",
                    PayAdjustmentId = addId,
                    CalculationOrder = (context.PayAdjustmentFieldConfigs
                    .Where(x => x.PayAdjustmentId == addId)
                    .OrderByDescending(x => x.CalculationOrder)
                    .FirstOrDefault()?.CalculationOrder ?? 0) + 1
                };

            var empFields = typeof(Employee).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy).Select(x => x.Name);
            ViewData["EmployeeFieldNames"] = new SelectList(typeof(Employee).GetProperties()
                            .Where(x => Attribute.IsDefined(x, typeof(Filters.SelectableFieldAttribute)))
                            .Select(x => x.Name), item.ListSelect);
            ViewData["DepartmentFieldNames"] = new SelectList(typeof(Department).GetProperties()
                            .Where(x => Attribute.IsDefined(x, typeof(Filters.SelectableFieldAttribute)))
                            .Select(x => x.Name), item.ListSelect);
            ViewData["WorkId"] = new SelectList(await context.Works.ToListAsync(), "Id", "Name", item.WorkId);
            //ViewData["AggregatedEmployeeInteractions"] = new SelectList(typeof(EmployeeInteractionAgg).GetProperties()
            //                .Select(x => x.Name), item.ListSelect);
            ViewData["ItemName"] = context.PayAdjustments.Find(item.PayAdjustmentId).Name;
            ViewData["CalculatableFields"] = payAdjustmentService.GetCalculatableFieldNames(addId);
            return PartialView("_AddOrUpdate", item);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddOrUpdate(PayAdjustmentFieldConfig model)
        {
            if(ModelState.IsValid)
            {
                if(string.IsNullOrWhiteSpace(model.DisplayName))
                    return ThrowJsonError("Display name is required");
                // if(model.AdditionId )
            }
            if (ModelState.IsValid)
            {
                // validate

                var result = payAdjustmentService.ValidateField(model);
                
                if (result.Succeeded == false)
                    return BadRequest(result.Errors.FirstOrDefault());

                model.CalculationIdentifier = model.DisplayName.GenerateSlug();
                model = payAdjustmentService.SetEvaluationFields(model);
                model.Calculation = payAdjustmentService.GetCalculation();

                if (model.Id > 0)
                {
                    context.PayAdjustmentFieldConfigs.Update(model);
                }
                else
                {
                    context.PayAdjustmentFieldConfigs.Add(model);
                }

                await context.SaveChangesAsync();

                return RedirectToAction("Index", "PayAdjustment");
            }

            return BadRequest(ModelState);
        }


        public IActionResult PayAdjustmentCalculationFields(int addId, int payrolId)
        {
            var items = context.PayrollPeriodPayAdjustments
                .Where(x => x.PayrollPeriodId == payrolId && x.PayAdjustmentId == addId).ToList();
            if (items == null || items?.Count <= 0) return BadRequest();

            ViewData["ItemName"] = context.PayAdjustments.Find(addId)?.Name;
            return PartialView("_PayAdjustmentCalculationFields", items);
        }

        public async Task<IActionResult> ViewSampleTable(int addId)
        {
            var fields = context.PayAdjustmentFieldConfigs.Where(x => x.PayAdjustmentId == addId).ToList();
            var masterAddition = context.PayAdjustments.Find(addId);

            var keyValues = await payAdjustmentService.GeneratePayAdjustmentsAndFieldValues(fields, masterAddition, isSample: true);
            return PartialView("_SampleData", keyValues);
        }


        public async Task<IActionResult> Validate(int payAdjustmentId)
        {
            var payAdjustment = await context.PayAdjustments.Include(x => x.Fields).FirstOrDefaultAsync(x => x.Id == payAdjustmentId);

            var validationResult = payAdjustmentService.Validate(payAdjustment);
            if (validationResult.Succeeded)
                return Json(new { Result = true, Message = "<i class='ion-checkmark-circled'></i> passing",
                    MessageText = $"Great! {payAdjustment.Name} can be calculated" });

            return Json(new { Result = false, Message = $"<i class='ion-alert-circled' role='tooltip' aria-label=" + validationResult.Errors.FirstOrDefault()?.Description + "></i> failing" , messageText = validationResult.Errors.FirstOrDefault()?.Description });
        }

        public async Task<IActionResult> SaveFields(List<PayAdjustmentFieldConfig> additionDeductionFields)
        {
            context.PayAdjustmentFieldConfigs.UpdateRange(additionDeductionFields);
            await context.SaveChangesAsync();

            return RedirectToAction("Change");
        }


        [HttpPost]
        public IActionResult Remove(int id, int payAdjustmentId)
        {
            if (ModelState.IsValid)
            {
                var add = context.PayAdjustmentFieldConfigs.FirstOrDefault(x=> x.Id == id && x.PayAdjustmentId == payAdjustmentId);
                if (add == null)
                    return BadRequest("Oooh! we didnt find that one");
                //if(context.PayrollPeriodPayAdjustments.Any(x=> x.PayAdjustmentId == payAdjustmentId))
                //    return BadRequest("Ouch! Some items are used as children, please remove them before proceed");

                context.PayAdjustmentFieldConfigs.Remove(add);
                context.SaveChanges();
                return RedirectToAction("Index", "PayAdjustment");
            }

            return BadRequest();
        }



        private List<PayAdjustment> GetNewList()
        {
            return context.PayAdjustments
                .Include(x => x.Fields)
                .OrderByDescending(x => x.CalculationOrder > 0)
                .ThenBy(x => x.CalculationOrder)
                .ToList();
        }
    }
}
