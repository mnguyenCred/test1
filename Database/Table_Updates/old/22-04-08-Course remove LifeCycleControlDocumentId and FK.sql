/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--22-04-08-Course remove LifeCycleControlDocumentId and FK
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
ALTER TABLE dbo.Course
	DROP CONSTRAINT FK_CourseLCCD_ReferenceResource
GO
ALTER TABLE dbo.ReferenceResource SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Course
	DROP COLUMN LifeCycleControlDocumentId
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
