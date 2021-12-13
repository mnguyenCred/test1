/****** Duplicate TaskStatements */
SELECT TOP (1000) 
     a.[CIN]
      ,a.[Course_Name]	
  ,'"' + a.[Task_Statement] + '"' as TaskStatement  ,dupTrainingTasks.ttl as Duplicates
   ,'"' + a.Work_Element_Task+ '"' as WorkElementTask 
    ,IndexIdentifier,Unique_Identifier
,a.[Rating]
     --a.[RankId]
      ,a.[Rank]
    --,a.[LevelId]
      ,a.[RankLevel]
    --,a.[Billet_TitleId]
      ,a.[Billet_Title]
   -- ,a.[FunctionalAreaId]
      ,a.[Functional_Area]
    --,a.[SourceId]
      ,a.[Source]
      ,a.Date_of_Source
    --,a.[WorkElementTypeId]
      ,a.Work_Element_Type
  --  ,a.[RatingLevelTaskId]
      --,a.Work_Element_Task
      ,a.[Task_Applicability]
   -- ,a.[FormalTrainingGapId]
      ,a.[Formal_Training_Gap]
   -- ,a.[CourseId]
 
      --,a.[Course_Type]
      --,a.[Curriculum_Control_Authority]
      --,a.[Life_Cycle_Control_Document]
      --,a.[CourseTaskId]
      --,a.[Task_Statement]

   -- ,a.[AssessmentId]
     -- ,a.[Current_Assessment_Approach]
      --,a.[NEC_Refresher_Training_Selection]
      --,a.[Journeyman_Core_Training_Selection]
      --,a.[Master_Core_Training_Selection]
      --,a.[Notes]
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021] a

  --where Task_Statement = 'Change and reset center area'
  --go
  inner join (
  /****** duplicate task statements - include CIN  ******/
SELECT CIN, Task_Statement
      ,count(*) ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  where Task_Statement <> 'N/A'
  group by CIN, Task_Statement having count(*) > 1
  ) dupTrainingTasks on a.CIN = dupTrainingTasks.CIN and a.Task_Statement = dupTrainingTasks.Task_Statement
  
  order by a.[Task_Statement], a.[CIN]
  ,a.[Rating]
     --a.[RankId]
      ,a.[Rank]
    --,a.[LevelId]
      ,a.[RankLevel]
	    ,a.[Billet_Title]
  go




