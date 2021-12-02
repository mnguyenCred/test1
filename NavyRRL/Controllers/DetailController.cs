using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NavyRRL.Controllers
{
    public class DetailController : Controller
    {
        // GET: Detail
        public ActionResult Index()
        {
			return View( "~/views/detail/detailv1.cshtml" );
        }
    }
}