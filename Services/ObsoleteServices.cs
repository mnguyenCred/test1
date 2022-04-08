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
	public class ObsoleteServices
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
				row.Course_CourseType_Name = NullifyNotApplicable( row.Course_CourseType_Name );
				row.Course_CodedNotation = NullifyNotApplicable( row.Course_CodedNotation );
				row.Course_Name = NullifyNotApplicable( row.Course_Name );
				row.Course_CourseType_Name = NullifyNotApplicable( row.Course_CourseType_Name );
				row.Course_CurriculumControlAuthority_Name = NullifyNotApplicable( row.Course_CurriculumControlAuthority_Name );
				row.Course_LifeCycleControlDocumentType_CodedNotation = NullifyNotApplicable( row.Course_LifeCycleControlDocumentType_CodedNotation );
				//this can be multiple now - check
				row.Course_AssessmentMethodType_Name = NullifyNotApplicable( row.Course_AssessmentMethodType_Name );

				row.ReferenceResource_PublicationDate = NullifyNotApplicable( row.ReferenceResource_PublicationDate );
				//if ( BaseFactory.IsValidDate( row.ReferenceResource_PublicationDate ) )
				//{
				//	//normalize - may want to make this an app key to allow flexiblig
				//	row.ReferenceResource_PublicationDate = DateTime.Parse( row.ReferenceResource_PublicationDate ).ToString( "yyyy-MM-dd" );
				//}

				//Concepts from Concept Schemes
				var payGradeType = FindConceptOrError( payGradeTypeConcepts, new Concept() { CodedNotation = row.PayGradeType_CodedNotation }, "Pay Grade (Rank)", row.PayGradeType_CodedNotation, result.Messages.Error );
				var trainingGapType = FindConceptOrError( trainingGapTypeConcepts, new Concept() { Name = row.RatingTask_TrainingGapType_Name }, "Training Gap Type", row.RatingTask_TrainingGapType_Name, result.Messages.Error );
				var applicabilityType = FindConceptOrError( applicabilityTypeConcepts, new Concept() { Name = row.RatingTask_ApplicabilityType_Name }, "Applicability Type", row.RatingTask_ApplicabilityType_Name, result.Messages.Error );
				var sharedSourceType = FindConceptOrError( sourceTypeConcepts, new Concept() { WorkElementType = row.Shared_ReferenceType }, "Reference Resource Type (for Rating-Level Task)", row.Shared_ReferenceType, result.Messages.Error );
				//var courseSourceType = FindConceptOrError( sourceTypeConcepts, row.Course_HasReferenceResource_Name, false, "Reference Resource Type (for Course)", result.Errors );
				var courseSourceType = sourceTypeConcepts.FirstOrDefault( m => m.CodedNotation == "LCCD" ); //Course Reference Resource Type is always a Life-Cycle Control Document
				var courseType = string.IsNullOrWhiteSpace( row.Course_CourseType_Name ) ?
					null : //It's okay if this is null, since only some rows have course types
					FindConceptOrError( courseTypeConcepts, new Concept() { Name = row.Course_CourseType_Name }, "Course Type", row.Course_CourseType_Name, result.Messages.Error );
				var assessmentMethodTypes = string.IsNullOrWhiteSpace( row.Course_AssessmentMethodType_Name ) ? 
					null : //It's okay if this is null, since only some rows have assessment methods
					FindConceptListOrError( assessmentMethodTypeConcepts, new Concept() { Name = row.Course_AssessmentMethodType_Name }, "Assessment Method Type", row.Course_AssessmentMethodType_Name, result.Messages.Error );
				

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
					//if ( BaseFactory.IsValidDate( taskSource.PublicationDate ) )
					//{
					//	//normalize
					//	taskSource.PublicationDate = DateTime.Parse( taskSource.PublicationDate ).ToString( "yyyy-MM-dd" );
					//}
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
						&& trainingGapType?.Name == row.RatingTask_TrainingGapType_Name
						&& applicabilityType?.Name == row.RatingTask_ApplicabilityType_Name
						&& payGradeType?.CodedNotation == row.PayGradeType_CodedNotation
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
					 && trainingGapType?.Name == row.RatingTask_TrainingGapType_Name 
					 && applicabilityType?.Name == row.RatingTask_ApplicabilityType_Name 
					 && payGradeType?.CodedNotation == row.PayGradeType_CodedNotation 
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
				if ( !string.IsNullOrWhiteSpace( row.Course_LifeCycleControlDocumentType_CodedNotation ) )
				{
					courseSource = graph.ReferenceResource.FirstOrDefault( m =>
						m.CodedNotation == row.Course_LifeCycleControlDocumentType_CodedNotation
					);
					if( courseSource == null )
					{
						courseSource = new ReferenceResource() 
						{ 
							RowId = Guid.NewGuid(), 
							CodedNotation = row.Course_LifeCycleControlDocumentType_CodedNotation
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
								//&& assessmentMethodTypes?.Name == row.Course_AssessmentMethodType_Name
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
							CourseType = new List<Guid>() { courseType.RowId }
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
		
        #endregion
	
	
		//
		public static ChangeSummary ProcessUploadV3( UploadableTable uploadedData, Guid ratingRowID, JObject debug = null )
		{
			//Hold data
			debug = debug ?? new JObject();
			var latestStepFlag = "FarthestStep";
			var summary = new ChangeSummary() { RowId = Guid.NewGuid() };
			int totalRows = 0;
			debug[latestStepFlag] = "Initial Setup";

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
				LoggingHelper.WriteLogFile( 1, string.Format( "Rating_upload_{0}_{1}.csv", currentRating.Name.Replace( " ", "_" ), DateTime.Now.ToString( "hhmmss" ) ), uploadedData.RawCSV, "", false );

				new BaseFactory().BulkLoadRMTL( currentRating.CodedNotation, uploadedData.RawCSV );
			}


			foreach( var row in uploadedData.Rows )
            {
				//look up rating task 
				//row.Row_CodedNotation
            };
			return summary;
		}
		//
	
	}
	
}