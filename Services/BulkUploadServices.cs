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
using System.Text.RegularExpressions;

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

		public static void ApplyChangeSummaryV2( ChangeSummary summary )
		{
			//Check summary
			if( summary == null )
			{
				return;
			}

			//Check user
			var user = AccountServices.GetCurrentUser();
			if( user?.Id == 0 )
			{
				summary.Messages.Error.Add( "Error - a current user was not found. You must authenticated and authorized to use this function!" );
				return;
			}
			//Should also do a permissions check

			//Track activity
			var saveStart = DateTime.Now;
			var saveDuration = new TimeSpan();
			new ActivityServices().AddActivity( new SiteActivity()
			{
				ActivityType = "RMTL",
				Activity = "Upload",
				Event = "Save Started",
				Comment = String.Format( "A bulk upload save was initiated by: '{0}' for Rating: '{1}' was committed.", user.FullName(), summary.RatingCodedNotation ?? "" ),
				ActionByUserId = user.Id,
				ActionByUser = user.FullName()
			} );

			//Count all of the items to be saved, to assist client-side monitoring
			var countableProperties = typeof( UploadableData ).GetProperties().Where( m => m.PropertyType.IsGenericType && m.PropertyType.Name.IndexOf("List") == 0 ).ToList();
			foreach( var property in countableProperties )
			{
				var newItemsCount = ( ( System.Collections.ICollection ) property.GetValue( summary.ItemsToBeCreated ) )?.Count ?? 0;
				var existingItemsCount = ( ( System.Collections.ICollection ) property.GetValue( summary.FinalizedChanges ) )?.Count ?? 0;
				summary.TotalItemsToSaveForClientMonitoring += newItemsCount + existingItemsCount;
			}

			//Save dependencies first, then work our way back/up
			SaveItems( summary, summary.ItemsToBeCreated.Organization, summary.FinalizedChanges.Organization, user.Id, OrganizationManager.SaveFromUpload );
			SaveItems( summary, summary.ItemsToBeCreated.ReferenceResource, summary.FinalizedChanges.ReferenceResource, user.Id, ReferenceResourceManager.SaveFromUpload );
			SaveItems( summary, summary.ItemsToBeCreated.BilletTitle, summary.FinalizedChanges.BilletTitle, user.Id, JobManager.SaveFromUpload );
			SaveItems( summary, summary.ItemsToBeCreated.WorkRole, summary.FinalizedChanges.WorkRole, user.Id, WorkRoleManager.SaveFromUpload );
			SaveItems( summary, summary.ItemsToBeCreated.ClusterAnalysisTitle, summary.FinalizedChanges.ClusterAnalysisTitle, user.Id, ClusterAnalysisTitleManager.SaveFromUpload );
			SaveItems( summary, summary.ItemsToBeCreated.Course, summary.FinalizedChanges.Course, user.Id, CourseManager.SaveFromUpload );
			SaveItems( summary, summary.ItemsToBeCreated.TrainingTask, summary.FinalizedChanges.TrainingTask, user.Id, TrainingTaskManager.SaveFromUpload );
			SaveItems( summary, summary.ItemsToBeCreated.RatingTask, summary.FinalizedChanges.RatingTask, user.Id, RatingTaskManager.SaveFromUpload );
			SaveItems( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, user.Id, ClusterAnalysisManager.SaveFromUpload );
			SaveItems( summary, summary.ItemsToBeCreated.CourseContext, summary.FinalizedChanges.CourseContext, user.Id, CourseContextManager.SaveFromUpload );
			SaveItems( summary, summary.ItemsToBeCreated.RatingContext, summary.FinalizedChanges.RatingContext, user.Id, RatingContextManager.SaveFromUpload );

			//Track activity
			saveDuration = DateTime.Now.Subtract( saveStart );
			summary.Messages.Note.Add( string.Format( "Save Duration: {0:N2} seconds ", saveDuration.TotalSeconds ) );
			var eventType = summary.ContinueSavingData ? "Completed" : "Canceled";
			new ActivityServices().AddActivity( new SiteActivity()
			{
				ActivityType = "RMTL",
				Activity = "Upload",
				Event = "Processing " + eventType,
				Comment = String.Format( "A bulk upload save was " + eventType.ToLower() + " by: '{0}' for Rating: '{1}'. Duration: {2:N2} seconds .", user.FullName(), summary.RatingCodedNotation ?? "", saveDuration.TotalSeconds ),
				ActionByUserId = user.Id,
				ActionByUser = user.FullName()
			} );

		}
		//

		public static void SaveItems<T>( ChangeSummary summary, List<T> newItems, List<T> existingItems, int userID, Action<T, int, ChangeSummary> SaveMethod )
		{
			//Cancel if the user hit the cancel button
			if ( !summary.ContinueSavingData )
			{
				var messageText = "Saving data canceled by request.";
				if ( summary.Messages.Note.FirstOrDefault( m => m == messageText ) == null )
				{
					summary.Messages.Note.Add( messageText );
				}

				return;
			}

			foreach( var item in newItems.Concat( existingItems ).ToList() )
			{
				//Run the save method
				SaveMethod( item, userID, summary );

				//Increment the summary's processing tracking
				summary.TotalItemsSavedForClientMonitoring++;
			}
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
				var rating = RatingManager.GetByRowId( item.RatingRowID );
				if( rating == null || rating.Id == 0 )
				{
					result.Errors.Add( "The selected rating was not found." );
					return result;
				}

				//Create a new summary object
				summary = new ChangeSummary() { RowId = item.TransactionGUID, RatingRowId = rating.RowId, RatingCodedNotation = rating.CodedNotation };
				summary.UploadStarted = DateTime.Now;
				summary.ContinueSavingData = true; //Set this true here and here only, so that it can be used later on to prevent double-saving the same summary

				//Pre-populate controlled vocabularies, since these will be used across all rows
				summary.ConceptSchemeMap = ConceptSchemeManager.GetConceptSchemeMap( true );
				summary.AllRatings = RatingManager.GetAll();
				summary.AllOrganizations = OrganizationManager.GetAll();

				//Sort these concepts for later
				summary.CandidatePlatformRegexHelpers = summary.ConceptSchemeMap.CandidatePlatformCategory.Concepts
					.SelectMany( m => new List<ConceptRegexHelper>() { new ConceptRegexHelper() { Value = m.Name, Concept = m }, new ConceptRegexHelper() { Value = m.CodedNotation, Concept = m } } )
					.Where( m => !string.IsNullOrWhiteSpace( m.Value ) )
					.OrderByDescending( m => m.Value.Contains( "/" ) ).ThenByDescending( m => m.Value.Length ).ToList();
				summary.CandidatePlatformRegexHelpers.ForEach( m => m.RegexEscapedValue = Regex.Escape( m.Value ) );
				//summary.ConceptSchemeMap.CandidatePlatformCategory.Concepts = summary.ConceptSchemeMap.CandidatePlatformCategory.Concepts.OrderByDescending( m => m.CodedNotation.Contains( "/" ) ).ThenByDescending( m => m.CodedNotation.Length ).ToList();

				//When the client does a lookup later on (especially for concepts), it will need to be able to find the above items in the summary's lookup graph
				//The server-side lookup also injects the @type value into the object, which is necessary for some of the client-side stuff
				summary.LookupGraph.AddRange( summary.AllRatings );
				summary.LookupGraph.AddRange( summary.AllOrganizations );
				typeof( ConceptSchemeMap ).GetProperties().Where( m => m.PropertyType == typeof( ConceptScheme ) ).ToList().ForEach( property => {
					summary.LookupGraph.AddRange( ( ( ConceptScheme ) property.GetValue( summary.ConceptSchemeMap ) ).Concepts );
				} );

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

			//Track start time
			DateTime sectionStarted = DateTime.Now;
			var sectionDuration = new TimeSpan();

			//Do something with the item and summary.  
			//The summary will be stored in the cache and retrieved again so that it can be used for all of the rows. 
			//At the end of the process, the summary will contain a list of updated entities (all changes applied) that should be ready to save to the database

			//Processing
			//Validation - Checks that should skip processing the row's data if there is an error

			#region Part I - components check
			//Get the controlled value items that show up in this row
			//Everything in any uploaded sheet should appear here. If any of these are not found, it's an error

			//Rating for this Row (for comparison)
			var rowRating = GetDataOrError( summary.AllRatings, ( m ) => 
				m.CodedNotation?.ToLower() == item.Row.Rating_CodedNotation?.ToLower(), 
				result, 
				"Rating indicated by this row is missing, empty, or N/A.",
				"Rating indicated by this row was not found in database: \"" + TextOrNA( item.Row.Rating_CodedNotation ) + "\"",
				item.Row.Rating_CodedNotation
			);

			//Selected Rating (for comparison)
			var selectedRating = GetDataOrError( summary.AllRatings, ( m ) => 
				m.RowId == item.RatingRowID, 
				result, 
				"Selected Rating is missing, empty, or N/A.",
				"Selected Rating not found in database: \"" + item.RatingRowID + "\"",
				item.RatingRowID.ToString()
			);

			//Pay Grade
			var rowPayGrade = GetDataOrError( summary.ConceptSchemeMap.PayGradeCategory.Concepts, ( m ) => 
				m.CodedNotation?.ToLower() == item.Row.PayGradeType_CodedNotation?.ToLower(), 
				result, 
				"Rank is missing, empty, or N/A.",
				"Rank not found in database: \"" + TextOrNA( item.Row.PayGradeType_CodedNotation ) + "\"",
				item.Row.PayGradeType_CodedNotation
			);

			//Pay Grade Level (A/J/M)
			var rowPayGradeLevel = GetDataOrError( summary.ConceptSchemeMap.PayGradeLevelCategory.Concepts, ( m ) =>
				m.CodedNotation?.ToLower() == item.Row.Level_Name?.ToLower(),
				result,
				"Level (A/J/M) is missing, empty, or N/A.",
				"Level (A/J/M) not found in database: \"" + TextOrNA( item.Row.Level_Name ) + "\"",
				item.Row.Level_Name
			);

			//Pay Grade and Pay Grade Level association
			if( rowPayGrade?.BroadMatch != rowPayGradeLevel?.RowId )
			{
				result.Errors.Add( "Rank (" + TextOrNA( rowPayGrade?.CodedNotation ) + ") does not correspond to Level (" + TextOrNA( rowPayGradeLevel?.CodedNotation ) + ")." );
				rowPayGradeLevel = summary.ConceptSchemeMap.PayGradeLevelCategory.Concepts.FirstOrDefault( m => m.RowId == ( rowPayGrade?.BroadMatch ?? Guid.Empty ) );
			}

			//Reference Resource Type (via Work Element Type)
			var rowSourceType = GetDataOrError( summary.ConceptSchemeMap.ReferenceResourceCategory.Concepts, ( m ) => 
				m.WorkElementType?.ToLower() == item.Row.Shared_ReferenceType?.ToLower() ||
				m.CodedNotation?.ToLower() == item.Row.Shared_ReferenceType?.ToLower(), 
				result,
				"Work Element Type is missing, empty, or N/A.",
				"Work Element Type not found in database: \"" + TextOrNA( item.Row.Shared_ReferenceType ) + "\"",
				item.Row.Shared_ReferenceType
			);

			//Task Applicability Type
			var rowTaskApplicabilityType = GetDataOrError( summary.ConceptSchemeMap.TaskApplicabilityCategory.Concepts, ( m ) => 
				m.Name?.ToLower() == item.Row.RatingTask_ApplicabilityType_Name?.ToLower(), 
				result,
				"Task Applicability Type is missing, empty, or N/A.",
				"Task Applicability Type not found in database: \"" + TextOrNA( item.Row.RatingTask_ApplicabilityType_Name ) + "\"",
				item.Row.RatingTask_ApplicabilityType_Name
			);

			//Training Gap Type
			var rowTrainingGapType = GetDataOrError( summary.ConceptSchemeMap.TrainingGapCategory.Concepts, ( m ) => 
				m.Name?.ToLower() == item.Row.RatingTask_TrainingGapType_Name?.ToLower(), 
				result,
				"Training Gap Type is missing, empty, or N/A.",
				"Training Gap Type not found in database: \"" + TextOrNA( item.Row.RatingTask_TrainingGapType_Name ) + "\"",
				item.Row.RatingTask_TrainingGapType_Name
			);

			//If any of the above are null, log an error and skip the rest
			if ( new List<object>() { rowRating, rowPayGrade, rowTaskApplicabilityType, rowTrainingGapType, selectedRating, rowSourceType }.Where( m => m == null ).Count() > 0 )
			{
				result.Errors.Add( "One or more controlled vocabulary values was not found in the database, or was missing, empty, or N/A. Processing this row cannot continue." );
				return result;
			}

			//Always use the Rating selected in the interface
			if( rowRating.RowId != selectedRating.RowId )
			{
				result.Errors.Add( "The Rating for this row (" + rowRating.CodedNotation + " - " + rowRating.Name + ") does not match the Rating selected for this upload (" + selectedRating.CodedNotation + " - " + selectedRating.Name + "). Processing this row cannot continue." );
				//rowRating = selectedRating;
				return result;
			}

			//Get the variable items that show up in this row
			//Billet Title
			var rowBilletTitle = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<BilletTitle>().FirstOrDefault( m =>
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
				( newItem ) => { summary.ItemsToBeCreated.BilletTitle.Add( newItem ); },
				//Handle empty/null
				() => string.IsNullOrWhiteSpace( item.Row.BilletTitle_Name ) || item.Row.BilletTitle_Name?.ToLower() == "n/a",
				"Billet Title/Job is missing, empty, or N/A."
			);

			//Work Role
			var rowWorkRole = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<WorkRole>().FirstOrDefault( m =>
					m.Name?.ToLower() == item.Row.WorkRole_Name?.ToLower()
				),
				//Or get from DB
				() => WorkRoleManager.GetByName( item.Row.WorkRole_Name ),
				//Or create new
				() => new WorkRole()
				{
					RowId = Guid.NewGuid(),
					Name = item.Row.WorkRole_Name
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.WorkRole.Add( newItem ); },
				//Handle empty/null
				() => string.IsNullOrWhiteSpace( item.Row.WorkRole_Name ) || item.Row.WorkRole_Name?.ToLower() == "n/a",
				"Functional Area is missing, empty, or N/A."
			);

			//Reference Resource
			var rowRatingTaskSource = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<ReferenceResource>().FirstOrDefault( m =>
					m.Name?.ToLower() == item.Row.ReferenceResource_Name?.ToLower() &&
					m.PublicationDate?.ToLower() == item.Row.ReferenceResource_PublicationDate?.ToLower()
				),
				//Or get from DB
				() => ReferenceResourceManager.GetForUploadOrNull( item.Row.ReferenceResource_Name, item.Row.ReferenceResource_PublicationDate ),
				//Or create new
				() => new ReferenceResource()
				{
					RowId = Guid.NewGuid(),
					Name = item.Row.ReferenceResource_Name,
					PublicationDate = item.Row.ReferenceResource_PublicationDate
					//Other properties are handled in the next section
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.ReferenceResource.Add( newItem ); },
				//Handle empty/null
				() => string.IsNullOrWhiteSpace( item.Row.ReferenceResource_Name ) || item.Row.ReferenceResource_Name?.ToLower() == "n/a" || string.IsNullOrWhiteSpace( item.Row.ReferenceResource_PublicationDate ) || item.Row.ReferenceResource_PublicationDate?.ToLower() == "n/a",
				"Task Source or Date of Source is missing, empty, or N/A."
			);

			//Rating Task
			var rowRatingTask = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary - if found would be a duplicate
				() => summary.GetAll<RatingTask>().FirstOrDefault( m =>
					m.Description.ToLower() == item.Row.RatingTask_Description.ToLower() &&
					m.HasReferenceResource == rowRatingTaskSource.RowId &&
					m.ReferenceType == rowSourceType.RowId
				),
				//Or get from DB
				() => RatingTaskManager.GetForUploadOrNull( item.Row.RatingTask_Description, rowRatingTaskSource.RowId, rowSourceType.RowId ),
				//Or create new
				() => new RatingTask()
				{
					RowId = Guid.NewGuid(),
					Description = item.Row.RatingTask_Description,
					HasReferenceResource = rowRatingTaskSource.RowId,
					ReferenceType = rowSourceType.RowId
					//Other properties are handled in the next section
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.RatingTask.Add( newItem ); },
				//Handle empty/null
				() => string.IsNullOrWhiteSpace( item.Row.RatingTask_Description ) || item.Row.RatingTask_Description?.ToLower() == "n/a" || rowRatingTaskSource == null || rowSourceType == null,
				"Rating Task is missing, empty, or N/A."
			);

			//If any Part I data is missing or there are any other errors, return an error
			if( result.Errors.Count() > 0 )
			{
				result.Errors.Add( "One or more required pieces of Part I data is missing, empty, or N/A. Processing this row cannot continue." );
				return result;
			}

			#endregion

			#region Part II - components check
			//Course Type
			var missingCourseTypeList = new List<string>();
			var rowCourseTypeList = GetDataListOrErrorForEach(
				summary.ConceptSchemeMap.CourseCategory.Concepts,
				SplitAndTrim( item.Row.Course_CourseType_Name, new List<string>() { ",", ";", "|" } ),
				( concept, value ) => value?.ToLower() == concept.Name?.ToLower(),
				result,
				item.Row.Course_CourseType_Name,
				"Course Type is missing, empty, or N/A.",
				( value ) => "Course Type not found in database: \"" + TextOrNA( value ) + "\"",
				missingCourseTypeList
			);

			//Curriculum Control Authority (Organization)
			var rowOrganizationCCA = GetDataOrError( summary.AllOrganizations, ( m ) =>
				( !string.IsNullOrWhiteSpace( m.Name ) && m.Name.ToLower() == item.Row.Course_CurriculumControlAuthority_Name?.ToLower() ) || //Look for name
				( !string.IsNullOrWhiteSpace( m.AlternateName ) && m.AlternateName.ToLower() == item.Row.Course_CurriculumControlAuthority_Name?.ToLower() ) || //Look for abbreviation
				( !string.IsNullOrWhiteSpace( m.Name ) && !string.IsNullOrWhiteSpace( m.AlternateName ) && ( ( m.Name + " (" + m.AlternateName + ")" ).ToLower() == item.Row.Course_CurriculumControlAuthority_Name?.ToLower()) ), //Look for concatenated form: "Name (Abbreviation)"
				result,
				"Curriculum Control Authority is missing, empty, or N/A.",
				"Curriculum Control Authority not found in database: \"" + TextOrNA( item.Row.Course_CurriculumControlAuthority_Name ) + "\"",
				item.Row.Course_CurriculumControlAuthority_Name
			);

			//Life Cycle Control Document Type 
			var rowCourseLCCDType = GetDataOrError( summary.ConceptSchemeMap.LifeCycleControlDocumentCategory.Concepts, ( m ) =>
				(
					( !string.IsNullOrWhiteSpace( m.Name ) && m.Name?.ToLower() == item.Row.Course_LifeCycleControlDocumentType_CodedNotation?.ToLower() ) ||
					( !string.IsNullOrWhiteSpace( m.CodedNotation ) && m.CodedNotation?.ToLower() == item.Row.Course_LifeCycleControlDocumentType_CodedNotation?.ToLower() )
				),
				result,
				"Life-Cycle Control Document Type is missing, empty, or N/A.",
				"Life-Cycle Control Document Type not found in database: \"" + TextOrNA( item.Row.Course_LifeCycleControlDocumentType_CodedNotation ) + "\"",
				item.Row.Course_LifeCycleControlDocumentType_CodedNotation
			);

			//Assessment Method Type
			var missingAssessmentMethodTypeList = new List<string>();
			var rowAssessmentMethodTypeList = GetDataListOrErrorForEach(
				summary.ConceptSchemeMap.AssessmentMethodCategory.Concepts,
				SplitAndTrim( item.Row.Course_AssessmentMethodType_Name, new List<string>() { ",", ";", "|" } ),
				( concept, value ) => value?.ToLower() == concept.Name?.ToLower(),
				result,
				item.Row.Course_AssessmentMethodType_Name,
				"Assessment Method Type is missing, empty, or N/A.",
				( value ) => "Assessment Method Type not found in database: \"" + TextOrNA( value ) + "\"",
				missingAssessmentMethodTypeList
			);

			//Course
			var rowCourse = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<Course>().FirstOrDefault( m =>
					m.Name.ToLower() == item.Row.Course_Name?.ToLower() &&
					m.CodedNotation?.ToLower() == item.Row.Course_CodedNotation?.ToLower() &&
					m.CurriculumControlAuthority == rowOrganizationCCA.RowId &&
					m.LifeCycleControlDocumentType == rowCourseLCCDType.RowId
				),
				//Or get from DB
				() => CourseManager.GetForUploadOrNull( item.Row.Course_Name, item.Row.Course_CodedNotation, rowOrganizationCCA.RowId, rowCourseLCCDType.RowId ),
				//Or create new
				() => new Course()
				{
					RowId = Guid.NewGuid(),
					Name = item.Row.Course_Name,
					CodedNotation = item.Row.Course_CodedNotation,
					CurriculumControlAuthority = rowOrganizationCCA.RowId,
					LifeCycleControlDocumentType = rowCourseLCCDType.RowId
					//Other properties are handled in the next section
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.Course.Add( newItem ); },
				//Handle empty/null
				() => string.IsNullOrWhiteSpace( item.Row.Course_Name ) || item.Row.Course_Name?.ToLower() == "n/a" || string.IsNullOrWhiteSpace( item.Row.Course_CodedNotation ) || item.Row.Course_CodedNotation?.ToLower() == "n/a" || rowOrganizationCCA == null || rowCourseLCCDType == null,
				"Course Name or CIN is missing, empty, or N/A."
			);

			//Training Task
			var rowTrainingTask = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<TrainingTask>().FirstOrDefault( m =>
					m.Description?.ToLower() == item.Row.TrainingTask_Description?.ToLower() &&
					m.HasReferenceResource == rowRatingTaskSource.RowId
				),
				//Or get from DB
				() => TrainingTaskManager.GetForUploadOrNull( item.Row.TrainingTask_Description, rowRatingTaskSource.RowId ),
				//Or create new
				() => new TrainingTask()
				{
					RowId = Guid.NewGuid(),
					Description = item.Row.TrainingTask_Description,
					HasReferenceResource = rowRatingTaskSource.RowId
					//Other properties are handled in the next section
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.TrainingTask.Add( newItem ); },
				//Handle empty/null
				() => string.IsNullOrWhiteSpace( item.Row.TrainingTask_Description ) || item.Row.TrainingTask_Description?.ToLower() == "n/a" || rowRatingTaskSource == null,
				"Training Task is missing, empty, or N/A."
			);

			//Course Context
			var rowCourseContext = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<CourseContext>().FirstOrDefault( m =>
					m.HasCourse == rowCourse.RowId &&
					m.HasTrainingTask == rowTrainingTask.RowId
				),
				//Or get from DB
				() => CourseContextManager.GetForUploadOrNull( rowCourse.RowId, rowTrainingTask.RowId ),
				//Or create new
				() => new CourseContext()
				{
					RowId = Guid.NewGuid(),
					HasCourse = rowCourse.RowId,
					HasTrainingTask = rowTrainingTask.RowId
					//Other properties handled in the next section
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.CourseContext.Add( newItem ); },
				//Skip all of this and set value to null if the following test is true
				() => rowCourse == null || 
				rowTrainingTask == null || 
				rowAssessmentMethodTypeList == null || rowAssessmentMethodTypeList.Count() == 0
			);

			//Validate Part II data
			var hasPart2Data = false;
			var ClearPart2DataAndErrors = new Action( () => {
				hasPart2Data = false;
				rowCourseTypeList = null;
				rowOrganizationCCA = null;
				rowCourseLCCDType = null;
				rowAssessmentMethodTypeList = null;
				rowCourse = null;
				rowTrainingTask = null;
				rowCourseContext = null;
				result.Errors = new List<string>();
				missingCourseTypeList = new List<string>();
				missingAssessmentMethodTypeList = new List<string>();
			} );

			//Handle Part II checks being skipped
			if ( item.SkipPart2Checks )
			{
				//Just clear the data and errors
				ClearPart2DataAndErrors();
			}
			//Otherwise, handle Part II data being present when it shouldn't be
			else if ( rowTrainingGapType.Name?.ToLower() == "yes" )
			{
				if(
					(rowCourseTypeList != null && rowCourseTypeList.Count() > 0) ||
					rowOrganizationCCA != null ||
					rowCourseLCCDType != null ||
					(rowAssessmentMethodTypeList != null && rowAssessmentMethodTypeList.Count() > 0) ||
					rowCourse != null ||
					rowTrainingTask != null ||
					rowCourseContext != null ||
					missingCourseTypeList.Count() > 0 ||
					missingAssessmentMethodTypeList.Count() > 0
				)
				{
					result.Warnings.Add( "One or more pieces of Part 2 data on this row have values despite the Formal Training Gap being \"Yes\". Because the Training Gap Type is \"Yes\", all Part 2 data for this row will be treated as \"N/A\"." );
				}

				//Clear the data and errors
				ClearPart2DataAndErrors();
			}
			//Otherwise, handle Part II data being incomplete (if Training Gap Type is any value other than "Yes")
			else if(
				rowCourseTypeList == null || rowCourseTypeList.Count() == 0 ||
				rowOrganizationCCA == null ||
				rowCourseLCCDType == null ||
				rowAssessmentMethodTypeList == null || rowAssessmentMethodTypeList.Count() == 0 ||
				rowCourse == null ||
				rowTrainingTask == null ||
				rowCourseContext == null ||
				missingCourseTypeList.Count() > 0 ||
				missingAssessmentMethodTypeList.Count() > 0
			)
			{
				result.Errors.Add( "One or more required pieces of Part 2 data is not found in the database, or is missing, empty, or N/A. Processing this row cannot continue." );
				return result;
			}
			//If it survived all of the checks, then flag hasPart2Data as true
			else
			{
				hasPart2Data = true;
			}
			


			/*
			//If the Training Gap Type is "Yes", then treat all course/training data as null, but check to see if it exists first (above) to facilitate the warning statement below

			//Handle validation for incomplete Part 3 data
			//Helper function
			var ClearPartTwoDataAndErrors = new Action( () => {
				hasCourseAndTrainingData = false;
				rowOrganizationCCA = null;
				rowCourseLCCDType = null;
				rowCourseTypeList = new List<Concept>();
				rowAssessmentMethodTypeList = new List<Concept>();
				item.Row.TrainingTask_Description = "";
				item.Row.Course_CodedNotation = "";
				item.Row.Course_Name = "";
				result.Errors = new List<string>();
			} );

			if ( item.SkipPart2Checks )
			{
				//Clear data and remove errors
				ClearPartTwoDataAndErrors();
			}
			else if ( shouldNotHaveTrainingData )
			{
				var hasDataWhenItShouldNot = new List<object>() { rowOrganizationCCA, rowCourseLCCDType }.Concat( rowAssessmentMethodTypeList ).Concat( rowCourseTypeList ).Where( m => m != null ).ToList();
				if ( hasDataWhenItShouldNot.Count() > 0 || !string.IsNullOrWhiteSpace( item.Row.TrainingTask_Description ) )
				{
					result.Warnings.Add( String.Format( "Incomplete course/training data found. All course/training related columns should either have data or be marked as \"N/A\". Since the Training Gap Type is \"Yes\", the incomplete data will be treated as \"N/A\"." ) );
				}

				//Clear data and remove errors
				ClearPartTwoDataAndErrors();
			}
			//Otherwise, return an error if any course/training data is missing
			else if ( 
				new List<object>() { rowOrganizationCCA, rowCourseLCCDType }.Where( m => m == null ).Count() > 0 || 
				new List<string>() { item.Row.TrainingTask_Description, item.Row.Course_CodedNotation, item.Row.Course_Name }.Where( m => string.IsNullOrWhiteSpace( m ) ).Count() > 0 || 
				rowAssessmentMethodTypeList.Count() == 0 ||
				rowCourseTypeList.Count() == 0 ||
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
			*/
			#endregion

			#region Part III  - components check
			//Training Solution Type
			var rowTrainingSolutionType = GetDataOrError( summary.ConceptSchemeMap.TrainingSolutionCategory.Concepts, ( m ) =>
				m.Name?.ToLower() == item.Row.Training_Solution_Type?.ToLower() || 
				m.CodedNotation?.ToLower() == item.Row.Training_Solution_Type?.ToLower(), 
				result,
				"Training Solution Type is missing, empty, or N/A.",
				"Training Solution Type not found in database: \"" + TextOrNA( item.Row.Training_Solution_Type ) + "\"", 
				item.Row.Training_Solution_Type
			);

			//Recommended Modality Type
			var rowRecommendModalityType = GetDataOrError( summary.ConceptSchemeMap.RecommendedModalityCategory.Concepts, ( m ) =>
				m.Name?.ToLower() == item.Row.Recommended_Modality?.ToLower() || 
				m.CodedNotation?.ToLower() == item.Row.Recommended_Modality?.ToLower(), 
				result,
				"Recommended Modality Type is missing, empty, or N/A.",
				"Recommended Modality Type not found in database: \"" + TextOrNA( item.Row.Recommended_Modality ) + "\"", 
				item.Row.Recommended_Modality
			);

			//Development Specification Type
			var rowDevelopmentSpecificationType = GetDataOrError( summary.ConceptSchemeMap.DevelopmentSpecificationCategory.Concepts, ( m ) => 
				m.Name?.ToLower() == item.Row.Development_Specification?.ToLower(), 
				result,
				"Development Specification Type is missing, empty, or N/A.",
				"Development Specification Type not found in database: \"" + TextOrNA( item.Row.Development_Specification ) + "\"", 
				item.Row.Development_Specification
			);

			//Development Ratio Type
			var rowDevelopmentRatioType = GetDataOrError( summary.ConceptSchemeMap.DevelopmentRatioCategory.Concepts, ( m ) =>
				m.Name?.ToLower() == item.Row.Development_Ratio?.ToLower(),
				result,
				"Development Ratio Type is missing, empty, or N/A.",
				"Development Ratio Type not found in database: \"" + TextOrNA( item.Row.Development_Ratio ) + "\"",
				item.Row.Development_Ratio
			);

			//CFM Placement Type
			var notFoundCFMPlacementTypes = new List<string>();
			var rowCFMPlacementTypeList = GetDataListOrErrorForEach( 
				summary.ConceptSchemeMap.CFMPlacementCategory.Concepts,
				SplitAndTrim( item.Row.CFM_Placement, new List<string>() { "/", ",", "|" } ),
				( concept, value ) => concept.Name?.ToLower() == value.ToLower() ||
				concept.CodedNotation?.ToLower() == value.ToLower(),
				result,
				item.Row.CFM_Placement,
				"CFM Placement Type is missing, empty, or N/A.",
				(value) => "CFM Placement Type not found in database: \"" + TextOrNA( value) + "\"",
				notFoundCFMPlacementTypes
			);

			//Candidate Platform Type
			/*
			var rowCandidatePlatformTypeList = GetDataListOrErrorForEach(
				summary.ConceptSchemeMap.CandidatePlatformCategory.Concepts,
				SplitAndTrim( item.Row.Candidate_Platform?.ToLower(), new List<string>() { "|" } ),
				( concept, value ) => value?.ToLower() == concept.CodedNotation?.ToLower(),
				result,
				( value ) => "Candidate Platform Type not found in database: " + TextOrNA( value )
			);
			*/

			//Custom handling for Candidate Platform Type because entries are separated by slashes, but may also contain slashes
			//First get the entries that are present in the list
			var rowCandidatePlatformTypeList = new List<Concept>();
			var notFoundCandidatePlatformTypes = new List<string>();
			var checkString = (item.Row.Candidate_Platform ?? "").ToLower();
			foreach ( var platform in summary.CandidatePlatformRegexHelpers ) //These concepts are already sorted to include first those with a / in their code, then by those with the longest code
			{
				//Skip the rest of the concepts if there's nothing left that could match anything
				if( checkString.Replace( "/", "" ).Length == 0 )
				{
					break;
				}

				//Try to find a match
				if ( Regex.Match( checkString, @" *(?:^|/) *(" + platform.RegexEscapedValue + @") *(?:/|$) *", RegexOptions.IgnoreCase ).Success )
				{
					rowCandidatePlatformTypeList.Add( platform.Concept );
					checkString = checkString.Replace( platform.Value.ToLower(), "" ).Trim(); //Remove the matched term
				}
			}
			//Replace all double (or more) slashes with a single slash
			checkString = Regex.Replace( checkString, @"(/{2,})", "/" ).Trim();
			//Remove leading/trailing slashes
			checkString = Regex.Replace( checkString, @"(?:^/*)|(?:/*$)", "" ).Trim();
			//If there is anything left in the string, then it contains one or more unknown entries that aren't in the database
			if ( checkString.Length > 0 )
			{
				result.Errors.Add( "Candidate Platform Type contains one or more entries that were not found in the database: \"" + checkString + "\"." );
				notFoundCandidatePlatformTypes.Add( checkString );
			}
			//Check for empty list
			if( rowCandidatePlatformTypeList.Count() == 0 )
			{
				result.Errors.Add( "Candidate Platform Type is missing, empty, or N/A." );
			}

			//Numeric fields
			var priorityPlacement = ParseNumberOrError( result, item.Row.Priority_Placement, UtilityManager.MapIntegerOrDefault, 0, ( value ) => value < 0, "Priority Placement must be an integer greater than or equal to 0." );
			var developmentTime = ParseNumberOrError( result, item.Row.Development_Time, UtilityManager.MapDecimalOrDefault, 0.0m, ( value ) => value < 0, "Development Time must be greater than or equal 0." );
			var estimatedInstructionalTime = ParseNumberOrError( result, item.Row.Estimated_Instructional_Time, UtilityManager.MapDecimalOrDefault, 0.0m, ( value ) => value < 0, "Estimated Instructional Time must be greater than or equal 0." );

			//Cluster Analysis Title
			var rowClusterAnalysisTitle = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<ClusterAnalysisTitle>().FirstOrDefault( m =>
					 m.Name == item.Row.Cluster_Analysis_Title
				),
				//Or get from DB
				() => ClusterAnalysisTitleManager.GetForUploadOrNull( item.Row.Cluster_Analysis_Title ),
				//Or create new
				() => new ClusterAnalysisTitle()
				{
					RowId = Guid.NewGuid(),
					Name = item.Row.Cluster_Analysis_Title
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.ClusterAnalysisTitle.Add( newItem ); },
				//Handle empty/null
				() => string.IsNullOrWhiteSpace(item.Row.Cluster_Analysis_Title) || item.Row.Cluster_Analysis_Title?.ToLower() == "n/a",
				"Cluster Analysis Title is missing, empty, or N/A."
			);

			//Cluster Analysis
			var rowClusterAnalysis = LookupOrGetFromDBOrCreateNew( summary, result,
				//Find in summary
				() => summary.GetAll<ClusterAnalysis>().FirstOrDefault( m =>
					 m.HasRatingTask == rowRatingTask.RowId &&
					 m.HasRating == rowRating.RowId &&
					 m.HasBilletTitle == rowBilletTitle.RowId &&
					 m.HasWorkRole == rowWorkRole.RowId &&
					 m.HasClusterAnalysisTitle == rowClusterAnalysisTitle.RowId
				),
				//Or get from DB
				() => ClusterAnalysisManager.GetForUploadOrNull( rowRating.RowId, rowRatingTask.RowId, rowBilletTitle.RowId, rowWorkRole.RowId, rowClusterAnalysisTitle.RowId ),
				//Or create new
				() => new ClusterAnalysis()
				{
					RowId = Guid.NewGuid(),
					HasRating = rowRating.RowId,
					HasRatingTask = rowRatingTask.RowId,
					HasBilletTitle = rowBilletTitle.RowId,
					HasWorkRole = rowWorkRole.RowId
					//Other properties are handled in the next section
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.ClusterAnalysis.Add( newItem ); },
				//Skip all of this and set value to null if the following test is true
				() => rowClusterAnalysisTitle == null ||
				rowTrainingSolutionType == null ||
				rowRecommendModalityType == null ||
				rowDevelopmentSpecificationType == null ||
				rowDevelopmentRatioType == null ||
				rowCFMPlacementTypeList == null || rowCFMPlacementTypeList.Count() == 0 ||
				notFoundCFMPlacementTypes.Count() > 0 ||
				rowCandidatePlatformTypeList == null || rowCandidatePlatformTypeList.Count() == 0 ||
				notFoundCandidatePlatformTypes.Count() > 0 ||
				//priorityPlacement == 0 || //Can be 0
				//developmentTime == 0 || //Can be 0
				//estimatedInstructionalTime == 0 || //Can be 0
				rowRating == null ||
				rowRatingTask == null ||
				rowBilletTitle == null ||
				rowWorkRole == null
			);

			//Validate Part III data
			var hasPart3Data = false;
			var ClearPart3DataAndErrors = new Action( () =>
			{
				hasPart3Data = false;
				rowClusterAnalysis = null;
				rowClusterAnalysisTitle = null;
				rowTrainingSolutionType = null;
				rowRecommendModalityType = null;
				rowDevelopmentSpecificationType = null;
				rowDevelopmentRatioType = null;
				rowCFMPlacementTypeList = null;
				rowCandidatePlatformTypeList = null;
				priorityPlacement = 0;
				developmentTime = 0.0m;
				estimatedInstructionalTime = 0.0m;
				result.Errors = new List<string>();
				notFoundCFMPlacementTypes = new List<string>();
				notFoundCandidatePlatformTypes = new List<string>();
			} );

			//Handle Part III checks being skipped
			if ( item.SkipPart3Checks )
			{
				//Just clear the data and errors
				ClearPart3DataAndErrors();
			}
			//Otherwise, if part III data is complete, flag hasPart3Data as true
			else if (
				rowClusterAnalysis != null &&
				rowClusterAnalysisTitle != null &&
				rowTrainingSolutionType != null &&
				rowRecommendModalityType != null &&
				rowDevelopmentSpecificationType != null &&
				rowDevelopmentRatioType != null &&
				rowCFMPlacementTypeList != null && rowCFMPlacementTypeList.Count() > 0 &&
				notFoundCFMPlacementTypes.Count() == 0 &&
				rowCandidatePlatformTypeList != null && rowCandidatePlatformTypeList.Count() > 0 &&
				notFoundCandidatePlatformTypes.Count() == 0 //&&
				//priorityPlacement > 0 &&
				//developmentTime > 0 &&
				//estimatedInstructionalTime > 0
			)
			{
				hasPart3Data = true;
			}
			//Otherwise, if part III data is completely N/A, clear the errors and flag hasPart3Data as false, but continue
			else if(
				rowClusterAnalysis == null &&
				rowClusterAnalysisTitle == null &&
				rowTrainingSolutionType == null &&
				rowRecommendModalityType == null &&
				rowDevelopmentSpecificationType == null &&
				rowDevelopmentRatioType == null &&
				( rowCFMPlacementTypeList == null || rowCFMPlacementTypeList.Count() == 0) &&
				notFoundCFMPlacementTypes.Count() == 0 &&
				( rowCandidatePlatformTypeList == null || rowCandidatePlatformTypeList.Count() == 0) &&
				notFoundCandidatePlatformTypes.Count() == 0 //&&
				//priorityPlacement == 0 &&
				//developmentTime == 0 &&
				//estimatedInstructionalTime == 0
			)
			{
				ClearPart3DataAndErrors();
			}
			//Otherwise, throw an error because some of the data is present, but not all of it
			else
			{
				result.Errors.Add( "One or more required pieces of Part 3 data is missing, empty, or N/A. Processing this row cannot continue." );
				return result;
			}

			#endregion

			#region Additional validation checks

			//If there are any errors, return before the rest of the row's data is processed
			if( result.Errors.Count() > 0 )
			{
				result.Errors.Add( "One or more critical errors were encountered while processing this row." );
				return result;
			}

			#endregion

			#region Build the Rating Context

			//Rating Context
			var rowRatingContext = LookupOrGetFromDBOrCreateNew( summary, result,
				//Every row is a Rating Context, so this one won't already be in the summary
				() => { return null; },
				//Or get from DB
				() => RatingContextManager.GetForUploadOrNull(
					rowRating.RowId,
					rowBilletTitle.RowId,
					rowWorkRole.RowId,
					rowRatingTask.RowId,
					rowTaskApplicabilityType.RowId,
					rowTrainingGapType.RowId,
					rowPayGrade.RowId,
					rowPayGradeLevel.RowId,
					( rowCourseContext ?? new CourseContext() ).RowId, //Can be null
					( rowClusterAnalysis ?? new ClusterAnalysis() ).RowId //Can be null
				),
				//Or create new
				() => new RatingContext()
				{
					RowId = Guid.NewGuid(),
					HasRating = rowRating.RowId,
					HasBilletTitle = rowBilletTitle.RowId,
					HasWorkRole = rowWorkRole.RowId,
					HasRatingTask = rowRatingTask.RowId,
					ApplicabilityType = rowTaskApplicabilityType.RowId,
					TrainingGapType = rowTrainingGapType.RowId,
					PayGradeType = rowPayGrade.RowId,
					PayGradeLevelType = rowPayGradeLevel.RowId,
					HasCourseContext = ( rowCourseContext ?? new CourseContext() ).RowId,
					HasClusterAnalysis = ( rowClusterAnalysis ?? new ClusterAnalysis() ).RowId,
					Note = item.Row.Note
					//Other properties are handled in the next section
				},
				//Store if newly created
				( newItem ) => { summary.ItemsToBeCreated.RatingContext.Add( newItem ); },
				//Handle empty/null
				() => rowRating == null || 
				rowBilletTitle == null || 
				rowWorkRole == null || 
				rowRatingTask == null || 
				rowTaskApplicabilityType == null || 
				rowTrainingGapType == null || 
				rowPayGrade == null || 
				rowPayGradeLevel == null,
				"One or more required pieces of data is missing, empty, or N/A."
			);

			//Validate Rating Context
			if ( rowRatingContext == null )
			{
				result.Errors.Add( "One or more required pieces of data is missing, empty, or N/A. Processing this row cannot continue." );
				return result;
			}

			//Check to see if the Rating Task is about to be associated with more than one Pay Grade from this upload
			var otherSummaryPayGradesForRatingTask = summary.GetAll<RatingContext>()
				.Where( m => m.HasRatingTask == rowRatingTask.RowId && m.PayGradeType != rowPayGrade.RowId )
				.Select( m => m.PayGradeType ).Distinct()
				.Select( m => summary.ConceptSchemeMap.PayGradeCategory.Concepts.FirstOrDefault( n => n.RowId == m ) )
				.Where( m => m != null ).ToList();
			if( otherSummaryPayGradesForRatingTask.Count() > 0 )
			{
				result.Warnings.Add( "This row indicates that the Rating Task has a Pay Grade of " + rowPayGrade.CodedNotation + ". However, this Rating Task is already associated with " + otherSummaryPayGradesForRatingTask.Count() + " other Pay Grade(s) from earlier row(s) in this RMTL sheet: " + string.Join( ", ", otherSummaryPayGradesForRatingTask.Select( m => m.CodedNotation ).ToList() ) + "." );
			}

			//If the Rating Task already exists in the database, check to see if any other Rating Contexts already in the database connect it to a different Pay Grade
			if( rowRatingTask.Id > 0 )
			{
				var otherDBPayGradesForRatingTask = RatingContextManager.GetAllPayGradeIDsForRatingTaskByID( rowRatingTask.Id, ( rowPayGrade ?? new Concept() ).Id )
					.Where( m => m != rowPayGrade.Id )
					.Select( m => summary.ConceptSchemeMap.PayGradeCategory.Concepts.FirstOrDefault( n => n.Id == m ) )
					.Where( m => m != null ).ToList();
				if( otherDBPayGradesForRatingTask.Count() > 0 )
				{
					result.Warnings.Add( "This row indicates that the Rating Task has a Pay Grade of " + rowPayGrade.CodedNotation + ". However, this Rating Task is already associated with " + otherDBPayGradesForRatingTask.Count() + " other Pay Grade(s) that are already in the database: " + string.Join( ", ", otherDBPayGradesForRatingTask.Select( m => m.CodedNotation ).ToList() ) + "." );
				}
			}

			#endregion

			#region Update entities with data
			//Now that all of the actors for this row have been found, created, and tracked...
			//Update them with the data from this row
			//These should only include properties that are NOT used for uniqueness checks above, which have values that may change after the object is first created in the summary, such as:
			// - Lists of GUIDs where multiple values are found across multiple rows rather than packed into one cell, or where such values may be appended by later rows
			// - Text values that are permitted to change
			// - Objects where the object must be partially created and later related to another object that wasn't ready at the time of initial creation (should be rare)
			//The rest should be set when the object is created above

			//Billet Title
			//No changes to track!

			//Work Role
			//No changes to track!

			//Rating Task
			//No changes to track!

			//Reference Resource
			HandleGuidListAddition( summary, summary.ItemsToBeCreated.ReferenceResource, summary.FinalizedChanges.ReferenceResource, result, rowRatingTaskSource, nameof( ReferenceResource.ReferenceType ), rowSourceType ); //Values may be appended by later rows

			//Rating Context
			HandleValueChange( summary, summary.ItemsToBeCreated.RatingContext, summary.FinalizedChanges.RatingContext, result, rowRatingContext, nameof( RatingContext.Note ), item.Row.Note ?? "" ); //The Note may change?

			//Part 2
			if ( hasPart2Data && !item.SkipPart2Checks )
			{
				//Training Task
				//No changes to track!

				//Course
				foreach( var rowCourseType in rowCourseTypeList ) //Values may be appended by later rows
				{
					HandleGuidListAddition( summary, summary.ItemsToBeCreated.Course, summary.FinalizedChanges.Course, result, rowCourse, nameof( Course.CourseType ), rowCourseType );
				}

				//Course Context
				foreach ( var rowAssessmentMethodType in rowAssessmentMethodTypeList ) //Values may be appended by later rows
				{
					HandleGuidListAddition( summary, summary.ItemsToBeCreated.CourseContext, summary.FinalizedChanges.CourseContext, result, rowCourseContext, nameof( CourseContext.AssessmentMethodType ), rowAssessmentMethodType );
				}
			}

			//Part 3
			if ( hasPart3Data && !item.SkipPart3Checks ) //Uniqueness checks are done above, so any of these could theoretically change in a subsequent (re-)upload
			{
				//Cluster Analysis
				HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.PriorityPlacement ), priorityPlacement );
				HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.DevelopmentTime ), developmentTime );
				HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.EstimatedInstructionalTime ), estimatedInstructionalTime );
				HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.HasClusterAnalysisTitle ), rowClusterAnalysisTitle?.RowId );
				HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.TrainingSolutionType ), rowTrainingSolutionType?.RowId );
				HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.RecommendedModalityType ), rowRecommendModalityType?.RowId );
				HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.DevelopmentSpecificationType ), rowDevelopmentSpecificationType?.RowId );
				HandleValueChange( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.DevelopmentRatioType ), rowDevelopmentRatioType?.RowId );
				foreach ( var candidatePlatformType in rowCandidatePlatformTypeList ) //Values may be appended by later rows
				{
					HandleGuidListAddition( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.CandidatePlatformType ), candidatePlatformType );
				}
				foreach ( var cfmPlacementType in rowCFMPlacementTypeList ) //Values may be appended by later rows
				{
					HandleGuidListAddition( summary, summary.ItemsToBeCreated.ClusterAnalysis, summary.FinalizedChanges.ClusterAnalysis, result, rowClusterAnalysis, nameof( ClusterAnalysis.CFMPlacementType ), cfmPlacementType );
				}
			}


			#endregion

			//Update the cached summary
			CacheChangeSummary( summary );

			sectionDuration = DateTime.Now.Subtract( sectionStarted );
			//summary.Messages.Note.Add( string.Format( "Duration: {0:N2} seconds ", sectionDuration.TotalSeconds ) );

			//Return the result
			return result;
		}
		//

		public static T LookupOrGetFromDBOrCreateNew<T>( ChangeSummary summary, UploadableItemResult result, Func<T> GetFromSummary, Func<T> GetFromDatabase, Func<T> CreateNew, Action<T> LogNewItem, Func<bool> ReturnNullIfTrue = null, string errorMessageIfNull = null ) where T : BaseObject, new()
		{
			//If there is a method to check whether the item is null (e.g. the cell contains N/A), and that test comes back true, then skip the rest and return null
			if ( ReturnNullIfTrue != null && ReturnNullIfTrue() )
			{
				if ( !string.IsNullOrWhiteSpace( errorMessageIfNull ) )
				{
					result.Errors.Add( errorMessageIfNull );
				}

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

		public static T GetDataOrError<T>( ChangeSummary summary, Func<T, bool> MatchWith, UploadableItemResult result, string emptyOrNAErrorMessage, string notFoundErrorMessage, string source, bool addErrorIfNullOrWhiteSpace = true ) where T : BaseObject
		{
			return GetDataOrError( summary.GetAll<T>(), MatchWith, result, emptyOrNAErrorMessage, notFoundErrorMessage, source, addErrorIfNullOrWhiteSpace );
		}
		//

		public static List<T> GetDataListOrError<T>( ChangeSummary summary, Func<T, bool> MatchWith, UploadableItemResult result, string emptyOrNAErrorMessage, string notFoundErrorMessage, string source, bool addErrorIfNullOrWhiteSpace = true ) where T : BaseObject
		{
			return GetDataListOrError<T>( summary.GetAll<T>(), MatchWith, result, emptyOrNAErrorMessage, notFoundErrorMessage, source, addErrorIfNullOrWhiteSpace );
		}
		//

		public static T GetDataOrError<T>( List<T> haystack, Func<T, bool> MatchWith, UploadableItemResult result, string emptyOrNAErrorMessage, string notFoundErrorMessage, string source, bool addErrorIfNullOrWhiteSpace = true ) where T : BaseObject
		{
			return GetDataListOrError( haystack, MatchWith, result, emptyOrNAErrorMessage, notFoundErrorMessage, source, addErrorIfNullOrWhiteSpace ).FirstOrDefault();
		}
		//

		public static List<T> GetDataListOrError<T>( List<T> haystack, Func<T, bool> MatchWith, UploadableItemResult result, string emptyOrNAErrorMessage, string notFoundErrorMessage, string source, bool addErrorIfNullOrWhiteSpace = true) where T : BaseObject
		{
			//Handle empty/null/N/A string
			if ( string.IsNullOrWhiteSpace( source ) || source.ToLower() == "n/a" )
			{
				if ( addErrorIfNullOrWhiteSpace )
				{
					result.Errors.Add( emptyOrNAErrorMessage );
				}
				return new List<T>();
			}

			//Find and return matches
			var matches = haystack.Where( m => MatchWith( m ) ).ToList();
			if( matches.Count() == 0 )
			{
				result.Errors.Add( notFoundErrorMessage );
			}

			return matches;
		}
		//

		public static List<T> GetDataListOrErrorForEach<T>( List<T> haystack, List<string> source, Func<T, string, bool> MatchWith, UploadableItemResult result, string rawSource, string emptyOrNAErrorMessage, Func<string, string> CreateErrorMessageSingle, List<string> notFoundItemList ) where T : BaseObject
		{
			//Handle empty/null/N/A string
			if( string.IsNullOrWhiteSpace( rawSource ) || rawSource.ToLower() == "n/a" )
			{
				result.Errors.Add( emptyOrNAErrorMessage );
				return new List<T>();
			}

			//Find and return matches and errors
			var matches = new List<T>();
			foreach( var value in source ?? new List<string>() )
			{
				var match = haystack.FirstOrDefault( concept => MatchWith( concept, value ) );
				if( match == null )
				{
					result.Errors.Add( CreateErrorMessageSingle( value ) );
					notFoundItemList.Add( value );
				}
				else
				{
					matches.Add( match );
				}
			}

			return matches;
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

		public static T ParseNumberOrError<T>( UploadableItemResult result, string sourceValue, Func<string, T, T> ParseMethod, T defaultValue, Func<T, bool> ErrorIfTrueMethod, string errorMessage )
		{
			var value = ParseMethod( sourceValue, defaultValue );
			if ( ErrorIfTrueMethod( value ) )
			{
				result.Errors.Add( errorMessage );
			}

			return value;
		}
		//

		private static void HandleGuidListAddition<T1, T2>( ChangeSummary summary, List<T1> itemsToBeCreatedList, List<T1> finalizedChangesList, UploadableItemResult result, T1 rowItem, string propertyName, T2 referenceToBeAppended ) where T1 : BaseObject where T2 : BaseObject
		{
			//Skip if either the rowItem or the referenceToBeAppended are null
			if( rowItem == null || referenceToBeAppended == null )
			{
				return;
			}

			try
			{
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
			catch ( Exception ex )
			{
				result.Errors.Add( "Error handling GUID list addition for " + typeof( T1 ).Name + "." + propertyName + ": " + 
					ex.Message + ( string.IsNullOrWhiteSpace(ex.InnerException?.Message) ? "" : "; " + ex.InnerException.Message ) + 
					". Please contact system administration." );
				return;
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

			try
			{
				//Get the property to check/update
				var property = typeof( T1 ).GetProperty( propertyName );

				//Get the current value for that property
				var currentValue = ( T2 ) property.GetValue( rowItem );

				//If both values are null/whitespace or both values are the same, do nothing and return
				if ( ( string.IsNullOrWhiteSpace( currentValue?.ToString() ) && string.IsNullOrWhiteSpace( newValue?.ToString() ) ) || ( currentValue?.ToString().ToLower() == newValue?.ToString().ToLower() ) )
				{
					return;
				}

				//Try one other comparison
				try
				{
					if ( newValue != null && newValue.Equals( currentValue ) )
					{
						return;
					}
				}
				catch { }

				//If the new value is null, warn the user
				if ( newValue == null || ( property.PropertyType == typeof( Guid ) && newValue?.ToString() == Guid.Empty.ToString() ) )
				{
					result.Warnings.Add( "New value for property " + propertyName + " is null or empty!" );
				}

				//Set the value
				//This will update the rowItem in the summary
				property.SetValue( rowItem, newValue );

				//Track the change in the result
				if ( property.PropertyType == typeof( Guid ) )
				{
					var guidValue = Guid.Empty;
					if ( Guid.TryParse( newValue?.ToString(), out guidValue ) )
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
			catch ( Exception ex )
			{
				result.Errors.Add( "Error handling value change for " + typeof( T1 ).Name + "." + propertyName + ": " +
					ex.Message + ( string.IsNullOrWhiteSpace( ex.InnerException?.Message ) ? "" : "; " + ex.InnerException.Message ) +
					". Please contact system administration." );
				return;
			}
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
			catch { }

			return value ?? "";
		}
		//

		#endregion

	}
}
