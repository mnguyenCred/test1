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
	DROP CONSTRAINT FK_RatingTask_ConceptWorkElelment
GO
ALTER TABLE dbo.[Course.AssessmentType]
	DROP CONSTRAINT [FK_Course.AssessmentType_Concept]
GO
ALTER TABLE dbo.[Course.CourseType]
	DROP CONSTRAINT [FK_CourseCourseType.Concept]
GO
ALTER TABLE dbo.[ReferenceResource.ReferenceType]
	DROP CONSTRAINT [FK_ReferenceResource.ReferenceType_ConceptScheme.Concept]
GO
ALTER TABLE dbo.[Entity.Concept]
	DROP CONSTRAINT [FK_Entity.Concept_ConceptScheme.Concept]
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT FK_RatingLevelTask_TaskApplicability
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT FK_RatingLevelTask_TrainingGap
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT FK_RatingLevelTask_Rank
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT FK_RatingLevelTask_Level
GO
ALTER TABLE dbo.[ConceptScheme.Concept] ADD
	IsActive bit NOT NULL CONSTRAINT [DF_ConceptScheme.Concept_IsActive] DEFAULT 1
GO
ALTER TABLE dbo.[ConceptScheme.Concept] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Entity.Concept] ADD CONSTRAINT
	[FK_Entity.Concept_ConceptScheme.Concept] FOREIGN KEY
	(
	ConceptId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.[Entity.Concept] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[ReferenceResource.ReferenceType] ADD CONSTRAINT
	[FK_ReferenceResource.ReferenceType_ConceptScheme.Concept] FOREIGN KEY
	(
	ReferenceTypeId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.[ReferenceResource.ReferenceType] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.CourseType] ADD CONSTRAINT
	[FK_CourseCourseType.Concept] FOREIGN KEY
	(
	CourseTypeConceptId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.[Course.CourseType] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.AssessmentType] ADD CONSTRAINT
	[FK_Course.AssessmentType_Concept] FOREIGN KEY
	(
	AssessmentMethodConceptId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.[Course.AssessmentType] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingTask ADD CONSTRAINT
	FK_RatingTask_ConceptWorkElelment FOREIGN KEY
	(
	WorkElementTypeId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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