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
ALTER TABLE dbo.Organization
	DROP CONSTRAINT DF_Organization_RowId
GO
ALTER TABLE dbo.Organization
	DROP CONSTRAINT DF_Organization_StatusId
GO
ALTER TABLE dbo.Organization
	DROP CONSTRAINT DF_Organization_Created
GO
ALTER TABLE dbo.Organization
	DROP CONSTRAINT DF_Organization_LastUpdated
GO
ALTER TABLE dbo.Organization
	DROP CONSTRAINT DF_Organization_ISQAOrganization
GO
ALTER TABLE dbo.Organization
	DROP CONSTRAINT DF_Organization_IsThirdPartyOrganization
GO
CREATE TABLE dbo.Tmp_Organization
	(
	Id int NOT NULL IDENTITY (1, 1),
	RowId uniqueidentifier NOT NULL,
	Name nvarchar(300) NOT NULL,
	Description nvarchar(MAX) NULL,
	CTID varchar(50) NULL,
	SubjectWebpage varchar(500) NULL,
	StatusId int NULL,
	CredentialRegistryId varchar(50) NULL,
	ImageURL varchar(500) NULL,
	AlternateName varchar(200) NULL,
	Created datetime NOT NULL,
	CreatedById int NULL,
	LastUpdated datetime NULL,
	LastUpdatedById int NULL,
	FoundingDate varchar(20) NULL,
	AvailabilityListing varchar(300) NULL,
	AgentPurpose varchar(500) NULL,
	AgentPurposeDescription varchar(MAX) NULL,
	MissionAndGoalsStatement varchar(500) NULL,
	MissionAndGoalsStatementDescription varchar(MAX) NULL,
	ISQAOrganization bit NULL,
	IsThirdPartyOrganization bit NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_Organization SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_Organization ADD CONSTRAINT
	DF_Organization_RowId DEFAULT (newid()) FOR RowId
GO
ALTER TABLE dbo.Tmp_Organization ADD CONSTRAINT
	DF_Organization_StatusId DEFAULT ((1)) FOR StatusId
GO
ALTER TABLE dbo.Tmp_Organization ADD CONSTRAINT
	DF_Organization_Created DEFAULT (getdate()) FOR Created
GO
ALTER TABLE dbo.Tmp_Organization ADD CONSTRAINT
	DF_Organization_LastUpdated DEFAULT (getdate()) FOR LastUpdated
GO
ALTER TABLE dbo.Tmp_Organization ADD CONSTRAINT
	DF_Organization_ISQAOrganization DEFAULT ((0)) FOR ISQAOrganization
GO
ALTER TABLE dbo.Tmp_Organization ADD CONSTRAINT
	DF_Organization_IsThirdPartyOrganization DEFAULT ((0)) FOR IsThirdPartyOrganization
GO
SET IDENTITY_INSERT dbo.Tmp_Organization ON
GO
IF EXISTS(SELECT * FROM dbo.Organization)
	 EXEC('INSERT INTO dbo.Tmp_Organization (Id, RowId, Name, Description, CTID, SubjectWebpage, StatusId, CredentialRegistryId, ImageURL, AlternateName, Created, CreatedById, LastUpdated, LastUpdatedById, FoundingDate, AvailabilityListing, AgentPurpose, AgentPurposeDescription, MissionAndGoalsStatement, MissionAndGoalsStatementDescription, ISQAOrganization, IsThirdPartyOrganization)
		SELECT Id, RowId, Name, Description, CTID, SubjectWebpage, StatusId, CredentialRegistryId, ImageURL, AlternateName, Created, CreatedById, LastUpdated, LastUpdatedById, FoundingDate, AvailabilityListing, AgentPurpose, AgentPurposeDescription, MissionAndGoalsStatement, MissionAndGoalsStatementDescription, ISQAOrganization, IsThirdPartyOrganization FROM dbo.Organization WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_Organization OFF
GO
DROP TABLE dbo.Organization
GO
EXECUTE sp_rename N'dbo.Tmp_Organization', N'Organization', 'OBJECT' 
GO
ALTER TABLE dbo.Organization ADD CONSTRAINT
	PK_Organization PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT