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

		public static SearchResultSet<T> GeneralSearch<T>( SearchQuery query, Func<SearchQuery, SearchResultSet<T>> searchMethod, JObject debug = null )
		{
			//Setup logging
			debug = debug ?? new JObject();
			var searchType = typeof( T ).Name;
			LoggingHelper.DoTrace( 6, thisClassName + "." + searchType + "Search - entered" );

			//Normalize the query
			NormalizeQuery( query, searchType );

			//Do the search
			LoggingHelper.DoTrace( 7, thisClassName + "." + searchType + "Search. Calling: " + typeof( T ).Name );
			var results = searchMethod( query );

			//Return the results
			return results;
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
			return GeneralSearch( query, Factories.ConceptManager.Search, debug );
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

		//CourseContext
		public static SearchResultSet<CourseContext> CourseContextSearch( SearchQuery query, JObject debug = null )
		{
			return GeneralSearch( query, Factories.CourseContextManager.Search, debug );
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
			//Sanitize Text filters, including Keywords
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
			var maxTake = query.IsExportMode ? 10000 : 250;
			query.Take = query.Take < -1 ? -1 : query.Take > maxTake ? maxTake : query.Take; //Max page size must not be smaller than the page size the RMTL search is looking for client-side!

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
