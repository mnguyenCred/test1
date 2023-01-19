using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models.Curation;

using Models.Search;
using Models.Schema;
using Navy.Utilities;
using Services;

namespace NavyRRL.Controllers
{
	[SessionState( System.Web.SessionState.SessionStateBehavior.ReadOnly )]
	[CustomAttributes.NavyAuthorize( "Edit", Roles = "Administrator, RMTL Developer, Site Staff" )]
	public class RMTLProjectController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "RMTLProject" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var results = SearchServices.RMTLProjectSearch( query );

			return JsonResponse( results, true );
		}
		//

		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view RMTL Project data." );
			//var data = new RMTLProject() { Name = "Temp Name", Description = "Temp Description" };
			var data = Factories.RMTLProjectManager.GetById( id ) ?? new RMTLProject();
			return View( data );
		}
		//

		[CustomAttributes.NavyAuthorize( "RMTL Project View", Roles = SiteReader )]
		public ActionResult GetByRowId( Guid id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit RMTL Project data." );
			var data = Factories.RMTLProjectManager.GetByRowId( id, true );
			return JsonResponse( data, data != null );
		}
		//

		[CustomAttributes.NavyAuthorize( "RMTL Project Edit", Roles = Admin_SiteManager_RMTLDeveloper )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit RMTL Project data." );
			if ( !AccountServices.IsUserSiteStaff() )
			{
				RedirectToAction( "NotAuthenticated", "Event" );
			}

			var data = Factories.RMTLProjectManager.GetById( id ) ?? new RMTLProject();
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

			var errors = new List<string>();
			Factories.RMTLProjectManager.SaveFromEditor( data, AccountServices.GetCurrentUser().Id, errors );
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