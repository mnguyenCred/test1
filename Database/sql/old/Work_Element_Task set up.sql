/****** Rating Level Task set up ******/

USE [NavyRRL]
GO
  SELECT 
[Work_Element_Task]    
,IndexIdentifier
,count(*) as ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [Work_Element_Task]
  ,IndexIdentifier having count(*) > 1





SELECT 
[Work_Element_Task]     
,IndexIdentifier
,count(*) as ttl
   
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [Source],[Work_Element_Task] having count(*) > 1

  SELECT 
[Work_Element_Task]     
,count(*) as ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [Work_Element_Task] having count(*) > 1


SELECT 
[Source],
[Work_Element_Task]     
,count(*) as ttl
	  --,[TaskNotes]
   --   ,[Task_Applicability]
   --   ,[Formal_Training_Gap]
   --   ,[CIN]
   --   ,[Course_Name]
   --   ,[Course_Type]
   --   ,[Curriculum_Control_Authority]
   --   ,[Life_Cycle_Control_Document]
   --   ,[Task_Statement]
   --   ,[Current_Assessment_Approach]

   
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [Source],[Work_Element_Task] having count(*) > 1

  
/*
SELECT 
IndexIdentifier, COUNT(*) as ttl
from [QM_RMTL_11232021]
group by IndexIdentifier having count(*) > 1
go

SELECT 
Unique_Identifier, COUNT(*) as ttl
from [QM_RMTL_11232021]
group by Unique_Identifier having count(*) > 1
go

*/

--CREATE  VIEW [dbo].RatingLevelTaskSummary
--AS

SELECT 
IndexIdentifier as TaskCodedNotation
,a.[RankId]
, isnull(c1.PrefLabel,'missing') As [Rank]
,a.[LevelId]
, isnull(c2.PrefLabel,'missing') As [Level]
,a.[RatingLevelTaskId]
,a.[FunctionalAreaId]
, isnull(b.name,'missing') As FunctionalArea
--,a.[Functional_Area]
,a.[SourceId]
, isnull(c.name,'missing') As Source
,c.SourceDate
--,a.[Source] as origSource
-- ,a.[Date_of_Source]
,a.[WorkElementTypeId]
, isnull(d.name,'missing') As WorkElementType
--,a.[Work_Element_Type]

,a.[Work_Element_Task] as RatingLevelTask
,a.TaskApplicabilityId
, isnull(e.PrefLabel,'missing') As TaskApplication
--,a.[Task_Applicability]
,a.FormalTrainingGapId
, isnull(f.PrefLabel,'missing') As FormalTrainingGap
--,a.[Formal_Training_Gap]
,a.CourseTaskId
,g.TaskStatement
,case when a.[TaskNotes] = 'N/A'  then NULL else a.[TaskNotes] end [TaskNotes]

	--     ,a.[RatingId]

   --   ,a.[BilletTitleId]  


   
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021] a
left join [ConceptScheme.Concept]	c1 on a.[RankId] = c1.Id
left join [ConceptScheme.Concept]	c2 on a.[LevelId] = c2.Id
left join FunctionalArea			b on a.FunctionalAreaId = b.Id
left join Source					c on a.SourceId = c.Id
left join WorkElementType			d on a.WorkElementTypeId = d.Id
left join [ConceptScheme.Concept]	e on a.TaskApplicabilityId = e.Id
left join [ConceptScheme.Concept]	f on a.FormalTrainingGapId = f.Id
Left Join [Course.Task]				g on a.CourseTaskId = g.Id


-- =======================================================
USE [NavyRRL]
GO


--***** there are some duplicate tasks, but different identifiers!
INSERT INTO [dbo].[RatingLevelTask]
           (
		   [CodedNotation],
		   [RankId]
           ,[LevelId]
           ,[FunctionalAreaId]
           ,[SourceId]
           ,[WorkElementTypeId]
           ,[WorkElementTask]
           ,[TaskApplicabilityId]
          -- ,[TaskStatusId]
           ,[FormalTrainingGapId]
           ,TrainingTaskId
           ,[Notes]
           --,[Created]
           --,[CreatedById]
           --,[LastUpdated]
           --,[LastUpdatedById]
		   )

SELECT 
IndexIdentifier as TaskCodedNotation
,a.[RankId]
,a.[LevelId]
,a.[FunctionalAreaId]
,a.[SourceId]
,a.[WorkElementTypeId]

,a.[Work_Element_Task] as RatingLevelTask
,a.TaskApplicabilityId
,a.FormalTrainingGapId
,a.CourseTaskId

,case when a.[TaskNotes] = 'N/A'  then NULL else a.[TaskNotes] end [TaskNotes]
   
 FROM [NavyRRL].[dbo].[QM_RMTL_11232021] a
 Left Join [RatingLevelTask] b on a.IndexIdentifier = b.CodedNotation
 where b.id is null 
 go

  Update dbo.[RatingLevelTask]
set CTID = 'ce-' + Lower(NewId())
where ISNULL(CTID,'') = ''
go

-- =======================================================

UPDATE [dbo].[QM_RMTL_11232021]
   SET [RatingLevelTaskId] = b.Id
--	select  a.IndexIdentifier, a.[Work_Element_Task], b.CodedNotation
from [QM_RMTL_11232021] a
inner join [RatingLevelTask] b on a.IndexIdentifier = b.CodedNotation 

GO

USE [NavyRRL]
GO

SELECT [Id]
      ,[CTID]
      ,[Name]
      ,[CodedNotation]
      ,[Description]
      ,[Version]
      ,[RatingUploadDate]
      ,[RatingPublicationDate]
      ,[Image]
      ,[CredentialRegistryId]
      ,[Created]
      ,[LastUpdated]
      ,[LastPublished]
      ,[RowId]
      ,[CredentialRegistryURI]
      ,[MainEntityOfPage]
  FROM [dbo].[Rating]

GO

