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
				var ratingTaskTokens = new List<string>();
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
				AppendIDsFilterIfPresent( query, "> CourseContextId > CourseContext > HasTrainingTaskId > TrainingTask", ids => {
					list = list.Where( m => ids.Contains( m.CourseContext.HasTrainingTaskId ) );
				} );

				//Rating Task Detail Page
				AppendNotNullFilterIfPresent( query, "> CourseContextId > CourseContext:NotNull", () => {
					list = list.Where( m => m.CourseContextId != null );
				} );

				//RMTL Search
				var trainingTaskTokens = new List<string>();
				AppendTextFilterIfPresent( query, "> CourseContextId > CourseContext > HasTrainingTaskId > TrainingTask.TextFields", text => {
					list = list.Where( m => m.CourseContext.TrainingTask.Description.Contains( text ) );
					trainingTaskTokens = GetRelevanceTokens( text );
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
				AppendIDsFilterIfPresent( query, "> PayGradeLevelTypeId > Concept", ids =>
				{
					list = list.Where( m => ids.Contains( m.PayGradeLevelTypeId ?? 0 ) );
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
						m.ClusterAnalysis.ClusterAnalysis_CFMPlacementType.Select( n => n.CFMPlacementConceptId ).Intersect( ids ).Count() > 0
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
						m.ClusterAnalysis.ClusterAnalysis_CFMPlacementType.Select( n => n.CFMPlacementConceptId ).Intersect( conceptIDs ).Count() > 0
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
							case "> HasRatingTask > RatingTask > Description":
								//For some reason, Entity Framework can't handle using SortAscDesc's object conversion here, but it works elsewhere, so break it out:
								if( ratingTaskTokens.Count() > 0 )
								{
									//For some reason, RelevanceHelper's usage of StringComparison.OrdinalIgnoreCase breaks Entity Framework here, but not elsewhere, so do it this way
									sorted = sortItem.Ascending ? 
										sorted.ThenBy( m => ratingTaskTokens.Select( n => m.RatingTask.Description.IndexOf( n.ToLower() ) ).Where( n => n != -1 ).Sum() ) : 
										sorted.ThenByDescending( m => ratingTaskTokens.Select( n => m.RatingTask.Description.ToLower().IndexOf( n.ToLower() ) ).Where( n => n != -1 ).Sum() );
								}
								else
								{
									sorted = SortAscOrDesc( sorted, sortItem, m => m.RatingTask.Description );
								}
							break;
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
							case "> HasCourseContext > CourseContext > HasTrainingTask > TrainingTask > Description":
								//For some reason, Entity Framework can't handle using SortAscDesc's object conversion here, but it works elsewhere, so break it out:
								if( trainingTaskTokens.Count() > 0 )
								{
									//For some reason, RelevanceHelper's usage of StringComparison.OrdinalIgnoreCase breaks Entity Framework here, but not elsewhere, so do it this way
									sorted = sortItem.Ascending ? 
										sorted.ThenBy( m => trainingTaskTokens.Select( n => m.CourseContext.TrainingTask.Description.IndexOf( n.ToLower() ) ).Where( n => n != -1 ).Sum() ) : 
										sorted.ThenByDescending( m => trainingTaskTokens.Select( n => m.CourseContext.TrainingTask.Description.ToLower().IndexOf( n.ToLower() ) ).Where( n => n != -1 ).Sum() );
								}
								else
								{
									sorted = SortAscOrDesc( sorted, sortItem, m => m.CourseContext.TrainingTask.Description );
								}
							break;
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
							case "> HasClusterAnalysis > ClusterAnalysis > CFMPlacementType > Concept > Name":
								sorted = sortItem.Ascending ?
									SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.ClusterAnalysis_CFMPlacementType.Select( n => n.ConceptScheme_Concept.CodedNotation ).OrderBy( n => n ).FirstOrDefault() ) :
									SortAscOrDesc( sorted, sortItem, m => m.ClusterAnalysis.ClusterAnalysis_CFMPlacementType.Select( n => n.ConceptScheme_Concept.CodedNotation ).OrderByDescending( n => n ).FirstOrDefault() );
								break;
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
					}, ( m, keywordParts ) => m.OrderBy( n => RelevanceHelper( n, keywordParts, o => o.RatingTask.Description ) + RelevanceHelper( n, keywordParts, o => o.Rating.CodedNotation ) + RelevanceHelper( n, keywordParts, o => o.Rating.Name ) ), keywords );
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

        #endregion
    }

}
