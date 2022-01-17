USE [NavyRRL]
GO
			OBSOLETE - REPLACED BY Course.Concept

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Course.SchoolType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CourseId] [int] NOT NULL,
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

ALTER TABLE [dbo].[Course.SchoolType]  WITH CHECK ADD  CONSTRAINT [FK_Course.SchoolType_Course] FOREIGN KEY([CourseId])
REFERENCES [dbo].[Course] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Course.SchoolType] CHECK CONSTRAINT [FK_Course.SchoolType_Course]
GO

ALTER TABLE [dbo].[Course.SchoolType]  WITH CHECK ADD  CONSTRAINT [FK_CourseSchoolType_ConceptScheme.SchoolType] FOREIGN KEY([CourseSchoolId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[Course.SchoolType] CHECK CONSTRAINT [FK_CourseSchoolType_ConceptScheme.SchoolType]
GO



--INSERT INTO [dbo].[Course.SchoolType]
--           (CourseId, [CourseSchoolId]
--           ,[Created]
--           --,[CreatedById]
--		   )

--SELECT a.[Id], b.Id as courseSchoolTypeId,a.[Created]
--      --
--      --,a.[CIN]
--      --,a.[Name]    ,a.[CourseType]

--      --,a.[LifeCycleControlDocumentId]
--      --,a.[AssessmentApproachId]
--      --,a.[CurriculumControlAuthorityId]
--      --,a.[LifeCycleControlDocument]
--      --,a.[CurriculumControlAuthority]
--      --,a.[CurrentAssessmentApproach]
--  FROM [dbo].[Course] a
--  inner join [ConceptScheme.Concept] b on a.CourseType = b.Name
--GO

/*
INSERT INTO [dbo].[Course.SchoolType]
           (CourseId, [CourseSchoolId]
           ,[Created]
           --,[CreatedById]
		   )

SELECT a.[Id], 1087 as courseSchoolTypeId,a.[Created]
      --
      --,a.[CIN]
      --,a.[Name]    ,a.[CourseType]

  FROM [dbo].[Course] a
  left join [Course.SchoolType] cst on a.id = cst.courseId
  --inner join [ConceptScheme.Concept] b on a.CourseType = b.Name
  where a.CourseType like 'A-School%'
  and cst.id is null
GO
*/
/*
INSERT INTO [dbo].[Course.SchoolType]
           (CourseId, [CourseSchoolId]
           ,[Created]
           --,[CreatedById]
		   )

SELECT a.[Id], 1091 as courseSchoolTypeId,a.[Created]
      --
      --,a.[CIN]
      --,a.[Name]    ,a.[CourseType]

  FROM [dbo].[Course] a
  left join [Course.SchoolType] cst on a.id = cst.courseId
  --inner join [ConceptScheme.Concept] b on a.CourseType = b.Name
  where a.CourseType like '%G-School%'
  and cst.id is null
GO

*/
/*
INSERT INTO [dbo].[Course.SchoolType]
           (CourseId, [CourseSchoolId]
           ,[Created]
           --,[CreatedById]
		   )

SELECT a.[Id], 1091 as courseSchoolTypeId,a.[Created]
      --
	  --,cst.CourseSchoolId
      --,a.[Name]    ,a.[CourseType]

  FROM [dbo].[Course] a
  left join [Course.SchoolType] cst on a.id = cst.CourseId
  --inner join [ConceptScheme.Concept] b on a.CourseType = b.Name
  where a.CourseType like '%| C-School%'
  and cst.id is null

GO

INSERT INTO [dbo].[Course.SchoolType]
           (CourseId, [CourseSchoolId]
           ,[Created]
           --,[CreatedById]
		   )

SELECT a.[Id], 1089 as courseSchoolTypeId,a.[Created]
      --
	  ,cst.CourseSchoolId
      --,a.[Name]    ,a.[CourseType]

  FROM [dbo].[Course] a
  left join [Course.SchoolType] cst on a.id = cst.CourseId
  --inner join [ConceptScheme.Concept] b on a.CourseType = b.Name
  where a.CourseType like 'F-School%'
  and cst.id is null

GO
*/