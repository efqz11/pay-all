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
    public class CompanyPlan : Audit
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int? CompanyAccountId { get; set; }
        public virtual CompanyAccount CompanyAccount { get; set; }

        public int? PlanId { get; set; }
        public virtual Plan Plan { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
