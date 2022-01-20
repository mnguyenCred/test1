Use NavyRRL
go

/*
SELECT base.[Id]
      ,base.[RowId]
      ,base.CourseId
	  ,b.ConceptSchemeId, b.Name as ConceptScheme,base.ConceptId, b.Concept
  FROM [dbo].[Course.Concept] a 
  inner join [ConceptSchemeSummary] b on base.ConceptId = b.conceptid and b.ConceptSchemeId=17

USE [NavyRRL]
GO

SELECT [Id]
      ,[RowId]
      ,[CodedNotation]
      ,[Name]
      ,[Description]
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
  FROM [dbo].[CourseSummary]

  order by name
GO




*/

Alter VIEW CourseSummary
AS

SELECT base.[Id]
	,base.[RowId]
	,base.[CodedNotation]
	,base.[Name]
	,base.[Description]
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
	--,base.[CourseType]
	--,base.[LifeCycleControlDocument]
	--,base.[CurriculumControlAuthority]
	--,base.[CurrentAssessmentApproach]
  FROM [dbo].[Course] base
  inner join Organization b on base.CurriculumControlAuthorityId = b.Id
  left join [dbo].[Course.Concept]	c on base.Id = c.courseId
	--LCCD
	inner join [ConceptSchemeSummary] d on c.ConceptId = d.conceptid and d.ConceptSchemeId=17


    CROSS APPLY (
    --SELECT distinct d.Name + '~' + convert(varchar(50), d.RowId) + ' | '
	SELECT distinct d.Name + ' | '
    FROM dbo.[Course]  a
		Inner join [dbo].[Course.Concept]	c on a.Id = c.CourseId
		inner join [ConceptScheme.Concept] d on c.ConceptId = d.Id and d.ConceptSchemeId=16
    WHERE  base.Id = a.Id
    FOR XML Path('') 
) CT (CourseTypes)
--
    CROSS APPLY (
    SELECT distinct d.Name + ' | '
    FROM dbo.[Course]  a
		Inner join [dbo].[Course.Concept]	c on a.Id = c.CourseId
		inner join [ConceptScheme.Concept] d on c.ConceptId = d.Id and d.ConceptSchemeId=13
    WHERE  base.Id = a.Id
    FOR XML Path('') 
) AMT (AssessmentMethodTypes)
  go
  grant select on CourseSummary to public
  go