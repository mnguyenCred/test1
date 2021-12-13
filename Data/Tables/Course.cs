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
    
    public partial class Course
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course()
        {
            this.Course_Task = new HashSet<Course_Task>();
        }
    
        public int Id { get; set; }
        public string CIN { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<int> CourseTypeId { get; set; }
        public Nullable<int> LifeCycleControlDocumentId { get; set; }
        public Nullable<int> AssessmentApproachId { get; set; }
        public Nullable<int> CurriculumControlAuthorityId { get; set; }
        public string CTID { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public string CourseType { get; set; }
        public string LifeCycleControlDocument { get; set; }
        public string CurriculumControlAuthority { get; set; }
        public string CurrentAssessmentApproach { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Task> Course_Task { get; set; }
    }
}
