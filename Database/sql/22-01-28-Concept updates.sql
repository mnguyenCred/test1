USE [NavyRRL]
GO
--22-01-28-Concept updates
UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'All Sailor Task (Ships)'
 WHERE name = 'All Sailor Task (ships)'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Seaman Recruit'
 WHERE CodedNotation = 'E1'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Seaman Apprentice'
 WHERE CodedNotation = 'E2'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Seaman'
 WHERE CodedNotation = 'E3'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Petty Officer Third Class'
 WHERE CodedNotation = 'E4'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Petty Officer Second Class'
 WHERE CodedNotation = 'E5'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Petty Officer First Class'
 WHERE CodedNotation = 'E6'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Chief Petty Officer'
 WHERE CodedNotation = 'E7'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Senior Chief Petty Officer'
 WHERE CodedNotation = 'E8'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [Name] = 'Master Chief Petty Officer'
 WHERE CodedNotation = 'E9'
GO

