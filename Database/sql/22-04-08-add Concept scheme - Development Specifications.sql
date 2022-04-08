USE [NavyRRL]
GO
--22-04-08-add Concept scheme - Development Specifications

INSERT INTO [dbo].[ConceptScheme]
           ([Id]
           ,[Name]
           ,[Description]
           ,[CTID]
           ,[SchemaUri]
           ,[Created],[CreatedById]
           ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (23
           ,'Development Specifications'
           ,'Development Specifications for cluster analysis'
           ,'ce-' + lower(NewId())
           ,'navy:DevelopmentSpecification'
           ,getdate(), 1
           ,getdate(), 1
		   )
GO

--
--???
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (23           ,'ICW-1/xAPI/H5P/MELD'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (23           ,'ICW-2/xAPI/H5P/Unity/MELD'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO


INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (23           ,'ICW-3/xAPI/H5P/Unity/MELD'           ,'ce-' + lower(NewId())          
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO


INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (23           ,'ICW-4/xAPI/H5P/Unity/MELD'           ,'ce-' + lower(NewId())          
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO


--=================

