using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models.Search;
using Models.Schema;
using Navy.Utilities;
using Services;
using Models.Curation;

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

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Concept data." );
			var data = Factories.ConceptSchemeManager.GetConcept( id );
			var converted = RDFServices.GetRDF( data );
			return RawJSONResponse( converted );
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
			var user = AccountServices.GetCurrentUser();
			ChangeSummary status = new ChangeSummary()
			{
				Action = "Edit"
			};
			data.LastUpdatedById = user.Id;
			var results = new Factories.ConceptSchemeManager().SaveConcept( data, ref status );
			if ( status.HasAnyErrors )
			{
				var msg = string.Join( "</br>", status.Messages.Error.ToArray() );
				ConsoleMessageHelper.SetConsoleErrorMessage( msg );
				return JsonResponse( data, false, status.Messages.Error, null );
			}
			else
			{
				//On success
				ConsoleMessageHelper.SetConsoleSuccessMessage( "Saved changes successfully." );
				return JsonResponse( data, true, null, null );
			}
			
		}
		//
	}
}