USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[CourseContext.AssessmentType]    Script Date: 10/1/2022 6:37:23 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CourseContext.AssessmentType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CourseContextId] [int] NOT NULL,
	[AssessmentMethodConceptId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_CourseContext.AssessmentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CourseContext.AssessmentType] ADD  CONSTRAINT [DF_CourseContext.AssessmentType_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[CourseContext.AssessmentType] ADD  CONSTRAINT [DF_CourseContext.AssessmentType_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[CourseContext.AssessmentType]  WITH CHECK ADD  CONSTRAINT [FK_CourseContext.AssessmentType_Concept] FOREIGN KEY([AssessmentMethodConceptId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[CourseContext.AssessmentType] CHECK CONSTRAINT [FK_CourseContext.AssessmentType_Concept]
GO

ALTER TABLE [dbo].[CourseContext.AssessmentType]  WITH CHECK ADD  CONSTRAINT [FK_CourseContext.AssessmentType_CourseContext] FOREIGN KEY([CourseContextId])
REFERENCES [dbo].[CourseContext] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[CourseContext.AssessmentType] CHECK CONSTRAINT [FK_CourseContext.AssessmentType_CourseContext]
GO


