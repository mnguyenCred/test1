using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Application;
using Models.Curation;

using ParentEntity = Models.Schema.Course;
using AppEntity = Models.Schema.TrainingTask;
using DBEntity = Data.Tables.TrainingTask;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Navy.Utilities;
using Models.Search;
using System.Runtime.Caching;

namespace Factories
{
    public class TrainingTaskManager : BaseFactory
    {
        public static new string thisClassName = "TrainingTaskManager";
        public static string cacheKey = "TrainingTaskCache";

		#region Persistance
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
				BasicSaveCore( context, entity, context.TrainingTask, userID, ( ent, dbEnt ) => {
					dbEnt.ReferenceResourceId = context.ReferenceResource.FirstOrDefault( m => m.RowId == ent.HasReferenceResource )?.Id ?? 0;
				}, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

		/// <summary>
		/// Save list of training tasks, from Course object
		/// </summary>
		/// <param name="input"></param>
		/// <param name="status"></param>
		public void SaveList( ParentEntity input, ref ChangeSummary status )
        {
            //need to do the check for stuff to delete - or TBD if the process step figures out the proper stuff
            //initially doesn't hurl to leave old tasks, as will be ignored. 
            //at very least, have check if referenced anywhere 
           
            if ( input.TrainingTasks != null )
            {
                foreach ( var item in input.TrainingTasks )
                {
                    Save( input, item, ref status );
                }
            }

            if ( input.HasTrainingTask?.Count > 0 )
            {
                //these are the guids, but the task can't be created before the course, so are these for tasks that exist?
                //and task cannot be orphaned as CourseId is non nullable
                //22-10-02 now with courseContext, can have orphan tasks
                //
                //this note came while working with the created list (although this course did already exist?)
            }
        }
        public bool Save(  AppEntity entity, ref ChangeSummary status )
        {
            //get parent
            ParentEntity parent = new ParentEntity();
            //will we have the guid?
            if ( IsValidGuid( entity.Course ) )
            {
                parent = CourseManager.GetByRowId( entity.Course );
            } else if ( !string.IsNullOrWhiteSpace( entity.CourseCodedNotation ) )
            {
                parent = CourseManager.GetByCodedNotation( entity.CourseCodedNotation );
            }

            return Save( parent, entity, ref status );
        }

        //from edit view
        public bool Save( string courseCodedNotation, AppEntity entity, ref ChangeSummary status )
        {
            //get parent
            ParentEntity parent = new ParentEntity();
            if ( !string.IsNullOrWhiteSpace( courseCodedNotation ) )
            {
                parent = CourseManager.GetByCodedNotation( entity.CourseCodedNotation );
            } else
            {
                //error
            }

            return Save( parent, entity, ref status );
        }
        public bool Save( ParentEntity parent, AppEntity input, ref ChangeSummary status )
        {
            bool isValid = true;
            try
            {
                using ( var context = new DataEntities() )
                {
                    //check existance
                    //or may want to assign and check for change (could be legit case change). 
                    //  use rowId if present - although the upload may have no way to determine if an existing task is being updated - 
                    DBEntity efEntity = new DBEntity();
                    if ( IsValidGuid( input.RowId ) )
                    {
                        efEntity = context.TrainingTask.FirstOrDefault( s => s.RowId == input.RowId );
                    }
                    //don't have a means to do other checks now
                    //if ( efEntity == null )
                    //{
                    //    efEntity = context.TrainingTask
                    //        .FirstOrDefault( s => s.CourseId == parent.Id && s.Description.ToLower() == input.Description.ToLower() );
                    //}
                    if ( efEntity?.Id > 0 )
                    {
                        //update
                        List<string> errors = new List<string>();
                        //this will include the extra props (like LifeCycleControlDocument, etc. for now)
                        //fill in fields that may not be in entity
                        input.RowId = efEntity.RowId;
                       // input.CourseId = efEntity.CourseId;
                        input.Created = efEntity.Created;
                        input.LastUpdated = efEntity.LastUpdated;
                        input.CreatedById = ( efEntity.CreatedById ?? 0 );
                        input.LastUpdatedById = ( efEntity.LastUpdatedById ?? 0 );
                        input.Id = efEntity.Id;
                        BaseFactory.AutoMap( input, efEntity, errors );

                        if ( HasStateChanged( context ) )
                        {
                            efEntity.LastUpdatedById = parent.LastUpdatedById;
                            efEntity.LastUpdated = DateTime.Now;
                            var count = context.SaveChanges();
                            //can be zero if no data changed
                            if ( count >= 0 )
                            {
                                input.LastUpdated = ( DateTime ) efEntity.LastUpdated;
                                SiteActivity sa = new SiteActivity()
                                {
                                    ActivityType = "TrainingTask",
                                    Activity = status.Action,
                                    Event = "Update",
                                    Comment = string.Format( "TrainingTask was updated. Task: {0}", FormatLongLabel( input.Description ) ),
                                    ActionByUserId = input.LastUpdatedById,
                                    ActivityObjectId = input.Id
                                };
                                new ActivityManager().SiteActivityAdd( sa );


                            }
                            else
                            {
                                //?no info on error

                                string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a CourseTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, (Id: {1}), Task: '{2}'", parent.Name, parent.Id, input.Description );
                                status.AddError( "Error - the update was not successful. " + message );

                                EmailManager.NotifyAdmin( thisClassName + ".CourseTaskSave Failed Failed", message );
                            }
                        }
                        //TBD: will AssessmentMethodType still be on the course, or on the training task.
                        //NOTE: if save comes from the standalone editor, there will be no asmts

                        //TrainingTaskAssessmentMethodSave( input, ref status );
                    }
                    else
                    {
                        var result = Add( parent, input, ref status );
                    }
                }
            }
            catch ( Exception ex )
            {
                var msg = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + ".Save" );
                status.AddError( thisClassName + String.Format(".Save. Error - the Save was not successful. CIN: {0}, TrainingTask: '{1}' msg: {2} ", parent.CodedNotation, FormatLongLabel(input.Description), msg) );
            }
            return isValid;
        }

        public bool Add( ParentEntity parent, AppEntity input, ref ChangeSummary status )
        {
            var entityType = "CourseTask";
            var efEntity = new Data.Tables.TrainingTask();
            status.HasSectionErrors = false;
            //need to do a look up

            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( IsValidGuid( input.RowId ) )
                        efEntity.RowId = input.RowId;
                    else
                        efEntity.RowId = Guid.NewGuid();
                    if ( IsValidCtid( input.CTID ) )
                        efEntity.CTID = input.CTID;
                    else
                        efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();

                   // efEntity.CourseId = parent.Id;
                    efEntity.Description = input.Description;
                    efEntity.CreatedById = efEntity.LastUpdatedById = parent.LastUpdatedById;
                    efEntity.Created = efEntity.LastUpdated = DateTime.Now;

                    context.TrainingTask.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        SiteActivity sa = new SiteActivity()
                        {
                            ActivityType = "TrainingTask",
                            Activity = status.Action,
                            Event = "Add",
                            Comment = string.Format( "TrainingTask was added. Task: {0}", FormatLongLabel( input.Description ) ),
                            ActionByUserId = parent.LastUpdatedById,
                            ActivityObjectId = parent.Id
                        };
                        new ActivityManager().SiteActivityAdd( sa );
                        //add assessments
                        input.Id = efEntity.Id;
                        //TrainingTaskAssessmentMethodSave( input, ref status );

                        //
                        return true;
                    }
                    else
                    {
                        //?no info on error
                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a {0}. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: '{2}' ({3}) for task: '{4}', was added for by the import.", entityType, parent.Name, parent.Id, input.Description );
                        status.AddError( thisClassName + String.Format( ".Add-'{0}' Error - the add was not successful. ", entityType ) + message );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add-'{0}', Course: '{1}' ({2}) for task: '{3}'", entityType, parent.Name, parent.Id, input.Description ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }
            return false;
        }


        #endregion


        #region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.TrainingTask, FilterMethod, MapFromDB, returnNullIfNotFound );
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

		public static AppEntity GetForUploadOrNull( string trainingTaskDescription, Guid referenceResourceRowID )
		{
			using ( var context = new DataEntities() )
			{
				var match = context.TrainingTask.FirstOrDefault( s =>
					s.Description.ToLower() == trainingTaskDescription.ToLower() &&
					context.ReferenceResource.FirstOrDefault( m => 
						s.ReferenceResourceId == m.Id && 
						m.RowId == referenceResourceRowID 
					) != null
				);

				if ( match != null )
				{
					return MapFromDB( match, context ) ;
				}
			}

			return null;
		}
		//

		/// <summary>
		/// Get all 
		/// May need a get all for a rating? Should not matter as this is external data?
		/// </summary>
		/// <returns></returns>
		public static List<AppEntity> GetAll()
        {
            var entity = new AppEntity();
            var list = new List<AppEntity>();

            using ( var context = new DataEntities() )
            {
                var results = context.TrainingTask
                        .OrderBy( s => s.Description )
                        .ToList();
                if ( results?.Count > 0 )
                {
                    foreach ( var item in results )
                    {
						list.Add( MapFromDB( item, context ) );
					}
				}

            }
            return list;
        }
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			var results = new List<AppEntity>();

			using ( var context = new DataEntities() )
			{
				var items = context.TrainingTask
					.Where( m => guids.Contains( m.RowId ) )
					.OrderBy( m => m.Description )
					.ToList();

				foreach( var item in items )
				{
					results.Add( MapFromDB( item, context ) );
				}
			}

			return results;
		}
		//

