
using Payroll.Filters;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class EmployeeRoleRelation
    {
        [Required]
        [Key]
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        [Key]
        public int EmployeeRoleId { get; set; }
        public virtual EmployeeRole EmployeeRole { get; set; }


        public EmployeeRoleRelation()
        {
        }
    }
}
