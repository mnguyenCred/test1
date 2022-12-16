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
	[SessionState( System.Web.SessionState.SessionStateBehavior.ReadOnly )]
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

		[CustomAttributes.NavyAuthorize( "Concept View", Roles = SiteReader )]
		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Concept data." );
			var data = Factories.ConceptManager.GetById( id );
			return View( data );
		}
		//

		[CustomAttributes.NavyAuthorize( "Concept View", Roles = SiteReader )]
		public ActionResult GetById( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Concept data." );
			var data = Factories.ConceptManager.GetById( id );
			return JsonResponse( data, data != null );
		}
		//

		[CustomAttributes.NavyAuthorize( "Concept View", Roles = SiteReader )]
		public ActionResult GetByRowId( Guid id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Concept data." );
			var data = Factories.ConceptManager.GetByRowId( id, true );
			return JsonResponse( data, data != null );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Concept data." );
			var data = Factories.ConceptManager.GetById( id );
			var converted = RDFServices.GetRDF( data );
			return RawJSONResponse( converted );
		}
		//

		[CustomAttributes.NavyAuthorize( "Concept Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Concept data." );
			var data = Factories.ConceptManager.GetById( id ) ?? new Concept();
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

			var errors = new List<string>();
			Factories.ConceptManager.SaveFromEditor( data, AccountServices.GetCurrentUser().Id, errors );
			return JsonResponse( data, errors.Count() == 0, errors );
		}
		//
	}
}