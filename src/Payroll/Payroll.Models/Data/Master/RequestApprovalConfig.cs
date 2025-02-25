using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Payroll.Filters;
using Payroll.Services;

namespace Payroll.Models
{
    public class RequestApprovalConfig : Audit
    {
        public int Id { get; set; }


        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }


        public RequestType RequestType { get; set; }
        public RequestProceessConfigActionBy RequestProceessConfigActionBy { get; set; }

        public string GetApprovalPersonHtml(ClaimsPrincipal User)
        {
             switch (RequestProceessConfigActionBy)
                {
                    case RequestProceessConfigActionBy.EmployeesWithRole:
                        return $"<i class='fa fa-user-shield'></i> {EmployeeRole.Role}";
                    case RequestProceessConfigActionBy.SpecificEmployee:
                    return $"<span><i class='fa fa-user-shield'></i> {Employee.GetSystemName(User)}</span>";
                    case RequestProceessConfigActionBy.Supervisor:
                    return $"<span><i class='fa fa-user-tie'></i> Supervisor</span>";
                        break;
                    case RequestProceessConfigActionBy.SupervisorsSupervisor:
                    return $"<i class='fa fa-user-tie'></i> Supervisor**";
                    case RequestProceessConfigActionBy.AutoActionAfterHours:
                    return $"<span><i class='fad fa-engine-warning'></i> auto {AutoAction.GetDisplayName()} after {AutoActionAfterHours} hours</span>";
                    default:
                    return "";
                }
        }

        public int Step { get; set; }

        public virtual DayOff DayOff { get; set; }
        public int? DayOffId { get; set; }

        public virtual EmployeeRole EmployeeRole { get; set; }
        public int? EmployeeRoleId { get; set; }

        public virtual Employee Employee { get; set; }
        public int? EmployeeId { get; set; }


        public bool IsAutomaticActiomAfterSubmission { get; set; }

        public int AutoActionAfterHours { get; set; }
        public WorkItemStatus? AutoAction { get; set; }


        public string AutoActionSummary { get; set; }
    }
}

