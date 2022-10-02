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
ALTER TABLE dbo.[Course.Task]
	DROP CONSTRAINT [FK_Course.Task_Course]
GO
ALTER TABLE dbo.Course ADD
	RowId uniqueidentifier NOT NULL CONSTRAINT DF_Course_RowId DEFAULT NewId()
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.Task] ADD
	RowId uniqueidentifier NOT NULL CONSTRAINT [DF_Course.Task_RowId] DEFAULT (newid())
GO
ALTER TABLE dbo.[Course.Task] ADD CONSTRAINT
	[FK_Course.Task_Course] FOREIGN KEY
	(
	CourseId
	) REFERENCES dbo.Course
	(
	Id
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.[Course.Task] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT



  --update [NavyRRL].[dbo].[Course.Task]
  --set rowid = NewId()

  /* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
ALTER TABLE dbo.Course ADD
	RowId uniqueidentifier NOT NULL CONSTRAINT DF_Course_RowId DEFAULT (newid())
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go
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
ALTER TABLE dbo.CurriculumControlAuthority ADD
	RowId uniqueidentifier NOT NULL CONSTRAINT DF_CurriculumControlAuthority_RowId DEFAULT (newid())
GO
ALTER TABLE dbo.CurriculumControlAuthority SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go

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
ALTER TABLE dbo.FunctionalArea ADD
	RowId uniqueidentifier NOT NULL CONSTRAINT DF_FunctionalArea_RowId DEFAULT (newid())
GO
ALTER TABLE dbo.FunctionalArea SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go