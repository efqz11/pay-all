using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class CompanyAccount : Audit
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [ForeignKey("AppUserOwner")]
        public string UserId { get; set; }
        public virtual AppUser AppUserOwner { get; set; }

        public int? CountryId { get; set; }
        public virtual Country Country { get; set; }

        public int? IndustryId { get; set; }
        public virtual Industry Industry { get; set; }



        public CompanyStatus Status { get; set; }

        public string CompanyRegistrationNo { get; set; }
        public double ComplectionPercent { get; set; }

        [Display(Name = "When to Apply Paid Time-offs after joining")]
        public WhenToApplyPaidTimeOffPolicyAfterJoining WhenToApplyPaidTimeOffPolicyAfterJoining { get; set; }

        //public int EarlyOntimeMinutes { get; set; }

        //[NotMapped]
        //public CompanyWorkTime[] CompanyWorkTimes
        //{
        //    get { return _CompanyWorkTimes == null ? null : JsonConvert.DeserializeObject<CompanyWorkTime[]>(_CompanyWorkTimes); }
        //    set { _CompanyWorkTimes = JsonConvert.SerializeObject(value); }
        //}

        //[Required]
        public string Address { get; set; }

        [NotMapped]
        public string Street1 { get; set; }
        [NotMapped]
        public string Street2 { get; set; }
        [NotMapped]
        public string City { get; set; }
        [NotMapped]
        public string Phone { get; set; }
        [NotMapped]
        public string State { get; set; }
        [NotMapped]
        public string Zip { get; set; }

        [Filters.NotificaitonAvatar]
        public string LogoUrl { get; set; }
        public string TimeZone { get; set; }

        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
        public string CurrencyFormat { get; set; }
        public string NameFormat { get; set; }


        public string CurrencySymbol { get; set; }

        public string Website { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        
        public string Hotline { get; set; }

        public string TaxCode { get; set; }

        public decimal? TaxPercentValue { get; set; }

        public string ManagingDirector { get; set; }


        public int? PayrolPeriodStartDate { get; set; }
        public int? PayrolPeriodEndDate { get; set; }
        public int? PayrolPeriodPayDate { get; set; }

        /// <summary>
        /// After End Date, how many days after will payrol be closed, No more approvals
        /// all are marked as closed -> start Payroll Processing towards Pay Date
        /// </summary>
        public int? PayrolPeriodClosingDateHDaysAfterEndDate { get; set; }
        /// <summary>
        /// After Close Date, how many days to actual Pay Date
        /// </summary>
        public int? PayrolPeriodPayDateHDaysAfterCloseDate { get; set; }

        public bool? IsEndDateOnNextMonth { get; set; }
        public bool IsConfigured => PayrolPeriodStartDate.HasValue && PayrolPeriodStartDate > 0 && PayrolPeriodEndDate.HasValue && PayrolPeriodEndDate > 0;
        public bool? IncludeStartDate { get; set; }
        public bool? IncludeEndDate { get; set; }
        public DayOfWeek WeekStartDay { get; set; }


        public int? PayrolPeriodDays { get; set; }


        ///// <summary>
        /////  connection string details
        ///// </summary>
        //public string InitialCatalog { get; set; }
        //public string DataSource { get; set; }
        //public string UserId { get; set; }
        //public string Password { get; set; }
        //public bool? IntegratedSecurity { get; set; }

        //[Display(Name= "Connection string name")]
        //public string ConfigConnectionStringName { get; set; }

        //public string CONNECTION_STRING { get; set; }
        //public bool ConnectionStatus { get; set; }
        
        //public CompanyStatus Status { get; set; }
        //public string CompanyStatusCssClass => Status == CompanyStatus.Approved ? "btn-success" :
        //    Status == CompanyStatus.Closed ? "btn-dark" :
        //    Status == CompanyStatus.Pending ? "btn-warning" : 
        //    Status == CompanyStatus.Rejected ? "btn-danger" : "";


        public string ApprovedById { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? ApprovedOnDate { get; set; }

        public int ProgressPercent { get; set; }
        public string NextStep { get; set; }


        //[Display(Name = "Allowed file types (csv)")]
        //public string AllowedFileTypes { get; set; }

        //[Display(Name = "Allowed image types (csv)")]
        //public string AllowedImageTypes { get; set; }
        //public string MaxFileUploadSizeMb { get; set; }
        //public string MaxImageUploadSizeMb { get; set; }

        public Dictionary<int, bool> ProgressBySteps { get; set; }
        public decimal MaxFileStorageSize { get; set; }

        public virtual List<AccessGrant> AccessGrants { get; set; }
        public virtual List<RequestProcessConfig> RequestProcessConfigs { get; set; }
        public virtual List<Notification> Notifications { get; set; }
        public CompanyAccount()
        {
            AccessGrants = new List<AccessGrant>();
            RequestProcessConfigs = new List<RequestProcessConfig>();
            Notifications = new List<Notification>();
        }

        public IList<int> DayOfWeekHolidays { get; set; }
        public int WorkingDaysCount => 7 - (DayOfWeekHolidays?.Count ?? 0);
        public IList<int> DayOfWeekOffDays { get; set; }

        public bool IsKpiConfigured { get; set; }
        public virtual IList<KpiConfig> KpiConfig { get; set; }

    }


    //public class CompanyWorkTypeConfig
    //{
    //    public TimeSpan StartTime { get; set; }
    //    public TimeSpan EndTime { get; set; }
    //    public CompanyWorkType CompanyWorkType { get; set; }

    //    public int TotakBreaks { get; set; }
    //    public int TotakWorkTimes { get; set; }
    //    public List<CompanyWorkTime> CompanyWorkTimes { get; set; }
    //}

    //public class CompanyWorkTime
    //{
    //    public string ShiftName { get; set; }
    //    public TimeSpan StartTime { get; set; }
    //    public TimeSpan EndTime { get; set; }


    //    public int TotakBreaks { get; set; }
    //    public List<CompanyBreakTime> BreakTime { get; set; }
    //}

    //public class CompanyBreakTime
    //{
    //    public TimeSpan StartTime { get; set; }
    //    public TimeSpan EndTime { get; set; }
    //    public bool IsFlexible { get; set; }

    //    public int TotakBreaks { get; set; }
    //    public int MyProperty { get; set; }
    //}
}
