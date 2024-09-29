using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    /// <summary>
    /// Create work Items
    /// ex: Daily - Sports News => require 3, lessDeduct: 100, moreCredit: 100, 
    /// RepeatingFrequency = everyMonth, ForDepartment|ForEmployee
    /// </summary>
    public class Work : Audit
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        [Required]
        public string Name { get; set; }
        public string Details { get; set; }
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }

        public WorkType Type { get; set; }

        public double MinsBeforeCheckIn { get; set; }
        //public double MinsBeforeCheckIn { get; set; }


        [Display(Name = "Total Required to complete")]
        public int TotalRequiredCount { get; set; }
        public bool HasSumbissions => TotalRequiredCount > 0;

        [Display(Name = "Amount to deducted on failing to meet requirement")]
        public decimal LessDeduct { get; set; }

        [Display(Name = "Amount credited to every additional item submitted")]
        public decimal MoreCredit { get; set; }
        

        public int DisplayOrder { get; set; }

        [Display(Name = "Repeating frequency")]
        public RecurringFrequency Frequency { get; set; }


        public List<WorkItem> WorkItems { get; set; }

        public string Duration => StartTime != TimeSpan.MinValue && EndTime != TimeSpan.MinValue && StartTime != EndTime ? GetDurationString() : "";

        [NotMapped]
        public bool IsReapating { get; set; }


        private string GetDurationString()
        {
            try
            {
                return StartTime.ToString("hh\\:mm").ToLower() + " - " + EndTime.ToString("hh\\:mm").ToLower();
            }
            catch (Exception)
            {
                return "";
            }
        }

        //[NotMapped]
        public bool IsAdvancedCreate { get; set; }

        
        public bool HasTime { get; set; }
        public string ColorCombination { get;  set; }



        public Work()
        {
            WorkItems = new List<WorkItem>();
        }
    }

}
