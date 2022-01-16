﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SM = Models.Search;
using Services;

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

		[HttpPost]
		public ActionResult MainSearch( SM.SearchQuery query )
		{
			//Handle the query
			//Need to get data + total results
			bool valid = true;
			string status = "";
			var results = new SM.SearchResultSet();
			results = new SearchServices().MainSearch( query, ref valid, ref status );
			//Return results
			return JsonResponse( results, valid, null, null );
		}
		//
    }
}