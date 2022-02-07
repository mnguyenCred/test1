USE [NavyRRL]
GO
--22-02-04 Concepts - add anticipated work element types
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], CodedNotation, WorkElementType
           ,[CTID]
           ,[ListId]
           ,[IsActive]
           ,[Created] ,[CreatedById]   
           ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (9, 'Naval Operational Support Center Tasks', 'NOSC', 'NOSC'
		   ,'ce-' + lower(newid())
		   ,25, 1, getdate(), 1, getdate(), 1
			)
GO
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], CodedNotation, WorkElementType
           ,[CTID]
           ,[ListId]
           ,[IsActive]
           ,[Created] ,[CreatedById]   
           ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (9, 'Troubleshooting Tasks','Troubleshooting Tasks','Troubleshooting Tasks'
		   ,'ce-' + lower(newid())
		   ,30, 1, getdate(), 1, getdate(), 1
			)
GO
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name], CodedNotation, WorkElementType
           ,[CTID]
           ,[ListId]
           ,[IsActive]
           ,[Created] ,[CreatedById]   
           ,[LastUpdated],[LastUpdatedById]
		   )
     VALUES
           (9, 'Repair Tasks','Repair Tasks','Repair Tasks'
		   ,'ce-' + lower(newid())
		   ,30, 1, getdate(), 1, getdate(), 1
			)
GO


