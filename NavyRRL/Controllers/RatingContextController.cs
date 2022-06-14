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

		[CustomAttributes.NavyAuthorize( "Rating Context View", Roles = SiteReader )]
		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Rating Context data." );
			var data = Factories.RatingContextManager.Get( id );

			return View( data );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Rating Context data." );
			var data = Factories.RatingContextManager.Get( id );
			var converted = RDFServices.GetRDF( data );

			return RawJSONResponse( converted );
		}
		//

		[CustomAttributes.NavyAuthorize( "Rating Context Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Rating Context data." );
			if ( !AccountServices.IsUserSiteStaff() )
			{
				RedirectToAction( "NotAuthenticated", "Event" );
			}

			var data = new RatingContext(); //Should get by ID or default to new (to enable new rating contexts to be created)
			if ( id > 0 )
			{
				data = Factories.RatingContextManager.Get( id );
			}

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

			var user = AccountServices.GetCurrentUser();
			ChangeSummary status = new ChangeSummary()
			{
				Action = "Edit"
			};
			data.LastUpdatedById = user.Id;
			var results = new Factories.RatingContextManager().Save( data, ref status );
			if ( status.HasAnyErrors )
			{
				var msg = string.Join( "</br>", status.Messages.Error.ToArray() );
				ConsoleMessageHelper.SetConsoleErrorMessage( "Saved changes successfully." );
			}
			else
			{
				//On success
				ConsoleMessageHelper.SetConsoleSuccessMessage( "Saved changes successfully." );
			}

			return JsonResponse( data, true, null, null );
		}
		//

	}
}