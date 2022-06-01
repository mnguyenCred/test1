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
    public class OrganizationController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "Organization" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			var results = SearchServices.OrganizationSearch( query );

			return JsonResponse( results, true );
		}
		//
		[CustomAttributes.NavyAuthorize( "Organization View", Roles = SiteReader )]
		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Organization data." );
			var data = Factories.OrganizationManager.Get( id );
			return View( data );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Organization data." );
			var data = Factories.OrganizationManager.Get( id );
			var converted = RDFServices.GetRDF( data );
			return RawJSONResponse( converted );
		}
		//

		[CustomAttributes.NavyAuthorize( "CCA Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Organization data." );
			var data = new Organization();
			if ( id > 0 )
			{
				data = Factories.OrganizationManager.Get( id );
			}
			return View( data );
		}
		//

		public ActionResult Save( Organization data )
		{
			//Validate the request
			if ( !AuthenticateOrFail() )
			{
				return JsonResponse( null, false, new List<string>() { "You must be authenticated and authorized to edit Organization data." }, null );
			}

			var user = AccountServices.GetCurrentUser();
			ChangeSummary status = new ChangeSummary()
			{
				Action = "Edit"
			};
			data.LastUpdatedById = user.Id;
			var results = new Factories.OrganizationManager().Save( data, user.Id, ref status );
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