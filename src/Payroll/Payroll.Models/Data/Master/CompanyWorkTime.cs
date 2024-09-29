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
    public class CompanyWorkTime : Audit
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        [NotMapped]
        public int MinEmployees { get; set; }
        [NotMapped]
        public int MaxEmployees { get; set; }


        public bool IsShift { get; set; }
        public string ShiftName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string ColorCombination { get; set; }
        public string GetBackgroundColor => ColorCombination == null ? "" : ColorCombination?.ToLower() == "green" ? "rgba(163, 187, 11, 1)" :
             ColorCombination.ToLower() == "blue" ? "rgba(82, 164, 208, 1)" :
             ColorCombination.ToLower() == "red" ? "rgba(212, 104, 104, 1)" : "";

        public double TotalHours => ((EndTime <= StartTime ? EndTime.TotalHours + 24 : EndTime.TotalHours) - StartTime.TotalHours);

        public DateTime _StartDateTime => new DateTime(2019, 11, 17, StartTime.Hours, StartTime.Minutes, StartTime.Seconds);
        public DateTime _EndDateTime => new DateTime(2019, 11, (EndTime.Hours < StartTime.Hours ? 18 : 17), EndTime.Hours, EndTime.Minutes, EndTime.Seconds);
        public TimeSpan _DiffDateTime => _EndDateTime - _StartDateTime;
        public string Duration => StartTime.ToString("hh\\:mm") + "—" + EndTime.ToString("hh\\:mm");

        public int TotakBreaks { get; set; }

        public CompanyWorkTime()
        {
        }
    }
}
