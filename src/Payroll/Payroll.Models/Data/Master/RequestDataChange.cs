using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Payroll.Filters;
using Payroll.Services;

namespace Payroll.Models
{
    public class RequestDataChange
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int RequestId { get; set; }
        public Request Request { get; set; }

        public Initial Initial { get; set; }

        public string NickName { get; set; }
        [Required]
        [SelectableField]
        public string FirstName { get; set; }
        [SelectableField]
        public string MiddleName { get; set; }
        [SelectableField]
        public string LastName { get; set; }

        public Gender Gender { get; set; }

        [NotificaitonAvatar]
        public string Avatar { get; set; }


        [Display(Name = "Personal Email")]
        public string EmailPersonal { get; set; }


        [Phone]
        [Display(Name = "Personal Phone")]
        public string PhonePersonal { get; set; }



        [Display(Name = "Nationality")]
        [SelectableField]
        public string _Nationality => Nationality?.Name ?? "";

        [Display(Name = "Nationality")]
        public int? NationalityId { get; set; }
        public Nationality Nationality { get; set; }

        [SelectableField]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "SecondaryLanguage")]
        public int? SecondaryLanguageId { get; set; }
        public SecondaryLanguage SecondaryLanguage { get; set; }


        public IdentityType IdentityType { get; set; }


        [Display(Name = "ID Card No./Passport No.")]
        [RegularExpression("(A|I)([0-9]){6}$", ErrorMessage = "{0} must have the format A123456 or I123456")]
        public string IdentityNumber { get; set; }



        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; }

        [Phone]
        [Display(Name = "Emergency Contact Number")]
        public string EmergencyContactNumber { get; set; }
        public int? EmergencyContactRelationshipId { get; set; }
        public EmergencyContactRelationship EmergencyContactRelationship { get; set; }

        [Display(Name = "Emergency contact relation to the employee")]
        [SelectableField]
        public string _EmergencyContactRelationship => EmergencyContactRelationship?.Name ?? "";


        [Display(Name = "Name of Bank")]
        public string BankName { get; set; }
        [Display(Name = "Account Name")]
        public string BankAccountName { get; set; }
        [Display(Name = "Account Number")]
        public string BankAccountNumber { get; set; }
        public string BankCurrency { get; set; }
        public string BankAddress { get; set; }
        public string BankSwiftCode { get; set; }
        public string BankIBAN { get; set; }


        /// EMPLOYEE DATA

        public string EmpID { get; set; }
        public string EmailWork { get; set; }

        public string PhoneWork { get; set; }

        public string PhoneWorkExt { get; set; }


        [Payroll.Filters.Url]
        public string TwitterId { get; set; }
        [Payroll.Filters.Url]
        public string FacebookId { get; set; }
        [Payroll.Filters.Url]
        public string InstagramId { get; set; }
        [Payroll.Filters.Url]
        public string LinkedInId { get; set; }




        public string Country { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        public string Country1 { get; set; }
        public string StreetAddress1 { get; set; }
        public string City1 { get; set; }
        public string State1 { get; set; }
        public string ZipCode1 { get; set; }
    }
}
