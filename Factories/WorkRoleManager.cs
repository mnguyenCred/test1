using System;
using System.Collections.Generic;
using System.Linq;

using Models.Application;
using Models.Curation;

using Navy.Utilities;

using AppEntity = Models.Schema.WorkRole;
using ViewEntity = Data.Views.WorkRoleSummary;
using DataEntities = Data.Tables.NavyRRLEntities;
using ViewEntities = Data.Views.ceNavyViewEntities;
using DBEntity = Data.Tables.WorkRole;
using Models.Search;

namespace Factories
{
    public class WorkRoleManager : BaseFactory
    {
        public static new string thisClassName = "WorkRoleManager";

		#region === persistance ==================
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
				BasicSaveCore( context, entity, context.WorkRole, userID, ( ent, dbEnt ) => { }, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

		/// <summary>
		/// Update a WorkRole
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Save( AppEntity entity, ref ChangeSummary status )
        {
            bool isValid = true;
            int count = 0;
            if ( entity == null )
            {
                return false;
            }
            if ( string.IsNullOrEmpty( entity.Name ) || entity.Name.ToLower() == "missing")
            {
                status.AddError( thisClassName + string.Format( ".Save. The WorkRole Name is required, and is missing. This could cause an issue if referenced by another entity. The name will be set to Missing, and will require followup. UID: '{0}'", entity.RowId ) );
                //entity.Name = "Missing";
                return false;
            }
            try
            {
                using ( var context = new DataEntities() )
                {
                    //look up if no id
                    if ( entity.Id == 0 )
                    {
                        var record = GetByName( entity.Name );
                        if ( record?.Id > 0 )
                        {
                            //currently no description, so can just return
                            //do a check to see if the rowId is different
                            entity.Id = record.Id;
                            return true;
                        }
                        else
                        {
                            //add
                            int newId = Add( entity, ref status );
                            if ( newId == 0 || status.HasSectionErrors )
                                isValid = false;
                        }
                    }
                    else
                    {
                        //TODO - consider if necessary, or interferes with anything
                        context.Configuration.LazyLoadingEnabled = false;
                        DBEntity efEntity = context.WorkRole
                                .SingleOrDefault( s => s.Id == entity.Id );

                        if ( efEntity != null && efEntity.Id > 0 )
                        {
                            //fill in fields that may not be in entity
                            entity.RowId = efEntity.RowId;
                            entity.Created = efEntity.Created;
                            entity.CreatedById = ( efEntity.CreatedById ?? 0 );
                            entity.Id = efEntity.Id;

                            MapToDB( entity, efEntity );

                            if ( HasStateChanged( context ) )
                            {
                                efEntity.LastUpdated = DateTime.Now;
                                efEntity.LastUpdatedById = entity.LastUpdatedById;
                                count = context.SaveChanges();
                                //can be zero if no data changed
                                if ( count >= 0 )
                                {
                                    entity.LastUpdated = ( DateTime ) efEntity.LastUpdated;
                                    isValid = true;
                                }
                                else
                                {
                                    //?no info on error

                                    isValid = false;
                                    string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a WorkRole. The process appeared to not work, but was not an exception, so we have no message, or no clue. WorkRole: {0}, Id: {1}", entity.Name, entity.Id );
                                    status.AddError( "Error - the update was not successful. " + message );
                                    EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                                }

                            }

                            if ( isValid )
                            {
                                SiteActivity sa = new SiteActivity()
                                {
                                    ActivityType = "WorkRole",
                                    Activity = "Import",
                                    Event = "Update",
                                    Comment = string.Format( "WorkRole was updated by the import. Name: {0}", entity.Name ),
                                    ActionByUserId = entity.LastUpdatedById,
                                    ActivityObjectId = entity.Id
                                };
                                new ActivityManager().SiteActivityAdd( sa );
                            }
                        }
                        else
                        {
                            status.AddError( "Error - update failed, as record was not found." );
                        }
                    }

                }
            }
            catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
            {
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), "WorkRole" );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), true );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
                isValid = false;
            }


            return isValid;
        }

        /// <summary>
        /// add a WorkRole
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int Add( AppEntity entity, ref ChangeSummary status )
        {
            DBEntity efEntity = new DBEntity();
            status.HasSectionErrors = false;
            using ( var context = new DataEntities() )
            {
                try
                {
                    entity.CreatedById = entity.LastUpdatedById;
                    MapToDB( entity, efEntity );

                    if ( IsValidGuid( entity.RowId ) )
                        efEntity.RowId = entity.RowId;
                    else
                        efEntity.RowId = Guid.NewGuid();
                    if ( IsValidCtid( entity.CTID ) )
                        efEntity.CTID = entity.CTID;
                    else
                        efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                    entity.Created = efEntity.Created = DateTime.Now;
                    entity.LastUpdated = efEntity.LastUpdated = DateTime.Now;
                    efEntity.CreatedById = efEntity.LastUpdatedById = entity.LastUpdatedById;

                    context.WorkRole.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        entity.RowId = efEntity.RowId;
                        entity.Id = efEntity.Id;
                        //
                        //add log entry
                        SiteActivity sa = new SiteActivity()
                        {
                            ActivityType = "WorkRole",
                            Activity = "Import",
                            Event = "Add",
                            Comment = string.Format( "WorkRole was added by the import. Name: {0}", entity.Name ),
                            ActionByUserId = entity.LastUpdatedById,
                            ActivityObjectId = entity.Id
                        };
                        new ActivityManager().SiteActivityAdd( sa );
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a WorkRole. The process appeared to not work, but was not an exception, so we have no message, or no clue. WorkRole: {0}, ctid: {1}", entity.Name, entity.CTID );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( "WorkRoleManager. Add Failed", message );
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}", entity.Name) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }

            return efEntity.Id;
        }
        public static void MapToDB( AppEntity input, DBEntity output )
        {
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );

        }

        /// <summary>
        /// Delete record
        /// Do not allow if referenced anywhere.
        /// DEFER FOR NOW
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="deletedBy"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( int recordId, AppUser deletedBy, ref string statusMessage )
        {
            bool isValid = false;
            using ( var context = new DataEntities() )
            {
                DBEntity efEntity = new DBEntity();
                try
                {
                    efEntity = context.WorkRole
                            .SingleOrDefault( s => s.Id == recordId );
                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        context.WorkRole.Remove( efEntity );
                        int count = context.SaveChanges();
                        //add activity log 
                        if ( count >= 0 )
                        {
                            isValid = true;
                        }
                    }
                    else
                    {
                        statusMessage = "Error/Warning - record was not found.";
                        isValid = true;
                    }


                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Delete(), Name: {0}", efEntity.Name ) );
                }
            }

            return isValid;
        }

        #endregion

        #region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.WorkRole, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

        //unlikely?
        public static AppEntity GetByName( string name, bool returnNullIfNotFound = false )
        {
			return GetSingleByFilter( m => m.Name?.ToLower() == name?.ToLower(), returnNullIfNotFound );
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

		/// <summary>
		/// Get all 
		/// May need a get all for a rating? Should not matter as this is external data?
		/// </summary>
		/// <returns></returns>
		public static List<AppEntity> GetAll( bool includeOnlyIfHasTasks = true)
        {
            var list = new List<AppEntity>();

            try
            {
                using ( var context = new ViewEntities() )
                {
                    var results = context.WorkRoleSummary
                            .Where( g => !includeOnlyIfHasTasks || g.HasRatingTasks > 0 )
                            .OrderBy( s => s.Name )
                            .ToList();
                    if ( results?.Count > 0 )
                    {
                        foreach ( var item in results )
                        {
                            if ( item != null && item.Id > 0 )
                            {
                                var entity = new AppEntity();
                                MapFromDB( item, entity );
                                list.Add( entity );
                            }
                        }
                    }

                }
            } catch ( Exception ex )
            {

            }
            return list;
        }
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			var results = new List<AppEntity>();

			using ( var context = new DataEntities() )
			{
				var items = context.WorkRole
					.Where( m => guids.Contains( m.RowId ) )
					.OrderBy( m => m.Description )
					.ToList();

				foreach ( var item in items )
				{
					results.Add( MapFromDB( item, context ) );
				}
			}

			return results;
		}
		//

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
		{
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.WorkRole.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Name.Contains( keywords ) ||
						m.Description.Contains( keywords ) ||
						m.CodedNotation.Contains( keywords )
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
            //

        }
		//


        #endregion

    }
}
