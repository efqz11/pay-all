using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class EmployeePayComponent : Audit
    {
        public int Id { get; set; }


        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }


        [Display(Name ="Effective Date")]
        public DateTime EffectiveDate { get; set; }


        [Required]
        [ForeignKey("Component")]
        public int PayAdjustmentId { get; set; }
        public virtual PayAdjustment PayAdjustment { get; set; }


        public decimal Total { get; set; }

        public PayComponentChangeReason? PayComponentChangeReason { get; set; }

        public PayFrequency PayFrequency { get; set; }


        public RecordStatus RecordStatus { get; set; }


        public EmployeePayComponent()
        {
        }
    }
}
