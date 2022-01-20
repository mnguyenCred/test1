﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Runtime.Caching;
using Newtonsoft.Json.Linq;
using Models.Application;
using Models.Schema;
using Models.Curation;
using Factories;

namespace Services
{
	public class BulkUploadServices
	{
		public static ChangeSummary ProcessUpload( UploadableTable uploaded, Guid ratingRowID, JObject debug = null )
		{
			debug = debug ?? new JObject();
			var result = new ChangeSummary() { RowId = Guid.NewGuid() };
			var referencedItems = new UploadableData();
			var latestStepFlag = "LatestStep";
			debug[ latestStepFlag ] = "Initial Setup";

			var currentRating = Factories.RatingManager.Get( ratingRowID );
			if( currentRating == null )
			{
				result.Messages.Error.Add( "Error: Unable to find Rating for identifier: " + ratingRowID );
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

			//mp - should add caching for these
			existing.BilletTitle = Factories.JobManager.GetAll();
			existing.Course = Factories.CourseManager.GetAll(true);
			existing.Organization = Factories.OrganizationManager.GetAll();
			existing.ReferenceResource = Factories.ReferenceResourceManager.GetAll();
			existing.WorkRole = Factories.WorkRoleManager.GetAll();
			//training task - really all?
			existing.TrainingTask = Factories.CourseManager.TrainingTaskGetAll();
			//should not get all rating task once have many rmtls
			int totalRows = 0;
			existing.RatingTask = Factories.RatingTaskManager.GetAll( currentRating.CodedNotation , true, ref totalRows);
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

				//Pre-cleaning
				row.Course_CourseType_Label = NullifyNotApplicable( row.Course_CourseType_Label );
				row.Course_CodedNotation = NullifyNotApplicable( row.Course_CodedNotation );
				row.Course_Name = NullifyNotApplicable( row.Course_Name );
				row.Course_CourseType_Label = NullifyNotApplicable( row.Course_CourseType_Label );
				row.Course_CurriculumControlAuthority_Name = NullifyNotApplicable( row.Course_CurriculumControlAuthority_Name );
				row.Course_HasReferenceResource_Name = NullifyNotApplicable( row.Course_HasReferenceResource_Name );
				row.Course_AssessmentMethodType_Label = NullifyNotApplicable( row.Course_AssessmentMethodType_Label );
				row.ReferenceResource_PublicationDate = NullifyNotApplicable( row.ReferenceResource_PublicationDate );
				if ( BaseFactory.IsValidDate( row.ReferenceResource_PublicationDate ) )
				{
					//normalize
					row.ReferenceResource_PublicationDate = DateTime.Parse( row.ReferenceResource_PublicationDate ).ToString( "yyyy-MM-dd" );
				}

				//Concepts from Concept Schemes
				var payGradeType = FindConceptOrError( payGradeTypeConcepts, new Concept() { CodedNotation = row.PayGradeType_Notation }, "Pay Grade (Rank)", row.PayGradeType_Notation, result.Messages.Error );
				var trainingGapType = FindConceptOrError( trainingGapTypeConcepts, new Concept() { Name = row.RatingTask_TrainingGapType_Label }, "Training Gap Type", row.RatingTask_TrainingGapType_Label, result.Messages.Error );
				var applicabilityType = FindConceptOrError( applicabilityTypeConcepts, new Concept() { Name = row.RatingTask_ApplicabilityType_Label }, "Applicability Type", row.RatingTask_ApplicabilityType_Label, result.Messages.Error );
				var sharedSourceType = FindConceptOrError( sourceTypeConcepts, new Concept() { WorkElementType = row.Shared_ReferenceType }, "Reference Resource Type (for Rating-Level Task)", row.Shared_ReferenceType, result.Messages.Error );
				//var courseSourceType = FindConceptOrError( sourceTypeConcepts, row.Course_HasReferenceResource_Name, false, "Reference Resource Type (for Course)", result.Errors );
				var courseSourceType = sourceTypeConcepts.FirstOrDefault( m => m.CodedNotation == "LCCD" ); //Course Reference Resource Type is always a Life-Cycle Control Document
				var courseType = string.IsNullOrWhiteSpace( row.Course_CourseType_Label ) ?
					null : //It's okay if this is null, since only some rows have course types
					FindConceptOrError( courseTypeConcepts, new Concept() { Name = row.Course_CourseType_Label }, "Course Type", row.Course_CourseType_Label, result.Messages.Error );
				var assessmentMethodTypes = string.IsNullOrWhiteSpace( row.Course_AssessmentMethodType_Label ) ? 
					null : //It's okay if this is null, since only some rows have assessment methods
					FindConceptListOrError( assessmentMethodTypeConcepts, new Concept() { Name = row.Course_AssessmentMethodType_Label }, "Assessment Method Type", row.Course_AssessmentMethodType_Label, result.Messages.Error );
				

				//Stop processing if one or more unknown concepts were detected
				if( result.Messages.Error.Count() > 0 )
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
					result.Messages.Create.Add( "Billet Title: " + billetTitle.Name );
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
				if ( workRole == null ) 
				{
					workRole = new WorkRole() 
					{ 
						RowId = Guid.NewGuid(), 
						Name = row.WorkRole_Name 
					};
					graph.WorkRole.Add( workRole );
					result.ItemsToBeCreated.WorkRole.Add( workRole );
					result.Messages.Create.Add( "Functional Area: " + workRole.Name );
				}
				else
				{
					Append( referencedItems.WorkRole, workRole );
				}
				debug[ latestStepFlag ] = "Got Work Role data for row " + rowCount;

				//Task Source (The Reference Resource for the task, distinct from the Reference Resource for the Course in another column)
				//mp-skip publication date
				var taskSource = graph.ReferenceResource.FirstOrDefault( m =>
					 m.Name == row.ReferenceResource_Name 
					 //&& m.PublicationDate ==  (row.ReferenceResource_PublicationDate ?? "")
					 //&& m.PublicationDate == ParseDateOrEmpty( row.ReferenceResource_PublicationDate )
				);
				//var taskSource2 = graph.ReferenceResource.FirstOrDefault( m =>
				//	 m.Name == row.ReferenceResource_Name 
				//);
				if ( taskSource == null ) 
				{
					taskSource = new ReferenceResource() 
					{ 
						RowId = Guid.NewGuid(), 
						Name = row.ReferenceResource_Name, 
						PublicationDate = ( row.ReferenceResource_PublicationDate ?? "" )
						//PublicationDate = ParseDateOrEmpty( row.ReferenceResource_PublicationDate )
					};
					if ( BaseFactory.IsValidDate( taskSource.PublicationDate ) )
					{
						//normalize
						taskSource.PublicationDate = DateTime.Parse( taskSource.PublicationDate ).ToString( "yyyy-MM-dd" );
					}
					graph.ReferenceResource.Add( taskSource );
					result.ItemsToBeCreated.ReferenceResource.Add( taskSource );
					result.Messages.Create.Add( "Reference Resource for a Task: " + taskSource.Name );
				}
				else
				{
					Append( referencedItems.ReferenceResource, taskSource );
				}
				debug[ latestStepFlag ] = "Got Task Source data for row " + rowCount;

				//Rating Task
				//why use the Note in the check?
				var ratingTask = graph.RatingTask.FirstOrDefault( m =>
					 m.Description == row.RatingTask_Description &&
					 //m.Note == row.Note &&
					 Find( graph.WorkRole, m.HasWorkRole ).Select( n => n.Name ).Contains( row.WorkRole_Name ) 
					 && Find( graph.ReferenceResource, m.HasReferenceResource )?.Name == row.ReferenceResource_Name
					 //&& Find( graph.ReferenceResource, m.HasReferenceResource )?.PublicationDate == ParseDateOrEmpty( row.ReferenceResource_PublicationDate ) 
					 //.need codedNotation here
					 && sharedSourceType?.CodedNotation == row.Shared_ReferenceType
					 && trainingGapType?.Name == row.RatingTask_TrainingGapType_Label 
					 && applicabilityType?.Name == row.RatingTask_ApplicabilityType_Label 
					 && payGradeType?.CodedNotation == row.PayGradeType_Notation 
					 && ( Find( graph.TrainingTask, m.HasTrainingTask )?.Description ?? "" ) == ( row.TrainingTask_Description ?? "" )
				);
				//
				var ratingTask2 = graph.RatingTask.FirstOrDefault( m =>
					 m.Description == row.RatingTask_Description 
					// && m.Note == row.Note 
					 && Find( graph.WorkRole, m.HasWorkRole ).Select( n => n.Name ).Contains( row.WorkRole_Name ) 
					 && Find( graph.ReferenceResource, m.HasReferenceResource )?.Name == row.ReferenceResource_Name 

					 //&& Find( graph.ReferenceResource, m.HasReferenceResource )?.PublicationDate == ParseDateOrEmpty( row.ReferenceResource_PublicationDate ) 
					 //.need codedNotation here
					 && sharedSourceType?.CodedNotation == row.Shared_ReferenceType 
					 && trainingGapType?.Name == row.RatingTask_TrainingGapType_Label 
					 && applicabilityType?.Name == row.RatingTask_ApplicabilityType_Label 
					 && payGradeType?.CodedNotation == row.PayGradeType_Notation 
					 //&& ( Find( graph.TrainingTask, m.HasTrainingTask )?.Description ?? "" ) == ( row.TrainingTask_Description ?? "" )
				);

				if ( ratingTask == null ) 
				{
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
				//Training Task
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
						result.Messages.Create.Add( "Training Task: " + trainingTask.Description );
					}
					//moved up to here
					else
					{
						Append( referencedItems.TrainingTask, trainingTask );
					}
				}
				//else
				//{
				//	Append( referencedItems.TrainingTask, trainingTask );
				//}
				debug[ latestStepFlag ] = "Got Training Task data for row " + rowCount;

				//Course Source (The Reference Resource for the course, distinct from the Reference Resource for the task in another column)
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
						result.Messages.Create.Add( "Reference Resource for a Course: " + courseSource.Name );
					}
					else
					{
						Append( referencedItems.ReferenceResource, courseSource );
					}
				}
				//else
				//{
				//	Append( referencedItems.ReferenceResource, courseSource );
				//}
				debug[ latestStepFlag ] = "Got Reference Resource data for row " + rowCount;

