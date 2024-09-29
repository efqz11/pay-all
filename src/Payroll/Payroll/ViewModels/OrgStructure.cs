using Payroll.Controllers;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class OrgStructure
    {
        internal int? departmentId;

        public string name { get; set; }
        public string title { get; set; }

        public List<OrgStructure> children { get; set; }
        public string avatar { get; internal set; }
        public int id { get; internal set; }
        public string department { get; internal set; }
        public string location { get; internal set; }
        public string division { get; internal set; }
        public string empstate { get; internal set; }
        public int? jobId { get; internal set; }
        public bool current { get; internal set; }
        public int? reportingJobId { get; internal set; }
        public int? ReportingEmployeeId { get; internal set; }
    }
}
