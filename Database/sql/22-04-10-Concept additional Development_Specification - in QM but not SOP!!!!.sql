USE [NavyRRL]
GO
--22-04-10-Concept additional Development_Specification - in QM but not SOP!!!!
select 'ce-' + lower(newId())
go

INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name], WorkElementType, Description, ListId
		   ,[CTID]           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (23           ,'xAPI/Captivate/ VSIM','',	'' , 25          
		   ,'ce-2c57249b-b62d-457c-90af-9f3cd56111f6'   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO

--Recommended_Modality
--from QM not in SOP!
INSERT INTO [dbo].[ConceptScheme.Concept]
           ([ConceptSchemeId]           ,[Name], WorkElementType, Description, ListId
		   ,[CTID]           ,[IsActive]           ,[Created]           ,[CreatedById]           ,[LastUpdated]           ,[LastUpdatedById]
		   )
     VALUES
           (21           ,'Performance Support/ Video','',	'Performance Support/ Video (NOT IN SOP)' , 25          
		   ,'ce-c4c6e5fd-7a74-4498-a5b0-e729acfc0e67'   ,1           ,getdate(), 1           ,getdate(), 1
		   )
GO


