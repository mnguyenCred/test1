using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Application;
using Models.Curation;

using AppEntity = Models.Schema.ConceptScheme;
using Concept = Models.Schema.Concept;
using DBEntity = Data.Tables.ConceptScheme;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class ConceptSchemeManager : BaseFactory
    {
        public static new string thisClassName = "ConceptSchemeManager";
        //
        public static string ConceptScheme_CommentStatus = "navy:CommentStatus";
        public static string ConceptScheme_CourseType = "navy:CourseType";
        public static string ConceptScheme_CurrentAssessmentApproach = "navy:CurrentAssessmentApproach";
        public static string ConceptScheme_LifeCycleControlDocument = "navy:LifeCycleControlDocument";
        public static string ConceptScheme_Pay_Grade = "navy:Paygrade";
        public static string ConceptScheme_ProjectStatus = "navy:ProjectStatus";
        public static string ConceptScheme_PayGradeLevel = "navy:PayGradeLevel";
        public static string ConceptScheme_ReferenceResource = "navy:ReferenceResource";
        public static string ConceptScheme_TaskApplicability = "navy:TaskApplicability";
        public static string ConceptScheme_TrainingGap = "navy:TrainingGap";

        public static string ConceptScheme_TrainingSolutionType = "navy:TrainingSolutionType";
        public static string ConceptScheme_RecommendedModality = "navy:RecommendedModality";
        public static string ConceptScheme_DevelopmentSpecification = "navy:DevelopmentSpecification";
        public static string ConceptScheme_CFMPlacement = "navy:CFMPlacement";
		public static string ConceptScheme_CandidatePlatformType = "navy:CandidatePlatformType";
		public static string ConceptScheme_DevelopmentRatio = "navy:DevelopmentRatio"; //??


		#region ConceptScheme
		#region ConceptScheme - Persistance
		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			SaveCore( entity, userID, "Edit", errors.Add );
		}
		//

		private static void SaveCore( AppEntity entity, int userID, string saveType, Action<string> AddErrorMethod )
		{
			using ( var context = new DataEntities() )
			{
				BasicSaveCore( context, entity, context.ConceptScheme, userID, ( ent, dbEnt ) => { }, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//


		//unlikely
		public bool Save( AppEntity entity, ref ChangeSummary status )
        {
            bool isValid = true;
            int count = 0;

            try
            {
                using ( var context = new DataEntities() )
                {
                    //look up if no id
                    if ( entity.Id == 0 )
                    {
                        //add
                        int newId = Add( entity, ref status );
                        if ( newId == 0 || status.HasSectionErrors )
                            isValid = false;

                        return isValid;

                    }
                    //update
                    //TODO - consider if necessary, or interferes with anything
                    //      - don't really want to include all training tasks
                    context.Configuration.LazyLoadingEnabled = false;
                    var efEntity = context.ConceptScheme
                            .SingleOrDefault( s => s.Id == entity.Id );

                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        //fill in fields that may not be in entity
                        entity.RowId = efEntity.RowId;
                        entity.Created = efEntity.Created;
                        entity.CreatedById = efEntity.CreatedById ?? 0;
                        entity.Id = efEntity.Id;

                        MapToDB( entity, efEntity );

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
                                SiteActivity sa = new SiteActivity()
                                {
                                    ActivityType = "ConceptScheme",
                                    Activity = status.Action,
                                    Event = "Update",
                                    Comment = string.Format( "ConceptScheme was updated. Name: {0}", entity.Name ),
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

            
                    }
                    else
                    {
                        status.AddError( "Error - update failed, as record was not found." );
                    }


                }
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
        //placeholder, as can't (yet) add a new concept scheme
        private int Add( AppEntity entity, ref ChangeSummary status )
        {
            var efEntity = new Data.Tables.ConceptScheme_Concept();
           
            return 0;
        }
        #endregion


		public static AppEntity GetByIdentifier( string identifier, bool returnNullIfEmpty = false )
		{
			return GetByIdentifier<DBEntity, AppEntity>( identifier, context => context.ConceptScheme, list => { 
				return list.FirstOrDefault( 
					item => item.Name.ToLower() == identifier.ToLower() || 
					item.SchemaUri.ToLower() == identifier.ToLower() 
				); 
			}, MapFromDB, returnNullIfEmpty );
		}
		//

		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.ConceptScheme, FilterMethod, MapFromDB, returnNullIfNotFound );
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

        public static AppEntity GetByName( string name, bool returnNullIfNotFound = false )
        {
			return GetSingleByFilter( m => m.Name?.ToLower() == name?.ToLower(), returnNullIfNotFound );
        }
		//

		/// <summary>
		/// Use this with the static strings at the top of this class beginning with "ConceptScheme_"
		/// </summary>
		/// <param name="shortURI"></param>
		/// <returns></returns>
		public static AppEntity GetbyShortUri( string shortURI, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.SchemaUri?.ToLower() == shortURI?.ToLower(), returnNullIfNotFound );
		}
		//

		public static List<AppEntity> GetAll( bool includeConcepts = true )
		{
			return GetItemList<DBEntity, AppEntity>( m => m.ConceptScheme.Where( n => n.SchemaUri != null ), MapFromDB );
		}
		//

		/// <summary>
		/// Get all Concept Schemes as a mapped-out object to make it easier to work with them<br />
		/// As with the rest of the code that references specific concept schemes, this will need to be updated if more are added/changed!
		/// </summary>
		/// <param name="includingConcepts"></param>
		/// <returns></returns>
		public static Models.Schema.ConceptSchemeMap GetConceptSchemeMap( bool includingConcepts = true )
		{
			var schemes = GetAll( includingConcepts );
			var result = new Models.Schema.ConceptSchemeMap()
			{
				AllConceptSchemes = schemes,
				CommentStatusCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_CommentStatus ),
				CourseCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_CourseType ),
				AssessmentMethodCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_CurrentAssessmentApproach ),
				LifeCycleControlDocumentCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_LifeCycleControlDocument ),
				PayGradeCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_Pay_Grade ),
				ProjectStatusCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_ProjectStatus ),
				PayGradeLevelCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_PayGradeLevel ),
				ReferenceResourceCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_ReferenceResource ),
				TaskApplicabilityCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_TaskApplicability ),
				TrainingGapCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_TrainingGap ),
				TrainingSolutionCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_TrainingSolutionType ),
				RecommendedModalityCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_RecommendedModality ),
				DevelopmentSpecificationCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_DevelopmentSpecification ),
				CandidatePlatformCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_CandidatePlatformType ),
				DevelopmentRatioCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_DevelopmentRatio ),
				CFMPlacementCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_CFMPlacement )
			};

			return result;
		}
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			var results = new List<AppEntity>();

			using ( var context = new DataEntities() )
			{
				var items = context.ConceptScheme
					.Where( m => guids.Contains( m.RowId ) )
					.OrderBy( m => m.Description )
					.ToList();

				foreach ( var item in items )
				{
					results.Add( MapFromDB( item, context ) );
				}
			}

			return results;
		}
		//

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
        {
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.ConceptScheme.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m => m.Name.Contains( keywords )	);
				}

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

			foreach ( var item in input.ConceptScheme_Concept.Where( m => m.IsActive ).OrderBy( m => m.ListId ).ThenBy( m => m.Name ).ToList() )
			{
				output.Concepts.Add( ConceptManager.MapFromDB( item, context ) );
			}

			return output;
        }
		//

        public static void MapToDB( AppEntity input, DBEntity output )
        {
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            //
        }
		//

        #endregion

    }
}
