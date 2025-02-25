using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class PayrollVm
    {

        public Payroll.Models.PayrollPeriod PayrollPeriod { get; set; }
        //public Employee Employee { get; set; }
        public List<EmployeeRecord> EmployeeRecords { get; set; }
        public List<PayAdjustment> AllPayAdjustments { get; set; }
        public List<PayAdjustment> PayrolPayAdjustments { get; set; }
        public int PayrolPeriodId { get; set; }
        public ParolTabs ActiveTab { get; set; }
        public List<EmployeePayAdjustmentTotal> EmployeePayAdjustmentTotals { get; set; }
        public List<EmployeeInteractionAgg> EmployeeInteractions { get; set; }
        public Dictionary<int, string> RecentPeriods { get; set; }
        public DateTime? SelectedDate { get; set; }
        public Dictionary<string, int> RelatedAttendanceChart { get; set; }
        public List<BiometricRecord> RelatedTimeSheet { get; set; }
        public List<Attendance> RelatedAttendances { get; set; }
        public List<EmployeeStatusDays> EmployeeStatusDays { get; set; }
        public int _EmpTotal_NoContract { get; set; }
        public int _EmpTotal_ExpiringContract { get; set; }
        public int _EmpTotal_NewContract { get; set; }
        public int _EmpTotal_ActiveContract { get; set; }
        public List<Employee> _ActiveContracts { get; set; }
        public List<KeyValuePair<DateTime, string>> PublicHolidaysDuringPeriod { get; set; }

        public PayrollVm()
        {
            EmployeePayAdjustmentTotals = new List<EmployeePayAdjustmentTotal>();
            EmployeeStatusDays = new List<EmployeeStatusDays>();
        }

    }

    public class EmployeeStatusDays
    {
        public int EmployeeId { get; set; }
        public AttendanceStatus CurrentStatus { get; set; }
        public int DaysCount { get; set; }
        public Employee Employee { get; set; }
        public int TotalEarlyMins { get; set; }
        public double TotalLateMins { get; set; }
        public double TotalHoursWorkedPerSchedule { get; set; }
        public double TotalWorkedHours { get; set; }

        public string GetAttendanceStatusString()
        {
            switch (CurrentStatus)
            {
                case AttendanceStatus.Absent: return $"<i class='fa fa-circle fa-sm text-dark' title=''></i> Absent";
                case AttendanceStatus.Early: return $"<i class='fa fa-circle fa-sm text-primary' title='{(int)TotalEarlyMins}mins early'></i> Early";
                case AttendanceStatus.OnTime: return $"<i class='fa fa-circle fa-sm text-success'></i> On time";
                case AttendanceStatus.Late: return $"<i class='fa fa-circle fa-sm text-danger' title='{(int)TotalLateMins}mins late'></i> Late ";
                case AttendanceStatus.Created:
                    return $"<i class='fa fa-circle fa-sm text-secondary'></i> Created";
                case AttendanceStatus.Waiting:
                    return $"<i class='fa fa-circle fa-sm text-warning'></i> Waiting";
                case AttendanceStatus.Recieved:
                case AttendanceStatus.Edited:
                default:
                    return "";
            }
        }
    }

    public class EmployeePayAdjustmentTotal
    {
        public int PayAdjustmentId { get; set; }
        public string PayAdjustmentName { get; set; }
        public decimal Total { get; set; }
    }

    public enum ParolTabs
    {
        PayPeriod = 1,
        Adjsutments,
        Finals
    }


    public class EmployeeRecord
    {

        public Employee Employee { get; set; }
        public List<Attendance> Attendances { get; set; }
        public List<DayVm> DayOffEmployees { get; set; }
        public List<WorkItem> WorkItems { get; set; }

        //public DayVm Day { get; set; }

        public EmployeeRecord()
        {
            Attendances = new List<Attendance>();
            DayOffEmployees = new List<DayVm>();
            WorkItems = new List<WorkItem>();
        }

    }


}
