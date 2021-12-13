USE [NavyRRL]
GO

/****** Object:  Table [dbo].[Rating]    Script Date: 12/12/2021 9:29:08 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Rating](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CTID] [varchar](50) NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[CodedNotation] [varchar](100) NULL,
	[Description] [varchar](max) NULL,
	[Version] [varchar](30) NULL,
	[RatingUploadDate] [varchar](50) NULL,
	[RatingPublicationDate] [varchar](50) NULL,
	[Image] [varchar](500) NULL,
	[CredentialRegistryId] [varchar](50) NULL,
	[Created] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
	[LastPublished] [datetime] NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CredentialRegistryURI] [varchar](200) NULL,
	[MainEntityOfPage] [varchar](600) NULL,
 CONSTRAINT [PK_Rating] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Rating] ADD  CONSTRAINT [DF_Rating_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Rating] ADD  CONSTRAINT [DF_Rating_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[Rating] ADD  CONSTRAINT [DF_Rating_LastPublished]  DEFAULT (getdate()) FOR [LastPublished]
GO

ALTER TABLE [dbo].[Rating] ADD  CONSTRAINT [DF_Rating_RowId]  DEFAULT (newid()) FOR [RowId]
GO


