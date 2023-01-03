

Use Navy_RRL_V2
go


----==============================================


/*


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnlistedClassification]') AND type in (N'U'))
DROP TABLE [dbo].[EnlistedClassification]
GO

DROP TABLE [dbo].ImportHistory


*/
----==============================================
--TBD - do we want to retain some event like for accounts?
truncate table Navy_RRL_V2.dbo.ActivityLog
--DELETE FROM Navy_RRL_V2.dbo.ActivityLog
--where ActivityType <> 'Account'

----------------------------------------------------
truncate table Navy_RRL_V2.dbo.Comment 

--GENERALLY DO NOT CLEAR CONCEPTS
--DELETE FROM Navy_RRL_V2.dbo.[ConceptScheme.Concept] 
--DELETE FROM Navy_RRL_V2.dbo.ConceptScheme   


--========================================================================
DELETE FROM Navy_RRL_V2.dbo.Entity
--========================================================================

----

DELETE FROM Navy_RRL_V2.dbo.[Import.RMTLStaging]
DELETE FROM Navy_RRL_V2.dbo.ImportRMTL
--========================================================================
DELETE FROM Navy_RRL_V2.dbo.RatingContext
--do this after RatingContext
DELETE FROM Navy_RRL_V2.dbo.ClusterAnalysis
truncate table Navy_RRL_V2.dbo.[ClusterAnalysis.HasCandidatePlatform] 
DELETE FROM Navy_RRL_V2.dbo.ClusterAnalysisTitle 

--	GENERALLY DO NOT CLEAR RATING ********************
--DELETE FROM Navy_RRL_V2.dbo.Rating
DELETE FROM Navy_RRL_V2.dbo.RatingTask   

--
--New
DELETE FROM Navy_RRL_V2.dbo.CourseContext
--DELETE FROM Navy_RRL_V2.dbo.[CourseContext.AssessmentType]   
DELETE FROM Navy_RRL_V2.dbo.Course

DELETE FROM Navy_RRL_V2.[dbo].[System.ProxyCodes]
--must be done after RatingTask
--may not want to always do this?
DELETE FROM Navy_RRL_V2.dbo.Job

--
--must be done after RatingTask
--NO LONGER CLEAR HAS HAVE ADDED ADDITIONAL DATA
--DELETE FROM Navy_RRL_V2.dbo.Organization
--


DELETE FROM Navy_RRL_V2.dbo.RMTLProject   

--New
DELETE FROM Navy_RRL_V2.dbo.TrainingTask   
DELETE FROM Navy_RRL_V2.dbo.ReferenceResource   
--DELETE FROM Navy_RRL_V2.dbo.[ReferenceResource.ReferenceType]   
--========================================================================

DELETE FROM Navy_RRL_V2.dbo.WorkRole

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
DBCC CHECKIDENT ('[Entity.Comment]', RESEED, 0);

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

DBCC CHECKIDENT ('[WorkRole]', RESEED, 0);



exec aspAllDatabaseTableCounts 'Navy_RRL_V2', 'base table', 10
go

--Now shrink the database



