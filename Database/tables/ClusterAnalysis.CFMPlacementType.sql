USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[ClusterAnalysis.CFMPlacementType]    Script Date: 1/12/2023 1:25:21 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ClusterAnalysis.CFMPlacementType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[ClusterAnalysisId] [int] NOT NULL,
	[CFMPlacementConceptId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_ClusterAnalysis.CFMPlacementType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_ClusterAnalysis.CFMPlacementType] UNIQUE NONCLUSTERED 
(
	[ClusterAnalysisId] ASC,
	[CFMPlacementConceptId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ClusterAnalysis.CFMPlacementType] ADD  CONSTRAINT [DF_ClusterAnalysis.CFMPlacementType_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[ClusterAnalysis.CFMPlacementType] ADD  CONSTRAINT [DF_ClusterAnalysis.CFMPlacementType_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[ClusterAnalysis.CFMPlacementType]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis.CFMPlacementType_ClusterAnalysis] FOREIGN KEY([ClusterAnalysisId])
REFERENCES [dbo].[ClusterAnalysis] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ClusterAnalysis.CFMPlacementType] CHECK CONSTRAINT [FK_ClusterAnalysis.CFMPlacementType_ClusterAnalysis]
GO

ALTER TABLE [dbo].[ClusterAnalysis.CFMPlacementType]  WITH CHECK ADD  CONSTRAINT [FK_ClusterAnalysis.CFMPlacementType_ConceptScheme.Concept] FOREIGN KEY([CFMPlacementConceptId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[ClusterAnalysis.CFMPlacementType] CHECK CONSTRAINT [FK_ClusterAnalysis.CFMPlacementType_ConceptScheme.Concept]
GO


