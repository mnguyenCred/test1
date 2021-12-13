USE [NavyRRL]
GO

/****** Object:  Table [dbo].[ImportRMTL]    Script Date: 12/12/2021 9:27:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ImportRMTL](
	[IndexIdentifier] [nvarchar](50) NOT NULL,
	[Unique_Identifier] [int] NOT NULL,
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
	[CIN] [nvarchar](50) NOT NULL,
	[Course_Name] [nvarchar](500) NOT NULL,
	[Course_Type] [nvarchar](50) NOT NULL,
	[Curriculum_Control_Authority] [nvarchar](50) NOT NULL,
	[Life_Cycle_Control_Document] [nvarchar](50) NOT NULL,
	[Task_Statement] [nvarchar](max) NOT NULL,
	[Current_Assessment_Approach] [nvarchar](50) NOT NULL,
	[TaskNotes] [nvarchar](max) NOT NULL,
	[Training_Solution_Type] [nvarchar](500) NOT NULL,
	[Cluster_Analysis_Title] [nvarchar](500) NOT NULL,
	[Recommended_Modality] [nvarchar](500) NOT NULL,
	[Development_Specification] [nvarchar](500) NOT NULL,
	[Candidate_Platform] [nvarchar](500) NOT NULL,
	[CFM_Placement] [nvarchar](500) NOT NULL,
	[Priority_Placement] [nvarchar](500) NOT NULL,
	[Development_Ratio] [nvarchar](500) NOT NULL,
	[Development_Time] [nvarchar](500) NOT NULL,
	[ClusterAnalysisNotes] [nvarchar](max) NOT NULL,
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
	[CourseTaskId] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


