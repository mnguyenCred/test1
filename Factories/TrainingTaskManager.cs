﻿using System;
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
        public void SaveList( ParentEntity input, bool doingDeletes, ref ChangeSummary status )
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

        public bool Save(  AppEntity entity, ref ChangeSummary status )
        {
            //get parent
            //will we have the guid?
            ParentEntity parent = CourseManager.Get( entity.Course );

            return Save( parent, entity, ref status );
        }

        public bool Save( ParentEntity parent, AppEntity input, ref ChangeSummary status )
        {
            bool isValid = true;
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
                    input.Created = efEntity.Created;
                    input.CreatedById = ( efEntity.CreatedById ?? 0 );
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
                            //TBD: will AssessmentMethodType still be on the course, or on the training task.
                            TrainingTaskAssessmentMethodSave( input, input.AssessmentMethodType, ref status );

                        }
                        else
                        {
                            //?no info on error

                            string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a CourseTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, Id: {1}, Task: '{2}'", parent.Name, parent.Id, input.Description );
                            status.AddError( "Error - the update was not successful. " + message );
                            EmailManager.NotifyAdmin( thisClassName + ".CourseTaskSave Failed Failed", message );
                        }

                    }
                }
                else
                {
                    var result = Add( parent, input, ref status );
                }

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
                        TrainingTaskAssessmentMethodSave( input, parent.AssessmentMethodType, ref status );

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
                    if ( concepts?.Count == 0 )
                        concepts = new List<Guid>();
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
                                if ( !concepts.Contains( ( Guid ) key ) )
                                {
                                    DeleteTrainingAssessmentType( input.Id, e.Id, ref status );
                                }
                            }
                        }
                    }
                    #endregion
                    //adds
                    if ( concepts != null )
                    {
                        foreach ( var child in concepts )
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

					//Handle Rating Task Connection
					var ratingTaskFilter = query.GetFilterByName( "navy:RatingTask" );
					//if( ratingTaskFilter != null && ratingTaskFilter.ItemIds?.Count() > 0 )
					//{
					//	list = list.Where( s =>
					//		s.RatingTask.Where( t =>
					//			ratingTaskFilter.IsNegation ?
					//				!ratingTaskFilter.ItemIds.Contains( t.Id ) :
					//				ratingTaskFilter.ItemIds.Contains( t.Id )
					//		).Count() > 0
					//	);
					//}

					//Get total
					query.TotalResults = list.Count();

					//Sort
					list = list.OrderBy( p => p.Description );

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
                output.CourseName = input.Course.Name;
                output.CourseCodedNotation = input.Course.CodedNotation;
                if ( appendingCourseCode )
                {
                    output.Description += " (" + output.CourseCodedNotation + ")";
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
