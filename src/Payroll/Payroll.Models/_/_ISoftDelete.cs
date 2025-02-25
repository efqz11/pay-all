using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public interface ISoftDelete
    {
        bool IsActive { get; set; }
        //bool IsDeleted { get; set; }
    }
}
