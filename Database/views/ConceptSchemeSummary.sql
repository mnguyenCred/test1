USE [NavyRRL]
GO

/****** Object:  View [dbo].[ConceptSchemeSummary]    Script Date: 12/14/2021 4:34:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/*

USE [ceNavy]
GO

SELECT [ConceptSchemeId]
      ,[Name]
      ,[conceptid]
      ,[Concept]
  FROM [dbo].[ConceptSchemeSummary]

ORDER BY Name, ListId, Concept
GO



*/
ALTER VIEW [dbo].[ConceptSchemeSummary]
AS
SELECT        TOP (100) PERCENT 
	a.Id AS ConceptSchemeId
	, a.Name
	, b.Id AS conceptid
	, b.ListId
	, b.Name AS Concept
	, b.CodedNotation
	, b.Description
FROM dbo.ConceptScheme a 
INNER JOIN dbo.[ConceptScheme.Concept] b ON a.Id = b.ConceptSchemeId
GO


