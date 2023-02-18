using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models.Search;
using Models.Schema;
using Navy.Utilities;
using Models.Curation;
using Services;

namespace NavyRRL.Controllers
{
	[SessionState( System.Web.SessionState.SessionStateBehavior.ReadOnly )]
	public class WorkRoleController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "WorkRole" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Functional Area data." );
			var results = SearchServices.WorkRoleSearch( query );

			return JsonResponse( results, true );
		}
		//

		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Functional Area data." );
			var data = Factories.WorkRoleManager.GetById( id );
			return View( data );
		}
		//

		public ActionResult GetByRowId( Guid id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Functional Area data." );
			var data = Factories.WorkRoleManager.GetByRowId( id, true );
			return JsonResponse( data, data != null );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Functional Area data." );
			var data = Factories.WorkRoleManager.GetById( id );
			var converted = RDFServices.GetRDF( data );
			return RawJSONResponse( converted );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Work Role Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Functional Area data." );
			if ( !AccountServices.IsUserSiteStaff() )
			{
				RedirectToAction( "NotAuthenticated", "Event" );
			}

			var data = Factories.WorkRoleManager.GetById( id ) ?? new WorkRole();
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

			var errors = new List<string>();
			Factories.WorkRoleManager.SaveFromEditor( data, AccountServices.GetCurrentUser().Id, errors );
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