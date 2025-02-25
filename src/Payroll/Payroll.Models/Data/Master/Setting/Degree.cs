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
    public class Degree : Audit
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public Degree()
        {
            
        }
    }
    

}
