USE [NavyRRL]
GO

/****** Object:  Table [dbo].[ReferenceResource]    Script Date: 1/12/2022 10:57:10 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ReferenceResource](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](300) NOT NULL,
	[Description] [varchar](max) NULL,
	[CodedNotation] [varchar](100) NULL,
	[PublicationDate] [nvarchar](50) NULL,
	[SubjectWebpage] [varchar](500) NULL,
	[StatusType] [int] NULL,
	[VersionIdentifier] [varchar](100) NULL,
	[ReferenceType] [int] NULL,
	[Note] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
	[CTID] [varchar](50) NULL,
 CONSTRAINT [PK_ReferenceResource] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ReferenceResource] ADD  CONSTRAINT [DF_ReferenceResource_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[ReferenceResource] ADD  CONSTRAINT [DF_ReferenceResource_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[ReferenceResource] ADD  CONSTRAINT [DF_ReferenceResource_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

USE [NavyRRL]
GO
--populate ReferenceResource
USE [NavyRRL]
GO
--populate ReferenceResource
INSERT INTO [dbo].[ReferenceResource]
           ( [Name]
           ,[Description]
           ,[PublicationDate]
           ,[ReferenceType]
           ,[Created]
           ,[LastUpdated]
           ,[CTID])

SELECT [Name]
      ,[Description]      
	  ,[SourceDate]
	  ,1111
      ,[Created]
      ,[LastUpdated]
      ,[CTID]
  FROM [dbo].[Source]
  where name like 'navedtra%'

GO

INSERT INTO [dbo].[ReferenceResource]
           ( [Name]
           ,[Description]
           ,[PublicationDate]
           ,[ReferenceType]
           ,[Created]
           ,[LastUpdated]
           ,[CTID])

SELECT [Name]
      ,[Description]      
	  ,[SourceDate]
	  ,1112
      ,[Created]
      ,[LastUpdated]
      ,[CTID]
  FROM [dbo].[Source]
  where name like 'pms%'

GO


INSERT INTO [dbo].[ReferenceResource]
           ( [Name]
           ,[Description]
           ,[PublicationDate]
           ,[ReferenceType]
           ,[Created]
           ,[LastUpdated]
           ,[CTID])

SELECT [Name]
      ,[Description]      
	  ,[SourceDate]
	  ,1109
      ,[Created]
      ,[LastUpdated]
      ,[CTID]
  FROM [dbo].[Source]
  where name = 'NAVPERS 18068F Vol I'

GO
INSERT INTO [dbo].[ReferenceResource]
           ( [Name]
           ,[Description]
           ,[PublicationDate]
           ,[ReferenceType]
           ,[Created]
           ,[LastUpdated]
           ,[CTID])

SELECT [Name]
      ,[Description]      
	  ,[SourceDate]
	  ,1110
      ,[Created]
      ,[LastUpdated]
      ,[CTID]
  FROM [dbo].[Source]
  where name = 'NAVPERS 18068F Vol II'

GO
--remainder are MIP
INSERT INTO [dbo].[ReferenceResource]
           ( [Name]
           ,[Description]
           ,[PublicationDate]
           ,[ReferenceType]
           ,[Created]
           ,[LastUpdated]
           ,[CTID])

SELECT a.[Name]
      ,a.[Description]      
	  ,a.[SourceDate]
	  ,1112
      ,a.[Created]
      ,a.[LastUpdated]
      ,a.[CTID]
  FROM [dbo].[Source] a
  left join [ReferenceResource] b on a.Name = b.Name
  where b.id is null 

GO
--now LCCD???
INSERT INTO [dbo].[ReferenceResource]
           ( [Name], CodedNotation
           ,[Description]
           ,[ReferenceType]
           ,[Created]
           ,[LastUpdated]
           ,[CTID])

SELECT  Label,  CodedNotation,Description, 1123
, GETDATE(), GETDATE(), 'ce-' + lower(newId())
FROM            [ConceptScheme.Concept]
WHERE        (ConceptSchemeId = 17)
ORDER BY ConceptSchemeId, Label





