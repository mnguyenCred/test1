using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

using Navy.Utilities;

using NavyRRL.Models;
using Services;

namespace NavyRRL.Areas.Admin.Controllers
{
    public class UserController : Controller
    {
        public static string thisClassName = "Admin.UserController";
        //HACK: UserManager works fine in the AccountController but not here. Adding the following solved the problem
        //22-01-21 - now doesn't work
        public UserManager<ApplicationUser> UserManager { get; private set; }

        // GET: Admin/User
        public ActionResult Index()
        {
            return View();
        }

        #region Add User
        [Authorize( Roles = "Administrator, Site Manager" )]
        public ActionResult AddUser()
        {
            //return View();
            return View();
        }

        [HttpPost]
        //[RequireHttps]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser( RegisterViewModel model )
        {
            int currentUserId = AccountServices.GetCurrentUserId();
            if ( ModelState.IsValid )
            {
                string statusMessage = "";
                var user = new ApplicationUser
                {
                    UserName = model.Email.Trim(),
                    Email = model.Email.Trim(),
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim()
                };
                //See HACK at top of page
                var result = await UserManager.CreateAsync( user, model.Password );

                if ( result.Succeeded )
                {
                    int id = new AccountServices().AddAccount( model.Email,
                        model.FirstName, model.LastName,
                        model.Email, user.Id,
                        model.Password, ref statusMessage );
                    if ( id > 0 )
                    {
                        string msg = "Successfully created account for {0}. ";
                        
                        ConsoleMessageHelper.SetConsoleSuccessMessage( string.Format( msg, user.FirstName ) );
                        //return View( "ConfirmationRequired" );
                        ModelState.Clear();
                        return View();
                    }
                    else
                    {
                        ConsoleMessageHelper.SetConsoleErrorMessage( "Error - " + statusMessage );
                        return View();
                    }
                }
                AddErrors( result );
            }

            // If we got this far, something failed, redisplay form

            return View();
        }
        #endregion
        public ActionResult LoadRecords()
            {
            var result = new JsonResult();

            try
            {
                #region According to Datatables.net, Server side parameters

                //global search
                var search = Request.Form["search[value]"];
                var draw = Request.Form["draw"];

                var orderBy = string.Empty;
                //column index
                var order = int.Parse( Request.Form["order[0][column]"] );
                //sort direction
                var orderDir = Request.Form["order[0][dir]"];

                int startRec = int.Parse( Request.Form["start"] );
                int pageSize = int.Parse( Request.Form["length"] );

                #endregion

                #region Where filter

                //individual column wise search
                var columnSearch = new List<string>();
                var globalSearch = new List<string>();
                DateTime dt = new DateTime();

                //Get all keys starting with columns    
                foreach ( var index in Request.Form.AllKeys.Where( x => Regex.Match( x, @"columns\[(\d+)]" ).Success ).Select( x => int.Parse( Regex.Match( x, @"\d+" ).Value ) ).Distinct().ToList() )
                {
                    //get individual columns search value
                    var value = Request.Form[string.Format( "columns[{0}][search][value]", index )];
                    if ( !string.IsNullOrWhiteSpace( value ) )
                    {
                        value = value.Trim();
                        string colName = Request.Form[string.Format( "columns[{0}][data]", index )];
                        if ( colName == "lastLogon" )
                        {
                            if ( DateTime.TryParse( value, out dt ) )
                            {
                                columnSearch.Add( string.Format( " (convert(varchar(10),{0},120) = '{1}') ", colName, dt.ToString( "yyyy-MM-dd" ) ) );
                            }
                        }
                        else
                            columnSearch.Add( string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], value ) );
                    }
                    //get column filter for global search
                    if ( !string.IsNullOrWhiteSpace( search ) )
                        globalSearch.Add( string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], search ) );

