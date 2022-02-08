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
    public class RMTLProjectController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "RMTLProject" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var tempResults = SearchController.CreateFakeResults<RMTLProject>( query.PageNumber, query.PageSize );
			return JsonResponse( tempResults, true );
		}
		//

		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view RMTL Project data." );
			var data = new RMTLProject() { Name = "Temp Name", Description = "Temp Description" };
			return View( data );
		}
		//

		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit RMTL Project data." );
			var data = new RMTLProject(); //Should get by ID or default to new (to enable new RMTL Projects to be created)
			return View( data );
		}
		//

		public ActionResult Save( RMTLProject data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit RMTL Project data." }, null );
			}

			//On success
			ConsoleMessageHelper.SetConsoleSuccessMessage( "Saved changes successfully." );
			return JsonResponse( data, true, null, null );
		}
		//
	}
}