use NavyRRL
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
use Navy_RRL_220405
go

SELECT top 1000
	[Id]
      ,[CTID]
  --    ,[RowId]
      ,[Ratings], RatingName
      ,[BilletTitles]
      ,[CodedNotation] as TaskCodedNotation
    --  ,[RankId]
      ,[Rank], [RankName]
    --  ,[PayGradeType]
    --  ,[LevelId]
      ,[Level]
      ,[FunctionalArea]
     -- ,[ReferenceResourceId]
      ,[ReferenceResource]
      ,[SourceDate]
     -- ,[HasReferenceResource]
      ,[WorkElementTypeId]
      ,[WorkElementType], WorkElementTypeAlternateName
     -- ,[ReferenceType]
      ,[RatingTask]
      ,[TaskApplicabilityId]
      ,[TaskApplicability]
    --  ,[ApplicabilityType]
      ,[FormalTrainingGapId]
      ,[FormalTrainingGap]
    --  ,[TrainingGapType]
      ,[CourseId]
    --  ,[CourseUID]
      ,[CIN]
      ,[CourseName]
      ,[CourseTypes]
      ,[TrainingTaskId]
      ,[TrainingTask]
      ,[HasTrainingTask]
      ,[AssessmentMethodTypes]
      ,[CurriculumControlAuthority]
      ,[LifeCycleControlDocument]
      ,[Notes]
      ,[Created]
      ,[CreatedById]
      ,[CreatedBy]
     -- ,[CreatedByUID]
      ,[LastUpdated]
      ,[LastUpdatedById]
      ,[ModifiedBy]
     -- ,[ModifiedByUID]
  FROM [dbo].[RatingTaskSummary] base
  where CodedNotation = 'PQ42-005'
  and ratings = 'STG'
  order by id 

  where ( base.id in (select a.[RatingTaskId] from [RatingTask.WorkRole] a inner join WorkRole b on a.WorkRoleId = b.Id where b.Id in (30) )) 
    where FunctionalArea= ''
	order by CodedNotation
	taskApplicabilityId=77
	and isnull(ratings,'') = ''
	id in (select a.[RatingTaskId] from [RatingTask.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation = 'qm' )

	--[TaskApplicabilityId]= 77
GO

select base.*
from   [dbo].[RatingTaskSummary] base
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
*/
Alter  VIEW [dbo].RatingTaskSummary
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
	,a.[RankId]
	, isnull(c1.CodedNotation,'') As [Rank]
	, isnull(c1.Name,'') As [RankName]
	,c1.RowId as PayGradeType
	,a.[LevelId]
	, isnull(c2.Name,'') As [Level]
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
	,a.ReferenceResourceId
	, isnull(c.name,'') As ReferenceResource
	,c.PublicationDate as SourceDate
	,c.RowId as HasReferenceResource
	--
	--	WorkElementType. Now a concept
	,a.[WorkElementTypeId]
	, isnull(wet.name,'') As WorkElementType
	, isnull(wet.WorkElementType,'') As WorkElementTypeAlternateName
	, isnull(wet.ListId, 30) As WorkElementTypeOrder
	,wet.RowId as ReferenceType


	-- RatingTask 
	,a.Description as RatingTask
	--
	,a.TaskApplicabilityId
	, isnull(e.Name,'') As TaskApplicability
	,e.RowId as ApplicabilityType
	--
	--FormalTrainingGap
	,a.FormalTrainingGapId
	, isnull(f.Name,'') As FormalTrainingGap
	,f.RowId as TrainingGapType
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
		,htt.TrainingTaskId
		--,a.TrainingTaskId
		,g.TrainingTask
		,g.TrainingTaskUID as HasTrainingTask
		--can be multiple
		,g.AssessmentMethodTypes
		--22-01-24-yes single 
		,g.CurriculumControlAuthority
		,g.CurriculumControlAuthorityId
		,g.CurriculumControlAuthorityUID
		--comfirm if can be multiple
		,g.LifeCycleControlDocument

	,a.Notes

      ,a.[Created] --as TaskCreated,
      ,a.[CreatedById], ac.FullName as CreatedBy
	  ,ac.RowId as CreatedByUID
      ,a.[LastUpdated]--as TaskLastUpdated,
      ,a.[LastUpdatedById], am.FullName as ModifiedBy
	  ,am.RowId as ModifiedByUID

   
	FROM [dbo].[RatingTask] a
	--Rating
	left join [RatingTask.HasRating] rtr on a.Id = rtr.[RatingTaskId]
		Left Join Rating r on rtr.RatingId = r.Id
	--BilletTitles
	--left join RatingTaskRatings rtr on a.Id = rtr.[RatingTaskId]
	--22-04-04 - allow multiple rows
	--left Join RatingTaskBillets rtb1 on a.Id = rtb1.RatingTaskId
	left join dbo.[RatingTask.HasJob] rtb on a.id = rtb.RatingTaskId
		left join Job job on rtb.JobId = job.Id 
	--Rank
	left join [ConceptScheme.Concept]	c1 on a.[RankId] = c1.Id
	--Level
	left join [ConceptScheme.Concept]	c2 on a.[LevelId] = c2.Id
--left join WorkRole					b on a.FunctionalAreaId = b.Id
--left join FunctionalArea			b on a.FunctionalAreaId = b.Id
left join ReferenceResource			c on a.ReferenceResourceId = c.Id
--WorkElementType	
left join [ConceptScheme.Concept]	wet on a.WorkElementTypeId = wet.Id
left join [ConceptScheme.Concept]	e on a.TaskApplicabilityId = e.Id
left join [ConceptScheme.Concept]	f on a.FormalTrainingGapId = f.Id
--TrainingTask. 22-03-28 - can now (even eventually) be multiple
Left Join [ratingTask.HasTrainingTask] htt on a.Id = htt.RatingTaskId
Left Join TrainingTaskSummary		g on htt.TrainingTaskId = g.TrainingTaskId
--Left Join TrainingTaskSummary		g on a.TrainingTaskId = g.TrainingTaskId

--Left Join [Course.Task]				g on a.TrainingTaskId = g.Id
--Left Join [Course]				h on g.CourseId = h.Id
  Left Join Account_Summary ac on a.CreatedById = ac.Id
  Left Join Account_Summary am on a.LastUpdatedById = am.Id
  --
  
    CROSS APPLY (
	SELECT distinct d.Name + '~' + convert(varchar(50),d.RowId) + ' | '
    FROM dbo.RatingTask  rt
		Inner join [dbo].[RatingTask.WorkRole]	rtw on rt.Id = rtw.RatingTaskId
		inner join WorkRole d on rtw.WorkRoleId = d.Id 
    WHERE  a.Id = rt.Id
    FOR XML Path('') 
) WR (WorkRoles)
go

grant select on RatingTaskSummary to public
go