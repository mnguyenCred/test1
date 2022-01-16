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
			Filters = new List<SearchFilter>();
		}

		/// <summary>
		/// TBD
		/// </summary>
		public string SortOrder { get; set; }

		/// <summary>
		/// List of Filters (including keywords) for this Query
		/// </summary>
		public List<SearchFilter> Filters { get; set; }
	}
	//

	public class SearchFilter
	{
		public SearchFilter()
		{
			ItemIds = new List<int>();
		}

		/// <summary>
		/// Name (ideally short URI) for this Filter
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Integer ID for this Filter
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// If the filter is used for a keyword, then the text value is included here
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// If the filter is used to select one or more items from a list, then the numeric IDs for the items are included here
		/// </summary>
		public List<int> ItemIds { get; set; }
	}
	//

	public class SearchResultSet
	{
		public SearchResultSet()
		{
			Results = new List<SearchResult>();
		}

		public int TotalResults { get; set; }
		public List<SearchResult> Results { get; set; }
	}
	//

	public class SearchResult
	{
		//TBD
	}
	//
}
