using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class DayOffEmployeeItem : Audit
    {
        public int Id { get; set; }

        [Required]
        public int DayOffEmployeeId { get; set; }
        public virtual DayOffEmployee DayOffEmployee { get; set; }

        
        public int? RequestId { get; set; }
        public virtual Request Request { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public int TotalDays { get; set; }

        public bool IsCreatedFromRequest { get; set; }
        public int? CreatedFromRequestId { get; set; }
        //public Request Request { get; set; }
        public DayOffEmployeeItemStatus Status { get; set; }

        public bool CreatedManually { get; set; }

    }
}
