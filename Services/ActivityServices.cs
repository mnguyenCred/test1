﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Models;
using Models.Application;
using Models.Import;
using ThisUser = Models.Application.AppUser;
using Factories;
using Navy.Utilities;

namespace Services
{
	public class ActivityServices : ServiceHelper
	{
		private static string thisClassName = "ActivityServices";
		ActivityManager mgr = new ActivityManager();

        #region Add site activity
        /// <summary>
        /// General purpose create of an Editor related site activity
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="activityEvent"></param>
        /// <param name="comment">A formatted user friendly description of the activity</param>
        /// <param name="actionByUserId">Optional userId of person initiating the activity</param>
        /// <param name="activityObjectId"></param>
        /// <param name="activityObjectParentEntityUid">Guid of top level parent for entity with activity</param>
        public void AddEditorActivity( string activityType, string activityEvent, string comment, int actionByUserId, int activityObjectId = 0, Guid? activityObjectParentEntityUid = null)
		{

			SiteActivity log = new SiteActivity();
			log.CreatedDate = System.DateTime.Now;
			log.ActivityType = activityType;
			log.Activity = "Editor";
			log.Event = activityEvent;
			log.Comment = comment;
			log.SessionId = ActivityManager.GetCurrentSessionId();
			log.IPAddress = ActivityManager.GetUserIPAddress();

			log.ActionByUserId = actionByUserId;
			log.ActivityObjectId = activityObjectId;
			log.TargetUserId = 0;
			log.ActivityObjectParentEntityUid = activityObjectParentEntityUid;

			try
			{
				mgr.SiteActivityAdd( log );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".AddEditorActivity()" );
				return;
			}
		}

        /// <summary>
        /// General purpose create of an Editor related site activity
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="activityEvent"></param>
        /// <param name="comment"></param>
        /// <param name="actionByUserId"></param>
        /// <param name="targetUserId"></param>
        /// <param name="activityObjectId"></param>
        /// <param name="objectRelatedId"></param>
        public void AddEditorActivity( string activityType, string activityEvent, string comment, int actionByUserId, int targetUserId = 0, int activityObjectId = 0, int objectRelatedId = 0 )
        {

            SiteActivity log = new SiteActivity();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = activityType;
            log.Activity = "Editor";
            log.Event = activityEvent;
            log.Comment = comment;
            log.SessionId = ActivityManager.GetCurrentSessionId();
            log.IPAddress = ActivityManager.GetUserIPAddress();

            log.ActionByUserId = actionByUserId;
            log.ActivityObjectId = activityObjectId;
            log.TargetUserId = targetUserId;
            log.ObjectRelatedId = objectRelatedId;

            try
            {
                mgr.SiteActivityAdd( log );
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".AddEditorActivity()" );
                return;
            }
        }
        /// <summary>
        /// Add site activity - provide specific ActivityType and Activity
        /// </summary>
        /// <param name="log"></param>
        public void AddActivity( SiteActivity log )
		{

			if ( log.SessionId == null || log.SessionId.Length < 10 )
				log.SessionId = ActivityManager.GetCurrentSessionId();

			if ( log.IPAddress == null || log.IPAddress.Length < 10 )
				log.IPAddress = ActivityManager.GetUserIPAddress();

			try
			{
				int id = mgr.SiteActivityAdd( log );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".AddActivity(SiteActivity log)" );
				return;
			}
		}
		#region Site Activity - account
		public static int UserRegistration( AppUser entity, string type = "Registration", string message = "" )
		{
			string ipAddress = ActivityManager.GetUserIPAddress();
			return UserRegistration( entity, ipAddress, type, message );

		}
		public static int UserRegistration( AppUser entity, string ipAddress )
		{
			return UserRegistration( entity, ipAddress, "Registration" );
			
		}

		public static int UserRegistrationFromGoogle( ThisUser entity, string ipAddress )
		{
			return UserRegistration( entity, ipAddress, "Google SSO Registration" );
			
		}

		public static int UserRegistration( ThisUser entity, string ipAddress, string type, string extra = "" )
		{
			string server = UtilityManager.GetAppKeyValue( "serverName", "" );

			SiteActivity log = new SiteActivity();
			log.CreatedDate = System.DateTime.Now;
			log.ActivityType = "Account";
			log.Activity = "Management";
			log.Event = type;
			log.Comment = string.Format( "{0} ({1}) {4}. From IPAddress: {2}, on server: {3}. {5}", entity.FullName(), entity.Id, ipAddress, server, type, extra );
			//actor type - person, system
			log.ActionByUserId = entity.Id;
			log.TargetUserId = entity.Id;
			log.SessionId = ActivityManager.GetCurrentSessionId();
			try
			{
				return new ActivityManager().SiteActivityAdd( log );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".UserRegistrationFromPortal()" );
				return 0;
			}
		}
		public static int UserRegistrationConfirmation( ThisUser entity, string type )
		{
			string server = UtilityManager.GetAppKeyValue( "serverName", "" );
			//EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
			SiteActivity log = new SiteActivity();

			try
			{
				log.CreatedDate = System.DateTime.Now;
				log.ActivityType = "Account";
				log.Activity = "Management";
				log.Event = "Confirmation";
				log.Comment = string.Format( "{0} ({1}) Registration Confirmation, on server: {2}, tyep: {3}", entity.FullName(), entity.Id, server, type );
				//actor type - person, system
				log.ActionByUserId = entity.Id;
				log.TargetUserId = entity.Id;

				log.SessionId = ActivityManager.GetCurrentSessionId();

				return new ActivityManager().SiteActivityAdd( log );

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".UserRegistrationConfirmation()" );
				return 0;
			}
		}
		public static int AdminLoginAsUserAuthentication( ThisUser entity )
		{
			return UserAuthentication( entity, "Admin PASSKEY Login " );
		}
		/// <summary>
		/// form based login
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static int UserAuthentication( ThisUser entity )
		{
			return UserAuthentication( entity, "login" );
		}
		public static int UserExternalAuthentication( ThisUser entity, string provider )
		{
			return UserAuthentication( entity, provider );
		}
		private static int UserAuthentication( ThisUser entity, string type )
		{

			string server = UtilityManager.GetAppKeyValue( "serverName", "" );
			SiteActivity log = new SiteActivity();
			log.CreatedDate = System.DateTime.Now;
			log.ActivityType = "Account";
			log.Activity = "Account";
			log.Event = "Authentication: " + type;
			log.Comment = string.Format( "{0} ({1}) logged in ({2}) on server: {3}", entity.FullName(), entity.Id, type, server );
			//actor type - person, system
			log.ActionByUserId = entity.Id;
			log.TargetUserId = entity.Id;
			log.SessionId = ActivityManager.GetCurrentSessionId();
			try
			{
				return new ActivityManager().SiteActivityAdd( log );

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".UserAuthentication()" );
				return 0;
			}
		}

		public static int UserAcknowledgement( ThisUser entity, string ackEvent )
		{

			string server = UtilityManager.GetAppKeyValue( "serverName", "" );
			SiteActivity log = new SiteActivity();
			log.CreatedDate = System.DateTime.Now;
			log.ActivityType = "Account";
			log.Activity = "Acknowledgement";
			log.Event = ackEvent;
			log.Comment = string.Format( "{0} ({1}) acknowledged '{2}' on server: {3}", entity.FullName(), entity.Id, ackEvent, server );
			//actor type - person, system
			log.ActionByUserId = entity.Id;
			log.TargetUserId = entity.Id;
			log.SessionId = ActivityManager.GetCurrentSessionId();
			try
			{
				return new ActivityManager().SiteActivityAdd( log );

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".UserAcknowledgement()" );
				return 0;
			}
		}
		public static void ApplicationStartActivity()
        {
            SiteActivity log = new SiteActivity();
            //bool isBot = false;
            string server = UtilityManager.GetAppKeyValue( "serverName", "" );
            try
            {
                log.CreatedDate = System.DateTime.Now;
                log.ActivityType = "Site";
                log.Activity = "Application";
                log.Event = "Start";
                log.Comment = "The application was restarted.";


                new ActivityManager().SiteActivityAdd( log );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ApplicationStartActivity()" );
                return;
            }

        }
        public static void SessionStartActivity( string comment, string sessionId, string ipAddress, string referrer, bool isBot )
		{
			SiteActivity log = new SiteActivity();
			//bool isBot = false;
			string server = UtilityManager.GetAppKeyValue( "serverName", "" );
			try
			{
				if ( comment == null )
					comment = "";

				log.CreatedDate = System.DateTime.Now;
				log.ActivityType = "Account";
				log.Activity = "Session";
				log.Event = "Start";
				log.Comment = comment + string.Format( " (on server: {0})", server );
				//already have agent
				//log.Comment += GetUserAgent(ref isBot);

				log.SessionId = sessionId;
				log.IPAddress = ipAddress;
				log.Referrer = referrer;

				new ActivityManager().SiteActivityAdd( log );

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".SessionStartActivity()" );
				return;
			}

		}
		#endregion

		//public static int SiteActivityAdd( string activity, string eventType, string comment
		//			, int actionByUserId, int targetUserId, int activityObjectId )
		//{
		//	return SiteActivityAdd( activity, eventType, comment, actionByUserId, targetUserId, activityObjectId, "", "" );
		//}

		public static int SiteActivityAdd(string activityType, string activity, string eventType, string comment
					, int actionByUserId, int targetUserId, int activityObjectId
					, string sessionId, string ipAddress )
		{
			string server = UtilityManager.GetAppKeyValue( "serverName", "" );
			SiteActivity log = new SiteActivity();
			if ( sessionId == null || sessionId.Length < 10 )
				sessionId = HttpContext.Current.Session.SessionID;

			if ( ipAddress == null || ipAddress.Length < 10 )
				ipAddress = ActivityManager.GetUserIPAddress();

			try
			{
				log.CreatedDate = System.DateTime.Now;
				log.ActivityType = activityType;
				log.Activity = activity;
				log.Event = eventType;
                log.Comment = comment;// + string.Format( " (on server: {0})", server );
				//actor type - person, system
				if ( actionByUserId > 0 )
					log.ActionByUserId = actionByUserId;
				if ( targetUserId > 0 )
					log.TargetUserId = targetUserId;
				if ( activityObjectId > 0 )
					log.ActivityObjectId = activityObjectId;

				log.SessionId = sessionId;
				log.IPAddress = ipAddress;

				return new ActivityManager().SiteActivityAdd( log );

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".SiteActivityAdd()" );
				return 0;
			}
		} //
        #endregion

        #region Activity Search
   
		public static List<SiteActivity> SearchAll( BaseSearchModel parms, ref int pTotalRows )
		{

			//probably should validate valid order by - or do in proc
			if ( string.IsNullOrWhiteSpace( parms.OrderBy ) )
			{
				parms.IsDescending = true;
				parms.OrderBy = "CreatedDate";
				//pOrderBy = "Created DESC";
			}
			else
			{
				if ( "id activity event email comment createddate actionbyuser".IndexOf( parms.OrderBy.ToLower() ) == -1 )
				{
					parms.OrderBy = "CreatedDate";
					//pOrderBy = "Created DESC";
				}
			}
			List<SiteActivity> list = ActivityManager.SearchAll( parms );
			pTotalRows = parms.TotalRows;
			return list;
		}

		public static List<SiteActivity> Search( BaseSearchModel parms, ref int pTotalRows )
		{

			//probably should validate valid order by - or do in proc
			if ( string.IsNullOrWhiteSpace( parms.OrderBy ) )
			{
				parms.IsDescending = true;
				parms.OrderBy = "CreatedDate desc";
				//pOrderBy = "Created DESC";
			}
			else
			{
				if ( "id activity event comment createddate actionbyuser".IndexOf( parms.OrderBy.ToLower() ) == -1 )
				{
					parms.OrderBy = "CreatedDate ";
                    if ( parms.IsDescending )
                        parms.OrderBy += " DESC";
                } else if ( parms.IsDescending && parms.OrderBy.ToLower().IndexOf("desc") == -1)
                {
                    parms.OrderBy = parms.OrderBy + " desc";
                }
			}
			List<SiteActivity> list = ActivityManager.Search( parms.Filter, parms.OrderBy, parms.PageNumber, parms.PageSize, ref pTotalRows );
			//pTotalRows = parms.TotalRows;
			return list;
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

		#endregion

	}
}
