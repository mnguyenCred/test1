USE [NavyRRL]
GO

ALTER TABLE [dbo].[ClusterAnalysis] DROP CONSTRAINT [FK_ClusterAnalysis_TrainingSolution.Concept]
GO

ALTER TABLE [dbo].[ClusterAnalysis] DROP CONSTRAINT [FK_ClusterAnalysis_RecommendedModality.Concept1]
GO

ALTER TABLE [dbo].[ClusterAnalysis] DROP CONSTRAINT [FK_ClusterAnalysis_RatingTask]
GO

ALTER TABLE [dbo].[ClusterAnalysis] DROP CONSTRAINT [DF_ClusterAnalysis_LastUpdated]
GO

ALTER TABLE [dbo].[ClusterAnalysis] DROP CONSTRAINT [DF_ClusterAnalysis_Created]
GO

ALTER TABLE [dbo].[ClusterAnalysis] DROP CONSTRAINT [DF_ClusterAnalysis_CTID]
GO

ALTER TABLE [dbo].[ClusterAnalysis] DROP CONSTRAINT [DF_ClusterAnalysis_RowId]
GO

/****** Object:  Table [dbo].[ClusterAnalysis]    Script Date: 4/8/2022 5:12:18 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClusterAnalysis]') AND type in (N'U'))
DROP TABLE [dbo].[ClusterAnalysis]
GO

/****** Object:  Table [dbo].[ClusterAnalysis]    Script Date: 4/8/2022 5:12:18 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ClusterAnalysis](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[RatingTaskId] [int] NOT NULL,
	[TrainingSolutionTypeId] [int] NULL,
	[ClusterAnalysisTitle] [nvarchar](500) NULL,
	[RecommendedModalityId] [int] NULL,
	[DevelopmentSpecificationId] [int] NULL,
	[CandidatePlatform] [nvarchar](100) NULL,
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
	[DevelopmentSpecification] [varchar](200) NULL,
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

ALTER TABLE [dbo].[ClusterAnalysis]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis_RatingTask] FOREIGN KEY([RatingTaskId])
REFERENCES [dbo].[RatingTask] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ClusterAnalysis] CHECK CONSTRAINT [FK_ClusterAnalysis_RatingTask]
GO

ALTER TABLE [dbo].[ClusterAnalysis]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis_RecommendedModality.Concept1] FOREIGN KEY([RecommendedModalityId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[ClusterAnalysis] CHECK CONSTRAINT [FK_ClusterAnalysis_RecommendedModality.Concept1]
GO

ALTER TABLE [dbo].[ClusterAnalysis]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis_TrainingSolution.Concept] FOREIGN KEY([TrainingSolutionTypeId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[ClusterAnalysis] CHECK CONSTRAINT [FK_ClusterAnalysis_TrainingSolution.Concept]
GO


