--use credFinder_ProdSync
--GO
--use Navy_RRL_V2
--go

Use Navy_RRL_V2
go

---- 
/*


*/

----==============================================
/****** clear obsolete ******/
DROP TABLE [dbo].[Course.AssessmentType]

--

/*
--first remove all FKs
DROP TABLE [dbo].[Course.Task]
DROP TABLE [dbo].[CourseTask.AssessmentType]

DROP TABLE [dbo].[Course.Organization]
DROP TABLE [dbo].[Entity.Concept]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnlistedClassification]') AND type in (N'U'))
DROP TABLE [dbo].[EnlistedClassification]
GO

DROP TABLE [dbo].Import_ABF_RMTL
DROP TABLE [dbo].ImportHistory

DROP TABLE [dbo].Import_QM_RMTL_11232021


DROP TABLE [dbo].ImportRMTL_BK211213


DROP TABLE [dbo].ImportRMTL_BK220108

DROP TABLE [dbo].[Prod.ConceptScheme.Concept]
DROP TABLE [dbo].[RMTLImport-211109]

DROP TABLE [dbo].[Subscription.EntityType]
DROP TABLE [dbo].Subscription

DROP TABLE [dbo].[System.PendingRepublish]

--
DROP TABLE [dbo].[RatingTask.HasJob]
DROP TABLE [dbo].[RatingTask.HasRating]
DROP TABLE [dbo].[RatingTask.HasRatingContext]
DROP TABLE [dbo].[RatingTask.HasTrainingTask]
DROP TABLE [dbo].[RatingTask.WorkRole]

DROP TABLE [dbo].[RegistryHistory]

BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[RmtlProject.Billet]
	DROP CONSTRAINT [FK_RmtlProject.Billet_Job]
GO
ALTER TABLE dbo.Job SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[RmtlProject.Billet]
	DROP CONSTRAINT [FK_RmtlProject.Billet_RMTLProject]
GO
ALTER TABLE dbo.RMTLProject SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[RmtlProject.Billet] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
DROP TABLE [dbo].[RmtlProject.Billet]

DROP TABLE [dbo].[RmtlProject.BilletTask]


*/
----==============================================
--TBD - do we want to retain some event like for accounts?
truncate table Navy_RRL_V2.dbo.ActivityLog
--DELETE FROM Navy_RRL_V2.dbo.ActivityLog
--where ActivityType <> 'Account'

truncate table Navy_RRL_V2.dbo.[ClusterAnalysis.HasCandidatePlatform] 
DELETE FROM Navy_RRL_V2.dbo.ClusterAnalysis
--truncate table Navy_RRL_V2.dbo.ClusterAnalysis 
----------------------------------------------------
truncate table Navy_RRL_V2.dbo.Comment 

--GENERALLY DO NOT CLEAR CONCEPTS
--DELETE FROM Navy_RRL_V2.dbo.[ConceptScheme.Concept] 
--DELETE FROM Navy_RRL_V2.dbo.ConceptScheme   
--
--must be done after RatingTask
--DELETE FROM Navy_RRL_V2.dbo.Course

--========================================================================
DELETE FROM Navy_RRL_V2.dbo.Entity
--========================================================================

----
--DELETE FROM Navy_RRL_V2.dbo.ImportHistory
DELETE FROM Navy_RRL_V2.dbo.[Import.RMTLStaging]
DELETE FROM Navy_RRL_V2.dbo.ImportRMTL
--========================================================================
--	GENERALLY DO CLEAR RATING ********************
--DELETE FROM Navy_RRL_V2.dbo.Rating
DELETE FROM Navy_RRL_V2.dbo.RatingTask   
--these two will be deleted due to RI from RatingTask
--DELETE FROM Navy_RRL_V2.dbo.[RatingTask.HasJob]
--DELETE FROM Navy_RRL_V2.dbo.[RatingTask.HasRating]  
--
DELETE FROM Navy_RRL_V2.dbo.CourseContext
DELETE FROM Navy_RRL_V2.dbo.Course

