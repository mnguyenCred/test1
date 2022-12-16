using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Curation;
using AppEntity = Models.Schema.Course;
using CourseTask = Models.Schema.TrainingTask;
using DBEntity = Data.Tables.Course;
using MSc = Models.Schema;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Models.Application;
using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class CourseManager : BaseFactory
    {
        public static new string thisClassName = "CourseManager";
		//public List<MSc.TrainingTask> AllNewtrainingTasks = new List<MSc.TrainingTask>();
		//public List<MSc.TrainingTask> AllUpdatedtrainingTasks = new List<MSc.TrainingTask>();
		#region Course - persistance ==================
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
				BasicSaveCore( context, entity, context.Course, userID, ( ent, dbEnt ) => {
					dbEnt.CurriculumControlAuthorityId = context.Organization.FirstOrDefault( m => m.RowId == ent.CurriculumControlAuthority )?.Id ?? 0;
					dbEnt.LifeCycleControlDocumentTypeId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.LifeCycleControlDocumentType )?.Id ?? 0;
				}, ( ent, dbEnt ) => {
					HandleMultiValueUpdate( context, userID, ent.CourseType, dbEnt, dbEnt.Course_CourseType, context.ConceptScheme_Concept, nameof( Course_CourseType.CourseId ), nameof( Course_CourseType.CourseTypeConceptId ) );
				}, saveType, AddErrorMethod );
			}
		}
		//

		/// <summary>
		/// Update a Course
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Save( AppEntity entity, ref ChangeSummary status )
        {
            bool isValid = true;
            int count = 0;
            //if (allNewtrainingTasks != null)
            //{
            //    AllNewtrainingTasks = allNewtrainingTasks;
            //}
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
                        var record = GetByCodedNotation( entity.CodedNotation );
                        if ( record?.Id > 0 )
                        {
                            entity.Id = record.Id;
                            //could be other updates, fall thru to the update
                            //return true;
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
                    //      - don't really want to include all training tasks
                    context.Configuration.LazyLoadingEnabled = false;
                    DBEntity efEntity = context.Course
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
                                        ActivityType = "Course",
                                        Activity = status.Action,
                                        Event = "Update",
                                        Comment = string.Format( "Course was updated by '{0}'. Name: {1}", status.Action, entity.Name ),
                                        ActionByUserId = entity.LastUpdatedById,
                                        ActivityObjectId = entity.Id
                                    };
                                    new ActivityManager().SiteActivityAdd( sa );
                              
                            }
                            else
                            {
                                //?no info on error

                                isValid = false;
                                string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a Course. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, Id: {1}", entity.Name, entity.Id );
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
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), "Course" );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), true );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
                isValid = false;
            }


            return isValid;
        }

        /// <summary>
        /// add a Course
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
                    if ( IsValidCtid( entity.CTID ) )
                        efEntity.CTID = entity.CTID;
                    else
                        efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                    entity.Created = efEntity.Created = DateTime.Now;
                    efEntity.CreatedById = efEntity.LastUpdatedById = entity.LastUpdatedById;
                    entity.LastUpdated = efEntity.LastUpdated = DateTime.Now;

                    context.Course.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        entity.RowId = efEntity.RowId;
                        entity.Id = efEntity.Id;
                        //
                        //add log entry
                        SiteActivity sa = new SiteActivity()
                        {
                            ActivityType = "Course",
                            Activity = status.Action,
                            Event = "Add",
                            Comment = string.Format( "Course: '{0} was added.", entity.Name ),
                            ActivityObjectId = entity.Id,
                            ActionByUserId = entity.LastUpdatedById
                        };
                        new ActivityManager().SiteActivityAdd( sa );
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a Course. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, CodedNotation: {1}", entity.Name, entity.CodedNotation );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Course" );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. " + message );

                    LoggingHelper.LogError( message, true );
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}, CodedNotation: {1}", entity.Name, entity.CodedNotation ) );
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
            //this will include the extra props (like LifeCycleControlDocument, etc. for now)
            BaseFactory.AutoMap( input, output, errors );
            /*
            if ( IsValidGuid( input.HasReferenceResource ) )
            {
                if ( output.LifeCycleControlDocumentId != null && output.ReferenceResource?.RowId == input.HasReferenceResource )
                {
                    //no action
                }
                else
                {
                    var entity = ReferenceResourceManager.Get( input.HasReferenceResource );
                    if ( entity?.Id > 0 )
                        output.LifeCycleControlDocumentId = ( int ) entity?.Id;
                    else
                    {
                        status.AddError( thisClassName + String.Format( ".MapToDB. Course: '{0}'. The related HasReferenceResource '{1}' was not found", input.Name, input.HasReferenceResource ) );
                    }
                }
            } else
            {
                output.LifeCycleControlDocumentId = null;
            }
            */
            //this may be removed if there can be multiple CCA
            //22-01-24 - Navy confirmed only one!
            if ( input.CurriculumControlAuthority != Guid.Empty )
            {
				var org = OrganizationManager.GetByRowId( input.CurriculumControlAuthority );
				if ( org != null && org.Id > 0 )
				{
					//TODO - now using Course.Organization
					output.CurriculumControlAuthorityId = org.Id;
					//only can handle one here
				}
				/*
				foreach ( var item in input.CurriculumControlAuthority )
                {
                    //all org adds will occur before here
                    var org = OrganizationManager.Get( item );
                    if ( org != null && org.Id > 0 )
                    {
                        //TODO - now using Course.Organization
                        output.CurriculumControlAuthorityId = org.Id;
                        //only can handle one here
                    }
                    else
                    {
                        //should not have a new org here
                        //NO, all new orgs will have been added first, so this would be an error
                    }
                }
				*/

            } else
            {
                output.CurriculumControlAuthorityId = null;
            }


            if ( IsValidGuid(input.LifeCycleControlDocumentType) )
            {
                output.LifeCycleControlDocumentTypeId = ( int ) ConceptManager.GetByRowId( input.LifeCycleControlDocumentType )?.Id;
            }
			else
			{
				output.LifeCycleControlDocumentTypeId = null;
			}

		}
        #endregion
        #region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.Course, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

        public static AppEntity GetByCodedNotation( string codedNotation, bool returnNullIfNotFound = false )
        {
			return GetSingleByFilter( m => m.CodedNotation?.ToLower() == codedNotation?.ToLower(), returnNullIfNotFound );
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

		public static AppEntity GetForUploadOrNull( string courseName, string courseCodedNotation, Guid curriculumControlAuthorityRowID, Guid lifeCycleControlDocumentTypeRowId )
		{
			using ( var context = new DataEntities() )
			{
				var match = context.Course.FirstOrDefault( m =>
					m.Name.ToLower() == courseName.ToLower() &&
					m.CodedNotation.ToLower() == courseCodedNotation.ToLower() &&
					context.Organization.FirstOrDefault( n => n.RowId == curriculumControlAuthorityRowID && n.Id == m.CurriculumControlAuthorityId ) != null &&
					context.ConceptScheme_Concept.FirstOrDefault( n => n.RowId == lifeCycleControlDocumentTypeRowId && n.Id == m.LifeCycleControlDocumentTypeId ) != null
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
		/// Get all 
		/// May need a get all for a rating? Should not matter as this is external data?
		/// </summary>
		/// <returns></returns>
		public static List<AppEntity> GetAll()
        {
            var entity = new AppEntity();
            var list = new List<AppEntity>();

            using ( var context = new DataEntities() )
            {
                var results = context.Course
                        .OrderBy( s => s.Name )
                        .ToList();

				foreach(var item in results )
				{
					list.Add( MapFromDB( item, context ) );
				}

            }
            return list;
        }
		//

        public static SearchResultSet<AppEntity> Search( SearchQuery query )
        {
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.Course.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m => 
						m.Name.Contains( keywords ) ||
						m.CodedNotation.Contains( keywords )
					);
				}

				//Organization Detail Page
				AppendIDsFilterIfPresent(query, "> CurriculumControlAuthorityId > Organization", ids =>
				{
					list = list.Where( m => ids.Contains( m.CurriculumControlAuthorityId ?? 0 ) );
				} );

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.Name, m => m.OrderBy( n => n.Name ) );

			}, MapFromDBForSearch );
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
			output.CurriculumControlAuthority = input.Organization?.RowId ?? Guid.Empty;
			output.LifeCycleControlDocumentType = input.ConceptScheme_Concept?.RowId ?? Guid.Empty;
			output.CourseType = input.Course_CourseType?.Select( m => m.ConceptScheme_Concept_CourseType ).Select( m => m.RowId ).ToList() ?? new List<Guid>();

			return output;
		}
		//

        #endregion

        /// <summary>
        /// Update:
        /// - training task
        /// - course types
        /// - CurrentAssessmentApproach
        /// </summary>
        /// <param name="input"></param>
        /// <param name="status"></param>
        public void UpdateParts( AppEntity input, ChangeSummary status )
        {
            try
            {
                //CourseTaskSave( input, ref status );
                //AssessmentMethod is passed as well
                new TrainingTaskManager().SaveList( input, ref status );

                //22-01-24 - CCA is confirmed to be a single
                //CurriculumControlAuthorityUpdate( input, ref status );
                //CourseConceptSave( input, ConceptSchemeManager.ConceptScheme_CourseType, input.CourseTypes, "CourseType", ref status );
                //CourseConceptSave( input, ConceptSchemeManager.ConceptScheme_CurrentAssessmentApproach, input.AssessmentMethodType, "AssessmentMethodType", ref status );
                CourseTypeSave( input, input.CourseType, ref status );

                //CourseAssessmentMethodSave( input, input.AssessmentMethodType, ref status );


            } catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }

        #region Course concepts
        /*
        public bool CourseAssessmentMethodSave( AppEntity input, List<Guid> concepts, ref ChangeSummary status )
        {
            bool success = false;
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.Course_AssessmentType();
            var entityType = "Course_AssessmentType";

            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( concepts?.Count == 0 )
                        concepts = new List<Guid>();
                    //check existance
                    var existing = context.Course_AssessmentType
                        .Where( s => s.CourseId == input.Id )
                        .ToList();
                    if ( existing == null )
                        existing = new List<Course_AssessmentType>();

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
                                    context.Course_AssessmentType.Remove( e );
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
                                var concept = ConceptSchemeManager.GetConcept( child );
                                if ( concept?.Id > 0 )
                                {
                                    efEntity.CourseId = input.Id;
                                    efEntity.AssessmentMethodConceptId = concept.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.Course_AssessmentType.Add( efEntity );

                                    int count = context.SaveChanges();
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For Course: '{0}' ({1}) a Course AssessmentMethod concept was not found for Identifier: {2}", input.Name, input.Id, child ) );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".CourseAssessmentMethodSave failed, Course: '{0}' ({1})", entityType, input.Name, input.Id ) );
                    status.AddError( thisClassName + ".CourseAssessmentMethodSave(). Error - the save was not successful. \r\n" + message );
                }
            }
            return success;
        }
        */
        //
        public bool CourseTypeSave( AppEntity input, List<Guid> concepts, ref ChangeSummary status )
        {
            bool success = false;
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.Course_CourseType();
            var entityType = "Course_CourseType";

            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( concepts?.Count == 0 )
                        concepts = new List<Guid>();
                    //check existance
                    var results =
                                    from entity in context.Course_CourseType
                                    join concept in context.ConceptScheme_Concept
                                    on entity.CourseTypeConceptId equals concept.Id
                                    where entity.CourseId == input.Id

                                    select concept;
                    //if ( existing == null )
                    //    existing = new List<ConceptScheme_Concept>();  
                    var existing = results?.ToList();
                    #region deletes check
                    if ( existing != null && existing.Count() > 0 )
                    {
                        //if exists not in input, delete it
                        foreach ( var e in existing )
                        {
                            var key = e.RowId;
                            if ( IsValidGuid( key ) )
                            {
                                if ( !concepts.Contains( ( Guid ) key ) )
                                {
                                    //DeleteCourseType( input.Id, e.Id, ref status );
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
                            if ( existing != null && existing.Count() > 0 )
                            {
                                foreach ( var item in existing )
                                {
                                    if ( item.RowId == child )
                                    {
                                        doingAdd = false;
                                        break;
                                    }
                                }                                    
                            }
                            if ( doingAdd )
                            {
                                var concept = ConceptManager.GetByRowId( child );
                                if ( concept?.Id > 0 )
                                {
                                    efEntity.CourseId = input.Id;
                                    efEntity.CourseTypeConceptId = concept.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.Course_CourseType.Add( efEntity );

                                    int count = context.SaveChanges();
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For Course: '{0}' ({1}) a Course CourseType was not found for Identifier: {2}", input.Name, input.Id, child ) );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".CourseCourseTypeSave failed, Course: '{0}' ({1})", entityType, input.Name, input.Id ) );
                    status.AddError( thisClassName + ".CourseCourseTypeSave(). Error - the save was not successful. \r\n" + message );
                }
            }
            return success;
        }
        public bool DeleteCourseType( int courseId, int conceptId, ref ChangeSummary status )
        {
            bool isValid = false;
            if ( conceptId == 0 )
            {
                //statusMessage = "Error - missing an identifier for the CourseConcept to remove";
                return false;
            }

            using ( var context = new DataEntities() )
            {
                var efEntity = context.Course_CourseType
                                .FirstOrDefault( s => s.CourseId == courseId && s.CourseTypeConceptId == conceptId);

                if ( efEntity != null && efEntity.Id > 0 )
                {
                    context.Course_CourseType.Remove( efEntity );
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                    }
                }
                else
                {
                    //statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
                    isValid = true;
                }
            }

            return isValid;
        }

        #endregion

    }

}
