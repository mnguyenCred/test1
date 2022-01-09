USE [NavyRRL]
GO

/****** Object:  StoredProcedure [dbo].[ImportRMTL]    Script Date: 12/13/2021 9:09:36 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



/*
USE NavyRRL
GO

--=====================================================
DECLARE @RC int, @Rating           varchar(50)
set @Rating='QM'

EXECUTE @RC = [PopulateRMTLTables] @Rating


*/


/* =============================================
Description:      Import RMTL

Options:

  @Rating - rating code - maybe N/A
  ------------------------------------------------------
Modifications
21-12-12 mparsons - new

*/

Alter PROCEDURE [dbo].[PopulateRMTLTables]
		@Rating           varchar(50) = ''
As
SET NOCOUNT ON;
declare @RMTLProjectd int
,@errmsg   nvarchar(2048)
,@ErrorsFound bit

set @ErrorsFound= 0

/* =================================
- clear existing data?
- no, the latter would have been done prior to populating ImportRMTL
- =================================
*/
if IsNull(@Rating,'') = '' 
Begin
	SELECT @errmsg = 'Error a rating code is required'
	print @errmsg

	RAISERROR(@errmsg, 10, 1)
end
--

SELECT @RMTLProjectd= a.[Id]    
  FROM [dbo].[RMTLProject] a inner Join Rating on a.RatingId = Rating.Id
  where Rating.CodedNotation = @Rating
  print '@RMTLProjectd = ' +convert(varchar,@RMTLProjectd)

if @RMTLProjectd < 1 
	Begin
			SELECT @errmsg = 'Error an RMTL Project was not found for the provided rating: ' + @Rating
		print @errmsg

		RAISERROR(@errmsg, 10, 1)
	end
-- rating - concept scheme
UPDATE [dbo].ImportRMTL
   SET [RatingId] = b.Id
--	select  a.Rating, b.Name, b.CodedNotation
from [ImportRMTL] a
inner join Rating b on a.Rating = b.CodedNotation 

-- Rank - concept scheme
UPDATE [dbo].ImportRMTL
   SET [RankId] = b.Id
--	select  a.[Rank], b.Label, CodedNotation
from ImportRMTL a
inner join [ConceptScheme.Concept] b on a.[Rank] = b.CodedNotation 
WHERE        (ConceptSchemeId = 3)

-- Level - concept scheme
UPDATE [dbo].ImportRMTL
   SET [LevelId] = b.Id
--	select  a.[RankLevel], b.Label, CodedNotation
from ImportRMTL a
inner join [ConceptScheme.Concept] b on a.[RankLevel] = b.CodedNotation 
WHERE        (ConceptSchemeId = 11)

-- ============================================
-- Functional_Area 
--	- add any new ones

INSERT INTO [dbo].[FunctionalArea]
           ([Name])
SELECT distinct  
	a.[Functional_Area] as Name
	--, '' As Description
	--, getdate() as DateCreated, getdate() as DateModified
	--,'ce-' + Lower(NewId()) as CTID

  FROM [dbo].ImportRMTL a
  left join [FunctionalArea] b on a.[Functional_Area] = b.Name
  where b.Id is null 
  order by 1

-- add CTIDS
Update dbo.[FunctionalArea]
	set CTID = 'ce-' + Lower(NewId())
where ISNULL(CTID,'') = ''
--
-- update [FunctionalAreaId] in ImportRMTL

  UPDATE [dbo].ImportRMTL
   SET [FunctionalAreaId] = b.Id
from ImportRMTL a
inner join FunctionalArea b on a.Functional_Area = b.name 

-- ============================================
-- Source 
--	- add any new ones

INSERT INTO [dbo].[Source]
           ([Name]
           ,[SourceDate]  )

SELECT distinct 
	  [Source] as Name
    ,case when Date_of_Source = 'N/a' then '' else Date_of_Source end SourceDate
  FROM [dbo].ImportRMTL a
  left join Source b on a.source = b.Name
  where b.Id is null 
  order by 1,2

-- add CTIDS
Update dbo.[Source]
	set CTID = 'ce-' + Lower(NewId())
where ISNULL(CTID,'') = ''
--
UPDATE [dbo].ImportRMTL
   SET SourceId = b.Id
from ImportRMTL a
inner join Source b on a.[Source] = b.name 

-- ============================================
-- WorkElementType 
--	- add any new ones

INSERT INTO [dbo].[WorkElementType]
           ([Name])
SELECT distinct  
	a.[Work_Element_Type] as Name
  FROM [dbo].ImportRMTL a
  left join [WorkElementType] b on a.[Work_Element_Type] = b.Name
  where b.Id is null 
  order by 1

-- add CTIDS
Update dbo.[WorkElementType]
	set CTID = 'ce-' + Lower(NewId())
where ISNULL(CTID,'') = ''
--

UPDATE [dbo].ImportRMTL
   SET WorkElementTypeId = b.Id
from ImportRMTL a
inner join WorkElementType b on a.[Work_Element_Type] = b.name 

-- ============================================
-- Task_Applicability 

UPDATE [dbo].ImportRMTL
   SET [TaskApplicabilityId] = b.Id
--	select  a.[Task_Applicability], b.Label, Id
from ImportRMTL a
inner join [ConceptScheme.Concept] b on a.[Task_Applicability] = b.Label 
WHERE        (ConceptSchemeId = 12)

-- ============================================
-- Formal_Training_Gap 

UPDATE [dbo].ImportRMTL
   SET [FormalTrainingGapId] = b.Id
--	select  a.[Formal_Training_Gap], b.Label,  b.CodedNotation , Id
from ImportRMTL a
inner join [ConceptScheme.Concept] b on a.[Formal_Training_Gap] = b.Label 
WHERE        (ConceptSchemeId = 10)

-- ============================================
-- Course Related 
-- how to check for dups?
--	- could get list of candidates and exclude all, or use the first one
--	- actually ony add the CIN, then do updates
CREATE TABLE #tempMessageTable(
	Unique_Identifier       int PRIMARY KEY  NOT NULL
	,Rating			varchar(50)
	,CIN			varchar(50)
	,CourseName		varchar(500)
	,[Message]		varchar(500)
)

INSERT INTO #tempMessageTable ( Unique_Identifier, Rating, CIN, CourseName,[Message]	)
	SELECT distinct a.Unique_Identifier, a.Rating, a.[CIN], a.[Course_Name],'Courses with same CIN and different names'
	FROM [dbo].ImportRMTL a
	inner join (
		SELECT   allCourses.[CIN]  ,count(*) ttl
		FROM 
		(	SELECT distinct  [CIN], [Course_Name]
			FROM [dbo].ImportRMTL
			WHERE CIN <> 'N/A'
			-- order by 1,2
		) allCourses
	  group by [CIN] having count(*) > 1
	) dups on a.CIN = dups.CIN
	if @@ROWCOUNT > 0 
	begin
		--should these be ignored
		print 'found duplicate CIN/Courses'
		set @ErrorsFound= 1
	end
-- create min. record ------
INSERT INTO [dbo].[Course]
           ([CIN] ,Name )

SELECT distinct 
base.[CIN], 'placeholder'    

  FROM [dbo].ImportRMTL base
  left join course b on base.CIN = b.CIN
  where base.CIN <> 'N/A'
	and b.id is  null
--
Update dbo.Course
set CTID = 'ce-' + Lower(NewId())
where ISNULL(CTID,'') = ''

-- now updated - but only for new?

UPDATE [dbo].[Course]
   SET [Name] = b.[Course_Name]
      ,[CourseType] = CASE WHEN CourseTypes IS NULL THEN ''
			  WHEN len(CourseTypes) = 0 THEN ''
			  ELSE left(CourseTypes,len(CourseTypes)-1)
		END
      ,[LifeCycleControlDocument] = CASE WHEN LCCDTypes IS NULL THEN ''
			  WHEN len(LCCDTypes) = 0 THEN ''
			  ELSE left(LCCDTypes,len(LCCDTypes)-1)
		  END 
      ,[CurriculumControlAuthority] = CASE WHEN CCATypes IS NULL THEN ''
			  WHEN len(CCATypes) = 0 THEN ''
			  ELSE left(CCATypes,len(CCATypes)-1)
		  END
      ,[CurrentAssessmentApproach] = b.[Current_Assessment_Approach]
from [Course] a
Inner join ImportRMTL b on a.CIN = b.CIN

CROSS APPLY (
    SELECT distinct rmtl.Course_Type  + ' | '
    FROM dbo.ImportRMTL rmtl  
    WHERE CIN <> 'N/A'
	and a.CIN = rmtl.CIN
    FOR XML Path('') ) ctCodes (CourseTypes)

CROSS APPLY (
    SELECT distinct rmtl.Curriculum_Control_Authority  + ' | '
    FROM dbo.ImportRMTL rmtl  
    WHERE CIN <> 'N/A'
	and a.CIN = rmtl.CIN
    FOR XML Path('') ) ccaCodes (CCATypes)

