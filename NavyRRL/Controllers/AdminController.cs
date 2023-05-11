using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Microsoft.AspNet.Identity;

using NavyRRL.Models;
using Navy.Utilities;

using Models.Application;
using Models.Import;
using Services;



namespace NavyRRL.Controllers
{
   // [Authorize( Roles = "Administrator, Site Staff" )]
    public class AdminController : BaseController
    {
        public static string thisClassName = "AdminController";

        //these could possibly become app config strings for more general auth that may change?
        //public const string Admin_SiteManager_SiteStaff = "Administrator, Site Manager, Site Staff";
        //public const string Admin_SiteManager = "Administrator, Site Manager";
        // GET: Admin
        //NOTE: Authorize kicks user back to login page. Need a custom option where already logged in 
        ////[CustomAttributes.NavyAuthorize( "Admin Home", Roles = Admin_SiteManager )]
        public ActionResult Index()
        {
            if (AccountServices.IsUserAnAdmin())
                return View();
            else
            {
                return new RedirectResult( "~/event/Notauthorized" );
            }
        }

		public ActionResult FormHelperV1Demo()
		{
			return View();
		}

		[HttpGet]
		public ActionResult ManageRolePermissions()
		{
			return View();
		}

		[HttpGet]
		public ActionResult GetAllUserRoles()
		{
            var output = new List<UserRole>();
            //Temporary/test data to be replaced with database call
            var roles = AccountServices.GetAllApplicationRoles();
            //foreach( var item in roles2)
            //{
            //    var role = new UserRole()
            //    {
            //        Name = item.Name,
            //    };
            //    //convert
            //    role.Id = int.Parse(item.Id);
            //    if ( item.AppFunctionPermission != null && item.AppFunctionPermission.Count > 0 )
            //    {
            //        var HasApplicationFunctionIds = item.AppFunctionPermission.Select( a => a.ApplicationFunctionId ).ToList();
            //    }

            //    output.Add( role);
            //}

   //         var roles = new List<UserRole>()
			//{
			//	new UserRole()
			//	{
			//		Id = 99991,
			//		Name = "Test Role 1",
			//		HasApplicationFunctionIds = new List<int>() { 991, 992 }
			//	},
			//	new UserRole()
			//	{
			//		Id = 99992,
			//		Name = "Test Role 2",
			//		HasApplicationFunctionIds = new List<int>() { 993 }
			//	},
			//	new UserRole()
			//	{
			//		Id = 99993,
			//		Name = "Test Role 3",
			//		HasApplicationFunctionIds = new List<int>() { 991, 994 }
			//	}
			//};

			//Return response
			return BaseController.JsonResponse( roles, true );
		}

		[HttpPost]
		public ActionResult SaveUserRole( UserRole role )
        {
            //Save the user role
            string statusMessage = "";
            var isValid = new AccountServices().SaveApplicationRolePermissions( role, ref statusMessage );
            //Return response
            return BaseController.JsonResponse( null, isValid );
		}

        /// <summary>
        /// Called from ManageRolePermissions.cshtml
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
		[HttpPost]
		public ActionResult DeleteUserRole( UserRole role )
		{
            var status = "";
            //Delete the ApplicationRole (probably should see if anything uses it first)
            var result = new AccountServices().DeleteApplicationRole( role, ref status );
			//Return response
			return BaseController.JsonResponse( null, false, new List<string>() { "Error - That role is still assigned to one or more users." } );
		}

