using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppEntity = Models.Schema.RatingTask;
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
        public static AppEntity Get( int id )
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
                    MapFromDB( item, entity );
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
		public static List<AppEntity> SearchForRating( string rating, string pOrderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows )
		{
			var keyword = HandleApostrophes( rating );
			string filter = String.Format( "base.id in (select a.[RatingTaskId] from [RatingTask.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation = '{0}' OR b.name = '{0}' )", keyword );
			string orderBy = "";


			return Search( filter, orderBy, pageNumber, pageSize, userId, ref pTotalRows );
			
		}

		public static List<AppEntity> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows, bool autocomplete = false )
		{
			string connectionString = DBConnectionRO();
			AppEntity item = new AppEntity();
			List<AppEntity> list = new List<AppEntity>();
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

						item = new AppEntity();
						item.Description = "Unexpected error encountered. System administration has been notified. Please try again later. ";
						item.Description = ex.Message;

						list.Add( item );
						return list;
					}
				}

				try
				{
					foreach ( DataRow dr in result.Rows )
					{
						item = new AppEntity();

						item.Id = GetRowColumn( dr, "Id", 0 );
						item.RowId = GetGuidType( dr, "RowId" );
						item.Description = GetRowColumn( dr, "Description", "" );
						item.Note = GetRowColumn( dr, "Notes", "" );
						item.CTID = GetRowPossibleColumn( dr, "CTID", "" );
						item.Created = GetRowColumn( dr, "Created", DateTime.MinValue );
						item.LastUpdated = GetRowColumn( dr, "LastUpdated", DateTime.MinValue );
						item.CodedNotation = GetRowPossibleColumn( dr, "CodedNotation", "" );
						item.Rank = GetRowPossibleColumn( dr, "Rank", "" );
						item.Level = GetRowPossibleColumn( dr, "Level", "" );
						item.FunctionalArea = GetRowPossibleColumn( dr, "FunctionalArea", "" );
						item.Source = GetRowPossibleColumn( dr, "Source", "" );
						item.SourceDate = GetRowPossibleColumn( dr, "SourceDate", "" );
						item.WorkElementType = GetRowPossibleColumn( dr, "WorkElementType", "" );
						item.TaskApplicability = GetRowPossibleColumn( dr, "TaskApplicability", "" );
						item.FormalTrainingGap = GetRowPossibleColumn( dr, "FormalTrainingGap", "" );

						item.CIN = GetRowPossibleColumn( dr, "CIN", "" );
						item.CourseName = GetRowPossibleColumn( dr, "CourseName", "" );
						item.CourseType = GetRowPossibleColumn( dr, "CourseType", "" );
						item.CurrentAssessmentApproach = GetRowPossibleColumn( dr, "CurrentAssessmentApproach", "" );
						item.TrainingTask = GetRowPossibleColumn( dr, "TrainingTask", "" );
						item.CurriculumControlAuthority = GetRowPossibleColumn( dr, "CurriculumControlAuthority", "" );
						item.LifeCycleControlDocument = GetRowPossibleColumn( dr, "LifeCycleControlDocument", "" );

						item.PayGradeType = GetGuidType( dr, "PayGradeType" );
						item.ReferenceType = GetGuidType( dr, "ReferenceType" );
						item.TrainingGapType = GetGuidType( dr, "TrainingGapType" );
						item.ReferenceType = GetGuidType( dr, "ReferenceType" );
						item.HasReferenceResource = GetGuidType( dr, "HasReferenceResource" );
						item.ApplicabilityType = GetGuidType( dr, "ApplicabilityType" );
						item.HasTrainingTask = GetGuidType( dr, "HasTrainingTask" );


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

		public static Guid GetGuidType( DataRow dr, string property)
		{
			string guid = GetRowColumn( dr, property );
			if ( !string.IsNullOrEmpty( guid ) )
				return new Guid( guid );
			else
				return new Guid();
		} //

		public static void MapFromDB( DBEntity input, AppEntity output )
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
