USE [Navy_RRL_V2]
GO
/*



SELECT [Id]
      ,[Name]
      ,[HasRatingTasks]
      ,[CodedNotation]
      ,[CTID]
      ,[Description]
      ,[MainEntityOfPage]
  FROM [dbo].[RatingSummary]
  where HasRatingTasks> 0

GO

USE [NavyRRL]
GO


*/
Alter View RatingSummary
as
SELECT  a.[Id]
	,a.[Name]
	,IsNull(hasRatingTasks.total, 0) as HasRatingTasks
	,a.[CodedNotation]
	,a.[CTID]
	,a.RowId
	,a.[Description]
	,a.[MainEntityOfPage]
	,a.Version

  FROM [dbo].[Rating] a 
  left join ( 
	Select b.RatingId, count(*) as total from RatingContext b
	group by b.RatingId
  ) hasRatingTasks on a.Id = hasRatingTasks.RatingId
 go
 grant select on RatingSummary to public
 go
