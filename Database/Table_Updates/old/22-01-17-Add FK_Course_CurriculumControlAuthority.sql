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
ALTER TABLE dbo.CurriculumControlAuthority SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.SchoolType]
	DROP CONSTRAINT [FK_Course.SchoolType_Course]
GO
ALTER TABLE dbo.Course ADD CONSTRAINT
	FK_Course_CurriculumControlAuthority FOREIGN KEY
	(
	CurriculumControlAuthorityId
	) REFERENCES dbo.CurriculumControlAuthority
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.SchoolType] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT