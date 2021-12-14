using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Models;
using Models.Common;
using Models.Search;
using ThisEntity = Models.Common.ConceptScheme;
using MN = Models.Node;
using MH = Models.Helpers.Cass;
using Factories;
using Manager =  Factories.ConceptSchemeManager;
using Utilities;



namespace CTIServices
{
	public class ConceptSchemeServices
	{
		public static string thisClassName = "ConceptSchemeServices";
		public const string EntityType = "ConceptScheme";
		public int Add( ThisEntity framework, AppUser user, ref List<string> messages, bool fromCaSS = true )
		{
			var newFrameworkID = new ConceptSchemeManager().Add( framework, user.Id, ref messages, fromCaSS );
			if ( newFrameworkID > 0 )
			{
				new ActivityServices().AddActivity( new SiteActivity()
				{
					ActivityType = "ConceptScheme",
					Activity = "Editor",
					Event = "Add",
					Comment = string.Format( "{0} added Concept Scheme: {1}, Id: {2}", user.FullName(), framework.Name, newFrameworkID ),
					ActivityObjectId = newFrameworkID,
					ActionByUserId = user.Id,
					ActivityObjectParentEntityUid = framework.RowId
				} );
			}
			return newFrameworkID;
		}
		//
		/// <summary>
		/// Approve a concept scheme
		/// </summary>
		/// <param name="ctid"></param>
		/// <param name="user"></param>
		/// <param name="messages"></param>
		public static void Approve( string ctid, AppUser user, ref List<string> messages )
		{
			//Validate
			var conceptScheme = Manager.GetByCtid( ctid );
			if ( conceptScheme == null || conceptScheme.Id == 0 )
			{
				messages.Add( "Concept Scheme Not Found for CTID: " + ctid );
				return;
			}
			if ( !ValidateFrameworkAction( conceptScheme, user, ref messages ) )
			{
				return;
			}
			//unlike framework, not getting payload.
			//one issue was the potentially inconsistant URL that was used
			//string payload = GetPayload( ctid );
			//if ( payload.ToLower().IndexOf( "conceptScheme not found" ) > -1 )
			//{
			//	//messages.Add( "Concept Scheme has not been saved. You must add competencies to a conceptScheme before doing an approval. " + ctid );
			//	//return;
			//}
			//Save
			conceptScheme.LastApproved = DateTime.Now;
			conceptScheme.LastApprovedById = user.Id;
			if (!new Manager().Update( conceptScheme, ref messages, true, true ))
			{
				return;
			}

			//TODO - replace this with a direct save. Just need to confirm other if still used by other processes
			//**** main issue appears to be that the interface doesn't update the dates after approve
			//		** the interface is now updated, so skip this step
			bool isPublished = false;
			string status = "";
			//TBD - do we need an Entity for ConceptScheme - doesn't appear so, except for approvals?
			//	- actually need to send an email!
			//if ( new ProfileServices().Entity_Approval_Save( "ConceptScheme", conceptScheme.RowId, user, ref isPublished, ref status, true ) == false )
			//{
			//	messages.Add( status );
			//}

			new ActivityServices().AddActivity( new SiteActivity()
			{
				ActivityType = EntityType,
				Activity = "Editor",
				Event = "Approval",
				Comment = string.Format( "{0} Approved conceptScheme: {1}, Id: {2}, Name: {3}", user.FullName(), EntityType, conceptScheme.Id, conceptScheme.Name ),
				ActivityObjectId = conceptScheme.Id,
				ActionByUserId = user.Id,
				ActivityObjectParentEntityUid = conceptScheme.RowId
			} );
			EntitySummary es = new EntitySummary()
			{
				Name = conceptScheme.Name,
				EntityType = EntityType,
				BaseId = conceptScheme.Id,
				OwningOrgId = conceptScheme.OrgId
			};

			if ( conceptScheme.OrgId > 0 && conceptScheme.OwningOrganization != null )
				es.OwningOrganization = conceptScheme.OwningOrganization.Name;

			new ProfileServices().SendApprovalEmail( es, user, ref status );

			string lastPublishDate = ActivityManager.GetLastPublishDate( EntityType.ToLower(), conceptScheme.Id );
			if ( lastPublishDate.Length > 5 )
				isPublished = true;
		}

