using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class CompanyFileShare : Audit
    {
        public int Id { get; set; }

        public int CompanyFileId { get; set; }
        public virtual CompanyFile CompanyFile { get; set; }

        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        public DateTime SharedDate { get; set; }
        public DateTime SignedDate { get; set; }

        public bool IsSigned { get; set; }
        public Dictionary<string, object> FileConfigValues { get; set; }

        public CompanyFileShare()
        {
            FileConfigValues = new Dictionary<string, object>();
        }
    }
}
