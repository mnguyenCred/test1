/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--22-04-02-remove unused columns

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
ALTER TABLE dbo.Organization
	DROP CONSTRAINT DF_Organization_StatusId
GO
ALTER TABLE dbo.Organization
	DROP CONSTRAINT DF_Organization_ISQAOrganization
GO
ALTER TABLE dbo.Organization
	DROP CONSTRAINT DF_Organization_IsThirdPartyOrganization
GO
ALTER TABLE dbo.Organization
	DROP COLUMN StatusId, CredentialRegistryId, ImageURL, FoundingDate, AvailabilityListing, AgentPurpose, AgentPurposeDescription, MissionAndGoalsStatement, MissionAndGoalsStatementDescription, ISQAOrganization, IsThirdPartyOrganization
GO
ALTER TABLE dbo.Organization SET (LOCK_ESCALATION = TABLE)
GO
COMMIT