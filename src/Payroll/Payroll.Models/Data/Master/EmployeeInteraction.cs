using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    /// <summary>
    /// WIll be used to store request approvals, substitution approvals, announcement recieved 
    /// </summary>
    public class EmployeeInteraction : Audit
    {
        public int Id { get; set; }
        public virtual Company Company { get; set; }
        public int CompanyId { get; set; }


        public virtual Employee Employee { get; set; }
        public int EmployeeId { get; set; }


        public int? RequestId { get; set; }
        public virtual Request Request { get; set; }

        public int? AnnouncementId { get; set; }
        public virtual Announcement Announcement { get; set; }



        public string Avatar { get; set; }
        public string RequestingEmployeeName { get; set; }
        public string RequestingEmployeeId { get; set; }
        public string Summary { get; set; }
        public string SummaryHtml { get; set; }


        public NotificationActionTakenType NotificationActionTakenType { get; set; }
        public EmployeeInteractionType EmployeeInteractionType { get; set; }
        //public RemindAbout RemindAbout { get; set; }
        public string Remarks { get; set; }


        public DateTime SentDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime? ActionTakenDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsRead { get; set; }


        public int? ParentNEmployeeInteractionId { get; set; }
        public virtual EmployeeInteraction ParentNEmployeeInteraction { get; set; }

        public virtual List<EmployeeInteraction> ChildEmployeeInteractions { get; set; }


        public int Step { get; set; }
        public string[] EmployeesWithRoles { get; set; }

        public RequestProceessConfigActionBy? ToBeReceivedBy { get; set; }


        public EmployeeInteraction()
        {
            ChildEmployeeInteractions = new List<EmployeeInteraction>();
        }
        
    }
}
