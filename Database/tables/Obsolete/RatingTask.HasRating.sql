USE [NavyRRL]
GO

/****** Object:  Table [dbo].[RatingTask.HasRating]    Script Date: 1/8/2022 9:38:07 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RatingTask.HasRating](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RatingTaskId] [int] NOT NULL,
	[RatingId] [int] NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_RatingLevelTask.RelatedRating] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RatingTask.HasRating] ADD  CONSTRAINT [DF_RatingLevelTask.RelatedRating_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[RatingTask.HasRating]  WITH CHECK ADD  CONSTRAINT [FK_RatingTask.HasRating_Rating] FOREIGN KEY([RatingId])
REFERENCES [dbo].[Rating] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RatingTask.HasRating] CHECK CONSTRAINT [FK_RatingTask.HasRating_Rating]
GO

ALTER TABLE [dbo].[RatingTask.HasRating]  WITH CHECK ADD  CONSTRAINT [FK_RatingTask.HasRatingTask_Task] FOREIGN KEY([RatingTaskId])
REFERENCES [dbo].[RatingTask] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RatingTask.HasRating] CHECK CONSTRAINT [FK_RatingTask.HasRatingTask_Task]
GO