		public bool Delete( int recordId, AppUser user, ref bool valid, ref List<string> messages )
		{
			try
			{
				var entity = Manager.Get( recordId );
				if ( !string.IsNullOrWhiteSpace( entity.CredentialRegistryId ) )
				{
					//should be deleting from registry!
					new RegistryServices().HandleUnPublishEventCompetencyFramework( recordId, user, ref messages );
				}

				valid = new Manager().Delete( recordId, ref messages );
				//
				new ActivityServices().AddEditorActivity( "CompetencyFramework", "Delete", string.Format( "{0} deleted CompetencyFramework: {1} (id: {2})", user.FullName(), entity.Name, entity.Id ), user.Id, 0, recordId );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "CompetencyFrameworkServices.DeleteFramework" );
				messages.Add( ex.Message );
				valid = false;
			}

			return valid;
		}
		/// <summary>
		/// Publish a Concept Scheme - from CaSS, if the user is allowed to do so and the framework is approved
		/// </summary>
		/// <param name="ctid"></param>
		/// <param name="user"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Publish( string ctid, AppUser user, ref List<string> messages, string community = "" )
		{
			bool isValid = true;
			//Validate
			var framework = Manager.GetByCtid( ctid );
			if ( !ValidateFrameworkAction( framework, user, ref messages ) )
			{
				return false;
			}
			if ( !framework.IsThisApproved() )
			{
				messages.Add( "You cannot publish the Concept Scheme until it has been approved." );
				return false;
			}
			if ( framework.CTID != ctid )
			{
				messages.Add( "The Concept Scheme CTID doesn't match the version in the publisher." );
				return false;
			}
			//
			string status = "";
			string frameworkExportJSON = GetPayloadFromCaSS( framework, ref status );
			//validate payload
			if ( string.IsNullOrWhiteSpace( frameworkExportJSON ) )
			{
				messages.Add( "Error: a valid payload was not found for the provided CTID." );
				return false;
			}
			else if ( frameworkExportJSON.IndexOf( "@context" ) == -1 
				|| (
					frameworkExportJSON.IndexOf( "@graph" ) == -1
					&& frameworkExportJSON.IndexOf( "\"@type\":\"ConceptScheme\"") == -1) //temp
				)
			{
				messages.Add( "Error: the payload is not formatted properly. OR Object not found or you did not supply sufficient permissions to access the object." );
				return false;
			}

			//Do the publish using the JSON exported from CASS
			framework.Payload = frameworkExportJSON;
			//
			isValid = new RegistryServices().PublishConceptScheme( framework, user, ref messages, community, true );

			//Update
			if ( isValid )
			{
				framework.LastPublished = DateTime.Now;
				framework.LastPublishedById = user.Id;

				//Save
				isValid = new Manager().Update( framework, ref messages, true, true );
			}
			return isValid;
		}
		//

		//Publish a Framework, if the user is allowed to do so and the framework is approved
		public static bool PublishFromSummary( int recordID, AppUser user, ref string statusMessage, string community = "" )
		{
			LoggingHelper.DoTrace( 6, string.Format( thisClassName + ".PublishFromSummary. Enter." ) );
			bool isValid = true;
			List<string> messages = new List<string>();
			//get and include concepts - this can help flag a progression model vs a CaSS conceptS
			var entity = Manager.Get( recordID, true );
			if ( entity == null || entity.Id == 0 )
			{
				statusMessage = "Error the ConceptScheme was not found for ID: " + recordID.ToString();
				return false;
			}
			if ( !entity.IsThisApproved() )
			{
				statusMessage = "You cannot publish the ConceptScheme until it has been approved.";
				return false;
			}
			//TODO - need to distinguish a ProgressionModel in CaSS. 
			//If the latter, need to get payload from CaSS!
			if ( entity.IsProgressionModel )
			{
				isValid = new RegistryServices().PublishConceptScheme( entity, user, ref messages, community, false );
			} else
			{
				isValid = new ConceptSchemeServices().Publish( entity.CTID, user, ref messages, community );
			}
			if (messages.Any())
				statusMessage = string.Join( "<br/>", messages.ToArray() );


			return isValid;
		}
		//

