using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

using Models.Application;
using Models.Curation;
using Models.Search;

using Navy.Utilities;

using AppEntity = Models.Schema.BilletTitle;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.Job;
namespace Factories
{
    public class JobManager : BaseFactory
    {
        public static new string thisClassName = "JobManager";
        public static string cacheKey = "JobCache";

        #region === persistance ==================
        /// <summary>
        /// Update a Job
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool Save( AppEntity entity, ref ChangeSummary status )
        {
            bool isValid = true;
            int count = 0;

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
                        DBEntity efEntity = context.Job
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
                                    string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a Job. The process appeared to not work, but was not an exception, so we have no message, or no clue. Job: {0}, Id: {1}", entity.Name, entity.Id );
                                    status.AddError( "Error - the update was not successful. " + message );
                                    EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                                }

                            }

                            if ( isValid )
                            {
                                SiteActivity sa = new SiteActivity()
                                {
                                    ActivityType = "Job",
                                    Activity = "Import",
                                    Event = "Update",
                                    Comment = string.Format( "Job was updated by the import. Name: {0}", entity.Name ),
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
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), "Job" );
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
        /// add a Job
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private int Add( AppEntity entity, ref ChangeSummary status )
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

                    context.Job.Add( efEntity );

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
                            ActivityType = "Job",
                            Activity = "Import",
                            Event = "Add",
                            Comment = string.Format( "Job was added by the import. Name: {0}", entity.Name ),
                            ActionByUserId = entity.LastUpdatedById,
                            ActivityObjectId = entity.Id
                        };
                        new ActivityManager().SiteActivityAdd( sa );
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a Job. The process appeared to not work, but was not an exception, so we have no message, or no clue. Job: {0}, ctid: {1}", entity.Name, entity.CTID );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( "JobManager. Add Failed", message );
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}", efEntity.Name ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }

            return efEntity.Id;
        }
        public void UpdateParts( AppEntity input, ChangeSummary status )
        {
            try
            {
                //HasRatingTask - this is done from RatingTask
                //HasRatingTaskUpdate( input, ref status );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }

        public static void MapToDB( AppEntity input, DBEntity output )
        {
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );

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
                var item = context.Job
                            .FirstOrDefault( s => s.Name.ToLower() == name.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }
            return entity;
        }
        public static AppEntity GetByName( string billetTitle )
        {
            var output = new AppEntity();
            var list = GetAll();
            output = list.FirstOrDefault( s => s.Name.ToLower() == billetTitle.ToLower() );
            return output;
        } //
        public static AppEntity Get( Guid rowId )
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                var item = context.Job
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
                var item = context.Job
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
        public static List<AppEntity> GetAll()
        {
            var entity = new AppEntity();
            var list = new List<AppEntity>();
            list = CheckCache();
            if (list?.Count > 0)
                return list;

            list = new List<AppEntity>();
            using ( var context = new DataEntities() )
            {
                var results = context.Job
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
                    AddToCache( list );
                }
               
            }
            return list;
        }
        public static List<AppEntity> Search( SearchQuery query )
        {
            var entity = new AppEntity();
            var output = new List<AppEntity>();
            var skip = 0;
            if ( query.PageNumber > 1 )
                skip = ( query.PageNumber - 1 ) * query.PageSize;
            var filter = "";
            if ( query.Filters?.Count > 0 )
            {
                filter = query.Filters[0].Text ?? "".TrimEnd();
            }
            try
            {
                using ( var context = new DataEntities() )
                {
                    var list = from Results in context.Job
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

        public static List<AppEntity> CheckCache()
        {
            var cache = new CachedBillets();
            var list = new List<AppEntity>();
            int cacheHours = 8;
            DateTime maxTime = DateTime.Now.AddHours( cacheHours * -1 );
            if ( MemoryCache.Default.Get( cacheKey ) != null && cacheHours > 0 )
            {
                cache = ( CachedBillets ) MemoryCache.Default.Get( cacheKey );
                try
                {
                    if ( cache.LastUpdated > maxTime )
                    {
                        LoggingHelper.DoTrace( 6, string.Format( thisClassName + ".CheckCache. Using cached version of BilletTitle" ) );
                        list = cache.Billets;
                        return list;
                    }
                }
                catch ( Exception ex )
                {
                    LoggingHelper.DoTrace( 5, thisClassName + ".CheckCache === exception " + ex.Message );
                }
            }
            //get
            return null;

        }
        public static void AddToCache( List<AppEntity> input)
        {
            int cacheHours = 8;
            //add to cache
            if ( cacheKey.Length > 0 && cacheHours > 0 )
            {
                var newCache = new CachedBillets()
                {
                    Billets = input,
                    LastUpdated = DateTime.Now
                };
                if ( MemoryCache.Default.Get( cacheKey ) != null )
                {
                    MemoryCache.Default.Remove( cacheKey );
                }
                //
                MemoryCache.Default.Add( cacheKey, newCache, new DateTimeOffset( DateTime.Now.AddHours( cacheHours ) ) );
                LoggingHelper.DoTrace( 5, thisClassName + ".AddToCache $$$ Updating cached version " );

            }
        }
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


        #endregion

    }
}
