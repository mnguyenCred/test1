USE [NavyRRL]
GO

/****** Object:  Table [dbo].[RmtlProject.BilletTask]    Script Date: 1/21/2022 1:00:22 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RmtlProject.BilletTask](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[ProjectBilletId] [int] NOT NULL,
	[RatingTaskId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_RmtlProject.BilletTask] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RmtlProject.BilletTask] ADD  CONSTRAINT [DF_RmtlProject.BilletTask_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[RmtlProject.BilletTask] ADD  CONSTRAINT [DF_RmtlProject.BilletTask_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[RmtlProject.BilletTask] ADD  CONSTRAINT [DF_RmtlProject.BilletTask_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[RmtlProject.BilletTask]  WITH CHECK ADD  CONSTRAINT [FK_RmtlProject.BilletTask_RatingTask] FOREIGN KEY([RatingTaskId])
REFERENCES [dbo].[RatingTask] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RmtlProject.BilletTask] CHECK CONSTRAINT [FK_RmtlProject.BilletTask_RatingTask]
GO

ALTER TABLE [dbo].[RmtlProject.BilletTask]  WITH CHECK ADD  CONSTRAINT [FK_RmtlProject.BilletTask_RmtlProject.Billet] FOREIGN KEY([ProjectBilletId])
REFERENCES [dbo].[RmtlProject.Billet] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RmtlProject.BilletTask] CHECK CONSTRAINT [FK_RmtlProject.BilletTask_RmtlProject.Billet]
GO


