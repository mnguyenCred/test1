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
    
    public partial class Course_Task
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_Task()
        {
            this.CourseTask_AssessmentType = new HashSet<CourseTask_AssessmentType>();
            this.RatingTask_HasTrainingTask = new HashSet<RatingTask_HasTrainingTask>();
            this.RatingTask_HasRatingContext = new HashSet<RatingTask_HasRatingContext>();
            this.RatingContext = new HashSet<RatingContext>();
        }
    
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public int CourseId { get; set; }
        public string Description { get; set; }
        public string CTID { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
    
        public virtual Course Course { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CourseTask_AssessmentType> CourseTask_AssessmentType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask_HasTrainingTask> RatingTask_HasTrainingTask { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask_HasRatingContext> RatingTask_HasRatingContext { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingContext> RatingContext { get; set; }
    }
}
