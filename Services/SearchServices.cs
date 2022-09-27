using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Schema;
using Models.Curation;
using Models.Search;
using Navy.Utilities;

using Newtonsoft.Json.Linq;

namespace Services
{
	public class SearchServices
	{
		public static string thisClassName = "SearchServices";


		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="valid">Only significant with status not empty</param>
		/// <param name="status">Caller will likely ignore unless there are no results</param>
		/// <param name="debug"></param>
		/// <returns></returns>
		public SearchResultSet<ResultWithExtraData<UploadableRow>> RMTLSearch( SearchQuery query, ref bool valid, ref string status, JObject debug = null )
		{
			debug = debug ?? new JObject();
			LoggingHelper.DoTrace( 6, thisClassName + ".RMTLSearch - entered" );

			//Normalize the query
			NormalizeQuery( query, "RatingTask" );

			//Do the search
			var totalResults = 0;
			LoggingHelper.DoTrace( 7, thisClassName + ".RMTLSearch. Calling: " + query.SearchType );
			var results = RatingTaskServices.RMTLSearch( query, ref totalResults );

			//Convert the results
			var output = new SearchResultSet<ResultWithExtraData<UploadableRow>>() { TotalResults = totalResults, SearchType = query.SearchType };
			foreach ( var item in results )
			{
				output.Results.Add( new ResultWithExtraData<UploadableRow>()
				{
					Data = new UploadableRow()
					{
						Row_CodedNotation = item.CodedNotation,
						Rating_CodedNotation = item.Ratings,
						PayGradeType_CodedNotation = item.Rank,
						Level_Name = item.Level,
						BilletTitle_Name = item.BilletTitles,
						WorkRole_Name = item.FunctionalArea,
						ReferenceResource_Name = item.ReferenceResource,
						ReferenceResource_PublicationDate = item.SourceDate,
						Shared_ReferenceType = item.WorkElementTypeAlternateName,
						RatingTask_Description = item.Description,
						RatingTask_ApplicabilityType_Name = item.TaskApplicability,
						RatingTask_TrainingGapType_Name = item.FormalTrainingGap,
						Course_CodedNotation = item.CIN,
						Course_Name = item.CourseName,
						Course_CourseType_Name = item.CourseType,
						Course_LifeCycleControlDocumentType_CodedNotation = item.LifeCycleControlDocument,
						Course_CurriculumControlAuthority_Name = item.CurriculumControlAuthority,
						TrainingTask_Description = item.TrainingTask,
						Course_AssessmentMethodType_Name = item.CurrentAssessmentApproach,
						Note = item.Note,
						Training_Solution_Type = item.TrainingSolutionType,
						Cluster_Analysis_Title = item.ClusterAnalysisTitle,
						Recommended_Modality = item.RecommendedModality,
						Development_Specification = item.DevelopmentSpecification,
						Candidate_Platform = item.CandidatePlatform,
						CFM_Placement = item.CFMPlacement,
						Priority_Placement = item.Priority_Placement,
						Development_Ratio = item.DevelopmentRatio,
						Estimated_Instructional_Time = item.Estimated_Instructional_Time,
						Development_Time = item.Development_Time,
						Cluster_Analysis_Notes = item.ClusterAnalysisNotes,
					},
					Extra = new JObject()
					{
						{ "ResultNumber", item.ResultNumber },
						{ "RecordId", item.Id },
						{ "CTID", item.CTID },
						{ "RowId", item.RowId.ToString() },
						{ "Created", item.Created.ToShortDateString() },
						{ "LastUpdated", item.LastUpdated.ToShortDateString() },
					}
				} );
			}

			//Return the results
			return output;
		}
		//

