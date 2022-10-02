USE [NavyRRL]
GO
--updates for spelling and potential update related to PQS
--======================================
--redo

UPDATE [dbo].[ConceptScheme.Concept]
   SET [WorkElementType] = 'NEC',
   name = 'Navy Enlisted Classification'
 WHERE name = 'Navy Enlisted Classifications'
GO

UPDATE [dbo].[ConceptScheme.Concept]
   SET [WorkElementType] = 'Naval Standard Task',
   name = 'Navy Standard'
 WHERE name = 'Navy Standards'
GO

/**/
UPDATE [dbo].[ConceptScheme.Concept]
   SET [WorkElementType] = '300 Series PQS Watch Station',
   Description='Standards from NAVEDTRA 43492'
   ,Name = '300 Series PQS Watch Station'
   ,CodedNotation='PQS'
 WHERE name = 'Navy Education and Training'
GO



