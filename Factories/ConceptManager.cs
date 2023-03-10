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
		public static new string thisClassName = "ConceptManager";
		//

		#region Persistence
		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			//Validate required fields
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.Name ), "Name must not be blank." );

			//Duplicate checks
			DuplicateCheck( "Concept Scheme", context => context.ConceptScheme_Concept.Where( m => m.RowId != entity.RowId && context.ConceptScheme.FirstOrDefault( n => n.RowId == entity.InScheme && n.Id == m.ConceptSchemeId ) != null ), errors, new List<StringCheckMapping<DBEntity>>()
			{
				new StringCheckMapping<DBEntity>( entity.Name, dbEnt => CompareStrings( entity.Name, dbEnt.Name ), "Name", "Another Concept in this Concept Scheme has a matching Name." ),
				new StringCheckMapping<DBEntity>( entity.CodedNotation, dbEnt => CompareStrings( entity.CodedNotation, dbEnt.CodedNotation ), "Coded Notation", "Another Concept in this Concept Scheme has a matching Code." ),
				new StringCheckMapping<DBEntity>( entity.WorkElementType, dbEnt => CompareStrings( entity.WorkElementType, dbEnt.WorkElementType ), "Work Element Type", "Another Concept in this Concept Scheme has a matching Work Element Type." )
			} );

			//Return if any errors
			if( errors.Count() > 0 )
			{
				return;
			}

			/* Referenced this to create the generic version
			using (var context = new DataEntities())
			{
				var scheme = context.ConceptScheme.FirstOrDefault(m => m.RowId == entity.InScheme);
				var conceptsDuplicates = context.ConceptScheme_Concept.Where(m => m.ConceptSchemeId == scheme.Id && m.RowId != entity.RowId);

                if (!String.IsNullOrWhiteSpace(entity.Name) && conceptsDuplicates.Where(m => m.Name.ToLower() == entity.Name.ToLower()).Count() > 0)
                {
                    errors.Add("Another Concept in this Concept Scheme has a matching Name");
                }

                if (!String.IsNullOrWhiteSpace(entity.CodedNotation) && conceptsDuplicates.Where(m => m.CodedNotation.ToLower() == entity.CodedNotation.ToLower()).Count() > 0)
                {
                    errors.Add("Another Concept in this Concept Scheme has a matching Coded Notation");
                }

                if (!String.IsNullOrWhiteSpace(entity.WorkElementType) && conceptsDuplicates.Where(m => m.WorkElementType.ToLower() == entity.WorkElementType.ToLower()).Count() > 0)
                {
                    errors.Add("Another Concept in this Concept Scheme has a matching Work Element Type");
                }

                if (errors.Count() > 0) {
					return;
				}
			}
			*/

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
				//Check for references from other Concepts
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
				//Traversal requires projection to avoid querying every row in the results
				var projected = list.Select( m => new { Main = m, ConceptScheme_Name = m.ConceptScheme.Name } );
				var sorted = HandleSort2( projected, query.SortOrder, m => m.Main.Name, m => m.Main.Id, m => m.OrderBy( n => n.ConceptScheme_Name ).ThenBy( n => n.Main.Name ), ( m, keywordParts ) => m.OrderBy( n => RelevanceHelper( n, keywordParts, o => o.Main.Name ) ).ThenBy( o => o.ConceptScheme_Name ), keywords );
				return sorted.Select( m => m.Main ).OrderBy( m => true );
				

			}, MapFromDBForSearch );
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
			output.BroadMatch = ( input.BroadMatchId == 0 || input.BroadMatchId == null ) ? Guid.Empty : context.ConceptScheme_Concept.FirstOrDefault( m => m.Id == input.BroadMatchId )?.RowId ?? Guid.Empty; //int ID field automatches

			return output;
		}
		//
		#endregion
	}
}
