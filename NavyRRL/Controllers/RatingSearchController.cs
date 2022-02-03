using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AM = Models.Application;
using Services;

namespace NavyRRL.Controllers
{
    public class RatingSearchController : BaseController
    {
		[Authorize( Roles = "Administrator, Site Staff" )]
		public ActionResult Index()
        {
			if ( !AccountServices.IsUserAuthenticated() )
			{
				AM.SiteMessage siteMessage = new AM.SiteMessage()
				{
					Title = "Invalid Request",
					Message = "You must be authenticated and authorized to use this feature"
				};
			}
			return View( "~/views/ratingsearch/ratingsearchv1.cshtml" );
		}
		//


	}
}