CROSS APPLY (
    SELECT distinct rmtl.Life_Cycle_Control_Document  + ' | '
    FROM dbo.ImportRMTL rmtl  
    WHERE CIN <> 'N/A'
	and a.CIN = rmtl.CIN
    FOR XML Path('') ) lccdCodes (LCCDTypes)
 WHERE [Name]= 'placeholder' 
--
UPDATE [dbo].ImportRMTL
   SET [CourseId] = b.id
from [dbo].ImportRMTL a
inner join course b on a.CIN = b.CIN 
--and a.Course_Name = b.CourseName
--

INSERT INTO [dbo].[Course.Task]
           ([CourseId] ,[TaskStatement]  )

SELECT distinct a.CourseId ,[Task_Statement]
from [dbo].ImportRMTL a
left join [Course.Task] b on a.Task_Statement = b.TaskStatement
where a.CIN <> 'N/A'
and b.id is null 
--642
--

Update dbo.[Course.Task]
set CTID = 'ce-' + Lower(NewId())
where ISNULL(CTID,'') = ''
--
UPDATE [dbo].ImportRMTL
   SET CourseTaskId = b.id
--	select distinct a.CourseId,CourseTaskId, [Task_Statement]      
from [dbo].ImportRMTL a
inner join [Course.Task] b on a.CourseId = b.CourseId 
and a.[Task_Statement]= b.TaskStatement
and a.CourseTaskId is null 
-- =================================


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
   
 FROM [dbo].ImportRMTL a
 Left Join [RatingLevelTask] b on a.IndexIdentifier = b.CodedNotation
 where b.id is null 
 --
 Update dbo.[RatingLevelTask]
set CTID = 'ce-' + Lower(NewId())
where ISNULL(CTID,'') = ''
--
UPDATE [dbo].ImportRMTL
   SET [RatingLevelTaskId] = b.Id
--	select  a.IndexIdentifier, a.[Work_Element_Task], b.CodedNotation
from ImportRMTL a
inner join [RatingLevelTask] b on a.IndexIdentifier = b.CodedNotation 
where a.[RatingLevelTaskId] is null

-- =================================
INSERT INTO [dbo].[Job]
           ( [Name]	  ,[CTID]   )
SELECT distinct  a.Billet_Title, '' as CTID --required
  FROM [dbo].ImportRMTL a
  left join Job b on a.Billet_Title = b.name
  where b.id is null 
  order by 1
--

Update dbo.[Job]
set CTID = 'ce-' + Lower(NewId())
where Isnull(CTID,'') = ''
--
UPDATE [dbo].ImportRMTL
   SET [BilletTitleId] = b.Id
--	select  a.Billet_Title, b.Name, b.Id
from ImportRMTL a
inner join Job b on a.Billet_Title = b.Name 

-- =================================
--declare @Rating varchar(50), @RMTLProjectd int
--set @Rating = 'QM'

SELECT @RMTLProjectd= a.[Id]    
  FROM [dbo].[RMTLProject] a inner Join Rating on a.RatingId = Rating.Id
  where Rating.CodedNotation = @Rating
  print '@RMTLProjectd = ' +convert(varchar,@RMTLProjectd)

  --
  if @RMTLProjectd > 0 
  Begin
	INSERT INTO [dbo].[RmtlProject.Billet]
           (RmtlProjectId, [JobId])
	SELECT distinct @RMTLProjectd, c.Id
	  FROM [dbo].ImportRMTL a
	  inner join RMTLProject	b on a.RatingId = b.RatingId
	  left join Job				c on a.Billet_Title = c.name
	  left join [RmtlProject.Billet] d on b.Id = d.RmtlProjectId and d.JobId = c.Id
	  where d.id is null 
	  order by 1

	  --
	  INSERT INTO [dbo].[RmtlProjectBilletTask]
           ([ProjectBilletId],[RatingLevelTaskId])

		SELECT distinct b.Id rmtlProjectBilletId, c.Id as ratingLevelTaskId
		--,a.Billet_Title, b.Name

		  FROM [dbo].ImportRMTL a
		  inner join	[RmtlProject.Billet] b on a.BilletTitleId = b.JobId
		  inner join	RatingLevelTask c on a.RatingLevelTaskId = c.Id
		  left join		[RmtlProjectBilletTask] d on b.Id = d.[ProjectBilletId] and d.[RatingLevelTaskId] = c.Id
		  where d.id is null 
		  order by 1

	end
	else Begin
		SELECT @errmsg = 'Error an RMTL Project was not found for the provided rating: ' + @Rating
		print @errmsg

		RAISERROR(@errmsg, 10, 1)
		end


GO


grant execute on [PopulateRMTLTables] to public
go