--	22-10-02 CourseContext rename properties with Has...

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
EXECUTE sp_rename N'dbo.CourseContext.CourseId', N'Tmp_HasCourseId', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.CourseContext.TrainingTaskId', N'Tmp_HasTrainingTaskId_1', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.CourseContext.Tmp_HasCourseId', N'HasCourseId', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.CourseContext.Tmp_HasTrainingTaskId_1', N'HasTrainingTaskId', 'COLUMN' 
GO
ALTER TABLE dbo.CourseContext SET (LOCK_ESCALATION = TABLE)
GO
COMMIT