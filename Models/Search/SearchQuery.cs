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
			SearchType = "ratingtask";
			PageNumber = 1;
			PageSize = 50;
			Keywords = "";
			//relevance is not implemented yet, so latest?
			SortOrder = "relevance";
		}
		/// <summary>
		/// While starting with RatingTask search, could be other searches in the future
		/// </summary>
		public string SearchType { get; set; }

		/// <summary>
		/// General Keywords
		/// For the main RatingTask search, there will be different keyword types. Including here for future localized searches (course, or training task, etc.)
		/// </summary>
		public string Keywords { get; set; }
		//
		public int PageNumber { get; set; }

		public int PageSize { get; set; }

		public int TotalResults { get; set; }
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
		public string SearchType { get; set; }
		public int TotalResults { get; set; }
		public List<SearchResult> Results { get; set; }
	}
	//

	public class SearchResult
	{
		//TBD
		public string Name { get; set; }
		public string FriendlyName { get; set; }
		public string Description { get; set; }
		public int RecordId { get; set; }
		public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
		public List<TagSet> Tags { get; set; } = new List<TagSet>();
	}
	//

	public class TagSet
	{
		public TagSet()
		{
			Items = new List<TagItem>();
		}
		public string Schema { get; set; } //
		public string Method { get; set; } //embedded, ajax, link
		public string Label { get; set; }
		public int CategoryId { get; set; }
		public List<TagItem> Items { get; set; }
		public int Count { get; set; }
	}
	//

	public class TagItem
	{
		public int CodeId { get; set; } //Should be the record integer ID from the code table itself
		public string Schema { get; set; }  //Used when CodeId is not viable
		public string Label { get; set; } //Used when all else fails
		public string Description { get; set; }
	}
}
