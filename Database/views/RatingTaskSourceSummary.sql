USE [NavyRRL]
GO

/****** Object:  View [dbo].[RatingTaskSourceSummary]    Script Date: 2/7/2022 10:09:55 AM ******/
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

/*
	Summary of the Sources for a Rating
	Currently uses a ROLLUP to include intermediary totals. Depending on how used by the code, may remove the ROLLUP.
	At the very least, the server side code needs to be able to differenciate the subtotals.
	- 'Source Total'
	- '--- Rating Total ---')

	Modifications
	22-02-07 mp - Seemed to have lost the updates to make the WorkElementType the primary focus, in order to show where there were zero tasks.		- may have to use a stored proc in order to display the zero tasks 
*/
Alter  VIEW [dbo].[RatingTaskSourceSummary]
AS

Select b.CodedNotation as Rating wet.Id, wet.Name as Source
,stats.FormalTrainingGap
,stats.Tasks

from [ConceptScheme.Concept]	wet 
left Join (
	SELECT 
	--	isnull(rtr.Ratings,'') as Ratings,
		rtr.RatingId,
		 isnull(wet.name,'--- Rating Total ---') As Source
		,wet.id as WorkElementTypeId
		,case when wet.Name is null and f.Name is null then ''
			when f.name is null then isnull(f.Name,'Source Total')
			else f.Name end as FormalTrainingGap
	--	, isnull(f.Name,'WET Total') As FormalTrainingGap
		, count(*) as Tasks
   
	FROM [dbo].[RatingTask] a
	left join [ConceptScheme.Concept]	wet on a.WorkElementTypeId = wet.Id
	left join [ConceptScheme.Concept]	f on a.FormalTrainingGapId = f.Id
	left join RatingTaskRatings rtr on a.Id = rtr.[RatingTaskId]

	--where rtr.Ratings = 'ALL' Or rtr.Ratings= 'QM'
	group by 
	--rtr.Ratings,
	 rtr.RatingId
	, wet.Name
	, wet.id
	, f.Name

	--with rollup
) stats on wet.Id = stats.WorkElementTypeId

left join [dbo].[Rating] b on stats.RatingId = b.Id


GO


