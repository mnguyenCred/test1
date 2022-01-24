USE [NavyRRL]
GO

INSERT INTO [dbo].[ImportHistory]
           (
		   [ImportDate]
           ,[IndexIdentifier]
           ,[Unique_Identifier]

           ,[Rating] ,[RatingId]              
           ,[Rank]  ,[RankId]
           ,[RankLevel],[LevelId]
           ,[Billet_Title]
           ,[BilletTitleId]
           ,[Functional_Area]       ,[FunctionalAreaId]
           ,[Source]  ,[SourceId]
           ,[Date_of_Source]
           ,[Work_Element_Type]    ,[WorkElementTypeId]
           ,[Work_Element_Task]  ,[RatingLevelTaskId]
           ,[Task_Applicability]  ,[TaskApplicabilityId]
           ,[Formal_Training_Gap]   ,[FormalTrainingGapId]
		    ,[CourseId]
           ,[CIN]
           ,[Course_Name]
           ,[Course_Type]
           ,[Curriculum_Control_Authority]
           ,[Life_Cycle_Control_Document]
           ,[Task_Statement]  ,[CourseTaskId]
           ,[Current_Assessment_Approach]
           ,[TaskNotes]

		   )

SELECT getdate()
    
	    ,[CodedNotation]
		,'' [Unique_Identifier]
      ,[Ratings], 0
	   ,[Rank]  ,[RankId]
	         ,[Level]     ,[LevelId]
      ,[BilletTitles], 0

      ,[FunctionalArea]      ,[FunctionalAreaId]
      
      ,[ReferenceResource],[ReferenceResourceId]
      ,[SourceDate]

      ,[WorkElementType]      ,[WorkElementTypeId]

      ,[RatingTask], Id
   
      ,[TaskApplicability]   ,[TaskApplicabilityId]
 
   
      ,[FormalTrainingGap]   ,[FormalTrainingGapId]

      ,[CourseId]
    
      ,[CIN]
      ,[CourseName]
      ,[CourseTypes]

      ,[TrainingTask]      ,[TrainingTaskId]
 
      ,[AssessmentMethodTypes]
      ,[CurriculumControlAuthority]
      ,[LifeCycleControlDocument]
      ,[Notes]

  FROM [dbo].[RatingTaskSummary]

GO


GO


