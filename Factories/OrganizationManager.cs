using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

using Models.Application;
using Models.Curation;
using Navy.Utilities;

using AppEntity = Models.Schema.Organization;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.Organization;
using Models.Search;

namespace Factories
{
    public class OrganizationManager : BaseFactory
    {
        public static new string thisClassName = "OrganizationManager";
        public static string cacheKey = "OrganizationCache";
        #region Organization - Persistence ==================
		public static void SaveFromUpload( AppEntity entity, int userID, ChangeSummary summary )
		{
			SaveCore( entity, userID, "Upload", summary.AddError );
		}
		//

		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			//Validate required fields
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.Name ), "Name must not be blank." );
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.AlternateName ), "Alternate Name must not be blank." );

			//Duplicate checks
			DuplicateCheck( "Organization", context => context.Organization.Where( m => m.RowId != entity.RowId ), errors, new List<StringCheckMapping<DBEntity>>()
			{
				new StringCheckMapping<DBEntity>( entity.Name, dbEnt => CompareStrings( entity.Name, dbEnt.Name ), "Name", null ),
				//new StringCheckMapping<DBEntity>( entity.Description, dbEnt => dbEnt.Description, "Description", null ), //Probably okay for this to be the same?
				//new StringCheckMapping<DBEntity>( entity.AlternateName, dbEnt => dbEnt.AlternateName, "Alternate Name", null ) //Allow same until we are told otherwise
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
				BasicSaveCore( context, entity, context.Organization, userID, ( ent, dbEnt ) => { }, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

		public static DeleteResult DeleteById( int id )
		{
			return BasicDeleteCore( "Organization", context => context.Organization, id, "> CourseContextId > CourseContext > HasCourseId > Course > CurriculumControlAuthorityId > Organization", ( context, list, target ) => 
			{
				//Check for references from Courses
				var coursesCount = context.Course.Where( m => m.CurriculumControlAuthorityId == id ).Count();
				if ( coursesCount > 0 )
				{
					return new DeleteResult( false, "This Organization is a Curriculum Control Authority for " + coursesCount + " Courses, so it cannot be deleted." );
				}

				return null;
			} );
		}
		//

		public static Models.DTO.MergeSummary GetMergeSummary( Guid rowID )
		{
			return GetMergeSummary( "Organization", rowID, m => m.Organization, ( context, match, summary ) =>
			{
				//Label
				summary.Label = match.Name;

				//Incoming
				summary.Incoming.Add( new Models.DTO.MergeSummaryItem( context.Course.Where( m => m.Organization.RowId == match.RowId ).Count(), "Courses" ) );
			} );
		}
		//

		public static void DoMerge( Models.DTO.MergeAttempt attempt )
		{
			DoMerge( attempt, m => m.Organization, ( context, source, destination ) =>
			{
				foreach ( var item in context.Course.Where( m => m.Organization.RowId == source.RowId ) )
				{
					item.CurriculumControlAuthorityId = destination.Id;
				}
			} );
		}
		//

		#endregion

		#region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.Organization, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

        public static AppEntity GetByName( string name, bool returnNullIfNotFound = false )
        {
			return GetSingleByFilter( m => m.Name?.ToLower() == name?.ToLower(), returnNullIfNotFound );
		}
		//

		public static AppEntity GetByAlternateName( string alternateName, bool returnNullIfNotFound = false )
        {
			return GetSingleByFilter( m => m.AlternateName?.ToLower() == alternateName?.ToLower(), returnNullIfNotFound );
        }
		//

		public static AppEntity GetByNameOrAlternateName( string name, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.Name?.ToLower() == name?.ToLower() || m.AlternateName?.ToLower() == name?.ToLower(), returnNullIfNotFound );
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

		/// <summary>
		/// Get all 
		/// </summary>
		/// <returns></returns>
		public static List<AppEntity> GetAll()
        {
			return GetItemList( context => context.Organization.OrderBy( m => m.Name ), MapFromDB, false );
        }
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			return GetMultipleByFilter( context => context.Organization, m => guids.Contains( m.RowId ), m => m.Name, false, MapFromDB, false );
		}
		//

        public static SearchResultSet<AppEntity> Search( SearchQuery query )
        {
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.Organization.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Name.Contains( keywords ) ||
						m.AlternateName.Contains(keywords)// ||
						//m.Description.Contains(keywords)
					);
				}

				//Enable filtering to just Organizations that have data
				AppendSimpleFilterIfPresent( query, "< CurriculumControlAuthorityId < Course < HasCourseId < CourseContext < CourseContextId < RatingContext:NotNull", () => {
					list = list.Where( m => context.RatingContext.Where( n => n.CourseContext.Course.Organization == m ).Count() > 0 );
				} );

				//Exclude items
				AppendIDsFilterIfPresent( query, "search:Exclude", ( ids ) =>
				{
					list = list.Where( m => !ids.Contains( m.Id ) );
				} );

				return HandleSort( list, query.SortOrder, m => m.Name, m => m.OrderBy( n => n.Name ), ( m, keywordParts ) => m.OrderBy( n => RelevanceHelper( n, keywordParts, o => o.Name ) ), keywords );

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

			return output;
		}
		//

		#endregion

	}
}
