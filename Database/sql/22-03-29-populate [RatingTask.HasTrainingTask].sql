USE [NavyRRL]
GO
--populate [RatingTask.HasTrainingTask]
INSERT INTO [dbo].[RatingTask.HasTrainingTask]
           ([RatingTaskId]
           ,[TrainingTaskId]
           ,[Created]
           ,[CreatedById])
  select a.Id, a.TrainingTaskId, a.Created, a.CreatedById
  
  from RatingTask a
  left join [RatingTask.HasTrainingTask] b on a.Id = b.ratingTaskId
  where a.TrainingTaskId is not null 
  and b.TrainingTaskId is null 
  go


