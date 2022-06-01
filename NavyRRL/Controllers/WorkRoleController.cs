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
    public class WorkRoleController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "WorkRole" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var results = SearchServices.WorkRoleSearch( query );

			return JsonResponse( results, true );
		}
		public SearchResultSet<T> ConvertResults<T>( SearchQuery query, List<WorkRole> results ) where T : WorkRole, new()
		{
			var gResults = new List<T>();

			var output = new SearchResultSet<T>();
			if ( results?.Count == 0 )
				return output;
			foreach ( var item in results )
			{
				var gResult = new T();
				gResult.Id = item.Id;
				gResult.Name = item.Name;

				gResults.Add( gResult );
			}
			output.TotalResults = query.TotalResults;
			output.Results = gResults;
			output.SearchType = query.SearchType;

			return output;
		}
		//
		//
		[CustomAttributes.NavyAuthorize( "Work Role View", Roles = SiteReader )]
		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Functional Area data." );
			var data = Factories.WorkRoleManager.Get( id );
			return View( data );
		}
		//
		[CustomAttributes.NavyAuthorize( "Work Role Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Functional Area data." );
			var data = Factories.WorkRoleManager.Get( id ) ?? new WorkRole();
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
			var user = AccountServices.GetCurrentUser();
			ChangeSummary status = new ChangeSummary()
			{
				Action = "Edit"
			};
			data.LastUpdatedById = user.Id;
			var results = new Factories.WorkRoleManager().Save( data, ref status );
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