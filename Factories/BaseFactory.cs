using Data.Tables;
using LumenWorks.Framework.IO.Csv;
using Models.Curation;
using Models.Search;
using Navy.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using DataEntities = Data.Tables.NavyRRLEntities;
using MSc = Models.Schema;
using Newtonsoft.Json.Linq;

namespace Factories
{
	/// <summary>
	/// 
	/// </summary>
	public class BaseFactory
	{
		public static string thisClassName = "BaseFactory";
		public List<string> warnings = new List<string>();

		public static string commonStatusMessage = "";

		public static int RELATIONSHIP_TYPE_HAS_PART = 1;
		public static int RELATIONSHIP_TYPE_IS_PART_OF = 2;


		public static bool IsDevEnv()
		{
			if ( UtilityManager.GetAppKeyValue( "environment", "no" ) == "development" )
				return true;
			else
				return false;
		}
		public static bool IsProduction()
		{

			if ( UtilityManager.GetAppKeyValue( "environment", "no" ) == "production" )
				return true;
			else
				return false;
		}
		#region Entity frameworks helpers
		public static bool HasStateChanged( DataEntities context )
		{
			if ( context.ChangeTracker.Entries().Any( e =>
					e.State == EntityState.Added ||
					e.State == EntityState.Modified ||
					e.State == EntityState.Deleted ) == true )
				return true;
			else
				return false;
		}
		//

		public static void AddErrorIf( List<string> errors, bool condition, string errorMessage )
		{
			if ( condition )
			{
				errors.Add( errorMessage );
			}
		}

		public static bool CompareStrings( string stringOne, string stringTwo )
		{
			return string.Equals( stringOne, stringTwo, StringComparison.OrdinalIgnoreCase );
		}
		//

		public static void DuplicateCheck<T2>( string typeLabel, Func<DataEntities, IQueryable<T2>> GetHaystackMethod, List<string> errors, List<StringCheckMapping<T2>> checkStrings = null, Action<IQueryable<T2>, DataEntities> ExtraChecksMethod = null ) where T2 : class, DBEntityBaseObject
		{
			using ( var context = new DataEntities() )
			{
				var haystack = GetHaystackMethod( context );

				foreach ( var item in checkStrings ?? new List<StringCheckMapping<T2>>() )
				{
					if ( !string.IsNullOrWhiteSpace( item.SourceValue ) )
					{
						if( haystack.Where( item.CompareMethod ).Count() > 0 )
						{
							errors.Add( item.CustomErrorMessage ?? "Another " + typeLabel + " has a matching " + item.PropertyLabel + "." );
						}
					}
				}

				ExtraChecksMethod?.Invoke( haystack, context );
			}
		}
		public class StringCheckMapping<T2> where T2 : class, DBEntityBaseObject
		{
			public StringCheckMapping( string sourceValue, Func<T2, bool> compareMethod, string propertyLabel, string customErrorMessage = null )
			{
				SourceValue = sourceValue;
				CompareMethod = compareMethod;
				PropertyLabel = propertyLabel;
				CustomErrorMessage = customErrorMessage;
			}
			public string SourceValue { get; set; }
			public Func<T2, bool> CompareMethod { get; set; }
			public string PropertyLabel { get; set; }
			public string CustomErrorMessage { get; set; }
		}
		//


		public static void BasicSaveCore<T1, T2>( DataEntities context, T1 entity, DbSet<T2> contextDBEntityList, int userID, Action<T1, T2> UpdateBeforeInitialSaveMethod, Action<T1, T2> UpdateBeforeSecondSaveMethod, string eventAction, Action<string> AddErrorMethod ) where T1 : Models.Schema.BaseObject where T2 : class, DBEntityBaseObject, new()
		{
			//Do not allow anything to be saved with a value of "N/A"
			foreach( var textProperty in typeof( T1 ).GetProperties().Where( m => m.PropertyType == typeof( string ) ) )
			{
				if( ( ( string ) textProperty.GetValue( entity ) )?.ToLower() == "n/a" )
				{
					AddErrorMethod( "A value of \"N/A\" is not allowed for property: " + textProperty.Name );
					return;
				}
			}

			//Continue
			var entityType = typeof( T1 );
			try
			{
				//Null check
				if ( entity == null )
				{
					AddErrorMethod( "Error - Attempted to save a null entity." );
					return;
				}

				//Setup action tracking
				var eventType = "";
				var eventComment = "";

				//Save/update the entity
				//This method runs inside of the using() statement initiated by whatever method called this method, so the context is already setup
				var dbEntity = contextDBEntityList.FirstOrDefault( m => m.RowId == entity.RowId );

				//Handle new entity
				if ( dbEntity == null || dbEntity.Id == 0 )
				{
					//Logging
					eventType = "Add";
					eventComment = entityType.Name + " was added via " + eventAction + ". RowID: " + entity.RowId;

					//Create identifiers and other tracking if not already present
					//Do these on the incoming entity before mapping to ensure that both entities have the same info after this method is finished
					entity.RowId = entity.RowId == Guid.Empty ? Guid.NewGuid() : entity.RowId; //Should already have been set by the import process, but just in case
					entity.CTID = string.IsNullOrWhiteSpace( entity.CTID ) ? "ce-" + entity.RowId.ToString().ToLower() : entity.CTID; //Also should already have been set by the import process, but just in case
					entity.Created = DateTime.Now;
					entity.CreatedById = userID;

					//Add the new item
					dbEntity = new T2();
					AutoMap( entity, dbEntity );
					dbEntity.CreatedById = userID; //Ensure this is set
					contextDBEntityList.Add( dbEntity );
				}
				//Handle updated entity
				else
				{
					//Logging
					eventType = "Save";
					eventComment = entityType.Name + " was updated via " + eventAction + ". RowID: " + entity.RowId;

					//Update the entity
					AutoMap( entity, dbEntity );
				}

				//Handle any related references to single-value entities (multi-value entity references require this object to be saved first, so those should be handled after calling the BasicSaveFromUpload method)
				UpdateBeforeInitialSaveMethod( entity, dbEntity );

				//If anything changed, save it
				if ( HasStateChanged( context ) )
				{
					//Ensure both objects get these properties updated
					entity.LastUpdated = DateTime.Now;
					entity.LastUpdatedById = userID;
					dbEntity.LastUpdated = entity.LastUpdated;
					dbEntity.LastUpdatedById = entity.LastUpdatedById;

					//Save the changes
					var count = context.SaveChanges();
					if ( count > 0 )
					{
						//Track the changes
						entity.Id = dbEntity.Id; //Used by lookups later on in the import process
						new ActivityManager().SiteActivityAdd( new Models.Application.SiteActivity()
						{
							ActivityType = eventType,
							Activity = eventAction,
							Event = eventType,
							Comment = eventComment,
							ActionByUserId = userID,
							ActivityObjectId = entity.Id
						} );
					}
					else
					{
						var message = entityType.Name + string.Format( " Save Failed", "Attempted to {0} a(n) {1}. The process appeared to not work, but was not an exception, so we have no message, or no clue. {1}: {2}, CTID: {3}",
							eventType.ToLower(), entityType.Name, entity.RowId.ToString(), entity.CTID );
						AddErrorMethod( "Error - saving the " + entityType.Name + " was not successful. " + message );
						EmailManager.NotifyAdmin( eventType.ToLower() + " Save Failed", message );
					}
				}

				//Handle any related references involving entities that require the parent entity to be saved above first (e.g. join tables for multi-value fields)
				UpdateBeforeSecondSaveMethod( entity, dbEntity );
				if ( HasStateChanged( context ) )
				{
					context.SaveChanges();
				}
			}
			catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
			{
				var message = new BaseFactory().HandleDBValidationError( dbex, entityType.Name + string.Format( " Save Failed. ID: {0}, RowID: {1}", entity.Id, entity.RowId ), entityType.Name );
				AddErrorMethod( entityType.Name + " Save Failed. Error - the save was not successful. " + message );
			}
			catch ( Exception ex )
			{
				var message = FormatExceptions( ex );
				LoggingHelper.LogError( ex, entityType.Name + string.Format( " Save Failed. ID: {0}, RowID: {1}", entity.Id, entity.RowId ), true );
				AddErrorMethod( entityType.Name + " Save Failed. Error - the save was not successful. " + message );
			}
		}
		//

		public static void HandleMultiValueUpdate<T1, T2, T3>(
			DataEntities context,
			int userID,
			List<Guid> entityValues,
			T1 dbEntity,
			ICollection<T2> joinTableRowsForDBEntity,
			DbSet<T3> destinationTable,
			string joinTablePropertyNameForDBEntity,
			string joinTablePropertyNameForDestination
		) where T1 : DBEntityBaseObject where T2 : DBEntityJoinTableItem, new() where T3 : class, DBEntityBaseObject
		{
			//Get the properties that are unique to this join table (e.g., SomethingId and SomethingConceptId)
			var joinTablePropertyForDBEntity = typeof( T2 ).GetProperty( joinTablePropertyNameForDBEntity );
			var joinTablePropertyForDestination = typeof( T2 ).GetProperty( joinTablePropertyNameForDestination );

			//Get the integer IDs of the referenced objects (e.g., referenced concepts) by looking them up via their GUIDs
			var updatedIDs = destinationTable.Where( referencedItem => entityValues.Contains( referencedItem.RowId ) ).Select( referencedItem => referencedItem.Id ).ToList();

			//Get the join table rows (attached to the DB entity) that are no longer relevant to the list of updated IDs
			var removals = joinTableRowsForDBEntity.Where( joinTableRow => !updatedIDs.Contains( ( int ) joinTablePropertyForDestination.GetValue( joinTableRow ) ) ).ToList();

			//Get the updated IDs that aren't already referenced by an existing join table row attached to the DB entity
			var additions = updatedIDs.Where( updatedID => !joinTableRowsForDBEntity.Select( joinTableRow => ( int ) joinTablePropertyForDestination.GetValue( joinTableRow ) ).Contains( updatedID ) ).ToList();

			//Remove the old rows where no longer needed
			foreach ( var removal in removals )
			{
				joinTableRowsForDBEntity.Remove( removal );
				context.Entry( removal ).State = EntityState.Deleted;
			}

			//Create new rows where needed
			foreach ( var addition in additions )
			{
				//Create the item
				var newJoinTableRow = new T2()
				{
					RowId = Guid.NewGuid(),
					Created = DateTime.Now,
					CreatedById = userID
				};

				//Set the properties that are unique to each join table
				joinTablePropertyForDBEntity.SetValue( newJoinTableRow, dbEntity.Id );
				joinTablePropertyForDestination.SetValue( newJoinTableRow, addition );

				//Add the item to the dbEntity's values
				joinTableRowsForDBEntity.Add( newJoinTableRow );
			}
		}
		//

		public static DeleteResult BasicDeleteCore<T1>( 
			string entityTypeLabel, 
			Func<DataEntities, DbSet<T1>> GetDBEntityListMethod, 
			int entityID, 
			string searchFilterNameOrNull, 
			Func<DataEntities, DbSet<T1>, T1, DeleteResult> ReturnDeleteResultOrNullBeforeDeleteIsAttemptedMethod 
		) where T1: class, DBEntityBaseObject
		{
			//If applicable, do a search to see whether other things reference this entity
			if( !string.IsNullOrWhiteSpace( searchFilterNameOrNull ) )
			{
				var totalResults = RatingContextManager.Search( new SearchQuery() { Skip = 0, Take = 0, Filters = new List<SearchFilter>() { new SearchFilter() { Name = searchFilterNameOrNull, ItemIds = new List<int>() { entityID } } } } ).TotalResults;
				if( totalResults > 0 )
				{
					return new DeleteResult( false, "The target " + entityTypeLabel + " is referenced by " + totalResults + " Rating Context object(s) and cannot be deleted." );
				}
			}

			//Otherwise, Continue
			using( var context = new DataEntities() )
			{
				//Get the list
				var list = GetDBEntityListMethod( context );

				//Ensure the target exists
				var toBeDeleted = list.FirstOrDefault( m => m.Id == entityID );
				if ( toBeDeleted == null )
				{
					return new DeleteResult( false, "The target " + entityTypeLabel + " does not exist." );
				}

				//Ensure it is not a protected entity
				if( toBeDeleted.CreatedById == -999 || context.ProtectedSystemEntities.FirstOrDefault( m => m.EntityRowId == toBeDeleted.RowId ) != null )
				{
					return new DeleteResult( false, "The target " + entityTypeLabel + " is part of the system and cannot be deleted." );
				}

				//Run any pre-delete checks that are specific to the caller of this method
				var preemptiveReturn = ReturnDeleteResultOrNullBeforeDeleteIsAttemptedMethod == null ? null : ReturnDeleteResultOrNullBeforeDeleteIsAttemptedMethod( context, list, toBeDeleted );
				if( preemptiveReturn != null )
				{
					return preemptiveReturn;
				}

				//Try to delete it
				try
				{
					list.Remove( toBeDeleted );
					context.SaveChanges();

					return new DeleteResult( true, "The target " + entityTypeLabel + " was successfully deleted." );
				}
				catch ( Exception ex )
				{
					return new DeleteResult( false, "Error deleting the target " + entityTypeLabel + ": " + ex.Message + ( !string.IsNullOrWhiteSpace( ex.InnerException?.Message ) ? "; " + ex.InnerException.Message : "" ), JObject.FromObject( ex ) );
				}
			}
		}
		//

		public static SearchResultSet<T2> HandleSearch<T1, T2>(
			SearchQuery query,
			Func<DataEntities, IEnumerable<T1>> SearchMethodWithOrderedResultSet,
			Func<T1, DataEntities, SearchResultSet<T2>, T2> MappingMethod
		) where T1 : class, DBEntityBaseObject where T2 : Models.Schema.BaseObject, new()
		{
			var appEntityResults = new SearchResultSet<T2>();

			try
			{
				using ( var context = new DataEntities() )
				{
					var orderedAllDBResults = SearchMethodWithOrderedResultSet( context );
					appEntityResults.TotalResults = orderedAllDBResults.Count();
					var dbPageResults = orderedAllDBResults.Skip( query.Skip ).Take( query.Take ).ToList();

					foreach ( var dbResult in dbPageResults )
					{
						appEntityResults.Results.Add( MappingMethod( dbResult, context, appEntityResults ) );
					}
				}
			}
			catch ( Exception ex )
			{
				appEntityResults.Debug?.Add( "Exception", ex.Message );
				appEntityResults.Debug?.Add( "Inner Exception", ex.InnerException?.Message );
			}

			return appEntityResults;
		}
		//

		public static IOrderedEnumerable<T> HandleSort<T>( IEnumerable<T> unsortedList, List<SortOrderItem> sortOrder, Func<T, object> GetAlphaPropertyMethod, Func<IEnumerable<T>, IOrderedEnumerable<T>> DefaultSortMethod, Func<IEnumerable<T>, List<string>, IOrderedEnumerable<T>> RelevanceSortMethod = null, string keywords = null ) where T : DBEntityBaseObject
		{
			//If no sort order is specified, use the default handler
			if ( sortOrder == null || sortOrder.Count() == 0 || sortOrder.FirstOrDefault( m => m.Column == "sortOrder:DefaultMethod" ) != null )
			{
				return DefaultSortMethod( unsortedList );
			}

			//Enable skipping the sorting to avoid a performance hit
			if ( sortOrder.FirstOrDefault( m => m.Column == "sortOrder:Unsorted" ) != null ) 
			{
				return unsortedList.OrderBy( m => true );
			}

			//If doing relevance-based sorting...
			if( sortOrder.FirstOrDefault( m => m.Column == "sortOrder:Relevance" ) != null )
			{
				if( RelevanceSortMethod != null && !string.IsNullOrWhiteSpace( keywords ) )
				{
					return RelevanceSortMethod( unsortedList, GetRelevanceTokens( keywords ) );
				}
				else
				{
					return DefaultSortMethod( unsortedList );
				}
			}

			//Otherwise, convert to IOrderedEnumerable
			var sorted = unsortedList.OrderBy( m => m != null );

			//For each SortItem...
			foreach( var sortItem in sortOrder )
			{
				//Get the column that matches, if available
				var column = typeof( T ).GetProperty( sortItem.Column );
				//The GetterMethod will be one of:
				var GetterMethod =
					//Getting the property specified by the Column, or
					column != null ? ( T m ) => { return column.GetValue( m ); } :
					//Getting the property specified by the GetAlphaPropertyMethod, or
					sortItem.Column == "sortOrder:DefaultAlphaSort" ? GetAlphaPropertyMethod :
					//Getting the Id for the object (fallback/default)
					( T m ) => { return m.Id; };

				//Apply the sort based on whether or not the SortItem indicates it should be Ascending
				sorted = sortItem.Ascending ? sorted.ThenBy( m => GetterMethod( m ) ) : sorted.ThenByDescending( m => GetterMethod( m ) );
			}

			return sorted;
		}
		//

		public static List<string> GetRelevanceTokens( string keywords, bool includePartialTokens = true )
		{
			if ( string.IsNullOrWhiteSpace( keywords ) )
			{
				return new List<string>();
			}

			var tokens = new List<string>() { keywords }.Concat( keywords.Split( new string[] { " " }, StringSplitOptions.RemoveEmptyEntries ) ).ToList();
			if ( includePartialTokens )
			{
				while ( keywords.Length > 0 )
				{
					tokens.Add( string.Join( "", keywords.Take( 3 ).ToList() ) );
					keywords = keywords.Substring( keywords.Length >= 3 ? 3 : keywords.Length );
				}
			}

			return tokens;
		}
		//

		public static int RelevanceHelperOLD<T>( T dbEnt, List<string> keywordParts, Func<T, string> GetStringField )
		{
			return keywordParts.Select( m =>
				GetStringField( dbEnt ) == null ? -1 : //If the value is not found, use the placeholder
				GetStringField( dbEnt ).IndexOf( m.ToString(), StringComparison.OrdinalIgnoreCase ) //Get the index of the first instance where the keyword part appears
			).Where( m => m != -1 ).Sum(); //Filter out placeholders and non-existent values and add the rest together
		}
		//

		public static int RelevanceHelper<T>( T dbEnt, List<string> keywordParts, Func<T, string> GetStringField, List<string> applicableColumns = null, string checkColumn = null, int boost = 0 )
		{
			//If we need to check for a column and it's not in the list, then return 0
			if ( applicableColumns != null && !applicableColumns.Contains( checkColumn ?? "" ) )
			{
				return 0;
			}
			//Otherwise (if not column checking, or if the column was in the list), perform the check
			else
			{
				return keywordParts.Select( m =>
					GetStringField( dbEnt ) == null ? -1 : //If the value is not found, use the placeholder
					GetStringField( dbEnt ).IndexOf( m.ToString(), StringComparison.OrdinalIgnoreCase ) //Get the index of the first instance where the keyword part appears
				).Where( m => m != -1 ) //Filter out placeholders and non-matches
				.Select( m => m - boost ).Sum(); //Boost the results (by subtraction since we're using .OrderBy()) and add them together
			}
		}
		//

