//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Data.Views
{
    using System;
    using System.Collections.Generic;
    
    public partial class RatingTaskSummary
    {
        public int Id { get; set; }
        public int RankId { get; set; }
        public string Rank { get; set; }
        public int LevelId { get; set; }
        public string Level { get; set; }
        public string FunctionalArea { get; set; }
        public string SourceDate { get; set; }
        public Nullable<int> WorkElementTypeId { get; set; }
        public string WorkElementType { get; set; }
        public Nullable<int> TaskApplicabilityId { get; set; }
        public string TaskApplicability { get; set; }
        public Nullable<int> FormalTrainingGapId { get; set; }
        public string FormalTrainingGap { get; set; }
        public string CIN { get; set; }
        public string CourseName { get; set; }
        public Nullable<int> TrainingTaskId { get; set; }
        public string CurriculumControlAuthority { get; set; }
        public string LifeCycleControlDocument { get; set; }
        public string Notes { get; set; }
        public string CodedNotation { get; set; }
        public Nullable<System.Guid> PayGradeType { get; set; }
        public Nullable<System.Guid> HasReferenceResource { get; set; }
        public Nullable<System.Guid> ReferenceType { get; set; }
        public Nullable<System.Guid> ApplicabilityType { get; set; }
        public Nullable<System.Guid> TrainingGapType { get; set; }
        public Nullable<System.Guid> HasTrainingTask { get; set; }
        public string CTID { get; set; }
        public System.Guid RowId { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public string TrainingTask { get; set; }
        public string RatingTask { get; set; }
        public string Ratings { get; set; }
        public string BilletTitles { get; set; }
        public Nullable<int> ReferenceResourceId { get; set; }
        public string ReferenceResource { get; set; }
        public Nullable<int> CourseId { get; set; }
        public Nullable<System.Guid> CourseUID { get; set; }
        public string CourseTypes { get; set; }
        public string AssessmentMethodTypes { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.Guid> CreatedByUID { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.Guid> ModifiedByUID { get; set; }
        public string RankName { get; set; }
        public Nullable<int> CurriculumControlAuthorityId { get; set; }
        public Nullable<System.Guid> CurriculumControlAuthorityUID { get; set; }
        public string WorkElementTypeAlternateName { get; set; }
        public int WorkElementTypeOrder { get; set; }
        public string RatingName { get; set; }
        public Nullable<System.Guid> HasRating { get; set; }
        public Nullable<int> ratingId { get; set; }
        public string CurrentAssessmentApproach { get; set; }
        public Nullable<int> TrainingSolutionTypeId { get; set; }
        public string TrainingSolutionType { get; set; }
        public string ClusterAnalysisTitle { get; set; }
        public Nullable<int> RecommendedModalityId { get; set; }
        public string RecommendedModality { get; set; }
        public string RecommentedModalityCodedNotation { get; set; }
        public Nullable<int> DevelopmentSpecificationId { get; set; }
        public string DevelopmentSpecification { get; set; }
        public string CandidatePlatform { get; set; }
        public string CFMPlacement { get; set; }
        public Nullable<int> PriorityPlacement { get; set; }
        public string DevelopmentRatio { get; set; }
        public Nullable<decimal> EstimatedInstructionalTime { get; set; }
        public Nullable<int> DevelopmentTime { get; set; }
        public string ClusterAnalysisNotes { get; set; }
        public Nullable<System.DateTime> ClusterAnalysisLastUpdated { get; set; }
    }
}
