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
    public class RatingController : BaseController
	{
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "Rating" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var results = SearchServices.RatingSearch( query );

			return JsonResponse( results, true );
		}
		//

		[CustomAttributes.NavyAuthorize( "Rating Detail", Roles = SiteReader )]
		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Rating data." );
			var data = Factories.RatingManager.Get( id );
			return View( data );
		}
		//

		[CustomAttributes.NavyAuthorize( "Rating View", Roles = SiteReader )]
		[Route("Rating/GetByRowID/{rowID}")]
		public ActionResult GetByRowID( Guid rowID )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Rating Data." );
			var data = Factories.RatingManager.Get( rowID );
			return JsonResponse( data, data != null );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Rating data." );
			var data = Factories.RatingManager.Get( id );
			var converted = RDFServices.GetRDF( data );
			return RawJSONResponse( converted );
		}
		//
		[CustomAttributes.NavyAuthorize( "Rating Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Rating data." );
			var data = Factories.RatingManager.Get( id ) ?? new Rating();
			return View( data );
		}
		//

		public ActionResult Save( Rating data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Rating data." }, null );
			}
			var user = AccountServices.GetCurrentUser();
			ChangeSummary status = new ChangeSummary()
			{
				Action = "Edit"
			};
			data.LastUpdatedById = user.Id;
			var results = new Factories.RatingManager().Save( data, ref status );
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