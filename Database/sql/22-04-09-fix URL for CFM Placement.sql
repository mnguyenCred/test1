USE [NavyRRL]
GO
--22-04-09-fix URL for CFM Placement
UPDATE [dbo].[ConceptScheme]
   SET [SchemaUri] = 'navy:CFMPlacement'
 WHERE Name = 'CFM Placement'
GO


