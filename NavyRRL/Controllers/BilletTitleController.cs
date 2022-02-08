using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models.Search;
using Models.Schema;
using Navy.Utilities;

namespace NavyRRL.Controllers
{
    public class BilletTitleController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "BilletTitle" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var tempResults = SearchController.CreateFakeResults<BilletTitle>( query.PageNumber, query.PageSize );
			return JsonResponse( tempResults, true );
		}
		//

		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Billet Title data." );
			var data = new BilletTitle() { Name = "Temp name" }; //Not sure how to get a billet title
			return View( data );
		}
		//

		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Billet Title data." );
			var data = new BilletTitle(); //Should get by ID or default to new (to enable new billet titles to be created)
			return View( data );
		}
		//

		public ActionResult Save( BilletTitle data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Billet Title data." }, null );
			}

			//On success
			ConsoleMessageHelper.SetConsoleSuccessMessage( "Saved changes successfully." );
			return JsonResponse( data, true, null, null );
		}
		//
	}
}