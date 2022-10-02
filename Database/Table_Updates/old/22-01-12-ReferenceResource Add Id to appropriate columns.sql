/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
EXECUTE sp_rename N'dbo.ReferenceResource.StatusType', N'Tmp_StatusTypeId_8', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.ReferenceResource.ReferenceType', N'Tmp_ReferenceTypeId_9', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.ReferenceResource.Tmp_StatusTypeId_8', N'StatusTypeId', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.ReferenceResource.Tmp_ReferenceTypeId_9', N'ReferenceTypeId', 'COLUMN' 
GO
ALTER TABLE dbo.ReferenceResource SET (LOCK_ESCALATION = TABLE)
GO
COMMIT