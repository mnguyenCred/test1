using System;
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
    public class ConceptController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "Concept" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var results = SearchServices.ConceptSearch( query );

			return JsonResponse( results, true );
		}
		//

		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Concept data." );
			var data = Factories.ConceptSchemeManager.GetConcept( id );
			return View( data );
		}
		//

		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Concept data." );
			var data = Factories.ConceptSchemeManager.GetConcept( id ) ?? new Concept();
			return View( data );
		}
		//

		public ActionResult Save( Concept data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Concept data." }, null );
			}

			//On success
			ConsoleMessageHelper.SetConsoleSuccessMessage( "Saved changes successfully." );
			return JsonResponse( data, true, null, null );
		}
		//
	}
}