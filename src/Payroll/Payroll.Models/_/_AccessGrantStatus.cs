using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public enum AccessGrantStatus
    {
        Active,
        ApplyOn,
        Expired, 
        Suspended,
        PendingApproval
    }
}
