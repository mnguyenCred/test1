﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ceNavyViewEntities : DbContext
    {
        public ceNavyViewEntities()
            : base("name=ceNavyViewEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<EntityProperty_Summary> EntityProperty_Summary { get; set; }
        public virtual DbSet<Entity_Reference_Summary> Entity_Reference_Summary { get; set; }
        public virtual DbSet<Activity_Summary> Activity_Summary { get; set; }
        public virtual DbSet<Account_Summary> Account_Summary { get; set; }
        public virtual DbSet<SubscriptionEntities> SubscriptionEntities { get; set; }
        public virtual DbSet<AspNetUserRoles_Summary> AspNetUserRoles_Summary { get; set; }
        public virtual DbSet<ConceptSchemeSummary> ConceptSchemeSummary { get; set; }
        public virtual DbSet<RatingLevelTaskSummary> RatingLevelTaskSummary { get; set; }
        public virtual DbSet<RmtlSummary> RmtlSummary { get; set; }
    }
}
