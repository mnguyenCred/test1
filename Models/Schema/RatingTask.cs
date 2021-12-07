using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class RatingTask : BaseObject
	{
		public string Description { get; set; }
		public string Note { get; set; }
		public Reference<TrainingTask> HasTrainingTask { get; set; }
		public Reference<ReferenceResource> HasReferenceResource { get; set; } //Source Document
		public Reference<Concept> PayGradeType { get; set; }
		public Reference<Concept> StatusType { get; set; }
		public Reference<Concept> ApplicabilityType { get; set; } //Will this be at the RMTL task level, or standalone task level?
		public Reference<Concept> TrainingGapType { get; set; }
		public Reference<WorkRole> HasWorkRole { get; set; }
	}
}
