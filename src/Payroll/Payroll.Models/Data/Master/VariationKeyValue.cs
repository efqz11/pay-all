using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class VariationKeyValue : Audit
    {
        public int PayrollPeriodEmployeeId { get; set; }
        public virtual PayrollPeriodEmployee PayrollPeriodEmployee { get; set; }


        public int Id { get; set; }

        /// <summary>
        /// KeyID = Addition or Deduction value {based on type}
        /// </summary>
        [Required]
        public int KeyId { get; set; }

        /// <summary>
        /// Unique name of Addition or Deduction
        /// </summary>
        [Required]
        public string Key { get; set; }

        /// <summary>
        /// Aggregated points
        /// </summary>
        [Required]
        public decimal Value { get; set; }

        public int MultiOrder { get; set; }

        [Required]
        public VariationType Type { get; set; }
    }

    public enum VariationType
    {
        [Display(Name = "Variable Deduction")]
        VariableDeduction,
        [Display(Name = "Variable Addition")]
        VariableAddition,
        [Display(Name = "Constant Addition")]
        ConstantAddition,
        [Display(Name ="Constant Deduction")]
        ConstantDeduction
    }
}
