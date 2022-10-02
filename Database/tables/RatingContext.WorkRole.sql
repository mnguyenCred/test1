USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[RatingContext.WorkRole]    Script Date: 10/1/2022 8:21:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RatingContext.WorkRole](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[RatingContextId] [int] NOT NULL,
	[WorkRoleId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_RatingContext.WorkRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RatingContext.WorkRole] ADD  CONSTRAINT [DF_RatingContext.WorkRole_RowId]  DEFAULT (newid()) FOR [RowId]
GO

ALTER TABLE [dbo].[RatingContext.WorkRole] ADD  CONSTRAINT [DF_RatingContext.WorkRole_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[RatingContext.WorkRole]  WITH CHECK ADD  CONSTRAINT [FK_RatingContext.WorkRole_WorkRole] FOREIGN KEY([WorkRoleId])
REFERENCES [dbo].[WorkRole] ([Id])
GO

ALTER TABLE [dbo].[RatingContext.WorkRole] CHECK CONSTRAINT [FK_RatingContext.WorkRole_WorkRole]
GO

ALTER TABLE [dbo].[RatingContext.WorkRole]  WITH CHECK ADD  CONSTRAINT [FK_RatingContextWorkRole_TO_RatingContext] FOREIGN KEY([RatingContextId])
REFERENCES [dbo].[RatingContext] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RatingContext.WorkRole] CHECK CONSTRAINT [FK_RatingContextWorkRole_TO_RatingContext]
GO


