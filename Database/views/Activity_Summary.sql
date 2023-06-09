USE [NavyRRL]
GO

/****** Object:  View [dbo].[Activity_Summary]    Script Date: 12/7/2021 10:26:14 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/*


SELECT top 1000
 [Id]
      ,[CreatedDate]
      ,[ActivityType]
      ,[EntityTypeId]
      ,[Activity]
      ,[Event]
      ,[Comment]
      ,[TargetUserId]
      ,[ActionByUserId],[ActionByUser]
      
      ,[ActivityObjectId]
      ,[ObjectRelatedId]
   --   ,[OwningOrgId]  ,[Organization]
    
      ,[TargetObjectId]
      ,[SessionId]
      ,[IPAddress]
      ,[Referrer]
   
  FROM [dbo].[Activity_Summary]

  where 
  --EntityTypeId=3  and 
  event like '%view%'
order by id desc



*/
ALTER  VIEW [dbo].[Activity_Summary]
AS

SELECT a.[Id]
      ,a.[CreatedDate]
      ,a.[ActivityType]
	  ,d.Id as EntityTypeId
      ,[Activity]
      ,[Event]
      ,[Comment]
      ,[TargetUserId]
      ,[ActionByUserId], b.FirstName + ' ' + b.LastName as ActionByUser
      ,[ActivityObjectId]
      ,[ObjectRelatedId]
    -- ,ec.OwningOrgId, o.Name as Organization
   ,RelatedTargetUrl
      ,[TargetObjectId]
      ,[SessionId]
      ,[IPAddress]
      ,[Referrer]
 
  FROM [dbo].[ActivityLog] a
	 left join Account b on a.ActionByUserId = b.Id
	 Left Join [Codes.EntityType] d on a.ActivityType = d.Title
	-- Left Join Entity_Cache ec on d.Id = ec.EntityTypeId and ActivityObjectId = ec.BaseId
	 --Left Join Organization o on ec.OwningOrgId = o.Id
	--where Activity <> 'session'
	--and convert(varchar(10),CreatedDate,120) = convert(varchar(10),getDate(),120)
	--order by createddate desc

GO

