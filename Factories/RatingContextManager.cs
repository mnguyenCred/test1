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
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.RatingContext;
using ViewContext = Data.Views.ceNavyViewEntities;
namespace Factories
{
	public class RatingContextManager : BaseFactory
	{
		public static new string thisClassName = "RatingContextManager";
		public static string cacheKey = "RatingContextCache";

		#region === Persistence ==================
		public static void SaveFromUpload( AppEntity entity, int userID, ChangeSummary summary )
		{
			SaveCore( entity, userID, "Upload", summary.AddError );
		}
		//

		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			//Validate required fields
			AddErrorIf( errors, entity.HasRating == Guid.Empty, "A Rating must be selected." );
			AddErrorIf( errors, entity.HasBilletTitle == Guid.Empty, "A Billet Title must be selected." );
			AddErrorIf( errors, entity.HasWorkRole == Guid.Empty, "A Functional Area must be selected." );
			AddErrorIf( errors, entity.HasRatingTask == Guid.Empty, "A Rating Task must be selected." );
			AddErrorIf( errors, entity.ApplicabilityType == Guid.Empty, "An Applicability Type must be selected." );
			AddErrorIf( errors, entity.TrainingGapType == Guid.Empty, "A Training Gap Type must be selected." );
			AddErrorIf( errors, entity.PayGradeType == Guid.Empty, "A Pay Grade Type must be selected." );
			//Fine to not have a Course Context
			//Fine to not have a Cluster Analysis

			//TBD - Add duplicate check (probably not necessary?)

			//Return if any errors
			if( errors.Count() > 0 )
			{
				return;
			}

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

		public static DeleteResult DeleteById( int id )
		{
			return BasicDeleteCore( "Rating Context", context => context.RatingContext, id, "", ( context, list, target ) =>
			{
				//Nothing else references a Rating Context, so just return null
				return null;
			} );
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
			return GetItemList( context => context.RatingContext.OrderBy( m => m.Id ), MapFromDB, false );
        }
        //

        public static List<AppEntity> GetMultiple( List<Guid> guids )
        {
			return GetMultipleByFilter( context => context.RatingContext, m => guids.Contains( m.RowId ), m => m.Id, false, MapFromDB, false );
        }
        //

		public static List<int> GetAllPayGradeIDsForRatingTaskByID( int ratingTaskID, int exceptForPayGradeTypeId )
		{
			using( var context = new DataEntities() )
			{
				return context.RatingContext.Where( m => m.RatingTaskId == ratingTaskID && m.PayGradeTypeId != exceptForPayGradeTypeId ).Select( m => m.PayGradeTypeId ).Distinct().ToList();
			}
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
			var ratingTaskTokens = new List<string>();
			var trainingTaskTokens = new List<string>();
			var keywords = GetSanitizedSearchFilterKeywords( query );
			var keywordTokens = GetRelevanceTokens( keywords );
			var noGapID = 0;

			var resultSet = HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//context.Database.Log = ( message ) => System.Diagnostics.Debug.WriteLine( message );
				noGapID = context.ConceptScheme_Concept.FirstOrDefault( n => n.Name.ToLower() == "no" )?.Id ?? 0;

				//Start Query
				var list = context.RatingContext.AsQueryable();
				//var list = context.RatingContext.Include( nameof( DBEntity.Rating ) ).Include( nameof( DBEntity.RatingTask ) ).AsQueryable();

				//Handle Keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Notes.Contains( keywords ) ||
						m.Rating.Name.Contains( keywords ) ||
						m.Rating.CodedNotation.Contains( keywords ) ||
						m.RatingTask.Description.Contains( keywords ) ||
						m.CourseContext.TrainingTask.Description.Contains( keywords ) ||
						m.ConceptScheme_Concept_TrainingGapType.Name.Contains( keywords )
					);
				}

				//Handle Filters
				//Detail Pages
				AppendIDsFilterIfPresent( query, ".Id", ids => {
					list = list.Where( m => ids.Contains( m.Id ) );
				} );

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

				//Detail Pages
				AppendTextFilterIfPresent( query, "> RatingId > Rating.CodedNotation", text => {
					list = list.Where( m => m.Rating.CodedNotation.Contains( text ) );
				} );

				//Detail Pages
				AppendTextFilterIfPresent( query, "> RatingId > Rating.Name", text => {
					list = list.Where( m => m.Rating.Name.Contains( text ) );
				} );

				//Billet Title Detail Page
				//RMTL Search
				AppendIDsFilterIfPresent( query, "> BilletTitleId > Job", ids => {
					list = list.Where( m => ids.Contains( m.BilletTitleId ?? 0 ) );
				} );

				//Detail Pages
				AppendTextFilterIfPresent( query, "> BilletTitleId > Job.Name", text => {
					list = list.Where( m => m.Job.Name.Contains( text ) );
				} );

