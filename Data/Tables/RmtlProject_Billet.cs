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
    
    public partial class RmtlProject_Billet
    {
        public int Id { get; set; }
        public int RmtlProjectId { get; set; }
        public int JobId { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }
    
        public virtual Job Job { get; set; }
        public virtual RMTLProject RMTLProject { get; set; }
    }
}
