using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
