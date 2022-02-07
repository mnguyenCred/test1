using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AM = Models.Application;
using Models.Curation;
using Navy.Utilities;
using Services;
using Models.Application;
using Models.Schema;
using System.Threading.Tasks;

namespace NavyRRL.Controllers
{
    public class DataController : Controller
    {

        public bool HasAuthorization()
        {
            AppUser user = AccountServices.GetCurrentUser();

            if ( !AccountServices.IsUserAuthenticated( user ) )
            {
                Session["siteMessage"] = AccountServices.NOT_AUTHENTICATED;
                ConsoleMessageHelper.SetConsoleErrorMessage( AccountServices.NOT_AUTHENTICATED );
                return false;
            }
            else if ( !AccountServices.IsUserSiteStaff( user ) )
            {
                Session["siteMessage"] = AccountServices.NOT_AUTHORIZED;

                ConsoleMessageHelper.SetConsoleErrorMessage( AccountServices.NOT_AUTHORIZED );
                return false;
            }
            return true;
        }
        // GET: Data
        public ActionResult Index()
        {
            AppUser user = AccountServices.GetCurrentUser();

            if ( !AccountServices.IsUserAuthenticated( user ) )
            {
                ConsoleMessageHelper.SetConsoleErrorMessage( AccountServices.NOT_AUTHENTICATED );
                return RedirectToAction( AccountServices.EVENT_AUTHENTICATED, "event" );
            }
            else if ( !AccountServices.IsUserSiteStaff( user ) )
            {
                ConsoleMessageHelper.SetConsoleErrorMessage( AccountServices.NOT_AUTHORIZED );
                return RedirectToAction( AccountServices.EVENT_AUTHORIZED, "event" );
            }
            return View();
        }

        public ActionResult ManageCCA()
        {
            string status = "";
            if ( !AccountServices.AuthorizationCheck( "", true, ref status ) )
            {

            }
            return View();
        }

        public ActionResult ManageCourses()
        {
            if ( !HasAuthorization() )
            {
                return RedirectToAction( "index", "event" );
            }
            return View();
        }
        [CustomAttributes.NavyAuthorize( "ManageConceptSchemes", Roles = "Administrator, Site Staff" )]
        public ActionResult ManageConceptSchemes()
        {
            return View();
        }


        #region  Functional Area
        [CustomAttributes.NavyAuthorize( "ManageFunctionalArea", Roles = "Administrator, Site Staff" )]
        public ActionResult ManageFunctionalArea()
        {
            List<WorkRole> input = RatingTaskServices.GetAllFunctionalAreas();
            return View( input );
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveFunctionalArea( int recordId, string name )
        {
            //long _id = 0;
            //if ( !string.IsNullOrEmpty( recordId ) && !string.IsNullOrEmpty( name ) )
            //{
            //    long.TryParse( custId, out _CustId );
            //}
            ChangeSummary status = new ChangeSummary();
            WorkRole workRole = new WorkRole()
            {
                Id = recordId,
                Name = name
            };
            var result = new RatingTaskServices().SaveFunctionalArea( workRole, ref status );

            return Json( new { status = "Success" } );
        }

        public ActionResult DeleteFunctionalArea( string recordId )
        {
            List<WorkRole> input = RatingTaskServices.GetAllFunctionalAreas();
            return View( input );
        }


        #endregion
    }
}