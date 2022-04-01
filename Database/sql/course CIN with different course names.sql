/****** Script for SelectTopNRows command from SSMS  ******/
--course CIN with different course names
--course name with different CINs

SELECT  
distinct
a.[CIN]
      --,a.[Course_Name]

    --  ,[Curriculum_Control_Authority_CCA]
    --  ,[Life_Cycle_Control_Document]
    --  ,[CTTL_PPP_TCCD_Statement]
	  --     ,[Course_Type_A_C_G_F_T]
	 -- ,Current_Assessment_Approach
	 ,count(*) as total

  FROM [NavyRRL].[dbo].[2022-24_Rating_upload_Sonar_Technician_(Surface)_102709] a
 inner join (
  SELECT distinct [CIN]
  FROM [NavyRRL].[dbo].[2022-24_Rating_upload_Sonar_Technician_(Surface)_102709]
  where cin <> 'N/A'
  ) uniqueCIN on a.CIN = uniqueCIN.CIN

  inner join (
  SELECT  [CIN]
     ,[Course_Name]
	  ,count(*) as total

  FROM [NavyRRL].[dbo].[2022-24_Rating_upload_Sonar_Technician_(Surface)_102709]
  where cin <> 'N/A'
  group by [CIN]  
  ,[Course_Name]

  ) CINCourse on a.CIN = CINCourse.CIN

  group by a.CIN
  having count(*) > 1
order by 1

go
