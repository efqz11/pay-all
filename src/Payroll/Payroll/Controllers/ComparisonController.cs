using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Helpers;
using ChartJSCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;

namespace Payroll.Controllers
{
    public class ComparisonController : Controller
    {
        private readonly PayrollDbContext context;
        private readonly UserResolverService userResolverService;
        private readonly CompanyService companyService;

        public ComparisonController(PayrollDbContext context, UserResolverService userResolverService, CompanyService companyService)
        {
            this.context = context;
            this.userResolverService = userResolverService;
            this.companyService = companyService;
        }

        public async Task<IActionResult> Index(int source = 0, int compare = 0, int empId = 0)
        {
            //var sourceDd = new List<PayrollPeriod> { new PayrollPeriod { Id = 0, Name = "Choose Period" } };
            //var payPeriodsInD
            //sourceDd.AddRange(context.Employees.ToList());
            var periods = await context.PayrollPeriods
                .Where(a=> a.CompanyId == userResolverService.GetCompanyId())
                .ToListAsync();
            ViewBag.SourcePeriodId = new SelectList(periods, "Id", "Name", source);
            ViewBag.ComaprePeriodId = new SelectList(periods, "Id", "Name", compare);
            var empdropdown = await companyService.GetEmployeesOfCurremtCompany(true);
            ViewBag.EmpId = new SelectList(empdropdown, "Id", "Name", empId);

            List<ComapareData> comapareDatas = new List<ComapareData>();
            var paySource = context.PayrollPeriodEmployees.Where(x => (empId == 0 || empId == x.EmployeeId) && x.PayrollPeriodId == source).Include(x => x.PayrollPeriod).Include(x => x.VariationKeyValues).FirstOrDefault();
            var payComapre = context.PayrollPeriodEmployees.Where(x => (empId == 0 || empId == x.EmployeeId) && x.PayrollPeriodId == compare).Include(x => x.PayrollPeriod).Include(x => x.VariationKeyValues).FirstOrDefault();

            if (payComapre == null || paySource == null)
                return View(new ComparsonVm());

            ViewBag.ComaprePeriodName = payComapre?.PayrollPeriod?.Name;
            ViewBag.SourcePeriodName = paySource?.PayrollPeriod?.Name;
            ViewBag.Employees = empdropdown;
            ViewBag.PayAdjustments = context.PayAdjustments.Where(x => x.CompanyId == userResolverService.GetCompanyId()).ToDictionary(x=> x.Id, x=> x.Name);
            ViewBag.EmployeeId = empId;
            List< EmployeePayCompare2D> topAndBottomList = null;

            if (empId == 0)
            {
                // type = period (all employees)

                // returns 3 Best and 3 Black listed Employee array
                comapareDatas = GetComparedPayrolPeriodList(source, compare, out topAndBottomList);
                //ViewBag.TopAndBottomList = topAndBottomList;
            }
            else
            {
                // type == Period && employee (1)
                var emp = context.Employees.Find(empId);
                ViewBag.EmployeeName = emp.Name;
                ViewBag.JobTitle = emp.JobTitle;
                //ViewBag.BasicSalary = emp.BasicSalary;
                comapareDatas = GetComparedEmployeeList(paySource, payComapre);
            }
            var comapaisonVm = new ComparsonVm
            {
                IsComparingPeriod = empId == 0,
                ComaprePeriodId = compare,
                SourcePeriodId = source,
                CompareDatas = comapareDatas,

                TopAndBottomList = topAndBottomList
                // ComparePeriodEmplees = new List<PayrollPeriodEmployee> { paySource, payComapre }
            };

            comapaisonVm = AddNetSalaryAndGrossPayComparison(comapaisonVm);

            ViewData["chart"] = GetComapreHorizontalBarGraph(comapaisonVm, paySource.PayrollPeriod.Name, payComapre.PayrollPeriod.Name);
            return View(comapaisonVm);
        }

