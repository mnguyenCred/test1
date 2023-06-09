USE [Navy_RRL_V2]
GO
/****** Object:  Table [dbo].[ClusterAnalysis]    Script Date: 10/2/2022 9:20:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ClusterAnalysis](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[TrainingSolutionTypeId] [int] NULL,
	[ClusterAnalysisTitleId] [int] NULL,
	[RecommendedModalityId] [int] NULL,
	[DevelopmentSpecificationId] [int] NULL,
	[CFMPlacementId] [int] NULL,
	[PriorityPlacement] [int] NULL,
	[DevelopmentRatio] [varchar](500) NULL,
	[EstimatedInstructionalTime] [decimal](9, 2) NULL,
	[DevelopmentTime] [decimal](9, 2) NULL,
	[CTID] [varchar](50) NULL,
	[Notes] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_ClusterAnalysis] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_ClusterAnalysis_RowId] UNIQUE NONCLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_ClusterAnalysis_CFMPlacementId]    Script Date: 10/2/2022 9:20:12 AM ******/
CREATE NONCLUSTERED INDEX [IX_ClusterAnalysis_CFMPlacementId] ON [dbo].[ClusterAnalysis]
(
	[CFMPlacementId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_ClusterAnalysis_ClusterAnalysisTitleId]    Script Date: 10/2/2022 9:20:12 AM ******/
CREATE NONCLUSTERED INDEX [IX_ClusterAnalysis_ClusterAnalysisTitleId] ON [dbo].[ClusterAnalysis]
(
	[ClusterAnalysisTitleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_ClusterAnalysis_DevelopmentSpecificationId]    Script Date: 10/2/2022 9:20:12 AM ******/
CREATE NONCLUSTERED INDEX [IX_ClusterAnalysis_DevelopmentSpecificationId] ON [dbo].[ClusterAnalysis]
(
	[DevelopmentSpecificationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_ClusterAnalysis_RecommendedModalityId]    Script Date: 10/2/2022 9:20:12 AM ******/
CREATE NONCLUSTERED INDEX [IX_ClusterAnalysis_RecommendedModalityId] ON [dbo].[ClusterAnalysis]
(
	[RecommendedModalityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_ClusterAnalysis_TrainingSolutionTypeId]    Script Date: 10/2/2022 9:20:12 AM ******/
CREATE NONCLUSTERED INDEX [IX_ClusterAnalysis_TrainingSolutionTypeId] ON [dbo].[ClusterAnalysis]
(
	[TrainingSolutionTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ClusterAnalysis] ADD  CONSTRAINT [DF_ClusterAnalysis_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[ClusterAnalysis] ADD  CONSTRAINT [DF_ClusterAnalysis_CTID]  DEFAULT ('ce-'+lower(newid())) FOR [CTID]
GO
ALTER TABLE [dbo].[ClusterAnalysis] ADD  CONSTRAINT [DF_ClusterAnalysis_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[ClusterAnalysis] ADD  CONSTRAINT [DF_ClusterAnalysis_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[ClusterAnalysis]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis_CFMPlacement.Concept] FOREIGN KEY([CFMPlacementId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO
ALTER TABLE [dbo].[ClusterAnalysis] CHECK CONSTRAINT [FK_ClusterAnalysis_CFMPlacement.Concept]
GO
ALTER TABLE [dbo].[ClusterAnalysis]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis_ClusterAnalysisTitle] FOREIGN KEY([ClusterAnalysisTitleId])
REFERENCES [dbo].[ClusterAnalysisTitle] ([Id])
GO
ALTER TABLE [dbo].[ClusterAnalysis] CHECK CONSTRAINT [FK_ClusterAnalysis_ClusterAnalysisTitle]
GO
ALTER TABLE [dbo].[ClusterAnalysis]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis_DevSpecificationConcept] FOREIGN KEY([DevelopmentSpecificationId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO
ALTER TABLE [dbo].[ClusterAnalysis] CHECK CONSTRAINT [FK_ClusterAnalysis_DevSpecificationConcept]
GO
ALTER TABLE [dbo].[ClusterAnalysis]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis_RecommendedModality.Concept] FOREIGN KEY([RecommendedModalityId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO
ALTER TABLE [dbo].[ClusterAnalysis] CHECK CONSTRAINT [FK_ClusterAnalysis_RecommendedModality.Concept]
GO
ALTER TABLE [dbo].[ClusterAnalysis]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis_TrainingSolution.Concept] FOREIGN KEY([TrainingSolutionTypeId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO
ALTER TABLE [dbo].[ClusterAnalysis] CHECK CONSTRAINT [FK_ClusterAnalysis_TrainingSolution.Concept]
GO
