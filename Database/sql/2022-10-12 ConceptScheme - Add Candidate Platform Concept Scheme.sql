USE [Navy_RRL_V2]
GO

INSERT INTO [dbo].[ConceptScheme]
           ([Id]
           ,[Name]
           ,[Description]
           ,[CTID]
           ,[SchemaUri]
           ,[CreatedById]
           ,[LastUpdatedById])
     VALUES
           (25
           ,'Candidate Platform Categories'
           ,'Categories of Candidate Platforms'
           ,'ce-b36a5e5f-5d13-4a99-9d45-9cfddbbb1c9e'
           ,'navy:CandidatePlatformType'
           ,1
           ,1)
GO


