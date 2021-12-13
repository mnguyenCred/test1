/****** Duplicate WorkElementTask 
- should check by billet title
- can be used for more than one combo of rank, level, and billet title
- will be better to use version with index
*/
SELECT TOP (1000) 
     a.Work_Element_Task,dupRatingTasks.ttl as Duplicates
	 ,IndexIdentifier,Unique_Identifier
,a.[Rating]
     --a.[RankId]
      ,a.[Rank]
    --,a.[LevelId]
      ,a.RankLevel
    --,a.[Billet_TitleId]
      ,a.[Billet_Title]
   -- ,a.[FunctionalAreaId]
      ,a.[Functional_Area]
    --,a.[SourceId]
      ,a.[Source]
      ,a.Date_of_Source
    --,a.[WorkElementTypeId]
      ,a.[Work_Element_Type]
  --  ,a.[RatingLevelTaskId]
      --,a.Work_Element_Task
      ,a.[Task_Applicability]
   -- ,a.[FormalTrainingGapId]
      ,a.[Formal_Training_Gap]
   -- ,a.[CourseId]
 ,a.[CIN]
      ,a.[Course_Name]	
  ,a.[Task_Statement]  
   
      ,a.[Course_Type]
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
SELECT Work_Element_Task, Billet_Title, [Rank] as Rank
      ,count(*) ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  where Work_Element_Task <> 'N/A'
  group by Work_Element_Task, Billet_Title,[Rank] having count(*) > 1

  ) dupRatingTasks on a.Work_Element_Task = dupRatingTasks.Work_Element_Task
  
  order by a.Work_Element_Task
  ,a.[Rating]
     --a.[RankId]
      ,a.[Rank]
    --,a.[LevelId]
      ,a.RankLevel
	    ,a.[Billet_Title]
  go




