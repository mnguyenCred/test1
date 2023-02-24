using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppEntity = Models.Schema.ReferenceResource;
using DBEntity = Data.Tables.ReferenceResource;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Models.Application;
using Models.Curation;

using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class ReferenceResourceManager : BaseFactory
    {
        public static new string thisClassName = "ReferenceResourceManager";

		#region ===  Persistance ==================
		public static void SaveFromUpload( AppEntity entity, int userID, ChangeSummary summary )
		{
			SaveCore( entity, userID, "Upload", summary.AddError );
		}
		//

		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			//Validate required fields
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.Name ), "Name must not be blank." );
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.PublicationDate ), "Publication Date must not be blank." );
			AddErrorIf( errors, entity.ReferenceType.Count() == 0, "One or more Reference Types must be selected." );

			//Return early if any errors to avoid errors in the next section
			if( errors.Count() > 0 )
			{
				return;
			}

			//Duplicate checks
			DuplicateCheck( "Reference Resource", context => context.ReferenceResource.Where( m => m.RowId != entity.RowId ), errors, null, ( haystack, context ) =>
			{
				//Custom handling because multiple Reference Resources can have the same name as long as they have different dates
				if ( haystack.Where( m =>
					 m.Name.ToLower() == entity.Name.ToLower() &&
					 m.PublicationDate.ToLower() == entity.PublicationDate.ToLower() &&
					 m.ReferenceResource_ReferenceType.Select( n => n.ConceptScheme_Concept_ReferenceType.RowId ).Intersect( entity.ReferenceType ).Count() == entity.ReferenceType.Count()
				).Count() > 0 )
				{
					errors.Add( "Another Reference Resource with the same Name, Publication Date, and Reference Type(s) already exists in the system." );
				}
			} );

			//Return if any errors
			if ( errors.Count() > 0 )
			{
				return;
			}

			SaveCore( entity, userID, "Edit", errors.Add );
		}
		//

		private static void SaveCore( AppEntity entity, int userID, string saveType, Action<string> AddErrorMethod )
		{
			using ( var context = new DataEntities() )
			{
				BasicSaveCore( context, entity, context.ReferenceResource, userID, ( ent, dbEnt ) => { }, ( ent, dbEnt ) => {
					HandleMultiValueUpdate( context, userID, ent.ReferenceType, dbEnt, dbEnt.ReferenceResource_ReferenceType, context.ConceptScheme_Concept, nameof( ReferenceResource_ReferenceType.ReferenceResourceId ), nameof( ReferenceResource_ReferenceType.ReferenceTypeId ) );
				}, saveType, AddErrorMethod );
			}
		}
		//

		public static DeleteResult DeleteById( int id )
		{
			return BasicDeleteCore( "Reference Resource", context => context.ReferenceResource, id, "> RatingTaskId > RatingTask > ReferenceResourceId > ReferenceResource", ( context, list, target ) =>
			{
				//Check for references from Rating Tasks
				var ratingTaskCount = context.RatingTask.Where( m => m.ReferenceResourceId == id ).Count();
				if ( ratingTaskCount > 0 )
				{
					return new DeleteResult( false, "This Reference Resource is the source for " + ratingTaskCount + " Rating Tasks, so it cannot be deleted." );
				}

				//Check for references from Training Tasks
				var trainingTaskCount = context.TrainingTask.Where( m => m.ReferenceResourceId == id ).Count();
				if ( trainingTaskCount > 0 )
				{
					return new DeleteResult( false, "This Reference Resource is used to derive the uniqueness of " + trainingTaskCount + " Training Tasks, so it cannot be deleted." );
				}

				return null;
			} );
		}
		//

		#endregion

		#region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.ReferenceResource, FilterMethod, MapFromDB, returnNullIfNotFound );
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

		public static AppEntity GetForUploadOrNull( string name, string publicationDate )
		{
			using ( var context = new DataEntities() )
			{
				var match = context.ReferenceResource.FirstOrDefault( m =>
					m.Name.ToLower() == name.ToLower() &&
					m.PublicationDate.ToLower() == publicationDate.ToLower()
				);

				if( match != null )
				{
					return MapFromDB( match, context );
				}
			}

			return null;
		}
		//

		/// <summary>
		/// Get all 
		/// May need a get all for a rating? Should not matter as this is external data?
		/// </summary>
		/// <returns></returns>
		public static List<AppEntity> GetAll()
        {
			return GetItemList( context => context.ReferenceResource.OrderBy( m => m.Name ), MapFromDB, false );
        }
		//

        public static SearchResultSet<AppEntity> Search( SearchQuery query )
        {
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.ReferenceResource.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Name.Contains( keywords ) ||
						m.PublicationDate.Contains( keywords ) ||
						m.Description.Contains( keywords )
					);
				}

				//Exclude items
				AppendIDsFilterIfPresent( query, "search:Exclude", ( ids ) =>
				{
					list = list.Where( m => !ids.Contains( m.Id ) );
				} );

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.Name, m => m.OrderBy( n => n.Name ).ThenBy( n => n.PublicationDate ), ( m, keywordParts ) => m.OrderBy( n => RelevanceHelper( n, keywordParts, o => o.Name ) + RelevanceHelper( n, keywordParts, o => o.PublicationDate ) ), keywords );

			}, MapFromDBForSearch );
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
			output.ReferenceType = input.ReferenceResource_ReferenceType?.Select( m => m.ConceptScheme_Concept_ReferenceType ).Select( m => m.RowId ).ToList() ?? new List<Guid>();

			return output;
		}
		//

        #endregion
    }
}
