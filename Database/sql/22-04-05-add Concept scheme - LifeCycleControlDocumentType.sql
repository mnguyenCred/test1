USE [NavyRRL]
GO
--22-04-05-add Concept scheme - LifeCycleControlDocumentType
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
           (25
           ,'LifeCycle Control Document Type'
           ,'LifeCycle control document types for courses.'
           ,'ce-' + lower(NewId())
           ,'navy:LifeCycleControlDocumentType'
           ,getdate(), 1
           ,getdate(), 1
		   )
GO

--CTTL, TCCD, not sure about Cirriculum Review
--
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID], CodedNotation
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (25           ,'CTTL'           ,'ce-' + lower(NewId()), 'CTTL'           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID], CodedNotation
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (25           ,'TCCD'           ,'ce-' + lower(NewId()), 'TCCD'           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO
--??
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID], CodedNotation
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (25           ,'Curriculum Review'           ,'ce-' + lower(NewId()), 'Curriculum Review'           
		   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO