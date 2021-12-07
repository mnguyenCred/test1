using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	//add functional area/work role
	class RatingTask : BaseObject
	{
		public string Description { get; set; }
		public string Note { get; set; }
		public Reference<TrainingTask> HasTrainingTask { get; set; }
		//source
		public Reference<ReferenceResource> HasReferenceResource { get; set; }
		public Reference<Concept> PayGradeType { get; set; }
		public Reference<Concept> StatusType { get; set; }
		//will this be at the rmtl task level, or standalone task level?
		public Reference<Concept> ApplicabilityType { get; set; }
		public Reference<Concept> TrainingGapType { get; set; }
	}
}
