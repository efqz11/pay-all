using Payroll.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Department : Audit
    {
        public int Id { get; set; }

        //[ForeignKey("LineManager")]
        //public int? LineManagerId { get; set; }
        
        //[NotMapped]
        //public virtual Employee LineManager { get; set; }


        [SelectableField]
        [Required]
        public string Name { get; set; }

        [Required]
        public int CompanyId { get; set; }
        public Company Company { get; set; }


        [SelectableField]
        public string RenewedFrom { get; set; }


        [SelectableField]
        [StringLength(10)]
        public string DeptCode { get; set; }


        [SelectableField]
        public decimal TotalHeadCount { get; set; }
        public decimal WorkHoursPerWeek { get; set; }

        public int DisplayOrder { get; set; }

        public virtual List<Employee> Employees { get; set; }
        public virtual List<DepartmentHead> DepartmentHeads { get; set; }
        //public virtual List<Schedule> Schedules { get; set; }


        public Department()
        {
            Employees = new List<Employee>();
            DepartmentHeads = new List<DepartmentHead>();
            //Schedules = new List<Schedule>();
        }
    }
}
