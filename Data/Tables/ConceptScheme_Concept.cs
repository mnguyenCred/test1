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
    
    public partial class ConceptScheme_Concept
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ConceptScheme_Concept()
        {
            this.ClusterAnalysis_HasCandidatePlatform = new HashSet<ClusterAnalysis_HasCandidatePlatform>();
            this.CourseContext_AssessmentType = new HashSet<CourseContext_AssessmentType>();
            this.Course_CourseType = new HashSet<Course_CourseType>();
            this.Course = new HashSet<Course>();
            this.RatingContext = new HashSet<RatingContext>();
            this.RatingContext1 = new HashSet<RatingContext>();
            this.RatingContext2 = new HashSet<RatingContext>();
            this.ReferenceResource_ReferenceType = new HashSet<ReferenceResource_ReferenceType>();
            this.RatingContext11 = new HashSet<RatingContext>();
            this.ClusterAnalysis1 = new HashSet<ClusterAnalysis>();
            this.ClusterAnalysis2 = new HashSet<ClusterAnalysis>();
            this.ClusterAnalysis3 = new HashSet<ClusterAnalysis>();
            this.ClusterAnalysis4 = new HashSet<ClusterAnalysis>();
            this.RatingTask = new HashSet<RatingTask>();
            this.ClusterAnalysis_CFMPlacementType = new HashSet<ClusterAnalysis_CFMPlacementType>();
        }
    
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public int ConceptSchemeId { get; set; }
        public string Name { get; set; }
        public string WorkElementType { get; set; }
        public string CodedNotation { get; set; }
        public string AlternateLabel { get; set; }
        public string Description { get; set; }
        public int ListId { get; set; }
        public bool IsActive { get; set; }
        public string CTID { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public Nullable<int> BroadMatchId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterAnalysis_HasCandidatePlatform> ClusterAnalysis_HasCandidatePlatform { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CourseContext_AssessmentType> CourseContext_AssessmentType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_CourseType> Course_CourseType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course> Course { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingContext> RatingContext { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingContext> RatingContext1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingContext> RatingContext2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReferenceResource_ReferenceType> ReferenceResource_ReferenceType { get; set; }
        public virtual ConceptScheme ConceptScheme { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingContext> RatingContext11 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterAnalysis> ClusterAnalysis1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterAnalysis> ClusterAnalysis2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterAnalysis> ClusterAnalysis3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterAnalysis> ClusterAnalysis4 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask> RatingTask { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterAnalysis_CFMPlacementType> ClusterAnalysis_CFMPlacementType { get; set; }
    }
}
