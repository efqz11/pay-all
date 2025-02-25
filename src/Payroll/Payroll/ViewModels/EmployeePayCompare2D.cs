using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class EmployeePayCompare2D
    {
        public int Id { get; set; }
        public decimal TopAddAmnt { get; set; }
        public VariationKeyValue[] AddArray { get; set; }
        public bool isAdd { get; set; }
        public decimal NetPay { get; internal set; }
        public string EmpName { get; internal set; }
        public string Avatar { get; internal set; }
        public EmployeeSummaryVm Employee { get; internal set; }
    }
}
