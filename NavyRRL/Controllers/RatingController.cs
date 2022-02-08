﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models.Search;
using Models.Schema;
using Services;
using Navy.Utilities;

namespace NavyRRL.Controllers
{
    public class RatingController : BaseController
	{
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "Rating" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var tempResults = SearchController.CreateFakeResults<Rating>( query.PageNumber, query.PageSize );
			return JsonResponse( tempResults, true );
		}
		//

		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Rating data." );
			var data = Factories.RatingManager.Get( id );
			return View( data );
		}
		//

		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Rating data." );
			var data = Factories.RatingManager.Get( id ) ?? new Rating();
			return View( data );
		}
		//

		public ActionResult Save( Rating data )
		{
			//Validate the request
			if( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Rating data." }, null );
			}

			//On success
			ConsoleMessageHelper.SetConsoleSuccessMessage( "Saved changes successfully." );
			return JsonResponse( data, true, null, null );
		}
		//
    }
}