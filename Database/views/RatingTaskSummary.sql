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

SELECT [Id]
      ,[CTID]
      ,[RowId]
      ,[Ratings]
      ,[BilletTitles]
      ,[CodedNotation] as TaskCodedNotation
      ,[RankId]
      ,[Rank], [RankName]
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
  FROM [dbo].[RatingTaskSummary]
    where taskApplicabilityId=77
	and isnull(ratings,'') = ''
	id in (select a.[RatingTaskId] from [RatingTask.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation = 'qm' )

	--[TaskApplicabilityId]= 77
GO



GO


*/

Alter  VIEW [dbo].RatingTaskSummary
AS

SELECT 
	a.Id,
	a.CTID,
	a.RowId,
	isnull(rtr.Ratings,'') as Ratings,
	isnull(rtb.BilletTitles,'') as BilletTitles,
	a.CodedNotation 
	,a.[RankId]
	, isnull(c1.CodedNotation,'missing') As [Rank]
	, isnull(c1.Name,'missing') As [RankName]
	,c1.RowId as PayGradeType
	,a.[LevelId]
	, isnull(c2.Name,'missing') As [Level]
	--FunctionalArea/WorkRole
	--,a.[FunctionalAreaId]
	--, isnull(b.name,'missing') As FunctionalArea
	--,b.RowId as FunctionalAreaUID
		,CASE
		WHEN WorkRoles IS NULL THEN ''
		WHEN len(WorkRoles) = 0 THEN ''
		ELSE left(WorkRoles,len(WorkRoles)-1)
		END AS FunctionalArea
	--		
	--,a.[SourceId]
	,a.ReferenceResourceId
	, isnull(c.name,'missing') As ReferenceResource
	,c.PublicationDate as SourceDate
	,c.RowId as HasReferenceResource
	--
	--	WorkElementType. Now a concept
	,a.[WorkElementTypeId]
	, isnull(wet.name,'missing') As WorkElementType
	,wet.RowId as ReferenceType


	-- RatingTask
	,a.Description as RatingTask
	--
	,a.TaskApplicabilityId
	, isnull(e.Name,'missing') As TaskApplicability
	,e.RowId as ApplicabilityType
	--
	--FormalTrainingGap
	,a.FormalTrainingGapId
	, isnull(f.Name,'missing') As FormalTrainingGap
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
		,a.TrainingTaskId
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
  left join RatingTaskRatings rtr on a.Id = rtr.[RatingTaskId]
  left Join RatingTaskBillets rtb on a.Id = rtb.RatingTaskId
left join [ConceptScheme.Concept]	c1 on a.[RankId] = c1.Id
left join [ConceptScheme.Concept]	c2 on a.[LevelId] = c2.Id
--left join WorkRole					b on a.FunctionalAreaId = b.Id
--left join FunctionalArea			b on a.FunctionalAreaId = b.Id
left join ReferenceResource			c on a.ReferenceResourceId = c.Id
--left join Source					c on a.SourceId = c.Id
left join [ConceptScheme.Concept]	wet on a.WorkElementTypeId = wet.Id
--left join WorkElementType			d on a.WorkElementTypeId = d.Id
left join [ConceptScheme.Concept]	e on a.TaskApplicabilityId = e.Id
left join [ConceptScheme.Concept]	f on a.FormalTrainingGapId = f.Id

Left Join TrainingTaskSummary		g on a.TrainingTaskId = g.TrainingTaskId
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