using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataEntities = Data.Tables.NavyRRLEntities;
using AppEntity = Models.Application.ApplicationRole;
using DBEntity = Data.Tables.ApplicationRole;
using Models.Search;
using Models.Curation;
using Newtonsoft.Json.Linq;

namespace Factories
{
	public class ApplicationRoleManager : BaseFactory
	{
		#region Persistence

		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			//Validate required fields
			AddErrorIf( errors, string.IsNullOrWhiteSpace( entity.Name ), "Name must not be empty." );
			AddErrorIf( errors, GetSingleByFilter( m => m.Id != entity.Id && m.Name.ToLower() == ( entity.Name?.ToLower() ?? "" ), true ) != null, "Another Role with that name already exists." );

			//Return if any errors
			if ( errors.Count() > 0 )
			{
				return;
			}

			SaveCore( entity, userID, "Edit", errors.Add );
		}
		//

		public static void SaveCore( AppEntity entity, int userID, string saveType, Action<string> AddErrorMethod )
		{
			using ( var context = new DataEntities() )
			{
				//Save the changes
				try
				{
					//Get the existing data or create it
					var existing = context.ApplicationRole.FirstOrDefault( m => m.Id == entity.Id );
					if ( existing == null )
					{
						existing = new DBEntity();
						context.ApplicationRole.Add( existing );
					}

					//Apply the changes
					AutoMap( entity, existing );
					context.SaveChanges(); //Call this now to ensure an ID is assigned in the event that the item is new

					//Add and remove function associations as needed
					var currentFunctionIDs = existing.AppFunctionPermission.Select( m => m.ApplicationFunctionId ).ToList();

					var addFunctionIDs = entity.HasApplicationFunctionIds.Where( m => !currentFunctionIDs.Contains( m ) ).ToList();
					foreach( var functionID in addFunctionIDs )
					{
						context.AppFunctionPermission.Add( new Data.Tables.AppFunctionPermission() { ApplicationFunctionId = functionID, RoleId = existing.Id } );
					}

					var removeFunctionIDs = currentFunctionIDs.Where( m => !entity.HasApplicationFunctionIds.Contains( m ) ).ToList();
					var removeFunctions = context.AppFunctionPermission.Where( m => m.RoleId == existing.Id && removeFunctionIDs.Contains( m.ApplicationFunctionId ) ).ToList();
					foreach( var functionItem in removeFunctions )
					{
						context.AppFunctionPermission.Remove( functionItem );
					}

					//Save the changes again
					context.SaveChanges();
				}
				catch ( Exception ex )
				{
					AddErrorMethod( ex.Message + ( string.IsNullOrWhiteSpace( ex.InnerException?.Message ) ? "" : "; " + ex.InnerException.Message ) );
				}
			}
		}
		//

		public static DeleteResult DeleteById( int id )
		{
			var result = new DeleteResult();
			using( var context = new DataEntities() )
			{
				//Get the item
				var toBeDeleted = context.ApplicationRole.FirstOrDefault( m => m.Id == id );
				AddErrorIf( result.Messages, toBeDeleted == null, "No item found with ID " + id + "." );

				//Run any pre-delete checks that are specific to the caller of this method
				var referencesCount = context.ApplicationUserRole.Where( m => m.ApplicationRole.Id == id ).Count();
				AddErrorIf( result.Messages, referencesCount > 0, "Unable to delete the target Application Role: It is associated with " + referencesCount + " users." );

				//Other checks TBD

				//Return if any error messages
				if( result.Messages.Count() > 0 )
				{
					return result;
				}

				//Try to delete it
				try
				{
					//First delete the associations between this role and its functions
					var removeFunctions = context.AppFunctionPermission.Where( m => m.RoleId == toBeDeleted.Id ).ToList();
					foreach ( var functionItem in removeFunctions )
					{
						context.AppFunctionPermission.Remove( functionItem );
					}

					//Then delete the role itself
					context.ApplicationRole.Remove( toBeDeleted );
					context.SaveChanges();

					return new DeleteResult( true, "The target Application Role was successfully deleted." );
				}
				catch ( Exception ex )
				{
					return new DeleteResult( false, "Error deleting the target Application Role: " + ex.Message + ( !string.IsNullOrWhiteSpace( ex.InnerException?.Message ) ? "; " + ex.InnerException.Message : "" ), JObject.FromObject( ex ) );
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
				var value = context.ApplicationRole.FirstOrDefault( FilterMethod );

				return value == null && returnNullIfNotFound ? null : MapFromDB( value, context );
			}
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
				var results = context.ApplicationRole.ToList().Select( m => MapFromDB( m, context ) ).ToList();
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
			output.HasApplicationFunctionIds = input.AppFunctionPermission.Select( m => m.ApplicationFunctionId ).ToList();

			return output;
		}
		//

		#endregion
	}
}
