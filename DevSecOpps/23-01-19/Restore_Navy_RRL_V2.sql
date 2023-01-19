/*
Steps
 =============================================================================
 1. Prepare for restore
 =============================================================================
	- Log into the sql server machine
	- copy the zip of the backup file to a folder on the server and unzip
	- this backup script would typically be in the same folder as the latter file. Double click to open in sql server and continue below
 =============================================================================
 2. Configure the restore script for the current environment
 =============================================================================
	- scroll down and update the following variables
		- @devSqlBackupLoc	Set to the path to the folder with back up file (end the path with a trailing back slash)
		- @devSqlDataLoc	Set to the folder that will contain the database data files
		- @@devSqlLogLoc	Set to the folder that will contain the database log files (often the same as the latter for VMs)
		- @BackupFile		Set to the actual name of the unzipped backup file (ex. Navy_RRL_V2_backupInitial_2023_01_19_forDevSec.bak')

	- Preview the restore
		Uncomment: set @RestoreAction = 'list' (remove the leading --)
		- press F5
		- review the Results and Messages tabs to ensure all of the variables and file paths are correct
	- Run the backup 
		Comment out: set @RestoreAction = 'list' (by inserting -- at the start of the statement)
		- Press F5
		- review
		- if an error occurs that states that the database is in use, see the section: 
				If an error occurs that the database is in use

		- If the database users have not been added to the current server, see the section:
				

  =============================================================================
  If an error occurs that the database is in use, highlight the below rows starting with --start to --end, then press F5 to execute
 =============================================================================
--start
use master 
go
declare @sql as varchar(20), @db as varchar(20), @spid as int
set @db = 'Navy_RRL_V2'
select @spid = min(spid)  from master..sysprocesses  where dbid = db_id(@db) 
and spid != @@spid    

while (@spid is not null)
begin
    print 'Killing process ' + cast(@spid as varchar) + ' ...'
    set @sql = 'kill ' + cast(@spid as varchar)
    exec (@sql)

    select 
        @spid = min(spid)  
    from 
        master..sysprocesses  
    where 
        dbid = db_id(@db) 
        and spid != @@spid
end 

print 'Process completed...'
--end

 =============================================================================
 ?. Add application users to current database server if not previously done
 =============================================================================
use master 
go
sp_addLogin 'navyAdmin', 'w@rkH#rdPl$yH%rd', master
go
sp_addLogin 'navyReader', 'w@rkH#rdPl$yH%rd', master
go

USE [Navy_RRL_V2]
GO
CREATE USER [navyReader] FOR LOGIN [navyReader]
GO

ALTER ROLE [db_datareader] ADD MEMBER [navyReader]
GO

 =============================================================================
 4. Sync database users from back to local server
 =============================================================================
Use Navy_RRL_V2
go
sp_change_users_login 'report'
go
sp_change_users_login 'update_one', 'navyAdmin','navyAdmin'
go
sp_change_users_login 'update_one', 'navyReader','navyReader'
go
sp_change_users_login 'report'
go




*/
-- Script to restore a Navy_RRL_V2 backup 

Declare
	@RestoreAction	varchar(20),
	@DestDatabase	varchar(150),
	@DestDatafile	varchar(150),
	@DestLogfile	varchar(150),
	@DestDataPath	varchar(150),
	@DestLogPath	varchar(150),
	@BackupFile		varchar(150),
	@BackupPath		varchar(150),

	@Datafile		varchar(150),
	@Logfile		varchar(150),
	@BackupDir  	varchar(150)
	,@DataLoc		varchar(150)
	,@LogLoc		varchar(150)

	,@devSqlDataLoc	varchar(150)
	,@devSqlLogLoc	varchar(150)
	,@devSqlBackupLoc	varchar(150)



-- ===============================================================

--
set @devSqlBackupLoc = 'D:\Data\SqlServer2016\backups\Navy_RRL_V2\'     
set @devSqlDataLoc = 'D:\data\sqlserver2016\'
set @devSqlLogLoc = 'D:\data\sqlserver2016\'

-- 
-- Actions:
-- ======================================================================
-- Use List to list the contents of the selected backup file
-- Use replace when restoring the backup from one db overtop another db (our typical scenario)
-- set @RestoreAction = 'recover'
 set @RestoreAction = 'replace'
set @RestoreAction = 'list'

set @DestDatabase = 'Navy_RRL_V2'
set @DestDatafile = @DestDatabase --+ '_Data'
set @DestLogfile  = @DestDatabase + '_Log'

-- set to the current locations===
set @BackupDir 	= @devSqlBackupLoc
set @DataLoc		= @devSqlDataLoc
set @LogLoc			= @devSqlLogLoc

set @BackupFile = 'Navy_RRL_V2_backupInitial_2023_01_19_forDevSec.bak'	

if 1 = 2 begin
-- If the source backup is the same as the dest. then use:
	set @Datafile = @DestDatabase + '_Data'
	set @Logfile = @DestDatabase + '_Log'
	end
else begin
-- Otherwise use (either name from backup or may need to check logical name used in the 
-- backup - that is first execute RESTORE FILELISTONLY FROM DISK = @BackupPath below
	set @Datafile = 'NavyRRL'
	set @Logfile  = 'NavyRRL_log'	
	end

-- following should just be generic variables
set @BackupPath = @BackupDir + @BackupFile

set @DestDataPath = @DataLoc + @DestDatafile + '.mdf'
set @DestLogPath  = @LogLoc  + @DestLogfile + '.ldf'
print '====== Restore request parameters ===================='
print '        Action: ' + @RestoreAction
print 'SOURCE:'
print '        Source: ' + @BackupPath
print '      Datafile: ' + @Datafile
print '       Logfile: ' + @Logfile
print '------------------------------------------------------'
print 'DESTINATION:'
print '      Database: ' + @DestDatabase
print '          Data: ' + @DestDataPath
print '           Log: ' + @DestLogPath
print ''
print '======================================================'
if @RestoreAction = 'list' begin
RESTORE FILELISTONLY FROM DISK = @BackupPath
end
else if @RestoreAction = 'recover' begin
RESTORE DATABASE @DestDatabase FROM DISK = @BackupPath
	WITH RECOVERY,
	MOVE @Datafile 	TO @DestDataPath,
	MOVE @Logfile 	TO @DestLogPath
end
else if @RestoreAction = 'replace' begin
RESTORE DATABASE @DestDatabase FROM DISK = @BackupPath
	WITH RECOVERY, 
		 REPLACE,
	MOVE @Datafile 	TO @DestDataPath,
	MOVE @Logfile 	TO @DestLogPath
end else begin
	print 'Invalid restore action of : ' +  @RestoreAction 
	print 'no action taken'
end
GO