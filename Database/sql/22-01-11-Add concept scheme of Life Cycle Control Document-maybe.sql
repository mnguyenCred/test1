USE [NavyRRL]
GO
/*



-- new reference resource
OBSOLETE 
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([RowId] ,[ConceptSchemeId]
           ,Name
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
           ,Name
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
*/

--=============================================
-- updates


UPDATE [dbo].[ConceptScheme.Concept]
   SET Name = 'Maintenance Index Page'
      ,[CodedNotation] = 'MIP'
      ,[Description] = NULL

 WHERE Name = 'MIP'
 GO
 UPDATE [dbo].[ConceptScheme.Concept]
   SET Name = 'Navy Education and Training'
      ,[CodedNotation] = 'NAVEDTRA'
      ,[Description] = '300 Series PQS Watch Station'
 WHERE Name = 'NAVEDTRA'
 GO
  UPDATE [dbo].[ConceptScheme.Concept]
   SET Name = 'Navy Personnel, Volume I'
      ,[CodedNotation] = 'NAVPERS I'
      ,[Description] = 'NAVPERS 18068F Vol I'
 WHERE Name = 'NAVPERS I'
 GO
 --
   UPDATE [dbo].[ConceptScheme.Concept]
   SET Name = 'Navy Personnel, Volume II'
      ,[CodedNotation] = 'NAVPERS II'
      ,[Description] = 'NAVPERS 18068F Vol II'
 WHERE Name = 'NAVPERS II'
 GO
 --
    UPDATE [dbo].[ConceptScheme.Concept]
   SET Name = 'Subject Matter Expert Panel Input'
      ,[CodedNotation] = 'SME'
      ,[Description] = 'Subject Matter Expert Panel Input'
 WHERE Name = 'SME'
 GO

