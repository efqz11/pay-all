using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class BulkImportMasterVm
    {
        
        public string JobID { get; set; }

        [Display(Description = "Job Type")]
        public JobType JobType { get; set; }


        [Display(Description = "Job Level")]
        [Required]
        public string Level { get; set; }


        [Required]
        [Display(Description = "Job title or position of the employee")]
        public string JobTitle { get; set; }

        //[Required]
        [Display(Description = "Employment Type of the employees current contract")]
        public EmployeeType EmploymentType { get; set; }

        //[Required]
        [Display(Description = "Current employment status")]
        public EmployeeStatus EmploymentStatus { get; set; }
        


        [Required]
        [Display(Description = "Name of the location and data may be created or unchanged")]
        public string Location { get; set; }



        [Required]
        [Display(Description = "Name of the department and data may be created or unchanged")]
        public string Department { get; set; }



        [Display(Description = "Reporting line (mapped to job title of supervisor)")]
        public string Reporting { get; set; }


        [Display(Description = "Head of Department (HOD) for this departmentm. (mapped to empID for related department)")]
        public string HOD { get; set; }



        public int RCN { get; set; }
        [Display(Name = "Employee ID", Description = "*Employee ID, leave empty to ignore whole record")]
        public string EmpID { get; set; }

        [SelectableField]
        public Initial Initial { get; set; }
        
        [Required]
        [SelectableField]
        [Display(Name = "Full Name", Description = "Full name field is optional and considered if First or Last name is empty, then this value will be truncated to first, middle and last name")]
        public string FullName { get; set; }

        [Required]
        [SelectableField]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Nick Name")]
        public string NickName { get; set; }


        public Gender Gender { get; set; }
        
        [Display(Name = "Marital Status")]
        public MaritialStatus MaritialStatus { get; set; }
        
        [Display(Name = "End date of Contract")]
        public DateTime? ContractEndDate { get; set; }

        [Display(Name = "End date of Probation")]
        public DateTime? ProbationEndDate { get; set; }

        [Required]
        [Display(Name = "Date of Joining")]
        public DateTime DateOfJoined { get; set; }

        [Display(Name = "No. of Days worked (weekly)")]
        public int WeeklyWorkingHours { get; set; }
        
        [Display(Name = "No. of Hours worked (daily)")]
        public int DailyWorkingHours { get; set; }

        [Display(Name = "ID Type")]
        public IdentityType IdentityType { get; set; }
        [Display(Name = "ID Number")]
        public string IdentityNumber { get; set; }



        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }


        [Display(Name = "Personal Email")]
        public string EmailPersonal { get; set; }

        [Display(Name = "Personal Phone")]
        public string PhonePersonal { get; set; }


        [Display(Name = "Work Email")]
        public string EmailWork { get; set; }
        [Display(Name = "Work Phone")]
        public string PhoneWork { get; set; }


        [Display(Name = "Work Extension")]
        [SelectableField]
        [MaxLength(5)]
        public string PhoneWorkExt { get; set; }
        
        
        //public string PermanentCountry { get; set; }
        [Display(Name = "Permanent Address")]
        public string PermanentAddress { get; set; }
        [Display(Name = "Permanent State")]
        public string PermanentState { get; set; }
        [Display(Name = "Permanent City")]
        public string PermanentCity { get; set; }
        //public string PermanentZipCode { get; set; }
        //public string PresentCountry { get; set; }
        [Display(Name = "Current / Present Address")]
        public string PresentAddress { get; set; }
        [Display(Name = "Present State")]
        public string PresentState { get; set; }
        [Display(Name = "Present City")]
        public string PresentCity { get; set; }
        //public string PresentZipCode { get; set; }

        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; }
        [Display(Name = "Emergency Contact Number")]
        public string EmergencyContactNumber { get; set; }

        [Display(Description = "Emergency contact relationship can be Brother, Sister or any other type of relation. Data will be created directly", Name = "Emergency contact relation to the employee")]
        public string EmergencyContactRelation { get; set; }





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



        public string TwitterId { get; set; }
        public string FacebookId { get; set; }
        public string InstagramId { get; set; }
        public string LinkedInId { get; set; }


        [Display(Description = "Name of the Nationality and data may be created or unchanged")]
        public string Nationality { get; set; }
        [Display(Name = "Visa Type")]
        public VisaType VisaType { get; set; }

        [PayAdjustmentField]
        [Display(Name = "Basic Salary")]
        public decimal BasicSalary { get; set; }
        [PayAdjustmentField]
        public decimal Allowances { get; set; }
        [PayAdjustmentField]
        public decimal Bonus { get; set; }
    }

    public class BulkImportMasterPostDataVm
    {
        public int sxColumnIndx { get; set; }
        public string xColumnName { get; set; }
        public string mappedFieldName { get; set; }
        public List<string> sampleData { get; set; }

        public bool IsMapped => mappedFieldName != "None" && !string.IsNullOrWhiteSpace(mappedFieldName);
        public BulkImportMasterPostDataVm()
        {
            sampleData = new List<string>();
        }
    }


}
