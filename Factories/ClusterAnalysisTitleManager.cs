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
using DBEntity = Models.Schema.ClusterAnalysisTitle; //TODO: Create Data.Tables class for this!

namespace Factories
{
	public class ClusterAnalysisTitleManager : BaseFactory
	{
		public static new string thisClassName = "ClusterAnalysisTitleManager";
		public static string cacheKey = "ClusterAnalysisTitleCache";

		#region === Persistance ==================

		/// <summary>
		/// Save a ClusterAnalysisTitle
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Save( AppEntity entity, ref ChangeSummary status )
		{
			//TODO: flesh this out
			return true;
		}
		//

		public static void MapToDB( AppEntity input, DBEntity output )
		{
			//watch for missing properties like rowId
			List<string> errors = new List<string>();
			BaseFactory.AutoMap( input, output, errors );
		}
		//

		#endregion

		#region Retrieval

        public static AppEntity Get( Guid rowId )
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
				/*
                var item = context.ClusterAnalysisTitle
                            .FirstOrDefault( s => s.RowId == rowId );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
				*/
            }
            return entity;
        }
		//

        public static AppEntity Get( int id )
        {
            var entity = new AppEntity();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
				/*
                var item = context.ClusterAnalysisTitle
							.SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
				*/
            }

            return entity;
        }
		//

		public static AppEntity GetByCTIDOrNull( string ctid )
		{
			if ( string.IsNullOrWhiteSpace( ctid ) )
			{
				return null;
			}

			using ( var context = new DataEntities() )
			{
				/*
				var item = context.ClusterAnalysisTitle
							.SingleOrDefault( s => s.CTID == ctid );

				if ( item != null && item.Id > 0 )
				{
					var entity = new AppEntity();
					MapFromDB( item, entity );
					return entity;
				}
				*/
			}

			return null;
		}
		//

		/// <summary>
		/// Get All 
		/// </summary>
		/// <returns></returns>
		public static List<AppEntity> GetAll()
        {
            var entity = new AppEntity();
            var list = new List<AppEntity>();

            list = new List<AppEntity>();
            using ( var context = new DataEntities() )
            {
				/*
                var results = context.ClusterAnalysisTitle
                        .OrderBy( s => s.Name )
                        .ToList();
                if ( results?.Count > 0 )
                {
                    foreach ( var item in results )
                    {
                        if ( item != null && item.Id > 0 )
                        {
                            entity = new AppEntity();
                            MapFromDB( item, entity );
                            list.Add( ( entity ) );
                        }
                    }
                    AddToCache( list );
                }
				*/
            }
            return list;
        }
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			var results = new List<AppEntity>();

			using ( var context = new DataEntities() )
			{
				/*
				var items = context.ClusterAnalysisTitle
					.Where( m => guids.Contains( m.RowId ) )
					.OrderBy( m => m.Description )
					.ToList();

				foreach ( var item in items )
				{
					var result = new AppEntity();
					MapFromDB( item, result );
					results.Add( result );
				}
				*/
			}

			return results;
		}
		//

		public static List<AppEntity> Search( SearchQuery query )
		{
			var output = new List<AppEntity>();
			var skip = ( query.PageNumber - 1 ) * query.PageSize;
			try
			{
				using ( var context = new DataEntities() )
				{
					/*
					//Start query
					var list = context.ClusterAnalysisTitle.AsQueryable();

					//Handle keywords filter
					var keywordsText = query.GetFilterTextByName( "search:Keyword" )?.ToLower();
					if ( !string.IsNullOrWhiteSpace( keywordsText ) )
					{
						list = list.Where( s =>
							s.Name.ToLower().Contains( keywordsText )
						);
					}

					//Get total
					query.TotalResults = list.Count();

					//Sort
					list = list.OrderBy( p => p.Description );

					//Get page
					var results = list.Skip( skip ).Take( query.PageSize )
						.Where( m => m != null ).ToList();

					//Populate
					foreach ( var item in results )
					{
						var entity = new AppEntity();
						MapFromDB( item, entity );
						output.Add( entity );
					}
					*/
				}

				return output;
			}
			catch ( Exception ex )
			{
				return new List<AppEntity>() { new AppEntity() { Name = "Error: " + ex.Message + " - " + ex.InnerException?.Message } };
			}
		}
		//

		//Should probably use a single generic method(?)
		public static List<AppEntity> CheckCache()
		{
			return null;
		}
		//

		//Should probably use a single generic method(?)
		public static void AddToCache( List<AppEntity> input )
		{

		}
		//

		public static void MapFromDB( DBEntity input, AppEntity output )
		{
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