		/// <summary>
		/// Get all training tasks for a rating
		/// </summary>
		/// <param name="ratingCodedNotation"></param>
		/// <param name="includingAllSailorsTasks">This should soon be obsolete</param>
		/// <param name="totalRows"></param>
		/// <returns></returns>
		public static List<AppEntity> GetAllForRating( string ratingCodedNotation, bool includingAllSailorsTasks, ref int totalRows )
        {
            int pageNumber = 1;
            //what is a reasonable max number for all tasks for a rating?
            int pageSize = 10000;
            //int totalRows = 0;
            if ( includingAllSailorsTasks )
                pageSize = 0;
            int userId = 0;
            return GetAll( ratingCodedNotation, includingAllSailorsTasks, pageNumber, pageSize, ref totalRows );
        }


        /// <summary>
        /// Get all for a rating
        /// </summary>
        /// <param name="ratingCodedNotation"></param>
        /// <param name="includingAllSailorsTasks"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRows"></param>
        /// <returns></returns>
        public static List<AppEntity> GetAll( string ratingCodedNotation, bool includingAllSailorsTasks, int pageNumber, int pageSize, ref int totalRows )
        {
            //!!! this is very slow when getting 1600+ . Could have an async task to pre-cache
            var entity = new AppEntity();
            var list = new List<AppEntity>();
            //caching will have to be specific to the request
            //-no caching if not getting all? That is if pageSize > 0
            if ( pageSize == 0 || pageSize > 1700 )
            {
                list = CheckCache( ratingCodedNotation, includingAllSailorsTasks );
                if ( list?.Count > 0 )
                    return list;
            }

            list = new List<AppEntity>();

            string filter = "";
            string orderBy = "";
            //int pageNumber = 1;
            //what is a reasonable max number for all tasks for a rating?
            //int pageSize = 2000;
            int userId = 0;
            //int pTotalRows = 0;
            if ( string.IsNullOrWhiteSpace( ratingCodedNotation ) )
            {
                //don't want to return all, 
            }
            var template = string.Format( "'{0}'", ratingCodedNotation.Trim() );
            if ( includingAllSailorsTasks )
            {
                template += ",'ALL'";
            }
            //22-03-29 mp - change to handle multiple training task per rating task.
            //              - technically will still work as the view will have duplicate rows where there are multiple training tasks
            filter = string.Format( "base.id in (select a.[RatingTaskId] from [RatingTask.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation in ({0}) and (len([TrainingTask]) > 0 ))", template );
            var results = RatingTaskManager.RMTLSearch( filter, orderBy, pageNumber, pageSize, userId, ref totalRows );

            //no clear difference between the convert and select?
            list = results.ConvertAll( m => new AppEntity()
            {
                CTID = m.CTID,
                RowId = m.RowId,
                //HasRating = m.HasRatings,      //not available-adding
                Description = m.TrainingTask,
                CourseName = m.CourseName,
                CourseCodedNotation = m.CIN,

            } ).ToList();

            AddToCache( list, ratingCodedNotation, includingAllSailorsTasks );


            return list;
        }
        public static List<AppEntity> CheckCache( string rating, bool includingAllSailorsTasks )
        {
            var cache = new CachedTrainingTask();
            var list = new List<AppEntity>();
            var key = cacheKey + String.Format( "_{0}_{1}", rating, includingAllSailorsTasks );
            int cacheHours = 8;
            DateTime maxTime = DateTime.Now.AddHours( cacheHours * -1 );
            if ( MemoryCache.Default.Get( key ) != null && cacheHours > 0 )
            {
                cache = ( CachedTrainingTask ) MemoryCache.Default.Get( key );
                try
                {
                    if ( cache.LastUpdated > maxTime )
                    {
                        LoggingHelper.DoTrace( 6, string.Format( thisClassName + ".CheckCache. Using cached version." ) );
                        list = cache.Objects;
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
        public static void AddToCache( List<AppEntity> input, string rating, bool includingAllSailorsTasks )
        {
            var key = cacheKey + String.Format( "_{0}_{1}", rating, includingAllSailorsTasks );

            int cacheHours = 8;
            //add to cache
            if ( key.Length > 0 && cacheHours > 0 )
            {
                var newCache = new CachedTrainingTask()
                {
                    Objects = input,
                    LastUpdated = DateTime.Now
                };
                if ( MemoryCache.Default.Get( key ) != null )
                {
                    MemoryCache.Default.Remove( key );
                }
                //
                MemoryCache.Default.Add( key, newCache, new DateTimeOffset( DateTime.Now.AddHours( cacheHours ) ) );
                LoggingHelper.DoTrace( 5, thisClassName + ".AddToCache $$$ Updating cached version " );

            }
        }

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
		{
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.TrainingTask.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Description.Contains( keywords )
					);
				}

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.Description, m => m.OrderBy( n => n.Description ) );

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
			output.HasReferenceResource = input.ReferenceResource?.RowId ?? Guid.Empty;

			return output;
        }

        #endregion
    }
    [Serializable]
    public class CachedTrainingTask
    {
        public CachedTrainingTask()
        {
            LastUpdated = DateTime.Now;
        }
        public DateTime LastUpdated { get; set; }
        public List<AppEntity> Objects { get; set; }

    }
}
