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
    
    public partial class WorkRoleSummary
    {
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public string Name { get; set; }
        public int HasRatingTasks { get; set; }
        public string Description { get; set; }
        public string CodedNotation { get; set; }
        public string Version { get; set; }
        public string CTID { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
    }
}
