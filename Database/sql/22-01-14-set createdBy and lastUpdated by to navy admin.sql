USE [NavyRRL]
GO
-- set createdBy and lastUpdated by to navy admin

UPDATE [dbo].Course
   SET [CreatedById] = 1
      ,[LastUpdatedById] = 1

GO
UPDATE [dbo].[Course.Task]
   SET [CreatedById] = 1
      ,[LastUpdatedById] = 1

GO
UPDATE [dbo].CurriculumControlAuthority
   SET [CreatedById] = 1
      ,[LastUpdatedById] = 1

GO
UPDATE [dbo].Job
   SET [CreatedById] = 1
      ,[LastUpdatedById] = 1
GO
UPDATE [dbo].[Job.HasRating]
   SET [CreatedById] = 1
     
GO

UPDATE [dbo].Rating
   SET [CreatedById] = 1
      ,[LastUpdatedById] = 1
GO

UPDATE [dbo].[RatingTask]
   SET [CreatedById] = 1
      ,[LastUpdatedById] = 1
GO

UPDATE [dbo].ReferenceResource
   SET [CreatedById] = 1
      ,[LastUpdatedById] = 1

GO

UPDATE [dbo].ConceptScheme
   SET [CreatedById] = 1
      ,[LastUpdatedById] = 1

GO
--UPDATE [dbo].[ConceptScheme.Concept]
--   SET [CreatedById] = 1
--      ,[LastUpdatedById] = 1

--GO