		/// <summary>
		/// Get CaSS payload using the MD5 string route
		/// may need to handle CTID or URI(@id).
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static string GetPayloadFromCaSS(ThisEntity entity, ref string status)
		{
			List<string> messages = new List<string>();
			//confirm the same export URL can be used with concept schemes
			//IF true, then change to use the same method as competency frameworks
			string exportUrl = UtilityManager.GetAppKeyValue( "cassExportUrl", "" );
			//should end with a period
			//21-11-11 mp - was told no longer necessary, however seems like it should be sandbox.
			//				Should no longer be necessary with the use of credRegistryResourceUrl
			var cassResourceUrlPrefix = ServiceHelper.GetAppKeyValue( "cassResourceUrlPrefix" );
			string statusMessage = "";
			string cassURI = "";
			if( !string.IsNullOrWhiteSpace( entity.EditorUri )
				&& Manager.IsUrlValid( entity.EditorUri, ref statusMessage, false ) )
			{
				cassURI = entity.EditorUri;
			}
			else if( ServiceHelper.IsValidCtid( entity.CTID, ref messages ) )
			{
				//WARNING: in the dev environment, the CER type URI for CaSS frameworks don't always use sandbox domain!
				//also the dev env doesn't use ce-??? ==> s this still true?
				
				//21-11-11 mp - this should now use a standard format registry url
				var registryUrl = UtilityManager.GetAppKeyValue( "credRegistryResourceUrl", "" );
				//cassURI = string.Format( "https://{0}credentialengineregistry.org/resources/", cassResourceUrlPrefix ) + entity.CTID;
				cassURI = registryUrl + entity.CTID;
			}
			else
			{
				//error
				return "A concept scheme must have a valid URL from the CaSS editor.";
			}
			DateTime started = DateTime.Now;
			try
			{
				string resourceUrl = string.Format( exportUrl, UtilityManager.GenerateMD5String( cassURI ) );

				LoggingHelper.DoTrace( 6, string.Format( "ConceptSchemeServices.GetPayloadFromCaSS( ThisEntity entity ). Name: {0}, CTID: {1} ,resourceUrl: {2}", entity.Name, entity.CTID, resourceUrl ) );

				//dev env of CASS doesn't use the ce- so strip. The app key for prod will include the ce-
				//var resourceURI = string.Format( getURL, CTID.Replace( "ce-", "" ) );
				var responseData = "";
				using( var client = new HttpClient() )
				{
					client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
					//need to handle large documents
					client.Timeout = new TimeSpan( 0, 20, 0 );

					var response = client.GetAsync( resourceUrl ).Result;
					responseData = response.Content.ReadAsStringAsync().Result;
				}
				TimeSpan duration = DateTime.Now.Subtract( started );
				LoggingHelper.DoTrace( 6, string.Format( "ConceptSchemeServices.GetPayloadFromCaSS - completed. for: '{0}',  elapsed: {1:N2} seconds", entity.Name, duration.TotalSeconds ) );

				return responseData;
			}
			catch( Exception ex )
			{
				TimeSpan duration = DateTime.Now.Subtract( started );
				LoggingHelper.DoTrace( 6, string.Format( "ConceptSchemeServices.GetPayloadFromCaSS - FAILED. for: '{0}',  elapsed: {1:N2} seconds", entity.Name, duration.TotalSeconds ) );

				status = BaseFactory.FormatExceptions( ex );
				LoggingHelper.LogError( ex, "ConceptSchemeServices.GetPayloadFromCaSS()" );
				return "";
			}
		}
		/// <summary>
		//
		//[Obsolete]
		//public static void Publish(string ctid, string frameworkExportJSON, AppUser user, ref List<string> messages)
		//{
		//	//Validate
		//	var framework = Manager.GetByCtid( ctid );
		//	if( !ValidateFrameworkAction( framework, user, ref messages ) )
		//	{
		//		return;
		//	}
		//	if( !framework.IsThisApproved() )
		//	{
		//		messages.Add( "You cannot publish the Concept Scheme until it has been approved." );
		//		return;
		//	}
		//	if( framework.CTID != ctid )
		//	{
		//		messages.Add( "The Concept Scheme CTID doesn't match the version in the publisher." );
		//		return;
		//	}
		//	//validate payload
		//	if( string.IsNullOrWhiteSpace( frameworkExportJSON ) )
		//	{
		//		messages.Add( "Error: a valid payload was not provided" );
		//		return;

