using System;
using System.Collections.Generic;
using System.Linq;

using Models.Application;
using Models.Curation;
using Models.Search;

using Navy.Utilities;

using AppEntity = Models.Schema.ClusterAnalysis;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.ClusterAnalysis;
using Data.Tables;

namespace Factories
{
    public class ClusterAnalysisManager : BaseFactory
    {
        public static new string thisClassName = "ClusterAnalysisManager";
		/*


        */
		#region ClusterAnalysis - persistance ==================
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
				BasicSaveCore( context, entity, context.ClusterAnalysis, userID, ( ent, dbEnt ) =>
				{
					dbEnt.HasRatingTaskId = context.RatingTask.FirstOrDefault( m => m.RowId == ent.HasRatingTask )?.Id ?? 0;
					dbEnt.HasRatingId = context.Rating.FirstOrDefault( m => m.RowId == ent.HasRating )?.Id ?? 0;
					dbEnt.BilletTitleId = context.Job.FirstOrDefault( m => m.RowId == ent.HasBilletTitle )?.Id ?? 0;
					dbEnt.WorkRoleId = context.WorkRole.FirstOrDefault( m => m.RowId == ent.HasWorkRole )?.Id ?? 0;
					dbEnt.HasClusterAnalysisTitleId = context.ClusterAnalysisTitle.FirstOrDefault( m => m.RowId == ent.HasClusterAnalysisTitle )?.Id ?? 0;
					dbEnt.TrainingSolutionTypeId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.TrainingSolutionType )?.Id ?? 0;
					dbEnt.RecommendedModalityTypeId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.RecommendedModalityType )?.Id ?? 0;
					dbEnt.DevelopmentSpecificationTypeId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.DevelopmentSpecificationType )?.Id ?? 0;
					dbEnt.DevelopmentRatioTypeId = context.ConceptScheme_Concept.FirstOrDefault( m => m.RowId == ent.DevelopmentRatioType )?.Id ?? 0;
				}, ( ent, dbEnt ) =>
				{
					HandleMultiValueUpdate( context, userID, ent.CandidatePlatformType, dbEnt, dbEnt.ClusterAnalysis_HasCandidatePlatform, context.ConceptScheme_Concept, nameof( ClusterAnalysis_HasCandidatePlatform.ClusterAnalysisId ), nameof( ClusterAnalysis_HasCandidatePlatform.CandidatePlatformConceptId ) );
					HandleMultiValueUpdate( context, userID, ent.CFMPlacementType, dbEnt, dbEnt.ClusterAnalysis_CFMPlacementType, context.ConceptScheme_Concept, nameof( ClusterAnalysis_CFMPlacementType.ClusterAnalysisId ), nameof( ClusterAnalysis_CFMPlacementType.CFMPlacementConceptId ) );
				}, saveType, AddErrorMethod );
			}
		}
		//

		/// <summary>
		/// Update a ClusterAnalysis
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
                //if ( entity.RatingTaskId < 1)
                //{
                //    var ratingTask = RatingTaskManager.Get( entity.RatingTaskRowId );
                //    if ( ratingTask?.Id > 0 )
                //        entity.RatingTaskId = ratingTask.Id;
                //    else
                //    {
                //        status.AddError( String.Format("A valid rating task identifier was not provided for Cluster Analysis record ({0}).", entity.RowId ));
                //        return false;
                //    }
                //}
                using ( var context = new DataEntities() )
                {
                    //if ( ValidateProfile( entity, ref status ) == false )
                    //    return false;
                    //look up if no id
                    if ( entity.Id == 0 )
                    {
                        //how to check for existing?
                        //just by rating task id for now
                        //this is more difficult now!
                        //var record = GetExisting( entity );
                        //if ( record?.Id > 0 )
                        //{
                        //    entity.Id = record.Id;
                        //    //could be other updates, fall thru to the update
                        //    //return true;
                        //}
                        //else
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
                    //TODO - ???
                    context.Configuration.LazyLoadingEnabled = false;
                    DBEntity efEntity = context.ClusterAnalysis
                            .FirstOrDefault( s => s.Id == entity.Id );

                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        //fill in fields that may not be in entity
                        entity.RowId = efEntity.RowId;
                        entity.Created = efEntity.Created;
                        entity.CreatedById = ( efEntity.CreatedById ?? 0 );
                        entity.Id = efEntity.Id;

                        MapToDB( entity, efEntity, status );

                        if ( HasStateChanged( context ) )
                        {
                            efEntity.LastUpdated = DateTime.Now;
                            efEntity.LastUpdatedById = entity.LastUpdatedById;
                            count = context.SaveChanges();
                            //can be zero if no data changed
                            if ( count >= 0 )
                            {
                                entity.LastUpdated = ( DateTime ) efEntity.LastUpdated;
                                isValid = true;
                            }
                            else
                            {
                                //?no info on error

                                isValid = false;
                                string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a ClusterAnalysis. The process appeared to not work, but was not an exception, so we have no message, or no clue. ClusterAnalysis: {0}, Id: {1}", entity.ClusterAnalysisTitle, entity.Id );
                                status.AddError( "Error - the update was not successful. " + message );
                                EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                            }

                        }

                        if ( isValid )
                        {
                            //just in case 
                            if ( entity.Id > 0 )
                                UpdateParts( entity, status );

                            SiteActivity sa = new SiteActivity()
                            {
                                ActivityType = "ClusterAnalysis",
                                Activity = status.Action,
                                Event = "Update",
                                Comment = string.Format( "ClusterAnalysis was updated by the import. Name: {0}", entity.ClusterAnalysisTitle ),
                                ActionByUserId = entity.LastUpdatedById,
                                ActivityObjectId = entity.Id
                            };
                            new ActivityManager().SiteActivityAdd( sa );
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
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, ClusterAnalysisTitle: {1}", entity.Id, entity.ClusterAnalysisTitle ), "ClusterAnalysis" );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.ClusterAnalysisTitle ), true );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
                isValid = false;
            }


            return isValid;
        }

        /// <summary>
        /// add a ClusterAnalysis
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

                    context.ClusterAnalysis.Add( efEntity );

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
                            ActivityType = "ClusterAnalysis",
                            Activity = status.Action,
                            Event = "Add",
                            Comment = string.Format( "ClusterAnalysis: '{0} was added.", entity.ClusterAnalysisTitle ),
                            ActivityObjectId = entity.Id,
                            ActionByUserId = entity.LastUpdatedById
                        };
                        new ActivityManager().SiteActivityAdd( sa );
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a ClusterAnalysis. The process appeared to not work, but was not an exception, so we have no message, or no clue. ClusterAnalysis: {0}, CodedNotation: {1}", entity.ClusterAnalysisTitle, entity.TrainingSolution );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "ClusterAnalysis" );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. " + message );

                    LoggingHelper.LogError( message, true );
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), ClusterAnalysisTitle:{0}, entity.TrainingSolutionType: {1}", entity.ClusterAnalysisTitle, entity.TrainingSolution ) );
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
            //
            BaseFactory.AutoMap( input, output, errors );
            //clean up Development ratio (pair off :00)
            //if ( output.DevelopmentRatio?.Length > 0 && output.DevelopmentRatio.EndsWith( ":00" )) 
            //{
            //    output.DevelopmentRatio = output.DevelopmentRatio.Substring(0, output.DevelopmentRatio.Length - 3);
            //}
            //if ( IsValidGuid( input.TrainingSolutionType ) )
            //{
            //    output.TrainingSolutionTypeId = ( int ) ConceptSchemeManager.GetConcept( input.TrainingSolutionTypeId )?.Id;
            //}
            if ( input.TrainingSolutionTypeId > 0 )
                output.TrainingSolutionTypeId = input.TrainingSolutionTypeId;

            else if ( !string.IsNullOrWhiteSpace( input.TrainingSolution ) )
            {
                output.TrainingSolutionTypeId = ( int ) ConceptManager.GetConceptFromScheme( ConceptSchemeManager.ConceptScheme_TrainingSolutionCategory, input.TrainingSolution )?.Id;
            }
            else
                output.TrainingSolutionTypeId = null;

            if ( input.RecommendedModalityTypeId > 0 )
                output.RecommendedModalityTypeId = input.RecommendedModalityTypeId;

            else if ( !string.IsNullOrWhiteSpace( input.RecommendedModality ) )
            {
                //
                output.RecommendedModalityTypeId = ( int ) ConceptManager.GetConceptFromScheme( ConceptSchemeManager.ConceptScheme_RecommendedModalityCategory, input.RecommendedModality )?.Id;
            }
            else
                output.RecommendedModalityTypeId = null;

            if ( input.DevelopmentSpecificationTypeId > 0 )
                output.DevelopmentSpecificationTypeId = input.DevelopmentSpecificationTypeId;

            else if ( !string.IsNullOrWhiteSpace( input.DevelopmentSpecification ) )
            {
                output.DevelopmentSpecificationTypeId = ( int ) ConceptManager.GetConceptFromScheme( ConceptSchemeManager.ConceptScheme_DevelopmentSpecificationCategory, input.DevelopmentSpecification )?.Id;
            }
            //if ( !string.IsNullOrWhiteSpace( input.CFMPlacement ) )
            //{
            //    output.CFMPlacementId = ( int ) ConceptSchemeManager.GetConcept( ConceptSchemeManager.ConceptScheme_CFMPlacement, input.CFMPlacement )?.Id;
            //}
            //confirm mapping happens for nullable fields
            output.EstimatedInstructionalTime = input.EstimatedInstructionalTime;
            output.DevelopmentTime = input.DevelopmentTime;
            output.PriorityPlacement = input.PriorityPlacement;
        }
        #endregion
        #region Retrieval

		public static AppEntity GetForUploadOrNull( Guid ratingRowID, Guid ratingTaskRowID, Guid billetTitleRowID, Guid workRoleRowID, Guid clusterAnalysisTitleRowID )
		{
			using( var context = new DataEntities() )
			{
				var match = context.ClusterAnalysis.FirstOrDefault( m =>
					m.Rating.RowId == ratingRowID &&
					m.RatingTask.RowId == ratingTaskRowID &&
					m.Job.RowId == billetTitleRowID &&
					m.WorkRole.RowId == workRoleRowID &&
					m.ClusterAnalysisTitle.RowId == clusterAnalysisTitleRowID
				);

				if( match != null )
				{
					return MapFromDB( match, context );
				}
			}

			return null;
		}
		//

		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.ClusterAnalysis, FilterMethod, MapFromDB, returnNullIfNotFound );
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
				var keywords = GetSanitizedSearchFilterKeywords( query );
				var list = context.ClusterAnalysis.AsQueryable();

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m => m.ClusterAnalysisTitle.Name.Contains( keywords ) );
				}

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.ClusterAnalysisTitle.Name, m => m.OrderBy( n => n.ClusterAnalysisTitle.Name ) );

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
			output.HasRating = input.Rating?.RowId ?? Guid.Empty;
			output.HasRatingTask = input.RatingTask?.RowId ?? Guid.Empty;
			output.HasBilletTitle = input.Job?.RowId ?? Guid.Empty;
			output.HasWorkRole = input.WorkRole?.RowId ?? Guid.Empty;
			output.HasClusterAnalysisTitle = input.ClusterAnalysisTitle?.RowId ?? Guid.Empty;
			output.TrainingSolutionType = input.ConceptScheme_Concept_TrainingSolutionType?.RowId ?? Guid.Empty;
			output.RecommendedModalityType = input.ConceptScheme_Concept_RecommendedModalityType?.RowId ?? Guid.Empty;
			output.DevelopmentSpecificationType = input.ConceptScheme_Concept_DevelopmentSpecificationType?.RowId ?? Guid.Empty;
			output.DevelopmentRatioType = input.ConceptScheme_Concept_DevelopmentRatioType?.RowId ?? Guid.Empty;
			output.CandidatePlatformType = input.ClusterAnalysis_HasCandidatePlatform?.Select( m => m.ConceptScheme_Concept ).Select( m => m.RowId ).ToList() ?? new List<Guid>();
			output.CFMPlacementType = input.ClusterAnalysis_CFMPlacementType?.Select( m => m.ConceptScheme_Concept ).Select( m => m.RowId ).ToList() ?? new List<Guid>();

			return output;
		}
		//

		#endregion

		/// <summary>
		/// Update:
		/// - training task
		/// - ClusterAnalysis types
		/// - CurrentAssessmentApproach
		/// </summary>
		/// <param name="input"></param>
		/// <param name="status"></param>
		public void UpdateParts( AppEntity input, ChangeSummary status )
        {
            try
            {
                /*
                ???????????????????????
                */

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }

 
    }

}
