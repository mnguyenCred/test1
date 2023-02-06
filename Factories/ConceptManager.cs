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

		public static DeleteResult DeleteById( int id )
		{
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
			return GetItemList( context => context.ConceptScheme_Concept.Where( m => m.IsActive || !onlyActiveConcepts ).OrderBy( m => m.Name ), MapFromDB, false );
		}
		//

		public static List<AppEntity> GetAllConceptsForScheme( string schemaURI, bool onlyActiveConcepts = true )
		{
			return GetMultipleByFilter( context => context.ConceptScheme_Concept, m => ( m.ConceptScheme.SchemaUri == schemaURI) && ( m.IsActive || !onlyActiveConcepts ), m => m.CodedNotation, false, MapFromDB, false );
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
						m.WorkElementType.Contains( keywords )
					);
				}

				//Exclude items
				AppendIDsFilterIfPresent( query, "search:Exclude", ( ids ) =>
				{
					list = list.Where( m => !ids.Contains( m.Id ) );
				} );

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.Name, m => m.OrderBy( n => n.ConceptScheme.Name ).ThenBy( n => n.Name ), ( m, keywordParts ) => m.OrderBy( n => RelevanceHelper( n, keywordParts, o => o.Name ) ).ThenBy( o => o.ConceptScheme.Name ), keywords );
				
				//Special handling for these because they're sorted first by concept scheme
				//return HandleConceptSortByConceptScheme( list, query.SortOrder ); //Not needed? TBD!

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
		//

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
			return GetMultipleByFilter( context => context.ConceptScheme_Concept, m => guids.Contains( m.RowId ), m => m.Description, false, MapFromDB, false );
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
