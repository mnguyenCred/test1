USE [NavyRRL]
GO

/****** Object:  Table [dbo].[Job.HasRating]    Script Date: 1/8/2022 9:01:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Job.HasRating](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[JobId] [int] NOT NULL,
	[HasRatingId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Job.HasRating] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Job.HasRating] ADD  CONSTRAINT [DF_Job.HasRating_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Job.HasRating]  WITH CHECK ADD  CONSTRAINT [FK_Job.HasRating_Job] FOREIGN KEY([JobId])
REFERENCES [dbo].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Job.HasRating] CHECK CONSTRAINT [FK_Job.HasRating_Job]
GO

ALTER TABLE [dbo].[Job.HasRating]  WITH CHECK ADD  CONSTRAINT [FK_Job.HasRating_Rating] FOREIGN KEY([HasRatingId])
REFERENCES [dbo].[Rating] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Job.HasRating] CHECK CONSTRAINT [FK_Job.HasRating_Rating]
GO


