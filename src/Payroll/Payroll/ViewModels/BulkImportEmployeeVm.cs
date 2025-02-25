using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class BulkImportEmployeeVm
    {
        //public bool _ContractCreated { get; set; }
        //public bool _UserAccessGrantCreated { get; set; }
        //public bool _EmployeeRecordUpdated { get; set; }

        public string EmpID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }


        public string NickName { get; set; }

        public string EmailAddress { get; set; }
        public string PhoneWork { get; set; }
        public string PhonePersonal { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactNumber { get; set; }

        [Display(Description = "Emergency contact relationship can be Brother, Sister or any other type of relation. Data will be created directly")]
        public string EmergencyContactRelation { get; set; }
        public string Address { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }



        public Gender Gender { get; set; }

        [Display(Description = "Name of the department and data may be created or unchanged")]
        public string DepartmentName { get; set; }

        public DateTime DateOfBirth { get; set; }
        public IdentityType IdentityType { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime DateOfJoined { get; set; }
        
        
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }

        public string TwitterId { get; set; }
        public string FacebookId { get; set; }
        public string InstagramId { get; set; }
        public string LinkedInId { get; set; }

        [Display(Description = "Name of the Nationality and data may be created or unchanged")]
        public string Nationality { get; set; }
        //public string Passport { get; set; }
        public VisaType VisaType { get; set; }

        //public string AgreementName { get; set; }
        //public DateTime AgreementStart { get; set; }
        //public DateTime AgreementEnd { get; set; }
        //public ContractType AgreementType { get; set; }
        //public string Designation { get; set; }
        //public bool _UserCreated { get; internal set; }
    }

    
}
