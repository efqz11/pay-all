using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Payroll.Models;
using Payroll.ViewModels.Accounts;
using Microsoft.AspNetCore.Http;
using Payroll.Services;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Payroll.ViewModels;

namespace Payroll.Controllers
{
    [Authorize]
    //[Route("[controller]/[action]")]
    public class AccountController : BaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AccessGrantService _accessGrantService;
        private readonly CompanyService _cmpService;
        private readonly UserResolverService _userResolverService;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            IHttpContextAccessor httpContextAccessor,
            AccessGrantService accessGrantService,
            CompanyService cmpService,
            UserResolverService userResolverService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _accessGrantService = accessGrantService;
            _cmpService = cmpService;
            _userResolverService = userResolverService;
        }

        [TempData]
        public string ErrorMessage { get; set; }


        [HttpGet]
        public async Task<IActionResult> Welcome()
        {
            var iusr = await _userManager.GetUserAsync(User);
            ViewBag.SurveyCs_IndustryId = new SelectList(await _accessGrantService.GetIndustries(), "Id", "Name", iusr.SurveyCs_IndustryId);
            return View(iusr);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Welcome(AppUser model)
        {
            var res = model.validateCustomizeExperience();
            ViewBag.SurveyCs_IndustryId = new SelectList(await _accessGrantService.GetIndustries(), "Id", "Name", model.SurveyCs_IndustryId);

            if (!res.Succeeded)
            {
                SetTempDataMessage("Please fill in the form", MsgAlertType.danger);
                return View(model);
            }

            var usr = await _userManager.GetUserAsync(User);
            usr.SurveyCs_CompanyEntityType = model.SurveyCs_CompanyEntityType;
            usr.CompanyName = model.CompanyName;
            usr.SurveyCs_IndustryId = model.SurveyCs_IndustryId;
            usr.SurveyCs_IndustryOwnWords = model.SurveyCs_IndustryOwnWords;
            usr.SurveyCs_NoW2Employees = model.SurveyCs_NoW2Employees;
            usr.SurveyCs_NoContractors = model.SurveyCs_NoContractors;
            usr.SurveyCs_NeedTrackTime = model.SurveyCs_NeedTrackTime;
            usr.SurveyCs_TrackTimeHow = model.SurveyCs_TrackTimeHow;
            usr.SurveyCs_EmpRoleString = model.SurveyCs_EmpRoleString;
            usr.SurveyCs_PayingToWhom = model.SurveyCs_PayingToWhom;
            usr.UserStatus = UserStatus.PendingCompanySetup;

            await _userManager.UpdateAsync(usr);
            var cmpAccount = await _accessGrantService.CreateCompanyForUserAsync(usr);

            await _accessGrantService.ChangeUserAccessGrant(cmpAccount.AccessGrants.First().Id);

            //SetTempDataMessage("User welcome information saved successfully", MsgAlertType.success);

            return View("SetupDone", cmpAccount);
            //return View(usr);
        }

        public async Task<IActionResult> SetupDone() => View((await _accessGrantService.GetAllAccessibleCompanyAccountsAsync())?.FirstOrDefault() ?? new CompanyAccount());

        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> Welcome()
        //{
        //    return View();
        //}
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            SetTempDataMessage("You’ve been signed out due to inactivity. Please sign in again to continue", MsgAlertType.danger);
            
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This does not count login failures towards account lockout
                // To enable password failures to trigger account lockout,
                // set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    var userId = _userResolverService.GetUserId();
                    var userAccessGrant =await _accessGrantService.GetFirstAccessGrantAsync(userId);
                    if (userAccessGrant != null)
                    {
                        // Get connection and set to Master payroll database
                        //_httpContextAccessor.HttpContext.Session.SetString("AccessGrant.CompanyName", userAccessGrant.CompanyAccount.Name);
                        //_httpContextAccessor.HttpContext.Session.SetString("AccessGrant.CompanyId", userAccessGrant.CompanyId.ToString());
                        //if (!string.IsNullOrWhiteSpace(userAccessGrant.CompanyAccount.LogoUrl))
                        //    _httpContextAccessor.HttpContext.Session.SetString("AccessGrant.CompanyLogo", userAccessGrant.CompanyAccount.LogoUrl);
                        //if (!string.IsNullOrWhiteSpace(userAccessGrant.Roles))
                        //    _httpContextAccessor.HttpContext.Session.SetString("AccessGrant.Roles", userAccessGrant.Roles);
                    }
                    return RedirectToLocal(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("RequiresTwoFactor.");
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    _logger.LogInformation("error. Invalid login attempt");
                    SetTempDataMessage("Ouch! your username or password doesn't match with our records", MsgAlertType.danger);
                    //ModelState.AddModelError("Password", "Invalid login attempt.");
                    ViewBag.IsError = true;
                    return View(model);
                }
            }


            //ModelState.AddModelError("Password", "Invalid login attempt.");
            // If execution got this far, something failed, redisplay the form.
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure that the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        public async Task<IActionResult> PendingConfirmation(){
            return View(await _userManager.GetUserAsync(User));
        }

        public async Task<IActionResult> SetupCompany(int id)
        {
            var cmp = await _cmpService.GetCompanyAccount(id);
            //if (cmp.Status == CompanyStatus.Approved)
            //    return PartialView("~/Views/Company/_CompanyApproved.cshtml", cmp);
            return View(cmp);
        }
        
        public async Task<IActionResult> GetSetupOverviewItem(int id, int step)
        {
            if(!Request.IsAjaxRequest())
                 return RedirectToAction(nameof(SetupCompany), new { id });
                 
            ViewBag.Id = id;
            var cmp = new SetupOverviewPageVm(step, FeatureMenus.GetFeatureStepItem(step));
            var isStepDone = await _cmpService.IsCompanyStepDone(id, step);
            var _cmp = await _cmpService.GetCompanyAccount(id);

            if (_cmp.Status == CompanyStatus.Approved)
                return PartialView("~/Views/Company/_CompanyApproved.cshtml", _cmp);

            if (isStepDone)
            {
                cmp.Company = _cmp;
                if(step == 7)
                    return RedirectToAction("Index", "HireOnboard");

                ViewBag.AlreadyCompleted = true;
                return PartialView("_SetupOverviewItemDone", cmp);
            }

            return PartialView("_SetupOverviewItem", cmp);
        }
        
        public async Task<IActionResult> GetSetupOverviewItemDone(int id, int step)
        {
            ViewBag.Id = id;
            var cmp = new SetupOverviewPageVm(step, FeatureMenus.GetFeatureStepItem(step));
            cmp.Company = await _cmpService.GetCompanyAccount(id);
            return PartialView("_SetupOverviewItemDone", cmp);
        }
            
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new AppUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new { userId = user.Id, code = code },
                        protocol: Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(model.Email, HtmlEncoder.Default.Encode(callbackUrl));

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created a new account with password.");
                    return View("PendingConfirmation", user);
                }
                AddErrors(result);
            }

            // If execution got this far, something failed, redisplay the form.
            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(Login));
        }


        // [HttpGet, Route("[controller]/ExternalLogin")]
        // public IActionResult ExternalLogin(string returnUrl, string provider = "google")
        // {
        //     string authenticationScheme = string.Empty;

        //     // Logic to select the authenticationScheme
        //     // which specifies which LoginProvider to use
        //     // comes in here
        //     authenticationScheme = GoogleDefaults.AuthenticationScheme;

        //     var auth = new AuthenticationProperties
        //     {
        //         RedirectUri = Url.Action(nameof(LoginCallback), new { provider, returnUrl })
        //     };

        //     return new ChallengeResult(authenticationScheme, auth);
        // }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
             var info = await _signInManager.GetExternalLoginInfoAsync();
            //  ExternalLoginInfo info = new ExternalLoginInfo(User,
            //     "Microsoft",
            //     User.Claims.Where(x=>x.Type== "http://schemas.microsoft.com/identity/claims/objectidentifier").FirstOrDefault().Value.ToString(),
            // "Microsoft" );
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                //ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLogin", new ExternalLoginViewModel { Email = email, ReturnUrl = returnUrl, ProviderDisplayName = info.LoginProvider });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                var user = new AppUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }
        

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if(result.Succeeded)
                return RedirectToAction(nameof(Welcome));
            return View("Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                //var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
                //await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                //   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If execution got this far, something failed, redisplay the form.
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
                SetTempDataMessage(error.Description, MsgAlertType.danger);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}