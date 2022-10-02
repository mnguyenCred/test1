USE [NavyRRL]
GO
--update to use Organization instead of CurriculumControlAuthority
UPDATE [dbo].[Course]
   SET [CurriculumControlAuthorityId] = [CurriculumControlAuthorityId] + 1
 WHERE [CurriculumControlAuthorityId] < 20
GO


