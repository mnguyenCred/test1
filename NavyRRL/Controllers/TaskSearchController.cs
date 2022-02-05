using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AM = Models.Application;
using SM = Models.Search;
using Services;

namespace NavyRRL.Controllers
{
    public class TaskSearchController : BaseController
    {
		[Authorize( Roles = "Administrator, Site Staff" )]
		public ActionResult Index()
        {
			if (!AccountServices.IsUserAuthenticated())
            {
				AM.SiteMessage siteMessage = new AM.SiteMessage()
				{
					Title = "Invalid Request",
					Message = "You must be authenticated and authorized to use this feature"
				};
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