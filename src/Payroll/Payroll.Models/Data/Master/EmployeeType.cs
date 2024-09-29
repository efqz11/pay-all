using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Payroll.Models
{
    //public class EmployeeType : Audit
    //{
    //    public int Id { get; set; }

    //    [Required]
    //    public int EmployeeId { get; set; }
    //    public Employee Employee { get; set; }

    //    [Display(Name = "Effective Date")]
    //    public DateTime EffectiveDate { get; set; }
    //    public DateTime? EndDate { get; set; }

    //    public DateTime GetEndDate() =>
    //        EndDate ?? DateTime.MaxValue;


    //    public string Comment { get; set; }

    //    public EmployeeStatus EmploymentStatus { get; set; }

    //    public RecordStatus RecordStatus { get; set; }


    //    public int? TerminationReasonId { get; set; }
    //    public TerminationReason TerminationReason { get; set; }

    //    public string GetDuration(ClaimsPrincipal user)
    //    {
    //        return EffectiveDate.GetDuration(EndDate, user, false);
    //    }

    //    public EmployeeType()
    //    {
    //    }
    //}
}
