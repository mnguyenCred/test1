//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Data.TablesOLD
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_Task
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public string TaskStatement { get; set; }
        public string CTID { get; set; }
    
        public virtual Course Course { get; set; }
    }
}
