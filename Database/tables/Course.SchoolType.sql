USE [NavyRRL]
GO

/****** Object:  Table [dbo].[Course.SchoolType]    Script Date: 1/16/2022 9:31:54 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Course.SchoolType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CourseSchoolId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Course.SchoolType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Course.SchoolType] ADD  CONSTRAINT [DF_Course.SchoolType_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[Course.SchoolType] ADD  CONSTRAINT [DF_Course.SchoolType_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Course.SchoolType]  WITH CHECK ADD  CONSTRAINT [FK_Table_1_ConceptScheme.SchoolType] FOREIGN KEY([CourseSchoolId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[Course.SchoolType] CHECK CONSTRAINT [FK_Table_1_ConceptScheme.SchoolType]
GO


