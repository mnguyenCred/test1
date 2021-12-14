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
				parms.OrderBy = "Unique_Identifier";
			}
			else
			{
				if ( "id indexidentifier unique_identifier rating rank ranklevel billet_title functional_area source date_of_source work_element_type work_element_task task_applicability formal_training_gap cin course_name course_type curriculum_control_authority life_cycle_control_document task_statement current_assessment_approach".IndexOf( parms.OrderBy.ToLower() ) == -1 )
				{
					parms.OrderBy = "Unique_Identifier";
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
