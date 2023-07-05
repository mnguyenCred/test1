﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Models.Search;
using Models.Schema;
using Services;
using Navy.Utilities;
using Models.Curation;
using Models.DTO;

namespace NavyRRL.Controllers
{
	[SessionState( System.Web.SessionState.SessionStateBehavior.ReadOnly )]
	public class BilletTitleController : BaseController
    {
		/// <summary>
		/// Identify the function code for authorization checks.
		/// Note additional checks will be necessary later to ensure the user can update the resource based on the RMTL rating to which they have access. 
		/// Update: This will be done in a separate controller - more to come later
		/// This note can be removed
		/// </summary>
        //public static string FunctionCode = "rmtl.update";

        public ActionResult Search()
		{
			return RedirectToAction( "Index", "Search", new { searchType = "BilletTitle" } );
		}
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Billet Title data." );
			var results = SearchServices.BilletTitleSearch( query );

			return JsonResponse( results, true );
		}
		//

		public ActionResult Detail( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Billet Title data." );
			var data = Factories.JobManager.GetById( id );
			//but may want to pass state so that an edit/add option is not presented
			return View( data );
		}
		//

		public ActionResult GetByRowId( Guid id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Billet Title data." );
			var data = Factories.JobManager.GetByRowId( id, true );
			return JsonResponse( data, data != null );
		}
		//

		public ActionResult JSON( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view Billet Title data." );
			var data = Factories.JobManager.GetById( id );
			var converted = RDFServices.GetRDF( data, null );
			return RawJSONResponse( converted );
		}
		//

		public ActionResult Edit( int id )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to edit Billet Title data." );
			if ( !AccountServices.IsUserSiteStaff() )
            {
				RedirectToAction( "NotAuthorized", "Event" );
			}

			var data = Factories.JobManager.GetById( id );
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

			var errors = new List<string>();
			Factories.JobManager.SaveFromEditor( data, AccountServices.GetCurrentUser().Id, errors );
			return JsonResponse( data, errors.Count() == 0, errors );
		}
		//

		public ActionResult Delete( int id )
		{
			if ( !AccountServices.IsUserAnAdmin() )
			{
				return JsonResponse( null, false, new List<string>() { "Deleting data requires administrator privileges." } );
			}

			var result = Factories.JobManager.DeleteById( id );
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

			var result = Factories.JobManager.GetMergeSummary( id );
			return JsonResponse( result, result.Valid, result.Messages );
		}
		//

		public ActionResult DoMerge( MergeAttempt attempt )
		{
			if ( !AccountServices.IsUserAnAdmin() )
			{
				return JsonResponse( null, false, new List<string>() { "Merging data requires administrator privileges." } );
			}

			Factories.JobManager.DoMerge( attempt );
			return JsonResponse( attempt, attempt.Valid, attempt.Messages );
		}
		//
	}
}