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
    /// Actor, Notifier, Entity, Entity type
    /// This table will hold the information regarding the notifiers, 
    /// the users to whom the notification has to be sent.
    /// </summary>
    public class Notification : Audit
    {
        public int Id { get; set; }


        /// <summary>
        /// Sending UserId (Publisher)
        /// </summary>
        [ForeignKey("ActionTakenUserId")]
        public virtual AppUser User { get; set; }
        public string ActionTakenUserId { get; set; }

        public string ActionTakenEmployeeAvatar { get; set; }
        public string ActionTakenEmployeeName { get; set; }
        public int ActionTakenEmployeeId { get; set; }


        public int CompanyAccountId { get; set; }
        public virtual CompanyAccount CompanyAccount { get; set; }


        public string EntityId { get; set; }

        public int NotificationTypeId { get; set; }
        public virtual NotificationType NotificationType { get; set; }



        public string Summary { get; set; }
        public string Avatar { get; set; }
        public string SummaryHtml { get; set; }


        public NotificationActionTakenType NotificationActionTakenType { get; set; }
        public string Remarks { get; set; }

        public string Url { get; set; }
        public bool IsXhrRequest { get; set; }

        public DateTime SentDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime? ActionTakenDate { get; set; }
        public bool IsRead { get; set; }


        public int? ParentNotificationId { get; set; }
        public virtual Notification ParentNotification { get; set; }

        public virtual List<Notification> ChildNotifications { get; set; }

        [NotMapped]
        public bool IgnoreChecks { get; set; }
        public int Step { get; set; }
        public string[] EmployeesWithRoles { get; set; }
        public int? EmployeeId { get; set; }
        public RequestProceessConfigActionBy? ToBeReceivedBy { get; set; }
        public int RequestingEmployeeId { get; set; }
        public string RequestingEmployeeAvatar { get; set; }
        public string RequestingEmployeeName { get; set; }
        public EmployeeNotificationType NotificationActionTypeEnum { get; set; }
        public bool IsFallbackNotification { get; set; }
        public string fallbackNotificationSummary { get; set; }

        public void SetFallbackSummary(string summary)
        {
            IsFallbackNotification = true;
            fallbackNotificationSummary = summary;
        }
        public Notification()
        {
            ChildNotifications = new List<Notification>();
        }
        
    }
}
