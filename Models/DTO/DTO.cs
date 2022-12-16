using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Models.DTO
{
	public class SimpleItem
	{
		public int Id { get; set; }
		public Guid RowId { get; set; }
		public string Name { get; set; }
		public string CodedNotation { get; set; }
		public string Description { get; set; }
	}
	//

	public class SimpleItemHelper
	{
		public static List<SimpleItem> GetSimpleItems<T>( List<T> source )
		{
			return source.Select( m => GetSimpleItem( m ) ).ToList();
		}
		//

		public static SimpleItem GetSimpleItem<T>( T source )
		{
			var sourceProperties = typeof( T ).GetProperties();
			var destinationProperties = typeof( SimpleItem ).GetProperties();
			var mappedItem = new SimpleItem();

			foreach ( var destinationProperty in destinationProperties )
			{
				var match = sourceProperties.FirstOrDefault( m => m.Name == destinationProperty.Name && m.PropertyType == destinationProperty.PropertyType );
				if ( match != null )
				{
					destinationProperty.SetValue( mappedItem, match.GetValue( source ) );
				}
			}

			return mappedItem;
		}
		//
	}
	//

	//Used with ~/Views/Shared/_DetailBasicInfo.cshtml
	public class DetailBasicInfoHelper
	{
		public DetailBasicInfoHelper() {
			PropertyList = new List<NamedString>();
		}
		public DetailBasicInfoHelper( Schema.BaseObject source )
		{
			PropertyList = new List<NamedString>();
			Id = source.Id;
			CTID = source.CTID;
			Created = source.Created;
			LastUpdated = source.LastUpdated;
			TypeLabel = source.GetType().Name;
			TypeURL = source.GetType().Name.ToLower();
		}

		public int Id { get; set; }
		public string CTID { get; set; }
		public string Name { get; set; }
		public DateTime Created { get; set; }
		public DateTime LastUpdated { get; set; }
		public string TypeLabel { get; set; }
		public string TypeURL { get; set; }
		public List<NamedString> PropertyList { get; set; }
	}
	//

	public class EditFormHelperV2
	{
		public EditFormHelperV2() { }
		public EditFormHelperV2( object mainData, JObject values = null )
		{
			MainData = JObject.FromObject( mainData );
			DataType = mainData.GetType().Name;
			Values = values;
		}

		public JObject MainData { get; set; }
		public string DataType { get; set; }
		public JObject Values { get; set; }
	}
	//

	public class NamedString : Utilities.NamedValue<string, string>
	{
		public NamedString( string key, string value ) : base( key, value ) { }
	}
	//

	public class LinkHelper
	{
		public static string GetDetailPageLink<T>( T source, Func<string, string> UrlDotContent, Func<T, string> GetLabelMethod ) where T : Schema.BaseObject
		{
			return source == null ? null : "<a href=\"" + UrlDotContent( "~/" + typeof( T ).Name + "/Detail/" + source.Id ) + "\">" + GetLabelMethod( source ) + "</a>";
		}
		//

		public static string GetDetailPageLinkList<T>( List<T> sources, Func<string, string> UrlDotContent, Func<T, string> GetLabelMethod, string beforeText = "", string joinerText = ", ", string afterText = "" ) where T : Schema.BaseObject
		{
			return sources == null || sources.Count() == 0 ? null : beforeText + string.Join( joinerText, sources.Select( source => GetDetailPageLink( source, UrlDotContent, GetLabelMethod ) ).ToList() ) + afterText;
		}
		//

	}
	//
}
