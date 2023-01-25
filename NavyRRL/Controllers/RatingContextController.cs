using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models.Search;
using Models.Schema;
using Services;
using Navy.Utilities;
using Models.Curation;

namespace NavyRRL.Controllers
{
	[SessionState( System.Web.SessionState.SessionStateBehavior.ReadOnly )]
	public class RatingContextController : BaseController
	{
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "RatingContext" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var results = SearchServices.RatingContextSearch( query );

			return JsonResponse( results, true );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Rating Context View", Roles = SiteReader )]
		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Rating Context data." );
			var data = Factories.RatingContextManager.GetById( id );

			return View( data );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Rating Context View", Roles = SiteReader )]
		public ActionResult GetByRowId( Guid id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Rating Context data." );
			var data = Factories.RatingContextManager.GetByRowId( id, true );
			return JsonResponse( data, data != null );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Rating Context data." );
			var data = Factories.RatingContextManager.GetById( id );
			var converted = RDFServices.GetRDF( data );

			return RawJSONResponse( converted );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Rating Context Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Rating Context data." );
			if ( !AccountServices.IsUserSiteStaff() )
			{
				RedirectToAction( "NotAuthenticated", "Event" );
			}

			var data = Factories.RatingContextManager.GetById( id );
			return View( data );
		}
		//

		public ActionResult Save( RatingContext data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Rating Context data." }, null );
			}

			var errors = new List<string>();
			Factories.RatingContextManager.SaveFromEditor( data, AccountServices.GetCurrentUser().Id, errors );
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