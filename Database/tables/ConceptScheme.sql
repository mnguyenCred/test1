USE [NavyRRL]
GO

/****** Object:  Table [dbo].[ConceptScheme]    Script Date: 12/12/2021 9:29:36 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ConceptScheme](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](400) NOT NULL,
	[Description] [varchar](max) NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CTID] [varchar](50) NULL,
	[SchemaUri] [varchar](50) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
	[LastApproved] [datetime] NULL,
	[LastApprovedById] [int] NULL,
	[LastPublished] [datetime] NULL,
	[LastPublishedById] [int] NULL,
 CONSTRAINT [PK_ConceptScheme] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ConceptScheme] ADD  CONSTRAINT [DF_ConceptScheme_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[ConceptScheme] ADD  CONSTRAINT [DF_ConceptScheme_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[ConceptScheme] ADD  CONSTRAINT [DF_ConceptScheme_CreatedById]  DEFAULT ((1)) FOR [CreatedById]
GO

ALTER TABLE [dbo].[ConceptScheme] ADD  CONSTRAINT [DF_ConceptScheme_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[ConceptScheme] ADD  CONSTRAINT [DF_ConceptScheme_LastUpdatedById]  DEFAULT ((1)) FOR [LastUpdatedById]
GO

ALTER TABLE [dbo].[ConceptScheme]  WITH CHECK ADD  CONSTRAINT [FK_ConceptScheme__AccountCreatedBy] FOREIGN KEY([CreatedById])
REFERENCES [dbo].[Account] ([Id])
GO

ALTER TABLE [dbo].[ConceptScheme] CHECK CONSTRAINT [FK_ConceptScheme__AccountCreatedBy]
GO

ALTER TABLE [dbo].[ConceptScheme]  WITH CHECK ADD  CONSTRAINT [FK_ConceptScheme_AccountApprovedBy] FOREIGN KEY([LastApprovedById])
REFERENCES [dbo].[Account] ([Id])
GO

ALTER TABLE [dbo].[ConceptScheme] CHECK CONSTRAINT [FK_ConceptScheme_AccountApprovedBy]
GO

ALTER TABLE [dbo].[ConceptScheme]  WITH CHECK ADD  CONSTRAINT [FK_ConceptScheme_AccountLastUpdatedBy] FOREIGN KEY([LastUpdatedById])
REFERENCES [dbo].[Account] ([Id])
GO

ALTER TABLE [dbo].[ConceptScheme] CHECK CONSTRAINT [FK_ConceptScheme_AccountLastUpdatedBy]
GO

ALTER TABLE [dbo].[ConceptScheme]  WITH CHECK ADD  CONSTRAINT [FK_ConceptScheme_AccountPublishedBy] FOREIGN KEY([LastPublishedById])
REFERENCES [dbo].[Account] ([Id])
GO

ALTER TABLE [dbo].[ConceptScheme] CHECK CONSTRAINT [FK_ConceptScheme_AccountPublishedBy]
GO


