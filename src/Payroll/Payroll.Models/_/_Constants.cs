using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Payroll.Models
{
    public static class CustomClaimTypes
    {
        public const string Profile = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/profile";
        public const string Nickname = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nickname";
        //public const string EmployeeCompanyName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/company";
        //public const string EmployeeCompanyLogo = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/defaultcompanylogo";
        public const string EmployeeRole = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/employee/role";
        public const string EmployeeRoleId = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/employee/roleid";
        /// <summary>
        /// HomeController.Manager.AsyncStateMachineAttribute,DebuggerStepThroughAttribute
        /// Controller.Action.(Attributes: POST,GET,..)
        /// </summary>
        public const string EmployeeRoleRoute = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/employee/role/route";
        public const string EmployeeRoleScope = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/employee/role/scope";
        public const string EmployeeTempSwitch = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/employee/tempswitch";
        public const string EmployeeTempSwitchOriginalUserId = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/employee/tempswitch/user";
        public const string EmployeeTempSwitchOriginalUserName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/employee/tempswitch/user/name";

        public const string formatter_date = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/format/date";
        public const string formatter_time = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/format/time";
        public const string formatter_name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/format/name";
        public const string formatter_number = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/format/number";

        public const string accessgrant_companyName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/company";
        public const string accessgrant_companyId = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/company/id";
        public const string accessgrant_companyLogo = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/company/logo";
        public const string accessgrant_companyTimeZone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/company/timezone";
        public const string accessgrant_roles = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/company/roles";


        public const string UserType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/user/type";
        public const string EmployeeId = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/employee/id";
        public const string EmployeeName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/employee/";
        public const string DepartmentId = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/employee/dept/id";
        public const string DepartmentName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/employee/dept";
        public const string JobTitle = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/employee/job";

        //public const string DefaultCompanyLogo = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/defaultcompanylogo";
        //public const string DefaultCompanyName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/defaultcompanyname";
        //public const string DefaultCompanyId = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/defaultcompanyid";
    }


    // MyProfile.View
    // MyProfile.Propose
    // MyProfile.Edit
    // MyDirectReports.View
    // MyDirectReports.Propose
    // MyDirectReports.Edit
    // AllEmployees.View
    // AllEmployees.Propose
    // AllEmployees.Edit

    public static class ReportTypes
    {
        public const int DailyAttendnace = 1001;
        public const int MonthlyAttendnace = 1002;
        public const int OTClaimed = 1004;
        public const int Pension = 18;
    }
    public static class BootstrapColors
    {
        public const string @default = "default";
        public const string danger = "danger";
        public const string dark = "dark";
        public const string info = "info";
        public const string light = "light";
        public const string primary = "primary";
        public const string secondary = "secondary";
        public const string success = "success";
        public const string warning = "warning";
        public const string purple = "purple";
        public const string aqua = "aqua";

        public static IList<string> GetColors()
        {
            return new[] { @default, danger, dark, info, light, primary, secondary, success, warning, purple, aqua };
        }
    }


    public static class DefaultPictures
    {
        public const string default_user = "~/img/default-image.png";
        public const string default_company = "~/img/default-company.jpg";
        public const string default_job = "~/img/default-job.jpg";
    }
    public static class AuditFileds
    {
        public const string CreatedDate = "CreatedDate";
        public const string CreatedById = "CreatedById";
        public const string CreatedByName = "CreatedByName";
        public const string CreatedByRoles = "CreatedByRoles";

        public const string ModifiedDate = "ModifiedDate";
        public const string ModifiedById = "ModifiedById";
        public const string ModifiedByName = "ModifiedByName";
        public const string ModifiedByRoles = "ModifiedByRoles";

        public const string IsDeleted = "IsDeleted";

        public const string IsActive = "IsActive";
    }
    
    public static class SystemUserInfo
    {
        public const string id = "7fadeeb8-f941-11e9-aad5-362b9e155667";
        public const string name = "System";
    }
    public static class SessionVariables
    {
        public const string WorkingEmployeeId = "WorkingEmployeeId";
        public const string WorkingEmployeeObject = "WorkingEmployeeObject";

        public const string EmployeeSelector = "EmployeeSelector";
        public const string UserSaveInSessionData = "UserSaveInSessionData";
        public const string EmployeeSelectorSummary = "EmployeeSelectorSummary";
    }

    
    public static class UploadSetting
    {
        public const string FileTypes = ".xlsx,.docx,.doc,.xls,.xlsx,.pdf,.txt,.jpeg,.png,.gif";
        public const string ImageTypes = ".png,.jpg,.jpeg,.gif";

        public const double MaxFileSizeMb = 10f;
        public const double MaxImageSizeMb = 4f;
    }

    public static class MyExpressions {
        public static Expression<Func<CompanyAccount, bool>> IsCompanyPayrpll = x=> x.PayrolPeriodStartDate.HasValue && x.PayrolPeriodStartDate > 0 && x.PayrolPeriodEndDate.HasValue && x.PayrolPeriodEndDate > 0;

        public static Expression<Func<PayAdjustment, bool>> IsConstantPayAdjustment = x => x.VariationType == VariationType.ConstantAddition || x.VariationType == VariationType.ConstantDeduction;
    }

    public static class Roles
    {
        //public const string ROLES_data_entry = "data_entry";
        //public const string ROLES_analyist = "analyst";
        //public const string ROLES_payroll = "payroll";
        //public const string ROLES_owner = "owner";
        //public const string ROLES_admin = "admin";

        // found in access grant (roles)
        public static class Company
        {
            public const string all_employees = "all_employees";
            public const string supervisor = "supervisor";
            public const string accountant = "accountant";
            public const string hr_manager = "hr_manager";
            public const string management = "management";
            public const string roster = "roster";
            public const string payroll = "payroll";
            public const string administrator = "administrator";

            public static string GetDescription(string role)
            {
                switch (role)
                {
                    case all_employees: return "By default, all employees will have this role which allows them to have access to their profile and related HR oinformation.";
                    case supervisor: return "Can view requests assigned to them, view profile of employees who is directly under them.";
                    case accountant: return "Has access to financial reports, tax reports and payroll data.";
                    case hr_manager: return "Can view all employees in the company. View and manage company structure, employee transfer and take action on requests.";
                    case management: return "View company and related billing information with Payall.";
                    case roster: return "Create, manage and approve rosters";
                    case payroll: return "Manage company pay componentsn and manage payroll cycle.";
                    case administrator: return "Administrator has access to all the features of the system.";
                    default:
                        return "";
                }
            }
        }

        // actual roles (AppRoles) -> AppUserRoles
        public static class PayAll
        {
            public const string admin = "admin";
            public const string company_action = "company_action";
            public const string user_action = "user_action";
        }

    }

    public static class Scope
    {
        public const string all_users = "*";
        public const string my_profile = "?";
        public const string my_direct_reports = "@";
    }

    /*
     * Roles
     * all_employees
     * emp.?.view, emp.?.search, emp.?.propose,
     * 
     * hr_manager
     * emp.*.view, emp.*.create, emp.*.update, emp.*.remove
     */
    public static class Permissions
    {
        public const string LIST = "LIST";
        public const string SEARCH = "SEARCH";
        public const string READ = "READ";
        public const string CREATE = "CREATE";
        public const string UPDATE = "UPDATE";
        public const string DELETE = "DELETE";
        public const string SWITCH = "SWITCH";
    }

    public class FeatureMenus {

        public static class MenuItem
        {
            public const string TimeTrackingApprovals = "Time Tracking approvals";
            public const string Company = "Company";
            public const string Schedule = "Schedule";
            public const string TimeOff = "Time off";
            public const string Staffs = "Staffs";
            public const string Payroll = "Payroll";
            public const string Roster = "Roster";
            public const string Jobs = "Jobs";
            public const string User = "User";
            public const string PayComponents = "Pay Components";
            public const string AbsenceCalendar = "AbsenceCalendar";
            public const string Calendar = "Calendar";
            public const string RequestApprovals = "RequestApprovals";
            public const string Notification = "Notification";
            public const string Announcements = "Announcements";
            public const string Home = "Home";
            public const string Admin = "Admin";
            public const string Report = "Reports";
            public const string Hiring = "Hiring";
        }

        public static string GetFeatureMenuItem(string menuItem) => FeatureSearchList.FirstOrDefault(x=> x.Name == menuItem)?.Icon ?? "";
        public static string GetFeatureMenuItemWithName(string menuItem) => (FeatureSearchList.FirstOrDefault(x => x.Name == menuItem)?.Icon ?? "") + " " + menuItem;
        public static (string icon, string title, string baseUrl) GetFeatureStepItem(int step) => CompanySetupSteps.ElementAt(step - 1);

        public static List<(string icon, string title, string baseUrl)> CompanySetupSteps = new List<(string, string, string)>
        {
            ("🏠", "Add company profile", "/Company/AddOrUpdate"),
            ( "⚙️", "Configure company", "/Company/SetupPayrolPeriod" ),
            ( "⛱️", "Time off", "/Dayoff" ),
            ( "🖼️", "Set a logo", "/Company/UploadImage" ),
            ("📅", "Working times", "/Company/ChangeWorkType"),
            ( "🖌️", "Custom formats", "/Company/UpdateFormatSettings" ),
            //( "📋", "Organization", "/Company/UpdateDeptDivisionLocations" ),
            ( "💰", "Add pay component", "/Company/PayComponents" ),
            ( "👪", "Add employees", "/Company/NewEmployeeProfile" ),
            ( "❤️", "Setup leaves", "/Dayoff/Index" ),
            //( "🔑", "Access rights", "/Company/EmployeeRoles" ),
            //( "👪", "Performance metrics", "/Employee/UpdateFormatSettings" ),
            ( "🏆", "Choose a plan", "/Company/ChoosePlan" ),
        };


        public static List<SearchResult> FeatureSearchList = new List<SearchResult>()
        {
            new SearchResult (MenuItem.TimeTrackingApprovals,  "⏱️", EntityType.Feature, "/Manage/TimeTrackingApprovals"),
            new SearchResult (MenuItem.Company,  "🏘️", EntityType.Feature, "/Company"),
            new SearchResult (MenuItem.Schedule,  "📅", EntityType.Feature, "/Schedule"),
            // new SearchResult ("Schedule",  "📅", EntityType.Feature, "/Jobs"),
            new SearchResult (MenuItem.Staffs,  "👨‍👩‍👧‍👧", EntityType.Feature, "/Employee"),
            new SearchResult (MenuItem.Hiring,  "👩🏻‍🏫", EntityType.Feature, "/hireonboard"),
            new SearchResult (MenuItem.Report,  "📈", EntityType.Feature, "/report"),
            new SearchResult (MenuItem.Payroll,  "💰", EntityType.Feature, "/Payroll"),
            new SearchResult (MenuItem.Roster,  "💰", EntityType.Feature, "/Roster"),
            new SearchResult (MenuItem.Jobs,  "💼", EntityType.Feature, "/Job"),
            new SearchResult (MenuItem.User,  "👤", EntityType.Feature, "/AppUser"),
            new SearchResult (MenuItem.PayComponents,  "🎈", EntityType.Feature, "/PayAdjustment"),
            // new SearchResult ("Schedule",  "📅", EntityType.Feature, "/Schedule/Tasks"),
            new SearchResult (MenuItem.AbsenceCalendar,  "📊", EntityType.Feature, "/Manage/Absence"),
            new SearchResult (MenuItem.Calendar,  "📆", EntityType.Feature, "/Manage/Calendar"),
            new SearchResult (MenuItem.RequestApprovals,  "💬", EntityType.Feature, "/Request"),
            new SearchResult (MenuItem.Notification,  "📌", EntityType.Feature, "/Notification"),
            new SearchResult (MenuItem.Announcements,  "📢", EntityType.Feature, "/NewsUpdate/AllAnnouncements"),

            new SearchResult (MenuItem.Home,  "☕", EntityType.Feature, "/Home/Manager"),
            new SearchResult (MenuItem.Admin,  "🤖", EntityType.Feature, "/Home"),
            new SearchResult (MenuItem.TimeOff,  "⛱️", EntityType.Feature, "/DayOff"),
        };
    }

    public class PresetPlanComponents
    {
        public static List<(string icon, string title, string desc, bool comingSoon)> List = new List<(string, string, string, bool)>
        {
            ("fad fa-users-crown", "HRIS", "Employee, departments, jobs, organization charts, etc...", false),
            ("fad fa-comment-alt-dollar", "Payroll", "Do payroll in mins, keep expense claims in check", false),
            ("fad fa-stars", "Performance", "360° reviews, feedback forms, competency maps", false),
            ("fad fa-calendar-alt", "Schedule", "Rostes, schedules and approvals", false),
            //("fad fa-stars", "Custom Payroll", "Rostes, schedules and approvals", false),
            //("fad fa-stars", "Performance", "360° reviews, feedback forms, competency maps", true),

            ("fad fa-user-headset", "Helpdesk", "Multi-purpose HR/IT/Finance/Travel helpdesk", true),
            ("fad fa-hand-holding-heart", "Benefits", "Employee benefits, claims and payroll sync", true),
            ("fad fa-folders", "Documents", "HR letters, HR drive, policy centre", true),
            ("fad fa-business-time", "Track", "Time-tracking, shift plans, leaves/time-off", false),
            ("fad fa-user-tag", "On/Offboarding", "On/off-boarding, letters, HR drive, policy centre", false),
            //("fad fa-stars", "Recruit", "Career site, ATS, hiring pipelines (kanban)", true),
            ("fad fa-chart-pie", "Report", "Career site, ATS, hiring pipelines (kanban)", false),
            ("fad fa-phone-laptop", "Asset manager", "Optimize, track and maintain asset inventory", true),
        };

        public static string Get(string name) => PresetPlanComponents.List.FirstOrDefault(x => x.title == name).icon ?? "fad fa-th";
    }

    public class PreConfigurePayComponents
    {
        public static List<(string icon, string title, string desc, PayAdjustment adj)> List = new List<(string, string, string, PayAdjustment)>
        {
            ("fal fa-sack-dollar", "Basic salary", "Basic salary used as constant addition for each active employment.", new PayAdjustment { Name = "Basic salary", VariationType = VariationType.ConstantAddition }),
            ("fal fa-hourglass-end", "Overtime pay", "Use approved overtime hours of each employee to calculate pay.", new PayAdjustment { Name = "Overtime pay", VariationType = VariationType.VariableAddition } ),
            ( "fad fa-hands-usd", "Pension charges", "Pension charges calcualated from 7% each employee's basic salary.", new PayAdjustment { Name = "Pension charges", VariationType = VariationType.VariableDeduction }),
            ( "fal fa-calendar-times", "Absent fine", "Use absent days to calculate fine amount and deduct from their pay.", new PayAdjustment { Name = "Absent fine", VariationType = VariationType.VariableDeduction }),
        };

        public static string Get(string name) => PreConfigurePayComponents.List.FirstOrDefault(x => x.title == name).icon ?? "fad fa-th";
    }

    public class Calendars
    {
        
        public static List<Tuple<string, string, string>> List = new List<Tuple<string, string, string>>
        {
            Tuple.Create("My Calendar", "dark", "fa fa-calendar-check" ),
            Tuple.Create("Brithdays", "warning", "fad fa-birthday-cake" ),
            Tuple.Create( "Public Holidays", "success", "fad fa-gifts"),
            Tuple.Create( "Start / End Days", "purple", "fad fa-calendar-star"),
            Tuple.Create("Anniversaries", "info", "fad fa-medal"),
            Tuple.Create("Leaves", "danger", "fad fa-calendar-minus")
        };

        //{
        //   new { "My Calenar", "warning" },
        //    ("Brithdays", "purple")
        //    ("Public Holidays", "success"),
        //    ("Anniversaries", "info")
        //};
    }


    public static class ScheduledSystemTasks
    {
        public const int PAID_TIME_OFF_INITIAL_ACCRUALS = 1;
        public const int PAID_TIME_OFF_YEARLY_ACCRUALS = 2;
        public const int MANDATORY_RETIREES_TODAY = 3;
        public const int END_EXPIRED_EMPLOYEE_ROLES = 4;
        public const int TERMINATE_EXPIRED_WAGE_EMPLOYEE = 5;
    }

    public static class EventLogResults
    {
        public const int SUCCESS = 1;
        public const int FAILURE = 2;
        public const int WARNING = 3;
        public const int INFORMATION = 4;
        public const int ERROR = 5;
    }

    public static class EvengLogDataTypes
    {
        public const int SCHEDULED_SYSTEM_TASK = 1;
        public const int USER = 2;
        public const int EMPLOYEE = 3;
        public const int REQUEST = 4;
        public const int COMPANY_TASK = 5;
        public const int USER_TASK = 6;
        public const int ATTENDANCE = 7;
        public const int WORK_ITEM = 8;
    }

    public static class EventLogTypes
    {
        public const int EXECUTE_SCHEDULED_TASK = 1;
        public const int USER_LOGIN = 2;
    }



        public static class CompanyDetailTabs
    {

        public const string Departments = "dept";
        public const string DayOffs = "dayOffs";
        public const string WorkBreakTimes = "workBreakTimes";
        public const string AccessGrants = "accessGrants";
        public const string PayAdjustments = "payAdjustments";
    }


    public static class NotificationTypeConstants
    {

        public const int CompanySubmittedForAction = 1;
        public const int CompanyActionTaken = 2;

        public const int ScheduleSubmittedForVerification = 3;
        public const int ScheduleVerificationUpdate = 4;

        public const int PublishAnnouncement = 5;
        public const int AnnouncementViewed = 6;

        public const int RequestSubmittedForAction = 7;
        public const int RequestActionTaken = 8;
        public const int RequestTransferEmployee = 9;
        public const int RequestEmployeeDataChange = 10;
        public const int RequestLeave = 11;

        public const string WorkBreakTimes = "workBreakTimes";
        public const string AccessGrants = "accessGrants";
        public const string PayAdjustments = "payAdjustments";
    }

    public static class Formatters
    {
        public static readonly string[] DateFormats = new[]
        {
            "dd-MM-yyyy",
            "dd-MMM-yyyy",
            "MM-dd-yyyy",
            "yyyy-MM-dd",
            "dd/MM/yy",
            "yy/MM/dd",
            "MM/dd/yy"
        };

        public static readonly string[] TimeFormats = new[]
        {
            "h:mm",
            "h:mm tt",
            "HH:mm",
            "hh:mm tt",
            "HH:mm:ss",
        };


        // 1234.567
        public static readonly string[] CurrencyFormats = new[]
        {
            "{0:0}",          // 1235
            "{0:0.0}",        // 1235.6
            "{0:0.000}",      // 1234.560
            "{0:n0}",         // 1,235
            "{0:n}",          // 1,235.57
            //"{0:#,##0.00}",   // 1.234,56
            //"{0:#,##0.00}",     // 1,234.56
            //"{0:#,0.####}",   // 1,234.567
            //"{0:#'0.00}",     // 1'234.56
            "{0:# 0.00}",     // 1 234.56
        };

        public static readonly string[] NameFormats = new[]
        {
            "{FirstName} {LastName}",
            "{FirstName} {MiddleName} {LastName}",
            "{Initial} {FirstName} {LastName}",
            "{LastName}, {FirstName}",
            "{Initial} {LastName}",
        };
    }


}
