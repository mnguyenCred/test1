USE [NavyRRL]
GO

/****** Object:  Table [dbo].[CourseTask.AssessmentType]    Script Date: 3/29/2022 3:00:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CourseTask.AssessmentType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CourseTaskId] [int] NOT NULL,
	[AssessmentMethodConceptId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_CourseTask.AssessmentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CourseTask.AssessmentType] ADD  CONSTRAINT [DF_CourseTask.AssessmentType_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[CourseTask.AssessmentType] ADD  CONSTRAINT [DF_CourseTask.AssessmentType_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[CourseTask.AssessmentType]  WITH CHECK ADD  CONSTRAINT [FK_CourseTask.AssessmentType_Concept] FOREIGN KEY([AssessmentMethodConceptId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[CourseTask.AssessmentType] CHECK CONSTRAINT [FK_CourseTask.AssessmentType_Concept]
GO

ALTER TABLE [dbo].[CourseTask.AssessmentType]  WITH CHECK ADD  CONSTRAINT [FK_CourseTask.AssessmentType_Course.Task] FOREIGN KEY([CourseTaskId])
REFERENCES [dbo].[Course.Task] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[CourseTask.AssessmentType] CHECK CONSTRAINT [FK_CourseTask.AssessmentType_Course.Task]
GO