        #region  Activity
        // GET: Admin/Activity
        ////[CustomAttributes.NavyAuthorize( "Admin Home", Roles = Admin_SiteManager_SiteStaff )]
        public ActionResult Activity()
        {
            if ( AccountServices.IsUserSiteStaff() )
                return View();
            else
            {
                return new RedirectResult( "~/event/Notauthorized" );
            }
          
        }
        /// <summary>
        /// Current Activity Search
        /// </summary>
        /// <returns></returns>
        public ActionResult ActivitySearch()
        {
            var result = new JsonResult();
            List<SiteActivity> list = new List<SiteActivity>();
            //global search
            var search = Request.Form["search[value]"];
            //what is draw??
            var draw = Request.Form["draw"];
            try
            {
                #region According to Datatables.net, Server side parameters                

                var orderBy = string.Empty;
                //column index
                var order = int.Parse( Request.Form["order[0][column]"] );
                //sort direction
                var orderDir = Request.Form["order[0][dir]"];

                int startRec = int.Parse( Request.Form["start"] );
                int pageSize = int.Parse( Request.Form["length"] );
                int pageNbr = ( startRec / pageSize ) + 1;
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
                        if ( colName == "DisplayDate" || colName == "Created" )
                        {
                            if ( DateTime.TryParse( value, out dt ) )
                            {
                                columnSearch.Add( string.Format( " (convert(varchar(10),CreatedDate,120) = '{0}') ", dt.ToString( "yyyy-MM-dd" ) ) );
                            }
                        }
                        else if ( colName == "ParentRecordId" || colName == "ParentEntityTypeId" || colName == "ActivityObjectId" || colName == "ActionByUserId" )
                        {
                            columnSearch.Add( string.Format( " ( {0} = {1} ) ", colName, value ) );
                        }
                        else
                        {
                            if ( value.Length > 1 && value.IndexOf( "!" ) == 0 )
                                columnSearch.Add( string.Format( "({0} NOT LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], value.Substring( 1 ) ) );
                            else
                                columnSearch.Add( string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], value ) );
                        }
                    }
                    //get column filter for global search
                    if ( !string.IsNullOrWhiteSpace( search ) )
                    {
                        if ( index > 0 ) //skip date
                            globalSearch.Add( string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], search ) );
                    }


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


                BaseSearchModel parms = new BaseSearchModel()
                {
                    Keyword = "",
                    OrderBy = orderBy,
                    IsDescending = orderDir == "desc" ? true : false,
                    PageNumber = pageNbr,
                    PageSize = pageSize
                };
                parms.Filter = where;
                var totalRecords = 0;
                list = ActivityServices.Search( parms, ref totalRecords );

