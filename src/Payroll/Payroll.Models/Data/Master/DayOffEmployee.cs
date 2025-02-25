using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class DayOffEmployee : Audit
    {
        public int Id { get; set; }

        [Required]
        public int DayOffId { get; set; }
        public virtual DayOff DayOff { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Column(TypeName = "decimal(3,1)")]
        public decimal TotalHours { get; set; }

        [Column(TypeName = "decimal(3,1)")]
        public decimal TotalRemainingHours { get; set; }

        [Column(TypeName = "decimal(3,1)")]
        public decimal TotalCollectedHours { get; set; }

        public DateTime NextRefreshDate { get; set; }
        public int Year { get; set; }


        public List<DayOffEmployeeItem> DayOffEmployeeItems { get; set; }
        public List<DayOffTracker> DayOffTrackers { get; set; }


        public DayOffEmployee()
        {
            DayOffEmployeeItems = new List<DayOffEmployeeItem>();
            DayOffTrackers = new List<DayOffTracker>();
        }

        public DayOffEmployee(int year, int empId, DayOff dayoff)
        {
            Year = year;
            EmployeeId = empId;
            TotalHours = dayoff.TotalHoursPerYear ?? 0m;
            DayOffId = dayoff.Id;
            NextRefreshDate = new DateTime((year + 1), 1, 1);
            TotalRemainingHours = dayoff.TotalHoursPerYear ?? 0m;
        }
    }
}
