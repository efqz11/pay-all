using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Job : Audit
    {
        public int Id { get; set; }


        public string JobID { get; set; }

        [Required]
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        [Required]
        [Display(Name = "Level")]
        public int LevelId { get; set; }
        public virtual Level Level { get; set; }


        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }


        [Display(Name = "Division")]
        public int? DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [Display(Name = "Location")]
        public int? LocationId { get; set; }
        public virtual Location Location { get; set; }


        [Display(Name ="Job Title")]
        public string JobTitle { get; set; }


        //[ForeignKey("ReportingEmployee")]
        //public int? ReportingEmployeeId { get; set; }
        //public Employee ReportingEmployee { get; set; }

        //public bool DirectlyReportingToMD { get; set; }

        public JobType JobType { get; set; }
        public JobStatus JobStatus { get; set; }

        // associated leaveIds (DayOff) ID
        public IList<int> DayOffIds { get; set; }
        public virtual IList<JobPayComponent> JobPayComponents { get; set; }
        public virtual IList<Employee> Employees { get; set; }
        public virtual IList<Job> ReportingJobs { get; set; }



        [ForeignKey("ReportingJob")]
        public int? ReportingJobId { get; set; }
        public Job ReportingJob { get; set; }

        // changing fields
        //public int? EmpId { get; set; }
        //public string EmpAvatar { get; set; }



        public Job()
        {
            Employees = new List<Employee>();
            ReportingJobs = new List<Job>();
            JobPayComponents = new List<JobPayComponent>();
        }
    }
}
