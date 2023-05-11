using System;
using System.Collections.Generic;
using System.Web;
//using System.Web.Mvc;
using System.Web.SessionState;

using DT = Data.Tables;
using Factories;

using Models.Application;

using Navy.Utilities;
using System.Web.Security;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq;
using System.Security.Policy;

namespace Services
{
    public class AccountServices
	{
		private static string thisClassName = "AccountServices";

		//messages
		public static string NOT_AUTHORIZED = "You do not have the proper privileges to access this function.";
		public static string NOT_AUTHENTICATED = "You must be authenticated (logged in) and authorized to perform this action.";

		//event actions
		public static string EVENT_AUTHORIZED = "NotAuthorized";
		public static string EVENT_AUTHENTICATED = "NotAuthenticated";

        #region Authorization methods
        public static bool IsUserSiteManager()
		{
			AppUser user = GetUserFromSession();
			if ( user == null || user.Id == 0 )
				return false;

			return IsUserSiteManager( user );
		}
		public static bool IsUserSiteManager( AppUser user )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( user.UserRoles.Contains( AccountManager.ROLE_ADMINISTRATOR )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_MANAGER )
				)
				return true;
			else
				return false;
		}
		public static bool IsUserAnAdmin()
		{
			AppUser user = GetUserFromSession();
			if ( user == null || user.Id == 0 )
				return false;

			return IsUserAnAdmin( user );
		}
		public static bool IsUserAnAdmin( AppUser user )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( user.UserRoles.Contains( AccountManager.ROLE_ADMINISTRATOR ))
				return true;
			else
				return false;
		}

		public static bool IsUserSiteStaff()
		{
			AppUser user = GetUserFromSession();
			if ( user == null || user.Id == 0 )
				return false;

			return IsUserSiteStaff( user );
		}
		public static bool IsUserSiteStaff( AppUser user )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( user.UserRoles.Contains( AccountManager.ROLE_ADMINISTRATOR )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_MANAGER )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_STAFF )
				)
				return true;
			else
				return false;
		}
		//
		public static bool HasRmtlDeveloperAccess()
		{
			AppUser user = GetUserFromSession();
			if ( user == null || user.Id == 0 )
				return false;

			return HasRmtlDeveloperAccess( user );
		}
		public static bool HasRmtlDeveloperAccess( AppUser user )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( user.UserRoles.Contains( AccountManager.ROLE_ADMINISTRATOR )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_MANAGER )
			  || user.UserRoles.Contains( AccountManager.ROLE_RMTL_DEVELOPER )
				)
				return true;
			else
				return false;
		}
		public static bool HasRatingContinuumManagerAccess( AppUser user )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( user.UserRoles.Contains( AccountManager.ROLE_ADMINISTRATOR )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_MANAGER )
			  || user.UserRoles.Contains( AccountManager.ROLE_RATING_CONTINUUM_MANAGER )
				)
				return true;
			else
				return false;
		}
		public static bool HasRatingContinuumAnalystAccess( AppUser user )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( user.UserRoles.Contains( AccountManager.ROLE_ADMINISTRATOR )
			  || user.UserRoles.Contains( AccountManager.ROLE_RATING_CONTINUUM_DEVELOPMENT_ANALYST )
				)
				return true;
			else
				return false;
		}
		public static bool HasRmtlSubjectMatterExpertAccess( AppUser user )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( user.UserRoles.Contains( AccountManager.ROLE_ADMINISTRATOR )
			  || user.UserRoles.Contains( AccountManager.ROLE_RMTL_SME )
				)
				return true;
			else
				return false;
		}
		/// <summary>
		/// Return true if user can view all parts of site.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static bool CanUserViewAllOfSite( AppUser user )
		{
			if ( user == null || user.Id == 0 )
				return false;

			if ( user.UserRoles.Contains( AccountManager.ROLE_ADMINISTRATOR )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_MANAGER )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_STAFF )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_READER ) //will retain htis
				)
				return true;
			else
				return false;
		}

		/// <summary>
		/// If true, user can view site during the beta period.
		/// Checks for a user in the session
		/// </summary>
		/// <returns></returns>
		public static bool CanUserViewSite()
		{
			//this method will not expect a status message
			string status = "";

			AppUser user = GetUserFromSession();
			if ( user == null || user.Id == 0 )
				return false;
			return CanUserViewSite( user, ref status );
		}

		/// <summary>
		/// If true, user can view site during the beta period
		/// </summary>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		private static bool CanUserViewSite( AppUser user, ref string status )
		{
			//this method will not expect a status message
			status = "";
			if ( user == null || user.Id == 0 )
			{
				status = "You must be authenticated and authorized before being allowed to view any content.";
				return false;
			}

			if ( user.UserRoles.Contains( AccountManager.ROLE_ADMINISTRATOR )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_MANAGER )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_STAFF )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_READER ) //will retain htis
				)
				return true;

			// allow if user is member of an org
			//depends on purpose, if site in general, ok, but not for viewing unpublished stuff
			//if ( OrganizationManager.IsMemberOfAnyOrganization( user.Id ) )
			//	return true;

			return false;
		}

		public static bool CanUserViewAllContent( AppUser user )
		{
			//this method will not expect a status message
			//status = "";
			if ( user == null || user.Id == 0 )
			{
				//status = "You must be authenticated and authorized before being allowed to view any content.";
				return false;
			}

			if ( user.UserRoles.Contains( "Administrator" )
			  || user.UserRoles.Contains( "Site Manager" )
			  || user.UserRoles.Contains( "Site Staff" )
			  || user.UserRoles.Contains( "Site Partner" )
			  || user.UserRoles.Contains( "Site Reader" )
				)
				return true;

			bool canEditorsViewAll = UtilityManager.GetAppKeyValue( "canEditorsViewAll", false );
			//if allowing anyone with edit for any org return true;
			//if ( canEditorsViewAll && OrganizationServices.IsMemberOfAnOrganization( user.Id ) )
			//	return true;

			return false;
		}


		/// <summary>
		/// Return true if current user can create content
		/// Called from header. 
		/// </summary>
		/// <returns></returns>
		public static bool CanUserCreateContent()
		{
			//this method will not expect a status message
			string status = "";
			AppUser user = GetUserFromSession();
			if ( user == null || user.Id == 0 )
				return false;
			return CanUserCreateContent( user, ref status );
		}
	
		/// <summary>
		/// Return true if user can publish content.
		/// May be to general to be used
		/// Essentially this relates to being able to upload rmtl lists, create rmtl projects
		/// </summary>
		/// <param name="user"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static bool CanUserCreateContent( AppUser user, ref string status )
		{
			status = "";
			if ( user == null || user.Id == 0 )
			{
				status = "You must be authenticated and authorized before being allowed to create any content.";
				return false;
			}

			if ( user.UserRoles.Contains( AccountManager.ROLE_ADMINISTRATOR )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_STAFF )
			  || user.UserRoles.Contains( AccountManager.ROLE_RMTL_DEVELOPER )
				)
				return true;

			//if allowing anyone with edit for any org return true;
			//if ( OrganizationServices.IsMemberOfAnOrganization( user.Id ) )
			//	return true;

			////allow once out of beta, and user is member of an org
			//if ( OrganizationManager.IsMemberOfAnyOrganization( user.Id ) )
			//	return true;

			status = "Sorry - You have not been authorized to add or update content on this site during this period. Please contact site management if you believe that you should have access during this site.";

			return false;
		}

		public static bool CanUserCreateRmtlProject( AppUser user, ref string status )
		{
			status = "";
			if ( user == null || user.Id == 0 )
			{
				status = "You must be authenticated and authorized before being allowed to create any content.";
				return false;
			}

			if ( user.UserRoles.Contains( AccountManager.ROLE_ADMINISTRATOR )
			  || user.UserRoles.Contains( AccountManager.ROLE_SITE_STAFF )
			  || user.UserRoles.Contains( AccountManager.ROLE_RMTL_DEVELOPER )
				)
				return true;

			status = "Sorry - You have not been authorized to create RMTL projects. Please contact site management if you believe that you should have this privilege.";

			return false;
		}

		/// <summary>
		/// Perform basic authorization checks. First establish an initial user object.
		/// Used where the user object is not to be returned.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="mustBeLoggedIn"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static bool AuthorizationCheck( bool mustBeLoggedIn, ref string status )
		{
			AppUser user = GetCurrentUser();
			return AuthorizationCheck( "", mustBeLoggedIn, ref status, ref user );
		}
		/// <summary>
		/// Do auth check - where user is not expected back, so can be instantiate here and passed to next version
		/// </summary>
		/// <param name="action"></param>
		/// <param name="mustBeLoggedIn"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static bool AuthorizationCheck( string action, bool mustBeLoggedIn, ref string status )
		{

			AppUser user = new AppUser(); //GetCurrentUser();
			return AuthorizationCheck( action, mustBeLoggedIn, ref status, ref user );
		}
		/// <summary>
		/// Perform basic authorization checks
		/// </summary>
		/// <param name="action"></param>
		/// <param name="mustBeLoggedIn"></param>
		/// <param name="status"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static bool AuthorizationCheck( string action, bool mustBeLoggedIn, ref string status, ref AppUser user )
		{
			bool isAuthorized = true;
			user = GetCurrentUser();
			bool isAuthenticated = IsUserAuthenticated( user );
			if ( mustBeLoggedIn && !isAuthenticated )
			{
				status = NOT_AUTHENTICATED + ( string.IsNullOrWhiteSpace( action ) ? "" : string.Format( " ({0}).", action ));
				return false;
			}

			if ( string.IsNullOrWhiteSpace( action ) )
				return true;

			//for authorization, may not be able to handle all, perhaps just those for a range of role ids using say minimum role. 
			return isAuthorized;

		}

		#endregion

		#region Create/Update
		/// <summary>
		/// Create a new account, based on the AspNetUser info!
		/// </summary>
		/// <param name="email"></param>
		/// <param name="firstName"></param>
		/// <param name="lastName"></param>
		/// <param name="userName"></param>
		/// <param name="userKey"></param>
		/// <param name="password">NOTE: may not be necessary as the hash in the aspNetUsers table is used?</param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Create( string email, string firstName, string lastName, string userName, string userKey, string password, string externalCEAccountIdentifier,
				ref string statusMessage,
				bool doingEmailConfirmation = false,
				bool isExternalSSO = false )
		{
			int id = 0;
			statusMessage = "";
			//this password, as stored in the account table, is not actually used
			string encryptedPassword = "";
			if ( !string.IsNullOrWhiteSpace( password ) )
				encryptedPassword = UtilityManager.Encrypt( password );

			AppUser user = new AppUser()
			{
				Email = email,
				UserName = email,
				FirstName = firstName,
				LastName = lastName,
				IsActive = !doingEmailConfirmation,
				AspNetUserId = userKey,
				Password = encryptedPassword,
				ExternalAccountIdentifier = externalCEAccountIdentifier
			};
			id = new AccountManager().Add( user, ref statusMessage );
			if ( id > 0 )
			{
				//don't want to add to session, user needs to confirm
				//AddUserToSession( HttpContext.Current.Session, user );

				
				string msg = string.Format( "New user registration. <br/>Email: {0}, <br/>Name: {1}<br/>Type: {2}", email, firstName + " " + lastName, ( isExternalSSO ? "External SSO" : "Forms" ) );
                ActivityServices.UserRegistration( user, "registration", msg );
                //EmailManager.SendSiteEmail( "New Application account", msg );
            }

			return id;
		} //

        #region Create/Update
        /// <summary>
        /// Create a new account, based on the AspNetUser info!
        /// </summary>
        /// <param name="email"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int CacCreate(string email, string firstName, string lastName, string userName, string userKey, string password, string externalCEAccountIdentifier,
                ref string statusMessage,
                bool doingEmailConfirmation = false,
                bool isExternalSSO = false)
        {
            int id = 0;
            statusMessage = "";

            //this password, as stored in the account table, is not actually used
            string encryptedPassword = "";
            if (!string.IsNullOrWhiteSpace(password))
                encryptedPassword = UtilityManager.Encrypt(password);

            AppUser user = new AppUser()
            {
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = !doingEmailConfirmation,
                AspNetUserId = userKey,
                Password = encryptedPassword,
                ExternalAccountIdentifier = externalCEAccountIdentifier

            };
            id = new AccountManager().Add(user, ref statusMessage);
            if (id > 0)
            {
                //don't want to add to session, user needs to confirm
                //AddUserToSession( HttpContext.Current.Session, user );


                string msg = string.Format("New user registration. <br/>Email: {0}, <br/>Name: {1}<br/>Type: {2}", email, firstName + " " + lastName, (isExternalSSO ? "External SSO" : "Forms"));
                ActivityServices.UserRegistration(user, "registration", msg);
                //EmailManager.SendSiteEmail( "New Application account", msg );
            }

            return id;
        } //
        #endregion
        /// <summary>
        /// Account created by a third party, not through registering
        /// </summary>
        /// <param name="email"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="userName"></param>
        /// <param name="userKey"></param>
        /// <param name="password"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int AddAccount( string email, string firstName, string lastName, string userName, string userKey, string password, int addedByUserId, ref string statusMessage )
		{
			int id = 0;
			statusMessage = "";
			//this password, as stored in the account table, is not actually used
			string encryptedPassword = "";
			if ( !string.IsNullOrWhiteSpace( password ) )
				encryptedPassword = UtilityManager.Encrypt( password );

			AppUser user = new AppUser()
			{
				Email = email,
				UserName = email,
				FirstName = firstName,
				LastName = lastName,
				IsActive = true,
				AspNetUserId = userKey,
				Password = encryptedPassword
			};
			id = new AccountManager().Add( user, ref statusMessage );
			if ( id > 0 )
			{
				//don't want to add to session, user needs to confirm
				//AddUserToSession( HttpContext.Current.Session, user );

				ActivityServices.AddUserActivity( user, addedByUserId );
				string msg = string.Format( "New Account. <br/>Email: {0}, <br/>Name: {1}<br/>Type: {2}", email, firstName + " " + lastName, "New Account" );
	
				//EmailServices.SendSiteEmail( "New Application Account", msg );
			}

			return id;
		} //

		/// <summary>
		/// update account, and AspNetUser
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public bool Update( AppUser user, bool usingSession, ref string statusMessage )
		{
			bool successful = new AccountManager().Update( user, ref statusMessage );
			if ( successful && usingSession )
			{
				AddUserToSession( HttpContext.Current.Session, user );
			}

			return successful;
		}

		public bool Delete( int userId, AppUser deletedBy, ref string message )
		{
			message = "";
			return new AccountManager().Delete( userId, deletedBy, ref message );
		}

		public bool ActivateUser( string aspNetId )
		{
			string statusMessage = "";
			AppUser user = GetUserByKey( aspNetId );
			if ( user != null && user.Id > 0 )
			{
				user.IsActive = true;
				if ( new AccountManager().Update( user, ref statusMessage ) )
				{
					//EmailServices.SendSiteEmail( "User Activated Application account", string.Format( "{0} activated a Application account. <br/>Email: {1}", user.FullName(), user.Email ) );
					
					return true;
				}
				else
				{
					EmailManager.NotifyAdmin( "Activate user failed", string.Format( "Attempted to activate user: {0}. <br/>Received invalid status: {1}", user.Email, statusMessage ) );
					return false;
				}
			}
			else
			{
				EmailManager.NotifyAdmin( "Activate user failed", string.Format( "Attempted to activate user aspNetId: {0}. <br/>However latter aspNetId was not found", aspNetId ) );
				return false;
			}

		}

		public bool SetUserEmailConfirmed( string aspNetId )
		{
			string statusMessage = "";
			//
			AppUser user = GetUserByKey( aspNetId, false );
			if ( user != null && user.Id > 0 )
			{
				user.IsActive = true;
				if ( new AccountManager().AspNetUsers_UpdateEmailConfirmed( aspNetId, ref statusMessage ) )
				{
					return true;
				}
				else
				{
					EmailManager.NotifyAdmin( "SetUserEmailConfirmed failed", string.Format( "Attempted to SetUserEmailConfirmed for user: {0}. <br/>Received invalid status: {1}", user.Email, statusMessage ) );
					return false;
				}
			}
			else
			{
				EmailManager.NotifyAdmin( "SetUserEmailConfirmed failed", string.Format( "Attempted to SetUserEmailConfirmed for user aspNetId: {0}. <br/>However latter aspNetId was not found", aspNetId ) );
				return false;
			}

		}

		public bool SetUserEmailConfirmedByEmail( string email )
		{
			string statusMessage = "";
			if (  AccountManager.AspNetUsers_UpdateEmailConfirmedByEmail( email, ref statusMessage ,"oldEmail") )
			{
				return true;
			}
			else
			{
				EmailManager.NotifyAdmin( "SetUserEmailConfirmedByEmail failed", string.Format( "Attempted to SetUserEmailConfirmedByEmail for user: {0}. <br/>Received invalid status: {1}", email, statusMessage ) );
				return false;
			}

		}
        #endregion

        #region Roles
        public bool AddRole( int userId, int roleId, int createdByUserId, ref string statusMessage )
		{
			return new AccountManager().AddRoleOld( userId, roleId, createdByUserId, ref statusMessage );
		}
        public bool DeleteApplicationRole( UserRole role, ref string statusMessage )
		{
            return new ApplicationManager().ApplicationRoleDelete( role, ref statusMessage );
        }
  //      public bool DeleteRoleFromUser( int userId,	int roleId, ref string statusMessage )
		//{
		//	AppUser user = AccountManager.AppUser_Get( userId );
		//	if ( user == null || user.Id < 1 )
		//	{
		//		statusMessage = "Error - account was not found.";
		//		return false;
		//	}

  //          //bool isValid = new AccountManager().DeleteRoleFromUserOld( user, roleId, ref statusMessage );
  //          bool isValid = new AccountManager().DeleteRoleFromUser( user, roleId, ref statusMessage );
  //          //TODO - add logging here or in the services
  //          return isValid;
		//}
  //      public void UpdateRolesForUserOld( string aspNetUserId, string[] roles )
  //      {
  //          AppUser user = GetCurrentUser();
  //          new AccountManager().UpdateRolesOld( aspNetUserId, roles );
  //      }
        public void UpdateRolesForUser( int userId, List<int> roles )
		{
			AppUser user = GetCurrentUser();
			new AccountManager().UpdateRolesForUser( userId, roles );
		}
        #endregion

        public NavyCredential GetNavyUserFromHeader( ref bool isValid, ref string message )
        {
            isValid = true;
            //
            try
            {
                HttpContext httpContext = HttpContext.Current;
				//confirm that Authorization is what we are looking for
                string authHeader = httpContext.Request.Headers["Authorization"];
                string f5Header = httpContext.Request.Headers["F5_USER"];
                //not sure of content type yet. Check if starts with {

                if ( string.IsNullOrWhiteSpace( authHeader ) && string.IsNullOrWhiteSpace(f5Header))
                {
                    isValid = false;
                    return null;
                }

				if (!string.IsNullOrWhiteSpace(authHeader)) {
					LoggingHelper.DoTrace(7, "$$$$$$$$ Found an authorization header: " + authHeader.Substring(0, 8) + "-...");


					if (authHeader.StartsWith("{"))
					{
						//Extract credentials
						authHeader = authHeader.Trim();
						var navyUser = JsonConvert.DeserializeObject<NavyCredential>(authHeader);
						if (navyUser != null && !string.IsNullOrWhiteSpace(navyUser.Email))
						{

							//NavyRRL.Models.app

							return navyUser;
						}
						else
						{
							//error
						}
					}
                }
				if (!string.IsNullOrWhiteSpace(f5Header))
				{
				
					string[] f5HeaderSplit = f5Header.Split(new string[] { "CN=" }, StringSplitOptions.None);
					if (f5HeaderSplit.Length != 2)
					{
						isValid = false;
						return null;
					}
					else
					{
						LoggingHelper.DoTrace(7, "$$$$$$$$ Found an F5 header: " + f5Header);
						string[] cnSplit = f5HeaderSplit[1].Split('.');

						if (cnSplit.Count() == 3 || cnSplit.Count() == 4)
						{
							NavyCredential navyUser = new NavyCredential();
							navyUser.FirstName = cnSplit[1];
							navyUser.LastName = cnSplit[0];
							navyUser.Identifier = cnSplit[cnSplit.Length - 1];
							return navyUser;
						}
						else
						{
                            isValid = false;
                            return null;

                        }



                    }
                    //looking for
                    //"F5_USER:C=US, O=U.S. Government, OU=DoD, OU=PKI, OU=USN, CN=INDSETH.MICHAEL.SHAWN.1234567890"
                }


            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "Exception encountered attempting to get API key from request header. " );
                message = "Error encountered validating ApiKey: '" + ex.Message + "'";
                isValid = false;
            }

            return null;
        }

        public AppUser GetUserFromHeader( ref bool isValid, ref string message )
        {
            isValid = true;
            //
            try
            {
                HttpContext httpContext = HttpContext.Current;
                string authHeader = httpContext.Request.Headers["Authorization"];
                //not sure of content type yet. Check if starts with {
                if ( string.IsNullOrWhiteSpace( authHeader ) )
                {
                    isValid=false;
                    return null;
				}
                LoggingHelper.DoTrace( 7, "$$$$$$$$ Found an authorization header: " + authHeader.Substring( 0, 8 ) + "-..." );
                if ( authHeader.StartsWith( "{" ))
                {
                    //Extract credentials
                    authHeader = authHeader.Trim();
                    var navyUser = JsonConvert.DeserializeObject<NavyCredential>( authHeader );
					if ( navyUser != null && !string.IsNullOrWhiteSpace( navyUser.Email))
					{
						var user = GetUserByEmail( navyUser.Email );
						if ( user != null && user.Id > 0 ) 
						{
							var userConfirmed = CheckIfUserIsConfirmedByEmail(navyUser.Email);
							if(userConfirmed != null)
							{
								return userConfirmed;
							}
							else
							{
								message = "user has not confirmed email";
                                isValid = false;
                            }
                            //return user;
						}
						//otherwise add the user
					} else
					{
						//error
					}
                }
                else if ( authHeader.ToLower().StartsWith( "token " ) && authHeader.Length > 36 )
                {
                    //this is for the registry
                    authHeader = authHeader.ToLower();
                    //token = authHeader.Substring( "token ".Length ).Trim();
                }             
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "Exception encountered attempting to get API key from request header. " );
                message = "Error encountered validating ApiKey: '" + ex.Message + "'";
                isValid = false;      
            }

            return null;
        }


        #region Owin email methods
        /// <summary>
        /// Send reset password email
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="toEmail"></param>
        /// <param name="url">Will be a formatted callback url</param>
        public static void SendEmail_ResetPassword( string toEmail, string url )
		{
			//should have a valid email at this point (if from identityConfig)
			AppUser user = GetUserByEmail( toEmail );

			bool isSecure = false;
			if ( UtilityManager.GetAppKeyValue( "usingSSL", false ) )
				isSecure = true;
			string bcc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "" );

			string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "" );
			string subject = "Reset Password for your Application account";

			string email = EmailManager.GetEmailText( "ForgotPassword" );
			string eMessage = "";

			try
			{
				//assign and substitute: 0-FirstName, 1-callback url from AccountController
				eMessage = string.Format( email, user.FirstName, url );


				EmailServices.SendEmail( toEmail, fromEmail, subject, eMessage, "", bcc );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".SendEmail_ResetPassword()" );
			}

		}

		/// <summary>
		/// Send email to confirm new account
		/// </summary>
		/// <param name="subject"></param>
		/// <param name="toEmail"></param>
		/// <param name="url">Will be a formatted callback url</param>
		public static void SendEmail_ConfirmAccount( string toEmail, string url )
		{
			//should have a valid email at this point (if from identityConfig)
			AppUser user = GetUserByEmail( toEmail );

			bool isSecure = false;
			//string toEmail = user.Email;
			string bcc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "" );

			string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", CodesManager.DefaultEmailAddress );
			string subject = "Confirm Your Application account";
			string email = EmailManager.GetEmailText( "ConfirmAccount" );
			string eMessage = "";

			try
			{

				//assign and substitute: 0-FirstName, 1-body from AccountController
				eMessage = string.Format( email, user.FirstName, url );

				EmailServices.SendEmail( toEmail, fromEmail, subject, eMessage, "", bcc );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".SendEmail_ConfirmPassword()" );
			}

		}
		public static void SendEmail_ContactUs( AppUser user, string reason )
		{
			string subject = "Contact Us Request";

			string toEmail = UtilityManager.GetAppKeyValue( "systemAdminEmail", CodesManager.DefaultEmailAddress );
			string appUrl = UtilityManager.GetAppKeyValue( "applicationURL" );
			string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", CodesManager.DefaultEmailAddress );
			//
			string rowTemplate = "<tr><td 'style=width: 225px; text-align:right; vertical-align: top'><b>{0}&nbsp;</b></td><td style='width: 500px;text-align:left; vertical-align: top'>{1}</td></tr>";
			string email = "<p>A contact request has been received:</p>";
			email += "<table><tbody>";
			email += string.Format( rowTemplate, "Name:", user.FullName() );
			email += string.Format( rowTemplate, "Email:", user.Email );
			if ( user.Message?.Length > 0 )
			{
				//assume IP address - more useful for the code to skip repeats from same IP?
				email += string.Format( rowTemplate, "IP Address: ", user.Message );
			}
			email += string.Format( rowTemplate, "Reason: ", reason );

			//email += string.Format( "<div style='display:inline-block;width: 500px;'><b>From:</b><span>{0}</span></div> <br/><div style='width: 500px;'><b>Email:</b> {1}</div></br>", user.FullName(), user.Email );
			//if (user.Message?.Length > 0)
			//         {
			//	//assume IP address - more useful for the code to skip repeats from same IP?
			//	email += string.Format( "<div style='width: 500px;'><b>IP Address:</b>{0}</div></br>", user.Message );
			//}
			email += "</tbody></table>";
			//email += string.Format( "<div style='width: 150px;text-align:right;'><b>Reason:</b></div><p style='margin-left: 50px; width: 50%;'>{0}</p>", reason) ;
			//email += "<p>Navy RRL Application</p>";
			email += string.Format("<p><a href='{0}'>Navy RRL Application</a></p>", appUrl);

			try
			{

				EmailServices.SendEmail( toEmail, fromEmail, subject, email, "", "" );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".SendEmail_ContactUs()" );
			}
		}
		public static void SendEmail_OnUnConfirmedEmail( string userEmail )
		{
			//should have a valid email at this point
			AppUser user = GetUserByEmail( userEmail );
			string subject = "Forgot password attempt with unconfirmed email";

			string toEmail = UtilityManager.GetAppKeyValue( "systemAdminEmail", CodesManager.DefaultEmailAddress);

			string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", CodesManager.DefaultEmailAddress );
			//string subject = "Forgot Password";
			string email = "User: {0} attempted Forgot Password, and email has not been confirmed.<br/>Email: {1}<br/>Created: {2}";
			string eMessage = "";

			try
			{
				eMessage = string.Format( email, user.FullName(), user.Email, user.Created );

				EmailServices.SendEmail( toEmail, fromEmail, subject, eMessage, "", "" );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".SendEmail_OnUnConfirmedEmail()" );
			}
		}


		public static void SendEmail_MissingSubjectType( string subject, string toEmail, string body )
		{
			//should have a valid email at this point (if from identityConfig)
			AppUser user = GetUserByEmail( toEmail );

			bool isSecure = false;
			
			string eMessage = string.Format("To email: {0}<br/>Subject: {1}<br/>Body: {2}", toEmail, subject, body);

			try
			{
				EmailManager.NotifyAdmin( "Email Subject Type not handled", "<p>Unexpected email subject encountered</p>" + eMessage);
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".SendEmail_MissingSubjectType()" );
			}

		}

		#endregion

		#region Read methods
		/// <summary>
		/// Retrieve a user by email address
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public static AppUser GetUserByEmail( string email )
		{
			AppUser user = AccountManager.AppUser_GetByEmail( email );

			return user;
		} //

        public static AppUser GetUserByIdentifier(string identifier)
        {
            AppUser user = AccountManager.AppUser_GetByIdentifier(identifier);
			AppUser confirmedUser = AccountManager.AppUser_CheckIfUserIsConfirmedByEmail(user.Email);

			if(confirmedUser.Id != 0)
			{
                return user;
            }
			else 
			{
				return null; 
			}

        } //
        public static AppUser GetUserByUserName( string username )
		{
			AppUser user = AccountManager.GetUserByUserName( username );

			return user;
		} //
		public static AppUser GetUserByCEAccountId( string accountIdentifier, bool addingToSession = false )
		{
			AppUser user = AccountManager.GetUserByCEAccountId( accountIdentifier );
			if (addingToSession && user != null && user.Id > 0)
				AddUserToSession(HttpContext.Current.Session, user);

			return user;

        } //

        public static AppUser CheckIfUserIsConfirmedByEmail(string email)
        {
            AppUser user = AccountManager.AppUser_CheckIfUserIsConfirmedByEmail(email);

            return user;
        } //
        /// <summary>
        /// Add User to session
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static AppUser SetUser(AppUser user)
        {
            AddUserToSession(HttpContext.Current.Session, user);
            return user;
        } //


		/// <summary>
		/// Get user by email, and add to the session
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public static AppUser SetUserByEmail( string email )
		{
			AppUser user = AccountManager.AppUser_GetByEmail( email );
			AddUserToSession( HttpContext.Current.Session, user );
			return user;
		} //

        public static AppUser SetUserByIdentifier(string identifier)
        {
            AppUser user = AccountManager.AppUser_GetByIdentifier(identifier);
            AddUserToSession(HttpContext.Current.Session, user);
            return user;
        } //

        /// <summary>
        /// User is authenticated, either get from session or via the Identity name
        /// </summary>
        /// <param name="identityName"></param>
        /// <returns></returns>
        public static AppUser GetCurrentUser( string identityName = "", bool doingOrganizationsCheck = false )
		{
			AppUser user = AccountServices.GetUserFromSession();
			if ( ( user == null || user.Id == 0 ) )
			{
                if ( !string.IsNullOrWhiteSpace( identityName ) )
                {
                    //NOTE identityName is related to the UserName
                    //TODO - need to add code to prevent dups between google register and direct register
                    user = GetUserByUserName( identityName );
                    if ( user != null && user.Id > 0 )
                        AddUserToSession( HttpContext.Current.Session, user );
                }
			} else
            {
                //may need a check if org exists, because??
                if (doingOrganizationsCheck)
                {
                    //if (user.Organizations != null && user.Organizations.Count == 0)
                    //{
                    //    //but don't want to this constantly, although would catch orgs that were added since login
                    //}
                }
            }

			return user;
		} //
		public static int GetCurrentUserId()
		{
			AppUser user = AccountServices.GetUserFromSession();
			if ( user == null || user.Id == 0 )
				return 0;
			else
				return user.Id;
		} //

		/// <summary>
		/// set the current user via an identity name at session start
		/// </summary>
		/// <param name="identityName"></param>
		/// <returns></returns>
		public static AppUser SetCurrentUser( string identityName )
		{
			AppUser user = AccountServices.GetUserFromSession();
			if ( !string.IsNullOrWhiteSpace( identityName ) )
			{
				//assume identityName is email
				//TODO - need to add code to prevent dups between google register and direct register
				user = GetUserByEmail( identityName );
				if ( user != null && user.Id > 0 )
					AddUserToSession( HttpContext.Current.Session, user );
			}

			return user;
		} //

		/// <summary>
		/// get account by the aspNetId,and add to session
		/// </summary>
		/// <param name="aspNetId"></param>
		/// <returns></returns>
		public static AppUser GetUserByKey( string aspNetId, bool onFoundAddToSession = true )
		{
			AppUser user = AccountManager.AppUser_GetByKey( aspNetId );
			if ( user != null && user.Id > 0  && onFoundAddToSession )
				AddUserToSession( HttpContext.Current.Session, user );

			return user;
		} //
		public static AppUser GetUser( int id )
		{
			AppUser user = AccountManager.AppUser_Get( id );

			return user;
		} //

		public static AppUser GetAccount( int id )
		{
			return AccountManager.Get( id );
		} //

		public static List<AppUser> EmailAutocomplete( string keyword = "", int maxTerms = 25 )
		{
			int userId = AccountServices.GetCurrentUserId();
			int pTotalRows = 0;
			string filter = string.Format( " ( email like '%{0}%'  or FirstName like '%{0}%'  or lastName like '%{0}%' ) ", keyword );
			return AccountManager.Search( filter, "Email", 1, maxTerms, ref pTotalRows );
		}

		public static List<AppUser> SearchByKeyword( string keywords, string pOrderBy, string sortDirection, int pageNumber, int pageSize, ref int pTotalRows )
		{
			string filter = "";
			SetKeywordFilter( keywords, ref filter );
			return Search( filter, pOrderBy, sortDirection, pageNumber, pageSize, ref pTotalRows );
		}

		/// <summary>
		/// Search using filter already formatted
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="pOrderBy"></param>
		/// <param name="sortDirection"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
		public static List<AppUser> Search( string filter, string pOrderBy, string sortDirection, int pageNumber, int pageSize, ref int pTotalRows )
		{
			//probably should validate valid order by - or do in proc
			if ( string.IsNullOrWhiteSpace( pOrderBy ) )
				pOrderBy = "LastName";

			if ( "firstname lastname email id created lastlogon".IndexOf( pOrderBy.ToLower() ) == -1 )
				pOrderBy = "LastName";

			if ( sortDirection.ToLower() == "desc" )
				pOrderBy += " DESC";


			//string filter = "";
			int userId = 0;
			AppUser user = AccountServices.GetCurrentUser();
			if ( user != null && user.Id > 0 )
				userId = user.Id;


			return AccountManager.Search( filter, pOrderBy, pageNumber, pageSize, ref pTotalRows, userId );
		}

		private static void SetKeywordFilter( string keywords, ref string where )
		{
			if ( string.IsNullOrWhiteSpace( keywords ) )
				return;
			string text = " (FirstName like '{0}' OR LastName like '{0}'  OR Email like '{0}'  ) ";

			string AND = "";
			if ( where.Length > 0 )
				AND = " AND ";
			//
			keywords = ServiceHelper.HandleApostrophes( keywords );
			if ( keywords.IndexOf( "%" ) == -1 )
				keywords = "%" + keywords.Trim() + "%";

			where = where + AND + string.Format( " ( " + text + " ) ", keywords );


		}

		[Obsolete]
		public static List<DT.AspNetRoles> GetRoles()
		{
			return AccountManager.GetRolesOld();
		}
		/// <summary>
		/// Get all active application roles
		/// </summary>
		/// <returns></returns>
		public static List<UserRole> GetAllApplicationRoles()
		{
			return ApplicationManager.GetAllApplicationRoles();
		}

        public static List<int> GetDefaultRoles()
        {
			var output = new List<int>();

            var defaultRole = UtilityManager.GetAppKeyValue( "defaultRole", "Site Reader" );
            var list = ApplicationManager.GetAllApplicationRoles();
            var inputRoles = list.FirstOrDefault( t => t.Name == defaultRole );
			if (inputRoles != null && inputRoles.Id > 0 )
				output.Add( inputRoles.Id );

			return output;
        }
        public bool SaveApplicationRolePermissions( UserRole role, ref string statusMessage )
		{
			//return AccountManager.SaveApplicationRolePermissions( role, ref statusMessage );
            return new ApplicationManager().SaveApplicationRolePermissions( role, ref statusMessage );
        }
		public static List<ApplicationFunction> GetApplicationFunctions()
		{
			return ApplicationManager.GetApplicationFunctions();
		}

        public static bool CanUserAccessFunction( int userId, string functionCode )
        {
			return ApplicationManager.CanUserAccessFunction( userId, functionCode );
        }
        #endregion


        #region Session methods
        /// <summary>
        /// Determine if current user is a logged in (authenticated) user 
        /// </summary>
        /// <returns></returns>
        public static bool IsUserAuthenticated()
		{
			bool isUserAuthenticated = false;
			try
			{
				AppUser appUser = GetUserFromSession();
				isUserAuthenticated = IsUserAuthenticated( appUser );
			}
			catch
			{

			}

			return isUserAuthenticated;
		} //
		public static bool IsUserAuthenticated( AppUser appUser )
		{
			bool isUserAuthenticated = false;
			try
			{
				if ( appUser == null || appUser.Id == 0 || appUser.IsActive == false )
				{
					isUserAuthenticated = false;
				}
				else
				{
					isUserAuthenticated = true;
				}
			}
			catch
			{

			}

			return isUserAuthenticated;
		} //
		public static AppUser GetUserFromSession()
		{
			if ( HttpContext.Current != null && HttpContext.Current.Session != null )
			{
				return GetUserFromSession( HttpContext.Current.Session );
			}
			else
				return null;
		} //

		public static AppUser GetUserFromSession( HttpSessionState session )
		{
			AppUser user = new AppUser();
			try
			{       //Get the user
				user = ( AppUser ) session[ "user" ];
				if ( user != null )
				{
					if ( user.Id == 0 || !user.IsValid )
					{
						user.IsValid = false;
						user.Id = 0;
					}
				} else
				{
					user = new AppUser();
					user.IsValid = false;
				}
			}
			catch
			{
				user = new AppUser();
				user.IsValid = false;
			}
			return user;
		}

		/// <summary>
		/// Sets the current user to the session.
		/// </summary>
		/// <param name="session">HTTP Session</param>
		/// <param name="appUser">application User</param>
		public static void AddUserToSession( HttpSessionState session, AppUser appUser )
		{
			session[ "user" ] = appUser;

		} //

		public static void RemoveUserFromSession()
		{
			if ( HttpContext.Current.Session[ "user" ] != null )
			{
				HttpContext.Current.Session.Remove( "user" );
			}

		} //
		#endregion
		#region Proxy Code methods

		//public string Create_ForgotPasswordProxyId( int userId, ref string statusMessage )
		//{
		//	int expiryDays = ServiceHelper.GetAppKeyValue( "forgotPasswordExiryDays", 1 );
		//	return new AccountManager().Create_ProxyLoginId( userId, "Forgot Password", expiryDays, ref statusMessage );
		//}

		/// <summary>
		/// Store an identity code
		/// </summary>
		/// <param name="proxyCode"></param>
		/// <param name="userEmail"></param>
		/// <param name="proxyType"></param>
		/// <returns></returns>
		public bool Proxies_StoreProxyCode( string proxyCode, string userEmail, string proxyType )
		{
			AppUser user = GetUserByEmail( userEmail );
			return Proxies_StoreProxyCode( proxyCode, user.Id, "ForgotPassword" );
		}

		/// <summary>
		/// Store an identity code
		/// </summary>
		/// <param name="proxyCode"></param>
		/// <param name="userId"></param>
		/// <param name="proxyType"></param>
		/// <returns></returns>
		private bool Proxies_StoreProxyCode( string proxyCode, int userId, string proxyType )
		{
			string statusMessage = "";

			int expiryDays = ServiceHelper.GetAppKeyValue( "forgotPasswordExiryDays", 1 );
			return new AccountManager().Store_ProxyCode( proxyCode, userId, proxyType, expiryDays, ref statusMessage );

		}
	
		public static bool Proxy_IsCodeActive( string proxyCode )
		{
			return new AccountManager().Proxy_IsCodeActive( proxyCode );
		}
		public bool Proxy_SetInactivate( string proxyCode )
		{
			string statusMessage = "";
			return new AccountManager().InactivateProxy( proxyCode, ref statusMessage );
		}
		/*	*/
		#endregion
	}
	public class NavyCredential
	{
		public NavyCredential() { }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
        public string Identifier { get; set; }
    }

}
