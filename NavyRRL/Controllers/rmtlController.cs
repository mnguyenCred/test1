using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

using Models.Import;
using Data.Tables;
using Navy.Utilities;
using Services;
using Models.Search;

namespace NavyRRL.Controllers
{
	[SessionState( System.Web.SessionState.SessionStateBehavior.ReadOnly )]
	public class RMTLController : BaseController
    {
		[CustomAttributes.NavyAuthorize( "RMTL Search", Roles = SiteReader )]
		public ActionResult Search()
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to use the RMTL Search." );
			return View( "~/Views/RMTL/RMTLSearchV3.cshtml" );
		}
		//

		[CustomAttributes.NavyAuthorize( "RMTL Search", Roles = SiteReader )]
		public ActionResult SearchV2()
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to use the RMTL Search." );
			return View( "~/Views/RMTL/RMTLSearchV2.cshtml" );
		}
		//

		[HttpPost]
		public ActionResult	DoSearch( SearchQuery query )
		{
			bool valid = true;
			string status = "";
			//var results = new SearchServices().RMTLSearch( query, ref valid, ref status );
			var results = SearchServices.RatingContextSearch( query );

			return JsonResponse( results, valid, new List<string>() { status }, null );
		}
		//

		[CustomAttributes.NavyAuthorize( "RMTL Home", Roles = "Administrator, RMTL Developer, Site Staff" )]
		public ActionResult Index()
        {
            return View();
        }
		//

		[CustomAttributes.NavyAuthorize( "RMTL Catalog", Roles = Admin_SiteManager_SiteStaff )]
		public ActionResult Catalog()
		{
			return View();
		}
		//

		public ActionResult CatalogSearch()
		{
			var result = new JsonResult();

			try
			{
				#region According to Datatables.net, Server side parameters

				//global search
				var search = Request.Form["search[value]"];
				//what is draw??
				var draw = Request.Form["draw"];

				var orderBy = string.Empty;
				//column index
				var order = int.Parse( Request.Form["order[0][column]"] );
				//sort direction
				var orderDir = Request.Form["order[0][dir]"];

				int startRec = int.Parse( Request.Form["start"] );
				int pageSize = int.Parse( Request.Form["length"] );
				int pageNbr = ( startRec / pageSize ) + 1;
				#endregion

				#region Where filter

				//individual column wise search
				var columnSearch = new List<string>();
				var globalSearch = new List<string>();
				DateTime dt = new DateTime();

				//Get all keys starting with columns    
				foreach ( var index in Request.Form.AllKeys.Where( x => Regex.Match( x, @"columns\[(\d+)]" ).Success ).Select( x => int.Parse( Regex.Match( x, @"\d+" ).Value ) ).Distinct().ToList() )
				{
					//get individual columns search value
					var value = Request.Form[string.Format( "columns[{0}][search][value]", index )];
					if ( !string.IsNullOrWhiteSpace( value.Trim() ) )
					{
						value = value.Trim();
						string colName = Request.Form[string.Format( "columns[{0}][data]", index )];
						if ( colName == "EventDate" || colName == "DisplayDay" || colName == "PublishDate" )
						{
							if ( DateTime.TryParse( value, out dt ) )
							{
								columnSearch.Add( string.Format( " (convert(varchar(10),PublishDate,120) = '{0}') ", dt.ToString( "yyyy-MM-dd" ) ) );
							}
						}
						else if ( colName == "DisplayDate" )
						{
							if ( DateTime.TryParse( value, out dt ) )
							{
								columnSearch.Add( string.Format( " (convert(varchar(10),LastUpdated,120) = '{0}') ", dt.ToString( "yyyy-MM-dd" ) ) );
							}
						}
						//allow comma separated integers - N.A here
						//else if ( colName == "ParentRecordId" || colName == "ParentEntityTypeId" )
						//{
						//	if ( value.IndexOf( "||" ) > 0 )
						//	{
						//		var itemList = value.Split( new string[] { "||" }, StringSplitOptions.None );
						//		string filter = "";
						//		string OR = "";
						//		foreach ( var item in itemList )
						//		{
						//			filter = OR + string.Format( " ( {0} = {1} ) ", colName, value );
						//			OR = " OR ";
						//		}
						//		columnSearch.Add( filter );
						//	}
						//	else
						//	{
						//		columnSearch.Add( string.Format( " ( {0} = {1} ) ", colName, value ) );
						//	}
						//}
						else
						{
							if ( value.Length > 1 && value.IndexOf( "!" ) == 0 )
							{
								columnSearch.Add( string.Format( "({0} NOT LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], value.Substring( 1 ) ) );
							}
							else
							{
								//check for OR, or || better to require upper case to avoid issue with phrases
								//should watch for incomplete typing
								if ( value.IndexOf( " OR " ) > 0 )
								{
									var itemList = value.Split( new string[] { " OR " }, StringSplitOptions.None );
									string filter = "";
									string OR = "";
									foreach ( var item in itemList )
									{
										filter += OR + string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], item.Trim() );
										OR = " OR ";
									}
									columnSearch.Add( filter );
								}
								else if ( value.IndexOf( "||" ) > 0 )
								{
									var itemList = value.Split( new string[] { "||" }, StringSplitOptions.None );
									string filter = "";
									string OR = "";
									foreach ( var item in itemList )
									{
										filter += OR + string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], item.Trim() );
										OR = " OR ";
									}
									columnSearch.Add( filter );
								}
								else if ( value.IndexOf( " | " ) > 0 )
								{
									var itemList = value.Split( new string[] { " | " }, StringSplitOptions.None );
									string filter = "";
									string OR = "";
									foreach ( var item in itemList )
									{
										filter += OR + string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], item.Trim() );
										OR = " OR ";
									}
									columnSearch.Add( filter );
								}
								else
								{
									columnSearch.Add( string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], value ) );
								}
							}
						}
					}
					//get column filter for global search
					if ( !string.IsNullOrWhiteSpace( search ) )
					{
						if ( index > 0 ) //skip date
						{
							if ( Request.Form[string.Format( "columns[{0}][data]", index )] != "Totals" )
								globalSearch.Add( string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], search ) );
						}
					}

					//get order by from order index
					if ( order == index )
						orderBy = Request.Form[string.Format( "columns[{0}][data]", index )];
				}

				var where = string.Empty;
				//concat all filters for global search
				if ( globalSearch.Any() )
					where = globalSearch.Aggregate( ( current, next ) => current + " OR " + next );

				if ( columnSearch.Any() )
					if ( !string.IsNullOrEmpty( where ) )
						where = string.Format( "({0}) AND ({1})", where, columnSearch.Aggregate( ( current, next ) => current + " AND " + next ) );
					else
						where = columnSearch.Aggregate( ( current, next ) => current + " AND " + next );

				#endregion


				BaseSearchModel parms = new BaseSearchModel()
				{
					Keyword = "",
					OrderBy = orderBy,
					IsDescending = orderDir == "desc" ? true : false,
					PageNumber = pageNbr,
					PageSize = pageSize
				};
				parms.Filter = where;
				var totalRecords = 0;
				var list = RatingTaskServices.Browse( parms, ref totalRecords );

				result = Json( new { data = list, draw = int.Parse( draw ), recordsTotal = totalRecords, recordsFiltered = totalRecords }, JsonRequestBehavior.AllowGet );
			}
			catch ( Exception ex )
			{
			}

			return result;
		}
		//

		public ActionResult ImportSummary()
        {
            return View();
        }
		//

		public ActionResult ImportSummarySearch()
		{
			var result = new JsonResult();

			try
			{
				#region According to Datatables.net, Server side parameters

				//global search
				var search = Request.Form["search[value]"];
				//what is draw??
				var draw = Request.Form["draw"];

				var orderBy = string.Empty;
				//column index
				var order = int.Parse( Request.Form["order[0][column]"] );
				//sort direction
				var orderDir = Request.Form["order[0][dir]"];

				int startRec = int.Parse( Request.Form["start"] );
				int pageSize = int.Parse( Request.Form["length"] );
				int pageNbr = ( startRec / pageSize ) + 1;
				#endregion

				#region Where filter

				//individual column wise search
				var columnSearch = new List<string>();
				var globalSearch = new List<string>();
				DateTime dt = new DateTime();

				//Get all keys starting with columns    
				foreach ( var index in Request.Form.AllKeys.Where( x => Regex.Match( x, @"columns\[(\d+)]" ).Success ).Select( x => int.Parse( Regex.Match( x, @"\d+" ).Value ) ).Distinct().ToList() )
				{
					//get individual columns search value
					var value = Request.Form[string.Format( "columns[{0}][search][value]", index )];
					if ( !string.IsNullOrWhiteSpace( value.Trim() ) )
					{
						value = value.Trim();
						string colName = Request.Form[string.Format( "columns[{0}][data]", index )];
						if ( colName == "EventDate" || colName == "DisplayDay" || colName == "PublishDate" )
						{
							if ( DateTime.TryParse( value, out dt ) )
							{
								columnSearch.Add( string.Format( " (convert(varchar(10),PublishDate,120) = '{0}') ", dt.ToString( "yyyy-MM-dd" ) ) );
							}
						}
						//allow comma separated integers - N.A here
						//else if ( colName == "ParentRecordId" || colName == "ParentEntityTypeId" )
						//{
						//	if ( value.IndexOf( "||" ) > 0 )
						//	{
						//		var itemList = value.Split( new string[] { "||" }, StringSplitOptions.None );
						//		string filter = "";
						//		string OR = "";
						//		foreach ( var item in itemList )
						//		{
						//			filter = OR + string.Format( " ( {0} = {1} ) ", colName, value );
						//			OR = " OR ";
						//		}
						//		columnSearch.Add( filter );
						//	}
						//	else
						//	{
						//		columnSearch.Add( string.Format( " ( {0} = {1} ) ", colName, value ) );
						//	}
						//}
						else
						{
							if ( value.Length > 1 && value.IndexOf( "!" ) == 0 )
								columnSearch.Add( string.Format( "({0} NOT LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], value.Substring( 1 ) ) );
							else
							{
								//check for OR, or ||
								//should watch for incomplete typing
								if ( value.IndexOf( " OR " ) > 0 )
								{
									var itemList = value.Split( new string[] { " OR " }, StringSplitOptions.None );
									string filter = "";
									string OR = "";
									foreach ( var item in itemList )
									{
										filter = OR + string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], item );
										OR = " OR ";
									}
									columnSearch.Add( filter );
								}
								else if ( value.IndexOf( "||" ) > 0 )
								{
									var itemList = value.Split( new string[] { "||" }, StringSplitOptions.None );
									string filter = "";
									string OR = "";
									foreach ( var item in itemList )
									{
										filter = OR + string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], item );
										OR = " OR ";
									}
									columnSearch.Add( filter );
								}
								else
								{
									columnSearch.Add( string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], value ) );
								}
							}
						}
					}
					//get column filter for global search
					if ( !string.IsNullOrWhiteSpace( search ) )
					{
						if ( index > 0 ) //skip date
						{
							if ( Request.Form[string.Format( "columns[{0}][data]", index )] != "Totals" )
								globalSearch.Add( string.Format( "({0} LIKE '%{1}%')", Request.Form[string.Format( "columns[{0}][data]", index )], search ) );
						}
					}

					//get order by from order index
					if ( order == index )
						orderBy = Request.Form[string.Format( "columns[{0}][data]", index )];
				}

				var where = string.Empty;
				//concat all filters for global search
				if ( globalSearch.Any() )
					where = globalSearch.Aggregate( ( current, next ) => current + " OR " + next );

				if ( columnSearch.Any() )
					if ( !string.IsNullOrEmpty( where ) )
						where = string.Format( "({0}) AND ({1})", where, columnSearch.Aggregate( ( current, next ) => current + " AND " + next ) );
					else
						where = columnSearch.Aggregate( ( current, next ) => current + " AND " + next );

				#endregion


				BaseSearchModel parms = new BaseSearchModel()
				{
					Keyword = "",
					OrderBy = orderBy,
					IsDescending = orderDir == "desc" ? true : false,
					PageNumber = pageNbr,
					PageSize = pageSize
				};
				parms.Filter = where;
				var totalRecords = 0;
				var list = ImportServices.ImportSummarySearch( parms, ref totalRecords );

				result = Json( new { data = list, draw = int.Parse( draw ), recordsTotal = totalRecords, recordsFiltered = totalRecords }, JsonRequestBehavior.AllowGet );
			}
			catch ( Exception ex )
			{

			}

			return result;
		}
		//


	}
}