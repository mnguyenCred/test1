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
            this.RatingTask_HasJob = new HashSet<RatingTask_HasJob>();
            this.RatingTask_HasRating = new HashSet<RatingTask_HasRating>();
            this.RatingTask_HasTrainingTask = new HashSet<RatingTask_HasTrainingTask>();
            this.RatingTask_WorkRole = new HashSet<RatingTask_WorkRole>();
            this.RmtlProject_BilletTask = new HashSet<RmtlProject_BilletTask>();
            this.ClusterAnalysis = new HashSet<ClusterAnalysis>();
        }
    
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public int RankId { get; set; }
        public int LevelId { get; set; }
        public Nullable<int> ReferenceResourceId { get; set; }
        public Nullable<int> WorkElementTypeId { get; set; }
        public string Description { get; set; }
        public Nullable<int> TaskApplicabilityId { get; set; }
        public Nullable<int> TaskStatusId { get; set; }
        public Nullable<int> FormalTrainingGapId { get; set; }
        public string CodedNotation { get; set; }
        public Nullable<int> TrainingTaskId { get; set; }
        public string CTID { get; set; }
        public string Notes { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
    
        public virtual ConceptScheme_Concept ConceptScheme_Concept { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_Rank { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_Applicability { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_TrainingGap { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_WorkElementType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask_HasJob> RatingTask_HasJob { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask_HasRating> RatingTask_HasRating { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask_HasTrainingTask> RatingTask_HasTrainingTask { get; set; }
        public virtual ReferenceResource ToReferenceResource { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask_WorkRole> RatingTask_WorkRole { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RmtlProject_BilletTask> RmtlProject_BilletTask { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterAnalysis> ClusterAnalysis { get; set; }
    }
}
