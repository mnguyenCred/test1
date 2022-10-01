USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[ReferenceResource]    Script Date: 10/1/2022 6:41:07 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ReferenceResource](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](300) NOT NULL,
	[Description] [varchar](max) NULL,
	[CodedNotation] [varchar](100) NULL,
	[PublicationDate] [nvarchar](50) NULL,
	[SubjectWebpage] [varchar](500) NULL,
	[StatusTypeId] [int] NULL,
	[VersionIdentifier] [varchar](100) NULL,
	[Note] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
	[CTID] [varchar](50) NULL,
	[ReferenceTypeId] [int] NULL,
 CONSTRAINT [PK_ReferenceResource] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ReferenceResource] ADD  CONSTRAINT [DF_ReferenceResource_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[ReferenceResource] ADD  CONSTRAINT [DF_ReferenceResource_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[ReferenceResource] ADD  CONSTRAINT [DF_ReferenceResource_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO


