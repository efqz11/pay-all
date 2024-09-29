using Payroll.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Address : Audit
    {
        public int Id { get; set; }


        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }


        public int? LocationId { get; set; }
        public Location Location { get; set; }


        public AddressType AddressType { get; set; }
        public string GetAddressString()
        {
            var _ = Street1;
            if (!string.IsNullOrWhiteSpace(Street2))
                _ += " " + Street2;
            _ += "\n"; //<br>
                _ += State;
            if (!string.IsNullOrWhiteSpace(State))
                _ += ",";

            if (!string.IsNullOrWhiteSpace(City))
                _ += City;

            _ += "\n"; //<br>
            if (!string.IsNullOrWhiteSpace(ZipCode))
                _ += ZipCode;

            return _;
        }

        [Required]
        [Display(Name = "Street Address")]
        [SelectableField]
        public string Street1 { get; set; }
        [Display(Name = "Street others")]
        [SelectableField]
        public string Street2 { get; set; }
        [SelectableField]
        public string ZipCode { get; set; }

        [SelectableField]
        [Display(Name = "City / Island")]
        public string City { get; set; }
        [Display(Name = "City / Island")]
        public int? CityId { get; set; }

        [Display(Name = "State / Atoll")]
        [SelectableField]
        public string State { get; set; }
        [Display(Name = "State / Atoll")]
        public int? StateId { get; set; }

        [StringLength(maximumLength:2)]
        [SelectableField]
        public string Country { get; set; }

        [Display(Name = "Country")]
        public int? CountryId { get; set; }



        public RecordStatus RecordStatus { get; set; }

    }
}
