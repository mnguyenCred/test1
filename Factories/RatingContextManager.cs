using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

using Models.Application;
using Models.Curation;
using Models.Search;

using Navy.Utilities;

using AppEntity = Models.Schema.RatingContext;
using EntitySummary = Models.Schema.RatingContextSummary;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.RatingContext;
using ViewContext = Data.Views.ceNavyViewEntities;
using CachedEntity = Factories.CachedRatingContext;
namespace Factories
{
	public class RatingContextManager : BaseFactory
	{
		public static new string thisClassName = "RatingContextManager";
		public static string cacheKey = "RatingContextCache";

		#region === persistance ==================
		/// <summary>
		/// Update a RatingContext
		/// </summary>
		/// <param name="input"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Save( AppEntity input, ref ChangeSummary status, bool fromUpload = true )
		{
            bool isValid = true;
            int count = 0;
            try
            {
                using ( var context = new DataEntities() )
                {
                    //if ( ValidateProfile( entity, ref status ) == false )
                    //    return false;
                    //look up if no id
                    if ( input.Id == 0 )
                    {
                        //need to identify for sure what is unique
                        //use codedNotation first if present
                        var record = Get( input, status.RatingCodedNotation );
                        if ( record?.Id > 0 )
                        {
                            input.Id = record.Id;
                            UpdateParts( input, status, fromUpload );
                            //??
                            return true;
                        }
                        else
                        {
                            //add
                            int newId = Add( input, ref status, fromUpload );
                            if ( newId == 0 || status.HasSectionErrors )
                                isValid = false;
                        }
                    }
                    else
                    {
                        //TODO - consider if necessary, or interferes with anything
                        context.Configuration.LazyLoadingEnabled = false;
                        DBEntity efEntity = context.RatingContext
                                .SingleOrDefault( s => s.Id == input.Id );

                        if ( efEntity != null && efEntity.Id > 0 )
                        {
                            //fill in fields that may not be in entity
                            input.RowId = efEntity.RowId;
                            input.Created = efEntity.Created;
                            input.CreatedById = ( efEntity.CreatedById ?? 0 );
                            input.Id = efEntity.Id;

                            MapToDB( input, efEntity, ref status );
                            bool hasChanged = false;
                            if ( HasStateChanged( context ) )
                            {
                                hasChanged = true;
                                efEntity.LastUpdated = DateTime.Now;
                                efEntity.LastUpdatedById = input.LastUpdatedById;
                                count = context.SaveChanges();
                                //can be zero if no data changed
                                if ( count >= 0 )
                                {
                                    input.LastUpdated = ( DateTime ) efEntity.LastUpdated;
                                    isValid = true;
                                }
                                else
                                {
                                    //?no info on error

                                    isValid = false;
                                    string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a RatingContext. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingContextId: {0}, HasRatingId: {1}", input.Id, input.HasRatingId);
                                    status.AddError( "Error - the update was not successful. " + message );
                                    EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                                }

                            }

                            if ( isValid )
                            {
                                //update parts
                                UpdateParts( input, status, fromUpload );
                                if ( hasChanged )
                                {
                                    SiteActivity sa = new SiteActivity()
                                    {
                                        ActivityType = "RatingContext",
                                        Activity = status.Action,
                                        Event = "Update",
                                        //Comment = string.Format( "RatingContext was updated. Name: {0}", FormatLongLabel( input.HasRatingContext.Description ) ),
                                        ActionByUserId = input.LastUpdatedById,
                                        ActivityObjectId = input.Id
                                    };
                                    new ActivityManager().SiteActivityAdd( sa );
                                }
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
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, CodedNotation: {1}", input.Id, input.CodedNotation ), "RatingContext" );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, CodedNotation: {1}", input.Id, input.CodedNotation ), true );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
                isValid = false;
            }
            return isValid;
        }
        //

        /// <summary>
        /// Add a RatingContext
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private int Add( AppEntity entity, ref ChangeSummary status, bool fromUpload )
        {
            DBEntity efEntity = new DBEntity();
            status.HasSectionErrors = false;
            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( !IsValidGuid( entity.PayGradeType ) || !IsValidGuid( entity.ApplicabilityType ) )
                    {
                        //probably an issue for updating
                        status.AddError( "Incomplete data was encountered trying to add a RatingContext - probably related to how updates are being done!!!" );
                        return 0;
                    }

                    MapToDB( entity, efEntity, ref status );

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

                    context.RatingContext.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        entity.RowId = efEntity.RowId;
                        entity.Id = efEntity.Id;
                        UpdateParts( entity, status, fromUpload );
                        //
                        //add log entry
                        SiteActivity sa = new SiteActivity()
                        {
                            ActivityType = "RatingContext",
                            Activity = status.Action,
                            Event = "Add",
                            Comment = string.Format( " A RatingContext was added. CodedNotation: {0}", entity.CodedNotation ),
                            ActionByUserId = entity.LastUpdatedById,
                            ActivityObjectId = entity.Id
                        };
                        new ActivityManager().SiteActivityAdd( sa );
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a RatingContext. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingContext: '{0}'", entity.CodedNotation );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( "RatingContextManager. Add Failed", message );
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Add() RatingContext: '{0}'", entity.CodedNotation ), "RatingContext" );
                    status.AddError( thisClassName + ".Add(). Data Validation Error - the save was not successful. " + message );

                    LoggingHelper.LogError( message, true );
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), RatingContext: '{0}'", entity.CodedNotation ) );
                    status.AddError( thisClassName + string.Format( ".Add(), RatingContext: '{0}'. ", entity.CodedNotation ) + message );
                }
            }

