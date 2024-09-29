using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Payroll.Models;

namespace Payroll.ViewModels
{
    public class EmploymentProfileVm
    {
        public Employee Employee { get; set; }
        [Required]
        public int CompanyId { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Initial Initial { get; set; }
        public DateTime StartDate { get; set; }
        [Required]
        public int LocationId { get; set; }
        public int DepartmentId { get; set; }

        public string NewDepartment { get; set; }
        public int ReportingEmployeeId { get; set; }

        [Required, EmailAddress]
        public string PersonalEmail { get; set; }
        public bool InviteThemToFill { get; set; }

        public EmployeeType EmployeeType { get; set; }
        public List<EmployeePayComponent> Wages { get; set; }
        public List<int> DayOffIds { get; set; }

        public string JobId { get; set; }
        
        //[Required]
        public string JobTitle { get; set; }
        public string Level { get;  set; }
    }

    public class EmploymentChangeVm
    {
        public int EmployeeId { get; set; }
        public int JobId { get; set; }
        public int ReportingEmployeeId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public Employee Employee { get; set; }
    }
}