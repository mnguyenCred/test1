use NavyRRL
go

/*
select count(*) from ImportHistory
--4049
select count(*) from RatingTask
--4049

select count(*) from [RatingTask.HasRating]
--2617

*/
Alter  VIEW [dbo].RatingTaskRatings
AS
SELECT [Id]
  
      ,[RatingTaskId]
      ,[RatingId]
	,		CASE
			WHEN Ratings IS NULL THEN ''
			WHEN len(Ratings) = 0 THEN ''
			ELSE left(Ratings,len(Ratings)-1)
			END AS Ratings
  FROM [dbo].[RatingTask.HasRating] base

  CROSS APPLY (
    SELECT distinct b.CodedNotation + '| '
    FROM dbo.[RatingTask.HasRating]  a
		inner join Rating b on a.RatingId = b.Id
    WHERE  base.[RatingTaskId] = a.[RatingTaskId]
    FOR XML Path('') 
) A (Ratings)

where Ratings is not null 
go

grant select on RatingTaskRatings to public
go