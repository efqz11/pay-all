using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class SoftDelete: ISoftDelete
    {
        public bool IsActive { get; set; }
        //public bool IsDeleted { get; set; }

        public SoftDelete()
        {
            //IsDeleted = false;
            IsActive = true;
        }
    }
}
