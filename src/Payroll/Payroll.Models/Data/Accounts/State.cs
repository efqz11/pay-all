
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
    public class State : Audit
    {
        public int Id { get; set; }

        public int? CountryId { get; set; }
        public virtual Country Country { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
