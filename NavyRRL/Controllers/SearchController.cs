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
    public class SearchController : BaseController
    {
		// GET: Search
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
            return View( "~/views/search/searchv2.cshtml" );
        }
		//
		public ActionResult SearchV1()
		{
			return View( "~/views/search/searchv1.cshtml" );
		}
		public ActionResult SearchV2()
		{
			return View( "~/views/search/searchv2.cshtml" );
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