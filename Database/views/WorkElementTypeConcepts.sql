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

SELECT [conceptid]
      ,[Concept]
      ,[CodedNotation]
      ,[Description]
      ,[WorkElementType]
      ,[IsAWorkElementType]
      ,WorkElementTypeOrder
      ,[CTID]
  FROM [dbo].[WorkElementTypeConcepts]

GO




*/
/*
View for Concepts  for WorkElementType a subset of type Reference Resource Categories

*/
Alter VIEW [dbo].[WorkElementTypeConcepts]
AS
SELECT
	--
	b.Id AS conceptid
	, b.Name AS Concept
	, b.CodedNotation
	, b.Description
	, b.WorkElementType
	, case when b.WorkElementType is not null then 1
	else 0 end as IsAWorkElementType
	--, case when b.WorkElementType is not null then WorkElementType
	--else b.Name end as WorkElementType
	, b.ListId As WorkElementTypeOrder
	, b.CTID
FROM dbo.[ConceptScheme.Concept] b 
where b.ConceptSchemeId = 9
and b.IsActive = 1
--and  b.WorkElementType is not null
GO



