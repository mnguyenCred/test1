USE [NavyRRL]
GO

/****** Object:  Table [dbo].[AppFunctionPermission]    Script Date: 9/22/2022 12:36:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AppFunctionPermission](
	[ApplicationFunctionId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
	[CanCreate] [bit] NOT NULL,
	[CanRead] [bit] NOT NULL,
	[CanUpdate] [bit] NOT NULL,
	[CanDelete] [bit] NOT NULL,
 CONSTRAINT [PK_AppFunctionPermission] PRIMARY KEY CLUSTERED 
(
	[ApplicationFunctionId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[AppFunctionPermission] ADD  CONSTRAINT [DF_Table_1_Create]  DEFAULT ((0)) FOR [CanCreate]
GO

ALTER TABLE [dbo].[AppFunctionPermission] ADD  CONSTRAINT [DF_Table_1_Read]  DEFAULT ((0)) FOR [CanRead]
GO

ALTER TABLE [dbo].[AppFunctionPermission] ADD  CONSTRAINT [DF_Table_1_Update]  DEFAULT ((0)) FOR [CanUpdate]
GO

ALTER TABLE [dbo].[AppFunctionPermission] ADD  CONSTRAINT [DF_Table_1_Delete]  DEFAULT ((0)) FOR [CanDelete]
GO

ALTER TABLE [dbo].[AppFunctionPermission]  WITH CHECK ADD  CONSTRAINT [FK_AppFunctionPermission_ApplicationFunction] FOREIGN KEY([ApplicationFunctionId])
REFERENCES [dbo].[ApplicationFunction] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[AppFunctionPermission] CHECK CONSTRAINT [FK_AppFunctionPermission_ApplicationFunction]
GO

ALTER TABLE [dbo].[AppFunctionPermission]  WITH CHECK ADD  CONSTRAINT [FK_AppFunctionPermission_ApplicationRole] FOREIGN KEY([RoleId])
REFERENCES [dbo].[ApplicationRole] ([Id])
GO

ALTER TABLE [dbo].[AppFunctionPermission] CHECK CONSTRAINT [FK_AppFunctionPermission_ApplicationRole]
GO


