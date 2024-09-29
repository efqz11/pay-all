using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Payroll.Model.Logs;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace Payroll.Database
{
    public class LogDbContext : DbContext
    {
       
        public LogDbContext(DbContextOptions<LogDbContext> options)
            : base(options)
        {

        }

        public DbSet<ApplicationLog> ApplicationLogs { get; set; }

        public DbSet<EventDataType> EventDataTypes { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<EventLogType> EventLogTypes { get; set; }
        public DbSet<EventResult> EventResults { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);



            builder.Entity<EventLog>().Property(e => e.AffectedEmployeeIds).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<string[]>(v));
            builder.Entity<EventLog>().Property(e => e.ActionDetails).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<string[]>(v));
        }
    }
}
