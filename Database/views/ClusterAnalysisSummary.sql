USE [NavyRRL]
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
      ,base.[RatingTaskId]

      ,base.[TrainingSolutionTypeId]
	  ,tst.Name as [TrainingSolutionType]
    -- ,base.[TrainingSolutionType] as tempTrainingSolutionType
      
      ,base.[ClusterAnalysisTitle]

      ,base.[RecommendedModalityId]
	  ,rm.Name as RecommendedModality
	  ,rm.CodedNotation as RecommentedModalityCodedNotation
	--	,base.[RecommendedModality]	 as tempRecommendedModality
      
      ,base.[DevelopmentSpecificationId]
	  ,devSpc.Name as DevelopmentSpecification
	  ,base.[DevelopmentSpecification] as tempDevelopmentSpecification

      ,base.[CandidatePlatform]
      ,base.[CFMPlacement]
      ,base.[PriorityPlacement]
      ,base.[DevelopmentRatio]
      ,base.[EstimatedInstructionalTime]
      ,base.[DevelopmentTime]

      ,base.[Notes]
--	  ,base.[CTID]
      ,base.[Created]      ,base.[CreatedById]
      ,base.[LastUpdated]      ,base.[LastUpdatedById]


  FROM [dbo].[ClusterAnalysis] base 

Left join [ConceptScheme.Concept] tst on base.[TrainingSolutionTypeId] = tst.Id
Left join [ConceptScheme.Concept] rm on base.[RecommendedModalityId] = rm.Id
Left join [ConceptScheme.Concept] devSpc on base.[DevelopmentSpecificationId] = devSpc.Id


GO
grant select on [ClusterAnalysisSummary] to public
go


