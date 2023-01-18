
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using LumenWorks.Framework.IO.Csv;
using System.Diagnostics;
using System.IO;
//using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using Models.Schema;
using Navy.Utilities;
using System.Data;
using System.Data.SqlClient;
using Factories;

namespace AppTestProject
{
    [TestClass]
    public class BulkUpload
    {
		[TestMethod]
		public void TestMethod1()
		{
			string filepath = "D:\\azure\\source\\repos\\Navy\\import\\STG V1 -updated identifiers -first 3000.csv";
			string filepath2 = "D:\\azure\\source\\repos\\Navy\\import\\2022-24_Rating_upload_Sonar_Technician_(Surface)_102709.csv";

			List<string> messages = new List<string>();
			var connectionString = BaseFactory.MainConnection();
			SqlConnection con = new SqlConnection( connectionString );
			var rating = "STG";
			//truncate the destination
			//may want to do this by rating? for concurrent use
			var query1 = "truncate table [Import.RMTLStaging]";
			var query = string.Format("DELETE FROM [dbo].[Import.RMTLStaging] WHERE [Rating]='{0}'", rating);

			using ( SqlConnection connection = new SqlConnection( connectionString ) )
			{
				SqlCommand command = new SqlCommand( query, connection );
				command.Connection.Open();
				command.ExecuteNonQuery();
			}
			StreamReader sr = new StreamReader( filepath2 );
			string skipline = sr.ReadLine();
			string line = sr.ReadLine();
			string[] value = line.Split( ',' );
			DataTable dt = new DataTable();
			DataRow row;
			int cntr = 0;
			try
			{
				var headings = GetHeadings();
				foreach ( var item in headings )
				{
					dt.Columns.Add( new DataColumn( item ) );
				}
				//foreach ( string dc in value )
				//{
				//	//will need to normalize the column header
				//	//probably better to hardcode this
				//	var header = dc.Replace( " ", "_" ).Replace( "/", "-" ).Replace( "(", "" ).Replace( ")", "" );
				//	dt.Columns.Add( new DataColumn( dc ) );
				//}
				
				while ( !sr.EndOfStream )
				{
					cntr++;
					var fullLine = sr.ReadLine();
					value = sr.ReadLine().Split( ',' );
					//sometimes have the cluster analysis
					//C#8+. range operator
					//var dest = value[0..19];
					if ( value.Length > dt.Columns.Count )
					{
						var dest = new string[dt.Columns.Count];
						for ( int i = dest.GetLowerBound( 0 ); i <= dest.GetUpperBound( 0 ); i++ )
							dest.SetValue( "", i );
						// source,source-index,dest,dest-index,count
						Array.Copy( value, 0, dest, 0, 19 );
						value = dest;
					} else if ( value.Length < dt.Columns.Count )
					{
						//any issues with less than columns?
					}
					if ( value.Length == dt.Columns.Count )
					{
						row = dt.NewRow();
						var dest = new string[dt.Columns.Count];
						for ( int i = dest.GetLowerBound( 0 ); i <= dest.GetUpperBound( 0 ); i++ )
							dest.SetValue( "", i );
						var colNbr = -1;
						foreach ( var item in value )
                        {
							colNbr++;
							var output = item;
							if (item.IndexOf(",") > -1)
                            {
								output = "\"" + item + "\"";
                            }
							if ( output.IndexOf( "'" ) > -1 )
							{
								output = output.Replace("'","''");
							}
							dest[colNbr] = output;
						}
						row.ItemArray = dest;
						dt.Rows.Add( row );
					} else
                    {
						//log
                    }
					if (cntr == 10)
                    {
						break;
                    }
				}
			}
			catch ( Exception ex )
			{

			}

			try { 
			SqlBulkCopy bc = new SqlBulkCopy( con.ConnectionString, SqlBulkCopyOptions.TableLock );
				bc.DestinationTableName = "[Import.RMTLStaging]";
				bc.BatchSize = dt.Rows.Count;
				con.Open();
				bc.WriteToServer( dt );
				bc.Close();
				con.Close();

			} catch (Exception ex)
            {

            }

		}
	
