﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Schema
{
    public class ClusterAnalysis : BaseObject
    {
        public int RatingTaskId { get; set; }

        public int TrainingSolutionTypeId { get; set; }
        public string TrainingSolutionType { get; set; }
        public string ClusterAnalysisTitle { get; set; }
        public int RecommendedModalityId { get; set; }
        public string RecommendedModality { get; set; }

        public string DevelopmentSpecification { get; set; }
        public string CandidatePlatform { get; set; }
        public string CFMPlacement { get; set; }


        public string PriorityPlacement { get; set; }
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
        /// <summary>
        /// duration in hours
        /// </summary>
        public int DevelopmentTime { get; set; }

        /// <summary>
        /// Development Cost (ROM)  
        /// Ratio time $100
        /// If the latter is consistent, then no need to store the info?
        /// Included with STG V1, but not later one
        /// </summary>
        public int DevelopmentCost{ get; set; }

        public string Notes { get; set; }

    }
}
