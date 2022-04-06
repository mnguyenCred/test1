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

namespace Services
{
	public class BulkUploadServices
	{
        
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
		public static void ApplyChangeSummary( ChangeSummary summary, ref SaveStatus status )
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
				Event = "Processing Started",
				Comment = String.Format( "A bulk upload initiated by: '{0}' for Rating: '{1}' was committed.", user.FullName(), summary.Rating ?? "" ),
				ActionByUserId = user.Id,
				ActionByUser = user.FullName()
			};
			new ActivityServices().AddActivity( sa );
			DateTime saveStarted = DateTime.Now;
			var saveDuration = new TimeSpan();
			//now what
			//go thru all non-rating task
			if (summary.ItemsToBeCreated != null)
            {
				//all dependent data has to be done first

				if ( summary.ItemsToBeCreated.Organization?.Count > 0 )
                {
					var orgMgr = new OrganizationManager();
					foreach (var item in summary.ItemsToBeCreated.Organization )
                    {
						item.CreatedById = item.LastUpdatedById = user.Id;
						orgMgr.Save( item, user.Id, ref summary );
                    }
                }
				if ( summary.ItemsToBeCreated.ReferenceResource?.Count > 0 )
				{
					var mgr = new ReferenceResourceManager();
					foreach ( var item in summary.ItemsToBeCreated.ReferenceResource )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
					//foreach ( var item in summary.ItemsToBeCreated.ReferenceResource )
					//{
					//	Navy.Utilities.LoggingHelper.DoTrace( 6, item.Name );
					//}
				}


				if ( summary.ItemsToBeCreated.Course?.Count > 0 )
				{				

					//is training task part of course, see there is a separate TrainingTask in UploadableData. the latter has no course Id/RowId to make an association?
					var courseMgr = new CourseManager();
					foreach ( var item in summary.ItemsToBeCreated.Course )
					{
						//get all tasks for this course
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
				} //if no courses, there could be training tasks?
				else
                {
					var trainTaskMgr = new TrainingTaskManager();
					//but what to do with them?
					if ( summary.ItemsToBeCreated.TrainingTask?.Count > 0 )
					{
						foreach( var item in summary.ItemsToBeCreated.TrainingTask )
                        {
							item.CreatedById = item.LastUpdatedById = user.Id;
							//there is no associate with the course? Need to store the CIN with training task
							//trainTaskMgr.Save( item, ref summary );
						}
					}
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
							LoggingHelper.DoTrace( 6, "BilletTitle: " + item.Name, false );
						}
					}
				}
				//ClusterAnalysis?? will be directly related to rating task
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
							var results = summary.ItemsToBeCreated.BilletTitle.Where( p => p.HasRatingTask.Contains(item.RowId) ).ToList();

