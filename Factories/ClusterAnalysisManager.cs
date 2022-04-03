using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Curation;
using AppEntity = Models.Schema.ClusterAnalysis;
using DBEntity = Data.Tables.ClusterAnalysis;
using MSc = Models.Schema;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Models.Application;
using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class ClusterAnalysisManager : BaseFactory
    {
        public static new string thisClassName = "ClusterAnalysisManager";
        #region ClusterAnalysis - persistance ==================
        /// <summary>
        /// Update a ClusterAnalysis
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
                        //how to check for existing?
                        var record = GetExisting( entity );
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
                    DBEntity efEntity = context.ClusterAnalysis
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
                                string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a ClusterAnalysis. The process appeared to not work, but was not an exception, so we have no message, or no clue. ClusterAnalysis: {0}, Id: {1}", entity.ClusterAnalysisTitle, entity.Id );
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
                                ActivityType = "ClusterAnalysis",
                                Activity = status.Action,
                                Event = "Update",
                                Comment = string.Format( "ClusterAnalysis was updated by the import. Name: {0}", entity.ClusterAnalysisTitle ),
                                ActionByUserId = entity.LastUpdatedById,
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
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, ClusterAnalysisTitle: {1}", entity.Id, entity.ClusterAnalysisTitle ), "ClusterAnalysis" );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.ClusterAnalysisTitle ), true );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
                isValid = false;
            }


            return isValid;
        }

        /// <summary>
        /// add a ClusterAnalysis
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

                    context.ClusterAnalysis.Add( efEntity );

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
                            ActivityType = "ClusterAnalysis",
                            Activity = status.Action,
                            Event = "Add",
                            Comment = string.Format( "ClusterAnalysis: '{0} was added.", entity.ClusterAnalysisTitle ),
                            ActivityObjectId = entity.Id,
                            ActionByUserId = entity.LastUpdatedById
                        };
                        new ActivityManager().SiteActivityAdd( sa );
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a ClusterAnalysis. The process appeared to not work, but was not an exception, so we have no message, or no clue. ClusterAnalysis: {0}, CodedNotation: {1}", entity.ClusterAnalysisTitle, entity.TrainingSolutionType );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "ClusterAnalysis" );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. " + message );

                    LoggingHelper.LogError( message, true );
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), ClusterAnalysisTitle:{0}, entity.TrainingSolutionType: {1}", entity.ClusterAnalysisTitle, entity.TrainingSolutionType ) );
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
            /*
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
                        status.AddError( thisClassName + String.Format( ".MapToDB. ClusterAnalysis: '{0}'. The related HasReferenceResource '{1}' was not found", input.Name, input.HasReferenceResource ) );
                    }
                }
            }
            else
            {
                output.LifeCycleControlDocumentId = null;
            }
            //
            //this may be removed if there can be multiple CCA
            //22-01-24 - Navy confirmed only one!
            if ( input.CurriculumControlAuthority?.Count > 0 )
            {
                foreach ( var item in input.CurriculumControlAuthority )
                {
                    //all org adds will occur before here
                    var org = OrganizationManager.Get( item );
                    if ( org != null && org.Id > 0 )
                    {
                        //TODO - now using ClusterAnalysis.Organization
                        output.CurriculumControlAuthorityId = org.Id;
                        //only can handle one here
                    }
                    else
                    {
                        //should not have a new org here
                        //NO, all new orgs will have been added first, so this would be an error
                    }
                }

            }
            */
        }
        #endregion
        #region Retrieval

        public static AppEntity GetExisting( AppEntity entity )
        {
            var existing = new AppEntity();
            //if ( string.IsNullOrWhiteSpace( codedNotation ) )
            //    return null;

            //using ( var context = new DataEntities() )
            //{
            //    var item = context.ClusterAnalysis
            //                .FirstOrDefault( s => s.CodedNotation.ToLower() == codedNotation.ToLower() );

            //    if ( item != null && item.Id > 0 )
            //    {
            //        MapFromDB( item, existing );
            //    }
            //}
            return existing;
        }
        public static AppEntity Get( Guid rowId, bool includingTrainingTasks = false )
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                var item = context.ClusterAnalysis
                            .FirstOrDefault( s => s.RowId == rowId );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingTrainingTasks );
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
                var item = context.ClusterAnalysis
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
        public static List<AppEntity> GetAll( )
        {
            var entity = new AppEntity();
            var list = new List<AppEntity>();

            using ( var context = new DataEntities() )
            {
                var results = context.ClusterAnalysis
                        .OrderBy( s => s.Id )
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
                    var list = from Results in context.ClusterAnalysis
                               select Results;
                    //if ( !string.IsNullOrWhiteSpace( filter ) )
                    //{
                    //    list = from Results in list
                    //            .Where( s =>
                    //            ( s.Name.ToLower().Contains( filter.ToLower() ) ) ||
                    //            ( s.CodedNotation.ToLower() == filter.ToLower() )
                    //            )
                    //           select Results;
                    //}
                    query.TotalResults = list.Count();
                    //sort order not handled
                    list = list.OrderBy( p => p.Id );

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
        public static void MapFromDB( DBEntity input, AppEntity output, bool includingTrainingTasks = false )
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }
            /*
            if ( input.ClusterAnalysis_AssessmentType != null )
            {
                foreach ( var item in input.ClusterAnalysis_AssessmentType )
                {
                    if ( item != null && item.ConceptScheme_Concept != null )
                    {
                        output.AssessmentMethodType.Add( item.ConceptScheme_Concept.RowId );
                        output.AssessmentMethods.Add( item.ConceptScheme_Concept.Name );
                    }
                }
            }
            //
            if ( input.ClusterAnalysis_ClusterAnalysisType?.Count > 0 )
            {
                output.ClusterAnalysisType = new List<Guid>();
                int cntr = 0;
                foreach ( var item in input.ClusterAnalysis_ClusterAnalysisType )
                {
                    cntr++;
                    if ( item != null && item.ConceptScheme_Concept != null )
                    {
                        output.ClusterAnalysisType.Add( item.ConceptScheme_Concept.RowId );
                        output.ClusterAnalysisTypeList.Add( item.ConceptScheme_Concept.Name );
                    }
                }
            }
            //
            if ( input.CurriculumControlAuthorityId != null )
            {
                output.CurriculumControlAuthorityId = ( int ) input.CurriculumControlAuthorityId;
                if ( input.Organization?.Id > 0 )
                {
                    output.OrganizationName = input.Organization.Name;
                    output.Organizations.Add( output.OrganizationName );
                    output.CurriculumControlAuthority.Add( input.Organization.RowId );
                }
            }
            //
            if ( input.LifeCycleControlDocumentId != null )
            {
                output.LifeCycleControlDocumentId = ( int ) input.LifeCycleControlDocumentId;
                if ( input.ReferenceResource?.Id > 0 )
                {
                    output.LifeCycleControlDocument = input.ReferenceResource.Name;
                    output.HasReferenceResource = input.ReferenceResource.RowId;
                }
            }
            //
            //if ( input.ClusterAnalysis_Organization?.Count > 0 )
            //{
            //    output.Organizations = new List<string>();
            //    foreach ( var item in input.ClusterAnalysis_Organization )
            //    {
            //        if ( item != null && item.Organization != null )
            //        {
            //            output.CurriculumControlAuthority.Add( item.Organization.RowId );
            //            output.Organizations.Add( item.Organization.Name );
            //        }
            //    }
            //}

            //
            if ( includingTrainingTasks && input?.ClusterAnalysis_Task?.Count > 0 )
            {
                foreach ( var item in input.ClusterAnalysis_Task )
                {
                    output.HasTrainingTask.Add( item.RowId );
                }
            }
            */
        }

        #endregion

        /// <summary>
        /// Update:
        /// - training task
        /// - ClusterAnalysis types
        /// - CurrentAssessmentApproach
        /// </summary>
        /// <param name="input"></param>
        /// <param name="status"></param>
        public void UpdateParts( AppEntity input, ChangeSummary status )
        {
            try
            {
                /*
                new TrainingTaskManager().SaveList( input, false, ref status );

                ClusterAnalysisTypeSave( input, input.ClusterAnalysisType, ref status );
                ClusterAnalysisAssessmentMethodSave( input, input.AssessmentMethodType, ref status );
                */

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }

 
    }

}
