USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[WorkRole]    Script Date: 10/1/2022 6:42:31 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[WorkRole](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](300) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CodedNotation] [varchar](100) NULL,
	[Version] [varchar](50) NULL,
	[CTID] [varchar](50) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_WorkRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[WorkRole] ADD  CONSTRAINT [DF_WorkRole_RowId_1]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[WorkRole] ADD  CONSTRAINT [DF_WorkRole_Created_1]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[WorkRole] ADD  CONSTRAINT [DF_WorkRole_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO


