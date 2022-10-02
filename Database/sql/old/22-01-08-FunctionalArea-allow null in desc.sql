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
CREATE TABLE dbo.Tmp_FunctionalArea
	(
	Id int NOT NULL IDENTITY (1, 1),
	Name nvarchar(200) NOT NULL,
	Description varchar(MAX) NULL,
	DateCreated datetime NULL,
	DateModified datetime NULL,
	CTID varchar(50) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_FunctionalArea SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_FunctionalArea ON
GO
IF EXISTS(SELECT * FROM dbo.FunctionalArea)
	 EXEC('INSERT INTO dbo.Tmp_FunctionalArea (Id, Name, Description, DateCreated, DateModified, CTID)
		SELECT Id, Name, Description, DateCreated, DateModified, CTID FROM dbo.FunctionalArea WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_FunctionalArea OFF
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT FK_RatingLevelTask_FunctionalArea
GO
DROP TABLE dbo.FunctionalArea
GO
EXECUTE sp_rename N'dbo.Tmp_FunctionalArea', N'FunctionalArea', 'OBJECT' 
GO
ALTER TABLE dbo.FunctionalArea ADD CONSTRAINT
	PK_FunctionalArea PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingTask ADD CONSTRAINT
	FK_RatingLevelTask_FunctionalArea FOREIGN KEY
	(
	FunctionalAreaId
	) REFERENCES dbo.FunctionalArea
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingTask SET (LOCK_ESCALATION = TABLE)
GO
COMMIT