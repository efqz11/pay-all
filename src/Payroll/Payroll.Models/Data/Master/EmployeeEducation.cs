using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{

    /// <summary>
    /// Actions can be basically anything, that communicates with employer with a contract (comapny) with employee
    /// </summary>
    public class EmployeeEducation : Audit
    {
        public int Id { get; set; }

        [Required]
        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }

        [Display(Name ="College/Institution")]
        public string CollegeInstitution { get; set; }

        public int DegreeId { get; set; }
        public string Degree { get; set; }

        [Display(Name ="Major/Specilization")]
        public string MajorSpecilization { get; set; }
        public string GPA { get; set; }



        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public bool? IsOnGoing { get; set; }

        public int DisplayOrder { get; set; }

        public EmployeeEducation()
        {
        }
    }
}
