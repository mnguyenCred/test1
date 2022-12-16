using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	/// <summary>
	/// ReferenceResource related to the Source property
	/// </summary>
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
		/// From Column: Date of Source<br />
		/// 22-03-23 mparsons - no longer convert to date format, retain original
		/// </summary>
		public string PublicationDate { get; set; }

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
		/// List of IDs for the Concepts for the Reference Types for this Reference Resource<br />
		/// From Column: Work Element Type
		/// </summary>
		public List<int> ReferenceTypeId { get; set; }

		/// <summary>
		/// GUID for the Concept for the Status Type for this Reference Resource<br />
		/// From Column: TBD<br />
		/// Obsolete: Does not appear to be necessary(?)
		/// </summary>
		[Obsolete]
		public Guid StatusType { get; set; }

		/// <summary>
		/// Maybe needed for import<br />
		/// Derived<br />
		/// Obsolete: Does not appear to be necessary(?)
		/// </summary>
		[Obsolete]
		public string StatusTypeId { get; set; }

	}
}
