/****** functional area ******/

SELECT  [Functional_Area], count(*) as ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [Functional_Area]
  go
  --select lower(newid())

  USE [NavyRRL]
GO

INSERT INTO [dbo].[FunctionalArea]
           ([Name]
           ,[Description]
           ,[DateCreated]
           ,[DateModified]
           ,[CTID])
  
SELECT distinct  
	a.[Functional_Area] as Name
	, '' As Description
	, getdate() as DateCreated, getdate() as DateModified
	,'ce-' + Lower(NewId()) as CTID
--, NULL as CreatedById, NULL as LastUpdatedById 

  FROM [NavyRRL].[dbo].[QM_RMTL_11232021] a
  left join [FunctionalArea] b on a.[Functional_Area] = b.Name
  where b.Id is null 
  order by 1
  go

UPDATE [dbo].[QM_RMTL_11232021]
   SET [FunctionalAreaId] = b.Id
from [QM_RMTL_11232021] a
inner join FunctionalArea b on a.FunctionalArea = b.name 

GO
/****** assessment ******/

UPDATE [dbo].[QM_RMTL_11232021]
   SET [FunctionalAreaId] = b.Id
from [QM_RMTL_11232021] a
inner join FunctionalArea b on a.FunctionalArea = b.name 

GO


/*****?????????????a ******/

SELECT  [Functional_Area], count(*) as ttl
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  group by [Functional_Area]
  go
  --select lower(newid())


  
SELECT distinct  [Functional_Area] as Name, '' As Description, getdate() as DateCreated, getdate() as DateModified
--, NULL as CreatedById, NULL as LastUpdatedById 

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
	--  Into FunctionalArea
  FROM [NavyRRL].[dbo].[QM_RMTL_11232021]
  --where source = 'NAVPERS 18068F Vol I'
  order by 1
  go

UPDATE [dbo].[QM_RMTL_11232021]
   SET [FunctionalAreaId] = b.Id
from [QM_RMTL_11232021] a
inner join FunctionalArea b on a.[Functional_Area] = b.name 

GO

