USE [NavyRRL]
GO

/****** Object:  Table [dbo].[Comment]    Script Date: 1/8/2022 9:00:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Comment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[RmtlProjectId] [int] NULL,
	[AppliesTo] [uniqueidentifier] NOT NULL,
	[Comment] [varchar](max) NOT NULL,
	[Status] [int] NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Comment] ADD  CONSTRAINT [DF_Comment_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[Comment] ADD  CONSTRAINT [DF_Comment_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Comment] ADD  CONSTRAINT [DF_Table_1_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[Comment]  WITH CHECK ADD  CONSTRAINT [FK_Comment_RMTLProject] FOREIGN KEY([RmtlProjectId])
REFERENCES [dbo].[RMTLProject] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_Comment_RMTLProject]
GO


