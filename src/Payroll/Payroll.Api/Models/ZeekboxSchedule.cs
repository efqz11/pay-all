using System;

namespace Payroll.Api.Models
{
    public class ZeekboxSchedule
    {
        public string ScheduleDocReference { get; set; }
        public string ScheduleRuleReference { get; set; }
        public string Summary { get; set; }
        public DateTime RunDate { get; set; }


        public void addSchedule(string project)
        {
        }
    }

}
