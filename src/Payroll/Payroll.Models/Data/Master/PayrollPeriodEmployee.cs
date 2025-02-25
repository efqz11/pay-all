using Newtonsoft.Json;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    /// <summary>
    /// AUTO GENERATED 2D view of employees by variations (CALCULATED)
    /// </summary>
    public class PayrollPeriodEmployee : Audit
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string EmpID { get; set; }
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        public int PayrollPeriodId { get; set; }
        public virtual PayrollPeriod PayrollPeriod { get; set; }



        public decimal GrossPay { get; set; }
        public decimal NetSalary { get; set; }

        
        /// <summary>
        /// Aggregated Addition and Deduction Values
        /// </summary>

        public decimal BasicSalary { get; set; }
        public string Designation { get; set; }

        /// <summary>
        /// List of all PayAdjustment and Fields
        /// </summary>
        public virtual List<VariationKeyValue> VariationKeyValues { get; set; }
        public virtual List<KpiValue> KpiValues { get; set; }

        //public string Dpartment { get; set; }


        // ANALYTICS FOR THE PAYROLL PERIOD
        public double WorkedHours { get; set; }
        public double WorkedMins { get; set; }

        public double LateMins { get; set; }
        public double LateHours { get; set; }
        public int LateDays { get; set; }

        //public double AbsentMins { get; set; }
        //public double AbsentHours { get; set; }
        public int AbsentDays { get; set; }


        public double OvertimeHours { get; set; }
        public double OvertimeMins { get; set; }

        public double LeaveDays { get; set; }

        public double TaskCompletedCount { get; set; }
        public double TaskFailedCount { get; set; }
        public decimal TaskCreditSum { get; set; }
        public decimal TaskDebitSum { get; set; }


        public int OvertimeCount { get;  set; }
        public int WorkedRecordsCount { get; set; }

        public int TaskSubmissionsCount { get; set; }
        public int TaskRemainingCount { get; set; }
        
        public IList<ChartDataX> ChartDataX  { get; set; }

        // Grade generated for pay period
        public string Grade { get;  set; }
        public decimal Percent { get;  set; }
        public string PercentStr { get;  set; }

        //[JsonIgnore]
        public string CssClass => "_" + Grade?.ToLower();
        public DateTime? GradeGeneratedDateTime { get;  set; }
        public bool IsGraded { get;  set; }

        public PayrollPeriodEmployee()
        {
            VariationKeyValues = new List<VariationKeyValue>();
            KpiValues = new List<KpiValue>();
        }
    }

    public class ChartDataX
    {
        public int EmployeeId { get; set; }
        public int PayrollPeriodEmployeeId { get; set; }

        public DateTime Date { get; set; }
        public string DateString { get; set; }
        public double ActualWorkedHours { get; set; }
        public double ActualWorkedHoursPerSchedule { get; set; }
        public int LateEmployeeCount { get; set; }
        public double OvertimeWorkedHoursPerSchedule { get;  set; }
        public int TotalAbsentCount { get; set; }
        public int TotalLateDays { get; set; }
        public int TotalLateHours { get; set; }
        public double TotalLateMins { get; set; }
        public double TotalScheduledHours { get; set; }
    }
}

