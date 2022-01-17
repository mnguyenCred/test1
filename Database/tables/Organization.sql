USE [NavyRRL]
GO

/****** Object:  Table [dbo].[Organization]    Script Date: 1/17/2022 10:56:35 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Organization](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](300) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CTID] [varchar](50) NULL,
	[SubjectWebpage] [varchar](500) NULL,
	[StatusId] [int] NULL,
	[CredentialRegistryId] [varchar](50) NULL,
	[ImageURL] [varchar](500) NULL,
	[AlternateName] [varchar](200) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
	[FoundingDate] [varchar](20) NULL,
	[AvailabilityListing] [varchar](300) NULL,
	[AgentPurpose] [varchar](500) NULL,
	[AgentPurposeDescription] [varchar](max) NULL,
	[MissionAndGoalsStatement] [varchar](500) NULL,
	[MissionAndGoalsStatementDescription] [varchar](max) NULL,
	[ISQAOrganization] [bit] NULL,
	[IsThirdPartyOrganization] [bit] NULL,
 CONSTRAINT [PK_Organization] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Organization] ADD  CONSTRAINT [DF_Organization_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[Organization] ADD  CONSTRAINT [DF_Organization_StatusId]  DEFAULT ((1)) FOR [StatusId]
GO

ALTER TABLE [dbo].[Organization] ADD  CONSTRAINT [DF_Organization_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Organization] ADD  CONSTRAINT [DF_Organization_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[Organization] ADD  CONSTRAINT [DF_Organization_ISQAOrganization]  DEFAULT ((0)) FOR [ISQAOrganization]
GO

ALTER TABLE [dbo].[Organization] ADD  CONSTRAINT [DF_Organization_IsThirdPartyOrganization]  DEFAULT ((0)) FOR [IsThirdPartyOrganization]
GO


