USE [NavyRRL]
GO
--22-04-01-add Concept scheme - Recommended Modality
/*
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
--???
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
           ([ConceptSchemeId]           ,[Name]           ,[CTID],CodedNotation
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (21           ,'Visual Simulation'           ,'ce-' + lower(NewId()) ,'VSIM'          
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO


INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID],CodedNotation
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (21           ,'Instructor-Facilitated Interactive Training (IFIT)'           ,'ce-' + lower(NewId()) ,'IFIT'          
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID],CodedNotation
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (21           ,'Self-Directed Interactive Training (SDIT)'           ,'ce-' + lower(NewId()) ,'SDIT'          
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO
*/
--=================


INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID],CodedNotation
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (21           ,'Unity (Virtual World)'           ,'ce-' + lower(NewId()) ,'Virtual World'          
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO
--
USE [NavyRRL]
GO
UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Virtual Simulation'
     
 WHERE [ConceptSchemeId]=21 and codedNotation = 'VSIM'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Instructor-Facilitated Interactive Training (IFIT)'
     
 WHERE [ConceptSchemeId]=21 and codedNotation = 'IFIT'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Self-Directed Interactive Training (SDIT)'
     
 WHERE [ConceptSchemeId]=21 and codedNotation = 'SDIT'
GO


UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Unity (Virtual World)'
     
 WHERE [ConceptSchemeId]=21 and codedNotation = 'Virtual World'
GO