/****** Rating ******/

SELECT  [Rating], count(*) as ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [Rating]
  go

  USE [NavyRRL]
GO


UPDATE [dbo].[QM_RMTL_11232021]
   SET [RatingId] = b.Id
--	select  a.Rating, b.Name, b.CodedNotation
from [QM_RMTL_11232021] a
inner join Rating b on a.Rating = b.CodedNotation 

GO

USE [NavyRRL]
GO

SELECT [Id]
      ,[CTID]
      ,[Name]
      ,[CodedNotation]
      ,[Description]
      ,[Version]
      ,[RatingUploadDate]
      ,[RatingPublicationDate]
      ,[Image]
      ,[CredentialRegistryId]
      ,[Created]
      ,[LastUpdated]
      ,[LastPublished]
      ,[RowId]
      ,[CredentialRegistryURI]
      ,[MainEntityOfPage]
  FROM [dbo].[Rating]

GO

