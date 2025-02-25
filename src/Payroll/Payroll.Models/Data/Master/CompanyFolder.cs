using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class CompanyFolder : Audit
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Color { get; set; }
        

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        

        

        public virtual List<CompanyFile> CompanyFiles { get; set; }

        public CompanyFolder()
        {
            CompanyFiles = new List<CompanyFile>();
        }
        
    }
    
}
