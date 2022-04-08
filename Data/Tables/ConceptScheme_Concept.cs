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
            this.Course_CourseType = new HashSet<Course_CourseType>();
            this.Course = new HashSet<Course>();
            this.CourseTask_AssessmentType = new HashSet<CourseTask_AssessmentType>();
            this.Entity_Concept = new HashSet<Entity_Concept>();
            this.RatingTask = new HashSet<RatingTask>();
            this.RatingTask1 = new HashSet<RatingTask>();
            this.RatingTask2 = new HashSet<RatingTask>();
            this.RatingTask3 = new HashSet<RatingTask>();
            this.RatingTask4 = new HashSet<RatingTask>();
            this.ReferenceResource_ReferenceType = new HashSet<ReferenceResource_ReferenceType>();
            this.ClusterAnalysis = new HashSet<ClusterAnalysis>();
            this.ClusterAnalysis1 = new HashSet<ClusterAnalysis>();
            this.ClusterAnalysis2 = new HashSet<ClusterAnalysis>();
        }
    
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public int ConceptSchemeId { get; set; }
        public string Name { get; set; }
        public string CTID { get; set; }
        public string CodedNotation { get; set; }
        public string AlternateLabel { get; set; }
        public string Description { get; set; }
        public Nullable<int> ListId { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public string WorkElementType { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_CourseType> Course_CourseType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course> Course { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CourseTask_AssessmentType> CourseTask_AssessmentType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Entity_Concept> Entity_Concept { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask> RatingTask { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask> RatingTask1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask> RatingTask2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask> RatingTask3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask> RatingTask4 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReferenceResource_ReferenceType> ReferenceResource_ReferenceType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterAnalysis> ClusterAnalysis { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterAnalysis> ClusterAnalysis1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClusterAnalysis> ClusterAnalysis2 { get; set; }
        public virtual ConceptScheme ConceptScheme { get; set; }
    }
}
