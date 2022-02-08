﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models.Search;
using Models.Schema;
using Navy.Utilities;
using Services;

namespace NavyRRL.Controllers
{
    public class RatingTaskController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "RatingTask" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var results = SearchServices.RatingTaskSearch( query );

			return JsonResponse( results, true );
		}
		//

		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Rating Task data." );
			var data = Factories.RatingTaskManager.Get( id, false );
			return View( data );
		}
		//

		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Rating Task data." );
			var data = Factories.RatingTaskManager.Get( id, false ) ?? new RatingTask();
			return View( data );
		}
		//

		public ActionResult Save( RatingTask data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Rating Task data." }, null );
			}

			//On success
			ConsoleMessageHelper.SetConsoleSuccessMessage( "Saved changes successfully." );
			return JsonResponse( data, true, null, null );
		}
		//
	}
}