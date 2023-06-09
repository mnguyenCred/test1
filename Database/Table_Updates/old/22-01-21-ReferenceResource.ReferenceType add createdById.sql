/*
   Friday, January 21, 202211:25:02 AM
   User: 
   Server: CREDENGINEMPARS
   Database: NavyRRL
   Application: 
*/

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
ALTER TABLE dbo.[ReferenceResource.ReferenceType] ADD
	CreatedById int NULL
GO
ALTER TABLE dbo.[ReferenceResource.ReferenceType] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
