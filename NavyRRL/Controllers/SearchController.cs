using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NavyRRL.Controllers
{
    public class SearchController : BaseController
    {
        // GET: Search
        public ActionResult Index()
        {
            return View( "~/views/search/searchv1.cshtml" );
        }
		//

		public ActionResult SearchV2()
		{
			return View( "~/views/search/searchv2.cshtml" );
		}
		//

    }
}