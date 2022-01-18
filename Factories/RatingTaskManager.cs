using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Application;
using Models.Import;
using AppEntity = Models.Schema.RatingTask;
using EntitySummary = Models.Schema.RatingTaskSummary;
using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using DBEntity = Data.Tables.RatingTask;

using Navy.Utilities;


namespace Factories
{
    public class RatingTaskManager : BaseFactory
    {
        public static string thisClassName = "RatingTaskManager";

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
        /// <param name="includingConcepts"></param>
        /// <returns></returns>
        public static AppEntity Get( AppEntity importEntity )
        {
            var entity = new AppEntity();
            //will probably have to d

            using ( var context = new ViewContext() )
            {
                var item = context.RatingTaskSummary
                            .FirstOrDefault( s => s.PayGradeType == importEntity.PayGradeType
                            && s.FunctionalAreaUID == importEntity.ReferenceType
                            && s.HasReferenceResource == importEntity.HasReferenceResource
                            && s.RatingTask.ToLower() == importEntity.Description.ToLower()
                            );

                if ( item != null && item.Id > 0 )
                {
                    //if exists, will just return the Id?
                    //or do a get, and continue?
                    entity = Get( item.Id, true );
                }
            }

            return entity;
        }

        public static AppEntity Get( ImportRMTL importEntity )
        {
            var entity = new AppEntity();
            //will probably have to d

            using ( var context = new ViewContext() )
            {
                var item = context.RatingTaskSummary
                            .FirstOrDefault( s => s.Rank == importEntity.Rank
                            && s.FunctionalArea == importEntity.Functional_Area
                            && s.ReferenceResource == importEntity.Source
                            && s.RatingTask.ToLower() == importEntity.Work_Element_Task.ToLower()
                            );

                if ( item != null && item.Id > 0 )
                {
                    //if exists, will just return the Id?
                    //or do a get, and continue?
                    entity = Get( item.Id, true );
                }
            }

            return entity;
        }

