using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DT= Data.Tables;
using Factories;
using Models.Import;
namespace Services
{
    public class ImportServices
    {

		public static List<DT.ImportRMTL> ImportSummarySearch( BaseSearchModel parms, ref int pTotalRows )
		{

			//probably should validate valid order by - or do in proc
			if ( string.IsNullOrWhiteSpace( parms.OrderBy ) )
			{
				parms.IsDescending = true;
				parms.OrderBy = "IndexIdentifier";
			}
			else
			{
				if ( "id dataownername entityname publishername publishmethoduri eventdate publishingentitytype".IndexOf( parms.OrderBy.ToLower() ) == -1 )
				{
					parms.OrderBy = "IndexIdentifier";
					//if ( parms.IsDescending )
					//	parms.OrderBy += " DESC";
				}
				else if ( parms.IsDescending && parms.OrderBy.ToLower().IndexOf( "desc" ) == -1 )
				{
					parms.OrderBy = parms.OrderBy + " desc";
				}
			}
			var list = ImportManager.ImportSearch( parms.Filter, parms.OrderBy, parms.PageNumber, parms.PageSize, ref pTotalRows );
			//pTotalRows = parms.TotalRows;
			return list;
		}
	}
}
