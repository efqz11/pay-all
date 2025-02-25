using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    /// <summary>
    /// One Column for some Pay Adjustment
    /// Say Name = new PayrollPeriodPayAdjustmentFieldValue()
    ///     No of Articles = new PayrollPeriodPayAdjustmentFieldValue()
    /// </summary>
    public class PayrollPeriodPayAdjustmentFieldValue
    {
        public int Id { get; set; }

        [Required]
        public int PayrollPeriodPayAdjustmentId { get; set; }
        public virtual PayrollPeriodPayAdjustment PayrollPeriodPayAdjustment { get; set; }


        ///// <summary>
        ///// COPY from the original config (PayAdjustmentFieldConfig)
        ///// </summary>
        //[Required]
        //public int PayAdjustmentFieldConfigId { get; set; }
        //public virtual PayAdjustmentFieldConfig PayAdjustmentFieldConfig { get; set; }

        public string PayAdjustment { get; set; }

        // public int KeyId { get; set; }
        public VariationType VariationType { get; set; }

        /// <summary>
        /// Name of column (AdditionDeductionField.DisplayName)
        /// </summary>
        public string Key { get; set; }
        
        
        public bool IsManualEntry { get; set; }

        // public FieldType FieldType { get; set; }
        public string ValueString { get; set; }
        public string ValueInt { get; set; }
        public string Value { get; set; }


        #region Field Config Copy

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
        /// hreshold value: actual value cannot go beyond this value (client/server) 
        /// this value as in value stored in this field for all rows
        /// </summary>
        public bool ThresholdValue { get; set; }


        /// <summary>
        /// Only for chart / Agg calculations {chart.*****} in formula
        /// Example: Absent days, leave days, works completed count, etc.
        /// </summary>
        public bool IsAggregated { get; set; }

        public bool IsEditable { get; set; }

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

        public string DisplayedValueFrontEnd { get; set; }
        #endregion
    }
}
