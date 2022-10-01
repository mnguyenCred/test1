

/*
USE [NavyRRL]
GO

SELECT [Id]
      ,[RowId]
      ,[Name]
      ,[ReferenceTypeId]
      ,[ReferenceType]
      ,[ReferenceTypeCodedNotation]
      ,[Description]
      ,[CodedNotation]
      ,[PublicationDate]
      ,[StatusTypeId]
      ,[VersionIdentifier]
      ,[Created]
      ,[CreatedById]
      ,[CreatedBy]
      ,[LastUpdated]
      ,[LastUpdatedById]
      ,[ModifiedBy]
  FROM [dbo].[ReferenceResourceSummary]

  where ReferenceType like '%pqs%'
GO




*/
Create  VIEW [dbo].ReferenceResourceSummary
AS
SELECT 
a.[Id]
      ,a.[RowId]
      ,a.[Name]
	  --obsolete as can be multiple
      --,a.[ReferenceTypeId]
	  ,b.ReferenceTypeId
	  --WorkElementType
		,c.Name as ReferenceType
		, c.CodedNotation as ReferenceTypeCodedNotation
      ,a.[Description]
      ,a.[CodedNotation]
      ,a.[PublicationDate]
     -- ,a.[SubjectWebpage]
      ,a.[StatusTypeId]
      ,a.[VersionIdentifier]

     -- ,a.[Note]
      ,a.[Created]
      ,a.[CreatedById], d.FullName as CreatedBy
      ,a.[LastUpdated]
      ,a.[LastUpdatedById], e.FullName as ModifiedBy
    --  ,a.[CTID]
  FROM [dbo].[ReferenceResource] a
  inner join  [ReferenceResource.ReferenceType] b on a.id = b.ReferenceResourceId
  inner join [ConceptScheme.Concept] c on b.ReferenceTypeId = c.id 
  Left Join Account_Summary d on a.CreatedById = d.Id
  Left Join Account_Summary e on a.LastUpdatedById = e.Id

  --where a.name = 'cttl'
  go
  grant select on ReferenceResourceSummary to public
  go