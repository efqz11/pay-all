using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class EmployeeAction : Audit
    {
        public int Id { get; set; }


        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }


        public int? JobId { get; set; }
        public Job Job { get; set; }

        public int? PayAdjustmentId { get; set; }
        public virtual PayAdjustment PayAdjustment { get; set; }

        public int? EmployeePayComponentId { get; set; }
        public virtual EmployeePayComponent EmployeePayComponent { get; set; }


         
        [ForeignKey("ReportingEmployee")]
        public int? ReportingEmployeeId { get; set; }
        public Employee ReportingEmployee { get; set; }



        [Display(Name ="On Date")]
        public DateTime OnDate { get; set; }

        public string Message { get; set; }
        public string Remarks { get; set; }

        public ActionType ActionType { get; set; }
        public RecordStatus RecordStatus { get; set; }

    }
}