							if ( results.Count > 0 )
							{
								item.HasBilletTitle.AddRange( results.Select( p => p.RowId ) );
							} else
                            {
								var resultsByCode = summary.ItemsToBeCreated.BilletTitle.Where( p => p.HasRatingTaskByCode.Contains( item.CodedNotation ) ).ToList();
								if( resultsByCode.Count > 0 )
								{
									//this will always be one (one per row). Alternate would be to do a set based approach after all rating tasks are created. 
									item.HasBilletTitle.AddRange( resultsByCode.Select( p => p.RowId ) );
								}
							}
						}
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
				}
			}
			if ( summary.AddedItemsToInnerListsForCopiesOfItems != null )
			{
				//what to do with these?
			}
			//changes
			//not sure how different
			if ( summary.ItemsToBeChanged != null )
			{
				if ( summary.ItemsToBeChanged.Organization?.Count > 0 )
				{
					var orgMgr = new OrganizationManager();
					foreach ( var item in summary.ItemsToBeChanged.Organization )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						orgMgr.Save( item, user.Id, ref summary );
					}
				}
				//
				if ( summary.ItemsToBeChanged.ReferenceResource?.Count > 0 )
				{
					var mgr = new ReferenceResourceManager();
					foreach ( var item in summary.ItemsToBeChanged.ReferenceResource )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
				}
				//
				if ( summary.ItemsToBeChanged.Course?.Count > 0 )
				{
					//is training task part of course, see there is a separate TrainingTask in UploadableData. the latter has no course Id/RowId to make an association?
					var courseMgr = new CourseManager();
					foreach ( var item in summary.ItemsToBeChanged.Course )
					{
						//get all tasks for this course
						if ( summary.ItemsToBeChanged.TrainingTask?.Count > 0 )
						{
							var results = summary.ItemsToBeChanged.TrainingTask.Where( p => item.HasTrainingTask.Any( p2 => p2 == p.RowId ) );
							item.TrainingTasks.AddRange( results );
						}
						//just in case, do we need to also get created?
						if ( summary.ItemsToBeCreated.TrainingTask?.Count > 0 )
						{
							var results = summary.ItemsToBeCreated.TrainingTask.Where( p => item.HasTrainingTask.Any( p2 => p2 == p.RowId ) );
							item.TrainingTasks.AddRange( results );
						}
						item.CreatedById = item.LastUpdatedById = user.Id;
						courseMgr.Save( item, ref summary );
					}
				} else
                {
					//check tasks
                }


				if ( summary.ItemsToBeChanged.WorkRole?.Count > 0 )
				{
					var mgr = new WorkRoleManager();
					foreach ( var item in summary.ItemsToBeChanged.WorkRole )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
				}

				if ( summary.ItemsToBeChanged.BilletTitle?.Count > 0 )
				{
					var mgr = new JobManager();
					foreach ( var item in summary.ItemsToBeChanged.BilletTitle )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
				}

				if ( summary.ItemsToBeChanged.RatingTask?.Count > 0 )
				{
					var mgr = new RatingTaskManager();
					foreach ( var item in summary.ItemsToBeChanged.RatingTask )
					{
						//not sure what to do for billets here?
						//get all billets for this task
						if ( summary.ItemsToBeChanged.BilletTitle?.Count > 0 )
						{
							//get billets that reference this task
							var results = summary.ItemsToBeChanged.BilletTitle.Where( p => p.HasRatingTask.Contains( item.RowId ) ).ToList();
							item.HasBilletTitle.AddRange( results.Select( p => p.RowId ) );
						}
						//do we need to check the created as well?
						if ( summary.ItemsToBeCreated.BilletTitle?.Count > 0 )
						{
							//get billets that reference this task
							var results = summary.ItemsToBeCreated.BilletTitle.Where( p => p.HasRatingTask.Contains( item.RowId ) ).ToList();
							item.HasBilletTitle.AddRange( results.Select( p => p.RowId ) );
						}
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
				}
			}


			//changes
			//not sure how different
			if ( summary.ItemsToBeDeleted != null )
			{

			}

			//copy messages

			//duration
			saveDuration = DateTime.Now.Subtract( saveStarted );
			summary.Messages.Note.Add( string.Format( "Save Duration: {0:N2} seconds ", saveDuration.TotalSeconds ) );

			sa = new SiteActivity()
			{
				ActivityType = "RMTL",
				Activity = "Upload",
				Event = "Processing Completed",
				Comment = String.Format( "A bulk upload was completed by: '{0}' for Rating: '{1}'. Duration: {2:N2} seconds .", user.FullName(), summary.Rating ?? "", saveDuration.TotalSeconds ),
				ActionByUserId = user.Id,
				ActionByUser = user.FullName()
			};
			new ActivityServices().AddActivity( sa );
		}
        //

        #region ProcessUpload V2
        public static ChangeSummary ProcessUploadV2( UploadableTable uploadedData, Guid ratingRowID, JObject debug = null )
		{
			//Hold data
			debug = debug ?? new JObject();
			var latestStepFlag = "FarthestStep";
			var summary = new ChangeSummary() { RowId = Guid.NewGuid() };
			int totalRows = 0;
			debug[ latestStepFlag ] = "Initial Setup";

			//Validate selected Rating
			var currentRating = RatingManager.Get( ratingRowID );
			if ( currentRating == null )
			{
				summary.Messages.Error.Add( "Error: Unable to find Rating for identifier: " + ratingRowID );
				return summary;
			}
			AppUser user = AccountServices.GetCurrentUser();
			if ( user?.Id == 0 )
			{
				summary.Messages.Error.Add( "Error - a current user was not found. You must authenticated and authorized to use this function!" );
				return summary;
			}
			//Valiate row count
			if ( uploadedData.Rows.Count == 0 )
			{
				summary.Messages.Error.Add( "Error: No rows were found to process." );
				return summary;
			}
			//save input file
			if ( uploadedData.RawCSV?.Length > 0 )
			{
				LoggingHelper.WriteLogFile( 1, string.Format( "Rating_upload_{0}_{1}.csv", currentRating.Name.Replace(" ","_"), DateTime.Now.ToString( "hhmmss" ) ), uploadedData.RawCSV, "", false );

				new BaseFactory().BulkLoadRMTL( currentRating.CodedNotation, uploadedData.RawCSV );
			}
			//Filter out rows that don't match the selected rating
			var nonMatchingRows = uploadedData.Rows.Where( m => m.Rating_CodedNotation?.ToLower() != currentRating.CodedNotation.ToLower() ).ToList();
			uploadedData.Rows = uploadedData.Rows.Where( m => !nonMatchingRows.Contains( m ) ).ToList();
			if( nonMatchingRows.Count() > 0 )
			{
				var nonMatchingCodes = nonMatchingRows.Select( m => m.Rating_CodedNotation ).Distinct().ToList();
				foreach( var code in nonMatchingCodes )
				{
					if ( !string.IsNullOrWhiteSpace( code ) )
					{
						summary.Messages.Warning.Add( "Detected " + nonMatchingRows.Where( m => m.Rating_CodedNotation == code ).Count() + " rows that did not match the selected Rating (" + currentRating.CodedNotation + ") and instead were for Rating: \"" + code + "\". These rows have been ignored." );
					}
				}
			}
			//extra check after matching check - if there is only one header row, willfail
			if ( uploadedData.Rows.Count == 0 )
			{
				summary.Messages.Error.Add( "Error: No rows were found to process after doing a match on Rating. This can occur if there is only one header row. The system expects two header rows. OR if the Rating selected from the interface is different from the Rating in the uploaded file." );
				return summary;
			}
			DateTime saveStarted = DateTime.Now;
			var saveDuration = new TimeSpan();

			LoggingHelper.DoTrace( 6, string.Format( "Rating: {0}, Tasks: {1}, User: {2}", currentRating.CodedNotation, uploadedData.Rows.Count(), user.FullName() ) );
			summary.Rating = currentRating.CodedNotation;
			SiteActivity sa = new SiteActivity()
			{
				ActivityType = "RMTL",
				Activity = "Upload",
				Event = "Reviewing",
				Comment = String.Format( "A bulk upload was initiated by: '{0}' for Rating: '{1}', Rows: {2}.", user.FullName(), currentRating.CodedNotation, uploadedData.Rows.Count ),
				ActionByUserId = user.Id,
				ActionByUser = user.FullName()
			};
			new ActivityServices().AddActivity( sa );

			//Get existing data
			var existingRatings = RatingManager.GetAll();
			var payGradeTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_Pay_Grade ).Concepts;
			var trainingGapTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TrainingGap ).Concepts;
			var applicabilityTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TaskApplicability ).Concepts;
			//WorkElementType/ReferenceResource
			//these should now be using Concept.WorkElementType
			var sourceTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_ReferenceResource, true ).Concepts;
			var courseTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_CourseType ).Concepts;
			var assessmentMethodTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_CurrentAssessmentApproach ).Concepts;
			debug[ latestStepFlag ] = "Got Concept Scheme data";

			var existingReferenceResources = ReferenceResourceManager.GetAll();

			//this will not be good where can be GT 8K+
			var existingRatingTasks = Factories.RatingTaskManager.GetAllForRating( currentRating.CodedNotation, true, ref totalRows );
			var existingBilletTitles = Factories.JobManager.GetAll();
			//these should probably be only those for the rating
			//var existingTrainingTasks = TrainingTaskManager.GetAll();
			var existingTrainingTasks = Factories.TrainingTaskManager.GetAllForRating( currentRating.CodedNotation, true, ref totalRows );
			var existingCourses = CourseManager.GetAll();
			var existingWorkRoles = WorkRoleManager.GetAll();
			var existingOrganizations = OrganizationManager.GetAll();
			debug[ latestStepFlag ] = "Got Existing data";

			/*
			 * These checks are handled above
			//need a check that only one rating is included
			var uploadedRatingMatchers = GetSheetMatchers<Rating, MatchableRating>( uploadedData.Rows, GetRowMatchHelper_Rating );
			//should only be one and must match current
			foreach ( var matcher in uploadedRatingMatchers )
			{
				matcher.Flattened.CodedNotation = matcher.Rows.Select( m => m.Rating_CodedNotation ).FirstOrDefault();
			}
			//Remove empty rows?
			uploadedRatingMatchers = uploadedRatingMatchers.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.CodedNotation ) ).ToList();

			if ( uploadedRatingMatchers != null )
            {
				bool foundRequestRating = false;
				foreach( var ratingMatcher in uploadedRatingMatchers )
                {
					if ( ratingMatcher.Flattened.CodedNotation.ToLower() != currentRating.CodedNotation.ToLower() )
					{
						summary.Messages.Error.Add( String.Format( "Error: A rating code ({0}) was found that doesn't match the requested rating: '{1}'. Only one rating may be uploaded at a time.", ratingMatcher.Flattened.CodedNotation, currentRating.CodedNotation ) );
					}
					else
						foundRequestRating = true;
				}
				if (!foundRequestRating )
					summary.Messages.Error.Add( String.Format( "Error: The requested rating code ({0}) was NOT in the uploaded data.", currentRating.CodedNotation ) );

				if ( summary.HasAnyErrors )
					return summary;
            } else
            {
				//no ratings found
				summary.Messages.Error.Add( "Error: No rating codes were found in the data. " );
				return summary;
			}
			*/



			//In order to facilitate a correct comparison, it must be done apples-to-apples
			//This approach attempts to achieve that by mapping both the uploaded data and the existing data into a common structure that can then easily be compared
			//Note: The order in which these methods are called is very important, since later methods rely on data determined by earlier methods!
			HandleUploadSheet_Organization( uploadedData.Rows, summary, currentRating, existingOrganizations );

			HandleUploadSheet_WorkRole( uploadedData.Rows, summary, existingWorkRoles );

			HandleUploadSheet_ReferenceResource( uploadedData.Rows, summary, existingReferenceResources, sourceTypeConcepts );

			HandleUploadSheet_TrainingTask( uploadedData.Rows, summary, existingTrainingTasks );

			HandleUploadSheet_Course( uploadedData.Rows, summary, existingCourses, existingOrganizations, existingReferenceResources, existingTrainingTasks, courseTypeConcepts, assessmentMethodTypeConcepts );

			//Billet Title needs to come after Rating Task because new/existing Rating Tasks are an input to figuring out Billet Title's HasRatingTask property
			HandleUploadSheet_BilletTitle( uploadedData.Rows, summary, currentRating, existingBilletTitles, existingRatingTasks, existingReferenceResources, payGradeTypeConcepts, sourceTypeConcepts, applicabilityTypeConcepts, trainingGapTypeConcepts );

			HandleUploadSheet_RatingTask( uploadedData.Rows, summary, currentRating, existingRatings, existingBilletTitles, existingRatingTasks, existingTrainingTasks, existingReferenceResources, existingWorkRoles, payGradeTypeConcepts, applicabilityTypeConcepts, trainingGapTypeConcepts, sourceTypeConcepts );


			debug[ latestStepFlag ] = "Handled all upload data";

			//Clean up the summary object
			summary.LookupGraph = summary.LookupGraph.Distinct().Where( m =>
				!summary.ItemsToBeCreated.BilletTitle.Contains( m ) &&
				!summary.ItemsToBeCreated.Course.Contains( m ) &&
				!summary.ItemsToBeCreated.Organization.Contains( m ) &&
				!summary.ItemsToBeCreated.RatingTask.Contains( m ) &&
				!summary.ItemsToBeCreated.ReferenceResource.Contains( m ) &&
				!summary.ItemsToBeCreated.TrainingTask.Contains( m ) &&
				!summary.ItemsToBeCreated.WorkRole.Contains( m ) &&
				!summary.ItemsToBeDeleted.BilletTitle.Contains( m ) &&
				!summary.ItemsToBeDeleted.Course.Contains( m ) &&
				!summary.ItemsToBeDeleted.Organization.Contains( m ) &&
				!summary.ItemsToBeDeleted.RatingTask.Contains( m ) &&
				!summary.ItemsToBeDeleted.ReferenceResource.Contains( m ) &&
				!summary.ItemsToBeDeleted.TrainingTask.Contains( m ) &&
				!summary.ItemsToBeDeleted.WorkRole.Contains( m )
			).ToList();
			debug[ latestStepFlag ] = "Cleaned up summary Lookup Graph";

			saveDuration = DateTime.Now.Subtract( saveStarted );
			summary.Messages.Note.Add( string.Format( "Duration: {0:N2} seconds ", saveDuration.TotalSeconds ) );
			return summary;
		}
		//


		//Organization
		public static void HandleUploadSheet_Organization( 
			List<UploadableRow> uploadedRows, 
			ChangeSummary summary, 
			Rating currentRating, 
			List<Organization> existingOrganizations = null )
		{
			//Get existing data
			existingOrganizations = existingOrganizations ?? OrganizationManager.GetAll();

			//Convert the uploaded data
			var uploadedOrganizationMatchers = GetSheetMatchers<Organization, MatchableOrganization>( uploadedRows, GetRowMatchHelper_Organization );
			foreach ( var matcher in uploadedOrganizationMatchers )
			{
				matcher.Flattened.Name = matcher.Rows.Select( m => m.Course_CurriculumControlAuthority_Name ).FirstOrDefault();
			}

			//Remove empty rows
			uploadedOrganizationMatchers = uploadedOrganizationMatchers.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.Name ) ).ToList();

			//Convert the existing data
			var existingOrganizationMatchers = GetSheetMatchersFromExisting<Organization, MatchableOrganization>( existingOrganizations );

			//Get loose matches
			var looseMatchingOrganizationMatchers = new List<SheetMatcher<Organization, MatchableOrganization>>();
			foreach ( var uploaded in uploadedOrganizationMatchers )
			{
				var matches = existingOrganizationMatchers.Where( m =>
					uploaded.Flattened.Name == m.Flattened.Name
				).ToList();

				HandleLooseMatchesFound( matches, uploaded, looseMatchingOrganizationMatchers, summary, "Curriculum Control Authority", m => m?.Name );
			}

			//Items that are completely identical (unchanged)
			var identicalOrganizationMatchers = looseMatchingOrganizationMatchers.Where( m =>
				 existingOrganizationMatchers.Where( n =>
					  m.Flattened.Name == n.Flattened.Name
				 ).Count() > 0
			).ToList();
			summary.UnchangedCount.Organization = identicalOrganizationMatchers.Count();

			//Items that match, but have a change (i.e., a different value for one or more List<>s)
			//Not applicable at this time for organizations

			//Items that were not found in the existing data (brand new items)
			var newOrganizationMatchers = uploadedOrganizationMatchers.Where( m => !looseMatchingOrganizationMatchers.Contains( m ) ).ToList();
			foreach ( var item in newOrganizationMatchers )
			{
				CreateNewItem( summary.ItemsToBeCreated.Organization, m => m.Name = item.Flattened.Name );
			}

			//Items that were not found in the uploaded data (deleted items)
			//Not applicable for Organizations
		}
		//

		//Work Role
		public static void HandleUploadSheet_WorkRole( List<UploadableRow> uploadedRows, ChangeSummary summary, List<WorkRole> existingWorkRoles = null )
		{
			//Get existing data
			existingWorkRoles = existingWorkRoles ?? WorkRoleManager.GetAll();

			//Convert the uploaded data
			var uploadedWorkRoleMatchers = GetSheetMatchers<WorkRole, MatchableWorkRole>( uploadedRows, GetRowMatchHelper_WorkRole );
			foreach( var matcher in uploadedWorkRoleMatchers )
			{
				matcher.Flattened.Name = matcher.Rows.Select( m => m.WorkRole_Name ).FirstOrDefault();
			}

			//Convert the existing data
			var existingWorkRoleMatchers = GetSheetMatchersFromExisting<WorkRole, MatchableWorkRole>( existingWorkRoles );

			//Get loose matches
			var looseMatchingWorkRoleMatchers = new List<SheetMatcher<WorkRole, MatchableWorkRole>>();
			foreach( var uploaded in uploadedWorkRoleMatchers )
			{
				var matches = existingWorkRoleMatchers.Where( m =>
					uploaded.Flattened.Name == m.Flattened.Name
				).ToList();

				HandleLooseMatchesFound( matches, uploaded, looseMatchingWorkRoleMatchers, summary, "Work Role", m => m?.Name );
			}

			//Items that are completely identical (unchanged)
			var identicalWorkRoleMatchers = looseMatchingWorkRoleMatchers.Where(m =>
				existingWorkRoleMatchers.Where( n =>
					m.Flattened.Name == n.Flattened.Name
				).Count() > 0
			).ToList();
			summary.UnchangedCount.WorkRole = identicalWorkRoleMatchers.Count();

			//Items that match, but have a change (i.e., a different value for one or more List<>s)
			//Not applicable at this time for Work Roles

			//Items that were not found in the existing data (brand new items)
			var newWorkRoleMatchers = uploadedWorkRoleMatchers.Where( m => !looseMatchingWorkRoleMatchers.Contains( m ) ).ToList();
			foreach(var item in newWorkRoleMatchers )
			{
				CreateNewItem( summary.ItemsToBeCreated.WorkRole, m => m.Name = item.Flattened.Name );
			}

			//Items that were not found in the uploaded data (deleted items)
			//Not applicable for Work Roles at this time (tends to lead to a lot of false positives since we're getting all work roles)
			/*
			var missingWorkRoleMatchers = existingWorkRoleMatchers.Where( m => !looseMatchingWorkRoleMatchers.Contains( m ) ).ToList();
			foreach ( var item in missingWorkRoleMatchers )
			{
				summary.ItemsToBeDeleted.WorkRole.Add( item.Data );
			}
			*/
		}
		//

		//Reference Resource
		public static void HandleUploadSheet_ReferenceResource( List<UploadableRow> uploadedRows, ChangeSummary summary, List<ReferenceResource> existingReferenceResources = null, List<Concept> sourceTypeConcepts = null )
		{
			//Get existing data
			existingReferenceResources = existingReferenceResources ?? ReferenceResourceManager.GetAll();
			sourceTypeConcepts = sourceTypeConcepts ?? ConceptSchemeManager.GetbyShortUri( ConceptSchemeManager.ConceptScheme_ReferenceResource ).Concepts;

			//Convert the uploaded data
			var uploadedReferenceResourceMatchers_Task = GetSheetMatchers<ReferenceResource, MatchableReferenceResource>( uploadedRows, GetRowMatchHelper_ReferenceResource_Task );
			foreach( var matcher in uploadedReferenceResourceMatchers_Task )
			{
				matcher.Flattened.Name = matcher.Rows.Select( m => m.ReferenceResource_Name ).FirstOrDefault();
				matcher.Flattened.PublicationDate = matcher.Rows.Select( m => m.ReferenceResource_PublicationDate ).FirstOrDefault();
				matcher.Flattened.ReferenceType_WorkElementType = matcher.Rows.Select( m => m.Shared_ReferenceType ).Distinct().ToList();
			}

			var uploadedReferenceResourceMatchers_Course = GetSheetMatchers<ReferenceResource, MatchableReferenceResource>( uploadedRows, GetRowMatchHelper_ReferenceResource_Course );
			foreach( var matcher in uploadedReferenceResourceMatchers_Course )
			{
				matcher.Flattened.Name = matcher.Rows.Select( m => m.Course_LifeCycleControlDocumentType_CodedNotation ).FirstOrDefault();
			}

			//Remove empty rows
			uploadedReferenceResourceMatchers_Task = uploadedReferenceResourceMatchers_Task.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.Name ) ).ToList();
			uploadedReferenceResourceMatchers_Course = uploadedReferenceResourceMatchers_Course.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.Name ) ).ToList();

			//Convert the existing data
			var existingReferenceResourceMatchers = GetSheetMatchersFromExisting<ReferenceResource, MatchableReferenceResource>( existingReferenceResources );
			foreach( var matcher in existingReferenceResourceMatchers )
			{
				matcher.Flattened.ReferenceType_WorkElementType = sourceTypeConcepts.Where( m => matcher.Data.ReferenceType.Contains( m.RowId ) ).Select( m => m.WorkElementType ).ToList();
				if ( matcher.Flattened.ReferenceType_WorkElementType?.Count == 0)
                {
					//CTTL
					//TCCD
                }
			}

			//Get loose matches
			var looseMatchReferenceResourceMatchers_Task = new List<SheetMatcher<ReferenceResource, MatchableReferenceResource>>();
			foreach( var uploaded in uploadedReferenceResourceMatchers_Task )
			{
				//only loose match: NAVPERS 18068F Vol II
				//22-03-24 mp - don't use date format, there can be values like FR 21-4
				var matches = existingReferenceResourceMatchers.Where( m =>
					uploaded.Flattened.Name == m.Flattened.Name 
					&& uploaded.Flattened.PublicationDate == m.Flattened.PublicationDate
					//&& ParseDateOrEmpty2( uploaded.Flattened.PublicationDate ) == ParseDateOrEmpty2( m.Flattened.PublicationDate ) 
				).ToList();
                //var matches2 = existingReferenceResourceMatchers.Where( m =>
                //    uploaded.Flattened.Name == m.Flattened.Name
                //).ToList();
                HandleLooseMatchesFound( matches, uploaded, looseMatchReferenceResourceMatchers_Task, summary, "Reference Resource (for Rating Task)", m => m?.Name );
			}
			
			var looseMatchReferenceResourceMatchers_Course = new List<SheetMatcher<ReferenceResource, MatchableReferenceResource>>();
			foreach (var uploaded in uploadedReferenceResourceMatchers_Course )
			{
				var matches = existingReferenceResourceMatchers.Where( m => 
					uploaded.Flattened.Name == m.Flattened.Name
				).ToList();

				HandleLooseMatchesFound( matches, uploaded, looseMatchReferenceResourceMatchers_Course, summary, "Reference Resource (for Course)", m => m?.Name );
			}

			//Items that are completely identical (unchanged)
			var identicalReferenceResourceMatchers_Task = looseMatchReferenceResourceMatchers_Task.Where( m =>
				existingReferenceResourceMatchers.Where( n =>
					AllMatch( m.Flattened.ReferenceType_WorkElementType, n.Flattened.ReferenceType_WorkElementType )
				).Count() > 0
			).ToList();
			summary.UnchangedCount.ReferenceResource += identicalReferenceResourceMatchers_Task.Count();

			var identicalReferenceResourceMatchers_Course = looseMatchReferenceResourceMatchers_Course.Where( m =>
				existingReferenceResourceMatchers.Where( n =>
					m.Flattened.Name == n.Flattened.Name
				).Count() > 0
			).ToList();
			summary.UnchangedCount.ReferenceResource += identicalReferenceResourceMatchers_Course.Count();

			//Items that match, but have a change (i.e., a different value for one or more List<>s)
			var updatedReferenceResourceMatchers_Task = looseMatchReferenceResourceMatchers_Task.Where( m => !identicalReferenceResourceMatchers_Task.Contains( m ) ).ToList();
			foreach( var item in updatedReferenceResourceMatchers_Task )
			{
				var existingMatcher = existingReferenceResourceMatchers.FirstOrDefault( m => m.Data == item.Data );
				AppendLookup( summary, existingMatcher.Data );

				//Handle added items
				var newSourceTypes = sourceTypeConcepts.Where( n => item.Flattened.ReferenceType_WorkElementType.Except( existingMatcher.Flattened.ReferenceType_WorkElementType ).Contains( n.WorkElementType ) ).ToList();
				HandleInnerItemChanges(
					existingMatcher.Data.RowId,
					m => m.ReferenceType = newSourceTypes.Select( n => n.RowId ).ToList(),
					m => m.ReferenceType.Count() > 0,
					summary.AddedItemsToInnerListsForCopiesOfItems.ReferenceResource
				);

				//Handle removed items
				var removedSourceTypes = sourceTypeConcepts.Where( n => existingMatcher.Flattened.ReferenceType_WorkElementType.Except( item.Flattened.ReferenceType_WorkElementType ).Contains( n.WorkElementType ) ).ToList();
				HandleInnerItemChanges(
					existingMatcher.Data.RowId,
					m => m.ReferenceType = removedSourceTypes.Select(n => n.RowId).ToList(),
					m => m.ReferenceType.Count() > 0,
					summary.RemovedItemsFromInnerListsForCopiesOfItems.ReferenceResource
				);

			}

			//Items that were not found in the existing data (brand new items)
			var newReferenceResourceMatchers_Task = uploadedReferenceResourceMatchers_Task.Where( m => !looseMatchReferenceResourceMatchers_Task.Contains( m ) ).ToList();
			foreach( var item in newReferenceResourceMatchers_Task )
			{
				CreateNewItem( summary.ItemsToBeCreated.ReferenceResource, m => 
				{ 
					m.Name = item.Flattened.Name;
					m.PublicationDate = item.Flattened.PublicationDate;
					m.ReferenceType = sourceTypeConcepts.Where( n => item.Flattened.ReferenceType_WorkElementType.Contains( n.WorkElementType ) ).Select( n => n.RowId ).ToList();
				} );
			}

			var newReferenceResourceMatchers_Course = uploadedReferenceResourceMatchers_Course.Where( m => !looseMatchReferenceResourceMatchers_Course.Contains( m ) ).ToList();
			foreach( var item in newReferenceResourceMatchers_Course )
			{
				CreateNewItem( summary.ItemsToBeCreated.ReferenceResource, m =>
				{
					m.Name = item.Flattened.Name;
					m.ReferenceType = sourceTypeConcepts.Where( n => n.CodedNotation == "LCCD" ).Select( n => n.RowId ).ToList(); //Course Reference Resource Type is always a Life-Cycle Control Document
				} );
			}

			//Items that were not found in the uploaded data (deleted items)
			//Not applicable for Reference Resources
		}
		//

		//Training Task
		public static void HandleUploadSheet_TrainingTask( List<UploadableRow> uploadedRows, ChangeSummary summary, List<TrainingTask> existingTrainingTasks = null )
		{
			//Get the existing data
			existingTrainingTasks = existingTrainingTasks ?? TrainingTaskManager.GetAll();
			
			//Convert the uploaded data
			var uploadedTrainingTaskMatchers = GetSheetMatchers<TrainingTask, MatchableTrainingTask>( uploadedRows, GetRowMatchHelper_TrainingTask );
			foreach ( var matcher in uploadedTrainingTaskMatchers )
			{
				matcher.Flattened.Description = matcher.Rows.Select( m => m.TrainingTask_Description ).FirstOrDefault();
				matcher.Flattened.CourseCodedNotation = matcher.Rows.Select( m => m.Course_CodedNotation ).FirstOrDefault();
			}

			//Remove empty rows
			uploadedTrainingTaskMatchers = uploadedTrainingTaskMatchers.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.Description ) ).ToList();

			//Convert the existing data
			var existingTrainingTaskMatchers = GetSheetMatchersFromExisting<TrainingTask, MatchableTrainingTask>( existingTrainingTasks );

			//Get loose matches
			var looseMatchingTrainingTaskMatchers = new List<SheetMatcher<TrainingTask, MatchableTrainingTask>>();
			foreach ( var uploaded in uploadedTrainingTaskMatchers )
			{
				var matches = existingTrainingTaskMatchers.Where( m =>
					uploaded.Flattened.Description.ToLower() == m.Flattened.Description.ToLower()
				).ToList();

				HandleLooseMatchesFound( matches, uploaded, looseMatchingTrainingTaskMatchers, summary, "Training Task", m => m?.Description );
			}

			//Items that are completely identical (unchanged)
			var identicalTrainingTaskMatchers = looseMatchingTrainingTaskMatchers.Where( m =>
				 existingTrainingTaskMatchers.Where( n =>
					 m.Flattened.Description.ToLower() == n.Flattened.Description.ToLower()
				 ).Count() > 0
			).ToList();
			summary.UnchangedCount.TrainingTask = identicalTrainingTaskMatchers.Count();

			//Items that match, but have a change (i.e., a different value for one or more List<>s)
			//Not applicable at this time for Training Tasks

			//Items that were not found in the existing data (brand new items)
			var newTrainingTaskMatchers = uploadedTrainingTaskMatchers.Where( m => !looseMatchingTrainingTaskMatchers.Contains( m ) ).ToList();
			foreach ( var item in newTrainingTaskMatchers )
			{
				CreateNewItem( summary.ItemsToBeCreated.TrainingTask, m => 
				{
					m.Description = item.Flattened.Description;
					m.CourseCodedNotation = item.Flattened.CourseCodedNotation;
				} );
			}

			//Items that were not found in the uploaded data (deleted items)
			//Not applicable for Training Tasks at this time (tends to lead to a lot of false positives since we're getting all training tasks)
			/*
			var missingTrainingTaskMatchers = existingTrainingTaskMatchers.Where( m => !looseMatchingTrainingTaskMatchers.Contains( m ) ).ToList();
			foreach ( var item in missingTrainingTaskMatchers )
			{
				summary.ItemsToBeDeleted.TrainingTask.Add( item.Data );
			}
			*/

		}
		//

		//Course
		public static void HandleUploadSheet_Course( List<UploadableRow> uploadedRows, ChangeSummary summary, List<Course> existingCourses = null, List<Organization> existingOrganizations = null, List<ReferenceResource> existingReferenceResources = null, List<TrainingTask> existingTrainingTasks = null, List<Concept> courseTypeConcepts = null, List<Concept> assessmentMethodTypeConcepts = null )
		{
			//Get the existing data
			existingCourses = existingCourses ?? CourseManager.GetAll();
			existingOrganizations = existingOrganizations ?? OrganizationManager.GetAll();
			existingTrainingTasks = existingTrainingTasks ?? TrainingTaskManager.GetAll();
			courseTypeConcepts = courseTypeConcepts ?? ConceptSchemeManager.GetbyShortUri( ConceptSchemeManager.ConceptScheme_CourseType ).Concepts;
			assessmentMethodTypeConcepts = assessmentMethodTypeConcepts ?? ConceptSchemeManager.GetbyShortUri( ConceptSchemeManager.ConceptScheme_CurrentAssessmentApproach ).Concepts;

			//Convert the uploaded data
			var uploadedCourseMatchers = GetSheetMatchers<Course, MatchableCourse>( uploadedRows, GetRowMatchHelper_Course );
			foreach ( var matcher in uploadedCourseMatchers )
			{
				matcher.Flattened.Name = matcher.Rows.Select( m => m.Course_Name ).FirstOrDefault();
				matcher.Flattened.CodedNotation = matcher.Rows.Select( m => m.Course_CodedNotation ).FirstOrDefault();
				//
				matcher.Flattened.LifeCycleControlDocumentType_CodedNotation = matcher.Rows.Select( m => m.Course_LifeCycleControlDocumentType_CodedNotation ).FirstOrDefault();
				matcher.Flattened.CourseType_Name = matcher.Rows.Select( m => m.Course_CourseType_Name ).Distinct().ToList();
				matcher.Flattened.CurriculumControlAuthority_Name = matcher.Rows.Select( m => m.Course_CurriculumControlAuthority_Name ).Distinct().ToList();
				matcher.Flattened.HasTrainingTask_Description = matcher.Rows.Select( m => m.TrainingTask_Description ).Distinct().ToList();
				matcher.Flattened.AssessmentMethodType_Name = matcher.Rows.Select( m => m.Course_AssessmentMethodType_Name ).Distinct().ToList();
			}

			//Remove empty rows
			uploadedCourseMatchers = uploadedCourseMatchers.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.Name ) ).ToList();

			//Convert the existing data
			var existingCourseMatchers = GetSheetMatchersFromExisting<Course, MatchableCourse>( existingCourses );
			foreach( var matcher in existingCourseMatchers )
			{
				matcher.Flattened.LifeCycleControlDocumentType_CodedNotation = existingReferenceResources.Where( m => matcher.Data.LifeCycleControlDocumentType == m.RowId ).Select( m => m.CodedNotation ).FirstOrDefault();
				matcher.Flattened.CourseType_Name = courseTypeConcepts.Where( m => matcher.Data.CourseType.Contains( m.RowId ) ).Select( m => m.Name ).ToList();
				matcher.Flattened.CurriculumControlAuthority_Name = existingOrganizations.Where( m => matcher.Data.CurriculumControlAuthority.Contains( m.RowId ) ).Select( m => m.Name ).ToList();
				matcher.Flattened.HasTrainingTask_Description = existingTrainingTasks.Where( m => matcher.Data.HasTrainingTask.Contains( m.RowId ) ).Select( m => m.Description ).ToList();
				//matcher.Flattened.AssessmentMethodType_Name = assessmentMethodTypeConcepts.Where( m => matcher.Data.AssessmentMethodType.Contains( m.RowId ) ).Select( m => m.Name ).ToList();
			}

			//Get loose matches
			var looseMatchingCourseMatchers = new List<SheetMatcher<Course, MatchableCourse>>();
			foreach ( var uploaded in uploadedCourseMatchers )
			{
				var matches = existingCourseMatchers.Where( m =>
					uploaded.Flattened.Name == m.Flattened.Name &&
					uploaded.Flattened.CodedNotation == m.Flattened.CodedNotation &&
					uploaded.Flattened.LifeCycleControlDocumentType_CodedNotation == m.Flattened.LifeCycleControlDocumentType_CodedNotation &&
					uploaded.Flattened.CourseType_Name == m.Flattened.CourseType_Name
				).ToList();

				HandleLooseMatchesFound( matches, uploaded, looseMatchingCourseMatchers, summary, "Course", m => m?.CodedNotation + " - " + m?.Name );
			}

			//Items that are completely identical (unchanged)
			var identicalCourseMatchers = looseMatchingCourseMatchers.Where( m =>
				 existingCourseMatchers.Where( n =>
					AllMatch(m.Flattened.CurriculumControlAuthority_Name, n.Flattened.CurriculumControlAuthority_Name) &&
					AllMatch(m.Flattened.HasTrainingTask_Description, n.Flattened.HasTrainingTask_Description) &&
					AllMatch(m.Flattened.AssessmentMethodType_Name, n.Flattened.AssessmentMethodType_Name)
				 ).Count() > 0
			).ToList();
			summary.UnchangedCount.Course = identicalCourseMatchers.Count();

			//Items that match, but have a change (i.e., a different value for one or more List<>s)
			var updatedCourseMatchers = looseMatchingCourseMatchers.Where( m => !identicalCourseMatchers.Contains( m ) ).ToList();
			foreach ( var item in updatedCourseMatchers )
			{
				var existingMatcher = existingCourseMatchers.FirstOrDefault( m => m.Data == item.Data );
				AppendLookup( summary, existingMatcher.Data );

				//Handle added items
				var newCCAs = existingOrganizations.Concat( summary.ItemsToBeCreated.Organization ).Where( n => item.Flattened.CurriculumControlAuthority_Name.Except( existingMatcher.Flattened.CurriculumControlAuthority_Name ).Contains( n.Name ) ).ToList();
				var newTrainingTasks = existingTrainingTasks.Concat( summary.ItemsToBeCreated.TrainingTask ).Where( n => item.Flattened.HasTrainingTask_Description.Except( existingMatcher.Flattened.HasTrainingTask_Description ).Contains( n.Description ) ).ToList();
				var newAssessmentMethods = assessmentMethodTypeConcepts.Where( n => item.Flattened.AssessmentMethodType_Name.Except( existingMatcher.Flattened.AssessmentMethodType_Name ).Contains( n.Name ) ).ToList();
				AppendLookup( summary, newCCAs );
				AppendLookup( summary, newTrainingTasks );
				HandleInnerItemChanges(
					existingMatcher.Data.RowId,
					m => {
						m.CurriculumControlAuthority = newCCAs.Select( n => n.RowId ).ToList();
						m.HasTrainingTask = newTrainingTasks.Select( n => n.RowId ).ToList();
						//m.AssessmentMethodType = newAssessmentMethods.Select( n => n.RowId ).ToList();
					},
					m => m.CurriculumControlAuthority.Count() > 0 || m.HasTrainingTask.Count() > 0,// || m.AssessmentMethodType.Count() > 0,
					summary.AddedItemsToInnerListsForCopiesOfItems.Course
				);

				//Handle removed items
				var removedCCAs = existingOrganizations.Where( n => existingMatcher.Flattened.CurriculumControlAuthority_Name.Except( item.Flattened.CurriculumControlAuthority_Name ).Contains( n.Name ) ).ToList();
				var removedTrainingTasks = existingTrainingTasks.Where( n => existingMatcher.Flattened.HasTrainingTask_Description.Except( item.Flattened.HasTrainingTask_Description ).Contains( n.Description ) ).ToList();
				var removedAssessmentMethods = assessmentMethodTypeConcepts.Where( n => existingMatcher.Flattened.AssessmentMethodType_Name.Except( item.Flattened.AssessmentMethodType_Name ).Contains( n.Name ) ).ToList();
				AppendLookup( summary, removedCCAs );
				AppendLookup( summary, removedTrainingTasks );
				HandleInnerItemChanges(
					existingMatcher.Data.RowId,
					m => {
						m.CurriculumControlAuthority = removedCCAs.Select( n => n.RowId ).ToList();
						m.HasTrainingTask = removedTrainingTasks.Select( n => n.RowId ).ToList();
						//m.AssessmentMethodType = removedAssessmentMethods.Select( n => n.RowId ).ToList();
					},
					m => m.CurriculumControlAuthority.Count() > 0 || m.HasTrainingTask.Count() > 0,// || m.AssessmentMethodType.Count() > 0,
					summary.RemovedItemsFromInnerListsForCopiesOfItems.Course
				);

			}

			//Items that were not found in the existing data (brand new items)
			var newCourseMatchers = uploadedCourseMatchers.Where( m => !looseMatchingCourseMatchers.Contains( m ) ).ToList();
			foreach ( var item in newCourseMatchers )
			{
				CreateNewItem( summary.ItemsToBeCreated.Course, m => {
					m.Name = item.Flattened.Name;
					m.CodedNotation = item.Flattened.CodedNotation;
					//m.HasReferenceResource = existingReferenceResources.Concat( summary.ItemsToBeCreated.ReferenceResource ).Where( n => item.Flattened.LifeCycleControlDocumentType_CodedNotation == n.CodedNotation ).Select( n => n.RowId ).FirstOrDefault();
					m.CourseType = courseTypeConcepts.Where( n => item.Flattened.CourseType_Name.Contains( n.Name ) ).Select( n => n.RowId ).ToList();
					m.CurriculumControlAuthority = existingOrganizations.Concat( summary.ItemsToBeCreated.Organization ).Where( n => item.Flattened.CurriculumControlAuthority_Name.Contains( n.Name ) ).Select( n => n.RowId ).ToList();
					m.HasTrainingTask = existingTrainingTasks.Concat( summary.ItemsToBeCreated.TrainingTask ).Where( n => item.Flattened.HasTrainingTask_Description.Contains( n.Description ) ).Select( n => n.RowId ).ToList();
					//m.AssessmentMethodType = assessmentMethodTypeConcepts.Where( n => item.Flattened.AssessmentMethodType_Name.Contains( n.Name ) ).Select( n => n.RowId ).ToList();
				} );
			}

			//Items that were not found in the uploaded data (deleted items)
			//Not applicable to Course at this time (tends to lead to a lot of false positives since we're getting all courses)
			/*
			var missingCourseMatchers = existingCourseMatchers.Where( m => !looseMatchingCourseMatchers.Contains( m ) ).ToList();
			foreach ( var item in missingCourseMatchers )
			{
				summary.ItemsToBeDeleted.Course.Add( item.Data );
			}
			*/

		}
		//

		//Rating Task
		public static void HandleUploadSheet_RatingTask( 
			List<UploadableRow> uploadedRows, 
			ChangeSummary summary, 
			Rating currentRating, 
			List<Rating> existingRatings = null,
			List<BilletTitle> existingBilletTitles = null,
			List<RatingTask> existingRatingTasks = null, 
			List<TrainingTask> existingTrainingTasks = null, 
			List<ReferenceResource> existingReferenceResources = null, 
			List<WorkRole> existingWorkRoles = null, 
			List<Concept> payGradeTypeConcepts = null, 
			List<Concept> applicabilityTypeConcepts = null, 
			List<Concept> trainingGapTypeConcepts = null, 
			List<Concept> sourceTypeConcepts = null 
		)
		{
			//Get the existing data
			var totalRows = 0;
			existingRatings = existingRatings ?? Factories.RatingManager.GetAll();
			existingRatingTasks = existingRatingTasks ?? Factories.RatingTaskManager.GetAllForRating( currentRating.CodedNotation, true, ref totalRows );
			existingTrainingTasks = existingTrainingTasks ?? Factories.TrainingTaskManager.GetAll();
			existingReferenceResources = existingReferenceResources ?? Factories.ReferenceResourceManager.GetAll();
			existingWorkRoles = existingWorkRoles ?? Factories.WorkRoleManager.GetAll();
			payGradeTypeConcepts = payGradeTypeConcepts ?? Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_Pay_Grade ).Concepts;
			applicabilityTypeConcepts = applicabilityTypeConcepts ?? Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TaskApplicability ).Concepts;
			trainingGapTypeConcepts = trainingGapTypeConcepts ?? Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TrainingGap ).Concepts;
			sourceTypeConcepts = sourceTypeConcepts ?? Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_ReferenceResource ).Concepts;

			//Convert the uploaded data
			var uploadedRatingTaskMatchers = GetSheetMatchers<RatingTask, MatchableRatingTask>( uploadedRows, GetRowMatchHelper_RatingTask );
			foreach ( var matcher in uploadedRatingTaskMatchers )
			{
				matcher.Flattened.Description = matcher.Rows.Select( m => m.RatingTask_Description ).FirstOrDefault();
				matcher.Flattened.HasCodedNotation = matcher.Rows.Select( m => m.Row_CodedNotation ).FirstOrDefault();
				if ( matcher.Flattened.HasCodedNotation == "PQ17-038" || matcher.Flattened.HasCodedNotation == "PQ31-007" )
				{

				}
				//this should equate to the RowId - ideally. But only if done at the beginning.
				matcher.Flattened.HasIdentifier = matcher.Rows.Select( m => m.Row_Identifier ).FirstOrDefault();
				matcher.Flattened.HasRating_CodedNotation = matcher.Rows.Select( m => m.Rating_CodedNotation ).Distinct().ToList();
				//
				matcher.Flattened.HasBilletTitle_Name = matcher.Rows.Select( m => m.BilletTitle_Name ).FirstOrDefault();
				//
				matcher.Flattened.HasTrainingTask_Description = matcher.Rows.Select( m => m.TrainingTask_Description ).FirstOrDefault();
				matcher.Flattened.HasReferenceResource_Name = matcher.Rows.Select( m => m.ReferenceResource_Name ).FirstOrDefault();
				matcher.Flattened.HasWorkRole_Name = matcher.Rows.Select( m => m.WorkRole_Name ).Distinct().ToList();
				matcher.Flattened.PayGradeType_CodedNotation = matcher.Rows.Select( m => m.PayGradeType_CodedNotation ).FirstOrDefault();
				matcher.Flattened.ApplicabilityType_Name = matcher.Rows.Select( m => m.RatingTask_ApplicabilityType_Name ).FirstOrDefault();
				matcher.Flattened.TrainingGapType_Name = matcher.Rows.Select( m => m.RatingTask_TrainingGapType_Name ).FirstOrDefault();
				matcher.Flattened.ReferenceType_WorkElementType = matcher.Rows.Select( m => m.Shared_ReferenceType ).FirstOrDefault();
				matcher.Flattened.HasReferenceResource_PublicationDate = matcher.Rows.Select( m => m.ReferenceResource_PublicationDate ).FirstOrDefault();
			}

			//Remove empty rows
			uploadedRatingTaskMatchers = uploadedRatingTaskMatchers.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.Description ) ).ToList();

			//Convert the existing data
			//scenario - uploading E9 and now matching against 786 existing
			var existingRatingTaskMatchers = GetSheetMatchersFromExisting<RatingTask, MatchableRatingTask>( existingRatingTasks );
			foreach ( var matcher in existingRatingTaskMatchers )
			{
				//CodedNotation??
				//Identifier
				//BilletTitle
				//matcher.Flattened.HasBillet = existingBilletTitles.Where( m => matcher.Data.HasBillet.Contains( m.RowId ) ).Select( m => m.RowId ).ToList();

				matcher.Flattened.HasRating_CodedNotation = existingRatings.Where( m => matcher.Data.HasRating.Contains( m.RowId ) ).Select( m => m.CodedNotation ).ToList();
				matcher.Flattened.HasTrainingTask_Description = existingTrainingTasks.FirstOrDefault( m => m.RowId == matcher.Data.HasTrainingTask )?.Description;
				matcher.Flattened.HasReferenceResource_Name = existingReferenceResources.FirstOrDefault( m => m.RowId == matcher.Data.HasReferenceResource )?.Name;
				matcher.Flattened.HasWorkRole_Name = existingWorkRoles.Where( m => matcher.Data.HasWorkRole.Contains( m.RowId ) ).Select( m => m.Name ).ToList();
				matcher.Flattened.PayGradeType_CodedNotation = payGradeTypeConcepts.FirstOrDefault( m => m.RowId == matcher.Data.PayGradeType )?.CodedNotation;
				matcher.Flattened.ApplicabilityType_Name = applicabilityTypeConcepts.FirstOrDefault( m => m.RowId == matcher.Data.ApplicabilityType )?.Name;
				matcher.Flattened.TrainingGapType_Name = trainingGapTypeConcepts.FirstOrDefault( m => m.RowId == matcher.Data.TrainingGapType )?.Name;
				matcher.Flattened.ReferenceType_WorkElementType = sourceTypeConcepts.FirstOrDefault( m => m.RowId == matcher.Data.ReferenceType )?.WorkElementType;
				matcher.Flattened.HasReferenceResource_PublicationDate = existingReferenceResources.FirstOrDefault( m => m.RowId == matcher.Data.HasReferenceResource )?.PublicationDate;
			}

			//Get loose matches
			var looseMatchingRatingTaskMatchers = new List<SheetMatcher<RatingTask, MatchableRatingTask>>();
			foreach ( var uploaded in uploadedRatingTaskMatchers )
			{
				var matches = existingRatingTaskMatchers.Where( n =>
					uploaded.Flattened.HasTrainingTask_Description == n.Flattened.HasTrainingTask_Description &&
					uploaded.Flattened.HasReferenceResource_Name == n.Flattened.HasReferenceResource_Name &&
					uploaded.Flattened.ApplicabilityType_Name == n.Flattened.ApplicabilityType_Name &&
					uploaded.Flattened.TrainingGapType_Name == n.Flattened.TrainingGapType_Name &&
					uploaded.Flattened.ReferenceType_WorkElementType == n.Flattened.ReferenceType_WorkElementType
				).ToList();
				//????
				var matches2 = existingRatingTaskMatchers.Where( n =>
					uploaded.Flattened.CodedNotation == n.Flattened.CodedNotation
				).ToList();

				HandleLooseMatchesFound( matches, uploaded, looseMatchingRatingTaskMatchers, summary, "Rating Task", m => m?.Description );
			}

			//Items that are completely identical (unchanged)
			var identicalRatingTaskMatchers = looseMatchingRatingTaskMatchers.Where( m =>
				existingRatingTaskMatchers.Where( n =>
					m.Data == n.Data &&
					//Consider it an exact match if the existing data already references this rating, regardless of whether it also references other ratings
					n.Flattened.HasRating_CodedNotation.Intersect( m.Flattened.HasRating_CodedNotation ).Count() > 0 &&
					//Consider it an exact match if the existing data already references this work role, regardless of whether it also references other work roles
					n.Flattened.HasWorkRole_Name.Intersect( m.Flattened.HasWorkRole_Name ).Count() > 0
				).Count() > 0
			).ToList();
			summary.UnchangedCount.RatingTask = identicalRatingTaskMatchers.Count();

			//Items that match, but have a change (i.e., a different value for one or more List<>s)
			var updatedRatingTaskMatchers = looseMatchingRatingTaskMatchers.Where( m => !identicalRatingTaskMatchers.Contains( m ) ).ToList();
			foreach ( var item in updatedRatingTaskMatchers )
			{
				var existingMatcher = existingRatingTaskMatchers.FirstOrDefault( m => m.Data == item.Data );
				AppendLookup( summary, existingMatcher.Data );

				//Handle added items
				var newRatings = existingRatings.Where( n => item.Flattened.HasRating_CodedNotation.Except( existingMatcher.Flattened.HasRating_CodedNotation ).Contains( n.CodedNotation ) ).ToList();
				var newWorkRoles = existingWorkRoles.Concat( summary.ItemsToBeCreated.WorkRole ).Where( n => item.Flattened.HasWorkRole_Name.Except( existingMatcher.Flattened.HasWorkRole_Name ).Contains( n.Name ) ).ToList();
				AppendLookup( summary, newWorkRoles );
				HandleInnerItemChanges(
					existingMatcher.Data.RowId,
					m => {
						m.HasRating = newRatings.Select( n => n.RowId ).ToList();
						m.HasWorkRole = newWorkRoles.Select( n => n.RowId ).ToList();
					},
					m => m.HasRating.Count() > 0 || m.HasWorkRole.Count > 0,
					summary.AddedItemsToInnerListsForCopiesOfItems.RatingTask
				);

				//Handle removed items
				//Not applicable for Rating Tasks at this time
			}

			//Items that were not found in the existing data (brand new items)
			var newRatingTaskMatchers = uploadedRatingTaskMatchers.Where( m => !looseMatchingRatingTaskMatchers.Contains( m ) ).ToList();
			foreach ( var item in newRatingTaskMatchers )
			{
				CreateNewItem( summary.ItemsToBeCreated.RatingTask, m =>
				{
					m.Description = item.Flattened.Description;
					m.CodedNotation = item.Flattened.HasCodedNotation;
					m.Identifier = item.Flattened.Identifier;
					//???
					m.HasBilletTitle = item.Flattened.HasBilletTitle;
					m.ApplicabilityType = FindConceptOrError( applicabilityTypeConcepts, new Concept() { Name = item.Flattened.ApplicabilityType_Name }, "Applicability Type", item.Flattened.ApplicabilityType_Name, summary.Messages.Error ).RowId;
					m.TrainingGapType = FindConceptOrError( trainingGapTypeConcepts, new Concept() { Name = item.Flattened.TrainingGapType_Name }, "Training Gap Type", item.Flattened.TrainingGapType_Name, summary.Messages.Error ).RowId;
					m.PayGradeType = FindConceptOrError( payGradeTypeConcepts, new Concept() { CodedNotation = item.Flattened.PayGradeType_CodedNotation }, "Pay Grade Type", item.Flattened.PayGradeType_CodedNotation, summary.Messages.Error ).RowId;
					m.ReferenceType = FindConceptOrError( sourceTypeConcepts, new Concept() { WorkElementType = item.Flattened.ReferenceType_WorkElementType }, "Reference Resource Type", item.Flattened.ReferenceType_WorkElementType, summary.Messages.Error ).RowId;
					m.HasRating = new List<Guid>() { currentRating.RowId };
					m.HasWorkRole = existingWorkRoles.Concat( summary.ItemsToBeCreated.WorkRole ).Where( n => item.Flattened.HasWorkRole_Name.Contains( n.Name ) ).Select( n => n.RowId ).ToList();

					m.HasReferenceResource = existingReferenceResources.Concat( summary.ItemsToBeCreated.ReferenceResource ).Where( n =>
						item.Flattened.HasReferenceResource_Name == n.Name
						//&& ParseDateOrEmpty( item.Flattened.HasReferenceResource_PublicationDate ) == ParseDateOrEmpty( n.PublicationDate )
						//22-03-24 mp - just do string compare due to various formats
						&& item.Flattened.HasReferenceResource_PublicationDate == n.PublicationDate 
						&& sourceTypeConcepts.Where( o => n.ReferenceType.Contains( o.RowId ) ).Select( o => o.WorkElementType ).ToList().Contains( item.Flattened.ReferenceType_WorkElementType )
					).Select( n => n.RowId ).FirstOrDefault();

					m.HasTrainingTask = existingTrainingTasks.Concat( summary.ItemsToBeCreated.TrainingTask ).Where( n =>
						item.Flattened.HasTrainingTask_Description == n.Description
					).Select( n => n.RowId ).FirstOrDefault();
					m.Note = item.Flattened.Note;
				} );
			}

			//Items that were not found in the uploaded data (deleted items)
			var missingRatingTaskMatchers = existingRatingTaskMatchers.Where( m => !looseMatchingRatingTaskMatchers.Contains( m ) ).ToList();
			foreach ( var item in missingRatingTaskMatchers )
			{
				summary.ItemsToBeDeleted.RatingTask.Add( item.Data );
			}

			//Special handling for Rating Tasks that may appear in other ratings
			var ratingTasksThatMayExistUnderOtherRatings = summary.ItemsToBeCreated.RatingTask.Select( m => new RatingTaskComparisonHelper() { PossiblyNewRatingTask = m } ).ToList();
			//TODO: Implement this
			//RatingTaskManager.FindMatchingExistingRatingTasksInBulk( ratingTasksThatMayExistUnderOtherRatings ); //Should modify the contents of the list and return void
			foreach( var foundTask in ratingTasksThatMayExistUnderOtherRatings.Where( m => m.MatchingExistingRatingTask != null && m.MatchingExistingRatingTask.Id > 0 ) )
			{
				summary.ItemsToBeCreated.RatingTask.Remove( foundTask.PossiblyNewRatingTask );
				AppendLookup( summary, foundTask.MatchingExistingRatingTask );
				summary.AddedItemsToInnerListsForCopiesOfItems.RatingTask.Add( new RatingTask()
				{
					RowId = foundTask.MatchingExistingRatingTask.RowId,
					HasRating = new List<Guid>() { currentRating.RowId }
				} );
			}
		}

		//Rating Task2 - with cluster analysis
		public static void HandleUploadSheet_RatingTask2(
			List<UploadableRow> uploadedRows,
			ChangeSummary summary,
			Rating currentRating,
			List<Rating> existingRatings = null,
			List<BilletTitle> existingBilletTitles = null,
			List<RatingTask> existingRatingTasks = null,
			List<TrainingTask> existingTrainingTasks = null,
			List<ReferenceResource> existingReferenceResources = null,
			List<WorkRole> existingWorkRoles = null,
			List<Concept> payGradeTypeConcepts = null,
			List<Concept> applicabilityTypeConcepts = null,
			List<Concept> trainingGapTypeConcepts = null,
			List<Concept> sourceTypeConcepts = null
		)
		{
			//Get the existing data
			var totalRows = 0;
			existingRatings = existingRatings ?? Factories.RatingManager.GetAll();
			existingRatingTasks = existingRatingTasks ?? Factories.RatingTaskManager.GetAllForRating( currentRating.CodedNotation, true, ref totalRows );
			existingTrainingTasks = existingTrainingTasks ?? Factories.TrainingTaskManager.GetAll();
			existingReferenceResources = existingReferenceResources ?? Factories.ReferenceResourceManager.GetAll();
			existingWorkRoles = existingWorkRoles ?? Factories.WorkRoleManager.GetAll();
			payGradeTypeConcepts = payGradeTypeConcepts ?? Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_Pay_Grade ).Concepts;
			applicabilityTypeConcepts = applicabilityTypeConcepts ?? Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TaskApplicability ).Concepts;
			trainingGapTypeConcepts = trainingGapTypeConcepts ?? Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TrainingGap ).Concepts;
			sourceTypeConcepts = sourceTypeConcepts ?? Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_ReferenceResource ).Concepts;

			//Convert the uploaded data
			var uploadedRatingTaskMatchers = GetSheetMatchers<RatingTask, MatchableRatingTask>( uploadedRows, GetRowMatchHelper_RatingTask );
			foreach ( var matcher in uploadedRatingTaskMatchers )
			{
				matcher.Flattened.Description = matcher.Rows.Select( m => m.RatingTask_Description ).FirstOrDefault();
				matcher.Flattened.HasCodedNotation = matcher.Rows.Select( m => m.Row_CodedNotation ).FirstOrDefault();
				if ( matcher.Flattened.HasCodedNotation == "PQ17-038" || matcher.Flattened.HasCodedNotation == "PQ31-007" )
				{

				}
				//this should equate to the RowId - ideally. But only if done at the beginning.
				matcher.Flattened.HasIdentifier = matcher.Rows.Select( m => m.Row_Identifier ).FirstOrDefault();
				matcher.Flattened.HasRating_CodedNotation = matcher.Rows.Select( m => m.Rating_CodedNotation ).Distinct().ToList();
				//
				matcher.Flattened.HasBilletTitle_Name = matcher.Rows.Select( m => m.BilletTitle_Name ).FirstOrDefault();
				//
				matcher.Flattened.HasTrainingTask_Description = matcher.Rows.Select( m => m.TrainingTask_Description ).FirstOrDefault();
				matcher.Flattened.HasReferenceResource_Name = matcher.Rows.Select( m => m.ReferenceResource_Name ).FirstOrDefault();
				matcher.Flattened.HasWorkRole_Name = matcher.Rows.Select( m => m.WorkRole_Name ).Distinct().ToList();
				matcher.Flattened.PayGradeType_CodedNotation = matcher.Rows.Select( m => m.PayGradeType_CodedNotation ).FirstOrDefault();
				matcher.Flattened.ApplicabilityType_Name = matcher.Rows.Select( m => m.RatingTask_ApplicabilityType_Name ).FirstOrDefault();
				matcher.Flattened.TrainingGapType_Name = matcher.Rows.Select( m => m.RatingTask_TrainingGapType_Name ).FirstOrDefault();
				matcher.Flattened.ReferenceType_WorkElementType = matcher.Rows.Select( m => m.Shared_ReferenceType ).FirstOrDefault();
				matcher.Flattened.HasReferenceResource_PublicationDate = matcher.Rows.Select( m => m.ReferenceResource_PublicationDate ).FirstOrDefault();
				//CA
				matcher.Flattened.Training_Solution_Type = matcher.Rows.Select( m => m.Training_Solution_Type ).FirstOrDefault();
				matcher.Flattened.Cluster_Analysis_Title = matcher.Rows.Select( m => m.Cluster_Analysis_Title ).FirstOrDefault();
				matcher.Flattened.Recommended_Modality = matcher.Rows.Select( m => m.Recommended_Modality ).FirstOrDefault();
				matcher.Flattened.Development_Specification = matcher.Rows.Select( m => m.Development_Specification ).FirstOrDefault();
				matcher.Flattened.Candidate_Platform = matcher.Rows.Select( m => m.Candidate_Platform ).FirstOrDefault();
				matcher.Flattened.Priority_Placement = matcher.Rows.Select( m => m.Priority_Placement ).FirstOrDefault();
				matcher.Flattened.Development_Ratio = matcher.Rows.Select( m => m.Development_Ratio ).FirstOrDefault();
				matcher.Flattened.Development_Time = matcher.Rows.Select( m => m.Development_Time ).FirstOrDefault();

				matcher.Flattened.Cluster_Analysis_Notes = matcher.Rows.Select( m => m.Cluster_Analysis_Notes ).FirstOrDefault();
			}

			//Remove empty rows
			uploadedRatingTaskMatchers = uploadedRatingTaskMatchers.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.Description ) ).ToList();

			//Convert the existing data
			//scenario - uploading E9 and now matching against 786 existing
			var existingRatingTaskMatchers = GetSheetMatchersFromExisting<RatingTask, MatchableRatingTask>( existingRatingTasks );
			foreach ( var matcher in existingRatingTaskMatchers )
			{
				//CodedNotation??
				//Identifier
				//BilletTitle
				//matcher.Flattened.HasBillet = existingBilletTitles.Where( m => matcher.Data.HasBillet.Contains( m.RowId ) ).Select( m => m.RowId ).ToList();

				matcher.Flattened.HasRating_CodedNotation = existingRatings.Where( m => matcher.Data.HasRating.Contains( m.RowId ) ).Select( m => m.CodedNotation ).ToList();
				matcher.Flattened.HasTrainingTask_Description = existingTrainingTasks.FirstOrDefault( m => m.RowId == matcher.Data.HasTrainingTask )?.Description;
				matcher.Flattened.HasReferenceResource_Name = existingReferenceResources.FirstOrDefault( m => m.RowId == matcher.Data.HasReferenceResource )?.Name;
				matcher.Flattened.HasWorkRole_Name = existingWorkRoles.Where( m => matcher.Data.HasWorkRole.Contains( m.RowId ) ).Select( m => m.Name ).ToList();
				matcher.Flattened.PayGradeType_CodedNotation = payGradeTypeConcepts.FirstOrDefault( m => m.RowId == matcher.Data.PayGradeType )?.CodedNotation;
				matcher.Flattened.ApplicabilityType_Name = applicabilityTypeConcepts.FirstOrDefault( m => m.RowId == matcher.Data.ApplicabilityType )?.Name;
				matcher.Flattened.TrainingGapType_Name = trainingGapTypeConcepts.FirstOrDefault( m => m.RowId == matcher.Data.TrainingGapType )?.Name;
				matcher.Flattened.ReferenceType_WorkElementType = sourceTypeConcepts.FirstOrDefault( m => m.RowId == matcher.Data.ReferenceType )?.WorkElementType;
				matcher.Flattened.HasReferenceResource_PublicationDate = existingReferenceResources.FirstOrDefault( m => m.RowId == matcher.Data.HasReferenceResource )?.PublicationDate;

				//not sure, but with a one-to-one relationship, CA will always be replaced
			}

			//Get loose matches
			var looseMatchingRatingTaskMatchers = new List<SheetMatcher<RatingTask, MatchableRatingTask>>();
			foreach ( var uploaded in uploadedRatingTaskMatchers )
			{
				var matches = existingRatingTaskMatchers.Where( n =>
					uploaded.Flattened.HasTrainingTask_Description == n.Flattened.HasTrainingTask_Description &&
					uploaded.Flattened.HasReferenceResource_Name == n.Flattened.HasReferenceResource_Name &&
					uploaded.Flattened.ApplicabilityType_Name == n.Flattened.ApplicabilityType_Name &&
					uploaded.Flattened.TrainingGapType_Name == n.Flattened.TrainingGapType_Name &&
					uploaded.Flattened.ReferenceType_WorkElementType == n.Flattened.ReferenceType_WorkElementType
				).ToList();
				//????
				var matches2 = existingRatingTaskMatchers.Where( n =>
					uploaded.Flattened.CodedNotation == n.Flattened.CodedNotation
				).ToList();

				HandleLooseMatchesFound( matches, uploaded, looseMatchingRatingTaskMatchers, summary, "Rating Task", m => m?.Description );
			}

			//Items that are completely identical (unchanged)
			var identicalRatingTaskMatchers = looseMatchingRatingTaskMatchers.Where( m =>
				existingRatingTaskMatchers.Where( n =>
					m.Data == n.Data &&
					//Consider it an exact match if the existing data already references this rating, regardless of whether it also references other ratings
					n.Flattened.HasRating_CodedNotation.Intersect( m.Flattened.HasRating_CodedNotation ).Count() > 0 &&
					//Consider it an exact match if the existing data already references this work role, regardless of whether it also references other work roles
					n.Flattened.HasWorkRole_Name.Intersect( m.Flattened.HasWorkRole_Name ).Count() > 0
				).Count() > 0
			).ToList();
			summary.UnchangedCount.RatingTask = identicalRatingTaskMatchers.Count();

			//Items that match, but have a change (i.e., a different value for one or more List<>s)
			var updatedRatingTaskMatchers = looseMatchingRatingTaskMatchers.Where( m => !identicalRatingTaskMatchers.Contains( m ) ).ToList();
			foreach ( var item in updatedRatingTaskMatchers )
			{
				var existingMatcher = existingRatingTaskMatchers.FirstOrDefault( m => m.Data == item.Data );
				AppendLookup( summary, existingMatcher.Data );

				//Handle added items
				var newRatings = existingRatings.Where( n => item.Flattened.HasRating_CodedNotation.Except( existingMatcher.Flattened.HasRating_CodedNotation ).Contains( n.CodedNotation ) ).ToList();
				var newWorkRoles = existingWorkRoles.Concat( summary.ItemsToBeCreated.WorkRole ).Where( n => item.Flattened.HasWorkRole_Name.Except( existingMatcher.Flattened.HasWorkRole_Name ).Contains( n.Name ) ).ToList();
				AppendLookup( summary, newWorkRoles );
				HandleInnerItemChanges(
					existingMatcher.Data.RowId,
					m => {
						m.HasRating = newRatings.Select( n => n.RowId ).ToList();
						m.HasWorkRole = newWorkRoles.Select( n => n.RowId ).ToList();
					},
					m => m.HasRating.Count() > 0 || m.HasWorkRole.Count > 0,
					summary.AddedItemsToInnerListsForCopiesOfItems.RatingTask
				);

				//Handle removed items
				//Not applicable for Rating Tasks at this time
			}

			//Items that were not found in the existing data (brand new items)
			var newRatingTaskMatchers = uploadedRatingTaskMatchers.Where( m => !looseMatchingRatingTaskMatchers.Contains( m ) ).ToList();
			foreach ( var item in newRatingTaskMatchers )
			{
				CreateNewItem( summary.ItemsToBeCreated.RatingTask, m =>
				{
					m.Description = item.Flattened.Description;
					m.CodedNotation = item.Flattened.HasCodedNotation;
					m.Identifier = item.Flattened.Identifier;
					//???
					m.HasBilletTitle = item.Flattened.HasBilletTitle;
					m.ApplicabilityType = FindConceptOrError( applicabilityTypeConcepts, new Concept() { Name = item.Flattened.ApplicabilityType_Name }, "Applicability Type", item.Flattened.ApplicabilityType_Name, summary.Messages.Error ).RowId;
					m.TrainingGapType = FindConceptOrError( trainingGapTypeConcepts, new Concept() { Name = item.Flattened.TrainingGapType_Name }, "Training Gap Type", item.Flattened.TrainingGapType_Name, summary.Messages.Error ).RowId;
					m.PayGradeType = FindConceptOrError( payGradeTypeConcepts, new Concept() { CodedNotation = item.Flattened.PayGradeType_CodedNotation }, "Pay Grade Type", item.Flattened.PayGradeType_CodedNotation, summary.Messages.Error ).RowId;
					m.ReferenceType = FindConceptOrError( sourceTypeConcepts, new Concept() { WorkElementType = item.Flattened.ReferenceType_WorkElementType }, "Reference Resource Type", item.Flattened.ReferenceType_WorkElementType, summary.Messages.Error ).RowId;
					m.HasRating = new List<Guid>() { currentRating.RowId };
					m.HasWorkRole = existingWorkRoles.Concat( summary.ItemsToBeCreated.WorkRole ).Where( n => item.Flattened.HasWorkRole_Name.Contains( n.Name ) ).Select( n => n.RowId ).ToList();

					m.HasReferenceResource = existingReferenceResources.Concat( summary.ItemsToBeCreated.ReferenceResource ).Where( n =>
						item.Flattened.HasReferenceResource_Name == n.Name
						//&& ParseDateOrEmpty( item.Flattened.HasReferenceResource_PublicationDate ) == ParseDateOrEmpty( n.PublicationDate )
						//22-03-24 mp - just do string compare due to various formats
						&& item.Flattened.HasReferenceResource_PublicationDate == n.PublicationDate
						&& sourceTypeConcepts.Where( o => n.ReferenceType.Contains( o.RowId ) ).Select( o => o.WorkElementType ).ToList().Contains( item.Flattened.ReferenceType_WorkElementType )
					).Select( n => n.RowId ).FirstOrDefault();

					m.HasTrainingTask = existingTrainingTasks.Concat( summary.ItemsToBeCreated.TrainingTask ).Where( n =>
						item.Flattened.HasTrainingTask_Description == n.Description
					).Select( n => n.RowId ).FirstOrDefault();
					m.Note = item.Flattened.Note;
				} );
			}

			//Items that were not found in the uploaded data (deleted items)
			var missingRatingTaskMatchers = existingRatingTaskMatchers.Where( m => !looseMatchingRatingTaskMatchers.Contains( m ) ).ToList();
			foreach ( var item in missingRatingTaskMatchers )
			{
				summary.ItemsToBeDeleted.RatingTask.Add( item.Data );
			}

			//Special handling for Rating Tasks that may appear in other ratings
			var ratingTasksThatMayExistUnderOtherRatings = summary.ItemsToBeCreated.RatingTask.Select( m => new RatingTaskComparisonHelper() { PossiblyNewRatingTask = m } ).ToList();
			//TODO: Implement this
			//RatingTaskManager.FindMatchingExistingRatingTasksInBulk( ratingTasksThatMayExistUnderOtherRatings ); //Should modify the contents of the list and return void
			foreach ( var foundTask in ratingTasksThatMayExistUnderOtherRatings.Where( m => m.MatchingExistingRatingTask != null && m.MatchingExistingRatingTask.Id > 0 ) )
			{
				summary.ItemsToBeCreated.RatingTask.Remove( foundTask.PossiblyNewRatingTask );
				AppendLookup( summary, foundTask.MatchingExistingRatingTask );
				summary.AddedItemsToInnerListsForCopiesOfItems.RatingTask.Add( new RatingTask()
				{
					RowId = foundTask.MatchingExistingRatingTask.RowId,
					HasRating = new List<Guid>() { currentRating.RowId }
				} );
			}
		}
		//
		//

		//Billet Title
		public static void HandleUploadSheet_BilletTitle( List<UploadableRow> uploadedRows, ChangeSummary summary, Rating currentRating, List<BilletTitle> existingBilletTitles = null, List<RatingTask> existingRatingTasks = null, List<ReferenceResource> existingReferenceResources = null, List<Concept> payGradeConcepts = null, List<Concept> sourceTypeConcepts = null, List<Concept> applicabilityTypeConcepts = null, List<Concept> trainingGapTypeConcepts = null )
		{
			//Get the existing data
			existingBilletTitles = existingBilletTitles ?? new List<BilletTitle>();// BilletTitleManager.GetAll(); //Need this get method
			existingReferenceResources = existingReferenceResources ?? ReferenceResourceManager.GetAll();
			payGradeConcepts = payGradeConcepts ?? ConceptSchemeManager.GetbyShortUri( ConceptSchemeManager.ConceptScheme_Pay_Grade ).Concepts;
			sourceTypeConcepts = sourceTypeConcepts ?? ConceptSchemeManager.GetbyShortUri( ConceptSchemeManager.ConceptScheme_ReferenceResource ).Concepts;
			applicabilityTypeConcepts = applicabilityTypeConcepts ?? ConceptSchemeManager.GetbyShortUri( ConceptSchemeManager.ConceptScheme_TaskApplicability ).Concepts;
			trainingGapTypeConcepts = trainingGapTypeConcepts ?? ConceptSchemeManager.GetbyShortUri( ConceptSchemeManager.ConceptScheme_TrainingGap ).Concepts;

			//Convert the uploaded data
			//NOTE focus on the Rows list, the Flattened data seems not relevant yet, it will be populated
			var uploadedBilletTitleMatchers = GetSheetMatchers<BilletTitle, MatchableBilletTitle>( uploadedRows, GetRowMatchHelper_BilletTitle );
			foreach ( var matcher in uploadedBilletTitleMatchers )
			{
				matcher.Flattened.HasRating_CodedNotation = currentRating.CodedNotation;
				
				matcher.Flattened.Name = matcher.Rows.Select( m => m.BilletTitle_Name ).FirstOrDefault();
				matcher.Flattened.HasRatingTask_MatchHelper = matcher.Rows.Select( m => GetRowMatchHelper_RatingTask( m ) ).Distinct().ToList();
				//this would be a list
				matcher.Flattened.HasRatingTaskCodedNotation = matcher.Rows.Select( m => m.Row_CodedNotation ).Distinct().ToList();
			}

			//Remove empty rows
			uploadedBilletTitleMatchers = uploadedBilletTitleMatchers.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.Name ) ).ToList();

			//Convert the existing data
			var existingBilletTitleMatchers = GetSheetMatchersFromExisting<BilletTitle, MatchableBilletTitle>( existingBilletTitles );
			foreach(var matcher in existingBilletTitleMatchers )
			{
				matcher.Flattened.HasRating_CodedNotation = currentRating.CodedNotation;
				matcher.Flattened.HasRatingTask_MatchHelper = existingRatingTasks.Where( m => matcher.Data.HasRatingTask.Contains( m.RowId ) ).Select( m =>
					GetExistingMatchHelper_RatingTask(m, payGradeConcepts, existingReferenceResources, sourceTypeConcepts, applicabilityTypeConcepts, trainingGapTypeConcepts )
				).ToList();
			}

			//Get loose matches
			var looseMatchingBilletTitleMatchers = new List<SheetMatcher<BilletTitle, MatchableBilletTitle>>();
			foreach ( var uploaded in uploadedBilletTitleMatchers )
			{
				var matches = existingBilletTitleMatchers.Where( m =>
					uploaded.Flattened.HasRating_CodedNotation == m.Flattened.HasRating_CodedNotation &&
					uploaded.Flattened.Name == m.Flattened.Name
				).ToList();

				HandleLooseMatchesFound( matches, uploaded, looseMatchingBilletTitleMatchers, summary, "Billet Title", m => m?.Name );
			}

			//Items that are completely identical (unchanged)
			var identicalBilletTitleMatchers = looseMatchingBilletTitleMatchers.Where( m =>
				 existingBilletTitleMatchers.Where( n =>
					m.Flattened.HasRatingTask_MatchHelper == n.Flattened.HasRatingTask_MatchHelper
				 ).Count() > 0
			).ToList();
			summary.UnchangedCount.BilletTitle = identicalBilletTitleMatchers.Count();

			//Special helper to save processing time on Rating Tasks that are existing/new
			var combinedTaskMatchHelpers = existingRatingTasks.Concat( summary.ItemsToBeCreated.RatingTask ).Select( m => new MergedMatchHelper<RatingTask>()
			{
				Data = m,
				MatchHelper = GetExistingMatchHelper_RatingTask( m, payGradeConcepts, existingReferenceResources, sourceTypeConcepts, applicabilityTypeConcepts, trainingGapTypeConcepts )
			} );

			//Items that match, but have a change (i.e., a different value for one or more List<>s)
			var updatedBilletTitleMatchers = looseMatchingBilletTitleMatchers.Where( m => !identicalBilletTitleMatchers.Contains( m ) ).ToList();
			foreach ( var item in updatedBilletTitleMatchers )
			{
				var existingMatcher = existingBilletTitleMatchers.FirstOrDefault( m => m.Data == item.Data );

				//Handle added items
				var newRatingTasks = combinedTaskMatchHelpers.Where( n => item.Flattened.HasRatingTask_MatchHelper.Except( existingMatcher.Flattened.HasRatingTask_MatchHelper ).Contains( n.MatchHelper ) ).ToList();
				AppendLookup( summary, newRatingTasks );
				HandleInnerItemChanges(
					existingMatcher.Data.RowId,
					m => m.HasRatingTask = newRatingTasks.Select( n => n.Data.RowId ).ToList(),
					m => m.HasRatingTask.Count() > 0,
					summary.AddedItemsToInnerListsForCopiesOfItems.BilletTitle
				);

				//Handle removed items
				var removedRatingTasks = combinedTaskMatchHelpers.Where( n => existingMatcher.Flattened.HasRatingTask_MatchHelper.Except( item.Flattened.HasRatingTask_MatchHelper ).Contains( n.MatchHelper ) ).ToList();
				AppendLookup( summary, removedRatingTasks );
				HandleInnerItemChanges(
					existingMatcher.Data.RowId,
					m => m.HasRatingTask = removedRatingTasks.Select( n => n.Data.RowId ).ToList(),
					m => m.HasRatingTask.Count() > 0,
					summary.RemovedItemsFromInnerListsForCopiesOfItems.BilletTitle
				);

			}

			//Items that were not found in the existing data (brand new items)
			var newBilletTitleMatchers = uploadedBilletTitleMatchers.Where( m => !looseMatchingBilletTitleMatchers.Contains( m ) ).ToList();
			foreach ( var item in newBilletTitleMatchers )
			{
				CreateNewItem( summary.ItemsToBeCreated.BilletTitle, m => {
					m.Name = item.Flattened.Name;
					m.HasRating = currentRating.RowId;
					m.HasRatingTask = combinedTaskMatchHelpers.Where( n =>
						 item.Flattened.HasRatingTask_MatchHelper.Contains( n.MatchHelper )
					).Select( n => n.Data.RowId ).ToList();
					m.HasRatingTaskByCode = item.Flattened.HasRatingTaskCodedNotation;
				} );
			}

			//Items that were not found in the uploaded data (deleted items)
			var missingBilletTitleMatchers = existingBilletTitleMatchers.Where( m => !looseMatchingBilletTitleMatchers.Contains( m ) ).ToList();
			foreach ( var item in missingBilletTitleMatchers )
			{
				summary.ItemsToBeDeleted.BilletTitle.Add( item.Data );
			}
		}
		//

		private static void AppendLookup<T>( ChangeSummary summary, List<T> items )
		{
			summary.LookupGraph.AddRange( items.Where( m => !summary.LookupGraph.Contains( m ) ).Select( m => ( object ) m ).ToList() );
		}
		//

		private static void AppendLookup<T>( ChangeSummary summary, T item )
		{
			AppendLookup( summary, new List<T>() { item } );
		}
		//

		private static string GetExistingMatchHelper_RatingTask(RatingTask task, List<Concept> payGradeConcepts, List<ReferenceResource> existingReferenceResources, List<Concept> sourceTypeConcepts, List<Concept> applicabilityTypeConcepts, List<Concept> trainingGapTypeConcepts )
		{
			return GetMergedRowMatchHelper( new List<string>()
			{
				payGradeConcepts.Where(n => task.PayGradeType == n.RowId).Select(n => n.CodedNotation).FirstOrDefault(),
				existingReferenceResources.Where(n => task.HasReferenceResource == n.RowId).Select(n => n.Name).FirstOrDefault(),
				existingReferenceResources.Where(n => task.HasReferenceResource == n.RowId).Select(n => n.PublicationDate).FirstOrDefault(),
				sourceTypeConcepts.Where(n => task.ReferenceType == n.RowId).Select(n => n.Name).FirstOrDefault(),
				task.Description,
				applicabilityTypeConcepts.Where(n => task.ApplicabilityType == n.RowId).Select(n => n.Name).FirstOrDefault(),
				trainingGapTypeConcepts.Where(n => task.TrainingGapType == n.RowId).Select(n => n.Name).FirstOrDefault()
			} );
		}
		//

		private static void HandleInnerItemChanges<T>( Guid existingItemRowId, Action<T> appendData, Func<T, bool> changeCheck, List<T> summaryContainer ) where T : BaseObject, new()
		{
			var innerChanges = new T() { RowId = existingItemRowId };
			appendData( innerChanges );
			if ( changeCheck( innerChanges ) )
			{
				summaryContainer.Add( innerChanges );
			}
		}
		//

		private static bool AllMatch<T>( List<T> list1, List<T> list2 )
		{
			return list1.Except( list2 ).Count() == 0 && list2.Except( list1 ).Count() == 0;
		}
		//

		private static void CreateNewItem<T>( List<T> summaryContainerForItemsToBeCreated, Action<T> appendData ) where T : BaseObject, new()
		{
			var item = new T()
			{
				RowId = Guid.NewGuid(),
				CTID = "ce-" + Guid.NewGuid().ToString().ToLower()
			};
			appendData( item );
			summaryContainerForItemsToBeCreated.Add( item );
		}
		//

		public static List<SheetMatcher<T1, T2>> GetSheetMatchersFromExisting<T1, T2>( List<T1> existingItems ) where T1 : new() where T2 : new()
		{
			var matchers = new List<SheetMatcher<T1, T2>>();
			var cntr = 0;
			foreach( var existing in existingItems )
			{
                cntr++;
                if ( cntr > 16 )
                {

                }
                var matcher = new SheetMatcher<T1, T2>() { Data = existing };
				BaseFactory.AutoMap( existing, matcher.Flattened );
				matchers.Add( matcher );
			}

			return matchers;
		}
		//

		public static void HandleLooseMatchesFound<T1, T2>( List<SheetMatcher<T1, T2>> matchesForUploadedItem, SheetMatcher<T1, T2> uploadedItem, List<SheetMatcher<T1, T2>> looseMatchingMatchersForType, ChangeSummary summary, string warningTypeLabel, Func<T2, string> getWarningItemLabel ) where T1 : BaseObject, new() where T2 : new()
		{
			if( matchesForUploadedItem.Count() > 0 )
			{
				uploadedItem.Data = matchesForUploadedItem.FirstOrDefault()?.Data;
				looseMatchingMatchersForType.Add( uploadedItem );
			}
			if( matchesForUploadedItem.Count() > 1 )
			{
				summary.Messages.Warning.Add( "Found more than one existing match for " + warningTypeLabel + " (only the first existing match will be updated): " + string.Join( ", ", matchesForUploadedItem.Select( m => m.Data?.RowId.ToString() ) ) + " match: " + getWarningItemLabel( uploadedItem.Flattened ) );
				summary.PossibleDuplicates.Add( new PossibleDuplicateSet() { Type = typeof( T1 ).Name, Items = matchesForUploadedItem.Select( m => (object) m.Data ).ToList() } );
			}
		}
		//

		/// <summary>
		/// What is happening here?
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <param name="rows"></param>
		/// <param name="matchFunction"></param>
		/// <returns></returns>
		private static List<SheetMatcher<T1, T2>> GetSheetMatchers<T1, T2>( List<UploadableRow> rows, Func<UploadableRow, string> matchFunction ) where T1: new() where T2: new()
		{
			var keys = rows.GroupBy( m => matchFunction( m ) ).Select( m => m.Key ).Distinct().ToList();
			return keys.Select( m => new SheetMatcher<T1, T2>()
			{
				Flattened = new T2(),
				Rows = rows.Where( n => matchFunction( n ) == m ).ToList()
			} ).ToList();
		}
		//

		private static string GetMergedRowMatchHelper( List<string> items )
		{
			var merged = items?.Where( m => !string.IsNullOrWhiteSpace( m ) ).ToList();
			return merged.Count() == 0 ? "" : string.Join( "__", merged );
		}
		private static string GetRowMatchHelper_Rating( UploadableRow row )
		{
			return GetMergedRowMatchHelper( new List<string>()
			{
				row.Rating_CodedNotation
			} );
		}
		private static string GetRowMatchHelper_BilletTitle( UploadableRow row )
		{
			return GetMergedRowMatchHelper( new List<string>()
			{
				row.BilletTitle_Name
			} );
		}
		private static string GetRowMatchHelper_Course( UploadableRow row )
		{
			return GetMergedRowMatchHelper( new List<string>() 
			{ 
				row.Course_LifeCycleControlDocumentType_CodedNotation, 
				row.Course_CodedNotation, 
				row.Course_Name, 
				row.Course_CourseType_Name, 
				row.Course_CurriculumControlAuthority_Name 
			} );
		}
		private static string GetRowMatchHelper_Organization( UploadableRow row )
		{
			return GetMergedRowMatchHelper( new List<string>()
			{
				row.Course_CurriculumControlAuthority_Name
			} );
		}
		private static string GetRowMatchHelper_RatingTask( UploadableRow row )
		{
			return GetMergedRowMatchHelper( new List<string>()
			{
				row.PayGradeType_CodedNotation,
				row.ReferenceResource_Name,
				row.ReferenceResource_PublicationDate,
				row.Shared_ReferenceType,
				row.RatingTask_Description,
				row.RatingTask_ApplicabilityType_Name,
				row.RatingTask_TrainingGapType_Name
			} );
		}
		private static string GetRowMatchHelper_ReferenceResource_Task( UploadableRow row )
		{
			return GetMergedRowMatchHelper( new List<string>()
			{
				row.ReferenceResource_Name,
				row.ReferenceResource_PublicationDate
			} );
		}
		private static string GetRowMatchHelper_ReferenceResource_Course( UploadableRow row )
		{
			return GetMergedRowMatchHelper( new List<string>()
			{
				row.Course_LifeCycleControlDocumentType_CodedNotation
			} );
		}
		private static string GetRowMatchHelper_TrainingTask( UploadableRow row )
		{
			return GetMergedRowMatchHelper( new List<string>()
			{
				row.Course_CodedNotation,
				row.TrainingTask_Description
			} );
		}
		private static string GetRowMatchHelper_WorkRole( UploadableRow row )
		{
			return GetMergedRowMatchHelper( new List<string>()
			{
				row.WorkRole_Name
			} );
		}
		//
		#endregion

		#region ProcessUpload V3 (UploadableItem/Row by Row)

		public static UploadableItemResult ProcessUploadedItem( UploadableItem item )
		{
			//Hold the result
			var result = new UploadableItemResult() { Valid = true };
			
			//Get the current transaction's summary, or create it if it doesn't exist
			var summary = GetCachedChangeSummary( item.TransactionGUID );
			if ( summary == null )
			{
				//Create a new summary object
				summary = new ChangeSummary() { RowId = item.TransactionGUID };

				//Pre-populate controlled vocabularies, since these will be used across all rows
				summary.LookupGraph.AddRange( ConceptSchemeManager.GetAllConcepts( true ) );
				summary.LookupGraph.AddRange( RatingManager.GetAll() );
				summary.LookupGraph.AddRange( OrganizationManager.GetAll() );

				//Cache the summary object
				CacheChangeSummary( summary );
			}

			AppUser user = AccountServices.GetCurrentUser();
			if ( user?.Id == 0 )
			{
				result.Message = "Error - a current user was not found. You must authenticated and authorized to use this function!";
				return result;
			}
			//Do something with the item and summary.  
			//The summary will be stored in the cache and retrieved again so that it can be used for all of the rows. 
			//At the end of the process, the summary will contain all of the changes, just like in V2.
			//Since each row's result is being sent back to the client on a row-by-row basis, the client will also aggregate a list of statuses/changes for each row (no need to send back the summary itself).

			//Processing
			//Get the controlled value items that show up in this row
			//Everything in any uploaded sheet should appear here. If any of these are not found, it's an error
			//Might also be an error if rowRating is the "ALL" Rating, now
			var rowRating = GetDataOrError<Rating>( summary, ( m ) => m.CodedNotation?.ToLower() == item.Row.Rating_CodedNotation?.ToLower(), result, "Rating not found in database: " + item.Row.Rating_CodedNotation ?? "" );
			var rowPayGrade = GetDataOrError<Concept>( summary, ( m ) => m.CodedNotation?.ToLower() == item.Row.PayGradeType_CodedNotation?.ToLower(), result, "Rank not found in database: " + item.Row.PayGradeType_CodedNotation ?? "" );
			var rowSourceType = GetDataOrError<Concept>( summary, ( m ) => m.WorkElementType?.ToLower() == item.Row.Shared_ReferenceType?.ToLower(), result, "Work Element Type not found in database: " + item.Row.Shared_ReferenceType ?? "" );
			var rowTaskApplicabilityType = GetDataOrError<Concept>( summary, ( m ) => m.Name?.ToLower() == item.Row.RatingTask_ApplicabilityType_Name?.ToLower(), result, "Task Applicability Type not found in database: " + item.Row.RatingTask_ApplicabilityType_Name ?? "" );
			var rowTrainingGapType = GetDataOrError<Concept>( summary, ( m ) => m.Name?.ToLower() == item.Row.RatingTask_TrainingGapType_Name?.ToLower(), result, "Training Gap Type not found in database: " + item.Row.RatingTask_TrainingGapType_Name ?? "" );
			
			//If any of the above are null, log an error and skip the rest
			if( new List<object>() { rowRating, rowPayGrade, rowSourceType, rowTaskApplicabilityType, rowTrainingGapType }.Where(m => m == null).Count() > 0 )
			{
				result.Message = "Unable to continue: One or more critical errors found";
				return result;
			}

			//These should throw an error if not found, unless all of the course/training columns are N/A
			var shouldNotHaveTrainingData = rowTrainingGapType.Name?.ToLower() == "yes";
			var rowCourseType = GetDataOrError<Concept>( summary, ( m ) => m.Name?.ToLower() == item.Row.Course_CourseType_Name?.ToLower(), result, "Course Type not found in database: " + item.Row.Course_CourseType_Name ?? "" );
			var rowOrganizationCCA = GetDataOrError<Organization>( summary, ( m ) => (m.Name != null && m.Name.ToLower() == item.Row.Course_CurriculumControlAuthority_Name?.ToLower()) || (m.ShortName != null && m.ShortName.ToLower() == item.Row.Course_CurriculumControlAuthority_Name?.ToLower()), result, "Curriculum Control Authority not found in database: " + item.Row.Course_CurriculumControlAuthority_Name ?? "" );
			var rowCourseSourceType = GetDataOrError<Concept>( summary, ( m ) => m.Name?.ToLower() == item.Row.Course_LifeCycleControlDocumentType_CodedNotation?.ToLower(), result, "Life-Cycle Control Document Type not found in database: " + item.Row.Course_LifeCycleControlDocumentType_CodedNotation ?? "" );
			var rowAssessmentMethodType = GetDataOrError<Concept>( summary, ( m ) => m.Name?.ToLower() == item.Row.Course_AssessmentMethodType_Name?.ToLower(), result, "Assessment Method Type not found in database: " + item.Row.Course_AssessmentMethodType_Name ?? "" );
			
			//If the Training Gap Type is "Yes", then treat all course/training data as null, but check to see if it exists first (above) to facilitate the warning statement below
			if( shouldNotHaveTrainingData )
			{
				var hasDataWhenItShouldNot = new List<object>() { rowCourseType, rowOrganizationCCA, rowCourseSourceType, rowAssessmentMethodType }.Where( m => m != null ).ToList();
				if ( hasDataWhenItShouldNot.Count() > 0 || !string.IsNullOrWhiteSpace( item.Row.TrainingTask_Description ) )
				{
					result.Warnings.Add( "Incomplete course/training data found. All course/training related columns should either have data or be marked as \"N/A\". Since the Training Gap Type is \"Yes\", the incomplete data will be treated as \"N/A\"." );
					rowCourseType = null;
					rowOrganizationCCA = null;
					rowCourseSourceType = null;
					rowAssessmentMethodType = null;
				}

				//Remove false errors
				result.Errors = new List<string>();
			}
			//Otherwise, return an error if any course/training data is missing
			else if( new List<object>() { rowCourseType, rowOrganizationCCA, rowCourseSourceType, rowAssessmentMethodType }.Where( m => m == null ).Count() > 0 || string.IsNullOrWhiteSpace( item.Row.TrainingTask_Description ) )
			{
				result.Errors.Add( "Incomplete course/training data found. All course/training related columns should either have data or be marked as \"N/A\". Since the Training Gap Type is \"" + rowTrainingGapType.Name + "\", this is an error and processing this row cannot continue." );
				return result;
			}

			//Special handling for "All" ratings
			var allRatingsRating = GetDataOrError<Rating>( summary, ( m ) => m.CodedNotation?.ToLower() == "all", result, "Rating Type of \"ALL\" not found in database." );
			var isAllRatingsRow = rowTaskApplicabilityType?.Name.ToLower().Contains( "all sailors" ) ?? false;

			//Get the variable items that show up in this row
			//Things from sheets here may be new/edited
			//First check the summary to see if it came in from an earlier row (or database). If not found in the summary, check the database. If not found there, assume it's new. Regardless, add it to the summary if it's not already in the summary.

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
				(newItem) => { summary.ItemsToBeCreated.BilletTitle.Add( newItem ); }
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

			//Source/Reference Resource for RatingTask
			var rowRatingTaskSource = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<ReferenceResource>()
				.FirstOrDefault( m =>
					m.Name?.ToLower() == item.Row.ReferenceResource_Name?.ToLower() &&
					m.PublicationDate?.ToLower() == item.Row.ReferenceResource_PublicationDate?.ToLower() &&
					m.ReferenceType.Contains( rowSourceType.RowId )
				),
				//Or get from DB
				() => ReferenceResourceManager.Get( item.Row.ReferenceResource_Name, item.Row.ReferenceResource_PublicationDate ),
				//Or create new
				() => new ReferenceResource()
				{
					RowId = Guid.NewGuid(),
					Name = item.Row.ReferenceResource_Name,
					PublicationDate = item.Row.ReferenceResource_PublicationDate,
					ReferenceType = new List<Guid>() { rowSourceType.RowId }
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.ReferenceResource.Add( newItem ); }
			);


			//Training Task
			var rowTrainingTask = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<TrainingTask>()
				.FirstOrDefault( m =>
					 m.Description?.ToLower() == item.Row.TrainingTask_Description?.ToLower()
				),
				//Or get from DB
				() => TrainingTaskManager.Get( item.Row.TrainingTask_Description ),
				//Or create new
				() => new TrainingTask()
				{
					RowId = Guid.NewGuid(),
					Description = item.Row.TrainingTask_Description
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.TrainingTask.Add( newItem ); },
				//Skip all of this and set value to null if the following test is true
				() => string.IsNullOrWhiteSpace( item.Row.TrainingTask_Description ) || item.Row.TrainingTask_Description.ToLower() == "n/a"
			);

			//Course
			var rowCourse = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<Course>()
				.FirstOrDefault( m =>
					 m.Name?.ToLower() == item.Row.Course_Name?.ToLower() &&
					 m.CodedNotation?.ToLower() == item.Row.Course_CodedNotation?.ToLower() &&
					 m.CourseType.Contains( rowCourseType.RowId ) &&
					 m.CurriculumControlAuthority.Contains( rowOrganizationCCA.RowId )
				),
				//Or get from DB
				() => CourseManager.GetForUpload( item.Row.Course_Name, item.Row.Course_CodedNotation, rowCourseType.RowId, rowOrganizationCCA.RowId ),
				//Or create new
				() => new Course()
				{
					RowId = Guid.NewGuid(),
					Name = item.Row.Course_Name,
					CodedNotation = item.Row.Course_CodedNotation,
					CourseType = new List<Guid>() { rowCourseType.RowId },
					LifeCycleControlDocumentType = rowCourseSourceType.RowId,
					CurriculumControlAuthority = new List<Guid>() { rowOrganizationCCA.RowId }
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.Course.Add( newItem ); },
				//Skip all of this and set value to null if the following test is true
				() => string.IsNullOrWhiteSpace( item.Row.Course_Name ) || item.Row.Course_Name.ToLower() == "n/a"
			);
			//check if coded notation exists
			var exists = summary.GetAll<RatingTask>()
				.FirstOrDefault( m => m.CodedNotation?.ToLower() == item.Row.Row_CodedNotation?.ToLower());
			if ( exists != null )
            {
				result.Errors.Add( string.Format("Row: {0}. The row Unique Idenitifier: '{1}' was used on a previous row.", item.Row.Row_Index, item.Row.Row_CodedNotation) );
			}

			//Rating Task
			var rowRatingTask = new RatingTask();
			if ( user.LastName == "Parsons" )
			{
				rowRatingTask = LookupOrGetFromDBOrCreateNew( summary, result,
					//Find in summary
					() => summary.GetAll<RatingTask>()
					.FirstOrDefault( m =>
						m.Description?.ToLower() == item.Row.RatingTask_Description?.ToLower() &&
						m.ApplicabilityType == rowTaskApplicabilityType.RowId &&
						m.TrainingGapType == rowTrainingGapType.RowId &&
						m.PayGradeType == rowPayGrade.RowId &&
						m.ReferenceType == rowSourceType.RowId &&
						m.HasReferenceResource == rowRatingTaskSource.RowId //Ensure rowRatingTaskSource is fully found/processed first or this lookup will fail
					),
					//Or get from DB
					() => RatingTaskManager.GetForUpload( rowRating.CodedNotation, item.Row.Row_CodedNotation ),
					//Or create new
					() => new RatingTask()
					{
						RowId = Guid.NewGuid(),
						Description = item.Row.RatingTask_Description,
						ApplicabilityType = rowTaskApplicabilityType.RowId,
						TrainingGapType = rowTrainingGapType.RowId,
						ReferenceType = rowSourceType.RowId,
						HasReferenceResource = rowRatingTaskSource.RowId,
						Note = item.Row.Note //Should Note be part of the uniqueness checks?
				},
					//Store if newly created
					( newItem ) => { summary.ItemsToBeCreated.RatingTask.Add( newItem ); }
				);
			}
			else
			{
				rowRatingTask = LookupOrGetFromDBOrCreateNew( summary, result,
					//Find in summary
					() => summary.GetAll<RatingTask>()
					.FirstOrDefault( m =>
						m.Description?.ToLower() == item.Row.RatingTask_Description?.ToLower() &&
						m.ApplicabilityType == rowTaskApplicabilityType.RowId &&
						m.TrainingGapType == rowTrainingGapType.RowId &&
						m.PayGradeType == rowPayGrade.RowId &&
						m.ReferenceType == rowSourceType.RowId &&
						m.HasReferenceResource == rowRatingTaskSource.RowId //Ensure rowRatingTaskSource is fully found/processed first or this lookup will fail
					),
					//Or get from DB
					() => RatingTaskManager.GetForUpload( rowRating.Id, item.Row.RatingTask_Description, rowTaskApplicabilityType.RowId, rowRatingTaskSource.RowId, rowSourceType.RowId, rowPayGrade.RowId, rowTrainingGapType.RowId ),
					//Or create new
					() => new RatingTask()
					{
						RowId = Guid.NewGuid(),
						Description = item.Row.RatingTask_Description,
						ApplicabilityType = rowTaskApplicabilityType.RowId,
						TrainingGapType = rowTrainingGapType.RowId,
						ReferenceType = rowSourceType.RowId,
						HasReferenceResource = rowRatingTaskSource.RowId,
						Note = item.Row.Note //Should Note be part of the uniqueness checks?
					},
					//Store if newly created
					( newItem ) => { summary.ItemsToBeCreated.RatingTask.Add( newItem ); }
				);
			}
			//Now that we have figured out who all of the actors are...
			//Handle cases where a new or existing item has associations added to it or removed from it (likely just added to it for now?)

			//Billet Title
			UpdateSummaryAndResultForItemProperty( summary, summary.ItemsToBeCreated.BilletTitle, summary.AddedItemsToInnerListsForCopiesOfItems.BilletTitle, result, rowBilletTitle, nameof( BilletTitle.HasRatingTask ), rowRatingTask );

			//Course
			UpdateSummaryAndResultForItemProperty( summary, summary.ItemsToBeCreated.Course, summary.AddedItemsToInnerListsForCopiesOfItems.Course, result, rowCourse, nameof( Course.HasTrainingTask ), rowTrainingTask );
			UpdateSummaryAndResultForItemProperty( summary, summary.ItemsToBeCreated.Course, summary.AddedItemsToInnerListsForCopiesOfItems.Course, result, rowCourse, nameof( Course.CurriculumControlAuthority ), rowOrganizationCCA );

			//Training Task
			UpdateSummaryAndResultForItemProperty( summary, summary.ItemsToBeCreated.TrainingTask, summary.AddedItemsToInnerListsForCopiesOfItems.TrainingTask, result, rowTrainingTask, nameof( TrainingTask.AssessmentMethodType ), rowAssessmentMethodType );

			//Rating Task
			UpdateSummaryAndResultForItemProperty( summary, summary.ItemsToBeCreated.RatingTask, summary.AddedItemsToInnerListsForCopiesOfItems.RatingTask, result, rowRatingTask, nameof( RatingTask.HasRating ), rowRating );
			UpdateSummaryAndResultForItemProperty( summary, summary.ItemsToBeCreated.RatingTask, summary.AddedItemsToInnerListsForCopiesOfItems.RatingTask, result, rowRatingTask, nameof( RatingTask.HasRating ), allRatingsRating );
			UpdateSummaryAndResultForItemProperty( summary, summary.ItemsToBeCreated.RatingTask, summary.AddedItemsToInnerListsForCopiesOfItems.RatingTask, result, rowRatingTask, nameof( RatingTask.HasBilletTitle ), rowBilletTitle );
			UpdateSummaryAndResultForItemProperty( summary, summary.ItemsToBeCreated.RatingTask, summary.AddedItemsToInnerListsForCopiesOfItems.RatingTask, result, rowRatingTask, nameof( RatingTask.HasWorkRole ), rowWorkRole );
			UpdateSummaryAndResultForItemProperty( summary, summary.ItemsToBeCreated.RatingTask, summary.AddedItemsToInnerListsForCopiesOfItems.RatingTask, result, rowRatingTask, nameof( RatingTask.HasTrainingTaskList ), rowTrainingTask );
			
			//Update the cached summary
			CacheChangeSummary( summary );

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

			//Return the item (or null)
			return targetItem;
		}
		//

		public static JObject JObjectify<T>( T rowItem ) where T: BaseObject
		{
			if( rowItem != null )
			{
				var newItem = JObject.FromObject( rowItem, new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None, DefaultValueHandling = DefaultValueHandling.Ignore } );
				newItem[ "@type" ] = rowItem.GetType().Name;
				return newItem;
			}

			return null;
		}
		//

		public static T GetDataOrError<T>( ChangeSummary summary, Func<T, bool> MatchWith, UploadableItemResult result, string errorMessage ) where T : BaseObject
		{
			var data = summary.GetAll<T>().FirstOrDefault( m => MatchWith( m ) );
			if( data == null )
			{
				result.Errors.Add( errorMessage );
			}

			return data;
		}
		//

		public static void UpdateSummaryAndResultForItemProperty<T1, T2>( ChangeSummary summary, List<T1> summaryItemsToBeCreatedList, List<T1> summaryAddedReferencesToExistingItemsList, UploadableItemResult result, T1 rowItem, string GUIDListPropertyName, T2 referenceToBeAppended ) where T1 : BaseObject, new() where T2 : BaseObject
		{
			//If the row item and the reference are not null...
			if( rowItem != null && referenceToBeAppended != null )
			{
				//Get the list property for the row item (e.g. BilletTitle.HasRatingTask)
				var listProperty = typeof( T1 ).GetProperty( GUIDListPropertyName );
				//Get the list itself
				var list = ( List<Guid> ) listProperty.GetValue( rowItem );

				//If the list doesn't already contain the GUID of the item to be added...
				if ( !list.Contains( referenceToBeAppended.RowId ) )
				{
					//Add the reference to the list itself (in the data, which will also update the item in the summary's new item list)
					list.Add( referenceToBeAppended.RowId );

					//For summary tracking, check to see if the item came from the database (e.g. truly already exists) or if it's from a previous row in the current upload session
					if ( summary.ItemsLoadedFromDatabase.Contains( rowItem.RowId ) )
					{
						//Find the update tracking reference for the existing item
						var match = summaryAddedReferencesToExistingItemsList.FirstOrDefault( m => m.RowId == rowItem.RowId );
						//If found, append the GUID to the list property for the match item (not the row item)
						if( match != null )
						{
							( ( List<Guid> ) listProperty.GetValue( match ) ).Add( referenceToBeAppended.RowId );
						}
						//Otherwise, create a new reference to that item, add the GUID to the list property for the new reference (not the row item), and add the new reference to the summary's list of changes for an existing item
						else
						{
							match = new T1() { RowId = rowItem.RowId };
							listProperty.SetValue( match, new List<Guid>() { referenceToBeAppended.RowId } ); //Avoid null reference for uninitialized List<Guid> by setting the value here
							summaryAddedReferencesToExistingItemsList.Add( match );
						}
					}

					//Now that the summary has been updated, also update the result for the current request
					result.Additions.Add( new Triple( rowItem.RowId, GUIDListPropertyName, referenceToBeAppended.RowId ) );
				}
			}
		}
		//

		#endregion

		private static Concept FindConceptOrError( List<Concept> haystack, Concept needle, string warningLabel, string warningValue, List<string> warningMessages )
		{
			var match = haystack?.FirstOrDefault( m => MatchString( m.Name, needle.Name ) || MatchString( m.CodedNotation, needle.CodedNotation ) || MatchString( m.WorkElementType, needle.WorkElementType ) );
			if ( match == null )
			{
				warningMessages.Add( "Error: Found unrecognized " + warningLabel + ": " + warningValue );
				return new Concept() { RowId = Guid.NewGuid(), Name = needle?.Name, CodedNotation = needle?.CodedNotation, WorkElementType = needle?.WorkElementType };
			}
			return match;
		}
		private static List<Concept> FindConceptListOrError( List<Concept> haystack, Concept needle, string warningLabel, string warningValue, List<string> warningMessages )
		{
			var matches = haystack?.Where( m =>
				( needle?.Name ?? "" ).ToLower().Contains( ( m?.Name ?? "" ).ToLower() )
				|| ( needle.CodedNotation != null && ( needle?.CodedNotation ?? "" ).ToLower().Contains( ( m?.CodedNotation ?? "" ).ToLower() ) )
				|| ( needle.WorkElementType != null && ( needle?.WorkElementType ?? "" ).ToLower().Contains( ( m?.WorkElementType ?? "" ).ToLower() ) )
			).ToList();
			if ( matches.Count() == 0 )
			{
				warningMessages.Add( "Error: Found unrecognized " + warningLabel + ": " + warningValue );
			}
			return matches;
		}
		//

		private static void HandlePossibleDuplicates<T>( string typeLabel, IEnumerable<IGrouping<string, T>> groupings, List<T> existingItems, List<T> newItems, List<string> duplicateMessages ) where T : BaseObject
		{
			foreach ( var maybeDuplicates in groupings.Where( m => m.Count() > 1 ).ToList() )
			{
				var itemRowIDs = maybeDuplicates.Select( m => m.RowId ).ToList();
				var existingCount = existingItems.Where( m => itemRowIDs.Contains( m.RowId ) ).Count();
				var newCount = newItems.Where( m => itemRowIDs.Contains( m.RowId ) ).Count();
				duplicateMessages.Add( "Found " + existingCount + " existing and " + newCount + " new instances of " + typeLabel + ": " + maybeDuplicates.Key );
			}
		}
		//

		//Temporary placeholder
		private static List<RatingTask> SomeMethodThatFindsRatingTasksInBulkIrrespectiveOfTheirRating( List<RatingTask> input )
		{
			return new List<RatingTask>();
		}
		//

		private static bool MatchString( string haystackCheck, string needleCheck )
		{
			return string.IsNullOrWhiteSpace( needleCheck ) ? false :
				string.IsNullOrWhiteSpace( haystackCheck ) ? false :
				haystackCheck.ToLower() == needleCheck.ToLower();
		}
		//

		private static void FlagItemsForDeletion<T>( List<T> existingItems, List<T> referencedItems, List<T> itemsToBeDeleted, List<string> changeNotes, string typeLabel, Func<T, string> getItemLabel )
		{
			foreach ( var item in existingItems.Where( m => !referencedItems.Contains( m ) ).ToList() )
			{
				itemsToBeDeleted.Add( item );
				changeNotes.Add( typeLabel + ": " + getItemLabel( item ) );
			}
		}
		//

		private static int GetUnchangedCount<T>( string propertyName, List<T> existing, ChangeSummary result ) where T : BaseObject
		{
			var property = typeof( UploadableData ).GetProperty( propertyName );
			return existing.Where( m =>
				 Find( ( List<T> ) property.GetValue( result.ItemsToBeChanged ), m.RowId ) == null &&
				 Find( ( List<T> ) property.GetValue( result.ItemsToBeDeleted ), m.RowId ) == null &&
				 Find( ( List<T> ) property.GetValue( result.AddedItemsToInnerListsForCopiesOfItems ), m.RowId ) == null &&
				 Find( ( List<T> ) property.GetValue( result.RemovedItemsFromInnerListsForCopiesOfItems ), m.RowId ) == null
			).Count();
		}
		//

		private static DateTime ParseDateOrEmpty( string value )
		{
			try
			{
				return DateTime.Parse( value );
			}
			catch
			{
				return DateTime.MinValue;
			}
		}
		//
		private static string ParseDateOrEmpty2( string value )
		{
			try
			{
				return DateTime.Parse( value ).ToString( "yyyy-MM-dd" );
			}
			catch
			{
				//or ""
				return DateTime.MinValue.ToString( "yyyy-MM-dd" );
			}
		}
		//
		private static void Append<T>( List<T> items, T item )
		{
			if ( items != null && item != null && !items.Contains( item ) )
			{
				items.Add( item );
			}
		}
		//

		private static string NullifyNotApplicable( string test )
		{
			return string.IsNullOrWhiteSpace( test ) ? null :
				test.ToLower() == "n/a" ? null :
				test;
		}

		public static T Find<T>( List<T> haystack, Guid needleRowID ) where T : BaseObject
		{
			return haystack?.FirstOrDefault( m => m?.RowId == needleRowID );
		}
		//

		public static List<T> Find<T>( List<T> haystack, List<Guid> needleRowIDs ) where T : BaseObject
		{
			return haystack?.Where( m => needleRowIDs?.Contains( m.RowId ) ?? false ).ToList();
		}
		//
	}
}
