using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Application;
using Models.Curation;

using ParentEntity = Models.Schema.Course;
using AppEntity = Models.Schema.TrainingTask;
using DBEntity = Data.Tables.TrainingTask;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Navy.Utilities;
using Models.Search;
using System.Runtime.Caching;

namespace Factories
{
    public class TrainingTaskManager : BaseFactory
    {
        public static new string thisClassName = "TrainingTaskManager";
        public static string cacheKey = "TrainingTaskCache";

		#region Persistance
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
				BasicSaveCore( context, entity, context.TrainingTask, userID, ( ent, dbEnt ) => {
					dbEnt.ReferenceResourceId = context.ReferenceResource.FirstOrDefault( m => m.RowId == ent.HasReferenceResource )?.Id ?? 0;
				}, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

        #endregion


        #region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.TrainingTask, FilterMethod, MapFromDB, returnNullIfNotFound );
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

		public static AppEntity GetForUploadOrNull( string trainingTaskDescription, Guid referenceResourceRowID )
		{
			using ( var context = new DataEntities() )
			{
				var match = context.TrainingTask.FirstOrDefault( s =>
					s.Description.ToLower() == trainingTaskDescription.ToLower() &&
					context.ReferenceResource.FirstOrDefault( m => 
						s.ReferenceResourceId == m.Id && 
						m.RowId == referenceResourceRowID 
					) != null
				);

				if ( match != null )
				{
					return MapFromDB( match, context ) ;
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
			return GetItemList( context => context.TrainingTask.OrderBy( m => m.Description ), MapFromDB, false );
        }
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			return GetMultipleByFilter( context => context.TrainingTask, m => guids.Contains( m.RowId ), m => m.Description, false, MapFromDB, false );
		}
		//

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
		{
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.TrainingTask.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Description.Contains( keywords )
					);
				}

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

			return output;
        }

        #endregion
    }
}
