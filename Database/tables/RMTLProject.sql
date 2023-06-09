USE [Navy_RRL_V2]
GO
/****** Object:  Table [dbo].[RMTLProject]    Script Date: 10/2/2022 9:20:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RMTLProject](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CTID] [varchar](50) NULL,
	[Name] [nvarchar](300) NOT NULL,
	[RatingId] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[StatusId] [int] NULL,
	[VersionControlIdentifier] [varchar](50) NULL,
	[Notes] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
	[LastApproved] [datetime] NULL,
	[LastApprovedById] [int] NULL,
	[LastPublished] [datetime] NULL,
	[LastPublishedById] [int] NULL,
 CONSTRAINT [PK_RMTLProject] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[RMTLProject] ADD  CONSTRAINT [DF_RMTLProject_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[RMTLProject] ADD  CONSTRAINT [DF_RMTLProject_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[RMTLProject] ADD  CONSTRAINT [DF_Table_1_Created1_1]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[RMTLProject] ADD  CONSTRAINT [DF_Table_1_Created2]  DEFAULT (getdate()) FOR [LastApproved]
GO
ALTER TABLE [dbo].[RMTLProject] ADD  CONSTRAINT [DF_Table_1_LastApproved1]  DEFAULT (getdate()) FOR [LastPublished]
GO
