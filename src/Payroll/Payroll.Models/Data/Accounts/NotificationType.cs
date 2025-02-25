using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class NotificationType : Audit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        
        public string Type { get; set; }
        //public string TypeString { get; set; }

        public bool RequireApproveRejectAction { get; set; }

        public string ChangedProperty { get; set; }
        //public string[] AllowedActions { get; set; }

        public UserType NotificationLevel { get; set; }

        /// <summary>
        /// Actual Notification Entity
        /// </summary>
        public EntityType EntityType { get; set; }

        /// <summary>
        /// To identify request
        /// </summary>
        public RequestType RequestType { get; set; }

        public string Icon { get; set; }
        public string Color { get; set; }


        public string SummaryTextWithPlaceholder { get; set; }
        public string ApprovedTextWithPlaceholder { get; set; }
        public string RejectedTextWithPlaceholder { get; set; }

        public string UrlWithPlaceholder { get; set; }
        public bool IsXhrRequest { get; set; }


        public NotificationReceivedBy NotificationReceivedBy { get; set; }
        /// <summary>
        /// Name matching Employee Role (name) defined
        /// </summary>
        public string[] UsersWithRoles { get; set; }

        public bool RequireReceivedNotes { get; set; }



        public virtual List<Notification> Notifications { get; set; }


        public NotificationType()
        {
            Notifications = new List<Notification>();
        }
    }
}
