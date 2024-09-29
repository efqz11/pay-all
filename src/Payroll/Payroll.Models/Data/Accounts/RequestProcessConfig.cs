using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class RequestProcessConfig : Audit
    {
        public int Id { get; set; }


        public int CompanyAccountId { get; set; }
        public virtual CompanyAccount CompanyAccount { get; set; }

        public RequestType RequestType { get; set; }

        public bool IsApprovalByDepartmentHead { get; set; }

        public bool IsApprovalByDefinedEmployees { get; set; }

        public int[] ApprovalByEmployeeIds { get; set; }


        public bool IsAutomaticActiomAfterSubmission { get; set; }

        public int AutoActionAfterHours { get; set; }
        public WorkItemStatus? AutoAction { get; set; }


        public string AutoActionSummary { get; set; }
    }
}
