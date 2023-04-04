using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using Models.Application;

using DataEntities = Data.Tables.NavyRRLEntities;
using EM = Data.Tables;
using ViewContext = Data.Views.ceNavyViewEntities;
using Views = Data.Views;
using Navy.Utilities;
using Models.Utilities;

namespace Factories
{
    public class AccountManager : BaseFactory
	{
		static new string thisClassName = "AccountManager";
		//AspNetRolesids 
		static int Administrator = 1;
		static int SiteManager = 2;
		static int SiteStaff = 3;

		static int RMTLDeveloper = 10;
		static int RCDAnalyst = 11;
		static int RatingContinuumManager = 13;
		static int RMTLSME = 13;

		static int SiteReader = 20;

		//AspNetRolesids 
		public static string ROLE_ADMINISTRATOR = "Administrator";
		public static string ROLE_SITE_MANAGER = "Site Manager";
		public static string ROLE_SITE_STAFF = "Site Staff";
		public static string ROLE_SITE_READER = "Site Reader";

		public static string ROLE_RMTL_DEVELOPER = "RMTL Developer";
		public static string ROLE_RATING_CONTINUUM_MANAGER = "Rating Continuum Manager";
		public static string ROLE_RATING_CONTINUUM_DEVELOPMENT_ANALYST = "Rating Continuum Development Analyst";
		public static string ROLE_RMTL_SME = "RMTL Subject Matter Expert";


		static string SessionLoginProxy = "Session Login Proxy";

