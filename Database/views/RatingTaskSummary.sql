Use Navy_RRL_V2
go

/*
USE [Navy_RRL_V2]
GO

SELECT [Id]
      ,[RowId]
      ,[RatingTask]
      ,[ReferenceResourceId]
      ,[ReferenceResource]
      ,[PublicationDate]
      ,[ReferenceTypeId]
      ,[ReferenceType]
      ,[CTID]
      ,[Created]
      ,[CreatedById]
      ,[LastUpdated]
      ,[LastUpdatedById]
  FROM [dbo].[RatingTaskSummary]

GO




*/
/*
Modifications
22-12-22 mp - new for V2. See RatingContextSummary for details

*/
Alter  VIEW [dbo].RatingTaskSummary
AS

SELECT TOP (1000) 
		rt.[Id]
      ,rt.[RowId]
	  ,rt.[Description] as RatingTask

      ,rt.[ReferenceResourceId]    
	  ,rr.Name as ReferenceResource
	  ,rr.PublicationDate
	
	  ,rt.[ReferenceTypeId]
	  ,css.Concept as ReferenceType
      ,rt.[CTID]
      ,rt.[Created]      ,rt.[CreatedById]      ,rt.[LastUpdated]      ,rt.[LastUpdatedById]

  FROM [dbo].[RatingTask] rt
  left join ReferenceResource rr on rt.ReferenceResourceId = rr.Id
 -- left join [ReferenceResource.ReferenceType] rrrt on rt.ReferenceTypeId = rrrt.id
  left join [ConceptSchemeSummary] css on rt.ReferenceTypeId = css.conceptid 
go

grant select on RatingTaskSummary to public
go