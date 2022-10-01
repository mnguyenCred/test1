USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[ImportRMTL]    Script Date: 10/1/2022 6:44:03 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ImportRMTL](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IndexIdentifier] [nvarchar](50) NULL,
	[Unique_Identifier] [int] NULL,
	[Rating] [nvarchar](50) NOT NULL,
	[Rank] [nvarchar](50) NOT NULL,
	[RankLevel] [nvarchar](50) NOT NULL,
	[Billet_Title] [nvarchar](500) NOT NULL,
	[Functional_Area] [nvarchar](500) NOT NULL,
	[Source] [nvarchar](500) NOT NULL,
	[Date_of_Source] [varchar](50) NOT NULL,
	[Work_Element_Type] [nvarchar](500) NOT NULL,
	[Work_Element_Task] [nvarchar](max) NOT NULL,
	[Task_Applicability] [nvarchar](500) NOT NULL,
	[Formal_Training_Gap] [nvarchar](500) NOT NULL,
	[CIN] [nvarchar](50) NULL,
	[Course_Name] [nvarchar](500) NULL,
	[Course_Type] [nvarchar](50) NULL,
	[Curriculum_Control_Authority] [nvarchar](50) NULL,
	[Life_Cycle_Control_Document] [nvarchar](50) NULL,
	[Task_Statement] [nvarchar](max) NULL,
	[Current_Assessment_Approach] [nvarchar](50) NULL,
	[TaskNotes] [nvarchar](max) NULL,
	[Training_Solution_Type] [nvarchar](500) NULL,
	[Cluster_Analysis_Title] [nvarchar](500) NULL,
	[Recommended_Modality] [nvarchar](500) NULL,
	[Development_Specification] [nvarchar](500) NULL,
	[Candidate_Platform] [nvarchar](500) NULL,
	[CFM_Placement] [nvarchar](500) NULL,
	[Priority_Placement] [nvarchar](500) NULL,
	[Development_Ratio] [nvarchar](500) NULL,
	[Development_Time] [nvarchar](500) NULL,
	[ClusterAnalysisNotes] [nvarchar](max) NULL,
	[RatingId] [int] NULL,
	[RankId] [int] NULL,
	[LevelId] [int] NULL,
	[BilletTitleId] [int] NULL,
	[FunctionalAreaId] [int] NULL,
	[SourceId] [int] NULL,
	[WorkElementTypeId] [int] NULL,
	[RatingLevelTaskId] [int] NULL,
	[TaskApplicabilityId] [int] NULL,
	[FormalTrainingGapId] [int] NULL,
	[CourseId] [int] NULL,
	[CourseTaskId] [int] NULL,
	[Message] [varchar](1000) NULL,
	[ImportDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ImportRMTL] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ImportRMTL] ADD  CONSTRAINT [DF_ImportRMTL_ImportDate]  DEFAULT (getdate()) FOR [ImportDate]
GO


