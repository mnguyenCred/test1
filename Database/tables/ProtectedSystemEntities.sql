USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[ProtectedSystemEntities]    Script Date: 1/12/2023 4:16:03 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ProtectedSystemEntities](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[EntityRowId] [uniqueidentifier] NOT NULL,
	[Note] [varchar](100) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_ProtectedSystemEntities] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ProtectedSystemEntities] ADD  CONSTRAINT [DF_ProtectedSystemEntities_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[ProtectedSystemEntities] ADD  CONSTRAINT [DF_ProtectedSystemEntities_Created]  DEFAULT (getdate()) FOR [Created]
GO


