using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class ReferenceResource : BaseObject
	{
		public ReferenceResource()
		{
			ReferenceType = new List<Guid>();
			SubjectWebpage = new List<string>();
		}

		/// <summary>
		/// Name of this Reference Resource<br />
		/// From Column: Source (in the context of Rating Tasks); Life-Cycle Control Document (in the context of Courses)
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description for this Reference Resource<br />
		/// From Column: TBD
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Coded Notation for this Reference Resource<br />
		/// From Column: TBD
		/// </summary>
		public string CodedNotation { get; set; }

		/// <summary>
		/// Note for this Reference Resource (It's not clear whether this belongs here or on Rating Task)<br />
		/// From Column: Notes
		/// </summary>
		public string Note { get; set; }

		/// <summary>
		/// Version Identifier for this Reference Resource<br />
		/// From Column: TBD
		/// </summary>
		public string VersionIdentifier { get; set; }

		/// <summary>
		/// Publication Date for this Reference Resource<br />
		/// From Column: Date of Source
		/// </summary>
		public DateTime PublicationDate { get; set; }

		/// <summary>
		/// Subject Webpage for this Reference Resource<br />
		/// From Column: TBD
		/// </summary>
		public List<string> SubjectWebpage { get; set; }

		/// <summary>
		/// List of GUIDs for the Concepts for the Reference Types for this Reference Resource<br />
		/// From Column: Work Element Type
		/// </summary>
		public List<Guid> ReferenceType { get; set; }

		/// <summary>
		/// GUID for the Concept for the Status Type for this Reference Resource<br />
		/// From Column: TBD
		/// </summary>
		public Guid StatusType { get; set; }


		//maybe needed for import
		//Derived
		public string StatusTypeId { get; set; }
		//will not be used later as ReferenceType is a list. Using for initial playing.
		public string ReferenceTypeId { get; set; }
	}
}
