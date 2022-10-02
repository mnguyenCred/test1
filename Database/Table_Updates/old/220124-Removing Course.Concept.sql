USE [NavyRRL]
GO

/****** Object:  Table [dbo].[Course.Concept]    Script Date: 1/24/2022 12:45:16 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Course.Concept]') AND type in (N'U'))
DROP TABLE [dbo].[Course.Concept]
GO
