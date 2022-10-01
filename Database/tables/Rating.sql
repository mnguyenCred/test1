USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[Rating]    Script Date: 10/1/2022 6:40:03 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Rating](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CTID] [varchar](50) NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
	[CodedNotation] [varchar](100) NULL,
	[Description] [nvarchar](max) NULL,
	[MainEntityOfPage] [varchar](600) NULL,
	[Version] [varchar](30) NULL,
	[RatingUploadDate] [varchar](50) NULL,
	[RatingPublicationDate] [varchar](50) NULL,
	[Image] [varchar](500) NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Rating] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Rating_RowId] UNIQUE NONCLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Rating] ADD  CONSTRAINT [DF_Rating_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[Rating] ADD  CONSTRAINT [DF_Rating_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Rating] ADD  CONSTRAINT [DF_Rating_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO


