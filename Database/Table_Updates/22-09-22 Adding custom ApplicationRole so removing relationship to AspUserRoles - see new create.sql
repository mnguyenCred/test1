-- 22-09-22 Adding custom ApplicationRole so removing relationship to AspUserRoles  - see new create

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
ALTER TABLE dbo.ApplicationRole SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.AppFunctionPermission
	DROP CONSTRAINT FK_AppFunctionPermission_ApplicationFunction
GO
ALTER TABLE dbo.ApplicationFunction SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.AppFunctionPermission
	DROP CONSTRAINT DF_Table_1_Create
GO
ALTER TABLE dbo.AppFunctionPermission
	DROP CONSTRAINT DF_Table_1_Read
GO
ALTER TABLE dbo.AppFunctionPermission
	DROP CONSTRAINT DF_Table_1_Update
GO
ALTER TABLE dbo.AppFunctionPermission
	DROP CONSTRAINT DF_Table_1_Delete
GO
CREATE TABLE dbo.Tmp_AppFunctionPermission
	(
	ApplicationFunctionId int NOT NULL,
	RoleId int NOT NULL,
	CanCreate bit NOT NULL,
	CanRead bit NOT NULL,
	CanUpdate bit NOT NULL,
	CanDelete bit NOT NULL,
	Action varchar(50) NULL,
	ActionId int NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_AppFunctionPermission SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_AppFunctionPermission ADD CONSTRAINT
	DF_Table_1_Create DEFAULT ((0)) FOR CanCreate
GO
ALTER TABLE dbo.Tmp_AppFunctionPermission ADD CONSTRAINT
	DF_Table_1_Read DEFAULT ((0)) FOR CanRead
GO
ALTER TABLE dbo.Tmp_AppFunctionPermission ADD CONSTRAINT
	DF_Table_1_Update DEFAULT ((0)) FOR CanUpdate
GO
ALTER TABLE dbo.Tmp_AppFunctionPermission ADD CONSTRAINT
	DF_Table_1_Delete DEFAULT ((0)) FOR CanDelete
GO
IF EXISTS(SELECT * FROM dbo.AppFunctionPermission)
	 EXEC('INSERT INTO dbo.Tmp_AppFunctionPermission (ApplicationFunctionId, RoleId, CanCreate, CanRead, CanUpdate, CanDelete, Action, ActionId)
		SELECT ApplicationFunctionId, CONVERT(int, RoleId), CanCreate, CanRead, CanUpdate, CanDelete, Action, ActionId FROM dbo.AppFunctionPermission WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.AppFunctionPermission
GO
EXECUTE sp_rename N'dbo.Tmp_AppFunctionPermission', N'AppFunctionPermission', 'OBJECT' 
GO
ALTER TABLE dbo.AppFunctionPermission ADD CONSTRAINT
	PK_AppFunctionPermission PRIMARY KEY CLUSTERED 
	(
	ApplicationFunctionId,
	RoleId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.AppFunctionPermission ADD CONSTRAINT
	FK_AppFunctionPermission_ApplicationFunction FOREIGN KEY
	(
	ApplicationFunctionId
	) REFERENCES dbo.ApplicationFunction
	(
	Id
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.AppFunctionPermission ADD CONSTRAINT
	FK_AppFunctionPermission_ApplicationRole FOREIGN KEY
	(
	RoleId
	) REFERENCES dbo.ApplicationRole
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT