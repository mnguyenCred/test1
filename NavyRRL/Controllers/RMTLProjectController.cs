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
			var data = Factories.RMTLProjectManager.Get( id ) ?? new RMTLProject();
			return View( data );
		}
		//

		[CustomAttributes.NavyAuthorize( "Reference Resource Edit", Roles = Admin_SiteManager_RMTLDeveloper )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit RMTL Project data." );
			var data = Factories.RMTLProjectManager.Get( id ) ?? new RMTLProject();
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
			var user = AccountServices.GetCurrentUser();
			ChangeSummary status = new ChangeSummary()
			{
				Action = "Edit"
			};
			data.LastUpdatedById = user.Id;
			var results = new Factories.RMTLProjectManager().Save( data, ref status );
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