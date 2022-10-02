USE [Navy_RRL_V2]
GO
--	22-10-02 Populate ApplicationUserRole
INSERT INTO [dbo].[ApplicationUserRole]
           ([UserId]
           ,[RoleId]
           ,[Created])

SELECT        
	u.Id, aur.RoleId, getdate()
	--,u.Email, 
	--u.SortName,
	--ar.Name As RoleName, 
	--u.Created, 
	
	--u.AspNetId 
	
FROM dbo.AspNetRoles ar
INNER JOIN dbo.AspNetUserRoles aur ON ar.Id = aur.RoleId 
INNER JOIN dbo.Account_Summary u ON aur.UserId = u.AspNetId


