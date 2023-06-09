USE [Navy_RRL_V2]
GO

ALTER TABLE [dbo].[CourseContext] DROP CONSTRAINT [FK_CourseContext_TrainingTask]
GO

ALTER TABLE [dbo].[CourseContext] DROP CONSTRAINT [FK_CourseContext_Course]
GO

ALTER TABLE [dbo].[CourseContext] DROP CONSTRAINT [DF_CourseContext_LastUpdated]
GO

ALTER TABLE [dbo].[CourseContext] DROP CONSTRAINT [DF_CourseContext_Created]
GO

ALTER TABLE [dbo].[CourseContext] DROP CONSTRAINT [DF_CourseContext_RowId]
GO

/****** Object:  Table [dbo].[CourseContext]    Script Date: 10/2/2022 12:46:52 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CourseContext]') AND type in (N'U'))
DROP TABLE [dbo].[CourseContext]
GO

/****** Object:  Table [dbo].[CourseContext]    Script Date: 10/2/2022 12:46:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CourseContext](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[HasCourseId] [int] NOT NULL,
	[HasTrainingTaskId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_CourseContext] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_CourseContext_RowId] UNIQUE NONCLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CourseContext] ADD  CONSTRAINT [DF_CourseContext_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[CourseContext] ADD  CONSTRAINT [DF_CourseContext_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[CourseContext] ADD  CONSTRAINT [DF_CourseContext_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[CourseContext]  WITH CHECK ADD  CONSTRAINT [FK_CourseContext_Course] FOREIGN KEY([HasCourseId])
REFERENCES [dbo].[Course] ([Id])
GO

ALTER TABLE [dbo].[CourseContext] CHECK CONSTRAINT [FK_CourseContext_Course]
GO

ALTER TABLE [dbo].[CourseContext]  WITH CHECK ADD  CONSTRAINT [FK_CourseContext_TrainingTask] FOREIGN KEY([HasTrainingTaskId])
REFERENCES [dbo].[TrainingTask] ([Id])
GO

ALTER TABLE [dbo].[CourseContext] CHECK CONSTRAINT [FK_CourseContext_TrainingTask]
GO


/****** Object:  Index [IX_CourseContext_CourseTrainingTask]    Script Date: 10/2/2022 12:49:53 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CourseContext_CourseTrainingTask] ON [dbo].[CourseContext]
(
	[HasCourseId] ASC,
	[HasTrainingTaskId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO



