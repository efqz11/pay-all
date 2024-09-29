using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models.JobScrape
{
    public class Jobsicle_Rating : Audit
    {
        public int id { get; set; }
        public string work_life_balance { get; set; }
        public string compensation { get; set; }
        public string job_security { get; set; }
        public string management { get; set; }
        public string culture { get; set; }
        public int jobsicleId { get;  set; }
    }


}
