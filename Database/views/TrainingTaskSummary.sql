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
      ,[CodedNotation]
      ,[CourseName]
      ,[TrainingTaskId]
      ,[TrainingTask]
      ,[TrainingTaskUID]
      ,[LifeCycleControlDocumentId]
      ,[LifeCycleControlDocument]
      ,[LifeCycleControlDocumentUID]
      ,[CourseTypes]
      ,[CurriculumControlAuthority]
      ,[CurriculumControlAuthorityId]
      ,[AssessmentMethodTypes]
      ,[CTID]
      ,[Created]
      ,[CreatedById]
      ,[LastUpdated]
      ,[LastUpdatedById]
  FROM [dbo].[TrainingTaskSummary]
  where trainingtask = 'Plot plan of intended movement (PIM) tracks'
  or codednotation = 'A-061-0070'
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
	-- now multiple, leaves as is until duplicate rows?
	--,base.[CurriculumControlAuthorityId]
	--, b.Name as CurriculumControlAuthority
	--
	,base.[LifeCycleControlDocumentId]
	,d.Name as LifeCycleControlDocument
	,d.RowId as LifeCycleControlDocumentUID
	--
	,CASE
		WHEN CourseTypes IS NULL THEN ''
		WHEN len(CourseTypes) = 0 THEN ''
		ELSE left(CourseTypes,len(CourseTypes)-1)
		END AS CourseTypes
	,CASE
		WHEN Organizations IS NULL THEN ''
		WHEN len(Organizations) = 0 THEN ''
		ELSE left(Organizations,len(Organizations)-1)
		END AS CurriculumControlAuthority
	,0 as [CurriculumControlAuthorityId]
	,CASE
		WHEN AssessmentMethodTypes IS NULL THEN ''
		WHEN len(AssessmentMethodTypes) = 0 THEN ''
		ELSE left(AssessmentMethodTypes,len(AssessmentMethodTypes)-1)
		END AS AssessmentMethodTypes
	,base.[CTID]
	,base.[Created]	,base.[CreatedById]
	,base.[LastUpdated]	,base.[LastUpdatedById]

  FROM [dbo].[Course] base
  Left join [Course.Task] task on base.Id  = task.CourseId

  --Left join [Course.Organization] e on base.Id = e.CourseId 
  --Left join Organization b on e.OrganizationId = b.Id
  --left join [dbo].[Course.Concept]	c on base.Id = c.courseId
	--LCCD
	Left join ReferenceResource d on base.[LifeCycleControlDocumentId] = d.Id 
	--


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
-- orgs
    CROSS APPLY (
    SELECT distinct d.Name + ' | '
    FROM dbo.[Course]  a
		Inner join [dbo].[Course.Organization]	c on a.Id = c.CourseId
		inner join Organization d on c.OrganizationId= d.Id 
    WHERE  base.Id = a.Id
    FOR XML Path('') 
) ORG (Organizations)
  go
  grant select on TrainingTaskSummary to public
  go