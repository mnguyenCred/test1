USE [NavyRRL]
GO

/****** Object:  Table [dbo].[RatingTask]    Script Date: 1/16/2022 10:57:34 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RatingTaskV2](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[RankId] [int] NOT NULL,
	[LevelId] [int] NOT NULL,
	[FunctionalAreaId] [int] NULL,
	[SourceId] [int] NULL,
	[WorkElementTypeId] [int] NULL,
	[Description] [nvarchar](max) NOT NULL,
	[TaskApplicabilityId] [int] NULL,
	[TaskStatusId] [int] NULL,
	[FormalTrainingGapId] [int] NULL,
	[CodedNotation] [varchar](100) NULL,
	[TrainingTaskId] [int] NULL,
	[CTID] [varchar](50) NULL,
	[Notes] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
	[ApplicabilityType] [uniqueidentifier] NULL,
	[HasReferenceResource] [uniqueidentifier] NULL,
	[HasTrainingTask] [uniqueidentifier] NULL,
	[PayGradeType] [uniqueidentifier] NULL,
	[ReferenceType] [uniqueidentifier] NULL,
	[TrainingGapType] [uniqueidentifier] NULL,
 CONSTRAINT [PK_RatingLevelTask] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[RatingTask] ADD  CONSTRAINT [DF_RatingTask_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[RatingTask] ADD  CONSTRAINT [DF_RatingLevelTask_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[RatingTask] ADD  CONSTRAINT [DF_RatingLevelTask_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[RatingTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_Course.Task] FOREIGN KEY([TrainingTaskId])
REFERENCES [dbo].[Course.Task] ([Id])
GO

ALTER TABLE [dbo].[RatingTask] CHECK CONSTRAINT [FK_RatingLevelTask_Course.Task]
GO

ALTER TABLE [dbo].[RatingTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_FunctionalArea] FOREIGN KEY([FunctionalAreaId])
REFERENCES [dbo].[FunctionalArea] ([Id])
GO

ALTER TABLE [dbo].[RatingTask] CHECK CONSTRAINT [FK_RatingLevelTask_FunctionalArea]
GO

ALTER TABLE [dbo].[RatingTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_Level] FOREIGN KEY([LevelId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingTask] CHECK CONSTRAINT [FK_RatingLevelTask_Level]
GO

ALTER TABLE [dbo].[RatingTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_Rank] FOREIGN KEY([RankId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingTask] CHECK CONSTRAINT [FK_RatingLevelTask_Rank]
GO

ALTER TABLE [dbo].[RatingTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_Source] FOREIGN KEY([SourceId])
REFERENCES [dbo].[Source] ([Id])
GO

ALTER TABLE [dbo].[RatingTask] CHECK CONSTRAINT [FK_RatingLevelTask_Source]
GO

ALTER TABLE [dbo].[RatingTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_TaskApplicability] FOREIGN KEY([TaskApplicabilityId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingTask] CHECK CONSTRAINT [FK_RatingLevelTask_TaskApplicability]
GO

ALTER TABLE [dbo].[RatingTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_TrainingGap] FOREIGN KEY([FormalTrainingGapId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingTask] CHECK CONSTRAINT [FK_RatingLevelTask_TrainingGap]
GO

ALTER TABLE [dbo].[RatingTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_WorkElementType] FOREIGN KEY([WorkElementTypeId])
REFERENCES [dbo].[WorkElementType] ([Id])
GO

ALTER TABLE [dbo].[RatingTask] CHECK CONSTRAINT [FK_RatingLevelTask_WorkElementType]
GO


