USE NavyRRL
GO

/****** Object:  Table [dbo].[System.ProxyCodes]    Script Date: 12/14/2021 1:21:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[System.ProxyCodes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProxyCode] [varchar](300) NOT NULL,
	[UserId] [int] NOT NULL,
	[IsIdentityProxy] [bit] NULL,
	[ProxyType] [varchar](50) NULL,
	[IsActive] [bit] NOT NULL,
	[Created] [datetime] NOT NULL,
	[ExpiryDate] [datetime] NOT NULL,
	[AccessDate] [datetime] NULL,
 CONSTRAINT [PK_System.ProxyCodes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[System.ProxyCodes] ADD  CONSTRAINT [DF_System.ProxyCodes_IsIdentityProxy]  DEFAULT ((0)) FOR [IsIdentityProxy]
GO

ALTER TABLE [dbo].[System.ProxyCodes] ADD  CONSTRAINT [DF_System.ProxyCodes_ProxyType]  DEFAULT ('Forgot Password') FOR [ProxyType]
GO

ALTER TABLE [dbo].[System.ProxyCodes] ADD  CONSTRAINT [DF_System.ProxyCodes_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[System.ProxyCodes] ADD  CONSTRAINT [DF_System.ProxyCodes_Created]  DEFAULT (getdate()) FOR [Created]
GO


