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
    public class BilletTitleController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "BilletTitle" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			//var tempResults = SearchController.CreateFakeResults<BilletTitle>( query.PageNumber, query.PageSize );
			//List<BilletTitle> results = Factories.JobManager.Search( query );

			//var output = ConvertResults<BilletTitle>( query, results );

			//return JsonResponse( output, true );
			bool valid = true;
			string status = "";
			var results = new SearchServices().BilletTitleSearch( query, ref valid, ref status );

			return JsonResponse( results, valid, new List<string>() { status }, null );
		}
		public SearchResultSet<T> ConvertResults<T>( SearchQuery query, List<BilletTitle> results ) where T : BilletTitle, new()
		{
			var gResults = new List<T>();

			var output = new SearchResultSet<T>();
			if ( results?.Count == 0 )
				return output;
			foreach (var item in results)
            {
				var gResult = new T();
				gResult.Id = item.Id;
				gResult.Name = item.Name;
				gResult.Description = item.Description;

				gResults.Add( gResult );
			}
			output.TotalResults = query.TotalResults;
			output.Results = gResults;
			output.SearchType = query.SearchType;

			return output;
		}
		//

		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Billet Title data." );
			var data = new BilletTitle() { Name = "Temp name" }; //Not sure how to get a billet title
			return View( data );
		}
		//

		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Billet Title data." );
			var data = new BilletTitle(); //Should get by ID or default to new (to enable new billet titles to be created)
			return View( data );
		}
		//

		public ActionResult Save( BilletTitle data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Billet Title data." }, null );
			}

			var user = AccountServices.GetCurrentUser();
			ChangeSummary status = new ChangeSummary()
			{
				Action = "Edit"
			};
			data.LastUpdatedById = user.Id;
			var results = new Factories.JobManager().Save( data, ref status );
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