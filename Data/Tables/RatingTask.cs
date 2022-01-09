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
    
    public partial class RatingTask
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RatingTask()
        {
            this.RatingTask_HasRating = new HashSet<RatingTask_HasRating>();
            this.RmtlProjectBilletTask = new HashSet<RmtlProjectBilletTask>();
        }
    
        public int Id { get; set; }
        public string CodedNotation { get; set; }
        public int RankId { get; set; }
        public int LevelId { get; set; }
        public int FunctionalAreaId { get; set; }
        public Nullable<int> SourceId { get; set; }
        public Nullable<int> WorkElementTypeId { get; set; }
        public string WorkElementTask { get; set; }
        public Nullable<int> TaskApplicabilityId { get; set; }
        public Nullable<int> TaskStatusId { get; set; }
        public Nullable<int> FormalTrainingGapId { get; set; }
        public string Notes { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public string CTID { get; set; }
        public Nullable<int> TrainingTaskId { get; set; }
        public System.Guid RowId { get; set; }
    
        public virtual ConceptScheme_Concept ConceptScheme_Rank { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_Level { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_Applicability { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_TrainingGap { get; set; }
        public virtual Course_Task Course_Task { get; set; }
        public virtual FunctionalArea FunctionalArea { get; set; }
        public virtual Source Source { get; set; }
        public virtual WorkElementType WorkElementType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask_HasRating> RatingTask_HasRating { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RmtlProjectBilletTask> RmtlProjectBilletTask { get; set; }
    }
}
