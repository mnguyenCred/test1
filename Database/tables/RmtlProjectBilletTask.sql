USE [NavyRRL]
GO

/****** Object:  Table [dbo].[RmtlProjectBilletTask]    Script Date: 1/8/2022 8:58:40 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RmtlProjectBilletTask](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProjectBilletId] [int] NOT NULL,
	[RatingLevelTaskId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_RmtlProjectBilletTask] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RmtlProjectBilletTask] ADD  CONSTRAINT [DF_RmtlProjectBilletTask_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[RmtlProjectBilletTask] ADD  CONSTRAINT [DF_RmtlProjectBilletTask_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[RmtlProjectBilletTask]  WITH CHECK ADD  CONSTRAINT [FK_RmtlProjectBilletTask_RatingLevelTask] FOREIGN KEY([RatingLevelTaskId])
REFERENCES [dbo].[RatingLevelTask] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RmtlProjectBilletTask] CHECK CONSTRAINT [FK_RmtlProjectBilletTask_RatingLevelTask]
GO

ALTER TABLE [dbo].[RmtlProjectBilletTask]  WITH CHECK ADD  CONSTRAINT [FK_RmtlProjectBilletTask_RmtlProject.Billet] FOREIGN KEY([ProjectBilletId])
REFERENCES [dbo].[RmtlProject.Billet] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RmtlProjectBilletTask] CHECK CONSTRAINT [FK_RmtlProjectBilletTask_RmtlProject.Billet]
GO


