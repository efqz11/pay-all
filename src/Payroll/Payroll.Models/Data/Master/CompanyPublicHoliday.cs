using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class CompanyPublicHoliday : Audit
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        
        [Required]
        public string Year { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool IsImported { get; set; }
        public bool IsManualEntry { get; set; }


        [NotMapped]
        public DateTime? Date2 { get; set; }
        [NotMapped]
        public bool HasRange { get; set; }


        public bool IsPublicHoliday { get; set; }

        public CompanyPublicHoliday()
        {
        }
    }
}
