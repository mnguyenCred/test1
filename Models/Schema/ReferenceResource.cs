using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class ReferenceResource : BaseObject
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string CodedNotation { get; set; }
		public string Note { get; set; }
		public string VersionIdentifier { get; set; }
		public DateTime PublicationDate { get; set; }
		public List<string> SubjectWebpage { get; set; }
		public Guid ReferenceType { get; set; } //GUID for the Concept for the Reference Type for this Reference Resource
		public Guid StatusType { get; set; } //GUID for the Concept for the Status Type for this Reference Resource
	}
}
