USE [NavyRRL]
GO

/****** Object:  Table [dbo].[ClusterAnalysisTitle]    Script Date: 9/28/2022 5:53:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ClusterAnalysisTitle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CTID] [varchar](50) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_ClusterAnalysisTitle] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ClusterAnalysisTitle] ADD  CONSTRAINT [DF_ClusterAnalysisTitle_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[ClusterAnalysisTitle] ADD  CONSTRAINT [DF_ClusterAnalysisTitle_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[ClusterAnalysisTitle] ADD  CONSTRAINT [DF_ClusterAnalysisTitle_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO


--Popullate

INSERT INTO [dbo].[ClusterAnalysisTitle]
           ([Name] )

SELECT distinct [ClusterAnalysisTitle]

  FROM [dbo].[ClusterAnalysis]

  go

  --populate ClusterAnalysisTitleId

UPDATE [dbo].[ClusterAnalysis]
   SET [ClusterAnalysisTitleId] = b.Id
from [ClusterAnalysis] a
inner join [ClusterAnalysisTitle] b on a.ClusterAnalysisTitle = b.Name

GO