		public static SearchResultSet<T> GeneralSearch<T>( SearchQuery query, Func<SearchQuery, List<T>> searchMethod, JObject debug = null )
		{
			//Setup logging
			debug = debug ?? new JObject();
			var searchType = typeof( T ).Name;
			LoggingHelper.DoTrace( 6, thisClassName + "." + searchType + "Search - entered" );

			//Normalize the query
			NormalizeQuery( query, searchType );

			//Do the search
			LoggingHelper.DoTrace( 7, thisClassName + "." + searchType + "Search. Calling: " + query.SearchType );
			var results = searchMethod( query );

			//Convert the results
			var output = new SearchResultSet<T>() { Results = results, TotalResults = query.TotalResults, SearchType = searchType };

			//Return the results
			return output;
		}

		//Billet Title
		public static SearchResultSet<BilletTitle> BilletTitleSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.JobManager.Search, debug );
		}
		//

		//Concept
		public static SearchResultSet<Concept> ConceptSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.ConceptSchemeManager.SearchConcept, debug );
		}
		//

		//ConceptScheme
		public static SearchResultSet<ConceptScheme> ConceptSchemeSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.ConceptSchemeManager.Search, debug );
		}
		//

		//Course
		public static SearchResultSet<Course> CourseSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.CourseManager.Search, debug );
		}
		//

		//Organization
		public static SearchResultSet<Organization> OrganizationSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.OrganizationManager.Search, debug );
		}
		//

		//Rating
		public static SearchResultSet<Rating> RatingSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.RatingManager.Search, debug );
		}
		//

		//RatingTask
		public static SearchResultSet<RatingTask> RatingTaskSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.RatingTaskManager.Search, debug );
		}
		//

		//RatingContext
		public static SearchResultSet<RatingContext> RatingContextSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.RatingContextManager.Search, debug );
		}
		//

		//ReferenceResource
		public static SearchResultSet<ReferenceResource> ReferenceResourceSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.ReferenceResourceManager.Search, debug );
		}
		//

		//RMTLProject
		public static SearchResultSet<RMTLProject> RMTLProjectSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.RMTLProjectManager.Search, debug );
			//return new SearchResultSet<RMTLProject>() { Results = new List<RMTLProject>() { new RMTLProject() { Name = "Not implemented yet!" } } };
		}
		//

		//TrainingTask
		public static SearchResultSet<TrainingTask> TrainingTaskSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.TrainingTaskManager.Search, debug );
		}
		//

		//WorkRole
		public static SearchResultSet<WorkRole> WorkRoleSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.WorkRoleManager.Search, debug );
		}
		//

		//ClusterAnalysis
		public static SearchResultSet<ClusterAnalysis> ClusterAnalysisSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.ClusterAnalysisManager.Search, debug );
		}
		//

		//ClusterAnalysisTitle
		public static SearchResultSet<ClusterAnalysisTitle> ClusterAnalysisTitleSearch(SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.ClusterAnalysisTitleManager.Search, debug );
		}
		//

		private static void NormalizeQuery( SearchQuery query, string searchType, JObject debug = null )
		{
			//Sanitize Keywords
			query.Keywords = SanitizeKeywordString( query.Keywords );
			foreach ( var filter in query.Filters.Where( m => !string.IsNullOrWhiteSpace( m.Text ) ).ToList() )
			{
				filter.Text = SanitizeKeywordString( filter.Text );
			}

			//Sanitize Sort Order
			foreach( var item in query.SortOrder )
			{
				item.Column = SanitizeKeywordString( item.Column );
			}

			//Sanitize Page Size
			//Need a way to get all (use -1)
			query.PageSize = query.PageSize < -1 ? -1 : query.PageSize > 250 ? 250 : query.PageSize;

			//Override search type
			query.SearchType = searchType ?? query.SearchType ?? "Unknown";

			//Testing
			debug?.Add( "Raw Query", JObject.FromObject( query ) );
		}
		//

		private static string SanitizeKeywordString( string text )
		{
			var result = string.IsNullOrWhiteSpace( text ) ? "" : text;
			result = ServiceHelper.CleanText( result );
			result = ServiceHelper.HandleApostrophes( result );
			result = result.Trim();
			return result;
		}
		//

	}
}
