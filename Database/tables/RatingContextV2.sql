USE [Navy_RRL_V2]
GO
/****** Object:  Table [dbo].[RatingContextV2]    Script Date: 10/2/2022 9:20:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RatingContextV2](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[RatingId] [int] NOT NULL,
	[RatingTaskId] [uniqueidentifier] NOT NULL,
	[BilletTitleId] [uniqueidentifier] NULL,
	[PayGradeTypeId] [uniqueidentifier] NOT NULL,
	[TaskApplicabilityId] [uniqueidentifier] NULL,
	[FormalTrainingGapId] [uniqueidentifier] NULL,
	[TrainingTaskId] [uniqueidentifier] NULL,
	[ClusterAnalysisId] [uniqueidentifier] NULL,
	[TaskStatusId] [int] NULL,
	[CodedNotation] [varchar](100) NULL,
	[Notes] [nvarchar](max) NULL,
	[CTID] [varchar](50) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_RatingContextV2] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[RatingContextV2]  WITH CHECK ADD  CONSTRAINT [FK_RatingContextV2_BilletTitle] FOREIGN KEY([BilletTitleId])
REFERENCES [dbo].[Job] ([RowId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RatingContextV2] CHECK CONSTRAINT [FK_RatingContextV2_BilletTitle]
GO
ALTER TABLE [dbo].[RatingContextV2]  WITH CHECK ADD  CONSTRAINT [FK_RatingContextV2_ClusterAnalysis] FOREIGN KEY([ClusterAnalysisId])
REFERENCES [dbo].[ClusterAnalysis] ([RowId])
GO
ALTER TABLE [dbo].[RatingContextV2] CHECK CONSTRAINT [FK_RatingContextV2_ClusterAnalysis]
GO
ALTER TABLE [dbo].[RatingContextV2]  WITH CHECK ADD  CONSTRAINT [FK_RatingContextV2_TaskApplicability] FOREIGN KEY([TaskApplicabilityId])
REFERENCES [dbo].[ConceptScheme.Concept] ([RowId])
GO
ALTER TABLE [dbo].[RatingContextV2] CHECK CONSTRAINT [FK_RatingContextV2_TaskApplicability]
GO
ALTER TABLE [dbo].[RatingContextV2]  WITH CHECK ADD  CONSTRAINT [FK_RatingContextV2_TrainingGap] FOREIGN KEY([FormalTrainingGapId])
REFERENCES [dbo].[ConceptScheme.Concept] ([RowId])
GO
ALTER TABLE [dbo].[RatingContextV2] CHECK CONSTRAINT [FK_RatingContextV2_TrainingGap]
GO
ALTER TABLE [dbo].[RatingContextV2]  WITH CHECK ADD  CONSTRAINT [FK_RatingContextV2_TrainingTask] FOREIGN KEY([TrainingTaskId])
REFERENCES [dbo].[TrainingTask] ([RowId])
GO
ALTER TABLE [dbo].[RatingContextV2] CHECK CONSTRAINT [FK_RatingContextV2_TrainingTask]
GO
