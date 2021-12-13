/****** Formal_Training_Gap set up ******/

SELECT  [Formal_Training_Gap], count(*) as ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [Formal_Training_Gap]
  go

  USE [NavyRRL]
GO


UPDATE [dbo].[QM_RMTL_11232021]
   SET [FormalTrainingGapId] = b.Id
--	select  a.[Formal_Training_Gap], b.PrefLabel,  b.CodedNotation , Id
from [QM_RMTL_11232021] a
inner join [ConceptScheme.Concept] b on a.[Formal_Training_Gap] = b.PrefLabel 
WHERE        (ConceptSchemeId = 10)
GO

SELECT        TOP (200) Id, ConceptSchemeId, PrefLabel, CTID, Description, ListId, CodedNotation, AlternateLabel, Created, LastUpdated
FROM            [ConceptScheme.Concept]
WHERE        (ConceptSchemeId = 10)
ORDER BY ConceptSchemeId, ListId, PrefLabel


