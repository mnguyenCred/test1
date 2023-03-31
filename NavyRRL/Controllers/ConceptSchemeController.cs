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
	public class ConceptSchemeController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "ConceptScheme" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Concept Scheme data." );
			var results = SearchServices.ConceptSchemeSearch( query );

			return JsonResponse( results, true );
		}
		//

		//SiteReader should just be everyone, so do we need this?
		//[CustomAttributes.NavyAuthorize( "Concept Scheme View", Roles = SiteReader )]
		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Concept Scheme data." );
			var data = Factories.ConceptSchemeManager.GetById( id );
			//data.Concepts = Factories.ConceptManager.GetAllConceptsForScheme( data.SchemaUri, false ); //Enable seeing disabled concepts on the detail page
			return View( data );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Concept Scheme data." );
			var data = Factories.ConceptSchemeManager.GetById( id );
			var converted = RDFServices.GetRDF( data, null );
			return RawJSONResponse( converted );
		}
		//

		public ActionResult GetById( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Concept Scheme data." );
			var data = Factories.ConceptSchemeManager.GetById( id, true );
			return JsonResponse( data, data != null );
		}
		//

		public ActionResult GetByRowId( Guid id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Concept Scheme data." );
			var data = Factories.ConceptSchemeManager.GetByRowId( id, true );
			return JsonResponse( data, data != null );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Concept Scheme Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Concept Scheme data." );
			if ( !AccountServices.IsUserSiteStaff() )
			{
				RedirectToAction( "NotAuthenticated", "Event" );
			}

			var data = Factories.ConceptSchemeManager.GetById( id ) ?? new ConceptScheme();
			return View( data );
		}
		//

		public ActionResult Save( ConceptScheme data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Concept Scheme data." }, null );
			}

			var errors = new List<string>();
			Factories.ConceptSchemeManager.SaveFromEditor( data, AccountServices.GetCurrentUser().Id, errors );
			return JsonResponse( data, errors.Count() == 0, errors );
		}
		//

		public ActionResult Delete( int id )
		{
			if ( !AccountServices.IsUserAnAdmin() )
			{
				return JsonResponse( null, false, new List<string>() { "Deleting data requires administrator privileges." } );
			}

			var result = Factories.ConceptSchemeManager.DeleteById( id );
			return JsonResponse( result, result.Successful, result.Messages );
		}
		//
	}
}