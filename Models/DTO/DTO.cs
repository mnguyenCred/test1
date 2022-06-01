using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

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
		public int Id { get; set; }
		public string CTID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string TypeLabel { get; set; }
		public string TypeURL { get; set; }
		public List<NamedString> Identifiers { get; set; }
	}
	public class NamedString : Utilities.NamedValue<string, string>
	{
		public NamedString( string key, string value ) : base( key, value ) { }
	}
	//
}
