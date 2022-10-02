/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--	22-04-04-Course-remove obsolete org and asmt relationships
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.Course
--	DROP CONSTRAINT FK_Course_Organization
--GO
--ALTER TABLE dbo.Organization SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.AssessmentType]
	DROP CONSTRAINT [FK_Course.AssessmentType_Course]
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.AssessmentType] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
--
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
ALTER TABLE dbo.Course
	DROP CONSTRAINT FK_Course_Organization
GO
ALTER TABLE dbo.Organization SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.Organization]
	DROP CONSTRAINT [FK_Course.Organization_Course]
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.AssessmentType] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.Organization] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

--may have to readd this just in case?
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
ALTER TABLE dbo.Organization SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Course ADD CONSTRAINT
	FK_Course_Organization FOREIGN KEY
	(
	CurriculumControlAuthorityId
	) REFERENCES dbo.Organization
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
