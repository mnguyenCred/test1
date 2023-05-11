

USE [Navy_RRL_V2]
GO

/****** Object:  StoredProcedure [dbo].[AccountSearch]    Script Date: 1/21/2022 11:21:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/*
USE [CTI]
GO

SELECT [Id]
      ,[UserName]
      ,[FirstName]
      ,[LastName]
      ,[lastLogon]
			,convert(varchar(10),[lastLogon],110) as l2
      ,[SortName]
      ,[Email]
      ,[IsActive]
      ,[Created]
      ,[LastUpdated]
      ,[LastUpdatedById]
      ,[RowId]
      ,[AspNetId]
      ,[Roles], OrgMbrs
  FROM [dbo].[Account_Summary]
GO



--=====================================================

DECLARE @RC int,@SortOrder varchar(100),@Filter varchar(5000)
DECLARE @StartPageIndex int, @PageSize int, @TotalRows int,@CurrentUserId	int
--
set @CurrentUserId = 0
set @SortOrder = ''
set @SortOrder = 'base.LastName'
-- blind search 
set @Filter = ' ( email like ''%parso%'' or lastName like ''%parso%'') '


set @Filter = ''
set @StartPageIndex = 1
set @PageSize = 55
--set statistics time on       
EXECUTE @RC = [AccountSearch]
     @Filter,@SortOrder  ,@StartPageIndex  ,@PageSize, @CurrentUserId, @TotalRows OUTPUT

select 'total rows = ' + convert(varchar,@TotalRows)

--set statistics time off       


*/


/* =============================================
Description:      Account search
Options:

  @StartPageIndex - starting page number. If interface is at 20 when next
page is requested, this would be set to 21?
  @PageSize - number of records on a page
  @TotalRows OUTPUT - total available rows. Used by interface to build a
custom pager
  ------------------------------------------------------
Modifications
22-01-14 mparsons - new

*/

Alter PROCEDURE [dbo].[AccountSearch]
		@Filter           varchar(5000)
		,@SortOrder       varchar(100)
		,@StartPageIndex  int
		,@PageSize        int
		,@CurrentUserId	int
		,@TotalRows       int OUTPUT

As

SET NOCOUNT ON;
-- paging
DECLARE
      @first_id               int
      ,@startRow        int
      ,@debugLevel      int
      ,@SQL             varchar(5000)
      ,@OrderBy         varchar(100)
	  ,@HasSitePrivileges bit
	  --,@CurrentUserId	int

-- =================================

--set @CurrentUserId = 24
Set @debugLevel = 4
set @HasSitePrivileges= 0

if len(@SortOrder) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by base.LastName '

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
      RowNumber         int PRIMARY KEY IDENTITY(1,1) NOT NULL,
      Id int,
      Title             varchar(200)
)
-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter 
		--+ ' AND IsActive = 1'	
     end
	--else set @Filter =' WHERE IsActive = 1'

  print '@Filter len: '  +  convert(varchar,len(@Filter))
  set @SQL = 'SELECT  base.Id, base.FullName 
        from [Account_Summary] base   '
        + @Filter
        
  if charindex( 'order by', lower(@Filter) ) = 0
    set @SQL = @SQL + ' ' + @OrderBy

  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL

  INSERT INTO #tempWorkTable (Id, Title)
  exec (@SQL)
  --print 'rows: ' + convert(varchar, @@ROWCOUNT)
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
SELECT @first_id = @StartPageIndex
PRINT '@first_id = ' + convert(varchar,@first_id)

if @first_id = 1 set @first_id = 0
if @PageSize = -1 set @PageSize = 0
--set max to return
SET ROWCOUNT @PageSize

-- ================================= 
SELECT        
		RowNumber, 
		base.id, 
		base.FirstName, 
		base.LastName,
		base.Email, 
		base.Created, 
		base.IsActive, 
		base.LastUpdated
		,base.AspNetId
		,base.LastUpdatedById
		,base.RowId
		,base.Roles
		--, base.OrgMbrs
		--OR ombrs.UserId is not null 
		,case when @HasSitePrivileges = 1 then 'true' 
		else 'false' end As CanEditRecord
		--,base.lastLogon
		,convert(varchar(10),base.lastLogon,110) as lastLogon
From #tempWorkTable work
	Inner join Account_Summary base on work.Id = base.Id 
	--left join [Organization.Member] ombrs on base.Id = ombrs.ParentOrgId and ombrs.UserId = @CurrentUserId

WHERE RowNumber > @first_id
order by RowNumber 

GO
grant execute on AccountSearch to public
go


