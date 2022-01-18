USE [NavyRRL]
GO

/****** Object:  Table [dbo].[Course.Organization]    Script Date: 1/17/2022 10:50:23 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Course.Organization](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CourseId] [int] NOT NULL,
	[OrganizationId] [int] NOT NULL,
	[Created] [date] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Course.Organization] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Course.Organization] ADD  CONSTRAINT [DF_Course.Organization_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[Course.Organization] ADD  CONSTRAINT [DF_Course.Organization_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Course.Organization]  WITH CHECK ADD  CONSTRAINT [FK_Course.Organization_Course.Concept] FOREIGN KEY([CourseId])
REFERENCES [dbo].[Course.Concept] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Course.Organization] CHECK CONSTRAINT [FK_Course.Organization_Course.Concept]
GO

ALTER TABLE [dbo].[Course.Organization]  WITH CHECK ADD  CONSTRAINT [FK_Course.Organization_Organization] FOREIGN KEY([OrganizationId])
REFERENCES [dbo].[Organization] ([Id])
GO

ALTER TABLE [dbo].[Course.Organization] CHECK CONSTRAINT [FK_Course.Organization_Organization]
GO


