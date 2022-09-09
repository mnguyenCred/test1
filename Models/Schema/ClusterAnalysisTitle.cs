using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Schema
{
	public class ClusterAnalysisTitle : BaseObject
	{
		//Properties of the CAT itself
		public int? PriorityPlacement { get; set; }
		public decimal? EstimatedInstructionalTime { get; set; }
		public decimal? DevelopmentTimeInHours { get; set; }
		public string Notes { get; set; }

		//GUIDs referencing other entities
		public Guid TrainingSolutionType { get; set; }
		public Guid RecommendedModalityType { get; set; }
		public Guid DevelopmentSpecificationType { get; set; }
		public List<Guid> CandidatePlatformType { get; set; }
		public Guid DevelopmentRatioType { get; set; }

		//IDs referencing other entities
		public int TrainingSolutionTypeId { get; set; }
		public int RecommendedModalityTypeId { get; set; }
		public int DevelopmentSpecificationTypeId { get; set; }
		public List<int> CandidatePlatformTypeId { get; set; }
		public int DevelopmentRatioTypeId { get; set; }

		//Candidate Platform Type can be derived from the Pay Grade for the associated Rating Context
	}
	//

	public class PopulatedClusterAnalysisTitle : ClusterAnalysisTitle
	{
		//Populated data for other entities
		public Concept TrainingSolutionTypeData { get; set; }
		public Concept RecommendedModalityTypeData { get; set; }
		public Concept DevelopmentSpecificationTypeData { get; set; }
		public List<Concept> CandidatePlatformTypeData { get; set; }
		public Concept DevelopmentRatioTypeData { get; set; }
	}
	//
}
