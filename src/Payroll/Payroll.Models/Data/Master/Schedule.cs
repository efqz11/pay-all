using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

    public class Schedule : Audit, IHasBackgroundJob
    {

        public int Id { get; set; }
        
        public int[] EmployeeIds { get; set; }
        
        public int[] WorkTimeIds { get; set; }

        [NotMapped]
        public int[] IgnoreDays { get; set; }
        


        public IList<int> IgnoreDaysData { get; set; }

        public IList<_Employee> EmployeeIdsData { get; set; }
        public IList<_WorkTime> WorkTimeIdsData { get; set; }
        public IList<DayVm> DaysData { get; set; }

        public string Name { get; set; }
        
        public DateTime? End { get; set; }
        [Required]
        public DateTime Start { get; set; }
        public string Duration => Start.ToString("dd MMM yyyy") + " - " + (End.HasValue ? End.Value.ToString("dd MMM yyyy") : "NA (repeat)");
        public string GetDuration(ClaimsPrincipal user) => Start.ToSystemFormat(user) + " - " + (End.HasValue ? End.Value.ToSystemFormat(user) : "NA (repeat)");


        public int Slots { get; set; }


        public string TimeZone { get; set; }
        public int MinHours { get; set; }

        public bool IsRepeating { get; set; }
        public bool IsForAllEmployees { get; set; }


        public bool IsEffectiveImmediately { get; set; }
        public DateTime? EffectiveDate { get; set; }

        

        public RecurringFrequency Repeat { get; set; }
        public DateTime? RepeatEndDate { get; set; }

        public string GetHeader(RosterCreateLineItemType selectedMenu)
        {
            switch (selectedMenu)
            {
                case RosterCreateLineItemType.AddInitialData:
                    return "Choose Shifts and Employees as Initial Data";
                case RosterCreateLineItemType.SetRotatingPattern:
                    return "Set Rotating pattern for week";
                case RosterCreateLineItemType.SetupEmployee:
                    return "Verify Employments";
                case RosterCreateLineItemType.SetupShift:
                    return "Manage Work arrangements with Shifts";
                //case RosterCreateLineItemType.SetupContracts:
                //    return "Set Contract of Employees";
                //case RosterCreateLineItemType.Penalties:
                //    return "Penalties for violations";
                //case RosterCreateLineItemType.Running:
                //    return "Running";
                case RosterCreateLineItemType.Complete:
                    return "Genereated Schedule";
                case RosterCreateLineItemType.Error:
                    break;
                default:
                    break;
            }
            return "";
        }

        public bool IsRepeatEndDateNever { get; set; }

        public ScheduleFor ScheduleFor { get; set; }
        public ScheduleStatus Status { get; set; }


        //public string Summary => ScheduleFor == ScheduleFor.Attendance ? $"WorkTime: {WorkId} {WorkDuration}" : $"{ScheduleFor}: {Work}{(WorkDuration == "" ? "" : WorkDuration)}";


        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public List<Attendance> Attendances { get; set; }
        public List<WorkItem> WorkItems { get; set; }

        
        public bool? HasBackgroundJob { get; set; }
        public string HangfireJobId { get; set; }
        public int? BackgroundJobId { get; set; }

        [NotMapped]
        public BackgroundJob BackgroundJob { get; set; }
        public string BackgroundJobDetails { get; set; }
        public bool bySchduler { get; set; }
        public DateTime? NextRunDate { get; set; }
        public bool? HasBackgroundJobEnded { get; set; }
        public IList<BackgroundJob> backgroundJobs { get; set; }


        public int? DepartmentId { get;  set; }
        public Department Department { get;  set; }
        public bool IsForDepartment { get;  set; }

        public int WorkId { get;  set; }
        public string WorkName { get; set; }

        public RosterCreateLineItemType SelectedMenu { get; set; }
        public char[] _Patten { get;  set; }
        /// <summary>
        /// MMMNMM-|EEENEE-
        /// </summary>
        public string _PattenString { get;  set; }
        public int _ConseqetiveDays { get;  set; }
        public double _TotalWorkingHoursPerWeek { get;  set; }
        //public bool IsGenerated { get; internal set; }
        public DateTime RosterGeneratedDate { get;  set; }

        [ForeignKey("ParentSchedule")]
        public int? ParentScheduleId { get; set; }
        public Schedule ParentSchedule { get; set; }
        public virtual ICollection<Schedule> FololwingSchedules { get; set; }
        public bool HaveConflicts { get;  set; }
        //public bool IsPublished { get; internal set; }

        public Schedule()
        {
            backgroundJobs = new List<BackgroundJob>();
            Attendances = new List<Attendance>();
            WorkItems = new List<WorkItem>();

            FololwingSchedules = new List<Schedule>();
        }

        public void CalculateSlots()
        {
            var totalDays = (int)(End - Start).Value.TotalDays;
            Slots = totalDays * WorkTimeIds.Length;
        }

        public int GetCalculateSlots()
        {
            var totalDays = (int)(End - Start).Value.TotalDays;
            return totalDays * WorkTimeIds.Length;
        }

        public void EnsureCanProceedRostering()
        {
            if (string.IsNullOrWhiteSpace(_PattenString))
                throw new ApplicationException("Patten for roation requirements was not set!");

            if (_ConseqetiveDays < 2)
                throw new ApplicationException("Consecutive days must be atleast 2 days");

            var totalDays = (End - Start).Value.TotalDays;
            if (totalDays <= 0)
                throw new ApplicationException("Time horizon or plannin period not set properly");

            //if (_TotalWorkingHoursPerWeek < 2)
            //    throw new ApplicationException("Consecutive days must be atleast 2 days");
        }

        public object GetName()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return Duration;

            return Name;
        }
    }

    
}

