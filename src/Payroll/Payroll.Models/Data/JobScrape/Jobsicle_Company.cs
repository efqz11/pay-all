using Newtonsoft.Json;
using Payroll.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models.JobScrape
{
    public class Jobsicle_Company : Audit
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string website { get; set; }
        public string phone { get; set; }
        public string size { get; set; }
        public string sector { get; set; }
        public string founded_on { get; set; }
        public string mission { get; set; }
        public string introduction { get; set; }
        public string why_work_with_us { get; set; }
        public string logo { get; set; }
        public int is_verified { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? created_at { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? updated_at { get; set; }
        public int allow_ratings { get; set; }
        public int category { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? deleted_at { get; set; }
        public int? follower_prefix { get; set; }
        public Jobsicle_Rating rating { get; set; }
        public bool already_rated_by_user { get; set; }
        public Jobsicle_OfficeHour office_hours { get; set; }
        public int rating_count { get; set; }
        public IList<Jobsicle_Photo> photos { get; set; }

        public int? jobsicleId { get; set; }
        public Jobsicle_Company()
        {
            photos = new List<Jobsicle_Photo>();
        }
    }


}
