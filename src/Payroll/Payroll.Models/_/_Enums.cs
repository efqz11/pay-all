
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public enum SurveyQn_HowDoRunPayroll{
        [Display(Name = "We’re new to running payroll")]
        New,
        [Display(Name = "We use another online payroll software")]
        Online,
        [Display(Name = "We do it ourselves manually")]
        Manual
    }

    public enum Survey_BusinessSetting{
        [Display(Name = "We work in an office")]
        Office,
        [Display(Name = "We work in retail or food service")]
        ReatailOrFood,
        [Display(Name = "We work in a factory, construction, or in the field")]
        FactoryOrConstruction
    }


    public enum ReportCategory
    {
        [Display(Name = "Employee Info")]
        EmployeeInfo,
        [Display(Name = "Time-off")]
        TimeOff,
        [Display(Name = "Data Review")]
        DataReview,
        Payroll,
        Attendance,
    }


    public enum AccrualMethod
    {
        [Display(Description = "Employees earn a set amount of time every year. For example, 120 hours per year")]
        Fixed = 1,
        [Display(Description = "Employees earn time based on the hours they work. For example, 1 hour per 20 hours worked")]
        Hourly,
    }
    public enum AccureTimeBasedOn
    {
        [Display(Name = "Hours worked only", Description = "Include only approved work hours")]
        HoursWorked = 1,
        [Display(Name = "All hours paid", Description = "This will include any paid time off that the employee has taken, including any company holidays.")]
        AllHoursPaid,
    }

    public enum UserStatus {
        PendingEmailVerification,
        PendingCustomizeSetup,
        PendingCompanySetup
    }

    public enum CompanyEntityType
    {
        [Display(Name = "C-CORPORATION")]
        C_CORPORATION = 1,
        [Display(Name = "S-CORPORATION")]
        S_CORPORATION,
        [Display(Name = "Sole proprietor")]
        SOLE_PROPRIETOR,
        LLC,
        LLP,
        [Display(Name = "Limited partnership")]
        LIMITED_PARTNERSHIP,
        [Display(Name = "Co-ownership")]
        CO_OWNERSHIP,
        [Display(Name = "Association")]
        ASSOCIATION,
        [Display(Name = "Trusteeship")]
        TRUSTEESHIP,
        [Display(Name = "General partnership")]
        GENERAL_PARTNERSHIP,
        [Display(Name = "Joint venture")]
        JOINT_VENTURE,
        [Display(Name = "Non-Profit")]
        NON_PROFIT
    }

    public enum GroupByCategory
    {
        Location,
        Department,
        JobType,
        ChooseEmployees
    }

    public enum MeasureUsing
    {
        [Display(Name = "Percentage (%)")]
        Percentage,
        [Display(Name = "Volume (numeric)")]
        Volume,
        [Display(Name = "Currency ($)")]
        Dollars
    }

    public enum GoalType
    {
        Individual,
        [Display(Name = "My Team (Direct Reports)")]
        MyTeam,
        Company,
    }

    public enum ContractSort
    {
        None,
        Recent,
        Expiring,
        Long,
        Short
    }

    /// <summary>
    /// Probation (Monthly)
    /// Full Time (Monthly)
    /// Part Time (Wage)
    /// Contract (Wage)
    /// </summary>
    public enum ContractType
    {
        PROBATION, //Monthly
        PART_TIME, // WAGE
        FULL_TIME, // Monthly
        CONTRACT // WAGE
    }

    public enum PayingTo
    {
        Myself,
        [Display(Name = "Full-time and Part-time employees")]
        W2Employees,
        [Display(Name = "Self-employed independent contractors")]
        Contracts,
        //[Display(Name = "Both Permanent employments and Self-employed independent contractors")]
        //Both,
        [Display(Name = "We’re not planning to pay anyone for at least 3 months")]
        NoneForLeastNext3
    }
    public enum HowToTrackTime
    {
        [Display(Name = "We want to use a time tracking integration")]
        Integration,
        [Display(Name = "We don’t want to track time in Payall—we’ll do it manually")]
        Manual,
        [Display(Name = "We want to use Payall’s built-in time tracking feature")]
        InBuilt
    }


    public enum MaritialStatus
    {
        Undefined,
        Single, 
        Married, 
        Divorced,
        Widowed,
    }


    public enum JobType
    {
        [Display(Name = "Full-Time")]
        FullTime,
        [Display(Name = "Part-Time")]
        PartTime,
        Contract,
    }

    public enum JobStatus
    {
        Vacant,
        PartiallyOccupied,
        Occupied,
        Abolished,
        Reserved,
        ProcessingEmployment,
        ProcessingTermination,
        ProcessingPromotion,
    }

