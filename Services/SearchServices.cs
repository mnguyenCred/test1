using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Schema;
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
		public SearchResultSet MainSearch( SearchQuery data, ref bool valid, ref string status, JObject debug = null )
		{
			debug = debug ?? new JObject();

			LoggingHelper.DoTrace( 6, "SearchServices.MainSearch - entered" );
            //var results = new SearchResultSet();
            #region Setup
            //Sanitize input
            //there will be different keyword types
            data.Keywords = string.IsNullOrWhiteSpace( data.Keywords ) ? "" : data.Keywords;
			data.Keywords = ServiceHelper.CleanText( data.Keywords );
			data.Keywords = ServiceHelper.HandleApostrophes( data.Keywords );
			data.Keywords = data.Keywords.Trim();

			//Sanitize input
			//need to translate sort order - should already be done, but not working in AB dev.
			//sortOrder:MostRelevant, sortOrder:Oldest, sortOrder:Newest, sortOrder:AtoZ, sortOrder:ZtoA
			if ( !string.IsNullOrWhiteSpace( data.SortOrder ) && data.SortOrder.IndexOf( "sortOrder:" ) == 0 )
			{
				switch ( data.SortOrder )
				{
					case "sortOrder:MostRelevant":
						data.SortOrder = "relevance";
						break;
					case "sortOrder:Oldest":
						data.SortOrder = "oldest";
						break;
					case "sortOrder:Newest":
						data.SortOrder = "newest";
						break;
					case "sortOrder:AtoZ":
						data.SortOrder = "alpha";
						break;
					case "sortOrder:ZtoA":
						data.SortOrder = "zalpha";
						break;
					default:
						data.SortOrder = "newest";
						break;

				}
			}
			var validSortOrders = new List<string>() { "newest", "oldest", "relevance", "alpha", "cost_lowest", "cost_highest", "duration_shortest", "duration_longest", "org_alpha", "zalpha" };
			if ( !validSortOrders.Contains( data.SortOrder ) )
			{
				data.SortOrder = validSortOrders.First();
			}

			//Default blind searches to "newest" when "relevance" is selected
			if ( string.IsNullOrWhiteSpace( data.Keywords ) && data.Filters.Count() == 0 && data.SortOrder == "relevance" )
			{
				data.SortOrder = "newest";
			}
			if ( data.PageSize < 10 || data.PageSize > 100 )
				data.PageSize = 25;

			//Determine search type
			var searchType = data.SearchType;
			if ( string.IsNullOrWhiteSpace( searchType ) )
			{
				valid = false;
				status = "Unable to determine search mode";
				return null;
			}

			//Testing
			debug.Add( "Raw Query", JObject.FromObject( data ) );
			#endregion
			//Do the search
			LoggingHelper.DoTrace( 7, thisClassName + ".MainSearch. Calling: " + searchType );

			var totalResults = 0;
			switch ( searchType.ToLower() )
			{
				case "ratingtask":
					{
						var results = RatingTaskServices.Search( data, ref totalResults );
						return ConvertRatingTaskResults( results, totalResults, searchType );
					}
				//case "course":
				//	{
				//		var results = CourseServices.Search( data, ref totalResults );
				//		return ConvertCourseResults( results, totalResults, searchType );
				//	}
			

				default:
					{
						valid = false;
						status = "Unknown search mode: " + searchType;
						return null;
					}
			}
		}

		private SearchResultSet ConvertRatingTaskResults( List<RatingTaskSummary> results, int totalResults, string searchType )
		{
			var output = new SearchResultSet() { TotalResults = totalResults, SearchType = searchType };

			foreach ( var item in results )
			{
				output.Results.Add( Result( "name?","friendlyName?", item.Description, item.Id,
					new Dictionary<string, object>()
					{
						{ "Rating", item.Ratings },
						{ "BilletTitles", item.BilletTitles },
						{ "Description", item.Description },
						{ "RecordId", item.Id },
						{ "ResultNumber", item.ResultNumber },
						{ "PayGrade", item.Rank },
						{ "Level", item.Level },
						{ "FunctionalArea", item.FunctionalArea },
						{ "Source", item.ReferenceResource },
						{ "SourceDate", item.SourceDate },
						{ "WorkElementType", item.WorkElementType },
						{ "TaskApplicability", item.TaskApplicability },
						{ "FormalTrainingGap", item.FormalTrainingGap },
						//{ "CanEditRecord", item.CanEditRecord },
						{ "CodedNotation", item.CodedNotation },
						{ "CIN", item.CIN }, //same as CodedNotation, so will remove
						{ "CourseName", item.CourseName },
						{ "TrainingTask", item.TrainingTask },
						{ "CurrentAssessmentApproach", item.CurrentAssessmentApproach },
						{ "CurriculumControlAuthority", item.CurriculumControlAuthority },
						{ "LifeCycleControlDocument", item.LifeCycleControlDocument },

						{ "SearchType", searchType },					
						{ "ctid", item.CTID },
                        { "Created", item.Created.ToShortDateString() },
						{ "LastUpdated", item.LastUpdated.ToShortDateString() },
						{ "T_LastUpdated", item.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss") },


					},
					new List<TagSet>()
				) );
			}
			return output;
		}
		public SearchResult Result( string name, string friendlyName, string description, int recordID, Dictionary<string, object> properties, List<TagSet> tags )
		{
			return new SearchResult()
			{
				Name = string.IsNullOrWhiteSpace( name ) ? "No name" : name,
				FriendlyName = string.IsNullOrWhiteSpace( friendlyName ) ? "Record" : friendlyName,
				Description = string.IsNullOrWhiteSpace( description ) ? "No description" : description,
				RecordId = recordID,
				Properties = properties == null ? new Dictionary<string, object>() : properties,
				Tags = tags == null ? new List<TagSet>() : tags,
			};
		}
	}
}
