using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;

using Models.Application;
using Models.Curation;
using Models.Import;
using Models.Schema;
using Models.Search;

using Navy.Utilities;

using AppEntity = Models.Schema.RatingTask;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.RatingTask;
using EntitySummary = Models.Schema.RatingTaskSummary;
using ViewContext = Data.Views.ceNavyViewEntities;

namespace Factories
{
	public class RatingTaskManager : BaseFactory
	{
		public static new string thisClassName = "RatingTaskManager";
		public static string cacheKey = "RatingTaskCache";
		public static string cacheKeySummary = "RatingTaskSummaryCache";

		#region === persistance ==================
		public static void SaveFromUpload( AppEntity entity, int userID, ChangeSummary summary )
		{
			SaveCore( entity, userID, "Upload", summary.AddError );
		}
		//

		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			SaveCore( entity, userID, "Edit", errors.Add );
		}
		//

		private static void SaveCore( AppEntity entity, int userID, string saveType, Action<string> AddErrorMethod )
		{
			using ( var context = new DataEntities() )
			{
				BasicSaveCore( context, entity, context.RatingTask, userID, ( ent, dbEnt ) => {
					dbEnt.ReferenceResourceId = context.ReferenceResource.FirstOrDefault( m => m.RowId == ent.HasReferenceResource )?.Id ?? 0;
					dbEnt.ReferenceTypeId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.ReferenceType )?.Id ?? 0;
				}, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//

		/// <summary>
		/// Save a RatingTask
		/// 22-03-29 mp - now (well some day) there can be multiple training tasks for an RT
		/// </summary>
		/// <param name="input"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Save( AppEntity input, ref ChangeSummary status, bool fromUpload = true )
		{
			bool isValid = true;
			int count = 0;
			try
			{
				using ( var context = new DataEntities() )
				{
					//if ( ValidateProfile( entity, ref status ) == false )
					//	return false;
					//look up if no id
					if ( input.Id == 0 )
					{
						//need to identify for sure what is unique
						//use codedNotation first if present
						var record = GetFromView( input, status.RatingCodedNotation );
						if ( record?.Id > 0 )
						{
							//
							input.Id = record.Id;
							UpdateParts( input, status, fromUpload );
							//??
							return true;
						}
						else
						{
							//add
							int newId = Add( input, ref status, fromUpload );
							if ( newId == 0 || status.HasSectionErrors )
								isValid = false;
						}
					}
					else
					{
						//TODO - consider if necessary, or interferes with anything
						context.Configuration.LazyLoadingEnabled = false;
						DBEntity efEntity = context.RatingTask
								.SingleOrDefault( s => s.Id == input.Id );

						if ( efEntity != null && efEntity.Id > 0 )
						{
							//fill in fields that may not be in entity
							input.RowId = efEntity.RowId;
							input.Created = efEntity.Created;
							input.CreatedById = ( efEntity.CreatedById ?? 0 );
							input.Id = efEntity.Id;

							MapToDB( input, efEntity );
							bool hasChanged = false;
							if ( HasStateChanged( context ) )
							{
								hasChanged = true;
								efEntity.LastUpdated = DateTime.Now;
								efEntity.LastUpdatedById = input.LastUpdatedById;
								count = context.SaveChanges();
								//can be zero if no data changed
								if ( count >= 0 )
								{
									input.LastUpdated = ( DateTime ) efEntity.LastUpdated;
									isValid = true;
								}
								else
								{
									//?no info on error

									isValid = false;
									string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a RatingTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingTask: {0}, Id: {1}", FormatLongLabel( input.Description ), input.Id );
									status.AddError( "Error - the update was not successful. " + message );
									EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
								}

							}

							if ( isValid )
							{
								//update parts
								UpdateParts( input, status, fromUpload );
								if ( hasChanged )
								{
									SiteActivity sa = new SiteActivity()
									{
										ActivityType = "RatingTask",
										Activity = status.Action,
										Event = "Update",
										Comment = string.Format( "RatingTask was updated. Name: {0}", FormatLongLabel( input.Description ) ),
										ActionByUserId = input.LastUpdatedById,
										ActivityObjectId = input.Id
									};
									new ActivityManager().SiteActivityAdd( sa );
								}
							}
						}
						else
						{
							status.AddError( "Error - update failed, as record was not found." );
						}
					}

				}
			}
			catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
			{
				string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", input.Id, FormatLongLabel( input.Description ) ), "RatingTask" );
				status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
			}
			catch ( Exception ex )
			{
				string message = FormatExceptions( ex );
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", input.Id, FormatLongLabel( input.Description ) ), true );
				status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
				isValid = false;
			}


			return isValid;
		}
		private int Add( AppEntity entity, ref ChangeSummary status, bool fromUpload )
		{
			DBEntity efEntity = new DBEntity();
			status.HasSectionErrors = false;
			using ( var context = new DataEntities() )
			{
				try
				{
					if ( !IsValidGuid( entity.PayGradeType ) || !IsValidGuid( entity.ApplicabilityType ) )
					{
						//probably an issue for updating
						status.AddError( "Incomplete data was encountered trying to add a RatingTask - probably related to how updates are being done!!!" );
						return 0;
					}
						
					MapToDB( entity, efEntity );

					if ( IsValidGuid( entity.RowId ) )
						efEntity.RowId = entity.RowId;
					else
						efEntity.RowId = Guid.NewGuid();
					if ( IsValidCtid( entity.CTID ) )
						efEntity.CTID = entity.CTID;
					else
						efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
					entity.Created = efEntity.Created = DateTime.Now;
					entity.LastUpdated = efEntity.LastUpdated = DateTime.Now;
					efEntity.CreatedById = efEntity.LastUpdatedById = entity.LastUpdatedById;

					context.RatingTask.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						entity.RowId = efEntity.RowId;
						entity.Id = efEntity.Id;
						UpdateParts( entity, status, fromUpload );
						//
						//add log entry
						SiteActivity sa = new SiteActivity()
						{
							ActivityType = "RatingTask",
							Activity = status.Action,
							Event = "Add",
							Comment = string.Format( " A RatingTask was added. Desc: {0}", FormatLongLabel( entity.Description) ),
							ActionByUserId = entity.LastUpdatedById,
							ActivityObjectId = entity.Id
						};
						new ActivityManager().SiteActivityAdd( sa );


						return efEntity.Id;
					}
					else
					{
						//?no info on error

						string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a RatingTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingTask: '{0}'", FormatLongLabel( entity.Description ) );
						status.AddError( thisClassName + ". Error - the add was not successful. " + message );
						EmailManager.NotifyAdmin( "RatingTaskManager. Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + string.Format(".Add() RatingTask: '{0}'", FormatLongLabel( entity.Description )), "RatingTask" );
					status.AddError( thisClassName + ".Add(). Data Validation Error - the save was not successful. " + message );

					LoggingHelper.LogError( message, true );
				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), RatingTask: '{0}'", FormatLongLabel( entity.Description ) ) );
					status.AddError( thisClassName + string.Format( ".Add(), RatingTask: '{0}'. ", FormatLongLabel( entity.Description ) )   + message );
				}
			}

			return efEntity.Id;
		}
		public void UpdateParts( AppEntity input, ChangeSummary status, bool fromUpload )
		{
			try
			{
				//FunctionArea/WorkRole
				WorkRoleUpdate( input, ref status );

				//RatingTask.HasRating
				HasRatingUpdate( input, ref status );
				//RatingTask.HasJob
				HasJobUpdate( input, ref status );

				if ( UtilityManager.GetAppKeyValue( "handlingMultipleTrainingTasksPerRatingTask", false ) )
				{
					TrainingTaskUpdate( input, ref status, fromUpload );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
			}
		}

		#region Training task
		/// <summary>
		/// Handle multiple training tasks
		/// Note there can only be one per upload row. However, have to be carefull to not delete a trainging task for a different rating
		/// </summary>
		/// <param name="input"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool TrainingTaskUpdate( AppEntity input, ref ChangeSummary status, bool fromUpload )
		{
			status.HasSectionErrors = false;
			return true;
			//var efEntity = new Data.Tables.RatingTask_HasTrainingTask();
			//var entityType = "RatingTask_HasTrainingTask";
			//using ( var context = new DataEntities() )
			//{
			//	try
			//	{
			//		if ( input.HasTrainingTask == null )
			//			input.HasTrainingTask = new List<Guid>();

			//		//if ( input.HasTrainingTaskList?.Count == 0 )
			//		//{
			//		//	//temp handling of old approach
			//		//	if ( IsValidGuid( input.HasTrainingTask ))
			//		//	{
			//		//		input.HasTrainingTaskList.Add( input.HasTrainingTask );
			//		//	}
			//		//	else 
			//		//		input.HasTrainingTaskList = new List<Guid>();
			//		//}
			//		//get existing
			//		//should include current rating
			//		//if not fromUpload, there will not be a current rating
			//		var results =   from hasTrainingTask in context.RatingTask_HasTrainingTask
			//						join task in context.Course_Task
			//							on hasTrainingTask.TrainingTaskId equals task.Id
			//						join hasRating in context.RatingTask_HasRating
			//							on hasTrainingTask.RatingTaskId equals hasRating.RatingTaskId
			//						join rating in context.Rating
			//							  on hasRating.RatingId equals rating.Id
			//						where hasTrainingTask.RatingTaskId == input.Id
			//						&& ( input.CurrentRatingCode.Length > 0 ? true : rating.CodedNotation.ToLower() == input.CurrentRatingCode.ToLower() 
			//							|| !fromUpload ) //don't really need this now?

			//						select task;
			//		var existing = results?.ToList();
			//		#region deletes check
			//		if ( existing.Any() )
			//		{
			//			//if exists not in input, delete it
			//			foreach ( var e in existing )
			//			{
			//				var key = e.RowId;
			//				if ( IsValidGuid( key ) )
			//				{
			//					//if from upload, will be for a single rating, so if current is not in existing 
			//					if ( fromUpload && !input.HasTrainingTask.Contains( ( Guid ) key ) )
			//					{
			//						//now with the rating check, can probably do a delete?
			//						DeleteRatingTaskTrainingTask( input.Id, e.Id, ref status );
			//					}
			//				}
			//			}
			//		}
			//		#endregion
			//		//adds
			//		if ( input.HasTrainingTask != null )
			//		{
			//			foreach ( var child in input.HasTrainingTask )
			//			{
			//				//if not in existing, then add
			//				bool doingAdd = true;
			//				if ( existing?.Count > 0 )
			//				{
			//					var isfound = existing.Select( s => s.RowId == child ).ToList();
			//					if ( isfound.Any() )
			//						doingAdd = false;
			//				}
			//				if ( doingAdd )
			//				{
			//					var related = TrainingTaskManager.Get( child );
			//					if ( related?.Id > 0 )
			//					{
			//						//ReferenceConceptAdd( input, concept.Id, input.LastUpdatedById, ref status );
			//						efEntity.RatingTaskId = input.Id;
			//						efEntity.TrainingTaskId = related.Id;
			//						efEntity.RowId = Guid.NewGuid();
			//						efEntity.CreatedById = input.LastUpdatedById;
			//						efEntity.Created = DateTime.Now;

			//						context.RatingTask_HasTrainingTask.Add( efEntity );

			//						// submit the change to database
			//						int count = context.SaveChanges();
			//						if ( count > 0 )
			//						{
			//							SiteActivity sa = new SiteActivity()
			//							{
			//								ActivityType = "RatingTask TrainingTask",
			//								Activity = status.Action,
			//								Event = "Add",
			//								Comment = string.Format( "RatingTask TrainingTask was added. Name: {0}", FormatLongLabel( related.Description ) ),
			//								ActionByUserId = input.LastUpdatedById,
			//								ActivityObjectId = input.Id
			//							};
			//							new ActivityManager().SiteActivityAdd( sa );

			//						}
			//					}
			//					else
			//					{
			//						status.AddError( String.Format( "Error. For RatingTask: '{0}' ({1} code: {2}) a HasTrainingTask was not found for Identifier: {3}", FormatLongLabel( input.Description ), input.Id, input.CodedNotation, child ) );
			//					}
			//				}
			//			}
			//			//
			//			return true;
			//		}
			//	}
			//	catch ( Exception ex )
			//	{
			//		string message = FormatExceptions( ex );
			//		LoggingHelper.LogError( ex, thisClassName + string.Format( ".HasTrainingTaskUpdate-'{0}', RatingTask: '{1}' ({2})", entityType, FormatLongLabel( input.Description ), input.Id ) );
			//		status.AddError( thisClassName + ".HasTrainingTaskUpdate(). Error - the save was not successful. \r\n" + message );
			//	}
			//}
			//return false;
		}

		public bool DeleteRatingTaskTrainingTask( int ratingTaskId, int trainingTaskId, ref ChangeSummary status )
		{
			bool isValid = true;
			if ( trainingTaskId == 0 )
			{
				//statusMessage = "Error - missing an identifier for the HasTrainingTask to remove";
				return false;
			}

			//using ( var context = new DataEntities() )
			//{
			//	var efEntity = context.RatingTask_HasTrainingTask
			//					.FirstOrDefault( s => s.RatingTaskId == ratingTaskId && s.TrainingTaskId == trainingTaskId );

			//	if ( efEntity != null && efEntity.Id > 0 )
			//	{
			//		context.RatingTask_HasTrainingTask.Remove( efEntity );
			//		int count = context.SaveChanges();
			//		if ( count > 0 )
			//		{
			//			isValid = true;
			//		}
			//	}
			//	else
			//	{
			//		//statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
			//		isValid = true;
			//	}
			//}

			return isValid;
		}

		#endregion
		#region WorkRole
		public bool WorkRoleUpdate( AppEntity input, ref ChangeSummary status )
		{
			status.HasSectionErrors = false;
			//var efEntity = new Data.Tables.RatingTask_WorkRole();
			//var entityType = "RatingTask_WorkRole";
			//using ( var context = new DataEntities() )
			//{
			//	try
			//	{
			//		if ( input.HasWorkRole?.Count == 0 )
			//			input.HasWorkRole = new List<Guid>();
			//		var results =
			//						from entity in context.RatingTask_WorkRole
			//						join related in context.WorkRole
			//						on entity.WorkRoleId equals related.Id
			//						where entity.RatingTaskId == input.Id

			//						select related;
			//		var existing = results?.ToList();
			//		#region deletes check
			//		if ( existing.Any() )
			//		{
			//			//if exists not in input, delete it
			//			foreach ( var e in existing )
			//			{
			//				var key = e.RowId;
			//				if ( IsValidGuid( key ) )
			//				{
			//					if ( !input.HasWorkRole.Contains( ( Guid ) key ) )
			//					{
			//						//DeleteRatingTaskWorkRole( input.Id, e.Id, ref status );
			//					}
			//				}
			//			}
			//		}
			//		#endregion
			//		//adds
			//		if ( input.HasWorkRole != null )
			//		{
			//			foreach ( var child in input.HasWorkRole )
			//			{
			//				//if not in existing, then add
			//				bool doingAdd = true;
			//				if ( existing?.Count > 0 )
			//				{
			//					var isfound = existing.Select( s => s.RowId == child ).ToList();
			//					if ( isfound.Any() )
			//						doingAdd = false;
			//				}
			//				if ( doingAdd )
			//				{
			//					var related = WorkRoleManager.Get( child );
			//					if ( related?.Id > 0 )
			//					{
			//						//ReferenceConceptAdd( input, concept.Id, input.LastUpdatedById, ref status );
			//						efEntity.RatingTaskId = input.Id;
			//						efEntity.WorkRoleId = related.Id;
			//						efEntity.RowId = Guid.NewGuid();
			//						efEntity.CreatedById = input.LastUpdatedById;
			//						efEntity.Created = DateTime.Now;

			//						context.RatingTask_WorkRole.Add( efEntity );

			//						// submit the change to database
			//						int count = context.SaveChanges();
			//						if ( count > 0 )
			//						{
			//							SiteActivity sa = new SiteActivity()
			//							{
			//								ActivityType = "RatingTask WorkRole",
			//								Activity = status.Action,
			//								Event = "Add",
			//								Comment = string.Format( "RatingTask WorkRole was added. Name: {0}", related.Name ),
			//								ActionByUserId = input.LastUpdatedById,
			//								ActivityObjectId = input.Id
			//							};
			//							new ActivityManager().SiteActivityAdd( sa );
									   
			//						}
			//					}
			//					else
			//					{
			//						status.AddError( String.Format( "Error. For RatingTask: '{0}' ({1}) a HasWorkRole was not found for Identifier: {2}", FormatLongLabel(input.Description), input.Id, child ) );
			//					}
			//				}
			//			}
			//			//
			//			return true;
			//		}
			//	}
			//	catch ( Exception ex )
			//	{
			//		string message = FormatExceptions( ex );
			//		LoggingHelper.LogError( ex, thisClassName + string.Format( ".HasWorkRoleUpdate-'{0}', RatingTask: '{1}' ({2})", entityType, FormatLongLabel( input.Description ), input.Id ) );
			//		status.AddError( thisClassName + ".HasWorkRoleUpdate(). Error - the save was not successful. \r\n" + message );
			//	}
			//}
			return true;
		}
		public bool DeleteRatingTaskWorkRole( int ratingTaskId, int workRoleId, ref ChangeSummary status )
		{
			bool isValid = true;
			if ( workRoleId == 0 )
			{
				//statusMessage = "Error - missing an identifier for the RatingTaskWorkRole( to remove";
				return false;
			}

			//using ( var context = new DataEntities() )
			//{
			//	var efEntity = context.RatingTask_WorkRole
			//					.FirstOrDefault( s => s.RatingTaskId == ratingTaskId && s.WorkRoleId == workRoleId );

			//	if ( efEntity != null && efEntity.Id > 0 )
			//	{
			//		context.RatingTask_WorkRole.Remove( efEntity );
			//		int count = context.SaveChanges();
			//		if ( count > 0 )
			//		{
			//			isValid = true;
			//		}
			//	}
			//	else
			//	{
			//		//statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
			//		isValid = true;
			//	}
			//}

			return isValid;
		}
		#endregion


		public bool HasRatingUpdate( AppEntity input, ref ChangeSummary status )
		{
			status.HasSectionErrors = false;
	   //	 var efEntity = new Data.Tables.RatingTask_HasRating();
	   //	 var entityType = "RatingTask_HasRating";
	   //	 using ( var context = new DataEntities() )
	   //	 {
	   //		 try
	   //		 {
	   //			 if ( input.HasRating?.Count == 0 )
	   //				 input.HasRating = new List<Guid>();
	   //			 var results =
	   //							 from entity in context.RatingTask_HasRating
	   //							 join related in context.Rating
	   //							 on entity.RatingId equals related.Id
	   //							 where entity.RatingTaskId == input.Id

	   //							 select related;
	   //			 var existing = results?.ToList();
	   //			 #region deletes check
	   //			 if ( existing.Any() )
	   //			 {
	   //				 //if exists not in input, delete it
	   //				 foreach ( var e in existing )
	   //				 {
	   //					 var key = e.RowId;
	   //					 if ( IsValidGuid( key ) )
	   //					 {
	   //						 if ( !input.HasRating.Contains( ( Guid ) key ) )
	   //						 {
	   //							 //DeleteRatingTaskHasRating( input.Id, e.Id, ref status );
	   //						 }
	   //					 }
	   //				 }
	   //			 }
	   //			 #endregion
	   //			 //adds
	   //			 if ( input.HasRating != null )
	   //			 {
	   //				 foreach ( var child in input.HasRating )
	   //				 {
	   //					 //if not in existing, then add
							///*
	   //					 bool doingAdd = true;
	   //					 if ( existing?.Count > 0 )
	   //					 {
	   //						 var isfound = existing.Select( s => s.RowId == child ).ToList();
	   //						 if ( isfound.Any() )
	   //							 doingAdd = false;
	   //					 }
							//*/
							//if( existing.Where( s => s.RowId == child ).Count() == 0 ) //Not sure why .Select() always returns at least one value but .Where() does not
	   //					 //if ( doingAdd )
	   //					 {
	   //						 var related = RatingManager.Get( child );
	   //						 if ( related?.Id > 0 )
	   //						 {
	   //							 //ReferenceConceptAdd( input, concept.Id, input.LastUpdatedById, ref status );
	   //							 efEntity.RatingTaskId = input.Id;
	   //							 efEntity.RatingId = related.Id;
	   //							 efEntity.RowId = Guid.NewGuid();
	   //							 efEntity.CreatedById = input.LastUpdatedById;
	   //							 efEntity.Created = DateTime.Now;

	   //							 context.RatingTask_HasRating.Add( efEntity );

	   //							 // submit the change to database
	   //							 int count = context.SaveChanges();
	   //							 if ( count > 0 )
	   //							 {
	   //								 SiteActivity sa = new SiteActivity()
	   //								 {
	   //									 ActivityType = "RatingTask HasRating",
	   //									 Activity = status.Action,
	   //									 Event = "Add",
	   //									 Comment = string.Format( "RatingTask Rating was added. Name: {0}", related.Name ),
	   //									 ActionByUserId = input.LastUpdatedById,
	   //									 ActivityObjectId = input.Id
	   //								 };
	   //								 new ActivityManager().SiteActivityAdd( sa );
	   //								 //									   
	   //							 }
	   //						 }
	   //						 else
	   //						 {
	   //							 status.AddError( String.Format( "Error. For RatingTask: '{0}' ({1}) a HasRating was not found for Identifier: {2}", FormatLongLabel( input.Description ), input.Id, child ) );
	   //						 }
	   //					 }
	   //				 }
	   //				 return true;
	   //			 }
	   //		 }
	   //		 catch ( Exception ex )
	   //		 {
	   //			 string message = FormatExceptions( ex );
	   //			 LoggingHelper.LogError( ex, thisClassName + string.Format( ".HasRatingUpdate-'{0}', RatingTask: '{1}' ({2})", entityType, FormatLongLabel( input.Description ), input.Id ) );
	   //			 status.AddError( thisClassName + ".HasRatingUpdate(). Error - the save was not successful. \r\n" + message );
	   //		 }
	   //	 }
			return true;
		}
		public bool DeleteRatingTaskHasRating( int ratingTaskId, int workRoleId, ref ChangeSummary status )
		{
			bool isValid = true;
			if ( workRoleId == 0 )
			{
				//statusMessage = "Error - missing an identifier for the CourseConcept to remove";
				return false;
			}

			//using ( var context = new DataEntities() )
			//{
			//	var efEntity = context.RatingTask_HasRating
			//					.FirstOrDefault( s => s.RatingTaskId == ratingTaskId && s.RatingId == workRoleId );

			//	if ( efEntity != null && efEntity.Id > 0 )
			//	{
			//		context.RatingTask_HasRating.Remove( efEntity );
			//		int count = context.SaveChanges();
			//		if ( count > 0 )
			//		{
			//			isValid = true;
			//		}
			//	}
			//	else
			//	{
			//		//statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
			//		isValid = true;
			//	}
			//}

			return isValid;
		}

		public bool HasJobUpdate( AppEntity input, ref ChangeSummary status )
		{
			status.HasSectionErrors = false;
			//var efEntity = new Data.Tables.RatingTask_HasJob();
			//var entityType = "RatingTask_HasJob";
			//using ( var context = new DataEntities() )
			//{
			//	try
			//	{
			//		if ( input.HasBilletTitle?.Count == 0 )
			//			input.HasBilletTitle = new List<Guid>();
			//		var results =
			//						from entity in context.RatingTask_HasJob
			//						join related in context.Job
			//						on entity.JobId equals related.Id
			//						where entity.RatingTaskId == input.Id

			//						select related;
			//		var existing = results?.ToList();
			//		#region deletes check
			//		if ( existing.Any() )
			//		{
			//			//if exists not in input, delete it
			//			foreach ( var e in existing )
			//			{
			//				var key = e.RowId;
			//				if ( IsValidGuid( key ) )
			//				{
			//					if ( !input.HasBilletTitle.Contains( ( Guid ) key ) )
			//					{
			//						//DeleteRatingTaskHasJob( input.Id, e.Id, ref status );
			//					}
			//				}
			//			}
			//		}
			//		#endregion
			//		//adds
			//		if ( input.HasBilletTitle != null )
			//		{
			//			foreach ( var child in input.HasBilletTitle )
			//			{
			//				//if not in existing, then add
			//				bool doingAdd = true;
			//				if ( existing?.Count > 0 )
			//				{
			//					var isfound = existing.Select( s => s.RowId == child ).ToList();
			//					if ( isfound.Any() )
			//						doingAdd = false;
			//				}
			//				if ( doingAdd )
			//				{
			//					var related = JobManager.Get( child );
			//					if ( related?.Id > 0 )
			//					{
			//						//ReferenceConceptAdd( input, concept.Id, input.LastUpdatedById, ref status );
			//						efEntity.RatingTaskId = input.Id;
			//						efEntity.JobId = related.Id;
			//						efEntity.RowId = Guid.NewGuid();
			//						efEntity.CreatedById = input.LastUpdatedById;
			//						efEntity.Created = DateTime.Now;

			//						context.RatingTask_HasJob.Add( efEntity );

			//						// submit the change to database
			//						int count = context.SaveChanges();
			//						if ( count > 0 )
			//						{
			//							SiteActivity sa = new SiteActivity()
			//							{
			//								ActivityType = "RatingTask BilletTitle",
			//								Activity = status.Action,
			//								Event = "Add",
			//								Comment = string.Format( "RatingTask BilletTitle was added. Name: {0}", related.Name ),
			//								ActionByUserId = input.LastUpdatedById,
			//								ActivityObjectId = input.Id
			//							};
			//							new ActivityManager().SiteActivityAdd( sa );
			//							//									   
			//						}
			//					}
			//					else
			//					{
			//						status.AddError( String.Format( "Error. For RatingTask: '{0}' ({1}) a HasBillet was not found for Identifier: {2}", FormatLongLabel( input.Description ), input.Id, child ) );
			//					}
			//				}
			//			}
			//		}
			//	}
			//	catch ( Exception ex )
			//	{
			//		string message = FormatExceptions( ex );
			//		LoggingHelper.LogError( ex, thisClassName + string.Format( ".HasRatingUpdate-'{0}', RatingTask: '{1}' ({2})", entityType, FormatLongLabel( input.Description ), input.Id ) );
			//		status.AddError( thisClassName + ".HasRatingUpdate(). Error - the save was not successful. \r\n" + message );
			//	}
			//}
			return true;
		}
		public bool DeleteRatingTaskHasJob( int ratingTaskId, int jobId, ref ChangeSummary status )
		{
			bool isValid = true;
			if ( jobId == 0 )
			{
				//statusMessage = "Error - missing an identifier for the CourseConcept to remove";
				return false;
			}

			//using ( var context = new DataEntities() )
			//{
			//	var efEntity = context.RatingTask_HasJob
			//					.FirstOrDefault( s => s.RatingTaskId == ratingTaskId && s.JobId == jobId );

			//	if ( efEntity != null && efEntity.Id > 0 )
			//	{
			//		context.RatingTask_HasJob.Remove( efEntity );
			//		int count = context.SaveChanges();
			//		if ( count > 0 )
			//		{
			//			isValid = true;
			//		}
			//	}
			//	else
			//	{
			//		//statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
			//		isValid = true;
			//	}
			//}

			return isValid;
		}


		public static void MapToDB( AppEntity input, DBEntity output )
		{
			//watch for missing properties like rowId
			List<string> errors = new List<string>();
			BaseFactory.AutoMap( input, output, errors );
		}
		//


		#endregion

		#region Retrieval
		/// <summary>
		/// Don't know the input for sure. Could be ImportRMTL
		/// check for an existing task using:
		/// - PayGrade
		/// - FunctionalArea (maybe not)
		/// - Source/ReferenceResource
		/// - RatingTask
		/// A this point checking for GUIDs
		/// </summary>
		/// <param name="importEntity"></param>
		/// <param name="currentRatingCodedNotation"></param>
		/// <returns></returns>
		public static AppEntity GetFromView( AppEntity importEntity, string currentRatingCodedNotation )
		{
			var entity = new AppEntity();
			//will probably have to d

			using ( var context = new ViewContext() )
			{
				var item = new Data.Views.RatingTaskSummary();
				//can't trust just coded notation, need to consider the current rating somewhere
				if ( !string.IsNullOrWhiteSpace( importEntity.CodedNotation ) )
				{
					item = context.RatingTaskSummary.FirstOrDefault( s => ( s.CodedNotation ?? "" ).ToLower() == importEntity.CodedNotation.ToLower()
								&& s.Ratings.Contains( currentRatingCodedNotation ) );
				}
				if ( item == null || item.Id == 0 )
				{
					//22-04-14 - TBD - not sure we want to do this approach - risky
					//item = context.RatingTaskSummary
					//			.FirstOrDefault( s => s.PayGradeType == importEntity.PayGradeType
					//			//&& s.FunctionalAreaUID == importEntity.ReferenceType //NOW a list, so not helpful
					//			&& s.HasReferenceResource == importEntity.HasReferenceResource
					//			&& s.RatingTask.ToLower() == importEntity.Description.ToLower()
					//			);
				}
				if ( item != null && item.Id > 0 )
				{
					//if exists, will just return the Id?
					//or do a get, and continue?
					entity = GetById( item.Id );
				}
			}

			return entity;
		}
		//

		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.RatingTask, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

		public static AppEntity GetByRowId( Guid rowId, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.RowId == rowId, returnNullIfNotFound );
		}
		//

		public static AppEntity GetById( int id, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.Id == id, returnNullIfNotFound );
		}
		//

		public static AppEntity GetByCTID( string ctid, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.CTID.ToLower() == ctid?.ToLower(), returnNullIfNotFound );
		}
		//

