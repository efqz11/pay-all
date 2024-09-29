using Payroll.Controllers;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class ComparsonVm
    {
        [Required]
        public int SourcePeriodId { get; set; }
        public int ComaprePeriodId { get; set; }

        public List<ComapareData> CompareDatas { get; set; }
        //public List<PayrollPeriodEmployee> ComparePeriodEmplees { get;  set; }
        public bool IsComparingPeriod { get; set; }
        public ComapareData GrossPayComaparison { get; set; }
        public ComapareData NetSalaryComaparison { get; set; }
        public List<EmployeePayCompare2D> TopAndBottomList { get; set; }

        public ComparsonVm()
        {
            CompareDatas = new List<ComapareData>();
            // VariationType = VariationType.Addition;
        }
    }




    public class ComapareData
    {
        public string Key { get; set; }
        public int KeyId { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal CompareValue { get; set; }

        public bool HasChanged => CurrentValue != CompareValue;
        public VariationType VariationType { get; internal set; }
        public decimal Percentage { get; internal set; }
        public string TrendText { get; internal set; }
        public List<ComapareData2> SecondCompareData { get; set; }
        public int MultiOrder { get; internal set; }
        public int CurrentValueEmplCount { get; internal set; }
        public int CompareValueEmplCount { get; internal set; }
        public int[] CurrentValueEmployees { get; internal set; }
        public int[] CompareValueEmployees { get; internal set; }

        public ComapareData()
        {
            SecondCompareData = new List<ComapareData2>();
        }
    }

    public class ComapareData2
    {
        public string Key { get; set; }
        public int KeyId { get; set; }
        public string CurruretValue { get; set; }
        public string CompareValue { get; set; }
        public VariationType VariationType { get;  set; }
        public decimal Percentage { get; internal set; }
        public string TrendText { get; internal set; }
        public List<ComapareData> SecondCompareData { get; set; }
        public BaseType? BaseType { get;  set; }
        public decimal CurruretValueDecimal { get; internal set; }
        public decimal CompareValueDecimal { get; internal set; }

        public ComapareData2()
        {
            SecondCompareData = new List<ComapareData>();
        }
    }
}
