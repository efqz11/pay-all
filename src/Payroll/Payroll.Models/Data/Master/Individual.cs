
using Payroll.Filters;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Payroll.Models
{
    //[AuditableEntity]
    public partial class Employee : Audit
    {
        //public int Id { get; set; }

        #region Personal info
        [SelectableField]
        public Initial Initial { get; set; }

        [Required]
        [SelectableField]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [SelectableField]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }
        [SelectableField]

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [SelectableField]
        public Gender Gender { get; set; }

        [SelectableField]
        public MaritialStatus MaritialStatus { get; set; }

        [SelectableField]
        [NotificaitonAvatar]
        public string Avatar { get; set; }


        [Display(Name = "Personal Email")]
        [SelectableField]
        public string EmailPersonal { get; set; }


        [Phone]
        [Display(Name = "Personal Phone")]
        [SelectableField]
        public string PhonePersonal { get; set; }
        public bool IsPhoneDisplayed { get; set; }
        public bool IsForeignEmployee { get; set; }


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
        public bool IsDobDisplayed { get; set; }


        [Display(Name = "Secondary Language")]
        public int? SecondaryLanguageId { get; set; }
        public SecondaryLanguage SecondaryLanguage { get; set; }


        [Display(Name = "Secondary Language")]
        [SelectableField]
        public string _SecondaryLanguage => SecondaryLanguage?.Name ?? "";

        [SelectableField]
        public IdentityType IdentityType { get; set; }

        [SelectableField]
        [Display(Name = "ID Card No./Passport No.")]
        [RegularExpression("(A|I)([0-9]){6}$", ErrorMessage = "{0} must have the format A123456 or I123456")]
        public string IdentityNumber { get; set; }

        [SelectableField]
        public int RCN { get; set; }





        [Display(Name = "Father's Name")]
        public string FathersName { get; set; }

        [Display(Name = "Mother's Name")]
        public string MothersName { get; set; }

        #endregion


        public string Bio_About { get; set; }



        #region emergency contacts 

        [Display(Name = "Contact's Name")]
        [SelectableField]
        public string EmergencyContactName { get; set; }

        [EmailAddress]
        [Display(Name = "Contact's Email")]
        [SelectableField]
        public string EmergencyContactEmail { get; set; }

        [Display(Name = "Contact's Phone no.")]
        [SelectableField]
        [Phone]
        public string EmergencyContactNumber { get; set; }

        public int? EmergencyContactRelationshipId { get; set; }
        public EmergencyContactRelationship EmergencyContactRelationship { get; set; }

        [Display(Name = "Emergency contact relation to the employee")]
        [SelectableField]
        public string _EmergencyContactRelationship => EmergencyContactRelationship?.Name ?? "";

        #endregion


        #region bankInfo

        [SelectableField]
        [StringLength(maximumLength: 2)]
        [Display(Name = "Bank's Located Country")]
        public string BankCoutnry { get; set; }

        [SelectableField]
        [Display(Name = "Name of the Bank")]
        public string BankName { get; set; }
        [SelectableField]
        [Display(Name = "Name of Account holder")]
        public string BankAccountName { get; set; }
        [Display(Name = "Bank Account No.")]
        [SelectableField]
        public string BankAccountNumber { get; set; }

        [SelectableField]
        public string BankCurrency { get; set; }

        [SelectableField]
        [Display(Name = "Bank Address / Branch")]
        public string BankAddress { get; set; }

        [Display(Name = "Swift Code")]
        [SelectableField]
        public string BankSwiftCode { get; set; }

        [Display(Name = "IBAN")]
        [SelectableField]
        public string BankIBAN { get; set; }

        #endregion

        //public string DisplayOrder { get; set; }


        public string GetSystemName(ClaimsPrincipal princial)
        {
            //return date?.ToString("ddd, MMM dd, yyyy HH:mm");
            return princial.FindFirstValue(CustomClaimTypes.formatter_name).StringFormat(new Dictionary<string, string>
            {
                { "Initial", Initial == 0 ? "" : Initial.ToString() },
                { "FirstName", FirstName },
                { "MiddleName", MiddleName },
                { "LastName", LastName }
            });
        }

        //public decimal BasicSalary { get; set; }

        //public decimal PhoneAllowance { get; set; }


        [Payroll.Filters.Url]
        public string TwitterId { get; set; }

        [Payroll.Filters.Url]
        public string FacebookId { get; set; }

        [Payroll.Filters.Url]
        public string InstagramId { get; set; }

        [Payroll.Filters.Url]
        public string LinkedInId { get; set; }



        //public virtual List<BackgroundJob> BackgroundJobs { get; set; }
        //public virtual List<Employee> Employees { get; set; }
        public List<Address> EmployeeAddresses { get; set; }
        //public string NickName { get; set; }

        //public Individual()
        //{
        //    Employees = new List<Employee>();
        //    BackgroundJobs = new List<BackgroundJob>();
        //    IndividualAddresses = new List<Address>();
        //    Avatar = "~/img/default-image.png";
        //}
    }
}
