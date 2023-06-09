/*
   Thursday, January 12, 20231:32:20 PM
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
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT [FK_ClusterAnalysis_CFMPlacement.Concept]
GO
ALTER TABLE dbo.[ConceptScheme.Concept] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
DROP INDEX IX_ClusterAnalysis_CFMPlacementId ON dbo.ClusterAnalysis
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP COLUMN CFMPlacementTypeId
GO
ALTER TABLE dbo.ClusterAnalysis SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
