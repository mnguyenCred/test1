use NavyRRL
go


/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RatingTask ADD
	ReferenceResourceId int NULL
GO
ALTER TABLE dbo.RatingTask SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO

-- Populate
USE [NavyRRL]
GO

UPDATE [dbo].[RatingTask]
   SET [ReferenceResourceId] = b.id 

--	select a.Id, a.SourceId, b.Name, c.Id, c.Name
from [RatingTask] a
inner join Source b on a.sourceId = b.Id
left join ReferenceResource c on b.name  = c.Name

GO



