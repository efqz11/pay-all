using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class PayAdjustment : Audit
    {
        [NotMapped]
        public int _fieldsCount { get; set; }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public VariationType VariationType { get; set; }
        public int CalculationOrder { get; set; }

        public bool? IsPensionCharge { get; set; }

        /// <summary>
        /// this pay ajustment going to be filled by employee
        /// for example: Phone Allowance
        ///              Service Allowance 
        ///             ... (constant value copied to each payrol)
        /// </summary>
        public bool? IsFilledByEmployee { get; set; }
        public bool? EnforceRequirement { get; set; }

        public virtual List<PayAdjustmentFieldConfig> Fields { get; set; }

        public PayAdjustment()
        {
            Fields = new List<PayAdjustmentFieldConfig>();
        }
    }
}
