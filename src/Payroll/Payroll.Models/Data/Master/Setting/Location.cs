using Payroll.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Location : Audit
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Required]
        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public List<Employee> Employees { get; set; }
        public List<Address> Addresses { get; set; }

        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }

        public Location()
        {
            Employees = new List<Employee>();
            Addresses = new List<Address>();
        }
    }
}
