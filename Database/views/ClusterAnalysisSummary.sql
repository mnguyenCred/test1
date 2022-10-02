USE [NavyRRL]
GO
USE Navy_RRL_V2
GO
/****** Object:  View [dbo].[ClusterAnalysisSummary]    Script Date: 4/9/2022 11:24:13 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/*
USE [NavyRRL]
GO

SELECT [Id]
      ,[RowId]
      ,[RatingTaskId]
      ,[TrainingSolutionTypeId]
      ,[TrainingSolutionType]
      ,[ClusterAnalysisTitle]
      ,[RecommendedModalityId]
      ,[RecommentedModality]
      ,[RecommentedModalityCodedNotation]
      ,[DevelopmentSpecificationId]
      ,[DevelopmentSpecification]
      ,[tempDevelopmentSpecification]
      ,[CandidatePlatform]
      ,[CFMPlacement]
      ,[PriorityPlacement]
      ,[DevelopmentRatio]
      ,[EstimatedInstructionalTime]
      ,[DevelopmentTime]
      ,[Notes]
      ,[Created]
      ,[CreatedById]
      ,[LastUpdated]
      ,[LastUpdatedById]
  FROM [dbo].[ClusterAnalysisSummary]
  order by TrainingSolutionType
  ,ClusterAnalysisTitle
GO




*/

Alter VIEW [dbo].[ClusterAnalysisSummary]
AS

SELECT  base.[Id]
      ,base.[RowId]
	  ,rc.Id as RatingContextId
      ,rc.[RatingTaskId]

      ,base.[TrainingSolutionTypeId]
	  ,tst.Name as [TrainingSolutionType]
    -- ,base.[TrainingSolutionType] as tempTrainingSolutionType
      
      ,cat.Name as [ClusterAnalysisTitle]

      ,base.[RecommendedModalityId]
	  ,rm.Name as RecommendedModality
	  ,rm.CodedNotation as RecommentedModalityCodedNotation
	--	,base.[RecommendedModality]	 as tempRecommendedModality
      
      ,base.[DevelopmentSpecificationId]
	  ,devSpc.Name as DevelopmentSpecification

      --,base.[CandidatePlatform]
	,CASE
		WHEN CandidatePlatformTypes IS NULL THEN ''
		WHEN len(CandidatePlatformTypes) = 0 THEN ''
		ELSE left(CandidatePlatformTypes,len(CandidatePlatformTypes)-1)
		END AS CandidatePlatform
      ,base.[CFMPlacementId]
	  ,cfmSpc.Name as[CFMPlacement]
	  ,base.[PriorityPlacement]
      ,base.[DevelopmentRatio]
      ,base.[EstimatedInstructionalTime]
      ,base.[DevelopmentTime]

      ,base.[Notes]
--	  ,base.[CTID]
      ,base.[Created]      ,base.[CreatedById]
      ,base.[LastUpdated]      ,base.[LastUpdatedById]


  FROM [dbo].[ClusterAnalysis] base 
  inner join RatingContext rc on base.Id = rc.ClusterAnalysisId
  inner join ClusterAnalysisTitle cat on base.ClusterAnalysisTitleId = cat.Id
Left join [ConceptScheme.Concept] tst on base.[TrainingSolutionTypeId] = tst.Id
Left join [ConceptScheme.Concept] rm on base.[RecommendedModalityId] = rm.Id
Left join [ConceptScheme.Concept] devSpc on base.[DevelopmentSpecificationId] = devSpc.Id
Left join [ConceptScheme.Concept] cfmSpc on base.CFMPlacementId = cfmSpc.Id

    CROSS APPLY (
    SELECT distinct d.Name + '/'
    FROM dbo.[ClusterAnalysis]  a
		Inner join [ClusterAnalysis.HasCandidatePlatform]	c on a.Id = c.ClusterAnalysisId
		inner join [ConceptScheme.Concept] d on c.CandidatePlatformConceptId = d.Id 
    WHERE  base.Id = a.Id
    FOR XML Path('') 
) CPT (CandidatePlatformTypes)

GO
grant select on [ClusterAnalysisSummary] to public
go


