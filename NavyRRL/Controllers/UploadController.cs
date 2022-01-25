﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json.Linq;

using SM = Models.Schema;
using CM = Models.Curation;
using Models.Application;
using Navy.Utilities;
namespace NavyRRL.Controllers
{
    public class UploadController : BaseController
    {
        // GET: Upload
        public ActionResult Index()
        {
			return View( "~/views/upload/uploadv1.cshtml" );
        }
		//

		public ActionResult UploadV2()
		{
			return View( "~/views/upload/uploadv2.cshtml" );
		}
		//

		//Initial processing of the data before any changes are made to the database
		public ActionResult ProcessUpload( CM.UploadableTable rawData, Guid ratingRowID )
		{
			//Construct Change Summary
			var debug = new JObject();
			var changeSummary = Services.BulkUploadServices.ProcessUploadV2( rawData, ratingRowID, debug );

			//Store Change Summary in the Application Cache
			Services.BulkUploadServices.CacheChangeSummary( changeSummary );

			/*
			//Temp
			var changeSummaryOld = Services.BulkUploadServices.ProcessUpload( rawData, ratingRowID, debug );
			//End Temp


			if ( UtilityManager.GetAppKeyValue( "usingProcessV2", true )) 
			{
				Services.BulkUploadServices.CacheChangeSummary( changeSummaryNew );
			} else
            {
				changeSummaryOld.Messages.Note.Add( "USING OLD PROCESS VERSION" );
				Services.BulkUploadServices.CacheChangeSummary( changeSummaryOld );

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