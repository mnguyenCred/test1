USE [NavyRRL]
GO

/****** Object:  Table [dbo].[RatingTask.HasRatingContext]    Script Date: 8/29/2022 5:32:28 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RatingTask.HasRatingContext](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[RatingTaskId] [int] NOT NULL,
	[RatingId] [int] NOT NULL,
	[RankId] [int] NOT NULL,
	[WorkRoleId] [int] NULL,
	[TaskApplicabilityId] [int] NULL,
	[FormalTrainingGapId] [int] NULL,
	[TrainingTaskId] [int] NULL,
	[CodedNotation] [varchar](100) NULL,
	[CTID] [varchar](50) NULL,
	[Notes] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_RatingLevelTask.HasRatingContext] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[RatingTask.HasRatingContext] ADD  CONSTRAINT [DF_RatingTask.HasRatingContext_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[RatingTask.HasRatingContext] ADD  CONSTRAINT [DF_RatingLevelTask.HasRatingContext_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[RatingTask.HasRatingContext] ADD  CONSTRAINT [DF_RatingTask.HasRatingContext_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[RatingTask.HasRatingContext]  WITH CHECK ADD  CONSTRAINT [FK_RatingTask.HasRatingContext_Rating] FOREIGN KEY([RatingId])
REFERENCES [dbo].[Rating] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RatingTask.HasRatingContext] CHECK CONSTRAINT [FK_RatingTask.HasRatingContext_Rating]
GO

ALTER TABLE [dbo].[RatingTask.HasRatingContext]  WITH CHECK ADD  CONSTRAINT [FK_RatingTask.HasRatingContext_TrainingTask] FOREIGN KEY([TrainingTaskId])
REFERENCES [dbo].[Course.Task] ([Id])
GO

ALTER TABLE [dbo].[RatingTask.HasRatingContext] CHECK CONSTRAINT [FK_RatingTask.HasRatingContext_TrainingTask]
GO

ALTER TABLE [dbo].[RatingTask.HasRatingContext]  WITH CHECK ADD  CONSTRAINT [FK_RatingTask.HasRatingContextTask_Task] FOREIGN KEY([RatingTaskId])
REFERENCES [dbo].[RatingTask] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RatingTask.HasRatingContext] CHECK CONSTRAINT [FK_RatingTask.HasRatingContextTask_Task]
GO
--====================================
ALTER TABLE [dbo].[RatingTask.HasRatingContext]  WITH CHECK ADD  CONSTRAINT [FK_HasRatingContext_Rank] FOREIGN KEY([RankId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingTask.HasRatingContext] CHECK CONSTRAINT [FK_HasRatingContext_Rank]
GO
--====================================

ALTER TABLE [dbo].[RatingTask.HasRatingContext]  WITH CHECK ADD  CONSTRAINT [FK_HasRatingContext_TaskApplicability] FOREIGN KEY([TaskApplicabilityId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingTask.HasRatingContext] CHECK CONSTRAINT [FK_HasRatingContext_TaskApplicability]
GO
--====================================
ALTER TABLE [dbo].[RatingTask.HasRatingContext]  WITH CHECK ADD  CONSTRAINT [FK_HasRatingContext_TrainingGap] FOREIGN KEY([FormalTrainingGapId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingTask.HasRatingContext] CHECK CONSTRAINT [FK_HasRatingContext_TrainingGap]
GO
--====================================
