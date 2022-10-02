
--NOTE set [LifeCycleControlDocumentId] to null before adding the constraint
USE [NavyRRL]
GO

UPDATE [dbo].[Course]
   SET [LifeCycleControlDocumentId] = NULL


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
ALTER TABLE dbo.[ConceptScheme.Concept] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Course
	DROP CONSTRAINT FK_Course_Organization
GO
ALTER TABLE dbo.Organization SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Course ADD CONSTRAINT
	FK_LifeCycleControlDocumentConcept FOREIGN KEY
	(
	LifeCycleControlDocumentId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Course
	DROP COLUMN CurriculumControlAuthorityId
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO


