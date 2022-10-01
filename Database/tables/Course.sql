USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[Course]    Script Date: 10/1/2022 6:37:08 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Course](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CodedNotation] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](300) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CurriculumControlAuthorityId] [int] NULL,
	[LifeCycleControlDocumentTypeId] [int] NULL,
	[LastUpdatedById] [int] NULL,
	[CTID] [varchar](50) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_Course] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Course] ADD  CONSTRAINT [DF_Course_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[Course] ADD  CONSTRAINT [DF_Course_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Course] ADD  CONSTRAINT [DF_Course_Created1]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[Course]  WITH CHECK ADD  CONSTRAINT [FK_Course_Organization] FOREIGN KEY([CurriculumControlAuthorityId])
REFERENCES [dbo].[Organization] ([Id])
GO

ALTER TABLE [dbo].[Course] CHECK CONSTRAINT [FK_Course_Organization]
GO

ALTER TABLE [dbo].[Course]  WITH CHECK ADD  CONSTRAINT [FK_CourseLCCD_LCCD.Concept] FOREIGN KEY([LifeCycleControlDocumentTypeId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[Course] CHECK CONSTRAINT [FK_CourseLCCD_LCCD.Concept]
GO


