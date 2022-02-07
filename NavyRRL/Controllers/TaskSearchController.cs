using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AM = Models.Application;
using SM = Models.Search;
using Services;
using Navy.Utilities;

namespace NavyRRL.Controllers
{
    public class TaskSearchController : BaseController
    {
		// GET: Search
		//Changed to custom Authorize as for the base Authorized, if user doesn't have role they immediately get sent back to log, no message!
		[CustomAttributes.NavyAuthorize( "Search", Roles = "Administrator, RMTL Developer, Site Staff" )]
		public ActionResult Index()
        {
			if (!AccountServices.IsUserAuthenticated())
            {
				AM.SiteMessage siteMessage = new AM.SiteMessage()
				{
					Title = "Invalid Request",
					Message = AccountServices.NOT_AUTHENTICATED
				};
				ConsoleMessageHelper.SetConsoleErrorMessage( AccountServices.NOT_AUTHENTICATED );
				return RedirectToAction( AccountServices.EVENT_AUTHENTICATED, "event", new { area = "" } );
			} else
            {

            }
            return View( "~/views/tasksearch/tasksearchv2.cshtml" );
        }
		//
		
		public ActionResult TaskSearchV1()
		{
			return View( "~/views/tasksearch/tasksearchv1.cshtml" );
		}
		//

		public ActionResult TaskSearchV2()
		{
			return View( "~/views/tasksearch/tasksearchv2.cshtml" );
		}
		//
		public ActionResult SearchV3()
		{
			return View( "~/views/search/searchv3.cshtml" );
		}
		//
		[HttpPost]
		public ActionResult MainSearch( SM.SearchQuery query )
		{
			bool valid = true;
			string status = "";
			var results = new SearchServices().MainSearch( query, ref valid, ref status );

			return JsonResponse( results, valid, new List<string>() { status }, null );
		}
		//
    }
}