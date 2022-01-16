USE [NavyRRL]
GO

/****** Object:  View [dbo].[WorkElementTypes]    Script Date: 1/16/2022 10:32:27 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



/*

USE [NavyRRL]
GO

SELECT [ConceptSchemeId]
      ,[ConceptScheme]
      ,[conceptid]
      ,[Concept]
      ,[CodedNotation]
      ,[Description]
      ,[WorkElementType]
      ,[IsAWorkElementType]
      ,[ListId]
      ,[CTID]
  FROM [dbo].[ReferenceResourceCategoryConcepts]

GO




*/
/*
View for Concepts of type Reference Resource Categories

*/
Create VIEW [dbo].[ReferenceResourceCategoryConcepts]
AS
SELECT        TOP (100) PERCENT 
--'Reference Resource Categories' as ConceptScheme,
	a.Id AS ConceptSchemeId
	, a.Name as ConceptScheme -- not really needed?
	--
	, b.Id AS conceptid
	, b.Name AS Concept
	, b.CodedNotation
	, b.Description
	, b.WorkElementType
	, case when b.WorkElementType is not null then 1
	else 0 end as IsAWorkElementType

	, b.ListId
	, b.CTID
FROM dbo.ConceptScheme a 
INNER JOIN dbo.[ConceptScheme.Concept] b ON a.Id = b.ConceptSchemeId
where a.id = 9
GO



