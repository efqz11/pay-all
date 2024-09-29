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
    public class CompanyPlanBilling : Audit
    {
        public int Id { get; set; }

        public int? CompanyPlanId { get; set; }
        public virtual CompanyPlan CompanyPlan { get; set; }

        public decimal TotalAmount { get; set; }
        public int TotalDays { get; set; }
        public bool IsProrated { get; set; }
        public decimal ProratedValue { get; set; }
        public DateTime BillDate { get; set; }
        public DateTime DueDate { get; set; }

    }
}
