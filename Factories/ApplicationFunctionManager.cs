using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataEntities = Data.Tables.NavyRRLEntities;
using AppEntity = Models.Application.ApplicationFunction;
using DBEntity = Data.Tables.ApplicationFunction;
using Models.Search;

namespace Factories
{
	public class ApplicationFunctionManager : BaseFactory
	{
		public static new string thisClassName = "ApplicationFunctionManager";

		#region Persistence

		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			//Validate required fields
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.Name ), "Name must not be empty." );
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.Description ), "Description must not be empty." );
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.CodedNotation ), "Coded Notation must not be empty." );

			//Return if any errors
			if (errors.Count() > 0 )
			{
				return;
			}

			SaveCore( entity, userID, "Edit", errors.Add );
		}
		//

		public static void SaveCore( AppEntity entity, int userID, string saveType, Action<string> AddErrorMethod )
		{
			using( var context = new DataEntities() )
			{
				//Get the existing data or create it
				var existing = context.ApplicationFunction.FirstOrDefault( m => m.Id == entity.Id );
				if( existing == null )
				{
					existing = new DBEntity();
					context.ApplicationFunction.Add( existing );
				}

				//Apply the changes
				AutoMap( entity, existing );

				//Save the changes
				try
				{
					context.SaveChanges();
				}
				catch ( Exception ex )
				{
					AddErrorMethod( ex.Message + ( string.IsNullOrWhiteSpace( ex.InnerException?.Message ) ? "" : "; " + ex.InnerException.Message ) );
				}
			}
		}
		//

		#endregion

		#region Retrieval

		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			using ( var context = new DataEntities() )
			{
				var value = context.ApplicationFunction.FirstOrDefault( FilterMethod );

				return value == null && returnNullIfNotFound ? null : MapFromDB( value, context );
			}
		}
		//

		public static AppEntity GetByCodedNotation( string codedNotation, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.CodedNotation?.ToLower() == codedNotation?.ToLower(), returnNullIfNotFound );
		}
		//

		public static AppEntity GetByName( string name, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.Name.ToLower() == name.ToLower(), returnNullIfNotFound );
		}
		//

		public static AppEntity GetById( int id, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.Id == id, returnNullIfNotFound );
		}
		//

		public static List<AppEntity> GetAll()
		{
			using ( var context = new DataEntities() )
			{
				var results = context.ApplicationFunction.ToList().Select( m => MapFromDB( m, context ) ).ToList();
				return results;
			}
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
