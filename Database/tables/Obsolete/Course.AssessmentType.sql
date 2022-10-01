USE [NavyRRL]
GO

/****** Object:  Table [dbo].[Course.AssessmentType]    Script Date: 1/22/2022 4:29:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Course.AssessmentType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CourseId] [int] NOT NULL,
	[AssessmentMethodConceptId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Course.AssessmentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Course.AssessmentType] ADD  CONSTRAINT [DF_Course.AssessmentType_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[Course.AssessmentType] ADD  CONSTRAINT [DF_Course.AssessmentType_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Course.AssessmentType]  WITH CHECK ADD  CONSTRAINT [FK_Course.AssessmentType_Concept] FOREIGN KEY([AssessmentMethodConceptId])REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[Course.AssessmentType] CHECK CONSTRAINT [FK_Course.AssessmentType_Concept]
GO

ALTER TABLE [dbo].[Course.AssessmentType]  WITH CHECK ADD  CONSTRAINT [FK_Course.AssessmentType_Course] FOREIGN KEY([CourseId])
REFERENCES [dbo].[Course] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Course.AssessmentType] CHECK CONSTRAINT [FK_Course.AssessmentType_Course]
GO


