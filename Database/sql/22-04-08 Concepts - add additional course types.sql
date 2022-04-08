USE [NavyRRL]
GO
--22-04-08 Concepts - add additional course types
-- have A C F G T
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], Description
           ,[CTID]
           ,[ListId]           ,[IsActive]           ,[Created] ,[CreatedById]              ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (16, 'D-School', 'Professional Development Functional Skill Training'
		   ,'ce-' + lower(newid())		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], Description
           ,[CTID]
           ,[ListId]           ,[IsActive]           ,[Created] ,[CreatedById]              ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (16, 'E-School', 'Professional Development Education'
		   ,'ce-' + lower(newid())		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
--
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], Description
           ,[CTID]
           ,[ListId]           ,[IsActive]           ,[Created] ,[CreatedById]              ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (16, 'H-School', 'Seabee Combat Warfare Specialist'
		   ,'ce-' + lower(newid())		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
--
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], Description
           ,[CTID]
           ,[ListId]           ,[IsActive]           ,[Created] ,[CreatedById]              ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (16, 'M-School', 'United States Marine Corp'
		   ,'ce-' + lower(newid())		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
--
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], Description
           ,[CTID]
           ,[ListId]           ,[IsActive]           ,[Created] ,[CreatedById]              ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (16, 'P-School', 'Officer Acquisition Programs'
		   ,'ce-' + lower(newid())		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
--
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], Description
           ,[CTID]
           ,[ListId]           ,[IsActive]           ,[Created] ,[CreatedById]              ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (16, 'R-School', 'Recruit Programs'
		   ,'ce-' + lower(newid())		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
--
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], Description
           ,[CTID]
           ,[ListId]           ,[IsActive]           ,[Created] ,[CreatedById]              ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (16, 'V-School', 'Aviation Training'
		   ,'ce-' + lower(newid())		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
--
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], Description
           ,[CTID]
           ,[ListId]           ,[IsActive]           ,[Created] ,[CreatedById]              ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (16, 'ISEA Training', 'Training provided by In-Service Engineering Agents (ISEA)'
		   ,'ce-' + lower(newid())		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
--
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], Description
           ,[CTID]
           ,[ListId]           ,[IsActive]           ,[Created] ,[CreatedById]              ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (16, 'Vendor Training', 'Training provided by vendors/manufacturers'
		   ,'ce-' + lower(newid())		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
--
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], Description
           ,[CTID]
           ,[ListId]           ,[IsActive]           ,[Created] ,[CreatedById]              ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (16, 'SAGT', 'Self-Assessment Groom Training (SAGT)'
		   ,'ce-' + lower(newid())		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
--
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], Description
           ,[CTID]
           ,[ListId]           ,[IsActive]           ,[Created] ,[CreatedById]              ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (16, 'D-SAGT', 'Deck Self-Assessment Groom Training (D-SAGT)'
		   ,'ce-' + lower(newid())		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
--
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], Description
           ,[CTID]
           ,[ListId]           ,[IsActive]           ,[Created] ,[CreatedById]              ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (16, 'Other', 'Training that does not fall within the realm of the above types'
		   ,'ce-' + lower(newid())		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
-- =================================================
print ' updates '
Update [dbo].[ConceptScheme.Concept]
set Description = 'Initial Skill Training'
where [ConceptSchemeId]= 16 and [Name]= 'A-School'		   
GO
--
Update [dbo].[ConceptScheme.Concept]
set Description = 'Skill Progression Training – NEC Awarding'
where [ConceptSchemeId]= 16 and [Name]= 'C-School'		   
GO
--
Update [dbo].[ConceptScheme.Concept]
set Description = 'Functional Training'
where [ConceptSchemeId]= 16 and [Name]= 'F-School'		   
GO
--
Update [dbo].[ConceptScheme.Concept]
set Description = 'Umbrella Segment Skill Progression Training'
where [ConceptSchemeId]= 16 and [Name]= 'G-School'
		   
GO
--
Update [dbo].[ConceptScheme.Concept]
set Description = 'Team Functional Skill Training'
where [ConceptSchemeId]= 16 and [Name]= 'T-School'
		   
GO