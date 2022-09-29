//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Data.Tables
{
    using System;
    using System.Collections.Generic;
    
    public partial class RatingTask_HasRatingContext
    {
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public int RatingTaskId { get; set; }
        public int RatingId { get; set; }
        public Nullable<int> FormalTrainingGapId { get; set; }
        public Nullable<int> TrainingTaskId { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<int> BilletTitleId { get; set; }
        public int RankId { get; set; }
        public Nullable<int> WorkRoleId { get; set; }
        public Nullable<int> TaskApplicabilityId { get; set; }
        public Nullable<int> TaskStatusId { get; set; }
        public string CodedNotation { get; set; }
        public string CTID { get; set; }
        public string Notes { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
    
        public virtual Course_Task Course_Task { get; set; }
        public virtual Rating Rating { get; set; }
        public virtual RatingTask RatingTask { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_Concept { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_Concept1 { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_Concept2 { get; set; }
    }
}
