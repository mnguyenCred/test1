/****** Script for SelectTopNRows command from SSMS  ******/
Update [Navy_RRL_V2].[dbo].[ConceptScheme]
Set SchemaUri = 'navy:PayGradeLevel' WHERE SchemaUri = 'navy:RatingLevel'

Update [Navy_RRL_V2].[dbo].[ConceptScheme]
Set Name = 'US Navy Pay Grade Levels' WHERE SchemaUri = 'navy:PayGradeLevel'