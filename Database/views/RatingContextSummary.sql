use Navy_RRL_V2
go


/*
SELECT 
IndexIdentifier, COUNT(*) as ttl
from [QM_RMTL_11232021]
group by IndexIdentifier having count(*) > 1
go

SELECT 
Unique_Identifier, COUNT(*) as ttl
from [QM_RMTL_11232021]
group by Unique_Identifier having count(*) > 1
go

USE [NavyRRL]
GO


SELECT top 1000
[Id]
      ,[CTID]
      ,[RowId]
      ,[RatingName]
      ,[Ratings]
      ,[HasRating]
      ,[ratingId]
      ,[BilletTitles]
      ,[CodedNotation]
      ,[RankId]
      ,[Rank]
      ,[RankName]
      ,[PayGradeType]
      ,[LevelId]
      ,[Level]
      ,[FunctionalArea]
      ,[ReferenceResourceId]
      ,[ReferenceResource]
      ,[SourceDate]
      ,[HasReferenceResource]
      ,[WorkElementTypeId]
      ,[WorkElementType]
      ,[WorkElementTypeAlternateName]
      ,[WorkElementTypeOrder]
      ,[ReferenceType]
      ,[RatingTask]
      ,[TaskApplicabilityId]
      ,[TaskApplicability]
      ,[ApplicabilityType]
      ,[FormalTrainingGapId]
      ,[FormalTrainingGap]
      ,[TrainingGapType]
      ,[CourseId]
      ,[CourseUID]
      ,[CIN]
      ,[CourseName]
      ,[CourseTypes]
      ,[TrainingTaskId]
      ,[TrainingTask]
      ,[HasTrainingTask]
      ,[AssessmentMethodTypes]
      ,[CurriculumControlAuthority]
      ,[CurriculumControlAuthorityId]
      ,[CurriculumControlAuthorityUID]
      ,[LifeCycleControlDocument]
      ,[Notes]
      ,[TrainingSolutionTypeId]
      ,[TrainingSolutionType]
      ,[ClusterAnalysisTitle]
      ,[RecommendedModalityId]
      ,[RecommendedModality]
      ,[RecommentedModalityCodedNotation]
      ,[DevelopmentSpecificationId]
      ,[DevelopmentSpecification]
      ,[CandidatePlatform]
      ,[CFMPlacement]
      ,[PriorityPlacement]
      ,[DevelopmentRatio]
      ,[EstimatedInstructionalTime]
      ,[DevelopmentTime]
      ,[ClusterAnalysisNotes]
      ,[Created]
      ,[CreatedById]
      ,[CreatedBy]
      ,[CreatedByUID]
      ,[LastUpdated]
      ,[LastUpdatedById]
      ,[ModifiedBy]
      ,[ModifiedByUID]
	  ,ClusterAnalysisLastUpdated
  FROM [dbo].[RatingContextSummary]
  where 
  --CodedNotation = 'PQ42-005' AND
  ratings = 'Ps'
  order by ClusterAnalysisLastUpdated desc  

  where ( base.id in (select a.[RatingTaskId] from [RatingTask.WorkRole] a inner join WorkRole b on a.WorkRoleId = b.Id where b.Id in (30) )) 
    where FunctionalArea= ''
	order by CodedNotation
	taskApplicabilityId=77
	and isnull(ratings,'') = ''
	id in (select a.[RatingTaskId] from [RatingTask.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation = 'qm' )

	--[TaskApplicabilityId]= 77
GO

select base.*
from   [dbo].[RatingContextSummary] base
inner join (
SELECT [CourseId]
     --,[AssessmentMethodConceptId]
     
  FROM [NavyRRL].[dbo].[Course.AssessmentType]
  group by courseid having COUNT(*) > 1
) courseAsmts on base.CourseId = courseAsmts.CourseId
GO

Knowledge/Performance Test Oral Board/Test 
*/
/*

Modifications
22-03-28 mp - Added RatingTask.HasTrainingTask - as can have multiple now (really only applicable with a globally unique identifier)
			- TBD - consider changing rating to have multiple rows where in multiple ratings
				- now will show one per row
				- review the query builder for this
22-04-04 mp - as for ratings, change billet title processing to result in separate rows where more than one billet per task.
			- otherwise the export could get messed up.
22-06-03 mp - temp change to FunctionalArea to not include the Guids 

*/
Create  VIEW [dbo].RatingContextSummary
AS

SELECT 
--top 1000
	a.Id,
	a.CTID,
	a.RowId
	,isnull(r.Name,'') as RatingName
		--this will now always be single
		,isnull(r.CodedNotation,'') as Ratings
		,r.RowId as HasRating
		,r.id as ratingId
	--,isnull(rtr.Ratings,'') as Ratings
	--,''BilletTitles,
	--,isnull(rtb.BilletTitles,'') as BilletTitles
	,isnull(job.Name,'') as BilletTitles
	,a.CodedNotation 
	,a.PayGradeTypeId
	, isnull(c1.CodedNotation,'') As [Rank]
	, isnull(c1.Name,'') As [RankName]
	,c1.RowId as PayGradeType
	,case when c1.CodedNotation = 'e5' or c1.CodedNotation = 'e6' then 'Journeyman'
		 when  c1.CodedNotation = 'e7' or c1.CodedNotation = 'e8' OR c1.CodedNotation = 'e9'then 'Master'
		 else 'Apprentice' end as [Level]
	--,a.[LevelId]
	--, isnull(c2.Name,'') As [Level]
	--FunctionalArea/WorkRole
	--,a.[FunctionalAreaId]
	--, isnull(b.name,'missing') As FunctionalArea
	--,b.RowId as FunctionalAreaUID
	,CASE
		WHEN WorkRoles IS NULL THEN ''
		WHEN len(WorkRoles) = 0 THEN ''
		ELSE left(WorkRoles,len(WorkRoles)-1)
		END AS FunctionalArea
	--		SOURCE
	--,a.[SourceId]

	,c.PublicationDate as SourceDate
	,c.RowId as HasReferenceResource
	--

	-- RatingTask 
	,rt.Description as RatingTask
		,rt.ReferenceResourceId
	--ReferenceResource/source
	, isnull(c.name,'') As ReferenceResource
	--	WorkElementType. Now a concept but related to ReferenceResource
	,wet.ReferenceTypeId as [WorkElementTypeId]
	, isnull(wetc.name,'') As WorkElementType
	, isnull(wetc.WorkElementType,'') As WorkElementTypeAlternateName
	, isnull(wetc.ListId, 30) As WorkElementTypeOrder
	,wet.RowId as ReferenceType
	--
	,a.TaskApplicabilityId
	, isnull(e.Name,'') As TaskApplicability
	,e.RowId as ApplicabilityType
	--
	--FormalTrainingGap
	,a.FormalTrainingGapId
	, isnull(f.Name,'') As FormalTrainingGap
	,f.RowId as TrainingGapType
