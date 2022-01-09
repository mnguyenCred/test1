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
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT [FK_RatingLevelTask_ConceptScheme.Concept2]
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT [FK_RatingLevelTask_ConceptScheme.Concept3]
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT [FK_RatingLevelTask_ConceptScheme.Concept]
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT [FK_RatingLevelTask_ConceptScheme.Concept1]
GO
ALTER TABLE dbo.[ConceptScheme.Concept] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingTask ADD CONSTRAINT
	FK_RatingLevelTask_TaskApplicability FOREIGN KEY
	(
	TaskApplicabilityId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingTask ADD CONSTRAINT
	FK_RatingLevelTask_TrainingGap FOREIGN KEY
	(
	FormalTrainingGapId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingTask ADD CONSTRAINT
	FK_RatingLevelTask_Rank FOREIGN KEY
	(
	RankId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingTask ADD CONSTRAINT
	FK_RatingLevelTask_Level FOREIGN KEY
	(
	LevelId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingTask SET (LOCK_ESCALATION = TABLE)
GO
COMMIT