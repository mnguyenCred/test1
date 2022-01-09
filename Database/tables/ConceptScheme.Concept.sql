USE [NavyRRL]
GO

/****** Object:  Table [dbo].[ConceptScheme.Concept]    Script Date: 1/8/2022 8:46:23 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ConceptScheme.Concept](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ConceptSchemeId] [int] NOT NULL,
	[Label] [varchar](1000) NOT NULL,
	[CTID] [varchar](50) NULL,
	[CodedNotation] [varchar](50) NULL,
	[AlternateLabel] [varchar](500) NULL,
	[Description] [varchar](max) NULL,
	[ListId] [int] NULL,
	[Created] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_ConceptScheme.Concept] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ConceptScheme.Concept] ADD  CONSTRAINT [DF_Table_1_SortOrder]  DEFAULT ((25)) FOR [ListId]
GO

ALTER TABLE [dbo].[ConceptScheme.Concept] ADD  CONSTRAINT [DF_ConceptScheme.Concept_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[ConceptScheme.Concept] ADD  CONSTRAINT [DF_ConceptScheme.Concept_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[ConceptScheme.Concept]  WITH CHECK ADD  CONSTRAINT [FK_ConceptScheme.Concept_ConceptScheme] FOREIGN KEY([ConceptSchemeId])
REFERENCES [dbo].[ConceptScheme] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ConceptScheme.Concept] CHECK CONSTRAINT [FK_ConceptScheme.Concept_ConceptScheme]
GO


