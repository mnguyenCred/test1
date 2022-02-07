USE [NavyRRL]
GO

/****** Object:  View [dbo].[RatingTaskSourceSummary]    Script Date: 2/4/2022 2:52:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



/*
USE [NavyRRL]
GO

SELECT Ratings
,	[Source]
      ,[FormalTrainingGap]
      ,[Tasks]
  FROM [dbo].[RatingTaskSourceSummary]
where Ratings= 'QM'
OR Ratings = 'ALL' 

GO
SELECT a.Ratings
,	a.[Source]
      ,a.[FormalTrainingGap]
      ,a.[Tasks]
  FROM [dbo].[RatingTaskSourceSummary] a
 inner join Rating rtr on a.Ratings = rtr.CodedNotation
where rtr.Id = 77
--OR Ratings = 'ALL' 
and FormalTrainingGap = 'yes'
GO

SELECT a.Ratings
, b.Concept as S2 
,	a.[Source]
      ,a.[FormalTrainingGap]
      ,a.[Tasks]
FROM WorkElementTypeConcepts b
left join [dbo].[RatingTaskSourceSummary] a on b.Concept = a.Source
left join Rating rtr on a.Ratings = rtr.CodedNotation
where rtr.Id = 77

--OR Ratings = 'ALL' 
and FormalTrainingGap = 'yes'

GO

*/
/*
LEFT JOIN All sources to summary to show zero totals 

SELECT 
	b.Ratings,
	a.[Concept] As Source
	,IsNull(b.[FormalTrainingGap], 0) as Gaps
	,isnull(b.[Tasks], 0) as Task

  FROM [dbo].[WorkElementTypeConcepts] a 
  left join (
	  SELECT a.Ratings
		,a.[Source]
		,a.[FormalTrainingGap]
		,a.[Tasks]
	  FROM [dbo].[RatingTaskSourceSummary] a
	where a.Ratings = 'QM'
	) b on a.concept = b.Source
	--inner join Rating rtr on b.Ratings = rtr.CodedNotation
	--where b.Ratings = 'QM'
  order by WorkElementTypeOrder
GO

*/

Alter  VIEW [dbo].[RatingTaskSourceSummary]
AS

SELECT 
	isnull(rtr.Ratings,'') as Ratings,
	isnull(wet.name,'--- Rating Total ---') As Source

	,case when wet.Name is null and f.Name is null then ''
		when f.name is null then isnull(f.Name,'Source Total')
		else f.Name end as FormalTrainingGap
	--	, isnull(f.Name,'WET Total') As FormalTrainingGap
	, count(*) as Tasks
   
FROM [dbo].[RatingTask] a
left join RatingTaskRatings rtr on a.Id = rtr.[RatingTaskId]
left join [ConceptScheme.Concept]	wet on a.WorkElementTypeId = wet.Id
left join [ConceptScheme.Concept]	f on a.FormalTrainingGapId = f.Id

--where rtr.Ratings = 'ALL' Or rtr.Ratings= 'QM'
group by 
rtr.Ratings
--, rtr.RatingId
, wet.Name
, f.Name

with rollup

GO

grant select on [RatingTaskSourceSummary] to public
go


