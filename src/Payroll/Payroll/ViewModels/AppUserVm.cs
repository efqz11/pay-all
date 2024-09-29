using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class AppUserCustomizeExperienceVm
    {
        public string UserName { get; set; }
        public string Id { get; private set; }
        public string FullName { get; set; }

        public DateTime? LastLoginTimeStamp { get; set; }


        [Required]
        [Display(Name = "Have you run payroll this year?")]
        public bool SurveyQn_PayrollThisYear { get; set; }

        [Required]
        [Display(Name = "How do you currently run payroll?")]
        public SurveyQn_HowDoRunPayroll SurveyQn_HowDoRunPayroll { get; set; }

        [Required]
        [Display(Name = "How would you describe your business setting?")]
        public Survey_BusinessSetting Survey_BusinessSetting { get; set; }

        [Required]
        [Display(Name = "Interested in offering health benefits to your team?")]
        public int Survey_InterestedHealthBenefits { get; set; }

        [Required(ErrorMessage = "⚠️ Please choose atleast one item")]
        [Display(Name = "Who is your company planning to pay?")]
        public IList<string> SurveyCs_PayingToWhom { get; set; }

        [Required(ErrorMessage = "⚠️ Please fill the number of Full-time and Part-time employees employees")]
        [Display(Name = "How many Full-time and Part-time employees employees currently work for your company?")]
        public int SurveyCs_NoW2Employees { get; set; }

        [Required(ErrorMessage = "⚠️ Please fill the number of Self-employed independent contractors")]
        [Display(Name = "How many contractors currently work for your company?")]
        public int SurveyCs_NoContractors { get; set; }

        [Required]
        [Display(Name = "Do you have employees who need to track their time?")]
        public bool? SurveyCs_NeedTrackTime { get; set; }


        [Display(Name = "How do you want these employees to track their time with PayAll?")]
        public HowToTrackTime? SurveyCs_TrackTimeHow { get; set; }

        [Required]
        [Display(Name = "Which of the options below best describes your role?")]
        public string SurveyCs_EmpRoleString { get; set; }

        [Required]
        [Display(Name = "What's your company's entity type?")]
        public CompanyEntityType? SurveyCs_CompanyEntityType { get; set; }

        [Required]
        public Industry SurveyCs_Industry { get; set; }


        [Display(Name = "What industry are you in?")]
        public int? SurveyCs_IndustryId { get; set; }

        [Display(Name = "Industry in your own words")]
        public string SurveyCs_IndustryOwnWords { get; set; }


        public AppUserCustomizeExperienceVm(AppUser model)
        {
            FullName = model.FullName;
            UserName = model.UserName;
            Id = model.Id;
        }
    }
}
