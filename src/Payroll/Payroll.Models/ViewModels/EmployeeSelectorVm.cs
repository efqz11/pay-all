using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{

    public class EmployeeSelectorVm
    {
        // id, name , count
        public List<Tuple<int?, string, int>> ByLocation { get; set; }
        public List<Tuple<int?, string, int>> ByDept { get; set; }
        public List<Tuple<int, EmployeeStatus, int>> ByEmpStatus { get; set; }
        public List<Tuple<int?, JobType?, int>> ByJobType { get; set; }

        public string GroupByCategoryValue { get; set; }
        public int[] GroupByCategoryValueArray =>  GroupByCategoryValue?.Split(",")?.Select(a => Convert.ToInt32(a)).ToArray() ?? default(int[]);
        public GroupByCategory GroupByCategory { get; set; }
        public int[] EmployeeIds { get; set; }

        //[Required]
        public string Action { get; set; }
        //[Required]
        public string Controller { get; set; }
        public string Summary { get; set; }
        public int TotalMatchedEmployees { get; set; }
        public int TotalFoundEmployees { get; set; }
        public bool IsEditView { get; set; }
        public string OnSuccess { get;  set; }
        public string ChosenEmployeeDataString { get;  set; }
        public string Update { get;  set; }
        public int? RouteDataId { get;  set; }

        public bool IsValid()
        {
            return TotalMatchedEmployees > 0 && (!string.IsNullOrWhiteSpace(GroupByCategoryValue) || EmployeeIds.Count() > 0) && EmployeeIds.Count() > 0;
        }
    }
}
