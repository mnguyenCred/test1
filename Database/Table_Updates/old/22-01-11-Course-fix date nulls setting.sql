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
ALTER TABLE dbo.Course
	DROP CONSTRAINT DF_Course_RowId
GO
ALTER TABLE dbo.Course
	DROP CONSTRAINT DF_Course_Created
GO
ALTER TABLE dbo.Course
	DROP CONSTRAINT DF_Course_Created1
GO
CREATE TABLE dbo.Tmp_Course
	(
	Id int NOT NULL IDENTITY (1, 1),
	RowId uniqueidentifier NOT NULL,
	CIN nvarchar(50) NOT NULL,
	Name nvarchar(300) NOT NULL,
	Description nvarchar(MAX) NULL,
	CourseTypeId int NULL,
	LifeCycleControlDocumentId int NULL,
	AssessmentApproachId int NULL,
	CurriculumControlAuthorityId int NULL,
	CTID varchar(50) NULL,
	Created datetime NOT NULL,
	CreatedById int NULL,
	LastUpdated datetime NOT NULL,
	LastUpdatedById int NULL,
	CourseType nvarchar(50) NULL,
	LifeCycleControlDocument nvarchar(100) NULL,
	CurriculumControlAuthority nvarchar(200) NULL,
	CurrentAssessmentApproach nvarchar(200) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_Course SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_Course ADD CONSTRAINT
	DF_Course_RowId DEFAULT (newid()) FOR RowId
GO
ALTER TABLE dbo.Tmp_Course ADD CONSTRAINT
	DF_Course_Created DEFAULT (getdate()) FOR Created
GO
ALTER TABLE dbo.Tmp_Course ADD CONSTRAINT
	DF_Course_Created1 DEFAULT (getdate()) FOR LastUpdated
GO
SET IDENTITY_INSERT dbo.Tmp_Course ON
GO
IF EXISTS(SELECT * FROM dbo.Course)
	 EXEC('INSERT INTO dbo.Tmp_Course (Id, RowId, CIN, Name, Description, CourseTypeId, LifeCycleControlDocumentId, AssessmentApproachId, CurriculumControlAuthorityId, CTID, Created, CreatedById, LastUpdated, LastUpdatedById, CourseType, LifeCycleControlDocument, CurriculumControlAuthority, CurrentAssessmentApproach)
		SELECT Id, RowId, CIN, Name, Description, CourseTypeId, LifeCycleControlDocumentId, AssessmentApproachId, CurriculumControlAuthorityId, CTID, Created, CreatedById, LastUpdated, LastUpdatedById, CourseType, LifeCycleControlDocument, CurriculumControlAuthority, CurrentAssessmentApproach FROM dbo.Course WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_Course OFF
GO
ALTER TABLE dbo.[Course.Task]
	DROP CONSTRAINT [FK_Course.Task_Course]
GO
DROP TABLE dbo.Course
GO
EXECUTE sp_rename N'dbo.Tmp_Course', N'Course', 'OBJECT' 
GO
ALTER TABLE dbo.Course ADD CONSTRAINT
	PK_Course PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
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