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
    public class ReportView : Audit
    {
        public int Id { get; set; }

        public virtual Report Report { get; set; }
        public string ReportId { get; set; }

        /// <summary>
        /// Sending UserId (Publisher)
        /// </summary>
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }
        public string UserId { get; set; }


        public int CompanyAccountId { get; set; }
        public virtual CompanyAccount CompanyAccount { get; set; }



        public DateTime ViewedDate { get; set; }


        public ReportView()
        {
        }
        
    }
}
