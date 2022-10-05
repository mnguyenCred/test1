USE [Navy_RRL_V2]
GO

/****** Object:  Table [dbo].[RatingContext.WorkRole]    Script Date: 10/5/2022 6:16:55 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RatingContext.WorkRole]') AND type in (N'U'))
DROP TABLE [dbo].[RatingContext.WorkRole]
GO


