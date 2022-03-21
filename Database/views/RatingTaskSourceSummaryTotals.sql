USE [NavyRRL]
GO

/****** Object:  View [dbo].[RatingTaskSourceSummary]    Script Date: 3/18/2022 4:31:04 PM ******/
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

*/

Create  VIEW [dbo].[RatingTaskSourceSummaryTotals]
AS

SELECT 
	isnull(rtr.Ratings,'') as Ratings,
--	rtr.RatingId,
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
grant select on [RatingTaskSourceSummaryTotals] to public
go

