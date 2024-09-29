using Microsoft.EntityFrameworkCore;
using Payroll.Database;
using Payroll.Filters;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Services
{
    public class AuditLogService
    {
        private readonly AccountDbContext context;
        private readonly PayrollDbContext payrolDbContext;
        private readonly UserResolverService userResolverService;

        public AuditLogService(AccountDbContext context, PayrollDbContext payrolDbContext, UserResolverService userResolverService)
        {
            this.context = context;
            this.payrolDbContext = payrolDbContext;
            this.userResolverService = userResolverService;
        }

        public async Task<IList<string>> GetAuditableEntityDropdown()
        {
            var x = context.GetType().GetProperties().Where(a => a.PropertyType.IsGenericType && (typeof(DbSet<>).IsAssignableFrom(a.PropertyType.GetGenericTypeDefinition()))).Select(a => a.PropertyType.GenericTypeArguments.First()).ToList();

            var accDbModels = x
                .Where(a => a.IsDefined(typeof(AuditableEntityAttribute), true))
                .Select(a => a.Name.Replace("Payroll.Models.", "")).ToList();

            x= payrolDbContext.GetType().GetProperties().Where(a => a.PropertyType.IsGenericType && (typeof(DbSet<>).IsAssignableFrom(a.PropertyType.GetGenericTypeDefinition()))).Select(a => a.PropertyType.GenericTypeArguments.First()).ToList();

            var payrolLDbCmodels = x
                .Where(a => a.IsDefined(typeof(AuditableEntityAttribute), true))
                .Select(a => a.Name.Replace("Payroll.Models.", "")).ToList();

            return accDbModels.Concat(payrolLDbCmodels).ToList();
        }

        public async Task<(int, List<AuditLog>)> GetAuditLogs(string keyId, string modal, DateTime? start = null, DateTime? end = null, int limit = 10)
        {

            var query = payrolDbContext.AuditLogs.Where(a => a.KeyId == keyId && a.ModelName == modal);
            if (start.HasValue && end.HasValue)
                query = payrolDbContext.AuditLogs.Where(a => a.AuditDateTimeUtc >= start && a.AuditDateTimeUtc <= end);

            int count = await query.CountAsync();
            var page = 1;

            var data = await query
             .OrderByDescending(a => a.AuditDateTimeUtc)
             .Skip((page - 1) * limit)
             .Take(limit)
             .ToListAsync();

            return (count, data);
        }

        public async Task<AuditLog> GetAuditLog(int id)
        {
            return await payrolDbContext.AuditLogs.FindAsync(id);
        }
        
    }

}
