﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Curation;
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
        public static new string thisClassName = "CourseManager";
        //public List<MSc.TrainingTask> AllNewtrainingTasks = new List<MSc.TrainingTask>();
        //public List<MSc.TrainingTask> AllUpdatedtrainingTasks = new List<MSc.TrainingTask>();
        #region Course - persistance ==================
        /// <summary>
        /// Update a Course
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool Save( AppEntity entity, ref ChangeSummary status )
        {
            bool isValid = true;
            int count = 0;
            //if (allNewtrainingTasks != null)
            //{
            //    AllNewtrainingTasks = allNewtrainingTasks;
            //}
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
                        var record = GetByCodedNotation( entity.CodedNotation );
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

                        MapToDB( entity, efEntity, status );

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
                        status.AddError( thisClassName + " Error - update failed, as record was not found." );
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
                    if ( IsValidCtid( entity.CTID ) )
                        efEntity.CTID = entity.CTID;
                    else
                        efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                    entity.Created = efEntity.Created = DateTime.Now;
                    efEntity.CreatedById = efEntity.LastUpdatedById = entity.LastUpdatedById;
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
                            Activity = "Import",//in the future there could be a different process.
                            Event = "Add",
                            Comment = string.Format( "Course: '{0} was added.", entity.Name ),
                            ActivityObjectId = entity.Id,
                            ActionByUserId = entity.LastUpdatedById
                        };
                        new ActivityManager().SiteActivityAdd( sa );
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a Course. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, CodedNotation: {1}", entity.Name, entity.CodedNotation );
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
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}, CodedNotation: {1}", entity.Name, entity.CodedNotation ) );
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
            //this will include the extra props (like LifeCycleControlDocument, etc. for now)
            BaseFactory.AutoMap( input, output, errors );
            //
            if ( IsValidGuid( input.HasReferenceResource ) )
            {
                if ( output.LifeCycleControlDocumentId != null && output.ReferenceResource?.RowId == input.HasReferenceResource )
                {
                    //no action
                }
                else
                {
                    var entity = ReferenceResourceManager.Get( input.HasReferenceResource );
                    if ( entity?.Id > 0 )
                        output.LifeCycleControlDocumentId = ( int ) entity?.Id;
                    else
                    {
                        status.AddError( thisClassName + String.Format( ".MapToDB. Course: '{0}'. The related HasReferenceResource '{1}' was not found", input.Name, input.HasReferenceResource ) );
                    }
                }
            } else
            {
                output.LifeCycleControlDocumentId = null;
            }
            //
            //this may be removed if there can be multiple CCA
            //if ( input.CurriculumControlAuthority?.Count > 0 )
            //{
            //    foreach ( var item in input.CurriculumControlAuthority )
            //    {
            //        //all org adds will occur before here
            //        var org = OrganizationManager.Get( item );
            //        if ( org != null && org.Id > 0 )
            //        {
            //            //TODO - now using Course.Organization
            //            output.CurriculumControlAuthorityId = org.Id;
            //            //only can handle one here
            //        }
            //        else
            //        {
            //            //should not have a new org here
            //            //NO, all new orgs will have been added first, so this would be an error
            //        }
            //    }

            //}
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

            //if ( !string.IsNullOrWhiteSpace( input.Curriculum_Control_Authority ) )
            //{
            //    var org = OrganizationManager.Get( input.Curriculum_Control_Authority, true );
            //    if ( org != null && org.Id > 0 )
            //    {
            //        output.CurriculumControlAuthorityId = org.Id;
            //    } else
            //    {
            //        ChangeSummary status = new ChangeSummary();
            //        org = new MSc.Organization()
            //        {
            //            Name = input.Curriculum_Control_Authority,
            //            AlternateName = input.Curriculum_Control_Authority
            //        };
            //        var isValid = new OrganizationManager().Save( org, input.LastUpdatedById, ref status );
            //        if ( isValid )
            //        {
            //            output.CurriculumControlAuthorityId = org.Id;
            //        }
            //    }
            //}
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
        public static AppEntity GetByCodedNotation( string codedNotation, bool includingTrainingTasks = false )
        {
            var entity = new AppEntity();
            if ( string.IsNullOrWhiteSpace( codedNotation ) )
                return null;

            using ( var context = new DataEntities() )
            {
                var item = context.Course
                            .FirstOrDefault( s => s.CodedNotation.ToLower() == codedNotation.ToLower() );

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
            if (input.Course_AssessmentType != null)
            {
                foreach (var item in input.Course_AssessmentType )
                {
                    if ( item != null && item.ConceptScheme_Concept != null )
                    {
                        output.AssessmentMethodType.Add( item.ConceptScheme_Concept.RowId );
                        output.AssessmentMethods.Add( item.ConceptScheme_Concept.Name );
                    }
                }
            }
            //
            if ( input.Course_CourseType?.Count > 0 )
            {
                output.CourseTypes = new List<Guid>();
                int cntr = 0;
                foreach ( var item in input.Course_CourseType )
                {
                    cntr++;
                    if ( item != null && item.ConceptScheme_Concept != null )
                    {
                        output.CourseTypes.Add( item.ConceptScheme_Concept.RowId );
                        if ( cntr == 1 )
                            output.CourseType = item.ConceptScheme_Concept.RowId;
                        output.CourseTypeList.Add( item.ConceptScheme_Concept.Name );
                    }
                }
            }
            //
            if ( input.LifeCycleControlDocumentId != null )
            {
                output.LifeCycleControlDocumentId = (int)input.LifeCycleControlDocumentId;
                if ( input.ReferenceResource?.Id > 0 )
                {
                    output.LifeCycleControlDocument = input.ReferenceResource.Name;
                    output.HasReferenceResource = input.ReferenceResource.RowId;
                }
            }
            //
            if ( input.Course_Organization?.Count > 0 )
            {
                output.Organizations = new List<string>();
                foreach ( var item in input.Course_Organization )
                {
                    if ( item != null && item.Organization != null )
                    {
                        output.CurriculumControlAuthority.Add( item.Organization.RowId );
                        output.Organizations.Add( item.Organization.Name );
                    }
                }
            }

            //
            if ( includingTrainingTasks && input?.Course_Task?.Count > 0 )
            {
                foreach ( var item in input.Course_Task )
                {
                    output.HasTrainingTask.Add( item.RowId );
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
        public void UpdateParts( AppEntity input, ChangeSummary status )
        {
            try
            {
                //CourseTaskSave( input, ref status );
                new TrainingTaskManager().SaveList( input, false, ref status );

                CurriculumControlAuthorityUpdate( input, ref status );
                //TBD
                if (IsValidGuid(input.CourseType))
                {
                    if ( input.CourseTypes == null)
                        input.CourseTypes = new List<Guid>();
                    input.CourseTypes.Add( input.CourseType );
                }
                //CourseConceptSave( input, ConceptSchemeManager.ConceptScheme_CourseType, input.CourseTypes, "CourseType", ref status );
                //CourseConceptSave( input, ConceptSchemeManager.ConceptScheme_CurrentAssessmentApproach, input.AssessmentMethodType, "AssessmentMethodType", ref status );
                CourseTypeSave( input, input.CourseTypes, ref status );
                CourseAssessmentMethodSave( input, input.AssessmentMethodType, ref status );


            } catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }
        /*
        public void UpdateParts( AppFullEntity input, ChangeSummary status )
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
        */
        #region TrainingTask
        ///// <summary>
        ///// Not sure if we want to get thousands of tasks
        ///// </summary>
        ///// <returns></returns>
        //public static List<CourseTask> TrainingTaskGetAll()
        //{
        //    var entity = new CourseTask();
        //    var list = new List<CourseTask>();

        //    using ( var context = new DataEntities() )
        //    {
        //        var results = context.Course_Task
        //                .OrderBy( s => s.Id )
        //                .ToList();
        //        if ( results?.Count > 0 )
        //        {
        //            foreach ( var item in results )
        //            {
        //                if ( item != null && item.Id > 0 )
        //                {
        //                    entity = new CourseTask();
        //                    MapFromDB( item, entity, true );
        //                    list.Add( ( entity ) );
        //                }
        //            }
        //        }

        //    }
        //    return list;
        //}
        //public static void MapFromDB( Course_Task input, CourseTask output, bool includingCourseId = false )
        //{
        //    //should include list of concepts
        //    List<string> errors = new List<string>();
        //    BaseFactory.AutoMap( input, output, errors );
        //    if ( input.RowId != output.RowId )
        //    {
        //        output.RowId = input.RowId;
        //    }
        //    //
        //    if ( includingCourseId )
        //    {
        //        if ( input.Course?.Id > 0 )
        //        {
        //            output.Course = input.Course.RowId;
        //        }
        //    }

        //}
        #endregion


        #region Course concepts
        public bool CourseAssessmentMethodSave( AppEntity input, List<Guid> concepts, ref ChangeSummary status )
        {
            bool success = false;
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.Course_AssessmentType();
            var entityType = "Course_AssessmentType";

            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( concepts?.Count == 0 )
                        concepts = new List<Guid>();
                    //check existance
                    var results =
                                    from entity in context.Course_AssessmentType
                                    join concept in context.ConceptScheme_Concept
                                    on entity.AssessmentMethodConceptId equals concept.Id
                                    where entity.CourseId == input.Id

                                    select new MSc.Concept()
                                    {
                                        Id = concept.Id,
                                        Name = concept.Name,
                                        RowId = concept.RowId,
                                        ConceptSchemeId = concept.ConceptSchemeId,
                                    };

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
                                    DeleteCourseAssessmentType( input.Id, e.Id, ref status );
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
                                    efEntity.CourseId = input.Id;
                                    efEntity.AssessmentMethodConceptId = concept.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.Course_AssessmentType.Add( efEntity );

                                    int count = context.SaveChanges();
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For Course: '{0}' ({1}) a Course entity was not found for Identifier: {2}", input.Name, input.Id, child ) );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".CourseAssessmentMethodSave failed, Course: '{0}' ({1})", entityType, input.Name, input.Id ) );
                    status.AddError( thisClassName + ".CourseAssessmentMethodSave(). Error - the save was not successful. \r\n" + message );
                }
            }
            return success;
        }
        public bool DeleteCourseAssessmentType( int courseId, int conceptId, ref ChangeSummary status )
        {
            bool isValid = false;
            if ( conceptId == 0 )
            {
                //statusMessage = "Error - missing an identifier for the CourseConcept to remove";
                return false;
            }

            using ( var context = new DataEntities() )
            {
                var efEntity = context.Course_AssessmentType
                                .FirstOrDefault( s => s.CourseId == courseId && s.AssessmentMethodConceptId == conceptId );

                if ( efEntity != null && efEntity.Id > 0 )
                {
                    context.Course_AssessmentType.Remove( efEntity );
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
        /*
        public bool CourseAssessmentMethodSave( AppEntity input, List<Guid> concepts, ref ChangeSummary status )
        {
            bool success = false;
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.Course_AssessmentType();
            var entityType = "Course_AssessmentType";

            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( concepts?.Count == 0 )
                        concepts = new List<Guid>();
                    //check existance
                    var existing = context.Course_AssessmentType
                        .Where( s => s.CourseId == input.Id )
                        .ToList();
                    if ( existing == null )
                        existing = new List<Course_AssessmentType>();

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
                                    context.Course_AssessmentType.Remove( e );
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
                                    efEntity.CourseId = input.Id;
                                    efEntity.AssessmentMethodConceptId = concept.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.Course_AssessmentType.Add( efEntity );

                                    int count = context.SaveChanges();
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For Course: '{0}' ({1}) a Course AssessmentMethod concept was not found for Identifier: {2}", input.Name, input.Id, child ) );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".CourseAssessmentMethodSave failed, Course: '{0}' ({1})", entityType, input.Name, input.Id ) );
                    status.AddError( thisClassName + ".CourseAssessmentMethodSave(). Error - the save was not successful. \r\n" + message );
                }
            }
            return success;
        }
        */
        //
        public bool CourseTypeSave( AppEntity input, List<Guid> concepts, ref ChangeSummary status )
        {
            bool success = false;
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.Course_CourseType();
            var entityType = "Course_CourseType";

            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( concepts?.Count == 0 )
                        concepts = new List<Guid>();
                    //check existance
                    var results =
                                    from entity in context.Course_CourseType
                                    join concept in context.ConceptScheme_Concept
                                    on entity.CourseTypeConceptId equals concept.Id
                                    where entity.CourseId == input.Id

                                    select new MSc.Concept()
                                    {
                                        Id = concept.Id,
                                        Name = concept.Name,
                                        RowId = concept.RowId,
                                        ConceptSchemeId = concept.ConceptSchemeId,
                                    };
                    //if ( existing == null )
                    //    existing = new List<ConceptScheme_Concept>();  
                    var existing = results?.ToList();
                    #region deletes check
                    if ( existing != null && existing.Count() > 0 )
                    {
                        //if exists not in input, delete it
                        foreach ( var e in existing )
                        {
                            var key = e.RowId;
                            if ( IsValidGuid( key ) )
                            {
                                if ( !concepts.Contains( ( Guid ) key ) )
                                {
                                    DeleteCourseType( input.Id, e.Id, ref status );
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
                            if ( existing != null && existing.Count() > 0 )
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
                                    efEntity.CourseId = input.Id;
                                    efEntity.CourseTypeConceptId = concept.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.Course_CourseType.Add( efEntity );

                                    int count = context.SaveChanges();
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For Course: '{0}' ({1}) a Course CourseType was not found for Identifier: {2}", input.Name, input.Id, child ) );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".CourseCourseTypeSave failed, Course: '{0}' ({1})", entityType, input.Name, input.Id ) );
                    status.AddError( thisClassName + ".CourseCourseTypeSave(). Error - the save was not successful. \r\n" + message );
                }
            }
            return success;
        }
        public bool DeleteCourseType( int courseId, int conceptId, ref ChangeSummary status )
        {
            bool isValid = false;
            if ( conceptId == 0 )
            {
                //statusMessage = "Error - missing an identifier for the CourseConcept to remove";
                return false;
            }

            using ( var context = new DataEntities() )
            {
                var efEntity = context.Course_CourseType
                                .FirstOrDefault( s => s.CourseId == courseId && s.CourseTypeConceptId == conceptId);

                if ( efEntity != null && efEntity.Id > 0 )
                {
                    context.Course_CourseType.Remove( efEntity );
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


        #region Course-Org
        public bool CurriculumControlAuthorityUpdate( AppEntity input, ref ChangeSummary status )
        {
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.Course_Organization();
            var entityType = "Course_Organization";
            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( input.CurriculumControlAuthority?.Count == 0 )
                        input.CurriculumControlAuthority = new List<Guid>();
                    //check existance
                    var existing = context.Course_Organization
                        .Where( s => s.CourseId == input.Id )
                        .ToList();
                    if ( existing == null )
                        existing = new List<Course_Organization>();
                    #region deletes check
                    if ( existing.Any() )
                    {
                        //if exists not in input, delete it
                        foreach ( var e in existing )
                        {
                            var key = e?.Organization.RowId;
                            if ( IsValidGuid( key ) )
                            {
                                if ( !input.CurriculumControlAuthority.Contains( ( Guid ) key ) )
                                {
                                    context.Course_Organization.Remove( e );
                                    int dcount = context.SaveChanges();
                                }
                            }
                        }
                    }
                    #endregion
                    //adds
                    if ( input.CurriculumControlAuthority != null )
                    {
                        foreach ( var child in input.CurriculumControlAuthority )
                        {
                            //if not in existing, then add
                            bool doingAdd = true;
                            if ( existing?.Count > 0 )
                            {
                                var isfound = existing.Select( s => s.Organization.RowId == child ).ToList();
                                if ( isfound.Any() )
                                    doingAdd = false;
                            }
                            if ( doingAdd ) 
                            {
                                var org = OrganizationManager.Get( child );
                                if ( org?.Id > 0 )
                                {
                                    //ReferenceConceptAdd( input, concept.Id, input.LastUpdatedById, ref status );
                                    efEntity.CourseId = input.Id;
                                    efEntity.OrganizationId = org.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.Course_Organization.Add( efEntity );

                                    // submit the change to database
                                    int count = context.SaveChanges();
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For Course: '{0}' ({1}) a Course CurriculumControlAuthority was not found for Identifier: {2}", input.Name, input.Id, child ) );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".CurriculumControlAuthorityUpdate-'{0}', RatingTask: '{1}' ({2})", entityType, input.Name, input.Id ) );
                    status.AddError( thisClassName + ".CurriculumControlAuthorityUpdate(). Error - the save was not successful. \r\n" + message );
                }
            }
            return false;
        }
        #endregion

    }

}
