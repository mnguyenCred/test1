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
using Models.Curation;

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


			var changeSummaryNew = Services.BulkUploadServices.ProcessUploadV2( rawData, ratingRowID, debug );
			var changeSummaryOld = new ChangeSummary();
			//Temp
			if ( UtilityManager.GetAppKeyValue( "includingProcessV1ToCompare", false ) )
			{
				changeSummaryOld = Services.BulkUploadServices.ProcessUpload( rawData, ratingRowID, debug );
			}
			//End Temp


			//Store Change Summary in the Application Cache
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

			var temp = new List<object>();
			temp.Add( new SM.RatingTask() { RowId = Guid.NewGuid(), Description = "Test" } );
			temp.Add( new SM.Concept() { RowId = Guid.NewGuid(), Name = "test 2" } );
			return JsonResponse( changeSummaryNew, true, null, new { Debug = debug, ChangeSummaryOld = changeSummaryOld, Lookup = temp } );
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