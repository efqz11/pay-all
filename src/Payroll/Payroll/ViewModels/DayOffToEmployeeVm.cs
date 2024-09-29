using Microsoft.AspNetCore.Mvc.Rendering;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class DayOffToEmployeeVm
    {
        public int? DayOffId { get; set; }
        public int[] Years { get; set; }
        public List<DayOff> dayOffs { get; set; }
        public List<DayOffEmployee> DayOffEmplsInYear { get; set; }
        public int? Year { get;  set; }
        public SelectList YearsSElectList { get;  set; }
        public SelectList DayOffsSelectList { get;  set; }
    }
}