        private ComparsonVm AddNetSalaryAndGrossPayComparison(ComparsonVm comapaisonVm)
        {
            var grossPay = new ComapareData
            {
                Key = "Gross Pay",
                CurrentValue = comapaisonVm.CompareDatas.Where(x => x.VariationType == VariationType.ConstantAddition || x.VariationType == VariationType.VariableAddition)?.Sum(x => x.CurrentValue) ?? 0m,
                CompareValue = comapaisonVm.CompareDatas.Where(x => x.VariationType == VariationType.ConstantAddition || x.VariationType == VariationType.VariableAddition)?.Sum(x => x.CompareValue) ?? 0m
            };

            var netSalary = new ComapareData
            {
                Key = "Net Salary",
                CurrentValue = grossPay.CurrentValue - comapaisonVm.CompareDatas.Where(x => x.Key != "Gross Pay" && (x.VariationType == VariationType.ConstantDeduction || x.VariationType == VariationType.VariableDeduction))?.Sum(x => x.CurrentValue) ?? 0m,
                CompareValue = grossPay.CompareValue - comapaisonVm.CompareDatas.Where(x => x.Key != "Gross Pay" && (x.VariationType == VariationType.ConstantDeduction || x.VariationType == VariationType.VariableDeduction))?.Sum(x => x.CompareValue) ?? 0m
            };

            //gross pay
            var lsit = new List<ComapareData>() { grossPay, netSalary };

            foreach (var item in lsit)
            {
                if (item.CompareValue > 0)
                {
                    var change = item.CurrentValue - item.CompareValue;
                    var calcChange = change / item.CompareValue;
                    var perc = Math.Round(calcChange * 100, 0);

                    item.Percentage = perc;
                    if (calcChange > 0)
                        item.TrendText = $"<span class='text-success'>{perc}% <i class='ion-arrow-graph-up-right'></i></span>";
                    else
                        item.TrendText = $"<span class='text-danger'>{Math.Abs(perc)}% <i class='ion-arrow-graph-down-right'></i></span>";
                }
            }
            comapaisonVm.GrossPayComaparison = lsit[0];
            comapaisonVm.NetSalaryComaparison = lsit[1];

            return comapaisonVm;
        }

