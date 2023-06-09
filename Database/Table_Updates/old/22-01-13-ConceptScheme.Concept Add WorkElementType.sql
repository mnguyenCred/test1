/*
   Thursday, January 13, 20228:57:30 AM
   User: mparsons
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
ALTER TABLE dbo.[ConceptScheme.Concept] ADD
	WorkElementType varchar(200) NULL
GO
ALTER TABLE dbo.[ConceptScheme.Concept] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go
UPDATE [dbo].[ConceptScheme.Concept]
   SET [WorkElementType] = 'Occupational Standard Task'
 WHERE name = 'Occupational Standards'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [WorkElementType] = 'Naval Standard Task'
 WHERE name = 'Navy Standards'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [WorkElementType] = 'NEC'
 WHERE name = 'Navy Enlisted Classification'
GO


UPDATE [dbo].[ConceptScheme.Concept]
   SET [WorkElementType] = '300 Series PQS Watch Station'
 WHERE name = '300 Series PQS Watch Station'
GO
--
UPDATE [dbo].[ConceptScheme.Concept]
   SET [WorkElementType] = 'Maintenance Index Page (MIP)'
 WHERE name = 'Maintenance Index Page'
GO
