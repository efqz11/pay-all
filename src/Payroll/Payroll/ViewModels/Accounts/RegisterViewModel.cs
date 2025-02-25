using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels.Accounts
{
    public class RegisterViewModel
    {
        //[Required]
        //[Display(Name = "User Name")]
        //public string UserName { get; set; }


        [Required(ErrorMessage = "Hi... We're PayAll, whats your full name?")]
        public string FullName{ get; set; }

        [Required(ErrorMessage = "What’s the official name of your business?")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "This will be your primary email address!")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }


        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        //[Required]
        //public bool AcceptTermsAndConditions { get; set; }
    }
}
