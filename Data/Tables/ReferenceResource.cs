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
    
    public partial class ReferenceResource
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ReferenceResource()
        {
            this.ReferenceResource_ReferenceType = new HashSet<ReferenceResource_ReferenceType>();
        }
    
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CodedNotation { get; set; }
        public string PublicationDate { get; set; }
        public string SubjectWebpage { get; set; }
        public string VersionIdentifier { get; set; }
        public string Note { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public string CTID { get; set; }
        public Nullable<int> StatusTypeId { get; set; }
        public Nullable<int> ReferenceTypeId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReferenceResource_ReferenceType> ReferenceResource_ReferenceType { get; set; }
    }
}