				//Course
				Course course = null;
				if ( !string.IsNullOrWhiteSpace( row.Course_CodedNotation ) && row.Course_CodedNotation.ToLower() != "n/a" )
				{
					course = graph.Course.FirstOrDefault( m =>
						m.Name == row.Course_Name &&
						m.CodedNotation == row.Course_CodedNotation &&
						assessmentMethodTypes.Select( n => n.Name ).Contains( row.Course_AssessmentMethodType_Label )
					);
					if( course == null )
					{
						course = new Course() 
						{ 
							RowId = Guid.NewGuid(), 
							Name = row.Course_Name, 
							CodedNotation = row.Course_CodedNotation,
							AssessmentMethodType = assessmentMethodTypes.Select( m => m.RowId ).ToList(),
							CourseType = courseType.RowId
						};
						graph.Course.Add( course );
						result.ItemsToBeCreated.Course.Add( course );
						result.Messages.Create.Add( "Course: " + course.CodedNotation + " - " + course.Name );
					}
					else
					{
						Append( referencedItems.Course, course );
					}
				}
				//else
				//{
				//	Append( referencedItems.Course, course );
				//}
				debug[ latestStepFlag ] = "Got Course data for row " + rowCount;

				//Organization
				Organization cca = null;
				if ( !string.IsNullOrWhiteSpace( row.Course_CurriculumControlAuthority_Name ) )
				{
					cca = graph.Organization.FirstOrDefault( m =>
						 m.Name == row.Course_CurriculumControlAuthority_Name
					);
					if( cca == null )
					{
						cca = new Organization() 
						{ 
							RowId = Guid.NewGuid(), 
							Name = row.Course_CurriculumControlAuthority_Name 
						};
						graph.Organization.Add( cca );
						result.ItemsToBeCreated.Organization.Add( cca );
						result.Messages.Create.Add( "Curriculum Control Authority: " + cca.Name );
					}
					else
					{
						Append( referencedItems.Organization, cca );
					}
				}
				//else
				//{
				//	Append( referencedItems.Organization, cca );
				//}
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
						//TODO - UploadedInnerListsForCopiesOfItems is NOT populated
						var tracked = Find( result.UploadedInnerListsForCopiesOfItems.BilletTitle, billetTitle.RowId );
						if( tracked == null )
						{
							tracked = new BilletTitle() { RowId = billetTitle.RowId };
							result.AddedItemsToInnerListsForCopiesOfItems.BilletTitle.Add( tracked );
						}
						tracked.HasRatingTask.Add( ratingTask.RowId );
						result.Messages.AddItem.Add( "Update Billet Title: " + billetTitle.Name + " with new Rating Task: " + ratingTask.Description );
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
				if( course != null )
				{
					if ( Find( existing.Course, course.RowId ) != null )
					{
						//Assign Training Tasks to Course if it doesn't already reference them
						if ( trainingTask != null && !course.HasTrainingTask.Contains( trainingTask.RowId ) )
						{
							//Use a temporary copy of the object to keep track of which items get added to it
							var tracked = Find( result.AddedItemsToInnerListsForCopiesOfItems.Course, course.RowId );
							if ( tracked == null )
							{
								tracked = new Course() { RowId = course.RowId };
								result.AddedItemsToInnerListsForCopiesOfItems.Course.Add( tracked );
							}
							tracked.HasTrainingTask.Add( trainingTask.RowId );
						}

						//Assign CCA to Course if it doesn't already reference them
						if ( cca != null && !course.CurriculumControlAuthority.Contains( cca.RowId ) )
						{
							//Use a temporary copy of the object to keep track of which items get added to it
							var tracked = Find( result.AddedItemsToInnerListsForCopiesOfItems.Course, course.RowId );
							if ( tracked == null )
							{
								tracked = new Course() { RowId = course.RowId };
								result.AddedItemsToInnerListsForCopiesOfItems.Course.Add( tracked );
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
				}
				debug[ latestStepFlag ] = "Processed Existing/New Course data for row " + rowCount;

				//If the ReferenceResource for the task already exists, and needs to be modified, then handle it
				if ( Find( existing.ReferenceResource, taskSource.RowId ) != null )
				{
					//Assign reference types to reference if it doesn't already reference them
					if ( sharedSourceType != null && !taskSource.ReferenceType.Contains( sharedSourceType.RowId ) )
					{
						//Use a temporary copy of the object to keep track of which items get added to it
						var tracked = Find( result.AddedItemsToInnerListsForCopiesOfItems.ReferenceResource, taskSource.RowId );
						if ( tracked == null )
						{
							tracked = new ReferenceResource() { RowId = taskSource.RowId };
							result.AddedItemsToInnerListsForCopiesOfItems.ReferenceResource.Add( tracked );
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
				if( courseSource != null )
				{
					if ( Find( existing.ReferenceResource, courseSource.RowId ) != null )
					{
						//Assign reference types to reference if it doesn't already reference them
						if ( courseSourceType != null && !courseSource.ReferenceType.Contains( courseSourceType.RowId ) )
						{
							//Use a temporary copy of the object to keep track of which items get added to it
							var tracked = Find( result.AddedItemsToInnerListsForCopiesOfItems.ReferenceResource, courseSource.RowId );
							if ( tracked == null )
							{
								tracked = new ReferenceResource() { RowId = courseSource.RowId };
								result.AddedItemsToInnerListsForCopiesOfItems.ReferenceResource.Add( tracked );
							}
							tracked.ReferenceType.Add( courseSourceType.RowId );
						}
					}
					//Otherwise, just treat it like a new ReferenceResource
					else
					{
						Append( courseSource.ReferenceType, courseSourceType.RowId );
					}
				}
				debug[ latestStepFlag ] = "Processed Reference Type for Reference Resource (for Course)";

			}
			if ( result.Messages.Error.Count() > 0 )
			{
				result.Messages.Error.Add( "One or more errors found. Processing cancelled. Please resolve the errors and try again." );
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
					result.Messages.Error.Add( "Error: This task seems to exist already, but a match wasn't able to be found: " + otherRatingTask.Description );
				}
				else
				{
					//Use a temporary copy of the object to keep track of which items get added to it
					var tracked = Find( result.AddedItemsToInnerListsForCopiesOfItems.RatingTask, otherRatingTask.RowId );
					if(tracked == null )
					{
						tracked = new RatingTask() { RowId = otherRatingTask.RowId };
						result.AddedItemsToInnerListsForCopiesOfItems.RatingTask.Add( tracked );
					}

					//Add Rating association
					if ( !otherRatingTask.HasRating.Contains( ratingRowID ) )
					{
						Append( tracked.HasRating, ratingRowID );
						result.Messages.AddItem.Add( "Add Rating association: " + currentRating.Name + " to Rating Task: " + otherRatingTask.Description );
					}

					//Add Work Role association
					foreach ( var roleRowID in match.HasWorkRole )
					{
						if ( !otherRatingTask.HasWorkRole.Contains( roleRowID ) )
						{
							Append( tracked.HasWorkRole, roleRowID );
							result.Messages.AddItem.Add( "Add Functional Area association: " + Find( graph.WorkRole, roleRowID )?.Name + " to Rating Task: " + otherRatingTask.Description );
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
				result.Messages.Create.Add( "Rating Task: " + ratingTask.Description );
			}
			debug[ latestStepFlag ] = "Created change notes for new tasks";

			//Process existing data to see if anything was missing from the uploaded data (attempt to detect deletes)
			FlagItemsForDeletion( existing.BilletTitle, referencedItems.BilletTitle, result.ItemsToBeDeleted.BilletTitle, result.Messages.Delete, "Billet Title", ( BilletTitle m ) => { return m.CTID + " - " + m.Name; } );
			FlagItemsForDeletion( existing.Course, referencedItems.Course, result.ItemsToBeDeleted.Course, result.Messages.Delete, "Course", ( Course m ) => { return m.CTID + " - " + m.Name; } );
			FlagItemsForDeletion( existing.Organization, referencedItems.Organization, result.ItemsToBeDeleted.Organization, result.Messages.Delete, "Organization (CCA)", ( Organization m ) => { return m.CTID + " - " + m.Name; } );
			FlagItemsForDeletion( existing.TrainingTask, referencedItems.TrainingTask, result.ItemsToBeDeleted.TrainingTask, result.Messages.Delete, "Training Task", ( TrainingTask m ) => { return m.CTID + " - " + m.Description; } );
			FlagItemsForDeletion( existing.WorkRole, referencedItems.WorkRole, result.ItemsToBeDeleted.WorkRole, result.Messages.Delete, "Functional Area", ( WorkRole m ) => { return m.CTID + " - " + m.Name; } );
			debug[ latestStepFlag ] = "Flagged normal items for deletion";

			//Only flag the task if it isn't associated with any other rating
			FlagItemsForDeletion( existing.RatingTask.Where( m => m.HasRating.Count() == 1 && m.HasRating.FirstOrDefault() == ratingRowID ).ToList(), referencedItems.RatingTask, result.ItemsToBeDeleted.RatingTask, result.Messages.Delete, "Rating Task", ( RatingTask m ) => { return m.CTID + " - " + m.Description; } );

			//Probably shouldn't ever delete reference resources(?)
			//FlagItemsForDeletion( existing.ReferenceResource, referencedItems.ReferenceResource, result.ItemsToBeDeleted.ReferenceResource, result.ChangeNote, "Reference Resource", ( ReferenceResource m ) => { return m.CTID + " - " + m.Name; } );
			debug[ latestStepFlag ] = "Flagged special items for deletion";

			//Handle items removed from inner lists
			foreach( var originalBilletTitle in existing.BilletTitle )
			{
				var uploadedMatch = Find( result.AddedItemsToInnerListsForCopiesOfItems.BilletTitle, originalBilletTitle.RowId );
				var removalTracker = new BilletTitle() { RowId = originalBilletTitle.RowId };

				if( uploadedMatch != null )
				{
					foreach ( var item in originalBilletTitle.HasRatingTask.Where( m => !uploadedMatch.HasRatingTask.Contains( m ) ).ToList() ) 
					{
						removalTracker.HasRatingTask.Add( item );
						result.Messages.RemoveItem.Add( "Remove Rating Task reference from Billet Title: " + uploadedMatch.Name + " - " + Find( existing.RatingTask, item )?.Description );
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
				var uploadedMatch = Find( result.AddedItemsToInnerListsForCopiesOfItems.Course, originalCourse.RowId );
				var removalTracker = new Course() { RowId = originalCourse.RowId };

				if( uploadedMatch != null )
				{
					foreach( var item in originalCourse.CurriculumControlAuthority.Where( m => !uploadedMatch.CurriculumControlAuthority.Contains( m ) ).ToList() )
					{
						removalTracker.CurriculumControlAuthority.Add( item );
						result.Messages.RemoveItem.Add( "Remove Curriculum Control Authority reference from Course: " + uploadedMatch.Name + " - " + Find( existing.Organization, item )?.Name );
					}

					foreach ( var item in originalCourse.HasTrainingTask.Where( m => !uploadedMatch.HasTrainingTask.Contains( m ) ).ToList() ) 
					{
						removalTracker.HasTrainingTask.Add( item );
						result.Messages.RemoveItem.Add( "Remove Training Task reference from Course: " + uploadedMatch.Name + " - " + Find( existing.TrainingTask, item )?.Description );
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
				var uploadedMatch = Find( result.AddedItemsToInnerListsForCopiesOfItems.RatingTask, originalTask.RowId );
				var removalTracker = new RatingTask() { RowId = originalTask.RowId };
				if( uploadedMatch != null )
				{
					foreach( var item in originalTask.HasWorkRole.Where( m => !uploadedMatch.HasWorkRole.Contains( m ) ).ToList() )
					{
						removalTracker.HasWorkRole.Add( item );
						result.Messages.RemoveItem.Add( "Remove Functional Area reference from Rating Task: " + Find( existing.WorkRole, item )?.Name + " - " + uploadedMatch.Description );
					}
				}

				//Special handling for cases where the Rating Task was removed from this Rating but still exists for other Ratings
				//If the original existing task was not referenced (ie was not in the spreadsheet) and references more than one rating, process it
				if ( Find( referencedItems.RatingTask, originalTask.RowId ) == null && originalTask.HasRating.Count() > 1 ) 
				{
					removalTracker.HasRating.Add( ratingRowID );
					result.Messages.RemoveItem.Add( "Remove Rating reference from Rating Task: " + currentRating.CodedNotation + " - " + originalTask.Description );
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

			//Look for potential duplicates
			HandlePossibleDuplicates( "Billet Title(s)", graph.BilletTitle.GroupBy( m => m.Name, StringComparer.OrdinalIgnoreCase ), existing.BilletTitle, result.ItemsToBeCreated.BilletTitle, result.Messages.Duplicate );
			HandlePossibleDuplicates( "Course(s)", graph.Course.GroupBy( m => m.Name, StringComparer.OrdinalIgnoreCase ), existing.Course, result.ItemsToBeCreated.Course, result.Messages.Duplicate );
			HandlePossibleDuplicates( "Organization(s)", graph.Organization.GroupBy( m => m.Name, StringComparer.OrdinalIgnoreCase ), existing.Organization, result.ItemsToBeCreated.Organization, result.Messages.Duplicate );
			HandlePossibleDuplicates( "Rating Task(s)", graph.RatingTask.GroupBy( m => m.Description, StringComparer.OrdinalIgnoreCase ), existing.RatingTask, result.ItemsToBeCreated.RatingTask, result.Messages.Duplicate );
			HandlePossibleDuplicates( "Reference Resource(s)", graph.ReferenceResource.GroupBy( m => m.Name, StringComparer.OrdinalIgnoreCase ), existing.ReferenceResource, result.ItemsToBeCreated.ReferenceResource, result.Messages.Duplicate );
			HandlePossibleDuplicates( "Training Task(s)", graph.TrainingTask.GroupBy( m => m.Description, StringComparer.OrdinalIgnoreCase ), existing.TrainingTask, result.ItemsToBeCreated.TrainingTask, result.Messages.Duplicate );
			HandlePossibleDuplicates( "Functional Area(s)", graph.WorkRole.GroupBy( m => m.Name, StringComparer.OrdinalIgnoreCase ), existing.WorkRole, result.ItemsToBeCreated.WorkRole, result.Messages.Duplicate );
			debug[ latestStepFlag ] = "Handled possible duplicates";

			debug[ latestStepFlag ] = "Finished processing upload.";
			return result;
		}
		//

		private static void HandlePossibleDuplicates<T>( string typeLabel, IEnumerable<IGrouping<string, T>> groupings, List<T> existingItems, List<T> newItems, List<string> duplicateMessages ) where T : BaseObject
		{
			foreach( var maybeDuplicates in groupings.Where(m => m.Count() > 1 ).ToList() )
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

		private static Concept FindConceptOrError( List<Concept> haystack, Concept needle, string warningLabel, string warningValue, List<string> warningMessages )
		{
			var match = haystack?.FirstOrDefault( m => MatchString( m.Name, needle.Name ) || MatchString( m.CodedNotation, needle.CodedNotation ) || MatchString( m.WorkElementType, needle.WorkElementType ) );
			if( match == null )
			{
				warningMessages.Add( "Error: Found unrecognized " + warningLabel + ": " + warningValue );
				return new Concept() { RowId = Guid.NewGuid(), Name = needle?.Name, CodedNotation = needle?.CodedNotation, WorkElementType = needle?.WorkElementType };
			}
			return match;
		}
		private static List<Concept> FindConceptListOrError( List<Concept> haystack, Concept needle, string warningLabel, string warningValue, List<string> warningMessages )
		{
			var matches = haystack?.Where( m => 
				(needle?.Name ?? "").ToLower().Contains( (m?.Name ?? "" ).ToLower() ) || 
				(needle?.CodedNotation ?? "").ToLower().Contains( (m?.CodedNotation ?? "" ).ToLower() ) || 
				(needle?.WorkElementType ?? "" ).ToLower().Contains(( m?.WorkElementType ?? "" ).ToLower() ) 
			).ToList();
			if( matches.Count() == 0 )
			{
				warningMessages.Add( "Error: Found unrecognized " + warningLabel + ": " + warningValue );
			}
			return matches;
		}
		private static bool MatchString( string haystackCheck, string needleCheck )
		{
			return string.IsNullOrWhiteSpace( needleCheck ) ? false :
				string.IsNullOrWhiteSpace( haystackCheck ) ? false :
				haystackCheck.ToLower() == needleCheck.ToLower();
		}
		//

		private static void FlagItemsForDeletion<T>( List<T> existingItems, List<T> referencedItems, List<T> itemsToBeDeleted, List<string> changeNotes, string typeLabel, Func<T, string> getItemLabel )
		{
			foreach( var item in existingItems.Where( m => !referencedItems.Contains( m ) ).ToList() )
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
				status.AddError( "Error - a current user was not found. You must authenticated and authorized to use this function!" );
				return;
            }
			//now what
			//go thru all non-rating task
			if (summary.ItemsToBeCreated != null)
            {
				if ( summary.ItemsToBeCreated.Organization?.Count > 0 )
                {
					var orgMgr = new OrganizationManager();
					foreach (var item in summary.ItemsToBeCreated.Organization )
                    {
						item.CreatedById = item.LastUpdatedById = user.Id;
						orgMgr.Save( item, user.Id, ref status );
                    }
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
						courseMgr.Save( item,  ref status );
					}
				}
				if ( summary.ItemsToBeCreated.ReferenceResource?.Count > 0 )
				{
					var mgr = new ReferenceResourceManager();
					foreach ( var item in summary.ItemsToBeCreated.ReferenceResource )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref status );
					}
				}

				if ( summary.ItemsToBeCreated.WorkRole?.Count > 0 )
				{
					var mgr = new WorkRoleManager();
					foreach ( var item in summary.ItemsToBeCreated.WorkRole )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref status );
					}
				}

				if ( summary.ItemsToBeCreated.BilletTitle?.Count > 0 )
				{
					var mgr = new JobManager();
					foreach ( var item in summary.ItemsToBeCreated.BilletTitle )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref status );
					}
				}

				if ( summary.ItemsToBeCreated.RatingTask?.Count > 0 )
				{
					var mgr = new RatingTaskManager();
					foreach ( var item in summary.ItemsToBeCreated.RatingTask )
					{
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref status );
					}
				}
			}
		}
		//


		public static ChangeSummary ProcessUpload2( UploadableTable uploadedData, Guid ratingRowID, JObject debug = null )
		{
			var summary = new ChangeSummary();

			var rating = RatingManager.Get( ratingRowID );
			var allRatings = RatingManager.GetAll();
			var payGradeTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_Pay_Grade ).Concepts;
			var trainingGapTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TrainingGap ).Concepts;
			var applicabilityTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TaskApplicability ).Concepts;
			var sourceTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_ReferenceResource ).Concepts;
			var courseTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_CourseType ).Concepts;
			var assessmentMethodTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_CurrentAssessmentApproach ).Concepts;


			var existingReferenceResources = ReferenceResourceManager.GetAll();
			var existingRatingTasks = RatingTaskManager.GetAllForRating( rating.CodedNotation );
			var existingBilletTitles = new List<BilletTitle>(); //Need a method to get these from the database
			var existingTrainingTasks = TrainingTaskManager.GetAll();
			var existingWorkRoles = WorkRoleManager.GetAll();

			//In order to facilitate a correct comparison, it must be done apples-to-apples
			//This approach attempts to achieve that by mapping both the uploaded data and the existing data into a common structure that can then easily be compared


			//Rating Tasks
			//convert the uploaded data
			var uploadedRatingTaskMatchers = GetSheetMatchers<RatingTask, MatchableRatingTask>( uploadedData.Rows, GetRowMatchHelper_RatingTask );
			foreach( var matcher in uploadedRatingTaskMatchers )
			{
				matcher.Flattened.Description = matcher.Rows.Select( m => m.RatingTask_Description ).FirstOrDefault();
				matcher.Flattened.HasRating_CodedNotation = matcher.Rows.Select( m => m.Rating_CodedNotation ).Distinct().ToList();
				matcher.Flattened.HasTrainingTask_Description = matcher.Rows.Select( m => m.TrainingTask_Description ).FirstOrDefault();
				matcher.Flattened.HasReferenceResource_Name = matcher.Rows.Select( m => m.ReferenceResource_Name ).FirstOrDefault();
				matcher.Flattened.HasWorkRole_Name = matcher.Rows.Select( m => m.WorkRole_Name ).Distinct().ToList();
				matcher.Flattened.PayGradeType_CodedNotation = matcher.Rows.Select( m => m.PayGradeType_Notation ).FirstOrDefault();
				matcher.Flattened.ApplicabilityType_Name = matcher.Rows.Select( m => m.RatingTask_ApplicabilityType_Label ).FirstOrDefault();
				matcher.Flattened.TrainingGapType_Name = matcher.Rows.Select( m => m.RatingTask_TrainingGapType_Label ).FirstOrDefault();
				matcher.Flattened.ReferenceType_Name = matcher.Rows.Select( m => m.Shared_ReferenceType ).FirstOrDefault();
			}

			//Convert the existing data
			var existingRatingTaskMatchers = new List<SheetMatcher<RatingTask, MatchableRatingTask>>();
			foreach( var existing in existingRatingTasks )
			{
				var matcher = new SheetMatcher<RatingTask, MatchableRatingTask>() 
				{ 
					Data = existing, 
					Flattened = ( MatchableRatingTask ) existing 
				};

				matcher.Flattened.HasRating_CodedNotation = allRatings.Where( m => existing.HasRating.Contains( m.RowId ) ).Select( m => m.CodedNotation ).ToList();
				matcher.Flattened.HasTrainingTask_Description = existingTrainingTasks.FirstOrDefault( m => m.RowId == existing.HasTrainingTask )?.Description;
				matcher.Flattened.HasReferenceResource_Name = existingReferenceResources.FirstOrDefault( m => m.RowId == existing.HasReferenceResource )?.Name;
				matcher.Flattened.HasWorkRole_Name = existingWorkRoles.Where( m => existing.HasWorkRole.Contains( m.RowId ) ).Select( m => m.Name ).ToList();
				matcher.Flattened.PayGradeType_CodedNotation = payGradeTypeConcepts.FirstOrDefault( m => m.RowId == existing.PayGradeType )?.CodedNotation;
				matcher.Flattened.ApplicabilityType_Name = applicabilityTypeConcepts.FirstOrDefault( m => m.RowId == existing.ApplicabilityType )?.Name;
				matcher.Flattened.TrainingGapType_Name = trainingGapTypeConcepts.FirstOrDefault( m => m.RowId == existing.TrainingGapType )?.Name;
				matcher.Flattened.ReferenceType_Name = sourceTypeConcepts.FirstOrDefault( m => m.RowId == existing.ReferenceType )?.Name;
			}

			//Get loose matches
			var looseMatchingRatingTaskMatchers = new List<SheetMatcher<RatingTask, MatchableRatingTask>>();
			foreach( var uploaded in uploadedRatingTaskMatchers )
			{
				var matches = existingRatingTaskMatchers.Where( n =>
					uploaded.Flattened.HasTrainingTask_Description == n.Flattened.HasTrainingTask_Description &&
					uploaded.Flattened.HasReferenceResource_Name == n.Flattened.HasReferenceResource_Name &&
					uploaded.Flattened.ApplicabilityType_Name == n.Flattened.ApplicabilityType_Name &&
					uploaded.Flattened.TrainingGapType_Name == n.Flattened.TrainingGapType_Name &&
					uploaded.Flattened.ReferenceType_Name == n.Flattened.ReferenceType_Name
				).ToList();
				if( matches.Count() > 0 )
				{
					uploaded.Data = matches.FirstOrDefault()?.Data;
					looseMatchingRatingTaskMatchers.Add( uploaded );
				}
				if( matches.Count() > 1 )
				{
					summary.Messages.Warning.Add( "Found more than one existing match for Rating Task (only the first existing match will be updated): " + string.Join( ", ", matches.Select( m => m.Data?.RowId.ToString() ) ) + " match: " + uploaded.Flattened.Description );
				}
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
			foreach( var item in updatedRatingTaskMatchers )
			{
				var existingMatcher = existingRatingTaskMatchers.FirstOrDefault( m => m.Data == item.Data );

				//Handle added items
				var newRatingCodes = item.Flattened.HasRating_CodedNotation.Except( existingMatcher.Flattened.HasRating_CodedNotation ).ToList();
				var newWorkRoles = item.Flattened.HasWorkRole_Name.Except( existingMatcher.Flattened.HasWorkRole_Name ).ToList();
				var innerItemsAdditions = new RatingTask()
				{
					RowId = existingMatcher.Data.RowId,
					HasRating = allRatings.Where( m => newRatingCodes.Contains( m.CodedNotation ) ).Select( m => m.RowId ).ToList()//,


					//Fix me
					//HasWorkRole = existingOrNewWorkRoles.Where( m => newWorkRoles.Contains( m.Name ) ).Select( m => m.RowId ).ToList()

				};
				if( innerItemsAdditions.HasRating.Count() > 0 || innerItemsAdditions.HasWorkRole.Count() > 0 )
				{
					summary.AddedItemsToInnerListsForCopiesOfItems.RatingTask.Add( innerItemsAdditions );
				}

				//No deletes based on RatingTask input data
			}

			//Items that were not found in the existing data (brand new items)
			var newRatingTaskMatchers = uploadedRatingTaskMatchers.Where( m => !looseMatchingRatingTaskMatchers.Contains( m ) ).ToList();
			foreach( var item in newRatingTaskMatchers )
			{
				var newItem = new RatingTask()
				{
					RowId = Guid.NewGuid(),
					CTID = "ce-" + Guid.NewGuid().ToString().ToLower(),
					Description = item.Flattened.Description,
					ApplicabilityType = FindConceptOrError( applicabilityTypeConcepts, new Concept() { Name = item.Flattened.ApplicabilityType_Name }, "Applicability Type", item.Flattened.ApplicabilityType_Name, summary.Messages.Error ).RowId,
					TrainingGapType = FindConceptOrError( trainingGapTypeConcepts, new Concept() { Name = item.Flattened.TrainingGapType_Name }, "Training Gap Type", item.Flattened.TrainingGapType_Name, summary.Messages.Error ).RowId,
					PayGradeType = FindConceptOrError( payGradeTypeConcepts, new Concept() { CodedNotation = item.Flattened.PayGradeType_CodedNotation }, "Pay Grade Type", item.Flattened.PayGradeType_CodedNotation, summary.Messages.Error ).RowId,
					ReferenceType = FindConceptOrError( sourceTypeConcepts, new Concept() { CodedNotation = item.Flattened.ReferenceType_Name }, "Reference Resource Type", item.Flattened.PayGradeType_CodedNotation, summary.Messages.Error ).RowId,
					HasRating = new List<Guid>() { rating.RowId },


					//Fix me
					//HasWorkRole = existingOrNewWorkRoles.Where( m => item.Flattened.HasWorkRole_Name.Contains( m.Name ) ).Select( m => m.RowId ).ToList(),
					//HasReferenceResource = FindGuidOrEmpty( existingOrNewReferenceResources, m => item.Flattened.HasReferenceResource_Name == m.Name ),
					//HasTrainingTask = FindGuidOrEmpty( existingOrNewTrainingTasks, m => item.Flattened.HasTrainingTask_Description == m.Description ),
					Note = item.Flattened.Note
				};
				summary.ItemsToBeCreated.RatingTask.Add( newItem );
			}

			//Items that were not found in the uploaded data (deleted items)
			var missingRatingTaskMatchers = existingRatingTaskMatchers.Where( m => !looseMatchingRatingTaskMatchers.Contains( m ) ).ToList();
			foreach( var item in missingRatingTaskMatchers )
			{
				summary.ItemsToBeDeleted.RatingTask.Add( item.Data );
			}

			

			/*
			var uploadedBilletTitleMatchers = GetSheetMatchers<BilletTitle>( uploadedData.Rows, GetRowMatchHelper_BilletTitle );

			var uploadedOrganizationMatchers = GetSheetMatchers<Organization>( uploadedData.Rows, GetRowMatchHelper_Organization );

			var uploadedTaskReferenceResourceMatchers = GetSheetMatchers<ReferenceResource>( uploadedData.Rows, GetRowMatchHelper_ReferenceResource_Task );

			var uploadedCourseReferenceResourceMatchers = GetSheetMatchers<ReferenceResource>( uploadedData.Rows, GetRowMatchHelper_ReferenceResource_Course );

			var uploadedWorkRoleMatchers = GetSheetMatchers<WorkRole>( uploadedData.Rows, GetRowMatchHelper_WorkRole );

			var uploadedCourseMatchers = GetSheetMatchers<Course>( uploadedData.Rows, GetRowMatchHelper_Course );

			var uploadedTrainingTaskMatchers = GetSheetMatchers<TrainingTask>( uploadedData.Rows, GetRowMatchHelper_TrainingTask );
			*/

			return summary;
		}
		private static Guid FindGuidOrEmpty<T>( List<T> haystack, Func<T, bool> findBy ) where T : BaseObject
		{
			var match = haystack.FirstOrDefault( m => findBy( m ) );
			return match == null ? Guid.Empty : match.RowId;
		}
		private static bool AllMatch<T>( List<T> list1, List<T> list2 )
		{
			return list1.Except( list2 ).Count() == 0 && list2.Except( list1 ).Count() == 0;
		}
		private static List<UploadableRow> FindRowsInRows( List<UploadableRow> findRows, List<UploadableRow> inRows, Func<UploadableRow, string> matchFunction )
		{
			return findRows.Where( m => inRows
				.Select( n => matchFunction( n ) )
				.Contains( matchFunction( m ) )
			).ToList();
		}
		private static List<SheetMatcher<T1, T2>> GetSheetMatchers<T1, T2>( List<UploadableRow> rows, Func<UploadableRow, string> matchFunction ) where T1: new() where T2: new()
		{
			var keys = rows.GroupBy( m => matchFunction( m ) ).Select( m => m.Key ).Distinct().ToList();
			return keys.Select( m => new SheetMatcher<T1, T2>()
			{
				Flattened = new T2(),
				Rows = rows.Where( n => matchFunction( n ) == m ).ToList()
			} ).ToList();
		}
		private static string GetMergedRowMatchHelper( List<string> items )
		{
			var merged = items?.Where( m => !string.IsNullOrWhiteSpace( m ) ).ToList();
			return merged.Count() == 0 ? "" : string.Join( "__", merged );
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
				row.Course_HasReferenceResource_Name, 
				row.Course_CodedNotation, 
				row.Course_Name, 
				row.Course_CourseType_Label, 
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
				row.PayGradeType_Notation,
				row.ReferenceResource_Name,
				row.ReferenceResource_PublicationDate,
				row.Shared_ReferenceType,
				row.RatingTask_Description,
				row.RatingTask_ApplicabilityType_Label,
				row.RatingTask_TrainingGapType_Label
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
				row.Course_HasReferenceResource_Name
			} );
		}
		private static string GetRowMatchHelper_TrainingTask( UploadableRow row )
		{
			return GetMergedRowMatchHelper( new List<string>()
			{
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

	}
}
