/****** Object:  Trigger [dbo].[trgJobAfterInsert]    Script Date: 10/2/2022 9:20:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER [dbo].[trgJobAfterInsert] 
	ON   [dbo].[Job]
FOR INSERT
AS  
Begin
    INSERT INTO [dbo].[Entity]
           ([EntityUid]
           ,[EntityTypeId]
           ,[Created]
					 ,EntityBaseId, EntityBaseName)
    SELECT RowId,102, getdate(), Id, Name
    FROM inserted;
End
GO
ALTER TABLE [dbo].[Job] ENABLE TRIGGER [trgJobAfterInsert]
GO