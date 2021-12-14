/****** Course  ******/

/*
reset
USE [NavyRRL]
GO

DELETE FROM [dbo].[Course.Task]

DELETE FROM [dbo].[Course]

truncate table dbo.[Course.Task]   
truncate table dbo.[Course]  

DBCC CHECKIDENT ('[Course]', RESEED, 0);
DBCC CHECKIDENT ('[Course.Task]', RESEED, 0);

*/


-- =======================================
-- duplicates checking
SELECT distinct  a.[CIN], a.[Course_Name]
FROM [dbo].[QM_RMTL_11232021] a
inner join (
SELECT 
      allCourses.[CIN]
     -- ,allCourses.[Course_Name]
	  ,count(*) ttl
  FROM 
	(	SELECT distinct  [CIN], [Course_Name]
		FROM [dbo].[QM_RMTL_11232021]
		WHERE CIN <> 'N/A'
	 -- order by 1,2
	) allCourses
  group by 
   [CIN]
    --  ,[Course_Name]
	having count(*) > 1
) dups on a.CIN = dups.CIN


-- =======================================
USE [NavyRRL]
GO

INSERT INTO [dbo].[Course]
           ([CIN]
           ,Name
		 --  ,Description
		 --  ,CTID
           --,[CourseTypeId]
           --,[LifeCycleControlDocumentId]
           ,[Created]
           --,[CreatedById]
           ,[LastUpdated]
           --,[LastUpdatedById]
		   ,CurrentAssessmentApproach
           ,[CourseType]
           ,[LifeCycleControlDocument]
           ,[CurriculumControlAuthority]
		   )

SELECT distinct 
base.[CIN]
      ,base.[Course_Name]
	 -- , '' As description
	 --do CTID separately or will mess up the distinct list
	 -- ,'ce-' + Lower(NewId()) as CTID
	  , getdate() as DateCreated, getdate() as DateModified 
	  ,base.[Current_Assessment_Approach]
	  ,CASE
          WHEN CourseTypes IS NULL THEN ''
          WHEN len(CourseTypes) = 0 THEN ''
          ELSE left(CourseTypes,len(CourseTypes)-1)
		END AS CourseTypes
	  ,CASE
          WHEN LCCDTypes IS NULL THEN ''
          WHEN len(LCCDTypes) = 0 THEN ''
          ELSE left(LCCDTypes,len(LCCDTypes)-1)
		  END AS LCCDTypes
	  ,CASE
          WHEN CCATypes IS NULL THEN ''
          WHEN len(CCATypes) = 0 THEN ''
          ELSE left(CCATypes,len(CCATypes)-1)
		  END AS CCATypes

  FROM [dbo].[QM_RMTL_11232021] base
  left join course b on base.CIN = b.CIN

CROSS APPLY (
    SELECT distinct rmtl.Course_Type  + ' | '
    FROM dbo.[QM_RMTL_11232021] rmtl  
    WHERE CIN <> 'N/A'
	and base.CIN = rmtl.CIN
    FOR XML Path('') ) ctCodes (CourseTypes)

CROSS APPLY (
    SELECT distinct rmtl.Curriculum_Control_Authority  + ' | '
    FROM dbo.[QM_RMTL_11232021] rmtl  
    WHERE CIN <> 'N/A'
	and base.CIN = rmtl.CIN
    FOR XML Path('') ) ccaCodes (CCATypes)

CROSS APPLY (
    SELECT distinct rmtl.Life_Cycle_Control_Document  + ' | '
    FROM dbo.[QM_RMTL_11232021] rmtl  
    WHERE CIN <> 'N/A'
	and base.CIN = rmtl.CIN
    FOR XML Path('') ) lccdCodes (LCCDTypes)
