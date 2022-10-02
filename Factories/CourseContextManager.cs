using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Curation;
using AppEntity = Models.Schema.CourseContext;
using AppFullEntity = Models.Schema.PopulatedCourseContext;
using CourseContextTask = Models.Schema.TrainingTask;
using DBEntity = Data.Tables.CourseContext;
using MSc = Models.Schema;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Models.Application;
using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class CourseContextManager : BaseFactory
    {
        public static new string thisClassName = "CourseContextManager";
        #region CourseContext - persistance ==================
        /// <summary>
        /// Update a CourseContext
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool Save( AppEntity entity, ref ChangeSummary status )
        {
            bool isValid = true;
            int count = 0;
            entity.LastUpdatedById = entity.CreatedById;
            try
            {
                using ( var context = new DataEntities() )
                {
                    //if ( ValidateProfile( entity, ref status ) == false )
                    //    return false;
                    //look up if no id
                    if ( entity.Id == 0 )
                    {
                        var record = Get( entity.HasCourseId, entity.HasTrainingTaskId );
                        if ( record?.Id > 0 )
                        {
                            entity.Id = record.Id;
                            return true;
                        }
                        else
                        {
                            //add
                            int newId = Add( entity, ref status );
                            if ( newId == 0 || status.HasSectionErrors )
                                isValid = false;
                            //just in case 
                            if ( newId > 0 )
                                UpdateParts( entity, status );
                            return isValid;
                        }
                    }
                    //update
                    //TODO - consider if necessary, or interferes with anything
                    context.Configuration.LazyLoadingEnabled = false;
                    DBEntity efEntity = context.CourseContext
                            .SingleOrDefault( s => s.Id == entity.Id );

                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        //fill in fields that may not be in entity
                        entity.RowId = efEntity.RowId;
                        entity.Created = efEntity.Created;
                        entity.CreatedById = ( efEntity.CreatedById ?? 0 );
                        entity.Id = efEntity.Id;

                        MapToDB( entity, efEntity, status );
                        bool hasChanges = false;
                        if ( HasStateChanged( context ) )
                        {
                            hasChanges = true;
                            efEntity.LastUpdated = DateTime.Now;
                            efEntity.LastUpdatedById = entity.LastUpdatedById;
                            count = context.SaveChanges();
                            //can be zero if no data changed
                            if ( count >= 0 )
                            {
                                entity.LastUpdated = ( DateTime ) efEntity.LastUpdated;
                                isValid = true;
                                //if ( hasChanges )
                                //{  }
                                    SiteActivity sa = new SiteActivity()
                                    {
                                        ActivityType = "CourseContext",
                                        Activity = status.Action,
                                        Event = "Update",
                                        Comment = string.Format( "CourseContext was updated by '{0}'.", status.Action ),
                                        ActionByUserId = entity.LastUpdatedById,
                                        ActivityObjectId = entity.Id
                                    };
                                    new ActivityManager().SiteActivityAdd( sa );
                              
                            }
                            else
                            {
                                //?no info on error

                                isValid = false;
                                string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a CourseContext. The process appeared to not work, but was not an exception, so we have no message, or no clue. HasCourseId: {0}, HasTrainingTaskId: {1}", entity.HasCourseId, entity.HasTrainingTaskId );
                                status.AddError( "Error - the update was not successful. " + message );
                                EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                            }
                        }

                        if ( isValid )
                        {
                            //just in case 
                            if ( entity.Id > 0 )
                                UpdateParts( entity, status );     
                        }
                    }
                    else
                    {
                        status.AddError( thisClassName + " Error - update failed, as record was not found." );
                    }
                }
            }
            catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
            {
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. HasCourseId: {0}, HasTrainingTaskId: {1}", entity.HasCourseId, entity.HasTrainingTaskId ), "CourseContext" );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save.HasCourseId: {0}, HasTrainingTaskId: {1}", entity.HasCourseId, entity.HasTrainingTaskId ), true );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
                isValid = false;
            }


            return isValid;
        }

        /// <summary>
        /// add a CourseContext
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
                    MapToDB( entity, efEntity, status );

                    if ( IsValidGuid( entity.RowId ) )
                        efEntity.RowId = entity.RowId;
                    else
                        efEntity.RowId = Guid.NewGuid();

                    entity.Created = efEntity.Created = DateTime.Now;
                    efEntity.CreatedById = efEntity.LastUpdatedById = entity.LastUpdatedById;
                    entity.LastUpdated = efEntity.LastUpdated = DateTime.Now;

                    context.CourseContext.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        entity.RowId = efEntity.RowId;
                        entity.Id = efEntity.Id;
                        //
                        //add log entry
                        //SiteActivity sa = new SiteActivity()
                        //{
                        //    ActivityType = "CourseContext",
                        //    Activity = status.Action,
                        //    Event = "Add",
                        //    Comment = string.Format( "CourseContext: '{0} was added.", entity.Name ),
                        //    ActivityObjectId = entity.Id,
                        //    ActionByUserId = entity.LastUpdatedById
                        //};
                        //new ActivityManager().SiteActivityAdd( sa );
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a CourseContext. The process appeared to not work, but was not an exception, so we have no message, or no clue.  HasCourseId: {0}, HasTrainingTaskId: {1}", entity.HasCourseId, entity.HasTrainingTaskId );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "CourseContext" );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. " + message );

                    LoggingHelper.LogError( message, true );
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), HasCourseId: {0}, HasTrainingTaskId: {1}", entity.HasCourseId, entity.HasTrainingTaskId ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }

            return efEntity.Id;
        }
        public static void MapToDB( AppEntity input, DBEntity output, ChangeSummary status )
        {
            status.HasSectionErrors = false;
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            //NOTE - NEED TO CHG FOR USE OF Has....
            BaseFactory.AutoMap( input, output, errors );
           

		}
        public static void MapToDB( AppFullEntity input, DBEntity output )
        {
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            //NOTE - NEED TO CHG FOR USE OF Has....
            BaseFactory.AutoMap( input, output, errors );
            
        }
        #endregion
        #region Retrieval
        //unlikely?
        public static AppEntity Get( int courseId, int trainingTaskId )
        {
            var entity = new AppEntity();
            using ( var context = new DataEntities() )
            {
                var item = context.CourseContext
                            .FirstOrDefault( s => s.HasCourseId ==  courseId && s.HasTrainingTaskId == trainingTaskId);

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
                var item = context.CourseContext
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
                var item = context.CourseContext
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }

            return entity;
		}
        public static void MapFromDB( DBEntity input, AppEntity output )
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }
            if ( input.CourseContext_AssessmentType != null )
            {
                foreach ( var item in input.CourseContext_AssessmentType )
                {
                    if ( item != null && item.ConceptScheme_Concept != null )
                    {
                        output.AssessmentMethodTypes.Add( item.ConceptScheme_Concept.RowId );
                    //    output.AssessmentMethods.Add( item.ConceptScheme_Concept.Name );
                    }
                }
            }
            //
            if ( input.Course != null)
            {
               //??
            }
            //
            if ( input.TrainingTask != null )
            {
                //??
            }

        }

        #endregion

        /// <summary>
        /// Update:
        /// - training task
        /// - CourseContext types
        /// - CurrentAssessmentApproach
        /// </summary>
        /// <param name="input"></param>
        /// <param name="status"></param>
        public void UpdateParts( AppEntity input, ChangeSummary status )
        {
            try
            {
                //AssessmentMethod is passed as well
                //CourseContextAssessmentMethodSave( input, ref status )

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }

        #region CourseContext concepts
        /*        */
        public bool CourseContextAssessmentMethodSave( AppEntity entity, List<Guid> concepts, ref ChangeSummary status )
        {
            bool success = false;
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.CourseContext_AssessmentType();
            var entityType = "CourseContext_AssessmentType";

            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( concepts?.Count == 0 )
                        concepts = new List<Guid>();
                    //check existance
                    var existing = context.CourseContext_AssessmentType
                        .Where( s => s.CourseContextId == entity.Id )
                        .ToList();
                    if ( existing == null )
                        existing = new List<CourseContext_AssessmentType>();

                    #region deletes check
                    if ( existing.Any() )
                    {
                        //if exists not in input, delete it
                        foreach ( var e in existing )
                        {
                            var key = e?.ConceptScheme_Concept.RowId;
                            if ( IsValidGuid( key ) )
                            {
                                if ( !concepts.Contains( ( Guid ) key ) )
                                {
                                    context.CourseContext_AssessmentType.Remove( e );
                                    int dcount = context.SaveChanges();
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
                                var isfound = existing.Select( s => s.ConceptScheme_Concept.RowId == child ).ToList();
                                if ( !isfound.Any() )
                                    doingAdd = false;
                            }
                            if ( doingAdd )
                            {
                                var concept = ConceptSchemeManager.GetConcept( child );
                                if ( concept?.Id > 0 )
                                {
                                    efEntity.CourseContextId = entity.Id;
                                    efEntity.AssessmentMethodConceptId = concept.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = entity.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.CourseContext_AssessmentType.Add( efEntity );

                                    int count = context.SaveChanges();
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For HasCourseId: {0}, HasTrainingTaskId: {1}) a CourseContext AssessmentMethod concept was not found for Identifier: {2}", entity.HasCourseId, entity.HasTrainingTaskId, child ) );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".CourseContextAssessmentMethodSave failed,  HasCourseId: {0}, HasTrainingTaskId: {1}", entity.HasCourseId, entity.HasTrainingTaskId ) );
                    status.AddError( thisClassName + ".CourseContextAssessmentMethodSave(). Error - the save was not successful. \r\n" + message );
                }
            }
            return success;
        }


        #endregion

    }

}
