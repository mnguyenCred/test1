/*
   Thursday, December 15, 20229:57:31 AM
   User: 
   Server: WORK-PC
   Database: Navy_RRL_V2
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
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT FK_RatingTask_to_ReferenceResource
GO
ALTER TABLE dbo.ReferenceResource SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT FK_RatingTask_ReferenceType
GO
ALTER TABLE dbo.[ConceptScheme.Concept] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.WorkRole SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Job SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT DF_RatingTask_RowId
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT DF_RatingLevelTask_Created
GO
ALTER TABLE dbo.RatingTask
	DROP CONSTRAINT DF_RatingLevelTask_LastUpdated
GO
CREATE TABLE dbo.Tmp_RatingTask
	(
	Id int NOT NULL IDENTITY (1, 1),
	RowId uniqueidentifier NOT NULL,
	ReferenceResourceId int NULL,
	Description nvarchar(MAX) NOT NULL,
	CTID varchar(50) NULL,
	Created datetime NOT NULL,
	CreatedById int NULL,
	LastUpdated datetime NOT NULL,
	LastUpdatedById int NULL,
	ReferenceTypeId int NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_RatingTask SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_RatingTask ADD CONSTRAINT
	DF_RatingTask_RowId DEFAULT (newid()) FOR RowId
GO
ALTER TABLE dbo.Tmp_RatingTask ADD CONSTRAINT
	DF_RatingLevelTask_Created DEFAULT (getdate()) FOR Created
GO
ALTER TABLE dbo.Tmp_RatingTask ADD CONSTRAINT
	DF_RatingLevelTask_LastUpdated DEFAULT (getdate()) FOR LastUpdated
GO
SET IDENTITY_INSERT dbo.Tmp_RatingTask ON
GO
IF EXISTS(SELECT * FROM dbo.RatingTask)
	 EXEC('INSERT INTO dbo.Tmp_RatingTask (Id, RowId, ReferenceResourceId, Description, CTID, Created, CreatedById, LastUpdated, LastUpdatedById, ReferenceTypeId)
		SELECT Id, RowId, ReferenceResourceId, Description, CTID, Created, CreatedById, LastUpdated, LastUpdatedById, ReferenceTypeId FROM dbo.RatingTask WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_RatingTask OFF
GO
ALTER TABLE dbo.RatingContext
	DROP CONSTRAINT FK_RatingContext_RatingTask
GO
ALTER TABLE dbo.ClusterAnalysis
	DROP CONSTRAINT FK_ClusterAnalysis_RatingTask
GO
DROP TABLE dbo.RatingTask
GO
EXECUTE sp_rename N'dbo.Tmp_RatingTask', N'RatingTask', 'OBJECT' 
GO
ALTER TABLE dbo.RatingTask ADD CONSTRAINT
	PK_RatingTask PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.RatingTask ADD CONSTRAINT
	IX_RatingTask_RowId UNIQUE NONCLUSTERED 
	(
	RowId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.RatingTask ADD CONSTRAINT
	FK_RatingTask_ReferenceType FOREIGN KEY
	(
	ReferenceTypeId
	) REFERENCES dbo.[ConceptScheme.Concept]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingTask ADD CONSTRAINT
	FK_RatingTask_to_ReferenceResource FOREIGN KEY
	(
	ReferenceResourceId
	) REFERENCES dbo.ReferenceResource
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingContext ADD CONSTRAINT
	FK_RatingContext_RatingTask FOREIGN KEY
	(
	RatingTaskId
	) REFERENCES dbo.RatingTask
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RatingContext SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ClusterAnalysis ADD
	BilletTitleId int NULL,
	WorkRoleId int NULL
GO
ALTER TABLE dbo.ClusterAnalysis ADD CONSTRAINT
	FK_ClusterAnalysis_RatingTask FOREIGN KEY
	(
	HasRatingTaskId
	) REFERENCES dbo.RatingTask
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ClusterAnalysis ADD CONSTRAINT
	FK_ClusterAnalysis_Job FOREIGN KEY
	(
	BilletTitleId
	) REFERENCES dbo.Job
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ClusterAnalysis ADD CONSTRAINT
	FK_ClusterAnalysis_WorkRole FOREIGN KEY
	(
	WorkRoleId
	) REFERENCES dbo.WorkRole
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ClusterAnalysis SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
