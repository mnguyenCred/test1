using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Application;
using Models.Curation;

using AppEntity = Models.Schema.Concept;
using DBEntity = Data.Tables.ConceptScheme_Concept;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Navy.Utilities;
using Models.Search;

namespace Factories
{
	public class ConceptManager : BaseFactory
	{
		public static new string thisClassName = "ConceptScheme";
		//

		#region Persistence
		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			SaveCore( entity, userID, "Edit", errors.Add );
		}
		//

		private static void SaveCore( AppEntity entity, int userID, string saveType, Action<string> AddErrorMethod )
		{
			using ( var context = new DataEntities() )
			{
				BasicSaveCore( context, entity, context.ConceptScheme_Concept, userID, ( ent, dbEnt ) => {
					dbEnt.ConceptSchemeId = context.ConceptScheme.FirstOrDefault( m => m.RowId == ent.InScheme )?.Id ?? 0;
					dbEnt.BroadMatchId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.BroadMatch )?.Id ?? 0;
				}, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

		/*
		//need alternate to handle workElementType
		public int Save( int conceptSchemeId, string conceptName, ref ChangeSummary status )
		{
			//check if exists
			var concept = GetConceptFromScheme( conceptSchemeId, conceptName );
			if ( concept?.Id > 0 )
				return concept.Id;
			//
			concept = new AppEntity()
			{
				ConceptSchemeId = conceptSchemeId,
				Name = conceptName,
				CodedNotation = conceptName
			};
			if ( Save( concept, ref status ) )
			{
				return concept.Id;
			}
			else
			{
				//caller needs to handle errors
				return 0;
			}

		}
		public bool Save( AppEntity entity, ref ChangeSummary status )
		{
			//must include conceptSchemeId
			//check inscheme
			if ( IsValidGuid( entity.InScheme ) ) //Editor uses GUID field, so check this first
			{
				var cs = GetByRowId( entity.InScheme );
				entity.ConceptSchemeId = cs.Id;
			}
			else if ( entity.ConceptSchemeId > 0 )
			{
				//Do nothing
			}
			else
			{
				status.AddError( "Error - A concept scheme Id or InScheme GUID must be provided with the Concept, and is missing. Please select an entry from the 'Belongs To Concept Scheme ' dropdown list." );
				return false;
			}

			bool isValid = true;
			int count = 0;
			//check if exists
			var concept = GetConceptFromScheme( entity.ConceptSchemeId, entity.Name );
			if ( concept?.Id > 0 )
			{
				//or set and fall thru - not clear if any updates at this time! Might depend on type
				entity.Id = concept.Id;
				//return true;    
			}

			try
			{
				using ( var context = new DataEntities() )
				{
					//look up if no id
					if ( entity.Id == 0 )
					{
						//add
						int newId = AddConcept( entity, ref status );
						if ( newId == 0 || status.HasSectionErrors )
							isValid = false;
						else
							entity.Id = newId;
						return isValid;

					}
					//update
					//TODO - consider if necessary, or interferes with anything
					//      - don't really want to include all training tasks
					context.Configuration.LazyLoadingEnabled = false;
					var efEntity = context.ConceptScheme_Concept
							.SingleOrDefault( s => s.Id == entity.Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						//fill in fields that may not be in entity
						entity.RowId = efEntity.RowId;
						entity.Created = efEntity.Created;
						entity.CreatedById = ( efEntity.CreatedById ?? 0 );
						entity.Id = efEntity.Id;

						MapToDB( entity, efEntity );

						if ( HasStateChanged( context ) )
						{
							efEntity.LastUpdated = DateTime.Now;
							efEntity.LastUpdatedById = entity.LastUpdatedById;
							count = context.SaveChanges();
							//can be zero if no data changed
							if ( count >= 0 )
							{
								entity.LastUpdated = ( DateTime ) efEntity.LastUpdated;
								isValid = true;
								SiteActivity sa = new SiteActivity()
								{
									ActivityType = "ConceptScheme",
									Activity = status.Action,
									Event = "Update",
									Comment = string.Format( "ConceptScheme was updated. Name: {0}", entity.Name ),
									ActionByUserId = entity.LastUpdatedById,
									ActivityObjectId = entity.Id
								};
								new ActivityManager().SiteActivityAdd( sa );
							}
							else
							{
								//?no info on error

								isValid = false;
								string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a Course. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, Id: {1}", entity.Name, entity.Id );
								status.AddError( "Error - the update was not successful. " + message );
								EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
							}

						}


					}
					else
					{
						status.AddError( "Error - update failed, as record was not found." );
					}


				}
			}
			catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
			{
				string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), "Course" );
				status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
			}
			catch ( Exception ex )
			{
				string message = FormatExceptions( ex );
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), true );
				status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
				isValid = false;
			}


			return isValid;
		}
		private int AddConcept( AppEntity entity, ref ChangeSummary status )
		{
			var efEntity = new Data.Tables.ConceptScheme_Concept();
			status.HasSectionErrors = false;
			int conceptId = 0;
			//assume lookup has been done
			using ( var context = new DataEntities() )
			{
				try
				{
					if ( entity.ConceptSchemeId == 0 )
					{
						status.AddError( "Error - A concept scheme Id must be provided with the Concept. Please select an entry from the 'Belongs To Concept Scheme ' dropdown list. " );
						return 0;
					}
					efEntity.ConceptSchemeId = entity.ConceptSchemeId;
					//require caller to set the codedNotation as needed
					MapToDB( entity, efEntity );
					efEntity.RowId = Guid.NewGuid();
					efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
					efEntity.CreatedById = efEntity.LastUpdatedById = entity.LastUpdatedById;

					context.ConceptScheme_Concept.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						//
						//add log entry
						SiteActivity sa = new SiteActivity()
						{
							ActivityType = "ConceptScheme",
							Activity = status.Action,
							Event = "Add Concept",
							Comment = string.Format( "Concept was added. Name: {0}", entity.Name ),
							ActionByUserId = entity.LastUpdatedById,
							ActivityObjectId = entity.Id
						};
						new ActivityManager().SiteActivityAdd( sa );


						return efEntity.Id;
					}
					else
					{
						//?no info on error

						string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a Course. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, ctid: {1}", entity.Name, entity.CTID );
						status.AddError( thisClassName + ". Error - the add was not successful. " + message );
						EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Course" );
					status.AddError( thisClassName + ".Add(). Error - the save was not successful. " + message );

					LoggingHelper.LogError( message, true );
				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}, CTID: {1}", efEntity.Name, efEntity.CTID ) );
					status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
				}
			}
			return 0;
		}
		//
		*/

		public static void MapToDB( AppEntity input, ConceptScheme_Concept output )
		{
			//watch for missing properties like rowId
			List<string> errors = new List<string>();
			BaseFactory.AutoMap( input, output, errors );
			//
		}
		//

		public static DeleteResult DeleteById( int id )
		{
			/*
			//Check for references from rating context objects
			var thingsUsingThisConcept = RatingContextManager.Search( new SearchQuery() { Skip = 0, Take = 0, Filters = new List<SearchFilter>() { new SearchFilter() { Name = "search:AllConceptPaths", ItemIds = new List<int>() { id } } } } );
			if( thingsUsingThisConcept.TotalResults > 0 )
			{
				return new DeleteResult( false, "This Concept is in use by " + thingsUsingThisConcept.TotalResults + " Rating Context objects, so it cannot be deleted." );
			}
			*/

			return BasicDeleteCore( "Concept", context => context.ConceptScheme_Concept, id, "search:AllConceptPaths", ( context, list, target ) =>
			{
				//Check for references from other concepts
				var narrowerConceptsCount = context.ConceptScheme_Concept.Where( m => m.BroadMatchId == id ).Count();
				if ( narrowerConceptsCount > 0 )
				{
					return new DeleteResult( false, "This Concept is a broader Concept for " + narrowerConceptsCount + " other Concepts, so it cannot be deleted." );
				}

				return null;
			} );
		}
		//

		#endregion

		#region Retrieval
		public static List<AppEntity> GetAll( bool onlyActiveConcepts = true )
		{
			var result = new List<AppEntity>();

			using ( var context = new DataEntities() )
			{
				var matches = context.ConceptScheme_Concept.AsQueryable();

				if ( onlyActiveConcepts )
				{
					matches = matches.Where( m => m.IsActive );
				}

				foreach ( var match in matches.ToList() )
				{
					result.Add( MapFromDB( match, context ) );
				}
			}

			return result;
		}
		//

		public static List<AppEntity> GetAllConceptsForScheme( string schemaURI, bool onlyActiveConcepts = true )
		{
			var result = new List<AppEntity>();

			using ( var context = new DataEntities() )
			{
				var matches = context.ConceptScheme_Concept.Where( m => m.ConceptScheme.SchemaUri == schemaURI );

				if ( onlyActiveConcepts )
				{
					matches = matches.Where( m => m.IsActive );
				}
				var dbConcepts = matches.ToList();

				foreach ( var dbConcept in dbConcepts )
				{
					result.Add( MapFromDB( dbConcept, context ) );
				}
			}

			return result;
		}
		//

		public static List<AppEntity> GetNarrowerConceptsForConcept( int conceptID )
		{
			return GetMultipleByFilter( context => context.ConceptScheme_Concept, m => m.BroadMatchId == conceptID, m => m.CodedNotation ?? m.Name, false, MapFromDB );
		}
		//

		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.ConceptScheme_Concept, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

		public static AppEntity GetByRowId( Guid rowId, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.RowId == rowId, returnNullIfNotFound );
		}
		//

		public static AppEntity GetById( int id, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.Id == id, returnNullIfNotFound );
		}
		//

