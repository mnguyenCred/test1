USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[APIKey]    Script Date: 2/16/2023 4:46:28 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[APIKey](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[API_Key] [uniqueidentifier] NOT NULL,
	[Created] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_APIKey] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[APIKey] ADD  CONSTRAINT [DF_APIKey_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[APIKey] ADD  CONSTRAINT [DF_APIKey_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[APIKey] ADD  CONSTRAINT [DF_APIKey_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[APIKey]  WITH CHECK ADD  CONSTRAINT [FK_APIKey_Account] FOREIGN KEY([UserId])
REFERENCES [dbo].[Account] ([Id])
GO

ALTER TABLE [dbo].[APIKey] CHECK CONSTRAINT [FK_APIKey_Account]
GO


