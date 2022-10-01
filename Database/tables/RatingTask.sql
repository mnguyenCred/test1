USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[RatingTask]    Script Date: 10/1/2022 6:40:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RatingTask](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[HasReferenceResourceId] [int] NULL,
	[Description] [nvarchar](max) NOT NULL,
	[CTID] [varchar](50) NULL,
	[Notes] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_RatingLevelTask] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[RatingTask] ADD  CONSTRAINT [DF_RatingTask_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[RatingTask] ADD  CONSTRAINT [DF_RatingLevelTask_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[RatingTask] ADD  CONSTRAINT [DF_RatingLevelTask_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[RatingTask]  WITH CHECK ADD  CONSTRAINT [FK_RatingTask_to_ReferenceResource] FOREIGN KEY([HasReferenceResourceId])
REFERENCES [dbo].[ReferenceResource] ([Id])
GO

ALTER TABLE [dbo].[RatingTask] CHECK CONSTRAINT [FK_RatingTask_to_ReferenceResource]
GO


