using Microsoft.Extensions.FileProviders;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class ScheduleCreateVm
    {
        public int Id { get; set; }

        [Required]
        public DateTime ShiftDurationStart { get; set; }
        //[Required]
        public DateTime? ShiftDurationEnd { get; set; }

        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }

        //public double[] IgnoreDaysData
        //{
        //    get
        //    {
        //        return Array.ConvertAll(IgnoreDays.S(';'), Double.Parse);
        //    }
        //    set
        //    {
        //        InternalData = String.Join(";", value.Select(p => p.ToString()).ToArray());
        //    }
        //}


        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        public string Details { get; set; }
        public List<IFileInfo> Attachments { get; set; }
        public bool HasTime { get; set; }

        public bool IsRecurring { get; set; }
        public bool IsAllDay { get; set; }
        public bool IsForAllEmployees { get; set; }
        public bool IsForDepartment { get; set; }

        public List<DayVm> Days { get; set; }

        public int[] EmployeeIds { get; set; }
        public int DepartmentId { get; set; }
        public int WorkId { get; set; }

        public bool IsEffectiveImmediately { get; set; }
        public DateTime? EffectiveDate { get; set; }



        public RecurringFrequency RecurringFrequency { get; set; }
        public string ShiftName { get; internal set; }
        public ScheduleFor ScheduleFor { get; set; }
        public bool SaveAndRun { get; set; }
        public Dictionary<int, string> Shifts { get;  set; }

        public List<Work> Works { get; set; }

        public int MinsBeforeCheckin { get; set; }
        public WorkType WorkType { get; set; }
        public DateTime DueDate { get; internal set; }

        //public bool IsRun { get; internal set; }


        //public string WorkDuration => shif.HasValue && WorkEnd.HasValue ? WorkStart?.ToString("hh\\:mm") + "—" + WorkEnd?.ToString("hh\\:mm") : "";
        //public string Summary => ScheduleFor == ScheduleFor.Attendance ? $"WorkTime: {ShiftName} {WorkDuration}" : $"{ScheduleFor}: {WorkName}{(WorkDuration == "" ? "" : WorkDuration)}";

    }

    public class DayVm
    {
        public int _TotalWorks { get; set; }
        public int _TotalAttendance { get; set; }

        public string WorkDuration => WorkStart.HasValue && WorkEnd.HasValue ? WorkStart?.ToString("hh\\:mm") + "—" + WorkEnd?.ToString("hh\\:mm") : "";

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan? WorkStart { get; set; }
        public TimeSpan? WorkEnd { get; set; }

        [NotMapped]
        public int EmployeeId { get; set; }

        //[NotMapped]
        //public int IgnoreDays { get; set; }

        public bool IsOff { get; set; }
        public int CompanyId { get; set; }

        public string Color { get; set; }

        public string ShiftName { get; set; }
        public int ShiftId { get; set; }

        
        [NotMapped]
        public int[] WorkIds { get; set; }
        public string WorkName { get; set; }

        public string TimeZone { get; set; }
        public int MinHours { get; set; }
        public DateTime Date { get;  set; }
        public int Day { get;  set; }


        // for display payrol period values
        public int DayOffId { get; set; }
        public string DayOffName { get; set; }
        public int? CreatedFromRequestId { get;  set; }
        public List<_WorkTime> WorkTimes { get;  set; }
        public List<_Work> _Work { get;  set; }
    }

    public class WeeklyEmployeeShiftVm
    {
        public CompanyWorkTime WorkTime { get; set; }
        public Employee Employee { get; set; }
        public List<Attendance> Attendances { get; set; }
        public List<WorkItem> WorkItems { get; set; }
        public List<DayOffEmplVm> DayOffs { get; set; }

        public WeeklyEmployeeShiftVm()
        {
            Attendances = new List<Attendance>();
            WorkItems = new List<WorkItem>();
            DayOffs = new List<DayOffEmplVm>();
        }

    }


    public class WorkItemScheduleVvm
    {
        public Work Works { get; set; }
        public List<WorkItem> WorkItems { get; set; }


        public WorkItemScheduleVvm()
        {
            WorkItems = new List<WorkItem>();
        }

    }
}
