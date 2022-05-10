﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

using Services;
using Navy.Utilities;
using Models.Application;


namespace NavyRRL.Controllers
{
    public class BaseController : Controller
    {
		//For requests to load pages (redirects allowable)
		public void AuthenticateOrRedirect( string customMessage = null, bool redirectOnFailure = true, string redirectControllerName = "Event", string redirectActionName = "NotAuthenticated" )
		{
			if ( !AccountServices.IsUserAuthenticated() )
			{
				ConsoleMessageHelper.SetConsoleErrorMessage( string.IsNullOrWhiteSpace( customMessage ) ? AccountServices.NOT_AUTHENTICATED : customMessage );

				if ( redirectOnFailure )
				{
					RedirectToAction( redirectActionName, redirectControllerName );
				}
			}
		}
		//

		//For requests for AJAX (no redirects)
		public bool AuthenticateOrFail()
		{
			return AccountServices.IsUserAuthenticated();
		}
		//

		//Default method to send a JSON response
		public static ActionResult JsonResponse( object data, bool valid = true, List<string> status = null, object extra = null )
		{
			return new ContentResult()
			{
				Content = JsonConvert.SerializeObject(
					new
					{
						Data = data,
						Valid = valid,
						Status = status,
						Extra = extra
					},
					Formatting.None,
					new JsonSerializerSettings()
					{
						NullValueHandling = NullValueHandling.Ignore,
						DefaultValueHandling = DefaultValueHandling.Ignore
					}
				),
				ContentEncoding = Encoding.UTF8,
				ContentType = "application/json"
			};
		}
		//

		//Send a raw JSON response
		public static ActionResult RawJSONResponse( JObject data )
		{
			return new ContentResult()
			{
				Content = data.ToString( Formatting.None ),
				ContentEncoding = Encoding.UTF8,
				ContentType = "application/json"
			};
		}
		//
	}
}