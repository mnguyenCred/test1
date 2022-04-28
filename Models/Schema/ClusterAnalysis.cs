using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Schema
{
    public class ClusterAnalysis : BaseObject
    {
        public int RatingTaskId { get; set; }
        public Guid RatingTaskRowId { get; set; }

        public int TrainingSolutionTypeId { get; set; }
        /// <summary>
        /// used from view
        /// </summary>
        public string TrainingSolution { get; set; }
        public Guid TrainingSolutionType { get; set; }
        public string ClusterAnalysisTitle { get; set; }
        public int RecommendedModalityId { get; set; }
        public Guid RecommendedModalityType { get; set; }
        /// <summary>
        /// used from view
        /// </summary>
        public string RecommendedModality { get; set; }
        public int DevelopmentSpecificationId { get; set; }
        /// <summary>
        /// used from view
        /// </summary>
        public string DevelopmentSpecification { get; set; }
        public Guid DevelopmentSpecificationType { get; set; }
        public string CandidatePlatform { get; set; }
        public string CFMPlacement { get; set; }


        public int PriorityPlacement { get; set; }
        public string PriorityPlacementLabel { get; set; }
        /// <summary>
        /// TBD
        /// </summary>
        public string DevelopmentRatio { get; set; }
        //
        /// <summary>
        /// Included with STG V1, but not later one
        /// possibly a decimal
        /// </summary>
        public decimal? EstimatedInstructionalTime { get; set; }
        public string EstimatedInstructionalTimeLabel { get; set; }
        /// <summary>
        /// duration in hours
        /// </summary>
        public int DevelopmentTime { get; set; }
        public string DevelopmentTimeLabel { get; set; }

        /// <summary>
        /// Development Cost (ROM)  
        /// Ratio time $100
        /// If the latter is consistent, then no need to store the info?
        /// Included with STG V1, but not later one
        /// </summary>
        public int DevelopmentCost{ get; set; }

        public string Notes { get; set; }

        /// <summary>
        /// used for general search
        /// </summary>
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
