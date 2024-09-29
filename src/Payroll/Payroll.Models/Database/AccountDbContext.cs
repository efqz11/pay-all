using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Payroll.Models;
using Payroll.Services;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace Payroll.Database
{
    public class AccountDbContext : IdentityDbContext<AppUser, AppRole, string, IdentityUserClaim<string>,
    AppUserRole, IdentityUserLogin<string>,
    IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        private readonly IConfiguration _config;
        private readonly UserResolverService _userResolverService;

        public AccountDbContext()
        {
        }

        public AccountDbContext(DbContextOptions<AccountDbContext> options, UserResolverService userResolverService)
            : base(options)
        {
            this._userResolverService = userResolverService;
        }

        public DbSet<Plan> Plans { get; set; }
        public DbSet<CompanyPlan> CompanyPlans { get; set; }
        public DbSet<CompanyPlanBilling> CompanyPlanBillings { get; set; }
        public DbSet<CompanyPlanBillingPayment> CompanyPlanBillingPayments { get; set; }

        public DbSet<AccessGrant> AccessGrants { get; set; }
        public DbSet<AccessGrantRole> AccessGrantRoles { get; set; }


        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<RequestProcessConfig> AppUserLogins { get; set; }


        public DbSet<CompanyAccount> CompanyAccounts { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<State> States { get; set; }



        public DbSet<Industry> Industries { get; set; }

        //public DbSet<KpiConfig> KpiConfigs { get; set; }


        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }


        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportView> ReportViews { get; set; }

        public DbSet<RequestProcessConfig> RequestProcessConfigs { get; set; }
        public DbSet<RequestNumberFormat> RequestNumberFormats { get; set; }
        public DbSet<TaskRunReport> TaskRunReports { get; set; }


        public DbSet<ScheduledSystemTask> ScheduledSystemTasks { get; set; }




        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(_config.GetConnectionString("AccountConnection"))
        //        .EnableSensitiveDataLogging();
        //}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // enable auto history functionality.
            builder.EnableAutoHistory(int.MaxValue);

            //builder.Entity<CompanyAccount>()
            //.Property(b => b._CompanyWorkTimes).HasColumnName("CompanyWorkTimes");




            builder.Entity<AppUser>(b =>
            {
                b.ToTable("AppUsers");
                b.HasMany(u => u.AppUserRoles)
                 .WithOne(ur => ur.AppUser)
                 .HasForeignKey(ur => ur.UserId)
                 .IsRequired();
            });

            builder.Entity<AppUserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.AppRole)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.AppUser)
                    .WithMany(r => r.AppUserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            builder.Entity<AppRole>().ToTable("AppRoles");
            builder.Entity<AppUserRole>().ToTable("AppUserRoles");

            

            builder.ApplyConfiguration(new NotificationTypeConfiguration());
            builder.ApplyConfiguration(new NotificationConfiguration());
            builder.ApplyConfiguration(new CompanyAccountConfiguration());
            builder.ApplyConfiguration(new RequestProcessConfigConfiguration());
            builder.ApplyConfiguration(new ReportConfigConfiguration());
            builder.ApplyConfiguration(new AppUserConfiguration());




            // enable auto history functionality.
            builder.EnableAutoHistory(int .MaxValue);
            
            
            foreach (var tp in builder.Model.GetEntityTypes())
            {
                var t = tp.ClrType;

                // set auditing properties
                if (typeof(IAudit).IsAssignableFrom(t))
                {
                    var method = SetAuditingShadowPropertiesMethodInfo.MakeGenericMethod(t);
                    method.Invoke(builder, new object[] { builder });
                }

                if (typeof(ISoftDelete).IsAssignableFrom(t))
                {
                    var method = SetIsDeletedShadowPropertyMethodInfo.MakeGenericMethod(t);
                    method.Invoke(builder, new object[] { builder });
                }


                // set global filters
                if (typeof(ISoftDelete).IsAssignableFrom(t))
                {
                    var method = SetGlobalQueryForSoftDeleteMethodInfo.MakeGenericMethod(t);
                    method.Invoke(this, new object[] { builder });
                }

            }

        }


        private readonly MethodInfo SetAuditingShadowPropertiesMethodInfo = typeof(ChangeTrackerExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
         .Single(t => t.IsGenericMethod && t.Name == "SetAuditingShadowProperties");

        private readonly MethodInfo SetIsDeletedShadowPropertyMethodInfo = typeof(ChangeTrackerExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(t => t.IsGenericMethod && t.Name == "SetIsDeletedShadowProperty");

        private readonly MethodInfo SetGlobalQueryForSoftDeleteMethodInfo = typeof(ChangeTrackerExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
          .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQueryForSoftDelete");

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            ChangeTracker.SetShadowProperties(_userResolverService);
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ChangeTracker.SetShadowProperties(_userResolverService);
            return base.SaveChanges();
        }

    }
}
