using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{

    public class DayOffEmplVm
    {
        public int UniqeId { get; set; }
        public int DayOffId { get; set; }
        public int? RequestId { get; set; }
        public int EmployeeId { get; set; }
        public string DayOffName { get; set; }
        public string DayOffColor { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsPending { get; set; }
        public string RequestedDuration { get; set; }
        public string RequestedStatusString { get; set; }
        public WorkItemStatus RequestedStatus { get; set; }
        public string RequestedStatusIcon { get; set; }
        public string RequestReference { get; set; }
    }
}
