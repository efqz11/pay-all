using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class KpiConfig
    {
        public int Id { get; set; }
        public int CmpId { get; set; }
        public KpiConfig()
        {

        }

        public KpiConfig(string kpi, int best, int worst, int actual, decimal weight, string displayName)
        {
            Kpi = kpi;
            Best = best;
            Worst = worst;
            Actual = actual;
            Weight = weight;
            DisplayName = displayName;
        }

        public string Kpi { get; set; }
        public int Best { get; set; }
        public int Worst { get; set; }
        public int Actual { get; set; }
        //public decimal Percent { get; set; }
        [Range(0,1)]
        public decimal Weight { get; set; }
        //public decimal Score { get; set; }
        public string DisplayName { get; set; }


        public bool IsTaskRelated { get; set; }
        public WorkType? WorkType { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDisciplinaryActionCount { get; set; }
    }
    
}
