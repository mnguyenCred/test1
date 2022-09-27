using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Schema
{
    public class ClusterAnalysis : BaseObject
    {
		public ClusterAnalysis()
		{
			CandidatePlatformType = new List<Guid>();
			CandidatePlatformTypeId = new List<int>();
		}

		/// <summary>
		/// Priority Placement
		/// </summary>
        public int PriorityPlacement { get; set; }

        /// <summary>
        /// Estimated Instructional Time
        /// </summary>
        public decimal? EstimatedInstructionalTime { get; set; }

        /// <summary>
        /// Development Time<br />
		/// Duration in hours
        /// </summary>
        public int DevelopmentTime { get; set; }

 		/// <summary>
		/// GUID reference to an instance of the Cluster Analysis Title class<br />
		/// Allows grouping multiple Cluster Analysis instances under one Title, similar to how a Functional Area or a Billet Title allows grouping of Rating Tasks
		/// </summary>
		public Guid HasClusterAnalysisTitle { get; set; }

		/// <summary>
		/// Id reference to an instance of the Cluster Analysis Title class<br />
		/// Allows grouping multiple Cluster Analysis instances under one Title, similar to how a Functional Area or a Billet Title allows grouping of Rating Tasks
		/// </summary>
		public int HasClusterAnalysisTitleId { get; set; }
		
		/// <summary>
		/// GUID reference to a Training Solution Type Concept
		/// </summary>
        public Guid TrainingSolutionType { get; set; }

		/// <summary>
		/// Id reference to a Training Solution Type Concept
		/// </summary>
        public int TrainingSolutionTypeId { get; set; }

		/// <summary>
		/// GUID reference to a Recommended Modality Type Concept
		/// </summary>
		public Guid RecommendedModalityType { get; set; }

		/// <summary>
		/// Id reference to a Recommended Modality Type Concept
		/// </summary>
		public int RecommendedModalityTypeId { get; set; }

		/// <summary>
		/// GUID reference to a Development Specification Type Concept
		/// </summary>
		public Guid DevelopmentSpecificationType { get; set; }

		/// <summary>
		/// Id reference to a Development Specification Type Concept
		/// </summary>
		public int DevelopmentSpecificationTypeId { get; set; }

		/// <summary>
		/// List of GUID references to Candidate Platform Type Concepts
		/// </summary>
		public List<Guid> CandidatePlatformType { get; set; }

		/// <summary>
		/// List of Id references to Candidate Platform Type Concepts
		/// </summary>
		public List<int> CandidatePlatformTypeId { get; set; }

		/// <summary>
		/// GUID reference to a Development Ratio Type Concept<br />
		/// Should correspond to the ICW ratio, but may not always do so.<br />
		/// Flag mismatches between this and ICW ratio as warnings, not errors.
		/// </summary>
		public Guid DevelopmentRatioType { get; set; }

		/// <summary>
		/// Id reference to a Development Ratio Type Concept<br />
		/// Should correspond to the ICW ratio, but may not always do so.<br />
		/// Flag mismatches between this and ICW ratio as warnings, not errors.
		/// </summary>
		public int DevelopmentRatioTypeId { get; set; }

		/// <summary>
		/// GUID reference to a CFM Placement Type Concept
		/// </summary>
		public Guid CFMPlacementType { get; set; }

		/// <summary>
		/// Id reference to a CFM Placement Type Concept
		/// </summary>
		public int CFMPlacementTypeId { get; set; }


		//Obsolete properties

        /// <summary>
        /// Used for general search<br />
		/// Obsolete: Use Cluster Analysis Title (the class) to provide the "name"
        /// </summary>
		[Obsolete]
        public string Name { get; set; }

		/// <summary>
		/// Obsolete: Use Cluster Analysis Title (the class) to provide this
		/// </summary>
		[Obsolete]
        public string ClusterAnalysisTitle { get; set; }

		/// <summary>
		/// Used from view
		/// Obsolete: Use TrainingSolutionType/TrainingSolutionTypeId instead
		/// </summary>
		[Obsolete]
        public string TrainingSolution { get; set; }

        /// <summary>
        /// Used from view<br />
		/// Obsolete: Use RecommendedModalityType/RecommendedModalityTypeId instead
        /// </summary>
		[Obsolete]
        public string RecommendedModality { get; set; }

		/// <summary>
		/// Used from view<br />
		/// Obsolete: Use DevelopmentSpecificationType/DevelopmentSpecificationTypeId instead
		/// </summary>
		[Obsolete]
		public string DevelopmentSpecification { get; set; }

		/// <summary>
		/// Obsolete: Belongs on RatingContext
		/// </summary>
		[Obsolete]
        public string Notes { get; set; }

		/// <summary>
		/// Obsolete: Rating Context should reference Cluster Analysis
		/// </summary>
		[Obsolete]
		public int RatingTaskId { get; set; }

		/// <summary>
		/// Obsolete: Rating Context should reference Cluster Analysis
		/// </summary>
		[Obsolete]
		public Guid RatingTaskRowId { get; set; }

		/// <summary>
		/// Obsolete: Use CandidatePlatformType/CandidatePlatformTypeId instead
		/// </summary>
		[Obsolete]
		public string CandidatePlatform { get; set; }

		/// <summary>
		/// Obsolete: Use CFMPlacementType/CFMPlacementTypeId instead
		/// </summary>
		[Obsolete]
		public string CFMPlacement { get; set; }


		/// <summary>
		/// Development Cost (ROM)<br />
		/// Ratio time $100<br />
		/// If the latter is consistent, then no need to store the info?<br />
		/// Included with STG V1, but not later one<br />
		/// Obsolete: Not used. This was probably included with the wrong column name in an early spreadsheet.
		/// </summary>
		[Obsolete]
		public int DevelopmentCost { get; set; }

		/// <summary>
		/// Obsolete: Use DevelopmentRatioType/DevelopmentRatioTypeId instead
		/// </summary>
		[Obsolete]
		public string DevelopmentRatio { get; set; }

		/// <summary>
		/// Obsolete: Does not seem to be needed(?)
		/// </summary>
		[Obsolete]
		public string PriorityPlacementLabel { get; set; } //Where is this used?

		/// <summary>
		/// Obsolete: Does not seem to be needed(?)
		/// </summary>
		[Obsolete]
		public string DevelopmentTimeLabel { get; set; } //Where is this used?

		/// <summary>
		/// Obsolete: Does not seem to be needed(?)
		/// </summary>
		[Obsolete]
		public string EstimatedInstructionalTimeLabel { get; set; } //Where is this used?

	}
}
