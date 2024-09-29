using Payroll.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class AccessGrant : Audit
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual AppUser User { get; set; }
        
        public int CompanyAccountId { get; set; }
        public CompanyAccount CompanyAccount { get; set; }

        public string Reason { get; set; }
        public AccessGrantStatus Status { get; set; }


        [SelectableField]
        [Display(Name= "Apply on specific date")]
        [DisplayFormat(ApplyFormatInEditMode = true,  DataFormatString = "{0:dd-MMM-yyyy")]
        [DataType(DataType.Date)]
        public DateTime? ApplyOnDate { get; set; }


        [SelectableField]
        [Display(Name = "Expiry Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// CSV of roles
        /// </summary>
        public string Roles { get; set; }

        public string RolesConstraint { get; set; }
        public bool IsDefault { get; set; }

        public AccessGrant()
        {
        }
    }
}
