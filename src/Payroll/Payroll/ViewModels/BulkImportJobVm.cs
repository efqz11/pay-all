using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class BulkImportJobVm
    {
        //public bool _ContractCreated { get; set; }
        //public bool _UserAccessGrantCreated { get; set; }
        //public bool _EmployeeRecordUpdated { get; set; }

        [Display(Description = "Name of the location and data may be created or unchanged")]
        public string Location { get; set; }


        [Display(Description = "Name of the department and data may be created or unchanged")]
        public string Department { get; set; }
        public string JobTitle { get; set; }
        public string JobID { get; set; }
        public JobType JobType { get; set; }

        public string Classification { get; set; }

        [Display(Description = "Total number of Jobs to be created")]
        public int Total { get; set; }
    }

    
}
