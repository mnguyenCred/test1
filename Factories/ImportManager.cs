using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using ThisEntity = Data.Tables.ImportRMTL;

using Navy.Utilities;

namespace Factories
{
    public class ImportManager : BaseFactory
	{
		public static string thisClassName = "ImportManager";
		public static List<ThisEntity> ImportSearch( string pFilter, string sortOrder, int pageNumber, int pageSize, ref int pTotalRows )
		{
			string connectionString = DBConnectionRO();
			var credRegistryGraphUrl = UtilityManager.GetAppKeyValue( "credRegistryGraphUrl" );
			var env = UtilityManager.GetAppKeyValue( "environment" );
			var credentialFinderSite = UtilityManager.GetAppKeyValue( "credentialFinderSite" );

			//
			ThisEntity item = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			var result = new DataTable();

			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}

				using ( SqlCommand command = new SqlCommand( "[ImportSummarySearch]", c ) )
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add( new SqlParameter( "@Filter", pFilter ) );
					command.Parameters.Add( new SqlParameter( "@SortOrder", sortOrder ) );
					command.Parameters.Add( new SqlParameter( "@StartPageIndex", pageNumber ) );
					command.Parameters.Add( new SqlParameter( "@PageSize", pageSize ) );

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
						string rows = command.Parameters[4].Value.ToString();
						pTotalRows = Int32.Parse( rows );
					}
					catch ( Exception ex )
					{
						pTotalRows = 0;
						LoggingHelper.LogError( ex, thisClassName + string.Format( ".Search() - Execute proc, Message: {0} \r\n Filter: {1} \r\n", ex.Message, pFilter ) );

						item = new ThisEntity();
						item.Billet_Title = "Unexpected error encountered. System administration has been notified. Please try again later. ";
						item.Work_Element_Task = ex.Message;

						list.Add( item );
						return list;
					}
				}

				foreach ( DataRow dr in result.Rows )
				{
					item = new ThisEntity();
					//item.Id = GetRowColumn( dr, "Id", 0 );
					item.IndexIdentifier = GetRowColumn( dr, "IndexIdentifier", "" );
					item.Unique_Identifier = GetRowColumn( dr, "Unique_Identifier", 0 );
					item.Rating = GetRowColumn( dr, "Rating", "" );

					item.Rank = GetRowColumn( dr, "Rank", "" );
					item.RankLevel = GetRowColumn( dr, "RankLevel", "" );
					item.Billet_Title = GetRowColumn( dr, "Billet_Title" );

					item.Functional_Area = GetRowColumn( dr, "Functional_Area", "" );
					item.Source = GetRowColumn( dr, "Source", "" );
					item.Date_of_Source = GetRowColumn( dr, "Date_of_Source", "" );
					item.Work_Element_Type = GetRowColumn( dr, "Work_Element_Type", "" );
					item.Work_Element_Task = GetRowColumn( dr, "Work_Element_Task" );
					item.Task_Applicability = GetRowColumn( dr, "Task_Applicability", "" );
					item.Formal_Training_Gap = GetRowColumn( dr, "Formal_Training_Gap", "" );

					item.CIN = GetRowColumn( dr, "CIN", "" );
					item.Course_Name = GetRowColumn( dr, "Course_Name", "" );
					item.Course_Type = GetRowColumn( dr, "Course_Type" );
					item.Curriculum_Control_Authority = GetRowColumn( dr, "Curriculum_Control_Authority", "" );
					item.Life_Cycle_Control_Document = GetRowColumn( dr, "Life_Cycle_Control_Document", "" );
					item.Task_Statement = GetRowColumn( dr, "Task_Statement", "" );

					item.Current_Assessment_Approach = GetRowColumn( dr, "Current_Assessment_Approach", "" );
					item.TaskNotes = GetRowColumn( dr, "TaskNotes" );

					item.Message = GetRowColumn( dr, "Message", "" );

					list.Add( item );
				}

				return list;

			}
		} //
	}
}
