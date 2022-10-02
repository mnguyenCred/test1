/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_ImportRMTL
	(
	IndexIdentifier nvarchar(50) NULL,
	Unique_Identifier int NULL,
	Rating nvarchar(50) NOT NULL,
	Rank nvarchar(50) NOT NULL,
	RankLevel nvarchar(50) NOT NULL,
	Billet_Title nvarchar(500) NOT NULL,
	Functional_Area nvarchar(500) NOT NULL,
	Source nvarchar(500) NOT NULL,
	Date_of_Source varchar(50) NOT NULL,
	Work_Element_Type nvarchar(500) NOT NULL,
	Work_Element_Task nvarchar(MAX) NOT NULL,
	Task_Applicability nvarchar(500) NOT NULL,
	Formal_Training_Gap nvarchar(500) NOT NULL,
	CIN nvarchar(50) NULL,
	Course_Name nvarchar(500) NULL,
	Course_Type nvarchar(50) NULL,
	Curriculum_Control_Authority nvarchar(50) NULL,
	Life_Cycle_Control_Document nvarchar(50) NULL,
	Task_Statement nvarchar(MAX) NULL,
	Current_Assessment_Approach nvarchar(50) NULL,
	TaskNotes nvarchar(MAX) NULL,
	Training_Solution_Type nvarchar(500) NULL,
	Cluster_Analysis_Title nvarchar(500) NULL,
	Recommended_Modality nvarchar(500) NULL,
	Development_Specification nvarchar(500) NULL,
	Candidate_Platform nvarchar(500) NULL,
	CFM_Placement nvarchar(500) NULL,
	Priority_Placement nvarchar(500) NULL,
	Development_Ratio nvarchar(500) NULL,
	Development_Time nvarchar(500) NULL,
	ClusterAnalysisNotes nvarchar(MAX) NULL,
	RatingId int NULL,
	RankId int NULL,
	LevelId int NULL,
	BilletTitleId int NULL,
	FunctionalAreaId int NULL,
	SourceId int NULL,
	WorkElementTypeId int NULL,
	RatingLevelTaskId int NULL,
	TaskApplicabilityId int NULL,
	FormalTrainingGapId int NULL,
	CourseId int NULL,
	CourseTaskId int NULL,
	Message varchar(1000) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_ImportRMTL SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM dbo.ImportRMTL)
	 EXEC('INSERT INTO dbo.Tmp_ImportRMTL (IndexIdentifier, Unique_Identifier, Rating, Rank, RankLevel, Billet_Title, Functional_Area, Source, Date_of_Source, Work_Element_Type, Work_Element_Task, Task_Applicability, Formal_Training_Gap, CIN, Course_Name, Course_Type, Curriculum_Control_Authority, Life_Cycle_Control_Document, Task_Statement, Current_Assessment_Approach, TaskNotes, Training_Solution_Type, Cluster_Analysis_Title, Recommended_Modality, Development_Specification, Candidate_Platform, CFM_Placement, Priority_Placement, Development_Ratio, Development_Time, ClusterAnalysisNotes, RatingId, RankId, LevelId, BilletTitleId, FunctionalAreaId, SourceId, WorkElementTypeId, RatingLevelTaskId, TaskApplicabilityId, FormalTrainingGapId, CourseId, CourseTaskId, Message)
		SELECT IndexIdentifier, Unique_Identifier, Rating, Rank, RankLevel, Billet_Title, Functional_Area, Source, Date_of_Source, Work_Element_Type, Work_Element_Task, Task_Applicability, Formal_Training_Gap, CIN, Course_Name, Course_Type, Curriculum_Control_Authority, Life_Cycle_Control_Document, Task_Statement, Current_Assessment_Approach, TaskNotes, Training_Solution_Type, Cluster_Analysis_Title, Recommended_Modality, Development_Specification, Candidate_Platform, CFM_Placement, Priority_Placement, Development_Ratio, Development_Time, ClusterAnalysisNotes, RatingId, RankId, LevelId, BilletTitleId, FunctionalAreaId, SourceId, WorkElementTypeId, RatingLevelTaskId, TaskApplicabilityId, FormalTrainingGapId, CourseId, CourseTaskId, Message FROM dbo.ImportRMTL WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.ImportRMTL
GO
EXECUTE sp_rename N'dbo.Tmp_ImportRMTL', N'ImportRMTL', 'OBJECT' 
GO
COMMIT
