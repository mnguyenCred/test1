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
		[CustomAttributes.NavyAuthorize( "Upload", Roles = "Administrator, RMTL Developer, Site Manager, Site Staff, Site Reader" )]
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

			}

			return View( "~/views/upload/uploadv3.cshtml" );
        }
		//

		public ActionResult ProcessUploadedItem( UploadableItem item )
		{
			//Process the current row
			var result = BulkUploadServices.ProcessUploadedItemV4( item );

			//Return the response
			return JsonResponse( result, true );
		}
		//

		public ActionResult StoreRawCSV()
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
			var transactionGUID = token[ "TransactionGUID" ].ToObject<Guid>();
			var ratingRowID = token[ "RatingRowID" ].ToObject<Guid>();
			var rawCSV = token[ "RawCSV" ].ToString();

			//Get the summary for this transaction
			var summary = BulkUploadServices.GetCachedChangeSummary( transactionGUID );

			//Temp for testing - remove this
			var finalNewItems = summary.ItemsToBeCreated;
			var finalChangedItems = summary.FinalizedChanges;
			var debuggerHere = "";
			//End Temp for testing - remove this

			
			//Do something with the raw CSV
			if ( rawCSV?.Length > 0 )
			{
				var currentRating = Factories.RatingManager.Get( ratingRowID );
				if ( currentRating?.Id == 0 )
				{
					summary.Messages.Error.Add( "Error: Could not save input file: Unable to find Rating for identifier: " + ratingRowID );
					//return summary;
				}
				else
				{
					//temp means  to log end of upload
					AppUser user = AccountServices.GetCurrentUser();
					if ( user?.Id == 0 )
					{
						//result.Errors.Add( "Error - a current user was not found. You must authenticated and authorized to use this function!" );
						//return result;
					}

					summary.UploadFinished = DateTime.Now; //Compare with summary.UploadStarted to determine how long it took
					var saveDuration = summary.UploadFinished.Subtract( summary.UploadStarted );
					summary.Messages.Note.Add( string.Format( "Upload Duration: {0:N2} seconds ", saveDuration.TotalSeconds ) );

					SiteActivity sa = new SiteActivity()
					{
						ActivityType = "RMTL",
						Activity = "Upload",
						Event = "Complete",
						Comment = String.Format( "The RMTL upload step is completed for Rating: '{0}' by '{1}'.", currentRating.Name, user.FullName() ),
						ActionByUserId = user.Id,
						ActionByUser = user.FullName()
					};
					new ActivityServices().AddActivity( sa );

					LoggingHelper.WriteLogFile( 1, string.Format( "Rating_upload_{0}_{1}.csv", currentRating.Name.Replace( " ", "_" ), DateTime.Now.ToString( "hhmmss" ) ), rawCSV, "", false );

					if ( rawCSV.Length > 300000 )
					{
						new Factories.BaseFactory().BulkLoadRMTL( currentRating.CodedNotation, rawCSV );
					}
				}
			}

			//Return the response
			return JsonResponse( null, true );
		}
		//

		public ActionResult LookupGraphItem( Guid transactionGUID, Guid itemRowID )
		{
			//Find the item (or null)
			var summary = BulkUploadServices.GetCachedChangeSummary( transactionGUID );
			var item = summary?.LookupItem<SM.BaseObject>( itemRowID );

			//Handle the rest
			if( item != null && item.Id > 0 )
			{
				//Append the @type and return the data
				var itemWithType = BulkUploadServices.JObjectify( item );
				return JsonResponse( itemWithType, true );
			}
			else
			{
				//Return error
				return JsonResponse( null, false, new List<string>() { "Unable to find data for GUID: " + itemRowID.ToString() } );
			}
		}
		//

		public ActionResult ConfirmChangesV3( Guid transactionGUID )
		{
			//Reject if user does not have permission to save data


			//Get the summary
			var summary = BulkUploadServices.GetCachedChangeSummary( transactionGUID );
			if(summary == null )
			{
				return JsonResponse( null, false, new List<string>() { "Unable to find cached change summary. Please upload the data again." } );
			}

			try
            {
				//Process the summary
				Services.BulkUploadServices.ApplyChangeSummary( summary );

			} catch (Exception ex)
            {
				LoggingHelper.LogError( ex, "UploadController.ConfirmChangesV3" );
            }
			//Status isn't used, so read messages from summary instead
			//Don't need to send back the entire summary

			//Temp - simulate processing
			//System.Threading.Thread.Sleep( 5000 );
			//summary.Messages.Error.Add( "Test Error 1" );
			//summary.Messages.Error.Add( "Test Error 2" );
			//summary.Messages.Create.Add( "Test Create" );
			//summary.Messages.Warning.Add( "Test Warning" );
			
			var confirmation = new JObject()
			{
				{ "Messages", JObject.FromObject( summary.Messages ) }
			};
			return JsonResponse( confirmation, true );
		}
		//

    }
}