USE [NavyRRL]
GO

/****** Object:  StoredProcedure [dbo].[RatingTask_DeleteForRatingCode]    Script Date: 4/16/2022 3:41:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*



exec [RatingTask_DeleteForRatingCode] 'ABF'
go


*/
-- =========================================================
-- = Delete all RatingTasks for Rating code

-- =========================================================
Alter PROCEDURE [dbo].[RatingTask_DeleteForRatingCode]
	@RatingCode	varchar(15)
   	with recompile
As

DELETE a
	FROM [dbo].[RatingTask] a
	inner join [RatingTask.HasRating] b on a.Id  = b.RatingTaskId
	inner join Rating c on b.RatingId = c.Id
      WHERE c.CodedNotation = @RatingCode

GO


