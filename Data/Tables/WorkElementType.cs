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
    
    public partial class WorkElementType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public WorkElementType()
        {
            this.RatingTask = new HashSet<RatingTask>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string CTID { get; set; }
        public System.Guid RowId { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingTask> RatingTask { get; set; }
    }
}
