using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppEntity = Models.Schema.Course;
using AppFullEntity = Models.Schema.CourseFull;
using CourseTask = Models.Schema.TrainingTask;
using DBEntity = Data.Tables.Course;
using MSc = Models.Schema;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Models.Application;
using Navy.Utilities;

namespace Factories
{
    public class CourseManager : BaseFactory
    {
        public static string thisClassName = "CourseManager";

        #region Course - persistance ==================
        /// <summary>
        /// Update a Course
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool Save( AppEntity entity, ref SaveStatus status )
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
                        var record = GetByCIN( entity.CodedNotation );
                        if ( record?.Id > 0 )
                        {
                            entity.Id = record.Id;
                            //could be other updates, fall thru to the update
                            //return true;
                        }
                        else
                        {
                            //add
                            int newId = Add( entity, ref status );
                            if ( newId == 0 || status.HasErrors )
                                isValid = false;
                            //just in case 
                            if ( newId > 0 )
                                UpdateParts( entity, status );
                            return isValid;
                        }
                    }
                    //update
                    //TODO - consider if necessary, or interferes with anything
                    //      - don't really want to include all training tasks
                    context.Configuration.LazyLoadingEnabled = false;
                    DBEntity efEntity = context.Course
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
                                string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a Course. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, Id: {1}", entity.Name, entity.Id );
                                status.AddError( "Error - the update was not successful. " + message );
                                EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                            }

                        }

                        if ( isValid )
                        {
                            //just in case 
                            if ( entity.Id > 0 )
                                UpdateParts( entity, status );

                            SiteActivity sa = new SiteActivity()
                            {
                                ActivityType = "Course",
                                Activity = "Import",
                                Event = "Update",
                                Comment = string.Format( "Course was updated by the import. Name: {0}", entity.Name ),
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
            catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
            {
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), "Course" );
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
        /// add a Course
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private int Add( AppEntity entity, ref SaveStatus status )
        {
            DBEntity efEntity = new DBEntity();
            using ( var context = new DataEntities() )
            {
                try
                {
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

                    context.Course.Add( efEntity );

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
                            ActivityType = "Course",
                            Activity = "Import",
                            Event = "Add",
                            Comment = string.Format( "Full Course was added by the import. Name: {0}", entity.Name ),
                            ActivityObjectId = entity.Id
                        };
                        new ActivityManager().SiteActivityAdd( sa );


                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a Course. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, ctid: {1}", entity.Name, entity.CTID );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Course" );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. " + message );

                    LoggingHelper.LogError( message, true );
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}, CTID: {1}", efEntity.Name, efEntity.CTID ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }

            return efEntity.Id;
        }
        public static void MapToDB( AppEntity input, DBEntity output )
        {
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            //this will include the extra props (like LifeCycleControlDocument, etc. for now)
            BaseFactory.AutoMap( input, output, errors );
            //
            if ( input.HasReferenceResource != null ) 
            {
                //
                output.LifeCycleControlDocumentId = ConceptSchemeManager.GetConcept( input.HasReferenceResource)?.Id;
            }
            //

            if ( input.CurriculumControlAuthority?.Count > 0 ) 
            {
                foreach( var item in input.CurriculumControlAuthority )
                {
                    //all org adds will occur before here
                    var org = OrganizationManager.Get( item );
                    if ( org != null && org.Id > 0 )
                    {
                        //TODO - now using Course.Organization
                        output.CurriculumControlAuthorityId = org.Id;
                    }
                    else
                    {
                        //should not have a new org here

                        //SaveStatus status = new SaveStatus();
                        //org = new MSc.Organization()
                        //{
                        //    Name = input.Curriculum_Control_Authority,
                        //    AlternateName = input.Curriculum_Control_Authority
                        //};
                        //var isValid = new OrganizationManager().Save( org, ref status );
                        //if ( isValid )
                        //{
                        //    output.CurriculumControlAuthorityId = org.Id;
                        //}
                    }
                }

            }
        }
        public static void MapToDB( AppFullEntity input, DBEntity output )
        {
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            //this will include the extra props (like LifeCycleControlDocument, etc. for now)
            BaseFactory.AutoMap( input, output, errors );
            //
            if ( !string.IsNullOrWhiteSpace( input.LifeCycleControlDocument ) )
            {               
                //can this be a concept scheme??
                output.LifeCycleControlDocumentId = ConceptSchemeManager.GetConcept( input.LifeCycleControlDocument, ConceptSchemeManager.ConceptScheme_LifeCycleControlDocument )?.Id;
            }
            //

            if ( !string.IsNullOrWhiteSpace( input.Curriculum_Control_Authority ) )
            {
                var org = OrganizationManager.Get( input.Curriculum_Control_Authority , true);
                if ( org != null && org.Id > 0 )
                {
                    output.CurriculumControlAuthorityId = org.Id;
                } else
                {
                    SaveStatus status = new SaveStatus();
                    org = new MSc.Organization()
                    {
                        Name = input.Curriculum_Control_Authority,
                        AlternateName = input.Curriculum_Control_Authority
                    };
                    var isValid = new OrganizationManager().Save( org, input.LastUpdatedById, ref status );
                    if ( isValid )
                    { 
                        output.CurriculumControlAuthorityId = org.Id;
                    }
                }
            }
        }
        #endregion
        #region Retrieval
        //unlikely?
        public static AppEntity Get( string name, bool includingTrainingTasks = false )
        {
            var entity = new AppEntity();
            if ( string.IsNullOrWhiteSpace( name ) )
                return null;

            using ( var context = new DataEntities() )
            {
                var item = context.Course
                            .FirstOrDefault( s => s.Name.ToLower() == name.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingTrainingTasks );
                }
            }
            return entity;
        }
        public static AppEntity GetByCIN( string CIN, bool includingTrainingTasks = false )
        {
            var entity = new AppEntity();
            if ( string.IsNullOrWhiteSpace( CIN ) )
                return null;

            using ( var context = new DataEntities() )
            {
                var item = context.Course
                            .FirstOrDefault( s => s.CIN.ToLower() == CIN.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingTrainingTasks );
                }
            }
            return entity;
        }
        public static AppEntity Get( Guid rowId, bool includingTrainingTasks = false )
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                var item = context.Course
                            .FirstOrDefault( s => s.RowId == rowId );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingTrainingTasks );
                }
            }
            return entity;
        }
        public static AppEntity Get( int id, bool includingTrainingTasks = false )
        {
            var entity = new AppEntity();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.Course
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingTrainingTasks );
                }
            }

            return entity;
        }
        /// <summary>
        /// Get all 
        /// May need a get all for a rating? Should not matter as this is external data?
        /// </summary>
        /// <returns></returns>
        public static List<AppEntity> GetAll( bool includingTrainingTasks = false )
        {
            var entity = new AppEntity();
            var list = new List<AppEntity>();

            using ( var context = new DataEntities() )
            {
                var results = context.Course
                        .OrderBy( s => s.Name )
                        .ToList();
                if ( results?.Count > 0 )
                {
                    foreach ( var item in results )
                    {
                        if ( item != null && item.Id > 0 )
                        {
                            entity = new AppEntity();
                            MapFromDB( item, entity, includingTrainingTasks );
                            list.Add( ( entity ) );
                        }
                    }
                }

            }
            return list;
        }
        public static void MapFromDB( DBEntity input, AppEntity output, bool includingTrainingTasks = false )
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }
            //
            if (includingTrainingTasks && input?.Course_Task?.Count > 0)
            {
                foreach( var item in input.Course_Task)
                {

                }
            }
        }

        #endregion

        /// <summary>
        /// Update:
        /// - training task
        /// - course types
        /// - CurrentAssessmentApproach
        /// </summary>
        /// <param name="input"></param>
        /// <param name="status"></param>
        public void UpdateParts( AppEntity input, SaveStatus status )
        {
            try
            {
                if ( input.TrainingTasks != null)
                {

                }
                //this needs to be multiple
                if ( input.CourseType != null )
                {
                    //var parts = input.Course_Type.Split( '-' ).ToList();
                    //if ( parts.Count > 0 )
                    //{
                    //    foreach(var item in parts )
                    //    {
                    //        var concept = ConceptSchemeManager.GetConcept( item, ConceptSchemeManager.ConceptScheme_CourseType );
                    //        if( concept?.Id > 0)
                    //        {
                    //            if (AddCourseConcept( input, concept.Id, input.LastUpdatedById, ref status ))
                    //            {
                    //                //add log entry
                    //                SiteActivity sa = new SiteActivity()
                    //                {
                    //                    ActivityType = "Course",
                    //                    Activity = "Import",
                    //                    Event = "Add Course_Concept",
                    //                    Comment = string.Format( "CourseType of: '{0}' for conceptScheme: '{1}', was added for Course: '{2}' by the import.", item, ConceptSchemeManager.ConceptScheme_CourseType, input.Name ),
                    //                };
                    //                new ActivityManager().SiteActivityAdd( sa );
                    //            }
                    //        } else
                    //        {
                    //            //add new
                    //        }
                    //    }
                    //}
                    //
                }

                if ( input.AssessmentMethodType != null )
                {
                    //this needs to be multiple

                }

            } catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }

        public void UpdateParts( AppFullEntity input, SaveStatus status )
        {
            try
            {
                if ( input.TrainingTasks != null )
                {

                }
                if ( !string.IsNullOrWhiteSpace( input.Course_Type ) )
                {
                    var parts = input.Course_Type.Split( '-' ).ToList();
                    if ( parts.Count > 0 )
                    {
                        foreach ( var item in parts )
                        {
                            var concept = ConceptSchemeManager.GetConcept( item, ConceptSchemeManager.ConceptScheme_CourseType );
                            if ( concept?.Id > 0 )
                            {
                                if ( AddCourseConcept( input, concept.Id, input.LastUpdatedById, ref status ) )
                                {
                                    //add log entry
                                    SiteActivity sa = new SiteActivity()
                                    {
                                        ActivityType = "Course",
                                        Activity = "Import",
                                        Event = "Add Course_Concept",
                                        Comment = string.Format( "CourseType of: '{0}' for conceptScheme: '{1}', was added for Course: '{2}' by the import.", item, ConceptSchemeManager.ConceptScheme_CourseType, input.Name ),
                                    };
                                    new ActivityManager().SiteActivityAdd( sa );
                                }
                            }
                            else
                            {
                                //add new
                            }
                        }
                    }
                    //
                }

                if ( !string.IsNullOrWhiteSpace( input.CurrentAssessmentApproach ) )
                {
                    //output.AssessmentApproachId = ConceptSchemeManager.GetConcept( input.CurrentAssessmentApproach, ConceptSchemeManager.ConceptScheme_CurrentAssessmentApproach )?.Id;
                    var parts = input.CurrentAssessmentApproach.Split( '-' ).ToList();
                    if ( parts.Count > 0 )
                    {
                        foreach ( var item in parts )
                        {
                            var concept = ConceptSchemeManager.GetConcept( item, ConceptSchemeManager.ConceptScheme_CurrentAssessmentApproach );
                            if ( concept?.Id > 0 )
                            {
                                if ( AddCourseConcept( input, concept.Id, input.LastUpdatedById, ref status ) )
                                {
                                    //add log entry
                                    SiteActivity sa = new SiteActivity()
                                    {
                                        ActivityType = "Course",
                                        Activity = "Import",
                                        Event = "Add Course_Concept",
                                        Comment = string.Format( "AssessmentApproach of: '{0}' for conceptScheme: '{1}', was added for Course: '{2}' by the import.", item, ConceptSchemeManager.ConceptScheme_CurrentAssessmentApproach, input.Name ),
                                    };
                                    new ActivityManager().SiteActivityAdd( sa );
                                }
                            }
                            else
                            {
                                //add new
                            }
                        }
                    }
                    //
                }

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }

        #region TrainingTask

        #endregion


        #region Course-Related
        public bool AddCourseConcept( AppFullEntity input, int conceptId, int userId, ref SaveStatus status )
        {
            ConceptSchemeManager csMgr = new ConceptSchemeManager();
            var efEntity = new Data.Tables.Course_Concept();

            using ( var context = new DataEntities() )
            {
                try
                {
                    efEntity.CourseId = input.Id;
                    efEntity.ConceptId = conceptId;
                    efEntity.RowId = Guid.NewGuid();
                    efEntity.CreatedById = userId;
                    //efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                    efEntity.Created = DateTime.Now;

                    context.Course_Concept.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        //
                        return true;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a Course_Concept. The process appeared to not work, but was not an exception, so we have no message, or no clue. CourseId of: '{0}' for conceptId: {1}, was added for Course: '{2}' by the import.", input.Id, conceptId, input.Name );
                        status.AddError( thisClassName + ". Error - the add Course_Concept was not successful. " + message );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".AddCourseConcept(), CourseId of: '{0}' for conceptId: {1}, for Course: '{2}'.", input.Id, conceptId, input.Name ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }
            return false;
        }
        //public void AddCourseConcept( AppFullEntity input, int conceptSchemeId, string concept, ref SaveStatus status )
        //{
        //    ConceptSchemeManager csMgr = new ConceptSchemeManager();
        //    var efEntity = new Data.Tables.Course_Concept();
        //    int conceptId = 0;
        //    //lookup - need to check name or CodedNotation
        //    var c = ConceptSchemeManager.GetConcept( conceptSchemeId, concept );
        //    if ( c != null && c.Id > 0)
        //        conceptId = c.Id;
        //    else
        //    {
        //        //add
        //        status.HasSectionErrors = false;
        //        conceptId = csMgr.SaveConcept( conceptSchemeId, concept, ref status );
        //        if ( conceptId == 0 )
        //        {
        //            LoggingHelper.DoTrace( 1, thisClassName + String.Format( ".AddCourseConcept. Failed to add Course_Concept of: '{0}' for conceptSchemeId: {1}, was added for Course: '{2}' by the import.", concept, conceptSchemeId, input.Name ) );
        //            return;
        //        }
        //    }
        //    using ( var context = new DataEntities() )
        //    {
        //        try
        //        {
        //            efEntity.CourseId = input.Id;
        //            efEntity.ConceptId = conceptId;
        //            efEntity.RowId = Guid.NewGuid();
        //            //efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
        //            efEntity.Created = DateTime.Now;

        //            context.Course_Concept.Add( efEntity );

        //            // submit the change to database
        //            int count = context.SaveChanges();
        //            if ( count > 0 )
        //            {
        //                //
        //                //add log entry
        //                SiteActivity sa = new SiteActivity()
        //                {
        //                    ActivityType = "Course",
        //                    Activity = "Import",
        //                    Event = "Add Course_Concept",
        //                    Comment = string.Format( "Course_Concept of: '{0}' for conceptSchemeId: {1}, was added for Course: '{2}' by the import.", concept, conceptSchemeId, input.Name ),
        //                    ActivityObjectId = efEntity.Id
        //                };
        //                new ActivityManager().SiteActivityAdd( sa );
        //           }
        //            else
        //            {
        //                //?no info on error

        //                string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a Course_Concept. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course_Concept of: '{0}' for conceptSchemeId: {1}, was added for Course: '{2}' by the import.", concept, conceptSchemeId, input.Name );
        //                status.AddError( thisClassName + ". Error - the add Course_Concept was not successful. " + message );
        //                EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
        //            }
        //        }
        //        catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
        //        {
        //            string message = HandleDBValidationError( dbex, thisClassName + ".AddCourseConcept() ", "Course" );
        //            status.AddError( thisClassName + ".Add(). Error - the save was not successful. " + message );

        //            LoggingHelper.LogError( message, true );
        //        }
        //        catch ( Exception ex )
        //        {
        //            string message = FormatExceptions( ex );
        //            LoggingHelper.LogError( ex, thisClassName + string.Format( ".AddCourseConcept(), Course_Concept of: '{0}' for conceptSchemeId: {1}, was added for Course: '{2}' by the import.", concept, conceptSchemeId, input.Name ) );
        //            status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
        //        }
        //    }
        //}
        public bool AddCourseOrganization( AppEntity input, int orgId, int userId, ref SaveStatus status )
        {
            var efEntity = new Data.Tables.Course_Organization();

            using ( var context = new DataEntities() )
            {
                try
                {
                    efEntity.CourseId = input.Id;
                    efEntity.OrganizationId = orgId;
                    efEntity.RowId = Guid.NewGuid();
                    efEntity.CreatedById = userId;
                    //efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                    efEntity.Created = DateTime.Now;

                    context.Course_Organization.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        //
                        return true;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a Course_Concept. The process appeared to not work, but was not an exception, so we have no message, or no clue. CourseId of: '{0}' for orgId: {1}, was added for Course: '{2}' by the import.", input.Id, orgId, input.Name );
                        status.AddError( thisClassName + ". Error - the add Course_Concept was not successful. " + message );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".AddCourseConcept(), CourseId of: '{0}' for orgId: {1}, for Course: '{2}'.", input.Id, orgId, input.Name ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }
            return false;
        }
        #endregion

    }

}