		#region persistance 
		public int Add( AppUser entity, ref string statusMessage )
		{
			EM.Account efEntity = new EM.Account();
			using ( var context = new DataEntities() )
			{
				try
				{

					AppUser_MapToDB( entity, efEntity );
					efEntity.RowId = Guid.NewGuid();

					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Account.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = string.Format( "AccountManager. Account_Add Failed", "Attempted to add a Account. The process appeared to not work, but there was not an exception, so we have no message, or no clue.Email: {0}", entity.Email );
						EmailManager.NotifyAdmin( "	Manager. Account_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".Account_Add() DbEntityValidationException, Email: {0}", efEntity.Email );
					foreach ( var eve in dbex.EntityValidationErrors )
					{
						message += string.Format( "\rEntity of type \"{0}\" in state \"{1}\" has the following validation errors:",
							eve.Entry.Entity.GetType().Name, eve.Entry.State );
						foreach ( var ve in eve.ValidationErrors )
						{
							message += string.Format( "- Property: \"{0}\", Error: \"{1}\"",
								ve.PropertyName, ve.ErrorMessage );
						}

						LoggingHelper.LogError( message, true );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_Add(), Email: {0}", efEntity.Email ) );
				}
			}

			return efEntity.Id;
		}

		public int Account_AddFromAspNetUser( string aspNetId, AppUser entity, ref string statusMessage )
		{
			EM.Account efEntity = new EM.Account();
			using ( var context = new DataEntities() )
			{
				try
				{
					EM.AspNetUsers user = AspNetUser_Get( entity.Email );

					AppUser_MapToDB( entity, efEntity );

					efEntity.RowId = Guid.NewGuid();

					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Account.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = string.Format( "AccountManager. Account_Add Failed", "Attempted to add a Account. The process appeared to not work, but there was not an exception, so we have no message, or no clue.Email: {0}", entity.Email );
						EmailManager.NotifyAdmin( "	Manager. Account_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".Account_Add() DbEntityValidationException, Email: {0}", efEntity.Email );
					foreach ( var eve in dbex.EntityValidationErrors )
					{
						message += string.Format( "\rEntity of type \"{0}\" in state \"{1}\" has the following validation errors:",
							eve.Entry.Entity.GetType().Name, eve.Entry.State );
						foreach ( var ve in eve.ValidationErrors )
						{
							message += string.Format( "- Property: \"{0}\", Error: \"{1}\"",
								ve.PropertyName, ve.ErrorMessage );
						}

						LoggingHelper.LogError( message, true );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_Add(), Email: {0}", efEntity.Email ) );
				}
			}

			return efEntity.Id;
		}

		public bool Update( AppUser entity, ref string statusMessage )
		{
			using ( var context = new DataEntities() )
			{
				try
				{
					EM.Account efEntity = context.Account
							.SingleOrDefault( s => s.Id == entity.Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						AppUser_MapToDB( entity, efEntity );
						context.Entry( efEntity ).State = System.Data.Entity.EntityState.Modified;

						if ( HasStateChanged( context ) )
						{
							efEntity.LastUpdated = System.DateTime.Now;

							// submit the change to database
							int count = context.SaveChanges();
							if ( count > 0 )
							{
								statusMessage = "successful";
								//arbitrarily update AspNetUsers???
								//soon to be obsolete?
								AspNetUsers_Update( entity, ref statusMessage );

								return true;
							}
							else
							{
								//?no info on error
								statusMessage = "Error - the update was not successful. ";
								string message = string.Format( thisClassName + ".Account_Update Failed", "Attempted to uddate a Account. The process appeared to not work, but there was not an exception, so we have no message, or no clue.Email: {0}", entity.Email );
								EmailManager.NotifyAdmin( thisClassName + ". Account_Update Failed", message );
							}
						}
					}

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_Update(), Email: {0}", entity.Email ) );
				}
			}

			return false;
		}

		public bool Delete( int userId, AppUser deletedBy, ref string statusMessage )
		{
			bool isValid = false;
			using ( var context = new DataEntities() )
			{
				try
				{
					EM.Account efEntity = context.Account
							.SingleOrDefault( s => s.Id == userId );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						var email = efEntity.Email;
						efEntity.IsActive = false;
						efEntity.LastUpdated = System.DateTime.Now;
						//23-02-24 - NOTE we cannot do a physical delete as userId could be an FK to other tables
						//context.Account.Remove( efEntity );

                        int count = context.SaveChanges();
						//add activity log (usually in caller method)
						if ( count >= 0 )
						{
							isValid = true;

							//need to handle AspNetUsers
							//AspNetUsers_Delete( email, ref statusMessage );

                            //AspNetUsers_LockOut( efEntity.AspNetId, ref statusMessage );

                            //new ActivityManager().SiteActivityAdd( new SiteActivity()
                            //{
                            //	ActivityType = "Account",
                            //	Activity = "Management",
                            //	Event = "Delete",
                            //	Comment = string.Format( "{0} set user: '{1}' inactive.", deletedBy.Email, efEntity.Email )
                            //} );
                        }
					}
					else
					{
						statusMessage = "Error/Warning - user was not found.";
						isValid = true;
					}


				}
				catch ( Exception ex )
				{
					//LoggingHelper.LogError( ex, thisClassName + string.Format( ".Delete(), Email: {0}", entity.Email ) );
				}
			}

			return isValid;
		}
		//

		public static Attempt<AppUser> GetUserFromAPIKey( string apiKey )
		{
			try
			{
				return GetUserFromAPIKey( Guid.Parse( apiKey ) );
			}
			catch
			{
				return new Attempt<AppUser>( null, false, "Missing or Invalid API Key." );
			}
		}
		//

		public static Attempt<AppUser> GetUserFromAPIKey( Guid apiKey )
		{
			using( var context = new DataEntities() )
			{
				var keyHolder = context.APIKey.FirstOrDefault( m => m.API_Key == apiKey );
				if( keyHolder == null )
				{
					return new Attempt<AppUser>( null, false, "Missing or Invalid API Key." );
				}
				else
				{
					var user = Get( keyHolder.UserId );
					if( user == null || user.Id == 0 )
					{
						return new Attempt<AppUser>( null, false, "API Key was found, but no matching user was found." );
					}
					else
					{
						return new Attempt<AppUser>( user, true );
					}
				}
			}
		}
		//

		public static Attempt<Guid> GetAPIKeyForUser( int userID )
		{
			using( var context = new DataEntities() )
			{
				var keyHolder = context.APIKey.FirstOrDefault( m => m.UserId == userID );
				if( keyHolder == null )
				{
					return GenerateNewAPIKeyForUser( userID );
				}
				else
				{
					return new Attempt<Guid>( keyHolder.API_Key, true );
				}
			}
		}
		//

		public static Attempt<Guid> GenerateNewAPIKeyForUser( int userID )
		{
			using ( var context = new DataEntities() )
			{
				var keyHolder = context.APIKey.FirstOrDefault( m => m.UserId == userID );
				if ( keyHolder == null )
				{
					keyHolder = new EM.APIKey() { 
						UserId = userID, 
						RowId = Guid.NewGuid(),
						API_Key = Guid.NewGuid(),
						Created = DateTime.Now,
						LastUpdated = DateTime.Now
					};
					context.APIKey.Add( keyHolder );
				}
				else
				{
					keyHolder.API_Key = Guid.NewGuid();
					keyHolder.LastUpdated = DateTime.Now;
				}

				try
				{
					context.SaveChanges();
					return new Attempt<Guid>( keyHolder.API_Key, true );
				}
				catch( Exception ex )
				{
					return new Attempt<Guid>( Guid.Empty, false, "Error generating new API Key for user: " + ex.Message );
				}
			}
		}
		//

		public static Attempt<bool> DeleteAPIKeyForUser( int userID )
		{
			using( var context = new DataEntities() )
			{
				var keyHolder = context.APIKey.FirstOrDefault( m => m.UserId == userID );
				if( keyHolder == null )
				{
					return new Attempt<bool>( false, true, "No API Key exists for that user." );
				}
				else
				{
					try
					{
						context.APIKey.Remove( keyHolder );
						context.SaveChanges();
						return new Attempt<bool>( true, true, "API Key deleted successfully." );
					}
					catch( Exception ex )
					{
						return new Attempt<bool>( false, false, "Error deleting API Key for user: " + ex.Message );
					}
				}
			}
		}
		//
		#endregion


		#region ====== AspNetUsers ======

		public static bool AspNetUsers_Update( AppUser entity, ref string statusMessage )
		{
			using ( var context = new DataEntities() )
			{
				try
				{
					EM.AspNetUsers efEntity = context.AspNetUsers
							.SingleOrDefault( s => s.Id == entity.AspNetUserId );

					if ( efEntity != null && efEntity.UserId > 0 )
					{
						efEntity.FirstName = entity.FirstName;
						efEntity.LastName = entity.LastName;
						efEntity.Email = entity.Email;
						efEntity.EmailConfirmed = true;
						//could be dangerous, as hidden??
						//efEntity.UserName = entity.UserName;

						if ( HasStateChanged( context ) )
						{
							// submit the change to database
							int count = context.SaveChanges();
							if ( count > 0 )
							{
								statusMessage = "successful";
								return true;
							}
							else
							{
								//?no info on error
								statusMessage = "Error - the update was not successful. ";
								string message = string.Format( thisClassName + ".AspNetUsers_Update Failed", "Attempted to update AspNetUsers (sync with Account). The process appeared to not work, but there was not an exception, so we have no message, or no clue.Email: {0}", entity.Email );
								EmailManager.NotifyAdmin( thisClassName + "AspNetUsers_Update Failed", message );
							}
						}

					}

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".AspNetUsers_Update(), Email: {0}", entity.Email ) );
				}
			}

			return false;
		}
		public static bool AspNetUsers_LockOut( string aspNetUserId, ref string statusMessage )
		{
			using ( var context = new DataEntities() )
			{
				try
				{
					EM.AspNetUsers efEntity = context.AspNetUsers
							.SingleOrDefault( s => s.Id == aspNetUserId );

					if ( efEntity != null && efEntity.UserId > 0 )
					{
						efEntity.LockoutEnabled = true;

						if ( HasStateChanged( context ) )
						{
							// submit the change to database
							int count = context.SaveChanges();
							if ( count > 0 )
							{
								statusMessage = "successful";
								return true;
							}
							else
							{
								//?no info on error
								statusMessage = "Error - the lockout was not successful. ";
								string message = string.Format( thisClassName + ".AspNetUsers_LockOut Failed", "Attempted to update AspNetUsers for lockout. The process appeared to not work, but there was not an exception, so we have no message, or no clue.Email: {0}", efEntity.Email );
								EmailManager.NotifyAdmin( thisClassName + "AspNetUsers_LockOut Failed", message );
							}
						}

					}

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".AspNetUsers_LockOut(), aspNetUserId: {0}", aspNetUserId ) );
				}
			}

			return false;
		}
        public static bool AspNetUsers_Delete( string email, ref string statusMessage )
        {
            using ( var context = new DataEntities() )
            {
                try
                {
                    EM.AspNetUsers efEntity = context.AspNetUsers
                            .FirstOrDefault( s => s.Email == email );

                    if ( efEntity != null && efEntity.UserId > 0 )
                    {
                        context.AspNetUsers.Remove( efEntity );
						int count = context.SaveChanges();
                        if ( count >= 0 )
                        {
                            statusMessage = "";
                            return true;
                        }
                        else
                        {
                            //?no info on error
                            statusMessage = "Error - the lockout was not successful. ";
                            string message = string.Format( thisClassName + ".AspNetUsers_LockOut Failed", "Attempted to update AspNetUsers for lockout. The process appeared to not work, but there was not an exception, so we have no message, or no clue.Email: {0}", efEntity.Email );
                            EmailManager.NotifyAdmin( thisClassName + "AspNetUsers_LockOut Failed", message );
                        }
                     

                    }

                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".AspNetUsers_LockOut(), aspNetUserId: {0}", email ) );
                }
            }

            return false;
        }

        public bool AspNetUsers_UpdateEmailConfirmed( string id, ref string statusMessage )
		{
			using ( var context = new DataEntities() )
			{
				EM.AspNetUsers efEntity = context.AspNetUsers
							.SingleOrDefault( s => s.Id == id );
				try
				{
					if ( efEntity != null && efEntity.UserId > 0 )
					{
						efEntity.EmailConfirmed = true;

						if ( HasStateChanged( context ) )
						{
							// submit the change to database
							int count = context.SaveChanges();
							if ( count > 0 )
							{
								statusMessage = "successful";
								return true;
							}
							else
							{
								//?no info on error
								statusMessage = "Error - the update of EmailConfirmed was not successful. ";
								string message = string.Format( thisClassName + ".AspNetUsers_Update Failed", "Attempted to update AspNetUsers.EmailConfirmed. The process appeared to not work, but there was not an exception, so we have no message, or no clue.Email: {0}", efEntity.Email );
								EmailManager.NotifyAdmin( thisClassName + "AspNetUsers_UpdateEmailConfirmed Failed", message );
							}
						}
						return true;
					}

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".AspNetUsers_UpdateEmailConfirmed(), Email: {0}", efEntity.Email ) );
				}
			}

			return false;
		}
		public bool AspNetUsers_UpdateEmailConfirmedByEmail( string email, ref string statusMessage )
		{
			using ( var context = new DataEntities() )
			{
				EM.AspNetUsers efEntity = context.AspNetUsers
							.SingleOrDefault( s => s.Email == email );
				try
				{
					if ( efEntity != null && efEntity.UserId > 0 )
					{
						efEntity.EmailConfirmed = true;

						if ( HasStateChanged( context ) )
						{
							// submit the change to database
							int count = context.SaveChanges();
							if ( count > 0 )
							{
								statusMessage = "successful";
								return true;
							}
							else
							{
								//?no info on error
								statusMessage = "Error - the update of EmailConfirmed was not successful. ";
								string message = string.Format( thisClassName + ".AspNetUsers_UpdateEmailConfirmedByEmail Failed", "Attempted to update AspNetUsers.EmailConfirmed. The process appeared to not work, but there was not an exception, so we have no message, or no clue.Email: {0}", efEntity.Email );
								EmailManager.NotifyAdmin( thisClassName + "AspNetUsers_UpdateEmailConfirmedByEmail Failed", message );
							}
						}
						return true;
					}

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".AspNetUsers_UpdateEmailConfirmedByEmail(), Email: {0}", efEntity.Email ) );
				}
			}

			return false;
		}


		public static EM.AspNetUsers AspNetUser_Get( string email )
		{
			EM.AspNetUsers entity = new EM.AspNetUsers();
			using ( var context = new DataEntities() )
			{
				entity = context.AspNetUsers
							.SingleOrDefault( s => s.Email.ToLower() == email.ToLower() );

				if ( entity != null && entity.Email != null && entity.Email.Length > 5 )
				{

				}
			}

			return entity;
		}

		#endregion

		#region retrieval
		/// <summary>
		/// Get user using View: Account_Summary
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		public static AppUser AppUser_Get( int Id )
		{
			AppUser entity = new AppUser();
			using ( var context = new ViewContext() )
			{
				Views.Account_Summary item = context.Account_Summary.SingleOrDefault( s => s.Id == Id );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity );
				}
			}

			return entity;
		}
		public static AppUser Get( int Id )
		{
			AppUser entity = new AppUser();
			using ( var context = new DataEntities() )
			{
				//this method may be bringing back to much info - evaluate
				EM.Account item = context.Account
							.SingleOrDefault( s => s.Id == Id );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity );
				}
			}

			return entity;
		}
		public static AppUser AppUser_GetByEmail( string email )
		{
			AppUser entity = new AppUser();
			email = ( email ?? "" ).Trim();

			using ( var context = new DataEntities() )
			{
				EM.Account efEntity = context.Account
							.FirstOrDefault( s => s.Email.ToLower() == email.ToLower() );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					MapFromDB( efEntity, entity );
				}
			}

			return entity;
		}
		//public static AppUser AppUser_GetSummaryByEmail( string email )
		//{ 
		//    AppUser entity = new AppUser();
		//    using (var context = new ViewContext())
		//    {
		//        Views.Account_Summary efEntity = context.Account_Summary
		//                    .FirstOrDefault( s => s.Email.ToLower() == email.ToLower() );

		//        if (efEntity != null && efEntity.Id > 0)
		//        {
		//            MapFromDB( efEntity, entity );
		//        }
		//    }

		//    return entity;
		//}
		public static AppUser GetUserByCEAccountId( string accountIdentifier )
		{
			AppUser entity = new AppUser();
			using ( var context = new DataEntities() )
			{
				EM.Account efEntity = context.Account
							.FirstOrDefault( s => s.ExternalAccountIdentifier == accountIdentifier );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					MapFromDB( efEntity, entity );
				}
			}

			return entity;
		}
		public static AppUser GetUserByUserName( string username )
		{
			AppUser entity = new AppUser();
			using ( var context = new DataEntities() )
			{
				//not sure if there might be an issue where email changes, and username didn't or??
				EM.Account efEntity = context.Account
							.FirstOrDefault( s => s.UserName.ToLower() == username.ToLower()
							//||							s.Email.ToLower() == username.ToLower()
							);

				if ( efEntity != null && efEntity.Id > 0 )
				{
					MapFromDB( efEntity, entity );
				}
				else
				{
					//check via email?
					if ( username.IndexOf( "@" ) > 0 )
					{
						return AppUser_GetByEmail( username );
					}
				}
			}

			return entity;
		}

		public static List<Views.AspNetUserRoles_Summary> GetUserRoles( int userId )
		{
			using ( var context = new ViewContext() )
			{
				return context.AspNetUserRoles_Summary.Where( x => x.Id == userId ).ToList();
			}
		}

		public static AppUser AppUser_GetByKey( string aspNetId )
		{
			AppUser entity = new AppUser();
			using ( var context = new DataEntities() )
			{
				EM.Account efEntity = context.Account
							.FirstOrDefault( s => s.AspNetId == aspNetId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					MapFromDB( efEntity, entity );
				}
			}

			return entity;
		}
		public static AppUser AppUser_GetFromAspUser( string email )
		{
			AppUser entity = new AppUser();
			using ( var context = new DataEntities() )
			{
				EM.AspNetUsers efEntity = context.AspNetUsers
							.SingleOrDefault( s => s.Email.ToLower() == email.ToLower() );

				if ( efEntity != null && efEntity.Email != null && efEntity.Email.Length > 5 )
				{
					entity = AppUser_GetByEmail( efEntity.Email );
				}
			}

			return entity;
		}
		/**/
		public static List<AppUser> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows, int userId = 0 )
		{
			string connectionString = DBConnectionRO();
			AppUser item = new AppUser();
			List<AppUser> list = new List<AppUser>();
			var result = new DataTable();
			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}

				using ( SqlCommand command = new SqlCommand( "AccountSearch", c ) )
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add( new SqlParameter( "@Filter", pFilter ) );
					command.Parameters.Add( new SqlParameter( "@SortOrder", pOrderBy ) );
					command.Parameters.Add( new SqlParameter( "@StartPageIndex", pageNumber ) );
					command.Parameters.Add( new SqlParameter( "@PageSize", pageSize == -1 ? 0 : pageSize ) );
					command.Parameters.Add( new SqlParameter( "@CurrentUserId", userId ) );

					SqlParameter totalRows = new SqlParameter( "@TotalRows", pTotalRows );
					totalRows.Direction = ParameterDirection.Output;
					command.Parameters.Add( totalRows );

					try
					{
						using ( SqlDataAdapter adapter = new SqlDataAdapter() )
						{
							adapter.SelectCommand = command;
							adapter.Fill( result );
						}
						string rows = command.Parameters[5].Value.ToString();
						pTotalRows = Int32.Parse( rows );
					}
					catch ( Exception ex )
					{
						pTotalRows = 0;
						LoggingHelper.LogError( ex, thisClassName + string.Format( ".Search() - Execute proc, Message: {0} \r\n Filter: {1} \r\n", ex.Message, pFilter ) );

						item = new AppUser();
						item.FirstName = "Error";
						item.LastName = "Unexpected error encountered. System administration has been notified. Please try again later. ";
						item.Email = ex.Message;
						list.Add( item );
						return list;
					}
			}
				try
				{
					foreach ( DataRow dr in result.Rows )
					{
						item = new AppUser();
						item.Id = GetRowColumn( dr, "Id", 0 );
						item.FirstName = GetRowColumn( dr, "FirstName", "missing" );
						item.LastName = GetRowColumn( dr, "LastName", "" );
						string rowId = GetRowColumn( dr, "RowId" );
						item.RowId = new Guid( rowId );

						item.Email = GetRowColumn( dr, "Email", "" );
						item.Roles = GetRowColumn( dr, "Roles", "" );
						//item.OrgMbrs = GetRowColumn( dr, "OrgMbrs", "" );
						//item.lastLogon = GetRowColumn( dr, "lastLogon", "" );
						//if ( IsValidDate( item.lastLogon ) )
						//	item.lastLogon = item.lastLogon.Substring( 0, 10 );

						list.Add( item );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.DoTrace( 1, thisClassName + ".Search. Exception: " + ex.Message );
				}
				return list;

			}
		}
		
		public static void MapFromDB( Views.Account_Summary input, AppUser output )
		{
			output.Id = input.Id;
			//to.RowId = fromEntity.RowId;
			output.UserName = input.UserName;

			output.AspNetUserId = input.AspNetId;
			//to.Password = fromEntity.Password;
			output.IsActive = input.IsActive == null ? false : ( bool ) input.IsActive;
			output.FirstName = input.FirstName;
			output.LastName = input.LastName;
			output.SortName = input.SortName;

			output.Email = input.Email;
			output.ExternalAccountIdentifier = input.ExternalAccountIdentifier;

			if ( IsValidDate( input.Created ) )
				output.Created = (DateTime)input.Created;
			if ( IsValidDate( input.LastUpdated ) )
				output.LastUpdated = ( DateTime ) input.LastUpdated;
			output.LastUpdatedById = input.LastUpdatedById == null ? 0 : ( int ) input.LastUpdatedById;
			//if ( IsValidDate( fromEntity.lastLogon ) )
			//{
			//	to.lastLogon = ( ( DateTime ) fromEntity.lastLogon ).ToString( "yyyy-MM-dd" );
			//}
			//else
			//	to.lastLogon = "None";

			output.UserRoles = new List<string>();
			if ( string.IsNullOrWhiteSpace( input.Roles ) == false )
			{
				var roles = input.Roles.Split( ',' );
				foreach ( string role in roles )
				{
					output.UserRoles.Add( role );
				}
			}


		} //
		public static void MapFromDB( EM.Account input, AppUser output )
		{
			output.Id = input.Id;
			output.RowId = input.RowId;
			output.UserName = input.UserName;

			output.AspNetUserId = input.AspNetId;
			//to.Password = fromEntity.Password;
			output.IsActive = input.IsActive == null ? false : ( bool ) input.IsActive;
			output.FirstName = input.FirstName;
			output.LastName = input.LastName;

			output.Email = input.Email;
			output.ExternalAccountIdentifier = input.ExternalAccountIdentifier;

			if ( IsValidDate( input.Created ) )
				output.Created = ( DateTime ) input.Created;
			if ( IsValidDate( input.LastUpdated ) )
				output.LastUpdated = ( DateTime ) input.LastUpdated;
			output.LastUpdatedById = input.LastUpdatedById == null ? 0 : ( int ) input.LastUpdatedById;

			output.UserRoles = new List<string>();
			//using applicationRoles
			if ( UtilityManager.GetAppKeyValue( "usingNewApplicationRoles", true ) )
			{
				if ( input.ApplicationUserRole != null )
				{
					foreach ( var role in input.ApplicationUserRole )
					{
						output.UserRoles.Add( role.ApplicationRole.Name );
					}
				}
			}
			else
			{
				if ( input.AspNetUsers != null )
				{
					foreach ( EM.AspNetUserRoles role in input.AspNetUsers.AspNetUserRoles )
					{
						output.UserRoles.Add( role.AspNetRoles.Name );
					}
				}
			}


		} //
		  //NOTE: AspNetRoles is to be a guid, so not likely to use this version
		  //public void GetAllUsersInRole( int roleId )
		  //{
		  //	using ( var context = new DataEntities() )
		  //	{
		  //		var customers = context.AspNetUsers
		  //			  .Where( u => u.AspNetRoles.Any( r => r.Id == roleId ) )
		  //			  .ToList();
		  //	}
		  //}
		private static void AppUser_MapToDB( AppUser fromEntity, EM.Account to )
		{
			to.Id = fromEntity.Id;
			//to.RowId = fromEntity.RowId;
			to.UserName = fromEntity.UserName;
			to.AspNetId = fromEntity.AspNetUserId;
			//to.Password = fromEntity.Password;
			to.IsActive = fromEntity.IsActive;

			to.FirstName = fromEntity.FirstName;
			to.LastName = fromEntity.LastName;
			to.Email = fromEntity.Email;

			to.ExternalAccountIdentifier = fromEntity.ExternalAccountIdentifier;

			to.Created = fromEntity.Created;
			to.LastUpdated = fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById;

		}
		#endregion

		#region Proxies

		public bool Store_ProxyCode( string proxyCode, int userId, string proxyType, int expiryDays, ref string statusMessage )
		{
			bool isValid = true;
			EM.System_ProxyCodes efEntity = new EM.System_ProxyCodes();
			//string proxyId = "";
			try
			{
				using ( var context = new DataEntities() )
				{
					efEntity.UserId = userId;
					efEntity.ProxyCode = proxyCode;
					//assuming if storing existing, likely for identity
					efEntity.IsIdentityProxy = true;
					efEntity.Created = System.DateTime.Now;
					efEntity.ExpiryDate = System.DateTime.Now.AddDays( expiryDays );

					efEntity.IsActive = true;
					efEntity.ProxyType = proxyType;

					context.System_ProxyCodes.Add( efEntity );

					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "Successful";
						int id = efEntity.Id;

					}
					else
					{
						//?no info on error
						return false;
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Store_ProxyCode()" );
				statusMessage = ex.Message;
				return false;
			}
			return isValid;
		}

		/// <summary>
		/// Create a proxy guid for requested purpose
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="proxyType"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public string Create_ProxyLoginId( int userId, string proxyType, int expiryDays, ref string statusMessage )
		{
			var efEntity = new EM.System_ProxyCodes();
			string proxyId = "";
			try
			{
				using ( var context = new DataEntities() )
				{
					efEntity.UserId = userId;
					efEntity.ProxyCode = Guid.NewGuid().ToString();
					//assuming if generated, not for identity
					efEntity.IsIdentityProxy = false;
					efEntity.Created = System.DateTime.Now;
					if ( proxyType == SessionLoginProxy )
					{
						//expire at midnight - not really good for night owls
						//efEntity.ExpiryDate = new System.DateTime( DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59 );
						efEntity.ExpiryDate = System.DateTime.Now.AddDays( expiryDays );
					}
					else
						efEntity.ExpiryDate = System.DateTime.Now.AddDays( expiryDays );

					efEntity.IsActive = true;
					efEntity.ProxyType = proxyType;

					context.System_ProxyCodes.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "Successful";
						int id = efEntity.Id;
						return efEntity.ProxyCode;
					}
					else
					{
						//?no info on error
						return proxyId;
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Create_Proxy()" );
				statusMessage = ex.Message;
				return proxyId;
			}
		}

		public bool Proxy_IsCodeActive( string proxyCode )
		{
			bool isValid = false;
			using ( var context = new DataEntities() )
			{

				var proxy = context.System_ProxyCodes.FirstOrDefault( s => s.ProxyCode == proxyCode );
				if ( proxy != null && proxy.Id > 0 )
				{
					if ( proxy.IsActive
						&& proxy.ExpiryDate > DateTime.Now )
						isValid = true;
				}
			}

			return isValid;

		}
		public bool InactivateProxy( string proxyCode, ref string statusMessage )
		{
			bool isValid = true;
			using ( var context = new DataEntities() )
			{
				var proxy = context.System_ProxyCodes.FirstOrDefault( s => s.ProxyCode == proxyCode );
				if ( proxy != null && proxy.Id > 0 )
				{
					proxy.IsActive = false;
					proxy.AccessDate = System.DateTime.Now;

					context.SaveChanges();
				}
			}
			return isValid;

		}
        #endregion


        #region Application Roles
        #region Roles - old
        /// <summary>
        /// Add role for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="createdByUserId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool AddRoleOld( int userId, int roleId, int createdByUserId, ref string statusMessage )
		{
			bool isValid = true;
			string aspNetUserId = "";
			if ( userId == 0 )
			{
				statusMessage = "Error - please provide a valid user";
				return false;
			}
			if ( roleId < 1 || roleId > SiteReader )
			{
				statusMessage = "Error - please provide a valid role identifier";
				return false;
			}

			AppUser user = AppUser_Get( userId );
			if ( user != null && user.Id > 0 )
				aspNetUserId = user.AspNetUserId;

			if ( !IsValidGuid( aspNetUserId ) )
			{
				statusMessage = "Error - please provide a valid user identifier";
				return false;
			}

			EM.AspNetUserRoles efEntity = new EM.AspNetUserRoles();
			using ( var context = new DataEntities() )
			{
				try
				{
					efEntity.UserId = aspNetUserId;
					efEntity.RoleId = roleId.ToString();
					efEntity.Created = System.DateTime.Now;

					context.AspNetUserRoles.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						//other, maybe notification
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the Account_AddRole was not successful. ";
						string message = string.Format( "AccountManager. Account_AddRole Failed", "Attempted to add an Account_AddRole. The process appeared to not work, but there was not an exception, so we have no message, or no clue. Email: {0}, roleId {1}, requestedBy: {2}", user.Email, roleId, createdByUserId );
						EmailManager.NotifyAdmin( "	Manager. Account_AddRole Failed", message );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_AddRole(), Email: {0}", user.Email ) );
					statusMessage = ex.Message;
					isValid = false;
				}
			}

			return isValid;
		}

		/// <summary>
		/// Remove a role from a user
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="roleId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool DeleteRoleFromUserOld( AppUser entity, int roleId, ref string statusMessage )
		{
			bool isValid = true;

			if ( entity == null || !IsValidGuid( entity.AspNetUserId ) )
			{
				statusMessage = "Error - please provide a value user identifier";
				return false;
			}
			if ( roleId < 1 || roleId > SiteReader )
			{
				statusMessage = "Error - please provide a value role identifier";
				return false;
			}

			using ( var context = new DataEntities() )
			{
				try
				{
					EM.AspNetUserRoles efEntity = context.AspNetUserRoles
							.SingleOrDefault( s => s.UserId == entity.AspNetUserId && s.RoleId == roleId.ToString() );

					if ( efEntity != null && !string.IsNullOrWhiteSpace( efEntity.RoleId ) )
					{
						context.AspNetUserRoles.Remove( efEntity );
						int count = context.SaveChanges();
						if ( count > 0 )
						{
							isValid = true;
							//TODO - add logging here or in the services
						}
					}
					else
					{
						statusMessage = "Error - delete failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_DeleteRole(), Email: {0}", entity.Email ) );
					statusMessage = ex.Message;
					isValid = false;
				}
			}

			return isValid;
		}

        public bool DeleteRoleFromUser( AppUser entity, int roleId, ref string statusMessage )
        {
            bool isValid = true;

            if ( entity == null || entity.Id == 0 || roleId == 0 )
            {
                statusMessage = "Error - please provide a value user identifier and application role identifier.";
                return false;
            }

            using ( var context = new DataEntities() )
            {
                try
                {
                    var efEntity = context.ApplicationUserRole
                            .SingleOrDefault( s => s.UserId == entity.Id && s.RoleId == roleId);

                    if ( efEntity != null && efEntity.UserId > 0 )
                    {
                        context.ApplicationUserRole.Remove( efEntity );
                        int count = context.SaveChanges();
                        if ( count > 0 )
                        {
                            isValid = true;
                            //TODO - add logging here or in the services
                        }
                    }
                    else
                    {
                        statusMessage = "Error - delete failed, as record was not found.";
                    }
                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_DeleteRole(), Email: {0}", entity.Email ) );
                    statusMessage = ex.Message;
                    isValid = false;
                }
            }

            return isValid;
        }

        public void UpdateRolesForUser( int userId, List<int> roles )
		{
			using ( var db = new DataEntities() )
			{
				try
				{
					var existRoles = db.ApplicationUserRole.Where( x => x.UserId == userId );
					var oldRoles = existRoles.Select( x => x.RoleId ).ToArray();

					if ( roles == null )
						roles = new List<int>();

					//Add New Roles Selected
					roles.Except( oldRoles ).ToList().ForEach( x =>
					{
						var userRole = new EM.ApplicationUserRole { UserId = userId, RoleId = x, Created = DateTime.Now };
						db.Entry( userRole ).State = System.Data.Entity.EntityState.Added;
					} );

					//Delete existing Roles unselected
					existRoles.Where( x => !roles.Contains( x.RoleId ) ).ToList().ForEach( x =>
					{
						db.Entry( x ).State = System.Data.Entity.EntityState.Deleted;
					} );

					db.SaveChanges();
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UpdateRoles(), UserId: {0}", userId ) );
					//statusMessage = ex.Message;

				}
			}
		}
        public void UpdateRolesOld( string aspNetUserId, string[] roles )
        {
            using ( var db = new DataEntities() )
            {
                try
                {
                    var existRoles = db.AspNetUserRoles.Where( x => x.UserId == aspNetUserId.ToString() );
                    var oldRoles = existRoles.Select( x => x.RoleId ).ToArray();

                    if ( roles == null )
                        roles = new string[] { };

                    //Add New Roles Selected
                    roles.Except( oldRoles ).ToList().ForEach( x =>
                    {
                        var userRole = new EM.AspNetUserRoles { UserId = aspNetUserId, RoleId = x, Created = DateTime.Now };
                        db.Entry( userRole ).State = System.Data.Entity.EntityState.Added;
                    } );

                    //Delete existing Roles unselected
                    existRoles.Where( x => !roles.Contains( x.RoleId ) ).ToList().ForEach( x =>
                    {
                        db.Entry( x ).State = System.Data.Entity.EntityState.Deleted;
                    } );

                    db.SaveChanges();
                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".UpdateRoles(), aspNetUserId: {0}", aspNetUserId ) );
                    //statusMessage = ex.Message;

                }
            }
        }

        public static List<EM.AspNetRoles> GetRolesOld()
		{
			using ( var context = new DataEntities() )
			{
				return context.AspNetRoles.Where( s => s.IsActive == true ).ToList();
			}
		}
        #endregion

        #region Roles - New - moved to ApplicationManager
  //      /// <summary>
  //      /// Get Application User Roles
  //      /// 22-09-22 - changed to use ApplicationRole rather than AspNetRoles
  //      /// </summary>
  //      /// <returns></returns>
  //      public static List<UserRole> GetAllApplicationRoles()
		//{
		//	var output = new List<UserRole>();
		//	using ( var context = new DataEntities() )
		//	{
		//		var list = context.ApplicationRole.Where( s => s.IsActive == true ).ToList();
		//		foreach ( var item in list )
  //              {
		//			var role = new UserRole()
		//			{
		//				Id = item.Id,
		//				Name = item.Name,
		//			};
		//			role.HasApplicationFunctionIds = ApplicationManager.GetApplicationFunctionIds( item.Id );
		//			output.Add( role );

		//		}
		//	}
		//	return output;
		//}
        //public bool SaveApplicationRolePermissions( UserRole input, ref string statusMessage  )
        //{
        //	//check for a new user role
        //	if ( input.Id == 0 )
        //	{

        //	}

        //	using ( var db = new DataEntities() )
        //	{
        //		try
        //		{
        //			var existRoles = db.AppFunctionPermission.Where( m => m.RoleId == input.Id ).ToList();
        //			var oldRoles = existRoles.Select( x => x.ApplicationFunctionId).ToArray();

        //			if ( input.HasApplicationFunctionIds == null )
        //				input.HasApplicationFunctionIds = new List<int>();

        //			//Add New Roles Selected
        //			input.HasApplicationFunctionIds.Except( oldRoles ).ToList().ForEach( x =>
        //			{
        //				//TBD - is presence enough, or will we want sublevel (CRUD) options
        //				var userRole = new EM.AppFunctionPermission { ApplicationFunctionId = x, RoleId = input.Id };
        //				db.Entry( userRole ).State = System.Data.Entity.EntityState.Added;
        //			} );

        //			//Delete existing Roles unselected
        //			existRoles.Where( x => !input.HasApplicationFunctionIds.Contains( x.ApplicationFunctionId ) ).ToList().ForEach( x =>
        //			{
        //				db.Entry( x ).State = System.Data.Entity.EntityState.Deleted;
        //			} );

        //			db.SaveChanges();
        //			return true;
        //		}
        //		catch ( Exception ex )
        //		{
        //			LoggingHelper.LogError( ex, thisClassName + string.Format( ".SaveApplicationRolePermissions(), UserRole: {0}", input.Name ) );
        //			statusMessage = ex.Message;
        //			return false;
        //		}
        //	}
        //}
        //public static List<int> GetApplicationFunctionIds( int roleId )
        //{
        //	using ( var context = new DataEntities() )
        //	{
        //		var list = context.AppFunctionPermission.Where( m => m.RoleId == roleId ).ToList();
        //		return list.Select( m => m.ApplicationFunctionId ).ToList();
        //	}
        //}

        //public static List<ApplicationFunction> GetApplicationFunctions()
        //{
        //	var output = new List<ApplicationFunction>();
        //	using ( var context = new DataEntities() )
        //	{
        //		var list = context.ApplicationFunction.ToList();
        //		foreach (var item in list)
        //              {
        //			output.Add( new ApplicationFunction()
        //			{
        //				Id = item.Id,
        //				Name = item.Name,
        //				CodedNotation = item.CodedNotation,
        //				Description = item.Description,
        //			} );
        //              }
        //	}

        //	return output;
        //}

        //public static List<ApplicationFunctionPermission> GetApplicationFunctionPermissions( int roleId )
        //{
        //	var output = new List<ApplicationFunctionPermission>();
        //	using ( var context = new DataEntities() )
        //	{
        //		var list = context.AppFunctionPermission.Where( m => m.RoleId == roleId ).ToList();
        //		foreach ( var item in list )
        //		{
        //			output.Add( new ApplicationFunctionPermission()
        //			{
        //				ApplicationFunctionId= item.ApplicationFunctionId,
        //				RoleId= item.RoleId,
        //				CanCreate= item.CanCreate,
        //				CanDelete= item.CanDelete,
        //				CanRead= item.CanRead,
        //				CanUpdate= item.CanUpdate,
        //			} );
        //		}
        //	}

        //	return output;
        //}

        #endregion
        #endregion
    }
}
