using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public abstract class Audit : SoftDelete, IAudit
    {
        //public DateTime CreatedDate { get; set; }
        //public string CreatedById { get; set; }
        //public string CreatedByName { get; set; }

        
        //public DateTime? ModifiedDate { get; set; }
        //public string ModifiedById { get; set; }
        //public string ModifiedByName { get; set; }

        public Audit()
        {
            //CreatedDate = DateTime.Now;
        }
    }
}
