USE [NavyRRL]
GO
--22-04-01-add Concept scheme - CFM Placement
-- not sure makes sense. Will it be year 1 to 15, or are there legit gaps?
INSERT INTO [dbo].[ConceptScheme]
           ([Id]           ,[Name]           ,[Description]           ,[CTID]
           ,[SchemaUri]           ,[Created],[CreatedById]
           ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (22
           ,'CFM Placement'
           ,'CFM placement for cluster analysis'
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
           (22,'Year 1','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 2','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 3','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 4','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 5','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 6','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 7','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 8','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 9','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 10','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 11','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 12','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 13','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 14','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name]           ,[CTID]
           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (22,'Year 15','ce-' + lower(NewId()),1,getdate(), 1,getdate(), 1
		   )
GO




