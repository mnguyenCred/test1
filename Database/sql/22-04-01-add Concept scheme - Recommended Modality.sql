USE [NavyRRL]
GO
--22-04-01-add Concept scheme - Recommended Modality
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
           (21
           ,'Recommended Modality'
           ,'Recommended modality for cluster analysis'
           ,'ce-' + lower(NewId())
           ,'navy:RecommendedModality'
           ,getdate(), 1
           ,getdate(), 1
		   )
GO

--

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (21           ,'Performance Support/Video'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (21           ,'Video'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO


INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (21           ,'VSIM'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO

