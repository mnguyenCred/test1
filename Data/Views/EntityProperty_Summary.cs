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
    
    public partial class EntityProperty_Summary
    {
        public int EntityId { get; set; }
        public System.Guid EntityUid { get; set; }
        public int EntityTypeId { get; set; }
        public Nullable<int> EntityBaseId { get; set; }
        public string EntityBaseName { get; set; }
        public string Title { get; set; }
        public int PropertyValueId { get; set; }
        public int EntityPropertyId { get; set; }
        public string Property { get; set; }
        public string PropertySchemaName { get; set; }
        public string ParentSchemaName { get; set; }
        public int CategoryId { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public string Category { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public string Description { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsSubType1 { get; set; }
    }
}
