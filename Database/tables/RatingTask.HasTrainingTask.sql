USE [NavyRRL]
GO

/****** Object:  Table [dbo].[RatingTask.HasTrainingTask]    Script Date: 3/25/2022 8:08:03 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RatingTask.HasTrainingTask](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[RatingTaskId] [int] NOT NULL,
	[TrainingTaskId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_RatingTask.HasTrainingTask] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RatingTask.HasTrainingTask] ADD  CONSTRAINT [DF_RatingTask.HasTrainingTask_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[RatingTask.HasTrainingTask] ADD  CONSTRAINT [DF_RatingTask.HasTrainingTask_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[RatingTask.HasTrainingTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingTask.HasTrainingTask_TrainingTask] FOREIGN KEY([TrainingTaskId])
REFERENCES [dbo].[Course.Task] ([Id])
GO

ALTER TABLE [dbo].[RatingTask.HasTrainingTask] CHECK CONSTRAINT [FK_RatingTask.HasTrainingTask_TrainingTask]
GO

ALTER TABLE [dbo].[RatingTask.HasTrainingTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingTask.HasTrainingTask_RatingTask] FOREIGN KEY([RatingTaskId])
REFERENCES [dbo].[RatingTask] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RatingTask.HasTrainingTask] CHECK CONSTRAINT [FK_RatingTask.HasTrainingTask_RatingTask]
GO


--
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
