/*
   Tuesday, October 11, 20223:44:33 PM
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
ALTER TABLE dbo.ReferenceResource SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.TrainingTask ADD
	ReferenceResourceId int NULL
GO
ALTER TABLE dbo.TrainingTask ADD CONSTRAINT
	FK_ReferenceResource_TrainingTask FOREIGN KEY
	(
	ReferenceResourceId
	) REFERENCES dbo.ReferenceResource
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TrainingTask SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
