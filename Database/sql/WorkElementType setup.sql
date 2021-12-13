/****** WorkElementType set up ******/


USE [NavyRRL]
GO

INSERT INTO [dbo].[WorkElementType]
           ([Name]
        --   ,[description]
		   ,CTID
           ,[DateCreated]
           ,[DateModified])

SELECT distinct  
	a.[Work_Element_Type] as Name
	--, '' As Description
		,'ce-' + Lower(NewId()) as CTID
	, getdate() as DateCreated, getdate() as DateModified
--, NULL as CreatedById, NULL as LastUpdatedById 

  FROM [NavyRRL].[dbo].[QM_RMTL_11232021] a
  left join [WorkElementType] b on a.[Work_Element_Type] = b.Name
  where b.Id is null 
  order by 1
  go

SELECT  [Rating],
[Work_Element_Type], count(*) as ttl

  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [Rating],[Work_Element_Type]
  order by 2,1
  go

  --select lower(newid())

SELECT distinct 

--[Rating]
--      ,[Rank]
--      ,[Level]
--      ,[BilletTitle]
--      ,[FunctionalArea]
--      ,
	  [Source]
    --  ,[SourceDate]
      ,[Work_Element_Type]

      ,[Work_Element_Task]

  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  where source = 'NAVPERS 18068F Vol I'
  order by 1,2
  go


  
SELECT distinct  [Work_Element_Type] as Name, '' As description, getdate() as DateCreated, getdate() as DateModified 

     -- ,[WorkElemenTask]
      --,[Task_Applicability]
      --,[Formal_Training_Gap]
      --,[CIN]
      --,[Course_Name]
      --,[Course_Type]
      --,[Curriculum_Control_Authority]
      --,[Life_Cycle_Control_Document]
      --,[Task_Statement]
      --,[Current_Assessment_Approach]
	 -- Into WorkElementType
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  where isnull([Work_Element_Type],'') <> '' AND   [Work_Element_Type] <> 'N/A'
  --where source = 'NAVPERS 18068F Vol I'
  order by 1
  go

  USE [NavyRRL]
GO

UPDATE [dbo].[QM_RMTL_11232021]
   SET WorkElementTypeId = b.Id
from [QM_RMTL_11232021] a
inner join WorkElementType b on a.[Work_Element_Type] = b.name 

GO

