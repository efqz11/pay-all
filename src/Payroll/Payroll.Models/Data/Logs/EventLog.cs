
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Payroll.Model.Logs
{
    public class EventLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        public int EventResultId { get; set; }
        public EventResult EventResult { get; set; }

        public int EventLogTypeId { get; set; }
        public EventLogType EventLogType { get; set; }


        public int EventDataTypeId { get; set; }
        public EventDataType EventDataType { get; set; }

        public string DataItemKey { get; set; }

        public string[] AffectedEmployeeIds { get; set; }

        public string[] ActionDetails { get; set; }


        public string Summary { get; set; }


        [Required]
        public DateTime TimeStamp { get; set; }

        public string IPAddress { get; set; }
        public string UserAgent { get; set; }


        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
