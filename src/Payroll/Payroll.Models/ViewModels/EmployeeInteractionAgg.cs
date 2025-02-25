using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class EmployeeInteractionAgg
    {
        public double WorkedHours { get; set; }
        public double WorkedMins { get; set; }

        public double LateMins { get; set; }
        public double LateHours { get; set; }
        public int LateDays { get; set; }

        //public double AbsentMins { get; set; }
        //public double AbsentHours { get; set; }
        public int AbsentDays { get; set; }


        public double OvertimeHours { get; set; }
        public double OvertimeMins { get; set; }

        public double LeaveDays { get; set; }

        public double TaskCompletedCount { get; set; }
        public double TaskFailedCount { get; set; }
        public decimal TaskCreditSum { get; set; }
        public decimal TaskDebitSum { get; set; }
        public TaskAggByWorkId[] TaskAggByWorkIds { get; set; }


        public int OvertimeCount { get;  set; }
        public int WorkedRecordsCount { get;  set; }
        public int EmployeeId { get;  set; }
        public int TaskSubmissionsCount { get; set; }
        public int TaskRemainingCount { get; set; }

        public DateTime OnDate { get; set; }
        public int DisciplinaryActionsCount { get; set; }
        public string DateString { get;  set; }
    }




    public class TaskAggByWorkId
    {
        public int WorkId { get; set; }
        public int TaskCompletedCount { get; set; }
        public int TaskFailedCount { get; set; }
        public decimal TaskCreditSum { get; set; }
        public decimal TaskDebitSum { get; set; }
    }

}
