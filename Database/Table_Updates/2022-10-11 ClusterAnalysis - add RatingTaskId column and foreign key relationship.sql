/*
   Tuesday, October 11, 20225:04:23 PM
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
ALTER TABLE dbo.RatingTask SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ClusterAnalysis ADD
	RatingTaskId int NULL
GO
ALTER TABLE dbo.ClusterAnalysis ADD CONSTRAINT
	FK_ClusterAnalysis_RatingTask FOREIGN KEY
	(
	RatingTaskId
	) REFERENCES dbo.RatingTask
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ClusterAnalysis SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
