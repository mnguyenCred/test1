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
                        var record = Get( entity.Name );
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
        //unlikely?
        public static AppEntity Get( string name )
        {
            var entity = new AppEntity();
            if ( string.IsNullOrWhiteSpace( name ) )
                return null;

            using ( var context = new DataEntities() )
            {
                var item = context.WorkRole
                            .FirstOrDefault( s => s.Name.ToLower() == name.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }
            return entity;
        }

        public static AppEntity Get( Guid rowId )
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                var item = context.WorkRole
                            .FirstOrDefault( s => s.RowId == rowId );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }
            return entity;
        }
        public static AppEntity Get( int id )
        {
            var entity = new AppEntity();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.WorkRole
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }

            return entity;
        }
        /// <summary>
        /// Get all 
        /// May need a get all for a rating? Should not matter as this is external data?
        /// </summary>
        /// <returns></returns>
        public static List<AppEntity> GetAll( bool includeOnlyIfHasTasks = true)
        {
            var entity = new AppEntity();
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
                                entity = new AppEntity();
                                MapFromDB( item, entity );
                                list.Add( ( entity ) );
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
					var result = new AppEntity();
					MapFromDB( item, result );
					results.Add( result );
				}
			}

			return results;
		}
		//


		public static List<AppEntity> Search( SearchQuery query )
		{
			var output = new List<AppEntity>();
			var skip = ( query.PageNumber - 1 ) * query.PageSize;
			try
			{
				using ( var context = new DataEntities() )
				{
					//Start query
					var list = context.WorkRole.AsQueryable();

					//Handle keywords filter
					var keywordsText = query.GetFilterTextByName( "search:Keyword" )?.ToLower();
					if ( !string.IsNullOrWhiteSpace( keywordsText ) )
					{
						list = list.Where( s =>
							s.Name.ToLower().Contains( keywordsText ) ||
							s.Description.ToLower().Contains( keywordsText ) ||
							s.CodedNotation.ToLower().Contains( keywordsText )
						);
					}

					//Handle Rating Task Connection
					var ratingTaskFilter = query.GetFilterByName( "navy:RatingTask" );
					if ( ratingTaskFilter != null && ratingTaskFilter.ItemIds?.Count() > 0 )
					{
						//Need to handle the negation (i.e. work roles that are not associated with this task), but all work roles get returned since there are many rows for each rating, each with a different task ID.
						//So get the work roles that do match first, then negate (or not). 
						//There's probably a better way to do this.
						var matchingWorkRoleIDs = context.RatingTask_WorkRole.Where( s => ratingTaskFilter.ItemIds.Contains( s.RatingTaskId ) ).Select( m => m.WorkRoleId ).Distinct().ToList();
						if ( ratingTaskFilter.IsNegation )
						{
							list = list.Where( s => !matchingWorkRoleIDs.Contains( s.Id ) );
						}
						else
						{
							list = list.Where( s => matchingWorkRoleIDs.Contains( s.Id ) );
						}
						/*
						list = list.Where( s =>
							 s.RatingTask_HasJob.Where( t =>
								  ratingTaskFilter.IsNegation ?
									  !ratingTaskFilter.ItemIds.Contains( t.RatingTaskId ) :
									  ratingTaskFilter.ItemIds.Contains( t.RatingTaskId )
							 ).Count() > 0
						);
						*/
					}

					//Get total
					query.TotalResults = list.Count();

					//Sort
					list = list.OrderBy( p => p.Description );

					//Get page
					var results = list.Skip( skip ).Take( query.PageSize )
						.Where( m => m != null ).ToList();

					//Populate
					foreach ( var item in results )
					{
						var entity = new AppEntity();
						MapFromDB( item, entity );
						output.Add( entity );
					}
				}

				return output;
			}
			catch ( Exception ex )
			{
				return new List<AppEntity>() { new AppEntity() { Description = "Error: " + ex.Message + " - " + ex.InnerException?.Message } };
			}
		}
		//

		/*
        public static List<AppEntity> Search ( SearchQuery query )
        {
            var entity = new AppEntity();
            var output = new List<AppEntity>();
            var skip = 0;
            if ( query.PageNumber > 1 )
                skip = ( query.PageNumber - 1 ) * query.PageSize;
            var filter = GetSearchFilterText( query );
            try
            {
                using ( var context = new ViewEntities() )
                {
                    var list = from Results in context.WorkRoleSummary
                                select Results;
                    if ( !string.IsNullOrWhiteSpace( filter ) )
                    {
                        list = from Results in list
                                .Where( s => ( s.Name.ToLower().Contains( filter.ToLower() )
                                ) )
                                select Results;
                    }
                    query.TotalResults = list.Count();
                    //sort order not handled
                    list = list.OrderBy( p => p.Name );

                    //
                    var results = list.Skip( skip ).Take( query.PageSize )
                        .ToList();
                    if ( results?.Count > 0 )
                    {
                        foreach ( var item in results )
                        {
                            if ( item != null && item.Id > 0 )
                            {
                                entity = new AppEntity();
                                MapFromDB( item, entity );
                                output.Add( ( entity ) );
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
		*/

		public static void MapFromDB( DBEntity input, AppEntity output )
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


        #endregion

    }
}
