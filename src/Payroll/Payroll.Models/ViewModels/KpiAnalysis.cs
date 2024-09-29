using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class KpiAnalysis
    {
        public List<KpiValue> KpiValues { get; set; }
        public decimal Percent { get; set; }
        public string Grade { get; set; }
        public string CssClass => "_" + Grade.ToLower();
        public string PercentStr { get;  set; }

        //public DateTime LastRunDate { get; set; }
        public DateTime? GradeGeneratedDateTime { get;  set; }
        public bool IsGraded { get;  set; }

        public KpiAnalysis()
        {
            KpiValues = new List<KpiValue>();
        }
    }

    public class KpiValue : Audit
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public PayrollPeriodEmployee PayrollPeriodEmployee { get; set; }
        public int? PayrollPeriodEmployeeId { get; set; }

        public KpiValue()
        {

        }

        public KpiValue(string kpi, int best, int worst, int actual, decimal percent, decimal weight, decimal score, string str)
        {
            Kpi = kpi;
            Best = best;
            Worst = worst;
            Actual = actual;
            Percent = percent;
            Weight = weight;
            Score = score;
            Str = str;
        }

        public string Kpi { get; set; }
        public int Best { get; set; }
        public int Worst { get; set; }
        public int Actual { get; set; }
        public decimal Percent { get; set; }
        public decimal Weight { get; set; }
        public decimal Score { get; set; }
        public string Str { get; set; }

        public bool HasBestWork => Worst >= 0 && Best == 0;


        public bool IsChanged { get; set; }
        public DateTime ChangedDate { get; set; }

        public float PercentIncrease { get; set; }
        public float PercentDecrease { get; set; }
    }
}
