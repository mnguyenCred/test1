USE NavyRRL
GO

/****** Object:  View [dbo].[AspNetUserRoles_Summary]    Script Date: 12/7/2021 10:21:18 AM ******/
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
  FROM [dbo].[AspNetUserRoles_Summary]
order by sortName, RoleName



*/
CREATE VIEW [dbo].[AspNetUserRoles_Summary]
AS
SELECT        
	u.Id, 
	u.Email, 
	u.SortName,
	ar.Name As RoleName, aur.RoleId, 
	u.Created, 
	
	u.AspNetId 
	
FROM dbo.AspNetRoles ar
INNER JOIN dbo.AspNetUserRoles aur ON ar.Id = aur.RoleId 
INNER JOIN dbo.Account_Summary u ON aur.UserId = u.AspNetId

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

grant select on [AspNetUserRoles_Summary] to public
go

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[41] 4[31] 2[10] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Account_Summary"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 299
               Right = 227
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "AspNetUserRoles"
            Begin Extent = 
               Top = 28
               Left = 326
               Bottom = 221
               Right = 496
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "AspNetRoles"
            Begin Extent = 
               Top = 34
               Left = 650
               Bottom = 193
               Right = 820
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 2055
         Width = 945
         Width = 1500
         Width = 2160
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 1500
         Table = 1815
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'AspNetUserRoles_Summary'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'AspNetUserRoles_Summary'
GO


