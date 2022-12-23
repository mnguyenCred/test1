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
		/// Included with STG V1, but not later one<br />
		/// possibly a decimal
		/// </summary>
		public decimal EstimatedInstructionalTime { get; set; }

		/// <summary>
		/// Development Time<br />
		/// Duration in hours
		/// </summary>
		public decimal DevelopmentTime { get; set; }

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

		/// <summary>
		/// RowID for the Rating Task this Cluster Analysis relates to.<br />
		/// Necessary to have this to narrow down Cluster Analyses to just a single instance when the RatingContext object has not yet been determined (e.g., during upload).
		/// </summary>
		public Guid HasRatingTask { get; set; }

		/// <summary>
		/// Integer ID for the Rating Task this Cluster Analysis relates to.
		/// </summary>
		public int HasRatingTaskId { get; set; }

		/// <summary>
		/// RowID for the Rating this Cluster Analysis is contextualized by.<br />
		/// Necessary to have this to narrow down Cluster Analyses to just a single instance when the RatingContext object has not yet been determined (e.g., during upload).
		/// </summary>
		public Guid HasRating { get; set; }

		/// <summary>
		/// Integer ID for the Rating this Cluster Analysis relates to.
		/// </summary>
		public int HasRatingId { get; set; }

		/// <summary>
		/// RowID for the Billet Title this Cluster Analysis is contextualized by.<br />
		/// Necessary to have this to narrow down Cluster Analyses to just a single instance when the RatingContext object has not yet been determined (e.g., during upload).
		/// </summary>
		public Guid HasBilletTitle { get; set; }

		/// <summary>
		/// Integer ID for the Billet Title this Cluster Analysis relates to.
		/// </summary>
		public int BilletTitleId { get; set; }

		/// <summary>
		/// RowID for the Work Role this Cluster Analysis is contextualized by.<br />
		/// Necessary to have this to narrow down Cluster Analyses to just a single instance when the RatingContext object has not yet been determined (e.g., during upload).
		/// </summary>
		public Guid HasWorkRole { get; set; }

		/// <summary>
		/// Integer ID for the Work Role this Cluster Analysis relates to.
		/// </summary>
		public int WorkRoleId { get; set; }

		//Obsolete properties
		//or for display purposes

		/// <summary>
		/// Obsolete: Use Cluster Analysis Title (the class) to provide this<br />
		/// MP - still need something to reference for messages/issues other than a Guid
		/// </summary>
		[Obsolete]
        public string ClusterAnalysisTitle { get; set; }

		/// <summary>
		/// Used from view<br />
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
		/// Obsolete: Use DevelopmentRatioType/DevelopmentRatioTypeId instead
		/// </summary>
		[Obsolete]
		public string DevelopmentRatio { get; set; }

	}
}
