using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class CompanyPlanBillingPayment : Audit
    {
        public int Id { get; set; }

        public int? CompanyPlanBillingId { get; set; }
        public virtual CompanyPlanBilling CompanyPlanBilling { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        
        public bool IsCleared { get; set; }

        public DateTime PaidDate { get; set; }
        public string PaidBy { get; set; }

    }
}
