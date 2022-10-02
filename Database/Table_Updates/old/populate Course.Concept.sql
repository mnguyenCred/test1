USE [NavyRRL]
GO
--populate Course.Concept
--school type
INSERT INTO [dbo].[Course.Concept]
           ([CourseId]
           ,[ConceptId]
           ,[Created]       
		   )
SELECT [CourseId]
      ,[CourseSchoolId]
      ,[Created]    
  FROM [dbo].[Course.SchoolType]
GO
-- LifeCycleControlDocument
INSERT INTO [dbo].[Course.Concept]
           ([CourseId]
           ,[ConceptId]
           ,[Created]       
		   )
SELECT [Id]
      ,LifeCycleControlDocumentId
      ,[Created]    
  FROM [dbo].[Course]
GO
--=====================================
-- AssessmentApproach
--Knowledge/Performance Test
INSERT INTO [dbo].[Course.Concept]
           ([CourseId]
           ,[ConceptId]
           ,[Created]       
		   )

 select a.id,91, a.Created
 --, a.[CurrentAssessmentApproach], b.id, b.Name
	  from [Course] a

GO
--Oral Board/Test
INSERT INTO [dbo].[Course.Concept]
           ([CourseId]
           ,[ConceptId]
           ,[Created]       
		   )

 select a.id,92, a.Created
 --, a.[CurrentAssessmentApproach], b.id, b.Name
	  from [Course] a
where CurrentAssessmentApproach like '%Oral Board/Test'
GO
--===================
--
USE [NavyRRL]
GO

SELECT distinct 
[Course_Type]
      ,[Curriculum_Control_Authority]
      ,[Life_Cycle_Control_Document]

      ,[Current_Assessment_Approach]
  
  FROM [dbo].[ImportHistory]
GO



