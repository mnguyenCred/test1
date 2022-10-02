using System;
using System.Collections.Generic;
using System.Linq;

using Models.Application;
using Models.Curation;
using Models.Search;

using Navy.Utilities;

using AppEntity = Models.Schema.ClusterAnalysis;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.ClusterAnalysis;

namespace Factories
{
    public class ClusterAnalysisManager : BaseFactory
    {
        public static new string thisClassName = "ClusterAnalysisManager";
        /*


        */
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
                //if ( entity.RatingTaskId < 1)
                //{
                //    var ratingTask = RatingTaskManager.Get( entity.RatingTaskRowId );
                //    if ( ratingTask?.Id > 0 )
                //        entity.RatingTaskId = ratingTask.Id;
                //    else
                //    {
                //        status.AddError( String.Format("A valid rating task identifier was not provided for Cluster Analysis record ({0}).", entity.RowId ));
                //        return false;
                //    }
                //}
                using ( var context = new DataEntities() )
                {
                    //if ( ValidateProfile( entity, ref status ) == false )
                    //    return false;
                    //look up if no id
                    if ( entity.Id == 0 )
                    {
                        //how to check for existing?
                        //just by rating task id for now
                        //this is more difficult now!
                        //var record = GetExisting( entity );
                        //if ( record?.Id > 0 )
                        //{
                        //    entity.Id = record.Id;
                        //    //could be other updates, fall thru to the update
                        //    //return true;
                        //}
                        //else
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
                    //TODO - ???
                    context.Configuration.LazyLoadingEnabled = false;
                    DBEntity efEntity = context.ClusterAnalysis
                            .FirstOrDefault( s => s.Id == entity.Id );

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

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a ClusterAnalysis. The process appeared to not work, but was not an exception, so we have no message, or no clue. ClusterAnalysis: {0}, CodedNotation: {1}", entity.ClusterAnalysisTitle, entity.TrainingSolution );
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
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), ClusterAnalysisTitle:{0}, entity.TrainingSolutionType: {1}", entity.ClusterAnalysisTitle, entity.TrainingSolution ) );
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
            //
            BaseFactory.AutoMap( input, output, errors );
            //clean up Development ratio (pair off :00)
            if ( output.DevelopmentRatio?.Length > 0 && output.DevelopmentRatio.EndsWith( ":00" )) 
            {
                output.DevelopmentRatio = output.DevelopmentRatio.Substring(0, output.DevelopmentRatio.Length - 3);
            }
            //if ( IsValidGuid( input.TrainingSolutionType ) )
            //{
            //    output.TrainingSolutionTypeId = ( int ) ConceptSchemeManager.GetConcept( input.TrainingSolutionTypeId )?.Id;
            //}
            if ( input.TrainingSolutionTypeId > 0 )
                output.TrainingSolutionTypeId = input.TrainingSolutionTypeId;

            else if ( !string.IsNullOrWhiteSpace( input.TrainingSolution ) )
            {
                output.TrainingSolutionTypeId = ( int ) ConceptSchemeManager.GetConcept( ConceptSchemeManager.ConceptScheme_TrainingSolutionType, input.TrainingSolution )?.Id;
            }
            else
                output.TrainingSolutionTypeId = null;

            if ( input.RecommendedModalityTypeId > 0 )
                output.RecommendedModalityId = input.RecommendedModalityTypeId;

            else if ( !string.IsNullOrWhiteSpace( input.RecommendedModality ) )
            {
                //
                output.RecommendedModalityId = ( int ) ConceptSchemeManager.GetConcept( ConceptSchemeManager.ConceptScheme_RecommendedModality, input.RecommendedModality )?.Id;
            }
            else
                output.RecommendedModalityId = null;

            if ( input.DevelopmentSpecificationTypeId > 0 )
                output.DevelopmentSpecificationId = input.DevelopmentSpecificationTypeId;

            else if ( !string.IsNullOrWhiteSpace( input.DevelopmentSpecification ) )
            {
                output.DevelopmentSpecificationId = ( int ) ConceptSchemeManager.GetConcept( ConceptSchemeManager.ConceptScheme_DevelopmentSpecification, input.DevelopmentSpecification )?.Id;
            }
            //if ( !string.IsNullOrWhiteSpace( input.CFMPlacement ) )
            //{
            //    output.CFMPlacementId = ( int ) ConceptSchemeManager.GetConcept( ConceptSchemeManager.ConceptScheme_CFMPlacement, input.CFMPlacement )?.Id;
            //}
            //confirm mapping happens for nullable fields
            output.EstimatedInstructionalTime = input.EstimatedInstructionalTime;
            output.DevelopmentTime = input.DevelopmentTime;
            output.PriorityPlacement = input.PriorityPlacement;
        }
        #endregion
        #region Retrieval

