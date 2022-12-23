Use Navy_RRL_V2

exec SP_RENAME '[ClusterAnalysis].[ClusterAnalysisTitleId]', 'HasClusterAnalysisTitleId', 'COLUMN';
exec SP_RENAME '[ClusterAnalysis].[RecommendedModalityId]', 'RecommendedModalityTypeId', 'COLUMN';
exec SP_RENAME '[ClusterAnalysis].[DevelopmentSpecificationId]', 'DevelopmentSpecificationTypeId', 'COLUMN';
exec SP_RENAME '[ClusterAnalysis].[DevelopmentRatioId]', 'DevelopmentRatioTypeId', 'COLUMN';
exec SP_RENAME '[ClusterAnalysis].[CFMPlacementId]', 'CFMPlacementTypeId', 'COLUMN';
exec SP_RENAME '[ClusterAnalysis].[RatingTaskId]', 'HasRatingTaskId', 'COLUMN';
exec SP_RENAME '[ClusterAnalysis].[RatingId]', 'HasRatingId', 'COLUMN';

