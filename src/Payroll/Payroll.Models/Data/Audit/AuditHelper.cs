using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Payroll.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    /// <summary>
    /// All Entity Changes To Audit Table
    /// This helper class is using the AuditEntry class and :
    /// 
    /// creating Audit log entities considering all possible data changes from the current IAuditDbContext
    /// adding log entities to the log table
    /// </summary>
    public class AuditHelper
    {
        readonly IAuditDbContext Db;

        public AuditHelper(IAuditDbContext db)
        {
            Db = db;
        }

        public void AddAuditLogs(Services.UserResolverService userResolverService)
        {
            Db.ChangeTracker.DetectChanges();
            List<AuditEntry> auditEntries = new List<AuditEntry>();
            foreach (EntityEntry entry in Db.ChangeTracker.Entries())
            {
                if (entry.Entity is Audit || entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged)
                {
                    continue;
                }
                var auditEntry = new AuditEntry(entry, userResolverService);
                auditEntries.Add(auditEntry);
            }

            if (auditEntries.Any())
            {
                var logs = auditEntries.Select(x => x.ToAudit());
                Db.AuditLogs.AddRange(logs);
            }
        }
    }
}
