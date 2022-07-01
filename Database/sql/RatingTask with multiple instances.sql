use navyrrl
go

SELECT 

      [RatingTask]
	  	  ,count(*) as Instances
	  ,[Rank]
      ,[FunctionalArea]
      ,[ReferenceResource]
      ,[SourceDate]  
      --,[WorkElementType]
      ,[WorkElementTypeAlternateName] as WorkElementType  
      ,[TaskApplicability]
      ,[FormalTrainingGap]
     
  FROM [NavyRRL].[dbo].[RatingTaskSummary]
  group by 
  
      [RatingTask]
	  ,[Rank]
      ,[FunctionalArea] 
      ,[ReferenceResource]
      ,[SourceDate]  
     -- ,[WorkElementType]
      ,[WorkElementTypeAlternateName]  
      ,[TaskApplicability]
      ,[FormalTrainingGap]
	  having count(*) > 1

order by 
      [RatingTask]
	  ,[Rank]
      ,[FunctionalArea] 
      ,[ReferenceResource]