/****** Import summary for rating task  ******/

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

*/



SELECT 
IndexIdentifier as TaskCodedNotation
,a.RatingId
,a.[RankId]
, isnull(c1.PrefLabel,'missing') As [Rank]
,a.[LevelId]
, isnull(c2.PrefLabel,'missing') As [Level]
,a.BilletTitleId
,a.Billet_Title
,a.[RatingLevelTaskId]
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

,a.[Work_Element_Task] as RatingLevelTask
,a.TaskApplicabilityId
, isnull(e.PrefLabel,'missing') As TaskApplication
--,a.[Task_Applicability]
,a.FormalTrainingGapId
, isnull(f.PrefLabel,'missing') As FormalTrainingGap
--,a.[Formal_Training_Gap]
,a.CourseTaskId
,g.TaskStatement
,case when a.[TaskNotes] = 'N/A'  then NULL else a.[TaskNotes] end [TaskNotes]

	--     ,a.[RatingId]

   --   ,a.[BilletTitleId]  


   
  FROM [NavyRRL].[dbo].QM_RMTL_11232021 a
left join [ConceptScheme.Concept]	c1 on a.[RankId] = c1.Id
left join [ConceptScheme.Concept]	c2 on a.[LevelId] = c2.Id
left join FunctionalArea			b on a.FunctionalAreaId = b.Id
left join Source					c on a.SourceId = c.Id
left join WorkElementType			d on a.WorkElementTypeId = d.Id
left join [ConceptScheme.Concept]	e on a.TaskApplicabilityId = e.Id
left join [ConceptScheme.Concept]	f on a.FormalTrainingGapId = f.Id
Left Join [Course.Task]				g on a.CourseTaskId = g.Id