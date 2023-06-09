USE [NavyRRL]
GO
/****** Object:  StoredProcedure [dbo].[ImportSummarySearch]    Script Date: 12/13/2021 1:37:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
USE NavyRRL
GO


--=====================================================

DECLARE @RC int, @Filter varchar(5000),@AccountsDB		varchar(100)
DECLARE @StartPageIndex int, @PageSize int, @TotalRows int,@SortOrder varchar(500),@GroupingByDay   bit
--, @GroupingByDay 

set @AccountsDB = 'ce_Accounts'
set @GroupingByDay = 0
set @SortOrder = 'Rating'
set @SortOrder = 'Id'

set @Filter = '  Rating = ''abf'' '

--set @Filter = '  convert(varchar(10),EventDate, 120) = ''2020-12-12''	'

--set @Filter = ''

set @StartPageIndex = 1
set @PageSize = 100
--set statistics time on       
EXECUTE @RC = [ImportSummarySearch]
     @Filter, @SortOrder, @StartPageIndex  ,@PageSize, @TotalRows OUTPUT

select 'total rows = ' + convert(varchar,@TotalRows)

--set statistics time off       


*/


/* =============================================
Description:      Search Import Summary

Options:

  @StartPageIndex - starting page number. If interface is at 20 when next
page is requested, this would be set to 21?
  @PageSize - number of records on a page
  @TotalRows OUTPUT - total available rows. Used by interface to build a
custom pager
  ------------------------------------------------------
Modifications
21-12-12 mparsons - new

*/

ALTER PROCEDURE [dbo].[ImportSummarySearch]
		@Filter           varchar(5000)
		,@SortOrder         varchar(500)
		,@StartPageIndex  int
		,@PageSize        int
		,@TotalRows       int OUTPUT

As

SET NOCOUNT ON;
-- paging
DECLARE
      @first_id               int
      ,@startRow        int
      ,@debugLevel      int
      ,@SQL             varchar(5000)
  --    ,@SortOrder         varchar(100)

-- =================================
--		,@GroupingByDay   bit = 0
declare @gbd varchar(100)
set @gbd = ''
if len(@SortOrder) > 0
      set @SortOrder = ' Order by ' + @SortOrder
else
      set @SortOrder = ' Order by Id '

Set @debugLevel = 4

--===================================================
-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1

-- =================================

CREATE TABLE #tempWorkTable(
	RowNumber         int PRIMARY KEY IDENTITY(1,1) NOT NULL
	,Id					int
	,IndexIdentifier		varchar(50)
	,Task		varchar(900)

)
-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))

  set @SQL = 'SELECT Id, [IndexIdentifier], Work_Element_Task  FROM [dbo].[ImportHistory] '
		+ @Filter


if charindex( 'order by', lower(@Filter) ) = 0
    set @SQL = @SQL + ' ' + @SortOrder

  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL

 INSERT INTO #tempWorkTable ( Id, [IndexIdentifier], Task 	)
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
	,a.[IndexIdentifier]
 ,[Unique_Identifier]
 ,[Message]
      ,[Rating]
      ,[Rank]
      ,[RankLevel]
      ,[Billet_Title]
      ,[Functional_Area]
      ,[Source]
      ,[Date_of_Source]
      ,[Work_Element_Type]
      ,[Work_Element_Task]
      ,[Task_Applicability]
      ,[Formal_Training_Gap]
      ,[CIN]
      ,[Course_Name]
      ,[Course_Type]
      ,[Curriculum_Control_Authority]
      ,[Life_Cycle_Control_Document]
      ,[Task_Statement]
      ,[Current_Assessment_Approach]
      ,[TaskNotes]

From #tempWorkTable a
Inner Join ImportHistory b on a.Id = b.Id
WHERE RowNumber > @first_id
order by RowNumber 

go
grant execute on [ImportSummarySearch] to public
go
