using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class ConceptScheme : BaseObject
	{
		public string Name { get; set; }
		public string Description { get; set; }
	}
	//

	public class Concept : BaseObject
	{
		public string Label { get; set; }
		public string Description { get; set; }
		public string Notation { get; set; }
		public Guid InScheme { get; set; }
	}
	//
}
