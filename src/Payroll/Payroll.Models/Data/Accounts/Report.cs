using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Report
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }

        public ReportCategory Category { get; set; }
        public virtual ICollection<ReportView> ReportViews { get; set; }


        public Report()
        {
            ReportViews = new List<ReportView>();
        }
        
    }
}
