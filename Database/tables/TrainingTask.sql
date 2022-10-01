USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[TrainingTask]    Script Date: 10/1/2022 6:42:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TrainingTask](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[CTID] [varchar](50) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_TrainingTask] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_TrainingTask_CTID] UNIQUE NONCLUSTERED 
(
	[CTID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_TrainingTask_RowId] UNIQUE NONCLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[TrainingTask] ADD  CONSTRAINT [DF_TrainingTask_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[TrainingTask] ADD  CONSTRAINT [DF_TrainingTask_CTID]  DEFAULT ('ce-'+lower(newid())) FOR [CTID]
GO

ALTER TABLE [dbo].[TrainingTask] ADD  CONSTRAINT [DF_TrainingTask_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[TrainingTask] ADD  CONSTRAINT [DF_TrainingTask_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO


