USE [NavyRRL]
GO

/****** Object:  View [dbo].[RmtlSummary]    Script Date: 12/12/2021 8:04:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
/*

USE [NavyRRL]
GO

SELECT [RMTLProejct]
      ,[Rating]
      ,[Rank]
      ,[Level]
      ,[BilletTitle]
      ,[FunctionalArea]
      ,[Source]
      ,[SourceDate]
      ,[WorkElementType]
      ,[WorkElementTask]
      ,[TaskApplicability]
      ,[FormalTrainingGap]
      ,[CIN]
      ,[CourseName]
      ,[CourseType]
      ,[TaskStatement]
      ,[CurrentAssessmentApproach]
      ,[CurriculumControlAuthority]
      ,[LifeCycleControlDocument]
  FROM [dbo].[RmtlSummary]

GO



*/
ALTER VIEW [dbo].[RmtlSummary]
AS
SELECT       
	a.Name AS RMTLProejct
	, b.Name AS Rating
	, f.Rank, f.[Level]
	, d.Name AS BilletTitle
	, f.FunctionalArea
	, f.Source, f.SourceDate
	, f.WorkElementType, f.WorkElementTask
	, f.TaskApplicability
	, f.FormalTrainingGap
	, f.CIN, f.CourseName
    , f.CourseType
	, f.TaskStatement
,f.CurrentAssessmentApproach
,f.CurriculumControlAuthority
,f.LifeCycleControlDocument
FROM            dbo.RmtlProjectBilletTask AS e 
INNER JOIN dbo.RatingLevelTaskSummary AS f ON e.RatingLevelTaskId = f.Id 
INNER JOIN dbo.RMTLProject AS a 
		INNER JOIN dbo.[RmtlProject.Billet] AS c ON a.Id = c.RmtlProjectId 
		INNER JOIN dbo.Job AS d ON c.JobId = d.Id 
		INNER JOIN dbo.Rating AS b ON a.RatingId = b.Id ON e.ProjectBilletId = c.Id
GO

