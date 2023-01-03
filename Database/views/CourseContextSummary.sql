

USE [Navy_RRL_V2]
GO
/*
SELECT base.[Id]
      ,base.[RowId]
      ,base.CourseId
	  ,b.ConceptSchemeId, b.Name as ConceptScheme,base.ConceptId, b.Concept
  FROM [dbo].[Course.Concept] a 
  inner join [ConceptSchemeSummary] b on base.ConceptId = b.conceptid and b.ConceptSchemeId=17

USE [Navy_RRL_V2]
GO

SELECT [Id]
      ,[RowId]
      ,[HasCourseId]
      ,[Course]
      ,[CourseRowId]
      ,[CourseCTID]
      ,[LifeCycleControlDocumentTypeId]
      ,[LifeCycleControlDocument]
      ,[LifeCycleControlDocumentUID]
      ,[CodedNotation]
      ,[CCA]
      ,[OrganizationCTID]
      ,[HasTrainingTaskId]
      ,[TrainingTask]
      ,[TrainingTaskRowId]
      ,[TrainingTaskCTID]
      ,[CourseTypes]
      ,[AssessmentMethodTypes]
      ,[Created]
      ,[CreatedById]
      ,[LastUpdated]
      ,[LastUpdatedById]
      ,[CTID]
  FROM [dbo].[CourseContextSummary]

GO


  order by name
GO




*/

Alter VIEW CourseContextSummary
AS

SELECT  a.[Id]
		,a.[RowId]
		,a.[HasCourseId]

		,course.Name as Course
		,course.CodedNotation as CIN
		,course.[CodedNotation]
		,course.RowId as CourseRowId
		,course.CTID as CourseCTID
		,course.LifeCycleControlDocumentTypeId
		,d.Concept as LifeCycleControlDocument
		,d.ConceptUID as LifeCycleControlDocumentUID

		,o.Name		as CurriculumControlAuthority
		,o.Id		as CurriculumControlAuthorityId
		,o.RowId	as CurriculumControlAuthorityUID
		,o.CTID		as OrganizationCTID
		,a.[HasTrainingTaskId]
		,c.Description as TrainingTask
		,c.RowId as TrainingTaskRowId
		,c.CTID as TrainingTaskCTID

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

		,a.[Created]
		,a.[CreatedById]
		,a.[LastUpdated]
		,a.[LastUpdatedById]
		,a.[CTID]
  FROM [dbo].[CourseContext] a
inner join Course course on a.HasCourseId = course.Id 
inner join Organization o on course.CurriculumControlAuthorityId = o.Id
inner join TrainingTask c on a.HasTrainingTaskId = c.Id 
--LCCD
inner join [ConceptSchemeSummary] d on course.LifeCycleControlDocumentTypeId = d.conceptid

CROSS APPLY (
    --SELECT distinct d.Name + '~' + convert(varchar(50), d.RowId) + ' | '
	SELECT distinct d.Name + ' | '
    FROM dbo.[Course]  a
		Inner join [dbo].[Course.CourseType]	c on a.Id = c.CourseId
		inner join [ConceptScheme.Concept] d on c.CourseTypeConceptId = d.Id --and d.ConceptSchemeId=16
    WHERE  course.Id = a.Id
    FOR XML Path('') 
) CT (CourseTypes)
--
CROSS APPLY (
    SELECT distinct d.Name + ' | '
    FROM dbo.[Course]  a
	Inner join [dbo].[CourseContext]	cc on a.Id = cc.HasCourseId
		Inner join [dbo].[CourseContext.AssessmentType]	c on cc.Id = c.CourseContextId
		inner join [ConceptScheme.Concept] d on c.AssessmentMethodConceptId = d.Id --and d.ConceptSchemeId=13
    WHERE  course.Id = a.Id
    FOR XML Path('') 
) AMT (AssessmentMethodTypes)

  go
  grant select on CourseContextSummary to public
  go