/// <summary>
/// Running status of employee (not used for HR purpose)
/// for example, ongoing promotion, terminaton, suspension, etc.
/// </summary>
    public enum EmployeeStatus
    {
        Incomplete,
        ActionNeeded,
        Active,
        Dismissed,

        [Display(Name = "On-Leave")]
        OnLeave,
        [Display(Name = "On-Suspension")]
        OnSuspension,

        //[Display(Name = "Full-Time")]
        //FullTime,
        //[Display(Name = "Part-Time")]
        //PartTime,
        //Intern,
        Terminated
    }

    public enum EmployeeType
    {
        None,
        Permanent,
        Contract,
        [Display(Name = "Salary/No Overtime")]
        Salary_NoOvertime = 1,
        [Display(Name = "Salary/Eligible for overtime")]
        Salary_Overtime,
        [Display(Name = "Paid by the hour")]
        PaidByHour,
    }

    public enum EmployeeSecondaryStatus
    {
        [Display(Name = "Offer not sent")]
        OfferNotSent,
        [Display(Name = "Offer sent, waiting for action")]
        OfferSent,
        [Display(Name = "Offer action taken")]
        OfferActionTaken,
        [Display(Name = "Waiting to finish self-onboarding")]
        WaitingSelfOnBoard,
        [Display(Name = "Personal info missing")]
        PersonalInfoMissing,
        
        [Display(Name = "Employment info missing")]
        EmploymentInfoMissing,
        None,
    }


    //public enum EmployeeType
    //{
    //    None,
    //    Permanent,
    //    Contract,
    //    Wage,
    //}

    public enum EmploymentTypeOther
    {
        Internal,
        External,
    }

    public enum LengthOfProbation
    {
        [Display(Name = "No probation")]
        None,
        [Display(Name = "1 month")]
        OneMonth,
        [Display(Name = "2 months")]
        TwoMonth,
        [Display(Name = "3 months")]
        ThreeMonths,
        [Display(Name = "4 months")]
        FounrMonths,
        [Display(Name = "5 months")]
        FiveMonths,
        [Display(Name = "End of probation period")]
        End,
    }

    /// <summary>
    /// Actual Human Resource Status
    /// </summary>
    public enum HrStatus
    {
        Recruitement,
        Employed,
        Probation,
        Permanant,
        Suspended,
        Terminated
    }


    public enum PayFrequency
    {
        PayPeriod
    }

    public enum RecordStatus
    {
        Incomplete,
        Active,
        Archived,
        InActive
    }

    public enum VisaStatus
    {
        Current,
        Expired
    }

    public enum VisaType
    {
        Local,
        
        [Display(Name = "Business Visa")]
        Business,
        
        [Display(Name = "Dependent Visa")]
        Dependent,

        [Display(Name = "WP transfer")]
        Work,

        [Display(Name = "Marriage Visa")]
        Marriage
    }


    public enum PayComponentChangeReason
    {
        Promotion,
        Demotion
    }

    public enum IdentityType
    {
        [Display(Name = "National ID No.")]
        NationalID,
        NID,
        [Display(Name = "Passport")]
        Passport,
        [Display(Name = "Social Security No.")]
        SSN,
        TBD,
    }

    public enum Gender
    {
        Male,
       Female,
       Other,
       TBD,
    }

    public enum RequireSubsitute
    {
        Yes,
        No,
        Optional
    }

    public enum Initial
    {
        Mr = 1,
        Mrs,
        Ms,
        Dr,
        Prof,
    }

    public enum FileType
    {
        BankDetails,
        IdentityProof,
        Employee,
        WorkItem,
        Request,
        CompanyFile
    }

    public enum CompanyFileType
    {
        Document,
        OfferTemplate,
    }
    public enum CompanyFileShareType
    {
        Individual,
        Team,
    }

    public enum CompanyStatus
    {
        [Display(Name = "Pending")]
        Pending,

        Approved,
        Closed,
        Rejected,
        Draft
    }

    public enum AttendanceStatus
    {
        // record created
        Created,
        Waiting,
        Recieved,
        Edited,
        Published,
        Early,
        OnTime,
        Late,
        Absent,
        EmployeeOnLeave
    }

    public enum MsgAlertType{
        info,
        warning,
        success,
        danger,
        
    }

    public enum AlertType
    {
        [Display(Name = "5 minutes before")]
        Before_5_Min,
        [Display(Name = "10 minutes before")]
        Before_10_Min,
        [Display(Name = "15 minutes before")]
        Before_15_Min,
        [Display(Name = "30 minutes before")]
        Before_30_Min,
        [Display(Name = "1 hour before")]
        Before_1_Hour,
        [Display(Name = "2 hours before")]
        Before_2_Hour,
        [Display(Name = "1 day before")]
        Before_1_Day,
        [Display(Name = "2 days before")]
        Before_2_Day,
        [Display(Name = "1 week before")]
        Before_1_Week,
    }

    public enum WhenToApplyPaidTimeOffPolicyAfterJoining
    {
        [Display(Name = "Immediately")]
        Immediately,
        [Display(Name = "After 3 months")]
        After_3_months,
        [Display(Name = "After 6 months")]
        After_6_months,
        [Display(Name = "After 1 year")]
        After_1_year
    }


    public enum AddressType
    {
        NA,
        [Display(Name = "Present Address")]
        Present,
        [Display(Name = "Permanant Address")]
        Permanant,
        Company,
    }
    public enum PayrollStatus
    {
        Draft,
        OnGoing,
        PendingPayment,
        Complete
    }
    public enum EmployeeNotificationType
    {
        None,
        Approval,
        Reminder,
    }

    public enum WorkItemStatus
    {
        Draft,
        Submitted,
        //PendingApproval,
        Approved,
        Rejected,
        Completed,
        FailedWithDeduction,
    }

    public enum WorkType
    {
        None = 0,

        [Display(Name = "Expect ClockIn Records")]
        ExpectClockInRecordsBefore = 1,

        [Display(Name = "Require Submissions")]
        RequireSubmissions
    }

    public enum TaskType
    {
        ScheduleWorkItemNextRun,
        PayrolSheetGenerated,

        EmployeeEnrollment,
        EmployeeProbationStart,
        EmployeeProbationEnd,
        EmployeePromotionStart,
        EmployeePromotionEnd,
        EmployeeTermination,

        [Display(Name = "Publish Announcement")]
        ExpireAnnouncement,
        [Display(Name = "Expire Announcement")]
        PublishAnnouncement
    }

    public enum EmployeeSelectorRole
    {
        None,
        Own,
        //[Display(Name = "All employees")]
        Global,
        [Display(Name = "My Reports")]
        MyReports,
        [Display(Name = "Own team")]
        OwnTeam,
        [Display(Name = "Own department")]
        OwnDepartment,
        [Display(Name = "Own location")]
        OwnLocation,
    }

    public enum EmployeeRoleScope
    {
        [Display(Name = "All Employees")]
        AlLEmployees,
        [Display(Name = "My Profile Only")]
        MyProfileOnly,
        [Display(Name = "My Direct Reports")]
        MyDirectReports,
    }


    public enum RemindAction
    {
        Reminder,
        Task
    }

    public enum RemindBeforeAfter
    {
        before,
        after
    }
    public enum RemindIn
    {
        days,
        weeks
    }
    //
    public enum RemindAbout { 

        Birthday,
        [Display(Name = "Probation period end")]
        ProbationEnd,

        [Display(Name = "Hire date")]
        HireDate,
        [Display(Name = "Last day of work")]
        LastDayOfWork,

        [Display(Name = "Contract ends")]
        ContractEnd,

        [Display(Name = "Termination date")]
        TerminationDate,

        [Display(Name = "Last salary change")]
        LastSalaryChange,

        [Display(Name = "Next absence")]
        NextAbsemce,

        [Display(Name = "Vsia expiry date")]
        VisaExpiryDate,
    }

    public enum EmployeeSelectorReminder
    {
        [Display(Name = "All employees")]
        All,
        Special,
        [Display(Name = "My Direct Reports")]
        MyDirectReport,
        [Display(Name = "My Reports")]
        MyReports,
    }


    public enum UserType
    {
        PayAll,
        Company,
        Employee,
        Admin,
        Manager,
    }

    public enum NotificationReceivedBy
    {
        UsersWithRoles,
        SpecificUsers,
        OwnerOfEntity,
        RequestApprovalConfig,
    }
    
    public enum NotificationActionTakenType
    {
        NoAction,
        Approved,
        Rejected,
        Expired,
    }

    public enum EmployeeInteractionType
    {
        RequestApproval,
        RequestSubsitituteApproval,
        Announcement,
        Reminder
    }

    public enum PerformanceIndicator
    {
        [Display(Name = "Late Mins")]
        LateMins,
        [Display(Name = "Absent Days")]
        AbsentDays,
        [Display(Name = "Off Days")]
        LeaveDays,
        [Display(Name = "Regular Hours")]
        WorkedHours,
        [Display(Name = "Overtime Hours")]
        OvertimeHours,
        [Display(Name = "Grade Changes")]
        GradeChanges,
    }

    public enum TimeFrame
    {
        Daily,
        Weekly,
        Monthly,
        Yearly,
    }

    //public enum NotificationType
    //{
    //    CompanySubmittedForAction
    //    CopanyActionTaken

    //    PublishAnnouncement,
    //    PaySlipGenerated,
    //    DutyRosterVerificationc,

    //    RequestSubmission,
    //    RequestApproval,
    //    RequestRejection
    //}

    public enum EntityType
    {
        Anything,
        CompanyAccount,
        Announcement,
        PayrolPeriod,
        Roster,
        Request,
        Company,
        AppUser,
        Employee,
        Schedule,
        Job,
        Feature
    }

    //public enum NotificationTriggerSource
    //{
    //    PublishAnnouncement,
    //    PublishMemo,
    //    ContractExpiry,
    //    NewSuggestionRequest,
    //    ScheduleVerification,
    //    BirthdayCelebration
    //}

    public enum CompanyWorkType
    {
        [Display(Name = "Fixed Timing(s)")]
        FixedTime = 1,
        [Display(Name = "Shift(s)")]
        Shifts = 2,
        [Display(Name = "Flexible Timing(s)")]
        Flexible = 3
    }

    public enum TaskStatus
    {
        Scheduled,
        Cancelled,
        Ended,
        Recurring,
    }

    public enum RequestType
    {
        Leave,
        Overtime,
        Attendance_Change,
        Work_Change,
        Work_Submission,
        //Report,
        Holiday,
        Document,
        Emp_DataChange,
        //WorkItem,
        //Request
    }

    public enum RecurringFrequency
    {
        // record created
        Never,
        Daily,
        Weekly,
        Monthly,
        [Display(Name = "Every Two Weeks")]
        Every2Weeks
    }

    public enum AccruralCarryoverFromPreviousYear
    {
        NoCarryOver,
        Unlimited,
        Limited,
    }
    public enum DayOffGrantEntititlementEvery
    {
        Year,
        HalfYear,
        Quarter,
        Month,
    }
    public enum DayOffProrateClcualtationTimeFrame
    {
        [Display(Name = "No proration")]
        No,
        [Display(Name = "Daily prorated")]
        Daily,
        [Display(Name = "Monthly prorated")]
        Monthly,
        [Display(Name = "Termination date shouldn't reduce the entitielment")]
        TerminationDateNotReduceEntitilement,
    }

    public enum DayOffGrantEntititlementAt
    {
        [Display(Name = "Throughout the year in pay periods")]
        Proportionately,

        [Display(Name = "Employee's anniversary date (all at once)")]
        EmployeeHireDate,

        [Display(Name = "Begining of each year  (all at once)")]
        BeginingOfYear,
    }

    public enum DayOffEmployeeItemStatus
    {
        Requested,
        Approved,
        Rejected,
        Invalidated,
        Cancelled
    }

    // Employee action


    /// <summary>
    /// Probation (Monthly)
    /// Full Time (Monthly)
    /// Part Time (Wage)
    /// Contract (Wage)
    /// </summary>
    public enum ActionType
    {
        NA,
        [Display(Name = "Create Position")]
        CP,
        [Display(Name = "Remove Position")]
        RP,
        [Display(Name = "Appointment")]
        APP,
        [Display(Name = "Promotion")]
        PR,
        [Display(Name = "Demotion")]
        DE,
        [Display(Name = "Transfer")]
        TRA,
        [Display(Name = "First Warning")]
        WRNO,
        [Display(Name = "Written Warning")]
        WRNW,
        [Display(Name = "Suspension")]
        SUS,
        [Display(Name = "Termination")]
        TER,
        [Display(Name = "Data Change")]
        DC,
        
        [Display(Name = "Terms Change")]
        TC,

        [Display(Name = "Migration")]
        MIG
    }

    public enum ActionStatus
    {
        Created,
        Completed,
        Scheduled
    }

    public enum AnnouncementStatus
    {
        Draft,
        Published,
        Scheduled,
        Expired
    }

    public enum ScheduleFor
    {
        Attendance,
        Work,
        Roster
    }

    public enum ScheduleStatus
    {
        Draft,
        Generated,
        Published
    }

    public enum AuditAction
    {
        None = 0,
        Create = 1,
        Update = 2,
        Delete = 3,
        StatusUpdate = 4
    }

    public enum AuditType
    {
        KeyValues,
        SummaryText
    }



    public enum RequestProceessConfigActionBy
    {
        [Display(Name = "Employees with role")]
        EmployeesWithRole,
        [Display(Name = "Specific Employee")]
        SpecificEmployee,
        [Display(Name = "Supervisor")]
        Supervisor,
        [Display(Name = "Supervisor's Supervisor")]
        SupervisorsSupervisor,
        [Display(Name = "Auto action after hour(s)")]
        AutoActionAfterHours,
    }
}