        public static AppEntity Get( int id, bool includingConcepts )
        {
            var entity = new AppEntity();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.RatingTask
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingConcepts );
                }
            }

            return entity;
		}

		public static AppEntity Get( Guid rowId, bool includingConcepts = false )
		{
			var entity = new AppEntity();

			using ( var context = new DataEntities() )
			{
				var item = context.RatingTask
							.FirstOrDefault( s => s.RowId == rowId );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity, includingConcepts );
				}
			}
			return entity;
		}


		/// <summary>
		/// assuming the rating code
		/// could use an or to also handle name
		/// </summary>
		/// <param name="rating"></param>
		/// <param name="pOrderBy"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <param name="userId"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
		public static List<EntitySummary> SearchForRating( string rating, string pOrderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows )
		{
			var keyword = HandleApostrophes( rating );
			string filter = String.Format( "base.id in (select a.[RatingTaskId] from [RatingTask.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation = '{0}' OR b.name = '{0}' )", keyword );
			string orderBy = "";


			return Search( filter, orderBy, pageNumber, pageSize, userId, ref pTotalRows );
			
		}

		public static List<EntitySummary> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows, bool autocomplete = false )
		{
			string connectionString = DBConnectionRO();
			EntitySummary item = new EntitySummary();
			List<EntitySummary> list = new List<EntitySummary>();
			var result = new DataTable();


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
						item.Description = "Unexpected error encountered. System administration has been notified. Please try again later. ";
						item.Description = ex.Message;

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
						item.Ratings = GetRowColumn( dr, "Ratings", "" );
						//do we need to populate HasRating (if so, could include in the pipe separated list of Ratings
						item.BilletTitles = GetRowColumn( dr, "BilletTitles", "" );
						//similarly, do we need a list of billet guids?

						item.Description = GetRowColumn( dr, "RatingTask", "" );
						item.Note = GetRowColumn( dr, "Notes", "" );
						item.CTID = GetRowPossibleColumn( dr, "CTID", "" );
						//
						item.Created = GetRowColumn( dr, "Created", DateTime.MinValue );
						item.Creator = GetRowPossibleColumn( dr, "CreatedBy","" );
						item.CreatedBy = GetGuidType( dr, "CreatedByUID" );
						item.LastUpdated = GetRowColumn( dr, "LastUpdated", DateTime.MinValue );
						item.ModifiedBy = GetRowPossibleColumn( dr, "ModifiedBy", "" );
						item.LastUpdatedBy = GetGuidType( dr, "ModifiedByUID" );

						item.CodedNotation = GetRowPossibleColumn( dr, "CodedNotation", "" );
						//
						item.Rank = GetRowPossibleColumn( dr, "Rank", "" );
						item.PayGradeType = GetGuidType( dr, "PayGradeType" );
						//
						item.Level = GetRowPossibleColumn( dr, "Level", "" );
						//
						item.FunctionalArea = GetRowColumn( dr, "FunctionalArea", "" );
						item.ReferenceType = GetGuidType( dr, "ReferenceType" );
						//
						item.Source = GetRowColumn( dr, "Source", "" );
						item.SourceDate = GetRowColumn( dr, "SourceDate", "" );
						item.HasReferenceResource = GetGuidType( dr, "HasReferenceResource" );
						//
						item.WorkElementType = GetRowPossibleColumn( dr, "WorkElementType", "" );
						item.ReferenceType = GetGuidType( dr, "ReferenceType" );
						//
						item.TaskApplicability = GetRowPossibleColumn( dr, "TaskApplicability", "" );
						item.ApplicabilityType = GetGuidType( dr, "ApplicabilityType" );
						//
						item.FormalTrainingGap = GetRowPossibleColumn( dr, "FormalTrainingGap", "" );
						item.TrainingGapType = GetGuidType( dr, "TrainingGapType" );

						item.CIN = GetRowColumn( dr, "CIN", "" );
						item.CourseName = GetRowColumn( dr, "CourseName", "" );
						item.CourseType = GetRowPossibleColumn( dr, "CourseType", "" );
						item.CurrentAssessmentApproach = GetRowPossibleColumn( dr, "CurrentAssessmentApproach", "" );
						//
						item.TrainingTask = GetRowPossibleColumn( dr, "TrainingTask", "" );
						item.HasTrainingTask = GetGuidType( dr, "HasTrainingTask" );
						//
						item.CurriculumControlAuthority = GetRowPossibleColumn( dr, "CurriculumControlAuthority", "" );
						item.LifeCycleControlDocument = GetRowPossibleColumn( dr, "LifeCycleControlDocument", "" );


						list.Add( item );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.DoTrace( 1, thisClassName + ".Search. Exception: " + ex.Message );
				}
				return list;

			}
		} //


		public static void MapFromDB( DBEntity input, AppEntity output, bool includingConcepts )
        {
            //test automap
            List<string> errors = new List<string>();
            BaseFactory.AutoMap(input, output, errors );


            //output.Id = input.Id;
            //the status may have to specific to the project - task context?
			//Yes, this would be specific to a project
            //output.StatusId = input.TaskStatusId ?? 1;
            //output.RowId = input.RowId;

            //output.Description = input.WorkElementTask;
            //
            //output.TaskApplicabilityId = input.TaskApplicabilityId;
            if ( input.TaskApplicabilityId > 0 )
            {
                ConceptSchemeManager.MapFromDB( input.ConceptScheme_Applicability, output.TaskApplicabilityType );
                output.ApplicabilityType = (output.TaskApplicabilityType)?.RowId ?? Guid.Empty;
                //OR
                //output.ApplicabilityType = ConceptSchemeManager.MapConcept( input.ConceptScheme_Applicability )?.RowId ?? Guid.Empty;

			}
            if ( input.FormalTrainingGapId > 0 )
            {
                ConceptSchemeManager.MapFromDB( input.ConceptScheme_TrainingGap, output.TaskTrainingGap );
                output.TrainingGapType = ( output.TaskTrainingGap )?.RowId ?? Guid.Empty;
                //OR
                //output.TrainingGapType = ConceptSchemeManager.MapConcept( input.ConceptScheme_TrainingGap )?.RowId ?? Guid.Empty;
			}
        }

        #endregion
        #region === persistance ==================
        public bool Save( ImportRMTL input, ref SaveStatus status )
        {
            bool isValid = true;
            int count = 0;
            try
            {
                using ( var context = new DataEntities() )
                {
                    //if ( ValidateProfile( entity, ref status ) == false )
                    //    return false;
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
        public bool Save( AppEntity input, ref SaveStatus status )
        {
            bool isValid = true;
            int count = 0;
            try
            {
                using ( var context = new DataEntities() )
                {
                    //if ( ValidateProfile( entity, ref status ) == false )
                    //    return false;
                    //look up if no id
                    if ( input.Id == 0 )
                    {
                        //need to identify for sure what is unique
                        var record = Get( input );
                        if ( record?.Id > 0 )
                        {
                            //
                            input.Id = record.Id;
                            UpdateParts( input, status );
                            //??
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
                            input.RowId = efEntity.RowId;
                            input.Created = efEntity.Created;
                            input.CreatedById = ( efEntity.CreatedById ?? 0 );
                            input.Id = efEntity.Id;

                            MapToDB( input, efEntity );

                            if ( HasStateChanged( context ) )
                            {
                                efEntity.LastUpdated = DateTime.Now;
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
                                    string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a RatingTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingTask: {0}, Id: {1}", input.Description, input.Id );
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
                                    Comment = string.Format( "RatingTask was updated by the import. Name: {0}", input.Description ),
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
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", input.Id, input.Description ), "RatingTask" );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", input.Id, input.Description ), true );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
                isValid = false;
            }


            return isValid;
        }
        private int Add( ImportRMTL input, ref SaveStatus status )
        {
            DBEntity efEntity = new DBEntity();
            using ( var context = new DataEntities() )
            {
                try
                {
                    MapToDB( input, efEntity );

                    //if ( IsValidGuid( input.RowId ) )
                    //    efEntity.RowId = input.RowId;
                    //else
                        efEntity.RowId = Guid.NewGuid();
                    efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                    input.ImportDate = efEntity.LastUpdated = efEntity.Created = DateTime.Now;

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

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a RatingTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingTask: {0}", input.Work_Element_Task );
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
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Description: {0}, CTID: {1}", efEntity.Description, efEntity.CTID ) );
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
        private int Add( AppEntity entity, ref SaveStatus status )
        {
            DBEntity efEntity = new DBEntity();
            using ( var context = new DataEntities() )
            {
                try
                {
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

                    context.RatingTask.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        entity.RowId = efEntity.RowId;
                        entity.Id = efEntity.Id;
                        UpdateParts( entity, status );
                        //
                        //add log entry
                        SiteActivity sa = new SiteActivity()
                        {
                            ActivityType = "RatingTask",
                            Activity = "Import",
                            Event = "Add",
                            Comment = string.Format( " A RatingTask was added by the import. Desc: {0}", entity.Description ),
                            ActivityObjectId = entity.Id
                        };
                        new ActivityManager().SiteActivityAdd( sa );


                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a RatingTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingTask: {0}, ctid: {1}", entity.Description, entity.CTID );
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
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Description: {0}, CTID: {1}", efEntity.Description, efEntity.CTID ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }

            return efEntity.Id;
        }
        public void UpdateParts( ImportRMTL input, SaveStatus status )
        {
            try
            {


            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }
        public void UpdateParts( AppEntity input, SaveStatus status )
        {
            try
            {
                //RatingTask.HasRating

                //RatingTask.HasJob

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }

        public static void MapToDB( AppEntity input, DBEntity output )
        {
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );

        }
        public static void MapToDB( ImportRMTL input, DBEntity output )
        {
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            //not much value for this?
            //BaseFactory.AutoMap( input, output, errors );

            output.Description = input.Work_Element_Task;

        }
        #endregion

    }
}
