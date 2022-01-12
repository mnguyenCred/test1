USE [NavyRRL]
GO

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



