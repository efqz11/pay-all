using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Payroll.Models;
using Payroll.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Configuration;
using Payroll.Filters;

namespace Payroll.Database
{
    public static class ChangeTrackerExtensions
    {
        public static void AddAuditEntries(this ChangeTracker changeTracker, UserResolverService userResolverService, IAuditDbContext auditDbContext)
        {
            changeTracker.DetectChanges();
            List<AuditEntry> auditEntries = new List<AuditEntry>();
            foreach (EntityEntry entry in auditDbContext.ChangeTracker.Entries()
                    .Where(x=> x.Entity.GetType().IsDefined(typeof(AuditableEntityAttribute), true)))
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached ||
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

                auditDbContext.AuditLogs.AddRange(logs);
            }
        }


        public static void SetShadowProperties(this ChangeTracker changeTracker, UserResolverService userResolverService)
        {
            changeTracker.DetectChanges();
            var timestamp = DateTime.UtcNow;
            var userId = userResolverService.GetUserId();
            var userName = userResolverService.GetUserName();
            var roles = userResolverService.Get("AccessGrant.Roles");


            foreach (var entry in changeTracker.Entries())
            {
                if (entry.Entity is IAudit)
                {
                    if (entry.State == EntityState.Modified)
                    {
                        entry.Property("ModifiedDate").CurrentValue = timestamp;
                        entry.Property("ModifiedById").CurrentValue = userId;
                        entry.Property("ModifiedByName").CurrentValue = userName;
                        entry.Property("ModifiedByRoles").CurrentValue = roles;
                    }
                    if (entry.State == EntityState.Added)
                    {
                        entry.Property("CreatedDate").CurrentValue = timestamp;
                        entry.Property("CreatedById").CurrentValue = userId;
                        entry.Property("CreatedByName").CurrentValue = userName;
                        entry.Property("CreatedByRoles").CurrentValue = roles;
                    }
                }

                if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete)
                {
                    entry.State = EntityState.Modified;
                    entry.Property("IsDeleted").CurrentValue = true;
                }
            }
        }



        public static void SetIsDeletedShadowProperty<T>(ModelBuilder builder) where T : class, ISoftDelete
        {
            // define shadow property
            builder.Entity<T>().Property<bool>("IsDeleted");
        }

        public static void SetAuditingShadowProperties<T>(ModelBuilder builder) where T : class, IAudit
        {
            // define shadow properties
            builder.Entity<T>().Property<DateTime>("CreatedDate").HasDefaultValueSql("GetUtcDate()");
            builder.Entity<T>().Property<DateTime?>("ModifiedDate").HasDefaultValueSql("GetUtcDate()");
            builder.Entity<T>().Property<string>("CreatedByRoles");
            builder.Entity<T>().Property<string>("CreatedByName");
            builder.Entity<T>().Property<string>("ModifiedByName");
            builder.Entity<T>().Property<string>("ModifiedByRoles");
            builder.Entity<T>().Property<string>("CreatedById");
            builder.Entity<T>().Property<string>("ModifiedById");

            builder.Entity<T>().Property<bool>("IsActive");

            foreach (var entityType in builder.Model.GetEntityTypes()
            .SelectMany(x => x.GetProperties())
            .Where(x => x.ClrType == typeof(decimal)))
            {
                // entityType.Relational().ColumnType = "decimal(18,4)";
                entityType.SetColumnType("decimal(18,4)");
            }

            // define FKs to User
            //builder.Entity<T>().HasOne<AppUser>().WithMany().HasForeignKey("CreatedById").OnDelete(DeleteBehavior.Restrict);
            //builder.Entity<T>().HasOne<AppUser>().WithMany().HasForeignKey("ModifiedById").OnDelete(DeleteBehavior.Restrict);
        }


        public static void SetGlobalQueryForSoftDelete<T>(ModelBuilder builder) where T : class, ISoftDelete
        {
            builder.Entity<T>().HasQueryFilter(item => !EF.Property<bool>(item, "IsDeleted"));
        }

    }


    //public static class ConnectionTools
    //{
    //    // all params are optional
    //    public static void ChangeDatabase(
    //        this DbContext source,
    //        string initialCatalog = "",
    //        string dataSource = "",
    //        string userId = "",
    //        string password = "",
    //        bool integratedSecuity = true,
    //        string configConnectionStringName = "")
    //    /* this would be used if the
    //    *  connectionString name varied from 
    //    *  the base EF class name */
    //    {
    //        try
    //        {
    //            // use the const name if it's not null, otherwise
    //            // using the convention of connection string = EF contextname
    //            // grab the type name and we're done
    //            var configNameEf = string.IsNullOrEmpty(configConnectionStringName)
    //                ? source.GetType().Name
    //                : configConnectionStringName;

    //            // add a reference to System.Configuration
    //            var entityCnxStringBuilder = new EntityConnectionStringBuilder
    //                (System.Configuration.ConfigurationManager
    //                    .ConnectionStrings[configNameEf].ConnectionString);

    //            // init the sqlbuilder with the full EF connectionstring cargo
    //            var sqlCnxStringBuilder = new SqlConnectionStringBuilder
    //                (entityCnxStringBuilder.ProviderConnectionString);

    //            // only populate parameters with values if added
    //            if (!string.IsNullOrEmpty(initialCatalog))
    //                sqlCnxStringBuilder.InitialCatalog = initialCatalog;
    //            if (!string.IsNullOrEmpty(dataSource))
    //                sqlCnxStringBuilder.DataSource = dataSource;
    //            if (!string.IsNullOrEmpty(userId))
    //                sqlCnxStringBuilder.UserID = userId;
    //            if (!string.IsNullOrEmpty(password))
    //                sqlCnxStringBuilder.Password = password;

    //            // set the integrated security status
    //            sqlCnxStringBuilder.IntegratedSecurity = integratedSecuity;

    //            // now flip the properties that were changed
    //            source.Database.Connection.ConnectionString
    //                = sqlCnxStringBuilder.ConnectionString;
    //        }
    //        catch (Exception ex)
    //        {
    //            // set log item if required
    //        }
    //    }
    //}
}