-- -----------------------------
    where base.CIN <> 'N/A'
	and base.Course_Name <> 'Introduction to Hazardous Waste Generation and Handling Temp Webinar Covid'
	and base.Course_Name <>'Introduction to Hazardous Waste Generation and Handling Temp Webinar COVID / Introduction to Hazardous Materials Ashore Global Online'
	and b.id is  null
  order by 1,2
  go

 Update dbo.Course
set CTID = 'ce-' + Lower(NewId())
where ISNULL(CTID,'') = ''
go

USE [NavyRRL]
GO

UPDATE [dbo].[QM_RMTL_11232021]
   SET [CourseId] = b.id
from [dbo].[QM_RMTL_11232021] a
inner join course b on a.CIN = b.CIN 
--and a.Course_Name = b.CourseName

GO
-- =================================================================
SELECT 
      distinct 
	  [CourseId]
      ,[CIN]
      ,[Course_Name]
     -- ,[Course_Type]
	 ,Current_Assessment_Approach
      ,[Curriculum_Control_Authority]
      ,[Life_Cycle_Control_Document]

  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  where courseid not in (9, 10)
  order by 2
-- =================================================================
--UPDATE [dbo].[Course]
--   CourseType=''SET 
--   ,[LifeCycleControlDocument] = <LifeCycleControlDocument, nvarchar(100),>
--      ,[CurriculumControlAuthority] = <CurriculumControlAuthority, nvarchar(200),>
--      ,[CurrentAssessmentApproach] = <CurrentAssessmentApproach, nvarchar(200),>
--from [dbo].[Course] a
--inner join [dbo].[QM_RMTL_11232021] b on a.name = b.[Course_Name]
-- WHERE <Search Conditions,,>
--GO


-- =================================================================
USE [NavyRRL]
GO

INSERT INTO [dbo].[Course.Task]
           ([CourseId]
           ,[TaskStatement]
		   --,CTID
           --,[AssessmentApproachId]
           --,[CourseTypeId]
           --,[Created]
           --,[CreatedById]
           --,[LastUpdated]
           --,[LastUpdatedById]
		   )

SELECT distinct 
--[CIN]
--      ,[Course_Name]
a.CourseId
	   ,[Task_Statement]
	 --  ,'ce-' + Lower(NewId())
     -- ,[Course_Type]

      --,[NEC_Refresher_Training_Selection]
      --,[Journeyman_Core_Training_Selection]
      --,[Master_Core_Training_Selection]
      --,[Notes]
      --,[AssessmentId]
      --,[CourseId]
      --,[CourseTaskId]
from [dbo].[QM_RMTL_11232021] a
left join [Course.Task] b on a.Task_Statement = b.TaskStatement
--inner join course b on a.courseId = b.Id and a.Course_Name = b.CourseName
    where a.CIN <> 'N/A'
	and b.id is null 
--642
go

Update dbo.[Course.Task]
set CTID = 'ce-' + Lower(NewId())
where ISNULL(CTID,'') = ''
go

--,[Course_Type]	  ,[Life_Cycle_Control_Document]      ,[Curriculum_Control_Authority]          ,[Current_Assessment_Approach] 
UPDATE [dbo].[QM_RMTL_11232021]
   SET CourseTaskId = b.id
--	select distinct a.CourseId,CourseTaskId, [Task_Statement]      
from [dbo].[QM_RMTL_11232021] a
inner join [Course.Task] b on a.CourseId = b.CourseId 
and a.[Task_Statement]= b.TaskStatement
	--and a.[Course_Type] = b.[CourseType]

	--and a.[Life_Cycle_Control_Document]= b.[LifeCycleControlDocument]
	--and a.[Curriculum_Control_Authority]= b.[CurriculumControlAuthority]
	--and a.[Current_Assessment_Approach]= b.[CurrentAssessmentApproach]
and b.id is null 
GO

