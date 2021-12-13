/****** RatingLevel ******/

SELECT  [RankLevel], count(*) as ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [RankLevel]
  go

  USE [NavyRRL]
GO


UPDATE [dbo].[QM_RMTL_11232021]
   SET [LevelId] = b.Id
--	select  a.[RankLevel], b.PrefLabel, CodedNotation
from [QM_RMTL_11232021] a
inner join [ConceptScheme.Concept] b on a.[RankLevel] = b.CodedNotation 
WHERE        (ConceptSchemeId = 11)
GO

SELECT        TOP (200) Id, ConceptSchemeId, PrefLabel, CTID, Description, ListId, CodedNotation, AlternateLabel, Created, LastUpdated
FROM            [ConceptScheme.Concept]
WHERE        (ConceptSchemeId = 11)
ORDER BY ConceptSchemeId, ListId, PrefLabel


