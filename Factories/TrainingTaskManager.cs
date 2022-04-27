using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Application;
using Models.Curation;

using ParentEntity = Models.Schema.Course;
using AppEntity = Models.Schema.TrainingTask;
using DBEntity = Data.Tables.Course_Task;

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
                //
                //this note came while working with the created list (although this course did already exist?)
            }
        }
        public void SaveList( string courseCodedNotation, List<AppEntity> inputList, ref ChangeSummary status )
        {
            //get parent
            ParentEntity parent = new ParentEntity();
            if ( !string.IsNullOrWhiteSpace( courseCodedNotation ) )
            {
                parent = CourseManager.GetByCodedNotation( courseCodedNotation );
                if (parent?.Id == 0)
                {
                    status.AddError( thisClassName + String.Format( ".SaveList(courseCodedNotation, inputList). Error - A course was not found for the provided CIN: {0}. ", courseCodedNotation ) );
                    return;
                }
            }
            else
            {
                //error. Or fall thru and see if present in the list
                status.AddError( thisClassName + String.Format( ".SaveList(courseCodedNotation, inputList). Error - A course code was not provided for method to save a list of tasks. " ) );
                return;
            }

            if ( inputList != null )
            {
                foreach ( var item in inputList )
                {
                    Save( parent, item, ref status );
                }
            }

        }
        public bool Save(  AppEntity entity, ref ChangeSummary status )
        {
            //get parent
            ParentEntity parent = new ParentEntity();
            //will we have the guid?
            if ( IsValidGuid( entity.Course ) )
            {
                parent = CourseManager.Get( entity.Course );
            } else if ( !string.IsNullOrWhiteSpace( entity.CourseCodedNotation ) )
            {
                parent = CourseManager.GetByCodedNotation( entity.CourseCodedNotation );
            }

            return Save( parent, entity, ref status );
        }

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
                        efEntity = context.Course_Task.FirstOrDefault( s => s.RowId == input.RowId );
                    }

                    if ( efEntity == null )
                    {
                        efEntity = context.Course_Task
                            .FirstOrDefault( s => s.CourseId == parent.Id && s.Description.ToLower() == input.Description.ToLower() );
                    }
                    if ( efEntity?.Id > 0 )
                    {
                        //update
                        List<string> errors = new List<string>();
                        //this will include the extra props (like LifeCycleControlDocument, etc. for now)
                        //fill in fields that may not be in entity
                        input.RowId = efEntity.RowId;
                        input.CourseId = efEntity.CourseId;
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

                        TrainingTaskAssessmentMethodSave( input, ref status );
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
            var efEntity = new Data.Tables.Course_Task();
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

                    efEntity.CourseId = parent.Id;
                    efEntity.Description = input.Description;
                    efEntity.CreatedById = efEntity.LastUpdatedById = parent.LastUpdatedById;
                    efEntity.Created = efEntity.LastUpdated = DateTime.Now;

                    context.Course_Task.Add( efEntity );

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
                        TrainingTaskAssessmentMethodSave( input, ref status );

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

        /// <summary>
        /// the asmt methods will be passed in the trainingTask now
        /// </summary>
        /// <param name="input"></param>
        /// <param name="concepts"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool TrainingTaskAssessmentMethodSave( AppEntity input, ref ChangeSummary status )
        {
            bool success = false;
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.CourseTask_AssessmentType();
            var entityType = "CourseTask_AssessmentType";

            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( input.AssessmentMethodType?.Count == 0 )
                        input.AssessmentMethodType = new List<Guid>();
                    //check existance
                    var results =   from entity in context.CourseTask_AssessmentType
                                    join concept in context.ConceptScheme_Concept
                                    on entity.AssessmentMethodConceptId equals concept.Id
                                    where entity.CourseTaskId == input.Id

                                    select concept;

                    //if ( existing == null )
                    //    existing = new List<ConceptScheme_Concept>();  
                    var existing = results?.ToList();

                    #region deletes check
                    if ( existing.Any() )
                    {
                        //if exists not in input, delete it
                        foreach ( var e in existing )
                        {
                            var key = e.RowId;
                            if ( IsValidGuid( key ) )
                            {
                                if ( !input.AssessmentMethodType.Contains( ( Guid ) key ) )
                                {
                                    status.AddWarning( String.Format("The current training task: '{0}', (course: {1}) didn't include an assessment method that was previously saved: '{2}'. Deletes are currently suspended to prevent incorrect deletes. ", FormatLongLabel( input.Description ), input.CourseCodedNotation,  e.Name ));

                                    //a training task could be on multiple rows or rmtls, the asmt types may not be consistent
                                    //DeleteTrainingAssessmentType( input.Id, e.Id, ref status );
                                }
                            }
                        }
                    }
                    #endregion
                    //adds
                    if ( input.AssessmentMethodType != null )
                    {
                        foreach ( var child in input.AssessmentMethodType )
                        {
                            //if not in existing, then add
                            bool doingAdd = true;
                            if ( existing?.Count > 0 )
                            {
                                foreach ( var item in existing )
                                {
                                    if ( item.RowId == child )
                                    {
                                        doingAdd = false;
                                        break;
                                    }
                                }
                            }
                            if ( doingAdd )
                            {
                                var concept = ConceptSchemeManager.GetConcept( child );
                                if ( concept?.Id > 0 )
                                {
                                    efEntity.CourseTaskId = input.Id;
                                    efEntity.AssessmentMethodConceptId = concept.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.CourseTask_AssessmentType.Add( efEntity );

                                    int count = context.SaveChanges();
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For TrainingTask: '{0}' ({1}) an AssessmentMethod entity was not found for Identifier: {2}", FormatLongLabel( input.Description ), input.Id, child ) );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".TrainingTaskAssessmentMethodSave failed, Course: '{0}' ({1})", entityType, FormatLongLabel( input.Description ), input.Id ) );
                    status.AddError( thisClassName + ".TrainingTaskAssessmentMethodSave(). Error - the save was not successful. \r\n" + message );
                }
            }
            return success;
        }
        public bool TrainingTaskAssessmentMethodSave( AppEntity input, List<Guid> concepts, ref ChangeSummary status )
        {
            bool success = false;
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.CourseTask_AssessmentType();
            var entityType = "CourseTask_AssessmentType";

            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( input.AssessmentMethodType?.Count == 0 )
                        input.AssessmentMethodType = new List<Guid>();
                    //check existance
                    var results = from entity in context.CourseTask_AssessmentType
                                  join concept in context.ConceptScheme_Concept
                                  on entity.AssessmentMethodConceptId equals concept.Id
                                  where entity.CourseTaskId == input.Id

                                  select concept;

                    //if ( existing == null )
                    //    existing = new List<ConceptScheme_Concept>();  
                    var existing = results?.ToList();

                    #region deletes check
                    if ( existing.Any() )
                    {
                        //if exists not in input, delete it
                        foreach ( var e in existing )
                        {
                            var key = e.RowId;
                            if ( IsValidGuid( key ) )
                            {
                                if ( !input.AssessmentMethodType.Contains( ( Guid ) key ) )
                                {
                                    DeleteTrainingAssessmentType( input.Id, e.Id, ref status );
                                }
                            }
                        }
                    }
                    #endregion
                    //adds
                    if ( input.AssessmentMethodType != null )
                    {
                        foreach ( var child in input.AssessmentMethodType )
                        {
                            //if not in existing, then add
                            bool doingAdd = true;
                            if ( existing?.Count > 0 )
                            {
                                foreach ( var item in existing )
                                {
                                    if ( item.RowId == child )
                                    {
                                        doingAdd = false;
                                        break;
                                    }
                                }
                            }
                            if ( doingAdd )
                            {
                                var concept = ConceptSchemeManager.GetConcept( child );
                                if ( concept?.Id > 0 )
                                {
                                    efEntity.CourseTaskId = input.Id;
                                    efEntity.AssessmentMethodConceptId = concept.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.CourseTask_AssessmentType.Add( efEntity );

                                    int count = context.SaveChanges();
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For TrainingTask: '{0}' ({1}) an AssessmentMethod entity was not found for Identifier: {2}", FormatLongLabel( input.Description ), input.Id, child ) );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".TrainingTaskAssessmentMethodSave failed, Course: '{0}' ({1})", entityType, FormatLongLabel( input.Description ), input.Id ) );
                    status.AddError( thisClassName + ".TrainingTaskAssessmentMethodSave(). Error - the save was not successful. \r\n" + message );
                }
            }
            return success;
        }

        public bool DeleteTrainingAssessmentType( int courseId, int conceptId, ref ChangeSummary status )
        {
            bool isValid = false;
            if ( conceptId == 0 )
            {
                //statusMessage = "Error - missing an identifier for the CourseConcept to remove";
                return false;
            }

            using ( var context = new DataEntities() )
            {
                var efEntity = context.CourseTask_AssessmentType
                                .FirstOrDefault( s => s.CourseTaskId == courseId && s.AssessmentMethodConceptId == conceptId );

                if ( efEntity != null && efEntity.Id > 0 )
                {
                    context.CourseTask_AssessmentType.Remove( efEntity );
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                    }
                }
                else
                {
                    //statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
                    isValid = true;
                }
            }

            return isValid;
        }

        #endregion


        #region Retrieval

		public static AppEntity Get( string description )
		{
			var entity = new AppEntity();

			using ( var context = new DataEntities() )
			{
				var item = context.Course_Task
							.FirstOrDefault( s => s.Description.ToLower() == description.ToLower() );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity );
				}
			}
			return entity;
		}
        /// <summary>
        /// Get a task that matches the course code and the task description
        /// </summary>
        /// <param name="courseCodedNotation"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static AppEntity Get( string courseCodedNotation, string description )
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                var item = context.Course_Task
                            .FirstOrDefault( s => s.Course.CodedNotation.ToLower() == courseCodedNotation.ToLower()
                            && s.Description.ToLower() == description.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                } else
                {
                    //attempt partial match? - no clear dependable approach
                }
            }
            return entity;
        }
        /// <summary>
        /// Get a training task that is associated with the current RMTL rowCodedNotation.
        /// this approach will properly handle updates. But what if meant to be a different task?
        /// Could include description and do a fuzzy compare to the returned one. 
        /// Or, check if there more than 
        /// </summary>
        /// <param name="ratingTaskCodedNotation">In the future where this has a rating prefix, it will be more reliable.</param>
        /// <param name="courseCodedNotation">May not be necessary. Could use to compare to the course code for the returned task.</param>
        /// <param name="description">Use description for compares</param>
        /// <returns></returns>
        public static AppEntity GetTrainingTaskForRatingTask( string ratingCode, string ratingTaskCodedNotation, string courseCodedNotation, string description, ref ChangeSummary summary )
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                //first just get existing RT->TT for current rating
                var results = from task in context.Course_Task
                              join hasTrainingTask in context.RatingTask_HasTrainingTask
                                    on task.Id equals hasTrainingTask.TrainingTaskId
                              //get ratingTask
                              join ratingTask in context.RatingTask
                                    on hasTrainingTask.RatingTaskId equals ratingTask.Id
                              join hasRating in context.RatingTask_HasRating
                                    on ratingTask.Id equals hasRating.RatingTaskId
                              join rating in context.Rating
                                    on hasRating.RatingId equals rating.Id
                              //Want to training task related to a particular ratingTask row!
                              //currently codes like NEC001 can be on multiple sheets
                              where ratingTask.CodedNotation.ToLower() == ratingTaskCodedNotation.ToLower()
                              && rating.CodedNotation.ToLower().Equals( ratingCode.ToLower() )

                              select task;
                //should only be one, but just in case
                var existing = results?.ToList().FirstOrDefault();
                if ( existing?.Id > 0 )
                {
                    if (existing.Course.CodedNotation.ToLower() != courseCodedNotation.ToLower() )
                    {
                        //if there is a change to a different course/training task, then probably should return essentially not found
                        //  also want to have a warning that the course changed for rating task!
                        summary.AddWarning( string.Format( "For RatingTask Identifier: '{0}' there is a change in Course. Previous CIN: '{1}', Current CIN: '{2}'", ratingTaskCodedNotation, existing.Course.CodedNotation, courseCodedNotation ) );
                        return entity;
                    } else
                    {
                        //same course, so return training task
                        MapFromDB( existing, entity );
                    }
                } else
                {
                    //otherwise not associated with a rating tasks, so look up for existing course training task
                    entity = Get( courseCodedNotation, description );
                }
            }
            return entity;
        }
        public static AppEntity Get( Guid rowId)
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                var item = context.Course_Task
                            .FirstOrDefault( s => s.RowId == rowId );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }
            return entity;
        }
        public static AppEntity Get( int id)
        {
            var entity = new AppEntity();
            if ( id < 1 ) //Wouldn't this always be true?
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.Course_Task
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }

            return entity;
        }
		//Not sure if there's a better way to get this?
		//public static AppEntity GetForRatingTask( int ratingTaskId )
		//{
		//	var entity = new AppEntity();

		//	using ( var context = new DataEntities() )
		//	{
		//		var item = context.Course_Task
		//					.FirstOrDefault( s => s.RatingTask.Where( m => m.Id == ratingTaskId ).Count() > 0 );

		//		if ( item != null && item.Id > 0 )
		//		{
		//			MapFromDB( item, entity );
		//		}
		//	}
		//	return entity;
		//}
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
                var results = context.Course_Task
                        .OrderBy( s => s.Description )
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
            return list;
        }

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

		public static List<AppEntity> Search( SearchQuery query )
		{
			var output = new List<AppEntity>();
			var skip = ( query.PageNumber - 1 ) * query.PageSize;
			try
			{
				using ( var context = new DataEntities() )
				{
					//Start query
					var list = context.Course_Task.AsQueryable();

					//Handle keywords filter
					var keywordsText = query.GetFilterTextByName( "search:Keyword" )?.ToLower();
					if ( !string.IsNullOrWhiteSpace( keywordsText ) )
					{
						list = list.Where( s =>
							 s.Description.ToLower().Contains( keywordsText )
						);
					}

					//Handle Course Connection
					var courseFilter = query.GetFilterByName( "ceterms:Course" );
                    if ( courseFilter != null && courseFilter.ItemIds?.Count() > 0 )
                    {
                        list = list.Where( s =>
                            courseFilter.IsNegation ?
                                !courseFilter.ItemIds.Contains( s.CourseId ) :
                                courseFilter.ItemIds.Contains( s.CourseId )
                        );
                    }
                    else
                    {
                        //if no keywords, include course name filter on keywords
                        if ( !string.IsNullOrWhiteSpace( keywordsText ) )
                        {
                            //not working?
                            list = list.Where( s =>
                             s.Course.Name.ToLower().Contains( keywordsText ) || s.Course.CodedNotation.ToLower().Contains( keywordsText )
                            );
                        }
                    }

					//Handle Rating Task Connection
					var ratingTaskFilter = query.GetFilterByName( "navy:RatingTask" );
                    if ( ratingTaskFilter != null && ratingTaskFilter.ItemIds?.Count() > 0 )
                    {
                        //not sure yet. get training task for a rating task
                        list = list.Where( s =>
                            s.RatingTask_HasTrainingTask.Where( t =>
                                ratingTaskFilter.IsNegation ?
                                    !ratingTaskFilter.ItemIds.Contains( t.RatingTaskId ) :
                                    ratingTaskFilter.ItemIds.Contains( t.RatingTaskId )
                            ).Count() > 0
                        );
                    }

                    //Get total
                    query.TotalResults = list.Count();

					//Sort
					list = list.OrderBy( p => p.Course.Name ).ThenBy( s => s.Description);

					//Get page and populate
					var results = list.Skip( skip ).Take( query.PageSize )
						.Where( m => m != null ).ToList();

					//Populate
					foreach ( var item in results )
					{
						var entity = new AppEntity();
						MapFromDB( item, entity, true );
						output.Add( entity );
					}
				}

				return output;
			}
			catch( Exception ex )
			{
				return new List<AppEntity>() { new AppEntity() { Description = "Error: " + ex.Message + " - " + ex.InnerException?.Message } };
			}
		}
		//

		/*
        public static List<AppEntity> Search( SearchQuery query )
        {
            var entity = new AppEntity();
            var output = new List<AppEntity>();
            var skip = 0;
            if ( query.PageNumber > 1 )
                skip = ( query.PageNumber - 1 ) * query.PageSize;
            var filter = GetSearchFilterText( query );

            try
            {
                using ( var context = new DataEntities() )
                {
                    var list = from Results in context.Course_Task
                               select Results;
                    if ( !string.IsNullOrWhiteSpace( filter ) )
                    {
                        list = from Results in list
                                .Where( s =>
                                ( s.Description.ToLower().Contains( filter.ToLower() ) ) 
                                )
                               select Results;
                    }
                    query.TotalResults = list.Count();
                    //sort order not handled
                    list = list.OrderBy( p => p.Description );

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

        public static void MapFromDB( DBEntity input, AppEntity output, bool appendingCourseCode = false )
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }
            //
            if (input.Course?.Id > 0 )
            {
                output.CourseName = output.CourseName = input.Course.Name;
                output.CourseCodedNotation = input.Course.CodedNotation;
                if ( appendingCourseCode )
                {
                    output.Description += " (" + output.CourseCodedNotation + ")";
                    //add fake name for search
                    output.Name = string.Format("Course: {0} ({1}) ", output.CourseName, output.CourseCodedNotation);
                }
            }

            if ( input.CourseTask_AssessmentType != null )
            {
                foreach ( var item in input.CourseTask_AssessmentType )
                {
                    if ( item != null && item.ConceptScheme_Concept != null )
                    {
                        output.AssessmentMethodType.Add( item.ConceptScheme_Concept.RowId );
                        output.AssessmentMethods.Add( item.ConceptScheme_Concept.Name );
                    }
                }
            }
        }

        public static AppEntity MapFromDB( DBEntity input, bool appendingCourseCode = false )
        {
            //should include list of concepts
            AppEntity output = new AppEntity();
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }
            //
            if ( input.Course?.Id > 0 )
            {
                output.CourseName = output.CourseName = input.Course.Name;
                output.CourseCodedNotation = input.Course.CodedNotation;
                if ( appendingCourseCode )
                {
                    output.Description += " (" + output.CourseCodedNotation + ")";
                    //add fake name for search
                    output.Name = string.Format( "Course: {0} ({1}) ", output.CourseName, output.CourseCodedNotation );
                }
            }

            if ( input.CourseTask_AssessmentType != null )
            {
                foreach ( var item in input.CourseTask_AssessmentType )
                {
                    if ( item != null && item.ConceptScheme_Concept != null )
                    {
                        output.AssessmentMethodType.Add( item.ConceptScheme_Concept.RowId );
                        output.AssessmentMethods.Add( item.ConceptScheme_Concept.Name );
                    }
                }
            }

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
