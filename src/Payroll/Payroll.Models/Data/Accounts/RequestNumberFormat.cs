using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class RequestNumberFormat : Audit
    {
        public int Id { get; set; }


        public int CompanyAccountId { get; set; }
        public virtual CompanyAccount CompanyAccount { get; set; }

        public RequestType RequestType { get; set; }

        [StringLength(5)]
        public string Prefix { get; set; }

        public string FormatString { get; set; }
        public bool IsResetAnnually { get; set; }
        public int StartingNumber { get; set; }


        public string AutoActionSummary { get; set; }
    }
}
