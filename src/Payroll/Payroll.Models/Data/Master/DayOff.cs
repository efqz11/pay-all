using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class DayOff : Audit
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
       
        // leave
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Total no. of hours per year
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalPerYear { get; set; }



        public bool CanRequestForBackDatedDays { get; set; }

        public bool MustRequestBefore { get; set; }
        public AlertType? MustRequestBeforeAlert { get; set; }

        public bool ExcludeForPublicHoliday { get; set; }

        [Display(Name = "Consider time tracked during absences of this type as overtime?")]
        public bool ConsiderTimeTrackedAsOvertime { get; set; }

        public bool RequiredDocumentForConseqetiveDays { get; set; }
        public int ConsquetiveDaysRequire { get; set; }


        public bool RequiredDocuments { get; set; }
        [RegularExpression("^([a-zA-Z0-9 ]+(?:,[A-Za-z0-9 ]+)*)$", ErrorMessage = "Please enter in CSV format")]
        public string RequiredDocumentList { get; set; }

        /// <summary>
        /// Needs to send document after
        /// </summary>
        public bool IsEmergency { get; set; }

        public bool IsForSpecificGender { get; set; }
        public Gender? Gender { get; set; }

        public bool IsByRequest { get; set; }

        [Display(Name= "Substitute required?")]
        public bool RequireSubstitiute { get; set; }

        [NotMapped]
        public RequireSubsitute RequireSubsituteEnum { get; set; }
        public bool RequireSubstitiuteOptional { get; set; }

        [Display(Name = "Can available balance hours go negative?")]
        public bool CanOverBookHours { get; set; }

        public string Color { get; set; }

        //// Accrural policy definition 
        //[Display(Name = "Enable accrual policies? *")]
        //public bool EnableAccruralPolicy { get; set; }
        //[Display(Name = "Accrual carryover from previous year *")]
        //public AccruralCarryoverFromPreviousYear AccruralCarryoverFromPreviousYear { get; set; }

        //[Display(Name = "Grant entitlement every")]
        //public DayOffGrantEntititlementEvery DayOffGrantEntititlementEvery { get; set; }
        public DayOffGrantEntititlementAt DayOffGrantEntititlementAt { get; set; }

        //public bool EnableWaitingTime { get; set; }
        //public int EntitilementGrantedDuringWaitingTime { get; set; }

        //public DayOffProrateClcualtationTimeFrame BeginingOfEmployment { get; set; }
        //public DayOffProrateClcualtationTimeFrame EndOfEmployment { get; set; }

        //public bool GrantExtraDaysBasedOnTimeOfEmployment { get; set; }
        public IDictionary<int, int> ExtraDaysAfter { get; set; }
        //public bool GrantExtraDayWithStartOfNextEntitlementPeriod { get; set; }

        // END: Accrural policy definition 
        public bool IsThereLimit { get; set; }
        public AccrualMethod? AccrualMethod { get; set; }

        // hourly (earned)
        public AccureTimeBasedOn? AccureTimeBasedOn { get; set; }
        public bool IsOvertimeCountedTowardsTimeOff { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? HoursEarned { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PerHoursWorked { get; set; }

        //fixed
        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalHoursPerYear { get; set; }

        public bool IsThereWaitingPeriodForAccrue { get; set; }
        [Display(Name = "Length of waiting period before accural")]
        public int? LengthWaitingPeriodForAccrue { get; set; }
        public bool IsThereWaitingPeriodForRequest { get; set; }
        [Display(Name = "Length of waiting period before you request")]
        public int? LengthWaitingPeriodForRequest { get; set; }




        [Display(Name = "Max hours accrued per year")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxAccuredHoursPerYear { get; set; }

        [Display(Name = "Max balance")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxBalance { get; set; }

        [Display(Name = "Is there a limit to the number of hours your employees can carry over from year to year?")]
        public bool IsThereCarryOverLimit { get; set; }
        [Display(Name = "Max Carryover ")]
        public decimal? CarryOverLimit { get; set; }

        [Display(Name = "If employees have an outstanding balance, should they be paid out upon dismissal?")]
        public bool IsOutstandingBalancePaidUponDismissial { get; set; }


        //public bool IsEnteredManually { get; set; }

        //public bool ResetEveryYear { get; set; }
        ////public bool ResetAnnuallyFromJoinDate { get; set; }

        //public bool IsApplicableOnProbation { get; set; }
        //public bool IsApplicationAfterFirstYear { get; set; }
        public bool CanPlanAhead { get; set; }

        //public IList<string> ReasonsToRequstChange { get; set; }
        public List<DayOffEmployee> DayOffEmployees { get; set; }


        public DayOff()
        {
            DayOffEmployees = new List<DayOffEmployee>();
        }
    }
}
