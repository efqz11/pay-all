using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Payroll.Models.ViewModels
{

    public class RequestActionVm
    {
        public bool IsApproved { get; set; }
        public string ActionTakenReasonSummary { get; set; }
        public int Id { get; set; }
    }

    public class ProcessRequestVm
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string NumberFormat { get; set; }

        public int CompanyId { get; set; }
        [JsonIgnore]
        public virtual Company Company { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }

        //public Employee Employee { get; set; }

        public DateTime CreationDate { get; set; }
        public RequestType RequestType { get; set; }


        public DateTime? SubmissionDate { get; set; }


        public string ActionTakenUserId { get; set; }
        public string ActionTakenUserName { get; set; }
        public DateTime? ActionTakenDate { get; set; }
        public string ActionTakenReasonSummary { get; set; }


        public WorkItemStatus Status { get; set; }



        // leave request
        public int? DayOffId { get; set; }
        //[JsonIgnore]
        //public DayOff DayOff { get; set; }

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string Duration => Start?.ToString("h tt").ToLower() + " - " + End?.ToString("h tt").ToLower() + (Start?.Date != End?.Date ? "*" : "");

        public string Reason { get; set; }
        public bool IsCustomReason { get; set; }

        //public int? DayOffEmployeeItemId { get; set; }
        //public DayOffEmployeeItem DayOffEmployeeItem { get; set; }

        // paternity leave

        // Overtime && Attendance_Change && Work_Change
        public int? AttendanceId { get; set; }
        //[JsonIgnore]
        //public Attendance Attendance { get; set; }

        public int? WorkItemId { get; set; }
        //[JsonIgnore]
        //public WorkItem WorkItem { get; set; }


        public bool IsTransferEmployee { get; set; }
        public string TransferredEmployeeName { get; set; }

        [ForeignKey("TransferredEmployee")]
        public int? TransferredEmployeeId { get; set; }

        //public Employee TransferredEmployee { get; set; }




        public TimeSpan? NewCheckinTime { get; set; }
        public TimeSpan? NewCheckOutTime { get; set; }


        [NotMapped]
        public TimeSpan? _StartTime { get; set; }
        [NotMapped]
        public TimeSpan? _EndTime { get; set; }
        public bool IsOvertime => _EndTime < _StartTime;
        [NotMapped]
        public string[] Documents { get; set; }
        [NotMapped]
        public bool? IsApproved { get; set; }

        public string ImagePath { get; set; }
        //public List<ImagePath> DocumentsData { get; set; }


        public string DocumentsData { get; set; }
    }
}
