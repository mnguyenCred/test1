Use NavyRRL
go

USE [Navy_RRL_V2]
GO
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
      ,[LifeCycleControlDocumentTypeId]
      ,[LifeCycleControlDocument]
      ,[LifeCycleControlDocumentUID]
      ,[CourseTypes]
      ,[AssessmentMethodTypes]
      ,[CTID]
      ,[Created]
      ,[CreatedById]
      ,[LastUpdated]
      ,[LastUpdatedById]
  FROM [dbo].[CourseContextSummary]

  order by name
GO




*/

Create VIEW CourseContextSummary
AS

SELECT base.[Id]
	,base.[RowId]
	,base.[CodedNotation]
	,base.[Name]
	,base.[Description]
	,base.[CurriculumControlAuthorityId]
	, b.Name as CurriculumControlAuthority
	--
	--,base.[LifeCycleControlDocumentId]
	,base.LifeCycleControlDocumentTypeId
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
	--LCCD
	inner join [ConceptSchemeSummary] d on base.LifeCycleControlDocumentTypeId = d.conceptid


    CROSS APPLY (
    --SELECT distinct d.Name + '~' + convert(varchar(50), d.RowId) + ' | '
	SELECT distinct d.Name + ' | '
    FROM dbo.[Course]  a
		Inner join [dbo].[Course.CourseType]	c on a.Id = c.CourseId
		inner join [ConceptScheme.Concept] d on c.CourseTypeConceptId = d.Id --and d.ConceptSchemeId=16
    WHERE  base.Id = a.Id
    FOR XML Path('') 
) CT (CourseTypes)
--
    CROSS APPLY (
    SELECT distinct d.Name + ' | '
    FROM dbo.[Course]  a
	Inner join [dbo].[CourseContext]	cc on a.Id = cc.CourseId
		Inner join [dbo].[CourseContext.AssessmentType]	c on cc.Id = c.CourseContextId
		inner join [ConceptScheme.Concept] d on c.AssessmentMethodConceptId = d.Id --and d.ConceptSchemeId=13
    WHERE  base.Id = a.Id
    FOR XML Path('') 
) AMT (AssessmentMethodTypes)
  go
  grant select on CourseContextSummary to public
  go