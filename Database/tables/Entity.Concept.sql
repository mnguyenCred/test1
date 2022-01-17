USE [NavyRRL]
GO

/****** Object:  Table [dbo].[Entity.Concept]    Script Date: 1/16/2022 5:20:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Entity.Concept](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[EntityId] [int] NOT NULL,
	[ConceptId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Entity.Concept] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Entity.Concept] ADD  CONSTRAINT [DF_Entity.Concept_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[Entity.Concept] ADD  CONSTRAINT [DF_Entity.Concept_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Entity.Concept]  WITH CHECK ADD  CONSTRAINT [FK_Entity.Concept_ConceptScheme.Concept] FOREIGN KEY([ConceptId])
REFERENCES [dbo].[ConceptScheme.Concept] ([Id])
GO

ALTER TABLE [dbo].[Entity.Concept] CHECK CONSTRAINT [FK_Entity.Concept_ConceptScheme.Concept]
GO

ALTER TABLE [dbo].[Entity.Concept]  WITH CHECK ADD  CONSTRAINT [FK_Entity.Concept_Entity] FOREIGN KEY([EntityId])
REFERENCES [dbo].[Entity] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Entity.Concept] CHECK CONSTRAINT [FK_Entity.Concept_Entity]
GO


