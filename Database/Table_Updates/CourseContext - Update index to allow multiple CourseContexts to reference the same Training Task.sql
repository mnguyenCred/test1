/*
   Thursday, February 23, 20237:14:07 PM
   User: 
   Server: WORK-PC
   Database: Navy_RRL_V2
   Application: 
*/

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
DROP INDEX IX_CourseContext_CourseTrainingTask ON dbo.CourseContext
GO
CREATE NONCLUSTERED INDEX IX_CourseContext_CourseTrainingTask ON dbo.CourseContext
	(
	HasCourseId,
	HasTrainingTaskId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.CourseContext SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
