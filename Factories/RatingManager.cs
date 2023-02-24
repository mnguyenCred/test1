using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppEntity = Models.Schema.Rating;
using DBEntity = Data.Tables.Rating;
using ViewEntity = Data.Views.RatingSummary;
using Models.Curation;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Models.Application;
using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class RatingManager : BaseFactory
    {
        public static new string thisClassName = "RatingManager";

		#region Rating - Persistence - NOT Likely? ==================
		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			//Validate required fields
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.Name ), "Name must not be blank." );
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.CodedNotation ), "Code must not be blank." );
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.Description ), "Description must not be blank." );

			//Duplicate checks
			DuplicateCheck( "Rating", context => context.Rating.Where( m => m.RowId != entity.RowId ), errors, new List<StringCheckMapping<DBEntity>>()
			{
				new StringCheckMapping<DBEntity>( entity.Name, dbEnt => CompareStrings( entity.Name, dbEnt.Name ), "Name", null ),
				new StringCheckMapping<DBEntity>( entity.CodedNotation, dbEnt => CompareStrings( entity.CodedNotation, dbEnt.CodedNotation ), "Code", null ),
				new StringCheckMapping<DBEntity>( entity.Description, dbEnt => CompareStrings( entity.Description, dbEnt.Description ), "Description", null )
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
				BasicSaveCore( context, entity, context.Rating, userID, ( ent, dbEnt ) => { }, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

		public static DeleteResult DeleteById( int id )
		{
			return BasicDeleteCore( "Rating", context => context.Rating, id, "> RatingId > Rating", ( context, list, target ) =>
			{
				//Check for references from Cluster Analysis objects
				var clusterAnalysisContextCount = context.ClusterAnalysis.Where( m => m.HasRatingId == id ).Count();
				if ( clusterAnalysisContextCount > 0 )
				{
					return new DeleteResult( false, "This Rating is referenced by " + clusterAnalysisContextCount + " Cluster Analysis objects, so it cannot be deleted." );
				}

				return null;
			} );
		}
		//

		#endregion

		#region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.Rating, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

		public static AppEntity GetByName( string name, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.Name?.ToLower() == name?.ToLower(), returnNullIfNotFound );
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

		public static List<AppEntity> GetAll()
		{
			return GetItemList( context => context.Rating.OrderBy( m => m.CodedNotation ), MapFromDB, false );
		}
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			return GetMultipleByFilter( context => context.Rating, m => guids.Contains( m.RowId ), m => m.CodedNotation, false, MapFromDB, false );
		}
		//

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
		{
            //TODO - where called for the drop down in the rmtl search should have an additional filter like 'hasRMTL'
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.Rating.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Name.Contains( keywords ) ||
						m.CodedNotation.Contains( keywords )
						//m.Description.Contains( keywords ) //Leads to too many false positive matches
					);
				}

				//Enable filtering to just Ratings that have data
				AppendNotNullFilterIfPresent( query, "< RatingId < RatingContext:NotNull", () => {
					list = list.Where( m => context.RatingContext.Where( n => n.Rating == m ).Count() > 0 );
				} );

				//Exclude items (Used in the RMTL Search Filters)
				AppendIDsFilterIfPresent( query, "search:Exclude", ( ids ) =>
				{
					list = list.Where( m => !ids.Contains( m.Id ) );
				} );

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.Name, m => m.OrderBy( n => n.CodedNotation ), ( m, keywordParts ) => m.OrderBy( n => RelevanceHelper( n, keywordParts, o => o.CodedNotation ) + RelevanceHelper( n, keywordParts, o => o.Name ) ), keywords );

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

		public static void MapFromDB( ViewEntity input, AppEntity output )
        {
            //
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }
        }
		//

        #endregion

    }
}
