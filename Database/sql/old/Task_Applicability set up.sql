/****** Task_Applicability set up ******/

SELECT  [Task_Applicability], count(*) as ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [Task_Applicability]
  go

  USE [NavyRRL]
GO


UPDATE [dbo].[QM_RMTL_11232021]
   SET [TaskApplicabilityId] = b.Id
--	select  a.[Task_Applicability], b.PrefLabel, Id
from [QM_RMTL_11232021] a
inner join [ConceptScheme.Concept] b on a.[Task_Applicability] = b.PrefLabel 
WHERE        (ConceptSchemeId = 12)
GO

SELECT        TOP (200) Id, ConceptSchemeId, PrefLabel, CTID, Description, ListId, CodedNotation, AlternateLabel, Created, LastUpdated
FROM            [ConceptScheme.Concept]
WHERE        (ConceptSchemeId = 12)
ORDER BY ConceptSchemeId, ListId, PrefLabel


