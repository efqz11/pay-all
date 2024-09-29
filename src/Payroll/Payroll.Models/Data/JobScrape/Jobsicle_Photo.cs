using Newtonsoft.Json;
using Payroll.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models.JobScrape
{
    public class Jobsicle_Photo : Audit
    {

        public int jobsicleId { get; set; }
        public int id { get; set; }

        [ForeignKey("company")]
        public int company_id { get; set; }

        public Jobsicle_Company company { get; set; }

        public string picture { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? created_at { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? updated_at { get; set; }

        public int slot { get; set; }
        
    }




}
