USE [NavyRRL]
GO

/****** Object:  Table [dbo].[ClusterAnalysis.HasCandidatePlatform]    Script Date: 9/8/2022 5:37:47 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ClusterAnalysis.HasCandidatePlatform](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[ClusterAnalysisId] [int] NOT NULL,
	[CandidatePlatformConceptId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_ClusterAnalysis.HasCandidatePlatform] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_ClusterAnalysis.HasCandidatePlatform] UNIQUE NONCLUSTERED 
(
	[ClusterAnalysisId] ASC,
	[CandidatePlatformConceptId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ClusterAnalysis.HasCandidatePlatform] ADD  CONSTRAINT [DF_ClusterAnalysis.HasCandidatePlatform_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[ClusterAnalysis.HasCandidatePlatform] ADD  CONSTRAINT [DF_ClusterAnalysis.HasCandidatePlatform_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[ClusterAnalysis.HasCandidatePlatform]  WITH CHECK ADD  CONSTRAINT [FK_HasCandidatePlatform_CandidatePlatform.Concept] FOREIGN KEY([CandidatePlatformConceptId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[ClusterAnalysis.HasCandidatePlatform] CHECK CONSTRAINT [FK_HasCandidatePlatform_CandidatePlatform.Concept]
GO

ALTER TABLE [dbo].[ClusterAnalysis.HasCandidatePlatform]  WITH CHECK ADD  CONSTRAINT [FK_HasCandidatePlatform_ClusterAnalysis] FOREIGN KEY([ClusterAnalysisId])
REFERENCES [dbo].[ClusterAnalysis] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ClusterAnalysis.HasCandidatePlatform] CHECK CONSTRAINT [FK_HasCandidatePlatform_ClusterAnalysis]
GO