                result = Json( new { data = list, draw = int.Parse( draw ), recordsTotal = totalRecords, recordsFiltered = totalRecords }, JsonRequestBehavior.AllowGet );
            }
            catch ( Exception ex )
            {
                LoggingHelper.DoTrace( 1, "ActivityController.DoSearch. " + ex.Message );
                list.Add( new SiteActivity() { ActivityType = "ActivityLog", Activity = search, Event = "Error", Comment = ex.Message } );
                result = Json( new { data = list, draw = int.Parse( draw ), recordsTotal = 1, recordsFiltered = 1 }, JsonRequestBehavior.AllowGet );
            }

            return result;
        }
        #endregion


        #region Users 
        ////[CustomAttributes.NavyAuthorize( "Admin Accounts", Roles = Admin_SiteManager )]
        public ActionResult Accounts()
        {
            if ( !AccountServices.IsUserAnAdmin() )
            {
                return new RedirectResult( "~/event/Notauthorized" );
            }
            return View();
        }

        public ActionResult AccountSearch()
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

       // //[CustomAttributes.NavyAuthorize( "Admin Edit Account", Roles = Admin_SiteManager )]
        public ActionResult EditAccount( int id )
        {
            if ( !AccountServices.IsUserAnAdmin() )
            {
                return new RedirectResult( "~/event/Notauthorized" );
            }
            var account = AccountServices.GetAccount( id );
            if ( account != null )
            {
                var model = new AccountViewModel { UserId = account.Id, Email = account.Email, FirstName = account.FirstName, LastName = account.LastName, Identifier = account.ExternalAccountIdentifier };
                //var roles2 = AccountServices.GetRoles();

                //model.SelectedRoles = roles.Where( x => account.UserRoles.Contains( x.Name ) ).Select( x => x.Id ).ToArray();
                //model.Roles = roles.Select( x => new SelectListItem { Text = x.Name, Value = x.Id, Selected = account.UserRoles.Contains( x.Name ) } ).ToList();

                var roles = AccountServices.GetAllApplicationRoles();
                model.SelectedRoles = roles.Where( x => account.UserRoles.Contains( x.Name ) ).Select( x => x.Id.ToString() ).ToArray();
                model.Roles = roles.Select( x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = account.UserRoles.Contains( x.Name ) } ).ToList();

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
                    account.ExternalAccountIdentifier = model.Identifier;

                    //Update Account and AspNetUser
                    var message = string.Empty;
                    var success = new AccountServices().Update( account, false, ref message );
                    if ( success )
                    {
                        //if null, should there be a check to delete all??
                        //if (model.SelectedRoles != null)
                        //Bulk Add/Remove Roles
                        //new AccountServices().UpdateRolesForUserOld( account.AspNetUserId, model.SelectedRoles );
                        //convert to int
                        var inputRoles = model.SelectedRoles.Select( Int32.Parse )?.ToList();
                        new AccountServices().UpdateRolesForUser( account.Id, inputRoles );

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

                //var roles = AccountServices.GetRoles();
                //model.SelectedRoles = roles.Where( x => account.UserRoles.Contains( x.Name ) ).Select( x => x.Id ).ToArray();
                //model.Roles = roles.Select( x => new SelectListItem { Text = x.Name, Value = x.Id, Selected = account.UserRoles.Contains( x.Name ) } ).ToList();

                //new 
                var roles = AccountServices.GetAllApplicationRoles();
                model.SelectedRoles = roles.Where( x => account.UserRoles.Contains( x.Name ) ).Select( x => x.Id.ToString() ).ToArray();
                model.Roles = roles.Select( x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = account.UserRoles.Contains( x.Name ) } ).ToList();
            }
            return PartialView( model );
        }

        ////[CustomAttributes.NavyAuthorize( "Admin Delete Account", Roles = Admin_SiteManager )]
        public void DeleteAccount( int id )
        {
            if ( !AccountServices.IsUserAuthenticated() )
            {
                ConsoleMessageHelper.SetConsoleErrorMessage( AccountServices.NOT_AUTHENTICATED );

                //return RedirectToAction( "Login", "Account", new { area = "" } );
            }
            else
            {
                if ( !AccountServices.IsUserAnAdmin() )
                {
                    ConsoleMessageHelper.SetConsoleErrorMessage( AccountServices.NOT_AUTHORIZED );
                    return;
                }
                var deletedBy = AccountServices.GetCurrentUser();
                //Update Account IsActive
                var message = string.Empty;
                new AccountServices().Delete( id, deletedBy, ref message );
            }
        }

        public JsonResult GetOrganizations( string keyword )
        {
            //var result = null;
            return Json( null, JsonRequestBehavior.AllowGet );
        }
        private void AddErrors( IdentityResult result )
        {
            foreach ( var error in result.Errors )
            {
                ModelState.AddModelError( "", error );
            }
        }
        #endregion


        #region Role testing
        ////[CustomAttributes.NavyAuthorize( "Admin RmtlDeveloperOnly", Roles = "RMTL Developer" )]
        public ActionResult RmtlDeveloperOnly()
        {
            //should not get here with out this role, but just in case
            AppUser user = AccountServices.GetCurrentUser();
            if (user?.Id > 0)
            {
                if (AccountServices.HasRmtlDeveloperAccess(user ))
                {
                    ConsoleMessageHelper.SetConsoleSuccessMessage( "Success, has the proper role." );
                    return RedirectToAction( "Index", "event");
                }
                else
                {
                    ConsoleMessageHelper.SetConsoleErrorMessage( AccountServices.NOT_AUTHORIZED );
                    return RedirectToAction( AccountServices.EVENT_AUTHORIZED, "event" );

                }
            }
            else
            {
                ConsoleMessageHelper.SetConsoleErrorMessage( AccountServices.NOT_AUTHENTICATED );
                return RedirectToAction( AccountServices.EVENT_AUTHENTICATED, "event" );
            }
          
        }

        ////[CustomAttributes.NavyAuthorize( "Admin RatingContinuumManagerOnly Home", Roles = "Rating Continuum Manager" )]
        public ActionResult RatingContinuumManagerOnly()
        {
            return RedirectToAction( "index", "event" );
        }

       // //[CustomAttributes.NavyAuthorize( "Admin RCDAnalystOnly", Roles = "Rating Continuum Development Analyst" )]
        public ActionResult RCDAnalystOnly()
        {
            return RedirectToAction( "index", "event" );
        }

        #endregion
    }
}