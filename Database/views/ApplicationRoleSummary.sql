USE [Navy_RRL_V2]
GO

/****** Object:  View [dbo].[ApplicationRoleSummary]    Script Date: 12/7/2021 10:21:18 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/*
USE [CTI]
GO

SELECT [Id],[SortName]  ,[RoleName]

      ,[Email]
      
      ,[Created]
      ,[RoleId]
       ,[AspNetId]   
  FROM [dbo].[ApplicationRoleSummary]
order by sortName, RoleName



*/
ALTER VIEW [dbo].[ApplicationRoleSummary]
AS
SELECT        
	u.Id, 
	u.Email, 
	u.SortName,
	ar.Name As RoleName, aur.RoleId, 
	u.Created, 
	
	u.AspNetId 
	
FROM dbo.ApplicationRole ar
INNER JOIN dbo.ApplicationUserRole aur ON ar.Id = aur.RoleId 
INNER JOIN dbo.Account_Summary u ON aur.UserId = u.Id

--CROSS APPLY (
--    SELECT '''' + convert(varchar,ar.Name) + ''', '
--   -- ,rsub.ResourceId
--    FROM dbo.AspNetRoles ar
--	INNER JOIN dbo.AspNetUserRoles aur ON ar.Id = aur.RoleId  
--    WHERE (base.IsActive = 1) 
--	AND base.[AspNetId] = aur.UserId
--    FOR XML Path('') 
--) D (Roles)


--CROSS APPLY (
--    SELECT ar2.Id + ', '
--   -- ,rsub.ResourceId
--    FROM dbo.AspNetRoles ar2
--	INNER JOIN dbo.AspNetUserRoles aur2 ON ar2.Id = aur2.RoleId  
--    WHERE (base.IsActive = 1) 
--	AND base.[AspNetId] = aur2.UserId
--    FOR XML Path('') 
--) R (RoleIds)


GO

grant select on [ApplicationRoleSummary] to public
go

