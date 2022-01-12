USE [NavyRRL]
GO
/*
INSERT INTO [dbo].[ConceptScheme]
           ([Id]
           ,[Name]
           ,[Description]
           ,[RowId]
           ,[CTID]
           ,[SchemaUri]
           ,[Created]
           ,[LastUpdated]
)
     VALUES
           (17,'Life Cycle Control Document'
           ,NULL
           ,newId()
           ,'ce-'+lower(newId())
           ,'navy:LifeCycleControlDocument'
           ,GETDATE()
           ,GETDATE()
           )
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([RowId] ,[ConceptSchemeId]
           ,[Label]
           ,[CTID] ,[CodedNotation]
           ,[Created]  ,[LastUpdated])
     VALUES
           (newId() ,17
           ,'A'
           ,'ce-'+lower(newId())
           ,'A'
            ,GETDATE(),GETDATE())
GO
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([RowId] ,[ConceptSchemeId]
           ,[Label]
           ,[CTID] ,[CodedNotation]
           ,[Created]  ,[LastUpdated])
     VALUES
           (newId() ,17
           ,'TCCD'
           ,'ce-'+lower(newId())
           ,'TCCD'
            ,GETDATE(),GETDATE())
GO
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([RowId] ,[ConceptSchemeId]
           ,[Label]
           ,[CTID] ,[CodedNotation]
           ,[Created]  ,[LastUpdated])
     VALUES
           (newId() ,17
           ,'CTTL'
           ,'ce-'+lower(newId())
           ,'CTTL'
            ,GETDATE(),GETDATE())
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([RowId] ,[ConceptSchemeId]
           ,[Label]
           ,[CTID] ,[CodedNotation]
           ,[Created]  ,[LastUpdated])
     VALUES
           (newId() ,17
           ,'Cirriculum Review'
           ,'ce-'+lower(newId())
           ,'CR'
            ,GETDATE(),GETDATE())
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([RowId] ,[ConceptSchemeId]
           ,[Label]
           ,[CTID] ,[CodedNotation]
           ,[Created]  ,[LastUpdated])
     VALUES
           (newId() ,17
           ,'Cirriculum Review'
           ,'ce-'+lower(newId())
           ,'CR'
            ,GETDATE(),GETDATE())
GO
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([RowId] ,[ConceptSchemeId]
           ,[Label]
           ,[CTID] ,[CodedNotation]
           ,[Created]  ,[LastUpdated])
     VALUES
           (newId() ,17
           ,'PPP Table'
           ,'ce-'+lower(newId())
           ,'PPP'
            ,GETDATE(),GETDATE())
GO
*/
-- new reference resource
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([RowId] ,[ConceptSchemeId]
           ,[Label]
           ,[CTID] ,[CodedNotation]
		   ,Description
           ,[Created]  ,[LastUpdated])
     VALUES
           (newId() ,9
           ,'Life-Cycle Control Document'
           ,'ce-5f4db99a-5335-43cb-8193-22dcf81e99cd'
           ,'LCCD'
		   ,''
            ,GETDATE(),GETDATE())
GO
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([RowId] ,[ConceptSchemeId]
           ,[Label]
           ,[CTID] ,[CodedNotation]
		   ,Description
           ,[Created]  ,[LastUpdated])
     VALUES
           (newId() ,9
           ,'Life-Cycle Control Document'
           ,'ce-5f4db99a-5335-43cb-8193-22dcf81e99cd'
           ,'NEC'
		   ,''
            ,GETDATE(),GETDATE())
GO


--=============================================
-- updates


UPDATE [dbo].[ConceptScheme.Concept]
   SET [Label] = 'Maintenance Index Page'
      ,[CodedNotation] = 'MIP'
      ,[Description] = NULL

 WHERE label = 'MIP'
 GO
 UPDATE [dbo].[ConceptScheme.Concept]
   SET [Label] = 'Navy Education and Training'
      ,[CodedNotation] = 'NAVEDTRA'
      ,[Description] = '300 Series PQS Watch Station'
 WHERE label = 'NAVEDTRA'
 GO
  UPDATE [dbo].[ConceptScheme.Concept]
   SET [Label] = 'Navy Personnel, Volume I'
      ,[CodedNotation] = 'NAVPERS I'
      ,[Description] = 'NAVPERS 18068F Vol I'
 WHERE label = 'NAVPERS I'
 GO
 --
   UPDATE [dbo].[ConceptScheme.Concept]
   SET [Label] = 'Navy Personnel, Volume II'
      ,[CodedNotation] = 'NAVPERS II'
      ,[Description] = 'NAVPERS 18068F Vol II'
 WHERE label = 'NAVPERS II'
 GO
 --
    UPDATE [dbo].[ConceptScheme.Concept]
   SET [Label] = 'Subject Matter Expert Panel Input'
      ,[CodedNotation] = 'SME'
      ,[Description] = 'Subject Matter Expert Panel Input'
 WHERE label = 'SME'
 GO

