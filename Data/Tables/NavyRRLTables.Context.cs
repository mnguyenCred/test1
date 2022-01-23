﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class NavyRRLEntities : DbContext
    {
        public NavyRRLEntities()
            : base("name=NavyRRLEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<ActivityLog> ActivityLog { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<ConceptScheme> ConceptScheme { get; set; }
        public virtual DbSet<ConceptScheme_Concept> ConceptScheme_Concept { get; set; }
        public virtual DbSet<Course> Course { get; set; }
        public virtual DbSet<Course_Task> Course_Task { get; set; }
        public virtual DbSet<Job> Job { get; set; }
        public virtual DbSet<Organization> Organization { get; set; }
        public virtual DbSet<Rating> Rating { get; set; }
        public virtual DbSet<RMTLProject> RMTLProject { get; set; }
        public virtual DbSet<RmtlProject_Billet> RmtlProject_Billet { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<System_ProxyCodes> System_ProxyCodes { get; set; }
        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<RatingTask> RatingTask { get; set; }
        public virtual DbSet<RatingTask_HasRating> RatingTask_HasRating { get; set; }
        public virtual DbSet<ImportRMTL> ImportRMTL { get; set; }
        public virtual DbSet<ReferenceResource> ReferenceResource { get; set; }
        public virtual DbSet<ReferenceResource_ReferenceType> ReferenceResource_ReferenceType { get; set; }
        public virtual DbSet<Codes_EntityType> Codes_EntityType { get; set; }
        public virtual DbSet<Course_Concept> Course_Concept { get; set; }
        public virtual DbSet<ImportHistory> ImportHistory { get; set; }
        public virtual DbSet<RatingTask_HasJob> RatingTask_HasJob { get; set; }
        public virtual DbSet<WorkRole> WorkRole { get; set; }
        public virtual DbSet<Course_Organization> Course_Organization { get; set; }
        public virtual DbSet<RatingTask_WorkRole> RatingTask_WorkRole { get; set; }
        public virtual DbSet<RmtlProject_BilletTask> RmtlProject_BilletTask { get; set; }
        public virtual DbSet<Course_AssessmentType> Course_AssessmentType { get; set; }
        public virtual DbSet<Course_CourseType> Course_CourseType { get; set; }
    }
}
