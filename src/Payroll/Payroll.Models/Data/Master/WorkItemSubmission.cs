using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class WorkItemSubmission : Audit
    {
        public int Id { get; set; }

        public int WorkItemId { get; set; }
        public WorkItem WorkItem { get; set; }

        public string Name { get; set; }
        [DataType(DataType.MultilineText)]
        public string Details { get; set; }

        [NotMapped]
        public string Url { get; set; }
        public bool IsApproved { get; set; }

        public DateTime? SentForApprovalDate { get; set; }


        public string ActionTakenUserId { get; set; }
        public string ActionTakenUserName { get; set; }
        public DateTime? ActionTakenDate { get; set; }
        public string ActionTakenReasonSummary { get; set; }


        public DateTime? SubmissionDate { get; set; }

        public WorkItemStatus Status { get; set; }
        public List<FileData> FileDatas { get; set; }
        public WorkItemSubmission()
        {
            FileDatas = new List<FileData>();
        }
    }
}
