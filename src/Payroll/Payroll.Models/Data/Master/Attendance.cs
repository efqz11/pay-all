
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Attendance : Audit
    {
        public int Id { get; set; }

        public int? ScheduleId { get; set; }
        public Schedule Schedule { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee  { get; set; }


        public int Week { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        [Display(Name = "Attendance for the day")]
        public DateTime Date { get; set; }


        public int? CompanyWorkTimeId { get; set; }
        public CompanyWorkTime CompanyWorkTime { get; set; }


        public int ShiftId { get; set; }
        public string ShiftName { get; set; }
        public string ShiftColor { get; set; }


        public DateTime WorkStartTime { get; set; }
        public DateTime WorkEndTime { get; set; }


        /// <summary>
        ///  employee data entry
        /// </summary>
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public bool IsCheckinUpdated { get; set; }
        public bool IsCheckOutUpdated { get; set; }


        // =========[ work ]===========
        // ====[ attnd  ]========= (TotalEarlyMins)
        // ============[ attnd  ]======== (TotalLateMins)
        // =======[ attnd  ]======== (TotalLateMins)


        /// <summary>
        ///  =========[ work ]===========
        //  ====[ attnd  ]========= )
        //  ====[ __  ]===============
        /// </summary>
        public double TotalEarlyMins { get; set; }

        /// <summary>
        ///  =========[ work ]===========
        //  =============[ attnd  ]=========
        //  ==========[_ ]===============
        /// </summary>
        public double TotalLateMins { get; set; }


        /// <summary>
        ///  =========[ work ]===========
        //  =============[ attnd  ]=========
        //  =================[ __ ]==========
        /// </summary>
        public double TotalAfterWorkMins { get;  set; }

        /// <summary>
        ///  =========[ work ]===========
        //  =============[ attnd  ]=========
        //  =============[ _____  ]=========
        /// </summary>
        public double TotalWorkedHours { get; set; }


        /// <summary>
        ///  =========[ work ]===========
        //  =============[ attnd  ]=========
        //  =============[ _ ]=========
        /// </summary>
        public double TotalHoursWorkedPerSchedule { get;  set; }

        /// <summary>
        /// Same as Total Worked per schedule 
        /// Company will set this option
        /// </summary>
        public double TotalWorkedHoursCalculated { get; set; }


        /// <summary>
        ///  =========[ work ]===========
        //  =============[ attnd  ]=========
        //  =================[ __ ]=========
        /// </summary>
        public double TotalHoursWorkedOutOfSchedule { get;  set; }


        /// <summary>
        ///  =========[ work ]===========
        //  =============[ attnd  ]=========
        //  ==========[  ]================
        /// </summary>
        public double TotalDefecitHours { get; set; }

        /// <summary>
        /// TotalHoursWorkedOutOfSchedule > 0
        /// And can create OT Request from here
        /// </summary>
        public bool WorkedOverTime { get; set; }


        public bool HasClockRecords { get; set; }
        public bool IsTransferred { get; set; }

        /// <summary>
        /// indicates if the OT amount (out of schedule, +8hr) has been claimed (requested and approved)
        /// </summary>
        public bool IsOTAmountClaimed { get; set; }

        //[ForeignKey("TransferrdEmployee")]
        //public bool IsTransferred { get; set; }

        //public Employee TransferrdEmployee { get; set; }
        //public DateTime? TranferedDate { get; set; }

        


        public AttendanceStatus CurrentStatus { get; set; }
        public string StatusString => GetAttendanceStatusString();

        private string GetAttendanceStatusString()
        {
            switch (CurrentStatus)
            {
                case AttendanceStatus.Absent: return $"<i class='fa fa-circle fa-sm text-dark' title='Absent'></i>";
                case AttendanceStatus.Early: return $"<i class='fa fa-circle fa-sm text-primary' title='{(int)TotalEarlyMins}mins early'></i> Early";
                case AttendanceStatus.OnTime: return $"<i class='fa fa-circle fa-sm text-success' title='On time'></i>";
                case AttendanceStatus.Late: return $"<i class='fa fa-circle fa-sm text-danger' title='Late ({(int)TotalLateMins}mins late)'></i> ";
                case AttendanceStatus.Created:
                    return $"<i class='fa fa-circle fa-sm text-secondary' title='Created'></i>";
                case AttendanceStatus.Waiting:
                    return $"<i class='fa fa-circle fa-sm text-warning' title='Waiting'></i> ";
                case AttendanceStatus.Recieved:
                case AttendanceStatus.Edited:
                case AttendanceStatus.Published:
                    return $"<i class='fa fa-circle fa-sm {(CheckInTime.HasValue && CheckInTime.Value < WorkStartTime.AddMinutes(-10) ? "text-primary" : CheckInTime.HasValue && CheckInTime.Value > WorkStartTime ? "text-danger" : "text-success")}' title='{(CheckInTime.HasValue && CheckInTime.Value < WorkStartTime.AddMinutes(-10) ? "Early" : CheckInTime.HasValue && CheckInTime.Value > WorkStartTime ? "Late" : "On time")}'></i>";
                default:
                    return "";
            }
        }



        public string Duration => WorkStartTime.ToString("htt").ToLower() + " - " + WorkEndTime.ToString("htt").ToLower() + (WorkStartTime.Date != WorkEndTime.Date ? "*" : "");
        public string DurationCheckin => (CheckInTime?.ToString("hh:mm tt")?.ToLower() ?? "NA") + " - " + (CheckOutTime?.ToString("hh:mm tt")?.ToLower() ?? "NA");


        //internal string _statusUpdates { get; set; }


        public IList<AttendanceStatusUpdate> StatusUpdates { get; set; }
        public IList<BiometricRecord> BiometricRecords { get; set; }
        public IList<Request> Requests { get; set; }
        public bool bySchduler { get;  set; }


        public bool IsCreatedFromRequest { get; set; }
        public int? CreatedFromRequestId { get; set; }
        public bool IsOvertime { get;  set; }
        
        public bool IsPublished { get;  set; }
        public DateTime PublishedDate { get;  set; }
        public bool HasError { get;  set; }
        public string ErroMsg { get;  set; }
        public double TotalBreakHours { get;  set; }



        //[NotMapped]
        //public AttendanceStatusUpdate[] StatusUpdates
        //{
        //    get { return _statusUpdates == null ? null : JsonConvert.DeserializeObject<AttendanceStatusUpdate[]>(_statusUpdates); }
        //    set { _statusUpdates = JsonConvert.SerializeObject(value); }
        //}


        public Attendance()
        {
            //StatusUpdates = new List<AttendanceStatusUpdate>();
            BiometricRecords = new List<BiometricRecord>();
            Requests = new List<Request>();
        }
    }

    public class AttendanceStatusUpdate
    {
        public AttendanceStatus Status { get; set; }
        public string StatusString { get; set; }
        public string ChangedByName { get; set; }
        public string ChangedByUserId { get; set; }
        public DateTime? ChangedDate { get; set; }
        public int TotalDurationIn { get; set; }

        public bool IsFromScheduler { get; set; }
    }

}