		public static T2 GetSingleItem<T1, T2>(
			Func<DataEntities, T1> GetMethod,
			Func<T1, DataEntities, T2> MappingMethod,
			bool returnNullIfNotFound = false
		) where T1 : class, DBEntityBaseObject where T2 : Models.Schema.BaseObject, new()
		{
			using ( var context = new DataEntities() )
			{
				var item = GetMethod( context );
				return MapDBToResult( context, item, MappingMethod, returnNullIfNotFound );
			}
		}
		//

		public static List<T2> GetItemList<T1, T2>(
			Func<DataEntities, IEnumerable<T1>> GetMethod,
			Func<T1, DataEntities, T2> MappingMethod,
			bool returnNullIfEmpty = false
		) where T1 : class, DBEntityBaseObject where T2 : Models.Schema.BaseObject, new()
		{
			var output = new List<T2>();

			using ( var context = new DataEntities() )
			{
				var list = GetMethod( context ).ToList();
				foreach( var item in list )
				{
					output.Add( MapDBToResult( context, item, MappingMethod, false ) );
				}
			}

			return returnNullIfEmpty && output.Count() == 0 ? null : output;
		}
		//

		public static T2 GetSingleByFilter<T1, T2>(
			Func<DataEntities, DbSet<T1>> GetDBEntityListMethod,
			Func<T1, bool> FilterMethod,
			Func<T1, DataEntities, T2> MappingMethod,
			bool returnNullIfNotFound = false
		) where T1 : class, DBEntityBaseObject where T2 : Models.Schema.BaseObject, new()
		{
			using ( var context = new DataEntities() )
			{
				var dbEntityList = GetDBEntityListMethod( context );
				var value = dbEntityList.FirstOrDefault( FilterMethod );
				return MapDBToResult( context, value, MappingMethod, returnNullIfNotFound );
			}
		}
		//

		public static List<T2> GetMultipleByFilter<T1, T2, T3>(
			Func<DataEntities, DbSet<T1>> GetDBEntityListMethod,
			Func<T1, bool> FilterMethod,
			Func<T1, T3> SortMethod,
			bool orderByDescending,
			Func<T1, DataEntities, T2> MappingMethod,
			bool returnNullIfEmpty = false
		) where T1 : class, DBEntityBaseObject where T2 : Models.Schema.BaseObject, new()
		{
			var results = new List<T2>();
			using ( var context = new DataEntities() )
			{
				var dbEntityList = GetDBEntityListMethod( context );
				var query = dbEntityList.Where( FilterMethod );
				if( SortMethod != null )
				{
					query = orderByDescending ? query.OrderByDescending( SortMethod ) : query.OrderBy( SortMethod );
				}
				var values = query.ToList();
				
				foreach( var value in values )
				{
					results.Add( MapDBToResult( context, value, MappingMethod, false ) );
				}
			}
			return returnNullIfEmpty && results.Count() == 0 ? null : results;
		}
		//

		public static T2 GetByIdentifier<T1, T2>( 
			string identifier,
			Func<DataEntities, DbSet<T1>> GetDBEntityListMethod,
			Func<DbSet<T1>, T1> ExtraGetMethod,
			Func<T1, DataEntities, T2> MappingMethod,
			bool returnNullIfNotFound = false
		) where T1 : class, DBEntityBaseObject where T2 : Models.Schema.BaseObject, new()
		{
			var testIntId = TryConvert( identifier, int.Parse, 0 );
			var testRowId = TryConvert( identifier, Guid.Parse, Guid.Empty );
			var testCTID = identifier.ToLower();

			return GetSingleItem( context =>
			 {
				 var dbEntityList = GetDBEntityListMethod( context );
				 var value =
					 testIntId > 0 ? dbEntityList.FirstOrDefault( m => m.Id == testIntId ) :
					 testRowId != Guid.Empty ? dbEntityList.FirstOrDefault( m => m.RowId == testRowId ) :
					 !string.IsNullOrWhiteSpace( testCTID ) ? dbEntityList.FirstOrDefault( m => m.CTID.ToLower() == testCTID ) :
					 null;

				 if ( value == null )
				 {
					 value = ExtraGetMethod( dbEntityList );
				 }

				 return value;
			 }, MappingMethod, returnNullIfNotFound );
		}
		//

		public static void AppendIDsFilterIfPresent( SearchQuery query, string filterName, Action<List<int>> AppendFilterMethod )
		{
			//Need to do it this way to handle the edge case on the RMTL search where the summary looks for one source type but the user's query has another. This should result in a logical AND-ing of the two filters.
			foreach( var filter in query.Filters.Where( m => m.Name.ToLower() == filterName.ToLower() ) )
			{
				if ( filter.ItemIds.Count() > 0 ) {
					AppendFilterMethod( filter.ItemIds );
				}
			}
		}
		//

		public static void AppendTextFilterIfPresent( SearchQuery query, string filterName, Action<string> AppendFilterMethod )
		{
			var text = query.GetFilterTextByName( filterName );
			text = HandleApostrophes( text ).Trim();
			if ( !string.IsNullOrWhiteSpace( text ) )
			{
				AppendFilterMethod( text );
			}
		}
		//

		public static void AppendNotNullFilterIfPresent( SearchQuery query, string filterName, Action AppendFilterMethod )
		{
			var filter = query.GetFilterByName( filterName );
			if( filter != null )
			{
				AppendFilterMethod();
			}
		}
		//

		public static T TryConvert<T>( string input, Func<string, T> ConvertMethod, T fallback )
		{
			try
			{
				return ConvertMethod( input );
			}
			catch
			{
				return fallback;
			}
		}
		//

		public static T2 MapDBToResult<T1, T2>(
			DataEntities context,
			T1 dbEntity,
			Func<T1, DataEntities, T2> MappingMethod,
			bool returnNullIfNotFound = false
		) where T1 : class, DBEntityBaseObject where T2 : Models.Schema.BaseObject, new()
		{
			if( dbEntity != null )
			{
				return MappingMethod( dbEntity, context );
			}

			return returnNullIfNotFound ? null : new T2();
		}
		//

		public static void MapAndAppendResourceIfNotNull<T1, T2, T3>( T1 input, DataEntities context, Func<T1, DataEntities, T2> MappingMethod, SearchResultSet<T3> resultSet ) 
			where T1 : class, DBEntityBaseObject
			where T2 : Models.Schema.BaseObject, new()
			where T3 : Models.Schema.BaseObject, new()
		{
			if ( input != null && resultSet.RelatedResources.FirstOrDefault( m => m[ "RowId" ]?.ToString() == input.RowId.ToString() ) == null )
			{
				var mapped = MappingMethod( input, context );
				if( mapped != null )
				{
					resultSet.RelatedResources.Add( JObject.FromObject( mapped ) );
				}
			}
		}
		//

		#endregion

		#region Database connections
		/// <summary>
		/// Get the read only connection string for the main database
		/// </summary>
		/// <returns></returns>
		public static string DBConnectionRO()
        {

            string conn = WebConfigurationManager.ConnectionStrings["navy_RO"].ConnectionString;
            return conn;

        }
        public static string MainConnection()
        {
            string conn = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return conn;
        }
        #endregion

        //Automatic mapping based on property name + type
        public static T2 AutoMap<T1, T2> ( T1 source, T2 destination, List<string> errors = null, List<string> skipProperties = null, int maxDepth = 10 ) where T2: class
		{
			//Ensure source and destination are not null
			errors = errors ?? new List<string>();
			if( source == null || destination == null )
			{
				errors.Add( "Error: Source and Destination must not be null" );
				return null;
			}

			//Get a list of properties for the source and destination
			var sourceProperties = source.GetType().GetProperties().ToList();
			var destinationProperties = destination.GetType().GetProperties().Where( m => m.CanWrite ).ToList();

			//Setup helpers
			skipProperties = skipProperties ?? new List<string>();
			errors = errors ?? new List<string>();

			//For each property in the destination object...
			foreach( var destinationProperty in destinationProperties.Where( m => !skipProperties.Contains( m.Name ) ).ToList() )
			{
				try
				{
					//Find a matching property (based on name and type) in the source object
					var matchingSourceProperty = sourceProperties.FirstOrDefault( sourceProperty =>
						sourceProperty.Name.ToLower() == destinationProperty.Name.ToLower() &&
						( Nullable.GetUnderlyingType( sourceProperty.PropertyType ) ?? sourceProperty.PropertyType ) == ( Nullable.GetUnderlyingType( destinationProperty.PropertyType ) ?? destinationProperty.PropertyType )
					);
					if( matchingSourceProperty != null )
					{
						//Update the destination property's value to match the source property's value
						//Caution: For non-value types (including List<>s), this will set the value to reference the same object in RAM. It does not create a separate copy.
						//Note that .SetValue() will use the appropriate default value for the type in cases where the source value is nullable and the destination value is not (i.e., a nullable int with a null value will be translated to a 0)
						destinationProperty.SetValue( destination, matchingSourceProperty.GetValue( source ) );
					}
				}
				catch( Exception ex )
				{
					//If there are any errors, add them to the errors list
					//The errors list is a List<string> passed by reference so no need to make it a "ref" or "out" parameter
					errors.Add( "Error mapping property: " + destinationProperty.Name + ": " + ex.Message + ( ex.InnerException != null ? "; Inner exception: " + ex.InnerException.Message : "" ) );
				}
			}

			//Return the mapped resource
			return destination;
		}
		//

