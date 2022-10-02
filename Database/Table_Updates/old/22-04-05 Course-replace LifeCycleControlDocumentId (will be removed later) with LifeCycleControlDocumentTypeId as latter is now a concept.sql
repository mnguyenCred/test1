/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
use NavyRRL
go
-- 22-04-05 Course-replace LifeCycleControlDocumentId (will be removed later) with LifeCycleControlDocumentTypeId as latter is now a concept
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
ALTER TABLE dbo.Course ADD
	LifeCycleControlDocumentTypeId int NULL
GO
ALTER TABLE dbo.Course ADD CONSTRAINT
	[FK_CourseLCCD_LCCD.Concept] FOREIGN KEY
	(
	LifeCycleControlDocumentTypeId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT