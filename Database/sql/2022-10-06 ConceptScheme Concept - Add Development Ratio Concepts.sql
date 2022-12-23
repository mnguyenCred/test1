/* 
	2022-10-06 - Add Development Ratio to Concepts to ConceptScheme.Concept table
*/
USE [Navy_RRL_V2]
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name]
           ,[CodedNotation]
           ,[AlternateLabel]
           ,[Description]
           ,[ListId]
           ,[IsActive]
           ,[CTID]
           ,[CreatedById]
           ,[LastUpdatedById])
     VALUES
           (24
           ,'70:1'
           ,'70:1'
           ,'ICW-1'
           ,'ICW-1 (70:1 ratio)'
           ,5
           ,1
           ,'ce-e73e4130-a3cd-43be-94d0-f07a3b419148'
           ,1
           ,1)
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name]
           ,[CodedNotation]
           ,[AlternateLabel]
           ,[Description]
           ,[ListId]
           ,[IsActive]
           ,[CTID]
           ,[CreatedById]
           ,[LastUpdatedById])
     VALUES
           (24
           ,'220:1'
           ,'220:1'
           ,'ICW-2'
           ,'ICW-2 (220:1 ratio)'
           ,10
           ,1
           ,'ce-1ab9fa1d-b511-41d4-afb0-5ff35ed16260'
           ,1
           ,1)
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name]
           ,[CodedNotation]
           ,[AlternateLabel]
           ,[Description]
           ,[ListId]
           ,[IsActive]
           ,[CTID]
           ,[CreatedById]
           ,[LastUpdatedById])
     VALUES
           (24
           ,'300:1'
           ,'300:1'
           ,'ICW-3'
           ,'ICW-3 (300:1 ratio)'
           ,15
           ,1
           ,'ce-60e72978-b004-44e1-9118-f020a152500a'
           ,1
           ,1)
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name]
           ,[CodedNotation]
           ,[AlternateLabel]
           ,[Description]
           ,[ListId]
           ,[IsActive]
           ,[CTID]
           ,[CreatedById]
           ,[LastUpdatedById])
     VALUES
           (24
           ,'750:1'
           ,'750:1'
           ,'ICW-4'
           ,'ICW-4 (750:1 ratio)'
           ,20
           ,1
           ,'ce-daf44952-0fc9-4dd3-bb22-1b999fda0114'
           ,1
           ,1)
GO

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]
           ,[Name]
           ,[CodedNotation]
           ,[AlternateLabel]
           ,[Description]
           ,[ListId]
           ,[IsActive]
           ,[CTID]
           ,[CreatedById]
           ,[LastUpdatedById])
     VALUES
           (24
           ,'1000:1'
           ,'1000:1'
           ,'Virtual World'
           ,'Virtual World (1000:1 ratio)'
           ,25
           ,1
           ,'ce-61407c32-cd24-4c5c-92d2-c6649e08f665'
           ,1
           ,1)
GO
