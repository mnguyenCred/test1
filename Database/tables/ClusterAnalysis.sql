USE [NavyRRL]
GO

/****** Object:  Table [dbo].[ClusterAnalysis]    Script Date: 4/1/2022 1:19:48 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ClusterAnalysis](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[RatingTaskId] [int] NOT NULL,
	[TrainingSolutionTypeId] [int] NOT NULL,
	[ClusterAnalysisTitle] [nvarchar](500) NOT NULL,
	[RecommendedModalityId] [int] NULL,
	[DevelopmentSpecification] [varchar](200) NULL,
	[CandidatePlatform] [nvarchar](100) NOT NULL,
	[CFMPlacement] [varchar](100) NULL,
	[PriorityPlacement] [int] NULL,
	[DevelopmentRatio] [varchar](100) NULL,
	[EstimatedInstructionalTime] [decimal](19, 2) NULL,
	[DevelopmentTime] [int] NULL,
	[CTID] [varchar](50) NULL,
	[Notes] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
	[TrainingSolutionType] [varchar](100) NULL,
	[RecommendedModality] [varchar](100) NULL,
 CONSTRAINT [PK_ClusterAnalysis] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ClusterAnalysis] ADD  CONSTRAINT [DF_ClusterAnalysis_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[ClusterAnalysis] ADD  CONSTRAINT [DF_ClusterAnalysis_CTID]  DEFAULT ('ce-'+lower(newid())) FOR [CTID]
GO

ALTER TABLE [dbo].[ClusterAnalysis] ADD  CONSTRAINT [DF_ClusterAnalysis_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[ClusterAnalysis] ADD  CONSTRAINT [DF_ClusterAnalysis_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[ClusterAnalysis]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis_ConceptScheme.Concept] FOREIGN KEY([TrainingSolutionTypeId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[ClusterAnalysis] CHECK CONSTRAINT [FK_ClusterAnalysis_ConceptScheme.Concept]
GO

ALTER TABLE [dbo].[ClusterAnalysis]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis_ConceptScheme.Concept1] FOREIGN KEY([RecommendedModalityId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[ClusterAnalysis] CHECK CONSTRAINT [FK_ClusterAnalysis_ConceptScheme.Concept1]
GO

ALTER TABLE [dbo].[ClusterAnalysis]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis_RatingTask] FOREIGN KEY([RatingTaskId])
REFERENCES [dbo].[RatingTask] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ClusterAnalysis] CHECK CONSTRAINT [FK_ClusterAnalysis_RatingTask]
GO


