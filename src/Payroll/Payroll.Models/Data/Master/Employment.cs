using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Employment : Audit
    {
        public int Id { get; set; }

        [Display(Name = "Effective Date")]
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime GetEndDate() =>
            EndDate ?? DateTime.MaxValue;

        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        [NotMapped]
        public bool UpdateFromJobPayComponents { get; set; }
        [Required]
        [Display(Name = "Job")]
        public int JobId { get; set; }
        public virtual Job Job { get; set; }

        //[Display(Name = "Division")]
        //public int? DivisionId { get; set; }
        //public virtual Division Division { get; set; }

        //[Display(Name = "Location")]
        //public int? LocationId { get; set; }
        //public virtual Location Location { get; set; }

        //[Display(Name ="Job Title")]
        //public string JobTitle { get; set; }


        [ForeignKey("ReportingEmployee")]
        public int? ReportingEmployeeId { get; set; }
        public Employee ReportingEmployee { get; set; }


        public bool DirectlyReportingToMD { get; set; }


        public EmployeeStatus EmploymentStatus { get; set; }
        public EmployeeType EmploymentType { get; set; }
        public EmploymentTypeOther EmploymentTypeOther { get; set; }
        

        public RecordStatus RecordStatus { get; set; }
        public List<EmployeePayComponent> EmployeePayComponents { get; set; }


        
        public int WeeklyWorkingHours { get; set; }
        public int DailyWorkingHours { get; set; }

        public Employment()
        {
            EmployeePayComponents = new List<EmployeePayComponent>();
        }
    }
}
