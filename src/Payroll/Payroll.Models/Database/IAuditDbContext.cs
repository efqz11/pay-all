using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Database
{
    public interface IAuditDbContext
    {
        DbSet<AuditLog> AuditLogs { get; set; }
        ChangeTracker ChangeTracker { get; }
    }
}
