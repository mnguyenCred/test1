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
    
    public partial class RatingContext
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RatingContext()
        {
            this.RatingContext_WorkRole = new HashSet<RatingContext_WorkRole>();
        }
    
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public int RatingId { get; set; }
        public int RatingTaskId { get; set; }
        public Nullable<int> BilletTitleId { get; set; }
        public int PayGradeTypeId { get; set; }
        public Nullable<int> TaskApplicabilityId { get; set; }
        public Nullable<int> FormalTrainingGapId { get; set; }
        public Nullable<int> TrainingTaskId { get; set; }
        public Nullable<int> ClusterAnalysisId { get; set; }
        public Nullable<int> TaskStatusId { get; set; }
        public string CodedNotation { get; set; }
        public string Notes { get; set; }
        public string CTID { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
    
        public virtual ClusterAnalysis ClusterAnalysis { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_Rank { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_TaskApplicability { get; set; }
        public virtual ConceptScheme_Concept ConceptScheme_TrainingGap { get; set; }
        public virtual Job Job { get; set; }
        public virtual Rating Rating { get; set; }
        public virtual RatingTask RatingTask { get; set; }
        public virtual TrainingTask TrainingTask { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingContext_WorkRole> RatingContext_WorkRole { get; set; }
    }
}
