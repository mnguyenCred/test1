USE [NavyRRL]
GO

/****** Object:  Table [dbo].[Course.Concept]    Script Date: 1/16/2022 4:49:21 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Course.Concept](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CourseId] [int] NOT NULL,
	[ConceptId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Course.Concept] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Course.Concept] ADD  CONSTRAINT [DF_Course.Concept_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[Course.Concept] ADD  CONSTRAINT [DF_Course.Concept_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Course.Concept]  WITH CHECK ADD  CONSTRAINT [FK_Course.Concept_ConceptScheme.Concept] FOREIGN KEY([ConceptId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[Course.Concept] CHECK CONSTRAINT [FK_Course.Concept_ConceptScheme.Concept]
GO

ALTER TABLE [dbo].[Course.Concept]  WITH CHECK ADD  CONSTRAINT [FK_Course.Concept_Course] FOREIGN KEY([CourseId])
REFERENCES [dbo].[Course] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Course.Concept] CHECK CONSTRAINT [FK_Course.Concept_Course]
GO


