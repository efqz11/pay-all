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
    public class EmployeePassport : Audit
    {
        public int Id { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public string Number { get; set; }


        [Display(Name = "Issued Date")]
        public DateTime IssuedDate { get; set; }

        [Display(Name = "Expiration Date")]
        public DateTime ExpirationDate { get; set; }

        public string Notes { get; set; }


        [Required]
        public string IssuingCountryCode { get; set; }


        public EmployeePassport()
        {
        }
    }
}
