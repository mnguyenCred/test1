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

USE [NavyRRL]
GO

SELECT [ConceptSchemeId]
      ,[Name]
      ,[conceptid]
      ,[ListId]
      ,[Concept]
      ,[CodedNotation],AlternateLabel
      ,[Description]
	  , IsActive
	, WorkElementType
      ,[ConceptUID]
  FROM [dbo].[ConceptSchemeSummary]
  where isactive = 1
  and ConceptSchemeId= 9 

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
	, b.AlternateLabel
	, b.Description
	, b.IsActive
	, b.WorkElementType
	, b.RowId as ConceptUID
FROM dbo.ConceptScheme a 
INNER JOIN dbo.[ConceptScheme.Concept] b ON a.Id = b.ConceptSchemeId
GO


