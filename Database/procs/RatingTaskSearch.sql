USE [NavyRRL]
GO
/****** Object:  StoredProcedure [dbo].[RatingTaskSearch]    Script Date: 12/13/2021 1:37:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
USE NavyRRL
GO


--=====================================================

DECLARE @RC int, @Filter varchar(5000)
DECLARE @StartPageIndex int, @PageSize int, @TotalRows int,@SortOrder varchar(500),@CurrentUserId	int


set @SortOrder = 'newest'
set @SortOrder = 'relevance'
--set @CurrentUserId = 108
set @Filter = '  Rating = ''abf'' '

set @Filter = ' base.id in (select a.[RatingTaskId] from [RatingTask.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation = ''qm'' )	'
set @Filter = 'base.id in (select a.[RatingTaskId] from [RatingTask.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation = ''Aviation Boatswain''''s Mate (Fuels)'' OR b.name = ''Aviation Boatswain''''s Mate (Fuels)'' )'
set @Filter = ''

set @StartPageIndex = 1
set @PageSize = 100
--set statistics time on       
EXECUTE @RC = [RatingTaskSearch]
     @Filter, @SortOrder, @StartPageIndex  ,@PageSize,  @CurrentUserId, @TotalRows OUTPUT

select 'total rows = ' + convert(varchar,@TotalRows)

--set statistics time off       


*/


/* =============================================
Description:      Search RatingTask

Options:

  @StartPageIndex - starting page number. If interface is at 20 when next
page is requested, this would be set to 21?
  @PageSize - number of records on a page
  @CurrentUserId - at some point will want to limit what a person can see
				- not clear how to do this at the task level, although maybe can see all tasks, just control editing?
  @TotalRows OUTPUT - total available rows. Used by interface to build a
custom pager
  ------------------------------------------------------
Modifications
22-01-11 mparsons - new

*/

Alter PROCEDURE [dbo].[RatingTaskSearch]
		@Filter           varchar(5000)
		,@SortOrder         varchar(500)
		,@StartPageIndex  int
		,@PageSize        int
		,@CurrentUserId	int -- at some point will want to limit what a person can see
		,@TotalRows       int OUTPUT

As

SET NOCOUNT ON;
-- paging
DECLARE
      @first_id               int
      ,@startRow        int
      ,@debugLevel      int
      ,@SQL             varchar(5000)
	   ,@HasSitePrivileges bit
      ,@OrderBy         varchar(100)

-- =================================


if @SortOrder = 'relevance' set @SortOrder = 'base.Ratings, base.Rank, base.FunctionalArea, base.Source '
else if @SortOrder = 'alpha' set @SortOrder = 'base.RatingTask '
else if @SortOrder = 'oldest' set @SortOrder = 'base.Created '
else if @SortOrder = 'newest' set @SortOrder = 'base.LastUpdated Desc, base.Ratings, base.Rank, base.FunctionalArea, base.Source  '
else if @SortOrder = 'id_lowest' set @SortOrder = 'base.Id'
else set @SortOrder = 'base.Ratings, base.Rank, base.FunctionalArea, base.Source '

if len(@SortOrder) > 0 
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by base.Created '

Set @debugLevel = 4
set @HasSitePrivileges= 0
--check for site privileges
if (exists (
select RoleId from [dbo].[AspNetUserRoles_Summary] where id = @CurrentUserId And RoleId in (1,2,3)
))
	set @HasSitePrivileges = 1
print '@HasSitePrivileges: ' + convert(varchar, @HasSitePrivileges)
--===================================================
-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1

-- =================================

CREATE TABLE #tempWorkTable(
	RowNumber         int PRIMARY KEY IDENTITY(1,1) NOT NULL
	,Id					int
	,Task		varchar(900)

)
-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))

  set @SQL = 'SELECT Id, RatingTask  FROM [dbo].[RatingTaskSummary] base '
		+ @Filter


if charindex( 'order by', lower(@Filter) ) = 0
    set @SQL = @SQL + ' ' + @OrderBy

  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL

 INSERT INTO #tempWorkTable ( Id, Task 	)
  exec (@SQL)

  SELECT @TotalRows = @@ROWCOUNT
-- =================================

print 'added to temp table: ' + convert(varchar,@TotalRows)
if @debugLevel > 7 begin
  select * from #tempWorkTable
  end

-- Calculate the range
--===================================================
PRINT '@StartPageIndex = ' + convert(varchar,@StartPageIndex)

SET ROWCOUNT @StartPageIndex
--SELECT @first_id = RowNumber FROM #tempWorkTable   ORDER BY RowNumber
SELECT @first_id = @StartPageIndex
PRINT '@first_id = ' + convert(varchar,@first_id)

if @first_id = 1 set @first_id = 0
--set max to return
SET ROWCOUNT @PageSize

-- ================================= 
SELECT       
	RowNumber 
	,a.Id
	,b.RowId
	,b.Created
	,b.LastUpdated
	,b.Ratings
	,b.BilletTitles
      ,b.[CodedNotation]
      ,b.[RankId]
      ,b.[Rank]
      ,b.[PayGradeType]
      ,b.[LevelId]
      ,b.[Level]
      ,b.[FunctionalAreaId]
      ,b.[FunctionalArea]
      ,b.[SourceId]
      ,b.[Source]
      ,b.[SourceDate]
      ,b.[HasReferenceResource]
      ,b.[WorkElementTypeId]
      ,b.[WorkElementType]
      ,b.[ReferenceType]
	  ,b.CTID
      ,b.[RatingTask]
      ,b.[TaskApplicabilityId]
      ,b.[TaskApplicability]
      ,b.[ApplicabilityType]
      ,b.[FormalTrainingGapId]
      ,b.[FormalTrainingGap]
      ,b.[TrainingGapType]
      ,b.[CIN]
      ,b.[CourseName]
      ,b.[CourseType]
      ,b.[TrainingTaskId]
      ,TrainingTask
      ,b.[HasTrainingTask]
      ,b.[CurrentAssessmentApproach]
      ,b.[CurriculumControlAuthority]
      ,b.[LifeCycleControlDocument]
      ,b.[Notes]
	  --
      ,[CreatedById]
      ,[CreatedBy],CreatedByUID
      ,[LastUpdatedById]
      ,[ModifiedBy],ModifiedByUID
From #tempWorkTable a
Inner Join RatingTaskSummary b on a.Id = b.Id
WHERE RowNumber > @first_id
order by RowNumber 

go
grant execute on [RatingTaskSearch] to public
go