		//	}
		//	else if( frameworkExportJSON.IndexOf( "@context" ) == -1
		//		|| (
		//			frameworkExportJSON.IndexOf( "@graph" ) == -1
		//			&& frameworkExportJSON.IndexOf( "\"@type\":\"ConceptScheme\"" ) == -1) //temp
		//		)
		//	{
		//		messages.Add( "Error: the payload is not formatted properly." );
		//		return;
		//	}

		//	//Do the publish using the JSON exported from CASS
		//	framework.Payload = frameworkExportJSON;
		//	List<SiteActivity> list = new List<SiteActivity>();
		//	bool valid = new RegistryServices().PublishConceptScheme( framework, user, ref messages, ref list );

		//	//Update
		//	if( valid )
		//	{
		//		//doesn't work this way. Details come from activity log
		//		//framework.IsPublished = true;
		//		framework.LastPublished = DateTime.Now;
		//		framework.LastPublishedById = user.Id;

		//		//Save
		//		new Manager().Update( framework, ref messages, true );
		//	}
		//}
		//
		public static void HandleUnPublishEvent( string ctid, AppUser user, ref List<string> messages )
		{
			//Validate
			var framework = Manager.GetByCtid( ctid );
			if ( !ValidateFrameworkAction( framework, user, ref messages ) )
			{
				return;
			}

			if ( framework.CTID != ctid )
			{
				messages.Add( "The Concept Scheme CTID doesn't match the version in the publisher." );
				return;
			}

			List<SiteActivity> list = new List<SiteActivity>();
			bool valid = new RegistryServices().HandleUnPublishEventConceptScheme( framework, user, ref messages  );

			//Update
			if ( valid )
			{
				string statusMessage = "";
				if (!new ConceptSchemeManager().HandleUnPublishEvent( framework.Id, user.Id, ref statusMessage ))
				{
					messages.Add( statusMessage );
				}
			}
		}
		//
		public static void Delete(ThisEntity record, AppUser user, ref List<string> messages)
		{

			//- check if published
			if ( record.IsPublished )
			{
				HandleUnPublishEvent( record.CTID, user, ref messages );
			}

			if ( new Manager().Delete( record.CTID, ref messages ) )
			{
				new ActivityServices().AddActivity( new SiteActivity()
				{
					ActivityType = "ConceptScheme",
					Activity = "Editor",
					Event = "Delete",
					Comment = string.Format( "{0} deleted concept scheme: {1}, CTID: {2}", user.FullName(), record.Name, record.CTID ),
					ActionByUserId = user.Id,
					ActivityObjectParentEntityUid = record.RowId
				} );
			}
		}
		//
		public static ThisEntity GetByCTID( string ctid, bool includingConcepts = false )
		{
			return Manager.GetByCtid( ctid, includingConcepts );
		}
		//
		public static ThisEntity GetByID( int id )
		{
			return Manager.Get( id );
		}
		//
		public static ThisEntity GetForDetail( int id )
		{
			return Manager.Get( id, true );
		}
		public static List<ThisEntity> GetAllForOwningOrganization( string orgUid, ref int pTotalRows )
		{
			

			List<ThisEntity> list = new List<ThisEntity>();
			if ( string.IsNullOrWhiteSpace( orgUid ) )
				return list;
			var org = OrganizationManager.GetForSummary( orgUid );
			var glist = Manager.GetAllForOrganization( org.Id );
			pTotalRows = glist.Count();
			return glist;

		}
		//
		public static string GetForReview( int id, AppUser user, ref bool isValid, ref List<string> messages, ref bool isApproved, ref DateTime? lastPublishDate, ref DateTime lastUpdatedDate, ref DateTime lastApprovedDate, ref string ctid )
		{
			//string statusMessage = "";
			var entity = Manager.Get( id, true );
			if ( entity == null || entity.Id == 0 )
			{
				isValid = false;
				messages.Add( "Error - the requested ConceptScheme was not found." );
				return "";
			}
			entity.CanUserEditEntity = true;
			isApproved = entity.IsThisApproved();
			ctid = entity.CTID;
			string crEnvelopeId = "";
			string payload = RegistryAssistantServices.ConceptSchemeMapper.AssistantRequest( entity, "format", "", user, false, ref isValid, ref messages, ref crEnvelopeId );
			lastUpdatedDate = entity.EntityLastUpdated;
			lastApprovedDate = entity.LastApproved;// entity.EntityApproval.Created;
			if ( ( entity.CredentialRegistryId ?? "" ).Length == 36 )
				lastPublishDate = ActivityManager.GetLastPublishDateTime( "pathway", id );
			return payload;
		}
		//
		public static List<ConceptSchemeSummary> Search( MainSearchInput data, ref int totalRows )
		{
			string where = "";
			List<string> messages = new List<string>();
			List<string> competencies = new List<string>();
			int userId = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;
			//only target records with a ctid
			where = " (len(Isnull(base.Ctid,'')) = 39) ";

			SetKeywordFilter( data.Keywords, false, ref where );
			//SearchServices.SetLanguageFilter( data, CodesManager.ENTITY_TYPE_CONCEPT_SCHEME, ref where );
	
			////
			SearchServices.SetAuthorizationFilter( user, "ConceptSchemeSummary", ref where );

			//SearchServices.HandleCustomFilters( data, 60, ref where );
			//
		
			SearchServices.SetDatesFilter( data, CodesManager.ENTITY_TYPE_CONCEPT_SCHEME, ref where, ref messages );
			//
			SearchServices.HandleApprovalFilters( data, 16, CodesManager.ENTITY_TYPE_CONCEPT_SCHEME, ref where );


			//can this be replaced by following
			SearchServices.SetRolesFilter( data, ref where );

			//owned/offered
			SearchServices.HandleOwingOrgFilters( data, CodesManager.ENTITY_TYPE_CONCEPT_SCHEME, ref where );
			//probably N/A
			SearchServices.SetBoundariesFilter( data, ref where );


			LoggingHelper.DoTrace( 5, "ConceptSchemeServices.Search(). Filter: " + where );
			BaseSearchModel bsm = new BaseSearchModel()
			{
				Filter = where,
				OrderBy = data.SortOrder,
				PageNumber = data.StartPage,
				PageSize = data.PageSize,
				UserId = userId
			};

			return Manager.Search( bsm, ref totalRows );
		}

