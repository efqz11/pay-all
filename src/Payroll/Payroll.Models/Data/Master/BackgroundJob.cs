using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    //public interface HasBackgroundJob
    //{
    //    IList<BackgroundJob> BackgroundJob { get; set; }
    //}

    public class BackgroundJob : Audit
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int? ScheduleId { get; set; }
        public virtual Schedule Schedule { get; set; }

        public int? AnnouncementId { get; set; }
        public virtual Announcement Announcement { get; set; }
        

        //public int? EmployeeId { get; set; }
        public int? CompanyAccountId { get; set; }

        //publManic Schedule Schedule { get; set; }

        [Required]
        public string Name { get; set; }
        
        public TaskType TaskType { get; set; }
        public TaskStatus TaskStatus { get; set; }

        public string HangfireJobId { get; set; }



        public DateTime? NextRunDate { get; set; }

        public string EndActions { get; set; }
        public string Details { get; set; }


        public DateTime RunDate { get; set; }
        public DateTime? EndedDate { get; set; }
        public Guid Identifier { get;  set; }

        public BackgroundJob()
        {
        }
    }
}
