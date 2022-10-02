USE [NavyRRL]
GO
--copy CurriculumControlAuthority to  organization
INSERT INTO [dbo].[Organization]
           (
		   [RowId],
		   [Name]
		    ,[AlternateName]
           ,[CTID]
           ,[Created]           ,[CreatedById]
           ,[LastUpdated]           ,[LastUpdatedById]
		   )

SELECT [RowId]
      ,[Name]
      ,[Abbreviation]
      ,[CTID]
      ,[Created]      ,[CreatedById]
      ,[LastUpdated]      ,[LastUpdatedById]
  FROM [dbo].[CurriculumControlAuthority]
GO




