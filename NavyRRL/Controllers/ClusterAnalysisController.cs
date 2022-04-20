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
    public class ClusterAnalysisController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "ClusterAnalysis" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var results = SearchServices.ClusterAnalysisSearch( query );

			return JsonResponse( results, true );
		}
		//

		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Cluster Analysis data." );
			var data = Factories.ClusterAnalysisManager.Get( id );
			return View( data );
		}
		//

		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Cluster Analysis data." );
			var data = Factories.ClusterAnalysisManager.Get( id ) ?? new ClusterAnalysis();
			return View( data );
		}
		//

		public ActionResult Save( ClusterAnalysis data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Cluster Analysis data." }, null );
			}

			var user = AccountServices.GetCurrentUser();
			ChangeSummary status = new ChangeSummary()
			{
				Action = "Edit"
			};
			data.LastUpdatedById = user.Id;
			//this type of save will not have a rating context!
			var results = new Factories.ClusterAnalysisManager().Save( data, ref status );
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