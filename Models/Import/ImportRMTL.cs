﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Import
{
    public partial class ImportRMTL
    {
        public int Id { get; set; }
        public string IndexIdentifier { get; set; }
        public Nullable<int> Unique_Identifier { get; set; }
        public string Rating { get; set; }
        public string Rank { get; set; }
        public string RankLevel { get; set; }
        public string Billet_Title { get; set; }
        public string Functional_Area { get; set; }
        public string Source { get; set; }
        public string Date_of_Source { get; set; }
        public string Work_Element_Type { get; set; }
        public string Work_Element_Task { get; set; }
        public string Task_Applicability { get; set; }
        public string Formal_Training_Gap { get; set; }
        public string CIN { get; set; }
        public string Course_Name { get; set; }
        public string Course_Type { get; set; }
        public string Curriculum_Control_Authority { get; set; }
        public string Life_Cycle_Control_Document { get; set; }
        public string Task_Statement { get; set; }
        public string Current_Assessment_Approach { get; set; }
        public string TaskNotes { get; set; }
        public string Training_Solution_Type { get; set; }
        public string Cluster_Analysis_Title { get; set; }
        public string Recommended_Modality { get; set; }
        public string Development_Specification { get; set; }
        public string Candidate_Platform { get; set; }
        public string CFM_Placement { get; set; }
        public string Priority_Placement { get; set; }
        public string Development_Ratio { get; set; }
        public string Development_Time { get; set; }
        public string ClusterAnalysisNotes { get; set; }
        public Nullable<int> RatingId { get; set; }
        public Nullable<int> RankId { get; set; }
        public Nullable<int> LevelId { get; set; }
        public Nullable<int> BilletTitleId { get; set; }
        public Nullable<int> FunctionalAreaId { get; set; }
        public Nullable<int> SourceId { get; set; }
        public Nullable<int> WorkElementTypeId { get; set; }
        public Nullable<int> RatingLevelTaskId { get; set; }
        public Nullable<int> TaskApplicabilityId { get; set; }
        public Nullable<int> FormalTrainingGapId { get; set; }
        public Nullable<int> CourseId { get; set; }
        public Nullable<int> CourseTaskId { get; set; }
        public string Message { get; set; }
        public System.DateTime ImportDate { get; set; }
    }
}
