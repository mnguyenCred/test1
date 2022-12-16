using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Curation;
using AppEntity = Models.Schema.CourseContext;
using AppFullEntity = Models.Schema.PopulatedCourseContext;
using CourseContextTask = Models.Schema.TrainingTask;
using DBEntity = Data.Tables.CourseContext;
using MSc = Models.Schema;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Models.Application;
using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class CourseContextManager : BaseFactory
    {
        public static new string thisClassName = "CourseContextManager";
		#region CourseContext - persistance ==================
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
				BasicSaveCore( context, entity, context.CourseContext, userID, ( ent, dbEnt ) => {
					dbEnt.HasTrainingTaskId = context.TrainingTask.FirstOrDefault( m => m.RowId == ent.HasTrainingTask )?.Id ?? 0;
					dbEnt.HasCourseId = context.Course.FirstOrDefault( m => m.RowId == ent.HasCourse )?.Id ?? 0;
				}, ( ent, dbEnt ) => {
					HandleMultiValueUpdate( context, userID, ent.AssessmentMethodType, dbEnt, dbEnt.CourseContext_AssessmentType, context.ConceptScheme_Concept, nameof( CourseContext_AssessmentType.CourseContextId ), nameof( CourseContext_AssessmentType.AssessmentMethodConceptId ) );
				}, saveType, AddErrorMethod );
			}
		}
		//

		/// <summary>
		/// Update a CourseContext
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Save( AppEntity entity, ref ChangeSummary status )
        {
            bool isValid = true;
            int count = 0;
            entity.LastUpdatedById = entity.CreatedById;
            try
            {
                using ( var context = new DataEntities() )
                {
                    //if ( ValidateProfile( entity, ref status ) == false )
                    //    return false;
                    //look up if no id
                    if ( entity.Id == 0 )
                    {
                        var record = GetByCourseIdAndTrainingTaskId( entity.HasCourseId, entity.HasTrainingTaskId );
                        if ( record?.Id > 0 )
                        {
                            entity.Id = record.Id;
                            return true;
                        }
                        else
                        {
                            //add
                            int newId = Add( entity, ref status );
                            if ( newId == 0 || status.HasSectionErrors )
                                isValid = false;
                            //just in case 
                            if ( newId > 0 )
                                UpdateParts( entity, status );
                            return isValid;
                        }
                    }
                    //update
                    //TODO - consider if necessary, or interferes with anything
                    context.Configuration.LazyLoadingEnabled = false;
                    DBEntity efEntity = context.CourseContext
                            .SingleOrDefault( s => s.Id == entity.Id );

                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        //fill in fields that may not be in entity
                        entity.RowId = efEntity.RowId;
                        entity.Created = efEntity.Created;
                        entity.CreatedById = ( efEntity.CreatedById ?? 0 );
                        entity.Id = efEntity.Id;

                        MapToDB( entity, efEntity, status );
                        bool hasChanges = false;
                        if ( HasStateChanged( context ) )
                        {
                            hasChanges = true;
                            efEntity.LastUpdated = DateTime.Now;
                            efEntity.LastUpdatedById = entity.LastUpdatedById;
                            count = context.SaveChanges();
                            //can be zero if no data changed
                            if ( count >= 0 )
                            {
                                entity.LastUpdated = ( DateTime ) efEntity.LastUpdated;
                                isValid = true;
                                //if ( hasChanges )
                                //{  }
                                    SiteActivity sa = new SiteActivity()
                                    {
                                        ActivityType = "CourseContext",
                                        Activity = status.Action,
                                        Event = "Update",
                                        Comment = string.Format( "CourseContext was updated by '{0}'.", status.Action ),
                                        ActionByUserId = entity.LastUpdatedById,
                                        ActivityObjectId = entity.Id
                                    };
                                    new ActivityManager().SiteActivityAdd( sa );
                              
                            }
                            else
                            {
                                //?no info on error

                                isValid = false;
                                string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a CourseContext. The process appeared to not work, but was not an exception, so we have no message, or no clue. HasCourseId: {0}, HasTrainingTaskId: {1}", entity.HasCourseId, entity.HasTrainingTaskId );
                                status.AddError( "Error - the update was not successful. " + message );
                                EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                            }
                        }

                        if ( isValid )
                        {
                            //just in case 
                            if ( entity.Id > 0 )
                                UpdateParts( entity, status );     
                        }
                    }
                    else
                    {
                        status.AddError( thisClassName + " Error - update failed, as record was not found." );
                    }
                }
            }
            catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
            {
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. HasCourseId: {0}, HasTrainingTaskId: {1}", entity.HasCourseId, entity.HasTrainingTaskId ), "CourseContext" );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save.HasCourseId: {0}, HasTrainingTaskId: {1}", entity.HasCourseId, entity.HasTrainingTaskId ), true );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
                isValid = false;
            }


            return isValid;
        }

        /// <summary>
        /// add a CourseContext
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private int Add( AppEntity entity, ref ChangeSummary status )
        {
            DBEntity efEntity = new DBEntity();
            status.HasSectionErrors = false;
            using ( var context = new DataEntities() )
            {
                try
                {
                    MapToDB( entity, efEntity, status );

                    if ( IsValidGuid( entity.RowId ) )
                        efEntity.RowId = entity.RowId;
                    else
                        efEntity.RowId = Guid.NewGuid();

                    entity.Created = efEntity.Created = DateTime.Now;
                    efEntity.CreatedById = efEntity.LastUpdatedById = entity.LastUpdatedById;
                    entity.LastUpdated = efEntity.LastUpdated = DateTime.Now;

                    context.CourseContext.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        entity.RowId = efEntity.RowId;
                        entity.Id = efEntity.Id;
                        //
                        //add log entry
                        //SiteActivity sa = new SiteActivity()
                        //{
                        //    ActivityType = "CourseContext",
                        //    Activity = status.Action,
                        //    Event = "Add",
                        //    Comment = string.Format( "CourseContext: '{0} was added.", entity.Name ),
                        //    ActivityObjectId = entity.Id,
                        //    ActionByUserId = entity.LastUpdatedById
                        //};
                        //new ActivityManager().SiteActivityAdd( sa );
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a CourseContext. The process appeared to not work, but was not an exception, so we have no message, or no clue.  HasCourseId: {0}, HasTrainingTaskId: {1}", entity.HasCourseId, entity.HasTrainingTaskId );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "CourseContext" );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. " + message );

                    LoggingHelper.LogError( message, true );
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), HasCourseId: {0}, HasTrainingTaskId: {1}", entity.HasCourseId, entity.HasTrainingTaskId ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }

            return efEntity.Id;
        }
        public static void MapToDB( AppEntity input, DBEntity output, ChangeSummary status )
        {
            status.HasSectionErrors = false;
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            //NOTE - NEED TO CHG FOR USE OF Has....
            BaseFactory.AutoMap( input, output, errors );
           

		}
		//

        #endregion

        #region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.CourseContext, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

        public static AppEntity GetByCourseIdAndTrainingTaskId( int courseId, int trainingTaskId, bool returnNullIfNotFound = false )
        {
			return GetSingleByFilter( m => m.HasCourseId == courseId && m.HasTrainingTaskId == trainingTaskId, returnNullIfNotFound );
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

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
		{
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.CourseContext.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m =>
						m.Course.CodedNotation.Contains( keywords ) ||
						m.Course.Name.Contains( keywords ) ||
						m.TrainingTask.Description.Contains( keywords )
					);
				}

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.Course.Name, m => m.OrderBy( n => n.Course.Name ) );

			}, MapFromDBForSearch );
		}
		//

		//Should only ever be one Course Context that has a specific combination of coded notation and training task text
		public static AppEntity GetForUploadOrNull( Guid courseRowID, Guid trainingTaskRowID )
		{
			using ( var context = new DataEntities() )
			{
				var match = context.CourseContext.FirstOrDefault( m =>
					m.Course.RowId == courseRowID &&
					m.TrainingTask.RowId == trainingTaskRowID
				);

				if ( match != null )
				{
					return MapFromDB( match, context );
				}
			}

			return null;
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
			output.HasTrainingTask = input.TrainingTask?.RowId ?? Guid.Empty;
			output.HasCourse = input.Course?.RowId ?? Guid.Empty;
			output.AssessmentMethodType = input.CourseContext_AssessmentType?.Select( m => m.ConceptScheme_Concept ).Select( m => m.RowId ).ToList() ?? new List<Guid>();

			return output;
		}

        #endregion

        /// <summary>
        /// Update:
        /// - training task
        /// - CourseContext types
        /// - CurrentAssessmentApproach
        /// </summary>
        /// <param name="input"></param>
        /// <param name="status"></param>
        public void UpdateParts( AppEntity input, ChangeSummary status )
        {
            try
            {
                //AssessmentMethod is passed as well
                //CourseContextAssessmentMethodSave( input, ref status )

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }

        #region CourseContext concepts
        /*        */
        public bool CourseContextAssessmentMethodSave( AppEntity entity, List<Guid> concepts, ref ChangeSummary status )
        {
            bool success = false;
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.CourseContext_AssessmentType();
            var entityType = "CourseContext_AssessmentType";

            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( concepts?.Count == 0 )
                        concepts = new List<Guid>();
                    //check existance
                    var existing = context.CourseContext_AssessmentType
                        .Where( s => s.CourseContextId == entity.Id )
                        .ToList();
                    if ( existing == null )
                        existing = new List<CourseContext_AssessmentType>();

                    #region deletes check
                    if ( existing.Any() )
                    {
                        //if exists not in input, delete it
                        foreach ( var e in existing )
                        {
                            var key = e?.ConceptScheme_Concept.RowId;
                            if ( IsValidGuid( key ) )
                            {
                                if ( !concepts.Contains( ( Guid ) key ) )
                                {
                                    context.CourseContext_AssessmentType.Remove( e );
                                    int dcount = context.SaveChanges();
                                }
                            }
                        }
                    }
                    #endregion
                    //adds
                    if ( concepts != null )
                    {
                        foreach ( var child in concepts )
                        {
                            //if not in existing, then add
                            bool doingAdd = true;
                            if ( existing?.Count > 0 )
                            {
                                var isfound = existing.Select( s => s.ConceptScheme_Concept.RowId == child ).ToList();
                                if ( !isfound.Any() )
                                    doingAdd = false;
                            }
                            if ( doingAdd )
                            {
                                var concept = ConceptManager.GetByRowId( child );
                                if ( concept?.Id > 0 )
                                {
                                    efEntity.CourseContextId = entity.Id;
                                    efEntity.AssessmentMethodConceptId = concept.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = entity.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.CourseContext_AssessmentType.Add( efEntity );

                                    int count = context.SaveChanges();
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For HasCourseId: {0}, HasTrainingTaskId: {1}) a CourseContext AssessmentMethod concept was not found for Identifier: {2}", entity.HasCourseId, entity.HasTrainingTaskId, child ) );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".CourseContextAssessmentMethodSave failed,  HasCourseId: {0}, HasTrainingTaskId: {1}", entity.HasCourseId, entity.HasTrainingTaskId ) );
                    status.AddError( thisClassName + ".CourseContextAssessmentMethodSave(). Error - the save was not successful. \r\n" + message );
                }
            }
            return success;
        }


        #endregion

    }

}
