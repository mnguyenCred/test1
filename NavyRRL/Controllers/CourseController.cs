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

		[CustomAttributes.NavyAuthorize( "Course View", Roles = SiteReader )]
		public ActionResult Detail( string id ) //String to accept CIN or ID
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Course data." );
			Course data;
			try
			{
				data = Factories.CourseManager.Get( int.Parse( id ), false );
			}
			catch
			{
				data = Factories.CourseManager.GetByCodedNotation( id, false );
			}
			return View( data );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Course data." );
			var data = Factories.CourseManager.Get( id, true );
			var converted = RDFServices.GetRDF( data );
			return RawJSONResponse( converted );
		}
		//

		[CustomAttributes.NavyAuthorize( "Course Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Course data." );
			var data = Factories.CourseManager.Get( id, true ) ?? new Course();
			return View( data );
		}
		//

		public ActionResult Save( CourseDTO data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Course data." }, null );
			}

			var user = AccountServices.GetCurrentUser();
			ChangeSummary status = new ChangeSummary()
			{
				Action = "Edit"
			};
			data.LastUpdatedById = user.Id;
			var results = new Factories.CourseManager().Save( data, ref status );
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