		private static void SetKeywordFilter( string keywords, bool isBasic, ref string where )
		{
			if ( string.IsNullOrWhiteSpace( keywords ) || string.IsNullOrWhiteSpace( keywords.Trim() ) )
				return;


			//trim trailing (org)
			if ( keywords.IndexOf( "('" ) > 0 )
				keywords = keywords.Substring( 0, keywords.IndexOf( "('" ) );

			//OR base.Description like '{0}'  
			string text = " (base.name like '{0}' OR base.OrganizationName like '{0}'  ) ";
			if ( SearchServices.IncludingDescriptionInKeywordFilter )
			{
				text = " (base.name like '{0}' OR base.OrganizationName like '{0}' OR base.Description like '{0}'  OR base.ExternalIdentifier = '{0}' ) ";
			}

			bool isCustomSearch = false;
			//for ctid, needs a valid ctid or guid
			if ( keywords.IndexOf( "ce-" ) > -1 && keywords.Length == 39 )
			{
				text = " ( CTID = '{0}' ) ";
				isCustomSearch = true;
			}
			else if ( ServiceHelper.IsValidGuid( keywords ) )
			{
				text = " ( CTID = 'ce-{0}' ) ";
				isCustomSearch = true;
			}
			else if ( ServiceHelper.IsInteger( keywords ) )
			{
				text = " ( Id = '{0}' ) ";
				isCustomSearch = true;
			}
			else if ( keywords.ToLower() == "[hascredentialregistryid]" )
			{
				text = " ( len(Isnull(CredentialRegistryId,'') ) = 36 ) ";
				isCustomSearch = true;
			}
			else if ( keywords.ToLower().IndexOf( "orgid:" ) == 0 )
			{
				string[] parts = keywords.Split( ':' );
				if ( parts.Count() > 1 )
				{
					if ( ServiceHelper.IsInteger( parts[ 1 ] ) )
					{
						text = string.Format( " ( OwningOrganizationId={0} ) ", parts[ 1 ].Trim() );
						isCustomSearch = true;
					}
				}
			}


			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";

			keywords = ServiceHelper.HandleApostrophes( keywords );
			if ( keywords.IndexOf( "%" ) == -1 && !isCustomSearch )
			{
				keywords = SearchServices.SearchifyWord( keywords );
			}

			//skip url  OR base.Url like '{0}' 
			if ( isBasic || isCustomSearch )
			{
				//if ( !includingFrameworkItemsInKeywordSearch )
				//	where = where + AND + string.Format( " ( " + text + " ) ", keywords );
				//else
					where = where + AND + string.Format( " ( " + text + " ) ", keywords );
			}
			else
			{
				//if ( using_EntityIndexSearch )
				//	where = where + AND + string.Format( " ( " + text + indexFilter + " ) ", keywords );
				//else
					where = where + AND + string.Format( " ( " + text  + " ) ", keywords );
			}

		}


