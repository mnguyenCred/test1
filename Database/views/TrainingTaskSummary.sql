Use Navy_RRL_V2
go

/*
USE [Navy_RRL_V2]
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

USE [Navy_RRL_V2]
GO

SELECT [CourseId]
      ,[CourseUID]
      ,[CodedNotation]
      ,[CourseName]
      ,[TrainingTaskId]
      ,[TrainingTask]
      ,[TrainingTaskUID]
      ,[CurriculumControlAuthorityId]
      ,[CurriculumControlAuthority]
      ,[CurriculumControlAuthorityUID]
      ,[LifeCycleControlDocumentTypeId]
      ,[LifeCycleControlDocument]
      ,[LifeCycleControlDocumentUID]
      ,[CourseTypes]
      ,[CurrentAssessmentApproach]
      ,[CTID]
      ,[Created]
      ,[CreatedById]
      ,[LastUpdated]
      ,[LastUpdatedById]
  FROM [dbo].[TrainingTaskSummary]
  where trainingtask = 'Plot plan of intended movement (PIM) tracks'
  or codednotation = 'A-061-0300'
  order by CourseName, TrainingTask

  Compute gyrocompass error
GO




*/
/*
Modifications
22-12-22 mp - new for V2.

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
	,base.[CurriculumControlAuthorityId]
	--, b.Name as CurriculumControlAuthority
	, case when isnull(b.alternateName,'') <> '' then b.name + ' (' + b.AlternateName + ')'
	else b.name end as CurriculumControlAuthority
	, b.RowId as CurriculumControlAuthorityUID
	--
	--,base.[LifeCycleControlDocumentId]
	,base.LifeCycleControlDocumentTypeId
	,d.Concept as LifeCycleControlDocument
	,d.ConceptUID as LifeCycleControlDocumentUID
	--
	,CASE
		WHEN CourseTypes IS NULL THEN ''
		WHEN len(CourseTypes) = 0 THEN ''
		ELSE left(CourseTypes,len(CourseTypes)-1)
		END AS CourseTypes
	--,CASE
	--	WHEN Organizations IS NULL THEN ''
	--	WHEN len(Organizations) = 0 THEN ''
	--	ELSE left(Organizations,len(Organizations)-1)
	--	END AS CurriculumControlAuthority

	,CASE
		WHEN AssessmentMethodTypes IS NULL THEN ''
		WHEN len(AssessmentMethodTypes) = 0 THEN ''
		ELSE left(AssessmentMethodTypes,len(AssessmentMethodTypes)-1)
		END AS CurrentAssessmentApproach
	,base.[CTID]
	,base.[Created]	,base.[CreatedById]
	,base.[LastUpdated]	,base.[LastUpdatedById]

FROM [dbo].[Course] base
Left join [CourseContext] cc on base.Id  = cc.HasCourseId
Left join [TrainingTask] task on cc.HasTrainingTaskId = task.Id

Left join Organization b on base.CurriculumControlAuthorityId = b.Id
--LCCD
inner join [ConceptSchemeSummary] d on base.LifeCycleControlDocumentTypeId = d.conceptid

--Left join ReferenceResource d on base.[LifeCycleControlDocumentId] = d.Id 
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
----
    CROSS APPLY (
    SELECT distinct d.Name + ' , '
    FROM dbo.[CourseContext]  a
		Inner join [CourseContext.AssessmentType]	c on a.Id = c.CourseContextId
		inner join [ConceptScheme.Concept] d on c.AssessmentMethodConceptId = d.Id 
    WHERE  base.Id = a.Id
    FOR XML Path('') 
) AMT (AssessmentMethodTypes)

  go
  grant select on TrainingTaskSummary to public
  go