		public static AppEntity GetForUploadOrNull( string ratingTaskDescription, Guid referenceResourceRowID, Guid referenceTypeRowID )
		{
			using ( var context = new DataEntities() )
			{
				var match = context.RatingTask.FirstOrDefault( m =>
					m.Description.ToLower() == ratingTaskDescription.ToLower() &&
					m.ReferenceResource.RowId == referenceResourceRowID &&
					m.ConceptScheme_Concept_ReferenceType.RowId == referenceTypeRowID
				);

				if( match != null )
				{
					return MapFromDB( match, context );
				}
			}

			return null;
		}
		//

		/// <summary>
		/// It is not clear that we want a get all - tens of thousands
		/// </summary>
		/// <returns></returns>
		public static List<AppEntity> GetAll()
		{
			var entity = new AppEntity();
			var list = new List<AppEntity>();

			using ( var context = new DataEntities() )
			{
				var results = context.RatingTask
					.OrderBy( s => s.Id )
					.ToList();

				foreach ( var item in results )
				{
					list.Add( MapFromDB( item, context ) );
				}
			}

			return list;
		}
		//

		public static List<AppEntity> GetMultiple( List<Guid> rowIDs )
		{

			var entity = new AppEntity();
			var list = new List<AppEntity>();
			if ( rowIDs == null || rowIDs.Count() == 0 )
			{
				return list;
			}

			using ( var context = new DataEntities() )
			{
				var results = context.RatingTask.Where( m => rowIDs.Contains( m.RowId ) )
					.OrderBy( s => s.Id )
					.ToList();

				foreach ( var item in results )
				{
					list.Add( MapFromDB( item, context ) );
				}
			}

			return list;
		}
		//

