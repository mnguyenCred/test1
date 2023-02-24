using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

using Models.Application;
using Models.Curation;
using Models.Search;

using Navy.Utilities;

using AppEntity = Models.Schema.BilletTitle;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.Job;
namespace Factories
{
    public class JobManager : BaseFactory
    {
        public static new string thisClassName = "JobManager";
        public static string cacheKey = "JobCache";

		#region === Persistence ==================
		public static void SaveFromUpload( AppEntity entity, int userID, ChangeSummary summary )
		{
			SaveCore( entity, userID, "Upload", summary.AddError );
		}
		//

		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			//Validate required fields
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.Name ), "Name must not be empty." );

			//Duplicate checks
			DuplicateCheck( "Billet Title", context => context.Job.Where( m => m.RowId != entity.RowId ), errors, new List<StringCheckMapping<DBEntity>>()
			{
				new StringCheckMapping<DBEntity>( entity.Name, dbEnt => CompareStrings( entity.Name, dbEnt.Name ), "Name" )
			} );

			//Return if any errors
			if( errors.Count() > 0 )
			{
				return;
			}

			SaveCore( entity, userID, "Edit", errors.Add );
		}
		//

		public static void Dupe2( IQueryable<DBEntity> haystack, string entValue, Func<DBEntity, bool> GetDBValue )
		{
			if( haystack.Where( GetDBValue ).Count() > 0 )
			{

			}
		}

		private static void SaveCore( AppEntity entity, int userID, string saveType, Action<string> AddErrorMethod )
		{
			using ( var context = new DataEntities() )
			{
				BasicSaveCore( context, entity, context.Job, userID, ( ent, dbEnt ) => { }, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

		public static DeleteResult DeleteById( int id )
		{
			return BasicDeleteCore( "Billet Title", context => context.Job, id, "> BilletTitleId > Job", ( context, list, target ) => 
			{
				//Check for references from Cluster Analysis objects
				var clusterAnalysisContextCount = context.ClusterAnalysis.Where( m => m.BilletTitleId == id ).Count();
				if ( clusterAnalysisContextCount > 0 )
				{
					return new DeleteResult( false, "This Billet Title is referenced by " + clusterAnalysisContextCount + " Cluster Analysis objects, so it cannot be deleted." );
				}

				return null;
			} );
		}
		//

        #endregion

        #region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.Job, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

        //unlikely?
        public static AppEntity GetByName( string name, bool returnNullIfNotFound = false )
        {
			return GetSingleByFilter( m => m.Name?.ToLower() == name?.ToLower(), returnNullIfNotFound );
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

		public static List<AppEntity> GetAll()
        {
			return GetItemList( context => context.Job.OrderBy( m => m.Name ), MapFromDB, false );
        }
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			return GetMultipleByFilter( context => context.Job, m => guids.Contains( m.RowId ), m => m.Name, false, MapFromDB, false );
		}
		//

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
		{
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var keywords = GetSanitizedSearchFilterKeywords( query );
				var list = context.Job.AsQueryable();

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( s =>
						s.Name.Contains( keywords ) ||
						s.Description.Contains( keywords ) ||
						s.CodedNotation.Contains( keywords )
					);
				}

				//Exclude items
				AppendIDsFilterIfPresent( query, "search:Exclude", ( ids ) =>
				{
					list = list.Where( m => !ids.Contains( m.Id ) );
				} );

				//Return ordered list
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
