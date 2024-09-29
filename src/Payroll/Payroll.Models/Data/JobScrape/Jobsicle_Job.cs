using Newtonsoft.Json;
using Payroll.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models.JobScrape
{
    public class Jobsicle_Job : Audit
    {
        public int? jobsicleId { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string company_name { get; set; }
        public string attachment { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime due_date { get; set; }
        public bool applied_date { get; set; }
        public string company_logo { get; set; }
        public bool user_likes_job { get; set; }
        public bool user_saved_job { get; set; }
        public string ref_no { get; set; }
        public string sector { get; set; }
        public string category { get; set; }
        public string type { get; set; }
        public string experience { get; set; }
        public string qualification { get; set; }
        public string salary_range { get; set; }
        public string location { get; set; }
        public int no_of_vacancies { get; set; }
        public int is_open { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? created_at { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? updated_at { get; set; }

        [ForeignKey("company")]
        public int? companyId { get; set; }

        public Jobsicle_Company company { get; set; }

        public int total_likes { get; set; }
        public IList<Jobsicle_Question> questions { get; set; }
        public int prevent_international_applicants { get; set; }
        public int prevent_online_application { get; set; }
        public string application_form { get; set; }
        public string interview_starts_on { get; set; }
        public string notification_email { get; set; }
        public IList<string> required_items { get; set; }
    }




}
