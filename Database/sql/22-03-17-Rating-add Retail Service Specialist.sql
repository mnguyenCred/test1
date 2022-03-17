USE [NavyRRL]
GO
--22-03-17-Rating-add Retail Service Specialist
INSERT INTO [dbo].[Rating]
           ([CTID]
           ,[Name]
           ,[CodedNotation]
           ,[Description]
           ,[Version]

           ,[RowId]
           ,[Created]
           ,[CreatedById]
           ,[LastUpdated]
           ,[LastUpdatedById])
     VALUES
           ('ce-1c2adc05-710e-49c9-8b48-a025b4c6e341'
           ,'Retail Service Specialist'
           ,'RS'
           ,NULL
           ,'1'

           ,newId()
           ,getdate()
           ,1
           ,getdate()
           ,1)
GO


