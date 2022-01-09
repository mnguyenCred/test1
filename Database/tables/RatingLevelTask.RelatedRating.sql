USE [NavyRRL]
GO

/****** Object:  Table [dbo].[RatingLevelTask.RelatedRating]    Script Date: 1/8/2022 7:57:11 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RatingLevelTask.RelatedRating](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RatingLevelTaskId] [int] NOT NULL,
	[RatingId] [int] NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_RatingLevelTask.RelatedRating] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RatingLevelTask.RelatedRating] ADD  CONSTRAINT [DF_RatingLevelTask.RelatedRating_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[RatingLevelTask.RelatedRating]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask.RelatedRating_Rating] FOREIGN KEY([RatingId])
REFERENCES [dbo].[Rating] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RatingLevelTask.RelatedRating] CHECK CONSTRAINT [FK_RatingLevelTask.RelatedRating_Rating]
GO

ALTER TABLE [dbo].[RatingLevelTask.RelatedRating]  WITH CHECK ADD  CONSTRAINT [FK_RatingLevelTask.RelatedRating_RatingLevelTask] FOREIGN KEY([RatingLevelTaskId])
REFERENCES [dbo].[RatingLevelTask] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RatingLevelTask.RelatedRating] CHECK CONSTRAINT [FK_RatingLevelTask.RelatedRating_RatingLevelTask]
GO


