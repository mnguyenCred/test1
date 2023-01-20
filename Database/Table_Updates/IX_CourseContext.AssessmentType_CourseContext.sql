USE [Navy_RRL_V2]
GO
CREATE NONCLUSTERED INDEX [IX_CourseContext.AssessmentType_CourseContext]
ON [dbo].[CourseContext.AssessmentType] ([CourseContextId])
INCLUDE ([AssessmentMethodConceptId])
GO
