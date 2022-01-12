USE [NavyRRL]
GO

/****** Object:  Table [dbo].[ReferenceResource.ReferenceType]    Script Date: 1/12/2022 12:32:04 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ReferenceResource.ReferenceType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[ReferenceResourceId] [int] NOT NULL,
	[ReferenceTypeId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
 CONSTRAINT [PK_ReferenceResource.ReferenceType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ReferenceResource.ReferenceType] ADD  CONSTRAINT [DF_ReferenceResource.ReferenceType_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[ReferenceResource.ReferenceType] ADD  CONSTRAINT [DF_ReferenceResource.ReferenceType_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[ReferenceResource.ReferenceType]  WITH CHECK ADD  CONSTRAINT [FK_ReferenceResource.ReferenceType_ConceptScheme.Concept] FOREIGN KEY([ReferenceTypeId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[ReferenceResource.ReferenceType] CHECK CONSTRAINT [FK_ReferenceResource.ReferenceType_ConceptScheme.Concept]
GO

ALTER TABLE [dbo].[ReferenceResource.ReferenceType]  WITH CHECK ADD  CONSTRAINT [FK_ReferenceResource.ReferenceType_ReferenceResource] FOREIGN KEY([ReferenceResourceId])
REFERENCES [dbo].[ReferenceResource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ReferenceResource.ReferenceType] CHECK CONSTRAINT [FK_ReferenceResource.ReferenceType_ReferenceResource]
GO
--populate from existing ReferenceResource
INSERT INTO [dbo].[ReferenceResource.ReferenceType]
           (	
		   [ReferenceResourceId]
           ,[ReferenceTypeId]
           ,[Created])

SELECT [Id] ,[ReferenceType]   ,[Created]
    
      --,[Name]
      --,[Description]
      --,[CodedNotation]
      --,[PublicationDate]
      --,[Note]
      --,[CTID]
  FROM [dbo].[ReferenceResource]

GO

