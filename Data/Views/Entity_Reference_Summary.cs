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
    
    public partial class Entity_Reference_Summary
    {
        public int PK { get; set; }
        public int EntityId { get; set; }
        public int EntityTypeId { get; set; }
        public string EntityType { get; set; }
        public System.Guid EntityUid { get; set; }
        public string EntityName { get; set; }
        public Nullable<int> EntityBaseId { get; set; }
        public int EntityReferenceId { get; set; }
        public int CategoryId { get; set; }
        public Nullable<int> PropertyValueId { get; set; }
        public string Title { get; set; }
        public string TextValue { get; set; }
        public string PropertyValue { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
    }
}
