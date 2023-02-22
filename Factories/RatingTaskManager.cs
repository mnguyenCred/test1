using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;

using Models.Application;
using Models.Curation;
using Models.Import;
using Models.Schema;
using Models.Search;

using Navy.Utilities;

using AppEntity = Models.Schema.RatingTask;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.RatingTask;
using ViewContext = Data.Views.ceNavyViewEntities;

namespace Factories
{
	public class RatingTaskManager : BaseFactory
	{
		public static new string thisClassName = "RatingTaskManager";
		public static string cacheKey = "RatingTaskCache";
		public static string cacheKeySummary = "RatingTaskSummaryCache";

		#region === persistance ==================
		public static void SaveFromUpload( AppEntity entity, int userID, ChangeSummary summary )
		{
			SaveCore( entity, userID, "Upload", summary.AddError );
		}
		//

		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			SaveCore( entity, userID, "Edit", errors.Add );
		}
		//

		private static void SaveCore( AppEntity entity, int userID, string saveType, Action<string> AddErrorMethod )
		{
			using ( var context = new DataEntities() )
			{
				BasicSaveCore( context, entity, context.RatingTask, userID, ( ent, dbEnt ) => {
					dbEnt.ReferenceResourceId = context.ReferenceResource.FirstOrDefault( m => m.RowId == ent.HasReferenceResource )?.Id ?? 0;
					dbEnt.ReferenceTypeId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.ReferenceType )?.Id ?? 0;
				}, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

		public static DeleteResult DeleteById( int id )
		{
			return BasicDeleteCore( "Rating Task", context => context.RatingTask, id, "> RatingTaskId > RatingTask", ( context, list, target ) =>
			{
				//Check for references from Cluster Analysis objects
				var clusterAnalysisContextCount = context.ClusterAnalysis.Where( m => m.HasRatingTaskId == id ).Count();
				if ( clusterAnalysisContextCount > 0 )
				{
					return new DeleteResult( false, "This Rating Task is referenced by " + clusterAnalysisContextCount + " Cluster Analysis objects, so it cannot be deleted." );
				}

				return null;
			} );
		}
		//


		#endregion

		#region Retrieval

		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.RatingTask, FilterMethod, MapFromDB, returnNullIfNotFound );
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

		public static AppEntity GetForUploadOrNull( string ratingTaskDescription, Guid referenceResourceRowID, Guid referenceTypeRowID )
		{
			using ( var context = new DataEntities() )
			{
				var match = context.RatingTask.FirstOrDefault( m =>
					m.Description.ToLower() == ratingTaskDescription.ToLower() &&
					m.ReferenceResource.RowId == referenceResourceRowID &&
					m.ConceptScheme_Concept_ReferenceType.RowId == referenceTypeRowID
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
		/// It is not clear that we want a get all - tens of thousands
		/// </summary>
		/// <returns></returns>
		public static List<AppEntity> GetAll()
		{
			return GetItemList( context => context.RatingTask.OrderBy( m => m.Id ), MapFromDB, false );
		}
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			return GetMultipleByFilter( context => context.RatingTask, m => guids.Contains( m.RowId ), m => m.Id, false, MapFromDB, false );
		}
		//

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
		{
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.RatingTask.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Description.Contains( keywords )
					);
				}

				//Rating Task Count By Source Type Table (used to filter by reference resource type)
				AppendIDsFilterIfPresent( query, "> ReferenceResourceTypeId > Concept", ids => {
					list = list.Where( m => ids.Contains( m.ReferenceTypeId ) );
				} );

				//Rating Task Count By Source Type Table (used for counting tasks with gaps)
				AppendIDsFilterIfPresent( query, "< RatingTaskId < RatingContext > FormalTrainingGapId > Concept", ids => {
					list = list.Where( m => context.RatingContext.Where( n => n.RatingTaskId == m.Id && ids.Contains( n.FormalTrainingGapId ?? 0 ) ).Count() > 0 );
				} );

				//Exclude items
				AppendIDsFilterIfPresent( query, "search:Exclude", ( ids ) =>
				{
					list = list.Where( m => !ids.Contains( m.Id ) );
				} );

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.Description, m => m.OrderBy( n => n.Description ), ( m, keywordParts ) => m.OrderBy( n => RelevanceHelper( n, keywordParts, o => o.Description ) ), keywords );

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
			output.HasReferenceResource = input.ReferenceResource?.RowId ?? Guid.Empty;
			output.ReferenceType = input.ConceptScheme_Concept_ReferenceType?.RowId ?? Guid.Empty;

			return output;
		}
		//

		#endregion

	}
}