		public static RatingTask FindMatchOrNullForUpload( string ratingTaskDescription, Guid hasReferenceResource_RowID )
		{
			using ( var context = new DataEntities() )
			{
				var match = context.RatingTask.Where( m =>
					m.Description.ToLower() == ratingTaskDescription.ToLower() &&
					m.ReferenceResource.RowId == hasReferenceResource_RowID
				).FirstOrDefault();

				if( match != null )
				{
					return MapFromDB( match, context );
				}
			}

			return null;
		}
		//

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
		{
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.RatingTask.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Description.Contains( keywords )
					);
				}

				//Rating Task Count By Source Type Table (used to filter by reference resource type)
				AppendIDsFilterIfPresent( query, "> ReferenceResourceTypeId > Concept", ids => {
					list = list.Where( m => ids.Contains( m.ReferenceTypeId ) );
				} );

				//Rating Task Count By Source Type Table (used for counting tasks with gaps)
				AppendIDsFilterIfPresent( query, "< RatingTaskId < RatingContext > FormalTrainingGapId > Concept", ids => {
					list = list.Where( m => context.RatingContext.Where( n => n.RatingTaskId == m.Id && ids.Contains( n.FormalTrainingGapId ?? 0 ) ).Count() > 0 );
				} );

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.Description, m => m.OrderBy( n => n.Description ) );

			}, MapFromDBForSearch );

		}
		//
		[Obsolete]
		public static List<EntitySummary> RMTLSearch( string pFilter, string pOrderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows, bool autocomplete = false )
		{
			string connectionString = DBConnectionRO();
			EntitySummary item = new EntitySummary();
			List<EntitySummary> list = new List<EntitySummary>();
			var result = new DataTable();

			if ( pageNumber < 1 )
				pageNumber = 1;
			if ( pageSize < 1 )
			{
				//if there is a filter, could allow getting all
				if ( !string.IsNullOrWhiteSpace( pFilter ) )
				{
					//ensure stored proc can handle this - it can
					pageSize = 0;
				}
			}

			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}

				using ( SqlCommand command = new SqlCommand( "[RatingTaskSearch]", c ) )
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add( new SqlParameter( "@Filter", pFilter ) );
					command.Parameters.Add( new SqlParameter( "@SortOrder", pOrderBy ) );
					command.Parameters.Add( new SqlParameter( "@StartPageIndex", pageNumber ) );
					command.Parameters.Add( new SqlParameter( "@PageSize", pageSize ) );
					command.Parameters.Add( new SqlParameter( "@CurrentUserId", userId ) );

					SqlParameter totalRows = new SqlParameter( "@TotalRows", pTotalRows );
					totalRows.Direction = ParameterDirection.Output;
					command.Parameters.Add( totalRows );
					try
					{
						using ( SqlDataAdapter adapter = new SqlDataAdapter() )
						{
							adapter.SelectCommand = command;
							adapter.Fill( result );
						}
						string rows = command.Parameters[5].Value.ToString();
						pTotalRows = Int32.Parse( rows );
					}
					catch ( Exception ex )
					{
						pTotalRows = 0;
						LoggingHelper.LogError( ex, thisClassName + string.Format( ".Search() - Execute proc, Message: {0} \r\n Filter: {1} \r\n", ex.Message, pFilter ) );

						item = new EntitySummary();
						item.RatingTask = "Unexpected error encountered. System administration has been notified. Please try again later. ";
						item.RatingTask = ex.Message;

						list.Add( item );
						return list;
					}
				}

				try
				{
					var resultNumber = 0;
					foreach ( DataRow dr in result.Rows )
					{
						item = new EntitySummary();
						resultNumber++;
						item.ResultNumber = resultNumber;
						item.Id = GetRowColumn( dr, "Id", 0 );
						item.RowId = GetGuidType( dr, "RowId" );
						//Ratings - coded notation
						item.Ratings = dr["Ratings"].ToString();// GetRowColumn( dr, "Ratings", "" );
						item.RatingName = dr["RatingName"].ToString();// GetRowColumn( dr, "RatingName", "" );
						//do we need to populate HasRatings (if so, could include in the pipe separated list of Ratings)
						//22-04-04 mp - search will now return a single HasRating
						var hasRatings = GetGuidType( dr, "HasRating" );
						if ( IsGuidValid( hasRatings ) )
						{
							item.HasRatings.Add( hasRatings );
						}
						//Hmm autocomplete is always false? Is this obsolete?
						//if ( autocomplete )
						//{
						//	item.HasRatings = GetRatingGuids( item.Ratings );
						//}
						//BilletTitles
						item.BilletTitles = dr["BilletTitles"].ToString();// GetRowColumn( dr, "BilletTitles", "" );
						//var bt= GetRowColumn( dr, "BilletTitles", "" );
						//could save previous and then first check the previous
						//similarly, do we need a list of billet guids?
						if ( autocomplete )
							item.HasBilletTitles = GetBilletTitleGuids( item.BilletTitles );


						item.RatingTask = dr["RatingTask"].ToString();// GetRowColumn( dr, "RatingTask", "" );
						item.Note = dr["Notes"].ToString();// GetRowColumn( dr, "Notes", "" );
						item.CTID = dr["CTID"].ToString();// GetRowPossibleColumn( dr, "CTID", "" );
														  //
						item.Created = GetRowColumn( dr, "Created", DateTime.MinValue );
						item.Creator = dr["CreatedBy"].ToString(); //GetRowPossibleColumn( dr, "CreatedBy","" );
																   //item.CreatedBy = GetGuidType( dr, "CreatedByUID" );
						item.LastUpdated = GetRowColumn( dr, "LastUpdated", DateTime.MinValue );
						item.ModifiedBy = dr["ModifiedBy"].ToString(); //GetRowPossibleColumn( dr, "ModifiedBy", "" );
																	   //item.LastUpdatedBy = GetGuidType( dr, "ModifiedByUID" );

						item.CodedNotation = dr["CodedNotation"].ToString();// GetRowPossibleColumn( dr, "CodedNotation", "" );
																			//
						item.Rank = dr["Rank"].ToString();
						item.RankName = dr["RankName"].ToString();
						item.PayGradeType = GetGuidType( dr, "PayGradeType" );
						//
						item.Level = dr["Level"].ToString();// GetRowPossibleColumn( dr, "Level", "" );
						//FunctionalArea  - not a pipe separated list 
						//22-01-23 - what to do about the HasWorkRole list. Could include and split out here?
						item.FunctionalArea = dr["FunctionalArea"].ToString(); //GetRowColumn( dr, "FunctionalArea", "" );
						if ( !string.IsNullOrWhiteSpace( item.FunctionalArea ) )
						{
							var workRoleList = "";
							item.HasWorkRole = GetFunctionalAreas( item.FunctionalArea, ref workRoleList );
							item.FunctionalArea = workRoleList;
						}
						//
						//
						//item.ReferenceResource = dr["ReferenceResource"].ToString().Trim();
						item.ReferenceResource = dr["ReferenceResource"].ToString(); //GetRowColumn( dr, "ReferenceResource", "" );
						item.SourceDate = dr["SourceDate"].ToString();// GetRowColumn( dr, "SourceDate", "" );
						item.HasReferenceResource = GetGuidType( dr, "HasReferenceResource" );
						//
						item.WorkElementType = dr["WorkElementType"].ToString(); //GetRowPossibleColumn( dr, "WorkElementType", "" );
						item.WorkElementTypeAlternateName = dr["WorkElementTypeAlternateName"].ToString();
						//item.ReferenceType = GetGuidType( dr, "ReferenceType" );
						//
						item.TaskApplicability = dr["TaskApplicability"].ToString().Trim();// GetRowPossibleColumn( dr, "TaskApplicability", "" );
						item.ApplicabilityType = GetGuidType( dr, "ApplicabilityType" );
						//
						item.FormalTrainingGap = dr["FormalTrainingGap"].ToString();// GetRowPossibleColumn( dr, "FormalTrainingGap", "" );
						item.TrainingGapType = GetGuidType( dr, "TrainingGapType" );

						item.CIN = dr["CIN"].ToString();// GetRowColumn( dr, "CIN", "" );
						item.CourseName = dr["CourseName"].ToString();// GetRowColumn( dr, "CourseName", "" );
						item.CourseTypes = dr["CourseTypes"].ToString().Trim();// GetRowPossibleColumn( dr, "CourseType", "" );
						item.CurrentAssessmentApproach = GetRowPossibleColumn( dr, "CurrentAssessmentApproach", "" );
						//
						item.TrainingTask = dr["TrainingTask"].ToString();// GetRowPossibleColumn( dr, "TrainingTask", "" );
																		  //item.HasTrainingTask = GetGuidType( dr, "HasTrainingTask" );


						//
						item.CurriculumControlAuthority = dr["CurriculumControlAuthority"].ToString().Trim();// GetRowPossibleColumn( dr, "CurriculumControlAuthority", "" );
						item.LifeCycleControlDocument = dr["LifeCycleControlDocument"].ToString().Trim();// GetRowPossibleColumn( dr, "LifeCycleControlDocument", "" );
						item.Notes = dr["Notes"].ToString();
						//Part III
						item.TrainingSolutionType = dr["TrainingSolutionType"].ToString().Trim(); ;
						item.ClusterAnalysisTitle = dr["ClusterAnalysisTitle"].ToString().Trim(); ;
						item.RecommendedModality = dr["RecommendedModality"].ToString().Trim(); ;
						item.DevelopmentSpecification = dr["DevelopmentSpecification"].ToString().Trim(); ;

						item.CandidatePlatform = dr["CandidatePlatform"].ToString().Trim(); ;
						item.CFMPlacement = dr["CFMPlacement"].ToString().Trim(); ;
						item.PriorityPlacement = GetRowPossibleColumn( dr, "PriorityPlacement", 0 );
						item.DevelopmentRatio = dr["DevelopmentRatio"].ToString().Trim(); ;

						item.EstimatedInstructionalTime = GetRowPossibleColumn( dr, "EstimatedInstructionalTime", 0.0M );
						item.DevelopmentTime = GetRowPossibleColumn( dr, "DevelopmentTime", 0 );
						item.ClusterAnalysisNotes = dr["ClusterAnalysisNotes"].ToString().Trim(); ;

						list.Add( item );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.DoTrace( 1, thisClassName + ".Search. Exception: " + ex.Message );
				}
				return list;

			}
		}
		//

		public static AppEntity MapFromDB( DBEntity input, DataEntities context )
		{
			return MapFromDBForSearch( input, context, null );
		}
		//

		public static AppEntity MapFromDBForSearch( DBEntity input, DataEntities context, SearchResultSet<AppEntity> resultSet = null )
		{
			var output = AutoMap( input, new AppEntity() );
			output.HasReferenceResource = input.ReferenceResource?.RowId ?? Guid.Empty;
			output.ReferenceType = input.ConceptScheme_Concept_ReferenceType?.RowId ?? Guid.Empty;

			return output;
		}
		//

		#endregion

		#region === ImportRMTL (prototype)  ==================
		/*
		public bool Save( ImportRMTL input, ref ChangeSummary status )
		{
			bool isValid = true;
			int count = 0;
			try
			{
				using ( var context = new DataEntities() )
				{
					//if ( ValidateProfile( entity, ref status ) == false )
					//	return false;
					//look up if no id
					if ( input.Id == 0 )
					{
						//need to identify for sure what is unique
						var record = Get( input );
						if ( record?.Id > 0 )
						{
							//
							input.Id = record.Id;
							//could be course updates etc. 
							UpdateParts( input, status );

							return true;
						}
						else
						{
							//add
							int newId = Add( input, ref status );
							if ( newId == 0 || status.HasErrors )
								isValid = false;
						}
					}
					else
					{
						//TODO - consider if necessary, or interferes with anything
						context.Configuration.LazyLoadingEnabled = false;
						DBEntity efEntity = context.RatingTask
								.SingleOrDefault( s => s.Id == input.Id );

						if ( efEntity != null && efEntity.Id > 0 )
						{
							//fill in fields that may not be in entity
							input.Id = efEntity.Id;

							MapToDB( input, efEntity );

							if ( HasStateChanged( context ) )
							{
								efEntity.LastUpdated = DateTime.Now;
								//efEntity.LastUpdatedById = input.LastUpdatedById;
								count = context.SaveChanges();
								//can be zero if no data changed
								if ( count >= 0 )
								{
									isValid = true;
								}
								else
								{
									//?no info on error

									isValid = false;
									string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a RatingTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingTask: {0}, Id: {1}", input.Work_Element_Task, input.Id );
									status.AddError( "Error - the update was not successful. " + message );
									EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
								}

							}

							if ( isValid )
							{
								UpdateParts( input, status );

								SiteActivity sa = new SiteActivity()
								{
									ActivityType = "RatingTask",
									Activity = "Import",
									Event = "Update",
									Comment = string.Format( "RatingTask was updated by the import. Name: {0}", input.Work_Element_Task ),
									ActivityObjectId = input.Id
								};
								new ActivityManager().SiteActivityAdd( sa );
							}
						}
						else
						{
							status.AddError( "Error - update failed, as record was not found." );
						}
					}

				}
			}
			catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
			{
				string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", input.Id, input.Work_Element_Task ), "RatingTask" );
				status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
			}
			catch ( Exception ex )
			{
				string message = FormatExceptions( ex );
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", input.Id, input.Work_Element_Task ), true );
				status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
				isValid = false;
			}


			return isValid;
		}

		/// <summary>
		/// Update a record
		/// </summary>
		/// <param name="input"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		private int Add( ImportRMTL input, ref ChangeSummary status )
		{
			DBEntity efEntity = new DBEntity();
			status.HasSectionErrors = false;
			using ( var context = new DataEntities() )
			{
				try
				{
					MapToDB( input, efEntity, status );

					//if ( IsValidGuid( input.RowId ) )
					//	efEntity.RowId = input.RowId;
					//else
					efEntity.RowId = Guid.NewGuid();
					efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
					input.ImportDate = efEntity.LastUpdated = efEntity.Created = DateTime.Now;
					//efEntity.CreatedById = efEntity.LastUpdatedById = input.LastUpdatedById;

					context.RatingTask.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						//input.RowId = efEntity.RowId;
						input.Id = efEntity.Id;
						UpdateParts( input, status );
						//
						//add log entry
						SiteActivity sa = new SiteActivity()
						{
							ActivityType = "RatingTask",
							Activity = "Import",
							Event = "Add",
							Comment = string.Format( " A RatingTask was added by the import. Desc: {0}", input.Work_Element_Task ),
							ActivityObjectId = input.Id
						};
						new ActivityManager().SiteActivityAdd( sa );


						return efEntity.Id;
					}
					else
					{
						//?no info on error

						string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a RatingTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingTask: {0}", FormatLongLabel( input.Work_Element_Task ) );
						status.AddError( thisClassName + ". Error - the add was not successful. " + message );
						EmailManager.NotifyAdmin( "RatingTaskManager. Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "RatingTask" );
					status.AddError( thisClassName + ".Add(). Error - the save was not successful. " + message );

					LoggingHelper.LogError( message, true );
				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Description: {0}", FormatLongLabel( input.Description ) ) );
					status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
				}
			}

			return efEntity.Id;
		}
		/// <summary>
		/// add a RatingTask
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public void UpdateParts( ImportRMTL input, ChangeSummary status )
		{
			try
			{


			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
			}
		}
		public static void MapToDB( ImportRMTL input, DBEntity output )
		{
			//watch for missing properties like rowId
			List<string> errors = new List<string>();
			//not much value for this?
			//BaseFactory.AutoMap( input, output, errors );

			output.Description = input.Work_Element_Task;

		}


		*/
		#endregion
	}
	[Serializable]
	public class CachedRatingTask
	{
		public CachedRatingTask()
		{
			LastUpdated = DateTime.Now;
		}
		public DateTime LastUpdated { get; set; }
		public List<AppEntity> Objects { get; set; }

	}
}
