using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	class RatingTask : BaseObject
	{
		public string Description { get; set; }
		public Reference<TrainingTask> HasTrainingTask { get; set; }
		public Reference<ReferenceResource> HasReferenceResource { get; set; }
		public Reference<Concept> PayGradeType { get; set; }
		public Reference<Concept> StatusType { get; set; }
		public Reference<Concept> ApplicabilityType { get; set; }
		public Reference<Concept> TrainingGapType { get; set; }
	}
}
