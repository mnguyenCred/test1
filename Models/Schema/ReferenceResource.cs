using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	class ReferenceResource : BaseObject
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string CodedNotation { get; set; }
		public string Note { get; set; }
		public string VersionIdentifier { get; set; }
		public DateTime PublicationDate { get; set; }
		public List<string> SubjectWebpage { get; set; }
		public Reference<Concept> ReferenceType { get; set; }
		public Reference<Concept> StatusType { get; set; }
	}
}