DELETE FROM Navy_RRL_V2.dbo.TrainingTask   
--DELETE FROM Navy_RRL_V2.dbo.[CourseContext.AssessmentType]   

--must be done after RatingTask
DELETE FROM Navy_RRL_V2.dbo.Job
--should be unnecesary, deleting Job will clear this
--DELETE FROM Navy_RRL_V2.dbo.[Job.HasRatingTask]
--
--must be done after RatingTask
--NO LONGER CLEAR HAS HAVE ADDED ADDITIONAL DATA
--DELETE FROM Navy_RRL_V2.dbo.Organization
--
DELETE FROM Navy_RRL_V2.dbo.ReferenceResource   
--DELETE FROM Navy_RRL_V2.dbo.[ReferenceResource.ReferenceType]   

DELETE FROM Navy_RRL_V2.dbo.RMTLProject   
--DELETE FROM Navy_RRL_V2.dbo.[RmtlProject.Billet]   
--DELETE FROM Navy_RRL_V2.dbo.[RmtlProjectBilletTask]   


--========================================================================
--DELETE FROM Navy_RRL_V2.dbo.Source
--
--DELETE FROM Navy_RRL_V2.dbo.WorkElementType
DELETE FROM Navy_RRL_V2.dbo.WorkRole

-- ================================
--reset identity ids to 0
DBCC CHECKIDENT ('[ActivityLog]', RESEED, 0);
DBCC CHECKIDENT ('[ClusterAnalysis]', RESEED, 0);
DBCC CHECKIDENT ('[ClusterAnalysis.HasCandidatePlatform]', RESEED, 0);
--DBCC CHECKIDENT ('[ClusterAnalysisTitle]', RESEED, 0);
DBCC CHECKIDENT ('[Comment]', RESEED, 0);

--DBCC CHECKIDENT ('[ConceptScheme]', RESEED, 0);
--DBCC CHECKIDENT ('[ConceptScheme.Concept]', RESEED, 0);

--
DBCC CHECKIDENT ('[Course]', RESEED, 0);
DBCC CHECKIDENT ('[Course.CourseType]', RESEED, 0);
DBCC CHECKIDENT ('[CourseContext]', RESEED, 0);
DBCC CHECKIDENT ('[CourseContext.AssessmentType]', RESEED, 0)
--
DBCC CHECKIDENT ('[Entity]', RESEED, 0);
--
--
DBCC CHECKIDENT ('[Import.RMTLStaging]', RESEED, 0);
DBCC CHECKIDENT ('[ImportRMTL]', RESEED, 0);
--
DBCC CHECKIDENT ('[Job]', RESEED, 0);
--
--DBCC CHECKIDENT ('[Organization]', RESEED, 0);
--DBCC CHECKIDENT ('[Rating]', RESEED, 0);
DBCC CHECKIDENT ('[RatingContext]', RESEED, 0);
DBCC CHECKIDENT ('[RatingTask]', RESEED, 0);

DBCC CHECKIDENT ('[ReferenceResource]', RESEED, 0);
DBCC CHECKIDENT ('[ReferenceResource.ReferenceType]', RESEED, 0);

DBCC CHECKIDENT ('[RMTLProject]', RESEED, 0);

DBCC CHECKIDENT ('[TrainingTask]', RESEED, 0);

--DBCC CHECKIDENT ('[WorkElementType]', RESEED, 0);
DBCC CHECKIDENT ('[WorkRole]', RESEED, 0);



--NOTE - no identity column
---DBCC CHECKIDENT ('[Entity_Cache]', RESEED, 0);

GO


exec aspAllDatabaseTableCounts 'Navy_RRL_V2', 'base table', 10
go

--Now shrink the database

--exec aspAllDatabaseTableCounts 'credFinderSandbox', 'base table', 10
--go
     

