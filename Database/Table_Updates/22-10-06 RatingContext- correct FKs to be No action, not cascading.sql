use NavyRRL
go


/*22-10-06 RatingContext- correct FKs to be No action, not cascading*/
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
--=============================================
ALTER TABLE dbo.RatingContext
	DROP CONSTRAINT FK_RatingContext_Rank
GO
ALTER TABLE dbo.[ConceptScheme.Concept] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
--=============================================
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingContext
	DROP CONSTRAINT FK_RatingContext_RatingTask
GO
ALTER TABLE dbo.RatingTask SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
--=============================================
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingContext
	DROP CONSTRAINT FK_RatingContext_BilletTitle
GO
ALTER TABLE dbo.Job SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
--=============================================
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingContext
	DROP CONSTRAINT FK_RatingContext_Rating
GO
ALTER TABLE dbo.Rating SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
--=============================================
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingContext
	DROP CONSTRAINT FK_RatingContext_TrainingTask
GO
ALTER TABLE dbo.TrainingTask SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
--=============================================
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingContext ADD CONSTRAINT
	FK_RatingContext_TrainingTask FOREIGN KEY
	(
	TrainingTaskId
	) REFERENCES dbo.TrainingTask
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingContext ADD CONSTRAINT
	FK_RatingContext_Rating FOREIGN KEY
	(
	RatingId
	) REFERENCES dbo.Rating
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingContext ADD CONSTRAINT
	FK_RatingContext_BilletTitle FOREIGN KEY
	(
	BilletTitleId
	) REFERENCES dbo.Job
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingContext ADD CONSTRAINT
	FK_RatingContext_RatingTask FOREIGN KEY
	(
	RatingTaskId
	) REFERENCES dbo.RatingTask
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingContext ADD CONSTRAINT
	FK_RatingContext_Rank FOREIGN KEY
	(
	PayGradeTypeId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingContext SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go

/*==================================================*/
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
ALTER TABLE dbo.RatingContext
	DROP CONSTRAINT FK_RatingContext_WorkRole
GO
ALTER TABLE dbo.WorkRole SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
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
ALTER TABLE dbo.RatingContext
	DROP CONSTRAINT FK_RatingContext_ClusterAnalysis
GO
ALTER TABLE dbo.ClusterAnalysis SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingContext ADD CONSTRAINT
	FK_RatingContext_ClusterAnalysis FOREIGN KEY
	(
	ClusterAnalysisId
	) REFERENCES dbo.ClusterAnalysis
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingContext SET (LOCK_ESCALATION = TABLE)
GO
COMMIT