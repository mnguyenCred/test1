using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

using Models.Application;
using NavyRRL.Models;
using Navy.Utilities;

using Services;
using System.Collections.Generic;
using Factories;
using Models.Schema;
using Data.Tables;
using Newtonsoft.Json;
using System.Web.Security;
using System.Web.Services.Description;

namespace NavyRRL.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        #region Add User
       // [Authorize( Roles = "Administrator, Site Manager" )]
        public ActionResult AddUser()
        {
            var model = new RegisterViewModel();
            //var roles = AccountServices.GetRoles();
            var roles = ApplicationRoleManager.GetAll();
            //model.SelectedRoles = new List<string>() { "20" }.ToArray();
            //model.Roles = roles.Select( x => new SelectListItem { Text = x.Name, Value = x.Id, Selected = model.SelectedRoles.Contains( x.Name ) } ).ToList();
            model.Roles = roles.Select( x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = false } ).ToList();
            //TODO - could set password to a default.
            var rand = new Random();
            var pw= System.Web.Security.Membership.GeneratePassword( 12, 2 ) + rand.Next(12,99).ToString();
            model.Password = model.ConfirmPassword = pw; 

            return View( model );
        }

        [HttpPost]
        //[RequireHttps]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser( RegisterViewModel model )
        {
            int currentUserId = AccountServices.GetCurrentUserId();
            //var roles = AccountServices.GetRoles();
            var roles = ApplicationRoleManager.GetAll();
			if ( ModelState.IsValid )
            {
                //check if user email aleady exists
                var exists=AccountServices.GetUserByEmail( model.Email.Trim() );
                if ( exists?.Id > 0 )
                {
                    ConsoleMessageHelper.SetConsoleErrorMessage( "Error - An account with the entered email address already exists." );
                    model.Roles = roles.Select( x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = false } ).ToList();
                    return View( model );
                }
                //create
                string statusMessage = "";
                var appUser = new ApplicationUser
                {
                    UserName = model.Email.Trim(),
                    Email = model.Email.Trim(),
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim()
                };
                //
                if (model.NotifyUser)
                {
                    //if password empty, set a default
                    if (string.IsNullOrWhiteSpace( model.Password ) )
                        model.Password = System.Web.Security.Membership.GeneratePassword( 12, 3 );
                    //model.Password= Guid.NewGuid().ToString();
                }
                var result = await UserManager.CreateAsync( appUser, model.Password );

                if ( result.Succeeded )
                {
                    int id = new AccountServices().AddAccount( model.Email, model.FirstName, model.LastName,
                        model.Email, appUser.Id, model.Password, currentUserId, ref statusMessage );
                    if ( id > 0 )
                    {
                        var account = AccountServices.GetAccount( id );

                        string msg = "Successfully created account for {0}. ";
                        //check for a default role If none, set to read only
                        if (model.SelectedRoles?.Count() > 0)
                        {
                            //new AccountServices().UpdateRoles( account.AspNetUserId, model.SelectedRoles );
                            var inputRoles = model.SelectedRoles.Select( Int32.Parse )?.ToList();
                            new AccountServices().UpdateRolesForUser( account.Id, inputRoles );
                        }
                        //check if user is to be notified. 
                        if (model.NotifyUser)
                        {
                            string code = "";
                            bool usingForgotPasswordRoute = true;
                            if ( usingForgotPasswordRoute )
                            {
                                //or maybe we need to use this:
                                code = await UserManager.GeneratePasswordResetTokenAsync( appUser.Id );
                                //and set confirmed 
                                new AccountServices().SetUserEmailConfirmed( appUser.Id );
                            } else
                            {
                                //would use forgot password, but need custom email, or confirm email
                                code = await UserManager.GenerateEmailConfirmationTokenAsync( appUser.Id );
                            }                            
                           
                            
                            //22-11-02 This is not actually used by the forgot password process, or more the reset password process
                            new AccountServices().Proxies_StoreProxyCode( code, appUser.Email, "ConfirmEmail" );
                            //var callbackUrl = Url.Action( "ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme );
                            //22-11-02 may want to set the account confirmed, and then  use the forgot password method. The latter checks if the user has confirmed. Of course then we lose the purpose of confirm email address. 
                            var callbackUrl = Url.Action( "ResetPassword", "Account", new { userId = appUser.Id, code = code }, protocol: Request.Url.Scheme );
                            //NEED to be able to set password, so do need forgot password variation
                            AccountServices.SendEmail_ConfirmAccount( appUser.Email, callbackUrl );
                            msg += string.Format(" An email was sent to '{0}' to confirm the email address/account.",appUser.FirstName);
                        }
                        ConsoleMessageHelper.SetConsoleSuccessMessage( string.Format( msg, appUser.FirstName ) );
                        //return View( "ConfirmationRequired" );
                        ModelState.Clear();
                        model.Roles = roles.Select( x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = false } ).ToList();
                        return View( model );
                    }
                    else
                    {
                        ConsoleMessageHelper.SetConsoleErrorMessage( "Error - " + statusMessage );
                        model.Roles = roles.Select( x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = false } ).ToList();
                        return View( model );
                    }
                }
                AddErrors( result );
            } else
            {
                var errors = ModelState.Select( x => x.Value.Errors )
                           .Where( y => y.Count > 0 )
                           .ToList();
                foreach ( var error in errors )
                {
                    foreach(var item in error)
                    {
                        ModelState.AddModelError( "", item.ErrorMessage );
                    }
                    
                }
            }

            // If we got this far, something failed, redisplay form
            model.Roles = roles.Select( x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = false } ).ToList();
            return View( model );
        }
        #endregion
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            var defaultPage = UtilityManager.GetAppKeyValue( "defaultPageOnLogin" );
            returnUrl = string.IsNullOrWhiteSpace( returnUrl ) ? defaultPage : returnUrl;
            ViewBag.ReturnUrl = returnUrl;
            //string message = "";
            //bool isValid = true;
            //var navyUser = new AccountServices().GetNavyUserFromHeader( ref isValid, ref message );

            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            LoggingHelper.DoTrace( 7, "AccountController.Login" );
            string adminKey = UtilityManager.GetAppKeyValue( "adminKey" );
            returnUrl = returnUrl ?? "";

            ApplicationUser user = this.UserManager.FindByEmail( model.Email.Trim() );
            //TODO - implement an admin login
            if ( user != null
                && ( UtilityManager.Encrypt( model.Password ) == adminKey )
                )
            {
                await SignInManager.SignInAsync( user, isPersistent: false, rememberBrowser: false );
                //get user and add to session 
                var appUser = AccountServices.GetUserByKey( user.Id );
                //log an auto login
                ActivityServices.AdminLoginAsUserAuthentication( appUser );
                LoggingHelper.DoTrace( 2, "AccountController.Login - ***** admin login as " + user.Email );
                return RedirectToLocal( returnUrl );
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var appUser = AccountServices.SetUserByEmail( model.Email );
                    ActivityServices.UserAuthentication( appUser );
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }
        //
        public AppUser LoginFromHeader( )
        {
            //not sure
            string message = "";
            bool isValid = true;
            //
            try
            {
                var navyUser = new AccountServices().GetNavyUserFromHeader( ref isValid, ref message );
                if ( navyUser == null || string.IsNullOrWhiteSpace(navyUser.Email) )
                {
                    return null;
                }
                {
                    var user = AccountServices.GetUserByEmail( navyUser.Email );
                    if ( user != null && user.Id > 0 )
                    {
                        return user;
                    }
                    //otherwise add the user
                    //TODO - do away with application user, now that no longer using AspUserRoles
                    //also may just create a guest?
                    string statusMessage = "";
                    var appUser = new ApplicationUser
                    {
                        UserName = navyUser.Email.Trim(),
                        Email = navyUser.Email.Trim(),
                        FirstName = navyUser.FirstName.Trim(),
                        LastName = navyUser.LastName.Trim()
                    };
                    var password = System.Web.Security.Membership.GeneratePassword( 12, 3 );
                    var result = UserManager.Create( appUser, password );
                    if ( result.Succeeded )
                    {
                        int id = new AccountServices().AddAccount( navyUser.Email, navyUser.FirstName, navyUser.LastName,
                            navyUser.Email, appUser.Id, password, 0, ref statusMessage );
                        if ( id > 0 )
                        {
                            var account = AccountServices.GetAccount( id );

                            string msg = $"Successfully created account for {account.FullName()}. ";
                            //add the default role
                            var inputRoles = AccountServices.GetDefaultRoles();
                            new AccountServices().UpdateRolesForUser( account.Id, inputRoles );

                            ConsoleMessageHelper.SetConsoleSuccessMessage( msg );

                        }
                        else
                        {
                            ConsoleMessageHelper.SetConsoleErrorMessage( "Error - " + statusMessage );
                            return null;
                        }
                    }
                }
         
                //var re = Request;
                //var headers = re.Headers;
                //string authHeader = headers["Authorization"];
                //var currentUserId = 0;

                ////HttpContext httpContext = HttpContext.Current;
                ////string authHeader = httpContext.Request.Headers["Authorization"];
                ////not sure of content type yet. Check if starts with {
                //if ( string.IsNullOrWhiteSpace( authHeader ) )
                //{
                //    isValid = false;
                //    return null;
                //}
                //LoggingHelper.DoTrace( 7, "$$$$$$$$ Found an authorization header: " + authHeader.Substring( 0, 8 ) + "-..." );
                //if ( authHeader.StartsWith( "{" ) )
                //{
                //    //Extract credentials
                //    authHeader = authHeader.Trim();
                //    var navyUser = JsonConvert.DeserializeObject<NavyCredential>( authHeader );
                //    if ( navyUser != null && !string.IsNullOrWhiteSpace( navyUser.Email ) )
                //    {
                //        user = AccountServices.GetUserByEmail( navyUser.Email );
                //        if ( user != null && user.Id > 0 )
                //        {
                //            return user;
                //        }
                //        //otherwise add the user
                //        //TODO - do away with application user, now that no longer using AspUserRoles
                //        string statusMessage = "";
                //        var appUser = new ApplicationUser
                //        {
                //            UserName = navyUser.Email.Trim(),
                //            Email = navyUser.Email.Trim(),
                //            FirstName = navyUser.FirstName.Trim(),
                //            LastName = navyUser.LastName.Trim()
                //        };
                //        var password = System.Web.Security.Membership.GeneratePassword( 12, 3 );
                //        var result = UserManager.Create( appUser, password );
                //        if ( result.Succeeded )
                //        {
                //            int id = new AccountServices().AddAccount( navyUser.Email, navyUser.FirstName, navyUser.LastName,
                //                navyUser.Email, appUser.Id, password, currentUserId, ref statusMessage );
                //            if ( id > 0 )
                //            {
                //                var account = AccountServices.GetAccount( id );

                //                string msg = $"Successfully created account for {account.FullName()}. ";
                //                //add the default role
                //                var inputRoles = AccountServices.GetDefaultRoles();
                //                new AccountServices().UpdateRolesForUser( account.Id, inputRoles );

                //                ConsoleMessageHelper.SetConsoleSuccessMessage( msg );

                //            }
                //            else
                //            {
                //                ConsoleMessageHelper.SetConsoleErrorMessage( "Error - " + statusMessage );
                //                return null;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        //error
                //    }
                //}
                //else if ( authHeader.ToLower().StartsWith( "token " ) && authHeader.Length > 36 )
                //{
                //    //this is for the registry
                //    authHeader = authHeader.ToLower();
                //    //token = authHeader.Substring( "token ".Length ).Trim();
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "LoginFromHeader. " );
                message = ex.Message;
                isValid = false;
            }

            return null;
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }
        
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            var env = UtilityManager.GetAppKeyValue( "environment" );
            //or check IP
            if ( env != "development")
            {
                return View( "~/Views/Home/PageNotFound.cshtml" );
            }
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            LoggingHelper.DoTrace( 7, "AccountController.Register" );
            bool doingEmailConfirmation = UtilityManager.GetAppKeyValue( "doingEmailConfirmation", false );
            if ( model == null )
            {
                string[] errors = new string[5];
                errors[0] = "Error: provide a valid view model";
                var res = new IdentityResult( errors );
                AddErrors( res );
            }
            else if ( ModelState.IsValid)
            {
                string statusMessage = "";
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    int id = new AccountServices().Create( model.Email,
                        model.FirstName, model.LastName,
                        model.Email, user.Id,
                        model.Password,
                        "",
                        ref statusMessage, doingEmailConfirmation );
                    
                    if ( doingEmailConfirmation == false )
                    {
                        await SignInManager.SignInAsync( user, isPersistent: false, rememberBrowser: false );
                        //get user and add to session 
                        var appUser = AccountServices.GetUserByKey( user.Id );
                    }
                    else
                    {
                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        string code = await UserManager.GenerateEmailConfirmationTokenAsync( user.Id );
                        var callbackUrl = Url.Action( "ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme );

                        //NOTE: the subject is really a code - do not change it here!!
                        //await UserManager.SendEmailAsync( user.Id, "Confirm_Account", callbackUrl );
                        AccountServices.SendEmail_ConfirmAccount( user.Email, callbackUrl );
                        new AccountServices().Proxies_StoreProxyCode( code, user.Email, "ConfirmEmail" );
                    }
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    //return RedirectToAction("Index", "Home");
                    return RedirectToAction( "RegisterConfirmation", "Account" );
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        [AllowAnonymous]
        public ActionResult RegisterConfirmation()
        {
            return View();
        }
        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            ViewBag.Header = "Confirm Email";
            if (userId == null || code == null)
            {
                ViewBag.Header = "Confirmation Failed";
                ViewBag.Message = "Error: both a user identifier and an access code must be provided.";
                return View( "ConfirmEmail" );
            }
            //new AccountServices().Proxies_StoreProxyCode( code, "mparsons+220321a@credentialengine.org", "ConfirmEmail" );
            if ( !AccountServices.Proxy_IsCodeActive( code ) )
            {
                ViewBag.Header = "Invalid Confirmation Code";
                ViewBag.Message = "Error: The confirmation code is invalid or has expired.";
                return View( "ConfirmEmail" );
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            //return View(result.Succeeded ? "ConfirmEmail" : "Error");
            if ( result.Succeeded )
            {
                new AccountServices().Proxy_SetInactivate( code );

                //activate user
                new AccountServices().ActivateUser( userId );
                //return View( "ConfirmEmail" );
                return View();
            }
            else
            {
                AddErrors( result );
                var msg = "";
                if ( result.Errors != null && result.Errors.Count() > 0 )
                    msg = "Confirmation of email failed: " + result.Errors.ToString();
                else
                    msg = "Confirmation of email failed ";

                ViewBag.Header = "Confirmation Failed";
                ViewBag.Message = msg;
                return View( "ConfirmEmail" );
            }
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool notifyOnEmailNotConfirmed = UtilityManager.GetAppKeyValue( "notifyOnEmailNotConfirmed", false );

                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null )
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    //return View("ForgotPasswordConfirmation");
                    // 16-09-02 mp - actually for now inform user of incorrect email
                    if ( UtilityManager.GetAppKeyValue( "notifyOnEmailNotFound", false ) )
                    {
                        ConsoleMessageHelper.SetConsoleErrorMessage( "Error - the entered email was not found in our system.<br/>Please try again or contact site administration for help" );

                        return View();
                    }
                    else
                    {
                        return View( "~/Views/Account/ForgotPasswordConfirmation.cshtml" );
                    }
                }
                else if ( !( await UserManager.IsEmailConfirmedAsync( user.Id ) ) )
                {
                    if ( notifyOnEmailNotConfirmed == false )
                    {
                        // Don't reveal that the user is not confirmed????
                        //log this in anticipation of issues
                        AccountServices.SendEmail_OnUnConfirmedEmail( model.Email );
                        return View( "~/Views/Account/ForgotPasswordConfirmation.cshtml" );
                    }
                    else
                    {
                        //do we allow fall thru - the reset will not set the email confirmed - should it?
                        //or resend the confirm email, and notify? the user may not know the password, and would have to do a forgot password regardless?

                        string code2 = await UserManager.GenerateEmailConfirmationTokenAsync( user.Id );
                        var callbackUrl2 = Url.Action( "ConfirmEmail", "Account", new { userId = user.Id, code = code2 }, protocol: Request.Url.Scheme );

                        //await UserManager.SendEmailAsync( user.Id, "Confirm Your Account", "Please confirm your account by clicking <a href=\"" + callbackUrl2 + "\">here</a>" );
                        //await UserManager.SendEmailAsync( user.Id, "Confirm_Account", callbackUrl2 );
                        AccountServices.SendEmail_ConfirmAccount( user.Email, callbackUrl2 );
                        ConsoleMessageHelper.SetConsoleInfoMessage( "NOTE - your email has never been confirmed. The confirmation email was resent." );
                        new AccountServices().Proxies_StoreProxyCode( code2, user.Email, "Re-ConfirmEmail" );

                        return View();
                    }
                }
                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync( user.Id );
                var callbackUrl = Url.Action( "ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme );
                //await UserManager.SendEmailAsync( user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>" );
                AccountServices.SendEmail_ResetPassword( user.Email, callbackUrl );
                return RedirectToAction( "ForgotPasswordConfirmation", "Account" );
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel() { Code = code};
            return code == null ? View("Error") : View( model );
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            try
            {
                AuthenticationManager.SignOut( DefaultAuthenticationTypes.ApplicationCookie );
            }
            catch { }
            finally
            {
                Session.Abandon();
                AccountServices.RemoveUserFromSession();
            }
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}