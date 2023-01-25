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
	public class ReferenceResourceController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "ReferenceResource" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var results = SearchServices.ReferenceResourceSearch( query );

			return JsonResponse( results, true );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Reference Resource View", Roles = SiteReader )]
		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Reference Resource data." );
			var data = Factories.ReferenceResourceManager.GetById( id );
			return View( data );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Reference Resource View", Roles = SiteReader )]
		public ActionResult GetByRowId( Guid id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Reference Resource data." );
			var data = Factories.ReferenceResourceManager.GetByRowId( id, true );
			return JsonResponse( data, data != null );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Reference Resource data." );
			var data = Factories.ReferenceResourceManager.GetById( id );
			var converted = RDFServices.GetRDF( data );
			return RawJSONResponse( converted );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Reference Resource Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Reference Resource data." );
			if ( !AccountServices.IsUserSiteStaff() )
			{
				RedirectToAction( "NotAuthenticated", "Event" );
			}

			var data = Factories.ReferenceResourceManager.GetById( id ) ?? new ReferenceResource();
			return View( data );
		}
		//

		public ActionResult Save( ReferenceResource data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Reference Resource data." }, null );
			}

			var errors = new List<string>();
			Factories.ReferenceResourceManager.SaveFromEditor( data, AccountServices.GetCurrentUser().Id, errors );
			return JsonResponse( data, errors.Count() == 0, errors );
		}
		//

		public ActionResult Delete( int id )
		{
			if ( !AccountServices.IsUserAnAdmin() )
			{
				return JsonResponse( null, false, new List<string>() { "Deleting data requires administrator privileges." } );
			}

			return JsonResponse( null, false, new List<string>() { "This feature is not implemented yet." } );
		}
		//
	}
}