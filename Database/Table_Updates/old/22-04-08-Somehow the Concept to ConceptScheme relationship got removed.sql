/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--22-04-08-Somehow the Concept to ConceptScheme relationship got removed
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
ALTER TABLE dbo.ConceptScheme SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.AssessmentType]
	DROP CONSTRAINT [FK_Course.AssessmentType_Concept]
GO
ALTER TABLE dbo.[ConceptScheme.Concept] ADD CONSTRAINT
	[FK_ConceptScheme.Concept_ConceptScheme] FOREIGN KEY
	(
	ConceptSchemeId
	) REFERENCES dbo.ConceptScheme
	(
	Id
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.[ConceptScheme.Concept] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.AssessmentType] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT