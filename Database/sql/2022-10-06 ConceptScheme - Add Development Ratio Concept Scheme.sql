/* 
	2022-10-06 - Add Development Ratio to ConceptScheme table
*/
USE [Navy_RRL_V2]
GO

INSERT INTO [dbo].[ConceptScheme]
           ([Id]
           ,[Name]
           ,[Description]
           ,[CTID]
           ,[SchemaUri]
           ,[CreatedById])
     VALUES
           (24
           ,'Development Ratio'
           ,'Development Ratios for Cluster Analysis'
           ,'ce-7b56ac98-b4c7-426f-a517-263a6b3e786f'
           ,'navy:DevelopmentRatio'
           ,1)
GO