		//
		//Set the LastUpdated date of the framework to now
		public void MarkUpdated( string ctid, string name, AppUser user, ref List<string> messages )
		{
			//Validate
			var framework = Manager.GetByCtid( ctid );
			if ( !ValidateFrameworkAction( framework, user, ref messages ) )
			{
				return;
			}

			//Update
			framework.Name = string.IsNullOrWhiteSpace( name ) ? framework.Name : name;
			framework.LastUpdated = DateTime.Now;
			framework.LastUpdatedById = user.Id;

			//Save
			Update( framework, ref messages );
		}
		//
		private bool Update( ThisEntity framework, ref List<string> messages )
		{
			var user = AccountServices.GetCurrentUser();
			framework.LastUpdatedById = user.Id;
			//if there are no changes to the framework, the Update method will return false, but without any messages
			bool isValid = new Manager().Update( framework, ref messages, false, true );
			if ( isValid )
			{

				new ActivityServices().AddActivity( new SiteActivity()
				{
					ActivityType = "ConceptScheme",
					Activity = "Editor",
					Event = "Update",
					Comment = string.Format( "{0} updated Concept Scheme: {1}, Id: {2}", user.FullName(), framework.Name, framework.Id ),
					ActivityObjectId = framework.Id,
					ActionByUserId = user.Id,
					ActivityObjectParentEntityUid = framework.RowId
				} );

			}
			else if ( messages.Count > 0 )
			{
				isValid = false;
			}

			return isValid;
		}
		//
		public static bool ValidateFrameworkAction( ThisEntity framework, AppUser user, ref List<string> messages )
		{
			if ( framework == null || framework.Id == 0 )
			{
				messages.Add( "Concept Scheme Not Found" );
				return false;
			}

			if ( !CanUserUpdateFramework( user, framework.OrgId ) )
			{
				messages.Add( "You don't have access to manage that Concept Scheme." );
				return false;
			}

			return true;
		}

		
		/// <summary>
		/// Get CaSS payload using the MD5 string route
		/// may need to handle CTID or URI(@id).
		/// </summary>
		/// <param name="identifier"></param>
		/// <returns></returns>
		[Obsolete]
		public static string GetPayloadFromCaSS(ThisEntity entity)
		{
			List<string> messages = new List<string>();
			string statusMessage = "";
			//change to conceptSchemeExportUrl
			string exportUrl = UtilityManager.GetAppKeyValue( "cassExportUrl", "" );
			//should end with a period. Should no longer be necessary with the use of credRegistryResourceUrl
			var cassResourceUrlPrefix = ServiceHelper.GetAppKeyValue( "cassResourceUrlPrefix" );
			
			string cassURI = "";
			if ( !string.IsNullOrWhiteSpace( entity.EditorUri )
				&& Manager.IsUrlValid( entity.EditorUri, ref statusMessage, true ) )
			{
				cassURI = entity.EditorUri;
			}
			else if ( ServiceHelper.IsValidCtid( entity.CTID, ref messages ) )
			{
				//WARNING: in the dev environment, the CER type URI for CaSS frameworks don't always use sandbox domain!
				//also the dev env doesn't use ce-??? ==> is this still true?
				//cassURI = "https://credentialengineregistry.org/resources/" + identifier;
				//21-11-11 mp - this should now use a standard format registry url
				var registryUrl = UtilityManager.GetAppKeyValue( "credRegistryResourceUrl", "" );
				//cassURI = string.Format( "https://{0}credentialengineregistry.org/resources/", cassResourceUrlPrefix ) + entity.CTID;
				cassURI = registryUrl + entity.CTID;
			}
			else
			{
				//error
				return "Error: A concept scheme must have a valid URL from the CaSS editor.";
			}

			string resourceUrl = string.Format( exportUrl, UtilityManager.GenerateMD5String( cassURI ) );

			//dev env of CASS doesn't use the ce- so strip. The app key for prod will include the ce-

			DateTime started = DateTime.Now;
			try
			{

				LoggingHelper.DoTrace( 6, string.Format( "ConceptSchemeServices.GetPayloadFromCaSS( ThisEntity entity ). Name: {0}, CTID: {1} ,resourceUrl: {2}", entity.Name, entity.CTID, resourceUrl ) );

				//dev env of CASS doesn't use the ce- so strip. The app key for prod will include the ce-
				//var resourceURI = string.Format( getURL, CTID.Replace( "ce-", "" ) );
				var responseData = "";
				using( var client = new HttpClient() )
				{
					client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
					//need to handle large documents
					client.Timeout = new TimeSpan( 0, 20, 0 );

					var response = client.GetAsync( resourceUrl ).Result;
					responseData = response.Content.ReadAsStringAsync().Result;
				}
				TimeSpan duration = DateTime.Now.Subtract( started );
				LoggingHelper.DoTrace( 6, string.Format( "ConceptSchemeServices.GetPayloadFromCaSS - completed. for: '{0}',  elapsed: {1:N2} seconds", entity.Name, duration.TotalSeconds ) );

				return responseData;
			}
			catch( Exception ex )
			{
				TimeSpan duration = DateTime.Now.Subtract( started );
				LoggingHelper.DoTrace( 6, string.Format( "ConceptSchemeServices.GetPayloadFromCaSS - FAILED. for: '{0}',  elapsed: {1:N2} seconds", entity.Name, duration.TotalSeconds ) );

				statusMessage = BaseFactory.FormatExceptions( ex );
				LoggingHelper.LogError( ex, "ConceptSchemeServices.GetPayloadFromCaSS()" );
				return "";
			}
		}
		public static bool CanUserUpdateFramework( AppUser user, int orgId )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( OrganizationManager.IsOrganizationMember( user.Id, orgId ) )
				return true;
			else if ( AccountServices.IsUserSiteStaff( user ) )
				return true;
			//check for user.Organizations
			var exists = user.Organizations.FirstOrDefault( a => a.Id == orgId );
			if ( exists != null && exists.Id > 0 )
				return true;

			return false;
		}
	}
}
