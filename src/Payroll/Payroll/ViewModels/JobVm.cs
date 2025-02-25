using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class BulkJobUpdateVm
    {
        
        public string JobTitle { get; set; }
        public int TotalJobs { get; set; }
        public bool MustUpdate { get; set; }

        public List<PayAdjustmentValue> PayAdjustmentValues { get; set; }

        public BulkJobUpdateVm()
        {
            PayAdjustmentValues = new List<PayAdjustmentValue>();
        }
    }

    public class PayAdjustmentValue {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value {get; set;}
        public bool ValueBool { get; set; }

        public VariationType VariationType { get; set; }
        public string DifferentValues { get; internal set; }
    }

    
}
