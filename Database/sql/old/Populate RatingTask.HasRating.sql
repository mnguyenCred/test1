USE [NavyRRL]
GO

SELECT [Rating]
	,[RatingId]
	,[Rank]
	,[RankId]
	,[RankLevel]
	,[Billet_Title]
	,[Functional_Area]
	,[Source]
	,[SourceId]
	,[RatingLevelTaskId]
	,[Work_Element_Task]

  FROM [dbo].[ImportRMTL_BK220108]

GO


INSERT INTO [dbo].[RatingTask.HasRating]
           ([RowId]
           ,[RatingTaskId]
           ,[RatingId]
           ,[Created])
SELECT NewId() as rowId    ,[RatingLevelTaskId] ,[RatingId], getdate()
--,[Rating]
	   
--      ,[Rank]
--	      ,[RankId]
--      ,[RankLevel]
--      ,[Billet_Title]
--      ,[Functional_Area]
--      ,[Source]
--	     ,[SourceId]
	
--      ,[Work_Element_Task]

  FROM [dbo].[ImportRMTL_BK220108]

  go
  INSERT INTO [dbo].[RatingTask.HasRating]
           ([RowId]
           ,[RatingTaskId]
           ,[RatingId]
           ,[Created])
SELECT NewId() as rowId    ,a.[RatingLevelTaskId] ,a.[RatingId], getdate()
--,a.[Rating]
	   
--      ,a.[Rank]
--	      ,a.[RankId]
--      ,a.[RankLevel]
--      ,a.[Billet_Title]
--      ,a.[Functional_Area]
--      ,a.[Source]
--	     ,a.[SourceId]
	
--      ,a.[Work_Element_Task]

  FROM [dbo].ImportRMTL a 
  left join [RatingTask.HasRating] b on a.RatingId  = b.ratingId and a.RatingLevelTaskId = b.ratingTaskId
  where a.Rating = 'ABF'
  and b.id is null 

  go

SELECT top 1000
a.[Id]
      ,a.[RowId]
      ,a.[RatingTaskId], c.Description as RatingTask
      ,a.[RatingId], b.Name As Rating
      ,a.[Created]
  FROM [dbo].[RatingTask.HasRating] a 
  inner join Rating b on a.ratingId = b.Id
  inner join RatingTask c on a.[RatingTaskId] = c.Id 

GO

