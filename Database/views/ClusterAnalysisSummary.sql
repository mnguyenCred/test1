
USE Navy_RRL_V2
GO
/****** Object:  View [dbo].[ClusterAnalysisSummary]    Script Date: 4/9/2022 11:24:13 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/*
USE [Navy_RRL_V2]
GO

SELECT [Id]
      ,[RowId]
      ,[RatingContextId]
      ,[RatingTaskId]
      ,[TrainingSolutionTypeId]
      ,[TrainingSolutionType]
      ,[ClusterAnalysisTitle]
      ,[RecommendedModalityTypeId]
      ,[RecommendedModality]
      ,[RecommentedModalityCodedNotation]
      ,[DevelopmentSpecificationTypeId]
      ,[DevelopmentSpecification]
      ,[CandidatePlatform]
      --,[CFMPlacementTypeId]
      --,[CFMPlacement]
	  ,CFMPlacementType
      ,[PriorityPlacement]
      ,[DevelopmentRatioTypeId]
      ,[DevelopmentRatio]
      ,[EstimatedInstructionalTime]
      ,[DevelopmentTime]
      ,[Notes]
      ,[Created]
      ,[CreatedById]
      ,[LastUpdated]
      ,[LastUpdatedById]
  FROM [dbo].[ClusterAnalysisSummary]

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

      ,base.RecommendedModalityTypeId
	  ,rm.Name as RecommendedModality
	  ,rm.CodedNotation as RecommentedModalityCodedNotation
	--	,base.[RecommendedModality]	 as tempRecommendedModality
      
      ,base.DevelopmentSpecificationTypeId
	  ,devSpc.Name as DevelopmentSpecification

      --,base.[CandidatePlatform]
	,CASE
		WHEN CandidatePlatformTypes IS NULL THEN ''
		WHEN len(CandidatePlatformTypes) = 0 THEN ''
		ELSE left(CandidatePlatformTypes,len(CandidatePlatformTypes)-1)
		END AS CandidatePlatform

   --   ,base.CFMPlacementTypeId
	  --,cfmSpc.Name as[CFMPlacement]
	 ,CASE
		WHEN CFMPlacementTypes IS NULL THEN ''
		WHEN len(CFMPlacementTypes) = 0 THEN ''
		ELSE left(CFMPlacementTypes,len(CFMPlacementTypes)-1)
		END AS CFMPlacementType

	  ,base.[PriorityPlacement]

      ,base.DevelopmentRatioTypeId
	  ,devRatio.Name as DevelopmentRatio

      ,base.[EstimatedInstructionalTime]
      ,base.[DevelopmentTime]

      ,base.[Notes]
--	  ,base.[CTID]
      ,base.[Created]      ,base.[CreatedById]
      ,base.[LastUpdated]      ,base.[LastUpdatedById]


  FROM [dbo].[ClusterAnalysis] base 
  inner join RatingContext rc on base.Id = rc.ClusterAnalysisId
  inner join ClusterAnalysisTitle cat on base.HasClusterAnalysisTitleId = cat.Id
Left join [ConceptScheme.Concept] tst on base.[TrainingSolutionTypeId] = tst.Id
Left join [ConceptScheme.Concept] rm on base.RecommendedModalityTypeId = rm.Id
Left join [ConceptScheme.Concept] devSpc on base.DevelopmentSpecificationTypeId = devSpc.Id
--Left join [ConceptScheme.Concept] cfmSpc on base.CFMPlacementTypeId = cfmSpc.Id
Left join [ConceptScheme.Concept] devRatio on base.DevelopmentRatioTypeId = devRatio.Id


CROSS APPLY (
    SELECT distinct d.Name + ' / '
    FROM dbo.[ClusterAnalysis]  a
		Inner join [ClusterAnalysis.HasCandidatePlatform]	c on a.Id = c.ClusterAnalysisId
		inner join [ConceptScheme.Concept] d on c.CandidatePlatformConceptId = d.Id 
    WHERE  base.Id = a.Id
    FOR XML Path('') 
) CPT (CandidatePlatformTypes)


CROSS APPLY (
    SELECT distinct d.Name + ' / '
    FROM dbo.[ClusterAnalysis]  a
		Inner join [ClusterAnalysis.CFMPlacementType]	c on a.Id = c.ClusterAnalysisId
		inner join [ConceptScheme.Concept] d on c.CFMPlacementConceptId = d.Id 
    WHERE  base.Id = a.Id
    FOR XML Path('') 
) CFMP (CFMPlacementTypes)
GO
grant select on [ClusterAnalysisSummary] to public
go


