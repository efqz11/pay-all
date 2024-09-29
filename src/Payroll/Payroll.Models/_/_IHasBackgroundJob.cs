using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public interface IHasBackgroundJob
    {
        bool? HasBackgroundJob { get; set; }
        string HangfireJobId { get; set; }
        int? BackgroundJobId { get; set; }
        
        //[NotMapped]
        //BackgroundJob BackgroundJob { get; set; }
        string BackgroundJobDetails { get; set; }
        DateTime? NextRunDate { get; set; }
        bool? HasBackgroundJobEnded { get; set; }

    }

}
