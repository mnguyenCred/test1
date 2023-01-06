using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

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
				BasicSaveCore( context, entity, context.RatingContext, userID, ( ent, dbEnt ) => {
					dbEnt.RatingId = context.Rating.FirstOrDefault( m => m.RowId == ent.HasRating )?.Id ?? 0;
					dbEnt.BilletTitleId = context.Job.FirstOrDefault( m => m.RowId == ent.HasBilletTitle )?.Id;
					dbEnt.WorkRoleId = context.WorkRole.FirstOrDefault( m => m.RowId == ent.HasWorkRole )?.Id;
					dbEnt.RatingTaskId = context.RatingTask.FirstOrDefault( m => m.RowId == ent.HasRatingTask )?.Id ?? 0;
					dbEnt.CourseContextId = context.CourseContext.FirstOrDefault( m => m.RowId == ent.HasCourseContext )?.Id;
					dbEnt.ClusterAnalysisId = context.ClusterAnalysis.FirstOrDefault( m => m.RowId == ent.HasClusterAnalysis )?.Id;
					dbEnt.TaskApplicabilityId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.ApplicabilityType )?.Id;
					dbEnt.FormalTrainingGapId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.TrainingGapType )?.Id;
					dbEnt.PayGradeTypeId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.PayGradeType )?.Id ?? 0;
					//dbEnt.PayGradeLevelTypeId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.PayGradeLevelType )?.Id ?? 0;
					dbEnt.PayGradeLevelTypeId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.PayGradeType )?.BroadMatchId ?? 0; //Enforce correct value
				}, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

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
                        var record = GetByCodedNotationAndRatingId( input.CodedNotation, input.HasRatingId );
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
               // var currentRankId = output.RankId;
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
               // output.RankId = 0;
            }
            //TaskApplicability
            if ( IsValidGuid( input.ApplicabilityType ) )
            {
                output.TaskApplicabilityId = ( int ) ConceptManager.GetByRowId( input.ApplicabilityType )?.Id;
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
                output.FormalTrainingGapId = ( int ) ConceptManager.GetByRowId( input.TrainingGapType )?.Id;
            }
            //
           

        }
        //

        #endregion

        #region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.RatingContext, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

		public static AppEntity GetByCodedNotationAndRatingId( string codedNotation, int ratingID, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.CodedNotation?.ToLower() == codedNotation?.ToLower() && m.RatingId == ratingID, returnNullIfNotFound );
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

				foreach ( var item in results )
				{
					list.Add( MapFromDB( item, context ) );
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

				foreach( var item in results )
				{
					list.Add( MapFromDB( item, context ) );
				}
            }
            return list;
        }
        //

		public static AppEntity GetForUploadOrNull(
			Guid ratingRowID,
			Guid billetTitleRowID,
			Guid workRoleRowID,
			Guid ratingTaskRowID,
			Guid taskApplicabilityTypeRowID,
			Guid trainingGapTypeRowID,
			Guid payGradeTypeRowID,
			Guid payGradeLevelTypeRowID,
			Guid courseContextRowID,
			Guid clusterAnalysisRowID
		)
		{
			using ( var context = new DataEntities() )
			{
				var match = context.RatingContext.FirstOrDefault( m =>
					m.Rating.RowId == ratingRowID &&
					m.Job.RowId == billetTitleRowID &&
					m.WorkRole.RowId == workRoleRowID &&
					m.RatingTask.RowId == ratingTaskRowID &&
					context.ConceptScheme_Concept.FirstOrDefault( n => n.RowId == taskApplicabilityTypeRowID && m.TaskApplicabilityId == n.Id ) != null &&
					context.ConceptScheme_Concept.FirstOrDefault( n => n.RowId == trainingGapTypeRowID && m.FormalTrainingGapId == n.Id ) != null &&
					context.ConceptScheme_Concept.FirstOrDefault( n => n.RowId == payGradeTypeRowID && m.PayGradeTypeId == n.Id ) != null &&
					context.ConceptScheme_Concept.FirstOrDefault( n => n.RowId == payGradeLevelTypeRowID && m.PayGradeLevelTypeId == n.Id ) != null &&
					( courseContextRowID == Guid.Empty ? true : m.CourseContext.RowId == courseContextRowID ) && //May be null
					( clusterAnalysisRowID == Guid.Empty ? true : m.ClusterAnalysis.RowId == clusterAnalysisRowID ) //May be null
				);

				if( match != null )
				{
					return MapFromDB( match, context );
				}
			}

			return null;
		}
		//

        public static SearchResultSet<AppEntity> Search( SearchQuery query )
        {
			var ratingTaskCount = 0;
			var resultSet = HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start Query
				var list = context.RatingContext.AsQueryable();

				//Handle Keywords
				var keywords = GetSanitizedSearchFilterKeywords( query );
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Rating.Name.Contains( keywords ) ||
						m.Rating.CodedNotation.Contains( keywords ) ||
						m.RatingTask.Description.Contains( keywords ) ||
						m.CourseContext.TrainingTask.Description.Contains( keywords ) ||
						m.ConceptScheme_Concept_TrainingGapType.Name.Contains( keywords )
					);
				}

				//Handle Filters
				//RMTL Search
				AppendIDsFilterIfPresent( query, "> FormalTrainingGapId > Concept", ids => {
					list = list.Where( m => ids.Contains( m.FormalTrainingGapId ?? 0 ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> RatingTaskId > RatingTask > ReferenceResourceTypeId > Concept", ids => {
					list = list.Where( m => ids.Contains( m.RatingTask.ConceptScheme_Concept_ReferenceType.Id ) );
				} );

				//Rating Detail Page
				//RMTL Search
				AppendIDsFilterIfPresent( query, "> RatingId > Rating", ids => {
					list = list.Where( m => ids.Contains( m.RatingId ) );
				} );

				//Billet Title Detail Page
				//RMTL Search
				AppendIDsFilterIfPresent( query, "> BilletTitleId > Job", ids => {
					list = list.Where( m => ids.Contains( m.BilletTitleId ?? 0 ) );
				} );

				//Cluster Analysis Detail Page
				AppendIDsFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis", ids => {
					list = list.Where( m => ids.Contains( m.ClusterAnalysisId ?? 0 ) );
				} );

				//Rating Task Detail Page
				AppendNotNullFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis:NotNull", () => {
					list = list.Where( m => m.ClusterAnalysisId != null );
				} );

				//Cluster Analysis Title Detail Page
				//RMTL Search
				AppendIDsFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > HasClusterAnalysisTitleId > ClusterAnalysisTitle", ids => {
					list = list.Where( m => ids.Contains( m.ClusterAnalysis.HasClusterAnalysisTitleId ?? 0 ) );
				} );

				//Course Detail Page
				//RMTL Search
				AppendIDsFilterIfPresent( query, "> CourseContextId > CourseContext > HasCourseId > Course", ids => {
					list = list.Where( m => ids.Contains( m.CourseContext.HasCourseId ) );
				} );

				//Course Context Detail Page
				AppendIDsFilterIfPresent( query, "> CourseContextId > CourseContext", ids => {
					list = list.Where( m => ids.Contains( m.CourseContextId ?? 0 ) );
				} );

				//Organization Detail Page
				AppendIDsFilterIfPresent( query, "> CourseContextId > CourseContext > HasCourseId > Course > CurriculumControlAuthorityId > Organization", ids => {
					list = list.Where( m => ids.Contains( m.CourseContext.Course.CurriculumControlAuthorityId ?? 0 ) );
				} );

				//Rating Task Detail Page
				AppendIDsFilterIfPresent( query, "> RatingTaskId > RatingTask", ids => {
					list = list.Where( m => ids.Contains( m.RatingTaskId ) );
				} );

				//RMTL Search
				AppendTextFilterIfPresent( query, "> RatingTaskId > RatingTask.TextFields", text => {
					list = list.Where( m => m.RatingTask.Description.Contains( text ) );
				} );

				//Reference Resource Detail Page
				//RMTL Search
				AppendIDsFilterIfPresent( query, "> RatingTaskId > RatingTask > ReferenceResourceId > ReferenceResource", ids => {
					list = list.Where( m => ids.Contains( m.RatingTask.ReferenceResourceId ?? 0 ) );
				} );

				//Training Task Detail Page
				AppendIDsFilterIfPresent( query, "> CourseContextId > CourseContext > HasTrainingTaskId > TrainingTask", ids => {
					list = list.Where( m => ids.Contains( m.CourseContext.HasTrainingTaskId ) );
				} );

				//Rating Task Detail Page
				AppendNotNullFilterIfPresent( query, "> CourseContextId > CourseContext:NotNull", () => {
					list = list.Where( m => m.CourseContextId != null );
				} );

				//RMTL Search
				AppendTextFilterIfPresent( query, "> CourseContextId > CourseContext > HasTrainingTaskId > TrainingTask.TextFields", text => {
					list = list.Where( m => m.CourseContext.TrainingTask.Description.Contains( text ) );
				} );

				//Work Role Detail Page
				//RMTL Search
				AppendIDsFilterIfPresent( query, "> WorkRoleId > WorkRole", ids => {
					list = list.Where( m => ids.Contains( m.WorkRoleId ?? 0 ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> PayGradeTypeId > Concept", ids =>
				{
					list = list.Where( m => ids.Contains( m.PayGradeTypeId ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> PayGradeLevelTypeId > Concept", ids =>
				{
					list = list.Where( m => ids.Contains( m.PayGradeLevelTypeId ?? 0 ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> FormalTrainingGapId > Concept", ids =>
				{
					list = list.Where( m => ids.Contains( m.FormalTrainingGapId ?? 0 ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> TaskApplicabilityId > Concept", ids =>
				{
					list = list.Where( m => ids.Contains( m.TaskApplicabilityId ?? 0 ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> CourseContextId > CourseContext > AssessmentMethodConceptId > Concept", ids => {
					list = list.Where( m => m.CourseContext.CourseContext_AssessmentType.Select( n => n.AssessmentMethodConceptId ).Intersect( ids ).Count() > 0 );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> CourseContextId > CourseContext > HasCourseId > Course > CourseTypeConceptId > Concept", ids => {
					list = list.Where( m => m.CourseContext.Course.Course_CourseType.Select( n => n.CourseTypeConceptId ).Intersect( ids ).Count() > 0 );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> CourseContextId > CourseContext > HasCourseId > Course > LifeCycleControlDocumentTypeId > Concept", ids =>
				{
					list = list.Where( m => ids.Contains( m.CourseContext.Course.LifeCycleControlDocumentTypeId ?? 0 ) );
				} );

				//RMTL Search
				AppendTextFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis.PriorityPlacement", text => {
					list = list.Where( m => ( m.ClusterAnalysis.PriorityPlacement ?? 0 ).ToString().Contains( text ) );
				} );

				//RMTL Search
				AppendTextFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis.EstimatedInstructionalTime", text => {
					list = list.Where( m => ( m.ClusterAnalysis.EstimatedInstructionalTime ?? 0 ).ToString().Contains( text ) );
				} );

				//RMTL Search
				AppendTextFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis.DevelopmentTime", text => {
					list = list.Where( m => ( m.ClusterAnalysis.DevelopmentTime ?? 0 ).ToString().Contains( text ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > TrainingSolutionTypeId > Concept", ids => {
					list = list.Where( m => ids.Contains( m.ClusterAnalysis.TrainingSolutionTypeId ?? 0 ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > RecommendedModalityTypeId > Concept", ids => {
					list = list.Where( m => ids.Contains( m.ClusterAnalysis.RecommendedModalityTypeId ?? 0 ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > DevelopmentSpecificationTypeId > Concept", ids => {
					list = list.Where( m => ids.Contains( m.ClusterAnalysis.DevelopmentSpecificationTypeId ?? 0 ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > CandidatePlatformConceptId > Concept", ids => {
					list = list.Where( m => m.ClusterAnalysis.ClusterAnalysis_HasCandidatePlatform.Select( n => n.CandidatePlatformConceptId ).Intersect( ids ).Count() > 0 );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > DevelopmentRatioTypeId > Concept", ids => {
					list = list.Where( m => ids.Contains( m.ClusterAnalysis.DevelopmentRatioTypeId ?? 0 ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > CFMPlacementTypeId > Concept", ids => {
					list = list.Where( m => ids.Contains( m.ClusterAnalysis.CFMPlacementTypeId ?? 0 ) );
				} );

				//Concept Detail Page
				//ConceptManager.DeleteById
				AppendIDsFilterIfPresent( query, "search:AllConceptPaths", ids => {
					list = list.Where( m =>
						m.RatingTask.ReferenceResource.ReferenceResource_ReferenceType.Select( n => n.ReferenceTypeId ).Intersect( ids ).Count() > 0 ||
						ids.Contains( m.TaskApplicabilityId ?? 0 ) ||
						ids.Contains( m.FormalTrainingGapId ?? 0 ) ||
						ids.Contains( m.PayGradeTypeId ) ||
						ids.Contains( m.PayGradeLevelTypeId ?? 0) ||
						m.CourseContext.CourseContext_AssessmentType.Select( n => n.AssessmentMethodConceptId ).Intersect( ids ).Count() > 0 ||
						m.CourseContext.Course.Course_CourseType.Select( n => n.CourseTypeConceptId ).Intersect( ids ).Count() > 0 ||
						ids.Contains( m.CourseContext.Course.LifeCycleControlDocumentTypeId ?? 0 ) ||
						ids.Contains( m.ClusterAnalysis.TrainingSolutionTypeId ?? 0 ) ||
						ids.Contains( m.ClusterAnalysis.RecommendedModalityTypeId ?? 0 ) ||
						ids.Contains( m.ClusterAnalysis.DevelopmentSpecificationTypeId ?? 0 ) ||
						m.ClusterAnalysis.ClusterAnalysis_HasCandidatePlatform.Select( n => n.CandidatePlatformConceptId ).Intersect( ids ).Count() > 0 ||
						ids.Contains( m.ClusterAnalysis.DevelopmentRatioTypeId ?? 0 ) ||
						ids.Contains( m.ClusterAnalysis.CFMPlacementTypeId ?? 0 )
					);
				} );

				//Concept Scheme Detail Page
				AppendIDsFilterIfPresent( query, "search:AllConceptSchemePaths", ids => {
					var conceptIDs = ids.Select( m => ConceptSchemeManager.GetById( m ) ).SelectMany( m => m.Concepts ).Select( m => m.Id ).ToList();
					list = list.Where( m =>
						m.RatingTask.ReferenceResource.ReferenceResource_ReferenceType.Select( n => n.ReferenceTypeId ).Intersect( conceptIDs ).Count() > 0 ||
						conceptIDs.Contains( m.TaskApplicabilityId ?? 0 ) ||
						conceptIDs.Contains( m.FormalTrainingGapId ?? 0 ) ||
						conceptIDs.Contains( m.PayGradeTypeId ) ||
						conceptIDs.Contains( m.PayGradeLevelTypeId ?? 0 ) ||
						m.CourseContext.CourseContext_AssessmentType.Select( n => n.AssessmentMethodConceptId ).Intersect( conceptIDs ).Count() > 0 ||
						m.CourseContext.Course.Course_CourseType.Select( n => n.CourseTypeConceptId ).Intersect( conceptIDs ).Count() > 0 ||
						conceptIDs.Contains( m.CourseContext.Course.LifeCycleControlDocumentTypeId ?? 0 ) ||
						conceptIDs.Contains( m.ClusterAnalysis.TrainingSolutionTypeId ?? 0 ) ||
						conceptIDs.Contains( m.ClusterAnalysis.RecommendedModalityTypeId ?? 0 ) ||
						conceptIDs.Contains( m.ClusterAnalysis.DevelopmentSpecificationTypeId ?? 0 ) ||
						m.ClusterAnalysis.ClusterAnalysis_HasCandidatePlatform.Select( n => n.CandidatePlatformConceptId ).Intersect( conceptIDs ).Count() > 0 ||
						conceptIDs.Contains( m.ClusterAnalysis.DevelopmentRatioTypeId ?? 0 ) ||
						conceptIDs.Contains( m.ClusterAnalysis.CFMPlacementTypeId ?? 0 )
					);
				} );

				//Count the number of Rating Tasks associated with these RatingContexts, as this count is used on the RMTL Search and on most detail pages
				ratingTaskCount = list.Select( m => m.RatingTask.Id ).Distinct().Count();

				//Custom handling for the multi-column-based sorting in the RMTL search
				if( query.SortOrder?.FirstOrDefault( m => m.Column == "sortOrder:RMTLSearchSortHandler" ) != null )
				{
					var sorted = list.OrderBy( m => m != null );
					foreach( var sortItem in query.SortOrder.Where( m => m.Column != "sortOrder:RMTLSearchSortHandler" ).ToList() )
					{
						switch ( sortItem.Column )
						{
							case "> RowId": sorted = SortAscOrDesc( sorted, sortItem, m => m.Id ); break;
							case "> HasRating > Rating > CodedNotation": sorted = SortAscOrDesc( sorted, sortItem, m => m.Rating.CodedNotation ); break;
							case "> PayGradeType > Concept > CodedNotation": sorted = SortAscOrDesc( sorted, sortItem, m => m.ConceptScheme_Concept_PayGradeType.CodedNotation ); break;
							case "> PayGradeLevelType > Concept > CodedNotation": sorted = SortAscOrDesc( sorted, sortItem, m => m.ConceptScheme_Concept_PayGradeLevelType.CodedNotation ); break;
							case "> HasBilletTitle > BilletTitle > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.Job.Name ); break;
							case "> HasWorkRole > WorkRole > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.WorkRole.Name ); break;
							case "> HasRatingTask > RatingTask > HasReferenceResource > ReferenceResource > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.RatingTask.ReferenceResource.Name ); break;
							case "> HasRatingTask > RatingTask > HasReferenceResource > ReferenceResource > PublicationDate": sorted = SortAscOrDesc( sorted, sortItem, m => m.RatingTask.ReferenceResource.PublicationDate ); break;
							case "> HasRatingTask > RatingTask > ReferenceType > Concept > WorkElementType": sorted = SortAscOrDesc( sorted, sortItem, m => m.RatingTask.ConceptScheme_Concept_ReferenceType.Name ); break;
							case "> HasRatingTask > RatingTask > Description": sorted = SortAscOrDesc( sorted, sortItem, m => m.RatingTask.Description ); break;
							case "> ApplicabilityType > Concept > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.ConceptScheme_Concept_TaskApplicabilityType.Name ); break;
							case "> TrainingGapType > Concept > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.ConceptScheme_Concept_TrainingGapType.Name ); break;
							case "> HasCourseContext > CourseContext > RowId": sorted = SortAscOrDesc( sorted, sortItem, m => m.CourseContext.Id ); break;
							case "> HasCourseContext > CourseContext > HasCourse > Course > CodedNotation": sorted = SortAscOrDesc( sorted, sortItem, m => m.CourseContext.Course.CodedNotation ); break;
							case "> HasCourseContext > CourseContext > HasCourse > Course > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.CourseContext.Course.Name ); break;
							case "> HasCourseContext > CourseContext > HasCourse > Course > CourseType > Concept > Name":
								sorted = sortItem.Ascending ?
									SortAscOrDesc( sorted, sortItem, m => m.CourseContext.Course.Course_CourseType.Select( n => n.ConceptScheme_Concept_CourseType.Name ).OrderBy( n => n ).FirstOrDefault() ) :
									SortAscOrDesc( sorted, sortItem, m => m.CourseContext.Course.Course_CourseType.Select( n => n.ConceptScheme_Concept_CourseType.Name ).OrderByDescending( n => n ).FirstOrDefault() );
								break;
							case "> HasCourseContext > CourseContext > HasCourse > Course > CurriculumControlAuthority > Organization > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.CourseContext.Course.Organization.Name ); break;
							case "> HasCourseContext > CourseContext > HasCourse > Course > LifeCycleControlDocumentType > Concept > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.CourseContext.Course.ConceptScheme_Concept.Name ); break;
							case "> HasCourseContext > CourseContext > HasTrainingTask > TrainingTask > Description": sorted = SortAscOrDesc( sorted, sortItem, m => m.CourseContext.TrainingTask.Description ); break;
							case "> HasCourseContext > CourseContext > AssessmentMethodType > Concept > Name":
								sorted = sortItem.Ascending ?
									SortAscOrDesc( sorted, sortItem, m => m.CourseContext.CourseContext_AssessmentType.Select( n => n.ConceptScheme_Concept.Name ).OrderBy( n => n ).FirstOrDefault() ) :
									SortAscOrDesc( sorted, sortItem, m => m.CourseContext.CourseContext_AssessmentType.Select( n => n.ConceptScheme_Concept.Name ).OrderByDescending( n => n ).FirstOrDefault() );
								break;
							case "> HasClusterAnalysis > ClusterAnalysis > TrainingSolutionType > Concept > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.ConceptScheme_Concept_TrainingSolutionType.Name ); break;
							case "> HasClusterAnalysis > ClusterAnalysis > RowId": sorted = SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.Id ); break;
							case "> HasClusterAnalysis > ClusterAnalysis > HasClusterAnalysisTitle > ClusterAnalysisTitle > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.ClusterAnalysisTitle.Name ); break;
							case "> HasClusterAnalysis > ClusterAnalysis > RecommendedModalityType > Concept > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.ConceptScheme_Concept_RecommendedModalityType.Name ); break;
							case "> HasClusterAnalysis > ClusterAnalysis > DevelopmentSpecificationType > Concept > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.ConceptScheme_Concept_DevelopmentSpecificationType.Name ); break;
							case "> HasClusterAnalysis > ClusterAnalysis > CandidatePlatformType > Concept > CodedNotation": 
								sorted = sortItem.Ascending ?
									SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.ClusterAnalysis_HasCandidatePlatform.Select( n => n.ConceptScheme_Concept.CodedNotation ).OrderBy( n => n ).FirstOrDefault() ) :
									SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.ClusterAnalysis_HasCandidatePlatform.Select( n => n.ConceptScheme_Concept.CodedNotation ).OrderByDescending( n => n ).FirstOrDefault() );
								break;
							case "> HasClusterAnalysis > ClusterAnalysis > CFMPlacementType > Concept > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.ConceptScheme_Concept_CFMPlacementType.Name ); break;
							case "> HasClusterAnalysis > ClusterAnalysis > PriorityPlacement": sorted = SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.PriorityPlacement ); break;
							case "> HasClusterAnalysis > ClusterAnalysis > DevelopmentRatioType > Concept > Name": sorted = SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.ConceptScheme_Concept_DevelopmentRatioType.Name ); break;
							case "> HasClusterAnalysis > ClusterAnalysis > EstimatedInstructionalTime": sorted = SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.EstimatedInstructionalTime ); break;
							case "> HasClusterAnalysis > ClusterAnalysis > DevelopmentTime": sorted = SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.DevelopmentTime ); break;
							case "> Notes": sorted = SortAscOrDesc( sorted, sortItem, m => m.Notes ); break;
							default: break;
						}
					}

					return sorted;
				}
				//Or normal sort handling
				else
				{
					//Return ordered list
					return HandleSort( list, query.SortOrder, m => m.RatingTask.Description, m => { 
						var noGapID = context.ConceptScheme_Concept.FirstOrDefault( n => n.Name.ToLower() == "no" )?.Id ?? 0;
						return m.OrderBy( n => n.FormalTrainingGapId == noGapID ).ThenBy( n => n.Rating.CodedNotation ).ThenBy( n => n.Job.Name ).ThenBy( n => n.WorkRole.Name ).ThenBy( n => n.RatingTask.Description ); 
					} );
				}

			}, MapFromDBForSearch );

			resultSet.ExtraData.Add( "RatingTaskCount", ratingTaskCount );
			return resultSet;
        }
		private static IOrderedQueryable<DBEntity> SortAscOrDesc( IOrderedQueryable<DBEntity> sorted, SortOrderItem sortItem, Expression<Func<DBEntity, object>> SortBy )
		{
			return sortItem.Ascending ? sorted.ThenBy( SortBy ) : sorted.ThenByDescending( SortBy );
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

		public static AppEntity MapFromDB( DBEntity input, DataEntities context )
		{
			return MapFromDBForSearch( input, context, null );
		}
		//

		public static AppEntity MapFromDBForSearch( DBEntity input, DataEntities context, SearchResultSet<AppEntity> resultSet = null )
		{
			var output = AutoMap( input, new AppEntity() );
			output.HasRating = input.Rating?.RowId ?? Guid.Empty;
			output.HasBilletTitle = input.Job?.RowId ?? Guid.Empty;
			output.HasWorkRole = input.WorkRole?.RowId ?? Guid.Empty;
			output.HasRatingTask = input.RatingTask?.RowId ?? Guid.Empty;
			output.HasCourseContext = input.CourseContext?.RowId ?? Guid.Empty;
			output.HasClusterAnalysis = input.ClusterAnalysis?.RowId ?? Guid.Empty;
			output.ApplicabilityType = input.ConceptScheme_Concept_TaskApplicabilityType?.RowId ?? Guid.Empty;
			output.TrainingGapType = input.ConceptScheme_Concept_TrainingGapType?.RowId ?? Guid.Empty;
			output.PayGradeType = input.ConceptScheme_Concept_PayGradeType?.RowId ?? Guid.Empty;
			output.PayGradeLevelType = input.ConceptScheme_Concept_PayGradeLevelType?.RowId ?? Guid.Empty;

			//If available, also append related resources
			if ( resultSet != null )
			{
				MapAndAppendResourceIfNotNull( input.RatingTask, context, RatingTaskManager.MapFromDB, resultSet );
				MapAndAppendResourceIfNotNull( input.CourseContext, context, CourseContextManager.MapFromDB, resultSet );
				MapAndAppendResourceIfNotNull( input.CourseContext?.TrainingTask, context, TrainingTaskManager.MapFromDB, resultSet );
			}

			return output;
		}
		//

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
