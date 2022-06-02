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
    public class RatingTaskController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "RatingTask" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var results = SearchServices.RatingTaskSearch( query );

			return JsonResponse( results, true );
		}
		//
		[CustomAttributes.NavyAuthorize( "Rating Task View", Roles = SiteReader )]
		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Rating Task data." );
			var data = Factories.RatingTaskManager.Get( id, true );
			return View( data );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Rating Task data." );
			var data = Factories.RatingTaskManager.Get( id, true );
			var converted = RDFServices.GetRDF( data );
			return RawJSONResponse( converted );
		}
		//

		[CustomAttributes.NavyAuthorize( "Rating Task Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Rating Task data." );
			var data = Factories.RatingTaskManager.Get( id, false ) ?? new RatingTask();
			return View( data );
		}
		//

		public ActionResult Save( RatingTaskDTO data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Rating Task data." }, null );
			}

			var user = AccountServices.GetCurrentUser();
			ChangeSummary status = new ChangeSummary()
			{
				Action = "Edit"
			};
			data.LastUpdatedById = user.Id;
			//this type of save will not have a rating context!
			var results = new Factories.RatingTaskManager().Save( data, ref status, false );
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