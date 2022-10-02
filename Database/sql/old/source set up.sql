/****** source set up ******/

SELECT  [Source], count(*) as ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [Source]
  go


  --select lower(newid())
USE [NavyRRL]
GO

INSERT INTO [dbo].[Source]
           ([Name]
           ,[SourceDate]
        --   ,[Description]
		,CTID
           ,[DateCreated]
          -- ,[CreatedById]
         --  ,[DateModified]
           ,[LastUpdatedById])

SELECT distinct 

	  [Source] as Name
     ,case when Date_of_Source = 'N/a' then '' else Date_of_Source end SourceDate
	 --,''
	 ,'ce-' + Lower(NewId()) as CTID
	 , getdate() as DateCreated, getdate() as DateModified 
      --,[WorkElementType]

      --,[WorkElemenTask]
      --,[Task_Applicability]
      --,[Formal_Training_Gap]
      --,[CIN]
      --,[Course_Name]
      --,[Course_Type]
      --,[Curriculum_Control_Authority]
      --,[Life_Cycle_Control_Document]
      --,[Task_Statement]
      --,[Current_Assessment_Approach]
	--  Into Source 
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021] a
  left join Source b on a.source = b.Name
  where b.Id is null 
 -- where source = 'NAVPERS 18068F Vol I'
  order by 1,2
  go


UPDATE [dbo].[QM_RMTL_11232021]
   SET SourceId = b.Id
from [QM_RMTL_11232021] a
inner join Source b on a.[Source] = b.name 

GO

