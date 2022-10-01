USE [NavyRRL]
GO

/****** Object:  Table [dbo].[RatingLevelTask]    Script Date: 1/8/2022 9:29:24 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RatingLevelTask](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CodedNotation] [varchar](100) NULL,
	[RankId] [int] NOT NULL,
	[LevelId] [int] NOT NULL,
	[FunctionalAreaId] [int] NOT NULL,
	[SourceId] [int] NULL,
	[WorkElementTypeId] [int] NULL,
	[WorkElementTask] [nvarchar](max) NOT NULL,
	[TaskApplicabilityId] [int] NULL,
	[TaskStatusId] [int] NULL,
	[FormalTrainingGapId] [int] NULL,
	[Notes] [nvarchar](max) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
	[CTID] [varchar](50) NULL,
	[TrainingTaskId] [int] NULL,
 CONSTRAINT [PK_RatingLevelTask] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[RatingLevelTask] ADD  CONSTRAINT [DF_RatingLevelTask_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[RatingLevelTask] ADD  CONSTRAINT [DF_RatingLevelTask_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[RatingLevelTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_ConceptScheme.Concept] FOREIGN KEY([RankId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingLevelTask] CHECK CONSTRAINT [FK_RatingLevelTask_ConceptScheme.Concept]
GO

ALTER TABLE [dbo].[RatingLevelTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_ConceptScheme.Concept1] FOREIGN KEY([LevelId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingLevelTask] CHECK CONSTRAINT [FK_RatingLevelTask_ConceptScheme.Concept1]
GO

ALTER TABLE [dbo].[RatingLevelTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_ConceptScheme.Concept2] FOREIGN KEY([TaskApplicabilityId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingLevelTask] CHECK CONSTRAINT [FK_RatingLevelTask_ConceptScheme.Concept2]
GO

ALTER TABLE [dbo].[RatingLevelTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_ConceptScheme.Concept3] FOREIGN KEY([FormalTrainingGapId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[RatingLevelTask] CHECK CONSTRAINT [FK_RatingLevelTask_ConceptScheme.Concept3]
GO

ALTER TABLE [dbo].[RatingLevelTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_Course.Task] FOREIGN KEY([TrainingTaskId])
REFERENCES [dbo].[Course.Task] ([Id])
GO

ALTER TABLE [dbo].[RatingLevelTask] CHECK CONSTRAINT [FK_RatingLevelTask_Course.Task]
GO

ALTER TABLE [dbo].[RatingLevelTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_FunctionalArea] FOREIGN KEY([FunctionalAreaId])
REFERENCES [dbo].[FunctionalArea] ([Id])
GO

ALTER TABLE [dbo].[RatingLevelTask] CHECK CONSTRAINT [FK_RatingLevelTask_FunctionalArea]
GO

ALTER TABLE [dbo].[RatingLevelTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_Source] FOREIGN KEY([SourceId])
REFERENCES [dbo].[Source] ([Id])
GO

ALTER TABLE [dbo].[RatingLevelTask] CHECK CONSTRAINT [FK_RatingLevelTask_Source]
GO

ALTER TABLE [dbo].[RatingLevelTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask_WorkElementType] FOREIGN KEY([WorkElementTypeId])
REFERENCES [dbo].[WorkElementType] ([Id])
GO

ALTER TABLE [dbo].[RatingLevelTask] CHECK CONSTRAINT [FK_RatingLevelTask_WorkElementType]
GO


