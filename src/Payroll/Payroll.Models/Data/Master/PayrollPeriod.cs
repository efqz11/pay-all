using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Payroll.Models
{

    public class PayrollPeriod : Audit
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [DataType(DataType.MultilineText)]
        public string Summary { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public DateTime? PayDate { get; set; }
        public DateTime? LastApprovalSubmissionDate { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public virtual List<PayrollPeriodEmployee> PayrollPeriodEmployees { get; set; }
        public decimal NetSalary { get; set; }
        public decimal GrossPay { get; set; }
        public decimal NetSalaryLastPeriod { get; set; }
        public decimal GrossPayLastPeriod { get; set; }

        public PayrollStatus Status { get; set; }
        public bool IsGenerated { get; set; }

        public DateTime? LastGeneratedDateTime { get; set; }

        //[NotMapped]
        //public (string status, string cssClass) StatusHtml { get; set; }

        //public (string status, string cssClass) CurrentStatus => GetCurrentStatus();

        //private (string status, string cssClass) GetCurrentStatus()
        //{
        //    if(PayrollPeriodEmployees.Count() <= 0)
        //    {
        //        return ("Created", "btn-warning");
        //    }
        //    if(PayrollPeriodEmployees.Count> 0)
        //    {

        //        if (NetSalary > 0)
        //        {
        //            return ("Completed", "btn-success");
        //        }

        //        return ("ON GOING", "btn-info");
        //    }
        //    return ("draft", "btn-dark");
        //}

        public string GetPercentChangeString(decimal previous, decimal current, ClaimsPrincipal user)
        {
            var perChange = CalculateChange(previous, current);
            if (perChange == 0) return "";
            else if(perChange > 0)
                return $"<span class='text-success'><i class='fad fa-angle-up'></i> {(perChange * 100).ToString("N0")}%</span>";
            else
                return $"<span class='text-danger'><i class='fad fa-angle-down'></i> {(perChange * 100).ToString("N0")}%</span>";
        }

        private decimal CalculateChange(decimal previous, decimal current)
        {
            if (previous == 0 || current == 0)
                return 0;

            var change = current - previous;
            return change / previous;
        }


        public virtual List<PayrollPeriodPayAdjustment> PayrollPeriodPayAdjustments { get; set; }
        // public virtual ICollection<PayrollPeriodDeduction> PayrollPeriodDeductions { get; set; }

        public string PayrollPeriodStrings => StartDate
            .ToString("dd-MMM-yyy") + " - " + EndDate
            .ToString("dd-MMM-yyy");

        [NotMapped]
        [Display(Name = "Generate values for constant payment adjustments")]
        public bool GenerateFieldsForConstantPayAdjustments { get; set; }

        public PayrollPeriod()
        {
            PayrollPeriodEmployees = new List<PayrollPeriodEmployee>();
            PayrollPeriodPayAdjustments = new List<PayrollPeriodPayAdjustment>();
        }
    }
}
