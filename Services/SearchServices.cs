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
			var results = RatingTaskServices.Search( query, ref totalResults );

			//Convert the results
			var output = new SearchResultSet<ResultWithExtraData<UploadableRow>>() { TotalResults = totalResults, SearchType = query.SearchType };
			foreach ( var item in results )
			{
				output.Results.Add( new ResultWithExtraData<UploadableRow>()
				{
					Data = new UploadableRow()
					{
						Rating_CodedNotation = item.Ratings,
						PayGradeType_CodedNotation = item.Rank,
						Level_Name = item.Level,
						BilletTitle_Name = item.BilletTitles,
						WorkRole_Name = item.FunctionalArea,
						ReferenceResource_Name = item.ReferenceResource,
						ReferenceResource_PublicationDate = item.SourceDate,
						Shared_ReferenceType = item.WorkElementType,
						RatingTask_Description = item.Description,
						RatingTask_ApplicabilityType_Name = item.TaskApplicability,
						RatingTask_TrainingGapType_Name = item.FormalTrainingGap,
						Course_CodedNotation = item.CIN,
						Course_Name = item.CourseName,
						Course_CourseType_Name = item.CourseType,
						Course_HasReferenceResource_Name = item.LifeCycleControlDocument,
						Course_CurriculumControlAuthority_Name = item.CurriculumControlAuthority,
						TrainingTask_Description = item.TrainingTask,
						Course_AssessmentMethodType_Name = item.CurrentAssessmentApproach,
						Note = item.Note,
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


		public SearchResultSet<ResultWithExtraData<Course>> CourseSearch( SearchQuery query, ref bool valid, ref string status, JObject debug = null )
		{
			debug = debug ?? new JObject();
			LoggingHelper.DoTrace( 6, thisClassName + ".CourseSearch - entered" );

			//Normalize the query
			NormalizeQuery( query, "Course" );

			//Do the search
			var totalResults = 0;
			LoggingHelper.DoTrace( 7, thisClassName + ".CourseSearch. Calling: " + query.SearchType );
			var results = new List<Course>();// CourseServices.Search( query, ref totalResults );

			//Convert the results
			var output = new SearchResultSet<ResultWithExtraData<Course>>() { TotalResults = totalResults, SearchType = query.SearchType };
			foreach( var item in results )
			{
				output.Results.Add( new ResultWithExtraData<Course>()
				{
					Data = item, //Course?
					Extra = new JObject()
					{
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

		private void NormalizeQuery( SearchQuery query, string searchType, JObject debug = null )
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

		private string SanitizeKeywordString( string text )
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
