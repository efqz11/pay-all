using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Payroll.Models;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Database
{

    #region Account DB Models
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.Property(e => e.NotificationActionTakenType).HasConversion(
                v => v.ToString(),
                v => (NotificationActionTakenType)Enum.Parse(typeof(NotificationActionTakenType), v));
            builder.Property(e => e.EmployeesWithRoles).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<string[]>(v));
        }
    }


    public class NotificationTypeConfiguration : IEntityTypeConfiguration<NotificationType>
    {
        public void Configure(EntityTypeBuilder<NotificationType> builder)
        {
            //builder.Property(e => e.AllowedActions).HasConversion(
            //    v => JsonConvert.SerializeObject(v),
            //    v => JsonConvert.DeserializeObject<string[]>(v));

            builder.Property(e => e.UsersWithRoles).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<string[]>(v));

            builder.Property(e => e.NotificationLevel).HasConversion(
                v => v.ToString(),
                v => (UserType)Enum.Parse(typeof(UserType), v));


            builder.Property(e => e.EntityType).HasConversion(
                v => v.ToString(),
                v => (EntityType)Enum.Parse(typeof(EntityType), v));

            builder.Property(e => e.NotificationReceivedBy).HasConversion(
                v => v.ToString(),
                v => (NotificationReceivedBy)Enum.Parse(typeof(NotificationReceivedBy), v));
        }
    }



    public class EmployeeInteractionConfiguration : IEntityTypeConfiguration<EmployeeInteraction>
    {
        public void Configure(EntityTypeBuilder<EmployeeInteraction> builder)
        {
            //builder.Property(e => e.AllowedActions).HasConversion(
            //    v => JsonConvert.SerializeObject(v),
            //    v => JsonConvert.DeserializeObject<string[]>(v));

            builder.Property(e => e.EmployeesWithRoles).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<string[]>(v));

        }
    }


    public class ReportConfigConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.Property(e => e.Category).HasConversion(
                v => v.ToString(),
                v => (ReportCategory)Enum.Parse(typeof(ReportCategory), v));
        }
    }
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(e => e.SurveyCs_PayingToWhom).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<IList<PayingTo>>(v));
        }
    }

    public class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder.Property(e => e.DayOffIds).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<int[]>(v));
        }
    }
    public class EmployeeRoleConfiguration : IEntityTypeConfiguration<EmployeeRole>
    {
        public void Configure(EntityTypeBuilder<EmployeeRole> builder)
        {
            builder.Property(e => e.CalendarDefaults).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<IDictionary<string, EmployeeSelectorRole>>(v));

            builder.Property(e => e.UserAccessRights).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<IDictionary<string, string[]>>(v));
        }
    }
    public class DayOffConfiguration : IEntityTypeConfiguration<DayOff>
    {
        public void Configure(EntityTypeBuilder<DayOff> builder)
        {
            builder.Property(e => e.ExtraDaysAfter).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<IDictionary<int, int>>(v));
        }
    }
    public class RequestProcessConfigConfiguration : IEntityTypeConfiguration<RequestProcessConfig>
    {
        public void Configure(EntityTypeBuilder<RequestProcessConfig> builder)
        {
            builder.Property(e => e.ApprovalByEmployeeIds).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<int[]>(v));
        }
    }

    public class PayrollPeriodConfiguration : IEntityTypeConfiguration<PayrollPeriod>
    {
        public void Configure(EntityTypeBuilder<PayrollPeriod> builder)
        {
            builder.Property(e => e.Status).HasConversion(
                v => v.ToString(),
                v => (PayrollStatus)Enum.Parse(typeof(PayrollStatus), v));
        }
    }

    public class CompanyAccountConfiguration : IEntityTypeConfiguration<CompanyAccount>
    {
        public void Configure(EntityTypeBuilder<CompanyAccount> builder)
        {
            builder.Property(e => e.Status)
          .HasConversion(
          v => v.ToString(),
          v => (CompanyStatus)Enum.Parse(typeof(CompanyStatus), v));
            builder.Property(e => e.WeekStartDay)
            .HasConversion(
            v => v.ToString(),
            v => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), v));

            builder.Property(e => e.ProgressBySteps).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Dictionary<int, bool>>(v));

            builder.Property(e => e.DayOfWeekHolidays).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<IList<int>>(v));

            builder.Property(e => e.DayOfWeekOffDays).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<IList<int>>(v));

            builder.Property(e => e.KpiConfig).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<IList<KpiConfig>>(v));
        }
    }

    #endregion




    public class PayrollPeriodEmployeeConfiguration : IEntityTypeConfiguration<PayrollPeriodEmployee>
    {
        public void Configure(EntityTypeBuilder<PayrollPeriodEmployee> builder)
        {
            builder.Property(e => e.ChartDataX).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<IList<ChartDataX>>(v));
        }
    }



    public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
    {
        public void Configure(EntityTypeBuilder<Schedule> builder)
        {
            // This Converter will perform the conversion to and from Json to the desired type
            builder.Property(e => e.EmployeeIdsData).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IList<_Employee>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            builder.Property(e => e.WorkTimeIdsData).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IList<_WorkTime>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            // This Converter will perform the conversion to and from Json to the desired type
            builder.Property(e => e.IgnoreDaysData).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<IList<int>>(v));

            builder.Property(e => e.EmployeeIds).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<int[]>(v));

            builder.Property(e => e.WorkTimeIds).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<int[]>(v));


            builder.Property(e => e._Patten).HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<char[]>(v));


            // This Converter will perform the conversion to and from Json to the desired type
            builder.Property(e => e.DaysData).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IList<DayVm>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }
    }

    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            // This Converter will perform the conversion to and from Json to the desired type
            builder.Property(e => e.EmployeeIdsData).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IList<int>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            builder.Property(e => e.EmployeeSelectorVm).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<EmployeeSelectorVm>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }
    }

    public class BiometricRecordConfiguration : IEntityTypeConfiguration<BiometricRecord>
    {
        public void Configure(EntityTypeBuilder<BiometricRecord> builder)
        {
            // This Converter will perform the conversion to and from Json to the desired type
            builder.Property(e => e.BiometricRecordState).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<BiometricRecordState>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            builder.Property(e => e.BiometricRecordType).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<BiometricRecordType>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }
    }

    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            // This Converter will perform the conversion to and from Json to the desired type
            builder.Property(e => e.StatusUpdates).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IList<AttendanceStatusUpdate>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }
    }
    public class RequestConfiguration : IEntityTypeConfiguration<Request>
    {
        public void Configure(EntityTypeBuilder<Request> builder)
        {
            // StatusUpdates
            builder.Property(e => e.StatusUpdates).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IList<RequestStatusUpdate>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));


        }
    }


    public class CompanyFileConfiguration : IEntityTypeConfiguration<CompanyFile>
    {
        public void Configure(EntityTypeBuilder<CompanyFile> builder)
        {
            // This Converter will perform the conversion to and from Json to the desired type
            builder.Property(e => e.FillableConfiguration).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IList<FillableConfiguration>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }
    }



    public class CompanyFileSharesConfiguration : IEntityTypeConfiguration<CompanyFileShare>
    {
        public void Configure(EntityTypeBuilder<CompanyFileShare> builder)
        {
            // This Converter will perform the conversion to and from Json to the desired type
            builder.Property(e => e.FileConfigValues).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }
    }
}