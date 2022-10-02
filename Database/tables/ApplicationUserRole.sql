USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[ApplicationUserRole]    Script Date: 10/2/2022 9:57:18 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ApplicationUserRole](
	[UserId] int NOT NULL,
	[RoleId] int NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_dbo.ApplicationUserRole] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ApplicationUserRole] ADD  CONSTRAINT [DF_ApplicationUserRole_Created]  DEFAULT (getdate()) FOR [Created]
GO
-- =====================================
ALTER TABLE [dbo].[ApplicationUserRole]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationUserRole_ApplicationRole] FOREIGN KEY([RoleId])
REFERENCES [dbo].[ApplicationRole] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ApplicationUserRole] CHECK CONSTRAINT [FK_ApplicationUserRole_ApplicationRole]
GO
-- =====================================
ALTER TABLE [dbo].[ApplicationUserRole]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationUserRole_Account] FOREIGN KEY([UserId])
REFERENCES [dbo].Account ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ApplicationUserRole] CHECK CONSTRAINT [FK_ApplicationUserRole_Account]
GO


