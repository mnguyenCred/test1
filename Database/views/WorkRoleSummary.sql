use NavyRRL
go

Alter View WorkRoleSummary 
As
SELECT TOP (1000) a.[Id]
      ,a.[RowId]
      ,a.[Name]
	  , IsNull(b.HasRatingTasks, 0) HasRatingTasks
      ,a.[Description]
      ,a.[CodedNotation]
      ,a.[Version]
      ,a.[CTID]
      ,a.[Created]
      ,a.[CreatedById]
      ,a.[LastUpdated]
      ,a.[LastUpdatedById]
  FROM [dbo].[WorkRole] a
  left join ( 
	Select WorkRoleId, count(*) as HasRatingTasks 
	from [RatingTask.WorkRole] 
	group by WorkRoleId 
	) b on a.id =  b.WorkRoleId

go
grant select on WorkRoleSummary to public
go