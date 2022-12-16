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
	[SessionState( System.Web.SessionState.SessionStateBehavior.ReadOnly )] //Allows for the polling to happen at the same time as the saving, instead of blocking the thread
	public class UploadController : BaseController
    {
		// GET: Upload
		[CustomAttributes.NavyAuthorize( "Upload", Roles = Admin_SiteManager_RMTLDeveloper )]
		public ActionResult Index()
        {
			AuthenticateOrRedirect( "You must be authenticated and authorized to use this feature." );
			return View( "~/views/upload/uploadv4.cshtml" );
        }
		//

		[HttpGet, Route("uploadv3")]
		public ActionResult IndexV3()
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to use this feature." );
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
				var currentRating = Factories.RatingManager.GetByRowId( ratingRowID );
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

		[CustomAttributes.NavyAuthorize( "Upload", Roles = Admin_SiteManager_RMTLDeveloper )]
		public ActionResult ConfirmChangesV3( Guid transactionGUID )
		{
			//Authenticate
			AuthenticateOrRedirect( "You must be authenticated and authorized to use this feature." );

			//Get the summary
			var summary = BulkUploadServices.GetCachedChangeSummary( transactionGUID );
			if( summary == null )
			{
				return JsonResponse( null, false, new List<string>() { "Unable to find cached change summary. Please upload the data again." } );
			}

			//If saving the data was canceled previously, don't allow trying to save it again, in order to prevent damaging the data any further
			if( !summary.ContinueSavingData )
			{
				return JsonResponse( null, false, new List<string>() { "Saving data for this upload was previously canceled. In order to minimize errors, you must start over." } );
			}

			//Otherwise, process the summary
			try
            {
				Services.BulkUploadServices.ApplyChangeSummaryV2( summary );
			} 
			catch ( Exception ex )
            {
				LoggingHelper.LogError( ex, "UploadController.ConfirmChangesV3" );
            }

			//Status isn't used, so read messages from summary instead
			//Don't need to send back the entire summary
			var confirmation = new JObject()
			{
				{ "Messages", JObject.FromObject( summary.Messages ) }
			};

			return JsonResponse( confirmation, true );
		}
		//

		public ActionResult GetSavingStatus( Guid transactionGUID, bool continueSavingData = true )
		{
			//Get the summary
			var summary = BulkUploadServices.GetCachedChangeSummary( transactionGUID );
			if ( summary == null )
			{
				return JsonResponse( null, false, new List<string>() { "Unable to find cached change summary. Please upload the data again." } );
			}

			//Flag to stop processing, if the cancel button was clicked
			summary.ContinueSavingData = continueSavingData;

			//Setup the response
			var response = new UploadV4SavePollingResponse()
			{
				TotalItems = summary.TotalItemsToSaveForClientMonitoring,
				TotalProcessed = summary.TotalItemsSavedForClientMonitoring,
				Messages = new List<UploadV4SaveMessage>()
			};

			//Parse out the messages that have been added since the last check
			ExtractSaveMessages( "Error", summary.Messages.Error, response.Messages );
			ExtractSaveMessages( "Warning", summary.Messages.Warning, response.Messages );
			ExtractSaveMessages( "Duplicate", summary.Messages.Duplicate, response.Messages );
			ExtractSaveMessages( "Miscellaneous", summary.Messages.Note, response.Messages );

			//Return the total counts and messages
			return JsonResponse( response, true );
		}
		private static void ExtractSaveMessages( string type, List<string> messagesSource, List<UploadV4SaveMessage> messagesDestination )
		{
			//Carefully extract the messages so as not to miss any due to multiple threads accessing the summary at the same time
			while( messagesSource.Count() > 0 )
			{
				messagesDestination.Add( new UploadV4SaveMessage() { Type = type, Message = messagesSource.FirstOrDefault() } );
				messagesSource.RemoveAt( 0 );
			}
		}
		public class UploadV4SavePollingResponse
		{
			public UploadV4SavePollingResponse()
			{
				Messages = new List<UploadV4SaveMessage>();
			}

			public int TotalItems { get; set; }
			public int TotalProcessed { get; set; }
			public List<UploadV4SaveMessage> Messages { get; set; }
		}
		public class UploadV4SaveMessage
		{
			public string Type { get; set; }
			public string Message { get; set; }
		}
		//
	}
}