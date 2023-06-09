/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
USE Navy_RRL_V2
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
ALTER TABLE dbo.CourseContext SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingContext
	DROP CONSTRAINT FK_RatingContext_TrainingTask
GO
ALTER TABLE dbo.TrainingTask SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingContext ADD
	CourseContextId int NULL
GO
ALTER TABLE dbo.RatingContext ADD CONSTRAINT
	FK_RatingContext_CourseContext FOREIGN KEY
	(
	CourseContextId
	) REFERENCES dbo.CourseContext
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingContext
	DROP COLUMN TrainingTaskId
GO
ALTER TABLE dbo.RatingContext SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
