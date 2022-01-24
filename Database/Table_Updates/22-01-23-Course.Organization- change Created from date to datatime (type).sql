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
ALTER TABLE dbo.[Course.Organization]
	DROP CONSTRAINT [FK_Course.Organization_Organization]
GO
ALTER TABLE dbo.Organization SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.Organization]
	DROP CONSTRAINT FK_CourseOrganization_Course
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.Organization]
	DROP CONSTRAINT [DF_Course.Organization_RowId]
GO
ALTER TABLE dbo.[Course.Organization]
	DROP CONSTRAINT [DF_Course.Organization_Created]
GO
CREATE TABLE dbo.[Tmp_Course.Organization]
	(
	Id int NOT NULL IDENTITY (1, 1),
	RowId uniqueidentifier NOT NULL,
	CourseId int NOT NULL,
	OrganizationId int NOT NULL,
	Created datetime NOT NULL,
	CreatedById int NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.[Tmp_Course.Organization] SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.[Tmp_Course.Organization] ADD CONSTRAINT
	[DF_Course.Organization_RowId] DEFAULT (newid()) FOR RowId
GO
ALTER TABLE dbo.[Tmp_Course.Organization] ADD CONSTRAINT
	[DF_Course.Organization_Created] DEFAULT (getdate()) FOR Created
GO
SET IDENTITY_INSERT dbo.[Tmp_Course.Organization] ON
GO
IF EXISTS(SELECT * FROM dbo.[Course.Organization])
	 EXEC('INSERT INTO dbo.[Tmp_Course.Organization] (Id, RowId, CourseId, OrganizationId, Created, CreatedById)
		SELECT Id, RowId, CourseId, OrganizationId, CONVERT(datetime, Created), CreatedById FROM dbo.[Course.Organization] WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.[Tmp_Course.Organization] OFF
GO
DROP TABLE dbo.[Course.Organization]
GO
EXECUTE sp_rename N'dbo.[Tmp_Course.Organization]', N'Course.Organization', 'OBJECT' 
GO
ALTER TABLE dbo.[Course.Organization] ADD CONSTRAINT
	[PK_Course.Organization] PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.[Course.Organization] ADD CONSTRAINT
	FK_CourseOrganization_Course FOREIGN KEY
	(
	CourseId
	) REFERENCES dbo.Course
	(
	Id
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.[Course.Organization] ADD CONSTRAINT
	[FK_Course.Organization_Organization] FOREIGN KEY
	(
	OrganizationId
	) REFERENCES dbo.Organization
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT