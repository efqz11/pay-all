
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    /// <summary>
    /// One Column for some Pay Adjustment (configuration)
    /// Say Name => from list:emplployee, field 
    ///     No of Articles => manual entry, number, calc: xxx 
    /// </summary>
    public class PayAdjustmentFieldConfig : Audit
    {
        public int Id { get; set; }
        public int? PayAdjustmentId { get; set; }
        

        public BaseType BaseType { get; set; }
        public FieldType FieldType { get; set; }
        public ListType ListType { get; set; }
        public string ListFilter { get; set; }

        /// <summary>
        /// Select will comma separated select data list (vaidated to type.GetProperties().FieldNames
        /// </summary>
        public string ListSelect { get; set; }

        /// <summary>
        /// How you wanto display in the table
        /// </summary>
        // [Required]
        public string DisplayName { get; set; }


        public string Calculation { get; set; }


        public int CalculationOrder { get; set; }
        public string CalculationIdentifier { get; set; }

        /// <summary>
        /// Generated value internally based on item usage
        /// most BaseType = Calculated
        /// and if BaseType = ManualEntry and has
        /// </summary>
        public bool IsClientCalculatable { get; set; }

        /// <summary>
        /// Sets of this field will be calculated on server
        /// </summary>
        public bool IsServerCalculatable { get; set; }

        // only if  calculation fields are {chart.****}
        public bool IsAggregated { get; set; }

        public bool IsEditable { get; set; }
        public int? WorkId { get; set; }


        public string OnBlur { get; set; }

        /// <summary>
        /// Js update target
        /// </summary>
        public string UpdateInputClass { get; set; }

        /// <summary>
        /// Evaluate method for JS (client side)
        /// </summary>
        public string EvalMethod { get; set; }

        /// <summary>
        /// Set field as return value to original pay adjustment:VALUE
        /// </summary>
        public bool IsReturn { get; set; }


        /// <summary>
        /// if return (true) then ask if there is ceil value
        /// </summary>
        public bool HasCeilValue { get; set; }

        /// <summary>
        /// Ceil value calculation formula (server calculated)
        /// </summary>
        public string CeilValueCalculation { get; set; }

        public string DisplayedValueFrontEnd => GetDisplayedValue();
        

        private string GetDisplayedValue()
        {
            var value = "";
            if (IsReturn)
                value = "*";
            switch (BaseType)
            {
                case BaseType.ComputedList:
                    value = "computed-list:";
                    switch (ListType)
                    {
                        case ListType.None:
                            break;
                        case ListType.Employee:
                            value += ListType.Employee.ToString()+":";
                            value += ListSelect.ToString();
                            break;
                        case ListType.Department:
                            value += ListType.Department.ToString() + ":";
                            value += ListSelect.ToString();
                            break;
                        //case ListType.Aggregated:
                        //    value += ListType.Aggregated.ToString() + ":";
                        //    value += ListSelect.ToString();
                            //break;
                        default:
                            break;
                    }
                    break;
                case BaseType.Calculated:
                    value += "calculated:";
                    value += Calculation;
                    break;
                case BaseType.ManualEntry:
                    value += "manual-entry:" + FieldType.ToString();
                    break;
                default:
                    break;
            }
            return value;
        }
    }


public enum BaseType
    {
        [Display(Name = "Computed list")]
        ComputedList,
        [Display(Name = "Calculation formula")]
        Calculated,
        [Display(Name = "Manual entry field")]
        ManualEntry,
    }

    public enum FieldType
    {
        Text,
        Number,
        Decimal,
        Date,
    }

    public enum ListType
    {
        None = 0,
        Employee,
        Department
        //Constant
    }
}
