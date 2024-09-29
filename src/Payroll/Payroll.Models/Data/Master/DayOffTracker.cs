using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class DayOffTracker : Audit
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        [Required]
        public int DayOffId { get; set; }
        public DayOff DayOff { get; set; }

        [Required]
        public int DayOffEmployeeId { get; set; }
        public DayOffEmployee DayOffEmployee { get; set; }


        [Column(TypeName = "decimal(3,1)")]
        public decimal TotalHours { get; set; }
        public int TotalDays { get; set; }

        [Column(TypeName = "decimal(3,1)")]
        public decimal RemainingBalance { get; set; }

        public bool IsAddOrSubState { get; set; }

        public DayOffEmployeeItemStatus Status { get; set; }

        /// Is added to list by system (accured or initial kickoff)
        public bool IsAccruredBySystem { get; set; }
        public bool IsCreateByDuringKickOff { get; set; }

        public string Summary { get; set; }
        public string JobIdentifier { get; set; }


        public int? RequestId { get; set; }
        public virtual Request Request { get; set; }

    }
}
