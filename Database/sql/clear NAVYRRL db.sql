--use credFinder_ProdSync
--GO
--use NavyRRL
--go

Use NavyRRL
go

---- 
/*


*/

/*


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'NavyRRL.dbo.[Entity.AssertionSummary]') AND type in (N'U'))
DROP TABLE NavyRRL.dbo.[Entity.AssertionSummary]
GO
*/
----==============================================
--TBD - do we want to retain some event like for accounts?
truncate table NavyRRL.dbo.ActivityLog
--DELETE FROM NavyRRL.dbo.ActivityLog
--where ActivityType <> 'Account'


--DELETE FROM NavyRRL.dbo.Assessment

----------------------------------------------------
truncate table NavyRRL.dbo.Comment 

--
--DELETE FROM NavyRRL.dbo.[ConceptScheme.Concept] 
--DELETE FROM NavyRRL.dbo.ConceptScheme   
--
--must be done after RatingTask
--DELETE FROM NavyRRL.dbo.Course

--========================================================================
DELETE FROM NavyRRL.dbo.Entity
--========================================================================

----
--DELETE FROM NavyRRL.dbo.ImportHistory
--DELETE FROM NavyRRL.dbo.[Import.RMTLStaging]
--DELETE FROM NavyRRL.dbo.ImportRMTL
--========================================================================
--DELETE FROM NavyRRL.dbo.Rating
DELETE FROM NavyRRL.dbo.RatingTask   
--these two will be deleted due to RI from RatingTask
--DELETE FROM NavyRRL.dbo.[RatingTask.HasJob]
--DELETE FROM NavyRRL.dbo.[RatingTask.HasRating]  
--
DELETE FROM NavyRRL.dbo.Course

--DELETE FROM NavyRRL.dbo.[Course.Task]   
--DELETE FROM NavyRRL.dbo.[Course.Organization]   
--DELETE FROM NavyRRL.dbo.[Course.SchoolType]   
--DELETE FROM NavyRRL.dbo.[Course.AssessmentType]   

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

--DELETE FROM NavyRRL.dbo.RMTLProject   
--DELETE FROM NavyRRL.dbo.[RmtlProject.Billet]   
--DELETE FROM NavyRRL.dbo.[RmtlProjectBilletTask]   
DELETE FROM NavyRRL.dbo.[RegistryHistory]   

--========================================================================
--DELETE FROM NavyRRL.dbo.Source
--
--DELETE FROM NavyRRL.dbo.WorkElementType
DELETE FROM NavyRRL.dbo.WorkRole

-- ================================
--reset identity ids to 0
--DBCC CHECKIDENT ('[ActivityLog]', RESEED, 0);
--DBCC CHECKIDENT ('[Assessment]', RESEED, 0);

DBCC CHECKIDENT ('[Comment]', RESEED, 0);

--DBCC CHECKIDENT ('[ConceptScheme]', RESEED, 0);
--DBCC CHECKIDENT ('[ConceptScheme.Concept]', RESEED, 0);
--
DBCC CHECKIDENT ('[Entity]', RESEED, 0);
--
DBCC CHECKIDENT ('[Course]', RESEED, 0);
--DBCC CHECKIDENT ('[Course.AssessmentType]', RESEED, 0)
DBCC CHECKIDENT ('[Course.CourseType]', RESEED, 0);
DBCC CHECKIDENT ('[Course.Organization]', RESEED, 0);
DBCC CHECKIDENT ('[Course.Task]', RESEED, 0);
DBCC CHECKIDENT ('[CourseTask.AssessmentType]', RESEED, 0)

--
--
--DBCC CHECKIDENT ('[ImportHistory]', RESEED, 0);
--
DBCC CHECKIDENT ('[Job]', RESEED, 0);
--DBCC CHECKIDENT ('[Job.HasRatingTask]', RESEED, 0);;
--
--DBCC CHECKIDENT ('[Organization]', RESEED, 0);
--DBCC CHECKIDENT ('[Rating]', RESEED, 0);
DBCC CHECKIDENT ('[RatingTask]', RESEED, 0);

DBCC CHECKIDENT ('[RatingTask.HasJob]', RESEED, 0);
DBCC CHECKIDENT ('[RatingTask.HasRating]', RESEED, 0);
DBCC CHECKIDENT ('[RatingTask.WorkRole]', RESEED, 0);
DBCC CHECKIDENT ('[RatingTask.HasTrainingTask]', RESEED, 0);


DBCC CHECKIDENT ('[ReferenceResource]', RESEED, 0);
DBCC CHECKIDENT ('[ReferenceResource.ReferenceType]', RESEED, 0);

--DBCC CHECKIDENT ('[RMTLProject]', RESEED, 0);
--DBCC CHECKIDENT ('[RmtlProject.Billet]', RESEED, 0);
--DBCC CHECKIDENT ('[RmtlProjectBilletTask]', RESEED, 0);
DBCC CHECKIDENT ('[RegistryHistory]', RESEED, 0);

--DBCC CHECKIDENT ('[WorkElementType]', RESEED, 0);
DBCC CHECKIDENT ('[WorkRole]', RESEED, 0);



--NOTE - no identity column
---DBCC CHECKIDENT ('[Entity_Cache]', RESEED, 0);

GO


exec aspAllDatabaseTableCounts 'NavyRRL', 'base table', 10
go

--Now shrink the database

--exec aspAllDatabaseTableCounts 'credFinderSandbox', 'base table', 10
--go
     

