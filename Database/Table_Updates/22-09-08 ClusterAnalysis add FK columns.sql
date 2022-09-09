
--22-09-08 ClusterAnalysis add FK columns 
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
ALTER TABLE dbo.[ConceptScheme.Concept] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ClusterAnalysis ADD
	DevelopmentSpecificationId int NULL,
	--CandidatePlatformId int NULL,
	CFMPlacementId int NULL
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
--actually will not have this column, using ClusterAnalysis.HasCandidatePlatform
--ALTER TABLE dbo.ClusterAnalysis ADD CONSTRAINT
--	[FK_ClusterAnalysis_CandidatePlatform.Concept] FOREIGN KEY
--	(
--	CandidatePlatformId
--	) REFERENCES dbo.[ConceptScheme.Concept]
--	(
--	Id
--	) ON UPDATE  NO ACTION 
--	 ON DELETE  NO ACTION 
	
--GO
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
ALTER TABLE dbo.ClusterAnalysis SET (LOCK_ESCALATION = TABLE)
GO
COMMIT