		public List<string> GetHeadings()
		{
			/*
	

			*/
			var heading = new List<string>()
			{
			  "Unique_Identifier",
			  "Rating",
				"Rank",
				"Level_A_J_M" ,
				"Billet_Title",
				"Functional_Area",
				"Source",
				"Date_of_Source",
				"Work_Element_Type",
				"Work_Element_Task",
				"Task_Applicability",
				"Formal_Training_Gap",
				"CIN",
				"Course_Name",
				"Course_Type_A_C_G_F_T",
				"Curriculum_Control_Authority_CCA",
				"Life_Cycle_Control_Document",
				"CTTL_PPP_TCCD_Statement",
				"Current_Assessment_Approach",
			};
			return heading;
		}
		[TestMethod]
        public void TestMethod2()
        {
			string filepath = "D:\\azure\\source\\repos\\Navy\\import\\STG V1 -updated identifiers -first 3000.csv";
			string filepath2 = "D:\\azure\\source\\repos\\Navy\\import\\2022-24_Rating_upload_Sonar_Technician_(Surface)_102709.csv";

			List<string> messages = new List<string>();
			int cntr = 0;
			var output = new List<RatingTask>();
			var connectionString = BaseFactory.MainConnection();
			SqlConnection con = new SqlConnection( connectionString );
			var rating = "STG";
			//truncate the destination
			//may want to do this by rating? for concurrent use
			var query1 = "truncate table [Import.RMTLStaging]";
			var query2 = string.Format( "DELETE FROM [dbo].[Import.RMTLStaging] WHERE [Rating]='{0}'", rating );

			using ( SqlConnection connection = new SqlConnection( connectionString ) )
			{
				SqlCommand command = new SqlCommand( query1, connection );
				command.Connection.Open();
				command.ExecuteNonQuery();
			}
			DataTable dt = new DataTable();
			DataRow row;

			try
			{
				using ( CsvReader csv = new CsvReader( new StreamReader( filepath2, System.Text.Encoding.UTF7 ), true ) )
				{
					int fieldCount = csv.FieldCount;
					//need to skip the first row
					csv.ReadNextRecord();
					//doesn't work to get second header row
					csv.ReadNextRecord();
					string[] headers = csv.GetFieldHeaders();
					var headings = GetHeadings();
					foreach ( var item in headings )
					{
						dt.Columns.Add( new DataColumn( item ) );
					}
					//validate headers
					while ( csv.ReadNextRecord() )
					{
						cntr++;
						if (cntr == 6)
                        {
							//skip
							//continue;
                        }
						var entity = new RatingTask();

						//check for two header lines
						if ( cntr == 17 )
						{
							//break;
						}

						row = dt.NewRow();
						var dest = new string[dt.Columns.Count];
						for ( int i = dest.GetLowerBound( 0 ); i <= dest.GetUpperBound( 0 ); i++ )
							dest.SetValue( "", i );

						//use header columns rather than hard-code index numbers to enable flexibility
						for ( int i = 0; i < fieldCount; i++ )
						{
							if ( i >= dest.Length )
							{
								break;
							}
							Debug.Write( string.Format( "{0} = {1};", headings[i], csv[i] ) );
							Debug.WriteLine( "" );
							if ( i == 18 )
							{
								//skip
								//continue;
							}
							
							dest[i] = csv[i];


							//may want to make case insensitive!
							//var header = headers[i].ToLower();
							//OR
							var header = headings[i].ToLower();

							switch ( header )
							{
								case "rating":
									//will need to add an externalId property
									//entity.Note = csv[i];
									break;
								case "unique_identifier":
									//entity.CodedNotation = csv[i];
									break;
								case "rank":
									//entity.Identifier = csv[i];
									break;

								case "Work_Element_Task":
									entity.Description = csv[i];
									break;
								

								default:
									//action?
									LoggingHelper.DoTrace( 1, string.Format( "AppTestProject.TestMethod1. Unknown header {0}", headers[i] ) );
									break;
							}
						}
						row.ItemArray = dest;
						dt.Rows.Add( row );
						//
						output.Add(entity);

					}
				}

			}
			catch ( Exception ex )
			{
				LoggingHelper.DoTrace( 1, string.Format( "AppTestProject.TestMethod1. {0}", ex.Message ) );
			}

			try
			{
				SqlBulkCopy bc = new SqlBulkCopy( con.ConnectionString, SqlBulkCopyOptions.TableLock );
				bc.DestinationTableName = "[Import.RMTLStaging]";
				bc.BatchSize = dt.Rows.Count;
				con.Open();
				bc.WriteToServer( dt );
				bc.Close();
				con.Close();

			}
			catch ( Exception ex )
			{

			}

		}
	}
}
