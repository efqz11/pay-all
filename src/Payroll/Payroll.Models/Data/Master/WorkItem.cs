using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class WorkItem : Audit
    {
        public int Id { get; set; }

        public int? WorkId { get; set; }
        public Work Work { get; set; }

        public int? ScheduleId { get; set; }
        public Schedule Schedule { get; set; }


        public int Week { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        [Display(Name = "Task for the day")]
        public DateTime Date { get; set; }


        public bool HasAttendance { get; set; }
        public int? AttendnaceId { get; set; }
        public Attendance Attendance { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }



        [Display(Name = "Work Start Date/Time")]
        public DateTime WorkStartTime { get; set; }
        [Display(Name = "Work End Date/Time")]
        public DateTime WorkEndTime { get; set; }
        public string WorkDurationTime => WorkStartTime.TimeOfDay.Equals(WorkEndTime.TimeOfDay) ? "all day": WorkStartTime.ToString("hh:mm tt").ToLower() + " - " + WorkEndTime.ToString("hh:mm tt").ToLower();

        public string DurationCheckin => (CheckInTime?.ToString("hh:mm tt")?.ToLower() ?? "NA") + " - " + (CheckOutTime?.ToString("hh:mm tt")?.ToLower() ?? "NA");

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }


        /// <summary>
        ///  employee data entry
        /// </summary>
        [Display(Name = "Check-In Time")]
        public DateTime? CheckInTime { get; set; }
        [Display(Name = "Check-Out Time")]
        public DateTime? CheckOutTime  { get; set; }

        public double TotalEarlyMins { get; set; }
        public double TotalLateMins { get; set; }

        public double TotalWorkedMins { get; set; }

        // ------------- CALCULATED  ------/
        public int TotalApproved { get; set; }
        public int PercentApproved { get; set; }

        public int TotalSubmitted { get; set; }
        public int PercentSubmitted { get; set; }

        public int RemainingSubmissions { get; set; }

        public decimal TotalAmountCredited { get; set; }
        public decimal TotalAmountDeducted { get; set; }

        public bool IsCompleted { get; set; }
        public WorkItemStatus Status { get; set; }


        public List<WorkItemSubmission> WorkItemSubmissions { get; set; }
        public string WorkColor { get; internal set; }


        public bool IsEmployeeTask { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public string WorkName => IsEmployeeTask ? TaskName : (Work?.Name ?? "NA");

        public List<Request> Requests { get; set; }
        public bool IsTransferred { get; set; }
        public bool IsCheckinUpdated { get;  set; }
        public bool IsCheckOutUpdated { get;  set; }
        public bool IsPublished { get;  set; }
        public DateTime PublishedDate { get;  set; }

        //public decimal TotalAllowance { get; set; }

        //public int GetApprovePercent()
        //{
        //    if(Work == null) return 0;
        //    return Work.IsAdvancedCreate ? Convert.ToInt32(Math.Round((TotalApproved / decimal.Parse(Work.TotalRequiredCount.ToString())) * 100m, 1)) : 0;
        //}

        public int GetSubmittedPercent(int totalSubmitted)
        {
            if (Work == null) return 0;
            return Work.IsAdvancedCreate ? Convert.ToInt32(Math.Round(((totalSubmitted) / decimal.Parse(Work.TotalRequiredCount.ToString())) * 100m, 1)) : 0;
        }

        public string GetCssStyleFromPercent(int percent) =>
            "progress_" + (percent <= 5 ? 5 : percent <= 25 ? 25 : percent <= 50 ? 50 : percent <= 75 ? 75 : 100);

        public WorkItem()
        {
            Requests = new List<Request>();
            WorkItemSubmissions = new List<WorkItemSubmission>();
        }

        public WorkItem(DateTime onDate)
        {
            Date = onDate;
            Year = onDate.Year;
            Month = onDate.Month;
            Day = onDate.Day;
            Requests = new List<Request>();
            WorkItemSubmissions = new List<WorkItemSubmission>();
        }
    }
}
