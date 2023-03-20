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
			//Validate required fields
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.Description ), "Description must not be empty." );
			AddErrorIf( errors, entity.HasReferenceResource == Guid.Empty, "A Reference Resource must be selected." );

			//Return early if any errors to avoid errors in the next section
			if ( errors.Count() > 0 )
			{
				return;
			}

			//Duplicate check
			DuplicateCheck( "Training Task", context => context.TrainingTask.Where( m => m.RowId != entity.RowId ), errors, null, ( haystack, context ) =>
			{
				//Custom handling because duplicate Training Tasks are okay as long as they come from different sources
				if ( haystack.Where( m =>
					 m.Description.ToLower() == entity.Description.ToLower() &&
					 m.ReferenceResource.RowId == entity.HasReferenceResource
				).Count() > 0 )
				{
					errors.Add( "An identical Training Task with the same values for all fields already exists in the system." );
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
				BasicSaveCore( context, entity, context.TrainingTask, userID, ( ent, dbEnt ) => {
					dbEnt.ReferenceResourceId = context.ReferenceResource.FirstOrDefault( m => m.RowId == ent.HasReferenceResource )?.Id ?? 0;
				}, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

		public static DeleteResult DeleteById( int id )
		{
			return BasicDeleteCore( "Training Task", context => context.TrainingTask, id, "> CourseContextId > CourseContext > HasTrainingTaskId > TrainingTask", ( context, list, target ) =>
			{
				//Check for references from Course Contexts
				var courseContextCount = context.CourseContext.Where( m => m.HasCourseId == id ).Count();
				if ( courseContextCount > 0 )
				{
					return new DeleteResult( false, "This Training Task is referenced by " + courseContextCount + " Course Context objects, so it cannot be deleted." );
				}

				return null;
			} );
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

				//Training Task Detail Page
				AppendTextFilterIfPresent( query, ".DescriptionExact", ( text ) =>
				{
					list = list.Where( m => m.Description.ToLower() == text.ToLower() );
				} );

				//Training Task Detail Page
				AppendTextFilterIfPresent( query, ".TextFields", ( text ) =>
				{
					list = list.Where( m => m.Description.Contains( text ) );
				} );

				//Training Task Detail Page
				AppendTextFilterIfPresent( query, "> HasReferenceResourceId > ReferenceResource.TextFields", ( text ) =>
				{
					list = list.Where( m => m.ReferenceResource.Name.Contains( text ) || m.ReferenceResource.CodedNotation.Contains( text ) || m.ReferenceResource.Description.Contains( text ) );
				} );

				//Training Task Detail Page
				AppendTextFilterIfPresent( query, "> HasReferenceResourceId > ReferenceResource.PublicationDate", ( text ) =>
				{
					list = list.Where( m => m.ReferenceResource.PublicationDate.Contains( text ) );
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
			output.HasReferenceResourceId = input.ReferenceResourceId ?? 0; //Wish these fields matched

			return output;
        }

        #endregion
    }
}
