using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Curation;
using AppEntity = Models.Schema.CourseContext;
using CourseContextTask = Models.Schema.TrainingTask;
using DBEntity = Data.Tables.CourseContext;
using MSc = Models.Schema;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Models.Application;
using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class CourseContextManager : BaseFactory
    {
        public static new string thisClassName = "CourseContextManager";
		#region CourseContext - Persistence ==================
		public static void SaveFromUpload( AppEntity entity, int userID, ChangeSummary summary )
		{
			SaveCore( entity, userID, "Upload", summary.AddError );
		}
		//

		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			//Validate required fields
			AddErrorIf( errors, entity.HasCourse == Guid.Empty, "A Course must be selected." );
			AddErrorIf( errors, entity.HasTrainingTask == Guid.Empty, "A Training Task must be selected." );
			AddErrorIf( errors, entity.AssessmentMethodType.Count() == 0, "One or more Assessment Methods must be selected." );

			//Return early if anything is blank to avoid errors in the next section
			if( errors.Count() > 0 )
			{
				return;
			} 

			//Duplicate check
			DuplicateCheck( "Course Context", context => context.CourseContext.Where( m => m.RowId != entity.RowId ), errors, null, ( haystack, context ) =>
			{
				if ( haystack.Where( m =>
					m.Course.RowId == entity.HasCourse &&
					m.TrainingTask.RowId == entity.HasTrainingTask &&
					m.CourseContext_AssessmentType.Select( n => n.ConceptScheme_Concept.RowId ).Intersect( entity.AssessmentMethodType ).Count() == entity.AssessmentMethodType.Count()
				).Count() > 0 )
				{
					errors.Add( "A Course Context with identical values for all fields already exists in the system." );
				}
			} );

			//Return if any errors
			if( errors.Count() > 0 )
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
				BasicSaveCore( context, entity, context.CourseContext, userID, ( ent, dbEnt ) => {
					dbEnt.HasTrainingTaskId = context.TrainingTask.FirstOrDefault( m => m.RowId == ent.HasTrainingTask )?.Id ?? 0;
					dbEnt.HasCourseId = context.Course.FirstOrDefault( m => m.RowId == ent.HasCourse )?.Id ?? 0;
				}, ( ent, dbEnt ) => {
					HandleMultiValueUpdate( context, userID, ent.AssessmentMethodType, dbEnt, dbEnt.CourseContext_AssessmentType, context.ConceptScheme_Concept, nameof( CourseContext_AssessmentType.CourseContextId ), nameof( CourseContext_AssessmentType.AssessmentMethodConceptId ) );
				}, saveType, AddErrorMethod );
			}
		}
		//

		public static DeleteResult DeleteById( int id )
		{
			return BasicDeleteCore( "Course Context", context => context.CourseContext, id, "> CourseContextId > CourseContext", ( context, list, target ) =>
			{
				//Nothing else references a Course Context, so just return null
				return null;
			} );
		}
		//

		#endregion

		#region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.CourseContext, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

        public static AppEntity GetByCourseIdAndTrainingTaskId( int courseId, int trainingTaskId, bool returnNullIfNotFound = false )
        {
			return GetSingleByFilter( m => m.HasCourseId == courseId && m.HasTrainingTaskId == trainingTaskId, returnNullIfNotFound );
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

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
		{
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.CourseContext.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Course.CodedNotation.Contains( keywords ) ||
						m.Course.Name.Contains( keywords ) ||
						m.TrainingTask.Description.Contains( keywords )
					);
				}

				//Handle Filters
				//Detail Pages
				AppendIDsFilterIfPresent( query, ".Id", ids => {
					list = list.Where( m => ids.Contains( m.Id ) );
				} );

				//Course Detail Page
				AppendIDsFilterIfPresent(query, "> HasCourseId > Course", ids => {
					list = list.Where( m => ids.Contains( m.HasCourseId ) );
				} );

				//Course Detail Page
				AppendTextFilterIfPresent( query, "> HasTrainingTaskId > TrainingTask.TextFields", text => {
					list = list.Where( m => m.TrainingTask.Description.Contains( text ) );
				} );

				//Course Detail Page
				AppendTextFilterIfPresent( query, "> AssessmentMethodConceptId > Concept.Name", text => {
					list = list.Where( m => m.CourseContext_AssessmentType.Where( n => n.ConceptScheme_Concept.Name.Contains( text ) ).Count() > 0 );
				} );

				//Exclude items
				AppendIDsFilterIfPresent( query, "search:Exclude", ( ids ) =>
				{
					list = list.Where( m => !ids.Contains( m.Id ) );
				} );

				//Return ordered list
				//Traversal requires projection to avoid querying every row in the results
				var projected = list.Select( m => new { Main = m, Course_Name = m.Course.Name, Course_CodedNotation = m.Course.CodedNotation, TrainingTask_Description = m.TrainingTask.Description } );
				var sorted = HandleSortV2( projected, query.SortOrder, m => m.Course_Name, m => m.Main.Id, m => m.OrderBy( n => n.Course_Name ), ( m, keywordParts ) => m.OrderBy( n => RelevanceHelper( n, keywordParts, o => o.Course_Name ) + RelevanceHelper( n, keywordParts, o => o.Course_CodedNotation ) + RelevanceHelper( n, keywordParts, o => o.TrainingTask_Description ) ), keywords );
				return sorted.Select( m => m.Main ).OrderBy( m => true );

			}, MapFromDBForSearch );
		}
		//

		//Should only ever be one Course Context that has a specific combination of Course and Training Task
		public static AppEntity GetForUploadOrNull( Guid courseRowID, Guid trainingTaskRowID )
		{
			using ( var context = new DataEntities() )
			{
				var match = context.CourseContext.FirstOrDefault( m =>
					m.Course.RowId == courseRowID &&
					m.TrainingTask.RowId == trainingTaskRowID
				);

				if ( match != null )
				{
					return MapFromDB( match, context );
				}
			}

			return null;
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
			output.HasTrainingTask = input.TrainingTask?.RowId ?? Guid.Empty; //int ID field automatches
			output.HasCourse = input.Course?.RowId ?? Guid.Empty; //int ID field automatches
			output.AssessmentMethodType = input.CourseContext_AssessmentType?.Select( m => m.ConceptScheme_Concept.RowId ).ToList() ?? new List<Guid>();
			output.AssessmentMethodTypeId = input.CourseContext_AssessmentType?.Select( m => m.AssessmentMethodConceptId ).ToList() ?? new List<int>();

			return output;
		}

        #endregion

    }

}
