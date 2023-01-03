--use credFinder_ProdSync
--GO
--use Navy_RRL_V2
--go

Use Navy_RRL_V2
go

Use NavyRRL
go

---- 
/*


*/

----==============================================
/****** clear obsolete ******/

/****** Object:  Table [dbo].[Entity.Concept]    Script Date: 12/22/2022 9:39:32 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Entity.Concept]') AND type in (N'U'))
DROP TABLE [dbo].[Entity.Concept]
GO



--

/*


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnlistedClassification]') AND type in (N'U'))
DROP TABLE [dbo].[EnlistedClassification]
GO



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
truncate table NavyRRL.dbo.ActivityLog
--DELETE FROM NavyRRL.dbo.ActivityLog
--where ActivityType <> 'Account'

truncate table NavyRRL.dbo.[ClusterAnalysis.HasCandidatePlatform] 
DELETE FROM NavyRRL.dbo.ClusterAnalysis
DELETE FROM NavyRRL.dbo.ClusterAnalysisTitle 
----------------------------------------------------
truncate table NavyRRL.dbo.Comment 

--GENERALLY DO NOT CLEAR CONCEPTS
--DELETE FROM NavyRRL.dbo.[ConceptScheme.Concept] 
--DELETE FROM NavyRRL.dbo.ConceptScheme   
--
--must be done after RatingTask
--DELETE FROM NavyRRL.dbo.Course

--========================================================================
DELETE FROM NavyRRL.dbo.Entity
--========================================================================

----
DELETE FROM NavyRRL.dbo.ImportHistory
DELETE FROM NavyRRL.dbo.[Import.RMTLStaging]
DELETE FROM NavyRRL.dbo.ImportRMTL
--========================================================================
--	GENERALLY DO CLEAR RATING ********************
--DELETE FROM NavyRRL.dbo.Rating
DELETE FROM NavyRRL.dbo.RatingTask   
--these two will be deleted due to RI from RatingTask
--DELETE FROM NavyRRL.dbo.[RatingTask.HasJob]
--DELETE FROM NavyRRL.dbo.[RatingTask.HasRating]  
--
--New
DELETE FROM NavyRRL.dbo.CourseContext

DELETE FROM NavyRRL.dbo.Course
--was missing RI, so doing manually
DELETE FROM [dbo].[Course.AssessmentType]
--New
DELETE FROM NavyRRL.dbo.TrainingTask   
--DELETE FROM NavyRRL.dbo.[CourseContext.AssessmentType]   

DELETE FROM NavyRRL.[dbo].[System.ProxyCodes]
--must be done after RatingTask
DELETE FROM NavyRRL.dbo.Job
--should be unnecesary, deleting Job will clear this
--DELETE FROM NavyRRL.dbo.[Job.HasRatingTask]
--
--must be done after RatingTask
--NO LONGER CLEAR HAS HAVE ADDED ADDITIONAL DATA
--DELETE FROM NavyRRL.dbo.Organization
--
DELETE FROM NavyRRL.dbo.ReferenceResource   
--DELETE FROM NavyRRL.dbo.[ReferenceResource.ReferenceType]   

DELETE FROM NavyRRL.dbo.RMTLProject   
--DELETE FROM NavyRRL.dbo.[RmtlProject.Billet]   
--DELETE FROM NavyRRL.dbo.[RmtlProjectBilletTask]   


--========================================================================
--DELETE FROM NavyRRL.dbo.Source
--
--DELETE FROM NavyRRL.dbo.WorkElementType
DELETE FROM NavyRRL.dbo.WorkRole

-- ================================
--reset identity ids to 0
DBCC CHECKIDENT ('[ActivityLog]', RESEED, 0);
DBCC CHECKIDENT ('[ClusterAnalysis]', RESEED, 0);
DBCC CHECKIDENT ('[ClusterAnalysisTitle]', RESEED, 0);

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




exec aspAllDatabaseTableCounts 'NavyRRL', 'base table', 10
go

exec aspAllDatabaseTableCounts 'Navy_RRL_V2', 'base table', 10
go

--Now shrink the database



