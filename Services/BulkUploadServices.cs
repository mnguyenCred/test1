using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;

using Models.Schema;
using Models.Curation;

namespace Services
{
	public class BulkUploadServices
	{
		public static ChangeSummary ProcessUpload( UploadableTable uploaded, Guid ratingRowID, JObject debug = null )
		{
			debug = debug ?? new JObject();
			var result = new ChangeSummary();
			var referencedItems = new UploadableData();
			var latestStepFlag = "LatestStep";
			debug[ latestStepFlag ] = "Initial Setup";

			var currentRating = Factories.RatingManager.Get( ratingRowID );
			if( currentRating == null )
			{
				result.Errors.Add( "Error: Unable to find Rating for identifier: " + ratingRowID );
				return result;
			}

			var existing = new UploadableData(); //Get from database for the selected rating
			//var concepts = new List<Concept>(); //Get from database, possibly as separate variables for each scheme
			var payGradeTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_Pay_Grade ).Concepts;
			var trainingGapTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TrainingGap ).Concepts;
			var applicabilityTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TaskApplicability ).Concepts;
			var sourceTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_ReferenceResource ).Concepts;
			var courseTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_CourseType ).Concepts;
			var assessmentMethodTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_CurrentAssessmentApproach ).Concepts;
			debug[ latestStepFlag ] = "Got data from the Database";

			//Create a graph that will be used for searching for matching data
			//The data in this graph needs to be a hybrid of known/existing data and freshly added data so that the correct connections are made in a later step
			//So the List<>s need to be new entities (hence Concat()) but the existing entity references inside them should still be the originals (passed by reference)
			var graph = new UploadableData()
			{
				BilletTitle = new List<BilletTitle>().Concat( existing.BilletTitle ).ToList(),
				Course = new List<Course>().Concat( existing.Course ).ToList(),
				Organization = new List<Organization>().Concat( existing.Organization ).ToList(),
				RatingTask = new List<RatingTask>().Concat( existing.RatingTask ).ToList(),
				ReferenceResource = new List<ReferenceResource>().Concat( existing.ReferenceResource ).ToList(),
				TrainingTask = new List<TrainingTask>().Concat( existing.TrainingTask ).ToList(),
				WorkRole = new List<WorkRole>().Concat( existing.WorkRole ).ToList()
			};
			debug[ latestStepFlag ] = "Created Graph holder";

			//For each row...
			var rowCount = 0;
			foreach( var row in uploaded.Rows )
			{
				debug[ "Current Row" ] = (rowCount + 1);
				rowCount++;

				//First, get a reference (as in RAM pointer) to each uploadable object in the current row of the spreadsheet
				//Look in the "graph" data (which is the combination of existing data and data from earlier rows in this upload)
				//If no match is found, create a new object and put it into the graph

				//Concepts from Concept Schemes
				var payGradeType = FindConceptOrError( payGradeTypeConcepts, row.PayGradeType_Notation, true, "Pay Grade (Rank)", result.Errors );
				var trainingGapType = FindConceptOrError( trainingGapTypeConcepts, row.RatingTask_TrainingGapType_Label, false, "Training Gap Type", result.Errors );
				var applicabilityType = FindConceptOrError( applicabilityTypeConcepts, row.RatingTask_ApplicabilityType_Label, false, "Applicability Type", result.Errors );
				var sharedSourceType = FindConceptOrError( sourceTypeConcepts, row.Shared_ReferenceType, false, "Reference Resource Type (for Rating-Level Task)", result.Errors );
				var courseSourceType = FindConceptOrError( sourceTypeConcepts, row.Course_HasReferenceResource_Name, false, "Reference Resource Type (for Course)", result.Errors );
				var courseType = string.IsNullOrWhiteSpace( row.Course_CourseType_Label ) ?
					null : //It's okay if this is null, since only some rows have course types
					FindConceptOrError( courseTypeConcepts, row.Course_CourseType_Label, false, "Course Type", result.Errors );
				var assessmentMethodType = string.IsNullOrWhiteSpace( row.Course_AssessmentMethodType_Label) ? 
					null : //It's okay if this is null, since only some rows have assessment methods
					FindConceptOrError( assessmentMethodTypeConcepts, row.Course_AssessmentMethodType_Label, false, "Pay Grade (Rank)", result.Errors );

				//Stop processing if one or more unknown concepts were detected
				if( result.Errors.Count() > 0 )
				{
					break;
				}
				debug[ latestStepFlag ] = "Got concept data for row " + rowCount;

				//Uploadable objects
				//Billet Title
				var billetTitle = graph.BilletTitle.FirstOrDefault( m =>
					 m.Name == row.BilletTitle_Name
				);
				if ( billetTitle == null ) {
					billetTitle = new BilletTitle() 
					{ 
						RowId = Guid.NewGuid(), 
						Name = row.BilletTitle_Name,
						HasRating = ratingRowID
					};
					graph.BilletTitle.Add( billetTitle );
					result.ItemsToBeCreated.BilletTitle.Add( billetTitle );
					result.ChangeNote.Add( "Created new Billet Title: " + billetTitle.Name );
				}
				else
				{
					Append( referencedItems.BilletTitle, billetTitle );
				}
				debug[ latestStepFlag ] = "Got Billet Title data for row " + rowCount;

				//Work Role (aka Functional Area)
				var workRole = graph.WorkRole.FirstOrDefault( m =>
					 m.Name == row.WorkRole_Name
				);
				if ( workRole == null ) {
					workRole = new WorkRole() 
					{ 
						RowId = Guid.NewGuid(), 
						Name = row.WorkRole_Name 
					};
					graph.WorkRole.Add( workRole );
					result.ItemsToBeCreated.WorkRole.Add( workRole );
					result.ChangeNote.Add( "Created new Functional Area: " + workRole.Name );
				}
				else
				{
					Append( referencedItems.WorkRole, workRole );
				}
				debug[ latestStepFlag ] = "Got Work Role data for row " + rowCount;

				//Task Source (The Reference Resource for the task, distinct from the Reference Resource for the Course in another column)
				var taskSource = graph.ReferenceResource.FirstOrDefault( m =>
					 m.Name == row.ReferenceResource_Name &&
					 m.PublicationDate == ParseDateOrEmpty( row.ReferenceResource_PublicationDate )
				);
				if ( taskSource == null ) {
					taskSource = new ReferenceResource() 
					{ 
						RowId = Guid.NewGuid(), 
						Name = row.ReferenceResource_Name, 
						PublicationDate = ParseDateOrEmpty( row.ReferenceResource_PublicationDate ) 
					};
					graph.ReferenceResource.Add( taskSource );
					result.ItemsToBeCreated.ReferenceResource.Add( taskSource );
					result.ChangeNote.Add( "Created new Reference Resource for a Task: " + taskSource.Name );
				}
				else
				{
					Append( referencedItems.ReferenceResource, taskSource );
				}
				debug[ latestStepFlag ] = "Got Task Source data for row " + rowCount;

				//Rating Task
				var ratingTask = graph.RatingTask.FirstOrDefault( m =>
					 m.Description == row.RatingTask_Description &&
					 m.Note == row.Note &&
					 Find( graph.WorkRole, m.HasWorkRole ).Select( n => n.Name ).Contains( row.WorkRole_Name ) &&
					 Find( graph.ReferenceResource, m.HasReferenceResource )?.Name == row.ReferenceResource_Name &&
					 Find( graph.ReferenceResource, m.HasReferenceResource )?.PublicationDate == ParseDateOrEmpty( row.ReferenceResource_PublicationDate ) &&
					 sharedSourceType?.Name == row.Shared_ReferenceType &&
					 trainingGapType?.Name == row.RatingTask_TrainingGapType_Label &&
					 applicabilityType?.Name == row.RatingTask_ApplicabilityType_Label &&
					 payGradeType?.CodedNotation == row.PayGradeType_Notation &&
					 ( Find( graph.TrainingTask, m.HasTrainingTask )?.Description ?? "" ) == ( row.TrainingTask_Description ?? "" )
				);
				if ( ratingTask == null ) {
					ratingTask = new RatingTask() 
					{ 
						RowId = Guid.NewGuid(), 
						Description = row.RatingTask_Description, 
						Note = row.Note,
						HasReferenceResource = taskSource.RowId,
						PayGradeType = payGradeType.RowId,
						ApplicabilityType = applicabilityType.RowId,
						TrainingGapType = trainingGapType.RowId,
						ReferenceType = sharedSourceType.RowId
					};
					graph.RatingTask.Add( ratingTask );
					result.ItemsToBeCreated.RatingTask.Add( ratingTask );
				}
				else
				{
					Append( referencedItems.RatingTask, ratingTask );
				}
				debug[ latestStepFlag ] = "Got Rating Task data for row " + rowCount;

				//The last few may or may not be present in some rows, hence the extra special handling for nulls
				//Course Source (The Reference Resource for the course, distinct from the Reference Resource for the task in another column)
				TrainingTask trainingTask = null;
				if ( !string.IsNullOrWhiteSpace( row.TrainingTask_Description ) )
				{
					trainingTask = graph.TrainingTask.FirstOrDefault( m =>
						m.Description == row.TrainingTask_Description
					);
					if(trainingTask == null )
					{
						trainingTask = new TrainingTask() 
						{ 
							RowId = Guid.NewGuid(), 
							Description = row.TrainingTask_Description 
						};
						graph.TrainingTask.Add( trainingTask );
						result.ItemsToBeCreated.TrainingTask.Add( trainingTask );
						result.ChangeNote.Add( "Created new Training Task: " + trainingTask.Description );
					}
				}
				else
				{
					Append( referencedItems.TrainingTask, trainingTask );
				}
				debug[ latestStepFlag ] = "Got Training Task data for row " + rowCount;

				ReferenceResource courseSource = null;
				if ( !string.IsNullOrWhiteSpace( row.Course_HasReferenceResource_Name ) )
				{
					courseSource = graph.ReferenceResource.FirstOrDefault( m =>
						m.Name == row.Course_HasReferenceResource_Name
					);
					if( courseSource == null )
					{
						courseSource = new ReferenceResource() 
						{ 
							RowId = Guid.NewGuid(), 
							Name = row.Course_HasReferenceResource_Name 
						};
						graph.ReferenceResource.Add( courseSource );
						result.ItemsToBeCreated.ReferenceResource.Add( courseSource );
						result.ChangeNote.Add( "Created new Reference Resource for a Course: " + courseSource.Name );
					}
				}
				else
				{
					Append( referencedItems.ReferenceResource, courseSource );
				}
				debug[ latestStepFlag ] = "Got Reference Resource data for row " + rowCount;

				//Course
				Course course = null;
				if ( !string.IsNullOrWhiteSpace( row.Course_CodedNotation ) )
				{
					course = graph.Course.FirstOrDefault( m =>
						m.Name == row.Course_Name &&
						m.CodedNotation == row.Course_CodedNotation &&
						assessmentMethodType?.Name == row.Course_AssessmentMethodType_Label
					);
					if(course == null )
					{
						course = new Course() 
						{ 
							RowId = Guid.NewGuid(), 
							Name = row.Course_Name, 
							CodedNotation = row.Course_CodedNotation,
							AssessmentMethodType = assessmentMethodType.RowId,
							CourseType = courseType.RowId
						};
						graph.Course.Add( course );
						result.ItemsToBeCreated.Course.Add( course );
						result.ChangeNote.Add( "Created new Course: " + course.CodedNotation + " - " + course.Name );
					}
				}
				else
				{
					Append( referencedItems.Course, course );
				}
				debug[ latestStepFlag ] = "Got Course data for row " + rowCount;

				//Organization
				Organization cca = null;
				if ( !string.IsNullOrWhiteSpace( row.Course_CurriculumControlAuthority_Name ) )
				{
					cca = graph.Organization.FirstOrDefault( m =>
						 m.Name == row.Course_CurriculumControlAuthority_Name
					);
					if(cca == null )
					{
						cca = new Organization() 
						{ 
							RowId = Guid.NewGuid(), 
							Name = row.Course_CurriculumControlAuthority_Name 
						};
						graph.Organization.Add( cca );
						result.ItemsToBeCreated.Organization.Add( cca );
						result.ChangeNote.Add( "Created new Curriculum Control Authority: " + cca.Name );
					}
				}
				else
				{
					Append( referencedItems.Organization, cca );
				}
				debug[ latestStepFlag ] = "Got Organization data for row " + rowCount;

				//Now that the objects have been retrieved and/or created, attach them to each other
				//Attachments are based on what data is connected to what other data for the current row
				//Once all of the rows are processed, the aggregation of these connections should present a complete graph of the data
				//This is also where we figure out whether something that was already in the existing data has been changed

				//If the Billet Title already exists, and needs to be modified, then handle it
				if ( Find( existing.BilletTitle, billetTitle.RowId ) != null )
				{
					//Assign Rating Task to Billet Title if it doesn't already contain it
					if ( !billetTitle.HasRatingTask.Contains( ratingTask.RowId ) )
					{
						//Use a temporary copy of the object to keep track of which items get added to it
						var tracked = Find( result.UploadedInnerListsForCopiesOfItems.BilletTitle, billetTitle.RowId );
						if( tracked == null )
						{
							tracked = new BilletTitle() { RowId = billetTitle.RowId };
							result.UploadedInnerListsForCopiesOfItems.BilletTitle.Add( tracked );
						}
						tracked.HasRatingTask.Add( ratingTask.RowId );
						result.ChangeNote.Add( "Updated Billet Title: " + billetTitle.Name + " with new Rating Task: " + ratingTask.Description );
					}
				}
				//Otherwise just update the new Billet Title
				else
				{
					Append( billetTitle.HasRatingTask, ratingTask.RowId );
				}
				debug[ latestStepFlag ] = "Processed Existing/New Billet Title data for row " + rowCount;

				//If the Rating Task already exists under the current rating, and needs to be modified, then handle it
				if( Find( existing.RatingTask, ratingTask.RowId ) != null )
				{
					//Not sure there would be any changes allowed this way?
				}
				//Otherwise, just treat it like a new Rating Task for now (we will check for it under another Rating later so we can process those in bulk)
				else
				{
					Append( ratingTask.HasRating, ratingRowID );
					Append( ratingTask.HasWorkRole, workRole.RowId );
					ratingTask.HasTrainingTask = trainingTask?.RowId ?? Guid.Empty; //Training Task gets determined after a new Rating Task is initialized, so it isn't available at that time
				}
				debug[ latestStepFlag ] = "Processed Existing/New Rating Task data for row " + rowCount;

				//If the Course already exists, and needs to be modified, then handle it
				if ( Find( existing.Course, course.RowId ) != null )
				{
					//Assign Training Tasks to Course if it doesn't already reference them
					if ( trainingTask != null && !course.HasTrainingTask.Contains( trainingTask.RowId ) )
					{
						//Use a temporary copy of the object to keep track of which items get added to it
						var tracked = Find( result.UploadedInnerListsForCopiesOfItems.Course, course.RowId );
						if ( tracked == null )
						{
							tracked = new Course() { RowId = course.RowId };
							result.UploadedInnerListsForCopiesOfItems.Course.Add( tracked );
						}
						tracked.HasTrainingTask.Add( trainingTask.RowId );
					}

					//Assign CCA to Course if it doesn't already reference them
					if( cca != null && !course.CurriculumControlAuthority.Contains( cca.RowId ) )
					{
						//Use a temporary copy of the object to keep track of which items get added to it
						var tracked = Find( result.UploadedInnerListsForCopiesOfItems.Course, course.RowId );
						if ( tracked == null )
						{
							tracked = new Course() { RowId = course.RowId };
							result.UploadedInnerListsForCopiesOfItems.Course.Add( tracked );
						}
						tracked.CurriculumControlAuthority.Add( cca.RowId );
					}
				}
				//Otherwise, just treat it like a new Course
				else
				{
					Append( course.CurriculumControlAuthority, cca.RowId );
					Append( course.HasTrainingTask, trainingTask.RowId );
				}
				debug[ latestStepFlag ] = "Processed Existing/New Course data for row " + rowCount;

				//If the ReferenceResource for the task already exists, and needs to be modified, then handle it
				if ( Find( existing.ReferenceResource, taskSource.RowId ) != null )
				{
					//Assign reference types to reference if it doesn't already reference them
					if ( sharedSourceType != null && !taskSource.ReferenceType.Contains( sharedSourceType.RowId ) )
					{
						//Use a temporary copy of the object to keep track of which items get added to it
						var tracked = Find( result.UploadedInnerListsForCopiesOfItems.ReferenceResource, taskSource.RowId );
						if ( tracked == null )
						{
							tracked = new ReferenceResource() { RowId = taskSource.RowId };
							result.UploadedInnerListsForCopiesOfItems.ReferenceResource.Add( tracked );
						}
						tracked.ReferenceType.Add( sharedSourceType.RowId );
					}
				}
				//Otherwise, just treat it like a new ReferenceResource
				else
				{
					Append( taskSource.ReferenceType, sharedSourceType.RowId );
				}
				debug[ latestStepFlag ] = "Processed Reference Type for Reference Resource (for Task)";

				//If the ReferenceResource for the task already exists, and needs to be modified, then handle it
				if ( Find( existing.ReferenceResource, courseSource.RowId ) != null )
				{
					//Assign reference types to reference if it doesn't already reference them
					if ( courseSourceType != null && !courseSource.ReferenceType.Contains( courseSourceType.RowId ) )
					{
						//Use a temporary copy of the object to keep track of which items get added to it
						var tracked = Find( result.UploadedInnerListsForCopiesOfItems.ReferenceResource, courseSource.RowId );
						if ( tracked == null )
						{
							tracked = new ReferenceResource() { RowId = courseSource.RowId };
							result.UploadedInnerListsForCopiesOfItems.ReferenceResource.Add( tracked );
						}
						tracked.ReferenceType.Add( courseSourceType.RowId );
					}
				}
				//Otherwise, just treat it like a new ReferenceResource
				else
				{
					Append( courseSource.ReferenceType, courseSourceType.RowId );
				}
				debug[ latestStepFlag ] = "Processed Reference Type for Reference Resource (for Course)";

			}
			if ( result.Errors.Count() > 0 )
			{
				result.Errors.Add( "One or more errors found. Processing cancelled. Please resolve the errors and try again." );
				return result;
			}
			debug[ latestStepFlag ] = "Finished processing spreadsheet data";

			//Process all of the seemingly-new Rating Tasks to see if they actually show up under some other Rating and just need to be associated with this one
			var ratingTasksToUpdate = new List<RatingTask>();
			var ratingTasksThatActuallyExistUnderOtherRatings = SomeMethodThatFindsRatingTasksInBulkIrrespectiveOfTheirRating( result.ItemsToBeCreated.RatingTask );
			debug[ latestStepFlag ] = "Got data for tasks that might exist under other ratings";

			foreach( var otherRatingTask in ratingTasksThatActuallyExistUnderOtherRatings )
			{
				debug[ "ProcessingOtherRatingTask" ] = otherRatingTask.RowId.ToString() + " - " + otherRatingTask.Description;
				var match = result.ItemsToBeCreated.RatingTask.FirstOrDefault( m => 
					m.Description == otherRatingTask.Description && 
					m.PayGradeType == otherRatingTask.PayGradeType && 
					m.HasReferenceResource == otherRatingTask.HasReferenceResource 
				);
				if ( match == null )
				{
					result.Errors.Add( "Error: This task seems to exist already, but a match wasn't able to be found: " + otherRatingTask.Description );
				}
				else
				{
					//Use a temporary copy of the object to keep track of which items get added to it
					var tracked = Find( result.UploadedInnerListsForCopiesOfItems.RatingTask, otherRatingTask.RowId );
					if(tracked == null )
					{
						tracked = new RatingTask() { RowId = otherRatingTask.RowId };
						result.UploadedInnerListsForCopiesOfItems.RatingTask.Add( tracked );
					}

					//Add Rating association
					if ( !otherRatingTask.HasRating.Contains( ratingRowID ) )
					{
						Append( tracked.HasRating, ratingRowID );
						result.ChangeNote.Add( "Added Rating association: " + currentRating.Name + " to Rating Task: " + otherRatingTask.Description );
					}

					//Add Work Role association
					foreach ( var roleRowID in match.HasWorkRole )
					{
						if ( !otherRatingTask.HasWorkRole.Contains( roleRowID ) )
						{
							Append( tracked.HasWorkRole, roleRowID );
							result.ChangeNote.Add( "Added Functional Area association: " + Find( graph.WorkRole, roleRowID )?.Name + " to Rating Task: " + otherRatingTask.Description );
						}
					}

					//Remove the "new" task from "items to be created" and put the real task in "items to be changed"
					result.ItemsToBeCreated.RatingTask.Remove( match );
					result.ItemsToBeChanged.RatingTask.Add( otherRatingTask );
				}
			}
			debug[ latestStepFlag ] = "Processed tasks that might exist under other ratings";

			//Now add creation notes for the truly new tasks
			foreach( var ratingTask in result.ItemsToBeCreated.RatingTask )
			{
				result.ChangeNote.Add( "Created new Rating Task: " + ratingTask.Description );
			}
			debug[ latestStepFlag ] = "Created change notes for new tasks";

			//Process existing data to see if anything was missing from the uploaded data (attempt to detect deletes)
			FlagItemsForDeletion( existing.BilletTitle, referencedItems.BilletTitle, result.ItemsToBeDeleted.BilletTitle, result.ChangeNote, "Billet Title", ( BilletTitle m ) => { return m.CTID + " - " + m.Name; } );
			FlagItemsForDeletion( existing.Course, referencedItems.Course, result.ItemsToBeDeleted.Course, result.ChangeNote, "Course", ( Course m ) => { return m.CTID + " - " + m.Name; } );
			FlagItemsForDeletion( existing.Organization, referencedItems.Organization, result.ItemsToBeDeleted.Organization, result.ChangeNote, "Organization (CCA)", ( Organization m ) => { return m.CTID + " - " + m.Name; } );
			FlagItemsForDeletion( existing.TrainingTask, referencedItems.TrainingTask, result.ItemsToBeDeleted.TrainingTask, result.ChangeNote, "Training Task", ( TrainingTask m ) => { return m.CTID + " - " + m.Description; } );
			FlagItemsForDeletion( existing.WorkRole, referencedItems.WorkRole, result.ItemsToBeDeleted.WorkRole, result.ChangeNote, "Functional Area", ( WorkRole m ) => { return m.CTID + " - " + m.Name; } );
			debug[ latestStepFlag ] = "Flagged normal items for deletion";

			//Only flag the task if it isn't associated with any other rating
			FlagItemsForDeletion( existing.RatingTask.Where( m => m.HasRating.Count() == 1 && m.HasRating.FirstOrDefault() == ratingRowID ).ToList(), referencedItems.RatingTask, result.ItemsToBeDeleted.RatingTask, result.ChangeNote, "Rating Task", ( RatingTask m ) => { return m.CTID + " - " + m.Description; } );

			//Probably shouldn't ever delete reference resources(?)
			//FlagItemsForDeletion( existing.ReferenceResource, referencedItems.ReferenceResource, result.ItemsToBeDeleted.ReferenceResource, result.ChangeNote, "Reference Resource", ( ReferenceResource m ) => { return m.CTID + " - " + m.Name; } );
			debug[ latestStepFlag ] = "Flagged special items for deletion";

			//Handle items removed from inner lists
			foreach( var originalBilletTitle in existing.BilletTitle )
			{
				var uploadedMatch = Find( result.UploadedInnerListsForCopiesOfItems.BilletTitle, originalBilletTitle.RowId );
				var removalTracker = new BilletTitle() { RowId = originalBilletTitle.RowId };

				if( uploadedMatch != null )
				{
					foreach ( var item in originalBilletTitle.HasRatingTask.Where( m => !uploadedMatch.HasRatingTask.Contains( m ) ).ToList() ) 
					{
						removalTracker.HasRatingTask.Add( item );
						result.ChangeNote.Add( "Removed Rating Task reference from Billet Title: " + uploadedMatch.Name + " - " + Find( existing.RatingTask, item )?.Description );
					}
				}

				if( removalTracker.HasRatingTask.Count() > 0 )
				{
					result.RemovedItemsFromInnerListsForCopiesOfItems.BilletTitle.Add( removalTracker );
				}
			}
			debug[ latestStepFlag ] = "Handled items removed from Billet Titles";

			foreach( var originalCourse in existing.Course )
			{
				var uploadedMatch = Find( result.UploadedInnerListsForCopiesOfItems.Course, originalCourse.RowId );
				var removalTracker = new Course() { RowId = originalCourse.RowId };

				if( uploadedMatch != null )
				{
					foreach( var item in originalCourse.CurriculumControlAuthority.Where( m => !uploadedMatch.CurriculumControlAuthority.Contains( m ) ).ToList() )
					{
						removalTracker.CurriculumControlAuthority.Add( item );
						result.ChangeNote.Add( "Removed Curriculum Control Authority reference from Course: " + uploadedMatch.Name + " - " + Find( existing.Organization, item )?.Name );
					}

					foreach ( var item in originalCourse.HasTrainingTask.Where( m => !uploadedMatch.HasTrainingTask.Contains( m ) ).ToList() ) 
					{
						removalTracker.HasTrainingTask.Add( item );
						result.ChangeNote.Add( "Removed Training Task reference from Course: " + uploadedMatch.Name + " - " + Find( existing.TrainingTask, item )?.Description );
					}
				}

				if( removalTracker.CurriculumControlAuthority.Count() > 0 || removalTracker.HasTrainingTask.Count() > 0 )
				{
					result.RemovedItemsFromInnerListsForCopiesOfItems.Course.Add( removalTracker );
				}
			}
			debug[ latestStepFlag ] = "Handled items removed from Courses";

			foreach( var originalTask in existing.RatingTask )
			{
				var uploadedMatch = Find( result.UploadedInnerListsForCopiesOfItems.RatingTask, originalTask.RowId );
				var removalTracker = new RatingTask() { RowId = originalTask.RowId };
				if( uploadedMatch != null )
				{
					foreach( var item in originalTask.HasWorkRole.Where( m => !uploadedMatch.HasWorkRole.Contains( m ) ).ToList() )
					{
						removalTracker.HasWorkRole.Add( item );
						result.ChangeNote.Add( "Removed Functional Area reference from Rating Task: " + Find( existing.WorkRole, item )?.Name + " - " + uploadedMatch.Description );
					}
				}

				//Special handling for cases where the Rating Task was removed from this Rating but still exists for other Ratings
				//If the original existing task was not referenced (ie was not in the spreadsheet) and references more than one rating, process it
				if ( Find( referencedItems.RatingTask, originalTask.RowId ) == null && originalTask.HasRating.Count() > 1 ) 
				{
					removalTracker.HasRating.Add( ratingRowID );
					result.ChangeNote.Add( "Removed Rating reference from Rating Task: " + currentRating.CodedNotation + " - " + originalTask.Description );
				}

				if( removalTracker.HasWorkRole.Count() > 0 || removalTracker.HasRating.Count() > 0 )
				{
					result.RemovedItemsFromInnerListsForCopiesOfItems.RatingTask.Add( removalTracker );
				}
			}
			debug[ latestStepFlag ] = "Handled items removed from Rating Tasks";

			//Figure out unchanged counts
			result.UnchangedCount.BilletTitle = GetUnchangedCount( nameof( existing.BilletTitle ), existing.BilletTitle, result );
			result.UnchangedCount.Course = GetUnchangedCount( nameof( existing.Course ), existing.Course, result );
			result.UnchangedCount.Organization = GetUnchangedCount( nameof( existing.Organization ), existing.Organization, result );
			result.UnchangedCount.RatingTask = GetUnchangedCount( nameof( existing.RatingTask ), existing.RatingTask, result );
			result.UnchangedCount.ReferenceResource = GetUnchangedCount( nameof( existing.ReferenceResource ), existing.ReferenceResource, result );
			result.UnchangedCount.TrainingTask = GetUnchangedCount( nameof( existing.TrainingTask ), existing.TrainingTask, result );
			result.UnchangedCount.WorkRole = GetUnchangedCount( nameof( existing.WorkRole ), existing.WorkRole, result );
			debug[ latestStepFlag ] = "Figured out unchanged item counts";

			debug[ latestStepFlag ] = "Finished processing upload.";
			return result;
		}
		//

		//Temporary placeholder
		private static List<RatingTask> SomeMethodThatFindsRatingTasksInBulkIrrespectiveOfTheirRating( List<RatingTask> input )
		{
			return new List<RatingTask>();
		}
		//

		private static Concept FindConceptOrError( List<Concept> haystack, string searchFor, bool useNotation, string warningLabel, List<string> warningMessages )
		{
			var match = haystack?.FirstOrDefault( m => useNotation ? ( m.CodedNotation?.ToLower() == searchFor?.ToLower() ) : ( m.Name?.ToLower() == searchFor?.ToLower() ) );
			if( match == null )
			{
				warningMessages.Add( "Error: Found unrecognized " + warningLabel + ": " + searchFor );
				return new Concept() { RowId = Guid.NewGuid(), Name = useNotation ? null : searchFor, CodedNotation = useNotation ? searchFor : null };
			}
			return match;
		}
		//

		private static void FlagItemsForDeletion<T>( List<T> existingItems, List<T> referencedItems, List<T> itemsToBeDeleted, List<string> changeNotes, string typeLabel, Func<T, string> getItemLabel )
		{
			foreach( var item in existingItems.Where( m => !referencedItems.Contains( m ) ).ToList() )
			{
				itemsToBeDeleted.Add( item );
				changeNotes.Add( "Warning: The following " + typeLabel + " was not detected in the uploaded data and will be deleted: " + getItemLabel( item ) );
			}
		}
		//

		private static int GetUnchangedCount<T>( string propertyName, List<T> existing, ChangeSummary result ) where T : BaseObject
		{
			var property = typeof( UploadableData ).GetProperty( propertyName );
			return existing.Where( m =>
				 Find( ( List<T> ) property.GetValue( result.ItemsToBeChanged ), m.RowId ) == null &&
				 Find( ( List<T> ) property.GetValue( result.ItemsToBeDeleted ), m.RowId ) == null &&
				 Find( ( List<T> ) property.GetValue( result.UploadedInnerListsForCopiesOfItems ), m.RowId ) == null &&
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

		private static void Append<T>( List<T> items, T item )
		{
			if ( items != null && item != null && !items.Contains( item ) )
			{
				items.Add( item );
			}
		}
		//

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
