using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SM=Models.Schema;
using AppEntity = Models.Schema.RMTLProject;
using DBEntity = Data.Tables.RMTLProject;
using ViewEntity = Data.Views.RMTLProjectSummary;
using Models.Curation;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Models.Application;
using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class RMTLProjectManager : BaseFactory
    {
        public static new string thisClassName = "RMTLProjectManager";

		#region RMTLProject - persistance - NOT Likely? ==================
		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			SaveCore( entity, userID, "Edit", errors.Add );
		}
		//

		private static void SaveCore( AppEntity entity, int userID, string saveType, Action<string> AddErrorMethod )
		{
			using ( var context = new DataEntities() )
			{
				BasicSaveCore( context, entity, context.RMTLProject, userID, ( ent, dbEnt ) => { }, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

		#endregion

		#region Retrieval

		public static List<AppEntity> GetAll()
		{
			return GetItemList( context => context.RMTLProject.OrderBy( m => m.Id ), MapFromDB, false );
		}
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			return GetMultipleByFilter( context => context.RMTLProject, m => guids.Contains( m.RowId ), m => m.Id, false, MapFromDB, false );
		}
		//

		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( context => context.RMTLProject, FilterMethod, MapFromDB, returnNullIfNotFound );
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
				var list = context.RMTLProject.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Name.Contains( keywords ) ||
						m.Description.Contains( keywords )
					);
				}

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.Name, m => m.OrderBy( n => n.Name ) );

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
			output.HasRating = context.Rating.FirstOrDefault( m => m.Id == output.RatingId )?.RowId ?? Guid.Empty;

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

		#region View-Based methods
		/// <summary>
		/// Get all 
		/// May need a get all for a rating? Should not matter as this is external data?
		/// </summary>
		/// <returns></returns>
		public static List<AppEntity> GetAllSummary()
		{
			var entity = new AppEntity();
			var list = new List<AppEntity>();

			using ( var context = new ViewContext() )
			{
				var results = context.RMTLProjectSummary
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
				}

			}
			return list;
		}

		public static List<AppEntity> SearchByView( SearchQuery query )
		{
			var output = new List<AppEntity>();
			var keywords = GetSanitizedSearchFilterKeywords( query );

			try
			{
				using ( var context = new ViewContext() )
				{
					var list = from Results in context.RMTLProjectSummary
							   select Results;
					if ( !string.IsNullOrWhiteSpace( keywords ) )
					{
						list = from Results in list
								.Where( s => ( s.Name.ToLower().Contains( keywords.ToLower() ) ) )
							   select Results;
					}

					//query.TotalResults = list.Count();
					//sort order not handled
					list = list.OrderBy( p => p.Name );

					//
					var results = list.Skip( query.Skip ).Take( query.Take )
						.ToList();
					if ( results?.Count > 0 )
					{
						foreach ( var item in results )
						{
							if ( item != null && item.Id > 0 )
							{
								var entity = new AppEntity();
								MapFromDB( item, entity );
								output.Add( entity );
							}
						}
					}

				}
			}
			catch ( Exception ex )
			{

			}
			return output;
		}
		//
		#endregion

	}
}
