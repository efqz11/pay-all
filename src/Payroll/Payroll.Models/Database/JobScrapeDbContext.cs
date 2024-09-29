using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Payroll.Models;
using Payroll.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace Payroll.Database
{
    public class JobScrapeDbContext : DbContext
    {
        private readonly IConfiguration _config;
        private readonly UserResolverService _userResolverService;

        public JobScrapeDbContext()
        {
        }

        public JobScrapeDbContext(DbContextOptions<JobScrapeDbContext> options, UserResolverService userResolverService)
            : base(options)
        {
            this._userResolverService = userResolverService;
        }


        public DbSet<Models.JobScrape.Jobsicle_Company> Jobsicle_Companies { get; set; }
        public DbSet<Models.JobScrape.Jobsicle_Job> Jobsicle_Jobs { get; set; }
        public DbSet<Models.JobScrape.Jobsicle_OfficeHour> Jobsicle_OfficeHours { get; set; }
        public DbSet<Models.JobScrape.Jobsicle_Rating> Jobsicle_Ratings { get; set; }
        public DbSet<Models.JobScrape.Jobsicle_Photo> Jobsicle_Photos { get; set; }


        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(_config.GetConnectionString("AccountConnection"))
        //        .EnableSensitiveDataLogging();
        //}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);



            builder.Entity<Models.JobScrape.Jobsicle_Job>()
                .Property(e => e.required_items).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IList<string>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            builder.Entity<Models.JobScrape.Jobsicle_Job>()
                .Property(e => e.questions).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IList<Models.JobScrape.Jobsicle_Question>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));


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


    public class CustomDateTimeConverter : DateTimeConverterBase
    {
        // created_at": "2019-11-24 14:36:19",
        // 03 Dec, 2019
        private const string Format = "yyyy-MM-dd HH:mm:ss";
        private const string Format1 = "dd MMM, yyyy";


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DateTime)value).ToString(Format));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }

            var s = reader.Value.ToString();
            DateTime result;
            if (DateTime.TryParseExact(s, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }
            if (DateTime.TryParseExact(s, Format1, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            return null;
        }
    }

}
