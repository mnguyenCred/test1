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
go


BEGIN TRANSACTION
GO
ALTER TABLE dbo.Course ADD
	CTID varchar(50) NULL
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go
Update dbo.Course
set CTID = 'ce-' + Lower(NewId())
go


BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Course.Task] ADD
	CTID varchar(50) NULL
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go
Update dbo.[Course.Task]
set CTID = 'ce-' + Lower(NewId())
go


BEGIN TRANSACTION
GO
ALTER TABLE dbo.CurriculumControlAuthority ADD
	CTID varchar(50) NULL
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go
Update dbo.CurriculumControlAuthority
set CTID = 'ce-' + Lower(NewId())
go


BEGIN TRANSACTION
GO
ALTER TABLE dbo.Assessment ADD
	CTID varchar(50) NULL
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go
Update dbo.Assessment
set CTID = 'ce-' + Lower(NewId())
go


BEGIN TRANSACTION
GO
ALTER TABLE dbo.[FunctionalArea] ADD
	CTID varchar(50) NULL
GO
ALTER TABLE dbo.Course SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go
Update dbo.[FunctionalArea]
set CTID = 'ce-' + Lower(NewId())
go

USE [NavyRRL]
GO
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Source ADD
	CTID varchar(50) NULL
GO
ALTER TABLE dbo.Source SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go
Update dbo.Source
set CTID = 'ce-' + Lower(NewId())
go


USE [NavyRRL]
GO
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[WorkElementType] ADD
	CTID varchar(50) NULL
GO
ALTER TABLE dbo.Source SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go
Update dbo.[WorkElementType]
set CTID = 'ce-' + Lower(NewId())
go

BEGIN TRANSACTION
GO
ALTER TABLE dbo.[RatingLevelTask] ADD
	CTID varchar(50) NULL
GO
ALTER TABLE dbo.Source SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
go
Update dbo.[RatingLevelTask]
set CTID = 'ce-' + Lower(NewId())
go
