USE [NavyRRL]
GO
--22-04-09-Concept scheme updates
select 'ce-' + lower(newId())

--******* NOTE THERE WILL BE AN ISSUE WHERE A CTID IS BEING GENERATED - IT WILL BE DIFFERENT ON DIFFERENT SERVERS ********
--				so need to remember to create and hard code!!
--
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name], WorkElementType, Description, ListId
		   ,[CTID]           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (9           ,'NAMTS JQR','NAMTS JQR',	'Example: NAMTS Electrical Repair Technician' , 16          
		   ,'ce-26cbac92-4138-4a5a-a63c-876708244215'    ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name], WorkElementType, Description, ListId
		   ,[CTID]           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (9           ,'QPT Title','QPT',	''  , 18         
		   ,'ce-913c5a2d-482f-4951-bf3d-29e673fb662a'   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name], WorkElementType, Description, ListId
		   ,[CTID]           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (9           ,'TDR','TDR',	'Any additional tasks that are requested by TYCOM that do not have any source documentation. Any exception must be approved by USFFC'  , 38         
		   ,'ce-230b8873-9f2a-49e7-b4c8-ca2966d9d67e'   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO

--TEMP ADD OF Other FOR LOCAL ONLY
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name], WorkElementType, Description, ListId
		   ,[CTID]           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (9           ,'Other','Other',	''  , 39         
		   ,'ce-b89dbd15-a094-4765-8bdd-72e9be74b57d'   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO