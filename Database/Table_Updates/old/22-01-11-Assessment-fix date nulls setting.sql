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
ALTER TABLE dbo.Assessment
	DROP CONSTRAINT DF_Assessment_RowId
GO
ALTER TABLE dbo.Assessment
	DROP CONSTRAINT DF_Assessment_Created
GO
ALTER TABLE dbo.Assessment
	DROP CONSTRAINT DF_Assessment_LastUpdated
GO
CREATE TABLE dbo.Tmp_Assessment
	(
	Id int NOT NULL IDENTITY (1, 1),
	RowId uniqueidentifier NOT NULL,
	Name nvarchar(500) NOT NULL,
	Description nvarchar(MAX) NULL,
	Created datetime NOT NULL,
	CreatedById int NULL,
	LastUpdated datetime NOT NULL,
	LastUpdatedById int NULL,
	CTID varchar(50) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_Assessment SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_Assessment ADD CONSTRAINT
	DF_Assessment_RowId DEFAULT (newid()) FOR RowId
GO
ALTER TABLE dbo.Tmp_Assessment ADD CONSTRAINT
	DF_Assessment_Created DEFAULT (getdate()) FOR Created
GO
ALTER TABLE dbo.Tmp_Assessment ADD CONSTRAINT
	DF_Assessment_LastUpdated DEFAULT (getdate()) FOR LastUpdated
GO
SET IDENTITY_INSERT dbo.Tmp_Assessment ON
GO
IF EXISTS(SELECT * FROM dbo.Assessment)
	 EXEC('INSERT INTO dbo.Tmp_Assessment (Id, RowId, Name, Description, Created, CreatedById, LastUpdated, LastUpdatedById, CTID)
		SELECT Id, RowId, Name, Description, Created, CreatedById, LastUpdated, LastUpdatedById, CTID FROM dbo.Assessment WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_Assessment OFF
GO
DROP TABLE dbo.Assessment
GO
EXECUTE sp_rename N'dbo.Tmp_Assessment', N'Assessment', 'OBJECT' 
GO
ALTER TABLE dbo.Assessment ADD CONSTRAINT
	PK_Assessment PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT