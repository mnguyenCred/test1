using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SM = Models.Search;

namespace NavyRRL.Controllers
{
    public class SearchController : BaseController
    {
        // GET: Search
        public ActionResult Index()
        {
            return View( "~/views/search/searchv1.cshtml" );
        }
		//

		public ActionResult SearchV2()
		{
			return View( "~/views/search/searchv2.cshtml" );
		}
		//

		public ActionResult MainSearch( SM.SearchQuery query )
		{
			//Handle the query
			//Need to get data + total results
			var results = new SM.SearchResultSet();

			//Return results
			return JsonResponse( results, true, null, null );
		}
		//
    }
}