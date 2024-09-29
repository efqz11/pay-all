using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class CompanyWorkBreakTime : Audit
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }


        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public DateTime _StartTime => new DateTime(2019, 11, 17, StartTime.Hours, StartTime.Minutes, StartTime.Seconds);
        public DateTime _EndTime => new DateTime(2019, 11, (EndTime.Hours < StartTime.Hours ? 18 : 17), EndTime.Hours, EndTime.Minutes, EndTime.Seconds);
        public bool IsFlexible { get; set; }
        public string Duration => StartTime.ToString("hh\\:mm") + "—" + EndTime.ToString("hh\\:mm");
        public int TotakBreaks { get; set; }
    }
}
