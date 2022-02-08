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
    public class WorkRoleController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "WorkRole" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var tempResults = SearchController.CreateFakeResults<WorkRole>( query.PageNumber, query.PageSize );
			return JsonResponse( tempResults, true );
		}
		//

		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Functional Area data." );
			var data = Factories.WorkRoleManager.Get( id );
			return View( data );
		}
		//

		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Functional Area data." );
			var data = Factories.WorkRoleManager.Get( id ) ?? new WorkRole();
			return View( data );
		}
		//

		public ActionResult Save( WorkRole data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Functional Area data." }, null );
			}

			//On success
			ConsoleMessageHelper.SetConsoleSuccessMessage( "Saved changes successfully." );
			return JsonResponse( data, true, null, null );
		}
		//
	}
}