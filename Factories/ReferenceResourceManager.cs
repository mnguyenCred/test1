using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppEntity = Models.Schema.ReferenceResource;
using DBEntity = Data.Tables.ReferenceResource;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Models.Application;
using Models.Curation;

using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class ReferenceResourceManager : BaseFactory
    {
        public static new string thisClassName = "ReferenceResourceManager";

		#region ===  Persistance ==================
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
				BasicSaveCore( context, entity, context.ReferenceResource, userID, ( ent, dbEnt ) => { }, ( ent, dbEnt ) => {
					HandleMultiValueUpdate( context, userID, ent.ReferenceType, dbEnt, dbEnt.ReferenceResource_ReferenceType, context.ConceptScheme_Concept, nameof( ReferenceResource_ReferenceType.ReferenceResourceId ), nameof( ReferenceResource_ReferenceType.ReferenceTypeId ) );
				}, saveType, AddErrorMethod );
			}
		}
		//

        #endregion

        #region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.ReferenceResource, FilterMethod, MapFromDB, returnNullIfNotFound );
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

		public static AppEntity GetForUploadOrNull( string name, string publicationDate )
		{
			using ( var context = new DataEntities() )
			{
				var match = context.ReferenceResource.FirstOrDefault( m =>
					m.Name.ToLower() == name.ToLower() &&
					m.PublicationDate.ToLower() == publicationDate.ToLower()
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
			return GetItemList( context => context.ReferenceResource.OrderBy( m => m.Name ), MapFromDB, false );
        }
		//

        public static SearchResultSet<AppEntity> Search( SearchQuery query )
        {
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.ReferenceResource.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Name.Contains( keywords ) ||
						m.CodedNotation.Contains( keywords ) ||
						m.Description.Contains( keywords )
					);
				}

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.Name, m => m.OrderBy( n => n.Name ).ThenBy( n => n.PublicationDate ) );

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
			output.ReferenceType = input.ReferenceResource_ReferenceType?.Select( m => m.ConceptScheme_Concept_ReferenceType ).Select( m => m.RowId ).ToList() ?? new List<Guid>();

			return output;
		}
		//

        #endregion
    }
}
