using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	class Course : BaseObject
	{
		public string Name { get; set; }
		public string CodedNotation { get; set; }
		public Reference<Organization> CurriculumControlAuthority { get; set; }
		public List<Reference<TrainingTask>> Teaches { get; set; }
		public Reference<ReferenceResource> HasReferenceResource { get; set; }
	}
}
