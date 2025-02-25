using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public interface IAudit : ISoftDelete
    {
        //DateTime CreatedDate { get; set; }
        //string CreatedById { get; set; }
        //string CreatedByName { get; set; }


        //DateTime? ModifiedDate { get; set; }
        //string ModifiedById { get; set; }
        //string ModifiedByName { get; set; }
    }
}