                    //get order by from order index
                    if ( order == index )
                        orderBy = Request.Form[string.Format( "columns[{0}][data]", index )];
                }

                var where = string.Empty;
                //concat all filters for global search
                if ( globalSearch.Any() )
                    where = globalSearch.Aggregate( ( current, next ) => current + " OR " + next );

                if ( columnSearch.Any() )
                    if ( !string.IsNullOrEmpty( where ) )
                        where = string.Format( "({0}) AND ({1})", where, columnSearch.Aggregate( ( current, next ) => current + " AND " + next ) );
                    else
                        where = columnSearch.Aggregate( ( current, next ) => current + " AND " + next );

                #endregion

                var totalRecords = 0;
                var list = AccountServices.Search( where, orderBy, orderDir, startRec / pageSize, pageSize, ref totalRecords );

                result = Json( new { data = list, draw = int.Parse( draw ), recordsTotal = totalRecords, recordsFiltered = totalRecords }, JsonRequestBehavior.AllowGet );
            }
            catch ( Exception ex )
            {
                LoggingHelper.DoTrace( 1, thisClassName + ".Search. Exception: " + ex.Message );

            }

            return result;
        }

        [Authorize( Roles = "Administrator, Site Manager" )]
        public ActionResult EditAccount( int id )
        {
            var account = AccountServices.GetAccount( id );
            if ( account != null )
            {
                var model = new AccountViewModel { UserId = account.Id, Email = account.Email, FirstName = account.FirstName, LastName = account.LastName };
                var roles = AccountServices.GetRoles();
                model.SelectedRoles = roles.Where( x => account.UserRoles.Contains( x.Name ) ).Select( x => x.Id ).ToArray();
                model.Roles = roles.Select( x => new SelectListItem { Text = x.Name, Value = x.Id, Selected = account.UserRoles.Contains( x.Name ) } ).ToList();

                //model.SelectedOrgs = account.Organizations.Select( x => x.Id ).ToArray();

                //If Organizations not found, get top 3 Organizations to show
                //if (!account.Organizations.Any())
                //    account.Organizations = AccountServices.GetOrganizations(string.Empty).OrderBy(x => x.Name).Take(3).Select(x => new CodeItem { Id = x.Id, Name = x.Name }).ToList();

                //model.Organizations = account.Organizations.Select( x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = true } ).ToList();
                return PartialView( model );
            }

            return HttpNotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAccount( AccountViewModel model )
        {
            var account = AccountServices.GetAccount( model.UserId );
            if ( account != null )
            {
                if ( ModelState.IsValid )
                {
                    account.FirstName = model.FirstName;
                    account.LastName = model.LastName;
                    account.Email = model.Email;

                    //Update Account and AspNetUser
                    var message = string.Empty;
                    var success = new AccountServices().Update( account, false, ref message );
                    if ( success )
                    {
                        //if null, should there be a check to delete all??
                        //if (model.SelectedRoles != null)
                        //Bulk Add/Remove Roles
                        new AccountServices().UpdateRoles( account.AspNetUserId, model.SelectedRoles );

                        //if null, should there be a check to delete all??
                        //Bulk Add/Remove Organizations
                        //OrganizationServices.UpdateUserOrganizations( model.UserId, model.SelectedOrgs, User.Identity.Name );
                    }

                    Response.StatusCode = ( int ) HttpStatusCode.OK;
                    return RedirectToAction( "EditAccount", new { id = model.UserId } );
                }
                else
                {
                    Response.StatusCode = ( int ) HttpStatusCode.InternalServerError;
                    ModelState.AddModelError( "", string.Join( Environment.NewLine, ModelState.Values.SelectMany( x => x.Errors ).Select( x => x.ErrorMessage ) ) );
                }

                var roles = AccountServices.GetRoles();
                model.SelectedRoles = roles.Where( x => account.UserRoles.Contains( x.Name ) ).Select( x => x.Id ).ToArray();
                model.Roles = roles.Select( x => new SelectListItem { Text = x.Name, Value = x.Id, Selected = account.UserRoles.Contains( x.Name ) } ).ToList();

                //model.SelectedOrgs = account.Organizations.Select( x => x.Id ).ToArray();
                //model.Organizations = account.Organizations.Select( x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = true } ).ToList();
            }
            return PartialView( model );
        }
        public void DeleteAccount( int id )
        {
            if ( !AccountServices.IsUserAuthenticated() )
            {
                ConsoleMessageHelper.SetConsoleErrorMessage( "You must be logged in and authorized to perform this action." );

                //return RedirectToAction( "Login", "Account", new { area = "" } );
            }
            else
            {
                var deletedBy = AccountServices.GetCurrentUser();
                //Update Account IsActive
                var message = string.Empty;
                new AccountServices().Delete( id, deletedBy, ref message );
            }
        }
        private void AddErrors( IdentityResult result )
        {
            foreach ( var error in result.Errors )
            {
                ModelState.AddModelError( "", error );
            }
        }

        #region Reset password
        [Authorize( Roles = "Administrator, Site Staff" )]
        public ActionResult ResetPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword( ResetPasswordViewModel model )
        {
            if ( !ModelState.IsValid )
            {
                return View( model );
            }

            var user = await UserManager.FindByEmailAsync( model.Email );
            if ( user == null )
            {
                ModelState.AddModelError( "", "The requested user email was not found." );
                return View( model );
            }
            string code = await UserManager.GeneratePasswordResetTokenAsync( user.Id );
            var result = await UserManager.ResetPasswordAsync( user.Id, code, model.Password );
            if ( result.Succeeded )
            {
                new AccountServices().SetUserEmailConfirmed( user.Id );
                return RedirectToAction( "ResetPasswordConfirmation", "User" );
            }
            AddErrors( result );


            return View();

        }
        public ActionResult ResetPasswordConfirmation()
        {
            return View();

        }
        #endregion
    }
}