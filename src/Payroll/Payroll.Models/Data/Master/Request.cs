using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Payroll.Filters;
using Payroll.Services;

namespace Payroll.Models
{
    [AuditableEntity]
    public class Request : Audit
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Number { get; set; }
        public string NumberFormat { get; set; }

        public int CompanyId { get; set; }
        [JsonIgnore]
        public virtual Company Company { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

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
        public DayOff DayOff { get; set; }

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
        [JsonIgnore]
        public Attendance Attendance { get; set; }

        public int? WorkItemId { get; set; }
        [JsonIgnore]
        public WorkItem WorkItem { get; set; }


        public bool IsTransferEmployee { get; set; }
        public string TransferredEmployeeName { get; set; }


        [ForeignKey("TransferredEmployee")]
        public int? TransferredEmployeeId { get; set; }

        public Employee TransferredEmployee { get; set; }




        public TimeSpan? NewCheckinTime { get; set; }
        public TimeSpan? NewCheckOutTime { get; set; }
        //public string NewCheckInCheckOutDuration => GetNewCheckInCheckOutDuration();

        public int TotalDays { get; set; }

        [DefaultValue(0.00f)]
        public double TotalHours { get; set; }
        public int DataChangesCount { get; set; }
        // Dispute

        // document
        public string DocumentType { get; set; }
        public bool IsLetter { get; set; }


        [NotMapped]
        public TimeSpan? _StartTime { get; set; }
        [NotMapped]
        public TimeSpan? _EndTime { get; set; }
        public bool IsOvertime => _EndTime < _StartTime;
        [NotMapped]
        public string[] Documents { get; set; }
        [NotMapped]
        public bool? IsApproved { get; set; }
        [NotMapped]
        public bool IsLastStep { get; set; }

        public string ImagePath { get; set; }
        //public List<ImagePath> DocumentsData { get; set; }


        public string DocumentsData { get; set; }
        public EntityType? DataChangeEnttity { get; set; }

        [NotMapped]
        public string[] DocumentsDataArray
        {
            get { return DocumentsData?.Split(',', StringSplitOptions.RemoveEmptyEntries); }
            set { DocumentsData = string.Join(',', value); }
        }

        public IList<RequestStatusUpdate> StatusUpdates { get; set; }
        public List<DayOffEmployeeItem> DayOffEmployeeItems { get; set; }
        public List<RequestDataChange> RequestDataChanges { get; set; }
        public List<JobActionHistory> JobActionHistories { get; set; }
        public List<FileData> FileDatas { get; set; }



        public string GetStatusString()
        {
            switch (Status)
            {
                case WorkItemStatus.Submitted: return GetIcon() + " Submitted";
                case WorkItemStatus.Approved: return GetIcon() + " Approved";
                case WorkItemStatus.Rejected: return GetIcon() + " Rejected";
                case WorkItemStatus.Draft:
                    return GetIcon() + " Created";
                default:
                    return "";
            }
        }
        public string GetIcon()
        {
            switch (Status)
            {
                case WorkItemStatus.Submitted: return $"<i class='fa fa-circle fa-sm text-primary' title=''></i>";
                case WorkItemStatus.Approved: return $"<i class='fa fa-circle fa-sm text-success'></i>";
                case WorkItemStatus.Rejected: return $"<i class='fa fa-circle fa-sm text-danger' title=''></i>";
                case WorkItemStatus.Draft:
                    return $"<i class='fa fa-circle fa-sm text-secondary'></i>";
                default:
                    return "";
            }
        }


        public string GetTotalDaysStringInfo()
        {
            var calcDays = Convert.ToInt32(((End?.Date - Start?.Date)?.TotalDays) + 1 ?? 1);
            if (TotalDays != calcDays)
                return "<i class='fal fa-info-circle text-warning' title='Some days may be excluded'></i>";
            return "";
        }

        public string GetNewCheckInCheckOutDuration(ClaimsPrincipal User)
        {
            try
            {
                return DateTime.Today.Add(NewCheckinTime.Value).ToSystemFormat(User, onlyTime: true).ToLower() + " - " + DateTime.Today.Add(NewCheckOutTime.Value).ToSystemFormat(User, onlyTime: true).ToLower();
            }
            catch (Exception)
            {
                return "Undefined Time(s)";
            }
        }

        public string GetRequestedDuration()
        {
            try
            {
                if (Start.HasValue && End.HasValue)
                {
                    var calcDays = Convert.ToInt32(((End?.Date - Start?.Date)?.TotalDays) + 1 ?? 1);
                    var day = calcDays + (calcDays > 1 ? " days" : "day");
                    if (calcDays <= 1)
                        return Start?.ToString("ddd, MMM dd, yyyy");
                    else
                        return Start?.ToString("MMM dd") + " - " + End?.ToString(Start?.Month != End?.Month ? "MMM dd, yyyy" : "dd, yyyy") + " (" + TotalDays + ")";
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }


        public Request()
        {
            FileDatas = new List<FileData>();
            StatusUpdates = new List<RequestStatusUpdate>();
            RequestDataChanges = new List<RequestDataChange>();
            DayOffEmployeeItems = new List<DayOffEmployeeItem>();
            JobActionHistories = new List<JobActionHistory>();
        }
        
    }

    //public class DocumentData
    //{
    //    public string Data { get; set; }
    //}


    public class RequestStatusUpdate
    {
        public WorkItemStatus Status { get; set; }
        public string StatusString { get; set; }
        public string ChangedByName { get; set; }
        public string ChangedByUserId { get; set; }
        public DateTime? ChangedDate { get; set; }
        public double TotalDurationInHours { get; set; }

        public bool IsFromScheduler { get; set; }
        public DateTime? UpdateDate { get; set; }
    }

}
