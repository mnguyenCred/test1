using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Navy.Utilities;

using System.Runtime.Caching;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Models.Application;
using Models.Schema;
using Models.Curation;
using Factories;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Services
{
	public class BulkUploadServices
	{
		public static string thisClassName = "BulkUploadServices";

		public static void CacheChangeSummary( ChangeSummary summary )
		{
			summary.RowId = summary.RowId == Guid.Empty ? Guid.NewGuid() : summary.RowId;
			MemoryCache.Default.Remove( summary.RowId.ToString() );
			MemoryCache.Default.Add( summary.RowId.ToString(), summary, new DateTimeOffset( DateTime.Now.AddHours( 1 ) ) );
		}
		//

		public static ChangeSummary GetCachedChangeSummary( Guid rowID )
		{
			return ( ChangeSummary ) MemoryCache.Default.Get( rowID.ToString() );
		}
		//
		public static void ApplyChangeSummary( ChangeSummary summary)
		{
			if ( summary == null )
				return;

			AppUser user = AccountServices.GetCurrentUser();
			if (user?.Id == 0 )
            {

				summary.Messages.Error.Add( "Error - a current user was not found. You must authenticated and authorized to use this function!" );
				return;
            }
			SiteActivity sa = new SiteActivity()
			{
				ActivityType = "RMTL",
				Activity = "Upload",
				Event = "Save Started",
				Comment = String.Format( "A bulk upload save was initiated by: '{0}' for Rating: '{1}' was committed.", user.FullName(), summary.RatingCodedNotation ?? "" ),
				ActionByUserId = user.Id,
				ActionByUser = user.FullName()
			};
			new ActivityServices().AddActivity( sa );
			DateTime saveStarted = DateTime.Now;
			var saveDuration = new TimeSpan();
			#region Handle ItemsToBeCreated
			//22-04-15 mp - there can be an issue doing all ItemsToBeCreated and then all FinalizedChanges:
			//			Reason for trainingTask issue: the course exists, so the training task is not added until the finalize step. This can result in a ratingTask that needs the trainingTasks finds it has not been saved yet.
			if ( summary.ItemsToBeCreated != null)
            {
				//all dependent data has to be done first
				//========== org should be OK to do one after the other
				if ( summary.ItemsToBeCreated.Organization?.Count > 0 )
                {
					var orgMgr = new OrganizationManager();
					foreach (var item in summary.ItemsToBeCreated.Organization )
                    {
						item.CreatedById = item.LastUpdatedById = user.Id;
						orgMgr.Save( item, user.Id, ref summary );
                    }
                }

				if ( summary.FinalizedChanges.Organization?.Count > 0 )
				{
					var orgMgr = new OrganizationManager();
					foreach ( var item in summary.FinalizedChanges.Organization )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						orgMgr.Save( item, user.Id, ref summary );
					}
				}

				//========== ReferenceResource should be OK to do one after the other
				if ( summary.ItemsToBeCreated.ReferenceResource?.Count > 0 )
				{
					var mgr = new ReferenceResourceManager();
					foreach ( var item in summary.ItemsToBeCreated.ReferenceResource )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
                    
					if ( UtilityManager.GetAppKeyValue( "listingInputRecords", false ) || UtilityManager.GetAppKeyValue( "environment" ) == "development" )
					{
						foreach ( var item in summary.ItemsToBeCreated.ReferenceResource )
						{
							LoggingHelper.DoTrace( 7, String.Format( "ReferenceResource: Name: {0}, Date: {1}, rowId: {2}.", item.Name, item.PublicationDate, item.RowId ), false );
						}
					}
				}
				//do we need to do these here, there is an issue where the referenceResource is not found when saving a rating task. 
				if ( summary.FinalizedChanges.ReferenceResource?.Count > 0 )
				{
					var mgr = new ReferenceResourceManager();
					foreach ( var item in summary.FinalizedChanges.ReferenceResource )
					{
						item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
				}

				//========== Course should be OK to do one after the other

				if ( summary.ItemsToBeCreated.Course?.Count > 0 )
				{

					//if course aleady exists, then the associated training tasks are not processed until FinalizedChanges. This would be a problem for RatingTasks that  under toBeCreated that use those tasks!!!
					var courseMgr = new CourseManager();
					foreach ( var item in summary.ItemsToBeCreated.Course )
					{
						//get all tasks for this course
						//?? or do task separately in the next step?
						if (summary.ItemsToBeCreated.TrainingTask?.Count > 0 )
                        {
							var results = summary.ItemsToBeCreated.TrainingTask.Where( p => item.HasTrainingTask.Any( p2 => p2 == p.RowId ) );
							item.TrainingTasks.AddRange( results );
						}

						item.CreatedById = item.LastUpdatedById = user.Id;
						courseMgr.Save( item,  ref summary );
					}
					if ( UtilityManager.GetAppKeyValue( "listingInputRecords", false ) || UtilityManager.GetAppKeyValue( "environment" ) == "development" )
					{
						foreach ( var item in summary.ItemsToBeCreated.Course )
						{
							LoggingHelper.DoTrace( 6, String.Format("Course: {0}, CIN: {1}.",item.Name, item.CodedNotation ), false);
						}
					}
				} 
				//if no courses, there could be training tasks for courses that already exist
				
				var trainTaskMgr = new TrainingTaskManager();
				//but what to do with them?
				//once the course code is present, do a select distinct and then process by course
				if ( summary.ItemsToBeCreated.TrainingTask?.Count > 0 )
				{
					//get course codes for training tasks
					//NOTE: should do all here, or need to exclude course from toBeCreated list
					var courseCodeList = summary.ItemsToBeCreated.TrainingTask.Select( p => p.CourseCodedNotation ).Distinct().ToList();
					foreach ( var item in courseCodeList )
					{
						if (string.IsNullOrWhiteSpace(item))
                        {
							//why
							continue;
                        }
						var parent = CourseManager.GetByCodedNotation( item );
						if ( parent?.Id == 0 )
						{
							summary.AddError( thisClassName + String.Format( ".ApplyChangeSummmary-ItemsToBeCreated.TrainingTask. Error - A course was not found for the provided CIN: {0}. ", item ) );
							continue;
						}

						var results = summary.ItemsToBeCreated.TrainingTask.Where( p => p.CourseCodedNotation == item ).ToList();
						
						parent.LastUpdatedById = user.Id;
						parent.TrainingTasks.AddRange( results );
						trainTaskMgr.SaveList( parent, ref summary );
					}
					//foreach ( var item in summary.ItemsToBeCreated.TrainingTask )
					//               {
					//	item.CreatedById = item.LastUpdatedById = user.Id;
					//	//there is no association with the course? Need to store the CIN with training task
					//	if (!string.IsNullOrWhiteSpace(item.CourseCodedNotation))
					//                   {
					//		trainTaskMgr.Save( item.CourseCodedNotation, item, ref summary );
					//	}
					//	else
					//                   {
					//		//may want to log as an issue
					//                   }
					//}
				}
				

				if ( summary.ItemsToBeCreated.WorkRole?.Count > 0 )
				{
					var mgr = new WorkRoleManager();
					foreach ( var item in summary.ItemsToBeCreated.WorkRole )
					{
						
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
					if ( UtilityManager.GetAppKeyValue( "listingInputRecords", false ) || UtilityManager.GetAppKeyValue( "environment" ) == "development" )
					{
						//make configurable
						foreach ( var item in summary.ItemsToBeCreated.WorkRole )
						{
							LoggingHelper.DoTrace( 6, "WorkRole: " + item.Name, false );
						}
					}
				}

				if ( summary.ItemsToBeCreated.BilletTitle?.Count > 0 )
				{
					var mgr = new JobManager();
					foreach ( BilletTitle item in summary.ItemsToBeCreated.BilletTitle )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
                    }
					if ( UtilityManager.GetAppKeyValue( "listingInputRecords", false ) || UtilityManager.GetAppKeyValue( "environment" ) == "development" )
					{
						//make configurable
						foreach ( var item in summary.ItemsToBeCreated.BilletTitle )
						{
							LoggingHelper.DoTrace( 6, "BilletTitle ItemsToBeCreated: " + item.Name, false );
						}
					}
				}
				//
				if ( summary.ItemsToBeCreated.RatingTask?.Count > 0 )
				{
					var mgr = new RatingTaskManager();
					int cntr = 0;
					foreach ( var item in summary.ItemsToBeCreated.RatingTask )
					{
						cntr++;
						if (item.CodedNotation == "OC-071" || item.CodedNotation == "PQ26-081" )
                        {

                        }
						//get all billets for this task
						if ( item.HasBilletTitle == null )
							item.HasBilletTitle = new List<Guid>();
						if ( summary.ItemsToBeCreated.BilletTitle?.Count > 0 )
						{
							//get billets that reference this task
							//this could be more for the reverse relationship
							var results = summary.ItemsToBeCreated.BilletTitle.Where( p => p.HasRatingTask.Contains(item.RowId) ).ToList();

							if ( results.Count > 0 )
							{
								//yes this just results in a duplicate
								//item.HasBilletTitle.AddRange( results.Select( p => p.RowId ) );
							} else
                            {
								//is there an alternative? should it have been found in hasRatingTask
								//there would only be one?
								/*
								var resultsByCode = summary.ItemsToBeCreated.BilletTitle.Where( p => p.HasRatingTaskByCode.Contains( item.CodedNotation ) ).ToList();
								if( resultsByCode.Count > 0 )
								{
									//this will always be one (one per row). Alternate would be to do a set based approach after all rating tasks are created. 
									item.HasBilletTitle.AddRange( resultsByCode.Select( p => p.RowId ) );
								}
								*/
							}
						}
						item.CreatedById = item.LastUpdatedById = user.Id;
						item.CurrentRatingCode = summary.RatingCodedNotation;
						mgr.Save( item, ref summary );
					}
				}

				//ClusterAnalysis?? will be directly related to rating task
				if ( summary.ItemsToBeCreated.ClusterAnalysis?.Count > 0 )
				{

					//is training task part of course, see there is a separate TrainingTask in UploadableData. the latter has no course Id/RowId to make an association?
					var mgr = new ClusterAnalysisManager();
					foreach ( var item in summary.ItemsToBeCreated.ClusterAnalysis )
					{						
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
					//if ( UtilityManager.GetAppKeyValue( "listingInputRecords", false ) || UtilityManager.GetAppKeyValue( "environment" ) == "development" )
					//{
					//	foreach ( var item in summary.ItemsToBeCreated.Course )
					//	{
					//		LoggingHelper.DoTrace( 6, String.Format( "Course: {0}, CIN: {1}.", item.Name, item.CodedNotation ), false );
					//	}
					//}
				}
			}


			#endregion
			#region Handle AddedItemsToInnerListsForCopiesOfItems
			//obsolete
			/*
			if ( summary.AddedItemsToInnerListsForCopiesOfItems != null )
			{
				//what to do with these? will be existing parents with child updates like course and training task
				if ( summary.AddedItemsToInnerListsForCopiesOfItems.Course?.Count > 0 )
				{
					//is training task part of course, see there is a separate TrainingTask in UploadableData. the latter has no course Id/RowId to make an association?
					var courseMgr = new CourseManager();
					foreach ( var item in summary.AddedItemsToInnerListsForCopiesOfItems.Course )
					{
						//is all data present?
						item.CreatedById = item.LastUpdatedById = user.Id;
						//do we want a full save here, or just focus on training tasks?
						//don't have the full course (just rowId), just parts, so 
						//may need to get the core record. See what is needed for parts
						var basicRecord = CourseManager.Get( item.RowId, false );
						item.Id = basicRecord.Id;
						item.Name = basicRecord.Name;
						//other?

						courseMgr.UpdateParts( item, summary );
					}
				}

				if ( summary.AddedItemsToInnerListsForCopiesOfItems.BilletTitle?.Count > 0 )
				{
					var mgr = new JobManager();
					//this will have HasRatingTasks. We save from the other side RatingTask.HasJob
					//foreach ( var item in summary.AddedItemsToInnerListsForCopiesOfItems.BilletTitle )
					//{						
					//	//item.CreatedById = item.LastUpdatedById = user.Id;
					//	//mgr.Save( item, ref summary );
					//}
				}


				if ( summary.AddedItemsToInnerListsForCopiesOfItems.TrainingTask?.Count > 0 )
				{
					var mgr = new TrainingTaskManager();
					foreach ( var item in summary.AddedItemsToInnerListsForCopiesOfItems.TrainingTask )
					{
						//TBD. Only item would be AssessmentMethodType
						item.CreatedById = item.LastUpdatedById = user.Id;
						//may need to get the core record. See what is needed for parts
						var basicRecord = TrainingTaskManager.Get( item.RowId );
						item.Id = basicRecord.Id;
						item.Description = basicRecord.Description;
						//TBD on if deletes will be an issue?
						mgr.TrainingTaskAssessmentMethodSave( item, ref summary );
					}
				}
				//

				if ( summary.AddedItemsToInnerListsForCopiesOfItems.RatingTask?.Count > 0 )
				{
					var mgr = new RatingTaskManager();
					foreach ( var item in summary.AddedItemsToInnerListsForCopiesOfItems.RatingTask )
					{
						//TBD. Only item would be AssessmentMethodType
						item.CreatedById = item.LastUpdatedById = user.Id;
						//may need to get the core record. See what is needed for parts
						var basicRecord = RatingTaskManager.Get( item.RowId );
						item.Id = basicRecord.Id;
						item.Description = basicRecord.Description;
						//TBD on if deletes will be an issue?
						mgr.UpdateParts( item, summary );
					}
				}

				if ( summary.AddedItemsToInnerListsForCopiesOfItems.ClusterAnalysis?.Count > 0 )
				{

					//not sure about this, no inner
					var mgr = new ClusterAnalysisManager();
					foreach ( var item in summary.AddedItemsToInnerListsForCopiesOfItems.ClusterAnalysis )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
				
				}
			}
			*/
			#endregion
			//
			#region Handle FinalizedChanges
			//22-04-14 now using FinalizedChanges  instead of ItemsToBeChanged
			if ( summary.FinalizedChanges != null )
			{
				if ( summary.FinalizedChanges.Organization?.Count > 0 )
				{
					var orgMgr = new OrganizationManager();
					foreach ( var item in summary.FinalizedChanges.Organization )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						orgMgr.Save( item, user.Id, ref summary );
					}
				}
				//
				if ( summary.FinalizedChanges.ReferenceResource?.Count > 0 )
				{
					//22-04-16 - this step is now being done with the ItemsToBeCreated step. Implications?
					var mgr = new ReferenceResourceManager();
					foreach ( var item in summary.FinalizedChanges.ReferenceResource )
					{
						item.LastUpdatedById = user.Id;
						//mgr.Save( item, ref summary );
					}
				}
				//
				if ( summary.FinalizedChanges.Course?.Count > 0 )
				{
					//is training task part of course, see there is a separate TrainingTask in UploadableData. the latter has no course Id/RowId to make an association?
					var courseMgr = new CourseManager();
					foreach ( var item in summary.FinalizedChanges.Course )
					{
						//get all tasks for this course
						//22-04-15 mp - OR skip and only do in TrainingTask step
						if ( summary.FinalizedChanges.TrainingTask?.Count > 0 )
						{
							var results = summary.FinalizedChanges.TrainingTask.Where( p => item.HasTrainingTask.Any( p2 => p2 == p.RowId ) );
							item.TrainingTasks.AddRange( results );
							if ( item.TrainingTasks.Count > 0 )
                            {

                            }
						}

						item.CreatedById = item.LastUpdatedById = user.Id;
						courseMgr.Save( item, ref summary );
					}
				}

				if ( summary.FinalizedChanges.TrainingTask?.Count > 0 )
				{
					//not sure training tasks should be separate, should always be part of course??????
					var mgr = new TrainingTaskManager();
					var courseCodeList = summary.FinalizedChanges.TrainingTask.Select( p => p.CourseCodedNotation ).Distinct().ToList();
					foreach ( var item in courseCodeList )
					{
						if ( string.IsNullOrWhiteSpace( item ) )
						{
							//why
							continue;
						}
						var parent = CourseManager.GetByCodedNotation( item );
						if ( parent?.Id == 0 )
						{
							summary.AddError( thisClassName + String.Format( ".ApplyChangeSummmary-FinalizedChanges.TrainingTask. Error - A course was not found for the provided CIN: {0}. ", item ) );
							return;
						}
						if (parent.CodedNotation == "J-500-0029F" )
                        {

                        }
						var results = summary.FinalizedChanges.TrainingTask.Where( p => p.CourseCodedNotation == item ).ToList();

						parent.LastUpdatedById = user.Id;
						parent.TrainingTasks.AddRange( results );
						mgr.SaveList( parent, ref summary );
					}

				}

				if ( summary.FinalizedChanges.WorkRole?.Count > 0 )
				{
					var mgr = new WorkRoleManager();
					foreach ( var item in summary.FinalizedChanges.WorkRole )
					{
						item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
				}

				if ( summary.FinalizedChanges.BilletTitle?.Count > 0 )
				{
					var mgr = new JobManager();
					foreach ( var item in summary.FinalizedChanges.BilletTitle )
					{
						item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
				}

				if ( summary.FinalizedChanges.RatingTask?.Count > 0 )
				{
					var mgr = new RatingTaskManager();
					foreach ( var item in summary.FinalizedChanges.RatingTask )
					{
						//not sure what to do for billets here?
						//get all billets for this task
						if ( summary.FinalizedChanges.BilletTitle?.Count > 0 )
						{
							//get billets that reference this task
							var results = summary.FinalizedChanges.BilletTitle.Where( p => p.HasRatingTask.Contains( item.RowId ) ).ToList();
							item.HasBilletTitle.AddRange( results.Select( p => p.RowId ) );
						}
						//do we need to check the created as well?
						if ( summary.ItemsToBeCreated.BilletTitle?.Count > 0 )
						{
							//get billets that reference this task
							var results = summary.ItemsToBeCreated.BilletTitle.Where( p => p.HasRatingTask.Contains( item.RowId ) ).ToList();
							item.HasBilletTitle.AddRange( results.Select( p => p.RowId ) );
						}
						item.LastUpdatedById = user.Id;
						//ISSUE THE FULL RATING TASK IS NOT HERE. HOW TO DO A PROPER SAVE?
						//CONFIRM FIXED
						//need to check if training task has change to a new one/course (not just updated descr)
						item.CurrentRatingCode = summary.RatingCodedNotation;
						mgr.Save( item, ref summary );
					}
				}


				if ( summary.FinalizedChanges.ClusterAnalysis?.Count > 0 )
				{
					var mgr = new ClusterAnalysisManager();
					foreach ( var item in summary.FinalizedChanges.ClusterAnalysis )
					{
						item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
				}
			}
			#endregion

			#region Handle ItemsToBeDeleted
			//not sure how different
			if ( summary.ItemsToBeDeleted != null )
			{

			}
			#endregion
			//copy messages

			//duration
			saveDuration = DateTime.Now.Subtract( saveStarted );
			summary.Messages.Note.Add( string.Format( "Save Duration: {0:N2} seconds ", saveDuration.TotalSeconds ) );

			sa = new SiteActivity()
			{
				ActivityType = "RMTL",
				Activity = "Upload",
				Event = "Processing Completed",
				Comment = String.Format( "A bulk upload save was completed by: '{0}' for Rating: '{1}'. Duration: {2:N2} seconds .", user.FullName(), summary.RatingCodedNotation ?? "", saveDuration.TotalSeconds ),
				ActionByUserId = user.Id,
				ActionByUser = user.FullName()
			};
			new ActivityServices().AddActivity( sa );
		}
        //

		#region ProcessUpload V4 (Row by Row, but simpler)
		public static UploadableItemResult ProcessUploadedItemV4( UploadableItem item )
		{
			//Hold the result
			var result = new UploadableItemResult() { Valid = true };
			//Validate user
			AppUser user = AccountServices.GetCurrentUser();
			if ( user?.Id == 0 )
			{
				result.Errors.Add( "Error - a current user was not found. You must authenticated and authorized to use this function!" );
				return result;
			}

			//Get the current transaction's summary, or create it if it doesn't exist
			var summary = GetCachedChangeSummary( item.TransactionGUID );
			if ( summary == null )
			{
				//Lookup the rating
				var rating = RatingManager.Get( item.RatingRowID );
				if( rating == null || rating.Id == 0 )
				{
					result.Errors.Add( "The selected rating was not found." );
					return result;
				}

				//Create a new summary object
				summary = new ChangeSummary() { RowId = item.TransactionGUID, RatingRowId = rating.RowId, RatingCodedNotation = rating.CodedNotation };
				summary.UploadStarted = DateTime.Now;

				//Pre-populate controlled vocabularies, since these will be used across all rows
				summary.LookupGraph.AddRange( ConceptSchemeManager.GetAllConcepts( true ) );
				summary.LookupGraph.AddRange( RatingManager.GetAll() );
				summary.LookupGraph.AddRange( OrganizationManager.GetAll() );

				//Cache the summary object
				CacheChangeSummary( summary );

				//Add an activity for the start of a new upload
				SiteActivity sa = new SiteActivity()
                {
                    ActivityType = "RMTL",
                    Activity = "Upload",
                    Event = "Starting",
                    Comment = String.Format( "A bulk upload was initiated by: '{0}' for Rating: '{1}'.", user.FullName(), rating.CodedNotation ),
                    ActionByUserId = user.Id,
                    ActionByUser = user.FullName()
                };
                new ActivityServices().AddActivity( sa );
            }

			//Can't do this now, due to the row by row nature of this method
			//Needs to be done by client? 
			//Could put it in the ChangeSummary
			DateTime sectionStarted = DateTime.Now;
			var sectionDuration = new TimeSpan();

			//Do something with the item and summary.  
			//The summary will be stored in the cache and retrieved again so that it can be used for all of the rows. 
			//At the end of the process, the summary will contain a list of updated entities (all changes applied) that should be ready to save to the database

			//Processing
			//Validation - Checks that should skip processing the row's data if there is an error

			//Must have a valid row Unique Identifier
			if ( string.IsNullOrWhiteSpace( item.Row.Row_CodedNotation ) || UtilityManager.IsInteger( item.Row.Row_CodedNotation ) )
			{
				result.Errors.Add( "Invalid row Unique Identifier: " + ( string.IsNullOrWhiteSpace( item.Row.Row_CodedNotation ) ? "(Empty)" : item.Row.Row_CodedNotation ) );
				result.Errors.Add( "A valid row Unique Identifier (e.g., \"NEC1-006\") must be provided, and must not be a number." );
				return result;
			}

			#region Part I  - components check
			//Get the controlled value items that show up in this row
			//Everything in any uploaded sheet should appear here. If any of these are not found, it's an error
			var rowRating = GetDataOrError<Rating>( summary, ( m ) => 
				m.CodedNotation?.ToLower() == item.Row.Rating_CodedNotation?.ToLower(), 
				result, 
				"Rating indicated by this row was not found in database: \"" + TextOrNA( item.Row.Rating_CodedNotation ) + "\"",
				item.Row.Rating_CodedNotation
			);
			var selectedRating = GetDataOrError<Rating>( summary, ( m ) => 
				m.RowId == item.RatingRowID, 
				result, 
				"Selected Rating not found in database: \"" + item.RatingRowID + "\"",
				item.RatingRowID.ToString()
			);
			var rowPayGrade = GetDataOrError<Concept>( summary, ( m ) => 
				m.CodedNotation?.ToLower() == item.Row.PayGradeType_CodedNotation?.ToLower(), 
				result, 
				"Rank not found in database: \"" + TextOrNA( item.Row.PayGradeType_CodedNotation ) + "\"",
				item.Row.PayGradeType_CodedNotation
			);
			//WorkElementType
			var rowSourceType = GetDataOrError<Concept>( summary, ( m ) => 
				m.WorkElementType?.ToLower() == item.Row.Shared_ReferenceType?.ToLower(), 
				result, 
				"Work Element Type not found in database: \"" + TextOrNA( item.Row.Shared_ReferenceType ) + "\"",
				item.Row.Shared_ReferenceType
			);
			var rowTaskApplicabilityType = GetDataOrError<Concept>( summary, ( m ) => 
				m.Name?.ToLower() == item.Row.RatingTask_ApplicabilityType_Name?.ToLower(), 
				result, 
				"Task Applicability Type not found in database: \"" + TextOrNA( item.Row.RatingTask_ApplicabilityType_Name ) + "\"",
				item.Row.RatingTask_ApplicabilityType_Name
			);
			var rowTrainingGapType = GetDataOrError<Concept>( summary, ( m ) => 
				m.Name?.ToLower() == item.Row.RatingTask_TrainingGapType_Name?.ToLower(), 
				result, 
				"Training Gap Type not found in database: \"" + TextOrNA( item.Row.RatingTask_TrainingGapType_Name ) + "\"",
				item.Row.RatingTask_TrainingGapType_Name
			);

			//If any of the above are null, log an error and skip the rest
			if ( new List<object>() { rowRating, rowPayGrade, rowSourceType, rowTaskApplicabilityType, rowTrainingGapType, selectedRating }.Where( m => m == null ).Count() > 0 )
			{
				result.Errors.Add( "One or more controlled vocabulary values was not found in the database. Processing this row cannot continue." );
				return result;
			}

			//Always use the Rating selected in the interface
			if( rowRating.RowId != selectedRating.RowId )
			{
				result.Errors.Add( "The Rating for this row (" + rowRating.CodedNotation + " - " + rowRating.Name + ") does not match the Rating selected for this upload (" + selectedRating.CodedNotation + " - " + selectedRating.Name + "). Processing this row cannot continue." );
				//rowRating = selectedRating;
				return result;
			}

			//TBD - at some point we will use the following combo for RatingTask codedNotation
			//We may need to ensure that the unique ID doesn't already include the rating code
			var ratingRatingTask_CodedNotation = string.Format( "{0}-{1}", rowRating.CodedNotation, item.Row.Row_CodedNotation );

			#endregion
			
			#region Part II - components check
			//These should throw an error if not found, unless all of the course/training columns are N/A
			var shouldNotHaveTrainingData = rowTrainingGapType.Name?.ToLower() == "yes";
			var hasCourseAndTrainingData = false;
			var rowCourseType = GetDataOrError<Concept>( summary, ( m ) => 
				m.SchemeUri == ConceptSchemeManager.ConceptScheme_CourseType &&
				m.Name?.ToLower() == item.Row.Course_CourseType_Name?.ToLower(), 
				result, 
				"Course Type not found in database: \"" + TextOrNA( item.Row.Course_CourseType_Name ) + "\"", 
				item.Row.Course_CourseType_Name 
			);
			var rowOrganizationCCA = GetDataOrError<Organization>( summary, ( m ) =>
				( !string.IsNullOrWhiteSpace( m.Name ) && m.Name.ToLower() == item.Row.Course_CurriculumControlAuthority_Name?.ToLower() ) ||
				( !string.IsNullOrWhiteSpace( m.ShortName ) && m.ShortName.ToLower() == item.Row.Course_CurriculumControlAuthority_Name?.ToLower() ),
				result,
				"Curriculum Control Authority not found in database: \"" + TextOrNA( item.Row.Course_CurriculumControlAuthority_Name ) + "\"",
				item.Row.Course_CurriculumControlAuthority_Name
			);
			var rowCourseLCCDType = GetDataOrError<Concept>( summary, ( m ) =>
				m.SchemeUri == ConceptSchemeManager.ConceptScheme_LifeCycleControlDocument &&
				(
					( !string.IsNullOrWhiteSpace( m.Name ) && m.Name?.ToLower() == item.Row.Course_LifeCycleControlDocumentType_CodedNotation?.ToLower() ) ||
					( !string.IsNullOrWhiteSpace( m.CodedNotation ) && m.CodedNotation?.ToLower() == item.Row.Course_LifeCycleControlDocumentType_CodedNotation?.ToLower() )
				),
				result,
				"Life-Cycle Control Document Type not found in database: \"" + TextOrNA( item.Row.Course_LifeCycleControlDocumentType_CodedNotation ) + "\"",
				item.Row.Course_LifeCycleControlDocumentType_CodedNotation
			);
			var rowAssessmentMethodTypeList = GetDataListOrError<Concept>( summary, ( m ) => 
				SplitAndTrim( item.Row.Course_AssessmentMethodType_Name?.ToLower(), "," ).Contains( m.Name?.ToLower() ), 
				result, 
				"Assessment Method Type not found in database: \"" + TextOrNA( item.Row.Course_AssessmentMethodType_Name ) + "\"",
				item.Row.Course_AssessmentMethodType_Name
			);

			//If the Training Gap Type is "Yes", then treat all course/training data as null, but check to see if it exists first (above) to facilitate the warning statement below
			//TBD - shouldn't the course coded notation be included? item.Row.Course_CodedNotation
			if ( shouldNotHaveTrainingData )
			{
				var hasDataWhenItShouldNot = new List<object>() { rowCourseType, rowOrganizationCCA, rowCourseLCCDType }.Concat( rowAssessmentMethodTypeList ).Where( m => m != null ).ToList();
				if ( hasDataWhenItShouldNot.Count() > 0 || !string.IsNullOrWhiteSpace( item.Row.TrainingTask_Description ) )
				{
					result.Warnings.Add( String.Format( "{0}. Incomplete course/training data found. All course/training related columns should either have data or be marked as \"N/A\". Since the Training Gap Type is \"Yes\", the incomplete data will be treated as \"N/A\".", item.Row?.Row_CodedNotation ) );
					rowCourseType = null;
					rowOrganizationCCA = null;
					rowCourseLCCDType = null;
					rowAssessmentMethodTypeList = new List<Concept>();
					item.Row.TrainingTask_Description = "";
				}

				//Remove false errors
				result.Errors = new List<string>();
			}
			//Otherwise, return an error if any course/training data is missing
			else if ( 
				new List<object>() { rowCourseType, rowOrganizationCCA, rowCourseLCCDType }.Where( m => m == null ).Count() > 0 || 
				new List<string>() { item.Row.TrainingTask_Description, item.Row.Course_CodedNotation, item.Row.Course_Name }.Where(m => string.IsNullOrWhiteSpace( m ) ).Count() > 0 || 
				rowAssessmentMethodTypeList.Count() == 0 || 
				result.Errors.Count() > 0 )
			{
				if ( string.IsNullOrWhiteSpace( item.Row.TrainingTask_Description ) )
				{
					result.Errors.Add( "Training Task data is missing for this row." );
				}
				result.Errors.Add( "Incomplete course/training data found. All course/training related columns should either have data or be marked as \"N/A\". Since the Training Gap Type is \"" + rowTrainingGapType.Name + "\", this is an error and processing this row cannot continue." );
				return result;
			}
			//Otherwise, look for course/training data later on
			else
			{
				hasCourseAndTrainingData = true;
			}

			#endregion

			#region Part III  - components check
			//Cluster Analysis
			var hasClusterAnalysisData = false;
			var rowTrainingSolutionType = GetDataOrError<Concept>( summary, ( m ) => 
				m.SchemeUri == ConceptSchemeManager.ConceptScheme_TrainingSolutionType && 
				m.Name?.ToLower() == item.Row.Training_Solution_Type?.ToLower(), 
				result, 
				"Training Solution Type not found in database: \"" + TextOrNA( item.Row.Training_Solution_Type ) + "\"", 
				item.Row.Training_Solution_Type,
				false
			);
			var rowRecommendModalityType = GetDataOrError<Concept>( summary, ( m ) => 
				m.SchemeUri == ConceptSchemeManager.ConceptScheme_RecommendedModality && 
				( m.Name?.ToLower() == item.Row.Recommended_Modality?.ToLower() || m.CodedNotation?.ToLower() == item.Row.Recommended_Modality?.ToLower() ), 
				result, 
				"Recommended Modality Type not found in database: \"" + TextOrNA( item.Row.Recommended_Modality ) + "\"", 
				item.Row.Recommended_Modality,
				false
			);
			var rowDevelopmentSpecificationType = GetDataOrError<Concept>( summary, ( m ) => 
				m.SchemeUri == ConceptSchemeManager.ConceptScheme_DevelopmentSpecification && 
				m.Name?.ToLower() == item.Row.Development_Specification?.ToLower(), 
				result, 
				"Development Specification Type not found in database: \"" + TextOrNA( item.Row.Development_Specification ) + "\"", 
				item.Row.Development_Specification,
				false
			);
			var priorityPlacement = UtilityManager.MapIntegerOrDefault( item.Row.Priority_Placement );
			var developmentTime = UtilityManager.MapIntegerOrDefault( item.Row.Development_Time );
			var estimatedInstructionalTime = (decimal?) UtilityManager.MapDecimalOrDefault( item.Row.Estimated_Instructional_Time );

			//If errors/warnings should happen due to Cluster Analysis data, do so here
			//Return here before the row is processed if row processing should not occur
			if ( priorityPlacement > 9 )
			{
				result.Warnings.Add( string.Format( "Priority Placement ({0}) is invalid. valid values are 1 through 9.", priorityPlacement ) );
			}

			#endregion

			#region Additional validation checks
			//Additional validation checks after all of the vocabularies have been figured out
			if ( UtilityManager.GetAppKeyValue( "doingRatingTaskDuplicateChecks", true ) )
			{
				//Check for duplicate Rating Task (minus Row Unique Identifier) from a previous row
				//Need to determine the task source and billet title for this row in order for the checks to work properly
				var tempRatingTaskSource = summary.GetAll<ReferenceResource>()
					.FirstOrDefault( m =>
						m.Name?.ToLower() == item.Row.ReferenceResource_Name?.ToLower() &&
						m.PublicationDate?.ToLower() == ParseDateOrValue( item.Row.ReferenceResource_PublicationDate?.ToLower()) &&
						m.ReferenceType.Contains( rowSourceType.RowId )
					) ??
					ReferenceResourceManager.Get( item.Row.ReferenceResource_Name, item.Row.ReferenceResource_PublicationDate ) ??
					new ReferenceResource();

				var tempBilletTitle = summary.GetAll<BilletTitle>().FirstOrDefault( m => m.Name.ToLower() == item.Row.BilletTitle_Name?.ToLower() ) ??
					JobManager.GetByName( item.Row.BilletTitle_Name ) ??
					new BilletTitle();

				var existingPreviousRatingTask = summary.GetAll<RatingTask>()
					.FirstOrDefault( m =>
						m.Description?.ToLower() == item.Row.RatingTask_Description?.ToLower() &&
						m.ApplicabilityType == rowTaskApplicabilityType.RowId &&
						m.TrainingGapType == rowTrainingGapType.RowId &&
						m.PayGradeType == rowPayGrade.RowId &&
						m.ReferenceType == rowSourceType.RowId &&
						m.HasReferenceResource == tempRatingTaskSource.RowId &&
						( UtilityManager.GetAppKeyValue( "includingBilletTitleInDuplicatesChecks", false ) ? m.HasBilletTitle.Contains( tempBilletTitle.RowId ) : true )
					);

				if ( existingPreviousRatingTask != null )
				{
					if ( UtilityManager.GetAppKeyValue( "treatingRatingTaskDuplicateAsError", true ) )
					{
						result.Errors.Add( string.Format( "For Unique Identifier: '{0}' the Rating Task data is the same as for a previous row with Unique Identifier: {1}.", item.Row.Row_CodedNotation, existingPreviousRatingTask.CodedNotation ) );
						result.Errors.Add( "Duplicate Rating Tasks are not allowed. Processing this row cannot continue." );
						return result;
					}
					else
					{
						result.Warnings.Add( string.Format( "For Unique Identifier: '{0}' the Rating Task data is the same as for a previous row with Unique Identifier: {1}.", item.Row.Row_CodedNotation, existingPreviousRatingTask.CodedNotation ) );
					}
				}
			}

			//If there are any errors, return before the rest of the row's data is processed
			if( result.Errors.Count() > 0 )
			{
				result.Errors.Add( "One or more critical errors were encountered while processing this row." );
				return result;
			}

			#endregion

			#region After Validation, process the row's contents
			//Get the variable items that show up in this row
			//Things from sheets here may be new/edited
			//First check the summary to see if it came in from an earlier row (or database). If not found in the summary, check the database. If not found there, assume it's new. Regardless, add it to the summary if it's not already in the summary.
			//When creating a new instance of a class, only include the properties that should never change after its creation (ie the ones used to look it up).
			//Properties that can be updated by subsequent rows or subsequent uploads should go in the next section instead

			//Billet Title
			var rowBilletTitle = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<BilletTitle>()
				.FirstOrDefault( m =>
					m.Name?.ToLower() == item.Row.BilletTitle_Name?.ToLower()
				),
				//Or get from DB
				() => JobManager.GetByName( item.Row.BilletTitle_Name ),
				//Or create new
				() => new BilletTitle()
				{
					RowId = Guid.NewGuid(),
					Name = item.Row.BilletTitle_Name
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.BilletTitle.Add( newItem ); }
			);

			//Work Role
			var rowWorkRole = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<WorkRole>()
				.FirstOrDefault( m =>
					m.Name?.ToLower() == item.Row.WorkRole_Name?.ToLower()
				),
				//Or get from DB
				() => WorkRoleManager.Get( item.Row.WorkRole_Name ),
				//Or create new
				() => new WorkRole()
				{
					RowId = Guid.NewGuid(),
					Name = item.Row.WorkRole_Name
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.WorkRole.Add( newItem ); }
			);
			//NAVPERS 18068F Vol II, NAVEDTRA 43119-M (303 - Basic Firefighting)
			if ( item.Row.ReferenceResource_Name == "NAVPERS 18068F Vol II" 
				|| item.Row.ReferenceResource_Name == "NAVEDTRA 43119-M (303 - Basic Firefighting)" )
			{

            }

			//Reference Resource
			var rowRatingTaskSource = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<ReferenceResource>()
				.FirstOrDefault( m =>
					m.Name?.ToLower() == item.Row.ReferenceResource_Name?.ToLower() &&
					m.PublicationDate?.ToLower() == ParseDateOrValue( item.Row.ReferenceResource_PublicationDate?.ToLower())
					//&& m.ReferenceType.Contains( rowSourceType.RowId )
				),
				//Or get from DB
				() => ReferenceResourceManager.Get( item.Row.ReferenceResource_Name, item.Row.ReferenceResource_PublicationDate ), //Should ReferenceType be used here as well?
				//Or create new
				() => new ReferenceResource()
				{
					RowId = Guid.NewGuid(),
					Name = item.Row.ReferenceResource_Name,
					PublicationDate = ParseDateOrValue( item.Row.ReferenceResource_PublicationDate )
					//Other properties are handled in the next section
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.ReferenceResource.Add( newItem ); }
			);

			//Training Task
			//TODO - if we associtate the rating task coded notation with the training task for the row, we should be able to identify changes to the training task.
			//		 - this can be done with RatingTask_HasTrainingTask in the trainingTask manager
			var rowTrainingTask = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<TrainingTask>()
				.FirstOrDefault( m =>
					m.Description?.ToLower() == item.Row.TrainingTask_Description?.ToLower() &&
					m.CourseCodedNotation.ToLower() == item.Row?.Course_CodedNotation.ToLower()
				),
				//Or get from DB
				//22-04-16 just description is not enough. We want the previous one even if the description has changed.
				//		NOTE: ensure there is a check so that if the training description has changed, there will be an update request
				() => TrainingTaskManager.GetTrainingTaskForRatingTask( rowRating.CodedNotation, item.Row.Row_CodedNotation, item.Row.Course_CodedNotation, item.Row.TrainingTask_Description, ref summary ),
				//() => TrainingTaskManager.Get( item.Row.TrainingTask_Description ),
				//Or create new
				() => new TrainingTask()
				{
					RowId = Guid.NewGuid(),
					Description = item.Row.TrainingTask_Description,
					CourseCodedNotation = item.Row.Course_CodedNotation
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.TrainingTask.Add( newItem ); },
				//Skip all of this and set value to null if the following test is true
				() => !hasCourseAndTrainingData || string.IsNullOrWhiteSpace( item.Row.TrainingTask_Description ) || item.Row.TrainingTask_Description.ToLower() == "n/a"
			);

			//Course
			var rowCourse = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<Course>()
				.FirstOrDefault( m =>
					 m.CodedNotation?.ToLower() == item.Row.Course_CodedNotation?.ToLower()
				),
				//Or get from DB
				() => CourseManager.GetByCodedNotation( item.Row.Course_CodedNotation, false ),
				//Or create new
				() => new Course()
				{
					RowId = Guid.NewGuid(),
					CodedNotation = item.Row.Course_CodedNotation
					//Other properties are handled in the next section
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.Course.Add( newItem ); },
				//Skip all of this and set value to null if the following test is true
				() => !hasCourseAndTrainingData || string.IsNullOrWhiteSpace( item.Row.Course_Name ) || item.Row.Course_Name.ToLower() == "n/a"
			);

			//Rating Task
			var rowRatingTask = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary - if found would be a duplicate
				() => null, //Force it to either be looked up from the database or created new, per decision on 2022-4-12 (Each row = one unique rating task, regardless of anything else)
				//Or get from DB
				() => UtilityManager.GetAppKeyValue( "ratingTaskUsingCodedNotationForLookups", false ) ?
					RatingTaskManager.GetForUpload( rowRating.CodedNotation, item.Row.Row_CodedNotation ) :
					RatingTaskManager.GetForUpload( rowRating.Id, item.Row.RatingTask_Description, rowTaskApplicabilityType.RowId, rowRatingTaskSource.RowId, rowSourceType.RowId, rowPayGrade.RowId, rowTrainingGapType.RowId ),
				//Or create new
				() => new RatingTask()
				{
					RowId = Guid.NewGuid(),
					CodedNotation = item.Row.Row_CodedNotation
					//Other properties are handled in the next section
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.RatingTask.Add( newItem ); }
			);

			//Cluster Analysis
			var rowClusterAnalysis = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<ClusterAnalysis>()
				.FirstOrDefault( m =>
					 m.RatingTaskRowId == rowRatingTask.RowId
				),
				//Or get from DB
				() => ClusterAnalysisManager.GetForUpload( rowRatingTask.RowId ),
				//Or create new
				() => new ClusterAnalysis()
				{
					RowId = Guid.NewGuid(),
					RatingTaskRowId = rowRatingTask.RowId
					//Other properties are handled in the next section
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.ClusterAnalysis.Add( newItem ); },
				//Skip all of this and set value to null if the following test is true
				//May be exceptions
				() => string.IsNullOrWhiteSpace( item.Row.Cluster_Analysis_Title ) || item.Row.Cluster_Analysis_Title.ToLower() == "n/a"
			);

            #endregion

            #region Update entities with data
            //Now that all of the actors for this row have been found, created, and tracked...
			//Update them with the data from this row

            //Billet Title
            HandleGuidListAddition( summary, summary.ItemsToBeCreated.BilletTitle, summary.FinalizedChanges.BilletTitle, result, rowBilletTitle, nameof( BilletTitle.HasRatingTask ), rowRatingTask );

			//Work Role
			//No changes to track!

			//Reference Resource
			HandleGuidListAddition( summary, summary.ItemsToBeCreated.ReferenceResource, summary.FinalizedChanges.ReferenceResource, result, rowRatingTaskSource, nameof( ReferenceResource.ReferenceType ), rowSourceType );

			//Training Task
			foreach ( var rowAssessmentMethodType in rowAssessmentMethodTypeList )
			{
				HandleGuidListAddition( summary, summary.ItemsToBeCreated.TrainingTask, summary.FinalizedChanges.TrainingTask, result, rowTrainingTask, nameof( TrainingTask.AssessmentMethodType ), rowAssessmentMethodType );
			}
			HandleValueChange( summary, summary.ItemsToBeCreated.TrainingTask, summary.FinalizedChanges.TrainingTask, result, rowTrainingTask, nameof( TrainingTask.Description ), item.Row.TrainingTask_Description );

			//Course
			HandleGuidListAddition( summary, summary.ItemsToBeCreated.Course, summary.FinalizedChanges.Course, result, rowCourse, nameof( Course.HasTrainingTask ), rowTrainingTask );
			HandleGuidListAddition( summary, summary.ItemsToBeCreated.Course, summary.FinalizedChanges.Course, result, rowCourse, nameof( Course.CourseType ), rowCourseType );
			HandleGuidListAddition( summary, summary.ItemsToBeCreated.Course, summary.FinalizedChanges.Course, result, rowCourse, nameof( Course.CurriculumControlAuthority ), rowOrganizationCCA );
			HandleValueChange( summary, summary.ItemsToBeCreated.Course, summary.FinalizedChanges.Course, result, rowCourse, nameof( Course.LifeCycleControlDocumentType ), ( rowCourseLCCDType ?? new Concept() ).RowId );
			HandleValueChange( summary, summary.ItemsToBeCreated.Course, summary.FinalizedChanges.Course, result, rowCourse, nameof( Course.Name ), item.Row.Course_Name );

			//Rating Task
			HandleGuidListAddition( summary, summary.ItemsToBeCreated.RatingTask, summary.FinalizedChanges.RatingTask, result, rowRatingTask, nameof( RatingTask.HasRating ), rowRating );
			HandleGuidListAddition( summary, summary.ItemsToBeCreated.RatingTask, summary.FinalizedChanges.RatingTask, result, rowRatingTask, nameof( RatingTask.HasBilletTitle ), rowBilletTitle );
			HandleGuidListAddition( summary, summary.ItemsToBeCreated.RatingTask, summary.FinalizedChanges.RatingTask, result, rowRatingTask, nameof( RatingTask.HasWorkRole ), rowWorkRole );
			HandleGuidListAddition( summary, summary.ItemsToBeCreated.RatingTask, summary.FinalizedChanges.RatingTask, result, rowRatingTask, nameof( RatingTask.HasTrainingTaskList ), rowTrainingTask );
			HandleValueChange( summary, summary.ItemsToBeCreated.RatingTask, summary.FinalizedChanges.RatingTask, result, rowRatingTask, nameof( RatingTask.Description ), item.Row.RatingTask_Description );
			HandleValueChange( summary, summary.ItemsToBeCreated.RatingTask, summary.FinalizedChanges.RatingTask, result, rowRatingTask, nameof( RatingTask.PayGradeType ), ( rowPayGrade ?? new Concept() ).RowId );
			HandleValueChange( summary, summary.ItemsToBeCreated.RatingTask, summary.FinalizedChanges.RatingTask, result, rowRatingTask, nameof( RatingTask.ApplicabilityType ), ( rowTaskApplicabilityType ?? new Concept() ).RowId );
			HandleValueChange( summary, summary.ItemsToBeCreated.RatingTask, summary.FinalizedChanges.RatingTask, result, rowRatingTask, nameof( RatingTask.TrainingGapType ), ( rowTrainingGapType ?? new Concept() ).RowId );
			HandleValueChange( summary, summary.ItemsToBeCreated.RatingTask, summary.FinalizedChanges.RatingTask, result, rowRatingTask, nameof( RatingTask.ReferenceType ), ( rowSourceType ?? new Concept() ).RowId );
			HandleValueChange( summary, summary.ItemsToBeCreated.RatingTask, summary.FinalizedChanges.RatingTask, result, rowRatingTask, nameof( RatingTask.HasReferenceResource ), ( rowRatingTaskSource ?? new ReferenceResource() ).RowId );
			HandleValueChange( summary, summary.ItemsToBeCreated.RatingTask, summary.FinalizedChanges.RatingTask, result, rowRatingTask, nameof( RatingTask.Note ), item.Row.Note ?? "" );

			//Cluster Analysis
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.ClusterAnalysisTitle ), item.Row.Cluster_Analysis_Title );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.RatingTaskRowId ), (rowRatingTask ?? new RatingTask()).RowId );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.TrainingSolutionType ), item.Row.Training_Solution_Type );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.TrainingSolutionTypeId ), ( rowTrainingSolutionType ?? new Concept() ).Id );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.RecommendedModality ), item.Row.Recommended_Modality );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.RecommendedModalityId ), ( rowRecommendModalityType ?? new Concept() ).Id );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.DevelopmentSpecification ), item.Row.Development_Specification );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.DevelopmentSpecificationId ), ( rowDevelopmentSpecificationType ?? new Concept() ).Id );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.CandidatePlatform ), item.Row.Candidate_Platform );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.CFMPlacement ), item.Row.CFM_Placement );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.PriorityPlacement ), priorityPlacement );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.DevelopmentRatio ), item.Row.Development_Ratio );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.DevelopmentTime ), developmentTime );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.EstimatedInstructionalTime ), estimatedInstructionalTime );
			HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.Notes ), item.Row.Cluster_Analysis_Notes ?? "" );

            #endregion

            //Update the cached summary
            CacheChangeSummary( summary );

			sectionDuration = DateTime.Now.Subtract( sectionStarted );
			//summary.Messages.Note.Add( string.Format( "Duration: {0:N2} seconds ", sectionDuration.TotalSeconds ) );

			//Return the result
			return result;
		}
		//

		public static T LookupOrGetFromDBOrCreateNew<T>( ChangeSummary summary, UploadableItemResult result, Func<T> GetFromSummary, Func<T> GetFromDatabase, Func<T> CreateNew, Action<T> LogNewItem, Func<bool> ReturnNullIfTrue = null ) where T : BaseObject, new()
		{
			//If there is a method to check whether the item is null (e.g. the cell contains N/A), and that test comes back true, then skip the rest and return null
			if ( ReturnNullIfTrue != null && ReturnNullIfTrue() )
			{
				return null;
			}

			//Otherwise, find it in the summary
			var targetItem = GetFromSummary();

			//If not found in the summary...
			if ( targetItem == null )
			{
				//Find it in the database
				targetItem = GetFromDatabase();

				//If not found in the database...
				if ( targetItem == null || targetItem.Id == 0 )
				{
					//Create a new one (if applicable)
					targetItem = CreateNew();

					//If newly created, put it into the summary's new items list
					LogNewItem( targetItem );

					//Also put it into the result's new items list. The client will handle the fact that data gets appended to this item later (in this and/or subsequent rows) after it's been serialized as a JObject.
					result.NewItems.Add( JObjectify( targetItem ) );
				}
				//If found in the database...
				else
				{
					//Note that it came from the database to help code distinguish between existing items and items from previous rows in the current upload session
					summary.ItemsLoadedFromDatabase.Add( targetItem.RowId );

					//Also put it into the result's existing items list
					result.ExistingItems.Add( JObjectify( targetItem ) );
				}

				//If it's not null, put it into the summary's lookup graph
				if ( targetItem != null )
				{
					summary.AppendItem( targetItem );
				}
			}

			//Track it in the row regardless of whether or not it's already in the summary (helps the client figure out which items showed up on which rows at the end)
			if ( targetItem != null )
			{
				result.RowItems.Add( targetItem.RowId );
			}

			//Return the item (or null)
			return targetItem;
		}
		//

		public static JObject JObjectify<T>( T rowItem ) where T : BaseObject
		{
			if ( rowItem != null )
			{
				var newItem = JObject.FromObject( rowItem, new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None, DefaultValueHandling = DefaultValueHandling.Ignore } );
				newItem[ "@type" ] = rowItem.GetType().Name;
				return newItem;
			}

			return null;
		}
		//

		public static T GetDataOrError<T>( ChangeSummary summary, Func<T, bool> MatchWith, UploadableItemResult result, string errorMessage, string source, bool addErrorIfNullOrWhiteSpace = true ) where T : BaseObject
		{
			return GetDataListOrError( summary, MatchWith, result, errorMessage, source, addErrorIfNullOrWhiteSpace ).FirstOrDefault();
		}
		//

		public static List<T> GetDataListOrError<T>( ChangeSummary summary, Func<T, bool> MatchWith, UploadableItemResult result, string errorMessage, string source, bool addErrorIfNullOrWhiteSpace = true ) where T : BaseObject
		{
			if ( string.IsNullOrWhiteSpace( source ) )
			{
				if ( addErrorIfNullOrWhiteSpace )
				{
					result.Errors.Add( errorMessage );
				}
				return new List<T>();
			}

			//Check for existence in LookupGraph
			var data = summary.GetAll<T>().Where( m => MatchWith( m ) ).ToList();
			if ( data.Count() == 0 )
			{
				result.Errors.Add( errorMessage );
			}

			return data;
		}
		//

		public static List<string> SplitAndTrim( string text, string splitOn )
		{
			return SplitAndTrim( text, new List<string>() { splitOn } );
		}
		public static List<string> SplitAndTrim(string text, List<string> splitOn )
		{
			return text == null ? new List<string>() : text.Split( splitOn.ToArray(), StringSplitOptions.RemoveEmptyEntries ).Select( m => m.Trim() ).ToList();
		}
		//

		private static void HandleGuidListAddition<T1, T2>( ChangeSummary summary, List<T1> itemsToBeCreatedList, List<T1> finalizedChangesList, UploadableItemResult result, T1 rowItem, string propertyName, T2 referenceToBeAppended ) where T1 : BaseObject where T2 : BaseObject
		{
			//Skip if either the rowItem or the referenceToBeAppended are null
			if( rowItem == null || referenceToBeAppended == null )
			{
				return;
			}

			//Get the property to check/update
			var property = typeof( T1 ).GetProperty( propertyName );

			//Get the current value for that property
			var currentValueList = ( List<Guid> ) property.GetValue( rowItem );

			//If the current list of GUIDs does not contain the target GUID...
			if ( !currentValueList.Contains( referenceToBeAppended.RowId ) )
			{
				//Add it to the list and update the value
				//The magic of passing by reference means that this will update the object in the summary
				currentValueList.Add( referenceToBeAppended.RowId );
				property.SetValue( rowItem, currentValueList );

				//Track the addition in the result
				//This will be aggregated client-side into the new item's data
				result.Additions.Add( new Triple( rowItem.RowId, propertyName, referenceToBeAppended.RowId ) );

				//Update the finalized changes list if applicable
				ReplaceItemInChangeTrackingLists( summary, rowItem, itemsToBeCreatedList, finalizedChangesList );
			}
		}
		//

		private static void HandleValueChange<T1, T2>( ChangeSummary summary, List<T1> itemsToBeCreatedList, List<T1> finalizedChangesList, UploadableItemResult result, T1 rowItem, string propertyName, T2 newValue ) where T1 : BaseObject
		{
			//Skip if the rowItem is null
			if( rowItem == null )
			{
				return;
			}

			//Get the property to check/update
			var property = typeof( T1 ).GetProperty( propertyName );

			//Get the current value for that property
			var currentValue = ( T2 ) property.GetValue( rowItem );

			//If both values are null/whitespace or both values are the same, do nothing and return
			if ( ( string.IsNullOrWhiteSpace( currentValue?.ToString() ) && string.IsNullOrWhiteSpace( newValue?.ToString() ) ) || ( currentValue?.ToString().ToLower() == newValue?.ToString().ToLower() ) )
			{
				return;
			}

			//If the new value is null, warn the user
			if( newValue == null || (property.PropertyType == typeof(Guid) && newValue?.ToString() == Guid.Empty.ToString()) )
			{
				result.Warnings.Add( "New value for property " + propertyName + " is null or empty!" );
			}

			//Set the value
			//This will update the rowItem in the summary
			property.SetValue( rowItem, newValue );

			//Track the change in the result
			if( property.PropertyType == typeof( Guid ) )
			{
				var guidValue = Guid.Empty;
				if( Guid.TryParse(newValue?.ToString(), out guidValue ) )
				{
					result.ReferenceChanges.Add( new Triple( rowItem.RowId, propertyName, guidValue ) );
				}
				else
				{
					result.Errors.Add( "Error parsing GUID reference for item: " + rowItem.RowId + ", property: " + propertyName + ", value: " + newValue?.ToString() );
					result.ValueChanges.Add( new Triple( rowItem.RowId, propertyName, newValue?.ToString().ToLower() ) );
				}
			}
			else
			{
				result.ValueChanges.Add( new Triple( rowItem.RowId, propertyName, newValue?.ToString() ) );
			}

			//Update the finalized changes list if applicable
			ReplaceItemInChangeTrackingLists( summary, rowItem, itemsToBeCreatedList, finalizedChangesList );
		}
		//

		private static void ReplaceItemInChangeTrackingLists<T>( ChangeSummary summary, T rowItem, List<T> itemsToBeCreatedList, List<T> finalizedChangesList ) where T : BaseObject
		{
			//Skip if null (shouldn't happen!)
			if( rowItem == null )
			{
				return;
			}

			//Remove the item from the appropriate summary list and re-add it to ensure that the summary always has the most up to date version to avoid any issues with object references not surviving caching
			//If the item already existed in the database, update the finalized changes list
			if ( summary.ItemsLoadedFromDatabase.Contains( rowItem.RowId ) )
			{
				finalizedChangesList.Remove( finalizedChangesList.FirstOrDefault( m => m.RowId == rowItem.RowId ) );
				finalizedChangesList.Add( rowItem );
			}
			//Otherwise update the new items list
			else
			{
				itemsToBeCreatedList.Remove( itemsToBeCreatedList.FirstOrDefault( m => m.RowId == rowItem.RowId ) );
				itemsToBeCreatedList.Add( rowItem );
			}

		}
		//

		private static string TextOrNA( string text )
		{
			return string.IsNullOrWhiteSpace( text ) ? "N/A" : text;
		}
		//

		private static string ParseDateOrValue( string value )
		{
			try
			{
				return DateTime.Parse( value ).ToString( "MM/dd/yyyy" );
			}
			catch
			{
				//or ""
				//return DateTime.MinValue.ToString( "MM/dd/yyyy" );
				return value;
			}
		}
		//

		#endregion

	}
}
