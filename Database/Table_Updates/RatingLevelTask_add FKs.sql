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
ALTER TABLE dbo.[Course.Task] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.WorkElementType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Source SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.FunctionalArea SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[ConceptScheme.Concept] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingLevelTask ADD CONSTRAINT
	[FK_RatingLevelTask_ConceptScheme.Concept] FOREIGN KEY
	(
	RankId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingLevelTask ADD CONSTRAINT
	[FK_RatingLevelTask_ConceptScheme.Concept1] FOREIGN KEY
	(
	LevelId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingLevelTask ADD CONSTRAINT
	FK_RatingLevelTask_FunctionalArea FOREIGN KEY
	(
	FunctionalAreaId
	) REFERENCES dbo.FunctionalArea
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingLevelTask ADD CONSTRAINT
	FK_RatingLevelTask_Source FOREIGN KEY
	(
	SourceId
	) REFERENCES dbo.Source
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingLevelTask ADD CONSTRAINT
	FK_RatingLevelTask_WorkElementType FOREIGN KEY
	(
	WorkElementTypeId
	) REFERENCES dbo.WorkElementType
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingLevelTask ADD CONSTRAINT
	[FK_RatingLevelTask_ConceptScheme.Concept2] FOREIGN KEY
	(
	TaskApplicabilityId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingLevelTask ADD CONSTRAINT
	[FK_RatingLevelTask_ConceptScheme.Concept3] FOREIGN KEY
	(
	FormalTrainingGapId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingLevelTask ADD CONSTRAINT
	[FK_RatingLevelTask_Course.Task] FOREIGN KEY
	(
	TrainingTaskId
	) REFERENCES dbo.[Course.Task]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingLevelTask SET (LOCK_ESCALATION = TABLE)
GO
COMMIT