            return efEntity.Id;
        }
        //

        public void UpdateParts( AppEntity input, ChangeSummary status, bool fromUpload )
        {
            try
            {
                //FunctionArea/WorkRole
                //WorkRoleUpdate( input, ref status );

                ////RatingContext.HasRating
                //HasRatingUpdate( input, ref status );
                ////RatingContext.HasJob
                //HasJobUpdate( input, ref status );

                //if ( UtilityManager.GetAppKeyValue( "handlingMultipleTrainingTasksPerRatingContext", false ) )
                //{
                //    TrainingTaskUpdate( input, ref status, fromUpload );
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }
        //

        public static void MapToDB( AppEntity input, DBEntity output, ref ChangeSummary status )
        {
            if ( input.Note?.ToLower() == "n/a" )
                input.Note = "";
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            /* will we have the usual problem of guids versus Ids?
             * confirm have 
             * billet title
             *  - Id must have been already saved
             * rating
             * functional areas??
             * rating task
             *  - rt must be saved first
             *  - reference resource etc is in the latter
             * applicability
             * paygrade
             * training gap
             * training task
                * the training task must be saved first     
             * cluster analysis
             *      - the cluster analysis must be saved first
             * 
             */ 
            //check if can handle nullables - or do these get missed
            //
            if ( IsValidGuid( input.PayGradeType ) )
            {
                var currentRankId = output.RankId;
               // var currentLevelId = output.LevelId;
                //
                //var concept = ConceptSchemeManager.GetConcept( input.PayGradeType );
                //if ( concept?.Id > 0 )
                //{
                //    output.RankId = concept.Id;
                //    if ( output.RankId != currentRankId || currentLevelId == 0 )
                //    {
                //        //level is tied to Paygrade.so
                //        var paygradeLevel = GetPayGradeLevel( concept.CodedNotation );
                //        output.LevelId = ( int ) ConceptSchemeManager.GetConcept( ConceptSchemeManager.ConceptScheme_RatingLevel, paygradeLevel )?.Id;

                //    }
                //}
            }
            else
            {
                output.RankId = 0;
            }
            //TaskApplicability
            if ( IsValidGuid( input.ApplicabilityType ) )
            {
                output.TaskApplicabilityId = ( int ) ConceptSchemeManager.GetConcept( input.ApplicabilityType )?.Id;
            }
            else
                output.TaskApplicabilityId = null;
            //HasReferenceResource - ReferenceResourceId
            //this is on RatingContext
            //if ( IsValidGuid( input.HasReferenceResource ) )
            //{
            //    //TODO - can we get this info prior to here??
            //    //output.ReferenceResourceId = ReferenceResourceManager.Get( input.HasReferenceResource )?.Id;

            //    if ( output.ReferenceResourceId != null && output.ToReferenceResource?.RowId == input.HasReferenceResource )
            //    {
            //        //no action
            //    }
            //    else
            //    {
            //        var entity = ReferenceResourceManager.Get( input.HasReferenceResource );
            //        if ( entity?.Id > 0 )
            //            output.ReferenceResourceId = ( int ) entity?.Id;
            //        else
            //        {
            //            status.AddError( thisClassName + String.Format( ".MapToDB. CodedNotation: '{0}' RatingContext: '{1}'. The related SOURCE (HasReferenceResource) '{2}' was not found", input.CodedNotation, FormatLongLabel( input.Description ), input.HasReferenceResource ) );
            //        }
            //    }
            //}
            //else
            //    output.ReferenceResourceId = null;
            //ReferenceType-WorkElementType
            //if ( IsValidGuid( input.ReferenceType ) )
            //{
            //    if ( output.WorkElementTypeId != null && output.ConceptScheme_WorkElementType?.RowId == input.ReferenceType )
            //    {
            //        //no action
            //    }
            //    else
            //    {
            //        var entity = ConceptSchemeManager.GetConcept( input.ReferenceType );
            //        if ( entity?.Id > 0 )
            //            output.WorkElementTypeId = ( int ) entity?.Id;
            //        else
            //        {
            //            status.AddError( thisClassName + String.Format( ".MapToDB. RatingContext: '{0}'. The related WorkElementType (ReferenceType) '{1}' was not found", FormatLongLabel( input.Description ), input.ReferenceType ) );
            //        }
            //    }

            //}
            //else
            //    output.WorkElementTypeId = null;

            if ( IsValidGuid( input.TrainingGapType ) )
            {
                output.FormalTrainingGapId = ( int ) ConceptSchemeManager.GetConcept( input.TrainingGapType )?.Id;
            }
            //
           

        }
        //

        #endregion

        #region Retrieval
        /// <summary>
        /// realistically we must be able to get by rating id and coded notation
        /// </summary>
        /// <param name="importEntity"></param>
        /// <param name="currentRatingCodedNotation"></param>
        /// <returns></returns>
        public static AppEntity Get( AppEntity importEntity, string currentRatingCodedNotation )
		{
			var entity = new AppEntity();
            var item = new Data.Tables.RatingContext();
            using ( var context = new DataEntities() )
            {
                if ( !string.IsNullOrWhiteSpace( importEntity.CodedNotation ) )
                {
                    item = context.RatingContext.FirstOrDefault( s => ( s.CodedNotation ?? "" ).ToLower() == importEntity.CodedNotation.ToLower()
                                && s.Rating.Id == importEntity.HasRatingId );
                }
                if ( item == null || item.Id == 0 )
                {
                    //22-04-14 - TBD - not sure we want to do this approach - risky

                }
                if ( item != null && item.Id > 0 )
                {
                    //if exists, will just return the Id?
                    //or do a get, and continue?
                    entity = Get( item.Id, true );
                }
            }

            return entity;
		}
        public static AppEntity Get( int id, bool includingConcepts )
        {
            var entity = new AppEntity();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.RatingContext
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingConcepts );
                }
            }

            return entity;
        }
        //

        public static AppEntity Get( Guid rowId, bool includingConcepts = false )
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                var item = context.RatingContext
                            .FirstOrDefault( s => s.RowId == rowId );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingConcepts );
                }
            }
            return entity;
        }
        //

        public static AppEntity GetByCTIDOrNull( string ctid, bool includingConcepts = false )
        {
            if ( string.IsNullOrWhiteSpace( ctid ) )
            {
                return null;
            }

            using ( var context = new DataEntities() )
            {
                var item = context.RatingContext
                            .SingleOrDefault( s => s.CTID == ctid );

                if ( item != null && item.Id > 0 )
                {
                    var entity = new AppEntity();
                    MapFromDB( item, entity, includingConcepts );
                    return entity;
                }
            }

            return null;
        }
        //

        /// <summary>
        /// It is not clear that we want a get all - tens of thousands
        /// </summary>
        /// <returns></returns>
        public static List<AppEntity> GetAll()
        {
            var entity = new AppEntity();
            var list = new List<AppEntity>();

            using ( var context = new DataEntities() )
            {
                var results = context.RatingContext
                        .OrderBy( s => s.Id )
                        .ToList();
                if ( results?.Count > 0 )
                {
                    foreach ( var item in results )
                    {
                        if ( item != null && item.Id > 0 )
                        {
                            entity = new AppEntity();
                            MapFromDB( item, entity, false );
                            list.Add( ( entity ) );
                        }
                    }
                }

            }
            return list;
        }
        //

        public static List<AppEntity> GetMultiple( List<Guid> rowIDs )
        {

            var entity = new AppEntity();
            var list = new List<AppEntity>();
            if ( rowIDs == null || rowIDs.Count() == 0 )
            {
                return list;
            }

            using ( var context = new DataEntities() )
            {
                var results = context.RatingContext
                    .Where( m => rowIDs.Contains( m.RowId ) )
                        .OrderBy( s => s.Id )
                        .ToList();
                if ( results?.Count > 0 )
                {
                    foreach ( var item in results )
                    {
                        if ( item != null && item.Id > 0 )
                        {
                            entity = new AppEntity();
                            MapFromDB( item, entity, false );
                            list.Add( ( entity ) );
                        }
                    }
                }

            }
            return list;
        }
        //

        //Find loose matches, so other code can figure out whether there are any exact matches
        public static AppEntity GetForUpload( string ratingTaskDescription, Guid applicabilityTypeRowID, Guid sourceRowID, Guid payGradeRowID, Guid trainingGapTypeRowID )
        {
            var result = new AppEntity();

            using ( var context = new DataEntities() )
            {
                //var matches = context.RatingContext.Where( m =>
                //    m.Description.ToLower() == ratingTaskDescription.ToLower() &&
                //    m.ConceptScheme_Applicability.RowId == applicabilityTypeRowID &&
                //    m.ToReferenceResource.RowId == sourceRowID &&
                //    m.ConceptScheme_Rank.RowId == payGradeRowID// &&
                //                                               //m.ConceptScheme_TrainingGap.RowId == trainingGapTypeRowID //Using training gap type as a discriminator leads to duplicate tasks getting created when really they're just linked (or not) to training for one rating but not the other
                //);
                //if ( matches != null && matches.Count() > 0 )
                //{
                //    foreach ( var item in matches )
                //    {
                //        MapFromDB( item, result, false );
                //    }
                //}

            }

            return result;
        }
        public static AppEntity GetForUpload( string ratingCodedNotation, string codedNotation )
        {
            var result = new AppEntity();
            if ( string.IsNullOrWhiteSpace( codedNotation ) )
            {
                return result;
            }
            using ( var context = new DataEntities() )
            {
                try
                {
                    //get existing
                    //var results = from ratingTask in context.RatingContext
                    //              join hasRating in context.RatingContext_HasRating
                    //                on ratingTask.Id equals hasRating.RatingContextId
                    //              join rating in context.Rating
                    //                on hasRating.RatingId equals rating.Id
                    //              where ratingTask.CodedNotation.ToLower() == codedNotation.ToLower()
                    //              && rating.CodedNotation.ToLower() == ratingCodedNotation.ToLower()

                    //              select ratingTask;
                    //var existing = results?.ToList();
                    //if ( existing != null && existing.Count() > 0 )
                    //{
                    //    if ( existing.Count() == 1 )
                    //    {
                    //        MapFromDB( existing[0], result, false );
                    //    }
                    //    else
                    //    {
                    //        //this should not be possible - as long as there is a check to prevent duplicate RT codes in an upload!
                    //    }
                    //}

                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".GetForUpload()- ratingCodedNotation:'{0}', codedNotation: '{1}' ", ratingCodedNotation, codedNotation ) );
                }
            }


            return result;
        }

        public static List<AppEntity> Search( SearchQuery query )
        {
            var output = new List<AppEntity>();
            var skip = ( query.PageNumber - 1 ) * query.PageSize;
            try
            {
                //using ( var context = new DataEntities() )
                //{
                //    //Start query
                //    var list = context.RatingContext.AsQueryable();

                //    //Handle keywords filter
                //    var keywordsText = query.GetFilterTextByName( "search:Keyword" )?.ToLower();
                //    if ( !string.IsNullOrWhiteSpace( keywordsText ) )
                //    {
                //        list = list.Where( s =>
                //             s.Description.ToLower().Contains( keywordsText ) ||
                //             s.CodedNotation.ToLower().Contains( keywordsText )
                //        );
                //    }

                //    //Handle Has Rating
                //    var ratingFilter = query.GetFilterByName( "navy:Rating" );
                //    if ( ratingFilter != null && ratingFilter.ItemIds?.Count() > 0 )
                //    {
                //        list = list.Where( s =>
                //             s.RatingContext_HasRating.Where( t =>
                //                  ratingFilter.IsNegation ?
                //                      !ratingFilter.ItemIds.Contains( t.RatingId ) :
                //                      ratingFilter.ItemIds.Contains( t.RatingId )
                //             ).Count() > 0
                //        );
                //    }

                //    //Handle Has Job
                //    var jobFilter = query.GetFilterByName( "navy:Job" );
                //    if ( jobFilter != null && jobFilter.ItemIds?.Count() > 0 )
                //    {
                //        list = list.Where( s =>
                //             s.RatingContext_HasJob.Where( t =>
                //                jobFilter.IsNegation ?
                //                    !jobFilter.ItemIds.Contains( t.JobId ) :
                //                    jobFilter.ItemIds.Contains( t.JobId )
                //             ).Count() > 0
                //        );
                //    }

                //    //Handle Has Work Role
                //    var workRoleFilter = query.GetFilterByName( "ceterms:WorkRole" );
                //    if ( workRoleFilter != null && workRoleFilter.ItemIds?.Count() > 0 )
                //    {
                //        list = list.Where( s =>
                //             s.RatingContext_WorkRole.Where( t =>
                //                workRoleFilter.IsNegation ?
                //                    !workRoleFilter.ItemIds.Contains( t.WorkRoleId ) :
                //                    workRoleFilter.ItemIds.Contains( t.WorkRoleId )
                //             ).Count() > 0
                //        );
                //    }

                //    //Handle Has Reference Resource
                //    var referenceResourceFilter = query.GetFilterByName( "navy:ReferenceResource" );
                //    if ( referenceResourceFilter != null && referenceResourceFilter.ItemIds?.Count() > 0 )
                //    {
                //        list = list.Where( s =>
                //            referenceResourceFilter.IsNegation ?
                //                !referenceResourceFilter.ItemIds.Contains( s.ReferenceResourceId ?? 0 ) :
                //                referenceResourceFilter.ItemIds.Contains( s.ReferenceResourceId ?? 0 )
                //        );
                //    }

                //    //Handle Has Reference Resource Category
                //    var referenceResourceCategoryFilter = query.GetFilterByName( "navy:ReferenceResourceCategory" );
                //    if ( referenceResourceCategoryFilter != null && referenceResourceCategoryFilter.ItemIds?.Count() > 0 )
                //    {
                //        list = list.Where( s =>
                //            referenceResourceCategoryFilter.IsNegation ?
                //                !referenceResourceCategoryFilter.ItemIds.Contains( s.WorkElementTypeId ?? 0 ) :
                //                referenceResourceCategoryFilter.ItemIds.Contains( s.WorkElementTypeId ?? 0 )
                //        );
                //    }

                //    //Handle Training Gap Category
                //    var trainingGapCategoryFilter = query.GetFilterByName( "navy:TrainingGapCategory" );
                //    if ( trainingGapCategoryFilter != null && trainingGapCategoryFilter.ItemIds?.Count() > 0 )
                //    {
                //        list = list.Where( s =>
                //            trainingGapCategoryFilter.IsNegation ?
                //                !trainingGapCategoryFilter.ItemIds.Contains( s.FormalTrainingGapId ?? 0 ) :
                //                trainingGapCategoryFilter.ItemIds.Contains( s.FormalTrainingGapId ?? 0 )
                //        );
                //    }

                //    //Handle Applicability Category
                //    var applicabilityCategoryFilter = query.GetFilterByName( "navy:ApplicabilityCategory" );
                //    if ( applicabilityCategoryFilter != null && applicabilityCategoryFilter.ItemIds?.Count() > 0 )
                //    {
                //        list = list.Where( s =>
                //            applicabilityCategoryFilter.IsNegation ?
                //                !applicabilityCategoryFilter.ItemIds.Contains( s.TaskApplicabilityId ?? 0 ) :
                //                applicabilityCategoryFilter.ItemIds.Contains( s.TaskApplicabilityId ?? 0 )
                //        );
                //    }

                //    //Handle Pay Grade Category
                //    var payGradeCategoryFilter = query.GetFilterByName( "navy:PayGradeCategory" );
                //    if ( payGradeCategoryFilter != null && payGradeCategoryFilter.ItemIds?.Count() > 0 )
                //    {
                //        list = list.Where( s =>
                //            payGradeCategoryFilter.IsNegation ?
                //                !payGradeCategoryFilter.ItemIds.Contains( s.RankId ) :
                //                payGradeCategoryFilter.ItemIds.Contains( s.RankId )
                //        );
                //    }


                //    //Get total
                //    query.TotalResults = list.Count();

                //    //Sort
                //    list = list.OrderBy( p => p.Description );

                //    //Get page
                //    var results = list.Skip( skip ).Take( query.PageSize == -1 ? query.TotalResults : query.PageSize )
                //        .Where( m => m != null ).ToList();

                //    //Populate
                //    foreach ( var item in results )
                //    {
                //        var entity = new AppEntity();
                //        MapFromDB( item, entity, query.GetAllData, !query.GetAllData );
                //        output.Add( entity );
                //    }
                //}

                return output;
            }
            catch ( Exception ex )
            {
                return new List<AppEntity>() { new AppEntity() {  CodedNotation = "Error: " + ex.Message + " - " + ex.InnerException?.Message } };
            }
        }
        //

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
        /// Get all rating tasks for the provided rating
        /// </summary>
        /// <param name="ratingCodedNotation"></param>
        /// <param name="includingAllSailorsTasks">If true, include All Sailor tasks</param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize">Consider: if zero, return all records</param>
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
                // template += ",'ALL'";
            }
            //make configurable - this is still OK
            filter = string.Format( "base.id in (select a.[RatingContextId] from [RatingContext.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation in ({0}) )", template );
            var results = RMTLSearch( filter, orderBy, pageNumber, pageSize, userId, ref totalRows );

            //no clear difference between the convert and select?
            //for multiple training tasks, will have duplicate rows, so should still have one training task per rating task search result
            //list = results.ConvertAll( m => new AppEntity()
            //{
            //    CTID = m.CTID,
            //    RowId = m.RowId,
            //    HasRating = m.HasRating,      //not available-adding
            //    PayGradeType = m.PayGradeType,
            //    HasBilletTitle = m.HasBilletTitles,
            //    HasWorkRole = m.HasWorkRole,
            //    HasReferenceResource = m.HasReferenceResource,
            //    ReferenceType = m.ReferenceType,
            //    Description = m.RatingContext,
            //    CodedNotation = m.CodedNotation,
            //    ApplicabilityType = m.ApplicabilityType,
            //    TaskTrainingGap = m.TaskTrainingGap,
            //    HasTrainingTask = m.HasTrainingTask,

            //} ).ToList();

            AddToCache( list, ratingCodedNotation, includingAllSailorsTasks );

            return list;
        }
        //

        public static void MapFromDB( DBEntity input, AppEntity output, bool includingConcepts, bool isSearchContext = false )
        {
            //test automap
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );

            //if ( input.RatingTask_HasRating?.Count > 0 )
            //{
            //    foreach ( var item in input.RatingTask_HasRating )
            //    {
            //        if ( item.Rating?.RowId != null )
            //        {
            //            output.HasRating.Add( item.Rating.RowId );
            //            output.RatingTitles.Add( item.Rating.Name );
            //            output.CurrentRatingCode = item.Rating.CodedNotation;
            //            //
            //        }
            //    }
            //}
            //if ( input.RatingTask_HasJob?.Count > 0 )
            //{
            //    foreach ( var item in input.RatingTask_HasJob )
            //    {
            //        if ( item.Job?.RowId != null )
            //        {
            //            output.HasBilletTitle.Add( item.Job.RowId );
            //            output.BilletTitles.Add( item.Job.Name );
            //            output.BilletTitle = item.Job.Name;
            //        }
            //    }
            //}
            //if ( !isSearchContext && input.RatingTask_WorkRole?.Count > 0 )
            //{
            //    foreach ( var item in input.RatingTask_WorkRole )
            //    {
            //        if ( item.WorkRole?.RowId != null )
            //        {
            //            output.HasWorkRole.Add( item.WorkRole.RowId );
            //            output.FunctionalArea.Add( item.WorkRole.Name );
            //        }
            //    }
            //}
            //if ( !isSearchContext && input.RankId > 0 )
            //{
            //    ConceptSchemeManager.MapFromDB( input.ConceptScheme_Rank, output.TaskPayGrade );
            //    output.PayGradeType = ( output.TaskPayGrade )?.RowId ?? Guid.Empty;
            //}
            //if ( !isSearchContext && input.ReferenceResourceId > 0 )
            //{
            //    ReferenceResourceManager.MapFromDB( input.ToReferenceResource, output.ReferenceResource );
            //    output.HasReferenceResource = ( output.ReferenceResource )?.RowId ?? Guid.Empty;
            //}
            //if ( !isSearchContext && input.WorkElementTypeId > 0 )
            //{
            //    ConceptSchemeManager.MapFromDB( input.ConceptScheme_WorkElementType, output.TaskReferenceType );
            //    output.ReferenceType = ( output.TaskReferenceType )?.RowId ?? Guid.Empty;
            //}
            //if ( !isSearchContext && input.TaskApplicabilityId > 0 )
            //{
            //    ConceptSchemeManager.MapFromDB( input.ConceptScheme_Applicability, output.TaskApplicabilityType );
            //    output.ApplicabilityType = ( output.TaskApplicabilityType )?.RowId ?? Guid.Empty;
            //    //OR
            //    //output.ApplicabilityType = ConceptSchemeManager.MapConcept( input.ConceptScheme_Applicability )?.RowId ?? Guid.Empty;

            //}
            //if ( !isSearchContext && input.FormalTrainingGapId > 0 )
            //{
            //    ConceptSchemeManager.MapFromDB( input.ConceptScheme_TrainingGap, output.TaskTrainingGap );
            //    output.TrainingGapType = ( output.TaskTrainingGap )?.RowId ?? Guid.Empty;
            //    //OR
            //    //output.TrainingGapType = ConceptSchemeManager.MapConcept( input.ConceptScheme_TrainingGap )?.RowId ?? Guid.Empty;
            //}

            //if ( !isSearchContext && input.RatingTask_HasTrainingTask?.Count > 0 )
            //{
            //    foreach ( var item in input.RatingTask_HasTrainingTask )
            //    {
            //        if ( item.Course_Task?.RowId != null )
            //        {
            //            output.HasTrainingTask.Add( item.Course_Task.RowId );
            //            output.TrainingTasks.Add( TrainingTaskManager.MapFromDB( item.Course_Task ) );
            //        }
            //    }
            //}

            //if ( !isSearchContext && input.ClusterAnalysis?.Count > 0 )
            //{
            //    //should only be one
            //    foreach ( var item in input.ClusterAnalysis )
            //    {
            //        output.ClusterAnalysis = ClusterAnalysisManager.MapFromDB( item );
            //    }
            //}
        }

        /// <summary>
        /// assuming the rating code
        /// could use an or to also handle name
        /// </summary>
        /// <param name="rating"></param>
        /// <param name="pOrderBy"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="userId"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static List<EntitySummary> SearchForRating( string rating, string orderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows )
        {
            var keyword = HandleApostrophes( rating );
            string filter = String.Format( "base.id in (select a.[RatingContextId] from [RatingContext.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation = '{0}' OR b.name = '{0}' )", keyword );


            return RMTLSearch( filter, orderBy, pageNumber, pageSize, userId, ref pTotalRows );

        }

        public static List<EntitySummary> RMTLSearch( string pFilter, string pOrderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows, bool autocomplete = false )
        {
            string connectionString = DBConnectionRO();
            EntitySummary item = new EntitySummary();
            List<EntitySummary> list = new List<EntitySummary>();
            var result = new DataTable();

            if ( pageNumber < 1 )
                pageNumber = 1;
            if ( pageSize < 1 )
            {
                //if there is a filter, could allow getting all
                if ( !string.IsNullOrWhiteSpace( pFilter ) )
                {
                    //ensure stored proc can handle this - it can
                    pageSize = 0;
                }
            }

            using ( SqlConnection c = new SqlConnection( connectionString ) )
            {
                c.Open();

                if ( string.IsNullOrEmpty( pFilter ) )
                {
                    pFilter = "";
                }

                using ( SqlCommand command = new SqlCommand( "[RatingContextSearch]", c ) )
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add( new SqlParameter( "@Filter", pFilter ) );
                    command.Parameters.Add( new SqlParameter( "@SortOrder", pOrderBy ) );
                    command.Parameters.Add( new SqlParameter( "@StartPageIndex", pageNumber ) );
                    command.Parameters.Add( new SqlParameter( "@PageSize", pageSize ) );
                    command.Parameters.Add( new SqlParameter( "@CurrentUserId", userId ) );

                    SqlParameter totalRows = new SqlParameter( "@TotalRows", pTotalRows );
                    totalRows.Direction = ParameterDirection.Output;
                    command.Parameters.Add( totalRows );
                    try
                    {
                        using ( SqlDataAdapter adapter = new SqlDataAdapter() )
                        {
                            adapter.SelectCommand = command;
                            adapter.Fill( result );
                        }
                        string rows = command.Parameters[5].Value.ToString();
                        pTotalRows = Int32.Parse( rows );
                    }
                    catch ( Exception ex )
                    {
                        pTotalRows = 0;
                        LoggingHelper.LogError( ex, thisClassName + string.Format( ".Search() - Execute proc, Message: {0} \r\n Filter: {1} \r\n", ex.Message, pFilter ) );

                        item = new EntitySummary();
                        //item.RatingContext = "Unexpected error encountered. System administration has been notified. Please try again later. ";
                        //item.RatingContext = ex.Message;

                        list.Add( item );
                        return list;
                    }
                }

                try
                {
                    var resultNumber = 0;
                    foreach ( DataRow dr in result.Rows )
                    {
                        item = new EntitySummary();
                        resultNumber++;
                        item.ResultNumber = resultNumber;
                        item.Id = GetRowColumn( dr, "Id", 0 );
                        item.RowId = GetGuidType( dr, "RowId" );
                        //Ratings - coded notation
                        item.Ratings = dr["Ratings"].ToString();// GetRowColumn( dr, "Ratings", "" );
                        item.RatingName = dr["RatingName"].ToString();// GetRowColumn( dr, "RatingName", "" );
                        //do we need to populate HasRatings (if so, could include in the pipe separated list of Ratings)
                        //22-04-04 mp - search will now return a single HasRating
                        var hasRatings = GetGuidType( dr, "HasRating" );
                        if ( IsGuidValid( hasRatings ) )
                        {
                            item.HasRatings.Add( hasRatings );
                        }
                        //Hmm autocomplete is always false? Is this obsolete?
                        //if ( autocomplete )
                        //{
                        //    item.HasRatings = GetRatingGuids( item.Ratings );
                        //}
                        //BilletTitles
                        item.BilletTitles = dr["BilletTitles"].ToString();// GetRowColumn( dr, "BilletTitles", "" );
                        //var bt= GetRowColumn( dr, "BilletTitles", "" );
                        //could save previous and then first check the previous
                        //similarly, do we need a list of billet guids?
                        if ( autocomplete )
                            item.HasBilletTitles = GetBilletTitleGuids( item.BilletTitles );


                       //?????? item.RatingContext = dr["RatingContext"].ToString();// GetRowColumn( dr, "RatingContext", "" );
                        item.Note = dr["Notes"].ToString();// GetRowColumn( dr, "Notes", "" );
                        item.CTID = dr["CTID"].ToString();// GetRowPossibleColumn( dr, "CTID", "" );
                                                          //
                        item.Created = GetRowColumn( dr, "Created", DateTime.MinValue );
                        item.Creator = dr["CreatedBy"].ToString(); //GetRowPossibleColumn( dr, "CreatedBy","" );
                                                                   //item.CreatedBy = GetGuidType( dr, "CreatedByUID" );
                        item.LastUpdated = GetRowColumn( dr, "LastUpdated", DateTime.MinValue );
                        item.ModifiedBy = dr["ModifiedBy"].ToString(); //GetRowPossibleColumn( dr, "ModifiedBy", "" );
                                                                       //item.LastUpdatedBy = GetGuidType( dr, "ModifiedByUID" );

                        item.CodedNotation = dr["CodedNotation"].ToString();// GetRowPossibleColumn( dr, "CodedNotation", "" );
                                                                            //
                        item.Rank = dr["Rank"].ToString();
                        item.RankName = dr["RankName"].ToString();
                        item.PayGradeType = GetGuidType( dr, "PayGradeType" );
                        //
                        item.Level = dr["Level"].ToString();// GetRowPossibleColumn( dr, "Level", "" );
                        //FunctionalArea  - not a pipe separated list 
                        //22-01-23 - what to do about the HasWorkRole list. Could include and split out here?
                        item.FunctionalArea = dr["FunctionalArea"].ToString(); //GetRowColumn( dr, "FunctionalArea", "" );
                        if ( !string.IsNullOrWhiteSpace( item.FunctionalArea ) )
                        {
                            var workRoleList = "";
                            item.HasWorkRole = GetFunctionalAreas( item.FunctionalArea, ref workRoleList );
                            item.FunctionalArea = workRoleList;
                        }
                        //
                        //
                        //item.ReferenceResource = dr["ReferenceResource"].ToString().Trim();
                        item.ReferenceResource = dr["ReferenceResource"].ToString(); //GetRowColumn( dr, "ReferenceResource", "" );
                        item.SourceDate = dr["SourceDate"].ToString();// GetRowColumn( dr, "SourceDate", "" );
                        item.HasReferenceResource = GetGuidType( dr, "HasReferenceResource" );
                        //
                        item.WorkElementType = dr["WorkElementType"].ToString(); //GetRowPossibleColumn( dr, "WorkElementType", "" );
                        item.WorkElementTypeAlternateName = dr["WorkElementTypeAlternateName"].ToString();
                        item.ReferenceType = GetGuidType( dr, "ReferenceType" );
                        //
                        item.TaskApplicability = dr["TaskApplicability"].ToString().Trim();// GetRowPossibleColumn( dr, "TaskApplicability", "" );
                        item.ApplicabilityType = GetGuidType( dr, "ApplicabilityType" );
                        //
                        item.FormalTrainingGap = dr["FormalTrainingGap"].ToString();// GetRowPossibleColumn( dr, "FormalTrainingGap", "" );
                        item.TrainingGapType = GetGuidType( dr, "TrainingGapType" );

                        item.CIN = dr["CIN"].ToString();// GetRowColumn( dr, "CIN", "" );
                        item.CourseName = dr["CourseName"].ToString();// GetRowColumn( dr, "CourseName", "" );
                        item.CourseTypes = dr["CourseTypes"].ToString().Trim();// GetRowPossibleColumn( dr, "CourseType", "" );
                        item.CurrentAssessmentApproach = GetRowPossibleColumn( dr, "CurrentAssessmentApproach", "" );
                        //
                        item.TrainingTask = dr["TrainingTask"].ToString();// GetRowPossibleColumn( dr, "TrainingTask", "" );
                                                                          //item.HasTrainingTask = GetGuidType( dr, "HasTrainingTask" );


                        //
                        item.CurriculumControlAuthority = dr["CurriculumControlAuthority"].ToString().Trim();// GetRowPossibleColumn( dr, "CurriculumControlAuthority", "" );
                        item.LifeCycleControlDocument = dr["LifeCycleControlDocument"].ToString().Trim();// GetRowPossibleColumn( dr, "LifeCycleControlDocument", "" );
                        item.Notes = dr["Notes"].ToString();
                        //Part III
                        item.TrainingSolutionType = dr["TrainingSolutionType"].ToString().Trim(); ;
                        item.ClusterAnalysisTitle = dr["ClusterAnalysisTitle"].ToString().Trim(); ;
                        item.RecommendedModality = dr["RecommendedModality"].ToString().Trim(); ;
                        item.DevelopmentSpecification = dr["DevelopmentSpecification"].ToString().Trim(); ;

                        item.CandidatePlatform = dr["CandidatePlatform"].ToString().Trim(); ;
                        item.CFMPlacement = dr["CFMPlacement"].ToString().Trim(); ;
                        item.PriorityPlacement = GetRowPossibleColumn( dr, "PriorityPlacement", 0 );
                        item.DevelopmentRatio = dr["DevelopmentRatio"].ToString().Trim(); ;

                        item.EstimatedInstructionalTime = GetRowPossibleColumn( dr, "EstimatedInstructionalTime", 0.0M );
                        item.DevelopmentTime = GetRowPossibleColumn( dr, "DevelopmentTime", 0 );
                        item.ClusterAnalysisNotes = dr["ClusterAnalysisNotes"].ToString().Trim(); ;

                        list.Add( item );
                    }
                }
                catch ( Exception ex )
                {
                    LoggingHelper.DoTrace( 1, thisClassName + ".Search. Exception: " + ex.Message );
                }
                return list;

            }
        } //

        //
        public static List<AppEntity> CheckCache( string rating, bool includingAllSailorsTasks )
        {
            var cache = new CachedRatingContext();
            var list = new List<AppEntity>();
            var key = cacheKey + String.Format( "_{0}_{1}", rating, includingAllSailorsTasks );
            int cacheHours = 8;
            DateTime maxTime = DateTime.Now.AddHours( cacheHours * -1 );
            if ( MemoryCache.Default.Get( key ) != null && cacheHours > 0 )
            {
                cache = ( CachedRatingContext ) MemoryCache.Default.Get( key );
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
                var newCache = new CachedRatingContext()
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
        #endregion
    }

}
