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
    
    public partial class RatingTask_HasRating
    {
        public int Id { get; set; }
        public System.Guid RowId { get; set; }
        public int RatingTaskId { get; set; }
        public int RatingId { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
    
        public virtual Rating Rating { get; set; }
        public virtual RatingTask RatingTask { get; set; }
    }
}
