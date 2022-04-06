using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using AM = Models.Application;
using SM = Models.Schema;
using CM = Models.Curation;
using Models.Application;
using Navy.Utilities;
using Models.Curation;
using Services;
using System.IO;

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

		public ActionResult UploadV3()
		{
			return View( "~/views/upload/uploadv3.cshtml" );
		}
		//

		public ActionResult ProcessUploadedItem( UploadableItem item )
		{
			//Process the current row
			var result = BulkUploadServices.ProcessUploadedItem( item );

			//Return the response
			return JsonResponse( result, true );
		}
		//

		public ActionResult StoreRawCSV( UploadableItem item )
		{
			//Get the summary for this transaction
			var summary = BulkUploadServices.GetCachedChangeSummary( item.TransactionGUID );

			//Do something with the raw CSV
			//item.RawCSV...
			if ( item.RawCSV?.Length > 0 )
			{
				var currentRating = Factories.RatingManager.Get( item.RatingRowID );
				if ( currentRating == null )
				{
					summary.Messages.Error.Add( "Error: Could save input file, as was unable to find Rating for identifier: " + item.RatingRowID );
					//return summary;
				}
				else
				{
					LoggingHelper.WriteLogFile( 1, string.Format( "Rating_upload_{0}_{1}.csv", currentRating.Name.Replace( " ", "_" ), DateTime.Now.ToString( "hhmmss" ) ), item.RawCSV, "", false );

					new Factories.BaseFactory().BulkLoadRMTL( currentRating.CodedNotation, item.RawCSV );
				}
			}
			//Process the summary

			//Return the response
			return JsonResponse( null, true );
		}
		//

		public ActionResult LookupGraphItem( Guid transactionGUID, Guid itemRowID )
		{
			//Find the item (or null)
			var summary = BulkUploadServices.GetCachedChangeSummary( transactionGUID );
			var item = summary?.LookupItem<SM.BaseObject>( itemRowID );

			var itemWithType = BulkUploadServices.JObjectify( item );

			//Return it
			return JsonResponse( itemWithType, true );
		}
		//

		//Initial processing of the data before any changes are made to the database
		//Expects the following arguments, but since we have to manually read them out of the request, we need to bypass MVC's default model binding
		//CM.UploadableTable rawData, Guid ratingRowID
		public ActionResult ProcessUpload()
		{
			//Get the raw request JSON
			Request.InputStream.Position = 0;
			var rawJSON = new StreamReader( Request.InputStream ).ReadToEnd();

			//Read it into a JToken
			//Have to do it this way to keep Newtonsoft from messing with the dates for some stupid reason
			JToken token;
			using ( var reader = new JsonTextReader( new StringReader( rawJSON ) ) { DateParseHandling = DateParseHandling.None } )
			{
				token = JToken.Load( reader );
			}

			//Extract the data from the request
			var rawData = token[ "rawData" ].ToObject<CM.UploadableTable>();
			var ratingRowID = token[ "ratingRowID" ].ToObject<Guid>();

			//Construct Change Summary
			var debug = new JObject();
			var changeSummary = Services.BulkUploadServices.ProcessUploadV2( rawData, ratingRowID, debug );

			//Store Change Summary in the Application Cache
			Services.BulkUploadServices.CacheChangeSummary( changeSummary );

			//Return the result
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