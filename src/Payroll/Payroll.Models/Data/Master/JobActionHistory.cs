using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class JobActionHistory : Audit
    {
        public int Id { get; set; }

        public ActionType ActionType { get; set; }

        //[Required]
        //public int? EmployeeId { get; set; }
        //public Employee Employee { get; set; }

        public string IndividualName { get; set; }


        public int? JobId { get; set; }
        public virtual Job Job { get; set; }

        public int? EmploymentId { get; set; }
        public virtual Employment Employment { get; set; }


        [Required]
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        public DateTime OnDate { get; set; }

        [Required]
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }


        public int? RelatedRequestId { get; set; }
        public virtual Request RelatedRequest { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }
        public virtual Department Department { get; set; }


        [Display(Name = "Division")]
        public int? DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [Display(Name = "Location")]
        public int? LocationId { get; set; }
        public virtual Location Location { get; set; }


        
        public int? PreviousJobId { get; set; }
        public virtual Job PreviousJob { get; set; }

        public int? PreviousEmploymentId { get; set; }
        public virtual Employment PreviousEmployment{ get; set; }

        
        public virtual List<JobActionHistoryChangeSet> JobActionHistoryChangeSets { get; set; }

        public string Remarks { get; set; }
        public int ChangeSetCount { get; set; }
        public string RelatedRequestReference { get; set; }
        public string JobIdString { get; set; }

        public JobActionHistory()
        {
        JobActionHistoryChangeSets = new List<JobActionHistoryChangeSet>();
        }
    }
}
