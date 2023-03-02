using System;
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
			Skip = 0;
			Take = 50;
			SortOrder = new List<SortOrderItem>();
		}
		public int Skip { get; set; }
		public int Take { get; set; }
		public List<SortOrderItem> SortOrder { get; set; }

		/// <summary>
		/// List of Filters (including keywords) for this Query
		/// </summary>
		public List<SearchFilter> Filters { get; set; }

		//Helper methods
		public SearchFilter GetFilterByName( string name )
		{
			return Filters?.FirstOrDefault( m => m.Name.ToLower() == name.ToLower() );
		}
		public SearchFilter GetFilterByID( int id )
		{
			return Filters?.FirstOrDefault( m => m.Id == id );
		}
		public string GetFilterTextByName( string name, string returnValueIfNull = null )
		{
			return GetFilterByName( name )?.Text ?? returnValueIfNull;
		}
		public List<int> GetFilterIDsByName( string name, List<int> returnValueIfNull = null )
		{
			return GetFilterByName( name )?.ItemIds ?? returnValueIfNull;
		}
		public List<Guid> GetFilterGUIDsByName( string name, List<Guid> returnValueIfNull = null )
		{
			return GetFilterByName( name )?.ItemGuids ?? returnValueIfNull;
		}
		public string GetFilterTextByID( int id, string returnValueIfNull = null )
		{
			return GetFilterByID( id )?.Text ?? returnValueIfNull;
		}
		public List<int> GetFilterIDsByID( int id, List<int> returnValueIfNull = null )
		{
			return GetFilterByID( id )?.ItemIds ?? returnValueIfNull;
		}
		public List<Guid> GetFilterGUIDsByID( int id, List<Guid> returnValueIfNull = null )
		{
			return GetFilterByID( id )?.ItemGuids ?? returnValueIfNull;
		}
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

		/// <summary>
		/// If the filter is used to select one or more items from a list, then the GUIDs for the items are included here
		/// </summary>
		public List<Guid> ItemGuids { get; set; }

		/// <summary>
		/// Indicates whether this filter should be treated as a negation filter, e.g. "where NOT ..."
		/// </summary>
		public bool IsNegation { get; set; }

		/// <summary>
		/// Experimental!
		/// </summary>
		public List<string> CustomColumns { get; set; }
	}
	//

	public class SortOrderItem
	{
		/// <summary>
		/// Name of the column to sort by. Currently uses the same names as the properties in the UploadableRow class.
		/// </summary>
		public string Column { get; set; }
		/// <summary>
		/// Whether to sort this item ascending or descending
		/// </summary>
		public bool Ascending { get; set; }
	}
	//

	public class SearchResultSet<T>
	{
		public SearchResultSet()
		{
			Results = new List<T>();
			RelatedResources = new List<JObject>();
			Debug = new JObject();
			ExtraData = new JObject();
		}
		public string SearchType { get; set; }
		public int TotalResults { get; set; }
		public JObject ExtraData { get; set; }
		public List<T> Results { get; set; }
		public List<JObject> RelatedResources { get; set; }
		public JObject Debug { get; set; }
	}
	//

	public class ResultWithExtraData<T>
	{
		public T Data { get; set; }
		public JObject Extra { get; set; }
	}
	//

}
