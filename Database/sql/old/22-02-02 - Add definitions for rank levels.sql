  USE [NavyRRL]
GO
/****** 22-02-02 - Add definitions for rank levels ******/
SELECT TOP (1000) [Id]
      ,[ConceptSchemeId]
      ,[Name]
      ,[CTID]
      ,[CodedNotation]
      ,[AlternateLabel]
      ,[ListId]
      ,[IsActive]

      ,[WorkElementType]
  FROM [NavyRRL].[dbo].[ConceptScheme.Concept]
  go


UPDATE [dbo].[ConceptScheme.Concept]
   SET [Description] = 'A Sailor in pay grades E5-E6 who have progressed from the apprentice-level. Journeyman-level personnel have acquired the necessary knowledge and skills to complete tasks and work assignments without supervision. Journeyman-level Sailors typically hold Naval Enlisted Classification (NEC) codes indicating proficiency with specific skills across various systems and equipment. Journeyman-level Sailors provide the direct oversight and provide guidance and support to apprentice-level Sailors. When required Journeyman-level Sailors will seek guidance, direction, and mentoring from master-level Sailors. Typically Journeyman-Level personnel execute work plans and schedules approved by Master-level Sailors.'

 WHERE name = 'Journeyman'
GO
UPDATE [dbo].[ConceptScheme.Concept]
   SET [Description] = 'A Sailor in pay grades E1-E4 who provides assistance to a skilled worker (journeyman) in order to learn the required skills of their rating. As the apprentice gains skills they can perform routine tasks under limited supervision and more complete apprentice level tasks with direct supervision.'

 WHERE name = 'Apprentice'
GO
UPDATE [dbo].[ConceptScheme.Concept]
   SET [Description] = 'A Sailor in pay grades E7-E9 who have progressed from the journeyman-level and provide guidance, direction, supervision, and mentoring to both Journeyman-level and Apprentice-level Sailors. Master-level Sailors develop long-range plans for qualifications, work schedules, maintenance plans, and interface with department senior management for interdepartmental coordination efforts.'

 WHERE name = 'Master'
GO