		//Automatic mapping via serialization
		//This is a little bit slower but guarantees that nothing is passed by reference
		//However it does not allow "skipping" properties
		public static bool AutoMapBySerialization<T1, T2>(T1 source, T2 destination, List<string> errors = null )
		{
			//Setup helpers
			errors = errors ?? new List<string>();
			var mappingWasSuccessful = true;

			try
			{
				//Brute-force deep-clone via serialization
				destination = JsonConvert.DeserializeObject<T2>( JsonConvert.SerializeObject( source ) );
			}
			catch ( Exception ex )
			{
				//If there are any errors, add them to the errors list
				//The errors list is a List<string> passed by reference so no need to make it a "ref" or "out" parameter
				errors.Add( "Error mapping data: " + ex.Message + ( ex.InnerException != null ? "; Inner exception: " + ex.InnerException.Message : "" ) );
				mappingWasSuccessful = false;
			}

			return mappingWasSuccessful;
		}


        #region data retrieval     
        public static string GetSanitizedSearchFilterKeywords( SearchQuery query )
        {
			var keywords = query.GetFilterTextByName( "search:Keyword", "" );
			return HandleApostrophes( keywords ).Trim();
        } 
		//

		public static SearchFilter GetSanitizedKeywordFilterOrNull( SearchQuery query )
		{
			var filter = query.GetFilterByName( "search:Keyword" );
			if ( filter != null )
			{
				filter.Text = HandleApostrophes( filter.Text ).Trim();
			}

			return filter;
		}
		//

        public static List<Guid> GetFunctionalAreas( string property, ref string workRoleList )
        {
            if ( string.IsNullOrEmpty( property ) )
                return null;
            var output = new List<Guid>();
            workRoleList = "";
            var pipe = "";
            //workRoles = new List<string>();
            string[] parts = property.Split( '|' );
            foreach ( var item in parts )
            {
                string[] part2 = property.Split( '~' );
                if ( part2.Length > 0 )
                {
                    workRoleList += pipe + part2[0].Trim();
                    pipe = "|";
                    //workRoles.Add( part2[0].Trim() );
                    if ( part2.Length == 2 )
                    {
                        if (IsValidGuid( part2[1] ) )
                            output.Add( new Guid( part2[1] ) );
                    }
                }
            }

            return output;
        }
		//

        public static Guid GetGuidType( DataRow dr, string property )
        {
            string guid2 = GetRowColumn( dr, property );
            string guid = dr[property].ToString();

            if ( !string.IsNullOrEmpty( guid ) && IsValidGuid( guid) )
                return new Guid( guid );
            else
                return new Guid();
        }
		//

