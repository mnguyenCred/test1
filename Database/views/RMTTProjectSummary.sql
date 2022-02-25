Use NavyRRL
go

Create View RMTLProjectSummary 
AS
SELECT a.[Id]
      ,a.[RowId]
      ,a.[CTID]
      ,a.[Name]
      ,a.[RatingId]
	  ,b.Name as Rating
	  ,b.CodedNotation as RatingCodedNotation
      ,a.[Description]
      ,a.[StatusId]
      ,a.[VersionControlIdentifier]
	  ,0 as RmtlTasks
	  ,0 as ChangeProposals
      ,a.[Notes]
      ,a.[Created]      ,a.[CreatedById]
      ,a.[LastUpdated]      ,a.[LastUpdatedById] 
	  ,a.[LastApproved]      ,a.[LastApprovedById]      
	  ,a.[LastPublished]      ,a.[LastPublishedById]
  FROM [dbo].[RMTLProject] a
  Inner Join Rating b on a.RatingId = b.Id
  --task counts

  --ChangeProposal counts

  go

