USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[CourseContext]    Script Date: 10/1/2022 6:37:15 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CourseContext](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CourseId] [int] NOT NULL,
	[TrainingTaskId] [int] NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_CourseContext] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CourseContext] ADD  CONSTRAINT [DF_CourseContext_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[CourseContext] ADD  CONSTRAINT [DF_CourseContext_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[CourseContext] ADD  CONSTRAINT [DF_CourseContext_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[CourseContext]  WITH CHECK ADD  CONSTRAINT [FK_CourseContext_Course] FOREIGN KEY([CourseId])
REFERENCES [dbo].[Course] ([Id])
GO

ALTER TABLE [dbo].[CourseContext] CHECK CONSTRAINT [FK_CourseContext_Course]
GO

ALTER TABLE [dbo].[CourseContext]  WITH CHECK ADD  CONSTRAINT [FK_CourseContext_TrainingTask] FOREIGN KEY([TrainingTaskId])
REFERENCES [dbo].[TrainingTask] ([Id])
GO

ALTER TABLE [dbo].[CourseContext] CHECK CONSTRAINT [FK_CourseContext_TrainingTask]
GO