        public static string GetRowColumn( DataRow row, string column, string defaultValue = "" )
        {
            string colValue = "";

            try
            {
                colValue = row[column].ToString();

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                if ( HasMessageBeenPreviouslySent( column ) == false )
                {
                    string queryString = GetWebUrl();
                    string exType = ex.GetType().ToString();
                    LoggingHelper.LogError( exType + " Exception in GetRowColumn( DataRow row, string column, string defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                }

                colValue = defaultValue;
            }
            return colValue;

        }

        public static string GetRowPossibleColumn( DataRow row, string column, string defaultValue = "" )
        {
            string colValue = "";

            try
            {
                colValue = row[column].ToString();

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {

                colValue = defaultValue;
            }
            return colValue;

        }

        /// <summary>
        /// Helper method to retrieve an int column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static int GetRowColumn( DataRow row, string column, int defaultValue )
        {
            int colValue = 0;

            try
            {
                colValue = Int32.Parse( row[column].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                if ( HasMessageBeenPreviouslySent( column ) == false )
                {
                    string queryString = GetWebUrl();
                    LoggingHelper.LogError( "Exception in GetRowColumn( DataRow row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                }


                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        }

        public static int GetRowPossibleColumn( DataRow row, string column, int defaultValue )
        {
            int colValue = 0;

            try
            {
                colValue = Int32.Parse( row[column].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        }
        public static decimal GetRowPossibleColumn( DataRow row, string column, decimal defaultValue )
        {
            decimal colValue = 0;

            try
            {
                colValue = decimal.Parse( row[column].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                //string queryString = GetWebUrl();

                //LoggingHelper.LogError( "Exception in GetRowColumn( DataRow row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        }
        public static bool GetRowColumn( DataRow row, string column, bool defaultValue )
        {
            bool colValue = false;

            try
            {
                //need to properly handle int values of 0,1, as bool
                string strValue = row[column].ToString();
                if ( !string.IsNullOrWhiteSpace( strValue ) && strValue.Trim().Length == 1 )
                {
                    strValue = strValue.Trim();
                    if ( strValue == "0" )
                        return false;
                    else if ( strValue == "1" )
                        return true;
                }
                colValue = bool.Parse( row[column].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                if ( HasMessageBeenPreviouslySent( column ) == false )
                {
                    string queryString = GetWebUrl();
                    LoggingHelper.LogError( "Exception in GetRowColumn( DataRow row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                }

                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        }
        public static DateTime GetRowColumn( DataRow row, string column, DateTime defaultValue )
        {
            DateTime colValue;

            try
            {
                string strValue = row[column].ToString();
                if ( DateTime.TryParse( strValue, out colValue ) == false )
                    colValue = defaultValue;
            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                if ( HasMessageBeenPreviouslySent( column ) == false )
                {
                    string queryString = GetWebUrl();
                    LoggingHelper.LogError( ex, " Exception in GetRowColumn( DataRow row, string column, string defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                }
                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        }
        public static bool HasMessageBeenPreviouslySent( string keyName )
        {

            string key = "missingColumn_" + keyName;
            //check cache for keyName
            if ( HttpRuntime.Cache[key] != null )
            {
                return true;
            }
            else
            {
                //not really much to store
                HttpRuntime.Cache.Insert( key, keyName );
            }

            return false;
        }
        protected static int GetField( int? field, int defaultValue = 0 )
        {
            int value = field != null ? ( int ) field : defaultValue;

            return value;
        } // end method
        protected static decimal GetField( decimal? field, decimal defaultValue = 0 )
        {
            decimal value = field != null ? ( decimal ) field : defaultValue;

            return value;
        } // end method
        protected static Guid GetField( Guid? field, Guid defaultValue )
        {
            Guid value = field != null ? ( Guid ) field : defaultValue;

            return value;
        } // end method

        protected static string GetMessages( List<string> messages )
        {
            if ( messages == null || messages.Count == 0 )
                return "";

            return string.Join( "<br/>", messages.ToArray() );

        }

        /// <summary>
        /// Split a comma separated list into a list of strings
        /// </summary>
        /// <param name="csl"></param>
        /// <returns></returns>
        public static List<string> CommaSeparatedListToStringList( string csl )
        {
            if ( string.IsNullOrWhiteSpace( csl ) )
                return new List<string>();

            try
            {
                return csl.Trim().Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Get the current url for reporting purposes
        /// </summary>
        /// <returns></returns>
        public static string GetWebUrl()
        {
            string queryString = "n/a";
            try
            {
                if ( HttpContext.Current != null && HttpContext.Current.Request != null )
                    queryString = HttpContext.Current.Request.RawUrl.ToString();
            }
            catch ( Exception ex )
            {
                //ignore
            }
            return queryString;
        }

        #endregion
        #region Dynamic Sql
        public static DataTable ReadTable( string tableViewName, string orderBy = "" )
        {
            // Table to store the query results
            DataTable table = new DataTable();
            if ( string.IsNullOrWhiteSpace( tableViewName ) )
                return table;
            if ( tableViewName.IndexOf( "[" ) == -1 )
                tableViewName = "[" + tableViewName.Trim() + "]";
            string sql = string.Format( "SELECT * FROM {0} ", tableViewName );
            if ( !string.IsNullOrWhiteSpace( orderBy ) )
                sql += " Order by " + orderBy;

            string connectionString = DBConnectionRO();
            // Creates a SQL connection
            using ( var connection = new SqlConnection( DBConnectionRO() ) )
            {
                connection.Open();

                // Creates a SQL command
                using ( var command = new SqlCommand( sql, connection ) )
                {
                    // Loads the query results into the table
                    table.Load( command.ExecuteReader() );
                }

                connection.Close();
            }

            return table;
        }
        public static DataTable ReadSql( string sql )
        {
            // Table to store the query results
            DataTable table = new DataTable();
            if ( string.IsNullOrWhiteSpace( sql ) )
                return table;

            string connectionString = DBConnectionRO();
            // Creates a SQL connection
            using ( var connection = new SqlConnection( DBConnectionRO() ) )
            {
                connection.Open();

                // Creates a SQL command
                using ( var command = new SqlCommand( sql, connection ) )
                {
                    // Loads the query results into the table
                    table.Load( command.ExecuteReader() );
                }

                connection.Close();
            }

            return table;
        } //


        /// <summary>
        /// Add an entry to the beginning of a Data Table
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="displayValue"></param>
        public static void AddEntryToTable( DataTable tbl, string displayValue )
        {
            DataRow r = tbl.NewRow();
            r[0] = displayValue;
            tbl.Rows.InsertAt( r, 0 );
        }

        /// <summary>
        /// Add an entry to the beginning of a Data Table. Uses a default key name of "id" and display column of "name"
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="keyValue"></param>
        /// <param name="displayValue"></param>
        public static void AddEntryToTable( DataTable tbl, int keyValue, string displayValue )
        {
            //DataRow r = tbl.NewRow();
            //r[ 0 ] = id;
            //r[ 1 ] = displayValue;
            //tbl.Rows.InsertAt( r, 0 );

            AddEntryToTable( tbl, keyValue, displayValue, "id", "name" );

        }

        /// <summary>
        /// Add an entry to the beginning of a Data Table. Uses a default key name of "id" and display column of "name"
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="keyValue"></param>
        /// <param name="displayValue"></param>
        /// <param name="keyName"></param>
        /// <param name="displayName"></param>
        public static void AddEntryToTable( DataTable tbl, int keyValue, string displayValue, string keyName, string displayName )
        {
            DataRow r = tbl.NewRow();
            r[keyName] = keyValue;
            r[displayName] = displayValue;
            tbl.Rows.InsertAt( r, 0 );

        }

        /// <summary>
        /// Add an entry to the beginning of a Data Table. Uses the provided key name and display column
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="keyValue"></param>
        /// <param name="displayValue"></param>
        /// <param name="keyName"></param>
        /// <param name="displayName"></param>
        public static void AddEntryToTable( DataTable tbl, string keyValue, string displayValue, string keyName, string displayName )
        {
            DataRow r = tbl.NewRow();
            r[keyName] = keyValue;
            r[displayName] = displayValue;
            tbl.Rows.InsertAt( r, 0 );

        }

        /// Check is dataset is valid and has at least one table with at least one row
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static bool DoesDataSetHaveRows( DataSet ds )
        {

            try
            {
                if ( ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 )
                    return true;
                else
                    return false;
            }
            catch
            {

                return false;
            }
        }//
        #endregion
   
        #region property validations, etc
        /// <summary>
        /// TBD - should an empty string be invalid, or should the caller handle?
        /// </summary>
        /// <param name="text"></param>
        /// <param name="propertyName"></param>
        /// <param name="minimumLength"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static bool ValidateText( string text, string propertyName, int minimumLength, ref List<string> messages )
        {
            bool isValid = true;
            if ( minimumLength > 0 && string.IsNullOrWhiteSpace( text ) )
            {
                messages.Add( string.Format( "A {0} must be entered.", propertyName ) );
                return false;
            }
            else if ( text.Length < minimumLength )
            {
                messages.Add( string.Format( "The {0} must be at least {1} characters.", propertyName, minimumLength ) );
                return false;
            }

            return isValid;
        }

        public static string HandleApostrophes( string strValue )
        {
            if ( string.IsNullOrWhiteSpace( strValue ) )
                return "";

            if ( strValue.IndexOf( "'" ) > -1 )
            {
                strValue = strValue.Replace( "'", "''" );
            }
            //handle where extras were added
            if ( strValue.IndexOf( "''''" ) > -1 )
            {
                strValue = strValue.Replace( "''''", "''" );
            }

            return strValue;
        }

        public static bool IsValidDate( DateTime date )
        {
            if ( date != null && date > new DateTime( 1492, 1, 1 ) )
                return true;
            else
                return false;
        }

        public static bool IsValidDate( DateTime? date )
        {
            if ( date != null && date > new DateTime( 1492, 1, 1 ) )
                return true;
            else
                return false;
        }

        public static bool IsValidDate( string date )
        {
            DateTime validDate;
            if ( string.IsNullOrWhiteSpace( date ) || date.Length < 8 )
                return false;

            if ( !string.IsNullOrWhiteSpace( date )
                && DateTime.TryParse( date, out validDate )
                && date.Length >= 8
                && validDate > new DateTime( 1492, 1, 1 )
                )
                return true;
            else
                return false;
        }

        public static bool IsInteger( string nbr )
        {
            int validNbr = 0;
            if ( !string.IsNullOrWhiteSpace( nbr ) && int.TryParse( nbr, out validNbr ) )
                return true;
            else
                return false;
        }

        public static bool IsDecimal( string nbr, ref decimal validNbr )
        {
            validNbr = 0;
            if ( !string.IsNullOrWhiteSpace( nbr ) && decimal.TryParse( nbr, out validNbr ) )
                return true;
            else
                return false;
        }

        public static bool IsValid( string nbr )
        {
            int validNbr = 0;
            if ( !string.IsNullOrWhiteSpace( nbr ) && int.TryParse( nbr, out validNbr ) )
                return true;
            else
                return false;
        }

        public static bool IsValidCtid( string ctid )
        {
            List<string> messages = new List<string>();

            return IsValidCtid( ctid, ref messages );
        }

        public static bool IsValidCtid( string ctid, ref List<string> messages, bool isRequired = false, bool skippingErrorMessages = true )
        {
            bool isValid = true;

            if ( string.IsNullOrWhiteSpace( ctid ) )
            {
                if ( isRequired )
                {
                    messages.Add( "Error - A valid CTID property must be entered." );
                }
                return false;
            }

            ctid = ctid.ToLower().Trim();
            if ( ctid.Length != 39 )
            {
                if ( !skippingErrorMessages )
                    messages.Add( "Error - Invalid CTID format. The proper format is ce-UUID. ex. ce-84365aea-57a5-4b5a-8c1c-eae95d7a8c9b" );
                return false;
            }

            if ( !ctid.StartsWith( "ce-" ) )
            {
                if ( !skippingErrorMessages )
                    messages.Add( "Error - The CTID property must begin with ce-" );
                return false;
            }
            //now we have the proper length and format, the remainder must be a valid guid
            if ( !IsValidGuid( ctid.Substring( 3, 36 ) ) )
            {
                if ( !skippingErrorMessages )
                    messages.Add( "Error - Invalid CTID format. The proper format is ce-UUID. ex. ce-84365aea-57a5-4b5a-8c1c-eae95d7a8c9b" );
                return false;
            }

            return isValid;
        }

        public static bool IsValidGuid( Guid field )
        {
            if ( ( field == null || field.ToString() == Guid.Empty.ToString() ) )
                return false;
            else
                return true;
        }

        protected bool IsValidGuid( Guid? field )
        {
            if ( ( field == null || field.ToString() == Guid.Empty.ToString() ) )
                return false;
            else
                return true;
        }

        public static bool IsValidGuid( string field )
        {
            if ( string.IsNullOrWhiteSpace( field )
                || field.Trim() == Guid.Empty.ToString()
                || field.Length != 36
                )
                return false;
            else
                return true;
        }

        public static bool IsGuidValid( Guid field )
        {
            if ( ( field == null || field.ToString() == Guid.Empty.ToString() ) )
                return false;
            else
                return true;
        }

        public static bool IsGuidValid( Guid? field )
        {
            if ( ( field == null || field.ToString() == Guid.Empty.ToString() ) )
                return false;
            else
                return true;
        }

        public static string AssignWithoutOverwriting( string input, string currentValue, bool doesEntityExist )
        {
            string value = "";
            if ( doesEntityExist )
            {
                //don't allow setting current to empty if not currently empty
                value = string.IsNullOrWhiteSpace( input ) ? currentValue : input;
            }
            else if ( !string.IsNullOrWhiteSpace( input ) )
            {
                //don't allow delete for initial
                value = input;
            }
            return value;
        }

        public static bool? AssignWithoutOverwriting( bool input, bool? currentValue, bool doesEntityExist )
        {
            bool? value = false;
            if ( doesEntityExist )
            {
                //don't allow setting current to empty if not currently empty
                if ( currentValue == null )
                    value = input;
                else
                {
                    value = input;
                }
            }
            else
            {
                value = input;
            }
            return value;
        }
        public static string GetData( string text, string defaultValue = "" )
        {
            if ( string.IsNullOrWhiteSpace( text ) == false )
                return text.Trim();
            else
                return defaultValue;
        }

        public static int? SetData( int value, int minValue )
        {
            if ( value >= minValue )
                return value;
            else
                return null;
        }

        public static decimal? SetData( decimal value, decimal minValue )
        {
            if ( value >= minValue )
                return value;
            else
                return null;
        }

        public static DateTime? SetDate( string value )
        {
            DateTime output;
            if ( DateTime.TryParse( value, out output ) )
                return output;
            else
                return null;
        }

        #region URL validation
        public static string NormalizeUrlData( string text, string defaultValue = "" )
        {
            if ( string.IsNullOrWhiteSpace( text ) == false )
            {
                text = text.TrimEnd( '/' );
                return text.Trim();
            }
            else
                return defaultValue;
        }

        public static bool IsUrlValid( string url, ref string statusMessage, bool doingExistanceCheck = true )
        {
            statusMessage = "";
            if ( string.IsNullOrWhiteSpace( url ) )
                return true;

            url = url.Trim();
            if ( !Uri.IsWellFormedUriString( url, UriKind.Absolute ) )
            {
                statusMessage = "The URL is not in a proper format (for example, must begin with http or https).";
                return false;
            }

            //may need to allow ftp, and others - not likely for this context?
            if ( url.ToLower().StartsWith( "http" ) == false )
            {
                statusMessage = "A URL must begin with http or https";
                return false;
            }
            //hack for pattern like https://https://www.sscc.edu
            if ( url.LastIndexOf( "//" ) > url.IndexOf( "//" ) )
            {
                statusMessage = "Invalid format, contains multiple sets of '//'";
                return false;
            }

            if ( !doingExistanceCheck )
                return true;

            bool isaImageUrl = false;
            if ( ( url.IndexOf( "//example." ) > 0 || url.IndexOf( "//www.example." ) > 0 ) )
            {
                if ( UtilityManager.GetAppKeyValue( "environment" ) == "production" )
                {
                    statusMessage = "Urls using example.org, etc are not allowed in this environment.";
                    return false;
                }
                else
                    return true;
            }
            var isOk = DoesRemoteFileExist( url, ref statusMessage, ref isaImageUrl );
            //optionally try other methods, or again with GET
            if ( !isOk && statusMessage == "999" )
                return true;

            if ( isOk & isaImageUrl )
            {
                statusMessage = " This property should not contain an image URL ";
                return false;
            }

            return isOk;
        }

        /// <summary>
        /// Checks the file exists or not.
        /// NOTE: **** keep method in sync with method in assistant api - ServiceHelperV2 ****
        /// </summary>
        /// <param name="url">The URL of the remote file.</param>
        /// <returns>True : If the file exits, False if file not exists</returns>
        public static bool DoesRemoteFileExist( string url, ref string responseStatus, ref bool isaimageurl )
        {
            //if true skip link checking for all - usually for a temporary reason
            if ( UtilityManager.GetAppKeyValue( "skippingLinkChecking", false ) )
            {
                LoggingHelper.DoTrace( 6, string.Format( "BaseFactory.DoesRemoteFileExist skippingLinkChecking for " + url ) );

                return true;
            }
            //the following is only used to handle certain errors. 
            //we may want a different property to allow publishing to skip completely (for speed, and already checked)
            bool treatingRemoteFileNotExistingAsError = UtilityManager.GetAppKeyValue( "treatingRemoteFileNotExistingAsError", true );
            //consider stripping off https?
            //or if not found and https, try http
            try
            {
                if ( SkippingValidation( url ) )
                    return true;
                //note there could be a cap on calls to safeBrowsing - have noted sporadic failures.
                //Perhaps should skip where in an apparant batch process!
                //SafeBrowsing.Reputation rep = SafeBrowsing.CheckUrl( url );
                //if ( rep != SafeBrowsing.Reputation.None )
                //{
                //    responseStatus = string.Format( "Url ({0}) failed SafeBrowsing check.", url );
                //    return false;
                //}
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create( url ) as HttpWebRequest;
                //NOTE - do use the HEAD option, as many sites reject that type of request
                //request.Method = "GET";
                //var agent = HttpContext.Current.Request.AcceptTypes;

                //request.ContentType = "text/html;charset=\"utf-8\";image/*";
                //testing:
                request.AllowAutoRedirect = true;
                request.Timeout = 10000;  //10 seconds
                request.KeepAlive = false;
                request.Accept = "*/*";
                //
                request.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1309.0 Safari/537.17";

                //users may be providing urls to sites that have invalid ssl certs installed.You can ignore those cert problems if you put this line in before you make the actual web request:
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback( AcceptAllCertifications );

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if ( response != null )
                    if ( response.ContentType.ToLower( CultureInfo.InvariantCulture ).Contains( "image/" ) )
                    {
                        isaimageurl = true;
                    }
                //Returns TRUE if the Status code == 200
                response.Close();
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    //if https, try again with http (don't do both, as would get loop). Perhaps should have the check start with http to https?
                    if ( url.ToLower().StartsWith( "https:" ) )
                    {
                        url = url.ToLower().Replace( "https:", "http:" );
                        LoggingHelper.DoTrace( 5, string.Format( "_____________Failed for https, trying again using http: {0}", url ) );

                        return DoesRemoteFileExist( url, ref responseStatus, ref isaimageurl );
                    }
                    else
                    {
                        var urlStatusCode = ( int ) response.StatusCode;
                        if ( urlStatusCode == 301 )
                        {
                            //for ( int i = 0; i < response.Headers.Count; ++i )
                            //	Console.WriteLine( "\nHeader Name:{0}, Value :{1}", response.Headers.Keys[ i ], response.Headers[ i ] );
                            string location = response.Headers.GetValues( "Location" ).FirstOrDefault();
                            if ( !string.IsNullOrWhiteSpace( location ) )
                            {
                                string clearUrl = url.Replace( "http://", "" ).Replace( "https://", "" ).Trim( '/' );
                                string clearLoc = location.Replace( "http://", "" ).Replace( "https://", "" ).Trim( '/' );
                                //L: http://www.tesu.edu/about/mission
                                //U: http://www.tesu.edu/about/mission.cfm
                                if ( location.Replace( "https", "http" ).Trim( '/' ) == url.Trim( '/' )
                                    || location.ToLower().Trim( '/' ) == url.ToLower().Trim( '/' )
                                    || url.ToLower().IndexOf( location.ToLower() ) == 0 //redirect just trims an extension
                                    || clearLoc.ToLower().IndexOf( clearUrl.ToLower() ) > 0
                                    )
                                {
                                    return true;
                                }
                                else if ( location.Replace( "mobile.twitter", "twitter" ).ToLower().Trim( '/' ) == url.ToLower().Trim( '/' )
                                    || location == "https://www.linkedin.com/error_pages/unsupported-browser.html"
                                    )
                                {
                                    //Redirect to: https://www.linkedin.com/error_pages/unsupported-browser.html
                                    return true;
                                }

                            }

                        }
                        LoggingHelper.DoTrace( 5, string.Format( "Url validation failed for: {0}, using method: GET, with status of: {1}", url, response.StatusCode ) );
                    }
                }


                responseStatus = response.StatusCode.ToString();

                return ( response.StatusCode == HttpStatusCode.OK );
                //apparantly sites like Linked In have can be a  problem
                //http://stackoverflow.com/questions/27231113/999-error-code-on-head-request-to-linkedin
                //may add code to skip linked In?, or allow on fail - which the same.
                //or some update, refer to the latter link

                //
            }
            catch ( WebException wex )
            {
                responseStatus = wex.Message;
                //
                if ( wex.Message.IndexOf( "(404)" ) > 1 )
                    return false;
                else if ( wex.Message.IndexOf( "Too many automatic redirections were attempted" ) > -1 )
                    return false;
                else if ( wex.Message.IndexOf( "(999" ) > 1 )
                    return true;
                else if ( wex.Message.IndexOf( "(400) Bad Request" ) > 1 )
                    return false;//this could be valid as in twitter
                else if ( wex.Message.IndexOf( "(401) Unauthorized" ) > 1 )
                    return true;
                else if ( wex.Message.IndexOf( "(403) Forbidden" ) > 1 )
                    return true;
                else if ( wex.Message.IndexOf( "(406) Not Acceptable" ) > 1 )
                    return true;
                else if ( wex.Message.IndexOf( "The operation has timed out" ) > -1 )
                {
                    //allow this as a 404 would not have timed out?
                    return true;
                }
                else if ( wex.Message.IndexOf( "(500) Internal Server Error" ) > 1 )
                    return true; //why is this true
                else if ( wex.Message.IndexOf( "Could not create SSL/TLS secure channel" ) > 1 )
                {
                    //https://www.naahq.org/education-careers/credentials/certification-for-apartment-maintenance-technicians 
                    return true;
                }
                else if ( wex.Message.IndexOf( "Could not establish trust relationship for the SSL/TLS secure channel" ) > -1 )
                {
                    return true;
                }
                else if ( wex.Message.IndexOf( "The underlying connection was closed: An unexpected error occurred on a send" ) > -1 )
                {
                    return true;
                }

                else if ( wex.Message.IndexOf( "The connection was closed unexpectedly" ) > -1 )
                {
                    //not sure if this should be an error?
                    if ( wex.Message == "The request was aborted: The connection was closed unexpectedly." )
                        return false;
                    else
                        return true;
                }
                else if ( wex.Message.IndexOf( "Detail=CR must be followed by LF" ) > 1 )
                {
                    return true;
                }
                //var pageContent = new StreamReader( wex.Response.GetResponseStream() )
                //		 .ReadToEnd();
                if ( !treatingRemoteFileNotExistingAsError )
                {
                    LoggingHelper.LogError( string.Format( "BaseFactory.DoesRemoteFileExists url: {0}. Exception Message:{1}; URL: {2}", url, wex.Message, GetWebUrl() ), true, "SKIPPING - Exception on URL Checking" );

                    return true;
                }

                LoggingHelper.DoTrace( 1, string.Format( "BaseFactory.DoesRemoteFileExists url: {0}. Exception Message:{1}", url, wex.Message ) );
                //LoggingHelper.LogError( string.Format( "BaseFactory.DoesRemoteFileExists url: {0}. Exception Message:{1}", url, wex.Message ), false, "Exception on URL Checking" );                responseStatus = wex.Message;
                return false;
            }
            catch ( Exception ex )
            {
                responseStatus = ex.Message;

                if ( ex.Message.IndexOf( "(999" ) > -1 )
                {
                    //linked in scenario
                    responseStatus = "999";
                    return true;

                }
                else if ( ex.Message.IndexOf( "Could not create SSL/TLS secure channel" ) > 1 )
                {
                    //https://www.naahq.org/education-careers/credentials/certification-for-apartment-maintenance-technicians 
                    return true;

                }
                else if ( ex.Message.IndexOf( "(500) Internal Server Error" ) > 1 )
                {
                    return false;
                }
                else if ( ex.Message.IndexOf( "(401) Unauthorized" ) > 1 )
                {
                    return true;
                }
                else if ( ex.Message.IndexOf( "Could not establish trust relationship for the SSL/TLS secure channel" ) > 1 )
                {
                    return true;
                }
                else if ( ex.Message.IndexOf( "Detail=CR must be followed by LF" ) > 1 )
                {
                    return true;
                }
                if ( !treatingRemoteFileNotExistingAsError )
                {
                    LoggingHelper.LogError( string.Format( "BaseFactory.DoesRemoteFileExists url: {0}. Exception Message:{1}", url, ex.Message ), true, "SKIPPING - Exception on URL Checking" );

                    return true;
                }

                LoggingHelper.DoTrace( 1, string.Format( "BaseFactory.DoesRemoteFileExists url: {0}. Exception Message:{1}", url, ex.Message ) );
                //LoggingHelper.LogError( string.Format( "BaseFactory.DoesRemoteFileExists url: {0}. Exception Message:{1}", url, ex.Message ), false, "Exception on URL Checking" );                //Any exception will returns false.
                return false;
            }
        }
        public static bool AcceptAllCertifications( object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors )
        {
            return true;
        }
        private static bool SkippingValidation( string url )
        {


            Uri myUri = new Uri( url );
            string host = myUri.Host;

            string exceptions = UtilityManager.GetAppKeyValue( "urlExceptions" );
            //quick method to avoid loop
            if ( exceptions.IndexOf( host ) > -1 )
            {
                LoggingHelper.DoTrace( 6, string.Format( "BaseFactory.SkippingValidation for " + url ) );
                return true;
            }


            //string[] domains = exceptions.Split( ';' );
            //foreach ( string item in domains )
            //{
            //	if ( url.ToLower().IndexOf( item.Trim() ) > 5 )
            //		return true;
            //}

            return false;
        }
        #endregion

        #region Email validation 
        public static bool IsValidEmail( string email, ref string statusMessage )
        {
            bool isValid = true;
            statusMessage = "";

            if ( string.IsNullOrWhiteSpace( email ) )
                return false;
            email = email.Trim();
            if ( email == "undefined" )
            {
                statusMessage = string.Format( "The email address ({0}) is invalid (because it is undefined).", email );
                return false;
            }
            int atPos = email.IndexOf( "@" );
            if ( atPos == -1 )
                statusMessage = string.Format( "The email address ({0}) is invalid, it doesn't contain an '@' symbol.", email );
            else if ( email.IndexOf( ".", atPos ) == -1 )
                statusMessage = string.Format( "The email address ({0}) is invalid, it doesn't contain a period after the '@' symbol.", email );

            if ( !string.IsNullOrWhiteSpace( statusMessage ) )
                return false;

          
            try
            {
                if ( !Regex.IsMatch( email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds( 250 ) ) )
                {
                    statusMessage = string.Format( "The email address ({0}) has an invalid format.", email );
                }
            }
            catch ( RegexMatchTimeoutException e )
            {
                statusMessage = e.Message;
                return false;
            }

            if ( string.IsNullOrWhiteSpace( statusMessage ) )
            {
                //try this anyway 
                return ValidateEmailAddress( email, ref statusMessage );
            }
            else
                return false;
        }
        public static bool ValidateEmailAddress( string address, ref string status )
        {
            bool valid = true;
            status = "";

            EmailValidationResult validationResult = new EmailValidationResult();
            address = address.Trim();
            try
            {
                var publicKey = System.Configuration.ConfigurationManager.AppSettings["MailgunPublicAPIKey"];
                var publicKeyEncoded = Convert.ToBase64String( UTF8Encoding.UTF8.GetBytes( "api" + ":" + publicKey ) );
                var url = "https://api.mailgun.net/v3/address/validate";
                var data = "";

                using ( var client = new HttpClient() )
                {
                    //21-01-15 - this generally is not very effective. Allowed: test, test@test (return valid=false, but no reason) 
                    //			- it did reject email with two @ symbols (which is also caught be the Regex.IsMatch in the previous method);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Basic", publicKeyEncoded );
                    var result = client.GetAsync( url + "?address=" + Uri.EscapeUriString( address ) ).Result;
                    valid = result.IsSuccessStatusCode;
                    status = valid ? "" : result.ReasonPhrase;
                    data = result.Content.ReadAsStringAsync().Result;
                }
                if ( status == "Forbidden" )
                {
                    //probably means was found, but system prevents checking
                    status = "";
                    return true;
                }
                else
                {
                    validationResult = JsonConvert.DeserializeObject<EmailValidationResult>( data );

                    if ( !validationResult.Valid )
                    {
                        //skip error if a proxy email
                        //Actually probably should not allow in API, unless in sandbox
                        if ( !IsProduction() && address.IndexOf( "||" ) > 2 && address.IndexOf( "@" ) > address.IndexOf( "||" ) )
                        {
                            valid = true;
                        }
                        else
                        {
                            valid = false;
                            status = string.IsNullOrWhiteSpace( validationResult.Suggestion ) ? "Email is not formatted correctly." : validationResult.Suggestion;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                status = "Error encountered attempting to validate the email address: " + ex.Message;
                valid = false;
            }

            return valid;
        }
        //

        #endregion

        #endregion

        public static string FormatLongLabel( string text, int maxLength = 75 )
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";
            if ( text.Length > maxLength )
                return text.Substring( 0, maxLength ) + " ...";
            else
                return text;
        }
        public static string ConvertSpecialCharacters( string text )
        {
            bool hasChanged = false;

            return ConvertSpecialCharacters( text, ref hasChanged );
        }
        /// <summary>
        /// Convert characters often resulting from external programs like Word
        /// NOTE: keep in sync with the RegistryAssistant Api version
        /// </summary>
        /// <param name="text"></param>
        /// <param name="hasChanged"></param>
        /// <returns></returns>
        public static string ConvertSpecialCharacters( string text, ref bool hasChanged )
        {
            hasChanged = false;
            if ( string.IsNullOrWhiteSpace( text ) )
                return "";
            string orginal = text.Trim();
            if ( ContainsUnicodeCharacter( text ) )
            {

                if ( text.IndexOf( '\u2013' ) > -1 ) text = text.Replace( '\u2013', '-' ); // en dash
                if ( text.IndexOf( '\u2014' ) > -1 ) text = text.Replace( '\u2014', '-' ); // em dash
                if ( text.IndexOf( '\u2015' ) > -1 ) text = text.Replace( '\u2015', '-' ); // horizontal bar
                if ( text.IndexOf( '\u2017' ) > -1 ) text = text.Replace( '\u2017', '_' ); // double low line
                if ( text.IndexOf( '\u2018' ) > -1 ) text = text.Replace( '\u2018', '\'' ); // left single quotation mark
                if ( text.IndexOf( '\u2019' ) > -1 ) text = text.Replace( '\u2019', '\'' ); // right single quotation mark
                if ( text.IndexOf( '\u201a' ) > -1 ) text = text.Replace( '\u201a', ',' ); // single low-9 quotation mark
                if ( text.IndexOf( '\u201b' ) > -1 ) text = text.Replace( '\u201b', '\'' ); // single high-reversed-9 quotation mark
                if ( text.IndexOf( '\u201c' ) > -1 ) text = text.Replace( '\u201c', '\"' ); // left double quotation mark
                if ( text.IndexOf( '\u201d' ) > -1 ) text = text.Replace( '\u201d', '\"' ); // right double quotation mark
                if ( text.IndexOf( '\u201e' ) > -1 ) text = text.Replace( '\u201e', '\"' ); // double low-9 quotation mark
                if ( text.IndexOf( '\u201f' ) > -1 ) text = text.Replace( '\u201f', '\"' ); // ???
                if ( text.IndexOf( '\u2026' ) > -1 ) text = text.Replace( "\u2026", "..." ); // horizontal ellipsis
                if ( text.IndexOf( '\u2032' ) > -1 ) text = text.Replace( '\u2032', '\'' ); // prime
                if ( text.IndexOf( '\u2033' ) > -1 ) text = text.Replace( '\u2033', '\"' ); // double prime
                if ( text.IndexOf( '\u2036' ) > -1 ) text = text.Replace( '\u2036', '\"' ); // ??
                if ( text.IndexOf( '\u0090' ) > -1 ) text = text.Replace( '\u0090', 'ê' ); // e circumflex
            }
            //Ã¢â‚¬Å“softÃ¢â‚¬Â


            text = text.Replace( "â€™", "'" );
            text = text.Replace( "â€\"", "-" );
            text = text.Replace( "\"â€ú", "-" );
            text = text.Replace( "â€¢", "-" );
            text = text.Replace( "Ã¢â‚¬Â¢", "-" );
            text = text.Replace( "ÃƒÂ¢Ã¢,Â¬Ã¢\"Â¢s", "'s" );
            text = text.Replace( "Ãƒ,Ã,Â ", " " );
            //20-06-30 new to check ===================
            //further below this is just set to empty?
            //text = text.Replace( "-Ã‚Â­", "-" );
            //Ã¢â‚¬â€œ
            //this doesn't work
            //text = text.Replace( " Ã¢â‚¬â€œ ­", " - " );
            //so doing parts
            if ( text.IndexOf( "Ã¢â" ) > -1 )
            {
                //try regex
                text = Regex.Replace( text, "Ã¢â‚¬â€œ", "-" );
                //
                text = text.Replace( "Ã¢â", "" );
                text = text.Replace( "¬", " - " );
                text = text.Replace( "â€œ", "" );
            }
            // ========================================
            //
            //don't do this as \r is valid
            //text = text.Replace( "\\\\r", "" );

            text = text.Replace( "\u009d", " " ); //
            text = text.Replace( "Ã,Â", "" ); //
            text = text.Replace( ".Â", " " ); //

            text = Regex.Replace( text, "’", "'" );
            text = Regex.Replace( text, "“", "'" );
            text = Regex.Replace( text, "”", "'" );
            //BIZARRE
            text = Regex.Replace( text, "Ã¢â,¬â\"¢", "'" );
            text = Regex.Replace( text, "–", "-" );

            text = Regex.Replace( text, "[Õ]", "'" );
            text = Regex.Replace( text, "[Ô]", "'" );
            text = Regex.Replace( text, "[Ò]", "\"" );
            text = Regex.Replace( text, "[Ó]", "\"" );
            text = Regex.Replace( text, "[Ñ]", " -" ); //Ñ
            text = Regex.Replace( text, "[Ž]", "é" );
            text = Regex.Replace( text, "[ˆ]", "à" );
            text = Regex.Replace( text, "[Ð]", "-" );
            //
            text = text.Replace( "‡", "á" ); //Ã³

            text = text.Replace( "ÃƒÂ³", "ó" ); //
            text = text.Replace( "Ã³", "ó" ); //
                                              //é
            text = text.Replace( "ÃƒÂ©", "é" ); //
            text = text.Replace( "Ã©", "é" ); //

            text = text.Replace( "ÃƒÂ¡", "á" ); //
            text = text.Replace( "Ã¡", "á" ); //Ã¡
            text = text.Replace( "ÃƒÂ", "à" ); //
                                               //
            text = text.Replace( "ÃƒÂ±", "ñ" ); //
            text = text.Replace( "Â±", "ñ" ); //"Ã±"
                                              //
            text = text.Replace( "ÃƒÂ-", "í" ); //???? same as à
            text = text.Replace( "ÃƒÂ­­", "í" ); //"Ã­as" "gÃ­a" "gÃ­as"
            text = text.Replace( "gÃ­as", "gías" ); //"Ã­as" "gÃ­a" "gÃ­as"
            text = text.Replace( "’", "í" ); //


            text = text.Replace( "ÃƒÂº", "ú" ); //"Ãº"
            text = text.Replace( "Âº", "ú" ); //"Ãº"
            text = text.Replace( "œ", "ú" ); //

            text = text.Replace( "quÕˆ", "qu'à" ); //
            text = text.Replace( "qu'ˆ", "qu'à" ); //
            text = text.Replace( "ci—n ", "ción " );
            //"Â¨"
            text = text.Replace( "Â¨", "®" ); //

            text = text.Replace( "teor'as", "teorías" ); // 
            text = text.Replace( "log'as", "logías" ); //
            text = text.Replace( "ense–anza", "enseñanza" ); //
                                                             //
            text = text.Replace( "Ã¢â,¬Ãº", "\"" ); //
            text = text.Replace( "Ã¢â,¬Â", "\"" ); //
                                                   //
            text = text.Replace( ", - Å\"soft, -Â", "\"soft\"" ); //

            //not sure if should do this arbitrarily here?
            if ( text.IndexOf( "Ã" ) > -1 || text.IndexOf( "Â" ) > -1 )
            {
                //string queryString = GetWebUrl();
                //LoggingHelper.DoTrace( 1, string.Format("@#@#@# found text containing Ã or Â, setting to blank. URL: {0}, Text:\r{1}", queryString, text ) );
                text = text.Replace( "Ã", "" ); //
                text = text.Replace( ",Â", "," ); //
                text = text.Replace( "Â", "" ); //

            }


            text = text.Replace( "ou�ll", "ou'll" ); //
            text = text.Replace( "�s", "'s" ); // 
            text = text.Replace( "�", "" ); // 
            text = Regex.Replace( text, "[—]", "-" ); //

            text = Regex.Replace( text, "[�]", " " ); //could be anything
                                                      //covered above
                                                      //text = Regex.Replace(text, "[«»\u201C\u201D\u201E\u201F\u2033\u2036]", "\"");
                                                      //text = Regex.Replace(text, "[\u2026]", "...");

            //

            if ( orginal != text.Trim() )
            {
                //should report any changes
                hasChanged = true;
                //text = orginal;
            }
            return text.Trim();
        } //

        public static bool ContainsUnicodeCharacter( string input )
        {
            const int MaxAnsiCode = 255;

            return input.Any( c => c > MaxAnsiCode );
        }
        public static void ListUnicodeCharacter( string input )
        {
            if ( !ContainsUnicodeCharacter( input ) )
                return;

            //string chg = Regex.Match(input, @"[^\u0000-\u007F]", "");

        }
        /// <summary>
        /// Format a title (such as for a library) to be url friendly
        /// NOTE: there are other methods:
        /// ILPathways.Utilities.UtilityManager.UrlFriendlyTitle()
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FormatFriendlyTitle( string text )
        {
            if ( text == null || text.Trim().Length == 0 )
                return "";

            string title = UrlFriendlyTitle( text );

            //encode just incase
            title = HttpUtility.HtmlEncode( title );
            return title;
        }
        /// <summary>
        /// Format a title (such as for a library) to be url friendly
        /// NOTE: there are other methods:
        /// ILPathways.Utilities.UtilityManager.UrlFriendlyTitle()
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private static string UrlFriendlyTitle( string title )
        {
            if ( title == null || title.Trim().Length == 0 )
                return "";

            title = title.Trim();

            string encodedTitle = title.Replace( " - ", "-" );
            encodedTitle = encodedTitle.Replace( " ", "_" );

            //for now allow embedded periods
            //encodedTitle = encodedTitle.Replace( ".", "-" );

            encodedTitle = encodedTitle.Replace( "'", "" );
            encodedTitle = encodedTitle.Replace( "&", "-" );
            encodedTitle = encodedTitle.Replace( "#", "" );
            encodedTitle = encodedTitle.Replace( "$", "S" );
            encodedTitle = encodedTitle.Replace( "%", "percent" );
            encodedTitle = encodedTitle.Replace( "^", "" );
            encodedTitle = encodedTitle.Replace( "*", "" );
            encodedTitle = encodedTitle.Replace( "+", "_" );
            encodedTitle = encodedTitle.Replace( "~", "_" );
            encodedTitle = encodedTitle.Replace( "`", "_" );
            encodedTitle = encodedTitle.Replace( "/", "_" );
            encodedTitle = encodedTitle.Replace( "://", "/" );
            encodedTitle = encodedTitle.Replace( ":", "" );
            encodedTitle = encodedTitle.Replace( ";", "" );
            encodedTitle = encodedTitle.Replace( "?", "" );
            encodedTitle = encodedTitle.Replace( "\"", "_" );
            encodedTitle = encodedTitle.Replace( "\\", "_" );
            encodedTitle = encodedTitle.Replace( "<", "_" );
            encodedTitle = encodedTitle.Replace( ">", "_" );
            encodedTitle = encodedTitle.Replace( "__", "_" );
            encodedTitle = encodedTitle.Replace( "__", "_" );
            encodedTitle = encodedTitle.Replace( "..", "_" );
            encodedTitle = encodedTitle.Replace( ".", "_" );

            if ( encodedTitle.EndsWith( "." ) )
                encodedTitle = encodedTitle.Substring( 0, encodedTitle.Length - 1 );

            return encodedTitle;
        } //
        public static string GenerateFriendlyName( string name )
        {
            if ( name == null || name.Trim().Length == 0 )
                return "";
            //another option could be use a pattern like the following?
            //string phrase = string.Format( "{0}-{1}", Id, name );

            string str = RemoveAccent( name ).ToLower();
            // invalid chars           
            str = Regex.Replace( str, @"[^a-z0-9\s-]", "" );
            // convert multiple spaces into one space   
            str = Regex.Replace( str, @"\s+", " " ).Trim();
            // cut and trim 
            str = str.Substring( 0, str.Length <= 45 ? str.Length : 45 ).Trim();
            str = Regex.Replace( str, @"\s", "-" ); // hyphens   
            return str;
        }
        private static string RemoveAccent( string text )
        {
            byte[] bytes = System.Text.Encoding.GetEncoding( "Cyrillic" ).GetBytes( text );
            return System.Text.Encoding.ASCII.GetString( bytes );
        }
        protected string HandleDBValidationError( System.Data.Entity.Validation.DbEntityValidationException dbex, string source, string title )
        {
            string message = string.Format( "{0} DbEntityValidationException, Name: {1}", source, title );

            foreach ( var eve in dbex.EntityValidationErrors )
            {
                message += string.Format( "\rEntity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    eve.Entry.Entity.GetType().Name, eve.Entry.State );
                foreach ( var ve in eve.ValidationErrors )
                {
                    message += string.Format( "- Property: \"{0}\", Error: \"{1}\"",
                        ve.PropertyName, ve.ErrorMessage );
                }

                LoggingHelper.LogError( message, true );
            }

            return message;
        }

        public static string FormatExceptions( Exception ex )
        {
            string message = ex.Message;

            if ( ex.InnerException != null )
            {
                message += FormatExceptions( ex.InnerException );
            }

            return message;
        }

        /// <summary>
        /// Strip off text that is randomly added that starts with jquery
        /// Will need additional check for numbers - determine actual format
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string StripJqueryTag( string text )
        {
            int pos2 = text.ToLower().IndexOf( "jquery" );
            if ( pos2 > 1 )
            {
                text = text.Substring( 0, pos2 );
            }

            return text;
        }

        public static JsonSerializerSettings GetJsonSettings()
        {
            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            return settings;
        }
    }

    public class EmailValidationResult
    {
        [JsonProperty( "address" )]
        public string Address { get; set; }
        [JsonProperty( "is_valid" )]
        public bool Valid { get; set; }
        [JsonProperty( "did_you_mean" )]
        public string Suggestion { get; set; }
    }
    //

}
