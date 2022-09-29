USE [NavyRRL]
GO

/****** Object:  Table [dbo].[ApplicationRole]    Script Date: 9/22/2022 12:38:36 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ApplicationRole](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_ApplicationRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ApplicationRole] ADD  CONSTRAINT [DF_ApplicationRole_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO


--populate from [AspNetRoles]

INSERT INTO [dbo].[ApplicationRole]
           ([Id]
           ,[Name]
           ,[IsActive])
SELECT convert(int, [Id])
      ,[Name]
      ,[IsActive]
  FROM [NavyRRL].[dbo].[AspNetRoles]


