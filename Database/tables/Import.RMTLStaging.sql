USE [NavyRRL]
GO

ALTER TABLE [dbo].[Import.RMTLStaging] DROP CONSTRAINT [DF_Import.RMTLStaging_Created]
GO

/****** Object:  Table [dbo].[Import.RMTLStaging]    Script Date: 4/6/2022 4:15:21 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Import.RMTLStaging]') AND type in (N'U'))
DROP TABLE [dbo].[Import.RMTLStaging]
GO

/****** Object:  Table [dbo].[Import.RMTLStaging]    Script Date: 4/6/2022 4:15:21 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Import.RMTLStaging](
	[Unique_Identifier] [varchar](50) NOT NULL,
	[Rating] [varchar](50) NOT NULL,
	[Rank] [varchar](50) NOT NULL,
	[Level_A_J_M] [varchar](50) NOT NULL,
	[Billet_Title] [varchar](max) NULL,
	[Functional_Area] [varchar](max) NULL,
	[Source] [varchar](max) NULL,
	[Date_of_Source] [varchar](50) NULL,
	[Work_Element_Type] [varchar](max) NULL,
	[Work_Element_Task] [varchar](max) NULL,
	[Task_Applicability] [varchar](max) NULL,
	[Formal_Training_Gap] [varchar](max) NULL,
	[CIN] [varchar](max) NULL,
	[Course_Name] [varchar](max) NULL,
	[Course_Type_A_C_G_F_T] [varchar](max) NULL,
	[Curriculum_Control_Authority_CCA] [varchar](max) NULL,
	[Life_Cycle_Control_Document] [varchar](max) NULL,
	[CTTL_PPP_TCCD_Statement] [varchar](max) NULL,
	[Current_Assessment_Approach] [varchar](max) NULL,
	[Part2Notes] [nvarchar](max) NULL,
	[Training_Solution_Type] [nvarchar](500) NULL,
	[Cluster_Analysis_Title] [nvarchar](500) NULL,
	[Recommended_Modality] [nvarchar](500) NULL,
	[Development_Specification] [nvarchar](500) NULL,
	[Candidate_Platform] [nvarchar](500) NULL,
	[CFM_Placement] [nvarchar](500) NULL,
	[Priority_Placement] [nvarchar](500) NULL,
	[Development_Ratio] [nvarchar](500) NULL,
	[Development_Time] [nvarchar](500) NULL,
	[EstimatedInstructionalTime] [nvarchar](500) NULL,
	[Part3Notes] [nvarchar](max) NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Import.RMTLStaging] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Import.RMTLStaging] ADD  CONSTRAINT [DF_Import.RMTLStaging_Created]  DEFAULT (getdate()) FOR [Created]
GO

