USE [Navy_RRL_V2]
GO

/****** Object:  View [dbo].[ApplicationUserRoleFunctionSummary]   Script Date: 12/28/2022 3:35:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/******  ******/

CREATE View [dbo].[ApplicationUserRoleFunctionSummary]
AS
SELECT a.Email
	,a.FirstName + a.LastName as UserName
	,aur.UserId	
	,a.[Id]	as RoleId
	,ar.[Name] as UserRole
	,c.Name	as ApplicationFunction
	,c.CodedNotation
	--,b.Action
	--,a.[IsActive]
  FROM Account a
  Inner Join ApplicationUserRole aur on a.id = aur.UserId
  inner join [dbo].ApplicationRole ar on aur.RoleId = ar.Id
  inner join AppFunctionPermission b on ar.Id = b.RoleId
  inner join ApplicationFunction c on b.ApplicationFunctionId = c.Id 
GO
grant select on [ApplicationUserRoleFunctionSummary]to public
go

