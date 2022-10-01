USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[Job]    Script Date: 10/1/2022 6:39:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Job](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CTID] [varchar](50) NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
	[CodedNotation] [varchar](100) NULL,
	[JobVersion] [int] NULL,
	[Description] [nvarchar](max) NULL,
	[ShortName30] [varchar](30) NULL,
	[ShortName14] [varchar](14) NULL,
	[RatingId] [int] NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Job] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Job] ADD  CONSTRAINT [DF_Job_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[Job] ADD  CONSTRAINT [DF_Job_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Job] ADD  CONSTRAINT [DF_Job_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO


