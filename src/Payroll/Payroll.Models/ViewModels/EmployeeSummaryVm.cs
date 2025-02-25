using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{

    public class EmployeeSummaryVm
    {
        public int Id { get; set; }

        [Display(Name = "Emp ID")]
        public string EmpID { get; set; }


        [Display(Name = "Avatar")]
        public string Avatar { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Current Date Time")]
        public string DateTimeNow { get; set; }

        [Display(Name = "Current Date")]
        public string DateNow { get; set; }

        [Display(Name = "Current Time")]
        public string TimeNow { get; set; }

        [Display(Name = "Designation")]
        public string Designation { get; set; }


        [Display(Name = "Department")]
        public string Department { get; set; }

        [Display(Name = "Location")]
        public string Location { get; set; }

        [Display(Name = "Division")]
        public string Division { get; set; }

        [Display(Name = "Employment Status")]
        public string EmployementStatus { get; set; }

        public string InstagramId { get; private set; }
        public string TwitterId { get; private set; }
        public string FacebookId { get; private set; }
        public string LinkedInId { get; private set; }


        [Display(Name = "ID Type")]
        public string IdentityType { get; private set; }

        [Display(Name = "ID Number")]

        public string IdentityNumber { get; private set; }
        public int WorkAnniversaryYear { get; }
        [Display(Name = "Joined Date")]
        public string JoinedDate { get; set; }

        [Display(Name = "Date Of Birth")]
        public string DateOfBirth { get; set; }


        [Display(Name = "Grade")]
        public string Grade { get; set; }
        [Display(Name = "Email")]
        public string Email { get;  set; }

        [Display(Name = "Phone")]
        public string Phone { get; set; }
        public string EmergencyContactNo { get; private set; }
        public string EmergencyContactName { get; private set; }
        public string EmergencyContactRelationship { get; private set; }
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Street")]
        public string Street { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }


        public List<EmployeeSummaryVm> DirectReports { get;  set; }
        public EmployeeSummaryVm Manager { get;  set;  }
        public double BackInDays { get; set; }

        public EmployeeSummaryVm()
        {

        }

        public EmployeeSummaryVm(int id, string nameDisplay, string photoLink, string department)
        {
            this.Id = id;
            this.FullName = nameDisplay;
            this.Avatar = photoLink;
            this.Department = department;
        }


        public EmployeeSummaryVm(int id, string nameDisplay, string photoLink, string department, string cssClass = "", string grade = "", string emailAddress = "", string designation = "")
        {
        //    this.Id = id;
            // this.Name = nameDisplay;
            this.Avatar = photoLink;
            this.Department = department;
            // this.CssClass = cssClass;
            this.Grade = grade;
            this.Email = emailAddress;
            this.Designation = designation;
        }

        public EmployeeSummaryVm(Employee emp, ClaimsPrincipal princial)
        {
            this.Avatar = emp.Avatar;
            this.Department = emp.Department != null ? emp.Department.Name : "";
            this.Location = emp.Location != null ? emp.Location.Name : "";
            this.Division = emp.Division != null ? emp.Division.Name : "";

            this.Designation = emp.JobTitle;

            this.EmpID = emp.EmpID;
            this.FirstName = emp.FirstName;
            this.MiddleName = emp.MiddleName;
            this.LastName = emp.LastName;
            this.FullName = emp.GetSystemName(princial);
            this.Email = emp.EmailWork;
            this.Grade = emp.Grade;
            this.Phone = emp.PhonePersonal;

            this.EmergencyContactNo = emp.EmergencyContactNumber;
            this.EmergencyContactName = emp.EmergencyContactName;
            this.EmergencyContactRelationship = emp.EmergencyContactRelationship != null ? emp.EmergencyContactRelationship.Name : "";

            this.EmployementStatus = emp.EmployeeStatus.GetDisplayName();

            this.InstagramId = emp.InstagramId;
            this.TwitterId = emp.TwitterId;
            this.FacebookId = emp.FacebookId;
            this.LinkedInId = emp.LinkedInId;

            this.IdentityType = emp.IdentityType.GetDisplayName();
            this.IdentityNumber = emp.IdentityNumber;
            this.WorkAnniversaryYear = emp.DateOfJoined.HasValue ? DateTime.Now.Year - emp.DateOfJoined.Value.Year : 0;

            if (emp.ReportingEmployee !=null && emp.Employments.Any(x=> x.RecordStatus == RecordStatus.Active))
            {
                 var _ = emp.Employments.FirstOrDefault(x => x.RecordStatus == RecordStatus.Active);
                 if (_.ReportingEmployee != null)
                    this.Manager = new EmployeeSummaryVm
                    {
                        EmpID = _.ReportingEmployee.EmpID,
                        Id = _.ReportingEmployeeId.Value,
                        FullName = _.ReportingEmployee.GetSystemName(princial),
                        Avatar = _.ReportingEmployee.Avatar,
                        Designation = _.ReportingEmployee.JobTitle
                    };
            }

            if (emp.EmployeeDirectReports != null && emp.EmployeeDirectReports.Any())
            {
                this.DirectReports = emp.EmployeeDirectReports
                    .Where(a=> a != null)
                    .Select(_ => new EmployeeSummaryVm
                {
                    EmpID = _.EmpID,
                    Id = _.Id,
                    FullName = _.GetSystemName(princial),
                    Avatar = _.Avatar,
                    Designation = _.JobTitle
                }).ToList();
            }
        }
    }
}
