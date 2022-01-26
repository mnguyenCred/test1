using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Navy.Utilities;

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
        #region ProcessUpload V1
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
			if ( uploaded.Rows.Count == 0)
			{
				result.Messages.Error.Add( "Error: No rows were found to process." );
				return result;
			}
			var alternateRating = new Rating();
			var alternateRatingCode = "";
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
			existing.TrainingTask = Factories.TrainingTaskManager.GetAll();
			//should not get all rating task once have many rmtls (thousandds
			int totalRows = 0;
			existing.RatingTask = Factories.RatingTaskManager.GetAllForRating( currentRating.CodedNotation , true, ref totalRows);
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
			Guid currentRatingRowID = ratingRowID;
			//For each row...
			var rowCount = 0;
			bool foundDifferentRating = false; 
			foreach( var row in uploaded.Rows )
			{
				debug[ "Current Row" ] = (rowCount + 1);
				rowCount++;
				if ( rowCount  ==501)
                {
					//break;
                }
				if ( row.Rating_CodedNotation == null )
					continue;

				//check for All 
				row.Rating_CodedNotation = NullifyNotApplicable( row.Rating_CodedNotation );
				if ( row.Rating_CodedNotation != currentRating.CodedNotation)
                {
					if ( alternateRatingCode == row.Rating_CodedNotation )
						continue;
					alternateRatingCode = row.Rating_CodedNotation;
					alternateRating = Factories.RatingManager.GetByCode( row.Rating_CodedNotation );
					currentRatingRowID = alternateRating.RowId;
                    //should be All
                    //make configurable, but could ignore
                    //produce a message 
                    if ( !foundDifferentRating )
                    {
                        //actually adding an error will stop process
                        result.Messages.Warning.Add( String.Format( "While processing: '{0}' a different rating was found: '{1}'. Only one rating may uploaded at a time. All other ratings will be ignored.", currentRating.CodedNotation, row.Rating_CodedNotation ) );
                    }
                    foundDifferentRating =true;
					//break;
					continue;
				} else
                {
					//assuming only 2
                }

				//how to handle identifier
				//this would be coded notation for the rating task, as well as the row. So second upload, the related training task can be accessed by an id
				row.Row_CodedNotation = NullifyNotApplicable( row.Row_CodedNotation );
				row.Row_Identifier = NullifyNotApplicable( row.Row_Identifier );
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
				//this can be multiple now - check
				row.Course_AssessmentMethodType_Label = NullifyNotApplicable( row.Course_AssessmentMethodType_Label );

				row.ReferenceResource_PublicationDate = NullifyNotApplicable( row.ReferenceResource_PublicationDate );
				if ( BaseFactory.IsValidDate( row.ReferenceResource_PublicationDate ) )
				{
					//normalize - may want to make this an app key to allow flexiblig
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
						HasRating = currentRatingRowID
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
				//not matching existing?
				//if have an identifier, use it
				//why use the Note in the check?
				RatingTask ratingTask = null;
				if (!string.IsNullOrEmpty( row.Row_CodedNotation))
                {
					ratingTask = graph.RatingTask.FirstOrDefault( m =>
						m.CodedNotation?.ToLower() == row.Row_CodedNotation?.ToLower() );
					//make sure found
					if ( string.IsNullOrWhiteSpace(ratingTask?.CodedNotation) )
						ratingTask = null;
				} else if ( !string.IsNullOrEmpty( row.Row_Identifier ) )
				{
					ratingTask = graph.RatingTask.FirstOrDefault( m =>
						m.RowId.ToString() == row.Row_Identifier );
					//make sure found
					if ( string.IsNullOrWhiteSpace( ratingTask?.CodedNotation ) )
						ratingTask = null;
				}

				if ( ratingTask == null )
				{

					ratingTask = graph.RatingTask.FirstOrDefault( m =>
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
				}

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
						CodedNotation = row.Row_CodedNotation,
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
					//needs to check for valid data
					Append( referencedItems.RatingTask, ratingTask );
				}
				debug[ latestStepFlag ] = "Got Rating Task data for row " + rowCount;

				//The last few may or may not be present in some rows, hence the extra special handling for nulls
				//Training Task
				TrainingTask trainingTask = null;
				//if the same row as before, ...
				if ( !string.IsNullOrWhiteSpace( row.TrainingTask_Description ) )
				{
					if ( BaseFactory.IsValidGuid( ratingTask.HasTrainingTask ) )
					{
						trainingTask = graph.TrainingTask.FirstOrDefault( m => m.RowId == ratingTask.HasTrainingTask );
					}

					if ( trainingTask == null )
					{
						//risky
						//should be able to get the training task that was originally connected to the rating task. 
						trainingTask = graph.TrainingTask.FirstOrDefault( m =>
							m.Description == row.TrainingTask_Description
						);
					}
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
					//CodedNotation should be unique? Don't want to get caught up with a name change
					course = graph.Course.FirstOrDefault( m =>
						m.CodedNotation.ToLower() == row.Course_CodedNotation.ToLower()
					);

					if ( course == null )
					{
						course = graph.Course.FirstOrDefault( m =>
								m.Name == row.Course_Name
								//&& m.CodedNotation == row.Course_CodedNotation
								//&& assessmentMethodTypes?.Name == row.Course_AssessmentMethodType_Label
							);
					}
					if ( course == null )
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
					//should ignore case here. Note that current data has a coded notation for the org name. 
					cca = graph.Organization.FirstOrDefault( m =>
						 m.Name.ToLower() == row.Course_CurriculumControlAuthority_Name.ToLower()
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
						var tracked = Find( result.AddedItemsToInnerListsForCopiesOfItems.BilletTitle, billetTitle.RowId );
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
					Append( ratingTask.HasRating, currentRatingRowID );
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
					if ( !otherRatingTask.HasRating.Contains( currentRatingRowID ) )
					{
						Append( tracked.HasRating, currentRatingRowID );
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
			FlagItemsForDeletion( existing.RatingTask.Where( m => m.HasRating.Count() == 1 && m.HasRating.FirstOrDefault() == currentRatingRowID ).ToList(), referencedItems.RatingTask, result.ItemsToBeDeleted.RatingTask, result.Messages.Delete, "Rating Task", ( RatingTask m ) => { return m.CTID + " - " + m.Description; } );

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
					removalTracker.HasRating.Add( currentRatingRowID );
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
				(needle?.Name ?? "").ToLower().Contains( (m?.Name ?? "" ).ToLower() ) 
				|| ( needle.CodedNotation != null && ( needle?.CodedNotation ?? "").ToLower().Contains( (m?.CodedNotation ?? "" ).ToLower()) ) 
				|| ( needle.WorkElementType != null && ( needle?.WorkElementType ?? "" ).ToLower().Contains(( m?.WorkElementType ?? "" ).ToLower()) ) 
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
        #endregion
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

				if ( summary.ItemsToBeCreated.RatingTask?.Count > 0 )
				{
					var mgr = new RatingTaskManager();
					int cntr = 0;
					foreach ( var item in summary.ItemsToBeCreated.RatingTask )
					{
						cntr++;
						//get all billets for this task
						if ( item.HasBillet == null )
							item.HasBillet = new List<Guid>();
						if ( summary.ItemsToBeCreated.BilletTitle?.Count > 0 )
						{
							//get billets that reference this task
							var results = summary.ItemsToBeCreated.BilletTitle.Where( p => p.HasRatingTask.Contains(item.RowId) ).ToList();

							if ( results.Count > 0 )
							{
								item.HasBillet.AddRange( results.Select( p => p.RowId ) );
							} else
                            {
								var resultsByCode = summary.ItemsToBeCreated.BilletTitle.Where( p => p.HasRatingTaskByCode.Contains( item.CodedNotation ) ).ToList();
								if( resultsByCode.Count > 0 )
								{
									//this will always be one (one per row). Alternate would be to do a set based approach after all rating tasks are created. 
									item.HasBillet.AddRange( resultsByCode.Select( p => p.RowId ) );
								}
							}
						}
						item.CreatedById = item.LastUpdatedById = user.Id;
						mgr.Save( item, ref summary );
					}
				}
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
							item.HasBillet.AddRange( results.Select( p => p.RowId ) );
						}
						//do we need to check the created as well?
						if ( summary.ItemsToBeCreated.BilletTitle?.Count > 0 )
						{
							//get billets that reference this task
							var results = summary.ItemsToBeCreated.BilletTitle.Where( p => p.HasRatingTask.Contains( item.RowId ) ).ToList();
							item.HasBillet.AddRange( results.Select( p => p.RowId ) );
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

			//Filter out rows that don't match the selected rating
			var nonMatchingRows = uploadedData.Rows.Where( m => m.Rating_CodedNotation?.ToLower() != currentRating.CodedNotation.ToLower() ).ToList();
			uploadedData.Rows = uploadedData.Rows.Where( m => !nonMatchingRows.Contains( m ) ).ToList();
			if( nonMatchingRows.Count() > 0 )
			{
				var nonMatchingCodes = nonMatchingRows.Select( m => m.Rating_CodedNotation ).Distinct().ToList();
				foreach( var code in nonMatchingCodes )
				{
					summary.Messages.Warning.Add( "Detected " + nonMatchingRows.Where(m => m.Rating_CodedNotation == code).Count() + " rows that did not match the selected Rating (" + currentRating.CodedNotation + ") and instead were for Rating: \"" + code + "\". These rows have been ignored.");
				}
			}
			
			//Valiate row count
			if ( uploadedData.Rows.Count == 0 )
			{
				summary.Messages.Error.Add( "Error: No rows were found to process." );
				return summary;
			}

			//Get existing data
			var existingRatings = RatingManager.GetAll();
			var payGradeTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_Pay_Grade ).Concepts;
			var trainingGapTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TrainingGap ).Concepts;
			var applicabilityTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TaskApplicability ).Concepts;
			var sourceTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_ReferenceResource ).Concepts;
			var courseTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_CourseType ).Concepts;
			var assessmentMethodTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_CurrentAssessmentApproach ).Concepts;
			debug[ latestStepFlag ] = "Got Concept Scheme data";

			var existingReferenceResources = ReferenceResourceManager.GetAll();
			var existingRatingTasks = Factories.RatingTaskManager.GetAllForRating( currentRating.CodedNotation, true, ref totalRows );
			var existingBilletTitles = Factories.JobManager.GetAll();
			var existingTrainingTasks = TrainingTaskManager.GetAll();
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
				matcher.Flattened.Name = matcher.Rows.Select( m => m.Course_HasReferenceResource_Name ).FirstOrDefault();
			}

			//Remove empty rows
			uploadedReferenceResourceMatchers_Task = uploadedReferenceResourceMatchers_Task.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.Name ) ).ToList();
			uploadedReferenceResourceMatchers_Course = uploadedReferenceResourceMatchers_Course.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.Name ) ).ToList();

			//Convert the existing data
			var existingReferenceResourceMatchers = GetSheetMatchersFromExisting<ReferenceResource, MatchableReferenceResource>( existingReferenceResources );
			foreach( var matcher in existingReferenceResourceMatchers )
			{
				matcher.Flattened.ReferenceType_WorkElementType = sourceTypeConcepts.Where( m => matcher.Data.ReferenceType.Contains( m.RowId ) ).Select( m => m.WorkElementType ).ToList();
			}

			//Get loose matches
			var looseMatchReferenceResourceMatchers_Task = new List<SheetMatcher<ReferenceResource, MatchableReferenceResource>>();
			foreach( var uploaded in uploadedReferenceResourceMatchers_Task )
			{
				var matches = existingReferenceResourceMatchers.Where( m =>
					uploaded.Flattened.Name == m.Flattened.Name &&
					ParseDateOrEmpty( uploaded.Flattened.PublicationDate ) == ParseDateOrEmpty( m.Flattened.PublicationDate ) 
				).ToList();

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
					uploaded.Flattened.Description == m.Flattened.Description
				).ToList();

				HandleLooseMatchesFound( matches, uploaded, looseMatchingTrainingTaskMatchers, summary, "Training Task", m => m?.Description );
			}

			//Items that are completely identical (unchanged)
			var identicalTrainingTaskMatchers = looseMatchingTrainingTaskMatchers.Where( m =>
				 existingTrainingTaskMatchers.Where( n =>
					 m.Flattened.Description == n.Flattened.Description
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
				matcher.Flattened.HasReferenceResource_Name = matcher.Rows.Select( m => m.Course_HasReferenceResource_Name ).FirstOrDefault();
				matcher.Flattened.CourseType_Name = matcher.Rows.Select( m => m.Course_CourseType_Label ).FirstOrDefault();
				matcher.Flattened.CurriculumControlAuthority_Name = matcher.Rows.Select( m => m.Course_CurriculumControlAuthority_Name ).Distinct().ToList();
				matcher.Flattened.HasTrainingTask_Description = matcher.Rows.Select( m => m.TrainingTask_Description ).Distinct().ToList();
				matcher.Flattened.AssessmentMethodType_Name = matcher.Rows.Select( m => m.Course_AssessmentMethodType_Label ).Distinct().ToList();
			}

			//Remove empty rows
			uploadedCourseMatchers = uploadedCourseMatchers.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.Name ) ).ToList();

			//Convert the existing data
			var existingCourseMatchers = GetSheetMatchersFromExisting<Course, MatchableCourse>( existingCourses );
			foreach( var matcher in existingCourseMatchers )
			{
				matcher.Flattened.HasReferenceResource_Name = existingReferenceResources.Where( m => matcher.Data.HasReferenceResource == m.RowId ).Select( m => m.Name ).FirstOrDefault();
				matcher.Flattened.CourseType_Name = courseTypeConcepts.Where( m => matcher.Data.CourseType == m.RowId ).Select( m => m.Name ).FirstOrDefault();
				matcher.Flattened.CurriculumControlAuthority_Name = existingOrganizations.Where( m => matcher.Data.CurriculumControlAuthority.Contains( m.RowId ) ).Select( m => m.Name ).ToList();
				matcher.Flattened.HasTrainingTask_Description = existingTrainingTasks.Where( m => matcher.Data.HasTrainingTask.Contains( m.RowId ) ).Select( m => m.Description ).ToList();
				matcher.Flattened.AssessmentMethodType_Name = assessmentMethodTypeConcepts.Where( m => matcher.Data.AssessmentMethodType.Contains( m.RowId ) ).Select( m => m.Name ).ToList();
			}

			//Get loose matches
			var looseMatchingCourseMatchers = new List<SheetMatcher<Course, MatchableCourse>>();
			foreach ( var uploaded in uploadedCourseMatchers )
			{
				var matches = existingCourseMatchers.Where( m =>
					uploaded.Flattened.Name == m.Flattened.Name &&
					uploaded.Flattened.CodedNotation == m.Flattened.CodedNotation &&
					uploaded.Flattened.HasReferenceResource_Name == m.Flattened.HasReferenceResource_Name &&
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
						m.AssessmentMethodType = newAssessmentMethods.Select( n => n.RowId ).ToList();
					},
					m => m.CurriculumControlAuthority.Count() > 0 || m.HasTrainingTask.Count() > 0 || m.AssessmentMethodType.Count() > 0,
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
						m.AssessmentMethodType = removedAssessmentMethods.Select( n => n.RowId ).ToList();
					},
					m => m.CurriculumControlAuthority.Count() > 0 || m.HasTrainingTask.Count() > 0 || m.AssessmentMethodType.Count() > 0,
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
					m.HasReferenceResource = existingReferenceResources.Concat( summary.ItemsToBeCreated.ReferenceResource ).Where( n => item.Flattened.HasReferenceResource_Name == n.Name ).Select( n => n.RowId ).FirstOrDefault();
					m.CourseType = courseTypeConcepts.Where( n => item.Flattened.CourseType_Name == n.Name ).Select( n => n.RowId ).FirstOrDefault();
					m.CurriculumControlAuthority = existingOrganizations.Concat( summary.ItemsToBeCreated.Organization ).Where( n => item.Flattened.CurriculumControlAuthority_Name.Contains( n.Name ) ).Select( n => n.RowId ).ToList();
					m.HasTrainingTask = existingTrainingTasks.Concat( summary.ItemsToBeCreated.TrainingTask ).Where( n => item.Flattened.HasTrainingTask_Description.Contains( n.Description ) ).Select( n => n.RowId ).ToList();
					m.AssessmentMethodType = assessmentMethodTypeConcepts.Where( n => item.Flattened.AssessmentMethodType_Name.Contains( n.Name ) ).Select( n => n.RowId ).ToList();
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
				//this should equate to the RowId - ideally. But only if done at the beginning.
				matcher.Flattened.HasIdentifier = matcher.Rows.Select( m => m.Row_Identifier ).FirstOrDefault();
				matcher.Flattened.HasRating_CodedNotation = matcher.Rows.Select( m => m.Rating_CodedNotation ).Distinct().ToList();
				//
				matcher.Flattened.HasBilletTitle_Name = matcher.Rows.Select( m => m.BilletTitle_Name ).FirstOrDefault();
				//
				matcher.Flattened.HasTrainingTask_Description = matcher.Rows.Select( m => m.TrainingTask_Description ).FirstOrDefault();
				matcher.Flattened.HasReferenceResource_Name = matcher.Rows.Select( m => m.ReferenceResource_Name ).FirstOrDefault();
				matcher.Flattened.HasWorkRole_Name = matcher.Rows.Select( m => m.WorkRole_Name ).Distinct().ToList();
				matcher.Flattened.PayGradeType_CodedNotation = matcher.Rows.Select( m => m.PayGradeType_Notation ).FirstOrDefault();
				matcher.Flattened.ApplicabilityType_Name = matcher.Rows.Select( m => m.RatingTask_ApplicabilityType_Label ).FirstOrDefault();
				matcher.Flattened.TrainingGapType_Name = matcher.Rows.Select( m => m.RatingTask_TrainingGapType_Label ).FirstOrDefault();
				matcher.Flattened.ReferenceType_WorkElementType = matcher.Rows.Select( m => m.Shared_ReferenceType ).FirstOrDefault();
				matcher.Flattened.HasReferenceResource_PublicationDate = matcher.Rows.Select( m => m.ReferenceResource_PublicationDate ).FirstOrDefault();
			}

			//Remove empty rows
			uploadedRatingTaskMatchers = uploadedRatingTaskMatchers.Where( m => !string.IsNullOrWhiteSpace( m.Flattened.Description ) ).ToList();

			//Convert the existing data
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
					m.HasBillet = item.Flattened.HasBillet;
					m.ApplicabilityType = FindConceptOrError( applicabilityTypeConcepts, new Concept() { Name = item.Flattened.ApplicabilityType_Name }, "Applicability Type", item.Flattened.ApplicabilityType_Name, summary.Messages.Error ).RowId;
					m.TrainingGapType = FindConceptOrError( trainingGapTypeConcepts, new Concept() { Name = item.Flattened.TrainingGapType_Name }, "Training Gap Type", item.Flattened.TrainingGapType_Name, summary.Messages.Error ).RowId;
					m.PayGradeType = FindConceptOrError( payGradeTypeConcepts, new Concept() { CodedNotation = item.Flattened.PayGradeType_CodedNotation }, "Pay Grade Type", item.Flattened.PayGradeType_CodedNotation, summary.Messages.Error ).RowId;
					m.ReferenceType = FindConceptOrError( sourceTypeConcepts, new Concept() { WorkElementType = item.Flattened.ReferenceType_WorkElementType }, "Reference Resource Type", item.Flattened.ReferenceType_WorkElementType, summary.Messages.Error ).RowId;
					m.HasRating = new List<Guid>() { currentRating.RowId };
					m.HasWorkRole = existingWorkRoles.Concat( summary.ItemsToBeCreated.WorkRole ).Where( n => item.Flattened.HasWorkRole_Name.Contains( n.Name ) ).Select( n => n.RowId ).ToList();
					m.HasReferenceResource = existingReferenceResources.Concat( summary.ItemsToBeCreated.ReferenceResource ).Where( n =>
						item.Flattened.HasReferenceResource_Name == n.Name &&
						ParseDateOrEmpty( item.Flattened.HasReferenceResource_PublicationDate ) == ParseDateOrEmpty( n.PublicationDate ) &&
						sourceTypeConcepts.Where( o => n.ReferenceType.Contains( o.RowId ) ).Select( o => o.WorkElementType ).ToList().Contains( item.Flattened.ReferenceType_WorkElementType )
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
			foreach( var existing in existingItems )
			{
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
    }
}
