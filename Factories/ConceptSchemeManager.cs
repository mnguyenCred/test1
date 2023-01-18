﻿using System;
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
        public static string ConceptScheme_CommentStatusCategory = "navy:CommentStatusCategory";
        public static string ConceptScheme_CourseCategory = "navy:CourseCategory";
        public static string ConceptScheme_AssessmentMethodCategory = "navy:AssessmentMethodCategory";
        public static string ConceptScheme_LifeCycleControlDocumentCategory = "navy:LifeCycleControlDocumentCategory";
        public static string ConceptScheme_PayGradeCategory = "navy:PayGradeCategory";
        public static string ConceptScheme_ProjectStatusCategory = "navy:ProjectStatusCategory";
        public static string ConceptScheme_PayGradeLevelCategory = "navy:PayGradeLevelCategory";
		public static string ConceptScheme_ReferenceResourceCategory = "navy:ReferenceResourceCategory";
        public static string ConceptScheme_TaskApplicabilityCategory = "navy:TaskApplicabilityCategory";
        public static string ConceptScheme_TrainingGapCategory = "navy:TrainingGapCategory";
        public static string ConceptScheme_TrainingSolutionCategory = "navy:TrainingSolutionCategory";
        public static string ConceptScheme_RecommendedModalityCategory = "navy:RecommendedModalityCategory";
        public static string ConceptScheme_DevelopmentSpecificationCategory = "navy:DevelopmentSpecificationCategory";
        public static string ConceptScheme_CFMPlacementCategory = "navy:CFMPlacementCategory";
		public static string ConceptScheme_CandidatePlatformTypeCategory = "navy:CandidatePlatformCategory";
		public static string ConceptScheme_DevelopmentRatioCategory = "navy:DevelopmentRatioCategory";


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

        #endregion

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
			return GetItemList( m => m.ConceptScheme.Where( n => n.SchemaUri != null ), MapFromDB );
		}
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			return GetMultipleByFilter( context => context.ConceptScheme, m => guids.Contains( m.RowId ), m => m.Name, false, MapFromDB, false );
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
				CommentStatusCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_CommentStatusCategory ),
				CourseCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_CourseCategory ),
				AssessmentMethodCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_AssessmentMethodCategory ),
				LifeCycleControlDocumentCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_LifeCycleControlDocumentCategory ),
				PayGradeCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_PayGradeCategory ),
				ProjectStatusCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_ProjectStatusCategory ),
				PayGradeLevelCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_PayGradeLevelCategory ),
				ReferenceResourceCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_ReferenceResourceCategory ),
				TaskApplicabilityCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_TaskApplicabilityCategory ),
				TrainingGapCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_TrainingGapCategory ),
				TrainingSolutionCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_TrainingSolutionCategory ),
				RecommendedModalityCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_RecommendedModalityCategory ),
				DevelopmentSpecificationCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_DevelopmentSpecificationCategory ),
				CandidatePlatformCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_CandidatePlatformTypeCategory ),
				DevelopmentRatioCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_DevelopmentRatioCategory ),
				CFMPlacementCategory = schemes.FirstOrDefault( scheme => scheme.SchemaUri == ConceptScheme_CFMPlacementCategory )
			};

			return result;
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

        #endregion

    }
}
