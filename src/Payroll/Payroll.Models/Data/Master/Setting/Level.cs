using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Level : Audit
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
       
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        public int DisplayOrder { get; set; }

        public virtual IList<Job> Jobs { get; set; }

        public Level()
        {
            Jobs = new List<Job>();
        }
    }
}
