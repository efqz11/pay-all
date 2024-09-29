using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class AppUser : IdentityUser, IAudit
    {
        public string NickName { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public UserStatus UserStatus { get; set; }
        public bool IsActive { get; set; }
        public string Avatar { get;  set; }
        public string TimeZone { get; set; }
        public string Country { get; set; }

        public string NameDisplay => NickName ?? UserName;

        public bool SendOtpAndLoginFirst { get; set; }
        public bool ChangePasswordOnLogin { get; set; }

        public UserType UserType { get; set; }

        public int? CompanyAccountId { get; set; }
        public virtual CompanyAccount CompanyAccount { get; set; }

        public virtual List<AppUserLogin> AppUserLogins { get; set; }
        public virtual List<AccessGrant> AccessGrants { get; set; }
        public virtual List<AppUserRole> AppUserRoles { get; set; }
        public virtual List<Notification> Notifications { get; set; }
        public virtual List<RefreshToken> RefreshTokens { get; set; }


        public DateTime? LastLoginTimeStamp { get; set; }

        #region Oboarding Survey questions
        
        [Display(Name = "Have you run payroll this year?")]
        public bool SurveyQn_PayrollThisYear { get; set; }

        [Display(Name = "How do you currently run payroll?")]
        public SurveyQn_HowDoRunPayroll SurveyQn_HowDoRunPayroll { get; set; }

        [Display(Name = "How would you describe your business setting?")]
        public Survey_BusinessSetting Survey_BusinessSetting { get; set; }


        [Display(Name = "Interested in offering health benefits to your team?")]
        public int Survey_InterestedHealthBenefits { get; set; }


        [Display(Name = "Who is your company planning to pay?")]
        public IList<PayingTo> SurveyCs_PayingToWhom { get; set; }


        [Display(Name = "How many Full-time and Part-time employees employees currently work for your company?")]
        public int SurveyCs_NoW2Employees { get; set; }

        public IdentityResult validateCustomizeExperience()
        {
            if (SurveyCs_PayingToWhom == null || !SurveyCs_PayingToWhom.Any())
                return IdentityResult.Failed();


            if (SurveyCs_IndustryId <= 0 && string.IsNullOrWhiteSpace(SurveyCs_IndustryOwnWords))
                return IdentityResult.Failed();

            //if (SurveyCs_IndustryId == 0 || (SurveyCs_IndustryId == 0 && string.IsNullOrWhiteSpace(SurveyCs_IndustryOwnWords)))
            //    return IdentityResult.Failed();
            return IdentityResult.Success;
        }

        [Display(Name = "How many contractors currently work for your company?")]
        public int SurveyCs_NoContractors { get; set; }
        
        [Display(Name = "Do you have employees who need to track their time?")]
        public bool SurveyCs_NeedTrackTime { get; set; }
        
        [Display(Name = "How do you want these employees to track their time with PayAll?")]
        public HowToTrackTime SurveyCs_TrackTimeHow { get; set; }

        [Display(Name = "Which of the options below best describes your role?")]
        public string SurveyCs_EmpRoleString { get; set; }

        [Display(Name = "What's your company's entity type?")]
        public CompanyEntityType SurveyCs_CompanyEntityType { get; set; }
        
        public Industry SurveyCs_Industry { get; set; }
        [Display(Name = "What industry are you in?")]
        public int? SurveyCs_IndustryId { get; set; }

        [Display(Name = "Industry in your own words")]
        public string SurveyCs_IndustryOwnWords { get; set; }

        #endregion

        [NotMapped]
        public string Roles { get; set; }


        [NotMapped]
        public bool? IsTempSwitching { get; set; }
        [NotMapped]
        public string TempSwitchingOriginalUserId { get; set; }
        [NotMapped]
        public string TempSwitchingOriginalUserName { get; set; }

        [NotMapped]       
         public IList<UserLoginInfo> CurrentLogins { get; set; }
        
        [NotMapped]
        public List<AuthenticationScheme> OtherLogins { get; set; }
        
        [NotMapped]
        public bool ShowRemoveButton { get; set; }

        public string[] GetUserRoles()
        {
            return AppUserRoles.Count() > 0 ? AppUserRoles.Select(z => z.AppRole.Name).ToArray() : default(string[]);
        }

        ////public virtual ICollection<IdentityUserRole<>> Roles { get; set; }
        //public string GetRoles => string.Join(", ", AppUserRoles.Select(x => x.Name));

        public AppUser()
        {
            AppUserLogins = new List<AppUserLogin>();
            AccessGrants = new List<AccessGrant>();
            AppUserRoles = new List<AppUserRole>();
            Notifications = new List<Notification>();
            RefreshTokens = new List<RefreshToken>();
        }
    }


    public class AppRole : IdentityRole
    {
        public ICollection<AppUserRole> UserRoles { get; set; }

        public AppRole()
        {

        }
        public AppRole(string roleName) : base(roleName)
        {
        }
        
    }

    public class AppUserRole : IdentityUserRole<string>
    {
        public virtual AppUser AppUser { get; set; }
        public virtual AppRole AppRole { get; set; }

    }
}
