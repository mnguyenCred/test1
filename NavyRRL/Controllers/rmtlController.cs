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
		public static string FunctionCode = "rmtl.search";
		/// <summary>
		/// Anyone can search?
		/// But shouldn't see non published stuff
		/// </summary>
		/// <returns></returns>
		////[CustomAttributes.NavyAuthorize( "RMTL Search", Roles = SiteReader )]
		public ActionResult Search()
		{
			//actually anyone can use it. But test with reader 
			AuthenticateOrRedirect( "You must be authenticated and authorized to use the RMTL Search.", FunctionCode );
			return View( "~/Views/RMTL/RMTLSearchV3.cshtml" );
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

	}
}