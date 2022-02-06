USE [NavyRRL]
GO

--22-02-06 AspNetRoles - remove extra space
UPDATE [dbo].[AspNetRoles]
   SET [Name] = 'Rating Continuum Development Analyst'
 WHERE name = 'Rating Continuum Development  Analyst'
GO