		public static AppEntity GetByCTID( string ctid, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.CTID.ToLower() == ctid?.ToLower(), returnNullIfNotFound );
		}
		//

		public static AppEntity GetByName( string name, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.Name.ToLower() == name?.ToLower(), returnNullIfNotFound );
		}
		//

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
		{
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.ConceptScheme_Concept.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Name.Contains( keywords ) ||
						m.CodedNotation.Contains( keywords ) ||
						m.WorkElementType.Contains( keywords ) ||
						m.ConceptScheme.Name.Contains( keywords )
					);
				}

				//Return ordered list
				//Special handling for these because they're sorted first by concept scheme
				return HandleConceptSortByConceptScheme( list, query.SortOrder );

			}, MapFromDBForSearch );
		}
		//

		private static IOrderedEnumerable<DBEntity> HandleConceptSortByConceptScheme( IEnumerable<DBEntity> unsortedList, List<SortOrderItem> sortOrder )
		{
			//Default handler if no sort order is specified
			if( sortOrder == null || sortOrder.Count() == 0 )
			{
				return unsortedList.OrderBy( m => m.ConceptScheme.Name ).ThenBy( m => m.Name );
			}

			//Sort by not null (mostly just to convert the IEnumerable to an IOrderedEnumerable)
			var sorted = unsortedList.OrderBy( m => m != null );

			//Then by Concept Scheme specified column, or Name, or ID
			sorted = SortConceptsByScheme( sorted, sortOrder, typeof( ConceptScheme ), m => m.ConceptScheme, m => m.ConceptScheme.Name, m => m.ConceptScheme.Id );

			//Then by Concept specified column, or Name, or ID
			sorted = SortConceptsByScheme( sorted, sortOrder, typeof( DBEntity ), m => m, m => m.Name, m => m.Id );

			//Return sorted results
			return sorted;
		}
		private static IOrderedEnumerable<DBEntity> SortConceptsByScheme( IOrderedEnumerable<DBEntity> sorted, List<SortOrderItem> sortOrder, Type columnType, Func<DBEntity, object> ColumnTraversalMethod, Func<DBEntity, object> DefaultAlphaSortTraversalMethod, Func<DBEntity, object> IdTraversalMethod )
		{
			//For each SortItem...
			foreach( var sortItem in sortOrder ?? new List<SortOrderItem>() )
			{
				//Find the matching column for the type (Concept Scheme or Concept, if it exists
				var column = columnType.GetProperty( sortItem.Column );
				
				//Determine the method that gets the property to sort by for that type
				Func<DBEntity, object> GetterMethod;
				if ( column != null )
				{
					//If the column exists, return the value for that column from the provided type
					GetterMethod = ( DBEntity m ) => { return column.GetValue( ColumnTraversalMethod( m ) ); };
				}
				else if ( sortItem.Column == "sortOrder:DefaultAlphaSort" )
				{
					//If using the default alpha sort, handle that
					GetterMethod = DefaultAlphaSortTraversalMethod;
				}
				else
				{
					//Otherwise, fall back to using the ID
					GetterMethod = IdTraversalMethod;
				}

				//Do the sorting, Ascending or Descending based on the SortItem.Ascending property
				sorted = sortItem.Ascending ? sorted.ThenBy( m => GetterMethod( m ) ) : sorted.ThenByDescending( m => GetterMethod( m ) );
			}

			//Return the sorted list
			return sorted;
		}
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			return GetMultipleByFilter<DBEntity, AppEntity, string>( context => context.ConceptScheme_Concept, m => guids.Contains( m.RowId ), m => m.Description, false, MapFromDB, false );
		}
		//

		/// <summary>
		/// Get a concept using the ConceptSchemaURI and concept Name or concept coded notation
		/// </summary>
		/// <param name="conceptSchemeUri"></param>
		/// <param name="concept"></param>
		/// <returns></returns>
		public static AppEntity GetConceptFromScheme( string conceptSchemeUri, string concept, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m =>
				m.ConceptScheme.SchemaUri.ToLower() == conceptSchemeUri.ToLower() &&
				(
					m.Name.ToLower() == concept.ToLower() ||
					m.CodedNotation.ToLower() == concept.ToLower()
				), returnNullIfNotFound
			);
		}
		//

		/// <summary>
		/// Get a concept using the ConceptScheme Id and concept Name or concept coded notation
		/// </summary>
		/// <param name="conceptSchemeId"></param>
		/// <param name="concept"></param>
		/// <returns></returns>
		public static AppEntity GetConceptFromScheme( int conceptSchemeId, string concept, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => 
				m.ConceptScheme.Id == conceptSchemeId &&
				(
					m.Name.ToLower() == concept.ToLower() ||
					m.CodedNotation.ToLower() == concept.ToLower()
				), returnNullIfNotFound 
			);
		}
		//

		public static AppEntity MapFromDB( DBEntity input, DataEntities context )
		{
			return MapFromDBForSearch( input, context, null );
		}
		//

		public static AppEntity MapFromDBForSearch( DBEntity input, DataEntities context, SearchResultSet<AppEntity> resultSet = null )
		{
			var output = AutoMap( input, new AppEntity() );
			output.ConceptSchemeId = input.ConceptScheme?.Id ?? 0;
			output.InScheme = input.ConceptScheme?.RowId ?? Guid.Empty;
			output.SchemeUri = input.ConceptScheme?.SchemaUri;
			output.BroadMatch = ( input.BroadMatchId == 0 || input.BroadMatchId == null ) ? Guid.Empty : context.ConceptScheme_Concept.FirstOrDefault( m => m.Id == input.BroadMatchId )?.RowId ?? Guid.Empty;

			return output;
		}
		//
		#endregion
	}
}
