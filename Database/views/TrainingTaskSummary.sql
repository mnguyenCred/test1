Use NavyRRL
go

/*
USE [NavyRRL]
GO

SELECT [ConceptSchemeId]
      ,[Name]
      ,[conceptid]
      ,[ListId]
      ,[Concept]
      ,[CodedNotation]
      ,[AlternateLabel]
      ,[Description]
      ,[ConceptUID]
  FROM [dbo].[ConceptSchemeSummary]
GO


USE [NavyRRL]
GO

SELECT [CourseId]
      ,[CourseUID]
      ,CodedNotation
      ,[CourseName]
      ,[TrainingTask]
	  TrainingTaskId
	  ,TrainingTaskUID
      ,[CurriculumControlAuthorityId]
      ,[CurriculumControlAuthority]
      ,[LifeCycleControlDocumentId]
      ,[LifeCycleControlDocument]
      ,[LifeCycleControlDocumentUID]
      ,[CourseTypes]
      ,[AssessmentMethodTypes]
      ,[CTID]
      ,[Created]
      ,[CreatedById]
      ,[LastUpdated]
      ,[LastUpdatedById]
  FROM [dbo].[TrainingTaskSummary]

  order by CourseName, TrainingTask
GO




*/

Alter VIEW TrainingTaskSummary
AS

SELECT base.[Id] as CourseId
	,base.[RowId] as CourseUID
	,base.CodedNotation
	,base.[Name] as CourseName
	,task.Id as TrainingTaskId
	,task.Description as TrainingTask
	,task.RowId as TrainingTaskUID
	,base.[CurriculumControlAuthorityId]
	, b.Name as CurriculumControlAuthority
	--
	,base.[LifeCycleControlDocumentId]
	,d.Concept as LifeCycleControlDocument
	,d.ConceptUID as LifeCycleControlDocumentUID
	,CASE
		WHEN CourseTypes IS NULL THEN ''
		WHEN len(CourseTypes) = 0 THEN ''
		ELSE left(CourseTypes,len(CourseTypes)-1)
		END AS CourseTypes
	,CASE
		WHEN AssessmentMethodTypes IS NULL THEN ''
		WHEN len(AssessmentMethodTypes) = 0 THEN ''
		ELSE left(AssessmentMethodTypes,len(AssessmentMethodTypes)-1)
		END AS AssessmentMethodTypes
	,base.[CTID]
	,base.[Created]	,base.[CreatedById]
	,base.[LastUpdated]	,base.[LastUpdatedById]

  FROM [dbo].[Course] base
  inner join [Course.Task] task on base.Id  = task.CourseId
  Left join Organization b on base.CurriculumControlAuthorityId = b.Id
  left join [dbo].[Course.Concept]	c on base.Id = c.courseId
	--LCCD
	Left join [ConceptSchemeSummary] d on c.ConceptId = d.conceptid and d.ConceptSchemeId=17


    CROSS APPLY (
    --SELECT distinct d.Name + '~' + convert(varchar(50), d.RowId) + ' | '
	SELECT distinct d.Name + ' | '
    FROM dbo.[Course]  a
		Inner join [dbo].[Course.CourseType]	c on a.Id = c.CourseId
		inner join [ConceptScheme.Concept] d on c.CourseTypeConceptId = d.Id 
    WHERE  base.Id = a.Id
    FOR XML Path('') 
) CT (CourseTypes)
--
    CROSS APPLY (
    SELECT distinct d.Name + ' | '
    FROM dbo.[Course]  a
		Inner join [dbo].[Course.AssessmentType]	c on a.Id = c.CourseId
		inner join [ConceptScheme.Concept] d on c.AssessmentMethodConceptId = d.Id 
    WHERE  base.Id = a.Id
    FOR XML Path('') 
) AMT (AssessmentMethodTypes)
  go
  grant select on TrainingTaskSummary to public
  go