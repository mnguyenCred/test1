using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

using Models.Application;
using Models.Curation;
using Models.Search;

using Navy.Utilities;

using AppEntity = Models.Schema.ClusterAnalysisTitle;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.ClusterAnalysisTitle; //TODO: Create Data.Tables class for this!

namespace Factories
{
	public class ClusterAnalysisTitleManager : BaseFactory
	{
		public static new string thisClassName = "ClusterAnalysisTitleManager";
		public static string cacheKey = "ClusterAnalysisTitleCache";

		#region === Persistance ==================

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
				BasicSaveCore( context, entity, context.ClusterAnalysisTitle, userID, ( ent, dbEnt ) => { }, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

		public static DeleteResult DeleteById( int id )
		{
			return BasicDeleteCore( "Cluster Analysis Title", context => context.ClusterAnalysisTitle, id, "> ClusterAnalysisId > ClusterAnalysis > HasClusterAnalysisTitleId > ClusterAnalysisTitle", ( context, list, target ) => 
			{
				//Check for references from Cluster Analysis objects
				var clusterAnalysisCount = context.ClusterAnalysis.Where( m => m.HasClusterAnalysisTitleId == id ).Count();
				if(clusterAnalysisCount > 0 )
				{
					return new DeleteResult( false, "This Cluster Analysis Title is referenced by " + clusterAnalysisCount + " Cluster Analysis objects, so it cannot be deleted." );
				}

				return null;
			} );
		}
		//

		#endregion

		#region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.ClusterAnalysisTitle, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

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

		public static AppEntity GetForUploadOrNull( string clusterAnalysisTitleName )
		{
			using(var context = new DataEntities() )
			{
				var match = context.ClusterAnalysisTitle.FirstOrDefault( m =>
					m.Name.ToLower() == clusterAnalysisTitleName.ToLower()
				);

				if ( match != null )
				{
					return MapFromDB( match, context );
				}
			}

			return null;
		}
		//

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
		{
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.ClusterAnalysisTitle.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m => m.Name.Contains( keywords ) );
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
