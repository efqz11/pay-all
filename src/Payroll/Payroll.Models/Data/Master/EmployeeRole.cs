
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
    public class EmployeeRole : Audit
    {
        public int Id { get; set; }

        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int RoleId { get; set; }

        [Required]
        public int CompanyId { get; set; }
        public Company Company { get; set; }


        [Required]
        public string Role { get; set; }

        public string Description { get; set; }

        public IDictionary<string, EmployeeSelectorRole> CalendarDefaults { get; set; }

        // IDictionary <scope|role, value>
        public IDictionary<string, string[]> UserAccessRights { get; set; }
        public bool Enable2fa { get; set; }
        public virtual IList<Reminder> Reminders { get; set; }
        public virtual IList<EmployeeRoleRelation> AssignedEmployees { get; set; }

        public EmployeeRole()
        {
            Reminders = new List<Reminder>();
            AssignedEmployees = new List<EmployeeRoleRelation>();
        }
    }
}
