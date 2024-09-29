using Payroll.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class DepartmentHead : Audit
    {
        public int Id { get; set; }
        
        [NotMapped]
        public int? CompanyId { get; set; }
        public int? EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }


        public int? DepartmentId { get; set; }
        public virtual Department Department { get; set; }


        public DepartmentHead()
        {

        }
    }
}
