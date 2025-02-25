using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class PayrollPeriodPayAdjustment
    {
        [Required]
        public int PayrollPeriodId { get; set; }
        public virtual PayrollPeriod PayrollPeriod { get; set; }

        public int Id { get; set; }


        //public Employee Employee { get; set; }
        //public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }


        [Required]
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        public VariationType VariationType { get; set; }

        [Required]
        public string Adjustment { get; set; }

        /// <summary>
        /// Total from(final) the fields table
        /// </summary>
        [Required]
        public decimal Total { get; set; }

        [Required]
        public int CalculationOrder { get; set; }

        [Required]
        [ForeignKey("PayAdjustment")]
        public int PayAdjustmentId { get; set; }
        public virtual PayAdjustment PayAdjustment { get; set; }


        public virtual List<PayrollPeriodPayAdjustmentFieldValue> PayrollPeriodPayAdjustmentFieldValues { get; set; }

        public PayrollPeriodPayAdjustment()
        {
            PayrollPeriodPayAdjustmentFieldValues = new List<PayrollPeriodPayAdjustmentFieldValue>();
        }
    }

}