-- =================================================================
SELECT distinct 
[CIN]
      ,[Course_Name]
      ,[Course_Type]
      ,[Curriculum_Control_Authority]
      ,[Life_Cycle_Control_Document]
      --,[Task_Statement]
      --,[Current_Assessment_Approach]
      --,[NEC_Refresher_Training_Selection]
      --,[Journeyman_Core_Training_Selection]
      --,[Master_Core_Training_Selection]
      --,[Notes]
      --,[AssessmentId]
      --,[CourseId]
      --,[CourseTaskId]
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]

  order by 1,2,3
  go

  SELECT distinct 
[CIN]
      ,[Course_Name]
     -- ,[Course_Type]
      ,[Curriculum_Control_Authority]
      --,[Life_Cycle_Control_Document]
      --,[Task_Statement]
      --,[Current_Assessment_Approach]
      --,[NEC_Refresher_Training_Selection]
      --,[Journeyman_Core_Training_Selection]
      --,[Master_Core_Training_Selection]
      --,[Notes]
      --,[AssessmentId]
      --,[CourseId]
      --,[CourseTaskId]
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  where CIN in ('A-495-0018','K-495-0441')
  order by 1,2,3
  go
  
  SELECT distinct 
[CIN]
      ,[Course_Name]
     -- ,[Course_Type]
     -- ,[Curriculum_Control_Authority]
      ,[Life_Cycle_Control_Document]
      --,[Task_Statement]
      --,[Current_Assessment_Approach]
      --,[NEC_Refresher_Training_Selection]
      --,[Journeyman_Core_Training_Selection]
      --,[Master_Core_Training_Selection]
      --,[Notes]
      --,[AssessmentId]
      --,[CourseId]
      --,[CourseTaskId]
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  where CIN in ('A-493-0080','A-500-0613','A-950-0001')
  order by 1,2,3
  go
  SELECT distinct 
[Curriculum_Control_Authority]
      --,[Life_Cycle_Control_Document]
      --,[Task_Statement]
      --,[Current_Assessment_Approach]
      --,[NEC_Refresher_Training_Selection]
      --,[Journeyman_Core_Training_Selection]
      --,[Master_Core_Training_Selection]
      --,[Notes]
      --,[AssessmentId]
      --,[CourseId]
      --,[CourseTaskId]
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  order by 1

  go

    SELECT distinct 
[Curriculum_Control_Authority]
      --,[Life_Cycle_Control_Document]
      --,[Task_Statement]
      ,[Current_Assessment_Approach]
      --,[NEC_Refresher_Training_Selection]
      --,[Journeyman_Core_Training_Selection]
      --,[Master_Core_Training_Selection]
      --,[Notes]
      --,[AssessmentId]
      --,[CourseId]
      --,[CourseTaskId]
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  where CIN <> 'N/A'
  order by 1

  go

    SELECT distinct 
[CIN]
      ,[Course_Name]
     -- ,[Course_Type]
     -- ,[Curriculum_Control_Authority]
      --,[Life_Cycle_Control_Document]
      --,[Task_Statement]
      ,[Current_Assessment_Approach]
      --,[NEC_Refresher_Training_Selection]
      --,[Journeyman_Core_Training_Selection]
      --,[Master_Core_Training_Selection]
      --,[Notes]
      --,[AssessmentId]
      --,[CourseId]
      --,[CourseTaskId]
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  where CIN <> 'N/A'
  --where CIN in ('A-493-0080','A-500-0613','A-950-0001')
  order by 1,2,3
  go

  
    SELECT distinct 
[Current_Assessment_Approach]
      --,[NEC_Refresher_Training_Selection]
      --,[Journeyman_Core_Training_Selection]
      --,[Master_Core_Training_Selection]

  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  where CIN <> 'N/A'
  --where CIN in ('A-493-0080','A-500-0613','A-950-0001')

  go
    
    SELECT distinct 
Course_Type
      --,[NEC_Refresher_Training_Selection]
      --,[Journeyman_Core_Training_Selection]
      --,[Master_Core_Training_Selection]

  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  where CIN <> 'N/A'
  --where CIN in ('A-493-0080','A-500-0613','A-950-0001')

  go