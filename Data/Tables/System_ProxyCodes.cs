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
    
    public partial class System_ProxyCodes
    {
        public int Id { get; set; }
        public string ProxyCode { get; set; }
        public int UserId { get; set; }
        public Nullable<bool> IsIdentityProxy { get; set; }
        public string ProxyType { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime ExpiryDate { get; set; }
        public Nullable<System.DateTime> AccessDate { get; set; }
    }
}
