using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Search
{
    public class SearchQuery
    {
		public SearchQuery()
		{
			SearchType = "ratingtask";
			StartPage = 1;
			PageSize = 50;
			Keywords = "";
			//relevance is not implemented yet, so latest?
			SortOrder = "relevance";
		}
		public string SearchType { get; set; }
		public int StartPage { get; set; }
		public int PageSize { get; set; }
		public string Keywords { get; set; }
		public string SortOrder { get; set; }
		//??
		public List<KeywordFilter> KeywordFilters { get; set; } = new List<KeywordFilter>();
		public List<Guid> Ratings { get; set; }
		public List<int> Paygrade { get; set; } = new List<int>();
		public List<int> SourceType { get; set; } = new List<int>();
		public List<int> ApplicabilityType { get; set; } = new List<int>();
		public List<int> CourseType { get; set; } = new List<int>();
	}

	public class KeywordFilter
    {
		/// <summary>
		/// Possible values include
		/// - RatingTaskKeyword
		/// - TrainingTaskKeyword
		/// - BilletTitleKeyword
		/// </summary>
		public string Name { get; set; }
		public string Value { get; set; }
    }
}
