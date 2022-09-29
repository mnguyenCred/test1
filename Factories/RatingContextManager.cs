using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Application;
using Models.Curation;
using Models.Search;

using Navy.Utilities;

using AppEntity = Models.Schema.RatingContext;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.RatingContext; 

namespace Factories
{
	public class RatingContextManager : BaseFactory
	{
		public static new string thisClassName = "RatingContextManager";
		public static string cacheKey = "RatingContextCache";

		#region === persistance ==================
		/// <summary>
		/// Update a RatingContext
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Save( AppEntity entity, ref ChangeSummary status )
		{
			throw new NotImplementedException();
		}
		//

		/// <summary>
		/// Add a RatingContext
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		private int Add( AppEntity entity, ref ChangeSummary status )
		{
			throw new NotImplementedException();
		}
		//

		public void UpdateParts( AppEntity input, ChangeSummary status )
		{
			throw new NotImplementedException();
		}
		//

		public static void MapToDB( AppEntity input, DBEntity output )
		{
			throw new NotImplementedException();
		}
		//

		#endregion

		#region Retrieval
		public static AppEntity Get( int id )
		{
			throw new NotImplementedException();
		}
		//

		public static AppEntity Get( Guid rowId )
		{
			throw new NotImplementedException();
		}
		//

		public static AppEntity GetByCTIDOrNull( string ctid )
		{
			throw new NotImplementedException();
		}
		//

		public static List<AppEntity> GetAll()
		{
			throw new NotImplementedException();
		}
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			throw new NotImplementedException();
		}
		//

		public static List<AppEntity> GetAllForRating( Guid ratingRowId )
		{
			throw new NotImplementedException();
		}
		//

		public static List<AppEntity> GetAllForRatingTask( Guid ratingTaskRowId )
		{
			throw new NotImplementedException();
		}
		//

		public static List<AppEntity> Search( SearchQuery query )
		{
			throw new NotImplementedException();
		}
		//

		public static void MapFromDB( DBEntity input, AppEntity output )
		{
			throw new NotImplementedException();
		}
		//
		#endregion
	}
}
