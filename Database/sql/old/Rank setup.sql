/****** Rank ******/

SELECT  [Rank], count(*) as ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [Rank]
  go

  USE [NavyRRL]
GO


UPDATE [dbo].[QM_RMTL_11232021]
   SET [RankId] = b.Id
--	select  a.Rank, b.PrefLabel, Id
from [QM_RMTL_11232021] a
inner join [ConceptScheme.Concept] b on a.[Rank] = b.CodedNotation 
WHERE        (ConceptSchemeId = 3)
GO

SELECT        TOP (200) Id, ConceptSchemeId, PrefLabel, CTID, Description, ListId, CodedNotation, AlternateLabel, Created, LastUpdated
FROM            [ConceptScheme.Concept]
WHERE        (ConceptSchemeId = 3)
ORDER BY ConceptSchemeId, ListId, PrefLabel


