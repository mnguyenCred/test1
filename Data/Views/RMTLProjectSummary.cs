//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Data.Views
{
    using System;
    using System.Collections.Generic;
    
    public partial class RMTLProjectSummary
    {
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public string CTID { get; set; }
        public string Name { get; set; }
        public int RatingId { get; set; }
        public string Rating { get; set; }
        public string RatingCodedNotation { get; set; }
        public string Description { get; set; }
        public Nullable<int> StatusId { get; set; }
        public string VersionControlIdentifier { get; set; }
        public int RmtlTasks { get; set; }
        public int ChangeProposals { get; set; }
        public string Notes { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public Nullable<System.DateTime> LastApproved { get; set; }
        public Nullable<int> LastApprovedById { get; set; }
        public Nullable<System.DateTime> LastPublished { get; set; }
        public Nullable<int> LastPublishedById { get; set; }
    }
}
