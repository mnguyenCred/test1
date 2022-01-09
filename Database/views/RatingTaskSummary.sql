/****** Script for SelectTopNRows command from SSMS  ******/

/*
SELECT 
IndexIdentifier, COUNT(*) as ttl
from [QM_RMTL_11232021]
group by IndexIdentifier having count(*) > 1
go

SELECT 
Unique_Identifier, COUNT(*) as ttl
from [QM_RMTL_11232021]
group by Unique_Identifier having count(*) > 1
go

USE [NavyRRL]
GO

SELECT [Id]
      ,[Identifier]
      ,[RankId]
      ,[Rank]
      ,[LevelId]
      ,[Level]
      ,[FunctionalAreaId]
      ,[FunctionalArea]
      ,[SourceId]
      ,[Source]
      ,[SourceDate]
      ,[WorkElementTypeId]
      ,[WorkElementType]
      ,[WorkElementTask]
      ,[TaskApplicabilityId]
      ,[TaskApplication]
      ,[FormalTrainingGapId]
      ,[FormalTrainingGap]
      ,[CIN]
      ,[CourseName]
      ,[CourseType]
      ,[TrainingTaskId]
      ,[TaskStatement]
	  ,CurrentAssessmentApproach
		,CurriculumControlAuthority
		,LifeCycleControlDocument
      ,[Notes]
  FROM [dbo].[RatingTaskSummary]

GO


*/

Create  VIEW [dbo].RatingTaskSummary
AS

SELECT 
a.Id,
a.CodedNotation As Identifier
,a.[RankId]
, isnull(c1.Label,'missing') As [Rank]
,a.[LevelId]
, isnull(c2.Label,'missing') As [Level]

,a.[FunctionalAreaId]
, isnull(b.name,'missing') As FunctionalArea
--,a.[Functional_Area]
,a.[SourceId]
, isnull(c.name,'missing') As Source
,c.SourceDate
--,a.[Source] as origSource
-- ,a.[Date_of_Source]
,a.[WorkElementTypeId]
, isnull(d.name,'missing') As WorkElementType
--,a.[Work_Element_Type]

,a.WorkElementTask
,a.TaskApplicabilityId
, isnull(e.Label,'missing') As TaskApplicability
--,a.[Task_Applicability]
,a.FormalTrainingGapId
, isnull(f.Label,'missing') As FormalTrainingGap
--,a.[Formal_Training_Gap]
,h.CIN
,h.Name as CourseName
,h.CourseType
,a.TrainingTaskId
,g.TaskStatement
,h.CurrentAssessmentApproach
,h.CurriculumControlAuthority
,h.LifeCycleControlDocument
,a.Notes

	--     ,a.[RatingId]

   --   ,a.[BilletTitleId]  


   
  FROM [NavyRRL].[dbo].[RatingTask] a
left join [ConceptScheme.Concept]	c1 on a.[RankId] = c1.Id
left join [ConceptScheme.Concept]	c2 on a.[LevelId] = c2.Id
left join FunctionalArea			b on a.FunctionalAreaId = b.Id
left join Source					c on a.SourceId = c.Id
left join WorkElementType			d on a.WorkElementTypeId = d.Id
left join [ConceptScheme.Concept]	e on a.TaskApplicabilityId = e.Id
left join [ConceptScheme.Concept]	f on a.FormalTrainingGapId = f.Id
Left Join [Course.Task]				g on a.TrainingTaskId = g.Id
Left Join [Course]				h on g.CourseId = h.Id

go

grant select on RatingTaskSummary to public
go