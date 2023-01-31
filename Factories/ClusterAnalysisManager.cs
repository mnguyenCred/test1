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

				//Exclude items
				AppendIDsFilterIfPresent( query, "search:Exclude", ( ids ) =>
				{
					list = list.Where( m => !ids.Contains( m.Id ) );
				} );

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.ClusterAnalysisTitle.Name, m => m.OrderBy( n => n.ClusterAnalysisTitle.Name ), ( m, keywordParts ) => m.OrderBy( n => RelevanceHelper( n, keywordParts, o => o.ClusterAnalysisTitle.Name ) ), keywords );

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
 
    }

}
