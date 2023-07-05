using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Schema
{
	public class Concept : BaseObject
	{
		//Equivalent RDF Type
		public const string RDFType = "skos:Concept";

		/// <summary>
		/// Concept Scheme this Concept belongs to
		/// </summary>
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

		/// <summary>
		/// URI for the Concept Scheme in which this Concept is a member
		/// </summary>
		public string SchemeUri { get; set; }

		/// <summary>
		/// Indicates whether or not this Concept is active
		/// </summary>
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

		/// <summary>
		/// Reference to Broader Concepts by GUID
		/// </summary>
		public Guid BroadMatch { get; set; }
		
		/// <summary>
		/// Reference to Broader Concepts by ID
		/// </summary>
		public int BroadMatchId { get; set; }

		/// <summary>
		/// Helper (Name Preferred)
		/// </summary>
		public string NameOrCodedNotation() { return !string.IsNullOrWhiteSpace( Name ) ? Name : !string.IsNullOrWhiteSpace( CodedNotation ) ? CodedNotation : "Unknown Concept"; }

		/// <summary>
		/// Helper (CodedNotation Preferred)
		/// </summary>
		public string CodedNotationOrName() { return !string.IsNullOrWhiteSpace( CodedNotation ) ? CodedNotation : !string.IsNullOrWhiteSpace( Name ) ? Name : "Unknown Concept"; }

	}
	//
}