        /// <summary>
        /// this is more difficult now!
        /// For updates, the rating context would have a clusterAnalysisId
        /// May not do this now. would have to check almost all properties
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static AppEntity GetExisting( int id )
        {
            //just by rating task id for now
            var existing = new AppEntity();
            //if ( entity.RatingTaskId == 0 )
            //    return null;

            using ( var context = new DataEntities() )
            {
                var item = context.ClusterAnalysis
                            .FirstOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, existing );
                }
            }
            return existing;
        }

        //TODO - Sigh, now a list
        //ratingContext -> clusterAnalysisId -> clusterAnalysis
        //ratingTask -> ratingContext.RatingTaskId -> ratingContext -> clusterAnalysisId -> clusterAnalysis
        public static AppEntity GetForUpload( Guid rowRatingTaskRowId )
        {
            var entity = new AppEntity();

            //using ( var context = new DataEntities() )
            //{
            //    var item = context.ClusterAnalysis
            //                .FirstOrDefault( s => s.RatingContext.RowId == rowRatingTaskRowId );

            //    if ( item != null && item.Id > 0 )
            //    {
            //        MapFromDB( item, entity );
            //    }
            //}
            return entity;
        }
        public static AppEntity Get( Guid rowId)
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                var item = context.ClusterAnalysis
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
                    if ( !string.IsNullOrWhiteSpace( filter ) )
                    {
                        //list = from Results in list
                        //        .Where( s =>
                        //        ( s.ClusterAnalysisTitle.ToLower().Contains( filter.ToLower() ) )
                        //            || ( s.ConceptScheme_TrainingSolution!= null && s.ConceptScheme_TrainingSolution.Name.ToLower().Contains(filter.ToLower()) )
                        //            || ( s.ConceptScheme_RecommendedModality != null && s.ConceptScheme_RecommendedModality.Name.ToLower().Contains(filter.ToLower()) )
                        //        )
                        //       select Results;
                    }
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
                                MapFromDB( item, entity, true );
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
        public static void MapFromDB( DBEntity input, AppEntity output, bool formatForSearch = false )
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }

            //handle nullables
            if ( input.TrainingSolutionTypeId != null )
            {
                output.TrainingSolutionTypeId = ( int ) input.TrainingSolutionTypeId;
                //if ( input.ConceptScheme_TrainingSolution?.Id > 0 )
                //{
                //    output.TrainingSolution = input.ConceptScheme_TrainingSolution.Name;
                //    output.TrainingSolutionType = input.ConceptScheme_TrainingSolution.RowId;
                //}
            }
            if ( input.DevelopmentSpecificationId != null )
            {
                output.DevelopmentSpecificationTypeId = ( int ) input.DevelopmentSpecificationId;
                //if ( input.ConceptScheme_DevelopementSpec?.Id > 0 )
                //{
                //    output.DevelopmentSpecification = input.ConceptScheme_DevelopementSpec.Name;
                //    output.DevelopmentSpecificationType = input.ConceptScheme_DevelopementSpec.RowId;
                //}
            }
            if ( input.RecommendedModalityId != null )
            {
                output.RecommendedModalityTypeId = ( int ) input.RecommendedModalityId;
                //if ( input.ConceptScheme_RecommendedModality?.Id > 0 )
                //{
                //    output.RecommendedModality = input.ConceptScheme_RecommendedModality.Name;
                //    output.RecommendedModalityType = input.ConceptScheme_RecommendedModality.RowId;
                //}
            }
            if ( input.PriorityPlacement != null )
            {
                output.PriorityPlacement = ( int ) input.PriorityPlacement;
               // output.PriorityPlacementLabel = output.PriorityPlacement.ToString();
            }

            if ( input.DevelopmentTime != null )
            {
                output.DevelopmentTime = input.DevelopmentTime;
              //  output.DevelopmentTimeLabel = output.DevelopmentTime.ToString();
            }
            if ( formatForSearch )
            {
                //output.Label = String.Format( "{0} ~ {1} ~ {2} ~ {3}", output.TrainingSolution, output.ClusterAnalysisTitle, output.RecommendedModality, output.DevelopmentSpecification );
            }
            /*

            */
        }
        public static AppEntity MapFromDB( DBEntity input, bool formatForSearch = false )
        {
            AppEntity output = new AppEntity();
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }
            if ( formatForSearch )
            {
                //output.Label = String.Format( "", output.ClusterAnalysisTitle, output.TrainingSolution, output.RecommendedModality );
            }
            //handle nullables
            if ( input.TrainingSolutionTypeId != null )
            {
                output.TrainingSolutionTypeId = ( int ) input.TrainingSolutionTypeId;
            }
            if ( input.DevelopmentSpecificationId != null )
            {
                output.DevelopmentSpecificationTypeId = ( int ) input.DevelopmentSpecificationId;
            }
            if ( input.RecommendedModalityId != null )
            {
                output.RecommendedModalityTypeId = ( int ) input.RecommendedModalityId;
            }
            if ( input.PriorityPlacement != null )
            {
                output.PriorityPlacement = ( int ) input.PriorityPlacement;
            }
            if ( input.DevelopmentTime != null )
            {
                output.DevelopmentTime = input.DevelopmentTime;
            }

            return output;
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
                ???????????????????????
                */

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }

 
    }

}
