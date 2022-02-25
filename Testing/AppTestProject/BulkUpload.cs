
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using LumenWorks.Framework.IO.Csv;
using System.Diagnostics;
using System.IO;
//using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using Models.Schema;
using Navy.Utilities;

namespace AppTestProject
{
    [TestClass]
    public class BulkUpload
    {
        [TestMethod]
        public void TestMethod1()
        {
            string file = "D:\\azure\\source\\repos\\Navy\\import\\STG V1 -updated identifiers -first 3000.csv";

			List<string> messages = new List<string>();
			int cntr = 0;

			try
			{
				using ( CsvReader csv = new CsvReader( new StreamReader( file, System.Text.Encoding.UTF7 ), true ) )
				{
					int fieldCount = csv.FieldCount;
					//need to skip the first row
					string[] headers = csv.GetFieldHeaders();
					//validate headers
					while ( csv.ReadNextRecord() )
					{
						cntr++;
						if (cntr == 1)
                        {
							//skip
							continue;
                        }
						var entity = new RatingTask();

						//check for two header lines
						if ( cntr == 1 )
						{

						}
						//use header columns rather than hard-code index numbers to enable flexibility
						for ( int i = 0; i < fieldCount; i++ )
						{
							Debug.Write( string.Format( "{0} = {1};", headers[i], csv[i] ) );
							Debug.WriteLine( "" );


							//may want to make case insensitive!
							var header = headers[i].ToLower();
							switch ( header )
							{
								case "rating":
									//will need to add an externalId property
									entity.Note = csv[i];
									break;
								case "ctid":
								case "ceterms:ctid":
									entity.CTID = csv[i];
									break;

								case "description":
								case "ceterms:description":
									entity.Description = csv[i];
									break;
								

								default:
									//action?
									LoggingHelper.DoTrace( 1, string.Format( "RA_UnitTestProject.PublishSophiaTVP. Unknown header {0}", headers[i] ) );
									break;
							}
						}
						//
						

					}
				}

			}
			catch ( Exception ex )
			{
				LoggingHelper.DoTrace( 1, string.Format( "RA_UnitTestProject.PublishSophiaTVP. {0}", ex.Message ) );
			}
		}
    }
}
