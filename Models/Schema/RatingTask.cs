using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	/*
	 * Notes
	 * - where is work element type area? Need to confirm if it really is part of the source table/class
	 */
	public class RatingTask : BaseObject
	{
		public string Description { get; set; }
		public string Note { get; set; }
		public Reference<TrainingTask> HasTrainingTask { get; set; }
		public Reference<ReferenceResource> HasReferenceResource { get; set; } //Source Document
		//rank
		public Reference<Concept> PayGradeType { get; set; }
		//level
		//can be derived from rank, should the latter be part of the paygrade table?
		public Reference<Concept> StatusType { get; set; }
		public Reference<Concept> ApplicabilityType { get; set; } //Will this be at the RMTL task level, or standalone task level?
		public Reference<Concept> TrainingGapType { get; set; }
		//functional area
		public Reference<WorkRole> HasWorkRole { get; set; }
	}
}
