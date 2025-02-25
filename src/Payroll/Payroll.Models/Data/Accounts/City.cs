
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
    public class City : Audit
    {
        public int Id { get; set; }

        public int StateId { get; set; }
        public virtual State State { get; set; }


        [Required]
        public string Name { get; set; }

        public string Code { get; set; }
    }
}
