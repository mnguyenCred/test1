/*
   Friday, January 21, 202212:17:36 PM
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
ALTER TABLE dbo.Rating SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Job ADD
	RatingId int NULL
GO
ALTER TABLE dbo.Job ADD CONSTRAINT
	FK_Job_Rating1 FOREIGN KEY
	(
	RatingId
	) REFERENCES dbo.Rating
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Job SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
