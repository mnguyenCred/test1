/****** Billet_Title set up ******/

SELECT  Billet_Title, count(*) as ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by Billet_Title
  order by 1
  go

USE [NavyRRL]
GO

INSERT INTO [dbo].[Job]
           (
		  
      [Name]
	  ,[CTID]
           --,[CodedNotation]
           --,[JobVersion]
           --,[Description]
           --,[ShortName30]
           --,[ShortName14]
  
           --,[Created]
           --,[LastUpdated]

		   )
SELECT distinct  a.Billet_Title, '' as CTID

  FROM [NavyRRL].[dbo].[QM_RMTL_11232021] a
  left join Job b on a.Billet_Title = b.name
  where b.id is null 
  order by 1
  go

   Update dbo.[Job]
set CTID = 'ce-' + Lower(NewId())
where Isnull(CTID,'') = ''
go


UPDATE [dbo].[QM_RMTL_11232021]
   SET [BilletTitleId] = b.Id
--	select  a.Billet_Title, b.Name, b.Id
from [QM_RMTL_11232021] a
inner join Job b on a.Billet_Title = b.Name 

GO

USE [NavyRRL]
GO

SELECT TOP (1000) [Id]
      ,[CTID]
      ,[Name]
      ,[CodedNotation]
      ,[JobVersion]
      ,[Description]
      ,[ShortName30]
      ,[ShortName14]
      ,[CredentialRegistryId]
      ,[Created]
      ,[LastUpdated]
      ,[LastPublished]
      ,[RowId]
  FROM [NavyRRL].[dbo].[Job]
GO



SELECT distinct  a.Billet_Title, '' as CTID

  FROM [NavyRRL].[dbo].[QM_RMTL_11232021] a
  left join Job b on a.Billet_Title = b.name
  where b.id is null 
  order by 1
  go


-- =============================================
USE [NavyRRL]
GO

INSERT INTO [dbo].[RmtlProject.Billet]
           ( 
		   RmtlProjectId
		   , [JobId]
           --,[Created]
           --,[LastUpdated]
		   )
SELECT distinct b.Id, c.Id
--,a.Billet_Title, b.Name

  FROM [NavyRRL].[dbo].[QM_RMTL_11232021] a
  inner join RMTLProject b on a.RatingId = b.RatingId
  left join Job c on a.Billet_Title = c.name
  left join [RmtlProject.Billet] d on b.Id = d.RmtlProjectId and d.JobId = c.Id
  where d.id is null 
  order by 1
  go


-- =============================================
-- billet tasks
/*
The RmtlProjectBilletTask has to consider all sailors 


*/
USE [NavyRRL]
GO

INSERT INTO [dbo].[RmtlProjectBilletTask]
           ([ProjectBilletId]
           ,[RatingLevelTaskId]
           --,[Created]
           --,[LastUpdated]
		   )

SELECT distinct b.Id rmtlProjectBilletId, c.Id as ratingLevelTaskId
--,a.Billet_Title, b.Name

  FROM [NavyRRL].[dbo].[QM_RMTL_11232021] a
  inner join [RmtlProject.Billet] b on a.BilletTitleId = b.JobId
  inner join RatingLevelTask c on a.RatingLevelTaskId = c.Id
  left join [RmtlProjectBilletTask] d on b.Id = d.[ProjectBilletId] and d.[RatingLevelTaskId] = c.Id
  where d.id is null 
  order by 1
  go




