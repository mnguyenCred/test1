USE [Navy_RRL_V2]
GO
--update applicationFunction codedNotation
UPDATE [dbo].[ApplicationFunction]
   SET [CodedNotation] = replace(CodedNotation,'rmtl_','rmtl.')
 WHERE[CodedNotation] like 'rmtl_%'
GO

UPDATE [dbo].[ApplicationFunction]
   SET [CodedNotation] = replace(CodedNotation,'task_','task.')
 WHERE[CodedNotation] like 'task_%'
GO

UPDATE [dbo].[ApplicationFunction]
   SET [CodedNotation] = replace(CodedNotation,'ratingtask_','ratingTask.')
 WHERE[CodedNotation] like 'ratingtask_%'
GO


UPDATE [dbo].[ApplicationFunction]
   SET [CodedNotation] = replace(CodedNotation,'trainingtask_','trainingTask.')
 WHERE[CodedNotation] like 'trainingtask_%'
GO



UPDATE [dbo].[ApplicationFunction]
   SET [CodedNotation] = replace(CodedNotation,'trainingtasksolution_','trainingTaskSolution.')
 WHERE[CodedNotation] like 'trainingtaskSolution_%'
GO
