using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Company : Audit
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }


        //[Required]
        public string Address => Locations?.FirstOrDefault()?.Addresses?.FirstOrDefault().GetAddressString() ?? "";
        public string LogoUrl { get; set; }

        public string Website { get; set; }
        
        public string Hotline { get; set; }

        public string TaxCode { get; set; }

        public decimal? TaxPercentValue { get; set; }

        public string ManagingDirector { get; set; }


        //public CompanyStatus Status { get; set; }


        public CompanyWorkType WorkType { get; set; }
        public double FlexibleBreakHourCount { get; set; }
        public bool IsBreakHourStrict { get; set; }
        public double EarlyOntimeMinutes { get; set; }
        public bool TrackOvertime { get; set; }
        public TimeFrame OvertimCalculationBasis { get; set; }
        public int OvertimeCliffHours { get; set; }
        public bool SetOffDefecitHoursAgainstOvertime { get; set; }


        public virtual List<Department> Departments { get; set; }
        public virtual List<Team> Teams { get; set; }
        public virtual List<Division> Divisions { get; set; }
        public virtual List<Location> Locations { get; set; }
        public virtual List<PayrollPeriod> PayrollPeriods { get; set; }
        public virtual List<PayAdjustment> PayAdjustments { get; set; }
        public virtual List<DayOff> DayOffs { get; set; }
        public virtual List<Announcement> Announcements { get; set; }
        public virtual List<CompanyWorkTime> WorkTimes { get; set; }
        public virtual List<CompanyWorkBreakTime> BreakTimes { get; set; }
        public virtual List<CompanyPublicHoliday> CompanyPublicHolidays { get; set; }
        public virtual List<CompanyFolder> CompanyFolders { get; set; }
        public virtual List<Employee> Employees { get; set; }
        public virtual List<EmployeeRole> EmployeeRoles { get; set; }
        public virtual List<RequestApprovalConfig> RequestApprovalConfigs { get; set; }




        /// TO BE USED IN CONTROLLER 
        /// EASY CRUD


        [NotMapped]
        public bool IsSaveOtf { get; set; }
        [NotMapped]
        public bool CreateAnotherShift { get; set; }
        [NotMapped]
        public bool CreateNewBreakHour { get; set; }
        [NotMapped]
        public bool CreateNewWorkHour { get; set; }
        [NotMapped]
        public bool IsChangeWorkType { get; set; }

        /// <summary>
        /// Indicate whether to update for history records or apply on future update
        /// </summary>
        [NotMapped]
        public bool UpdateHistory { get; set; }

        public Company()
        {
            Departments = new List<Department>();
            Teams = new List<Team>();
            EmployeeRoles = new List<EmployeeRole>();
            PayrollPeriods = new List<PayrollPeriod>();
            PayAdjustments = new List<PayAdjustment>();
            DayOffs = new List<DayOff>();
            Announcements = new List<Announcement>();
            WorkTimes = new List<CompanyWorkTime>();
            BreakTimes = new List<CompanyWorkBreakTime>();

            CompanyPublicHolidays = new List<CompanyPublicHoliday>();
            RequestApprovalConfigs = new List<RequestApprovalConfig>();
            CompanyFolders = new List<CompanyFolder>();
            Employees = new List<Employee>();
        }

    }


}
