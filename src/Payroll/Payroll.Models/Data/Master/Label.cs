using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Label : Audit
    {
        public int Id { get; set; }
        

        public int? CompanyId { get; set; }
        public virtual Company Company { get; set; }

        [Required]
        public string Name { get; set; }
        public string Color { get; set; }
        
        public IList<FileData> FileDatas { get; set; }

        public Label()
        {
            FileDatas = new List<FileData>();
        }
    }

}
