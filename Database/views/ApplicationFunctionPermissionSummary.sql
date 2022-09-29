/****** Script for SelectTopNRows command from SSMS  ******/

Alter View ApplicationFunctionPermissionSummary
AS
SELECT a.[Id]	as RoleId
      ,a.[Name] as UserRole
	  ,c.Name	as ApplicationFunction
	  , c.CodedNotation
	  --,b.Action
     --,a.[IsActive]
  FROM [dbo].ApplicationRole a
  inner join AppFunctionPermission b on a.Id = b.RoleId
  inner join ApplicationFunction c on b.ApplicationFunctionId = c.Id 
 go

 grant select on ApplicationFunctionPermissionSummary to public
 go