--	22-10-05 add workRoleId directly on RatingContext
use Navy_RRL_V2
go

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
ALTER TABLE dbo.RatingContext ADD
	WorkRoleId int NULL
GO
ALTER TABLE dbo.RatingContext SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

--================================
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
ALTER TABLE dbo.WorkRole SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[RatingContext.WorkRole]
	DROP CONSTRAINT FK_RatingContextWorkRole_TO_RatingContext
GO
ALTER TABLE dbo.RatingContext ADD CONSTRAINT
	FK_RatingContext_WorkRole FOREIGN KEY
	(
	WorkRoleId
	) REFERENCES dbo.WorkRole
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingContext SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[RatingContext.WorkRole] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
