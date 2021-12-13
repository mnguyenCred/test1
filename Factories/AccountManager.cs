﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Application;
using EM = Data.Tables;
using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Views = Data.Views;
using Navy.Utilities;

namespace Factories
{
    public class AccountManager : BaseFactory
	{
		static string thisClassName = "AccountManager";

		static int Administrator = 1;
		static int SiteManager = 2;
		static int SiteStaff = 3;

		static int RMTLDeveloper = 10;
		static int RMTLSME = 11;
		static int RCDCAnalyst = 12;
		static int RatingContinuumManager = 13;

		static int SiteReader = 20;

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
						efEntity.IsActive = false;
						efEntity.LastUpdated = System.DateTime.Now;
						int count = context.SaveChanges();
						//add activity log (usually in caller method)
						if ( count >= 0 )
						{
							isValid = true;

							//need to handle AspNetUsers
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
		/*
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

					using ( SqlDataAdapter adapter = new SqlDataAdapter() )
					{
						adapter.SelectCommand = command;
						adapter.Fill( result );
					}
					string rows = command.Parameters[5].Value.ToString();
					try
					{
						pTotalRows = Int32.Parse( rows );
					}
					catch
					{
						pTotalRows = 0;
					}
				}

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
					item.OrgMbrs = GetRowColumn( dr, "OrgMbrs", "" );
					item.lastLogon = GetRowColumn( dr, "lastLogon", "" );
					if ( IsValidDate( item.lastLogon ) )
						item.lastLogon = item.lastLogon.Substring( 0, 10 );

					list.Add( item );
				}

				return list;

			}
		}
		*/
		public static void MapFromDB( Views.Account_Summary input, AppUser to )
		{
			to.Id = input.Id;
			//to.RowId = fromEntity.RowId;
			to.UserName = input.UserName;

			to.AspNetUserId = input.AspNetId;
			//to.Password = fromEntity.Password;
			to.IsActive = input.IsActive == null ? false : ( bool ) input.IsActive;
			to.FirstName = input.FirstName;
			to.LastName = input.LastName;
			to.SortName = input.SortName;

			to.Email = input.Email;
			to.ExternalAccountIdentifier = input.ExternalAccountIdentifier;

			if ( IsValidDate( input.Created ) )
				to.Created = (DateTime)input.Created;
			if ( IsValidDate( input.LastUpdated ) )
				to.LastUpdated = ( DateTime ) input.LastUpdated;
			to.LastUpdatedById = input.LastUpdatedById == null ? 0 : ( int ) input.LastUpdatedById;
			//if ( IsValidDate( fromEntity.lastLogon ) )
			//{
			//	to.lastLogon = ( ( DateTime ) fromEntity.lastLogon ).ToString( "yyyy-MM-dd" );
			//}
			//else
			//	to.lastLogon = "None";

			to.UserRoles = new List<string>();
			if ( string.IsNullOrWhiteSpace( input.Roles ) == false )
			{
				var roles = input.Roles.Split( ',' );
				foreach ( string role in roles )
				{
					to.UserRoles.Add( role );
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
			if ( input.AspNetUsers != null )
			{
				foreach ( EM.AspNetUserRoles role in input.AspNetUsers.AspNetUserRoles )
				{
					output.UserRoles.Add( role.AspNetRoles.Name );
				}
			}



		} //
		  //NOTE: AspNetRoles is to be a guid, so not likely to use this version
		  //public void GetAllUsersInRole( int roleId )
		  //{
		  //	using ( var context = new Data.CTIEntities() )
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
	}
}