--====================== Part II ====================== 
	-- individual course parts
	/*
		,h.CodedNotation
		,h.Name as CourseName
		--can be multiple
		,h.CourseType
		,a.TrainingTaskId
		,g.Description as TrainingTask
		,g.RowId as HasTrainingTask
		--can be multiple
		,h.CurrentAssessmentApproach
		--single or multiple?
		,h.CurriculumControlAuthority
		--can be multiple
		,h.LifeCycleControlDocument
		*/
	--or used the view
		,g.CourseId, g.CourseUID
		,g.CodedNotation as CIN
		,g.CourseName
		--can be multiple
		,g.CourseTypes
		,a.TrainingTaskId
		--,a.TrainingTaskId
		,g.TrainingTask
		,g.TrainingTaskUID as HasTrainingTask
		--can be multiple
		,g.CurrentAssessmentApproach
		,g.CurrentAssessmentApproach as AssessmentMethodTypes
		--22-01-24-yes single 
		,g.CurriculumControlAuthority
		,g.CurriculumControlAuthorityId
		,g.CurriculumControlAuthorityUID
		--comfirm if can be multiple
		,g.LifeCycleControlDocument

	,a.Notes
	--====================== Part III ====================== 
	,cas.[TrainingSolutionTypeId]
	,cas.[TrainingSolutionType]
      
	,cas.[ClusterAnalysisTitle]

	,cas.[RecommendedModalityId]
	,cas.RecommendedModality RecommendedModality
	,cas.RecommentedModalityCodedNotation
      
	,cas.[DevelopmentSpecificationId]
	,cas.DevelopmentSpecification

	,cas.[CandidatePlatform]
	,cas.[CFMPlacement]
	,cas.[PriorityPlacement]
	,cas.[DevelopmentRatio]
	,cas.[EstimatedInstructionalTime]
	,cas.[DevelopmentTime]
	,cas.Notes as ClusterAnalysisNotes

	--
      ,a.[Created] --as TaskCreated,
      ,a.[CreatedById], ac.FullName as CreatedBy
	  ,ac.RowId as CreatedByUID
      ,a.[LastUpdated]--as TaskLastUpdated,
      ,a.[LastUpdatedById], am.FullName as ModifiedBy
	  ,am.RowId as ModifiedByUID
	   ,cas.[LastUpdated]as ClusterAnalysisLastUpdated
   
	FROM [dbo].[RatingContext] a
	inner join [dbo].[RatingTask] rt on a.RatingTaskId = rt.Id
	--Rating
	Left Join Rating r on a.RatingId = r.Id
	--BilletTitles
	left join Job job on a.BilletTitleId = job.Id 
	--Rank
	left join [ConceptScheme.Concept]	c1 on a.PayGradeTypeId = c1.Id
	--Level ?TBD
	--left join [ConceptScheme.Concept]	c2 on a.[LevelId] = c2.Id
--left join WorkRole					b on a.FunctionalAreaId = b.Id
--left join FunctionalArea			b on a.FunctionalAreaId = b.Id
left join ReferenceResource			c on rt.ReferenceResourceId = c.Id
--WorkElementType	
  inner join  [ReferenceResource.ReferenceType] wet on c.id = wet.ReferenceResourceId
  inner join [ConceptScheme.Concept] wetc on wet.ReferenceTypeId = wetc.id 


left join [ConceptScheme.Concept]	e on a.TaskApplicabilityId = e.Id
left join [ConceptScheme.Concept]	f on a.FormalTrainingGapId = f.Id
--TrainingTask. 22-03-28 - can now (even eventually) be multiple
Left Join [TrainingTask] htt on a.TrainingTaskId = htt.Id
Left Join TrainingTaskSummary		g on a.TrainingTaskId = g.TrainingTaskId

---=========================================
  Left Join Account_Summary ac on a.CreatedById = ac.Id
  Left Join Account_Summary am on a.LastUpdatedById = am.Id
  --
  left Join [ClusterAnalysisSummary] cas on a.Id = cas.ratingTaskId

    CROSS APPLY (
	SELECT distinct d.Name + ' |'
    FROM dbo.RatingContext  rt
		Inner join [dbo].[RatingContext.WorkRole]	rcwr on rt.Id = rcwr.RatingContextId
		inner join WorkRole d on rcwr.WorkRoleId = d.Id 
    WHERE  a.Id = rt.Id
    FOR XML Path('') 
) WR (WorkRoles)
go

grant select on RatingContextSummary to public
go