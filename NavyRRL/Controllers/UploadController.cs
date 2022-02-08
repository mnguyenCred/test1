using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json.Linq;

using AM = Models.Application;
using SM = Models.Schema;
using CM = Models.Curation;
using Models.Application;
using Navy.Utilities;
using Models.Curation;
using Services;
namespace NavyRRL.Controllers
{
    public class UploadController : BaseController
    {
		// GET: Upload
		[CustomAttributes.NavyAuthorize( "Search", Roles = "Administrator, RMTL Developer, Site Staff" )]

		public ActionResult Index()
        {
			if ( !AccountServices.IsUserAuthenticated() )
			{
				AM.SiteMessage siteMessage = new AM.SiteMessage()
				{
					Title = "Invalid Request",
					Message = "You must be authenticated and authorized to use this feature"
				};
				ConsoleMessageHelper.SetConsoleErrorMessage( "You must be logged in and authorized to perform this action." );
				return RedirectToAction( AccountServices.EVENT_AUTHENTICATED, "event" );

			} else
            {

            }
			return View( "~/views/upload/uploadv2.cshtml" );
        }
		//

		[CustomAttributes.NavyAuthorize( "Search", Roles = "Administrator, RMTL Developer, Site Staff" )]

		public ActionResult UploadV2()
		{
			return View( "~/views/upload/uploadv2.cshtml" );
		}
		//
		public ActionResult UploadV1()
		{
			return View( "~/views/upload/uploadv1.cshtml" );
		}
		//
		//Initial processing of the data before any changes are made to the database
		public ActionResult ProcessUpload( CM.UploadableTable rawData, Guid ratingRowID )
		{
			var jsonResult = Json( rawData, JsonRequestBehavior.AllowGet );
			jsonResult.MaxJsonLength = int.MaxValue;

			//Construct Change Summary
			var debug = new JObject();
			var changeSummary = Services.BulkUploadServices.ProcessUploadV2( rawData, ratingRowID, debug );

			//Store Change Summary in the Application Cache
			Services.BulkUploadServices.CacheChangeSummary( changeSummary );

			/*
			var changeSummaryNew = Services.BulkUploadServices.ProcessUploadV2( rawData, ratingRowID, debug );
			var changeSummaryOld = new ChangeSummary();
			//Temp
			if ( UtilityManager.GetAppKeyValue( "includingProcessV1ToCompare", false ) )
			{
				changeSummaryOld = Services.BulkUploadServices.ProcessUpload( rawData, ratingRowID, debug );
			}
			//End Temp


			if ( UtilityManager.GetAppKeyValue( "usingProcessV2", true )) 
			{
				Services.BulkUploadServices.CacheChangeSummary( changeSummaryNew );
			} else
            {
				if ( UtilityManager.GetAppKeyValue( "includingProcessV1ToCompare", false ) )
				{
                    changeSummaryOld.Messages.Note.Add( "USING OLD PROCESS VERSION" );
                    Services.BulkUploadServices.CacheChangeSummary( changeSummaryOld );
                }


			}
			*/

			return JsonResponse( changeSummary, true, null, new { Debug = debug } );
        }
		//

		public ActionResult ConfirmChanges( Guid changeSummaryRowID )
		{
			var summary = Services.BulkUploadServices.GetCachedChangeSummary( changeSummaryRowID );
			if( summary != null )
			{
				//Update the database
				var debug = new JObject();
				SaveStatus status = new SaveStatus();
				Services.BulkUploadServices.ApplyChangeSummary( summary, ref status );
				//check for messages


				//For now, return the summary object (for testing purposes)
				return JsonResponse( summary, true, null, debug );
			}
			else
			{
				return JsonResponse( null, false, new List<string>() { "Unable to find the cached summary of changes. This is usually caused by too much time passing between the initial processing and the confirmation of changes. Please re-process the spreadsheet and try again." }, null );
			}
		}
		//

    }
}