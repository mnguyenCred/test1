USE [NavyRRL]
GO

/*
select count(*) from ImportHistory
--4049
select count(*) from RatingTask
--4049

select count(*) from [RatingTask.HasRating]
--2617



SELECT top 1000
	a.[Id]
      ,a.[RatingTaskId]
      ,a.[JobId]
      ,a.[BilletTitles]
  FROM [dbo].[RatingTaskBillets] a
  inner join ratingTask b on a.RatingTaskId = b.id 
  where a.jobid > 108


GO


*/
Alter  VIEW [dbo].RatingTaskBillets
AS
SELECT [Id]
  
      ,[RatingTaskId]
      ,JobId
	,		CASE
			WHEN BilletTitles IS NULL THEN ''
			WHEN len(BilletTitles) = 0 THEN ''
			ELSE left(BilletTitles,len(BilletTitles)-1)
			END AS BilletTitles
  FROM [dbo].[RatingTask.HasJob] base

  CROSS APPLY (
    SELECT 
		distinct b.Name + '| '
		--distinct b.Name + '~' + convert(varchar,b.id) + '| '
    FROM dbo.[RatingTask.HasJob]  a
		inner join Job b on a.JobId = b.Id
    WHERE  base.[RatingTaskId] = a.[RatingTaskId]
    FOR XML Path('') 
) A (BilletTitles)

where BilletTitles is not null 
go

grant select on RatingTaskBillets to public
go