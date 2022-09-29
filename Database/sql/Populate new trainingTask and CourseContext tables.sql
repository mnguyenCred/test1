USE [NavyRRL]
GO
--populate new trainingTask and CourseContext tables
--	mostly for prototyping as likely will clear all and start over. 


--duplicates check
Select  a.CTID, count(*)
from [Course.Task] a 
group by a.CTID having COUNT(*) > 1
go


--populate training task
truncate table [TrainingTask]
go

INSERT INTO [dbo].[TrainingTask]
           ([RowId]
           ,[Description]
           ,[CTID]
           ,[Created]
           ,[CreatedById]
           ,[LastUpdated]
           ,[LastUpdatedById])
Select a.RowId, a.Description, 'ce-' + lower(newId()), a.Created, a.CreatedById, a.LastUpdated, a.LastUpdatedById
from [Course.Task] a 

go

--populate courseContext
INSERT INTO [dbo].[CourseContext]
           ([RowId]
           ,[CourseId]
           ,[TrainingTaskId]
           ,[Created]
           ,[CreatedById]
           ,[LastUpdated]
           ,[LastUpdatedById])

Select newid(), a.CourseId, b.Id 
,a.Created, a.CreatedById, a.LastUpdated, a.LastUpdatedById
from [Course.Task] a 
inner join TrainingTask b on a.RowId = b.RowId

go

