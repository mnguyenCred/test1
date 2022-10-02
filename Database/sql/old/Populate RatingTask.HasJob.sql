/****** Script for SelectTopNRows command from SSMS  ******/

--truncate table [RatingTask.HasJob]

USE [NavyRRL]
GO

INSERT INTO [dbo].[RatingTask.HasJob]
           ([RatingTaskId]
           ,[JobId]
           ,[Created]
           ,[CreatedById])


SELECT [RatingLevelTaskId], BilletTitleId, DATEADD(d,-9, GETDATE()), 1

--,[Rating]   
            
--      ,[Work_Element_Task]
      --,[Task_Applicability]
      --,[Formal_Training_Gap]
      --,[CIN]
      --,[Course_Name]
      --,[Course_Type]
      --,[Curriculum_Control_Authority]
      --,[Life_Cycle_Control_Document]
      --,[Task_Statement]
      --,[Current_Assessment_Approach]
      --,[TaskNotes]
      --,[Training_Solution_Type]
      --,[Cluster_Analysis_Title]
      --,[Recommended_Modality]
      --,[Development_Specification]
      --,[Candidate_Platform]
      --,[CFM_Placement]
      --,[Priority_Placement]
      --,[Development_Ratio]
      --,[Development_Time]
      --,[ClusterAnalysisNotes]
   
      --,[RankId]
      --,[LevelId]
      --,[BilletTitleId]
      --,[FunctionalAreaId]
      --,[SourceId]
      --,[WorkElementTypeId]

      --,[TaskApplicabilityId]
      --,[FormalTrainingGapId]
      --,[CourseId]
      --,[CourseTaskId]
      --,[Message]
  FROM [NavyRRL].[dbo].[ImportHistory]
  where [RatingLevelTaskId] is not null 
