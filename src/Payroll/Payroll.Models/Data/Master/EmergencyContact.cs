using Payroll.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class EmergencyContact : Audit
    {
        public int Id { get; set; }


        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }


        [Display(Name = "Contact's Name")]
        [SelectableField]
        public string ContactName { get; set; }

        [EmailAddress]
        [Display(Name = "Contact's Email")]
        [SelectableField]
        public string ContactEmail { get; set; }

        [EmailAddress]
        [Display(Name = "Contact's Phone no.")]
        [SelectableField]
        [Phone]
        public string ContactNumber { get; set; }

        public int? EmergencyContactRelationshipId { get; set; }
        public EmergencyContactRelationship EmergencyContactRelationship { get; set; }

        [Display(Name = "Emergency contact relation to the employee")]
        [SelectableField]
        public string _EmergencyContactRelationship => EmergencyContactRelationship?.Name ?? "";


        public RecordStatus RecordStatus { get; set; }

    }
}
