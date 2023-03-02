using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Models.Application;
using NavyRRL.Models;
using Navy.Utilities;

using Services;
namespace NavyRRL.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string message = "";
            bool isValid = true;
            //TBD - don't do header check if we have a user
            if (!AccountServices.IsUserAuthenticated())
            {
                var navyUser = new AccountServices().GetNavyUserFromHeader( ref isValid, ref message );
                if ( navyUser != null && !string.IsNullOrWhiteSpace( navyUser.Email ) )
                {
                    AccountServices.SetUserByEmail( navyUser.Email );
                }
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        /// <summary>
        /// placeholder for use where an endpoint is not available yet. 
        /// </summary>
        /// <returns></returns>
        public ActionResult Placeholder()
        {
            return View();
        }

        public ActionResult ContactUs()
        {
            ViewBag.Message = "Contact Us.";

            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //public async Task<ActionResult> ContactUs( ContactUsViewModel model )
        public ActionResult ContactUs( ContactUsViewModel model )
        {
            if ( !ModelState.IsValid )
            {
                return View( model );
            }

            if ( ModelState.IsValid )
            {
                //validate email and text
                //this should already be done
                //how about a valid email address?
                if (string.IsNullOrWhiteSpace( model.Email ) )
                {

                }
                var trimmedEmail = model.Email.Trim();
                string message = "";
                if ( !IsValidEmail( trimmedEmail, ref message )) 
                {
                    ModelState.AddModelError( "", message );
                    return View( model );
                }
                if ( string.IsNullOrWhiteSpace(model.Reason ) || model.Reason.Length < 15)
                {
                    ModelState.AddModelError( "", "Please provide a reason (minimum of 15 characters)" );
                    return View( model );
                }
                AppUser user = new AppUser()
                {
                    Email = trimmedEmail,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                user.Message = GetUserIPAddress();
                AccountServices.SendEmail_ContactUs( user, model.Reason );
                return RedirectToAction( "Index", "Home" );
            }

            // If we got this far, something failed, redisplay form
            return View( model );
        }
        private string GetUserIPAddress()
        {
            string ip = "unknown";
            try
            {
                ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if ( ip == null || ip == "" || ip.ToLower() == "unknown" )
                {
                    ip = Request.ServerVariables["REMOTE_ADDR"];
                }
                if ( ip == "::1" )
                    ip = "localhost";
            }
            catch ( Exception ex )
            {

            }

            return ip;
        } 
		//

        bool IsValidEmail( string email, ref string message )
        {
            var trimmedEmail = email.Trim();

            if ( trimmedEmail.EndsWith( "." ) )
            {
                message = "Email is invalid, must contain a domain.";
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress( email );
                return addr.Address == trimmedEmail;
            }
            catch
            {
                message = "Email is invalid.";

                return false;
            }
        }
        public ActionResult PageNotFound()
        {
            //return View( "~/Views/Home/PageNotFound.cshtml" );
            return View();
        }
    }
}