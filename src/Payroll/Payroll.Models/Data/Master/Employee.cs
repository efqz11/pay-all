
using Payroll.Filters;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Payroll.Models
{
    [AuditableEntity]
    public partial class Employee : Audit, IHasCompanyReference
    {
        //[NotMapped]
        //public int index { get; set; }

        [Display(Name = "Company")]
        public int CompanyId { get; set; }
        public Company Company { get; set; }


        //[Display(Name = "Individual")]
        //public int IndividualId { get; set; }
        //public Individual Individual { get; set; }

        public int Id { get; set; }


        // fields used to track performance after every payroll
        public decimal Percent { get; set; }
        public string Grade { get; set; }
        public string CssClass { get; set; }
        public string PercentStr { get; set; }
        public List<KpiValue> KpiValues { get; set; }

        
        // auto updating fields
        public DateTime? NextVisaExpiryDate { get; set; }
        public DateTime? LastPromotedDate { get; set; }
        public DateTime? LastContractEndedDate { get; set; }


        //[ForeignKey("AppUser")]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserPicture { get; set; }


        #region HR Data
        // COPIED FROM INDIVIDUAL :D


        /// 1. EmployeeTypes, 2.EmployeeJobInfos
        // public string JobTitle { get; set; }
        // public string JobIDString { get; set; }
        // public JobType? JobType { get; set; }
        //public string DepartmentName { get; set; }
        //public string DivisionName { get; set; }
        //public string LocationName { get; set; }



        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        [Display(Name = "Division")]
        public int? DivisionId { get; set; }
        public Division Division { get; set; }

        [Display(Name = "Location")]
        public int? LocationId { get; set; }
        public Location Location { get; set; }


        [Display(Name = "Job")]
        public int? JobId { get; set; }
        public Job Job { get; set; }


        // auto updating HR info  (easy access)

        [Display(Name = "Location")]
        public string LocationName { get; set; }
        [Display(Name = "Department")]
        public string DepartmentName { get; set; }
        [Display(Name = "Division")]
        public string DivisionName { get; set; }

        public string JobTitle { get; set; }
        public string JobIDString { get; set; }
        public JobType? JobType { get; set; }
        [NotMapped]
        public String Level { get; set; }



        #endregion

        #region AUTO UPDATABLE Fields (Personal)

        // moved to individual

        #endregion

        #region HR Information

        [Display(Name = "Work Email")]
        [SelectableField]
        public string EmailWork { get; set; }

        // Contact
        [Display(Name = "Work Phone")]
        [SelectableField]
        public string PhoneWork { get; set; }


        [Display(Name = "Work Extension")]
        [SelectableField]
        [MaxLength(5)]
        public string PhoneWorkExt { get; set; }

        public decimal BasicSalary { get; set; }


        [SelectableField]
        [Display(Name = "Date of Joining")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy")]
        [DataType(DataType.Date)]
        public DateTime? DateOfJoined { get; set; }

        public LengthOfProbation LengthOfProbation { get; set; }
        public DateTime? ProbationStartDate { get; set; }

        [Display(Name = "End date of Probation")]
        public DateTime? ProbationEndDate { get; set; }


        public DateTime? DateRegistered { get; set; }
        public DateTime? DateExpiry { get; set; }
        public DateTime? DateCancelled { get; set; }

        [SelectableField]
        [Display(Name = "Termination Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy")]
        [DataType(DataType.Date)]
        public DateTime? DateOfTermination { get; set; }
        public DateTime? NoticeDate { get; set; }
        public DateTime? LastDayAtWork { get; set; }

        [Display(Name = "Start date of Contract")]    
        public DateTime? ContractStartDate { get; set; }
        
        [Display(Name = "End date of Contract")]
        public DateTime? ContractEndDate { get; set; }

        [Display(Name = "No. of Days worked (weekly)")]
        public int WeeklyWorkingHours { get; set; }
        public int DailyWorkingHours { get; set; }
        public int DaysWorkingInWeek { get; set; }
        
        public int IsDaysWorkingInWeekFlexible { get; set; }



        [Phone]
        [Display(Name = "Tax File Number")]
        public string TaxFileNumber { get; set; }

        [DataType(DataType.MultilineText)]

        #endregion

        public HrStatus HrStatus { get; set; }
        public string StatusCss => GetStatusStyle();

        private string GetStatusStyle()
        {
            switch (HrStatus)
            {
                case HrStatus.Recruitement:
                    return "is-primary is-light";
                    break;
                case HrStatus.Employed:
                    return "is-success is-light";
                    break;
                case HrStatus.Probation:
                    return "is-warning is-light";
                    break;
                case HrStatus.Permanant:
                    return "is-info is-light";
                    break;
                case HrStatus.Suspended:
                    return "is-danger is-light";
                    break;
                case HrStatus.Terminated:
                    return "is-danger";
                    break;
                default:
                    return "";
                    break;
            }
        }

        [NotMapped]
        public virtual AppUser AppUser { get; set; }

        public bool HasUserAccount { get; set; }
        public bool IsResponsibleForPensionCharges { get; set; }
        public bool IsResponsibleForAccounts { get; set; }

        public string NickName { get; set; }
        public string NameDisplay => !string.IsNullOrWhiteSpace(NickName) ? NickName : FirstName;


        //public string HangfireJobId { get; set; }
        //public int? BackgroundJobId { get; set; }

        //[NotMapped]
        //public virtual BackgroundJob BackgroundJob { get; set; }

        [NotMapped]
        public string Name { get; set; }


        [SelectableField]
        public string EmpID { get; set; }


        [NotMapped]
        [Display(Name = "Street Address")]
        public string Street { get; set; }
        [NotMapped]
        public int? CityId { get; set; }
        [NotMapped]
        public int? StateId { get; set; }
        [NotMapped]
        public string ZipCode { get; set; }

        public string DisplayOrder { get; set; }

        //[SelectableField]
        //public string JobType { get; set; }


        //public decimal BasicSalary { get; set; }

        //public decimal PhoneAllowance { get; set; }


        //public string Avatar => Individual?.Avatar ?? "";


        public virtual List<EmployeeTeam> EmployeeTeams { get; set; }
        public virtual List<BankDetail> BankDetails { get; set; }
        public virtual List<EmergencyContact> EmergencyContacts { get; set; }
        public virtual List<PayrollPeriodEmployee> PayrollPeriodEmployees { get; set; }
        public virtual List<DayOffEmployee> DayOffEmployees { get; set; }
        public virtual List<Attendance> Attendances { get; set; }
        public virtual List<WorkItem> WorkItems { get; set; }
        public virtual List<Request> Requests { get; set; }
        public bool? HasBackgroundJob { get; set; }
        //public int? JobId { get; set; }
        public string BackgroundJobDetails { get; set; }
        public bool bySchduler { get; set; }
        public DateTime? NextRunDate { get; set; }
        public bool? HasBackgroundJobEnded { get; set; }
        public List<FileData> FileDatas { get; set; }
        public List<DepartmentHead> DepartmentHeads { get; set; }

        //public object Gender { get; set; }


        [ForeignKey("ReportingEmployee")]
        public int? ReportingEmployeeId { get; set; }
        public Employee ReportingEmployee { get; set; }



        //public List<EmployeeType> EmployeeTypes { get; set; }
        public List<Employment> Employments { get; set; }
        public List<Employee> EmployeeDirectReports { get; set; }
        public List<EmployeePayComponent> EmployeePayComponents { get; set; }
        public List<EmployeeAction> EmployeeActions { get; set; }
        public List<JobActionHistory> JobActionHistories { get; set; }
        public List<EmployeeAction> EmployeeActionDirectReports { get; set; }
        public List<EmployeeRoleRelation> EmployeeRoles { get; set; }


        //public int? EmergencyContactRelationshipId { get; set; }
        //public EmergencyContactRelationship EmergencyContactRelationship { get; set; }

        public EmployeeStatus EmployeeStatus { get; set; }
        public bool IsIncomplete => EmployeeStatus == EmployeeStatus.Incomplete;
        public bool IsSelfOnBoarding => EmployeeStatus == EmployeeStatus.ActionNeeded && EmployeeSecondaryStatus == EmployeeSecondaryStatus.WaitingSelfOnBoard;
        public EmployeeSecondaryStatus EmployeeSecondaryStatus { get; set; }
        public EmployeeType EmploymentType { get; set; }
        public EmploymentTypeOther EmploymentTypeOther { get; set; }
        public RecordStatus RecordStatus { get; set; }
        // end



        /// Calcualtable fields
        [NotMapped]
        public bool UpdateFromJobPayComponents { get; set; }
        [NotMapped]
        public double _lateMins { get; set; }
        [NotMapped]
        public double _otHrs { get; set; }
        [NotMapped]
        public int _lateDays { get; set; }
        [NotMapped]
        public int _AbsentDays { get; set; }
        [NotMapped]
        public double _totalWorkedHrsPerSchedule { get; set; }
        [NotMapped]
        public double _totalWorkedHrs { get; set; }
        [NotMapped]
        public decimal _submissionOverallCompletionPercent { get; set; }

        [NotMapped]
        public decimal _clockTasksOverallCompletionPercent { get; set; }
        [NotMapped]
        public AttendanceStatus CurrentStatus { get; set; }
        [NotMapped]
        public bool HasProbationEndDate { get; set; }
        [NotMapped]
        public bool IsCreateNewJob { get; set; }



        public string GetDuration(ClaimsPrincipal user)
        {
            var _st = ContractStartDate ?? DateOfJoined.GetValueOrDefault();
            return _st.GetDuration(ContractEndDate, user, false);
        }
        public DateTime GetEndDate() =>
            LastContractEndedDate ?? ContractEndDate.GetValueOrDefault();
        public Employee()
        {
            EmergencyContacts = new List<EmergencyContact>();
            BankDetails = new List<BankDetail>();
            EmployeeTeams = new List<EmployeeTeam>();
            //EmployeeTypes = new List<EmployeeType>();
            EmployeeRoles = new List<EmployeeRoleRelation>();
            Employments = new List<Employment>();
            EmployeeActions = new List<EmployeeAction>();
            EmployeeActionDirectReports = new List<EmployeeAction>();
            EmployeeDirectReports = new List<Employee>();
            EmployeePayComponents = new List<EmployeePayComponent>();
            Avatar = "~/img/default-image.png";

            DepartmentHeads = new List<DepartmentHead>();
            PayrollPeriodEmployees = new List<PayrollPeriodEmployee>();
            DayOffEmployees = new List<DayOffEmployee>();
            Attendances = new List<Attendance>();
            FileDatas = new List<FileData>();
            Requests = new List<Request>();
            KpiValues = new List<KpiValue>();
            //Individual = new Individual();

            WorkItems = new List<WorkItem>();
            JobActionHistories = new List<JobActionHistory>();
        }
    }
}
