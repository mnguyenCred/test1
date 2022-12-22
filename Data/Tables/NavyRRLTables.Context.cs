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
        public virtual DbSet<AppFunctionPermission> AppFunctionPermission { get; set; }
        public virtual DbSet<ApplicationFunction> ApplicationFunction { get; set; }
        public virtual DbSet<ApplicationRole> ApplicationRole { get; set; }
        public virtual DbSet<ApplicationUserRole> ApplicationUserRole { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<ClusterAnalysis> ClusterAnalysis { get; set; }
        public virtual DbSet<ClusterAnalysis_HasCandidatePlatform> ClusterAnalysis_HasCandidatePlatform { get; set; }
        public virtual DbSet<ClusterAnalysisTitle> ClusterAnalysisTitle { get; set; }
        public virtual DbSet<ConceptScheme> ConceptScheme { get; set; }
        public virtual DbSet<ConceptScheme_Concept> ConceptScheme_Concept { get; set; }
        public virtual DbSet<Course> Course { get; set; }
        public virtual DbSet<Course_CourseType> Course_CourseType { get; set; }
        public virtual DbSet<CourseContext> CourseContext { get; set; }
        public virtual DbSet<CourseContext_AssessmentType> CourseContext_AssessmentType { get; set; }
        public virtual DbSet<ImportRMTL> ImportRMTL { get; set; }
        public virtual DbSet<Job> Job { get; set; }
        public virtual DbSet<Organization> Organization { get; set; }
        public virtual DbSet<Rating> Rating { get; set; }
        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<RatingContext> RatingContext { get; set; }
        public virtual DbSet<RatingTask> RatingTask { get; set; }
        public virtual DbSet<ReferenceResource> ReferenceResource { get; set; }
        public virtual DbSet<ReferenceResource_ReferenceType> ReferenceResource_ReferenceType { get; set; }
        public virtual DbSet<RMTLProject> RMTLProject { get; set; }
        public virtual DbSet<TrainingTask> TrainingTask { get; set; }
        public virtual DbSet<WorkRole> WorkRole { get; set; }
        public virtual DbSet<System_ProxyCodes> System_ProxyCodes { get; set; }
    }
}
