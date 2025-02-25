
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
    public class BiometricRecord : Audit
    {
        public int Id { get; set; }

        //public int? AttendanceId { get; set; }
        //public Attendance Attendance { get; set; }

        public int CompanyId { get; set; }
        [JsonIgnore]
        public Company Company { get; set; }

        public int EmployeeId { get; set; }
        [JsonIgnore]
        public Employee Employee  { get; set; }

        public int? AttendanceId { get; set; }
        [JsonIgnore]
        public Attendance Attendance { get; set; }
        public int? WorkItemId { get; set; }
        [JsonIgnore]
        public WorkItem WorkItem { get; set; }


        public int Week { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        [Display(Name = "Attendance DAY")]
        public DateTime Date { get; set; }

        [Display(Name = "Attendance TIME")]
        public TimeSpan Time { get; set; }

        [Display(Name = "Attendance DATE-TIME")]
        public DateTime DateTime { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }


        public BiometricRecordType BiometricRecordType { get; set; }
        public BiometricRecordState BiometricRecordState { get; set; }

        public string Location { get; set; }
        public bool IsManualEntry { get; internal set; }
        public int OrderBy { get; set; }

        public string Data { get; set; }

        //public List<BiometricRecordRelate>  BiometricRecordRelates { get; set; }
        public string MachineId { get;  set; }

        public BiometricRecord()
        {
            //BiometricRecordRelates = new List<BiometricRecordRelate>();
            //Requests = new List<Request>();
        }

        public void SetDates()
        {
            Date = DateTime.Date;
            Time = DateTime.TimeOfDay;
            Year = DateTime.Year;
            Month = DateTime.Month;
            Day = DateTime.Day;
            Hour = DateTime.Hour;
            Minute = DateTime.Minute;
            Second = DateTime.Second;
        }
    }

    public enum BiometricRecordType
    {
        FingerPrint,
        RetinaAndIrisPattern,
        Voice,
        DNA
    }

    public enum BiometricRecordState
    {
        [Display(Name = "CHECK-IN")]
        CheckIn,
        [Display(Name = "CHECK-OUT")]
        CheckOut,
        [Display(Name = "BREAK-IN")]
        BreakIn,
        [Display(Name = "BREAK-OUT")]
        BreakOut,

        //BizIn,
        //BizOut
    }

}
