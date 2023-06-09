USE [Navy_RRL_V2]
GO
/****** Object:  Table [dbo].[Course.CourseType]    Script Date: 10/2/2022 9:20:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Course.CourseType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CourseId] [int] NOT NULL,
	[CourseTypeConceptId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Course.CourseType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Course.CourseType] ADD  CONSTRAINT [DF_Course.CourseType_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Course.CourseType] ADD  CONSTRAINT [DF_Course.CourseType_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Course.CourseType]  WITH CHECK ADD  CONSTRAINT [FK_CourseCourseType.Concept] FOREIGN KEY([CourseTypeConceptId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO
ALTER TABLE [dbo].[Course.CourseType] CHECK CONSTRAINT [FK_CourseCourseType.Concept]
GO
ALTER TABLE [dbo].[Course.CourseType]  WITH CHECK ADD  CONSTRAINT [FK_CourseCourseType_Course] FOREIGN KEY([CourseId])
REFERENCES [dbo].[Course] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Course.CourseType] CHECK CONSTRAINT [FK_CourseCourseType_Course]
GO
