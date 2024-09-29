using Microsoft.AspNetCore.Identity;
using Payroll.Database;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Payroll.ViewModels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Payroll.Services
{
    public class PayAdjustmentService
    {
        private string computedstring;
        private readonly PayrollDbContext context;
        private readonly UserResolverService userResolverService;
        private readonly EmployeeService employeeService;
        private readonly PayrollService payrollService;

        //private const List<String> CALCULATED_FIELD_PREXES = new List<String> { CALCULATED_FIELD_PREFIX_EMPLOYEE, CALCULATED_FIELD_PREFIX_PAYFIELD };

        private const string CALCULATED_FIELD_PREFIX_PAYFIELD = "field.";
        private const string CALCULATED_FIELD_PREFIX_CHART = "chart.";
        private const string CALCULATED_FIELD_PREFIX_EMPLOYEE = "employee.";

        //private const string CALCULATED_FIELD_PREFIX_EMPLOYEE = "{employee.";
        public PayAdjustmentService(PayrollDbContext context, UserResolverService userResolverService, EmployeeService employeeService, PayrollService payrollService)
        {
            this.context = context;
            this.userResolverService = userResolverService;
            this.employeeService = employeeService;
            this.payrollService = payrollService;
        }

        /// <summary>
        /// CALL AFTER VALIDATE()
        /// </summary>
        /// <returns></returns>
        public string GetCalculation() => computedstring;

        /// <summary>
        /// Privodes list of calculatable values
        /// Validation is run againt these values
        /// for ex:  {employee.BasicSalary}
        /// </summary>
        /// <param name="payAdjustmentId"></param>
        /// <returns></returns>
        public List<FieldCalculationResultVm> GetCalculatableFieldNames(int payAdjustmentId = 0)
        {
            var dict = context.PayAdjustments.Where(x => x.CompanyId == userResolverService.GetCompanyId())
                .Where(MyExpressions.IsConstantPayAdjustment)
                .Select(x => new FieldCalculationResultVm
            {
                //ListType = ListType.Employee,
                FieldName = "{" + CALCULATED_FIELD_PREFIX_EMPLOYEE + x.Name + "}",
                FieldType = "Employee"
            }).ToList();

            dict.AddRange(typeof(EmployeeInteractionAgg).GetProperties()
                            .Select(x => new FieldCalculationResultVm
                            {
                                //ListType= ListType.Field,
                                FieldName = "{" + CALCULATED_FIELD_PREFIX_CHART + x.Name + "}",
                                FieldType = "Chart"
                            }).ToList());

            dict.AddRange(
            context.PayAdjustmentFieldConfigs.Where(x => x.PayAdjustmentId == payAdjustmentId)
                .Select(x => new FieldCalculationResultVm
                {
                    //ListType= ListType.Field,
                    FieldName = "{" + CALCULATED_FIELD_PREFIX_PAYFIELD+ x.CalculationIdentifier + "}",
                    FieldType = "Field"
                }).ToList());


            //dict.Add(new FieldCalculationResultVm(ListType.Employee, "{employee.BasicSalary}"));

            return dict;
        }


        public PayAdjustmentFieldConfig SetEvaluationFields(PayAdjustmentFieldConfig item)
        {
            var formatValue = "";
            string type = "server";
            item.IsServerCalculatable = true;
            item.IsClientCalculatable = false;
            item.IsAggregated = false;
            if (item.BaseType == BaseType.ManualEntry && (item.FieldType == FieldType.Number || item.FieldType == FieldType.Decimal))
                type = "client";

            if (item.BaseType == BaseType.Calculated)
            {
                var formula = item.Calculation;
                var prefix = "";

                do
                {
                    if (!formula.StartsWith("{"))
                    {
                        if (formula.IndexOf("{") >= 0)
                            prefix = formula.Substring(0, formula.IndexOf("{"));
                        else
                            prefix = formula;
                        formatValue += prefix;
                    }

                    if (formula.IndexOf("{") >= 0)
                    {
                        var cutoff = (formula.IndexOf("{") > 0) ? formula.IndexOf("{") - 1 : 0;
                        var firstInsider = formula
                                .Substring(formula.IndexOf("{"), formula.IndexOf("}") - cutoff).Replace("}", "").Replace("{", "");
                        if (firstInsider.StartsWith(CALCULATED_FIELD_PREFIX_EMPLOYEE))
                        {
                            item.IsServerCalculatable = true;
                        }
                        else if (firstInsider.StartsWith(CALCULATED_FIELD_PREFIX_CHART))
                        {
                            item.IsServerCalculatable = true;
                            item.IsAggregated = true;
                            if (!typeof(TaskAggByWorkId).GetProperties().Any(x => "chart." + x.Name == firstInsider))
                                item.WorkId = (int?)null;
                        }
                        else if (firstInsider.StartsWith(CALCULATED_FIELD_PREFIX_PAYFIELD))
                        {
                            var calculationIdntifier = firstInsider.Replace(CALCULATED_FIELD_PREFIX_PAYFIELD, "");
                            var sourceField = context.PayAdjustmentFieldConfigs.FirstOrDefault(a => a.PayAdjustmentId == item.PayAdjustmentId && a.CalculationIdentifier == calculationIdntifier);
                            if (sourceField != null)
                            {
                                if (sourceField.BaseType == BaseType.ManualEntry && (sourceField.FieldType == FieldType.Decimal || sourceField.FieldType == FieldType.Number))
                                {
                                    type = "client";
                                    sourceField.UpdateInputClass = item.CalculationIdentifier;
                                    sourceField.EvalMethod = item.Calculation;
                                    item.IsClientCalculatable = true;
                                    context.SaveChanges();
                                }
                                //else
                                //{

                                //}
                            }
                        }
                        formula = formula.Remove(0, (prefix + "{" + firstInsider + "}").Count());
                        // formula = formula.Replace(prefix + "{" + firstInsider + "}", string.Empty);
                    }
                    else
                    {
                        formula = formula.Replace(prefix, string.Empty);
                    }
                }
                while (!string.IsNullOrWhiteSpace(formula));

                if(type == "client")
                {
                    item.IsClientCalculatable = true;
                    item.IsServerCalculatable = false;
                }

            }
            return item;
        }


        /// <summary>
        /// Validate field config, Calcualated fields will be validated 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IdentityResult ValidateField(PayAdjustmentFieldConfig model)
        {
            computedstring = GetComputedString(model);
            switch (model.BaseType)
            {
                case BaseType.ComputedList:
                    var _listType = model.ListType;
                    var _selectField = model.ListSelect;

                    // validate computed list types
                    bool isSelectFieldValidated = ValidateComputedListType(model);
                    
                    if(isSelectFieldValidated == true)
                        return IdentityResult.Success;

                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "121",
                        Description = $"Error in field '{model.DisplayName}', Please enter valid list field for model "+ model.ListType.ToString()
                    });
                case BaseType.Calculated:
                    var formula = model.Calculation;

                    /// {field.no-of-articles}*15
                    /// {employee.BasicSalary}/2
                    /// 3*{employee.BasicSalary}
                    var prefix = "";
                    string formatValue = "";
                    decimal valueDecimal;

                    do
                    {
                        if (!formula.StartsWith("{"))
                        {
                            if (formula.IndexOf("{") >= 0)
                                prefix = formula.Substring(0, formula.IndexOf("{"));
                            else
                                prefix = formula;
                            formatValue += prefix;
                        }

                        if(formula.IndexOf("{") >= 0)
                        {
                            var cutoff = (formula.IndexOf("{") > 0) ? formula.IndexOf("{") - 1 : 0;
                            var firstInsider = formula
                                    .Substring(formula.IndexOf("{"), formula.IndexOf("}") - cutoff).Replace("}", "").Replace("{", "");
                            if (firstInsider.StartsWith(CALCULATED_FIELD_PREFIX_EMPLOYEE))
                            {
                                var empFieldName = firstInsider.Replace(CALCULATED_FIELD_PREFIX_EMPLOYEE, "");
                                var fields = GetCalculatableFieldNames();
                                if (fields.Any(x => x.FieldName == "{" + firstInsider + "}"))
                                {
                                    var empField = typeof(Employee).GetProperties()
                                    .Select(x => x.Name).FirstOrDefault(x => x == empFieldName);
                                    if (empField != null)
                                    {
                                        var emp = new Employee();
                                        var empFieldValue = emp.GetType().GetProperty(empField).GetValue(emp, null)?.ToString();
                                        formatValue += empFieldValue;
                                    }
                                    else { formatValue += 1; } // apya djustment will be evaludated later
                                }
                                else
                                    return IdentityResult.Failed(new IdentityError
                                    {
                                        Code = "505",
                                        Description = $"Employee field {empFieldName} was not found"
                                    });
                            }
                            else if (firstInsider.StartsWith(CALCULATED_FIELD_PREFIX_CHART))
                            {
                                var empFieldName = firstInsider.Replace(CALCULATED_FIELD_PREFIX_CHART, "");
                                var fields = GetCalculatableFieldNames();
                                if (fields.Any(x => x.FieldName == "{" + firstInsider + "}"))
                                {
                                    var aggField = typeof(EmployeeInteractionAgg).GetProperties()
                                    .Select(x => x.Name).FirstOrDefault(x => x == empFieldName);
                                    if (aggField != null)
                                    {
                                        if (typeof(TaskAggByWorkId).GetProperties()
                                    .Select(x => x.Name).Any(x => x == empFieldName))
                                        {
                                            // contains name ig workID Agg list
                                            if (model.WorkId.HasValue && model.WorkId > 0)
                                            {
                                                var aggWorkId = new TaskAggByWorkId();
                                                var empFieldAggValue = aggWorkId.GetType()
                                                    .GetProperty(aggField)
                                                    .GetValue(aggWorkId, null)?.ToString();
                                                formatValue += empFieldAggValue;
                                            }
                                            else
                                                return IdentityResult.Failed(new IdentityError
                                                {
                                                    Code = "701",
                                                    Description = $"Chart field {empFieldName} requires valid Work or Task"
                                                });
                                        }
                                        else
                                        {
                                            var emp = new EmployeeInteractionAgg();
                                            var empFieldValue = emp.GetType().GetProperty(aggField).GetValue(emp, null)?.ToString();
                                            formatValue += empFieldValue;
                                        }
                                    }
                                    else
                                        return IdentityResult.Failed(new IdentityError
                                        {
                                            Code = "702",
                                            Description = $"Chart field {empFieldName} could not be evaluated"
                                        });
                                }
                                else
                                    return IdentityResult.Failed(new IdentityError
                                    {
                                        Code = "605",
                                        Description = $"Chart field {empFieldName} was not found"
                                    });
                            }
                            else if (firstInsider.StartsWith(CALCULATED_FIELD_PREFIX_PAYFIELD))
                            {
                                var field = firstInsider.Replace(CALCULATED_FIELD_PREFIX_PAYFIELD, "");
                                if (context.PayAdjustmentFieldConfigs.Any(x => x.PayAdjustmentId == model.PayAdjustmentId && x.CalculationIdentifier == field))
                                {
                                    // will be resolved later (so adding dummy value = 1)
                                    formatValue += "1";
                                }
                                else
                                    return IdentityResult.Failed(new IdentityError
                                    {
                                        Code = "501",
                                        Description = $"Field calculation identifier {field} was not found"
                                    });
                            }
                            else if (Decimal.TryParse(firstInsider, out valueDecimal))
                            {
                                formatValue += firstInsider;
                            }
                            else
                            {
                                return IdentityResult.Failed(new IdentityError
                                {
                                    Code = "509",
                                    Description = "Calculation is invalid, please check field names and field calculation identifier"
                                });
                            }

                            formula = formula.Remove(0, (prefix + "{" + firstInsider + "}").Count());
                                // .Replace(prefix + "{" + firstInsider + "}", string.Empty);
                        }
                        else
                        {
                            formula = formula.Replace(prefix, string.Empty);
                        }
                    }
                    while (!string.IsNullOrWhiteSpace(formula));

                    try
                    {
                        new DataTable().Compute(formatValue, "").ToString();
                    }
                    catch (Exception)
                    {
                        return IdentityResult.Failed(new IdentityError
                        {
                            Code = "519",
                            Description = "Formula could not be evaluated"
                        });
                    }
                    return IdentityResult.Success;
                case BaseType.ManualEntry:
                    return IdentityResult.Success;
                default:
                    break;
            }

            return new IdentityResult();
        }

        public string EvaluateEmployeeFieldValues(PayAdjustmentFieldConfig model, Employee emp, List<PayrollPeriodPayAdjustmentFieldValue> currentRow, EmployeeInteractionAgg agg)
        {
            switch (model.BaseType)
            {
                case BaseType.ComputedList:
                    //var _listType = model.ListType;
                    //var _selectField = model.ListSelect;

                    //// validate computed list types
                    //bool isSelectFieldValidated = ValidateComputedListType(model);

                    //if (isSelectFieldValidated == true)
                    //    return IdentityResult.Success;

                    //return IdentityResult.Failed(new IdentityError
                    //{
                    //    Code = "121",
                    //    Description = "Please enter valid list field for model " + model.ListType.ToString()
                    //});
                case BaseType.Calculated:
                    var formula = model.Calculation;

                    /// {field.no-of-articles}*15
                    /// {employee.BasicSalary}/2
                    /// 3*{employee.BasicSalary}
                    var prefix = "";
                    string formatValue = "";
                    decimal valueDecimal;


                    do
                    {
                        if (!formula.StartsWith("{"))
                        {
                            if (formula.IndexOf("{") >= 0)
                                prefix = formula.Substring(0, formula.IndexOf("{"));
                            else
                                prefix = formula;
                            formatValue += prefix;
                        }

                        if (formula.IndexOf("{") >= 0)
                        {
                            var cutoff = (formula.IndexOf("{") > 0) ? formula.IndexOf("{") - 1 : 0;
                            var firstInsider = formula
                                    .Substring(formula.IndexOf("{"), formula.IndexOf("}") - cutoff).Replace("}", "").Replace("{", "");
                            if (firstInsider.StartsWith(CALCULATED_FIELD_PREFIX_EMPLOYEE))
                            {
                                var empFieldName = firstInsider.Replace(CALCULATED_FIELD_PREFIX_EMPLOYEE, "");
                                var fields = GetCalculatableFieldNames();
                                if (fields.Any(x => x.FieldName == "{" + firstInsider + "}"))
                                {
                                    if (emp.EmployeePayComponents.Any(x => x.PayAdjustment?.Name == empFieldName))
                                    {
                                        formatValue += (emp.EmployeePayComponents?.FirstOrDefault(x => x.PayAdjustment?.Name == empFieldName)?.Total ?? 0).ToString();
                                    }
                                    else
                                    {
                                        var empField = typeof(Employee).GetProperties()
                                            .Select(x => x.Name).FirstOrDefault(x => x == empFieldName);
                                        formatValue += emp.GetType().GetProperty(empField).GetValue(emp, null)?.ToString();
                                    }
                                }
                            }
                            else if (firstInsider.StartsWith(CALCULATED_FIELD_PREFIX_CHART))
                            {
                                var empFieldName = firstInsider.Replace(CALCULATED_FIELD_PREFIX_CHART, "");
                                var fields = GetCalculatableFieldNames();
                                if (fields.Any(x => x.FieldName == "{" + firstInsider + "}"))
                                {
                                    var empField = typeof(EmployeeInteractionAgg).GetProperties()
                                        .Select(x => x.Name).FirstOrDefault(x => x == empFieldName);

                                    if (agg == null)
                                        formatValue += 0;
                                    else
                                    {
                                        if (model.WorkId.HasValue && model.WorkId.Value > 0)
                                        {
                                            var first = agg.TaskAggByWorkIds.FirstOrDefault(x => x.WorkId == model.WorkId.Value);
                                            if (first != null)
                                                formatValue = first.GetType().GetProperty(empField).GetValue(first, null)?.ToString();
                                        }
                                        else
                                            formatValue += agg.GetType().GetProperty(empField).GetValue(agg, null)?.ToString() ?? "0";

                                    }
                                }
                            }
                            else if (firstInsider.StartsWith(CALCULATED_FIELD_PREFIX_PAYFIELD))
                            {
                                var field = firstInsider.Replace(CALCULATED_FIELD_PREFIX_PAYFIELD, "");
                                if (context.PayAdjustmentFieldConfigs.Any(x => x.PayAdjustmentId == model.PayAdjustmentId && x.CalculationIdentifier == field))
                                {
                                    var item = currentRow.FirstOrDefault(x => x.CalculationIdentifier == field);
                                    if(item != null)
                                    {
                                        decimal payFieldValue = 0;
                                        decimal.TryParse(item.Value, out payFieldValue);
                                        // will be resolved later (so adding dummy value = 1)
                                        formatValue += payFieldValue;
                                    }
                                }
                            }
                            else if (Decimal.TryParse(firstInsider, out valueDecimal))
                            {
                                formatValue += firstInsider;
                            }

                            formula = formula.Remove(0, (prefix + "{" + firstInsider + "}").Count());
                            // formula = formula.Replace(prefix + "{" + firstInsider + "}", string.Empty);
                        }
                        else
                        {
                            formula = formula.Replace(prefix, string.Empty);
                        }
                    }
                    while (!string.IsNullOrWhiteSpace(formula));

                    try
                    {
                        formatValue = new DataTable().Compute(formatValue, "").ToString();
                    }
                    catch (Exception)
                    {
                        throw new Exception("Formula could not be evaluated");
                    }
                    

                    return formatValue;
                case BaseType.ManualEntry:
                    return "";
                default:
                    break;
            }

            return "";
        }

        private bool ValidateComputedListType(PayAdjustmentFieldConfig model)
        {
            switch (model.ListType)
            {
                case ListType.None:
                    break;
                case ListType.Employee:
                    return typeof(Employee).GetProperties()
                            .Where(x=> Attribute.IsDefined(x, typeof(Filters.SelectableFieldAttribute)))
                            .Select(x => x.Name).ToList()
                            .Any(x => x == model.ListSelect);

                case ListType.Department:
                    return typeof(Department).GetProperties()
                            .Where(x => Attribute.IsDefined(x, typeof(Filters.SelectableFieldAttribute)))
                            .Select(x => x.Name).ToList()
                            .Any(x => x == model.ListSelect);
                //case ListType.Aggregated:
                //    return typeof(EmployeeInteractionAgg).GetProperties()
                //            .Select(x => x.Name).ToList()
                //            .Any(x => x == model.ListSelect);
                //case ListType.Additions:
                //    break;
                //case ListType.Deductions:
                //    break;
                default:
                    break;
            }
            return false;
        }


        /// <summary>
        /// Validate field config, Calcualated fields will be validated 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IdentityResult Validate(PayAdjustment payAdjustment)
        {
            if(payAdjustment.Fields == null || payAdjustment.Fields.Count <= 0)
                return IdentityResult.Success;

            foreach (var item in payAdjustment.Fields)
            {
                var res = ValidateField(item);
                if (res.Succeeded == false)
                    return IdentityResult.Failed(res.Errors.ToArray());
            }

            if(payAdjustment.Fields.Count(x=> x.IsReturn) == 1)
            return IdentityResult.Success;

            return IdentityResult.Failed(new[] { new IdentityError { Code = "101", Description = "Please set one field as return value" } });
        }


        private string GetComputedString(PayAdjustmentFieldConfig model)
        {
            var str = "";

            str = GetKeyValuePair(nameof(model.BaseType), model.BaseType.ToString());
            switch (model.BaseType)
            {
                case BaseType.ComputedList:
                    switch (model.ListType)
                    {
                        case ListType.None:
                            break;
                        case ListType.Employee:
                            str = GetKeyValuePair(nameof(model.ListType), model.ListType.ToString());
                            
                            str += GetKeyValuePair(nameof(model.ListSelect), model.ListSelect);
                            break;
                        //case ListType.Additions:
                        //    break;
                        //case ListType.Deductions:
                        //    break;
                        default:
                            break;
                    }
                    break;
                case BaseType.Calculated:
                    str = model.Calculation;
                    break;
                case BaseType.ManualEntry:
                    str = GetKeyValuePair(nameof(model.FieldType), model.FieldType.ToString());
                    break;
                default:
                    break;
            }

            return str;
        }

        /// <summary>
        /// Generate Pay Adjustments and field values
        /// for each Employee
        /// </summary>
        /// <param name="payAdjustmentId"></param>
        /// <param name="fieldConfigs"></param>
        /// <param name="masterAddition"></param>
        /// <returns></returns>
        public async Task<List<PayrollPeriodPayAdjustment>> GeneratePayAdjustmentsAndFieldValues(List<PayAdjustmentFieldConfig> fieldConfigs, PayAdjustment masterAddition, int payrolId = 0, bool isSample = true)
        {
            List<Employee> employees = null;
            List<EmployeeInteractionAgg> aggList = null;

            if (isSample)
            {
                employees = await employeeService.GetAllEmployeesInMyCompanyForPayroll(isSample, payAdjustmentId: masterAddition.Id);

                aggList = await payrollService.GetEmployeePayPeriodInteractionsAsync(payrolId, limit: employees.Count());

            }
            else
            {
                var ppDates = await context.PayrollPeriods.Where(a => a.CompanyId == userResolverService.GetCompanyId() && a.Id == payrolId)
                    .Select(a => new { a.StartDate, a.EndDate }).FirstOrDefaultAsync();

                employees = await employeeService.GetAllEmployeesInMyCompanyForPayroll(isSample, ppDates.StartDate, ppDates.EndDate, payAdjustmentId: masterAddition.Id);

                aggList = await payrollService.GetEmployeePayPeriodInteractionsAsync(payrolId, limit: employees.Count());

            }


            var payrolPeriodAdjustment = new List<PayrollPeriodPayAdjustment>();
            foreach (var emp in employees)
            {
                var _adjsutment = new PayrollPeriodPayAdjustment
                {
                    PayAdjustmentId = masterAddition.Id,
                    EmployeeId = emp.Id,
                    EmployeeName = emp.GetSystemName(userResolverService.GetClaimsPrincipal()),
                    Adjustment = masterAddition.Name,
                    CalculationOrder = masterAddition.CalculationOrder,
                    VariationType = masterAddition.VariationType,
                    PayrollPeriodId = payrolId,
                    // Total = masterAddition.VariationType == VariationType.Constant ? emp.EmployeePayAdjustments.FirstOrDefault(x => x.PayAdjustmentId == masterAddition.Id)?.Total ?? 0 : 0
                };


                // Get field values (rows and columns)
                var columnValues = GetFieldValues(fieldConfigs, emp, masterAddition, aggList.FirstOrDefault(x => x.EmployeeId == emp.Id));
                _adjsutment.PayrollPeriodPayAdjustmentFieldValues.AddRange(columnValues);

                payrolPeriodAdjustment.Add(_adjsutment);
            }


            payrolPeriodAdjustment.ForEach(adj =>
            {
                var deci = 0m;
                try
                {
                    adj.Total = decimal.Parse(adj.PayrollPeriodPayAdjustmentFieldValues.FirstOrDefault(x => x.IsReturn)?.Value);
                }
                catch (Exception)
                {
                    adj.Total = deci;
                }
            });

            return payrolPeriodAdjustment;
        }



        public List<PayrollPeriodPayAdjustmentFieldValue> GetFieldValues(List<PayAdjustmentFieldConfig> fieldConfigs, Employee emp, PayAdjustment payAdjustment, EmployeeInteractionAgg agg)
        {
            var list = new List<PayrollPeriodPayAdjustmentFieldValue>();

            if(payAdjustment.VariationType == VariationType.ConstantAddition || payAdjustment.VariationType == VariationType.ConstantDeduction)
            {
                list.Add(new PayrollPeriodPayAdjustmentFieldValue
                {
                    Key = "Name",
                    VariationType = payAdjustment.VariationType,
                    BaseType = BaseType.ComputedList,
                    ListType = ListType.Employee,
                    ListSelect = emp.GetSystemName(userResolverService.GetClaimsPrincipal()),
                    DisplayName = "Name",
                    Value = emp.GetSystemName(userResolverService.GetClaimsPrincipal()),
                    CalculationOrder = 1
                });

                list.Add(new PayrollPeriodPayAdjustmentFieldValue
                {
                    Key = payAdjustment.Name,
                    VariationType = payAdjustment.VariationType,
                    BaseType = BaseType.Calculated,
                    IsReturn = true,
                    DisplayName = payAdjustment.Name,
                    CalculationOrder = 2,
                    Value = (emp.EmployeePayComponents.FirstOrDefault(x => x.IsActive && x.PayAdjustmentId == payAdjustment.Id)?.Total ?? 0.00m).ToString()
                });
            }
            else
            {
                foreach (var column in fieldConfigs.OrderBy(x=> x.CalculationOrder))
                {
                    var columnValues = new PayrollPeriodPayAdjustmentFieldValue
                    {
                        Key = column.DisplayName,
                        VariationType = payAdjustment.VariationType,
                        BaseType = column.BaseType,
                        FieldType = column.FieldType,
                        ListType = column.ListType,
                        ListFilter = column.ListFilter,
                        ListSelect = column.ListSelect,
                        IsAggregated = column.IsAggregated,
                        IsEditable = column.IsEditable,

                        DisplayName = column.DisplayName,
                        Calculation = column.Calculation,
                        CalculationOrder = column.CalculationOrder,
                        CalculationIdentifier = column.CalculationIdentifier,
                        IsClientCalculatable = column.IsClientCalculatable,
                        OnBlur = column.OnBlur,
                        UpdateInputClass = column.UpdateInputClass,
                        EvalMethod = column.EvalMethod,
                        IsReturn = column.IsReturn,
                        DisplayedValueFrontEnd = column.DisplayedValueFrontEnd
                    };
                    switch (column.BaseType)
                    {
                        case BaseType.ComputedList:
                            switch (column.ListType)
                            {
                                case ListType.Employee:
                                    columnValues.ListSelect = emp.GetType().GetProperty(column.ListSelect).GetValue(emp, null)?.ToString();
                                    break;
                                case ListType.Department:
                                    columnValues.ListSelect = emp.Department.GetType().GetProperty(column.ListSelect).GetValue(emp.Department, null)?.ToString();
                                    break;
                                //case ListType.Aggregated:
                                //    if(agg != null)
                                //    {
                                //        columnValues.ListSelect = agg.GetType().GetProperty(column.ListSelect).GetValue(agg, null)?.ToString();
                                //        columnValues.Value = agg.GetType().GetProperty(column.ListSelect).GetValue(agg, null)?.ToString();
                                //    }
                                //    break;
                            }
                            break;

                        case BaseType.Calculated:
                            if (column.IsClientCalculatable == true || column.IsEditable)
                            {
                                if (column.Calculation.StartsWith(CALCULATED_FIELD_PREFIX_EMPLOYEE))
                                {
                                    var empFieldValue = "";
                                    var empField = column.Calculation.Substring(0, column.Calculation.IndexOf("}")).Replace(CALCULATED_FIELD_PREFIX_EMPLOYEE, "").Replace("}", "");
                                    if(emp.GetType().GetProperties().Any(x=> x.Name == empField))
                                    {
                                        empFieldValue = emp.GetType().GetProperty(empField).GetValue(emp, null)?.ToString();

                                        // replace place holder with values
                                        var finalPattern = column.Calculation.Replace("{employee." + empField + "}", empFieldValue);

                                        columnValues.Value = new DataTable().Compute(finalPattern, "").ToString();
                                    }
                                    else
                                    {
                                        empFieldValue = (emp.EmployeePayComponents.FirstOrDefault(x => x.PayAdjustment.Name == empField)?.Total ?? 0).ToString();
                                        columnValues.Value = empFieldValue;
                                    }
                                }
                                else if (column.Calculation.StartsWith(CALCULATED_FIELD_PREFIX_CHART))
                                {
                                    var empFieldValue = "";
                                    var aggField = column.Calculation.Substring(0, column.Calculation.IndexOf("}")).Replace(CALCULATED_FIELD_PREFIX_EMPLOYEE, "").Replace("}", "");
                                    if (emp.GetType().GetProperties().Any(x => x.Name == aggField))
                                    {
                                        if(column.WorkId.HasValue && column.WorkId.Value > 0)
                                        {
                                            var first = agg.TaskAggByWorkIds.FirstOrDefault(x => x.WorkId == column.WorkId.Value);
                                            if(first != null)
                                                empFieldValue = first.GetType().GetProperty(aggField).GetValue(first, null)?.ToString();
                                        }
                                        else
                                            empFieldValue = agg.GetType().GetProperty(aggField).GetValue(agg, null)?.ToString();

                                        // replace place holder with values
                                        var finalPattern = column.Calculation.Replace("{chart." + aggField + "}", empFieldValue);

                                        columnValues.Value = new DataTable().Compute(finalPattern, "").ToString();
                                    }
                                    else
                                    {
                                        empFieldValue = agg.GetType().GetProperty(aggField).GetValue(agg, null)?.ToString();
                                        // (emp.EmployeePayAdjustments.FirstOrDefault(x => x.Adjustment == aggField)?.Total ?? 0).ToString();
                                        columnValues.Value = empFieldValue;
                                    }
                                }
                            }

                            if (column.IsServerCalculatable == true || column.IsEditable)
                            {
                                columnValues.Value = EvaluateEmployeeFieldValues(column, emp, list, agg);
                            }
                            break;
                    }

                    list.Add(columnValues);
                }
            }
            return list;
        }

        private string GetKeyValuePair(string v, string d)
        {
            // if (v == d) return "";
            return $"{v}={d};";
        }

    }
}