				//Cluster Analysis Detail Page
				AppendIDsFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis", ids => {
					list = list.Where( m => ids.Contains( m.ClusterAnalysisId ?? 0 ) );
				} );

				//Rating Task Detail Page
				//RMTL Search
				AppendSimpleFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis:NotNull", () => {
					list = list.Where( m => m.ClusterAnalysisId != null );
				} );

				//RMTL Search
				AppendSimpleFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis:IsNull", () => {
					list = list.Where( m => m.ClusterAnalysisId == 0 || m.ClusterAnalysisId == null );
				} );

				//Cluster Analysis Title Detail Page
				//RMTL Search
				AppendIDsFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > HasClusterAnalysisTitleId > ClusterAnalysisTitle", ids => {
					list = list.Where( m => ids.Contains( m.ClusterAnalysis.HasClusterAnalysisTitleId ?? 0 ) );
				} );

				//Detail Pages
				AppendTextFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > HasClusterAnalysisTitleId > ClusterAnalysisTitle.Name", text => {
					list = list.Where( m => m.ClusterAnalysis.ClusterAnalysisTitle.Name.Contains( text ) );
				} );

				//Course Detail Page
				//RMTL Search
				AppendIDsFilterIfPresent( query, "> CourseContextId > CourseContext > HasCourseId > Course", ids => {
					list = list.Where( m => ids.Contains( m.CourseContext.HasCourseId ) );
				} );

				//Detail Pages
				AppendTextFilterIfPresent( query, "> CourseContextId > CourseContext > HasCourseId > Course.Name", text => {
					list = list.Where( m => m.CourseContext.Course.Name.Contains( text ) );
				} );

				//Detail Pages
				AppendTextFilterIfPresent( query, "> CourseContextId > CourseContext > HasCourseId > Course.CodedNotation", text => {
					list = list.Where( m => m.CourseContext.Course.CodedNotation.Contains( text ) );
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
					ratingTaskTokens = GetRelevanceTokens( text );
				} );

				//Reference Resource Detail Page
				//RMTL Search
				AppendIDsFilterIfPresent( query, "> RatingTaskId > RatingTask > ReferenceResourceId > ReferenceResource", ids => {
					list = list.Where( m => ids.Contains( m.RatingTask.ReferenceResourceId ?? 0 ) );
				} );

				//Training Task Detail Page
				AppendTextFilterIfPresent( query, "> RatingTaskId > RatingTask > ReferenceResourceId > ReferenceResource.PublicationDate", text => {
					list = list.Where( m => m.RatingTask.ReferenceResource.PublicationDate.Contains( text ) );
				} );

				//Training Task Detail Page
				AppendTextFilterIfPresent( query, "> RatingTaskId > RatingTask > ReferenceResourceId > ReferenceResource.Name", text => {
					list = list.Where( m => m.RatingTask.ReferenceResource.Name.Contains( text ) );
				} );

				//Training Task Detail Page
				AppendIDsFilterIfPresent( query, "> CourseContextId > CourseContext > HasTrainingTaskId > TrainingTask", ids => {
					list = list.Where( m => ids.Contains( m.CourseContext.HasTrainingTaskId ) );
				} );

				//Training Task Detail Page
				AppendIDsFilterIfPresent( query, "> CourseContextId > CourseContext > HasTrainingTaskId > TrainingTask:Exclude", ids => {
					list = list.Where( m => !ids.Contains( m.CourseContext.HasTrainingTaskId ) );
				} );

				//RMTL Search
				AppendTextFilterIfPresent( query, "> CourseContextId > CourseContext > HasTrainingTaskId > TrainingTask:DescriptionExact", text => {
					list = list.Where( m => m.CourseContext.TrainingTask.Description.ToLower() == text.ToLower() );
				} );

				//Rating Task Detail Page
				//RMTL Search
				AppendSimpleFilterIfPresent( query, "> CourseContextId > CourseContext:NotNull", () => {
					list = list.Where( m => m.CourseContextId != null );
				} );

				//RMTL Search
				AppendSimpleFilterIfPresent( query, "> CourseContextId > CourseContext:IsNull", () => {
					list = list.Where( m => m.CourseContextId == 0 || m.CourseContextId == null );
				} );

				//RMTL Search
				AppendTextFilterIfPresent( query, "> CourseContextId > CourseContext > HasTrainingTaskId > TrainingTask.TextFields", text => {
					list = list.Where( m => m.CourseContext.TrainingTask.Description.Contains( text ) );
					trainingTaskTokens = GetRelevanceTokens( text );
				} );

				//Work Role Detail Page
				//RMTL Search
				AppendIDsFilterIfPresent( query, "> WorkRoleId > WorkRole", ids => {
					list = list.Where( m => ids.Contains( m.WorkRoleId ?? 0 ) );
				} );

				//Detail Pages
				AppendTextFilterIfPresent( query, "> WorkRoleId > WorkRole.Name", text => {
					list = list.Where( m => m.WorkRole.Name.Contains( text ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> PayGradeTypeId > Concept", ids => {
					list = list.Where( m => ids.Contains( m.PayGradeTypeId ) );
				} );

				//Detail Pages
				AppendTextFilterIfPresent( query, "> PayGradeTypeId > Concept.TextFields", text => {
					list = list.Where( m => m.ConceptScheme_Concept_PayGradeType.Name.Contains( text ) || m.ConceptScheme_Concept_PayGradeType.CodedNotation.Contains( text ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> PayGradeTypeId > Concept.ExclusiveToRatingTask", ids =>
				{
					list = list.Where( m =>
						ids.Contains( m.PayGradeTypeId ) &&
						context.RatingContext.Where( n => !ids.Contains( n.PayGradeTypeId ) && n.RatingTaskId == m.RatingTaskId ).Count() == 0
					);
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> PayGradeTypeId > Concept.MultipleForRatingTask", ids =>
				{
					list = list.Where( m =>
						context.RatingContext.Where( n => n.PayGradeTypeId != m.PayGradeTypeId && n.RatingTaskId == m.RatingTaskId ).Count() > 0
					);
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> PayGradeLevelTypeId > Concept", ids => {
					list = list.Where( m => ids.Contains( m.PayGradeLevelTypeId ?? 0 ) );
				} );

				//Detail Page
				AppendTextFilterIfPresent( query, "> PayGradeLevelTypeId > Concept.TextFields", text => {
					list = list.Where( m => m.ConceptScheme_Concept_PayGradeLevelType.Name.Contains( text ) || m.ConceptScheme_Concept_PayGradeLevelType.CodedNotation.Contains( text ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> PayGradeLevelTypeId > Concept.ExclusiveToRatingTask", ids =>
				{
					list = list.Where( m =>
						ids.Contains( m.PayGradeLevelTypeId ?? 0 ) &&
						context.RatingContext.Where( n => !ids.Contains( n.PayGradeLevelTypeId ?? 0 ) && n.RatingTaskId == m.RatingTaskId ).Count() == 0
					);
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> PayGradeLevelTypeId > Concept.MultipleForRatingTask", ids =>
				{
					list = list.Where( m =>
						context.RatingContext.Where( n => n.PayGradeLevelTypeId != m.PayGradeLevelTypeId && n.RatingTaskId == m.RatingTaskId ).Count() > 0
					);
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> FormalTrainingGapId > Concept", ids =>
				{
					list = list.Where( m => ids.Contains( m.FormalTrainingGapId ?? 0 ) );
				} );

				//Detail Pages
				AppendTextFilterIfPresent( query, "> FormalTrainingGapId > Concept.Name", text =>
				{
					list = list.Where( m => m.ConceptScheme_Concept_TrainingGapType.Name.Contains( text ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> TaskApplicabilityId > Concept", ids =>
				{
					list = list.Where( m => ids.Contains( m.TaskApplicabilityId ?? 0 ) );
				} );

				//RMTL Search
				AppendTextFilterIfPresent( query, "> TaskApplicabilityId > Concept.TextFields", text =>
				{
					list = list.Where( m => m.ConceptScheme_Concept_TaskApplicabilityType.Name.Contains( text ) || m.ConceptScheme_Concept_TaskApplicabilityType.CodedNotation.Contains( text ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> CourseContextId > CourseContext > AssessmentMethodConceptId > Concept", ids => {
					list = list.Where( m => m.CourseContext.CourseContext_AssessmentType.Select( n => n.AssessmentMethodConceptId ).Intersect( ids ).Count() > 0 );
				} );

				//RMTL Search
				AppendTextFilterIfPresent( query, "> CourseContextId > CourseContext > AssessmentMethodConceptId > Concept.Name", text => {
					list = list.Where( m => m.CourseContext.CourseContext_AssessmentType.Where( n => n.ConceptScheme_Concept.Name.Contains( text ) ).Count() > 0 );
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

				//Detail Pages
				AppendTextFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > TrainingSolutionTypeId > Concept.Name", text => {
					list = list.Where( m => m.ClusterAnalysis.ConceptScheme_Concept_TrainingSolutionType.Name.Contains( text ) );
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

				//Detail Pages
				AppendTextFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > CandidatePlatformConceptId > Concept.TextFields", text => {
					list = list.Where( m => m.ClusterAnalysis.ClusterAnalysis_HasCandidatePlatform.Where( n => n.ConceptScheme_Concept.Name.Contains( text ) || n.ConceptScheme_Concept.CodedNotation.Contains( text ) ).Count() > 0 );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > DevelopmentRatioTypeId > Concept", ids => {
					list = list.Where( m => ids.Contains( m.ClusterAnalysis.DevelopmentRatioTypeId ?? 0 ) );
				} );

				//RMTL Search
				AppendIDsFilterIfPresent( query, "> ClusterAnalysisId > ClusterAnalysis > CFMPlacementTypeId > Concept", ids => {
					list = list.Where( m => m.ClusterAnalysis.ClusterAnalysis_CFMPlacementType.Select( n => n.CFMPlacementConceptId ).Intersect( ids ).Count() > 0 );
				} );

				//Exclude items
				AppendIDsFilterIfPresent( query, "search:Exclude", ( ids ) =>
				{
					list = list.Where( m => !ids.Contains( m.Id ) );
				} );

				//Concept Detail Page
				//ConceptManager.DeleteById
				AppendIDsFilterIfPresent( query, "search:AllConceptPaths", ids => {
					list = list.Where( m =>
						//m.RatingTask.ReferenceResource.ReferenceResource_ReferenceType.Select( n => n.ReferenceTypeId ).Intersect( ids ).Count() > 0 ||
						m.RatingTask.ReferenceResource.ReferenceResource_ReferenceType.Any( n => ids.Contains( n.ReferenceTypeId ) ) ||
						ids.Contains( m.TaskApplicabilityId ?? 0 ) ||
						ids.Contains( m.FormalTrainingGapId ?? 0 ) ||
						ids.Contains( m.PayGradeTypeId ) ||
						ids.Contains( m.PayGradeLevelTypeId ?? 0) ||
						//m.CourseContext.CourseContext_AssessmentType.Select( n => n.AssessmentMethodConceptId ).Intersect( ids ).Count() > 0 ||
						m.CourseContext.CourseContext_AssessmentType.Any( n => ids.Contains( n.AssessmentMethodConceptId ) ) ||
						//m.CourseContext.Course.Course_CourseType.Select( n => n.CourseTypeConceptId ).Intersect( ids ).Count() > 0 ||
						m.CourseContext.Course.Course_CourseType.Any( n => ids.Contains( n.CourseTypeConceptId ) ) ||
						ids.Contains( m.CourseContext.Course.LifeCycleControlDocumentTypeId ?? 0 ) ||
						ids.Contains( m.ClusterAnalysis.TrainingSolutionTypeId ?? 0 ) ||
						ids.Contains( m.ClusterAnalysis.RecommendedModalityTypeId ?? 0 ) ||
						ids.Contains( m.ClusterAnalysis.DevelopmentSpecificationTypeId ?? 0 ) ||
						//m.ClusterAnalysis.ClusterAnalysis_HasCandidatePlatform.Select( n => n.CandidatePlatformConceptId ).Intersect( ids ).Count() > 0 ||
						m.ClusterAnalysis.ClusterAnalysis_HasCandidatePlatform.Any( n => ids.Contains( n.CandidatePlatformConceptId ) ) ||
						ids.Contains( m.ClusterAnalysis.DevelopmentRatioTypeId ?? 0 ) ||
						//m.ClusterAnalysis.ClusterAnalysis_CFMPlacementType.Select( n => n.CFMPlacementConceptId ).Intersect( ids ).Count() > 0
						m.ClusterAnalysis.ClusterAnalysis_CFMPlacementType.Any( n => ids.Contains( n.CFMPlacementConceptId ) )
					);
				} );

				//Concept Scheme Detail Page
				AppendIDsFilterIfPresent( query, "search:AllConceptSchemePaths", ids => {
					var conceptIDs = ids.Select( m => ConceptSchemeManager.GetById( m ) ).SelectMany( m => m.Concepts ).Select( m => m.Id ).ToList();
					list = list.Where( m =>
						//m.RatingTask.ReferenceResource.ReferenceResource_ReferenceType.Select( n => n.ReferenceTypeId ).Intersect( conceptIDs ).Count() > 0 ||
						m.RatingTask.ReferenceResource.ReferenceResource_ReferenceType.Any( n => ids.Contains( n.ReferenceTypeId ) ) ||
						conceptIDs.Contains( m.TaskApplicabilityId ?? 0 ) ||
						conceptIDs.Contains( m.FormalTrainingGapId ?? 0 ) ||
						conceptIDs.Contains( m.PayGradeTypeId ) ||
						conceptIDs.Contains( m.PayGradeLevelTypeId ?? 0 ) ||
						//m.CourseContext.CourseContext_AssessmentType.Select( n => n.AssessmentMethodConceptId ).Intersect( conceptIDs ).Count() > 0 ||
						m.CourseContext.CourseContext_AssessmentType.Any( n => ids.Contains( n.AssessmentMethodConceptId ) ) ||
						//m.CourseContext.Course.Course_CourseType.Select( n => n.CourseTypeConceptId ).Intersect( conceptIDs ).Count() > 0 ||
						m.CourseContext.Course.Course_CourseType.Any( n => ids.Contains( n.CourseTypeConceptId ) ) ||
						conceptIDs.Contains( m.CourseContext.Course.LifeCycleControlDocumentTypeId ?? 0 ) ||
						conceptIDs.Contains( m.ClusterAnalysis.TrainingSolutionTypeId ?? 0 ) ||
						conceptIDs.Contains( m.ClusterAnalysis.RecommendedModalityTypeId ?? 0 ) ||
						conceptIDs.Contains( m.ClusterAnalysis.DevelopmentSpecificationTypeId ?? 0 ) ||
						//m.ClusterAnalysis.ClusterAnalysis_HasCandidatePlatform.Select( n => n.CandidatePlatformConceptId ).Intersect( conceptIDs ).Count() > 0 ||
						m.ClusterAnalysis.ClusterAnalysis_HasCandidatePlatform.Any( n => ids.Contains( n.CandidatePlatformConceptId ) ) ||
						conceptIDs.Contains( m.ClusterAnalysis.DevelopmentRatioTypeId ?? 0 ) ||
						//m.ClusterAnalysis.ClusterAnalysis_CFMPlacementType.Select( n => n.CFMPlacementConceptId ).Intersect( conceptIDs ).Count() > 0
						m.ClusterAnalysis.ClusterAnalysis_CFMPlacementType.Any( n => ids.Contains( n.CFMPlacementConceptId ) )
					);
				} );

				//Count the number of Rating Tasks associated with these RatingContexts, as this count is used on the RMTL Search and on most detail pages
				ratingTaskCount = list.Select( m => m.RatingTask.Id ).Distinct().Count();

				//Custom handling for the multi-column-based sorting in the RMTL search
				if ( query.SortOrder?.FirstOrDefault( m => m.Column == "sortOrder:RMTLSearchSortHandler" ) != null )
				{
					var projected = list.Select( m => new
					{
						Main = m,
						Rating_CodedNotation = m.Rating.CodedNotation,
						PayGrade_CodedNotation = m.ConceptScheme_Concept_PayGradeType.CodedNotation,
						PayGradeLevel_CodedNotation = m.ConceptScheme_Concept_PayGradeLevelType.CodedNotation,
						Job_Name = m.Job.Name,
						WorkRole_Name = m.WorkRole.Name,
						RatingTask_ReferenceResource_Name = m.RatingTask.ReferenceResource.Name,
						RatingTask_ReferenceResource_PublicationDate = m.RatingTask.ReferenceResource.PublicationDate,
						RatingTask_ReferenceType_Name = m.RatingTask.ConceptScheme_Concept_ReferenceType.Name,
						RatingTask_Description = m.RatingTask.Description,
						TaskApplicabilityType_Name = m.ConceptScheme_Concept_TaskApplicabilityType.Name,
						TrainingGapType_Name = m.ConceptScheme_Concept_TrainingGapType.Name,
						CourseContext_Course_CodedNotation = m.CourseContext.Course.CodedNotation ?? "",
						CourseContext_Course_Name = m.CourseContext.Course.Name ?? "",
						CourseContext_Course_CourseType_Name = m.CourseContext.Course.Course_CourseType.Select( n => n.ConceptScheme_Concept_CourseType.Name ),
						CourseContext_Course_Organization_Name = m.CourseContext.Course.Organization.Name ?? "",
						CourseContext_Course_LifecycleControlDocumentType_Name = m.CourseContext.Course.ConceptScheme_Concept.Name ?? "",
						CourseContext_TrainingTask_Description = m.CourseContext.TrainingTask.Description,
						CourseContext_AssessmentMethodType_Name = m.CourseContext.CourseContext_AssessmentType.Select( n => n.ConceptScheme_Concept.Name ),
						ClusterAnalysis_ClusterAnalysisTitle_Name = m.ClusterAnalysis.ClusterAnalysisTitle.Name ?? "",
						ClusterAnalysis_TrainingSolutionType_Name = m.ClusterAnalysis.ConceptScheme_Concept_TrainingSolutionType.Name ?? "",
						ClusterAnalysis_RecommendedModalityType_Name = m.ClusterAnalysis.ConceptScheme_Concept_RecommendedModalityType.Name ?? "",
						ClusterAnalysis_DevelopmentSpecificationType_Name = m.ClusterAnalysis.ConceptScheme_Concept_DevelopmentSpecificationType.Name ?? "",
						ClusterAnalysis_CandidatePlatformType_CodedNotation = m.ClusterAnalysis.ClusterAnalysis_HasCandidatePlatform.Select( n => n.ConceptScheme_Concept.CodedNotation ),
						ClusterAnalysis_CFMPlacementType_Name = m.ClusterAnalysis.ClusterAnalysis_CFMPlacementType.Select( n => n.ConceptScheme_Concept.Name ),
						ClusterAnalysis_DevelopmentRatioType_Name = m.ClusterAnalysis.ConceptScheme_Concept_DevelopmentRatioType.Name ?? "",
						ClusterAnalysis_PriorityPlacement = m.ClusterAnalysis.PriorityPlacement ?? 0,
						ClusterAnalysis_EstimatedInstructionalTime = m.ClusterAnalysis.EstimatedInstructionalTime ?? 0,
						ClusterAnalysis_DevelopmentTime = m.ClusterAnalysis.DevelopmentTime ?? 0
					} ).OrderBy( m => true );

					foreach ( var sortItem in query.SortOrder.Where( m => m.Column != "sortOrder:RMTLSearchSortHandler" ).ToList() )
					{
						if ( sortItem.Ascending )
						{
							switch ( sortItem.Column )
							{
								case "> RowId": projected = projected.ThenBy( m => m.Main.Id ); break;
								case "> HasRating > Rating > CodedNotation": projected = projected.ThenBy( m => m.Rating_CodedNotation ); break;
								case "> PayGradeType > Concept > CodedNotation": projected = projected.ThenBy( m => m.PayGrade_CodedNotation ); break;
								case "> PayGradeLevelType > Concept > CodedNotation": projected = projected.ThenBy( m => m.PayGradeLevel_CodedNotation ); break;
								case "> HasBilletTitle > BilletTitle > Name": projected = projected.ThenBy( m => m.Job_Name ); break;
								case "> HasWorkRole > WorkRole > Name": projected = projected.ThenBy( m => m.WorkRole_Name ); break;
								case "> HasRatingTask > RatingTask > HasReferenceResource > ReferenceResource > Name": projected = projected.ThenBy( m => m.RatingTask_ReferenceResource_Name ); break;
								case "> HasRatingTask > RatingTask > HasReferenceResource > ReferenceResource > PublicationDate": projected = projected.ThenBy( m => m.RatingTask_ReferenceResource_PublicationDate ); break;
								case "> HasRatingTask > RatingTask > ReferenceType > Concept > WorkElementType": projected = projected.ThenBy( m => m.RatingTask_ReferenceType_Name ); break;
								case "> HasRatingTask > RatingTask > Description": projected = projected.ThenBy( m => m.RatingTask_Description ); break;
								case "> ApplicabilityType > Concept > Name": projected = projected.ThenBy( m => m.TaskApplicabilityType_Name ); break;
								case "> TrainingGapType > Concept > Name": projected = projected.ThenBy( m => m.TrainingGapType_Name ); break;
								case "> HasCourseContext > CourseContext > RowId": projected = projected.ThenBy( m => m.Main.CourseContextId ?? 0 ); break;
								case "> HasCourseContext > CourseContext > HasCourse > Course > CodedNotation": projected = projected.ThenBy( m => m.CourseContext_Course_CodedNotation ); break;
								case "> HasCourseContext > CourseContext > HasCourse > Course > Name": projected = projected.ThenBy( m => m.CourseContext_Course_Name ); break;
								case "> HasCourseContext > CourseContext > HasCourse > Course > CourseType > Concept > Name": projected = projected.ThenBy( m => m.CourseContext_Course_CourseType_Name ); break;
								case "> HasCourseContext > CourseContext > HasCourse > Course > CurriculumControlAuthority > Organization > Name": projected = projected.ThenBy( m => m.CourseContext_Course_Organization_Name ); break;
								case "> HasCourseContext > CourseContext > HasCourse > Course > LifeCycleControlDocumentType > Concept > Name": projected = projected.ThenBy( m => m.CourseContext_Course_LifecycleControlDocumentType_Name ); break;
								case "> HasCourseContext > CourseContext > HasTrainingTask > TrainingTask > Description": projected = projected.ThenBy( m => m.CourseContext_TrainingTask_Description ); break;
								case "> HasCourseContext > CourseContext > AssessmentMethodType > Concept > Name": projected = projected.ThenBy( m => m.CourseContext_AssessmentMethodType_Name.OrderBy( n => n ).FirstOrDefault() ?? "" ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > TrainingSolutionType > Concept > Name": projected = projected.ThenBy( m => m.ClusterAnalysis_TrainingSolutionType_Name ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > RowId": projected = projected.ThenBy( m => m.Main.ClusterAnalysisId ?? 0 ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > HasClusterAnalysisTitle > ClusterAnalysisTitle > Name": projected = projected.ThenBy( m => m.ClusterAnalysis_ClusterAnalysisTitle_Name ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > RecommendedModalityType > Concept > Name": projected = projected.ThenBy( m => m.ClusterAnalysis_RecommendedModalityType_Name ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > DevelopmentSpecificationType > Concept > Name": projected = projected.ThenBy( m => m.ClusterAnalysis_DevelopmentSpecificationType_Name ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > CandidatePlatformType > Concept > CodedNotation": projected = projected.ThenBy( m => m.ClusterAnalysis_CandidatePlatformType_CodedNotation.OrderBy(n => n).FirstOrDefault() ?? "" ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > CFMPlacementType > Concept > Name": projected = projected.ThenBy( m => m.ClusterAnalysis_CFMPlacementType_Name.OrderBy(n => n).FirstOrDefault() ?? "" ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > PriorityPlacement": projected = projected.ThenBy( m => m.ClusterAnalysis_PriorityPlacement ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > DevelopmentRatioType > Concept > Name": projected = projected.ThenBy( m => m.ClusterAnalysis_DevelopmentRatioType_Name ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > EstimatedInstructionalTime": projected = projected.ThenBy( m => m.ClusterAnalysis_EstimatedInstructionalTime ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > DevelopmentTime": projected = projected.ThenBy( m => m.ClusterAnalysis_DevelopmentTime ); break;
								case "> Notes": projected = projected.ThenBy( m => m.Main.Notes ?? "" ); break;
								default: break;
							}

						}
						else
						{
							switch ( sortItem.Column )
							{
								case "> RowId": projected = projected.ThenByDescending( m => m.Main.Id ); break;
								case "> HasRating > Rating > CodedNotation": projected = projected.ThenByDescending( m => m.Rating_CodedNotation ); break;
								case "> PayGradeType > Concept > CodedNotation": projected = projected.ThenByDescending( m => m.PayGrade_CodedNotation ); break;
								case "> PayGradeLevelType > Concept > CodedNotation": projected = projected.ThenByDescending( m => m.PayGradeLevel_CodedNotation ); break;
								case "> HasBilletTitle > BilletTitle > Name": projected = projected.ThenByDescending( m => m.Job_Name ); break;
								case "> HasWorkRole > WorkRole > Name": projected = projected.ThenByDescending( m => m.WorkRole_Name ); break;
								case "> HasRatingTask > RatingTask > HasReferenceResource > ReferenceResource > Name": projected = projected.ThenByDescending( m => m.RatingTask_ReferenceResource_Name ); break;
								case "> HasRatingTask > RatingTask > HasReferenceResource > ReferenceResource > PublicationDate": projected = projected.ThenByDescending( m => m.RatingTask_ReferenceResource_PublicationDate ); break;
								case "> HasRatingTask > RatingTask > ReferenceType > Concept > WorkElementType": projected = projected.ThenByDescending( m => m.RatingTask_ReferenceType_Name ); break;
								case "> HasRatingTask > RatingTask > Description": projected = projected.ThenByDescending( m => m.RatingTask_Description ); break;
								case "> ApplicabilityType > Concept > Name": projected = projected.ThenByDescending( m => m.TaskApplicabilityType_Name ); break;
								case "> TrainingGapType > Concept > Name": projected = projected.ThenByDescending( m => m.TrainingGapType_Name ); break;
								case "> HasCourseContext > CourseContext > RowId": projected = projected.ThenByDescending( m => m.Main.CourseContextId ?? 0 ); break;
								case "> HasCourseContext > CourseContext > HasCourse > Course > CodedNotation": projected = projected.ThenByDescending( m => m.CourseContext_Course_CodedNotation ); break;
								case "> HasCourseContext > CourseContext > HasCourse > Course > Name": projected = projected.ThenByDescending( m => m.CourseContext_Course_Name ); break;
								case "> HasCourseContext > CourseContext > HasCourse > Course > CourseType > Concept > Name": projected = projected.ThenByDescending( m => m.CourseContext_Course_CourseType_Name ); break;
								case "> HasCourseContext > CourseContext > HasCourse > Course > CurriculumControlAuthority > Organization > Name": projected = projected.ThenByDescending( m => m.CourseContext_Course_Organization_Name ); break;
								case "> HasCourseContext > CourseContext > HasCourse > Course > LifeCycleControlDocumentType > Concept > Name": projected = projected.ThenByDescending( m => m.CourseContext_Course_LifecycleControlDocumentType_Name ); break;
								case "> HasCourseContext > CourseContext > HasTrainingTask > TrainingTask > Description": projected = projected.ThenByDescending( m => m.CourseContext_TrainingTask_Description ); break;
								case "> HasCourseContext > CourseContext > AssessmentMethodType > Concept > Name": projected = projected.ThenByDescending( m => m.CourseContext_AssessmentMethodType_Name.OrderBy( n => n ).FirstOrDefault() ?? "" ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > TrainingSolutionType > Concept > Name": projected = projected.ThenByDescending( m => m.ClusterAnalysis_TrainingSolutionType_Name ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > RowId": projected = projected.ThenByDescending( m => m.Main.ClusterAnalysisId ?? 0 ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > HasClusterAnalysisTitle > ClusterAnalysisTitle > Name": projected = projected.ThenByDescending( m => m.ClusterAnalysis_ClusterAnalysisTitle_Name ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > RecommendedModalityType > Concept > Name": projected = projected.ThenByDescending( m => m.ClusterAnalysis_RecommendedModalityType_Name ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > DevelopmentSpecificationType > Concept > Name": projected = projected.ThenByDescending( m => m.ClusterAnalysis_DevelopmentSpecificationType_Name ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > CandidatePlatformType > Concept > CodedNotation": projected = projected.ThenByDescending( m => m.ClusterAnalysis_CandidatePlatformType_CodedNotation.OrderBy( n => n ).FirstOrDefault() ?? "" ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > CFMPlacementType > Concept > Name": projected = projected.ThenByDescending( m => m.ClusterAnalysis_CFMPlacementType_Name.OrderBy( n => n ).FirstOrDefault() ?? "" ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > PriorityPlacement": projected = projected.ThenByDescending( m => m.ClusterAnalysis_PriorityPlacement ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > DevelopmentRatioType > Concept > Name": projected = projected.ThenByDescending( m => m.ClusterAnalysis_DevelopmentRatioType_Name ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > EstimatedInstructionalTime": projected = projected.ThenByDescending( m => m.ClusterAnalysis_EstimatedInstructionalTime ); break;
								case "> HasClusterAnalysis > ClusterAnalysis > DevelopmentTime": projected = projected.ThenByDescending( m => m.ClusterAnalysis_DevelopmentTime ); break;
								case "> Notes": projected = projected.ThenByDescending( m => m.Main.Notes ?? "" ); break;
								default: break;
							}
						}

					}

					return projected.Select( m => m.Main ).AsEnumerable<DBEntity>().OrderBy( m => true );
				}
				//Or normal sort handling
				else
				{
					//Return ordered list
					//Traversal requires projection to avoid querying every row in the results
					var projected = list.Select( m => new { Main = m, Notes = m.Notes ?? "", RatingTask_Description = m.RatingTask.Description, TrainingGapID = m.FormalTrainingGapId ?? 0, Rating_CodedNotation = m.Rating.CodedNotation, Rating_Name = m.Rating.Name } );
					
					var sorted = HandleSortV2( projected, query.SortOrder, m => m.RatingTask_Description, m => m.Main.Id,
					m => m.OrderByDescending( n => n.TrainingGapID == noGapID )
							.ThenBy( n => n.Rating_CodedNotation )
							.ThenBy( n => n.RatingTask_Description ),
					( m, keywordParts ) => m.OrderBy( n =>
						//This approach does still trigger multiple extra queries but it may be fast enough anyway(?)
						//Better approach TBD
						RelevanceHelper( n, keywordParts, o => o.RatingTask_Description, null, null, 25 ) +
						RelevanceHelper( n, keywordParts, o => o.Rating_CodedNotation ) +
						RelevanceHelper( n, keywordParts, o => o.Rating_Name ) +
						RelevanceHelper( n, keywordParts, o => o.Main.Notes )
					), keywords );

					return sorted.Select( m => m.Main ).OrderBy( m => true );
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

		public static AppEntity MapFromDB( DBEntity input, DataEntities context )
		{
			return MapFromDBForSearch( input, context, null );
		}
		//

		public static AppEntity MapFromDBForSearch( DBEntity input, DataEntities context, SearchResultSet<AppEntity> resultSet = null )
		{
			var output = AutoMap( input, new AppEntity() );
			output.HasRating = input.Rating?.RowId ?? Guid.Empty;
			output.HasRatingId = input.RatingId; //Wish these property names matched
			output.HasBilletTitle = input.Job?.RowId ?? Guid.Empty;
			output.HasBilletTitleId = input.BilletTitleId ?? 0; //Wish these property names matched
			output.HasWorkRole = input.WorkRole?.RowId ?? Guid.Empty;
			output.HasWorkRoleId = input.WorkRoleId ?? 0; //Wish these property names matched
			output.HasRatingTask = input.RatingTask?.RowId ?? Guid.Empty;
			output.HasRatingTaskId = input.RatingTaskId; //Wish these property names matched
			output.HasCourseContext = input.CourseContext?.RowId ?? Guid.Empty;
			output.HasCourseContextId = input.CourseContextId ?? 0; //Wish these property names matched
			output.HasClusterAnalysis = input.ClusterAnalysis?.RowId ?? Guid.Empty;
			output.HasClusterAnalysisId = input.ClusterAnalysisId ?? 0; //Wish these property names matched
			output.ApplicabilityType = input.ConceptScheme_Concept_TaskApplicabilityType?.RowId ?? Guid.Empty;
			output.ApplicabilityTypeId = input.TaskApplicabilityId ?? 0; //Wish these property names matched
			output.TrainingGapType = input.ConceptScheme_Concept_TrainingGapType?.RowId ?? Guid.Empty;
			output.TrainingGapTypeId = input.FormalTrainingGapId ?? 0; //Wish these property names matched
			output.PayGradeType = input.ConceptScheme_Concept_PayGradeType?.RowId ?? Guid.Empty; //int ID field automatches
			output.PayGradeLevelType = input.ConceptScheme_Concept_PayGradeLevelType?.RowId ?? Guid.Empty; //int ID field automatches

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

        #endregion
    }

}
