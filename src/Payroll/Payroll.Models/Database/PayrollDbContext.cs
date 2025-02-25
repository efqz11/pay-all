

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace Payroll.Database
{
    public class PayrollDbContext : DbContext, IAuditDbContext
    {
        private readonly string _connectionString;
        private readonly UserResolverService userResolverService;


        //public PayrolDbContext(DbContextOptions options) : base(options)
        //{
        //}

        public PayrollDbContext(DbContextOptions<PayrollDbContext> options, UserResolverService userResolverService) : base(options)
        {
            this.userResolverService = userResolverService;
        }

        // settings
        // ------------
        public DbSet<Degree> Degrees { get; set; }
        public DbSet<EmergencyContactRelationship> EmergencyContactRelationships { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }

        public DbSet<SecondaryLanguage> SecondaryLanguages { get; set; }
        public DbSet<TerminationReason> TerminationReasons { get; set; }
        public DbSet<Visa> Visa { get; set; }

        // ---------------

        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<BackgroundJob> BackgroundJobs { get; set; }


        public DbSet<BiometricRecord> BiometricRecords { get; set; }
        public DbSet<JobActionHistory> JobActionHistories { get; set; }
        public DbSet<JobActionHistoryChangeSet> JobActionHistoryChangeSets { get; set; }


        //public DbSet<Individual> Individuals { get; set; }
        public DbSet<EmployeePassport> EmployeePassports { get; set; }
        public DbSet<Address> EmployeeAddresses { get; set; }
        public DbSet<EmployeeEducation> EmployeeEducations { get; set; }



        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<RequestApprovalConfig> RequestApprovalConfigs { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyFolder> CompanyFolders { get; set; }
        public DbSet<CompanyFile> CompanyFiles { get; set; }
        public DbSet<CompanyFileShare> CompanyFileShares { get; set; }
        public DbSet<CompanyPublicHoliday> CompanyPublicHolidays { get; set; }
        public DbSet<CompanyWorkTime> CompanyWorkTimes { get; set; }
        public DbSet<CompanyWorkBreakTime> CompanyWorkBreakTimes { get; set; }

        public DbSet<Level> Levels { get; set; }

        

        public DbSet<DayOff> DayOffs { get; set; }
        public DbSet<DayOffEmployee> DayOffEmployees { get; set; }
        public DbSet<DayOffEmployeeItem> DayOffEmployeeItems { get; set; }
        public DbSet<DayOffTracker> DayOffTrackers { get; set; }



        public DbSet<Department> Departments { get; set; }
        public DbSet<DepartmentHead> DepartmentHeads { get; set; }
        public DbSet<Team> Teams { get; set; }


        public DbSet<Division> Divisions { get; set; }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeInteraction> EmployeeInteractions { get; set; }
        public DbSet<EmployeeTeam> EmployeeTeams { get; set; }



        public DbSet<EmployeeAction> EmployeeActions { get; set; }
        public DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public DbSet<EmployeeRoleRelation> EmployeeRoleRelations { get; set; }

        
        public DbSet<Employment> Employments { get; set; }

        public DbSet<EmployeePayComponent> EmployeePayComponents { get; set; }

        //public DbSet<EmployeeType> EmployeeTypes { get; set; }
        public DbSet<EmployeeVisaInfo> EmployeeVisaInfos { get; set; }

        public DbSet<FileData> FileDatas { get; set; }
        public DbSet<Label> Labels { get; set; }
        //public DbSet<Goal> Goals { get; set; }


        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobPayComponent> JobPayComponents { get; set; }

        public DbSet<Location> Locations { get; set; }
        public DbSet<PayAdjustment> PayAdjustments { get; set; }
        public DbSet<PayAdjustmentFieldConfig> PayAdjustmentFieldConfigs { get; set; }



        public DbSet<PayrollPeriod> PayrollPeriods { get; set; }
        public DbSet<PayrollPeriodEmployee> PayrollPeriodEmployees { get; set; }
        public DbSet<PayrollPeriodPayAdjustment> PayrollPeriodPayAdjustments { get; set; }
        public DbSet<PayrollPeriodPayAdjustmentFieldValue> PayrollPeriodPayAdjustmentFieldValues { get; set; }


        public DbSet<Request> Requests { get; set; }
        public DbSet<RequestDataChange> RequestDataChanges { get; set; }
        public DbSet<Schedule> Schedules { get; set; }


        public DbSet<VariationKeyValue> VariationKeyValues { get; set; }


        public DbSet<KpiValue> KpiValues { get; set; }
        public DbSet<Visa> Visas { get; set; }


        public DbSet<Work> Works { get; set; }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<WorkItemSubmission> WorkItemSubmissions { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // enable auto history functionality.
            builder.EnableAutoHistory(int .MaxValue);

            //builder.Entity<Employee>().Property(e => e.Gender).HasConversion(
            //v => v.ToString(),
            //v => (Gender)Enum.Parse(typeof(Gender), v));

            ////builder.Entity<Employee>().Property(e => e.HrStatus)
            ////.HasConversion(v => v.ToString(),
            ////v => (HrStatus)Enum.Parse(typeof(HrStatus), v));

            //builder.Entity<Employee>().Property(e => e.IdentityType).HasConversion(
            //v => v.ToString(),
            //v => (IdentityType)Enum.Parse(typeof(IdentityType), v));

            //builder.Entity<Company>().Property(e => e.WorkType)
            //.HasConversion(
            //v => v.ToString(),
            //v => (CompanyWorkType)Enum.Parse(typeof(CompanyWorkType), v));

            ////builder.Entity<Company>().Property(e => e.Status)
            ////.HasConversion(v => v.ToString(),
            ////v => (CompanyStatus)Enum.Parse(typeof(CompanyStatus), v));

            //builder.Entity<Attendance>().Property(e => e.CurrentStatus).HasConversion(
            //v => v.ToString(),
            //v => (AttendanceStatus)Enum.Parse(typeof(AttendanceStatus), v));
            //builder.Entity<Schedule>().Property(e => e.ScheduleFor).HasConversion(
            //v => v.ToString(),
            //v => (ScheduleFor)Enum.Parse(typeof(ScheduleFor), v));

            //builder.Entity<BackgroundJob>().Property(e => e.TaskType)
            //.HasConversion(
            //v => v.ToString(),
            //v => (TaskType)Enum.Parse(typeof(TaskType), v));


            //builder.Entity<Request>().Property(e => e.RequestType)
            //.HasConversion(
            //v => v.ToString(),
            //v => (RequestType)Enum.Parse(typeof(RequestType), v));

            //builder.Entity<Request>().Property(e => e.Status)
            //.HasConversion(
            //v => v.ToString(),
            //v => (WorkItemStatus)Enum.Parse(typeof(WorkItemStatus), v));

            //builder.Entity<FileData>().Property(e => e.FileType)
            //.HasConversion(
            //v => v.ToString(),
            //v => (FileType)Enum.Parse(typeof(FileType), v));


            builder.Entity<EmployeeRoleRelation>().HasKey(a => new { a.EmployeeId, a.EmployeeRoleId });

            builder.ApplyConfiguration(new PayrollPeriodEmployeeConfiguration());
            builder.ApplyConfiguration(new PayrollPeriodConfiguration());
            builder.ApplyConfiguration(new AnnouncementConfiguration());
            builder.ApplyConfiguration(new ScheduleConfiguration());
            builder.ApplyConfiguration(new AttendanceConfiguration());
            builder.ApplyConfiguration(new RequestConfiguration());
            builder.ApplyConfiguration(new CompanyFileConfiguration());
            builder.ApplyConfiguration(new CompanyFileSharesConfiguration());
            builder.ApplyConfiguration(new JobConfiguration());
            builder.ApplyConfiguration(new EmployeeRoleConfiguration());
            builder.ApplyConfiguration(new DayOffConfiguration());
            builder.ApplyConfiguration(new EmployeeInteractionConfiguration());

            // Unable to determine the relationship represented by navigation property 'Attendance.Employee' of type 'Employee'. Either manually configure the relationship, or ignore this property using the '[NotMapped]' attribute or by using 'EntityTypeBuilder.Ignore' in 'OnModelCreating'.
            //builder.Entity<Attendance>().HasOne(d => d.Employee).WithMany(d => d.Attendances).OnDelete(DeleteBehavior.Restrict);

            // Unable to determine the relationship represented by navigation property 'Employee.Requests' of type 'List<Request
            builder.Entity<Employee>().HasMany(d => d.Requests).WithOne(d => d.Employee).HasForeignKey(x=> x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Request>().HasOne(d => d.Employee).WithMany(d => d.Requests).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);


            //builder.Entity<Team>().HasMany(d => d.Employees).WithOne(d => d.Team).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
            //builder.Entity<Employee>().HasOne(d => d.Team).WithMany(d => d.Employees).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);


            // UUnable to determine the relationship represented by navigation property 'Employee.JobActionHistories' of type 'List<JobActionHistory>'.
            builder.Entity<Employee>().HasMany(d => d.JobActionHistories).WithOne(d => d.Employee).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<JobActionHistory>().HasOne(d => d.Employee).WithMany(d => d.JobActionHistories).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);

            //The ALTER TABLE statement conflicted with the FOREIGN KEY SAME TABLE constraint "FK_Schedules_Schedules_ParentScheduleId".The conflict occurred in database "Payroll.Master_v2", table "dbo.Schedules", column
            builder.Entity<Schedule>()
                        .HasOne(x => x.ParentSchedule)
                        .WithMany(x => x.FololwingSchedules)
                        .HasForeignKey(x => x.ParentScheduleId);
            

            //Unable to determine the relationship represented by navigation property 'Employee.EmployeeJobInfos' of type 'List<EmployeeJobInfo>'
            builder.Entity<Employment>()
                        .HasOne(x => x.Employee)
                        .WithMany(x => x.Employments)
                        .HasForeignKey(x => x.EmployeeId);
            builder.Entity<Employee>()
                        .HasOne(x => x.ReportingEmployee)
                        .WithMany(x => x.EmployeeDirectReports)
                        .HasForeignKey(x => x.ReportingEmployeeId);


            builder.Entity<EmployeeAction>()
                .HasOne(x => x.Employee)
                .WithMany(x => x.EmployeeActions)
                .HasForeignKey(x => x.EmployeeId);
            builder.Entity<EmployeeAction>()
                        .HasOne(x => x.ReportingEmployee)
                        .WithMany(x => x.EmployeeActionDirectReports)
                        .HasForeignKey(x => x.ReportingEmployeeId);

            //// (sql FK error)
            //// ALTER TABLE [KpiValues] ADD CONSTRAINT [FK_KpiValues_PayrollPeriodEmployees_PayrollPeriodEmployeeId] FOREIGN KEY ([PayrollPeriodEmployeeId]) REFERENCES [PayrollPeriodEmployees] ([Id]) ON DELETE NO ACTION;
            //builder.Entity<KpiValue>()
            //            .HasOne(x => x.PayrollPeriodEmployee)
            //            .WithMany(x => x.KpiValues)
            //            .OnDelete(DeleteBehavior.Restrict);
            //builder.Entity<PayrollPeriodEmployee>()
            //            .HasMany(x => x.KpiValues)
            //            .WithOne(x => x.PayrollPeriodEmployee)
            //            .OnDelete(DeleteBehavior.Cascade);


            //builder.Entity<EmployeeInteraction>().HasOne(d => d.Employee).WithMany(d => d.EmployeeInteractions).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            //builder.Entity<EmployeeInteraction>().HasOne(d => d.Request).WithMany(d => d.EmployeeInteractions).HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Restrict);

            //////Unable to determine the relationship represented by navigation property 'Attendance.Employee' of type 'Employee
            //builder.Entity<Attendance>().HasOne(d => d.Employee).WithMany(d => d.Attendances).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            //builder.Entity<Request>().HasOne(d => d.Attendance).WithMany(d => d.Requests).HasForeignKey(x => x.AttendanceId).OnDelete(DeleteBehavior.Restrict);

            //builder.Entity<Employee>().HasMany(d => d.Attendances).WithOne(d => d.Employee).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);

            //builder.Entity<Request>().HasOne(d => d.Employee).WithMany(d => d.Requests).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            //builder.Entity<Employee>().HasMany(d => d.Requests).WithOne(d => d.Employee).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);


            //builder.Entity<Schedule>().Property(p => p.IgnoreDaysData).HasConversion(
            //v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
            //v => JsonConvert.DeserializeObject<IList<int>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            //builder.Entity<Department>().HasOne(d => d.LineManager);
            //builder.Entity<Department>().HasMany(d => d.Employees).WithOne(t => t.Department).OnDelete(DeleteBehavior.Restrict);


            //builder.Entity<Department>().HasMany(d => d.Schedules).WithOne(d => d.Department).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            //builder.Entity<Schedule>().HasOne(d => d.Department).WithMany(d => d.Schedules).HasForeignKey(x => x.Department).OnDelete(DeleteBehavior.Restrict);

            // FK_Schedules_Departments_DepartmentId
            //builder.Entity<Schedule>().HasOne(d => d.Department).WithMany(t => t.Schedules).OnDelete(DeleteBehavior.Restrict);



            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(x => x.GetForeignKeys()))
                relationship.DeleteBehavior = DeleteBehavior.Restrict;

            // The child/dependent side could not be determined for the one-to-one relationship between 'BackgroundJob.Schedule' and 'Schedule.BackgroundJob'. To identify the child/dependent
            //builder.Entity<Schedule>().HasOne(d => d.BackgroundJob).WithOne(d => d.Schedule).OnDelete(DeleteBehavior.Restrict); 


            // builder.Entity<PayrollPeriod>().HasMany(x => x.PayrollPeriodEmployees).WithOne(x => x.PayrollPeriod).OnDelete(DeleteBehavior.Cascade);

            foreach (var tp in builder.Model.GetEntityTypes().ToList())
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


        private readonly MethodInfo SetAuditingShadowPropertiesMethodInfo = typeof(ChangeTrackerExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(t => t.IsGenericMethod && t.Name == "SetAuditingShadowProperties");

        private readonly MethodInfo SetIsDeletedShadowPropertyMethodInfo = typeof(ChangeTrackerExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(t => t.IsGenericMethod && t.Name == "SetIsDeletedShadowProperty");

        private readonly MethodInfo SetGlobalQueryForSoftDeleteMethodInfo = typeof(ChangeTrackerExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(t => t.IsGenericMethod && t.Name == "SetGlobalQueryForSoftDelete");

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            ChangeTracker.SetShadowProperties(userResolverService);
            ChangeTracker.AddAuditEntries(userResolverService, this);

            var result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }

        public override int SaveChanges()
        {
            ChangeTracker.SetShadowProperties(userResolverService);
            ChangeTracker.AddAuditEntries(userResolverService, this);

            return base.SaveChanges();
        }
    }
}
