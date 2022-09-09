USE [NavyRRL]
GO

/****** Object:  Table [dbo].[RatingContext]    Script Date: 8/29/2022 6:02:03 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RatingContext](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[RatingId] [int] NOT NULL,
	[RatingTaskId] [int] NOT NULL,
	[BilletTitleId] [int] NULL,
	[RankId] [int] NOT NULL,
	[WorkRoleId] [int] NULL,
	[TaskApplicabilityId] [int] NULL,
	[FormalTrainingGapId] [int] NULL,
	[TrainingTaskId] [int] NULL,
	[TaskStatusId] [int] NULL,
	[CodedNotation] [varchar](100) NULL,
	[CTID] [varchar](50) NULL,
	[Notes] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_RatingContext] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[RatingContext] ADD  CONSTRAINT [DF_RatingContext_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[RatingContext] ADD  CONSTRAINT [DF_RatingContext_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[RatingContext] ADD  CONSTRAINT [DF_RatingContext_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[RatingContext]  WITH CHECK ADD  CONSTRAINT [FK_RatingContext_BilletTitle] FOREIGN KEY([BilletTitleId])
REFERENCES [dbo].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RatingContext] CHECK CONSTRAINT [FK_RatingContext_BilletTitle]
GO

ALTER TABLE [dbo].[RatingContext]  WITH CHECK ADD  CONSTRAINT [FK_RatingContext_Rank] FOREIGN KEY([RankId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingContext] CHECK CONSTRAINT [FK_RatingContext_Rank]
GO

ALTER TABLE [dbo].[RatingContext]  WITH CHECK ADD  CONSTRAINT [FK_RatingContext_Rating] FOREIGN KEY([RatingId])
REFERENCES [dbo].[Rating] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RatingContext] CHECK CONSTRAINT [FK_RatingContext_Rating]
GO

ALTER TABLE [dbo].[RatingContext]  WITH CHECK ADD  CONSTRAINT [FK_RatingContext_RatingTask] FOREIGN KEY([RatingTaskId])
REFERENCES [dbo].[RatingTask] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RatingContext] CHECK CONSTRAINT [FK_RatingContext_RatingTask]
GO

ALTER TABLE [dbo].[RatingContext]  WITH CHECK ADD  CONSTRAINT [FK_RatingContext_TaskApplicability] FOREIGN KEY([TaskApplicabilityId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingContext] CHECK CONSTRAINT [FK_RatingContext_TaskApplicability]
GO

ALTER TABLE [dbo].[RatingContext]  WITH CHECK ADD  CONSTRAINT [FK_RatingContext_TrainingGap] FOREIGN KEY([FormalTrainingGapId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingContext] CHECK CONSTRAINT [FK_RatingContext_TrainingGap]
GO

ALTER TABLE [dbo].[RatingContext]  WITH CHECK ADD  CONSTRAINT [FK_RatingContext_TrainingTask] FOREIGN KEY([TrainingTaskId])
REFERENCES [dbo].[Course.Task] ([Id])
GO

ALTER TABLE [dbo].[RatingContext] CHECK CONSTRAINT [FK_RatingContext_TrainingTask]
GO

ALTER TABLE [dbo].[RatingContext]  WITH CHECK ADD  CONSTRAINT [FK_RatingContext_WorkRole] FOREIGN KEY([WorkRoleId])
REFERENCES [dbo].[WorkRole] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RatingContext] CHECK CONSTRAINT [FK_RatingContext_WorkRole]
GO