        private List<ComapareData> GetComparedPayrolPeriodList(int sourceId, int comapreId, out List<EmployeePayCompare2D> employeePayCompar)
        {
            employeePayCompar = null;

            var sourcePeriodEmployees = context.PayrollPeriodEmployees.Where(x => x.PayrollPeriodId == sourceId).Include(x => x.PayrollPeriod).Include(x => x.VariationKeyValues).ToList();
            var comaprePeiodEmployees = context.PayrollPeriodEmployees.Where(x => x.PayrollPeriodId == comapreId).Include(x => x.PayrollPeriod).Include(x => x.VariationKeyValues).ToList();

            var sourceHeaders = sourcePeriodEmployees.FirstOrDefault().VariationKeyValues
                .OrderBy(x => x.MultiOrder)
                .Select(x => new { MultiOrder = 0, x.Key, x.KeyId, x.Type });

            var comparePeriodHeaders = comaprePeiodEmployees.FirstOrDefault().VariationKeyValues.OrderBy(x => x.MultiOrder).Select(x => new { MultiOrder = 0, x.Key, x.KeyId, x.Type });
            var keyHeaders = sourceHeaders.Union(comparePeriodHeaders)
                    .Distinct().ToList();
            if (keyHeaders.Count <= 0)
                return new List<ComapareData>();

            //var listed = topListed.Select(x=> new
            //{
            //    empId = x.empId,
            //    Net = comaprePeiodEmployees.FirstOrDefault(a=> a.EmployeeId == x.empId)?.NetSalary,
            //    Basic = 
            //})

            var datas = keyHeaders.Select(x => new ComapareData
            {
                MultiOrder = comparePeriodHeaders.FirstOrDefault(q => q.KeyId == x.KeyId)?.MultiOrder ?? -1, //  x.MultiOrder,
                Key = x.Key,
                KeyId = x.KeyId,
                CurrentValue = sourcePeriodEmployees.SelectMany(t => t.VariationKeyValues.Where(s => s.KeyId == x.KeyId && x.Type == s.Type))?.Sum(z => z.Value) ?? 0m,
                CompareValue = comaprePeiodEmployees.SelectMany(t => t.VariationKeyValues.Where(s => s.KeyId == x.KeyId && x.Type == s.Type))?.Sum(z => z.Value) ?? 0m,


                CurrentValueEmplCount = sourcePeriodEmployees.Count(t => t.VariationKeyValues.Where(s => s.KeyId == x.KeyId && x.Type == s.Type).Sum(a=> a.Value) > 0),
                CompareValueEmplCount = comaprePeiodEmployees.Count(t => t.VariationKeyValues.Where(s => s.KeyId == x.KeyId && x.Type == s.Type).Sum(a=> a.Value) > 0),

                CurrentValueEmployees = sourcePeriodEmployees.Where(t => t.VariationKeyValues.Where(s => s.KeyId == x.KeyId && x.Type == s.Type).Sum(a=> a.Value) > 0).Select(a=> a.EmployeeId).ToArray(),
                CompareValueEmployees = sourcePeriodEmployees.Where(t => t.VariationKeyValues.Where(s => s.KeyId == x.KeyId && x.Type == s.Type).Sum(a=> a.Value) > 0).Select(a=> a.EmployeeId).ToArray(),
                SecondCompareData = GetComparedPayrolPeriodFieldValuesList(sourceId, comapreId, x.KeyId, x.Type),
                VariationType = x.Type,
            }).ToList();


            foreach (var item in datas)
            {
                if(item.CompareValue > 0)
                {
                    var change = item.CurrentValue - item.CompareValue;
                    var calcChange = change / item.CompareValue;
                    var perc = Math.Round(calcChange * 100, 0);

                    item.Percentage = perc;
                    if (calcChange > 0)
                        item.TrendText = $"<span class='text-success'>{perc}% <i class='ion-arrow-graph-up-right'></i></span>";
                    else
                        item.TrendText = $"<span class='text-danger'>{Math.Abs(perc)}% <i class='ion-arrow-graph-down-right'></i></span>";
                }

                // add negative to 
                if(item.VariationType == VariationType.ConstantDeduction || item.VariationType == VariationType.VariableDeduction)
                {
                    item.CurrentValue *= -1;
                    item.CompareValue *=  -1;
                }
            }



            employeePayCompar = new List<EmployeePayCompare2D>();
            // find black listed empls
            // all empls who got less net salary with max deductions
            var topListed = comaprePeiodEmployees.OrderByDescending(x => x.VariationKeyValues.Where(v => v.Type == VariationType.VariableAddition).Sum(w => w.Value))
                                .Take(3).Select(x => new EmployeePayCompare2D
                                {
                                    Id = x.EmployeeId,
                                    NetPay = x.NetSalary,
                                    TopAddAmnt = x.VariationKeyValues.Where(v => v.Type == VariationType.VariableAddition).Sum(w => w.Value),
                                    AddArray = x.VariationKeyValues.Where(v => v.Type == VariationType.VariableAddition).OrderByDescending(w => w.Value).Take(3).ToArray(),
                                    isAdd = true
                                }).ToList();

            var blackListed = comaprePeiodEmployees.OrderByDescending(x => x.VariationKeyValues.Where(v => v.Type == VariationType.VariableDeduction).Sum(w => w.Value))
                                .Take(3).Select(x => new EmployeePayCompare2D
                                {
                                    Id = x.EmployeeId,
                                    NetPay = x.NetSalary,
                                    TopAddAmnt = x.VariationKeyValues.Where(v => v.Type == VariationType.VariableDeduction).Sum(w => w.Value),
                                    AddArray = x.VariationKeyValues.Where(v => v.Type == VariationType.VariableDeduction).OrderByDescending(w => w.Value).Take(3).ToArray(),
                                    isAdd = false
                                }).ToList();

            employeePayCompar.AddRange(topListed);
            employeePayCompar.AddRange(blackListed);


            return datas;
        }

        private List<ComapareData> GetComparedEmployeeList(PayrollPeriodEmployee sourcePayrollPeriodEmployee, PayrollPeriodEmployee comparePayrollPeriodEmployee)
        {
            var sourceHeaders = sourcePayrollPeriodEmployee.VariationKeyValues.OrderBy(x=> x.MultiOrder).Select(x => new { x.MultiOrder, x.Key, x.KeyId, x.Type });
            var keyHeaders = sourceHeaders.Union(comparePayrollPeriodEmployee.VariationKeyValues.OrderBy(x => x.MultiOrder).Select(x => new { x.MultiOrder, x.Key, x.KeyId, x.Type }))
                    .Distinct().ToList();

            var datas = keyHeaders.Select(x => new ComapareData
            {
                Key = x.Key,
                KeyId = x.KeyId,
                MultiOrder = x.MultiOrder,
                CurrentValue = sourcePayrollPeriodEmployee.VariationKeyValues.Where(s => s.KeyId == x.KeyId && x.Type == s.Type)?.Sum(z => z.Value) ?? 0m,
                CompareValue = comparePayrollPeriodEmployee.VariationKeyValues.Where(s => s.KeyId == x.KeyId && x.Type == s.Type)?.Sum(z => z.Value) ?? 0m,
                SecondCompareData = GetComparedPayrolPeriodFieldValuesList(sourcePayrollPeriodEmployee.PayrollPeriodId, comparePayrollPeriodEmployee.PayrollPeriodId, x.KeyId, x.Type, empId: sourcePayrollPeriodEmployee.EmployeeId),
                // SecondCompareData= new List<ComapareData>
                VariationType = x.Type,
            }).ToList();



            var grossPay = 0;
            var netSalayr = 0;
            foreach (var item in datas)
            {
                if (item.CompareValue > 0)
                {
                    var change = item.CurrentValue - item.CompareValue;
                    var calcChange = change / item.CompareValue;
                    var perc = Math.Round(calcChange * 100, 0);

                    item.Percentage = perc;
                    if (calcChange > 0)
                        item.TrendText = $"<span class='text-success'>{perc}% <i class='ion-arrow-graph-up-right'></i></span>";
                    else
                        item.TrendText = $"<span class='text-danger'>{Math.Abs(perc)}% <i class='ion-arrow-graph-down-right'></i></span>";
                }


                if (item.VariationType == VariationType.ConstantDeduction || item.VariationType == VariationType.VariableDeduction)
                {
                    item.CurrentValue *= -1;
                    item.CompareValue *= -1;
                }
            }

            
            return datas;
        }


        private List<ComapareData2> GetComparedPayrolPeriodFieldValuesList(int sourceId, int comapreId, int addId, VariationType adjustmentVariantType, int empId = 0)
        {
            if (adjustmentVariantType == VariationType.ConstantAddition || adjustmentVariantType == VariationType.ConstantDeduction)
                return new List<ComapareData2>();

            var sourcePeriodPayAdjustments = context.PayrollPeriodPayAdjustments.Include(x => x.PayrollPeriodPayAdjustmentFieldValues).Where(x => x.PayrollPeriodId == sourceId && x.PayAdjustmentId == addId && (empId == 0 || empId == x.EmployeeId)).ToList();
            var comaprePeiodPayAdjustments = context.PayrollPeriodPayAdjustments.Include(x => x.PayrollPeriodPayAdjustmentFieldValues).Where(x => x.PayrollPeriodId == comapreId && x.PayAdjustmentId == addId && (empId == 0 || empId == x.EmployeeId)).ToList();
            if (sourcePeriodPayAdjustments == null || comaprePeiodPayAdjustments == null)
                return new List<ComapareData2>();

            var sourcePeriodPayAdjustmentsFieldValues = sourcePeriodPayAdjustments.SelectMany(x => x.PayrollPeriodPayAdjustmentFieldValues);
            var comparePeriodPayAdjustmentsFieldValues = comaprePeiodPayAdjustments.SelectMany(x => x.PayrollPeriodPayAdjustmentFieldValues);

            try
            {
                var sourceHeadersTest = sourcePeriodPayAdjustments.FirstOrDefault().PayrollPeriodPayAdjustmentFieldValues.Select(x => new { x.Key, x.DisplayName }).Distinct().ToList();
                var keyHeadersRest= sourceHeadersTest.Union(comaprePeiodPayAdjustments?.FirstOrDefault().PayrollPeriodPayAdjustmentFieldValues.Select(x => new { x.Key, x.DisplayName }).Distinct().ToList());
                if (keyHeadersRest.Count() <= 0)
                    return new List<ComapareData2>();
            }
            catch (Exception)
            {
                return new List<ComapareData2>();
            }

            var sourceHeaders = sourcePeriodPayAdjustments.FirstOrDefault().PayrollPeriodPayAdjustmentFieldValues.Select(x => new { x.Key, x.DisplayName }).Distinct().ToList();
            var keyHeaders = sourceHeaders.Union(comaprePeiodPayAdjustments?.FirstOrDefault().PayrollPeriodPayAdjustmentFieldValues.Select(x => new { x.Key, x.DisplayName }).Distinct().ToList());
            if (keyHeaders?.Count() <= 0)
                return new List<ComapareData2>();

            var datas = keyHeaders.Select(x => new ComapareData2
            {
                Key = x.Key,
                // KeyId = x.KeyId,
                CurruretValueDecimal = empId == 0 ? sourcePeriodPayAdjustmentsFieldValues
                            .Where(s=> s.Key == x.Key && (s.BaseType == BaseType.Calculated || 
                            (s.BaseType==BaseType.ManualEntry && (s.FieldType == FieldType.Decimal || s.FieldType == FieldType.Number))))
                            .Sum(s=> GetDecimalValue(s.Value)) : 0,
                CompareValueDecimal = empId == 0 ? comparePeriodPayAdjustmentsFieldValues
                            .Where(s => s.Key == x.Key && (s.BaseType == BaseType.Calculated ||
                            (s.BaseType == BaseType.ManualEntry && (s.FieldType == FieldType.Decimal || s.FieldType == FieldType.Number))))
                            .Sum(s => GetDecimalValue(s.Value)) : 0,
                CurruretValue = sourcePeriodPayAdjustmentsFieldValues
                            .FirstOrDefault(s => s.Key == x.Key)?.Value,
                CompareValue = comparePeriodPayAdjustmentsFieldValues
                            .FirstOrDefault(s => s.Key == x.Key)?.Value
                //BaseType = comaprePeiodEmployees.PayrollPeriodPayAdjustmentFieldValues.FirstOrDefault(s => s.Key == x.Key)?.BaseType,
                // VariationType = x.Type,
            }).ToList();

            foreach (var item in datas)
            {
                if(empId == 0)
                {
                    if(item.CurruretValueDecimal >0 && item.CompareValueDecimal > 0)
                    {
                        item.CurruretValue = item.CurruretValueDecimal.ToString();
                        item.CompareValue = item.CompareValueDecimal.ToString();
                    }
                    else
                    {
                        item.CurruretValue = "(multiple values)";
                        item.CompareValue = "(multiple values)";
                    }
                }
                decimal curentVal = -3m;
                decimal compareVal = -3m;
                if (decimal.TryParse(item.CurruretValue, out curentVal) && decimal.TryParse(item.CompareValue, out compareVal))
                {
                    if (compareVal > 0)
                    {
                        try
                        {
                            var change = curentVal - compareVal;
                            var calcChange = change / compareVal;
                            var perc = Math.Round(calcChange * 100, 0);

                            item.Percentage = perc;
                            if (calcChange > 0)
                                item.TrendText = $"<span class='text-success'>{perc}% <i class='ion-arrow-graph-up-right'></i></span>";
                            else
                                item.TrendText = $"<span class='text-danger'>{Math.Abs(perc)}% <i class='ion-arrow-graph-down-right'></i></span>";
                        }
                        catch (DivideByZeroException)
                        {
                        }
                    }
                }
            }



            return datas;
        }

        private decimal GetDecimalValue(string value)
        {
            decimal decValue = 0;

            try
            {
                Decimal.TryParse(value, out decValue);
            }
            catch (Exception)
            {
                
            }
            return decValue;
        }

