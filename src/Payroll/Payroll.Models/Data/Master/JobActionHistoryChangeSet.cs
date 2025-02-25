using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class JobActionHistoryChangeSet : Audit
    {
        public int Id { get; set; }


        public int JobActionHistoryId { get; set; }
        public virtual JobActionHistory JobActionHistory { get; set; }

        public string FieldName { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }
        public bool? ChangeState { get; set; }
    }
}
