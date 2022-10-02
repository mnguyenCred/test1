USE [NavyRRL]
GO
--	Course FK updates
--trim stuff first 
UPDATE [dbo].[Course]
   SET [CourseType] = rtrim(CourseType)
      ,[LifeCycleControlDocument] = ltrim(rtrim(LifeCycleControlDocument))
      ,[CurriculumControlAuthority] =  ltrim( rtrim(CurriculumControlAuthority))
      ,[CurrentAssessmentApproach] = ltrim(rtrim(CurrentAssessmentApproach))
go

UPDATE [dbo].[Course]
   SET [LifeCycleControlDocumentId] = b.id	

--	  select a.id, a.[LifeCycleControlDocument], b.id, b.Name
	  from [Course] a
	  inner join [ConceptScheme.Concept] b on a.[LifeCycleControlDocument] = b.Name

GO
UPDATE [dbo].[Course]
   SET AssessmentApproachId = b.id	

--	  select a.id, a.[CurrentAssessmentApproach], b.id, b.Name
	  from [Course] a
	  inner join [ConceptScheme.Concept] b on a.CurrentAssessmentApproach = b.Name

GO


UPDATE [dbo].[Course]
   SET [CurriculumControlAuthorityId] = b.id

--	select a.id, a.CurriculumControlAuthority, b.Name	  
	from [Course] a
	  inner join CurriculumControlAuthority b on a.[CurriculumControlAuthority] = b.Name

GO

SELECT distinct CurriculumControlAuthority
FROM     Course
order by 1
go
SELECT distinct CurrentAssessmentApproach
FROM     Course
order by 1
go
