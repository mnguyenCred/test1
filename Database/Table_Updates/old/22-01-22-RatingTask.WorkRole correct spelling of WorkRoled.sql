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
EXECUTE sp_rename N'dbo.[RatingTask.WorkRole].WorkRoled', N'Tmp_WorkRoleId', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.[RatingTask.WorkRole].Tmp_WorkRoleId', N'WorkRoleId', 'COLUMN' 
GO
ALTER TABLE dbo.[RatingTask.WorkRole] SET (LOCK_ESCALATION = TABLE)
GO
