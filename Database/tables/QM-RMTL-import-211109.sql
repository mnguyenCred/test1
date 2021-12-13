USE [NavyRRL]
GO

/****** Object:  Table [dbo].[QM-RMTL-import-211109]    Script Date: 12/12/2021 10:44:38 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[QM-RMTL-import-211109](
	[Rating] [nvarchar](50) NOT NULL,
	[RankId] [int] NULL,
	[Rank] [nvarchar](50) NOT NULL,
	[LevelId] [int] NULL,
	[Level] [nvarchar](50) NOT NULL,
	[BilletTitleId] [int] NULL,
	[BilletTitle] [nvarchar](max) NOT NULL,
	[FunctionalAreaId] [int] NULL,
	[FunctionalArea] [nvarchar](max) NOT NULL,
	[SourceId] [int] NULL,
	[Source] [nvarchar](max) NOT NULL,
	[SourceDate] [nvarchar](50) NOT NULL,
	[WorkElementTypeId] [int] NULL,
	[WorkElementType] [nvarchar](50) NOT NULL,
	[RatingLevelTaskId] [int] NULL,
	[WorkElementTask] [nvarchar](max) NOT NULL,
	[Task_Applicability] [nvarchar](50) NOT NULL,
	[FormalTrainingGapId] [int] NULL,
	[Formal_Training_Gap] [nvarchar](50) NOT NULL,
	[CourseId] [int] NULL,
	[CIN] [nvarchar](50) NOT NULL,
	[Course_Name] [nvarchar](max) NOT NULL,
	[Course_Type] [nvarchar](50) NOT NULL,
	[Curriculum_Control_Authority] [nvarchar](50) NOT NULL,
	[Life_Cycle_Control_Document] [nvarchar](50) NOT NULL,
	[CourseTaskId] [int] NULL,
	[Task_Statement] [nvarchar](max) NOT NULL,
	[AssessmentId] [int] NULL,
	[Current_Assessment_Approach] [nvarchar](550) NOT NULL,
	[NEC_Refresher_Training_Selection] [nvarchar](50) NULL,
	[Journeyman_Core_Training_Selection] [nvarchar](50) NULL,
	[Master_Core_Training_Selection] [nvarchar](50) NULL,
	[Notes] [nvarchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


