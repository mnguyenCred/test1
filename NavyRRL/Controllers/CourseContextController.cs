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
	public class CourseContextController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "CourseContext" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Course Context data." );
			var results = SearchServices.CourseContextSearch( query );

			return JsonResponse( results, true );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Course Context View", Roles = SiteReader )]
		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Course Context data." );
			var data = Factories.CourseContextManager.GetById( id );

			return View( data );
		}
		//

		public ActionResult GetByRowId( Guid id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Course Context data." );
			var data = Factories.CourseContextManager.GetByRowId( id, true );
			return JsonResponse( data, data != null );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Course Context data." );
			var data = Factories.CourseContextManager.GetById( id );
			var converted = RDFServices.GetRDF( data );
			return RawJSONResponse( converted );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Course Context Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Course Context data." );
			if ( !AccountServices.IsUserSiteStaff() )
			{
				RedirectToAction( "NotAuthenticated", "Event" );
			}

			var data = Factories.CourseContextManager.GetById( id ) ?? new CourseContext();
			return View( data );
		}
		//

		public ActionResult Save( CourseContext data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Course Context data." }, null );
			}

			var errors = new List<string>();
			Factories.CourseContextManager.SaveFromEditor( data, AccountServices.GetCurrentUser().Id, errors );
			return JsonResponse( data, errors.Count() == 0, errors );
		}
		//

		public ActionResult Delete( int id )
		{
			if ( !AccountServices.IsUserAnAdmin() )
			{
				return JsonResponse( null, false, new List<string>() { "Deleting data requires administrator privileges." } );
			}

			var result = Factories.CourseContextManager.DeleteById( id );
			return JsonResponse( result, result.Successful, result.Messages );
		}
		//
	}
}