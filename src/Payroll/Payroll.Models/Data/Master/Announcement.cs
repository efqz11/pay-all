using Newtonsoft.Json;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Announcement : Audit
    {
        public int Id { get; set; }

        public int CreatedEmployeeId { get; set; }
        public Employee CreatedEmployee { get; set; }



        public int CompanyId { get; set; }
        public Company Company { get; set; }

        //public int? DepartmentId { get; set; }
        //public Department Department { get; set; }

        //public int? EmployeeId { get; set; }
        //public Employee Employee { get; set; }

        //public bool IsPublic { get; set; }
        //public bool IsForEmployee { get; set; }
        //public bool IsForDepartment { get; set; }
        [JsonIgnore]
        public EmployeeSelectorVm EmployeeSelectorVm { get; set; }
        public AnnouncementStatus Status { get; set; }

        [NotMapped]
        public bool HasExpiry { get; set; }

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Summary { get; set; }
        public bool HasSummary { get; set; }

        public List<FileData> FileDatas { get; set; }

        //public bool IsPublished { get; set; }
        

        public int ViewedCount { get;  set; }
        public int TotalInteractionsCount { get; set; }

        [JsonIgnore]
        public IList<int> EmployeeIdsData { get; set; }
        public DateTime? PublishedDate { get;  set; }
        public DateTime? ExpiredDate { get;  set; }

        public List<BackgroundJob> backgroundJobs { get; set; }

        public Announcement()
        {
            backgroundJobs = new List<BackgroundJob>();
            FileDatas = new List<FileData>();
            //IsPublic = true;
        }

        public bool HasPeriodDefined()
        {
            return this != null && this.Start.HasValue && this.End.HasValue;
        }
    }
}
