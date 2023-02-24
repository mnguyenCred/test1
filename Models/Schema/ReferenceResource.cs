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
		//Equivalent RDF Type
		public const string RDFType = "navy:ReferenceResource";

		public ReferenceResource()
		{
			ReferenceType = new List<Guid>();
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
		/// Publication Date for this Reference Resource<br />
		/// From Column: Date of Source<br />
		/// 22-03-23 mparsons - no longer convert to date format, retain original
		/// </summary>
		public string PublicationDate { get; set; }

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

	}
}
