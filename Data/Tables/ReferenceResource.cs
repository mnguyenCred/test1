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
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CodedNotation { get; set; }
        public string PublicationDate { get; set; }
        public string SubjectWebpage { get; set; }
        public Nullable<int> StatusType { get; set; }
        public string VersionIdentifier { get; set; }
        public Nullable<int> ReferenceType { get; set; }
        public string Note { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public string CTID { get; set; }
    }
}