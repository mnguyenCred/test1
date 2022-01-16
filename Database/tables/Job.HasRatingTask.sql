USE [NavyRRL]
GO
--******** not sure if will be used as the likely course is RatingTask.HasJob

/****** Object:  Table [dbo].[Job.HasRatingTask]    Script Date: 1/14/2022 4:40:47 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Job.HasRatingTask](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[JobId] [int] NOT NULL,
	[HasRatingTaskId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Job.HasRatingTask] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Job.HasRatingTask] ADD  CONSTRAINT [DF_Job.HasRatingTask_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[Job.HasRatingTask] ADD  CONSTRAINT [DF_Job.HasRatingTask_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Job.HasRatingTask]  WITH CHECK ADD  CONSTRAINT [FK_Job.HasRatingTask_Job] FOREIGN KEY([JobId])
REFERENCES [dbo].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Job.HasRatingTask] CHECK CONSTRAINT [FK_Job.HasRatingTask_Job]
GO

ALTER TABLE [dbo].[Job.HasRatingTask]  WITH CHECK ADD  CONSTRAINT [FK_Job.HasRatingTask_Rating] FOREIGN KEY([HasRatingTaskId])
REFERENCES [dbo].[Rating] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Job.HasRatingTask] CHECK CONSTRAINT [FK_Job.HasRatingTask_Rating]
GO


