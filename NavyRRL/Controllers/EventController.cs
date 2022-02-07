using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NavyRRL.Controllers
{
    public class EventController : Controller
    {
        // GET: Event
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NotAuthenticated()
        {
            return View();
        }

        public ActionResult NotAuthorized()
        {
            return View();
        }
    }
}