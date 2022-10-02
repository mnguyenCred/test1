/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--22-04-09-ConceptScheme.Concept-Make ListId non-nullable
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
ALTER TABLE dbo.[ConceptScheme.Concept]
	DROP CONSTRAINT [FK_ConceptScheme.Concept_ConceptScheme]
GO
ALTER TABLE dbo.ConceptScheme SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[ConceptScheme.Concept]
	DROP CONSTRAINT [DF_ConceptScheme.Concept_RowId]
GO
ALTER TABLE dbo.[ConceptScheme.Concept]
	DROP CONSTRAINT DF_Table_1_SortOrder
GO
ALTER TABLE dbo.[ConceptScheme.Concept]
	DROP CONSTRAINT [DF_ConceptScheme.Concept_IsActive]
GO
ALTER TABLE dbo.[ConceptScheme.Concept]
	DROP CONSTRAINT [DF_ConceptScheme.Concept_Created]
GO
ALTER TABLE dbo.[ConceptScheme.Concept]
	DROP CONSTRAINT [DF_ConceptScheme.Concept_LastUpdated]
GO
CREATE TABLE dbo.[Tmp_ConceptScheme.Concept]
	(
	Id int NOT NULL IDENTITY (1, 1),
	RowId uniqueidentifier NOT NULL,
	ConceptSchemeId int NOT NULL,
	Name varchar(1000) NOT NULL,
	CTID varchar(50) NULL,
	CodedNotation varchar(50) NULL,
	AlternateLabel varchar(500) NULL,
	Description varchar(MAX) NULL,
	ListId int NOT NULL,
	IsActive bit NOT NULL,
	Created datetime NOT NULL,
	CreatedById int NULL,
	LastUpdated datetime NOT NULL,
	LastUpdatedById int NULL,
	WorkElementType varchar(200) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.[Tmp_ConceptScheme.Concept] SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.[Tmp_ConceptScheme.Concept] ADD CONSTRAINT
	[DF_ConceptScheme.Concept_RowId] DEFAULT (newid()) FOR RowId
GO
ALTER TABLE dbo.[Tmp_ConceptScheme.Concept] ADD CONSTRAINT
	DF_Table_1_SortOrder DEFAULT ((25)) FOR ListId
GO
ALTER TABLE dbo.[Tmp_ConceptScheme.Concept] ADD CONSTRAINT
	[DF_ConceptScheme.Concept_IsActive] DEFAULT ((1)) FOR IsActive
GO
ALTER TABLE dbo.[Tmp_ConceptScheme.Concept] ADD CONSTRAINT
	[DF_ConceptScheme.Concept_Created] DEFAULT (getdate()) FOR Created
GO
ALTER TABLE dbo.[Tmp_ConceptScheme.Concept] ADD CONSTRAINT
	[DF_ConceptScheme.Concept_LastUpdated] DEFAULT (getdate()) FOR LastUpdated
GO
SET IDENTITY_INSERT dbo.[Tmp_ConceptScheme.Concept] ON
GO
IF EXISTS(SELECT * FROM dbo.[ConceptScheme.Concept])
	 EXEC('INSERT INTO dbo.[Tmp_ConceptScheme.Concept] (Id, RowId, ConceptSchemeId, Name, CTID, CodedNotation, AlternateLabel, Description, ListId, IsActive, Created, CreatedById, LastUpdated, LastUpdatedById, WorkElementType)
		SELECT Id, RowId, ConceptSchemeId, Name, CTID, CodedNotation, AlternateLabel, Description, ListId, IsActive, Created, CreatedById, LastUpdated, LastUpdatedById, WorkElementType FROM dbo.[ConceptScheme.Concept] WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.[Tmp_ConceptScheme.Concept] OFF
GO
ALTER TABLE dbo.[CourseTask.AssessmentType]
	DROP CONSTRAINT [FK_CourseTask.AssessmentType_Concept]
GO
ALTER TABLE dbo.[Entity.Concept]
	DROP CONSTRAINT [FK_Entity.Concept_ConceptScheme.Concept]
GO
ALTER TABLE dbo.[ReferenceResource.ReferenceType]
	DROP CONSTRAINT [FK_ReferenceResource.ReferenceType_ConceptScheme.Concept]
GO
ALTER TABLE dbo.[Course.CourseType]
	DROP CONSTRAINT [FK_CourseCourseType.Concept]
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT FK_RatingTask_ConceptWorkElelment
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
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT [FK_ClusterAnalysis_TrainingSolution.Concept]
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT [FK_ClusterAnalysis_RecommendedModality.Concept1]
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT [FK_ClusterAnalysis_DevSpec.Concept]
GO
ALTER TABLE dbo.Course
	DROP CONSTRAINT [FK_CourseLCCD_LCCD.Concept]
GO
DROP TABLE dbo.[ConceptScheme.Concept]
GO
EXECUTE sp_rename N'dbo.[Tmp_ConceptScheme.Concept]', N'ConceptScheme.Concept', 'OBJECT' 
GO
ALTER TABLE dbo.[ConceptScheme.Concept] ADD CONSTRAINT
	[PK_ConceptScheme.Concept] PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.[ConceptScheme.Concept] ADD CONSTRAINT
	[FK_ConceptScheme.Concept_ConceptScheme] FOREIGN KEY
	(
	ConceptSchemeId
	) REFERENCES dbo.ConceptScheme
	(
	Id
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
COMMIT
BEGIN TRANSACTION
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
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ClusterAnalysis ADD CONSTRAINT
	[FK_ClusterAnalysis_TrainingSolution.Concept] FOREIGN KEY
	(
	TrainingSolutionTypeId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ClusterAnalysis ADD CONSTRAINT
	[FK_ClusterAnalysis_RecommendedModality.Concept1] FOREIGN KEY
	(
	RecommendedModalityId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ClusterAnalysis ADD CONSTRAINT
	[FK_ClusterAnalysis_DevSpec.Concept] FOREIGN KEY
	(
	DevelopmentSpecificationId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ClusterAnalysis SET (LOCK_ESCALATION = TABLE)
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
ALTER TABLE dbo.[CourseTask.AssessmentType] ADD CONSTRAINT
	[FK_CourseTask.AssessmentType_Concept] FOREIGN KEY
	(
	AssessmentMethodConceptId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.[CourseTask.AssessmentType] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT