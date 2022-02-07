USE [NavyRRL]
GO
--22-02-02 - CCA-Organization - populate full name
SELECT TOP (1000) [Id]
      ,[RowId]
      ,[Name]      ,[AlternateName]
      ,[Description]
      ,[CTID]

  FROM [NavyRRL].[dbo].[Organization]
  go
SELECT [Id]
    --  ,[RowId]
      ,[Name]      
	  --,[AlternateName]
	  ,case 
		When name = 'CENSERVSUPP' then 'Is this:  Center for Service Support (CSS)'
		when  name = 'CENSEABEESFACENG' then 'Is this:  Center for Seabees and Facilities Engineering (CSFE)'
		when name = 'SWOS' then '???? does not appear to be in list'
		when name = 'SWSCOLCOM' then '???? does not appear to be in list'
		when name = 'CENSAFE' then '???? does not appear to be in list'
	   else '' end as listDifferences
	   --,[Description]
  --    ,[CTID]
 --     ,[SubjectWebpage]
   --   ,[StatusId]

  --    ,[Created]      ,[CreatedById]      ,[LastUpdated]      ,[LastUpdatedById]
  FROM [dbo].[Organization]
  where name = AlternateName
  order by name 
GO



UPDATE [dbo].[Organization]
   SET [AlternateName] = Name
GO


--not found yet
--UPDATE [dbo].[Organization]
--   SET  [Name] = 'Navy Recruiting Command'
-- WHERE [AlternateName]='NRC'
--GO
INSERT INTO [dbo].[Organization]
           ( [Name],[AlternateName],[CTID]           
           ,[Created],[CreatedById],[LastUpdated],[LastUpdatedById])
     VALUES
           ('Navy Recruiting Command','NRC','ce-' + Lower(NewId())
           ,GETDATE(),1, GETDATE(), 1  )
GO


--=================
UPDATE [dbo].[Organization]
   SET  [Name] = 'Naval Service Training Command'
 WHERE [AlternateName]='NSTC'
GO
--UPDATE [dbo].[Organization]
--   SET  [Name] = 'Center for Explosive Ordnance Disposal and Diving'
-- WHERE [AlternateName]='CEODD'
--GO
INSERT INTO [dbo].[Organization]
           ( [Name],[AlternateName],[CTID]           
           ,[Created],[CreatedById],[LastUpdated],[LastUpdatedById])
     VALUES
           ('Center for Explosive Ordnance Disposal and Diving','CEODD','ce-' + Lower(NewId())
           ,GETDATE(),1, GETDATE(), 1  )
GO

--UPDATE [dbo].[Organization]
--   SET  [Name] = 'Center for Information Warfare Training'
-- WHERE [AlternateName]='CIWC'
--GO
INSERT INTO [dbo].[Organization]
           ( [Name],[AlternateName],[CTID]           
           ,[Created],[CreatedById],[LastUpdated],[LastUpdatedById])
     VALUES
           ('Center for Information Warfare Training','CIWC','ce-' + Lower(NewId())
           ,GETDATE(),1, GETDATE(), 1  )
GO
UPDATE [dbo].[Organization]
   SET  [Name] = 'Center for Naval Aviation Technical Training'
 WHERE [AlternateName]='CNATT'
GO

-- ===========================
--problem
UPDATE [dbo].[Organization]
   SET  [Name] = 'Center for Seabees and Facilities Engineering'
 WHERE [AlternateName]='CENSEABEESFACENG'
GO
--UPDATE [dbo].[Organization]
--   SET  [Name] = 'Center for Seabees and Facilities Engineering'
-- WHERE [AlternateName]='CSFE'
--GO
--INSERT INTO [dbo].[Organization]
--           ( [Name],[AlternateName],[CTID]           
--           ,[Created],[CreatedById],[LastUpdated],[LastUpdatedById])
--     VALUES
--           ('Center for Seabees and Facilities Engineering','CSFE','ce-' + Lower(NewId())
--           ,GETDATE(),1, GETDATE(), 1  )
--GO
--===========================================
--UPDATE [dbo].[Organization]
--   SET  [Name] = 'Center for Security Forces'
-- WHERE [AlternateName]='CSF'
--GO
INSERT INTO [dbo].[Organization]
           ( [Name],[AlternateName],[CTID]           
           ,[Created],[CreatedById],[LastUpdated],[LastUpdatedById])
     VALUES
           ('Center for Security Forces','CSF','ce-' + Lower(NewId())
           ,GETDATE(),1, GETDATE(), 1  )
GO
-- =      ===============
--problem, existing has: CENSERVSUPP
UPDATE [dbo].[Organization]
   SET  [Name] = 'Center for Service Support'
 WHERE [AlternateName]='CENSERVSUPP'
GO
--UPDATE [dbo].[Organization]
--   SET  [Name] = 'Center for Service Support'
-- WHERE [AlternateName]='CSS'
--GO
--INSERT INTO [dbo].[Organization]
--           ( [Name],[AlternateName],[CTID]           
--           ,[Created],[CreatedById],[LastUpdated],[LastUpdatedById])
--     VALUES
--           ('Center for Service Support','CSS','ce-' + Lower(NewId())
--           ,GETDATE(),1, GETDATE(), 1  )
--GO
--UPDATE [dbo].[Organization]
--   SET  [Name] = 'Engineering Duty Officer School'
-- WHERE [AlternateName]='EDO'
--GO
INSERT INTO [dbo].[Organization]
           ( [Name],[AlternateName],[CTID]           
           ,[Created],[CreatedById],[LastUpdated],[LastUpdatedById])
     VALUES
           ('Engineering Duty Officer School','EDO','ce-' + Lower(NewId())
           ,GETDATE(),1, GETDATE(), 1  )
GO
--===================
UPDATE [dbo].[Organization]
   SET  [Name] = 'Naval Aviation Schools Command'
 WHERE [AlternateName]='NASC'
GO
/*
UPDATE [dbo].[Organization]
   SET  [Name] = 'Naval Education and Training Security Assistance Field Activity'
 WHERE [AlternateName]='NETSAFA'
GO
*/
INSERT INTO [dbo].[Organization]
           ( [Name],[AlternateName],[CTID]           
           ,[Created],[CreatedById],[LastUpdated],[LastUpdatedById])
     VALUES
           ('Naval Education and Training Security Assistance Field Activity','NETSAFA','ce-' + Lower(NewId())
           ,GETDATE(),1, GETDATE(), 1  )
GO
--===================
UPDATE [dbo].[Organization]
   SET  [Name] = 'Naval Leadership and Ethics Center'
 WHERE [AlternateName]='NLEC'
GO

/*
UPDATE [dbo].[Organization]
   SET  [Name] = 'Naval Special Warfare Leadership Education and Development Command'
 WHERE [AlternateName]='NSWLEDC'
GO
*/
INSERT INTO [dbo].[Organization]
           ( [Name],[AlternateName],[CTID]           
           ,[Created],[CreatedById],[LastUpdated],[LastUpdatedById])
     VALUES
           ('Naval Special Warfare Leadership Education and Development Command','NSWLEDC','ce-' + Lower(NewId())
           ,GETDATE(),1, GETDATE(), 1  )
GO
--===================
/*
UPDATE [dbo].[Organization]
   SET  [Name] = 'Senior Enlisted Academy'
 WHERE [AlternateName]='SEA'
GO
*/
INSERT INTO [dbo].[Organization]
           ( [Name],[AlternateName],[CTID]           
           ,[Created],[CreatedById],[LastUpdated],[LastUpdatedById])
     VALUES
           ('Senior Enlisted Academy','SEA','ce-' + Lower(NewId())
           ,GETDATE(),1, GETDATE(), 1  )
GO
/*
UPDATE [dbo].[Organization]
   SET  [Name] = 'Submarine Learning Center'
 WHERE [AlternateName]='SLC'
GO
*/
INSERT INTO [dbo].[Organization]
           ( [Name],[AlternateName],[CTID]           
           ,[Created],[CreatedById],[LastUpdated],[LastUpdatedById])
     VALUES
           ('Submarine Learning Center','SLC','ce-' + Lower(NewId())
           ,GETDATE(),1, GETDATE(), 1  )
GO
/*
UPDATE [dbo].[Organization]
   SET  [Name] = 'Surface Combat Systems Training Command'
 WHERE [AlternateName]='SCSTC'
GO
*/
INSERT INTO [dbo].[Organization]
           ( [Name],[AlternateName],[CTID]           
           ,[Created],[CreatedById],[LastUpdated],[LastUpdatedById])
     VALUES
           ('Surface Combat Systems Training Command','SCSTC','ce-' + Lower(NewId())
           ,GETDATE(),1, GETDATE(), 1  )
GO
--===================
--problem
UPDATE [dbo].[Organization]
   SET  [Name] = 'Surface Warfare Schools Command'
 WHERE [AlternateName]='SWSCOLCOM'
GO
--UPDATE [dbo].[Organization]
--   SET  [Name] = 'Surface Warfare Schools Command'
-- WHERE [AlternateName]='SWSC'
--GO
--Missing
CENSERVSUPP
CENSEABEESFACENG
SWSCOLCOM
CENSAFE
--TBD
--UPDATE [dbo].[Organization]
--   SET  [Name] = Name + ' (' + [AlternateName] + ')'

--GO

