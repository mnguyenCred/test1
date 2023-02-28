using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Curation;
using AppEntity = Models.Schema.Course;
using CourseTask = Models.Schema.TrainingTask;
using DBEntity = Data.Tables.Course;
using MSc = Models.Schema;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Models.Application;
using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class CourseManager : BaseFactory
    {
        public static new string thisClassName = "CourseManager";
		//public List<MSc.TrainingTask> AllNewtrainingTasks = new List<MSc.TrainingTask>();
		//public List<MSc.TrainingTask> AllUpdatedtrainingTasks = new List<MSc.TrainingTask>();
		#region Course - Persistence ==================
		public static void SaveFromUpload( AppEntity entity, int userID, ChangeSummary summary )
		{
			SaveCore( entity, userID, "Upload", summary.AddError );
		}
		//

		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			//Validate required fields
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.Name ), "Name must not be empty." );
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.CodedNotation ), "Course Code must not be empty." );

			//Duplicate check
			DuplicateCheck( "Course", context => context.Course.Where( m => m.RowId != entity.RowId ), errors, new List<StringCheckMapping<DBEntity>>()
			{
				new StringCheckMapping<DBEntity>( entity.Name, dbEnt => CompareStrings( entity.Name, dbEnt.Name ), "Name", null ),
				//new StringCheckMapping<DBEntity>( () => entity.Description, dbEnt => dbEnt.Description, "Description", null ), //Probably okay for this to be the same?
				new StringCheckMapping<DBEntity>( entity.CodedNotation, dbEnt => CompareStrings( entity.CodedNotation, dbEnt.CodedNotation ), "Course Code", null )
			} ); //Don't need to check the connections to other objects, since those can be the same across many Courses

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
				BasicSaveCore( context, entity, context.Course, userID, ( ent, dbEnt ) => {
					dbEnt.CurriculumControlAuthorityId = context.Organization.FirstOrDefault( m => m.RowId == ent.CurriculumControlAuthority )?.Id ?? 0;
					dbEnt.LifeCycleControlDocumentTypeId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.LifeCycleControlDocumentType )?.Id ?? 0;
				}, ( ent, dbEnt ) => {
					HandleMultiValueUpdate( context, userID, ent.CourseType, dbEnt, dbEnt.Course_CourseType, context.ConceptScheme_Concept, nameof( Course_CourseType.CourseId ), nameof( Course_CourseType.CourseTypeConceptId ) );
				}, saveType, AddErrorMethod );
			}
		}
		//

		public static DeleteResult DeleteById( int id )
		{
			return BasicDeleteCore( "Course", context => context.Course, id, "> CourseContextId > CourseContext > HasCourseId > Course", ( context, list, target ) =>
			{
				//Check for references from Course Contexts
				var courseContextCount = context.CourseContext.Where( m => m.HasCourseId == id ).Count();
				if( courseContextCount > 0 )
				{
					return new DeleteResult( false, "This Course is referenced by " + courseContextCount + " Course Context objects, so it cannot be deleted." );
				}

				return null;
			} );
		}
		//

		#endregion

		#region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.Course, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

        public static AppEntity GetByCodedNotation( string codedNotation, bool returnNullIfNotFound = false )
        {
			return GetSingleByFilter( m => m.CodedNotation?.ToLower() == codedNotation?.ToLower(), returnNullIfNotFound );
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

		public static AppEntity GetForUploadOrNull( string courseName, string courseCodedNotation, Guid curriculumControlAuthorityRowID, Guid lifeCycleControlDocumentTypeRowId )
		{
			using ( var context = new DataEntities() )
			{
				var match = context.Course.FirstOrDefault( m =>
					m.Name.ToLower() == courseName.ToLower() &&
					m.CodedNotation.ToLower() == courseCodedNotation.ToLower() &&
					context.Organization.FirstOrDefault( n => n.RowId == curriculumControlAuthorityRowID && n.Id == m.CurriculumControlAuthorityId ) != null &&
					context.ConceptScheme_Concept.FirstOrDefault( n => n.RowId == lifeCycleControlDocumentTypeRowId && n.Id == m.LifeCycleControlDocumentTypeId ) != null
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
			return GetItemList( context => context.Course.OrderBy( m => m.Name ), MapFromDB, false );
        }
		//

        public static SearchResultSet<AppEntity> Search( SearchQuery query )
        {
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.Course.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m => 
						m.Name.Contains( keywords ) ||
						m.CodedNotation.Contains( keywords )
					);
				}

				//Organization Detail Page
				AppendIDsFilterIfPresent(query, "> CurriculumControlAuthorityId > Organization", ids =>
				{
					list = list.Where( m => ids.Contains( m.CurriculumControlAuthorityId ?? 0 ) );
				} );

				//Exclude items
				AppendIDsFilterIfPresent( query, "search:Exclude", ( ids ) =>
				{
					list = list.Where( m => !ids.Contains( m.Id ) );
				} );

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.Name, m => m.OrderBy( n => n.Name ), ( m, keywordParts ) => m.OrderBy( n => RelevanceHelper( n, keywordParts, o => o.Name ) + RelevanceHelper( n, keywordParts, o => o.CodedNotation ) ), keywords );

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
			output.CurriculumControlAuthority = input.Organization?.RowId ?? Guid.Empty;
			output.LifeCycleControlDocumentType = input.ConceptScheme_Concept?.RowId ?? Guid.Empty;
			output.CourseType = input.Course_CourseType?.Select( m => m.ConceptScheme_Concept_CourseType ).Select( m => m.RowId ).ToList() ?? new List<Guid>();

			return output;
		}
		//

        #endregion

    }

}
