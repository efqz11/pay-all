using Payroll.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Team : Audit
    {
        public int Id { get; set; }

        //[ForeignKey("LineManager")]
        //public int? EmployeeId { get; set; }

        //[NotMapped]
        //public virtual Employee LineManager { get; set; }


        [SelectableField]
        [Required]
        public string Name { get; set; }

        [Required]
        public int CompanyId { get; set; }
        public Company Company { get; set; }


        public int DisplayOrder { get; set; }

        public virtual List<EmployeeTeam> EmployeeTeams { get; set; }

        public Team()
        {
            EmployeeTeams = new List<EmployeeTeam>();
        }
    }
}
