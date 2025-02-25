using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payroll.Models
{
    public interface IHasCompanyReference
    {
        [Display(Name = "Company")]
        int CompanyId { get; set; }
        Company Company { get; set; }
    }
}
