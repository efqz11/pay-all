using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
  
    public class SecondaryLanguage : Audit
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public SecondaryLanguage()
        {
        }
    }

}
