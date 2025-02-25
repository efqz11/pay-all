using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace Payroll.ViewModels
{
    public class PayItemVm
    {
        [Required]
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        
        public PayrollPeriod PayrolPeriod { get; set; }
        public int PayrolPeriodId { get; set; }

        public VariationType VariationType { get; set; }

        public List<PayrollPeriodPayAdjustment> PayrolPayAdjustments { get; set; }
        // public List<PayrollPeriodDeduction> PayrollPeriodDeductions { get; set; }

        public PayAdjustment NextItem { get; set; }
        public PayAdjustment PrevItem { get; set; }
        public List<PayAdjustment> MastPayAdjustments { get; internal set; }
        public List<Tuple<int, int, int, decimal>> FieldCounts { get; internal set; }
        public int EmployeeCount { get; internal set; }
        public bool IsLoading { get; internal set; }

        public PayItemVm()
        {

            VariationType = VariationType.VariableAddition;
        }
    }
}
