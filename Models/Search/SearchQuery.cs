﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

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

	public class SearchResultSet<T>
	{
		public SearchResultSet()
		{
			Results = new List<T>();
		}
		public string SearchType { get; set; }
		public int TotalResults { get; set; }
		public List<T> Results { get; set; }
	}
	//

	public class ResultWithExtraData<T>
	{
		public T Data { get; set; }
		public JObject Extra { get; set; }
	}
	//

}
