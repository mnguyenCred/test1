using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	class Course : BaseObject
	{
		public string Name { get; set; }
		public string CodedNotation { get; set; }
		public List<Reference<Organization>> CurriculumControlAuthority { get; set; }
		public List<Reference<TrainingTask>> HasTrainingTask { get; set; }
		public Reference<ReferenceResource> HasReferenceResource { get; set; }
		public Reference<Concept> CourseType { get; set; }
		public Reference<Concept> AssessmentMethodType { get; set; }
	}
}
