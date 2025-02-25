using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Models;
using Microsoft.AspNetCore.Mvc;
using Payroll.Controllers;
using Payroll.Models;

namespace Payroll.ViewModels
{
    public class HomeAdminVm
    {
        public int DepartmentCount { get; set; }
        public int EmployeeCount { get; set; }
        public Chart DepartmentByEmployeePie { get; set; }
        public List<Attendance> AttendanceTodayAndThisWeek { get; set; }
        public List<Employee> UpcomingBirthDays { get; set; }
        public List<CompanyAccount> NewCompaniesRegistering { get; set; }
        public PayrollPeriod LastPayrol { get; set; }
        public Chart JobTypeByEmployeePie { get; internal set; }
        public Chart AttendanceStatusByAttendancePie { get; set; }
        public int CompanyAccountCount { get; internal set; }
        public int WorkCount { get; internal set; }
        public int PayAdjustmentCount { get; internal set; }
        public List<Announcement> Announcements { get; internal set; }
        public Dictionary<RequestType, int> RequestNotificationsDictionry { get; internal set; }
        public int PendingTmeSheetCount { get; internal set; }
    }


    public class HomeEmployeeVm
    {
        public Employee Employee { get; set; }
        public List<WeeklyEmployeeShiftVm> AttedanceSchedule { get; set; }
        public List<Work> WorkSchedule { get; set; }
        public List<DayOffEmployee> DayOffs { get; internal set; }
        public List<WorkItem> WeekScheduleTasks { get; internal set; }
        public List<CompanyWorkTime> WorkTimes { get; internal set; }
        public List<Request> Requests { get; internal set; }
        public List<DayOffEmplVm> MyRequests { get; internal set; }
        public bool ShowCompleted { get; internal set; }
        public List<TaskData> TaskCompletionRates { get; set; }
        public List<Announcement> Announcements { get; internal set; }
        public List<CompanyPublicHoliday> PublicHolidaysUpcoming { get; internal set; }
        public int AwaitingRequestCount { get; internal set; }
        public List<DayOffEmployeeItem> DayOffEmpls { get; internal set; }
        public List<Notification> Notifications { get; internal set; }
        public string EmployeeUserId { get; internal set; }
        public FilterForm FilterForm { get; internal set; }
        public IEnumerable<Tuple<DateTime, double>> ThisWeekTimeSheetStats { get; internal set; }
        public Dictionary<string, decimal> DayOffBalances { get; internal set; }
        public Dictionary<string, decimal> DayOffUsages { get; internal set; }
        public Dictionary<string, int> LeaveRequestByStatus { get; internal set; }
        public Dictionary<DateTime, List<EmployeeSummaryVm>> BirthDaysInMonth { get; internal set; }
        public Dictionary<DateTime, List<EmployeeSummaryVm>> WorkAnniversaries { get; internal set; }
        public List<Employee> MyTeam { get; internal set; }
        public List<EmployeeSummaryVm> WhoIsOut { get; internal set; }
        public List<Employee> MyDepartment { get; internal set; }

        //public List<LEave { get; set; }
    }


    public class FilterForm
    {
        public FilterForm()
        {

        }

        public FilterForm(int id, DateTime start, DateTime end)
        {
            EmployeeId = id;
            Start = start;
            End = end;
        }

        public int EmployeeId { get; set; }
        public string UserId { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int Type { get; set; }
        public bool ShowSeen { get; set; }
        public int Page { get; internal set; }
        public int Limit { get; internal set; }
        public TimeFrame? TimeFrame { get; internal set; }
        public PerformanceIndicator? Indicator { get; internal set; }
        public string DurationText { get; internal set; }
    }
}