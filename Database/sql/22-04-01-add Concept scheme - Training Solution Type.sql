USE [NavyRRL]
GO
--22-04-01-add Concept scheme - Training Solution Type
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
           (20
           ,'Training Solution Type'
           ,'Training solution types for cluster analysis'
           ,'ce-' + lower(NewId())
           ,'navy:TrainingSolutionType'
           ,getdate(), 1
           ,getdate(), 1
		   )
GO

--NEC Refresher,Journeyman Core,Master Core,Targeted Refresher,Block Zero Addition,Performance Support,SOJT,Course Revision

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (20           ,'NEC Refresher'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (20           ,'Journeyman Core'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO


INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (20           ,'Master Core'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO


INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (20           ,'Targeted Refresher'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (20           ,'Block Zero Addition'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (20           ,'Performance Support'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (20           ,'SOJT'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO


INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (20           ,'Course Revision'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO



INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (20           ,'New Course Development'           ,'ce-' + lower(NewId())           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO