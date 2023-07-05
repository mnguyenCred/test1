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
using Models.DTO;

namespace NavyRRL.Controllers
{
	[SessionState( System.Web.SessionState.SessionStateBehavior.ReadOnly )]
	public class OrganizationController : BaseController
    {
		public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "Organization" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Organization data." );
			var results = SearchServices.OrganizationSearch( query );

			return JsonResponse( results, true );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Organization View", Roles = SiteReader )]
		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Organization data." );
			var data = Factories.OrganizationManager.GetById( id );
			return View( data );
		}
		//

		//[CustomAttributes.NavyAuthorize( "Organization View", Roles = SiteReader )]
		public ActionResult GetByRowId( Guid id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Organization data." );
			var data = Factories.OrganizationManager.GetByRowId( id, true );
			return JsonResponse( data, data != null );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Organization data." );
			var data = Factories.OrganizationManager.GetById( id );
			var converted = RDFServices.GetRDF( data, null );
			return RawJSONResponse( converted );
		}
		//

		//[CustomAttributes.NavyAuthorize( "CCA Edit", Roles = Admin_SiteManager )]
		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Organization data." );
			if ( !AccountServices.IsUserSiteStaff() )
			{
				RedirectToAction( "NotAuthenticated", "Event" );
			}

			var data = Factories.OrganizationManager.GetById( id );
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

			var errors = new List<string>();
			Factories.OrganizationManager.SaveFromEditor( data, AccountServices.GetCurrentUser().Id, errors );
			return JsonResponse( data, errors.Count() == 0, errors );
		}
		//

		public ActionResult Delete( int id )
		{
			if ( !AccountServices.IsUserAnAdmin() )
			{
				return JsonResponse( null, false, new List<string>() { "Deleting data requires administrator privileges." } );
			}

			var result = Factories.OrganizationManager.DeleteById( id );
			return JsonResponse( result, result.Successful, result.Messages );
		}
		//

		public ActionResult Merge()
		{
			if ( !AccountServices.IsUserAnAdmin() )
			{
				return JsonResponse( null, false, new List<string>() { "Merging data requires administrator privileges." } );
			}

			return View();
		}
		//

		public ActionResult GetMergeSummary( Guid id )
		{
			if ( !AccountServices.IsUserAnAdmin() )
			{
				return JsonResponse( null, false, new List<string>() { "Merging data requires administrator privileges." } );
			}

			var result = Factories.OrganizationManager.GetMergeSummary( id );
			return JsonResponse( result, result.Valid, result.Messages );
		}
		//

		public ActionResult DoMerge( MergeAttempt attempt )
		{
			if ( !AccountServices.IsUserAnAdmin() )
			{
				return JsonResponse( null, false, new List<string>() { "Merging data requires administrator privileges." } );
			}

			Factories.OrganizationManager.DoMerge( attempt );
			return JsonResponse( attempt, attempt.Valid, attempt.Messages );
		}
		//
	}
}