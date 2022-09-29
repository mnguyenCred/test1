use NavyRRL
go

--22-09-09 ClusterAnalysis - change DevelopmentTime to decimal, lower EstimatedInstructionalTime
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
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT [FK_ClusterAnalysis_CFMPlacement.Concept]
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT [FK_ClusterAnalysis_TrainingSolution.Concept]
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT [FK_ClusterAnalysis_RecommendedModality.Concept]
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT [FK_ClusterAnalysis_DevSpec.Concept]
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT FK_ClusterAnalysis_CFMPlacementConcept
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT [FK_ClusterAnalysis_ConceptScheme.Concept]
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT [FK_ClusterAnalysis_ConceptScheme.Concept1]
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT FK_ClusterAnalysis_DevSpecificationConcept
GO
ALTER TABLE dbo.[ConceptScheme.Concept] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT FK_ClusterAnalysis_RatingTask
GO
ALTER TABLE dbo.RatingTask SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT DF_ClusterAnalysis_RowId
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT DF_ClusterAnalysis_CTID
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT DF_ClusterAnalysis_Created
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT DF_ClusterAnalysis_LastUpdated
GO
CREATE TABLE dbo.Tmp_ClusterAnalysis
	(
	Id int NOT NULL IDENTITY (1, 1),
	RowId uniqueidentifier NOT NULL,
	RatingTaskId int NOT NULL,
	TrainingSolutionTypeId int NULL,
	ClusterAnalysisTitle nvarchar(500) NULL,
	RecommendedModalityId int NULL,
	DevelopmentSpecificationId int NULL,
	CFMPlacementId int NULL,
	PriorityPlacement int NULL,
	DevelopmentRatio varchar(500) NULL,
	EstimatedInstructionalTime decimal(9, 2) NULL,
	DevelopmentTime decimal(9, 2) NULL,
	CTID varchar(50) NULL,
	Notes nvarchar(MAX) NULL,
	Created datetime NOT NULL,
	CreatedById int NULL,
	LastUpdated datetime NOT NULL,
	LastUpdatedById int NULL,
	TrainingSolutionType varchar(100) NULL,
	RecommendedModality varchar(100) NULL,
	DevelopmentSpecification varchar(200) NULL,
	CandidatePlatform nvarchar(MAX) NULL,
	CFMPlacement varchar(500) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_ClusterAnalysis SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_ClusterAnalysis ADD CONSTRAINT
	DF_ClusterAnalysis_RowId DEFAULT (newid()) FOR RowId
GO
ALTER TABLE dbo.Tmp_ClusterAnalysis ADD CONSTRAINT
	DF_ClusterAnalysis_CTID DEFAULT ('ce-'+lower(newid())) FOR CTID
GO
ALTER TABLE dbo.Tmp_ClusterAnalysis ADD CONSTRAINT
	DF_ClusterAnalysis_Created DEFAULT (getdate()) FOR Created
GO
ALTER TABLE dbo.Tmp_ClusterAnalysis ADD CONSTRAINT
	DF_ClusterAnalysis_LastUpdated DEFAULT (getdate()) FOR LastUpdated
GO
SET IDENTITY_INSERT dbo.Tmp_ClusterAnalysis ON
GO
IF EXISTS(SELECT * FROM dbo.ClusterAnalysis)
	 EXEC('INSERT INTO dbo.Tmp_ClusterAnalysis (Id, RowId, RatingTaskId, TrainingSolutionTypeId, ClusterAnalysisTitle, RecommendedModalityId, DevelopmentSpecificationId, CFMPlacementId, PriorityPlacement, DevelopmentRatio, EstimatedInstructionalTime, DevelopmentTime, CTID, Notes, Created, CreatedById, LastUpdated, LastUpdatedById, TrainingSolutionType, RecommendedModality, DevelopmentSpecification, CandidatePlatform, CFMPlacement)
		SELECT Id, RowId, RatingTaskId, TrainingSolutionTypeId, ClusterAnalysisTitle, RecommendedModalityId, DevelopmentSpecificationId, CFMPlacementId, PriorityPlacement, DevelopmentRatio, CONVERT(decimal(9, 2), EstimatedInstructionalTime), CONVERT(decimal(9, 2), DevelopmentTime), CTID, Notes, Created, CreatedById, LastUpdated, LastUpdatedById, TrainingSolutionType, RecommendedModality, DevelopmentSpecification, CandidatePlatform, CFMPlacement FROM dbo.ClusterAnalysis WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_ClusterAnalysis OFF
GO
ALTER TABLE dbo.[ClusterAnalysis.HasCandidatePlatform]
	DROP CONSTRAINT FK_HasCandidatePlatform_ClusterAnalysis
GO
DROP TABLE dbo.ClusterAnalysis
GO
EXECUTE sp_rename N'dbo.Tmp_ClusterAnalysis', N'ClusterAnalysis', 'OBJECT' 
GO
ALTER TABLE dbo.ClusterAnalysis ADD CONSTRAINT
	PK_ClusterAnalysis PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.ClusterAnalysis ADD CONSTRAINT
	FK_ClusterAnalysis_RatingTask FOREIGN KEY
	(
	RatingTaskId
	) REFERENCES dbo.RatingTask
	(
	Id
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ClusterAnalysis ADD CONSTRAINT
	[FK_ClusterAnalysis_CFMPlacement.Concept] FOREIGN KEY
	(
	CFMPlacementId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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
	[FK_ClusterAnalysis_RecommendedModality.Concept] FOREIGN KEY
	(
	RecommendedModalityId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ClusterAnalysis ADD CONSTRAINT
	FK_ClusterAnalysis_DevSpecificationConcept FOREIGN KEY
	(
	DevelopmentSpecificationId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[ClusterAnalysis.HasCandidatePlatform] ADD CONSTRAINT
	FK_HasCandidatePlatform_ClusterAnalysis FOREIGN KEY
	(
	ClusterAnalysisId
	) REFERENCES dbo.ClusterAnalysis
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.[ClusterAnalysis.HasCandidatePlatform] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT