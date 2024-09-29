using Payroll.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class TaskRunReport : Audit
    {
        public int Id { get; set; }
        
        //public int CompanyAccountId { get; set; }
        //public CompanyAccount CompanyAccount { get; set; }

        public bool IsRecurringJob { get; set; }
        public string JobName { get; set; }
        public string Report { get;  set; }
        public TaskReportType TaskReportType { get;  set; }

        public TaskRunReport()
        {
        }
    }

    public enum TaskReportType
    {
        ContractToggleEveryDay,
        MapContractToEmployeeWeekly,
    }
}
