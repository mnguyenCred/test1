using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class ConceptScheme : BaseObject
	{
		/// <summary>
		/// Name of this Concept Scheme
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description for this Concept Scheme
		/// </summary>
		public string Description { get; set; }
		public string SchemaUri { get; set; }
		/// <summary>
		/// List of Concepts for this Concept Scheme<br />
		/// Caution: Data is embedded here
		/// </summary>
		public List<Concept> Concepts { get; set; } = new List<Concept>();
	}
	//

	public class Concept : BaseObject
	{
		public int ConceptSchemeId { get; set; }
		/// <summary>
		/// Preferred Label for this Concept<br />
		/// AKA skos:prefLabel
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description for this Concept<br />
		/// AKA skos:definition
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Notation for this Concept<br />
		/// AKA skos:notation
		/// </summary>
		public string CodedNotation { get; set; }

		/// <summary>
		/// Guid for the Concept Scheme in which this Concept is a member<br />
		/// AKA skos:inScheme
		/// </summary>
		public Guid InScheme { get; set; }
		public string SchemeUri { get; set; }

		public bool IsActive { get; set; }
		/// <summary>
		/// used for sort order.
		/// Most concepts will default to the same value(25). 
		/// Order by ListId, then name. Where concepts need to be order by other (essentially) name, use ListId to group. 
		/// </summary>
		public int ListId { get; set; }

		/// <summary>
		/// Helps figure out which value from the spreadsheet this Concept aligns to
		/// </summary>
		public string WorkElementType { get; set; }
	}
	//
}