        public Chart GetComapreHorizontalBarGraph(ComparsonVm comparsonVm, string sourcePeriodName, string comparePeriodName)
        {
            var compareData = comparsonVm.CompareDatas;
            Chart chart = new Chart();

            chart.Type = Enums.ChartType.HorizontalBar;
            
            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = compareData.Select(x => x.Key).ToList();

            chart.Options.Responsive = true;
            chart.Options.MaintainAspectRatio = false;
            
            //{
            //    Data
            //}

            BarDataset dataset = new BarDataset()
            {
                Label = "Current (" + sourcePeriodName + ")",
                Data = compareData.OrderBy(x => x.MultiOrder).Select(x=> double.Parse(x.CurrentValue.ToString())).ToList(),
                Type = Enums.ChartType.HorizontalBar,
                BackgroundColor = new List<ChartColor>
                {
                    ChartColor.FromRgba(255, 99, 132, 0.2),
                },
                BorderColor = new List<ChartColor>
                {
                    ChartColor.FromRgb(255, 99, 132),
                },
                BorderWidth = new List<int>() { 1 }
                //BackgroundColor = ChartColor.FromRgba(75, 192, 192, 0.4),
                //BorderColor = ChartColor.FromRgb(75, 192, 192),
            };

            BarDataset datasetCompare = new BarDataset()
            {
                Label = "Compare (" + comparePeriodName + ")",
                Data = compareData.OrderBy(x => x.MultiOrder).Select(x => double.Parse(x.CompareValue.ToString())).ToList(),
                Type = Enums.ChartType.HorizontalBar,
                BackgroundColor = new List<ChartColor>
                {
                    ChartColor.FromRgba(75, 192, 192, 0.2),
                },
                BorderColor = new List<ChartColor>
                {
                    ChartColor.FromRgb(75, 192, 192),
                },
                BorderWidth = new List<int>() { 1 }
            };

            data.Datasets = new List<Dataset>();
            data.Datasets.Add(dataset);
            data.Datasets.Add(datasetCompare);

            chart.Data = data;

            return chart;
        }

        //public IActionResult emp(int payrolId, int addId)
        //{
        //    var additions = context.PayrollPeriodAdditions
        //        .Where(x => x.AdditionId == addId && x.PayrollPeriodId == payrolId)
        //        .OrderBy(x=> x.EmployeeName)
        //        .Include(x=> x.PayrollPeriod)
        //        .Include(x => x.OriginalAddition)
        //        .ToList();

        //    ViewBag.ItemId = new SelectList(context.Additions.ToList(), "Id", "Name", addId);
        //    ViewBag.PayrolPeriodId = new SelectList(context.PayrollPeriods.ToList(), "Id", "Name", payrolId);
        //    ViewBag.Employees = context.Employees.ToList();

        //    return View(new PayItemVm
        //    {
        //        ItemId = addId,
        //        PayrolPeriodId = payrolId,
        //        PayrollPeriodAdditions = additions
        //    });
        //}

        [HttpPost]
        public IActionResult Index(PayAdjustment[] model)
        {
            if (ModelState.IsValid)
            {
                //add.ForEach(t =>
                //{
                //    t.PayrollPeriodId = model.PayrolPeriodId;
                //    t.AdditionId = model.ItemId;
                //    // t.EmployeeName = model.ItemName;
                //    // t.CalculationOrder = model.CalculationOrder
                //});

                context.PayAdjustments.UpdateRange(model);
                context.SaveChanges();
            }

            return PartialView("_Listing", GetNewList());
        }

        [HttpPost]
        public IActionResult Create()
        {
            if (ModelState.IsValid)
            {
                context.PayAdjustments.Add(new PayAdjustment
                {
                    Name = "New Adjustment"
                });
                context.SaveChanges();
                return PartialView("_Listing", GetNewList());
            }

            return BadRequest();
        }

        [HttpPost]
        public IActionResult Remove(int itemId = 0)
        {
            if (ModelState.IsValid)
            {
                var add = context.PayAdjustments.Find(itemId);
                if (add == null)
                    return BadRequest("Oooh! we didnt find that one");
                if(context.PayrollPeriodPayAdjustments.Any(x=> x.PayAdjustmentId == itemId))
                    return BadRequest("Ouch! Some items are used as children, please remove them before proceed");

                context.PayAdjustments.Remove(add);
                context.SaveChanges();
                return PartialView("_Listing", GetNewList());
            }

            return BadRequest();
        }

        private List<PayAdjustment> GetNewList()
        {
            return context.PayAdjustments
                .ToList();
        }
        
    }

}
