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
	public class CourseController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "Course" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var results = SearchServices.CourseSearch( query );

			return JsonResponse( results, true );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Course View", Roles = SiteReader )]
		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Course data." );
			var data = Factories.CourseManager.GetById( id );

			return View( data );
		}
		//

		public ActionResult GetByRowId( Guid id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Course data." );
			var data = Factories.CourseManager.GetByRowId( id, true );
			return JsonResponse( data, data != null );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Course data." );
			var data = Factories.CourseManager.GetById( id );
			var converted = RDFServices.GetRDF( data );
			return RawJSONResponse( converted );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Course Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Course data." );
			if ( !AccountServices.IsUserSiteStaff() )
			{
				RedirectToAction( "NotAuthenticated", "Event" );
			}

			var data = Factories.CourseManager.GetById( id ) ?? new Course();
			return View( data );
		}
		//

		public ActionResult Save( Course data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Course data." }, null );
			}

			var errors = new List<string>();
			Factories.CourseManager.SaveFromEditor( data, AccountServices.GetCurrentUser().Id, errors );
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